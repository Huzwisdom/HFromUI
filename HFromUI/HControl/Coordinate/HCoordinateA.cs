using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HMath;
using HFromUI.HFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HFrom;

namespace HFromUI.HControl.Coordinate
{
    using HFromUI.HLangage;
    using HFromUI.HColor;
    using HFromUI.HEnum;
    using HFromUI.HFrom.From;
    public partial class HCoordinateA : UserControl
    {

        private Timer timer;
        public HCoordinateA()
        {
            InitializeComponent();
                if (!this.DesignMode&& LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                HCoordinateDraw.CoordinateDrawList.Name = this.Name;
                HCoordinateDraw.CoordinateDrawList.SelectValueChanged += (o, e) => { Coordinate_ValueChanged(o, e); };
                aPropertyGridSet.PropertyValueChanged += (o, e) => { PropertyGrid_PropertyValueChanged(o, e); };
                aPropertyGridSet.SetRowHeight(12);

                LoadTranslationLanguage();

                if (timer == null)
                {
                    timer = new Timer();
                    timer.Interval = 100;
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }

                HTranslation.TranslationLanguageChanged += HTranslation_TranslationLanguageChanged;

            }
        }

        private void HTranslation_TranslationLanguageChanged(object sender, EventArgs e)
        {
            LoadTranslationLanguage();
        }


        /// <summary>LoadTranslationLanguage 方法。</summary>
        public void LoadTranslationLanguage()
        {
            toolSbtn_Mouse.Text = HTranslation.GetContent("鼠标");
            toolSbtn_Select.Text = HTranslation.GetContent("选择");
            toolSbtn_DecimalPlaces.Text = HTranslation.GetContent("小数点设置");
            toolSbtn_Point.Text = HTranslation.GetContent("画点");
            toolSbtn_Line.Text = HTranslation.GetContent("画线");
            toolSbtn_3pArc.Text = HTranslation.GetContent("画圆弧");
            toolSbtn_Continuous.Text = HTranslation.GetContent("是否连续画线");
            toolSbtn_Number.Text = HTranslation.GetContent("捕捉");
            toolSbtn_Layer.Text = HTranslation.GetContent("图层");
            toolSbtn_DXF.Text = HTranslation.GetContent("导入DXF");


            toolSbtn_Enable.Text = HTranslation.GetContent("使能绘图");


            toolSbtn_Open.Text = HTranslation.GetContent("打开文件");
            toolSbtn_Save.Text = HTranslation.GetContent("保存文件");
            toolSbtn_Del.Text = HTranslation.GetContent("删除所选择的线");
            toolSbtn_Forward.Text = HTranslation.GetContent("向前撤回");
            toolSbtn_backward.Text = HTranslation.GetContent("向后撤回");
            toolSbtn_Top.Text = HTranslation.GetContent("置顶");
            toolSbtn_Bottom.Text = HTranslation.GetContent("置底");

            toolSbtn_Format.Text = HTranslation.GetContent("设置幅面");
            toolSbtn_GoFormat.Text = HTranslation.GetContent("去幅面中心位置");
            toolSbtn_Appropriate.Text = HTranslation.GetContent("去合适的位置");
            toolSbtn_Last.Text = HTranslation.GetContent("上一个");
            toolSbtn_Next.Text = HTranslation.GetContent("下一个");
            toolSbtn_Group.Text = HTranslation.GetContent("群组");
            toolSbtn_UnGroup.Text = HTranslation.GetContent("解除群组");
            toolSbtn_ShowPrestore.Text = HTranslation.GetContent("是否显示预存");
            toolSbtn_Prestore.Text = HTranslation.GetContent("保存预存");
            toolSbtn_Set.Text = HTranslation.GetContent("设置");

            toolSbtn_GCode.Text = HTranslation.GetContent("G代码");
            toolSbtn_Start.Text = HTranslation.GetContent("模拟启动");
            toolSbtn_Stop.Text = HTranslation.GetContent("模拟停止");

            toolSbtn_GoLast.Text = HTranslation.GetContent("线段前移");
            toolSbtn_GoNext.Text = HTranslation.GetContent("线段后移");
        }

        /// <summary>IsLoad 字段。</summary>
        private bool IsLoad = false;

        /// <summary>LoadIni 方法。</summary>
        public void LoadIni(bool isloadC = false)
        {
            if (!IsLoad && (this.Name != nameof(HCoordinateA) || isloadC))
            {
                IsLoad = true;
                // HCoordinateDraw.CoordinateDrawList.IsContinuous= Convert.ToBoolean( HAppData.Get(this.Name+"IsContinuous",false));


                HCoordinateDraw.CoordinateDrawList.IsEnableLayer = Convert.ToBoolean(HAppData.Get(this.Name + "IsEnableLayer", false));
                HCoordinateDraw.CoordinateDrawList.Layer = Convert.ToInt32(HAppData.Get(this.Name + "Layer", 0));

                HCoordinateDraw.CoordinateScreen.SetDecimalPlaces = Convert.ToInt32(HAppData.Get(this.Name + "SetDecimalPlaces", 5));

                HCoordinateDraw.CoordinateDrawList.IsEnableFormat = Convert.ToBoolean(HAppData.Get(this.Name + "IsEnableFormat", false));
                HCoordinateDraw.CoordinateDrawList.FormatX1 = Convert.ToDouble(HAppData.Get(this.Name + "FormatX1", 0));
                HCoordinateDraw.CoordinateDrawList.FormatX2 = Convert.ToDouble(HAppData.Get(this.Name + "FormatX2", 0));
                HCoordinateDraw.CoordinateDrawList.FormatY1 = Convert.ToDouble(HAppData.Get(this.Name + "FormatY1", 0));
                HCoordinateDraw.CoordinateDrawList.FormatY2 = Convert.ToDouble(HAppData.Get(this.Name + "FormatY2", 0));
                HAppData.Save();
            }
        }
        /// <summary>Timer_Tick 方法。</summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadIni();
            if (HCoordinateDraw.CoordinateDrawList.IsRunShapes)
            {
                toolSbtn_Start.BackColor = HColors.Greens.Basil;
                toolSbtn_Stop.BackColor = HColors.Reds.BritishRed;
                HCoordinateDraw.CoordinateScreen.EnableDraw = false;
                toolSbtn_Enable.Enabled= toolSbtn_Layer.Enabled = false;
            }
            else
            {
                toolSbtn_Start.BackColor = HColors.Whites.White;
                toolSbtn_Stop.BackColor = HColors.Whites.White;
                toolSbtn_Enable.Enabled = toolSbtn_Layer.Enabled = true;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectGUID.Count > 1)
            {
                toolSbtn_Group.Enabled = true;
            }
            else
            {
                toolSbtn_Group.Enabled = false;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase is HDrawGroup)
            {
                toolSbtn_UnGroup.Enabled = true;
            }
            else
            {
                toolSbtn_UnGroup.Enabled = false;
            }
            if (HCoordinateDraw.CoordinateDrawList.IsCapture)
            {
                toolSbtn_Number.BackColor = HColors.Grays.AshGray;
            }
            else
            {
                toolSbtn_Number.BackColor = HColors.Whites.White;
            }
            if (HCoordinateDraw.CoordinateDrawList.IsContinuous)
            {
                toolSbtn_Continuous.BackColor = HColors.Grays.AshGray;
            }
            else
            {
                toolSbtn_Continuous.BackColor = HColors.Whites.White;
            }
            toolSbtn_Forward.Enabled = HCoordinateDraw.CoordinateDrawList.DrawChangeForward();
            toolSbtn_backward.Enabled=HCoordinateDraw.CoordinateDrawList.DrawChangeBackward();
            if (HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                toolSbtn_Enable.BackColor = HColors.Oranges.Apricot;
                aPropertyGridSet.Enabled = true;
            }
            else
            {
                if (aPropertyGridSet.Enabled)
                {
                    HCoordinateDraw.Clear();
                }
                aPropertyGridSet.Enabled = false;
                HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.None;
                toolSbtn_Enable.BackColor = HColors.Whites.White;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase == null || HCoordinateDraw.CoordinateDrawList.SelectGUID.Count != 1)
            {
                toolSbtn_GoNext.Enabled = toolSbtn_GoLast.Enabled= toolSbtn_Top.Enabled = toolSbtn_Bottom.Enabled = toolSbtn_Last.Enabled = toolSbtn_Next.Enabled = false;
            }
            else
            {
                toolSbtn_GoNext.Enabled = toolSbtn_GoLast.Enabled = toolSbtn_Top.Enabled = toolSbtn_Bottom.Enabled = toolSbtn_Last.Enabled = toolSbtn_Next.Enabled = true;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase == null)
            {
                aPropertyGridSet.SetValue(null);
            }
            switch (HCoordinateDraw.CoordinateDrawList.SelectedShapeType)
            {
                case HShapeType.Select:
                    toolSbtn_Mouse.BackColor = HColors.Whites.White;
                    toolSbtn_Select.BackColor = HColors.Grays.AshGray;
                    toolSbtn_Point.BackColor = HColors.Whites.White;
                    toolSbtn_Line.BackColor = HColors.Whites.White;
                    toolSbtn_3pArc.BackColor = HColors.Whites.White;
                    break;
                case HShapeType.None:
                    toolSbtn_Mouse.BackColor = HColors.Grays.AshGray;
                    toolSbtn_Select.BackColor = HColors.Whites.White;
                    toolSbtn_Point.BackColor = HColors.Whites.White;
                    toolSbtn_Line.BackColor = HColors.Whites.White;
                    toolSbtn_3pArc.BackColor = HColors.Whites.White;
                    break;
                case HShapeType.Line:
                    toolSbtn_Mouse.BackColor = HColors.Whites.White;
                    toolSbtn_Select.BackColor = HColors.Whites.White;
                    toolSbtn_Point.BackColor = HColors.Whites.White;
                    toolSbtn_Line.BackColor = HColors.Grays.AshGray;
                    toolSbtn_3pArc.BackColor = HColors.Whites.White;
                    break;
                case HShapeType.Arc3P:
                    toolSbtn_Mouse.BackColor = HColors.Whites.White;
                    toolSbtn_Select.BackColor = HColors.Whites.White;
                    toolSbtn_Point.BackColor = HColors.Whites.White;
                    toolSbtn_Line.BackColor = HColors.Whites.White;
                    toolSbtn_3pArc.BackColor = HColors.Grays.AshGray;
                    break;
                case HShapeType.Point:
                    toolSbtn_Mouse.BackColor = HColors.Whites.White;
                    toolSbtn_Select.BackColor = HColors.Whites.White;
                    toolSbtn_Point.BackColor = HColors.Grays.AshGray;
                    toolSbtn_Line.BackColor = HColors.Whites.White;
                    toolSbtn_3pArc.BackColor = HColors.Whites.White;
                    break;
                default:
                    toolSbtn_Mouse.BackColor = HColors.Whites.White;
                    toolSbtn_Select.BackColor = HColors.Whites.White;
                    toolSbtn_Point.BackColor = HColors.Whites.White;
                    toolSbtn_Line.BackColor = HColors.Whites.White;
                    toolSbtn_3pArc.BackColor = HColors.Whites.White;
                    break;
            }

        }
        private HDrawBase HDrawBaseValueChanged;
        /// <summary>PropertyGrid_PropertyValueChanged 方法。</summary>
        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (HDrawBaseValueChanged != null)
            {
               HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(HDrawChange.Update(HDrawBaseValueChanged.id, HDrawBaseValueChanged.Clone()));
             
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase != null)
            {
                HCoordinateDraw.CoordinateDrawList.SelectDrawBase = HCoordinateDraw.CoordinateDrawList.Shapes[HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HCoordinateDraw.CoordinateDrawList.SelectDrawBase.GuidCode)];
            }
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }
        /// <summary>Coordinate_ValueChanged 方法。</summary>
        private void Coordinate_ValueChanged(object sender, EventArgs e)
        {
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase!=null)
            {
                HDrawBaseValueChanged = HCoordinateDraw.CoordinateDrawList.SelectDrawBase.Clone();
                HDrawBaseValueChanged.id = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HDrawBaseValueChanged.GuidCode);
                HCoordinateDraw.CoordinateDrawList.SelectDrawBase.id = HDrawBaseValueChanged.id;
            }
            aPropertyGridSet.SetValue(HCoordinateDraw.CoordinateDrawList.SelectDrawBase);
         
        }

        /// <summary>转换为 olSbtn_Mouse_Click。</summary>
        private void toolSbtn_Mouse_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.None;
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
        }

        /// <summary>转换为 olSbtn_Select_Click。</summary>
        private void toolSbtn_Select_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.Select;
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
        }

        /// <summary>转换为 olSbtn_Point_Click。</summary>
        private void toolSbtn_Point_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.Point;
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
        }

        /// <summary>转换为 olSbtn_Line_Click。</summary>
        private void toolSbtn_Line_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.Line;
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
            if (HCoordinateDraw.CoordinateDrawList.IsContinuous)
            {
                HCoordinateDraw.AddPoint(true);
            }
        }

        /// <summary>转换为 olSbtn_3pArc_Click。</summary>
        private void toolSbtn_3pArc_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.SelectedShapeType = HShapeType.Arc3P;
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
            if (HCoordinateDraw.CoordinateDrawList.IsContinuous)
            {
                HCoordinateDraw.AddPoint(true);
            }
        }

        /// <summary>转换为 olSbtn_Continuous_Click。</summary>
        private void toolSbtn_Continuous_Click(object sender, EventArgs e)
        {
            aPropertyGridSet.SetValue(null);
            HCoordinateDraw.SelectShape(-1); HCoordinateDraw.Clear();
            HCoordinateDraw.CoordinateDrawList.IsContinuous = !HCoordinateDraw.CoordinateDrawList.IsContinuous;
            if (HCoordinateDraw.CoordinateDrawList.IsContinuous)
            {
                HCoordinateDraw.AddPoint(true);
            }
            HAppData.Set(this.Name + "IsContinuous", HCoordinateDraw.CoordinateDrawList.IsContinuous);
        }
        /// <summary>转换为 olSbtn_Layer_Click。</summary>
        private void toolSbtn_Layer_Click(object sender, EventArgs e)
        {
            LoadIni(true);
            SetLayer setLayer = new SetLayer();
            if (!setLayer.IsShow && IsLoad)
            {
                setLayer.SetValue(HCoordinateDraw.CoordinateDrawList.IsEnableLayer, HCoordinateDraw.CoordinateDrawList.Layer);
                DialogResult dialogResult = setLayer.ZzShowDialog("", HTranslation.GetContent("设置图层"), 2, HTranslation.GetContent("确认"), HTranslation.GetContent("取消"));

                if (dialogResult == DialogResult.OK)
                {
                    HCoordinateDraw.CoordinateDrawList.IsEnableLayer = setLayer.IsEnableLayer;
                    HCoordinateDraw.CoordinateDrawList.Layer = setLayer.Layer;

                    HAppData.Set(this.Name + "IsEnableLayer", HCoordinateDraw.CoordinateDrawList.IsEnableLayer);
                    HAppData.Set(this.Name + "Layer", HCoordinateDraw.CoordinateDrawList.Layer);

                    HCoordinateDraw.CoordinateScreen.IsRefresh = true;
                }

            }
            setLayer.Dispose();
            setLayer = null;
        }

        /// <summary>转换为 olSbtn_Enable_Click。</summary>
        private void toolSbtn_Enable_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateScreen.EnableDraw = !HCoordinateDraw.CoordinateScreen.EnableDraw;
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Open_Click。</summary>
        private void toolSbtn_Open_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = $@"{HTranslation.GetContent("图形文件")} (*.shapes)|*.shapes|{HTranslation.GetContent("文本文件")} (*.txt)|*.txt|{HTranslation.GetContent("所有文件")} (*.*)|*.*";
                openFileDialog.DefaultExt = "shapes";
                openFileDialog.Title = HTranslation.GetContent("打开图形数据"); ;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    // 调用加载方法（需事先定义 LoadShapesFromFile）
                    OK<HList<HDrawBase>> result = LoadShapesFromFile(filePath);
                    if (result.IsSuccess)
                    {
                        HCoordinateDraw.Clear(); HCoordinateDraw.SelectShape(-1);
                        // 清空原有列表并添加新数据（或直接替换引用）
                        HCoordinateDraw.CoordinateDrawList.Shapes.Clear();
                        foreach (var shape in result.Value)
                        { HCoordinateDraw.CoordinateDrawList.Shapes.Add(shape); }
                        HCoordinateDraw.CoordinateDrawList.IsEnableLayer = true;
                        HCoordinateDraw.CoordinateDrawList.DrawChangeClear();
                        HCoordinateDraw.CoordinateScreen.IsRefresh = true;
                    }
                    else
                    {
                        ZvMessageShow zxMessageShow = new ZvMessageShow();
                        DialogResult dialogResult = zxMessageShow.ZzShowDialog(HTranslation.GetContent("加载失败：", (result.Message ?? HTranslation.GetContent("文件格式错误"))), HTranslation.GetContent("打开文件错误"), 0, HTranslation.GetContent("确认"));
                      
                    }
                }
            }
        }

        /// <summary>转换为 olSbtn_Save_Click。</summary>
        private void toolSbtn_Save_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = $@"{HTranslation.GetContent("图形文件")} (*.shapes)|*.shapes|{HTranslation.GetContent("文本文件")} (*.txt)|*.txt|{HTranslation.GetContent("所有文件")} (*.*)|*.*";
                saveFileDialog.DefaultExt = "shapes";
                saveFileDialog.Title = HTranslation.GetContent("保存图形数据");

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    // 调用保存方法（需事先定义 SaveShapesToFile）
                    OK success = SaveShapesToFile(filePath, HCoordinateDraw.CoordinateDrawList.Shapes);
                    if (!success)
                    {
                        ZvMessageShow zxMessageShow = new ZvMessageShow();
                        DialogResult dialogResult = zxMessageShow.ZzShowDialog(HTranslation.GetContent("保存失败，请检查路径或权限。")+ success, HTranslation.GetContent("保存文件错误"), 0, HTranslation.GetContent("确认"));

                    }
                }
            }
        }

        /// <summary>转换为 olSbtn_Forward_Click。</summary>
        private void toolSbtn_Forward_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            HCoordinateDraw.Clear(); HCoordinateDraw.SelectShape(-1);
            HCoordinateDraw.CoordinateDrawList.SelectDrawBase = null;
            HCoordinateDraw.CoordinateDrawList.DrawChangeForward(true);
            Coordinate_ValueChanged(HCoordinateDraw.CoordinateDrawList.SelectDrawBase,null);

            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_backward_Click。</summary>
        private void toolSbtn_backward_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            HCoordinateDraw.Clear(); HCoordinateDraw.SelectShape(-1);
            HCoordinateDraw.CoordinateDrawList.SelectDrawBase = null;
            HCoordinateDraw.CoordinateDrawList.DrawChangeBackward(true);
            Coordinate_ValueChanged(HCoordinateDraw.CoordinateDrawList.SelectDrawBase, null);

            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Top_Click。</summary>
        private void toolSbtn_Top_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase == null)
            {
                return;
            }
            int buffer = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HCoordinateDraw.CoordinateDrawList.SelectDrawBase.GuidCode);
            HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(HDrawChange.Change(buffer, true));
            HCoordinateDraw.CoordinateDrawList.Shapes.MoveToLast(buffer);
            Coordinate_ValueChanged(null, null);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Bottom_Click。</summary>
        private void toolSbtn_Bottom_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase == null)
            {
                return;
            }
            int buffer = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HCoordinateDraw.CoordinateDrawList.SelectDrawBase.GuidCode);
            HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(HDrawChange.Change(buffer, false));
            HCoordinateDraw.CoordinateDrawList.Shapes.MoveToFirst(buffer);
            Coordinate_ValueChanged(null, null);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Format_Click。</summary>
        private void toolSbtn_Format_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            SetFormat setFormat = new SetFormat();
            LoadIni(true);
            if (!setFormat.IsShow && IsLoad)
            {
                setFormat.SetValue(HCoordinateDraw.CoordinateDrawList.IsEnableFormat, HCoordinateDraw.CoordinateDrawList.FormatX1, HCoordinateDraw.CoordinateDrawList.FormatY1, HCoordinateDraw.CoordinateDrawList.FormatX2, HCoordinateDraw.CoordinateDrawList.FormatY2);
                DialogResult dialogResult = setFormat.ZzShowDialog("", HTranslation.GetContent("设置幅面"), 2, HTranslation.GetContent("确认"), HTranslation.GetContent("取消"));
                if (dialogResult == DialogResult.OK)
                {
                    HCoordinateDraw.CoordinateDrawList.IsEnableFormat = setFormat.IsEnableFormat;
                    HCoordinateDraw.CoordinateDrawList.FormatX1 = setFormat.FormatX1;
                    HCoordinateDraw.CoordinateDrawList.FormatX2 = setFormat.FormatX2;
                    HCoordinateDraw.CoordinateDrawList.FormatY1 = setFormat.FormatY1;
                    HCoordinateDraw.CoordinateDrawList.FormatY2 = setFormat.FormatY2;

                    HAppData.Set(this.Name + "IsEnableFormat", HCoordinateDraw.CoordinateDrawList.IsEnableFormat);
                    HAppData.Set(this.Name + "FormatX1", HCoordinateDraw.CoordinateDrawList.FormatX1);
                    HAppData.Set(this.Name + "FormatX2", HCoordinateDraw.CoordinateDrawList.FormatX2);
                    HAppData.Set(this.Name + "FormatY1", HCoordinateDraw.CoordinateDrawList.FormatY1);
                    HAppData.Set(this.Name + "FormatY2", HCoordinateDraw.CoordinateDrawList.FormatY2);
                    HCoordinateDraw.CoordinateScreen.IsRefresh = true;
                }

            }
            setFormat.Dispose();
            setFormat = null;
        }

        /// <summary>转换为 olSbtn_GoFormat_Click。</summary>
        private void toolSbtn_GoFormat_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.ViewOffsetCenter((HCoordinateDraw.CoordinateDrawList.FormatX1 + HCoordinateDraw.CoordinateDrawList.FormatX2) / 2, (HCoordinateDraw.CoordinateDrawList.FormatY1 + HCoordinateDraw.CoordinateDrawList.FormatY2) / 2);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Appropriate_Click。</summary>
        private void toolSbtn_Appropriate_Click(object sender, EventArgs e)
        {
            if (HCoordinateDraw.CoordinateDrawList.Shapes == null || HCoordinateDraw.CoordinateDrawList.Shapes.Count == 0)
            {
                return;
            }
            List<HPoint3D> hPoint3Ds = new List<HPoint3D>();
            foreach (var item in HCoordinateDraw.CoordinateDrawList.Shapes)
            {
                hPoint3Ds.AddRange(item.GetPoint(0, 0));
            }
            HPoints hPoints = new HPoints(hPoint3Ds);
            HRectangle hRectangle = new HRectangle(new HPoint(hPoints.Bounds.minX, hPoints.Bounds.minY), new HPoint(hPoints.Bounds.maxX, hPoints.Bounds.maxY));
            HDouble viewScale = HCoordinateDraw.CoordinateScreen.ViewScaleMin;
            HDouble hDouble = HCoordinateDraw.CoordinateScreen.ScreenRectangle.Width;
            HDouble view = 0;
            if (HCoordinateDraw.CoordinateScreen.ScreenRectangle.Width > HCoordinateDraw.CoordinateScreen.ScreenRectangle.Height)
            {
                hDouble = HCoordinateDraw.CoordinateScreen.ScreenRectangle.Height;
                view = (hDouble / hRectangle.Height)*0.8;
            }
            else
            {
                view = (hDouble / hRectangle.Width) * 0.8;
            }
            if (view < HCoordinateDraw.CoordinateScreen.ViewScaleMin)
            {
                view = HCoordinateDraw.CoordinateScreen.ViewScaleMin;
            }
            if (view > HCoordinateDraw.CoordinateScreen.ViewScaleMax)
            {
                view = HCoordinateDraw.CoordinateScreen.ViewScaleMax;
            }
            HCoordinateDraw.CoordinateScreen.ViewScale = view;
            HPoint3D hPoint = HDrawList.GetAverage(hPoint3Ds.ToArray());
            HCoordinateDraw.ViewOffsetCenter(hPoint.X.Value, hPoint.Y.Value);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Last_Click。</summary>
        private void toolSbtn_Last_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            HCoordinateDraw.CoordinateDrawList.Last(aPropertyGridSet.SetValue);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Next_Click。</summary>
        private void toolSbtn_Next_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            HCoordinateDraw.CoordinateDrawList.Next(aPropertyGridSet.SetValue);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_Group_Click。</summary>
        private void toolSbtn_Group_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectGUID.Count > 1)
            {
                // 在 Group 按钮执行操作前，先记录原始图形用于撤销
                List<int> gr = new List<int>();
                List<HDrawBase> clones = new List<HDrawBase>();
                foreach (var item in HCoordinateDraw.CoordinateDrawList.SelectGUID)
                {
                    int idx = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(item);
                    gr.Add(idx);
                    clones.Add(HCoordinateDraw.CoordinateDrawList.Shapes[idx].Clone());
                }

                int max = gr.Max();
                int buffer = max - gr.Count + 1;
                if (buffer < 0) buffer = 0;

                // 移除选中的图形
                HList<HDrawBase> lists = HCoordinateDraw.CoordinateDrawList.Shapes.GetListAndRemove(gr.ToArray());

                HDrawBase groupObj = new HDrawGroup(lists);
                groupObj.Layer = lists[0].Layer;

                // 插入组
                if (buffer >= HCoordinateDraw.CoordinateDrawList.Shapes.Count)
                    HCoordinateDraw.CoordinateDrawList.Shapes.Add(groupObj);
                else
                    HCoordinateDraw.CoordinateDrawList.Shapes.Insert(buffer, groupObj);

                // 记录 Group 操作
                HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(
                    HDrawChange.Group(gr.ToArray(), clones.ToArray(), buffer)
                );

                HCoordinateDraw.SelectShape(-1);
                HCoordinateDraw.CoordinateScreen.IsRefresh = true;
            }
        }

        /// <summary>转换为 olSbtn_UnGroup_Click。</summary>
        private void toolSbtn_UnGroup_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase != null && HCoordinateDraw.CoordinateDrawList.SelectDrawBase is HDrawGroup)
            {
                HDrawGroup group = HCoordinateDraw.CoordinateDrawList.SelectDrawBase as HDrawGroup;
                int idx = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(group.GuidCode);
                HDrawBase groupClone = group.Clone();   // 保存组的克隆，用于撤销

                bool isAdd = idx == HCoordinateDraw.CoordinateDrawList.Shapes.Count - 1;

                // 移除组
                HCoordinateDraw.CoordinateDrawList.Shapes.GetListAndRemove(new int[] { idx });

                // 插入组内元素
                HDrawBase[] children = group.Group.ToArray();
                if (isAdd)
                    HCoordinateDraw.CoordinateDrawList.Shapes.Add(children);
                else
                    HCoordinateDraw.CoordinateDrawList.Shapes.Insert(idx, children);

                // 记录 UnGroup 操作
                HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(
                    HDrawChange.UnGroup(idx, groupClone)
                );

                HCoordinateDraw.SelectShape(-1);
                HCoordinateDraw.CoordinateScreen.IsRefresh = true;
            }
        }

        /// <summary>转换为 olSbtn_Number_Click。</summary>
        private void toolSbtn_Number_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.IsCapture = !HCoordinateDraw.CoordinateDrawList.IsCapture;
        }

        /// <summary>转换为 olSbtn_DXF_Click。</summary>
        private void toolSbtn_DXF_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }


        }

        /// <summary>转换为 olSbtn_Del_Click。</summary>
        private void toolSbtn_Del_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            HCoordinateDraw.DeleteShapes();
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_DecimalPlaces_Click。</summary>
        private void toolSbtn_DecimalPlaces_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            LoadIni(true);
            SetDecimalPlaces setDecimalPlaces = new SetDecimalPlaces();
            if (!setDecimalPlaces.IsShow && IsLoad)
            {
                setDecimalPlaces.SetValue(HCoordinateDraw.CoordinateScreen.SetDecimalPlaces.Value);
                DialogResult dialogResult = setDecimalPlaces.ZzShowDialog("", HTranslation.GetContent("设置小数保留位数"), 2, HTranslation.GetContent("确认"), HTranslation.GetContent("取消"));

                if (dialogResult == DialogResult.OK)
                {
                    HCoordinateDraw.CoordinateScreen.SetDecimalPlaces = setDecimalPlaces.DecimalPlaces;

                    HAppData.Set(this.Name + "SetDecimalPlaces", HCoordinateDraw.CoordinateScreen.SetDecimalPlaces);
                    HCoordinateDraw.CoordinateScreen.IsRefresh = true;
                }

            }
            setDecimalPlaces.Dispose();
            setDecimalPlaces = null;
        }


        /// <summary>
        /// 将给定的图元列表保存到文件（使用 Save(2) 格式）。
        /// </summary>
        /// <param name="filePath">文件完整路径（支持带扩展名或不带，由 HTxtFile 自动处理）</param>
        /// <param name="shapes">要保存的图元列表</param>
        /// <returns>是否保存成功</returns>
        public OK SaveShapesToFile(string filePath, HList<HDrawBase> shapes)
        {
            try
            {
                if (shapes == null || shapes.Count == 0)
                    return false;

                var sb = new StringBuilder();
                foreach (var shape in shapes)
                {
                    OK<string> result = shape.Save(2);
                    if (result.IsSuccess && !string.IsNullOrEmpty(result.Value))
                        sb.Append(result.Value);
                }

                // HTxtFile 构造函数自动处理路径和默认 UTF‑8 编码
                HTxtFile txtFile = new HTxtFile(filePath, Encoding.UTF8);
                // WriteAllText 内部会写入换行，直接写入整个字符串即可
                return txtFile.WriteAllText(sb.ToString().TrimEnd());
            }
            catch (Exception ex)
            {
                // 记录日志（可选），异常时返回失败 OK（FromMessage 非空消息即失败）
                return OK.FromMessage(ex.Message);
            }
        }

        /// <summary>
        /// 从文件中加载所有图元，返回新的 HList<HDrawBase>。
        /// 支持 HDrawLine、HDraw3PArc、HDrawPoint、HDrawGroup（含多层嵌套）。
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns>包含加载结果和列表的操作结果对象</returns>
        public OK<HList<HDrawBase>> LoadShapesFromFile(string filePath)
        {
            var result = new OK<HList<HDrawBase>>();
            try
            {
                HTxtFile txtFile = new HTxtFile(filePath, Encoding.UTF8);
                string[] lines = txtFile.ReadAllLines();   // 文件不存在时返回空数组
                if (lines == null || lines.Length == 0)
                {
                    result.IsSuccess = false;
                    result.Message = HTranslation.GetContent( "文件为空或不存在");
                    return result;
                }

                var shapes = new HList<HDrawBase>();
                int index = 0;
                while (index < lines.Length)
                {
                    string line = lines[index];
                    // 跳过空行和非开始标记行
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("[") || !line.Contains("_Start"))
                    {
                        index++;
                        continue;
                    }

                    // 提取类型名，例如 "HDrawLine"
                    int underscoreIdx = line.IndexOf('_');
                    if (underscoreIdx < 0) { index++; continue; }
                    string typeName = line.Substring(1, underscoreIdx - 1);

                    // 深度计数器定位对应结束行（支持嵌套）
                    int depth = 1;
                    int endIdx = index + 1;
                    while (endIdx < lines.Length && depth > 0)
                    {
                        if (lines[endIdx].StartsWith("[") && lines[endIdx].Contains("_Start"))
                            depth++;
                        else if (lines[endIdx].StartsWith("[") && lines[endIdx].Contains("_End"))
                            depth--;
                        endIdx++;
                    }
                    endIdx--;   // 实际结束行索引

                    // 收集完整的子数据块（含首尾标记）
                    var subData = new List<string>();
                    for (int i = index; i <= endIdx; i++)
                        subData.Add(lines[i]);

                    // 根据类型创建对应实例
                    HDrawBase subItem = null;
                    switch (typeName)
                    {
                        case "HDrawLine": subItem = new HDrawLine(); break;
                        case "HDraw3PArc": subItem = new HDraw3PArc(); break;
                        case "HDrawPoint": subItem = new HDrawPoint(); break;
                        case "HDrawGroup": subItem = new HDrawGroup(); break;
                        default:
                            // 未知类型，跳过该块
                            index = endIdx + 1;
                            continue;
                    }

                    // 调用子对象的 Load 进行反序列化
                    OK<HDrawBase> subOK = subItem.Load(subData.ToArray(), 2);
                    if (subOK.IsSuccess)
                        shapes.Add(subOK.Value);

                    index = endIdx + 1;   // 移动到下一个图元块
                }

                result.IsSuccess = true;
                result.Value = shapes;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = HTranslation.GetContent($"加载异常：")+ex.Message;
                return result;
            }




        }

        /// <summary>转换为 olSbtn_Set_Click。</summary>
        private void toolSbtn_Set_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }


        }

        /// <summary>转换为 olSbtn_GoLast_Click。</summary>
        private void toolSbtn_GoLast_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase==null)
            {
                return;
            }
            int buffer = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HCoordinateDraw.CoordinateDrawList.SelectDrawBase.GuidCode);
            HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(HDrawChange.Change(buffer, buffer-1));
            HCoordinateDraw.CoordinateDrawList.Shapes.MoveForward(buffer);
            Coordinate_ValueChanged(null,null);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_GoNext_Click。</summary>
        private void toolSbtn_GoNext_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
            if (HCoordinateDraw.CoordinateDrawList.SelectDrawBase == null)
            {
                return;
            }
            int buffer = HCoordinateDraw.CoordinateDrawList.GetShapesIndex(HCoordinateDraw.CoordinateDrawList.SelectDrawBase.GuidCode);
            HCoordinateDraw.CoordinateDrawList.DrawChangeAdd(HDrawChange.Change(buffer, buffer + 1));
            HCoordinateDraw.CoordinateDrawList.Shapes.MoveBackward(buffer);
            Coordinate_ValueChanged(null, null);
            HCoordinateDraw.CoordinateScreen.IsRefresh = true;
        }

        /// <summary>转换为 olSbtn_GCode_Click。</summary>
        private void toolSbtn_GCode_Click(object sender, EventArgs e)
        {

        }
        private Timer timerrun;
        /// <summary>timerint 字段。</summary>
        private int timerint = 0;
        /// <summary>转换为 olSbtn_Start_Click。</summary>
        private void toolSbtn_Start_Click(object sender, EventArgs e)
        {
            HList<HDrawBase> shapesrun  = new HList<HDrawBase>();
            HCoordinateDraw.CoordinateDrawList.IsRunShapes = false;
            timerint = 0;
            if (HCoordinateDraw.CoordinateDrawList.IsEnableLayer)
            {
                shapesrun = HCoordinateDraw.CoordinateDrawList.Shapes;
            }
            else
            {
                foreach (var item in HCoordinateDraw.CoordinateDrawList.Shapes)
                {
                    if (item.Layer== HCoordinateDraw.CoordinateDrawList.Layer)
                    {
                        shapesrun.Add(item.Clone());
                    }
                }
            }
            HCoordinateDraw.CoordinateDrawList.RunShapes = HCoordinateDraw.CoordinateDrawList.InsertConnectingDashedLines(shapesrun);
            HCoordinateDraw.CoordinateDrawList.RunPoint2Ds = HCoordinateDraw.CoordinateDrawList.SamplePathWithPoints2D(HCoordinateDraw.CoordinateDrawList.RunShapes,50000);
            if (HCoordinateDraw.CoordinateDrawList.RunPoint2Ds.Count==0)
            {
                return;
            }
            DialogResult dialogResult= ZxMessageShow.ShowDialog(HTranslation.GetContent($"是否开启模拟运行？"),
                HTranslation.GetContent($"模拟运行"),2,
                HTranslation.GetContent($"确认"),
                HTranslation.GetContent($"取消")
                );
            if (dialogResult== DialogResult.OK)
            {
                HCoordinateDraw.Clear();
                HCoordinateDraw.SelectShape(-1);
                if (timerrun==null)
                {
                    timerrun = new Timer();
                }
                timerrun.Interval = 40;
                timerrun.Tick += Timerrun_Tick;
                HCoordinateDraw.CoordinateDrawList.IsRunShapes = true;
                timerint = 0;
                timerrun.Start();
            }
        }

        /// <summary>Timerrun_Tick 方法。</summary>
        private void Timerrun_Tick(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (timerint < HCoordinateDraw.CoordinateDrawList.RunPoint2Ds.Count)
            {
                HCoordinateDraw.CoordinateDrawList.HDrawRunPoint.Set(HCoordinateDraw.CoordinateDrawList.RunPoint2Ds[timerint]);
            }
            if (timerint > HCoordinateDraw.CoordinateDrawList.RunPoint2Ds.Count - 1)
            {
                timerint++;
            }
            if (HCoordinateDraw.CoordinateDrawList.IsRunShapes)
            {
                HCoordinateDraw.CoordinateScreen.IsRefresh = true;
            }
            if (timerint> HCoordinateDraw.CoordinateDrawList.RunPoint2Ds.Count+2)
            {
                timerrun.Stop();
            }
            timerint++;
        }

        /// <summary>转换为 olSbtn_Stop_Click。</summary>
        private void toolSbtn_Stop_Click(object sender, EventArgs e)
        {
            HCoordinateDraw.CoordinateDrawList.IsRunShapes = false;
            HCoordinateDraw.CoordinateDrawList.HDrawRunPoint.Set(null);
            if (timerrun==null)
            {
                return;
            }
            timerrun.Stop();
            timerrun.Dispose();
            timerrun = null;
        }

        /// <summary>转换为 olSbtn_Prestore_Click。</summary>
        private void toolSbtn_Prestore_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
        }

        /// <summary>转换为 olSbtn_ShowPrestore_Click。</summary>
        private void toolSbtn_ShowPrestore_Click(object sender, EventArgs e)
        {
            if (!HCoordinateDraw.CoordinateScreen.EnableDraw)
            {
                return;
            }
        }
    }
}
