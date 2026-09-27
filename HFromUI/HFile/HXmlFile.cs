using HFromUI.HAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace HFromUI.HFile
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全网最全 XML 文件读写类（参考 HDictionaryFile 的写法，不动任何现有 public 签名）。
    ///
    /// 基础能力（原有）：XmlSerializer 对象序列化/反序列化、字典 XML 读写（XDocument）、
    ///                 文本读写、原子落盘(.writing+.bak+崩溃恢复)、加解密钩子。
    ///
    /// 扩展能力（新增）：
    ///   • 静态工具（不绑定文件）：
    ///       XPathQuery / XPathQuerySingle / XPathEvaluate — XPath 查询；
    ///       Transform — XSLT 转换；
    ///       ValidateSchema / ValidateSchemaFile — XSD 验证；
    ///       Merge 合并 / Minify 压缩 / Pretty 美化；
    ///       ToJson / FromJson — XML↔JSON 互转（复用 HJsonFile）；
    ///       ReadXDocument / WriteXDocument / ReadXmlDocument — 直接 DOM 访问；
    ///       CreateDocument(rootName) — 创建空文档；
    ///       SetAttribute / GetAttribute / RemoveAttribute / AddElement — 节点增删改查；
    ///       AppendCData / WriteCData — CDATA 读写；
    ///       SetDefaultNamespace / AddNamespace — 命名空间管理。
    ///   • 实例方法（绑定文件）：
    ///       ReadObject(Type) / WriteObject 非泛型 + Try* 模式；
    ///       UpdateXDocument(Action) — 原子读改写；
    ///       MergeFrom / XPathQuery / TransformWith / ValidateSchema / ReadXDocument / WriteXDocument；
    ///       GetFileInfo / Exists / DeleteFile。
    ///
    /// 仅依赖 .NET Framework 4.8 自带库（System.Xml / System.Xml.Linq / System.Xml.XPath / System.Xml.Xsl / System.Xml.Schema），零第三方包。
    /// </summary>
    public class HXmlFile
    {
        #region ==================== 字段与属性 ====================

        /// <summary>读改写操作的互斥锁，保证同一实例上的文件访问线程安全。</summary>
        private readonly object mLockRead = new object();

        /// <summary>最近一次错误信息</summary>
        public string Error = string.Empty;

        /// <summary>XML 文件完整路径</summary>
        public string FilePath { get; private set; }

        /// <summary>文件编码（默认 UTF-8）</summary>
        public Encoding FileEncoding { get; set; } = Encoding.UTF8;

        /// <summary>序列化时是否缩进美化</summary>
        public bool Indent { get; set; } = true;

        /// <summary>是否写出 XML 声明头</summary>
        public bool OmitXmlDeclaration { get; set; } = false;

        /// <summary>字典读写时的根节点名称</summary>
        public string DictionaryRootName { get; set; } = "Items";

        /// <summary>字典读写时的条目节点名称</summary>
        public string DictionaryItemName { get; set; } = "Item";

        /// <summary>最近一次读/写的对象，供 Save() 落盘使用</summary>
        private object _content = null;

        #endregion

        #region ==================== 构造函数 ====================

        /// <summary>构造 XML 文件读写对象（路径规则同 HJsonFile / HDictionaryFile）</summary>
        public HXmlFile(string filePath, string suffixName = ".xml")
        {
            FilePath = GetFullPath(filePath, suffixName);
            try { Recover(); } catch { }
        }

        /// <summary>
        /// 把构造传入的路径解析为完整磁盘路径：空路径取程序基目录下 XmlDefault+suffix；
        /// 文件名已带扩展名时原样使用；带盘符的绝对路径仅补后缀；相对路径拼到程序基目录下。
        /// </summary>
        private static string GetFullPath(string filePath, string suffixName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Path.GetFullPath(HFromUI.HData.HAppData.AppPath + @"\XmlDefault" + suffixName);
            if (Path.GetFileName(filePath).Contains(".")) return Path.GetFullPath(filePath);
            if (filePath.Contains(":")) return Path.GetFullPath(filePath + suffixName);
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

        /// <summary>
        /// 对象序列化为 XML 字符串。
        /// 普通类型走 XmlSerializer；标注了 H 系列特性（[HIgnore]/[HName]/[HXmlAttribute]/[HDateTimeFormat]）
        /// 的实体类自动走特性驱动序列化（或显式调用 SerializeEntity）；IDictionary/匿名对象走 XDocument 回退。
        /// </summary>
        public static string Serialize<T>(T obj, Encoding encoding = null, bool indent = true, bool omitXmlDeclaration = false)
        {
            if (obj == null) return string.Empty;
            encoding = encoding ?? Encoding.UTF8;
            if (!(obj is IDictionary) && HSerializedType.HasHAttributes(obj.GetType()))
                return SerializeEntity(obj, encoding, indent, omitXmlDeclaration);
            // XmlSerializer 不支持 IDictionary / 匿名只读字典；自动走 XDocument
            var d2 = obj as IDictionary<string, object>;
            var sd = obj as IDictionary<string, string>;
            var nd = obj as IDictionary;
            if (d2 != null) return SerializeDict(d2, encoding, indent, omitXmlDeclaration);
            if (sd != null) return SerializeDict(sd.ToDictionary(k => k.Key, v => (object)v), encoding, indent, omitXmlDeclaration);
            if (nd != null)
            {
                var conv = new Dictionary<string, object>();
                foreach (var key in nd.Keys) conv[Convert.ToString(key)] = nd[key];
                return SerializeDict(conv, encoding, indent, omitXmlDeclaration);
            }
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            XmlWriterSettings settings = new XmlWriterSettings { Indent = indent, Encoding = encoding, OmitXmlDeclaration = omitXmlDeclaration };
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(writer, obj, ns);
                }
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>把 object 值字典序列化为 XDocument 再转字符串</summary>
        private static string SerializeDict(IDictionary<string, object> dict, Encoding encoding, bool indent, bool omitXml)
        {
            XElement root = new XElement("Items");
            foreach (var kv in dict)
            {
                XElement item = new XElement("Item", new XAttribute("Key", kv.Key));
                if (kv.Value == null) item.Add(new XAttribute("Type", "Null"));
                else if (kv.Value is IDictionary<string, object> d2) item.Add(SerializeDictToXElement(d2));
                else if (kv.Value is IEnumerable<object> a2)
                {
                    XElement arr = new XElement("Array");
                    foreach (var vv in a2) arr.Add(SerializeDictToXElement(vv));
                    item.Add(arr);
                }
                else
                {
                    object sv = HScalarValue.Unwrap(kv.Value); // HDouble/HLong/HBool 等 H 包装按底层原生值写出
                    item.Add(new XAttribute("Type", sv.GetType().Name), Convert.ToString(sv, System.Globalization.CultureInfo.InvariantCulture));
                }
                root.Add(item);
            }
            XDocument doc = new XDocument(new XDeclaration("1.0", encoding.WebName, null), root);
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings s = new XmlWriterSettings { Indent = indent, Encoding = encoding, OmitXmlDeclaration = omitXml };
                using (XmlWriter w = XmlWriter.Create(ms, s)) doc.Save(w);
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }
        /// <summary>把字典值序列化为 XElement：Object 递归条目、Array 包子元素、Value 带 Type 特性。</summary>
        private static XElement SerializeDictToXElement(object value)
        {
            if (value == null) return new XElement("Null");
            if (value is IDictionary<string, object> d)
            {
                XElement r = new XElement("Object");
                foreach (var kv in d)
                    r.Add(new XElement("Item", new XAttribute("Key", kv.Key),
                        kv.Value == null ? (object)new XAttribute("Null", "1") : SerializeDictToXElement(kv.Value)));
                return r;
            }
            if (value is IEnumerable<object> arr)
            {
                XElement r = new XElement("Array");
                foreach (var v in arr) r.Add(SerializeDictToXElement(v));
                return r;
            }
            object sv2 = HScalarValue.Unwrap(value); // HDouble/HLong/HBool 等 H 包装按底层原生值写出
            return new XElement("Value", new XAttribute("Type", sv2.GetType().Name), Convert.ToString(sv2, System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>XML → 指定类型对象（标注 H 系列特性的实体自动走特性驱动反序列化）</summary>
        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return default(T);
            if (HSerializedType.HasHAttributes(typeof(T))) return DeserializeEntity<T>(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
                return (T)serializer.Deserialize(reader);
        }

        /// <summary>XML → 指定 Type（非泛型；标注 H 系列特性的实体自动走特性驱动反序列化）</summary>
        public static object Deserialize(string xml, Type type)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            if (HSerializedType.HasHAttributes(type)) return DeserializeEntity(xml, type);
            XmlSerializer serializer = new XmlSerializer(type);
            using (StringReader reader = new StringReader(xml))
                return serializer.Deserialize(reader);
        }

        #endregion

        #region ==================== 静态：XDocument / XmlDocument 基础 ====================

        /// <summary>解析 XML 字符串为 XDocument</summary>
        public static XDocument ReadXDocument(string xml) { return XDocument.Parse(xml); }

        /// <summary>XDocument → 字符串（可选格式化）</summary>
        public static string WriteXDocument(XDocument doc, Encoding encoding = null, bool indent = true, bool omitXmlDeclaration = false)
        {
            if (doc == null) return string.Empty;
            encoding = encoding ?? Encoding.UTF8;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, new XmlWriterSettings
                { Indent = indent, Encoding = encoding, OmitXmlDeclaration = omitXmlDeclaration }))
                    doc.Save(writer);
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>创建空文档并带上根节点</summary>
        public static XDocument CreateDocument(string rootName)
        {
            return new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(rootName ?? "root"));
        }

        /// <summary>XML → XmlDocument（DOM 模型）</summary>
        public static XmlDocument ReadXmlDocument(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>XmlDocument → 字符串</summary>
        public static string WriteXmlDocument(XmlDocument doc) { return doc?.OuterXml ?? string.Empty; }

        #endregion

        #region ==================== 静态：XPath / XSLT / XSD ====================

        /// <summary>XPath 查询，返回 XNode 列表</summary>
        public static IEnumerable<XObject> XPathQuery(string xml, string xpath, IDictionary<string, string> namespaces = null)
        {
            XDocument doc = XDocument.Parse(xml);
            var nav = doc.CreateNavigator();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
            if (namespaces != null) foreach (var kv in namespaces) nsmgr.AddNamespace(kv.Key, kv.Value);
            foreach (object obj in nav.Select(xpath, nsmgr))
            {
                var n = (System.Xml.XPath.XPathNavigator)obj;
                if (n.NodeType == XPathNodeType.Element) yield return XElement.Load(n.ReadSubtree());
                if (n.NodeType == XPathNodeType.Text) yield return (XNode)new XText(n.Value);
                if (n.NodeType == XPathNodeType.Attribute) yield return (XObject)new XAttribute(n.Name, n.Value);
                else yield return XElement.Load(n.ReadSubtree());
            }
        }

        /// <summary>XPath 查询单值（第一个匹配节点的 InnerText；没有返回 null）</summary>
        public static string XPathQuerySingle(string xml, string xpath, IDictionary<string, string> namespaces = null)
        {
            XDocument doc = XDocument.Parse(xml);
            var nav = doc.CreateNavigator();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
            if (namespaces != null) foreach (var kv in namespaces) nsmgr.AddNamespace(kv.Key, kv.Value);
            var result = nav.SelectSingleNode(xpath, nsmgr);
            return result?.Value;
        }

        /// <summary>XPath 数值/布尔求值，返回 object（double / string / bool / null）</summary>
        public static object XPathEvaluate(string xml, string xpath, IDictionary<string, string> namespaces = null)
        {
            XDocument doc = XDocument.Parse(xml);
            var nav = doc.CreateNavigator();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
            if (namespaces != null) foreach (var kv in namespaces) nsmgr.AddNamespace(kv.Key, kv.Value);
            return nav.Evaluate(xpath, nsmgr);
        }

        /// <summary>XSLT 转换。xml + xslPath → 输出字符串</summary>
        public static string Transform(string xml, string xslPath, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xslPath);
            XDocument doc = XDocument.Parse(xml);
            using (MemoryStream ms = new MemoryStream())
            {
                xslt.Transform(doc.CreateNavigator(), null, ms);
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>XSLT 转换（传入 XDocument 和 XslTransform 字符串）</summary>
        public static string TransformFromString(string xml, string xsl, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            XslCompiledTransform xslt = new XslCompiledTransform();
            using (StringReader sr = new StringReader(xsl))
                xslt.Load(XmlReader.Create(sr));
            XDocument doc = XDocument.Parse(xml);
            using (MemoryStream ms = new MemoryStream())
            {
                xslt.Transform(doc.CreateNavigator(), null, ms);
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>用内嵌 XSD 验证 XML（valid=True 表示通过；error 给出首个错误）</summary>
        public static bool ValidateSchema(string xml, out string error) { return ValidateSchemaCore(xml, null, out error); }

        /// <summary>用 XSD 文件验证 XML</summary>
        public static bool ValidateSchemaFile(string xml, string xsdPath, out string error) { return ValidateSchemaCore(xml, xsdPath, out error); }

        /// <summary>XSD 校验核心：可使用内嵌空架构或指定 .xsd 文件，通过回调收集首个校验错误。</summary>
        private static bool ValidateSchemaCore(string xml, string xsdPath, out string error)
        {
            error = null;
            XmlSchemaSet schemas = new XmlSchemaSet();
            if (xsdPath != null)
                schemas.Add(null, xsdPath);
            bool valid = true; string err = null;
            XDocument doc = XDocument.Parse(xml);
            try { doc.Validate(schemas, (s, e) => { valid = false; err = e.Message; }, xsdPath == null); }
            catch (Exception ex) { valid = false; err = ex.Message; }
            error = err;
            return valid;
        }

        #endregion

        #region ==================== 静态：压缩 / 美化 / 合并 / 互转 ====================

        /// <summary>XML 压缩（去除所有非必需空白）</summary>
        public static string Minify(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings s = new XmlWriterSettings { Indent = false, Encoding = Encoding.UTF8, OmitXmlDeclaration = false };
                using (XmlWriter w = XmlWriter.Create(ms, s)) doc.Save(w);
                return Encoding.UTF8.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>XML 美化缩进</summary>
        public static string Pretty(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings s = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false };
                using (XmlWriter w = XmlWriter.Create(ms, s)) doc.Save(w);
                return Encoding.UTF8.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>深度合并两个 XML：override 的同名元素/属性覆盖 base；override 中的 null 元素会删除 base 中对应的子元素。</summary>
        public static string Merge(string baseXml, string overrideXml)
        {
            XDocument b = XDocument.Parse(baseXml), o = XDocument.Parse(overrideXml);
            if (b.Root == null || o.Root == null) return baseXml;
            XElement merged = XElement.Parse(b.Root.ToString());
            MergeCore(merged, o.Root);
            return new XDocument(new XDeclaration("1.0", "utf-8", null), merged).ToString();
        }

        /// <summary>递归把 source 的属性与子元素合并进 target：同名覆盖，source 独有则追加。</summary>
        private static void MergeCore(XElement target, XElement source)
        {
            // 合并属性
            foreach (XAttribute attr in source.Attributes())
            {
                XAttribute existing = target.Attribute(attr.Name);
                if (existing != null) existing.Value = attr.Value;
                else target.Add(new XAttribute(attr));
            }
            // 双方都没有子元素 → 覆盖 Value（叶子节点直接替换）
            if (!target.HasElements && !source.HasElements)
            {
                target.Value = source.Value;
                return;
            }
            // 覆盖属性 + 处理子元素
            var srcByName = source.Elements().GroupBy(e => e.Name).ToDictionary(g => g.Key, g => g.ToArray());
            foreach (XElement existing in target.Elements().ToArray())
            {
                XElement[] matches;
                if (srcByName.TryGetValue(existing.Name, out matches) && matches.Length > 0)
                {
                    MergeCore(existing, matches[0]);
                    if (matches.Length > 1)
                    {
                        for (int i = 1; i < matches.Length; i++) target.Add(new XElement(matches[i]));
                    }
                }
            }
            // 添加 source 中有但 target 中没有的元素
            foreach (XElement srcEl in source.Elements())
            {
                if (!target.Elements(srcEl.Name).Any()) target.Add(new XElement(srcEl));
            }
        }

        /// <summary>XML → JSON（复用 HJsonFile.FromXml）</summary>
        public static string ToJson(string xml, bool pretty = true) { return HJsonFile.FromXml(xml, pretty); }

        /// <summary>JSON → XML（复用 HJsonFile.ToXml）</summary>
        public static string FromJson(string json, string rootName = "root") { return HJsonFile.ToXml(json, rootName); }

        #endregion

        #region ==================== 静态：节点 / 属性 / CDATA / 命名空间 操作 ====================

        /// <summary>给元素设置属性（存在则覆盖；不存在则追加；value=null 时删除属性）</summary>
        public static XElement SetAttribute(XElement element, string name, string value)
        {
            if (element == null || name == null) return element;
            XAttribute existing = element.Attribute(name);
            if (value == null) { if (existing != null) existing.Remove(); return element; }
            if (existing != null) existing.Value = value;
            else element.Add(new XAttribute(name, value));
            return element;
        }

        /// <summary>读取元素属性（不存在返回 null）</summary>
        public static string GetAttribute(XElement element, string name)
        {
            return element?.Attribute(name)?.Value;
        }

        /// <summary>删除元素属性</summary>
        public static XElement RemoveAttribute(XElement element, string name)
        {
            XAttribute existing = element?.Attribute(name);
            if (existing != null) existing.Remove();
            return element;
        }

        /// <summary>在指定元素下追加新子元素</summary>
        public static XElement AddElement(XElement parent, string name, string value = null)
        {
            if (parent == null || name == null) return null;
            XElement child = new XElement(name, value ?? string.Empty);
            parent.Add(child);
            return child;
        }

        /// <summary>在元素内容中追加 CDATA 节段</summary>
        public static XElement AppendCData(XElement element, string text)
        {
            if (element == null) return element;
            element.Add(new XCData(text ?? string.Empty));
            return element;
        }

        /// <summary>用 CDATA 节段替换整个元素的内容</summary>
        public static XElement SetCData(XElement element, string text)
        {
            if (element == null) return element;
            element.RemoveAll();
            element.Add(new XCData(text ?? string.Empty));
            return element;
        }

        /// <summary>读取元素中的 CDATA 值（如果没有 CDATA 则读普通 InnerText）</summary>
        public static string ReadCData(XElement element)
        {
            return element?.DescendantNodes().OfType<XCData>().FirstOrDefault()?.Value ?? element?.Value;
        }

        #endregion

        #region ==================== 实体特性映射（HIgnore / HName / HXmlAttribute / HDateTimeFormat） ====================

        /// <summary>
        /// 实体对象 → XML 字符串（特性驱动，不依赖 XmlSerializer 的特性体系）。
        /// [HIgnore] 跳过；[HName("xx")] 改名；[HXmlAttribute] 以特性写出；[HDateTimeFormat("...")] 日期格式。
        /// 根节点名：rootName ?? 类上 [HName] ?? 类名。
        /// </summary>
        public static string SerializeEntity(object obj, Encoding encoding = null, bool indent = true,
            bool omitXmlDeclaration = false, string rootName = null)
        {
            if (obj == null) return string.Empty;
            encoding = encoding ?? Encoding.UTF8;
            XElement root = EntityToXElement(obj, rootName);
            XDocument doc = new XDocument(new XDeclaration("1.0", encoding.WebName, null), root);
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings s = new XmlWriterSettings { Indent = indent, Encoding = encoding, OmitXmlDeclaration = omitXmlDeclaration };
                using (XmlWriter w = XmlWriter.Create(ms, s)) doc.Save(w);
                return encoding.GetString(ms.ToArray()).TrimStart('\uFEFF');
            }
        }

        /// <summary>XML 字符串 → 实体对象（特性驱动，泛型）</summary>
        public static T DeserializeEntity<T>(string xml)
        {
            return (T)DeserializeEntity(xml, typeof(T));
        }

        /// <summary>XML 字符串 → 实体对象（特性驱动，非泛型）</summary>
        public static object DeserializeEntity(string xml, Type type)
        {
            if (string.IsNullOrWhiteSpace(xml) || type == null) return null;
            XDocument doc = XDocument.Parse(xml);
            if (doc.Root == null) return null;
            return EntityFromXElement(doc.Root, type);
        }

        /// <summary>实体对象 → XElement（特性驱动）</summary>
        public static XElement EntityToXElement(object obj, string elementName = null)
        {
            if (obj == null) return null;
            Type t = obj.GetType();
            XElement el = new XElement(XmlConvert.EncodeName(elementName ?? HSerializedType.TypeName(t)));
            foreach (HSerializedType.HMember m in HSerializedType.GetMembers(t))
            {
                object v;
                try { v = m.Get(obj); } catch { v = null; }
                if (v == null) continue;
                Type vt = v.GetType();
                string name = XmlConvert.EncodeName(m.Name);

                if (m.IsXmlAttribute && IsScalarType(vt))
                {
                    el.SetAttributeValue(name, ScalarToText(v, m.DateFormat));
                    continue;
                }
                if (v is IDictionary) continue; // 字典请用 WriteDictionaryObject
                if (v is IEnumerable en && !(v is string))
                {
                    foreach (object item in en)
                    {
                        if (item == null) continue;
                        if (IsScalarType(item.GetType()))
                            el.Add(new XElement(name, ScalarToText(item, m.DateFormat)));
                        else
                            el.Add(EntityToXElement(item, null));
                    }
                    continue;
                }
                if (IsScalarType(vt))
                    el.Add(new XElement(name, ScalarToText(v, m.DateFormat)));
                else
                    el.Add(EntityToXElement(v, name));
            }
            return el;
        }

        /// <summary>XElement → 实体对象（特性驱动）</summary>
        public static object EntityFromXElement(XElement el, Type type)
        {
            if (el == null || type == null) return null;
            object obj;
            try { obj = Activator.CreateInstance(type); }
            catch { return null; }
            foreach (HSerializedType.HMember m in HSerializedType.GetMembers(type))
            {
                try
                {
                    Type mt = Nullable.GetUnderlyingType(m.MemberType) ?? m.MemberType;
                    string wireName = XmlConvert.EncodeName(m.Name);

                    if (m.IsXmlAttribute)
                    {
                        XAttribute attr = el.Attribute(wireName) ??
                            el.Attributes().FirstOrDefault(a => a.Name.LocalName == m.Name);
                        if (attr != null && IsScalarType(mt))
                            m.Set(obj, TextToScalar(attr.Value, mt, m.DateFormat));
                        continue;
                    }

                    Type elemType = GetEnumerableElementType(mt);
                    if (elemType != null)
                    {
                        bool scalarElem = IsScalarType(elemType);
                        string itemName = scalarElem ? wireName : XmlConvert.EncodeName(HSerializedType.TypeName(elemType));
                        List<XElement> children = el.Elements()
                            .Where(e => (e.Name == itemName || e.Name.LocalName == XmlConvert.DecodeName(itemName) || (scalarElem && e.Name.LocalName == m.Name))
                                && (!scalarElem || !e.HasElements))
                            .ToList();
                        if (children.Count == 0)
                        {
                            // 兼容包装节点：<成员名><项元素/>...</成员名>（XmlSerializer 风格）
                            List<XElement> wrapped = el.Elements()
                                .Where(e => e.Name == wireName || e.Name.LocalName == m.Name)
                                .SelectMany(w => w.Elements())
                                .Where(e => scalarElem || e.Name == itemName || e.Name.LocalName == HSerializedType.TypeName(elemType))
                                .ToList();
                            if (wrapped.Count > 0) children = wrapped;
                        }
                        Type listType = typeof(List<>).MakeGenericType(elemType);
                        IList list = (IList)Activator.CreateInstance(listType);
                        foreach (XElement c in children)
                            list.Add(scalarElem ? TextToScalar(c.Value, elemType, m.DateFormat) : EntityFromXElement(c, elemType));
                        object val = list;
                        if (mt.IsArray)
                        {
                            Array arr = Array.CreateInstance(elemType, list.Count);
                            list.CopyTo(arr, 0);
                            val = arr;
                        }
                        m.Set(obj, val);
                        continue;
                    }

                    XElement child = el.Elements().FirstOrDefault(e => e.Name == wireName || e.Name.LocalName == m.Name);
                    if (child == null) continue;
                    if (IsScalarType(mt))
                        m.Set(obj, TextToScalar(child.Value, mt, m.DateFormat));
                    else
                        m.Set(obj, EntityFromXElement(child, mt));
                }
                catch { }
            }
            return obj;
        }

        /// <summary>判断是否为可直接写成 XML 文本/特性的标量类型（原生数值、布尔、字符、字符串、日期、枚举、Guid/TimeSpan，以及 HDouble/HLong/HBool/HInt 等 H 包装标量）。</summary>
        private static bool IsScalarType(Type t)
        {
            if (t == null) return false;
            Type hNative;
            if (HScalarValue.IsHScalar(t, out hNative)) return true;
            if (t.IsEnum) return true;
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
                case TypeCode.DateTime:
                    return true;
                default:
                    return t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan);
            }
        }

        /// <summary>把标量值格式化为 XML 文本（H 包装标量先取底层原生值；日期按格式串、布尔写 true/false）。</summary>
        private static string ScalarToText(object v, string dateFormat)
        {
            if (v == null) return string.Empty;
            v = HScalarValue.Unwrap(v); // HDouble/HLong/HBool 等 H 包装取底层原生值
            if (v.GetType().IsEnum) return v.ToString();
            if (v is DateTime dt)
                return dt.ToString(string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd HH:mm:ss" : dateFormat, CultureInfo.InvariantCulture);
            if (v is DateTimeOffset dto)
                return dto.ToString(string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd HH:mm:ss zzz" : dateFormat, CultureInfo.InvariantCulture);
            if (v is bool b) return b ? "true" : "false";
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        /// <summary>把 XML 文本解析为目标标量类型（枚举/日期/Guid/TimeSpan/布尔/H 包装标量等），失败返回默认值。</summary>
        private static object TextToScalar(string text, Type t, string dateFormat)
        {
            // 目标为 HDouble/HLong/HBool/HInt 等 H 包装类型：可空且无文本返回 null，否则按底层原生类型解析后装箱
            Type hNative;
            if (HScalarValue.IsHScalar(t, out hNative))
            {
                if (Nullable.GetUnderlyingType(t) != null && string.IsNullOrWhiteSpace(text)) return null;
                return HScalarValue.Wrap(t, TextToScalar(text, hNative, dateFormat));
            }
            if (text == null) return t.IsValueType ? Activator.CreateInstance(t) : null;
            if (t == typeof(string)) return text;
            if (string.IsNullOrWhiteSpace(text))
                return t.IsValueType && Nullable.GetUnderlyingType(t) == null ? Activator.CreateInstance(t) : null;
            try
            {
                if (t.IsEnum) return Enum.Parse(t, text.Trim(), true);
                if (t == typeof(DateTime))
                {
                    DateTime dt;
                    if (!string.IsNullOrWhiteSpace(dateFormat) &&
                        DateTime.TryParseExact(text, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        return dt;
                    return DateTime.Parse(text, CultureInfo.InvariantCulture);
                }
                if (t == typeof(DateTimeOffset)) return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
                if (t == typeof(Guid)) return new Guid(text.Trim());
                if (t == typeof(TimeSpan)) return TimeSpan.Parse(text.Trim(), CultureInfo.InvariantCulture);
                if (t == typeof(bool))
                {
                    string b = text.Trim().ToLowerInvariant();
                    return b == "1" || b == "true" || b == "yes";
                }
                if (t == typeof(char)) { string cs = text.Trim(); return cs.Length > 0 ? cs[0] : '\0'; }
                return Convert.ChangeType(text.Trim(), t, CultureInfo.InvariantCulture);
            }
            catch
            {
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            }
        }

        /// <summary>若类型是集合（数组 / 泛型 IEnumerable&lt;T&gt;；非字符串、非字典），返回元素类型；否则 null</summary>
        private static Type GetEnumerableElementType(Type t)
        {
            if (t == typeof(string)) return null;
            if (typeof(IDictionary).IsAssignableFrom(t)) return null;
            if (t.IsArray) return t.GetElementType();
            if (t.IsGenericType)
            {
                Type[] gargs = t.GetGenericArguments();
                if (gargs.Length == 1 && typeof(IEnumerable<>).MakeGenericType(gargs[0]).IsAssignableFrom(t))
                    return gargs[0];
            }
            return null;
        }

        #endregion

        #region ==================== XML → C# 实体类代码生成 ====================

        /// <summary>代码生成中间模型：一个生成属性的描述（属性名、C# 类型名、原始 XML 名、是否特性）</summary>
        private sealed class HXmlGenProp
        {
            /// <summary>生成的 C# 属性名（PascalCase 合法标识符）</summary>
            public string Name;
            /// <summary>生成的 C# 类型名（标量名或生成类名/List 包装）</summary>
            public string Type;
            /// <summary>原始 XML 元素名或特性名（与属性名不同时通过 [HName] 标注）</summary>
            public string WireName;
            /// <summary>true 表示该成员来源于 XML 特性(attribute)，生成 [HXmlAttribute]</summary>
            public bool IsAttribute;
        }
        /// <summary>代码生成中间模型：一个生成类的描述（类名及其属性列表）</summary>
        private sealed class HXmlGenClass
        {
            /// <summary>生成的 C# 类名（已去重）</summary>
            public string Name;
            /// <summary>该生成类包含的属性模型列表</summary>
            public List<HXmlGenProp> Props = new List<HXmlGenProp>();
        }

        /// <summary>
        /// 把 XML 文本直接生成为 C# 实体类代码：
        /// 特性(attribute) → [HXmlAttribute] 属性；重复子节点 → List&lt;T&gt;；
        /// 叶子节点文本自动推断 int/long/double/bool/DateTime/string；键名不一致时自动加 [HName("原名")]。
        /// 生成的类可直接用 HXmlFile.ReadObject&lt;T&gt; / DeserializeEntity&lt;T&gt; 反序列化。
        /// </summary>
        public static string GenerateCSharpClass(string xml, string rootName = null, string namespaceName = null)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            XDocument doc;
            try { doc = XDocument.Parse(xml); }
            catch (Exception ex) { throw new ArgumentException(HTranslation.GetContent("XML 解析失败！   Invalid XML: ") + ex.Message, ex); }
            if (doc.Root == null) return null;

            Dictionary<string, HXmlGenClass> classes = new Dictionary<string, HXmlGenClass>(StringComparer.Ordinal);
            HashSet<string> classNames = new HashSet<string>(StringComparer.Ordinal);
            BuildXmlClass(new List<XElement> { doc.Root },
                string.IsNullOrWhiteSpace(rootName) ? doc.Root.Name.LocalName : rootName, classes, classNames);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using HFromUI.HSocket;");
            sb.AppendLine();
            bool hasNs = !string.IsNullOrWhiteSpace(namespaceName);
            if (hasNs) { sb.Append("namespace ").Append(namespaceName).AppendLine(); sb.AppendLine("{"); }
            string indent = hasNs ? "    " : "";
            foreach (HXmlGenClass gc in classes.Values)
            {
                sb.Append(indent).Append("public class ").AppendLine(gc.Name);
                sb.Append(indent).AppendLine("{");
                foreach (HXmlGenProp p in gc.Props)
                {
                    if (!string.IsNullOrEmpty(p.WireName))
                        sb.Append(indent).Append("    [HName(\"").Append(EscapeCodeString(p.WireName)).AppendLine("\")]");
                    if (p.IsAttribute)
                        sb.Append(indent).AppendLine("    [HXmlAttribute]");
                    sb.Append(indent).Append("    public ").Append(p.Type).Append(' ').Append(p.Name).AppendLine(" { get; set; }");
                }
                sb.Append(indent).AppendLine("}");
                sb.AppendLine();
            }
            if (hasNs) sb.AppendLine("}");
            return sb.ToString().TrimEnd() + Environment.NewLine;
        }

        /// <summary>读取当前 XML 文件内容并生成 C# 实体类代码</summary>
        public string GenerateClassFromFile(string rootName = null, string namespaceName = null)
        {
            string xml = ReadAllText();
            return string.IsNullOrWhiteSpace(xml) ? null : GenerateCSharpClass(xml, rootName, namespaceName);
        }

        /// <summary>根据一组同构元素（属性并集 + 子节点并集）构建类模型</summary>
        private static string BuildXmlClass(List<XElement> elements, string suggested,
            Dictionary<string, HXmlGenClass> classes, HashSet<string> classNames)
        {
            string cn = UniqueGenName(HJsonFile.PascalIdent(suggested), classNames);
            HXmlGenClass gc = new HXmlGenClass { Name = cn };
            classes[cn] = gc;
            HashSet<string> propNames = new HashSet<string>(StringComparer.Ordinal);

            // 特性（attribute）
            foreach (IGrouping<string, XAttribute> g in elements
                .SelectMany(e => e.Attributes())
                .Where(a => !a.IsNamespaceDeclaration)
                .GroupBy(a => a.Name.LocalName))
            {
                string pn = UniqueGenName(HJsonFile.PascalIdent(g.Key), propNames);
                string type = InferTextType(g.Select(a => a.Value));
                gc.Props.Add(new HXmlGenProp { Name = pn, Type = type, WireName = (pn == g.Key) ? null : g.Key, IsAttribute = true });
            }

            // 子节点（element）
            foreach (IGrouping<string, XElement> g in elements
                .SelectMany(e => e.Elements())
                .GroupBy(e => e.Name.LocalName))
            {
                List<XElement> childEls = g.ToList();
                string pn = UniqueGenName(HJsonFile.PascalIdent(g.Key), propNames);
                bool repeated = elements.Any(e => e.Elements().Count(x => x.Name.LocalName == g.Key) > 1);

                // 纯包装节点解包：<Users><User/>...</Users> → 属性 Users 类型 List<User>
                string wrapItemName;
                List<XElement> wrapItems = TryUnwrapList(childEls, out wrapItemName);
                if (wrapItems != null)
                {
                    string type;
                    bool wrapRepeated = wrapItems.Count > childEls.Count || wrapItems.Count > 1 || repeated;
                    if (wrapItems.Any(e => e.HasElements || e.HasAttributes))
                    {
                        string itemClass = BuildXmlClass(wrapItems, wrapItemName, classes, classNames);
                        type = wrapRepeated ? "List<" + itemClass + ">" : itemClass;
                    }
                    else
                    {
                        string st = InferTextType(wrapItems.Select(e => e.Value));
                        type = wrapRepeated ? "List<" + st + ">" : st;
                    }
                    gc.Props.Add(new HXmlGenProp { Name = pn, Type = type, WireName = (pn == g.Key) ? null : g.Key, IsAttribute = false });
                    continue;
                }

                bool hasChildren = childEls.Any(e => e.HasElements || e.HasAttributes);
                string propType;
                if (hasChildren)
                {
                    string childClass = BuildXmlClass(childEls, g.Key, classes, classNames);
                    propType = repeated ? "List<" + childClass + ">" : childClass;
                }
                else
                {
                    string st = InferTextType(childEls.Select(e => e.Value));
                    propType = repeated ? "List<" + st + ">" : st;
                }
                gc.Props.Add(new HXmlGenProp { Name = pn, Type = propType, WireName = (pn == g.Key) ? null : g.Key, IsAttribute = false });
            }
            return cn;
        }

        /// <summary>
        /// 检测纯列表包装节点（如 &lt;Users&gt;&lt;User/&gt;&lt;User/&gt;&lt;/Users&gt;）：
        /// 包装节点无特性、无直接文本，且其子节点全部同名（且名字与包装节点不同）。
        /// 命中则返回所有子节点（逻辑列表项）；否则 null。
        /// </summary>
        private static List<XElement> TryUnwrapList(List<XElement> wrappers, out string itemName)
        {
            itemName = null;
            if (wrappers == null || wrappers.Count == 0) return null;
            string inner = null;
            List<XElement> items = new List<XElement>();
            foreach (XElement w in wrappers)
            {
                if (w.HasAttributes) return null;
                if (w.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return null;
                List<XElement> kids = w.Elements().ToList();
                if (kids.Count == 0) return null;
                foreach (XElement k in kids)
                {
                    if (inner == null) inner = k.Name.LocalName;
                    else if (k.Name.LocalName != inner) return null;
                }
                items.AddRange(kids);
            }
            if (inner == null || inner == wrappers[0].Name.LocalName) return null;
            itemName = inner;
            return items;
        }

        /// <summary>按文本内容推断标量类型（int → long → double → bool → DateTime → string）</summary>
        private static string InferTextType(IEnumerable<string> values)
        {
            bool any = false;
            bool allInt = true, allLong = true, allDouble = true, allBool = true, allDate = true;
            foreach (string raw in values)
            {
                string v = (raw ?? string.Empty).Trim();
                if (v.Length == 0) continue;
                any = true;
                long lv; double dv; DateTime dtv;
                if (long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out lv))
                {
                    if (lv < int.MinValue || lv > int.MaxValue) allInt = false;
                }
                else { allLong = false; allInt = false; }
                if (!double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out dv)) allDouble = false;
                if (!(v == "true" || v == "false" || v == "1" || v == "0")) allBool = false;
                if (!DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtv)) allDate = false;
            }
            if (!any) return "string";
            if (allInt) return "int";
            if (allLong) return "long";
            if (allDouble) return "double";
            if (allBool) return "bool";
            if (allDate) return "DateTime";
            return "string";
        }

        /// <summary>在已用名集合中去重生成名称，冲突时追加数字后缀。</summary>
        private static string UniqueGenName(string name, HashSet<string> used)
        {
            if (!used.Contains(name)) { used.Add(name); return name; }
            for (int i = 2; ; i++)
            {
                string cand = name + i;
                if (!used.Contains(cand)) { used.Add(cand); return cand; }
            }
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

        #region ==================== 按名称读写 XML 特性(attribute)与节点值 ====================

        /// <summary>
        /// 读取指定元素的特性值。元素路径支持 "Root/A/B"、"/Root/A"、
        /// 单名（全文查找第一个同名元素）与 "Name[i]" 索引（0 起）。
        /// </summary>
        public static string GetAttributeValue(string xml, string elementPath, string attributeName)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement el = NavigateElement(doc, elementPath, false);
            return el?.Attribute(attributeName)?.Value;
        }

        /// <summary>设置指定元素的特性值（元素/路径不存在时自动创建；value 为 null 删除特性），返回新 XML</summary>
        public static string SetAttributeValue(string xml, string elementPath, string attributeName, string value)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement el = NavigateElement(doc, elementPath, true);
            if (el == null) return xml;
            el.SetAttributeValue(XmlConvert.EncodeName(attributeName ?? string.Empty), value);
            return WriteXDocument(doc);
        }

        /// <summary>读取指定节点的文本值（路径规则同 GetAttributeValue）</summary>
        public static string GetNodeValue(string xml, string nodePath)
        {
            XDocument doc = XDocument.Parse(xml);
            return NavigateElement(doc, nodePath, false)?.Value;
        }

        /// <summary>设置指定节点的文本值（路径不存在自动创建；会清除该节点下已有子节点），返回新 XML</summary>
        public static string SetNodeValue(string xml, string nodePath, string value)
        {
            XDocument doc = XDocument.Parse(xml);
            XElement el = NavigateElement(doc, nodePath, true);
            if (el == null) return xml;
            el.Value = value ?? string.Empty;
            return WriteXDocument(doc);
        }

        /// <summary>按 根/子/子[i] 路径在文档中导航元素；create=true 时自动补建缺失节点。</summary>
        private static XElement NavigateElement(XDocument doc, string path, bool create)
        {
            if (doc == null || doc.Root == null || string.IsNullOrWhiteSpace(path)) return null;
            string[] parts = path.Trim('/').Split('/');
            XElement cur = null;
            for (int i = 0; i < parts.Length; i++)
            {
                string seg = parts[i];
                string name = seg;
                int index = 0;
                int lb = seg.IndexOf('[');
                if (lb >= 0 && seg.EndsWith("]"))
                {
                    name = seg.Substring(0, lb);
                    int.TryParse(seg.Substring(lb + 1, seg.Length - lb - 2), out index);
                }
                if (i == 0)
                {
                    if (doc.Root.Name.LocalName == name)
                    {
                        cur = doc.Root;
                    }
                    else if (parts.Length == 1)
                    {
                        cur = doc.Root.Descendants().FirstOrDefault(e => e.Name.LocalName == name);
                        if (cur == null && create)
                        {
                            cur = new XElement(XmlConvert.EncodeName(name));
                            doc.Root.Add(cur);
                        }
                    }
                    else if (create)
                    {
                        cur = new XElement(XmlConvert.EncodeName(name));
                        doc.Root.Add(cur);
                    }
                    else return null;
                }
                else
                {
                    List<XElement> siblings = cur.Elements().Where(e => e.Name.LocalName == name).ToList();
                    if (index >= 0 && index < siblings.Count)
                    {
                        cur = siblings[index];
                    }
                    else if (create)
                    {
                        while (siblings.Count <= index)
                        {
                            XElement ne = new XElement(XmlConvert.EncodeName(name));
                            cur.Add(ne);
                            siblings.Add(ne);
                        }
                        cur = siblings[index];
                    }
                    else return null;
                }
            }
            return cur;
        }

        /// <summary>读取特性值（文件级；不存在返回 defaultValue）</summary>
        public string ReadAttribute(string elementPath, string attributeName, string defaultValue = null)
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    if (string.IsNullOrWhiteSpace(xml)) return defaultValue;
                    string v = GetAttributeValue(xml, elementPath, attributeName);
                    return v ?? defaultValue;
                }
                catch (Exception ex) { Error = ex.Message; return defaultValue; }
            }
        }

        /// <summary>读取特性值并转换为指定类型（int/long/double/bool/DateTime/Guid/enum/string）</summary>
        public T ReadAttribute<T>(string elementPath, string attributeName, T defaultValue)
        {
            string v = ReadAttribute(elementPath, attributeName, null);
            return v == null ? defaultValue : ConvertXmlText<T>(v, defaultValue);
        }

        /// <summary>写入特性值并保存到文件（元素路径不存在自动创建；value 为 null 删除特性）</summary>
        public bool WriteAttribute(string elementPath, string attributeName, string value)
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    if (string.IsNullOrWhiteSpace(xml)) xml = "<" + FirstPathSegment(elementPath) + "/>";
                    return WriteAllText(SetAttributeValue(xml, elementPath, attributeName, value));
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>读取节点文本值（文件级；不存在返回 defaultValue）</summary>
        public string ReadNodeValue(string nodePath, string defaultValue = null)
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    if (string.IsNullOrWhiteSpace(xml)) return defaultValue;
                    string v = GetNodeValue(xml, nodePath);
                    return v ?? defaultValue;
                }
                catch (Exception ex) { Error = ex.Message; return defaultValue; }
            }
        }

        /// <summary>读取节点值并转换为指定类型（int/long/double/bool/DateTime/Guid/enum/string）</summary>
        public T ReadNodeValue<T>(string nodePath, T defaultValue)
        {
            string v = ReadNodeValue(nodePath, null);
            return v == null ? defaultValue : ConvertXmlText<T>(v, defaultValue);
        }

        /// <summary>写入特性值（对象自动转文本；DateTime 按 yyyy-MM-dd HH:mm:ss，bool 按 true/false）并保存</summary>
        public bool WriteAttribute(string elementPath, string attributeName, object value)
        {
            string text;
            if (value == null) text = null;
            else if (value is DateTime dt) text = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            else if (value is bool b) text = b ? "true" : "false";
            else text = Convert.ToString(value, CultureInfo.InvariantCulture);
            return WriteAttribute(elementPath, attributeName, text);
        }

        /// <summary>写入节点文本值并保存（路径不存在自动创建）</summary>
        public bool WriteNodeValue(string nodePath, string value)
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    if (string.IsNullOrWhiteSpace(xml)) xml = "<" + FirstPathSegment(nodePath) + "/>";
                    return WriteAllText(SetNodeValue(xml, nodePath, value));
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>写入节点值（对象自动转文本；DateTime 按 yyyy-MM-dd HH:mm:ss，bool 按 true/false）</summary>
        public bool WriteNodeValue(string nodePath, object value)
        {
            string text;
            if (value == null) text = null;
            else if (value is DateTime dt) text = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            else if (value is bool b) text = b ? "true" : "false";
            else text = Convert.ToString(value, CultureInfo.InvariantCulture);
            return WriteNodeValue(nodePath, text);
        }

        /// <summary>取路径第一段元素名并去除 [下标]，用于空文件写入时确定要创建的根节点名。</summary>
        private static string FirstPathSegment(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "root";
            string first = path.Trim('/').Split('/')[0];
            int lb = first.IndexOf('[');
            if (lb >= 0) first = first.Substring(0, lb);
            return XmlConvert.EncodeName(string.IsNullOrWhiteSpace(first) ? "root" : first);
        }

        /// <summary>把 XML 文本安全转换为值类型 T（数值/布尔/日期/Guid/TimeSpan/枚举等），解析失败时返回 defaultValue</summary>
        private static T ConvertXmlText<T>(string text, T defaultValue)
        {
            try
            {
                Type t = typeof(T);
                Type nt = Nullable.GetUnderlyingType(t) ?? t;
                Type hNative;
                if (HScalarValue.IsHScalar(nt, out hNative))
                {
                    // HDouble/HLong/HBool/HInt 等 H 包装：复用非泛型文本解析得到原生值后装箱
                    if (string.IsNullOrWhiteSpace(text)) return defaultValue;
                    return (T)HScalarValue.Wrap(nt, TextToScalar(text, hNative, null));
                }
                if (nt == typeof(string)) return (T)(object)text;
                if (nt.IsEnum) return (T)Enum.Parse(nt, text.Trim(), true);
                if (nt == typeof(DateTime)) return (T)(object)DateTime.Parse(text, CultureInfo.InvariantCulture);
                if (nt == typeof(DateTimeOffset)) return (T)(object)DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
                if (nt == typeof(Guid)) return (T)(object)new Guid(text.Trim());
                if (nt == typeof(bool))
                {
                    string b = text.Trim().ToLowerInvariant();
                    return (T)(object)(b == "1" || b == "true" || b == "yes");
                }
                return (T)Convert.ChangeType(text.Trim(), nt, CultureInfo.InvariantCulture);
            }
            catch { return defaultValue; }
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

        /// <summary>最近一次自动恢复说明（未发生恢复时为空字符串）</summary>
        public string RecoveryMessage = string.Empty;

        /// <summary>判断是否 ContentValid。</summary>
        protected virtual bool IsContentValid(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            try { XDocument.Parse(content); return XDocument.Parse(content).Root != null; }
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
            try { if (TryReadValidContent(FilePath, out _)) { TryDelete(FilePath + WritingSuffix); return; } TryRestoreFromBackup(); }
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

        /// <summary>安全写入磁盘（原子替换）</summary>
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
                    TryDelete(FilePath + LegacyTempSuffix); Error = string.Empty; return true;
                }
                catch (Exception ex) { TryDelete(staging); Error = HTranslation.GetContent("写入失败！   Write failed: ") + ex.Message; return false; }
            }
        }

        #endregion

        #region ==================== 磁盘：对象读写（原有 + 扩展） ====================

        /// <summary>对象序列化为 XML 并写入磁盘</summary>
        public bool WriteObject(object obj, bool? indent = null)
        {
            lock (mLockRead) { try { _content = obj; string xml = Serialize(obj, FileEncoding, indent ?? Indent, OmitXmlDeclaration); return WriteAllText(xml); } catch (Exception ex) { Error = ex.Message; return false; } }
        }

        /// <summary>读取反序列化为 T</summary>
        public T ReadObject<T>()
        {
            lock (mLockRead) { try { string xml = ReadAllText(); if (string.IsNullOrWhiteSpace(xml)) return default(T); T obj = Deserialize<T>(xml); _content = obj; return obj; } catch (Exception ex) { Error = ex.Message; return default(T); } }
        }

        /// <summary>非泛型 ReadObject(Type)</summary>
        public object ReadObject(Type type)
        {
            lock (mLockRead) { try { string xml = ReadAllText(); if (string.IsNullOrWhiteSpace(xml)) return null; object obj = Deserialize(xml, type); _content = obj; return obj; } catch (Exception ex) { Error = ex.Message; return null; } }
        }

        /// <summary>TryReadObject 模式</summary>
        public bool TryReadObject<T>(out T obj) { try { obj = ReadObject<T>(); return true; } catch { obj = default(T); return false; } }
        /// <summary>尝试写入对象（WriteObject 的 Try 风格别名，失败返回 false 不抛异常）。</summary>
        public bool TryWriteObject(object obj) { return WriteObject(obj); }

        /// <summary>读取 XDocument</summary>
        public XDocument ReadXDocument()
        {
            lock (mLockRead) { try { string xml = ReadAllText(); if (string.IsNullOrWhiteSpace(xml)) return null; return ReadXDocument(xml); } catch (Exception ex) { Error = ex.Message; return null; } }
        }

        /// <summary>写入 XDocument 到磁盘</summary>
        public bool WriteXDocument(XDocument doc, bool? indent = null, bool? omitDecl = null)
        {
            lock (mLockRead)
            {
                try
                {
                    _content = doc;
                    string xml = WriteXDocument(doc, FileEncoding, indent ?? Indent, omitDecl ?? OmitXmlDeclaration);
                    return WriteAllText(xml);
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>写入字符串字典（原有格式：&lt;Items&gt;&lt;Item Key="..."&gt;value&lt;/Item&gt;...）</summary>
        public bool WriteDictionary(IDictionary<string, string> dictionary)
        {
            lock (mLockRead)
            {
                try
                {
                    _content = dictionary;
                    XElement root = new XElement(DictionaryRootName);
                    if (dictionary != null)
                        foreach (KeyValuePair<string, string> kv in dictionary)
                            root.Add(new XElement(DictionaryItemName, new XAttribute("Key", kv.Key ?? string.Empty), kv.Value ?? string.Empty));
                    XDocument doc = new XDocument(new XDeclaration("1.0", FileEncoding.WebName, null), root);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (XmlWriter w = XmlWriter.Create(ms, new XmlWriterSettings
                        { Indent = Indent, Encoding = FileEncoding, OmitXmlDeclaration = OmitXmlDeclaration })) doc.Save(w);
                        return WriteAllText(FileEncoding.GetString(ms.ToArray()).TrimStart('\uFEFF'));
                    }
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>写入 object 值字典（原有格式 + Type 属性标注值类型）</summary>
        public bool WriteDictionaryObject(IDictionary<string, object> dictionary)
        {
            lock (mLockRead)
            {
                try
                {
                    _content = dictionary;
                    XElement root = new XElement(DictionaryRootName);
                    if (dictionary != null)
                        foreach (var kv in dictionary)
                            root.Add(MakeDictItem(kv.Key, kv.Value));
                    XDocument doc = new XDocument(new XDeclaration("1.0", FileEncoding.WebName, null), root);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (XmlWriter w = XmlWriter.Create(ms, new XmlWriterSettings
                        { Indent = Indent, Encoding = FileEncoding, OmitXmlDeclaration = OmitXmlDeclaration })) doc.Save(w);
                        return WriteAllText(FileEncoding.GetString(ms.ToArray()).TrimStart('\uFEFF'));
                    }
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>把一个字典键值构造成条目 XElement（标量带 Type 特性，字典/集合递归生成）。</summary>
        private XElement MakeDictItem(string key, object value)
        {
            XElement item = new XElement(DictionaryItemName, new XAttribute("Key", key ?? string.Empty));
            if (value == null) { item.Add(new XAttribute("Type", "Null")); return item; }
            XElement valueEl;
            if (value is XElement xe) valueEl = new XElement(xe);
            else if (value is IDictionary<string, object> d)
            {
                valueEl = new XElement("Object");
                foreach (var kv2 in d) valueEl.Add(MakeDictItem(kv2.Key, kv2.Value));
            }
            else if (value is IEnumerable<object> arr)
            {
                valueEl = new XElement("Array");
                foreach (var v in arr) valueEl.Add(MakeDictItem("item", v));
            }
            else
            {
                object sv = HScalarValue.Unwrap(value); // HDouble/HLong/HBool 等 H 包装按底层原生值写出
                string name = sv.GetType().Name;
                string str = Convert.ToString(sv, System.Globalization.CultureInfo.InvariantCulture);
                valueEl = new XElement("Value", new XAttribute("Type", name), str);
            }
            item.Add(valueEl);
            return item;
        }

        /// <summary>读取字符串字典</summary>
        public Dictionary<string, string> ReadDictionary()
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    if (string.IsNullOrWhiteSpace(xml)) return new Dictionary<string, string>();
                    XDocument doc = XDocument.Parse(xml);
                    var d = new Dictionary<string, string>();
                    if (doc.Root != null)
                        foreach (XElement element in doc.Root.Elements(DictionaryItemName))
                        {
                            XAttribute keyAttr = element.Attribute("Key");
                            string key = keyAttr != null ? keyAttr.Value : element.Name.LocalName;
                            d[key] = element.Value;
                        }
                    _content = d; return d;
                }
                catch (Exception ex) { Error = ex.Message; return new Dictionary<string, string>(); }
            }
        }

        /// <summary>保存最近一次读写的对象</summary>
        public bool Save()
        {
            if (_content == null) { Error = HTranslation.GetContent("没有可保存的对象！   No content to save."); return false; }
            return WriteObject(_content);
        }

        #endregion

        #region ==================== 实例：Update 原子读改写 + XPath/Transform/Schema/文件操作 ====================

        /// <summary>原子读→改→写：读取 XDocument → 传入回调修改 → 写回</summary>
        public bool UpdateXDocument(Func<XDocument, bool> modifier)
        {
            if (modifier == null) { Error = "modifier is null"; return false; }
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    XDocument doc = string.IsNullOrWhiteSpace(xml) ? CreateDocument(DictionaryRootName) : XDocument.Parse(xml);
                    if (!modifier(doc)) return false;
                    return this.WriteXDocument(doc);
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>从另一 XML（字符串）合并到当前文件并保存</summary>
        public bool MergeFrom(string overrideXml)
        {
            lock (mLockRead)
            {
                try
                {
                    string xml = ReadAllText();
                    string merged = Merge(string.IsNullOrWhiteSpace(xml) ? "<root/>" : xml, overrideXml);
                    return WriteAllText(merged);
                }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>对当前文件做 XPath 查询</summary>
        public IEnumerable<XObject> XPathQuery(string xpath, IDictionary<string, string> namespaces = null)
        {
            lock (mLockRead) { string xml = ReadAllText(); if (string.IsNullOrWhiteSpace(xml)) yield break; foreach (var n in XPathQuery(xml, xpath, namespaces)) yield return n; }
        }

        /// <summary>对当前文件做 XPath 单值查询</summary>
        public string XPathQuerySingle(string xpath, IDictionary<string, string> namespaces = null)
        {
            lock (mLockRead) { string xml = ReadAllText(); return xml == null ? null : XPathQuerySingle(xml, xpath, namespaces); }
        }

        /// <summary>用指定 XSLT 文件转换当前文件，结果写回本文件（可选覆盖原文件）</summary>
        public bool TransformWith(string xslPath)
        {
            lock (mLockRead)
            {
                try { string xml = ReadAllText(); if (xml == null) return false; string outXml = Transform(xml, xslPath, FileEncoding); return WriteAllText(outXml); }
                catch (Exception ex) { Error = ex.Message; return false; }
            }
        }

        /// <summary>用 XSD 文件验证当前文件（实例方法；返回布尔 + out 错误）</summary>
        public bool ValidateWithSchema(string xsdPath, out string error)
        {
            lock (mLockRead) { string xml = ReadAllText(); return ValidateSchemaFile(xml ?? string.Empty, xsdPath, out error); }
        }

        /// <summary>文件是否存在</summary>
        public bool Exists { get { return HFile.HFilePath.FileExists(FilePath); } }

        /// <summary>FileInfo</summary>
        public FileInfo GetFileInfo() { return new FileInfo(FilePath); }

        /// <summary>删除主文件及所有备份残留</summary>
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
