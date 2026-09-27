using System;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HData;
using HFromUI.HFrom;
namespace HFromUI.HMath
{
    using HFromUI.HColor;
    using HFromUI.HLangage;
    using HFromUI.HFrom.From;
    using HFromUI.HControl.Tools.Message;
    using HFromUI.HEnum;
    public class HScreenImage : HScreen
    {
        /// <summary>视图最小缩放。</summary>
        public override HDouble ViewScaleMin { set; get; } = 0.1;
        /// <summary>视图最大缩放。</summary>
        public override HDouble ViewScaleMax { set; get; } = 1000;
        /// <summary>RefreshHz 成员。</summary>
        public override int RefreshHz { set; get; } = -1;
        private Bitmap image;
        /// <summary>imageTopLeftWorld 字段。</summary>
        private HPoint imageTopLeftWorld = new HPoint(0, 0);
        /// <summary>SelectedShapeType 成员。</summary>
        public HShapeType SelectedShapeType { set; get; } = HShapeType.None;
        /// <summary>LastMousePosition 成员。</summary>
        public HPoint LastMousePosition { set; get; }
        /// <summary>LastMousePositionWorldPoint 成员。</summary>
        public HPoint LastMousePositionWorldPoint { set; get; } = new HPoint();
        /// <summary>LastViewOffsetPosition 成员。</summary>
        public HPoint LastViewOffsetPosition { set; get; }
        public Bitmap Image
        {
            get
            { 
                return image;
            }
            set
            {
                if (image != value) // 引用比较，如果是同一张图片则不触发
                {
                    image = value;
                    // 触发事件
                    OnImageChanged(EventArgs.Empty);
                }
            }
        }
        public event EventHandler ImageChanged;
        /// <summary>响应 ImageChanged 事件。</summary>
        protected virtual void OnImageChanged(EventArgs e)
        {
            ImageChanged?.Invoke(this, e);
        }
        /// <summary>当前图片的宽度（像素，无图时为 0）。</summary>
        public HDouble ImageWidth => image?.Width ?? 0d;
        /// <summary>当前图片的高度（像素，无图时为 0）。</summary>
        public HDouble ImageHeight => image?.Height ?? 0d;
        /// <summary>
        /// 图像左上角在世界坐标系中的位置（可设置）
        /// </summary>
        public HPoint ImageTopLeftWorld
        {
            get => imageTopLeftWorld;
            set => imageTopLeftWorld = value ?? new HPoint(0, 0);
        }
        public HPoint ImagePoint
        {
            get { return WorldToImage(WorldPoint); }
        }
        /// <summary>
        /// 世界坐标 → 图像像素坐标（原点在图像左上角，Y轴向下）
        /// </summary>
        public HPoint WorldToImage(HPoint worldPoint)
        {
            if (image == null)
                throw new InvalidOperationException(HTranslation.GetContent("图像未设置。"));
            if (worldPoint == null)
                throw new ArgumentNullException(nameof(worldPoint));
            double px = worldPoint.X.Value - ImageTopLeftWorld.X.Value;
            double py = ImageTopLeftWorld.Y.Value - worldPoint.Y.Value; // Y轴反向（世界向上，图像向下）
            return new HPoint(px, py);
        }
        /// <summary>
        /// 图像像素坐标 → 世界坐标（图像像素原点在左上角，Y轴向下）
        /// </summary>
        public HPoint ImageToWorld(Point imagePoint)
        {
            if (image == null)
                throw new InvalidOperationException(HTranslation.GetContent("图像未设置。"));
            double wx = ImageTopLeftWorld.X.Value + imagePoint.X;
            double wy = ImageTopLeftWorld.Y.Value - imagePoint.Y;
            return new HPoint(wx, wy);
        }
        /// <summary>
        /// 获取图像在世界坐标系中的矩形区域（左上角 → 右下角）
        /// </summary>
        public HRect GetRectWorld()
        {
            // 左上角: (ImageTopLeftWorld.X.Value, ImageTopLeftWorld.Y.Value)
            // 右下角: X + 宽度, Y - 高度（因为世界Y向上，图像向下延伸）
            return new HRectangle(
                imageTopLeftWorld,
                new HPoint(imageTopLeftWorld.X.Value + ImageWidth, imageTopLeftWorld.Y.Value - ImageHeight)
            ).ToHRect();
        }
        /// <summary>
        /// 获取图像在屏幕坐标系中的矩形区域（用于绘制）
        /// </summary>
        public HRect GetScreenRectWorld()
        {
            return new HRectangle(
                WorldToScreen(imageTopLeftWorld),
                WorldToScreen(new HPoint(imageTopLeftWorld.X.Value + ImageWidth, imageTopLeftWorld.Y.Value - ImageHeight))
            ).ToHRect();
        }
        /// <summary>按图片在屏幕坐标系中的矩形区域绘制图片。</summary>
        /// <param name="g">绘图对象。</param>
        public void DrawImage(Graphics g)
        {
            if (Image != null)
                g.DrawImage(Image, GetScreenRectWorld().ToRectangleF());
        }
        /// <summary>绘制图片边框（红色矩形框）。</summary>
        /// <param name="g">绘图对象。</param>
        public void DrawImageRect(Graphics g)
        {
            RectangleF rectangleF = GetScreenRectWorld().ToRectangleF();
            g.DrawRectangle(new Pen(HColors.Reds.BloodRed, 2), rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
        }
        /// <summary>弹出打开文件对话框选择图片，并更新视图范围。</summary>
        public void OpenImageDialog()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter =
                    $@"{HTranslation.GetContent("图片文件")} (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|" +
                    $@"{HTranslation.GetContent("所有文件")} (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.Title = HTranslation.GetContent("请选择一张图片");
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        Image = new Bitmap(filePath);
                        SetImageViewOffsetCenter();
                        UpdateViewBoundsFromImage(1);
                        FitWorldRectangle(GetRectWorld().ToHRectangle(),true);
                    }
                    catch (OutOfMemoryException)
                    {
                        HMessageA.ShowDialog(HTranslation.GetContent("文件不是有效的图片格式，或文件已损坏。"), HTranslation.GetContent("打开图片出错"), 0, HTranslation.GetContent("确认"));
                    }
                    catch (Exception ex)
                    {
                        HMessageA.ShowDialog(HTranslation.GetContent("加载图片时出错：") + ex.Message, HTranslation.GetContent("打开图片出错"), 0, HTranslation.GetContent("确认"));
                    }
                }
            }
        }
        /// <summary>调整视图偏移，使图片世界矩形居中显示。</summary>
        public void SetImageViewOffsetCenter()
        {
            HRect hRect = GetRectWorld();
            ViewOffsetCenter(hRect.X.Value+ hRect.Width/2, hRect.Y.Value+ hRect.Height/2);
        }
        /// <summary>
        /// 根据图片尺寸和指定的缩放倍数设置 ViewBounds，
        /// 图片始终位于边界框正中央。
        /// </summary>
        /// <param name="scale">边界框相对于图片的缩放倍数，默认为 1.0（与图片等大）</param>
        public void UpdateViewBoundsFromImage(double scale = 1.0)
        {
            if (Image == null)
            {
                ViewBounds = null;
                return;
            }
            // 图片的世界中心
            double centerX = ImageTopLeftWorld.X.Value + ImageWidth.Value / 2.0;
            double centerY = ImageTopLeftWorld.Y.Value - ImageHeight.Value / 2.0;
            // 边界框的半宽、半高
            double halfW = (ImageWidth.Value * scale) / 2.0;
            double halfH = (ImageHeight.Value * scale) / 2.0;
            double left = centerX - halfW;
            double right = centerX + halfW;
            double bottom = centerY - halfH;
            double top = centerY + halfH;
            // 用对角点构造，HRectangle 自动规范方向
            ViewBounds = new HRectangle(new HPoint(left, bottom), new HPoint(right, top));
        }
    }
}
