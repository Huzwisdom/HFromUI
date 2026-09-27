using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    /// <summary>
    /// HTreeList 列描述组件
    /// 定义列的标题、字段绑定、宽度、对齐、排序、可见性、编辑与格式化等属性
    /// </summary>
    [ToolboxItem(true)]
    [DefaultProperty("HeaderText")]
    public class HTreeListColumn : Component
    {
        #region 字段

        /// <summary>名称。</summary>
        private string name = string.Empty;
        /// <summary>headerText 字段。</summary>
        private string headerText = string.Empty;
        /// <summary>fieldName 字段。</summary>
        private string fieldName = string.Empty;
        /// <summary>宽度。</summary>
        private int width = 100;
        /// <summary>textAlign 字段。</summary>
        private HorizontalAlignment textAlign = HorizontalAlignment.Left;
        /// <summary>sortable 字段。</summary>
        private bool sortable = true;
        /// <summary>是否可见。</summary>
        private bool visible = true;
        /// <summary>editable 字段。</summary>
        private bool editable = false;
        /// <summary>format 字段。</summary>
        private string format = string.Empty;

        #endregion

        #region 事件

        /// <summary>列属性发生变更时触发（标题、字段、宽度、对齐等任意属性变化）</summary>
        public event EventHandler Changed;

        /// <summary>列宽度发生变更时触发</summary>
        public event EventHandler WidthChanged;

        #endregion

        #region 构造函数

        /// <summary>初始化 HTreeListColumn 的新实例</summary>
        public HTreeListColumn()
        {
        }

        /// <summary>使用列标题初始化 HTreeListColumn 的新实例</summary>
        /// <param name="headerText">列标题文本</param>
        public HTreeListColumn(string headerText)
        {
            this.headerText = headerText;
            if (string.IsNullOrEmpty(this.name))
            {
                this.name = headerText;
            }
        }

        /// <summary>使用列名、标题、字段与宽度初始化 HTreeListColumn 的新实例</summary>
        public HTreeListColumn(string name, string headerText, string fieldName, int width)
        {
            this.name = name;
            this.headerText = headerText;
            this.fieldName = fieldName;
            this.width = width < 0 ? 0 : width;
        }

        #endregion

        #region 属性

        /// <summary>列名称（唯一标识，与节点文本字典键配合使用）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列名称"), HDescriptionLanguage("列名称"), Browsable(true)]
        [DefaultValue("")]
        public string Name
        {
            get { return name; }
            set
            {
                if (name != (value ?? string.Empty))
                {
                    name = value ?? string.Empty;
                    OnChanged();
                }
            }
        }

        /// <summary>列标题文本</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列标题文本"), HDescriptionLanguage("列标题文本"), Browsable(true)]
        [DefaultValue("")]
        public string HeaderText
        {
            get { return headerText; }
            set
            {
                if (headerText != (value ?? string.Empty))
                {
                    headerText = value ?? string.Empty;
                    OnChanged();
                }
            }
        }

        /// <summary>字段名称（对应节点 Text 字典的键）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("字段名称"), HDescriptionLanguage("字段名称"), Browsable(true)]
        [DefaultValue("")]
        public string FieldName
        {
            get { return fieldName; }
            set
            {
                if (fieldName != (value ?? string.Empty))
                {
                    fieldName = value ?? string.Empty;
                    OnChanged();
                }
            }
        }

        /// <summary>列宽（像素）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列宽"), HDescriptionLanguage("列宽"), Browsable(true)]
        [DefaultValue(100)]
        public int Width
        {
            get { return width; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (width != value)
                {
                    width = value;
                    OnWidthChanged();
                    OnChanged();
                }
            }
        }

        /// <summary>单元格文本对齐方式</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本对齐方式"), HDescriptionLanguage("文本对齐方式"), Browsable(true)]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get { return textAlign; }
            set
            {
                if (textAlign != value)
                {
                    textAlign = value;
                    OnChanged();
                }
            }
        }

        /// <summary>是否允许点击列头进行排序</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否允许排序"), HDescriptionLanguage("是否允许排序"), Browsable(true)]
        [DefaultValue(true)]
        public bool Sortable
        {
            get { return sortable; }
            set
            {
                if (sortable != value)
                {
                    sortable = value;
                    OnChanged();
                }
            }
        }

        /// <summary>是否显示该列</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示"), HDescriptionLanguage("是否显示"), Browsable(true)]
        [DefaultValue(true)]
        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    OnChanged();
                }
            }
        }

        /// <summary>是否允许编辑该列单元格</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否可编辑"), HDescriptionLanguage("是否可编辑"), Browsable(true)]
        [DefaultValue(false)]
        public bool Editable
        {
            get { return editable; }
            set
            {
                if (editable != value)
                {
                    editable = value;
                    OnChanged();
                }
            }
        }

        /// <summary>格式化字符串（数值/日期，例如 N2、0.00、yyyy-MM-dd）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("格式化字符串"), HDescriptionLanguage("格式化字符串"), Browsable(true)]
        [DefaultValue("")]
        public string Format
        {
            get { return format; }
            set
            {
                if (format != (value ?? string.Empty))
                {
                    format = value ?? string.Empty;
                    OnChanged();
                }
            }
        }

        #endregion

        #region 受保护方法

        /// <summary>触发属性变更事件（所有属性 setter 均调用此方法）</summary>
        protected virtual void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>触发列宽变更事件</summary>
        protected virtual void OnWidthChanged()
        {
            EventHandler handler = WidthChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        /// <summary>返回列的显示文本</summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(headerText))
            {
                return headerText;
            }
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            return "HTreeListColumn";
        }
    }
}