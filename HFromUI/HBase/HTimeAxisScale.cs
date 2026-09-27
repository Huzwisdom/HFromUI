using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public class HTimeAxisScale
    {
        /// <summary>TimeScales 成员。</summary>
        public string[] TimeScales =new string[] { "1ms","5ms","10ms","20ms","50ms","500ms","1s","5s","10s","1min","5min","10min","1h","5h","10h","1d","10d","100d"};
        /// <summary>ValueScales 成员。</summary>
        public HDouble[] ValueScales = new HDouble[] { 0.00000001d, 0.0000001d, 0.000001d, 0.00001d, 0.0001d, 0.001d, 0.01d, 0.1d, 0.5d, 1d, 2d, 5d, 10d, 50d, 100d, 200d, 500d, 1000d, 5000d, 10000d, 100000d, 1000000d, 10000000d, 100000000d, };


        /// <summary>HTimeValues 成员。</summary>
        public List<HTimeValue> HTimeValues = new List<HTimeValue>();
        /// <summary>ValueTotalMinorSegments 成员。</summary>
        public int ValueTotalMinorSegments { set; get; }
        /// <summary>ValueMinorStep 成员。</summary>
        public HDouble ValueMinorStep { get; private set; }
        /// <summary>ValueMin 成员。</summary>
        public HDouble ValueMin { get;private set; }

        /// <summary>ValueMax 成员。</summary>
        public HDouble ValueMax { get;private set;}

        /// <summary>TimeEnd 成员。</summary>
        public DateTime TimeEnd { get; set; }
        /// <summary>TimeStart 成员。</summary>
        public DateTime TimeStart { get; set; }
        /// <summary>TimeMinorStep 成员。</summary>
        public TimeSpan TimeMinorStep { get;private set; }

        /// <summary>LineShortLengthX 成员。</summary>
        public HDouble LineShortLengthX { set; get; }
        /// <summary>LineShortrLengthY 成员。</summary>
        public HDouble LineShortrLengthY { set; get; }



        /// <summary>LineDirectionX 成员。</summary>
        public bool LineDirectionX { set; get; }

        /// <summary>LineDirectionY 成员。</summary>
        public bool LineDirectionY { set; get; }

        /// <summary>LineLongLengthX 成员。</summary>
        public HDouble LineLongLengthX { set; get; }
        /// <summary>LineLongLengthY 成员。</summary>
        public HDouble LineLongLengthY { set; get; }

        public HPoint Location { get; set; }//要画的屏幕起点

        public Size Size { set; get; }//要画的屏幕的长和宽

        public List<HTimeScaleX> HTimeScaleXs { get; private set; }//用于画小线段或是长线段的，在X轴上

        public List<HValueScaleY> HValueScaleY { get; private set; }//用于画小线段或是长线段的，用在Y轴上

    }

    public struct HTimeScaleX
    { 
        /// <summary>LineStart 成员。</summary>
        public HPoint LineStart  { get; set; }
        /// <summary>LineEnd 成员。</summary>
        public HPoint LineEnd { get; set; }
        public DateTime  Time { get; set; }//当前时间

    }

    public struct HValueScaleY
    {
        /// <summary>LineStart 成员。</summary>
        public HPoint LineStart { get; set; }
        /// <summary>LineEnd 成员。</summary>
        public HPoint LineEnd { get; set; }
        public HDouble Value  { get; set; }//当前值

    }
}
