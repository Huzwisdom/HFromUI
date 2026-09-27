using HFromUI.HAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HFromUI.HFrom.Base;
using HFromUI.HFrom.Text;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HLangage;
    /// <summary>
    /// 搜索+分页+过滤一体化数据网格
    /// 参考 DevExpress GridControl + Telerik RadGridView + Syncfusion SfDataGrid
    /// </summary>
    [DefaultEvent("PageChanged")]
    [ToolboxItem(true)]
    public partial class HPagedDataGridView : HControlBase
    {
        #region 控件

        private HSearchBox searchBox;
        private HDataGridView grid;
        private HPagination pagination;
        #endregion

        #region 字段

        private IList _allData;                  // 内存模式下的全部数据
        /// <summary>_sortColumn 字段。</summary>
        private string _sortColumn = string.Empty;
        /// <summary>_sortDescending 字段。</summary>
        private bool _sortDescending = false;
        private readonly Dictionary<string, HashSet<string>> _filters = new Dictionary<string, HashSet<string>>();
        /// <summary>_enableColumnFilter 字段。</summary>
        private bool _enableColumnFilter = false;

        #endregion

        #region 委托/属性

        /// <summary>数据提供委托（设置后每次分页/搜索/排序时调用）</summary>
        public Func<HPagedQuery, HPagedResult> ProvideData { get; set; }

        /// <summary>Grid 成员。</summary>
        /// <summary>Grid 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("内部数据网格"), HDescriptionLanguage("内部数据网格"), Browsable(true)]
        public HDataGridView Grid => grid;

        /// <summary>SearchBox 成员。</summary>
        /// <summary>SearchBox 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("内部搜索框"), HDescriptionLanguage("内部搜索框"), Browsable(true)]
        public HSearchBox SearchBox => searchBox;

        /// <summary>Pagination 成员。</summary>
        /// <summary>Pagination 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("内部分页条"), HDescriptionLanguage("内部分页条"), Browsable(true)]
        public HPagination Pagination => pagination;

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用列过滤"), HDescriptionLanguage("是否启用列过滤"), Browsable(true)]
        public bool EnableColumnFilter
        {
            get { return _enableColumnFilter; }
            set { _enableColumnFilter = value; }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("内存数据源"), HDescriptionLanguage("内存数据源（当未设置 ProvideData 时使用）"), Browsable(true)]
        public IList DataSource
        {
            get { return _allData; }
            set
            {
                _allData = value;
                if (ProvideData == null)
                {
                    pagination.TotalRecords = value?.Count ?? 0;
                    LoadMemoryPage();
                }
            }
        }

        #endregion

        public HPagedDataGridView()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            // 初始化内部控件
            searchBox = new HSearchBox
            {
                Dock = DockStyle.Top,
                Height = 36,
                WatermarkText = HTranslation.GetContent("搜索...")
            };
            searchBox.Search += OnSearch;

            grid = new HDataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.ColumnHeaderMouseClick += OnColumnHeaderClick;

            pagination = new HPagination
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                PageSize = 20
            };
            pagination.PageChanged += OnPageChanged;

            // 控件添加顺序：Fill 必须最后
            this.Controls.Add(pagination);
            this.Controls.Add(grid);
            this.Controls.Add(searchBox);

            this.SizeChanged += (s, e) => { /* Dock 自动处理 */ };
        }

        #region 搜索

        /// <summary>响应 Search 事件。</summary>
        private void OnSearch(object sender, HSearchEventArgs e)
        {
            pagination.CurrentPage = 1;
            if (ProvideData != null)
                QueryRemoteData();
            else
                LoadMemoryPage();
        }

        #endregion

        #region 分页

        /// <summary>响应 PageChanged 事件。</summary>
        private void OnPageChanged(object sender, HPagingEventArgs e)
        {
            if (ProvideData != null)
                QueryRemoteData();
            else
                LoadMemoryPage();
        }

        #endregion

        #region 排序

        /// <summary>响应 ColumnHeaderClick 事件。</summary>
        private void OnColumnHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count) return;

            string colName = grid.Columns[e.ColumnIndex].Name;
            if (string.IsNullOrEmpty(colName)) colName = grid.Columns[e.ColumnIndex].DataPropertyName;

            if (_sortColumn == colName)
                _sortDescending = !_sortDescending;
            else
            {
                _sortColumn = colName;
                _sortDescending = false;
            }

            // 清除之前的排序标记
            foreach (DataGridViewColumn col in grid.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            grid.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection =
                _sortDescending ? SortOrder.Descending : SortOrder.Ascending;

            if (ProvideData != null)
                QueryRemoteData();
            else
                LoadMemoryPage();
        }

        #endregion

        #region 列过滤

        /// <summary>打开列过滤弹窗</summary>
        public void ShowColumnFilter(string columnName, Control anchorControl)
        {
            if (!_enableColumnFilter || string.IsNullOrEmpty(columnName)) return;

            var filterForm = new HDataFilterForm(anchorControl) { Size = new Size(200, 300) };
            var distinctValues = GetDistinctValues(columnName);
            HashSet<string> initiallyChecked = null;
            if (_filters.ContainsKey(columnName))
                initiallyChecked = _filters[columnName];
            filterForm.SetValues(distinctValues, initiallyChecked);
            filterForm.FilterApplied += (s, e) =>
            {
                if (e.CheckedValues == null)
                    _filters.Remove(columnName);
                else
                    _filters[columnName] = e.CheckedValues;

                pagination.CurrentPage = 1;
                if (ProvideData != null)
                    QueryRemoteData();
                else
                    LoadMemoryPage();
            };
            filterForm.Show();
        }

        /// <summary>获取 distinctValues。</summary>
        private IEnumerable<string> GetDistinctValues(string columnName)
        {
            var set = new HashSet<string>();
            if (_allData == null) return set;

            foreach (var item in _allData)
            {
                if (item == null) continue;
                var prop = item.GetType().GetProperty(columnName);
                if (prop != null)
                {
                    var val = prop.GetValue(item, null);
                    set.Add(val == null ? string.Empty : val.ToString());
                }
                else
                {
                    // 尝试作为 DataRowView 或字典
                    var dv = item as System.Data.DataRowView;
                    if (dv != null && dv.DataView.Table.Columns.Contains(columnName))
                        set.Add(dv[columnName]?.ToString() ?? string.Empty);
                }
            }
            return set.OrderBy(s => s);
        }

        #endregion

        #region 数据加载

        /// <summary>使用 ProvideData 委托查询数据</summary>
        private void QueryRemoteData()
        {
            if (ProvideData == null) return;

            var query = new HPagedQuery
            {
                Page = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                SearchText = searchBox.SearchText,
                SortColumn = _sortColumn,
                SortDescending = _sortDescending,
                Filters = new Dictionary<string, HashSet<string>>(_filters)
            };

            HPagedResult result = ProvideData(query);
            if (result == null) return;

            pagination.TotalRecords = result.TotalRecords;
            pagination.RefreshUI();

            grid.DataSource = result.Rows as IList ?? (result.Rows != null ? ToList(result.Rows) : null);
        }

        /// <summary>内存模式分页</summary>
        private void LoadMemoryPage()
        {
            if (_allData == null) return;

            // 应用搜索过滤
            var filtered = FilterMemoryData();
            // 应用排序
            var sorted = SortMemoryData(filtered);
            // 更新总数
            pagination.TotalRecords = sorted.Count;
            pagination.RefreshUI();

            // 分页
            int offset = pagination.Offset;
            int limit = pagination.Limit;
            var pageData = new List<object>();
            for (int i = offset; i < offset + limit && i < sorted.Count; i++)
            {
                pageData.Add(sorted[i]);
            }

            grid.DataSource = pageData;
        }

        /// <summary>FilterMemoryData 方法。</summary>
        private List<object> FilterMemoryData()
        {
            var result = new List<object>();
            string searchText = searchBox.SearchText?.ToLowerInvariant() ?? string.Empty;

            foreach (var item in _allData)
            {
                if (item == null) continue;
                bool match = true;

                // 文本搜索
                if (!string.IsNullOrEmpty(searchText))
                {
                    match = false;
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        var val = prop.GetValue(item, null);
                        if (val != null && val.ToString().ToLowerInvariant().Contains(searchText))
                        {
                            match = true;
                            break;
                        }
                    }
                }

                // 列过滤
                if (match)
                {
                    foreach (var kvp in _filters)
                    {
                        var prop = item.GetType().GetProperty(kvp.Key);
                        if (prop != null)
                        {
                            var val = prop.GetValue(item, null);
                            string valStr = val == null ? string.Empty : val.ToString();
                            if (!kvp.Value.Contains(valStr))
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                }

                if (match)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>SortMemoryData 方法。</summary>
        private List<object> SortMemoryData(List<object> data)
        {
            if (string.IsNullOrEmpty(_sortColumn)) return data;

            // 使用反射获取属性值进行排序
            var sorted = data.ToList();
            sorted.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return _sortDescending ? 1 : -1;
                if (b == null) return _sortDescending ? -1 : 1;

                var prop = a.GetType().GetProperty(_sortColumn);
                if (prop == null) return 0;

                var va = prop.GetValue(a, null);
                var vb = prop.GetValue(b, null);

                if (va == null && vb == null) return 0;
                if (va == null) return _sortDescending ? 1 : -1;
                if (vb == null) return _sortDescending ? -1 : 1;

                int cmp;
                if (va is IComparable ica && vb is IComparable icb)
                    cmp = ica.CompareTo(icb);
                else
                    cmp = string.Compare(va.ToString(), vb.ToString(), StringComparison.Ordinal);

                return _sortDescending ? -cmp : cmp;
            });
            return sorted;
        }

        /// <summary>转换为 List。</summary>
        private static IList ToList(IEnumerable source)
        {
            var list = new List<object>();
            foreach (var item in source)
                list.Add(item);
            return list;
        }

        #endregion

        #region 公开方法

        /// <summary>刷新数据（重新查询当前页）</summary>
        public void RefreshData()
        {
            if (ProvideData != null)
                QueryRemoteData();
            else
                LoadMemoryPage();
        }

        /// <summary>重置搜索和过滤</summary>
        public void ResetFilters()
        {
            searchBox.SearchText = string.Empty;
            _filters.Clear();
            _sortColumn = string.Empty;
            _sortDescending = false;
            foreach (DataGridViewColumn col in grid.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            pagination.CurrentPage = 1;
            RefreshData();
        }

        #endregion
    }

    #region 查询/结果类

    /// <summary>分页查询参数</summary>
    public class HPagedQuery
    {
        /// <summary>Page 成员。</summary>
        public int Page { get; set; }
        /// <summary>PageSize 成员。</summary>
        public int PageSize { get; set; }
        /// <summary>SearchText 成员。</summary>
        public string SearchText { get; set; }
        /// <summary>SortColumn 成员。</summary>
        public string SortColumn { get; set; }
        /// <summary>SortDescending 成员。</summary>
        public bool SortDescending { get; set; }
        public Dictionary<string, HashSet<string>> Filters { get; set; }
    }

    /// <summary>分页查询结果</summary>
    public class HPagedResult
    {
        /// <summary>Rows 成员。</summary>
        public IEnumerable Rows { get; set; }
        /// <summary>TotalRecords 成员。</summary>
        public int TotalRecords { get; set; }
    }

    #endregion
}