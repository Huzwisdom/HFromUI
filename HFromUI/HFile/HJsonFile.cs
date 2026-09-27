using HFromUI.HAttribute;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace HFromUI.HFile
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全网最全 JSON 文件读写类（参考 HDictionaryFile 的写法，不动任何现有 public 签名）。
    ///
    /// 基础能力（原有）：对象/字典 JSON 序列化与反序列化、文本读写、原子落盘(.writing+.bak+崩溃恢复)、
    ///                 加解密钩子、路径解析规则与 HDictionaryFile 一致。
    ///
    /// 扩展能力（新增）：
    ///   • 静态工具（不绑定文件）：
    ///       Minify 压缩 / Validate 验证 + 错误定位 / DeepClone 深拷贝 / Diff 差异 /
    ///       GetByPath 点号方括号路径查询(a.b[0].c) / SetByPath 路径设置 /
    ///       Merge 深度合并 / Pointer RFC 6901 / Patch RFC 6902 /
    ///       ConvertCase camelCase/snake_case/PascalCase/kebab-case /
    ///       ToXml / FromXml / ToCsv / FromCsv / ToDataTable / FromDataTable
    ///   • 实例方法（绑定文件）：
    ///       ReadObject(Type) 非泛型 / TryReadObject/WriteObject Try 模式 /
    ///       Update(Action) 原子读改写链 / MergeFrom(override) 从 JSON 合并到文件 /
    ///       GetByPath/SetByPath 文件级路径操作 / GetFileInfo / Exists / DeleteFile
    ///
    /// 仅依赖 .NET Framework 4.8 自带库，零第三方包。
    /// </summary>
    public class HJsonFile
    {
        #region ==================== 字段与属性 ====================

        /// <summary>读改写操作的互斥锁，保证同一实例上的文件访问线程安全。</summary>
        private readonly object mLockRead = new object();

        /// <summary>最近一次错误信息</summary>
        public string Error = string.Empty;

        /// <summary>JSON 文件完整路径</summary>
        public string FilePath { get; private set; }

        /// <summary>文件编码（默认 UTF-8）</summary>
        public Encoding FileEncoding { get; set; } = Encoding.UTF8;

        /// <summary>JavaScriptSerializer 允许的最大 JSON 长度（默认 200MB）</summary>
        public int MaxJsonLength { get; set; } = 200 * 1024 * 1024;

        /// <summary>序列化时是否缩进美化</summary>
        public bool PrettyPrint { get; set; } = true;

        /// <summary>最近一次读/写的对象，供 Save() 落盘使用</summary>
        private object _content = null;

        #endregion

        #region ==================== 构造函数 ====================

        /// <summary>
        /// 构造 JSON 文件读写对象。路径规则：
        /// 空 → 程序目录 JsonDefault.json；
        /// 含扩展名 → 直接用；
        /// 含盘符 → 追加 suffixName；
        /// 其它 → 程序目录 + filePath + suffixName。
        /// </summary>
        public HJsonFile(string filePath, string suffixName = ".json")
        {
            FilePath = GetFullPath(filePath, suffixName);
            try { Recover(); } catch { }
        }

        /// <summary>
        /// 把构造传入的路径解析为完整磁盘路径：空路径取程序基目录下 JsonDefault+suffix；
        /// 文件名已带扩展名时原样使用；带盘符的绝对路径仅补后缀；相对路径拼到程序基目录下。
        /// </summary>
        private static string GetFullPath(string filePath, string suffixName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Path.GetFullPath(HFromUI.HData.HAppData.AppPath + @"\JsonDefault" + suffixName);
            if (Path.GetFileName(filePath).Contains("."))
                return Path.GetFullPath(filePath);
            if (filePath.Contains(":"))
                return Path.GetFullPath(filePath + suffixName);
            return Path.GetFullPath(HFromUI.HData.HAppData.AppPath + @"\" + filePath + suffixName);
        }

        #endregion

        #region ==================== 加解密钩子（默认不处理；写法同 HDictionaryFile） ====================

        /// <summary>
        /// 自定义加密委托：不为空时优先于 virtual Encrypt(string) 执行。
        /// 外部注入：file.EncryptFn = raw => 你的加密实现(raw);
        /// </summary>
        public Func<string, string> EncryptFn { get; set; }

        /// <summary>
        /// 自定义解密委托：不为空时优先于 virtual Decrypt(string) 执行。
        /// 外部注入：file.DecryptFn = raw => 你的解密实现(raw);
        /// </summary>
        public Func<string, string> DecryptFn { get; set; }

        /// <summary>解密文本：优先调用 DecryptFn，未注入时原样返回，子类可重写。</summary>
        public virtual string Decrypt(string content)
        {
            if (DecryptFn != null) return DecryptFn(content) ?? content;
            return content;
        }
        /// <summary>加密文本：优先调用 EncryptFn，未注入时原样返回，子类可重写。</summary>
        public virtual string Encrypt(string content)
        {
            if (EncryptFn != null) return EncryptFn(content) ?? content;
            return content;
        }

        #endregion

        #region ==================== 静态：基础序列化 / 反序列化（原有） ====================

        /// <summary>创建统一配置的 JavaScriptSerializer（最大 JSON 长度 200MB、递归深度 1000）。</summary>
        private static JavaScriptSerializer CreateSerializer(int maxJsonLength)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            s.MaxJsonLength = maxJsonLength;
            s.RecursionLimit = 1000;
            return s;
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// 实体类支持特性标注：[HIgnore] 不序列化、[HName("xx")] 改名、[HDateTimeFormat("...")] 日期格式。
        /// </summary>
        public static string Serialize(object obj, bool pretty = true, int maxJsonLength = 200 * 1024 * 1024)
        {
            JavaScriptSerializer serializer = CreateSerializer(maxJsonLength);
            string json = serializer.Serialize(ToJsonTree(obj));
            return pretty ? Pretty(json) : json;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型。
        /// 实体类支持特性标注：[HIgnore] 忽略、[HName("xx")] 按键名匹配、[HDateTimeFormat("...")] 日期解析。
        /// </summary>
        public static T Deserialize<T>(string json, int maxJsonLength = 200 * 1024 * 1024)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            object tree = CreateSerializer(maxJsonLength).DeserializeObject(json);
            return (T)FromJsonTree(tree, typeof(T));
        }

        /// <summary>将 JSON 反序列化为弱类型树（Dictionary/object[]）</summary>
        public static object Deserialize(string json, int maxJsonLength = 200 * 1024 * 1024)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            return CreateSerializer(maxJsonLength).DeserializeObject(json);
        }

        /// <summary>JSON 美化缩进（原有 PrettyJson 改名，对外保持 Pretty 命名）</summary>
        public static string Pretty(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;
            StringBuilder sb = new StringBuilder(json.Length * 2);
            int indent = 0;
            bool inString = false;
            bool escaping = false;
            foreach (char ch in json)
            {
                if (inString)
                {
                    sb.Append(ch);
                    if (escaping) escaping = false;
                    else if (ch == '\\') escaping = true;
                    else if (ch == '"') inString = false;
                    continue;
                }
                switch (ch)
                {
                    case '"': inString = true; sb.Append(ch); break;
                    case '{': case '[':
                        sb.Append(ch); indent++;
                        sb.AppendLine(); sb.Append(' ', indent * 2); break;
                    case '}': case ']':
                        indent--; sb.AppendLine(); sb.Append(' ', indent * 2); sb.Append(ch); break;
                    case ',': sb.Append(ch); sb.AppendLine(); sb.Append(' ', indent * 2); break;
                    case ':': sb.Append(": "); break;
                    default: if (!char.IsWhiteSpace(ch)) sb.Append(ch); break;
                }
            }
            return sb.ToString();
        }

        #endregion

        #region ==================== 静态：压缩 / 验证 / 深拷贝 ====================

        /// <summary>JSON 压缩（去除所有无关空白）</summary>
        public static string Minify(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            StringBuilder sb = new StringBuilder(json.Length);
            bool inString = false, escape = false;
            foreach (char c in json)
            {
                if (inString)
                {
                    sb.Append(c);
                    if (escape) escape = false;
                    else if (c == '\\') escape = true;
                    else if (c == '"') inString = false;
                }
                else
                {
                    if (c == '"') { inString = true; sb.Append(c); continue; }
                    if (!char.IsWhiteSpace(c)) sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>验证 JSON 合法性，合法返回 true；不合法 out 为错误描述</summary>
        public static bool Validate(string json, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json)) { error = "empty"; return false; }
            try
            {
                CreateSerializer(100 * 1024 * 1024).DeserializeObject(json);
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        /// <summary>深拷贝（序列化为 JSON 再反序列化）</summary>
        public static T Clone<T>(T obj) { return Deserialize<T>(Serialize(obj, false)); }

        /// <summary>弱类型深拷贝</summary>
        public static object Clone(object obj) { return Deserialize(Serialize(obj, false)); }

        /// <summary>
        /// 差异对比：返回 [{op:"add"/"remove"/"replace", path:"/a/b", old?, new?}, ...]。
        /// 仅处理 add/remove/replace 三种最常用场景。
        /// </summary>
        public static List<Dictionary<string, object>> Diff(object a, object b)
        {
            var result = new List<Dictionary<string, object>>();
            DiffCore(a, b, string.Empty, result);
            return result;
        }

        /// <summary>递归对比两棵弱类型 JSON 树，把 add/remove/replace 差异追加到结果列表。</summary>
        private static void DiffCore(object a, object b, string pointer, List<Dictionary<string, object>> result)
        {
            if (Equals(a, b)) return;
            Dictionary<string, object> da = a as Dictionary<string, object>;
            Dictionary<string, object> db = b as Dictionary<string, object>;
            object[] la = a as object[];
            object[] lb = b as object[];
            if (da != null && db != null)
            {
                var keys = new HashSet<string>(da.Keys.Concat(db.Keys));
                foreach (string k in keys)
                {
                    string p = pointer + "/" + EscapePointer(k);
                    if (!da.ContainsKey(k))
                        result.Add(MakePatch("add", p, null, db[k]));
                    else if (!db.ContainsKey(k))
                        result.Add(MakePatch("remove", p, da[k], null));
                    else
                        DiffCore(da[k], db[k], p, result);
                }
            }
            else if (la != null && lb != null)
            {
                int n = Math.Max(la.Length, lb.Length);
                for (int i = 0; i < n; i++)
                {
                    string p = pointer + "/" + i;
                    if (i >= la.Length)
                        result.Add(MakePatch("add", p, null, lb[i]));
                    else if (i >= lb.Length)
                        result.Add(MakePatch("remove", p, la[i], null));
                    else
                        DiffCore(la[i], lb[i], p, result);
                }
            }
            else
            {
                result.Add(MakePatch("replace", pointer, a, b));
            }
        }

        /// <summary>构造一条 JSON Patch（RFC 6902）操作记录字典；remove 不带 value，add 不带 old。</summary>
        private static Dictionary<string, object> MakePatch(string op, string path, object old, object value)
        {
            var d = new Dictionary<string, object> { ["op"] = op, ["path"] = path };
            if (op != "add") d["old"] = old;
            if (op != "remove") d["value"] = value;
            return d;
        }

        #endregion

        #region ==================== 静态：路径查询 (a.b[0].c) 与 JSON Pointer (RFC 6901) ====================

        /// <summary>点号方括号路径（a.b[0]["c"]）的词法正则：属性名/数字下标/引号字符串下标。</summary>
        private static readonly Regex DotPathRegex = new Regex(@"([A-Za-z_$][\w$]*)|\[(\d+)\]|\[(""((?:[^""]|\\.)*)"")\]", RegexOptions.Compiled);

        /// <summary>
        /// 点号方括号路径查询。例如 "name"、"address.city"、"items[0].name"。
        /// 找不到返回 null。
        /// </summary>
        public static object GetByPath(object root, string path)
        {
            if (root == null || string.IsNullOrWhiteSpace(path)) return root;
            object cur = root;
            foreach (Match m in DotPathRegex.Matches(path))
            {
                if (cur == null) return null;
                if (m.Groups[1].Success)
                {
                    var d = cur as Dictionary<string, object>;
                    if (d == null) return null;
                    if (!d.TryGetValue(m.Groups[1].Value, out cur)) return null;
                }
                else if (m.Groups[2].Success)
                {
                    var arr = cur as object[];
                    if (arr == null) return null;
                    int i = int.Parse(m.Groups[2].Value);
                    if (i < 0 || i >= arr.Length) return null;
                    cur = arr[i];
                }
                else if (m.Groups[3].Success)
                {
                    var d = cur as Dictionary<string, object>;
                    if (d == null) return null;
                    string k = m.Groups[4].Success ? UnescapeJsonString(m.Groups[4].Value) : m.Groups[3].Value;
                    if (!d.TryGetValue(k, out cur)) return null;
                }
            }
            return cur;
        }

        /// <summary>按路径设置值；路径不存在时中间节点自动创建。</summary>
        public static bool SetByPath(object root, string path, object value)
        {
            if (root == null || string.IsNullOrWhiteSpace(path)) return false;
            var matches = DotPathRegex.Matches(path);
            object cur = root;
            for (int i = 0; i < matches.Count - 1; i++)
            {
                Match m = matches[i];
                Match next = matches[i + 1];
                if (m.Groups[1].Success)
                {
                    var d = cur as Dictionary<string, object>;
                    if (d == null) return false;
                    string key = m.Groups[1].Value;
                    if (!d.TryGetValue(key, out object nextVal) || nextVal == null)
                    {
                        // 根据下一段类型决定创建 Dict 还是 Array
                        nextVal = next.Groups[2].Success ? (object)new List<object>() : new Dictionary<string, object>();
                        d[key] = nextVal;
                    }
                    cur = nextVal;
                }
                else if (m.Groups[2].Success)
                {
                    var list = cur as List<object>;
                    if (list == null) return false;
                    int idx = int.Parse(m.Groups[2].Value);
                    while (list.Count <= idx) list.Add(null);
                    if (list[idx] == null)
                        list[idx] = next.Groups[2].Success ? (object)new List<object>() : new Dictionary<string, object>();
                    cur = list[idx];
                }
            }
            Match last = matches[matches.Count - 1];
            if (last.Groups[1].Success)
            {
                var d = cur as Dictionary<string, object>;
                if (d == null) return false;
                d[last.Groups[1].Value] = value;
                return true;
            }
            if (last.Groups[2].Success)
            {
                var list = cur as List<object>;
                if (list == null) return false;
                int idx = int.Parse(last.Groups[2].Value);
                while (list.Count <= idx) list.Add(null);
                list[idx] = value;
                return true;
            }
            return false;
        }

        /// <summary>反转义 JSON 字符串中的反斜杠转义序列（含 uXXXX）。</summary>
        private static string UnescapeJsonString(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char nx = s[++i];
                    switch (nx)
                    {
                        case '"': case '\\': case '/': sb.Append(nx); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            sb.Append((char)Convert.ToInt32(s.Substring(++i, 4), 16));
                            i += 3; break;
                        default: sb.Append(nx); break;
                    }
                }
                else sb.Append(s[i]);
            }
            return sb.ToString();
        }

        /// <summary>JSON Pointer RFC 6901 查询；失败返回 null</summary>
        public static object GetByPointer(object root, string pointer)
        {
            if (string.IsNullOrEmpty(pointer)) return root;
            if (pointer[0] != '/') return null;
            string[] parts = pointer.TrimStart('/').Split('/');
            object cur = root;
            foreach (string raw in parts)
            {
                string token = raw.Replace("~1", "/").Replace("~0", "~");
                if (cur == null) return null;
                var d = cur as Dictionary<string, object>;
                if (d != null) { if (!d.TryGetValue(token, out cur)) return null; continue; }
                var arr = cur as object[];
                if (arr != null) { int i; if (!int.TryParse(token, out i) || i < 0 || i >= arr.Length) return null; cur = arr[i]; continue; }
                return null;
            }
            return cur;
        }

        /// <summary>按 RFC 6901 转义 JSON Pointer 令牌：~ 写成 ~0，/ 写成 ~1。</summary>
        private static string EscapePointer(string s) { return s.Replace("~", "~0").Replace("/", "~1"); }

        #endregion

        #region ==================== 静态：深度合并 / JSON Patch RFC 6902 ====================

        /// <summary>
        /// 深度合并。override 覆盖 base 的同名字段；override 中 null 键会删掉 base 中对应的键；
        /// 对象递归合并；数组直接被 override 数组替换。
        /// </summary>
        public static object Merge(object baseObj, object overrideObj)
        {
            if (baseObj == null) return Clone(overrideObj);
            if (overrideObj == null) return Clone(baseObj);
            var bd = baseObj as Dictionary<string, object>;
            var od = overrideObj as Dictionary<string, object>;
            if (bd != null && od != null)
            {
                var result = new Dictionary<string, object>(bd);
                foreach (var kv in od)
                {
                    if (kv.Value == null) { result.Remove(kv.Key); continue; }
                    object existing;
                    if (result.TryGetValue(kv.Key, out existing))
                        result[kv.Key] = Merge(existing, kv.Value);
                    else
                        result[kv.Key] = Clone(kv.Value);
                }
                return result;
            }
            return Clone(overrideObj);
        }

        /// <summary>应用 JSON Patch（RFC 6902）。patch 为 [{op,path,value?,from?}, ...]。成功返回 true。</summary>
        public static bool ApplyPatch(ref object root, IEnumerable<Dictionary<string, object>> patches)
        {
            if (patches == null) return false;
            try
            {
                foreach (var op in patches)
                {
                    string type = Convert.ToString(op["op"]);
                    string path = Convert.ToString(op["path"]);
                    switch (type)
                    {
                        case "add": root = PatchAdd(root, path, op["value"]); break;
                        case "remove": root = PatchRemove(root, path); break;
                        case "replace": root = PatchReplace(root, path, op["value"]); break;
                        case "copy": root = PatchCopy(root, path, Convert.ToString(op["from"])); break;
                        case "move": root = PatchMove(root, path, Convert.ToString(op["from"])); break;
                        case "test":
                            object expect = GetByPointer(root, path);
                            if (!Equals(expect, op["value"])) return false;
                            break;
                        default: return false;
                    }
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>把弱类型 JSON 数组 object[] 转为可插入删除的 List<object>。</summary>
        private static List<object> AsList(object arr)
        {
            var list = new List<object>();
            if (arr is object[] aa) list.AddRange(aa);
            return list;
        }
        /// <summary>把 List<object> 转回弱类型 object[] 数组。</summary>
        private static object AsArray(List<object> list) { return list.ToArray(); }

        /// <summary>沿 JSON Pointer 定位父节点，并把最后一段令牌（已解码）通过 out 参数返回。</summary>
        private static object NavigateParent(object root, string pointer, out string lastToken)
        {
            string[] parts = pointer.TrimStart('/').Split('/');
            string[] parent = new string[parts.Length - 1];
            Array.Copy(parts, 0, parent, 0, parent.Length);
            lastToken = parts[parts.Length - 1].Replace("~1", "/").Replace("~0", "~");
            object cur = root;
            foreach (string p in parent)
            {
                if (cur == null) throw new InvalidOperationException();
                var d = cur as Dictionary<string, object>;
                if (d != null) { cur = d[p.Replace("~1", "/").Replace("~0", "~")]; continue; }
                var arr = cur as object[];
                int i; if (!int.TryParse(p, out i)) throw new InvalidOperationException();
                cur = arr[i];
            }
            return cur;
        }

        /// <summary>RFC 6902 add：对象加键或数组在指定下标插入元素。</summary>
        private static object PatchAdd(object root, string pointer, object value)
        {
            if (pointer == "") return Clone(value);
            string token; object parent = NavigateParent(root, pointer, out token);
            var d = parent as Dictionary<string, object>;
            if (d != null) { d[token] = Clone(value); return root; }
            var arr = AsList(parent);
            int idx; if (!int.TryParse(token, out idx)) throw new InvalidOperationException();
            arr.Insert(idx, Clone(value));
            return AsArray(arr);
        }
        /// <summary>RFC 6902 remove：删除对象键或数组指定下标元素。</summary>
        private static object PatchRemove(object root, string pointer)
        {
            string token; object parent = NavigateParent(root, pointer, out token);
            var d = parent as Dictionary<string, object>;
            if (d != null) { d.Remove(token); return root; }
            var arr = AsList(parent);
            arr.RemoveAt(int.Parse(token));
            return AsArray(arr);
        }
        /// <summary>RFC 6902 replace：整体替换指针指向的值。</summary>
        private static object PatchReplace(object root, string pointer, object value)
        {
            string token; object parent = NavigateParent(root, pointer, out token);
            var d = parent as Dictionary<string, object>;
            if (d != null) { d[token] = Clone(value); return root; }
            var arr = AsList(parent); arr[int.Parse(token)] = Clone(value); return AsArray(arr);
        }
        /// <summary>RFC 6902 copy：取出 from 处值的深拷贝后在目标处 add。</summary>
        private static object PatchCopy(object root, string dest, string from)
        {
            object v = GetByPointer(root, from);
            return PatchAdd(root, dest, v);
        }
        /// <summary>RFC 6902 move：先从 from 移除，再把该值 add 到目标。</summary>
        private static object PatchMove(object root, string dest, string from)
        {
            object v = GetByPointer(root, from);
            root = PatchRemove(root, from);
            return PatchAdd(root, dest, v);
        }

        #endregion

        #region ==================== 静态：大小写转换 (camel / snake / Pascal / kebab) ====================

        /// <summary>
        /// 标识符命名风格枚举：Camel=小驼峰(camelCase)，Snake=下划线(snake_case)，
        /// Pascal=大驼峰(PascalCase)，Kebab=短横线(kebab-case)，None=不转换。
        /// </summary>
        public enum CaseStyle { Camel, Snake, Pascal, Kebab, None }

        /// <summary>把字符串转成指定风格</summary>
        public static string ConvertCase(string input, CaseStyle style)
        {
            if (string.IsNullOrWhiteSpace(input) || style == CaseStyle.None) return input;
            // 先切分：按非字母数字边界 + 小写→大写边界
            var words = new List<string>();
            var cur = new StringBuilder();
            bool prevLower = false;
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (prevLower && char.IsUpper(c)) { words.Add(cur.ToString()); cur.Clear(); }
                    cur.Append(c); prevLower = char.IsLower(c);
                }
                else
                {
                    if (cur.Length > 0) { words.Add(cur.ToString()); cur.Clear(); }
                    prevLower = false;
                }
            }
            if (cur.Length > 0) words.Add(cur.ToString());
            if (words.Count == 0) return input;

            string Join(Func<string, string> normalizer, string sep, bool firstLower)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < words.Count; i++)
                {
                    string w = normalizer(words[i]);
                    if (i == 0 && firstLower) sb.Append(char.ToLowerInvariant(w[0])).Append(w.Substring(1));
                    else sb.Append(char.ToUpperInvariant(w[0])).Append(w.Substring(1));
                    if (i < words.Count - 1) sb.Append(sep);
                }
                return sb.ToString();
            }
            switch (style)
            {
                case CaseStyle.Camel:  return Join(w => w.ToLowerInvariant(), "", true);
                case CaseStyle.Pascal: return Join(w => w.ToLowerInvariant(), "", false);
                case CaseStyle.Snake:  return Join(w => w.ToLowerInvariant(), "_", false);
                case CaseStyle.Kebab:  return Join(w => w.ToLowerInvariant(), "-", false);
                default: return input;
            }
        }

        /// <summary>把整棵 JSON 树的所有键转换为指定风格（递归）</summary>
        public static object ConvertCase(object root, CaseStyle style, bool convertPascalToCamel = false)
        {
            var d = root as Dictionary<string, object>;
            if (d != null)
            {
                var r = new Dictionary<string, object>();
                foreach (var kv in d)
                {
                    string newKey = ConvertCase(kv.Key, style);
                    if (convertPascalToCamel && style == CaseStyle.Camel && char.IsUpper(newKey[0]))
                        newKey = char.ToLowerInvariant(newKey[0]) + newKey.Substring(1);
                    r[newKey] = ConvertCase(kv.Value, style, convertPascalToCamel);
                }
                return r;
            }
            var arr = root as object[];
            if (arr != null) return arr.Select(x => ConvertCase(x, style, convertPascalToCamel)).ToArray();
            return root;
        }

        #endregion

        #region ==================== 静态：JSON ↔ XML ↔ CSV ↔ DataTable ====================

        /// <summary>JSON → XML 字符串。根节点名默认 root；数组元素用 &lt;item> 包裹。</summary>
        public static string ToXml(string json, string rootName = "root")
        {
            object obj = Deserialize(json);
            XElement root = ToXElement(obj, rootName ?? "root");
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            return doc.ToString();
        }

        /// <summary>XML → JSON 字符串。取首个元素作为 JSON 根。</summary>
        public static string FromXml(string xml, bool pretty = true)
        {
            XDocument doc = XDocument.Parse(xml);
            if (doc.Root == null) return "null";
            object obj = FromXElement(doc.Root);
            return Serialize(obj, pretty);
        }

        /// <summary>转换为 XElement。</summary>
        private static XElement ToXElement(object obj, string name)
        {
            name = XmlConvert.EncodeName(name ?? "root");
            var d = obj as Dictionary<string, object>;
            if (d != null)
            {
                var el = new XElement(name);
                foreach (var kv in d)
                    el.Add(ToXElement(kv.Value, kv.Key));
                return el;
            }
            var arr = obj as object[];
            if (arr != null)
            {
                var el = new XElement(name);
                foreach (var item in arr)
                    el.Add(ToXElement(item, "item"));
                return el;
            }
            string val = obj == null ? "" : Convert.ToString(obj, CultureInfo.InvariantCulture);
            return new XElement(name, val ?? "");
        }

        /// <summary>从 XElement 创建实例。</summary>
        private static object FromXElement(XElement el)
        {
            // 若有子元素 → Dictionary；若没有子元素 → 标量；混合 → 优先 Dictionary
            if (!el.HasElements)
            {
                string v = el.Value;
                // 数字/布尔/null 自动尝试
                if (v == null || v == "") return null;
                double d; long l; bool b;
                if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return d;
                if (long.TryParse(v, out l)) return l;
                if (bool.TryParse(v, out b)) return b;
                if (v.Equals("null", StringComparison.OrdinalIgnoreCase)) return null;
                return v;
            }
            // 同名子元素合并为数组
            var groups = el.Elements().GroupBy(e => e.Name.LocalName).ToArray();
            // 如果所有子元素同名 → 数组
            if (groups.Length == 1 && groups[0].Count() > 1)
                return groups[0].Select(g => FromXElement(g)).ToArray();
            // 否则对象
            var dict = new Dictionary<string, object>();
            foreach (var g in groups)
            {
                var items = g.Select(e => FromXElement(e)).ToArray();
                dict[g.Key] = items.Length == 1 ? items[0] : (object)items;
            }
            return dict;
        }

        /// <summary>JSON 数组 → CSV。第一行表头自动从首元素的键取。</summary>
        public static string ToCsv(string json, string delimiter = ",")
        {
            object obj = Deserialize(json);
            var arr = obj as object[];
            if (arr == null) { var d = obj as Dictionary<string, object>; if (d != null) arr = new[] { d }; }
            if (arr == null || arr.Length == 0) return string.Empty;
            var headers = new List<string>();
            foreach (var item in arr)
            {
                var d = item as Dictionary<string, object>; if (d == null) continue;
                foreach (string k in d.Keys) if (!headers.Contains(k)) headers.Add(k);
            }
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(delimiter, headers.Select(h => EscapeCsv(h, delimiter))));
            foreach (var item in arr)
            {
                var d = item as Dictionary<string, object>; if (d == null) { sb.AppendLine(); continue; }
                sb.AppendLine(string.Join(delimiter, headers.Select(h => EscapeCsv(d.ContainsKey(h) ? Convert.ToString(d[h], CultureInfo.InvariantCulture) ?? "" : "", delimiter))));
            }
            return sb.ToString().TrimEnd('\r', '\n');
        }

        /// <summary>CSV → JSON 数组。首行表头（空表头则用 col0/col1）。</summary>
        public static string FromCsv(string csv, string delimiter = ",", string pretty = "true")
        {
            bool prettyB; bool.TryParse(pretty, out prettyB);
            var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return "[]";
            string headerLine = lines[0];
            var headers = SplitCsvLine(headerLine, delimiter);
            for (int i = 0; i < headers.Length; i++) if (string.IsNullOrWhiteSpace(headers[i])) headers[i] = "col" + i;
            var list = new List<Dictionary<string, object>>();
            for (int i = 1; i < lines.Length; i++)
            {
                var fields = SplitCsvLine(lines[i], delimiter);
                if (fields.Length == 0) continue;
                var d = new Dictionary<string, object>();
                for (int j = 0; j < headers.Length; j++)
                {
                    string val = j < fields.Length ? fields[j] : "";
                    object parsed;
                    if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double dd)) parsed = dd;
                    else if (long.TryParse(val, out long ll)) parsed = ll;
                    else if (bool.TryParse(val, out bool bb)) parsed = bb;
                    else parsed = val;
                    d[headers[j]] = parsed;
                }
                list.Add(d);
            }
            return Serialize(list, prettyB);
        }

        /// <summary>按 CSV 规则转义单个字段：含分隔符/引号/换行时用双引号包裹并把引号 doubled。</summary>
        private static string EscapeCsv(string s, string delim)
        {
            if (s == null) return "";
            if (s.Contains(delim) || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
        /// <summary>按分隔符与引号配对状态切分一行 CSV 为字段数组。</summary>
        private static string[] SplitCsvLine(string line, string delim)
        {
            var result = new List<string>(); var cur = new StringBuilder(); bool inQ = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQ && i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; }
                    else inQ = !inQ;
                }
                else if (!inQ && c == delim[0]) { result.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(c);
            }
            result.Add(cur.ToString());
            return result.ToArray();
        }

        /// <summary>JSON 数组 → DataTable。数组元素必须都是对象。</summary>
        public static DataTable ToDataTable(string json)
        {
            var arr = Deserialize(json) as object[];
            DataTable dt = new DataTable();
            if (arr == null || arr.Length == 0) return dt;
            foreach (var item in arr)
            {
                var d = item as Dictionary<string, object>; if (d == null) continue;
                foreach (var k in d.Keys)
                    if (!dt.Columns.Contains(k))
                        dt.Columns.Add(k);
            }
            foreach (var item in arr)
            {
                var d = item as Dictionary<string, object>; if (d == null) continue;
                DataRow row = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                    row[col.ColumnName] = d.ContainsKey(col.ColumnName) ? ConvertToCellValue(d[col.ColumnName]) : DBNull.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }
        /// <summary>把 JSON 值转换为可写入 DataTable 单元格的对象（数组/对象序列化为 JSON 文本）。</summary>
        private static object ConvertToCellValue(object v)
        {
            if (v == null) return DBNull.Value;
            if (v is object[] arr) return Serialize(arr, false);
            if (v is Dictionary<string, object> dict) return Serialize(dict, false);
            return v;
        }

        /// <summary>DataTable → JSON 数组。</summary>
        public static string FromDataTable(DataTable dt, bool pretty = true)
        {
            if (dt == null) return "[]";
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var d = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    d[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                list.Add(d);
            }
            return Serialize(list, pretty);
        }

        #endregion

        #region ==================== 实体特性映射（HIgnore / HName / HDateTimeFormat） ====================

        /// <summary>实体特性映射时允许递归的最大深度（与 HSerializedType 保持一致，防止循环引用栈溢出）</summary>
        private const int HMaxMapDepth = HSerializedType.MaxDepth;

        /// <summary>获取某类型参与序列化的成员列表（委托 HSerializedType，已按 HIgnore/HName 等特性过滤改名）</summary>
        private static List<HSerializedType.HMember> GetSerializedMembers(Type type)
        {
            return HSerializedType.GetMembers(type);
        }

        /// <summary>类型（或其成员）是否使用了 H 系列序列化特性（HIgnore/HName/HDateTimeFormat/HXmlAttribute）</summary>
        public static bool HasHAttributes(Type type)
        {
            return HSerializedType.HasHAttributes(type);
        }

        /// <summary>
        /// 实体对象 → 可直接 JSON 序列化的弱类型树（Dictionary/List/标量）。
        /// 特性：[HIgnore] 跳过、[HName] 改名、[HDateTimeFormat] 日期格式。
        /// </summary>
        public static object ToJsonTree(object obj)
        {
            return ToJsonTreeCore(obj, 0, null);
        }

        /// <summary>实体/字典/集合 → 弱类型 JSON 树的递归核心：标量原样返回（H 包装取底层值），日期按格式串，字典转 Dictionary，集合转 List，其余实体按成员映射展开。</summary>
        private static object ToJsonTreeCore(object obj, int depth, string dateFormat)
        {
            if (obj == null || depth > HMaxMapDepth) return null;
            Type t = obj.GetType();
            if (t.IsEnum) return Convert.ChangeType(obj, Enum.GetUnderlyingType(t), CultureInfo.InvariantCulture);
            // H 系列标量包装（HDouble/HLong/HBool/HInt 等）按底层原生值平铺输出
            Type hNativeType;
            if (HScalarValue.IsHScalar(t, out hNativeType)) return HScalarValue.Unwrap(obj);
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return obj;
                case TypeCode.DateTime:
                    return ((DateTime)obj).ToString(string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd HH:mm:ss" : dateFormat, CultureInfo.InvariantCulture);
                default:
                    if (obj is DateTimeOffset dto)
                        return dto.ToString(string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd HH:mm:ss zzz" : dateFormat, CultureInfo.InvariantCulture);
                    if (obj is Guid g) return g.ToString("D");
                    if (obj is TimeSpan ts) return ts.ToString("c");
                    IDictionary dict = obj as IDictionary;
                    if (dict != null)
                    {
                        Dictionary<string, object> d = new Dictionary<string, object>();
                        foreach (DictionaryEntry e in dict)
                            d[Convert.ToString(e.Key, CultureInfo.InvariantCulture)] = ToJsonTreeCore(e.Value, depth + 1, null);
                        return d;
                    }
                    if (obj is IEnumerable en)
                    {
                        List<object> list = new List<object>();
                        foreach (object item in en) list.Add(ToJsonTreeCore(item, depth + 1, null));
                        return list;
                    }
                    Dictionary<string, object> ed = new Dictionary<string, object>();
                    foreach (HSerializedType.HMember m in GetSerializedMembers(t))
                        ed[m.Name] = ToJsonTreeCore(m.Get(obj), depth + 1, m.DateFormat);
                    return ed;
            }
        }

        /// <summary>弱类型树（Dictionary/List/标量）→ 实体对象</summary>
        public static object FromJsonTree(object tree, Type targetType)
        {
            return FromJsonTreeCore(tree, targetType, 0);
        }

        /// <summary>弱类型树 → 实体对象（泛型）</summary>
        public static T FromJsonTree<T>(object tree)
        {
            return (T)FromJsonTreeCore(tree, typeof(T), 0);
        }

        /// <summary>弱类型 JSON 树 → 目标类型实例的递归核心：字典转实体/字典，数组转数组/泛型集合，叶子走 ConvertScalar。</summary>
        private static object FromJsonTreeCore(object tree, Type target, int depth)
        {
            if (depth > HMaxMapDepth) return null;
            Type nt = Nullable.GetUnderlyingType(target) ?? target;
            if (tree == null)
            {
                if (target.IsValueType && Nullable.GetUnderlyingType(target) == null) return Activator.CreateInstance(target);
                return null;
            }
            if (target == typeof(object)) return tree;
            if (nt == typeof(string)) return Convert.ToString(tree, CultureInfo.InvariantCulture);

            IDictionary<string, object> d = tree as IDictionary<string, object>;
            if (d != null)
            {
                if (typeof(IDictionary<string, object>).IsAssignableFrom(nt)) return d;
                if (nt.IsGenericType)
                {
                    Type[] gargs = nt.GetGenericArguments();
                    if (gargs.Length == 2 && typeof(IDictionary<,>).MakeGenericType(gargs).IsAssignableFrom(nt))
                    {
                        IDictionary nd = (IDictionary)Activator.CreateInstance(nt);
                        foreach (KeyValuePair<string, object> kv in d)
                            nd[Convert.ChangeType(kv.Key, gargs[0], CultureInfo.InvariantCulture)] = FromJsonTreeCore(kv.Value, gargs[1], depth + 1);
                        return nd;
                    }
                }
                if (typeof(IDictionary).IsAssignableFrom(nt) && !nt.IsInterface && !nt.IsAbstract)
                {
                    IDictionary nd = (IDictionary)Activator.CreateInstance(nt);
                    foreach (KeyValuePair<string, object> kv in d) nd[kv.Key] = kv.Value;
                    return nd;
                }
                return PopulateEntity(d, nt, depth);
            }

            IList list = tree as IList;
            if (list != null)
            {
                if (nt.IsArray)
                {
                    Type et = nt.GetElementType();
                    Array arr = Array.CreateInstance(et, list.Count);
                    for (int i = 0; i < list.Count; i++) arr.SetValue(FromJsonTreeCore(list[i], et, depth + 1), i);
                    return arr;
                }
                if (nt.IsGenericType)
                {
                    Type[] gargs = nt.GetGenericArguments();
                    if (gargs.Length == 1 && typeof(IEnumerable<>).MakeGenericType(gargs[0]).IsAssignableFrom(nt))
                    {
                        Type lt = typeof(List<>).MakeGenericType(gargs[0]);
                        IList il = (IList)Activator.CreateInstance(lt);
                        foreach (object item in list) il.Add(FromJsonTreeCore(item, gargs[0], depth + 1));
                        return il;
                    }
                }
                if (typeof(IEnumerable).IsAssignableFrom(nt))
                {
                    List<object> lo = new List<object>();
                    foreach (object item in list) lo.Add(item);
                    return lo;
                }
                return tree;
            }
            return ConvertScalar(tree, nt, null);
        }

        /// <summary>新建实体实例并按成员映射把字典各键值填充进去，逐键容错。</summary>
        private static object PopulateEntity(IDictionary<string, object> d, Type type, int depth)
        {
            object obj;
            try { obj = Activator.CreateInstance(type); }
            catch { return null; }
            List<HSerializedType.HMember> members = GetSerializedMembers(type);
            foreach (KeyValuePair<string, object> kv in d)
            {
                HSerializedType.HMember m = FindMember(members, kv.Key);
                if (m == null) continue;
                try
                {
                    Type mt = Nullable.GetUnderlyingType(m.MemberType) ?? m.MemberType;
                    object value = (kv.Value is IDictionary<string, object> || kv.Value is IList)
                        ? FromJsonTreeCore(kv.Value, m.MemberType, depth + 1)
                        : ConvertScalar(kv.Value, mt, m.DateFormat);
                    m.Set(obj, value);
                }
                catch { }
            }
            return obj;
        }

        /// <summary>先精确名、再忽略大小写、再按原始名依次匹配实体成员映射。</summary>
        private static HSerializedType.HMember FindMember(List<HSerializedType.HMember> members, string key)
        {
            foreach (HSerializedType.HMember m in members) if (m.Name == key) return m;
            foreach (HSerializedType.HMember m in members) if (string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)) return m;
            foreach (HSerializedType.HMember m in members) if (string.Equals(m.OriginalName, key, StringComparison.OrdinalIgnoreCase)) return m;
            return null;
        }

        /// <summary>旧版 ASP.NET /Date(毫秒数)/ 日期字面量的识别正则。</summary>
        private static readonly Regex HJsonDateRegex = new Regex(@"^/Date\((-?\d+)(?:[+-]\d{4})?\)/$", RegexOptions.Compiled);

        /// <summary>弱类型标量 → 指定类型值（兼容 /Date()/ 旧格式、枚举、可空类型、Guid、TimeSpan、H 系列标量包装）</summary>
        private static object ConvertScalar(object v, Type t, string dateFormat)
        {
            if (v == null) return null;
            if (t.IsInstanceOfType(v)) return v;
            // 目标为 HDouble/HLong/HBool/HInt 等 H 包装类型：先按底层原生类型转换，再装箱为 H 实例
            Type hNativeType;
            if (HScalarValue.IsHScalar(t, out hNativeType))
                return HScalarValue.Wrap(t, ConvertScalar(v, hNativeType, dateFormat));
            try
            {
                if (t.IsEnum)
                {
                    string es = v as string;
                    return es != null ? Enum.Parse(t, es, true) : Enum.ToObject(t, Convert.ToInt64(v, CultureInfo.InvariantCulture));
                }
                if (t == typeof(DateTime))
                {
                    DateTime dt;
                    if (v is DateTime) return (DateTime)v;
                    string s = Convert.ToString(v, CultureInfo.InvariantCulture);
                    if (!string.IsNullOrWhiteSpace(dateFormat) &&
                        DateTime.TryParseExact(s, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        return dt;
                    Match md = HJsonDateRegex.Match(s ?? string.Empty);
                    if (md.Success)
                        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .AddMilliseconds(long.Parse(md.Groups[1].Value, CultureInfo.InvariantCulture)).ToLocalTime();
                    return DateTime.Parse(s, CultureInfo.InvariantCulture);
                }
                if (t == typeof(DateTimeOffset))
                    return v is DateTimeOffset ? (object)(DateTimeOffset)v : DateTimeOffset.Parse(Convert.ToString(v, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                if (t == typeof(Guid)) return new Guid(Convert.ToString(v, CultureInfo.InvariantCulture));
                if (t == typeof(TimeSpan)) return TimeSpan.Parse(Convert.ToString(v, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                if (t == typeof(char))
                {
                    string cs = Convert.ToString(v, CultureInfo.InvariantCulture);
                    return string.IsNullOrEmpty(cs) ? '\0' : cs[0];
                }
                if (t == typeof(bool))
                {
                    if (v is bool) return (bool)v;
                    string bs = Convert.ToString(v, CultureInfo.InvariantCulture).Trim().ToLowerInvariant();
                    return bs == "1" || bs == "true" || bs == "yes" || bs == "y";
                }
                return Convert.ChangeType(v, t, CultureInfo.InvariantCulture);
            }
            catch
            {
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }
        }

        #endregion

        #region ==================== JSON → C# 实体类代码生成 ====================

        /// <summary>代码生成中间模型：一个生成属性的描述（属性名、C# 类型名、原始 JSON 键名）</summary>
        private sealed class HGenProp
        {
            /// <summary>生成的 C# 属性名（PascalCase 合法标识符）</summary>
            public string Name;
            /// <summary>生成的 C# 类型名（标量名或生成类名/List 包装）</summary>
            public string Type;
            /// <summary>原始 JSON 键名（与属性名不同时通过 [HName] 标注）</summary>
            public string WireName; // 原始 JSON 键名（与属性名不同时加 [HName]）
        }
        /// <summary>代码生成中间模型：一个生成类的描述（类名及其属性列表）</summary>
        private sealed class HGenClass
        {
            /// <summary>生成的 C# 类名（已去重）</summary>
            public string Name;
            /// <summary>该类包含的全部属性描述</summary>
            public List<HGenProp> Props = new List<HGenProp>();
        }

        /// <summary>
        /// 把 JSON 文本直接生成为 C# 实体类代码。自动推断 int/long/double/bool/DateTime/string、
        /// 嵌套对象与集合；键名与属性名不一致时自动加 [HName("原名")]，生成的类可直接用
        /// HJsonFile.Deserialize&lt;T&gt; / ReadObject&lt;T&gt; 反序列化。
        /// </summary>
        public static string GenerateCSharpClass(string json, string rootName = "Root", string namespaceName = null)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            object tree;
            try { tree = Deserialize(json); }
            catch (Exception ex) { throw new ArgumentException(HTranslation.GetContent("JSON 解析失败！   Invalid JSON: ") + ex.Message, ex); }

            Dictionary<string, HGenClass> classes = new Dictionary<string, HGenClass>(StringComparer.Ordinal);
            HashSet<string> classNames = new HashSet<string>(StringComparer.Ordinal);
            InferAndBuild(tree, string.IsNullOrWhiteSpace(rootName) ? "Root" : rootName, classes, classNames);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using HFromUI.HSocket;");
            sb.AppendLine();
            bool hasNs = !string.IsNullOrWhiteSpace(namespaceName);
            if (hasNs) { sb.Append("namespace ").Append(namespaceName).AppendLine(); sb.AppendLine("{"); }
            string indent = hasNs ? "    " : "";
            foreach (HGenClass gc in classes.Values)
            {
                sb.Append(indent).Append("public class ").AppendLine(gc.Name);
                sb.Append(indent).AppendLine("{");
                foreach (HGenProp p in gc.Props)
                {
                    if (!string.IsNullOrEmpty(p.WireName))
                        sb.Append(indent).Append("    [HName(\"").Append(EscapeCodeString(p.WireName)).AppendLine("\")]");
                    sb.Append(indent).Append("    public ").Append(p.Type).Append(' ').Append(p.Name).AppendLine(" { get; set; }");
                }
                sb.Append(indent).AppendLine("}");
                sb.AppendLine();
            }
            if (hasNs) sb.AppendLine("}");
            return sb.ToString().TrimEnd() + Environment.NewLine;
        }

        /// <summary>读取当前 JSON 文件内容并生成 C# 实体类代码</summary>
        public string GenerateClassFromFile(string rootName = "Root", string namespaceName = null)
        {
            string json = ReadAllText();
            return string.IsNullOrWhiteSpace(json) ? null : GenerateCSharpClass(json, rootName, namespaceName);
        }

        /// <summary>推断 JSON 节点对应的 C# 类型名；对象与对象数组递归生成实体类，标量数组统一元素类型。</summary>
        private static string InferAndBuild(object v, string suggested, Dictionary<string, HGenClass> classes, HashSet<string> classNames)
        {
            if (v == null) return "object";
            Dictionary<string, object> d = v as Dictionary<string, object>;
            if (d != null) return BuildClassFromDict(d, suggested, classes, classNames);
            IList list = v as IList;
            if (list != null)
            {
                if (list.Count == 0) return "List<object>";
                Dictionary<string, object> merged = null;
                bool allDict = true;
                foreach (object item in list)
                {
                    Dictionary<string, object> id = item as Dictionary<string, object>;
                    if (id == null) { allDict = false; break; }
                    if (merged == null) merged = new Dictionary<string, object>(id);
                    else
                    {
                        foreach (KeyValuePair<string, object> kv in id)
                            if (!merged.ContainsKey(kv.Key) || merged[kv.Key] == null) merged[kv.Key] = kv.Value;
                    }
                }
                if (allDict && merged != null)
                {
                    string cn = BuildClassFromDict(merged, Singularize(suggested), classes, classNames);
                    return "List<" + cn + ">";
                }
                string unified = null;
                foreach (object item in list)
                {
                    string t = ScalarTypeName(item);
                    unified = unified == null ? t : UnifyScalarType(unified, t);
                }
                return "List<" + (unified ?? "object") + ">";
            }
            return ScalarTypeName(v) ?? "object";
        }

        /// <summary>为一个 JSON 对象字典生成 C# 类模型并登记到 classes，逐属性递归推断类型。</summary>
        private static string BuildClassFromDict(Dictionary<string, object> d, string suggested, Dictionary<string, HGenClass> classes, HashSet<string> classNames)
        {
            string cn = UniqueName(PascalIdent(suggested), classNames);
            HGenClass gc = new HGenClass { Name = cn };
            classes[cn] = gc;
            HashSet<string> propNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> kv in d)
            {
                string pn = UniqueName(PascalIdent(kv.Key), propNames);
                string type = InferAndBuild(kv.Value, kv.Key, classes, classNames);
                gc.Props.Add(new HGenProp { Name = pn, Type = type, WireName = (pn == kv.Key) ? null : kv.Key });
            }
            return cn;
        }

        /// <summary>按 JSON 标量值推断 C# 类型名（字符串再尝试识别日期时间）。</summary>
        private static string ScalarTypeName(object v)
        {
            if (v == null) return "object";
            if (v is bool) return "bool";
            if (v is int) return "int";
            if (v is long) return "long";
            if (v is decimal) return "decimal";
            if (v is double || v is float) return "double";
            string s = v as string;
            if (s != null) return LooksLikeDateTime(s) ? "DateTime" : "string";
            return "object";
        }

        /// <summary>类 ISO 8601 日期时间字符串的识别正则（日期部分必填，时间部分可选）。</summary>
        private static readonly Regex HDateLikeRegex =
            new Regex(@"^\d{4}-\d{2}-\d{2}([T ]\d{1,2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:?\d{2})?)?$", RegexOptions.Compiled);

        /// <summary>正则与 TryParse 双重判断字符串是否像可解析的日期时间。</summary>
        private static bool LooksLikeDateTime(string s)
        {
            DateTime dt;
            return HDateLikeRegex.IsMatch(s.Trim()) &&
                   DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }

        /// <summary>统一数组元素的两个标量类型名：数值取宽度更大者，不同族退化为 string。</summary>
        private static string UnifyScalarType(string a, string b)
        {
            if (a == b) return a;
            int ra = ScalarRank(a), rb = ScalarRank(b);
            if (ra >= 0 && rb >= 0) return ra >= rb ? a : b;
            return "string";
        }

        /// <summary>数值类型宽度排名：int=1、long=2、double=3、decimal=4，非数值为 -1。</summary>
        private static int ScalarRank(string t)
        {
            switch (t)
            {
                case "int": return 1;
                case "long": return 2;
                case "double": return 3;
                case "decimal": return 4;
                default: return -1;
            }
        }

        /// <summary>把任意键名转为合法的 PascalCase C# 标识符</summary>
        public static string PascalIdent(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Prop";
            StringBuilder sb = new StringBuilder(s.Length);
            bool nextUpper = true;
            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(nextUpper ? char.ToUpperInvariant(c) : c);
                    nextUpper = false;
                }
                else nextUpper = true;
            }
            string r = sb.ToString();
            if (r.Length == 0) r = "Prop";
            if (char.IsDigit(r[0])) r = "_" + r;
            return r;
        }

        /// <summary>在已用名集合中为名称去重，冲突时追加数字后缀（2、3……）。</summary>
        private static string UniqueName(string name, HashSet<string> used)
        {
            if (!used.Contains(name)) { used.Add(name); return name; }
            for (int i = 2; ; i++)
            {
                string cand = name + i;
                if (!used.Contains(cand)) { used.Add(cand); return cand; }
            }
        }

        /// <summary>简易英文单数化（ies→y、去尾 s），用于由集合名推断元素类名。</summary>
        private static string Singularize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Item";
            if (name.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && name.Length > 3)
                return name.Substring(0, name.Length - 3) + "y";
            if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) && name.Length > 3
                && !name.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 1);
            return name + "Item";
        }

        /// <summary>把字符串转义为可嵌入 C# 双引号字面量的内容（反斜杠/引号/回车换行制表符）。</summary>
        private static string EscapeCodeString(string s)
        {
            if (s == null) return string.Empty;
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        #endregion

        #region ==================== 磁盘：安全读写（原有原子替换 + 备份 + 自动恢复） ====================

        /// <summary>成功写入后保留的最近一份备份后缀（File.Replace 时由旧主文件生成，供主文件损坏时回滚）</summary>
        private const string BackupSuffix = ".bak";
        /// <summary>原子写入的暂存文件后缀：新内容先写到此文件，再一次性替换主文件</summary>
        private const string WritingSuffix = ".writing";
        /// <summary>兼容旧版 HDictionaryFile 崩溃备份的遗留后缀：恢复时作为 .bak 之后的第二候选，写成功后清理</summary>
        private const string LegacyTempSuffix = ".temp";
        /// <summary>是否已在本实例执行过崩溃恢复（保证恢复逻辑只跑一次）</summary>
        private bool _recovered = false;

        /// <summary>最近一次自动恢复说明（无恢复时为空）</summary>
        public string RecoveryMessage = string.Empty;

        /// <summary>判断是否 ContentValid。</summary>
        protected virtual bool IsContentValid(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            try { CreateSerializer(MaxJsonLength).DeserializeObject(content); return true; }
            catch { return false; }
        }

        /// <summary>崩溃恢复（构造时自动调用）</summary>
        public void Recover()
        {
            lock (mLockRead) { try { EnsureRecovered(); } catch (Exception ex) { Error = ex.Message; } }
        }
        /// <summary>确保崩溃恢复只执行一次：主文件内容有效则清理暂存文件，否则尝试从备份恢复。</summary>
        private void EnsureRecovered()
        {
            if (_recovered) return; _recovered = true;
            try
            {
                if (TryReadValidContent(FilePath, out _)) { TryDelete(FilePath + WritingSuffix); return; }
                TryRestoreFromBackup();
            }
            catch (Exception ex) { Error = ex.Message; }
        }

        /// <summary>读取并解密指定路径文件，且必须通过内容格式校验才算有效。</summary>
        private bool TryReadValidContent(string path, out string decrypted)
        {
            decrypted = null;
            if (!HFile.HFilePath.FileExists(path)) return false;
            try { string dec = Decrypt(File.ReadAllText(path, FileEncoding)); if (!IsContentValid(dec)) return false; decrypted = dec; return true; }
            catch { return false; }
        }

        /// <summary>依次尝试用 .bak 与遗留 .temp 备份恢复主文件；损坏主文件改名为 .corrupt-时间戳 保留。</summary>
        private bool TryRestoreFromBackup()
        {
            string[] candidates = { FilePath + BackupSuffix, FilePath + LegacyTempSuffix };
            foreach (string cand in candidates)
            {
                if (!TryReadValidContent(cand, out _)) continue;
                try
                {
                    if (HFile.HFilePath.FileExists(FilePath))
                    {
                        string corrupt = FilePath + ".corrupt-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        try { File.Move(FilePath, corrupt); } catch { TryDelete(FilePath); }
                    }
                    File.Copy(cand, FilePath, true);
                    RecoveryMessage = HTranslation.GetContent("主文件损坏或缺失，已从备份恢复！   Restored from backup: ") + cand;
                    Error = RecoveryMessage;
                    TryDelete(FilePath + WritingSuffix);
                    return true;
                }
                catch (Exception ex) { Error = ex.Message; }
            }
            return false;
        }

        /// <summary>静默删除文件：文件不存在或删除被占用均忽略异常。</summary>
        private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }

        /// <summary>以独占方式创建文件写入全部字节并 Flush(true)，强制落到物理磁盘。</summary>
        private void WriteAllTextFlush(string path, string text)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = FileEncoding.GetBytes(text);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush(true);
            }
        }

        /// <summary>从磁盘读取全部文本（自动恢复 + Decrypt）</summary>
        public string ReadAllText()
        {
            lock (mLockRead)
            {
                try
                {
                    EnsureRecovered();
                    if (!HFile.HFilePath.FileExists(FilePath)) { Error = HTranslation.GetContent("文件不存在！   File does not exist: ") + FilePath; return null; }
                    string dec = Decrypt(File.ReadAllText(FilePath, FileEncoding));
                    if (!IsContentValid(dec))
                    {
                        if (TryRestoreFromBackup() && HFile.HFilePath.FileExists(FilePath)) return Decrypt(File.ReadAllText(FilePath, FileEncoding));
                        Error = HTranslation.GetContent("文件损坏且无可用备份！   Corrupted with no backup: ") + FilePath; return null;
                    }
                    return dec;
                }
                catch (Exception ex) { Error = ex.Message; return null; }
            }
        }

        /// <summary>安全写入磁盘（.writing → File.Replace → .bak，原子替换）</summary>
        public bool WriteAllText(string content)
        {
            lock (mLockRead)
            {
                string staging = FilePath + WritingSuffix, bak = FilePath + BackupSuffix;
                try
                {
                    string directory = Path.GetDirectoryName(FilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                    TryDelete(staging);
                    WriteAllTextFlush(staging, Encrypt(content ?? string.Empty));
                    if (HFile.HFilePath.FileExists(FilePath))
                    {
                        bool replaced = false;
                        try { TryDelete(bak); File.Replace(staging, FilePath, bak); replaced = true; } catch { replaced = false; }
                        if (!replaced)
                        {
                            try { File.Copy(FilePath, bak, true); File.Copy(staging, FilePath, true); TryDelete(staging); }
                            catch (Exception ex) { TryDelete(staging); Error = HTranslation.GetContent("写入失败！   Write failed: ") + ex.Message; return false; }
                        }
                    }
                    else { try { File.Move(staging, FilePath); } catch { File.Copy(staging, FilePath, true); TryDelete(staging); } }
                    TryDelete(FilePath + LegacyTempSuffix);
                    Error = string.Empty; return true;
                }
                catch (Exception ex) { TryDelete(staging); Error = HTranslation.GetContent("写入失败！   Write failed: ") + ex.Message; return false; }
            }
        }

        #endregion

        #region ==================== 磁盘：对象读写（原有 + 扩展） ====================

        /// <summary>将对象序列化为 JSON 并写入磁盘</summary>
        public bool WriteObject(object obj, bool? pretty = null)
        {
            lock (mLockRead) { try { _content = obj; string json = Serialize(obj, pretty ?? PrettyPrint, MaxJsonLength); return WriteAllText(json); } catch (Exception ex) { Error = ex.Message; return false; } }
        }

        /// <summary>从磁盘读取并反序列化为指定类型</summary>
        public T ReadObject<T>()
        {
            lock (mLockRead) { try { string json = ReadAllText(); if (string.IsNullOrWhiteSpace(json)) return default(T); T obj = Deserialize<T>(json, MaxJsonLength); _content = obj; return obj; } catch (Exception ex) { Error = ex.Message; return default(T); } }
        }

        /// <summary>从磁盘读取并反序列化为弱类型对象</summary>
        public object ReadObject()
        {
            lock (mLockRead) { try { string json = ReadAllText(); if (string.IsNullOrWhiteSpace(json)) return null; object obj = Deserialize(json, MaxJsonLength); _content = obj; return obj; } catch (Exception ex) { Error = ex.Message; return null; } }
        }

        /// <summary>非泛型 ReadObject(Type)</summary>
        public object ReadObject(Type type)
        {
            lock (mLockRead)
            {
                try
                {
                    string json = ReadAllText(); if (string.IsNullOrWhiteSpace(json)) return null;
                    var mi = typeof(HJsonFile).GetMethod("Deserialize", new[] { typeof(string), typeof(int) }).MakeGenericMethod(type);
                    object obj = mi.Invoke(null, new object[] { json, MaxJsonLength });
                    _content = obj; return obj;
                }
                catch (Exception ex) { Error = ex.Message; return null; }
            }
        }

        /// <summary>尝试读取 T，失败返回 false，不抛异常</summary>
        public bool TryReadObject<T>(out T obj)
        {
            try { obj = ReadObject<T>(); return true; }
            catch { obj = default(T); return false; }
        }

        /// <summary>尝试写入，失败返回 false</summary>
        public bool TryWriteObject(object obj) { return WriteObject(obj); }

        /// <summary>读取为字典（根必须是对象 {}）</summary>
        public Dictionary<string, object> ReadDictionary() { return ReadObject<Dictionary<string, object>>(); }

        /// <summary>写入字典</summary>
        public bool WriteDictionary(IDictionary<string, object> dictionary) { return WriteObject(dictionary); }

        /// <summary>保存最近一次读/写的对象到磁盘</summary>
        public bool Save()
        {
            if (_content == null) { Error = HTranslation.GetContent("没有可保存的对象！   No content to save."); return false; }
            return WriteObject(_content);
        }

        #endregion

        #region ==================== 实例：Update 原子读改写 + 文件级路径操作 ====================

        /// <summary>
        /// 原子读→改→写：读取为 Dictionary → 传入回调修改 → 写回。
        /// 任何步骤失败或回调返回 false 都不改变文件。
        /// </summary>
        public bool Update(Func<Dictionary<string, object>, bool> modifier)
        {
            if (modifier == null) { Error = "modifier is null"; return false; }
            lock (mLockRead)
            {
                try
                {
                    object root = ReadObject();
                    var d = root as Dictionary<string, object>;
                    if (d == null) { d = new Dictionary<string, object>(); }
                    if (!modifier(d)) return false;
                    _content = d;
                    return WriteAllText(Serialize(d, PrettyPrint, MaxJsonLength));
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>从另一 JSON（字符串/对象）合并到当前文件并保存</summary>
        public bool MergeFrom(object overrideSource)
        {
            lock (mLockRead)
            {
                try
                {
                    object root = ReadObject() ?? new Dictionary<string, object>();
                    object ovr = overrideSource is string ? Deserialize((string)overrideSource, MaxJsonLength) : overrideSource;
                    object merged = Merge(root, ovr);
                    _content = merged;
                    return WriteAllText(Serialize(merged, PrettyPrint, MaxJsonLength));
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>在文件 JSON 上按路径取值</summary>
        public object GetByPath(string path) { lock (mLockRead) { return GetByPath(ReadObject(), path); } }

        /// <summary>在文件 JSON 上按路径设置并保存</summary>
        public bool SetByPath(string path, object value)
        {
            lock (mLockRead)
            {
                object root = ReadObject() ?? new Dictionary<string, object>();
                if (!SetByPath(root, path, value)) { Error = "path not settable"; return false; }
                _content = root;
                return WriteAllText(Serialize(root, PrettyPrint, MaxJsonLength));
            }
        }

        /// <summary>文件是否存在</summary>
        public bool Exists { get { return HFile.HFilePath.FileExists(FilePath); } }

        /// <summary>文件信息（不存在时各属性为默认值）</summary>
        public FileInfo GetFileInfo() { return new FileInfo(FilePath); }

        /// <summary>删除主文件及 .bak / .writing / .temp 所有备份残留</summary>
        public void DeleteFile()
        {
            lock (mLockRead)
            {
                TryDelete(FilePath);
                TryDelete(FilePath + BackupSuffix);
                TryDelete(FilePath + WritingSuffix);
                TryDelete(FilePath + LegacyTempSuffix);
            }
        }

        #endregion
    }
}
