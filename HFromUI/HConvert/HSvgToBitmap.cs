using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;

namespace HFromUI.HConvert
{
    /// <summary>
    /// SVG 路径（path 的 d 属性格式，支持 M/L/A/Z 命令与相对/绝对坐标）到
    /// GDI+ <see cref="GraphicsPath"/> / <see cref="Bitmap"/> 的转换器。
    /// 纯 GDI+ 实现，不依赖任何 SVG 组件；渲染结果为 32bppArgb 位图，支持透明背景。
    /// </summary>
    public static class HSvgToBitmap
    {
        /// <summary>将一条 SVG 路径字符串转换为 <see cref="GraphicsPath"/>，调用方负责释放返回对象。</summary>
        /// <param name="svgPathData">SVG path 的 d 属性内容。</param>
        /// <returns>与路径等价的 GDI+ 路径；输入为空时返回空路径。</returns>
        public static GraphicsPath ToPath(string svgPathData)
        {
            GraphicsPath path = new GraphicsPath();
            if (string.IsNullOrEmpty(svgPathData)) return path;

            List<string> tokens = Tokenize(svgPathData);
            int i = 0;
            float curX = 0, curY = 0, startX = 0, startY = 0;
            char lastCmd = '\0';

            while (i < tokens.Count)
            {
                string token = tokens[i];
                bool isCmd = char.IsLetter(token[0]);
                char cmd = isCmd ? token[0] : lastCmd;
                bool relative = isCmd ? char.IsLower(token[0]) : char.IsLower(lastCmd);
                if (isCmd) i++;

                switch (char.ToUpper(cmd))
                {
                    case 'M': // 移动到点
                        float xM = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yM = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xM += curX; yM += curY; }
                        curX = xM; curY = yM; startX = xM; startY = yM;
                        path.StartFigure();
                        path.AddLine(xM, yM, xM, yM);
                        break;
                    case 'L': // 直线到点
                        float xL = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yL = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xL += curX; yL += curY; }
                        path.AddLine(curX, curY, xL, yL);
                        curX = xL; curY = yL;
                        break;
                    case 'A': // 椭圆弧
                        float rx = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float ry = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float phi = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float fA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float fS = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float xA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xA += curX; yA += curY; }
                        DrawArc(path, curX, curY, rx, ry, phi, fA, fS, xA, yA);
                        curX = xA; curY = yA;
                        break;
                    case 'Z': // 闭合路径
                        path.CloseFigure();
                        curX = startX; curY = startY;
                        break;
                }
                lastCmd = cmd;
            }
            return path;
        }

        /// <summary>计算多个路径合并后的外接边界矩形。</summary>
        public static RectangleF GetBounds(params GraphicsPath[] paths)
        {
            if (paths == null || paths.Length == 0) return RectangleF.Empty;
            using (GraphicsPath combined = new GraphicsPath())
            {
                foreach (GraphicsPath p in paths)
                {
                    if (p != null) combined.AddPath(p, false);
                }
                return combined.GetBounds();
            }
        }

        /// <summary>
        /// 将填充路径与描边路径等比缩放、居中渲染为指定尺寸的 32bppArgb 位图，调用方负责释放返回位图。
        /// </summary>
        /// <param name="width">输出位图宽度（像素）。</param>
        /// <param name="height">输出位图高度（像素）。</param>
        /// <param name="fillPath">需要填充的路径，可为 null。</param>
        /// <param name="fillColor">填充色。</param>
        /// <param name="strokePath">需要描边的路径，可为 null。</param>
        /// <param name="strokeColor">描边色。</param>
        /// <param name="strokeWidth">原始坐标系下的描边宽度。</param>
        /// <param name="fillRatio">图形占位图短边的比例（0..1），默认 0.85。</param>
        /// <param name="backColor">背景色，为 null 时透明背景。</param>
        public static Bitmap ToBitmap(int width, int height,
            GraphicsPath fillPath, Color fillColor,
            GraphicsPath strokePath, Color strokeColor, float strokeWidth,
            float fillRatio = 0.85f, Color? backColor = null)
        {
            RectangleF bounds = GetBounds(fillPath, strokePath);
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if (backColor.HasValue) g.Clear(backColor.Value);
                else g.Clear(Color.Transparent);

                if (bounds.Width > 0f && bounds.Height > 0f)
                {
                    float scale = (Math.Min(width, height) * fillRatio) / Math.Max(bounds.Width, bounds.Height);
                    float offsetX = (width - bounds.Width * scale) / 2f - bounds.X * scale;
                    float offsetY = (height - bounds.Height * scale) / 2f - bounds.Y * scale;

                    g.TranslateTransform(offsetX, offsetY);
                    g.ScaleTransform(scale, scale);

                    if (strokePath != null)
                    {
                        using (Pen pen = new Pen(strokeColor, strokeWidth / scale))
                            g.DrawPath(pen, strokePath);
                    }
                    if (fillPath != null)
                    {
                        using (SolidBrush brush = new SolidBrush(fillColor))
                            g.FillPath(brush, fillPath);
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// 直接从 SVG 路径字符串渲染位图：方法内部解析并释放路径，调用方负责释放返回位图。
        /// </summary>
        /// <param name="width">输出位图宽度（像素）。</param>
        /// <param name="height">输出位图高度（像素）。</param>
        /// <param name="fillSvg">需要填充的 SVG 路径，可为 null。</param>
        /// <param name="fillColor">填充色。</param>
        /// <param name="strokeSvg">需要描边的 SVG 路径，可为 null。</param>
        /// <param name="strokeColor">描边色。</param>
        /// <param name="strokeWidth">原始坐标系下的描边宽度。</param>
        /// <param name="fillRatio">图形占位图短边的比例（0..1），默认 0.85。</param>
        public static Bitmap ToBitmap(int width, int height,
            string fillSvg, Color fillColor,
            string strokeSvg, Color strokeColor, float strokeWidth,
            float fillRatio = 0.85f)
        {
            GraphicsPath fillPath = string.IsNullOrEmpty(fillSvg) ? null : ToPath(fillSvg);
            GraphicsPath strokePath = string.IsNullOrEmpty(strokeSvg) ? null : ToPath(strokeSvg);
            try
            {
                return ToBitmap(width, height, fillPath, fillColor, strokePath, strokeColor, strokeWidth, fillRatio);
            }
            finally
            {
                fillPath?.Dispose();
                strokePath?.Dispose();
            }
        }

        /// <summary>将 SVG 路径字符串拆分为命令字母和数字标记。</summary>
        private static List<string> Tokenize(string data)
        {
            List<string> tokens = new List<string>();
            int i = 0;
            while (i < data.Length)
            {
                char c = data[i];
                // 跳过空白字符和逗号
                if (char.IsWhiteSpace(c) || c == ',') { i++; continue; }
                if (char.IsLetter(c))
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                else if (c == '-' || c == '+' || c == '.' || char.IsDigit(c))
                {
                    int start = i;
                    if (c == '-' || c == '+') i++;
                    while (i < data.Length && (char.IsDigit(data[i]) || data[i] == '.')) i++;
                    tokens.Add(data.Substring(start, i - start));
                }
                else { i++; }
            }
            return tokens;
        }

        /// <summary>向路径添加符合 SVG 规范的椭圆弧。</summary>
        private static void DrawArc(GraphicsPath path, float x1, float y1, float rx, float ry,
                                   float phi, float fA, float fS, float x2, float y2)
        {
            if (Math.Abs(x1 - x2) < 1e-6 && Math.Abs(y1 - y2) < 1e-6) return;
            rx = Math.Abs(rx); ry = Math.Abs(ry);
            if (rx < 1e-6 || ry < 1e-6) { path.AddLine(x1, y1, x2, y2); return; }

            double phiRad = phi * Math.PI / 180.0;
            double cosPhi = Math.Cos(phiRad), sinPhi = Math.Sin(phiRad);
            double dx = (x1 - x2) / 2.0, dy = (y1 - y2) / 2.0;
            double x1p = cosPhi * dx + sinPhi * dy;
            double y1p = -sinPhi * dx + cosPhi * dy;

            double lam = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
            if (lam > 1) { double s = Math.Sqrt(lam); rx *= (float)s; ry *= (float)s; }

            double rxSq = rx * rx, rySq = ry * ry;
            double num = rxSq * rySq - rxSq * y1p * y1p - rySq * x1p * x1p;
            num = Math.Max(0, num);
            double coeff = (fA == fS ? -1 : 1) * Math.Sqrt(num / (rxSq * y1p * y1p + rySq * x1p * x1p));
            double cxp = coeff * rx * y1p / ry;
            double cyp = coeff * (-ry * x1p / rx);
            double cx = cosPhi * cxp - sinPhi * cyp + (x1 + x2) / 2.0;
            double cy = sinPhi * cxp + cosPhi * cyp + (y1 + y2) / 2.0;

            double th1 = Angle(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry);
            double dth = Angle((x1p - cxp) / rx, (y1p - cyp) / ry, (-x1p - cxp) / rx, (-y1p - cyp) / ry);
            dth %= 2 * Math.PI;
            if (fS == 0 && dth > 0) dth -= 2 * Math.PI;
            else if (fS == 1 && dth < 0) dth += 2 * Math.PI;

            float startAngle = (float)(th1 * 180.0 / Math.PI);
            float sweepAngle = (float)(dth * 180.0 / Math.PI);
            RectangleF rect = new RectangleF((float)(cx - rx), (float)(cy - ry), (float)(2 * rx), (float)(2 * ry));
            path.AddArc(rect, startAngle, sweepAngle);
        }

        /// <summary>计算两个向量之间的夹角（带符号）。</summary>
        private static double Angle(double ux, double uy, double vx, double vy)
        {
            double dot = ux * vx + uy * vy;
            double len = Math.Sqrt(ux * ux + uy * uy) * Math.Sqrt(vx * vx + vy * vy);
            double ang = Math.Acos(Math.Max(-1, Math.Min(1, dot / len)));
            double sign = ux * vy - uy * vx;
            return sign < 0 ? -ang : ang;
        }
    }
}
