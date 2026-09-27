using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Resources;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Security;
using System.Collections.ObjectModel;
using HFromUI.HAttribute;

namespace HFromUI.HFrom.HMI
{
    public enum GraphDirection
{
    Upward = 1,
    Downward,
    Leftward,
    Rightward
}
public enum RotateDirection
{
    ClockWise,
    AntiClockWise
}

public enum ConveyerStyleH
{
    Horizontal = 1,
    Downslope,
    Upslope
}
public enum DirectionStyleH
{
    Horizontal = 1,
    Vertical
}
public enum CurveStyle
{
    LineSegment,
    Curve,
    Section,
    StepLine,
    StepLineWithoutVertical,
    LineDot,
    CurveDot,
    LineDash,
    CurveDash,
    LineLongDath,
    CurveLongDath
}
public enum MarkTextPositionStyle
{
    Up = 1,
    Right = 2,
    Down = 3,
    Left = 4,
    Center = 5,
    Auto = 10
}
public enum ScaleMode
{
    None,
    OnlyX,
    Both,
    XThenY
}
public enum ProgressStyle
{
    None,
    Number,
    Percent,
    Size,
    Customer
}
public enum ProgressStyleH
{
    Horizontal = 1,
    Vertical,
    Circular
}
public enum RenderStyleH
{
    Ellipse = 1,
    Rectangle,
    Rhombus
}
public enum PipeTurnDirectionH
{
    Up = 1,
    Down,
    Left,
    Right,
    None
}

public class FromHelper
{
    public static StringFormat StringFormatCenter
    {
        get;
        set;
    }

    public static StringFormat StringFormatLeft
    {
        get;
        set;
    }

    public static StringFormat StringFormatRight
    {
        get;
        set;
    }

    public static StringFormat StringFormatDefault
    {
        get;
        set;
    }

    public static StringFormat StringFormatTopCenter
    {
        get;
        set;
    }

    static FromHelper()
    {
        StringFormatDefault = new StringFormat();
        StringFormatCenter = new StringFormat();
        StringFormatCenter.Alignment = StringAlignment.Center;
        StringFormatCenter.LineAlignment = StringAlignment.Center;
        StringFormatLeft = new StringFormat();
        StringFormatLeft.LineAlignment = StringAlignment.Center;
        StringFormatLeft.Alignment = StringAlignment.Near;
        StringFormatRight = new StringFormat();
        StringFormatRight.LineAlignment = StringAlignment.Center;
        StringFormatRight.Alignment = StringAlignment.Far;
        StringFormatTopCenter = new StringFormat();
        StringFormatTopCenter.Alignment = StringAlignment.Center;
        StringFormatTopCenter.LineAlignment = StringAlignment.Near;
    }

    /// <summary>Middle 方法。</summary>
    public static int Middle(int min, int value, int max)
    {
        if (value > max)
        {
            return max;
        }
        if (value < min)
        {
            return min;
        }
        return value;
    }

    /// <summary>获取 rhombusFromRectangle。</summary>
    public static Point[] GetRhombusFromRectangle(Rectangle rect)
    {
        return new Point[5]
        {
            new Point(rect.X, rect.Y + rect.Height / 2),
            new Point(rect.X + rect.Width / 2, rect.Y + rect.Height - 1),
            new Point(rect.X + rect.Width - 1, rect.Y + rect.Height / 2),
            new Point(rect.X + rect.Width / 2, rect.Y),
            new Point(rect.X, rect.Y + rect.Height / 2)
        };
    }

    /// <summary>ComputePaintLocationY 方法。</summary>
    public static float ComputePaintLocationY(int max, int min, int height, int value)
    {
        if ((float)(max - min) == 0f)
        {
            return height;
        }
        return (float)height - (float)(value - min) * 1f / (float)(max - min) * (float)height;
    }

    /// <summary>ComputePaintLocationY 方法。</summary>
    public static float ComputePaintLocationY(float max, float min, float height, float value)
    {
        if (max - min == 0f)
        {
            return height;
        }
        float num = max - min;
        if (num == 0f)
        {
            num = 1f;
        }
        return height - (value - min) / num * height;
    }

    /// <summary>ComputePaintLocationX 方法。</summary>
    public static float ComputePaintLocationX(float max, float min, float width, float value)
    {
        if (max - min == 0f)
        {
            return width;
        }
        float num = max - min;
        if (num == 0f)
        {
            num = 1f;
        }
        return (value - min) / num * width;
    }

    /// <summary>ComputeValueFromPaintLocationY 方法。</summary>
    public static float ComputeValueFromPaintLocationY(float max, float min, float height, float paint)
    {
        if (max - min == 0f)
        {
            return max;
        }
        float num = max - min;
        if (num == 0f)
        {
            num = 1f;
        }
        return (height - paint) * num / height + min;
    }

    /// <summary>ComputePaintLocationY 方法。</summary>
    public static float ComputePaintLocationY(ReferenceAxis referenceAxis, float height, float value)
    {
        return ComputePaintLocationY(referenceAxis.Max, referenceAxis.Min, height, value);
    }

    /// <summary>PaintCoordinateDivide 方法。</summary>
    public static void PaintCoordinateDivide(Graphics g, System.Drawing.Pen penLine, System.Drawing.Pen penDash, Font font, System.Drawing.Brush brush, StringFormat sf, int degree, int max, int min, int width, int height, int left = 60, int right = 8, int up = 8, int down = 8)
    {
        for (int i = 0; i <= degree; i++)
        {
            int value = (max - min) * i / degree + min;
            int num = (int)ComputePaintLocationY(max, min, height - up - down, value) + up + 1;
            g.DrawLine(penLine, left - 1, num, left - 4, num);
            if (i != 0)
            {
                g.DrawLine(penDash, left, num, width - right, num);
            }
            g.DrawString(value.ToString(), font, brush, new Rectangle(-5, num - font.Height / 2, left, font.Height), sf);
        }
    }

    /// <summary>PaintTriangle 方法。</summary>
    public static void PaintTriangle(Graphics g, System.Drawing.Brush brush, Point point, int size, GraphDirection direction)
    {
        Point[] array = new Point[4];
        switch (direction)
        {
            case GraphDirection.Leftward:
                array[0] = new Point(point.X, point.Y - size);
                array[1] = new Point(point.X, point.Y + size);
                array[2] = new Point(point.X - 2 * size, point.Y);
                break;
            case GraphDirection.Rightward:
                array[0] = new Point(point.X, point.Y - size);
                array[1] = new Point(point.X, point.Y + size);
                array[2] = new Point(point.X + 2 * size, point.Y);
                break;
            case GraphDirection.Upward:
                array[0] = new Point(point.X - size, point.Y);
                array[1] = new Point(point.X + size, point.Y);
                array[2] = new Point(point.X, point.Y - 2 * size);
                break;
            default:
                array[0] = new Point(point.X - size, point.Y);
                array[1] = new Point(point.X + size, point.Y);
                array[2] = new Point(point.X, point.Y + 2 * size);
                break;
        }
        array[3] = array[0];
        g.FillPolygon(brush, array);
    }

    /// <summary>DrawLeftRight 方法。</summary>
    public static void DrawLeftRight(Graphics g, System.Drawing.Pen pen, Rectangle rectangle, GraphDirection direction)
    {
        switch (direction)
        {
            case GraphDirection.Leftward:
                g.DrawLines(pen, new Point[3]
                {
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y),
                new Point(rectangle.X, rectangle.Y + rectangle.Height / 2),
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height)
                });
                g.DrawLines(pen, new Point[3]
                {
                new Point(rectangle.X + rectangle.Width, rectangle.Y),
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2),
                new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height)
                });
                break;
            case GraphDirection.Rightward:
                g.DrawLines(pen, new Point[3]
                {
                new Point(rectangle.X, rectangle.Y),
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2),
                new Point(rectangle.X, rectangle.Y + rectangle.Height)
                });
                g.DrawLines(pen, new Point[3]
                {
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y),
                new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height / 2),
                new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height)
                });
                break;
        }
    }

    /// <summary>PaintTriangle 方法。</summary>
    public static void PaintTriangle(Graphics g, System.Drawing.Brush brush, PointF point, int size, GraphDirection direction)
    {
        PointF[] array = new PointF[4];
        switch (direction)
        {
            case GraphDirection.Leftward:
                array[0] = new PointF(point.X, point.Y - (float)size);
                array[1] = new PointF(point.X, point.Y + (float)size);
                array[2] = new PointF(point.X - (float)(2 * size), point.Y);
                break;
            case GraphDirection.Rightward:
                array[0] = new PointF(point.X, point.Y - (float)size);
                array[1] = new PointF(point.X, point.Y + (float)size);
                array[2] = new PointF(point.X + (float)(2 * size), point.Y);
                break;
            case GraphDirection.Upward:
                array[0] = new PointF(point.X - (float)size, point.Y);
                array[1] = new PointF(point.X + (float)size, point.Y);
                array[2] = new PointF(point.X, point.Y - (float)(2 * size));
                break;
            default:
                array[0] = new PointF(point.X - (float)size, point.Y);
                array[1] = new PointF(point.X + (float)size, point.Y);
                array[2] = new PointF(point.X, point.Y + (float)(2 * size));
                break;
        }
        array[3] = array[0];
        g.FillPolygon(brush, array);
    }

    public static void AddArrayData<T>(ref T[] array, T[] data, int max)
    {
        if (data == null || data.Length == 0)
        {
            return;
        }
        if (array.Length == max)
        {
            Array.Copy(array, data.Length, array, 0, array.Length - data.Length);
            Array.Copy(data, 0, array, array.Length - data.Length, data.Length);
        }
        else if (array.Length + data.Length > max)
        {
            T[] array2 = new T[max];
            for (int i = 0; i < max - data.Length; i++)
            {
                array2[i] = array[i + (array.Length - max + data.Length)];
            }
            for (int j = 0; j < data.Length; j++)
            {
                array2[array2.Length - data.Length + j] = data[j];
            }
            array = array2;
        }
        else
        {
            T[] array3 = new T[array.Length + data.Length];
            for (int k = 0; k < array.Length; k++)
            {
                array3[k] = array[k];
            }
            for (int l = 0; l < data.Length; l++)
            {
                array3[array3.Length - data.Length + l] = data[l];
            }
            array = array3;
        }
    }

    /// <summary>ConvertSize 方法。</summary>
    public static SizeF ConvertSize(SizeF size, float angle)
    {
        System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
        matrix.Rotate(angle);
        PointF[] array = new PointF[4];
        array[0].X = (0f - size.Width) / 2f;
        array[0].Y = (0f - size.Height) / 2f;
        array[1].X = (0f - size.Width) / 2f;
        array[1].Y = size.Height / 2f;
        array[2].X = size.Width / 2f;
        array[2].Y = size.Height / 2f;
        array[3].X = size.Width / 2f;
        array[3].Y = (0f - size.Height) / 2f;
        matrix.TransformPoints(array);
        float num = float.MaxValue;
        float num2 = float.MinValue;
        float num3 = float.MaxValue;
        float num4 = float.MinValue;
        PointF[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            PointF pointF = array2[i];
            if (pointF.X < num)
            {
                num = pointF.X;
            }
            if (pointF.X > num2)
            {
                num2 = pointF.X;
            }
            if (pointF.Y < num3)
            {
                num3 = pointF.Y;
            }
            if (pointF.Y > num4)
            {
                num4 = pointF.Y;
            }
        }
        return new SizeF(num2 - num, num4 - num3);
    }

    /// <summary>绘制文本。</summary>
    public static void DrawString(Graphics g, string s, Font font, System.Drawing.Brush brush, PointF point, StringFormat format, float angle)
    {
        System.Drawing.Drawing2D.Matrix transform = g.Transform;
        System.Drawing.Drawing2D.Matrix transform2 = g.Transform;
        transform2.RotateAt(angle, point);
        g.Transform = transform2;
        g.DrawString(s, font, brush, point, format);
        g.Transform = transform;
    }

    /// <summary>获取 pow。</summary>
    private static int GetPow(int digit)
    {
        int num = 1;
        for (int i = 0; i < digit; i++)
        {
            num *= 10;
        }
        return num;
    }

    /// <summary>TranlateArrayToDouble 方法。</summary>
    public static double[] TranlateArrayToDouble(int[] values)
    {
        if (values == null)
        {
            return null;
        }
        double[] array = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            array[i] = values[i];
        }
        return array;
    }

    /// <summary>TranlateArrayToDouble 方法。</summary>
    public static double[] TranlateArrayToDouble(float[] values)
    {
        double[] array = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            array[i] = values[i];
        }
        return array;
    }

    /// <summary>CalculateMaxSectionFrom 方法。</summary>
    public static int CalculateMaxSectionFrom(double[] values)
    {
        double num = values.Max();
        if (num <= 5.0)
        {
            return 5;
        }
        if (num <= 9.0)
        {
            return 10;
        }
        int num2 = Convert.ToInt32(Math.Ceiling(num));
        int digit = num2.ToString().Length - 2;
        int num3 = int.Parse(num2.ToString().Substring(0, 2));
        if (num3 < 11)
        {
            return 12 * GetPow(digit);
        }
        if (num3 < 13)
        {
            return 14 * GetPow(digit);
        }
        if (num3 < 15)
        {
            return 16 * GetPow(digit);
        }
        if (num3 < 17)
        {
            return 18 * GetPow(digit);
        }
        if (num3 < 19)
        {
            return 20 * GetPow(digit);
        }
        if (num3 < 21)
        {
            return 22 * GetPow(digit);
        }
        if (num3 < 23)
        {
            return 24 * GetPow(digit);
        }
        if (num3 < 25)
        {
            return 26 * GetPow(digit);
        }
        if (num3 < 27)
        {
            return 28 * GetPow(digit);
        }
        if (num3 < 29)
        {
            return 30 * GetPow(digit);
        }
        if (num3 < 33)
        {
            return 34 * GetPow(digit);
        }
        if (num3 < 40)
        {
            return 40 * GetPow(digit);
        }
        if (num3 < 50)
        {
            return 50 * GetPow(digit);
        }
        if (num3 < 60)
        {
            return 60 * GetPow(digit);
        }
        if (num3 < 80)
        {
            return 80 * GetPow(digit);
        }
        if (num3 < 95)
        {
            return 100 * GetPow(digit);
        }
        return 100 * GetPow(digit);
    }

    /// <summary>CalculateMaxSectionFrom 方法。</summary>
    public static int CalculateMaxSectionFrom(Dictionary<string, double[]> values)
    {
        return CalculateMaxSectionFrom(values.Select((KeyValuePair<string, double[]> m) => m.Value.Max()).ToArray());
    }

    /// <summary>获取 colorLight。</summary>
    public static System.Drawing.Color GetColorLight(System.Drawing.Color color)
    {
        return GetColorLight(color, 40);
    }

    /// <summary>获取 colorDeep。</summary>
    public static System.Drawing.Color GetColorDeep(System.Drawing.Color color)
    {
        return GetColorLight(color, -40);
    }

    /// <summary>获取 colorLight。</summary>
    public static System.Drawing.Color GetColorLight(System.Drawing.Color color, int scale)
    {
        if (scale > 0)
        {
            return System.Drawing.Color.FromArgb(color.R + (255 - color.R) * scale / 100, color.G + (255 - color.G) * scale / 100, color.B + (255 - color.B) * scale / 100);
        }
        return System.Drawing.Color.FromArgb(color.R + color.R * scale / 100, color.G + color.G * scale / 100, color.B + color.B * scale / 100);
    }

    /// <summary>获取 colorOffset。</summary>
    public static System.Drawing.Color GetColorOffset(System.Drawing.Color color, int offset)
    {
        int num = color.R + offset;
        if (num < 0)
        {
            num = 0;
        }
        if (num > 255)
        {
            num = 255;
        }
        int num2 = color.G + offset;
        if (num2 < 0)
        {
            num2 = 0;
        }
        if (num2 > 255)
        {
            num2 = 255;
        }
        int num3 = color.B + offset;
        if (num3 < 0)
        {
            num3 = 0;
        }
        if (num3 > 255)
        {
            num3 = 255;
        }
        return System.Drawing.Color.FromArgb(num, num2, num3);
    }

    /// <summary>获取 colorLightFive。</summary>
    public static System.Drawing.Color GetColorLightFive(System.Drawing.Color color)
    {
        return GetColorLight(color, 50);
    }



    /// <summary>获取 pointsFrom。</summary>
    public static PointF[] GetPointsFrom(string points, float soureWidth, float sourceHeight, float width, float height, float dx = 0f, float dy = 0f)
    {
        string[] array = points.Split(new char[1]
        {
            ' '
        }, StringSplitOptions.RemoveEmptyEntries);
        PointF[] array2 = new PointF[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            int num = array[i].IndexOf(',');
            float num2 = Convert.ToSingle(array[i].Substring(0, num));
            float num3 = Convert.ToSingle(array[i].Substring(num + 1));
            array2[i] = new PointF(width * (num2 + dx) / soureWidth, height * (num3 + dy) / sourceHeight);
        }
        return array2;
    }

    /// <summary>获取 roundRectange。</summary>
    public static GraphicsPath GetRoundRectange(Rectangle rectangle, int radius, bool topLeft, bool topRight, bool buttomRight, bool buttomLeft)
    {
        GraphicsPath graphicsPath = new GraphicsPath();
        Point pt = new Point(rectangle.X + (topLeft ? radius : 0), rectangle.Y);
        Point pt2 = new Point(rectangle.X + rectangle.Width - 1 - (topRight ? radius : 0), rectangle.Y);
        graphicsPath.AddLine(pt, pt2);
        if (topRight && radius > 0)
        {
            graphicsPath.AddArc(rectangle.X + rectangle.Width - radius * 2 - 1, rectangle.Y, radius * 2, radius * 2, 270f, 90f);
        }
        Point pt3 = new Point(rectangle.X + rectangle.Width - 1, rectangle.Y + (topRight ? radius : 0));
        Point pt4 = new Point(rectangle.X + rectangle.Width - 1, rectangle.Y + rectangle.Height - 1 - (buttomRight ? radius : 0));
        graphicsPath.AddLine(pt3, pt4);
        if (buttomRight && radius > 0)
        {
            graphicsPath.AddArc(rectangle.X + rectangle.Width - radius * 2 - 1, rectangle.Y + rectangle.Height - radius * 2 - 1, radius * 2, radius * 2, 0f, 90f);
        }
        Point pt5 = new Point(rectangle.X + rectangle.Width - 1 - (buttomRight ? radius : 0), rectangle.Y + rectangle.Height - 1);
        Point pt6 = new Point(rectangle.X + (buttomLeft ? radius : 0), rectangle.Y + rectangle.Height - 1);
        graphicsPath.AddLine(pt5, pt6);
        if (buttomLeft && radius > 0)
        {
            graphicsPath.AddArc(rectangle.X, rectangle.Y + rectangle.Height - radius * 2 - 1, radius * 2, radius * 2, 90f, 90f);
        }
        Point pt7 = new Point(rectangle.X, rectangle.Y + rectangle.Height - 1 - (buttomLeft ? radius : 0));
        Point pt8 = new Point(rectangle.X, rectangle.Y + (topLeft ? radius : 0));
        graphicsPath.AddLine(pt7, pt8);
        if (topLeft && radius > 0)
        {
            graphicsPath.AddArc(rectangle.X, rectangle.Y, radius * 2, radius * 2, 180f, 90f);
        }
        return graphicsPath;
    }

    /// <summary>FloatMatchEvaluator 方法。</summary>
    private static string FloatMatchEvaluator(Match m)
    {
        if (m.Value.Length == 5)
        {
            return m.Value.Substring(0, 3);
        }
        return m.Value.Substring(0, 2);
    }

    /// <summary>获取 formatString。</summary>
    public static string GetFormatString(string format, float value, bool remainZero = false)
    {
        try
        {
            string text = string.Format(format, value);
            if (remainZero)
            {
                return text;
            }
            return Regex.Replace(text, "[Ee][+-][0]+", FloatMatchEvaluator);
        }
        catch
        {
            return "Wrong";
        }
    }
}

public class ReferenceAxisConverter : TypeConverter
{
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        return TypeDescriptor.GetProperties(value, attributes);
    }

    /// <summary>获取 propertiesSupported。</summary>
    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
        return true;
    }
}

[TypeConverter(typeof(ReferenceAxisConverter))]
public class ReferenceAxis
{
    private float _max;

    private float _min;

    private string _unit;

    private string _format;

    private System.Drawing.Color _color;

    private UserControl _control;

    private System.Drawing.Pen _pen;

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("最大")]
    [HDescriptionLanguage("获取或设置当前的Y轴的最大值")]
    [Browsable(true)]
    [DefaultValue(100f)]
    public float Max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
            _control?.Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("最小")]
    [HDescriptionLanguage("获取或设置当前的Y轴的最小值")]
    [Browsable(true)]
    [DefaultValue(0f)]
    public float Min
    {
        get
        {
            return _min;
        }
        set
        {
            _min = value;
            _control?.Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("单位")]
    [HDescriptionLanguage("获取或设置当前的Y轴的单位值")]
    [Browsable(true)]
    [DefaultValue("")]
    public string Unit
    {
        get
        {
            return _unit;
        }
        set
        {
            _unit = value;
            _control?.Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色")]
    [HDescriptionLanguage("获取或设置当前的坐标系的颜色信息")]
    [Browsable(true)]
    [DefaultValue(typeof(System.Drawing.Color), "LightGray")]
    public System.Drawing.Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            Brush?.Dispose();
            Brush = new SolidBrush(_color);
            _pen?.Dispose();
            _pen = new System.Drawing.Pen(_color, 1f);
            _control?.Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("格式")]
    [HDescriptionLanguage("获取或设置当前坐标轴数字的格式化信息，默认为 {0}, 直接转字符串")]
    [Browsable(true)]
    [DefaultValue("{0}")]
    public string Format
    {
        get
        {
            return _format;
        }
        set
        {
            _format = value;
            _control?.Invalidate();
        }
    }

    [Browsable(false)]
    public System.Drawing.Brush Brush
    {
        get;
        set;
    }

    public ReferenceAxis()
    {
        Max = 100f;
        Min = 0f;
        Color = System.Drawing.Color.LightGray;
        Format = "{0}";
    }

    public ReferenceAxis(UserControl userControl)
        : this()
    {
        _control = userControl;
    }

    public ReferenceAxis(float max, float min)
    {
        Max = max;
        Min = min;
    }

    /// <summary>获取 pen。</summary>
    public System.Drawing.Pen GetPen()
    {
        if (_pen != null)
        {
            return _pen;
        }
        _pen = new System.Drawing.Pen(Color, 1f);
        return _pen;
    }
}
public class AuxiliaryLine : IDisposable
{
    /// <summary>disposedValue 字段。</summary>
    private bool disposedValue = false;

    public float Value
    {
        get;
        set;
    }

    public float PaintValue
    {
        get;
        set;
    }

    public float PaintValueBackUp
    {
        get;
        set;
    }

    public System.Drawing.Color LineColor
    {
        get;
        set;
    }

    public System.Drawing.Pen PenDash
    {
        get;
        set;
    }

    public System.Drawing.Pen PenSolid
    {
        get;
        set;
    }

    public float LineThickness
    {
        get;
        set;
    }

    public System.Drawing.Brush LineTextBrush
    {
        get;
        set;
    }

    public bool IsLeftFrame
    {
        get;
        set;
    }

    public bool IsDashStyle
    {
        get;
        set;
    } = true;


    /// <summary>获取 pen。</summary>
    public System.Drawing.Pen GetPen()
    {
        return IsDashStyle ? PenDash : PenSolid;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                PenDash?.Dispose();
                PenSolid?.Dispose();
                LineTextBrush?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }
}
public class CurveItemH
{
    /// <summary>数据。</summary>
    public float[] Data = null;

    /// <summary>MarkText 成员。</summary>
    public string[] MarkText = null;

    public float LineThickness
    {
        get;
        set;
    }

    public CurveStyle Style
    {
        get;
        set;
    }

    public System.Drawing.Color LineColor
    {
        get;
        set;
    }

    public int ReferenceAxisIndex
    {
        get;
        set;
    }

    public bool Visible
    {
        get;
        set;
    }

    public bool LineRenderVisiable
    {
        get;
        set;
    }

    public RectangleF TitleRegion
    {
        get;
        set;
    }

    public string RenderFormat
    {
        get;
        set;
    } = "{0}";


    public CurveItemH()
    {
        LineColor = System.Drawing.Color.Red;
        LineThickness = 1f;
        ReferenceAxisIndex = 0;
        Visible = true;
        LineRenderVisiable = true;
        TitleRegion = new RectangleF(0f, 0f, 0f, 0f);
        Style = CurveStyle.LineSegment;
    }
}
public class AuxiliaryLable
{
    public string Text
    {
        get;
        set;
    }

    public System.Drawing.Brush TextBrush
    {
        get;
        set;
    }

    public System.Drawing.Brush TextBack
    {
        get;
        set;
    }

    public float LocationX
    {
        get;
        set;
    }

    public AuxiliaryLable()
    {
        TextBrush = System.Drawing.Brushes.Black;
        TextBack = System.Drawing.Brushes.Transparent;
        LocationX = 0.5f;
    }
}
public class MarkTextH
{
    public string CurveKey
    {
        get;
        set;
    }

    public int Index
    {
        get;
        set;
    }

    public string MarkText
    {
        get;
        set;
    }

    public Image MarkImage
    {
        get;
        set;
    }

    public System.Drawing.Brush TextBrush
    {
        get;
        set;
    }

    public System.Drawing.Brush CircleBrush
    {
        get;
        set;
    }

    public int MarkTextOffect
    {
        get;
        set;
    } = 5;


    public MarkTextPositionStyle PositionStyle
    {
        get;
        set;
    } = MarkTextPositionStyle.Auto;


    public MarkTextH()
    {
        CircleBrush = System.Drawing.Brushes.DodgerBlue;
        TextBrush = System.Drawing.Brushes.Black;
        MarkTextOffect = 5;
    }

    /// <summary>CalculateDirectionFromDataIndex 方法。</summary>
    public static MarkTextPositionStyle CalculateDirectionFromDataIndex(float[] data, int Index)
    {
        float num = (Index == 0) ? data[Index] : data[Index - 1];
        float num2 = (Index == data.Length - 1) ? data[Index] : data[Index + 1];
        if (num < data[Index] && data[Index] < num2)
        {
            return MarkTextPositionStyle.Left;
        }
        if (num > data[Index] && data[Index] > num2)
        {
            return MarkTextPositionStyle.Right;
        }
        if (num <= data[Index] && data[Index] >= num2)
        {
            return MarkTextPositionStyle.Up;
        }
        if (num >= data[Index] && data[Index] <= num2)
        {
            return MarkTextPositionStyle.Down;
        }
        return MarkTextPositionStyle.Up;
    }

    /// <summary>CalculateDirectionFromDataIndex 方法。</summary>
    public static MarkTextPositionStyle CalculateDirectionFromDataIndex(PointF[] data, int Index)
    {
        float num = (Index == 0) ? data[Index].Y : data[Index - 1].Y;
        float num2 = (Index == data.Length - 1) ? data[Index].Y : data[Index + 1].Y;
        if (num < data[Index].Y && data[Index].Y < num2)
        {
            return MarkTextPositionStyle.Left;
        }
        if (num > data[Index].Y && data[Index].Y > num2)
        {
            return MarkTextPositionStyle.Right;
        }
        if (num <= data[Index].Y && data[Index].Y >= num2)
        {
            return MarkTextPositionStyle.Up;
        }
        if (num >= data[Index].Y && data[Index].Y <= num2)
        {
            return MarkTextPositionStyle.Down;
        }
        return MarkTextPositionStyle.Up;
    }
}
internal static class ClientUtils
{
    internal class WeakRefCollection : IList, ICollection, IEnumerable
    {
        internal class WeakRefObject
        {
            private int hash;

            private WeakReference weakHolder;

            /// <summary>IsAlive 成员。</summary>
            internal bool IsAlive => weakHolder.IsAlive;

            /// <summary>Target 成员。</summary>
            internal object Target => weakHolder.Target;

            internal WeakRefObject(object obj)
            {
                weakHolder = new WeakReference(obj);
                hash = obj.GetHashCode();
            }

            public override int GetHashCode()
            {
                return hash;
            }

            public override bool Equals(object obj)
            {
                WeakRefObject weakRefObject = obj as WeakRefObject;
                if (weakRefObject == this)
                {
                    return true;
                }
                if (weakRefObject == null)
                {
                    return false;
                }
                if (weakRefObject.Target != Target && (Target == null || !Target.Equals(weakRefObject.Target)))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>refCheckThreshold 字段。</summary>
        private int refCheckThreshold = int.MaxValue;

        private ArrayList _innerList;

        /// <summary>InnerList 成员。</summary>
        internal ArrayList InnerList => _innerList;

        public int RefCheckThreshold
        {
            get
            {
                return refCheckThreshold;
            }
            set
            {
                refCheckThreshold = value;
            }
        }

        public object this[int index]
        {
            get
            {
                WeakRefObject weakRefObject = InnerList[index] as WeakRefObject;
                if (weakRefObject != null && weakRefObject.IsAlive)
                {
                    return weakRefObject.Target;
                }
                return null;
            }
            set
            {
                InnerList[index] = CreateWeakRefObject(value);
            }
        }

        /// <summary>IsFixedSize 成员。</summary>
        public bool IsFixedSize => InnerList.IsFixedSize;

        /// <summary>数量。</summary>
        public int Count => InnerList.Count;

        object ICollection.SyncRoot => InnerList.SyncRoot;

        public bool IsReadOnly => InnerList.IsReadOnly;

        bool ICollection.IsSynchronized => InnerList.IsSynchronized;

        internal WeakRefCollection()
        {
            _innerList = new ArrayList(4);
        }

        internal WeakRefCollection(int size)
        {
            _innerList = new ArrayList(size);
        }

        /// <summary>ScavengeReferences 方法。</summary>
        public void ScavengeReferences()
        {
            int num = 0;
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                object obj = this[num];
                if (obj == null)
                {
                    InnerList.RemoveAt(num);
                }
                else
                {
                    num++;
                }
            }
        }

        public override bool Equals(object obj)
        {
            WeakRefCollection weakRefCollection = obj as WeakRefCollection;
            if (weakRefCollection == this)
            {
                return true;
            }
            if (weakRefCollection == null || Count != weakRefCollection.Count)
            {
                return false;
            }
            for (int i = 0; i < Count; i++)
            {
                if (InnerList[i] != weakRefCollection.InnerList[i] && (InnerList[i] == null || !InnerList[i].Equals(weakRefCollection.InnerList[i])))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>CreateWeakRefObject 方法。</summary>
        private WeakRefObject CreateWeakRefObject(object value)
        {
            if (value == null)
            {
                return null;
            }
            return new WeakRefObject(value);
        }

        /// <summary>Copy 方法。</summary>
        private static void Copy(WeakRefCollection sourceList, int sourceIndex, WeakRefCollection destinationList, int destinationIndex, int length)
        {
            if (sourceIndex < destinationIndex)
            {
                sourceIndex += length;
                destinationIndex += length;
                while (length > 0)
                {
                    destinationList.InnerList[--destinationIndex] = sourceList.InnerList[--sourceIndex];
                    length--;
                }
            }
            else
            {
                while (length > 0)
                {
                    destinationList.InnerList[destinationIndex++] = sourceList.InnerList[sourceIndex++];
                    length--;
                }
            }
        }

        /// <summary>RemoveByHashCode 方法。</summary>
        public void RemoveByHashCode(object value)
        {
            if (value == null)
            {
                return;
            }
            int hashCode = value.GetHashCode();
            int num = 0;
            while (true)
            {
                if (num < InnerList.Count)
                {
                    if (InnerList[num] != null && InnerList[num].GetHashCode() == hashCode)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            RemoveAt(num);
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            InnerList.Clear();
        }

        /// <summary>判断包含。</summary>
        public bool Contains(object value)
        {
            return InnerList.Contains(CreateWeakRefObject(value));
        }

        /// <summary>RemoveAt 方法。</summary>
        public void RemoveAt(int index)
        {
            InnerList.RemoveAt(index);
        }

        /// <summary>移除。</summary>
        public void Remove(object value)
        {
            InnerList.Remove(CreateWeakRefObject(value));
        }

        /// <summary>IndexOf 方法。</summary>
        public int IndexOf(object value)
        {
            return InnerList.IndexOf(CreateWeakRefObject(value));
        }

        /// <summary>插入。</summary>
        public void Insert(int index, object value)
        {
            InnerList.Insert(index, CreateWeakRefObject(value));
        }

        /// <summary>添加。</summary>
        public int Add(object value)
        {
            if (Count > RefCheckThreshold)
            {
                ScavengeReferences();
            }
            return InnerList.Add(CreateWeakRefObject(value));
        }

        public void CopyTo(Array array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        /// <summary>获取 enumerator。</summary>
        public IEnumerator GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }
    }

    /// <summary>判断是否 CriticalException。</summary>
    public static bool IsCriticalException(Exception ex)
    {
        if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException))
        {
            return ex is AccessViolationException;
        }
        return true;
    }

    /// <summary>判断是否 SecurityOrCriticalException。</summary>
    public static bool IsSecurityOrCriticalException(Exception ex)
    {
        if (!(ex is SecurityException))
        {
            return IsCriticalException(ex);
        }
        return true;
    }

    /// <summary>获取 bitCount。</summary>
    public static int GetBitCount(uint x)
    {
        int num = 0;
        while (x != 0)
        {
            x &= x - 1;
            num++;
        }
        return num;
    }

    /// <summary>判断是否 EnumValid。</summary>
    public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
    {
        return value >= minValue && value <= maxValue;
    }

    /// <summary>判断是否 EnumValid。</summary>
    public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue, int maxNumberOfBitsOn)
    {
        return value >= minValue && value <= maxValue && GetBitCount((uint)value) <= maxNumberOfBitsOn;
    }

    /// <summary>判断是否 EnumValid_Masked。</summary>
    public static bool IsEnumValid_Masked(Enum enumValue, int value, uint mask)
    {
        return (value & mask) == value;
    }

    /// <summary>判断是否 EnumValid_NotSequential。</summary>
    public static bool IsEnumValid_NotSequential(Enum enumValue, int value, params int[] enumValues)
    {
        for (int i = 0; i < enumValues.Length; i++)
        {
            if (enumValues[i] == value)
            {
                return true;
            }
        }
        return false;
    }
}
public class CurveHelperH
{
    /// <summary>DrawTextByPoint 方法。</summary>
    public static void DrawTextByPoint(Graphics g, string text, PointF center, Font font, System.Drawing.Brush brush, MarkTextPositionStyle markTextPosition, int markTextOffect)
    {
        if (!string.IsNullOrEmpty(text))
        {
            switch (markTextPosition)
            {
                case MarkTextPositionStyle.Left:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height - (float)markTextOffect, 100 - markTextOffect, font.Height + markTextOffect), FromHelper.StringFormatRight);
                    break;
                case MarkTextPositionStyle.Up:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height - (float)markTextOffect, 200f, font.Height + 2), FromHelper.StringFormatCenter);
                    break;
                case MarkTextPositionStyle.Right:
                    g.DrawString(text, font, brush, new RectangleF(center.X + (float)markTextOffect, center.Y - (float)font.Height - (float)markTextOffect, 100f, font.Height + markTextOffect), FromHelper.StringFormatLeft);
                    break;
                case MarkTextPositionStyle.Down:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y + (float)markTextOffect, 200f, font.Height + 2), FromHelper.StringFormatCenter);
                    break;
                case MarkTextPositionStyle.Center:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height, 200f, font.Height * 2), FromHelper.StringFormatCenter);
                    break;
            }
        }
    }

    /// <summary>DrawMarkTextHPoint 方法。</summary>
    public static void DrawMarkTextHPoint(Graphics g, MarkTextH markText, PointF center, Font font, MarkTextPositionStyle markTextPosition)
    {
        if (markText == null)
        {
            return;
        }
        g.FillEllipse(markText.CircleBrush, new RectangleF(center.X - 3f, center.Y - 3f, 6f, 6f));
        if (!string.IsNullOrEmpty(markText.MarkText))
        {
            DrawTextByPoint(g, markText.MarkText, center, font, markText.TextBrush, markTextPosition, markText.MarkTextOffect);
        }
        if (markText.MarkImage != null)
        {
            switch (markTextPosition)
            {
                case MarkTextPositionStyle.Left:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)markText.MarkImage.Width - (float)markText.MarkTextOffect, center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Up:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Right:
                    g.DrawImage(markText.MarkImage, new PointF(center.X + (float)markText.MarkTextOffect, center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Down:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y + (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Center:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y - (float)(markText.MarkImage.Height / 2)));
                    break;
            }
        }
    }

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern int GdipGetPenDashStyle(HandleRef pen, out int dashstyle);
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern int GdipSetPenDashStyle(HandleRef pen, int dashstyle);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern int GdipGetPenDashCount(HandleRef pen, out int dashcount);


    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern int GdipGetPenDashArray(HandleRef pen, IntPtr memorydash, int count);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern int GdipSetPenDashArray(HandleRef pen, HandleRef memorydash, int count);


    internal sealed class SR
    {
        internal const string CantTellPrinterName = "CantTellPrinterName";

        internal const string CantChangeImmutableObjects = "CantChangeImmutableObjects";

        internal const string CantMakeIconTransparent = "CantMakeIconTransparent";

        internal const string ColorNotSystemColor = "ColorNotSystemColor";

        internal const string DotNET_ComponentType = "DotNET_ComponentType";

        internal const string GdiplusAborted = "GdiplusAborted";

        internal const string GdiplusAccessDenied = "GdiplusAccessDenied";

        internal const string GdiplusCannotCreateGraphicsFromIndexedPixelFormat = "GdiplusCannotCreateGraphicsFromIndexedPixelFormat";

        internal const string GdiplusCannotSetPixelFromIndexedPixelFormat = "GdiplusCannotSetPixelFromIndexedPixelFormat";

        internal const string GdiplusDestPointsInvalidParallelogram = "GdiplusDestPointsInvalidParallelogram";

        internal const string GdiplusDestPointsInvalidLength = "GdiplusDestPointsInvalidLength";

        internal const string GdiplusFileNotFound = "GdiplusFileNotFound";

        internal const string GdiplusFontFamilyNotFound = "GdiplusFontFamilyNotFound";

        internal const string GdiplusFontStyleNotFound = "GdiplusFontStyleNotFound";

        internal const string GdiplusGenericError = "GdiplusGenericError";

        internal const string GdiplusInsufficientBuffer = "GdiplusInsufficientBuffer";

        internal const string GdiplusInvalidParameter = "GdiplusInvalidParameter";

        internal const string GdiplusInvalidRectangle = "GdiplusInvalidRectangle";

        internal const string GdiplusInvalidSize = "GdiplusInvalidSize";

        internal const string GdiplusOutOfMemory = "GdiplusOutOfMemory";

        internal const string GdiplusNotImplemented = "GdiplusNotImplemented";

        internal const string GdiplusNotInitialized = "GdiplusNotInitialized";

        internal const string GdiplusNotTrueTypeFont = "GdiplusNotTrueTypeFont";

        internal const string GdiplusNotTrueTypeFont_NoName = "GdiplusNotTrueTypeFont_NoName";

        internal const string GdiplusObjectBusy = "GdiplusObjectBusy";

        internal const string GdiplusOverflow = "GdiplusOverflow";

        internal const string GdiplusPropertyNotFoundError = "GdiplusPropertyNotFoundError";

        internal const string GdiplusPropertyNotSupportedError = "GdiplusPropertyNotSupportedError";

        internal const string GdiplusUnknown = "GdiplusUnknown";

        internal const string GdiplusUnknownImageFormat = "GdiplusUnknownImageFormat";

        internal const string GdiplusUnsupportedGdiplusVersion = "GdiplusUnsupportedGdiplusVersion";

        internal const string GdiplusWrongState = "GdiplusWrongState";

        internal const string GlobalAssemblyCache = "GlobalAssemblyCache";

        internal const string GraphicsBufferCurrentlyBusy = "GraphicsBufferCurrentlyBusy";

        internal const string GraphicsBufferQueryFail = "GraphicsBufferQueryFail";

        internal const string ToolboxItemLocked = "ToolboxItemLocked";

        internal const string ToolboxItemInvalidPropertyType = "ToolboxItemInvalidPropertyType";

        internal const string ToolboxItemValueNotSerializable = "ToolboxItemValueNotSerializable";

        internal const string ToolboxItemInvalidKey = "ToolboxItemInvalidKey";

        internal const string IllegalState = "IllegalState";

        internal const string InterpolationColorsColorBlendNotSet = "InterpolationColorsColorBlendNotSet";

        internal const string InterpolationColorsCommon = "InterpolationColorsCommon";

        internal const string InterpolationColorsInvalidColorBlendObject = "InterpolationColorsInvalidColorBlendObject";

        internal const string InterpolationColorsInvalidStartPosition = "InterpolationColorsInvalidStartPosition";

        internal const string InterpolationColorsInvalidEndPosition = "InterpolationColorsInvalidEndPosition";

        internal const string InterpolationColorsLength = "InterpolationColorsLength";

        internal const string InterpolationColorsLengthsDiffer = "InterpolationColorsLengthsDiffer";

        internal const string InvalidArgument = "InvalidArgument";

        internal const string InvalidBoundArgument = "InvalidBoundArgument";

        internal const string InvalidClassName = "InvalidClassName";

        internal const string InvalidColor = "InvalidColor";

        internal const string InvalidDashPattern = "InvalidDashPattern";

        internal const string InvalidEx2BoundArgument = "InvalidEx2BoundArgument";

        internal const string InvalidFrame = "InvalidFrame";

        internal const string InvalidGDIHandle = "InvalidGDIHandle";

        internal const string InvalidImage = "InvalidImage";

        internal const string InvalidLowBoundArgumentEx = "InvalidLowBoundArgumentEx";

        internal const string InvalidPermissionLevel = "InvalidPermissionLevel";

        internal const string InvalidPermissionState = "InvalidPermissionState";

        internal const string InvalidPictureType = "InvalidPictureType";

        internal const string InvalidPrinterException_InvalidPrinter = "InvalidPrinterException_InvalidPrinter";

        internal const string InvalidPrinterException_NoDefaultPrinter = "InvalidPrinterException_NoDefaultPrinter";

        internal const string InvalidPrinterHandle = "InvalidPrinterHandle";

        internal const string ValidRangeX = "ValidRangeX";

        internal const string ValidRangeY = "ValidRangeY";

        internal const string NativeHandle0 = "NativeHandle0";

        internal const string NoDefaultPrinter = "NoDefaultPrinter";

        internal const string NotImplemented = "NotImplemented";

        internal const string PDOCbeginPrintDescr = "PDOCbeginPrintDescr";

        internal const string PDOCdocumentNameDescr = "PDOCdocumentNameDescr";

        internal const string PDOCdocumentPageSettingsDescr = "PDOCdocumentPageSettingsDescr";

        internal const string PDOCendPrintDescr = "PDOCendPrintDescr";

        internal const string PDOCoriginAtMarginsDescr = "PDOCoriginAtMarginsDescr";

        internal const string PDOCprintControllerDescr = "PDOCprintControllerDescr";

        internal const string PDOCprintPageDescr = "PDOCprintPageDescr";

        internal const string PDOCprinterSettingsDescr = "PDOCprinterSettingsDescr";

        internal const string PDOCqueryPageSettingsDescr = "PDOCqueryPageSettingsDescr";

        internal const string PrintDocumentDesc = "PrintDocumentDesc";

        internal const string PrintingPermissionBadXml = "PrintingPermissionBadXml";

        internal const string PrintingPermissionAttributeInvalidPermissionLevel = "PrintingPermissionAttributeInvalidPermissionLevel";

        internal const string PropertyValueInvalidEntry = "PropertyValueInvalidEntry";

        internal const string PSizeNotCustom = "PSizeNotCustom";

        internal const string ResourceNotFound = "ResourceNotFound";

        internal const string TargetNotPrintingPermission = "TargetNotPrintingPermission";

        internal const string TextParseFailedFormat = "TextParseFailedFormat";

        internal const string TriStateCompareError = "TriStateCompareError";

        internal const string toStringIcon = "toStringIcon";

        internal const string toStringNone = "toStringNone";

        internal const string DCTypeInvalid = "DCTypeInvalid";

        private static SR loader;

        private ResourceManager resources;

        /// <summary>Culture 字段。</summary>
        private static CultureInfo Culture => null;

        /// <summary>Resources 成员。</summary>
        public static ResourceManager Resources => GetLoader().resources;

        internal SR()
        {
            resources = new ResourceManager("System.Drawing.Res", GetType().Assembly);
        }

        /// <summary>获取 loader。</summary>
        private static SR GetLoader()
        {
            if (loader == null)
            {
                SR value = new SR();
                Interlocked.CompareExchange(ref loader, value, null);
            }
            return loader;
        }

        /// <summary>获取 string。</summary>
        public static string GetString(string name, params object[] args)
        {
            SR sR = GetLoader();
            if (sR == null)
            {
                return null;
            }
            string @string = sR.resources.GetString(name, Culture);
            if (args != null && args.Length != 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string text = args[i] as string;
                    if (text != null && text.Length > 1024)
                    {
                        args[i] = text.Substring(0, 1021) + "...";
                    }
                }
                return string.Format(CultureInfo.CurrentCulture, @string, args);
            }
            return @string;
        }

        /// <summary>获取 string。</summary>
        public static string GetString(string name)
        {
            return GetLoader()?.resources.GetString(name, Culture);
        }

        /// <summary>获取 string。</summary>
        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        /// <summary>获取 object。</summary>
        public static object GetObject(string name)
        {
            return GetLoader()?.resources.GetObject(name, Culture);
        }
    }
    /// <summary>StatusException 方法。</summary>
    internal static Exception StatusException(int status)
    {
        switch (status)
        {
            case 1:
                return new ExternalException(SR.GetString("GdiplusGenericError"), -2147467259);
            case 2:
                return new ArgumentException(SR.GetString("GdiplusInvalidParameter"));
            case 3:
                return new OutOfMemoryException(SR.GetString("GdiplusOutOfMemory"));
            case 4:
                return new InvalidOperationException(SR.GetString("GdiplusObjectBusy"));
            case 5:
                return new OutOfMemoryException(SR.GetString("GdiplusInsufficientBuffer"));
            case 6:
                return new NotImplementedException(SR.GetString("GdiplusNotImplemented"));
            case 7:
                return new ExternalException(SR.GetString("GdiplusGenericError"), -2147467259);
            case 8:
                return new InvalidOperationException(SR.GetString("GdiplusWrongState"));
            case 9:
                return new ExternalException(SR.GetString("GdiplusAborted"), -2147467260);
            case 10:
                return new FileNotFoundException(SR.GetString("GdiplusFileNotFound"));
            case 11:
                return new OverflowException(SR.GetString("GdiplusOverflow"));
            case 12:
                return new ExternalException(SR.GetString("GdiplusAccessDenied"), -2147024891);
            case 13:
                return new ArgumentException(SR.GetString("GdiplusUnknownImageFormat"));
            case 19:
                return new ArgumentException(SR.GetString("GdiplusPropertyNotFoundError"));
            case 20:
                return new ArgumentException(SR.GetString("GdiplusPropertyNotSupportedError"));
            case 14:
                return new ArgumentException(SR.GetString("GdiplusFontFamilyNotFound", "?"));
            case 15:
                return new ArgumentException(SR.GetString("GdiplusFontStyleNotFound", "?", "?"));
            case 16:
                return new ArgumentException(SR.GetString("GdiplusNotTrueTypeFont_NoName"));
            case 17:
                return new ExternalException(SR.GetString("GdiplusUnsupportedGdiplusVersion"), -2147467259);
            case 18:
                return new ExternalException(SR.GetString("GdiplusNotInitialized"), -2147467259);
            default:
                return new ExternalException(SR.GetString("GdiplusUnknown"), -2147418113);
        }
    }
    public float[] DashPattern
    {
        get
        {
            int dashcount = 0;
            int num = GdipGetPenDashCount(new HandleRef(this, NativePen), out dashcount);
            if (num != 0)
            {
                throw StatusException(num);
            }
            int num2 = dashcount;
            IntPtr intPtr = Marshal.AllocHGlobal(checked(4 * num2));
            num = GdipGetPenDashArray(new HandleRef(this, NativePen), intPtr, num2);
            try
            {
                if (num != 0)
                {
                    throw StatusException(num);
                }
                float[] array = new float[num2];
                Marshal.Copy(intPtr, array, 0, num2);
                return array;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
        set
        {
            if (immutable)
            {
                throw new ArgumentException(SR.GetString("CantChangeImmutableObjects", "Pen"));
            }
            if (value == null || value.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidDashPattern"));
            }
            int num = value.Length;
            IntPtr intPtr = Marshal.AllocHGlobal(checked(4 * num));
            try
            {
                Marshal.Copy(value, 0, intPtr, num);
                int num2 = GdipSetPenDashArray(new HandleRef(this, NativePen), new HandleRef(intPtr, intPtr), num);
                if (num2 != 0)
                {
                    throw StatusException(num2);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
    }
    /// <summary>EnsureValidDashPattern 方法。</summary>
    private void EnsureValidDashPattern()
    {
        int dashcount = 0;
        int num = GdipGetPenDashCount(new HandleRef(this, NativePen), out dashcount);
        if (num != 0)
        {
            throw StatusException(num);
        }
        if (dashcount == 0)
        {
            DashPattern = new float[1]
            {
            1f
            };
        }
    }
    private IntPtr nativePen;

    [Browsable(false)]
    /// <summary>NativePen 成员。</summary>
    /// <summary>NativePen 字段。</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal IntPtr NativePen => nativePen;
    private bool immutable;
    public System.Drawing.Drawing2D.DashStyle DashStyle
    {
        get
        {
            int dashstyle = 0;
            int num = GdipGetPenDashStyle(new HandleRef(this, NativePen), out dashstyle);
            if (num != 0)
            {
                throw StatusException(num);
            }
            return (System.Drawing.Drawing2D.DashStyle)dashstyle;
        }
        set
        {
            if (!ClientUtils.IsEnumValid(value, (int)value, 0, 5))
            {
                throw new InvalidEnumArgumentException("value", (int)value, typeof(System.Drawing.Drawing2D.DashStyle));
            }
            if (immutable)
            {
                throw new ArgumentException(SR.GetString("CantChangeImmutableObjects", "Pen"));
            }
            int num = GdipSetPenDashStyle(new HandleRef(this, NativePen), (int)value);
            if (num != 0)
            {
                throw StatusException(num);
            }
            if (value == System.Drawing.Drawing2D.DashStyle.Custom)
            {
                EnsureValidDashPattern();
            }
        }
    }
    /// <summary>DrawLineCore 方法。</summary>
    public static void DrawLineCore(Graphics g, CurveItemH line, List<PointF> listPoints, int pointsRadius, float referenceY = -1f)
    {
        if (listPoints.Count > 1)
        {
            using (System.Drawing.Pen pen = new System.Drawing.Pen(line.LineColor, line.LineThickness))
            {
                if (line.Style == CurveStyle.LineSegment)
                {
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.Curve)
                {
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineDot)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveDot)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineDash)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveDash)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineLongDath)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[2]
                    {
                        5f,
                        5f
                    };
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveLongDath)
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = new float[2]
                    {
                        5f,
                        5f
                    };
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.Section)
                {
                    g.DrawCurve(pen, listPoints.ToArray());
                    if (listPoints.Count > 0)
                    {
                        GraphicsPath graphicsPath = new GraphicsPath();
                        graphicsPath.AddCurve(listPoints.ToArray());
                        graphicsPath.AddLines(new PointF[4]
                        {
                            listPoints[listPoints.Count - 1],
                            new PointF(listPoints[listPoints.Count - 1].X, referenceY),
                            new PointF(listPoints[0].X, referenceY),
                            new PointF(listPoints[0].X, listPoints[0].Y)
                        });
                        using (System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(64, pen.Color)))
                        {
                            g.FillPath(brush, graphicsPath);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < listPoints.Count - 1; i++)
                    {
                        PointF pointF = new PointF(listPoints[i + 1].X, listPoints[i].Y);
                        g.DrawLine(pen, listPoints[i], pointF);
                        if (line.Style == CurveStyle.StepLine)
                        {
                            g.DrawLine(pen, pointF, listPoints[i + 1]);
                        }
                    }
                }
            }
            if (pointsRadius > 0)
            {
                using (System.Drawing.Brush brush2 = new SolidBrush(line.LineColor))
                {
                    for (int j = 0; j < listPoints.Count; j++)
                    {
                        g.FillEllipse(brush2, listPoints[j].X - (float)pointsRadius, listPoints[j].Y - (float)pointsRadius, pointsRadius * 2, pointsRadius * 2);
                    }
                }
            }
        }
    }
}
public class MarkSectionBaseH
{
    public int StartIndex
    {
        get;
        set;
    }

    public int EndIndex
    {
        get;
        set;
    }
}
public class MarkForeSectionH : MarkSectionBaseH
{
    public float StartHeight
    {
        get;
        set;
    }

    public float Height
    {
        get;
        set;
    }

    public bool IsRenderTimeText
    {
        get;
        set;
    }

    public System.Drawing.Pen LinePen
    {
        get;
        set;
    }

    public System.Drawing.Brush FontBrush
    {
        get;
        set;
    }

    public string MarkText
    {
        get;
        set;
    }

    public Dictionary<string, string> CursorTexts
    {
        get;
        set;
    }

    public MarkForeSectionH()
    {
        LinePen = Pens.Cyan;
        FontBrush = System.Drawing.Brushes.Yellow;
        IsRenderTimeText = true;
        CursorTexts = new Dictionary<string, string>();
    }
}
public class MarkLineH
{
    public System.Drawing.Brush TextBrush
    {
        get;
        set;
    }

    public System.Drawing.Brush CircleBrush
    {
        get;
        set;
    }

    public System.Drawing.Pen LinePen
    {
        get;
        set;
    }

    public PointF[] Points
    {
        get;
        set;
    }

    public string[] Marks
    {
        get;
        set;
    }

    public bool IsLineClosed
    {
        get;
        set;
    }

    public bool IsLeftFrame
    {
        get;
        set;
    }

    public MarkLineH()
    {
        TextBrush = System.Drawing.Brushes.DodgerBlue;
        CircleBrush = System.Drawing.Brushes.DodgerBlue;
        LinePen = Pens.DodgerBlue;
        IsLeftFrame = true;
    }
}
public class MarkImageH
{
    public int Index
    {
        get;
        set;
    }

    public float OffsetY
    {
        get;
        set;
    }

    public Image MarkImage
    {
        get;
        set;
    }

    public ContentAlignment ReferencePoint
    {
        get;
        set;
    }

    public bool ScaleEnable
    {
        get;
        set;
    }

    public MarkImageH()
    {
        ReferencePoint = ContentAlignment.TopLeft;
        ScaleEnable = true;
    }
}
public class MarkBackSectionH : MarkSectionBaseH
{
    public System.Drawing.Color BackColor
    {
        get;
        set;
    }

    public string MarkText
    {
        get;
        set;
    }

    public MarkBackSectionH()
    {
        BackColor = System.Drawing.Color.FromArgb(52, 52, 52);
    }
}

public class ReferenceAxisCollection : Collection<ReferenceAxis>
{
    private HCurveHistory CurveHistoryH;

    public ReferenceAxisCollection(HCurveHistory CurveHistoryH)
    {
        this.CurveHistoryH = CurveHistoryH;
    }

    /// <summary>InsertItem 方法。</summary>
    protected override void InsertItem(int index, ReferenceAxis item)
    {
        base.InsertItem(index, item);
        CurveHistoryH?.Invalidate();
    }

    /// <summary>设置 item。</summary>
    protected override void SetItem(int index, ReferenceAxis item)
    {
        base.SetItem(index, item);
        CurveHistoryH?.Invalidate();
    }

    /// <summary>ClearItems 方法。</summary>
    protected override void ClearItems()
    {
        base.ClearItems();
        CurveHistoryH?.Invalidate();
    }

    /// <summary>RemoveItem 方法。</summary>
    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        CurveHistoryH?.Invalidate();
    }
}

public class CurveRangeH
{
    /// <summary>Upper 成员。</summary>
    public float[] Upper = null;

    /// <summary>Lower 成员。</summary>
    public float[] Lower = null;

    public float LineThickness
    {
        get;
        set;
    }

    public CurveStyle Style
    {
        get;
        set;
    }

    public System.Drawing.Color LineColor
    {
        get;
        set;
    }

    public int ReferenceAxisIndex
    {
        get;
        set;
    }

    public CurveRangeH()
    {
        LineColor = System.Drawing.Color.Red;
        LineThickness = 1f;
        ReferenceAxisIndex = 0;
        Style = CurveStyle.LineSegment;
    }
}

public class MarkValue
{
    [HDescriptionLanguage("获取或设置当前的值数据")]
    public float Value
    {
        get;
        set;
    }

    [HDescriptionLanguage("获取或设置当前的颜色信息")]
    public System.Drawing.Color Color
    {
        get;
        set;
    }

    [HDescriptionLanguage("获取或设置当前的刻度宽度信息")]
    public float LineWidth
    {
        get;
        set;
    }

    public MarkValue()
    {
        Value = 0f;
        Color = System.Drawing.Color.DodgerBlue;
        LineWidth = 1f;
    }
}
public class MarkValueCollection : Collection<MarkValue>
{
    public Control control
    {
        get;
        internal set;
    }

    /// <summary>InsertItem 方法。</summary>
    protected override void InsertItem(int index, MarkValue item)
    {
        base.InsertItem(index, item);
        control?.Invalidate();
    }

    /// <summary>设置 item。</summary>
    protected override void SetItem(int index, MarkValue item)
    {
        base.SetItem(index, item);
        control?.Invalidate();
    }

    /// <summary>ClearItems 方法。</summary>
    protected override void ClearItems()
    {
        base.ClearItems();
        control?.Invalidate();
    }

    /// <summary>RemoveItem 方法。</summary>
    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        control?.Invalidate();
    }
}
public class PieItemH
{
    public string Name
    {
        get;
        set;
    }

    public int Value
    {
        get;
        set;
    }

    public System.Drawing.Color PieColor
    {
        get;
        set;
    }

    public System.Drawing.Color FontColor
    {
        get;
        set;
    }

    public System.Drawing.Color LineColor
    {
        get;
        set;
    }

    public PieItemH()
    {
        PieColor = System.Drawing.Color.DodgerBlue;
        LineColor = System.Drawing.Color.DimGray;
        FontColor = System.Drawing.Color.DimGray;
    }
}
}