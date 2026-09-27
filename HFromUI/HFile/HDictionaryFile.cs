using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HEnum;

namespace HFromUI.HFile
{
    using HFromUI.HLangage;
    /// <summary>
    /// 键值对字典文件读写类：以纯文本逐行存储（分隔符默认 |&lt;&gt;|），支持加密钩子、类型化读写、
    /// 超容量淘汰与批量保存。
    ///
    /// 崩溃恢复机制（与 HJsonFile/HXmlFile 等价的旧版实现，后缀固定 .temp）：
    ///   • WriteAll 写入前先把旧正式文件复制为 文件名.temp 作为备份，全部行写完且成功后才删除 .temp；
    ///   • 构造时执行 Load()：若发现上次中断遗留的 .temp，能解析则回滚恢复为正式文件，
    ///     不能解析（说明新内容已完整落盘、仅残留备份）则回退读取正式文件；两者都失败抛出文件损坏异常。
    ///
    /// 线程安全：实例内部以 mLockRead 保护读改写操作；多线程对同一文件请复用同一个实例。
    /// </summary>
    public class HDictionaryFile
    {
        /// <summary>内存中的键值表（值统一按字符串落盘，读取时再按 HDictionaryType 转换）</summary>
        private ConcurrentDictionary<string, object> dictionaryList = new ConcurrentDictionary<string, object>();
        /// <summary>writeNumber 字段：距上次整盘重写以来的增量写入计数。</summary>
        private int writeNumber = 0;
        /// <summary>suffixPathName 字段：崩溃恢复备份文件后缀。</summary>
        private string suffixPathName = ".temp";
        /// <summary>ListMaxNumber 成员：内存键值上限，读取时超出后淘汰最早的键；&lt;=0 表示不限制。</summary>
        public int ListMaxNumber { set; get; } = 100000;

        /// <summary>SetWrite 成员：同键更新时的攒批整盘重写阈值，&gt;0 表示累计达到该次数后重写一次；&lt;=0 表示只追加不重写。</summary>
        public int SetWrite { set; get; } = -1;
        /// <summary>按键索引内存字典（键不存在时 get 会抛 KeyNotFoundException）</summary>
        public object this[string key]
        {
            get
            {
                return dictionaryList[key];
            }
            set
            {
                dictionaryList[key] = value;
            }
        }
        /// <summary>获取或设置内存键值表本身（替换后后续读写均针对新表）</summary>
        public ConcurrentDictionary<string, object> DictionaryList
        {
            set
            {
                dictionaryList = value;
            }
            get
            {
                return dictionaryList;
            }
        }

        /// <summary>SplitChar 成员：每行键与值之间的分隔符，落盘格式为 键+分隔符+值。</summary>
        public string SplitChar { set; get; } = "|<>|";

        /// <summary>底层文本文件读写对象（真正的磁盘 IO 与编码由 HTxtFile 承担）</summary>
        private HTxtFile txtFile;
        /// <summary>mLockRead 字段：读改写互斥锁，保证同键更新、整盘重写等复合操作的原子性。</summary>
        private object mLockRead = new object();

        /// <summary>最近一次错误信息（正常时为空字符串，调用方可据此判断失败原因）</summary>
        public string Error = string.Empty;
        /// <summary>获取或设置底层文件编码（透传给 HTxtFile）</summary>
        public Encoding FileEncoding
        {
            set
            {
                txtFile.FileEncoding = value;
            }
            get
            {
                return txtFile.FileEncoding;
            }
        }
        /// <summary>获取或设置底层文本文件的完整路径</summary>
        public string FilePath
        {
            get
            {
                return txtFile.FilePath;
            }
            set
            {
                txtFile.FilePath = value;
            }
        }
        /// <summary>
        /// 构造字典文件读写对象并立即执行崩溃恢复加载。
        /// </summary>
        /// <param name="filePath">文件路径或文件名；为空时使用程序目录下的 TxtDefault.txt，规则详见 GetTxtFullPath。</param>
        /// <param name="suffixName">filePath 不含扩展名时追加的后缀；为空默认 .txt。</param>
        /// <param name="splitChar">自定义键值分隔符；为空使用默认的 |&lt;&gt;|。</param>
        /// <exception cref="Exception">正式文件与 .temp 备份均无法解析时抛出文件损坏异常。</exception>
        public HDictionaryFile(string filePath, string suffixName = "", string splitChar = "")
        {
            if (!string.IsNullOrWhiteSpace(splitChar))
            {
                SplitChar = splitChar;
            }
            txtFile = new HTxtFile(GetTxtFullPath(filePath, suffixName));
            Load();
        }
        /// <summary>
        /// 加载字典文件：若存在上次写入中断留下的临时文件则先尝试恢复为正式文件，
        /// 临时文件无法解析时回退读取正式文件，两者都失败时抛出文件损坏异常。
        /// </summary>
        private void Load()
        {
            string textpath = txtFile.FilePath;
            // 先把底层文件指向崩溃备份（正式路径 + .temp）
            txtFile.FilePath = textpath + suffixPathName;
            if (HFile.HFilePath.FileExists(txtFile.FilePath))
            {
                // 存在 .temp：说明上一次 WriteAll 可能中途中断
                if (ReadAll())
                {
                    // .temp（旧数据备份）能解析：回滚——删除可能写坏的正式文件，用备份覆盖回去
                    bool isok=true;
                    if (HFile.HFilePath.FileExists(textpath))
                    {
                     isok&=  HFile.HFilePath.DeleteFile(textpath);
                    }
                    if (HFile.HFilePath.CopyFile(txtFile.FilePath, textpath, true))
                    {
                        if (HFile.HFilePath.FileExists(txtFile.FilePath))
                        {
                           isok&=   HFile.HFilePath.DeleteFile(txtFile.FilePath);
                        }
                    }
                    else
                    {
                         isok&=  false;
                    }
                    if (!isok)
                    {

                         throw new Exception(HTranslation.GetContent("文件有重复或是已经损坏!Copy   The file is duplicated or corrupted!") + textpath);
                    }
                     txtFile.FilePath = textpath;
                }
                else
                {
                    // .temp 无法解析：说明新内容已完整写入正式文件、仅残留备份，改读正式文件
                    txtFile.FilePath = textpath;
                    if (!ReadAll())
                    {
                        throw new Exception(HTranslation.GetContent("文件有重复或是已经损坏!   The file is duplicated or corrupted!") + textpath);
                    }
                }
            }
            else
            {
                // 无遗留备份：正常加载正式文件
                txtFile.FilePath = textpath;
                if (!ReadAll())
                {
                    throw new Exception(HTranslation.GetContent("文件有重复或是已经损坏!   The file is duplicated or corrupted!") + textpath);
                }
            }
        }
        /// <summary>
        /// 按构造参数解析出文本文件的完整绝对路径。
        /// </summary>
        /// <param name="filePath">空串/空白：程序目录 TxtDefault.txt；含扩展名：原样使用；含盘符：当前目录追加后缀；其它：程序目录下追加。</param>
        /// <param name="suffixName">无扩展名时追加的后缀；为空使用 .txt。</param>
        /// <returns>归一化后的绝对路径。</returns>
        private string GetTxtFullPath(string filePath = "", string suffixName = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Path.GetFullPath(HFromUI.HData.HAppData.AppPath + @"\TxtDefault" + (string.IsNullOrWhiteSpace(suffixName) ? ".txt" : suffixName));
            }
            else
            {
                if (Path.GetFileName(filePath).Contains("."))
                {
                    return Path.GetFullPath(filePath);
                }
                else
                {
                    if (filePath.Contains(":"))
                    {
                        return Path.GetFullPath($@"{filePath}" + (string.IsNullOrWhiteSpace(suffixName) ? ".txt" : suffixName));
                    }
                    else
                    {
                        return Path.GetFullPath(HFromUI.HData.HAppData.AppPath + $@"\{filePath}" + (string.IsNullOrWhiteSpace(suffixName) ? ".txt" : suffixName));
                    }

                }

            }
        }
        /// <summary>
        /// 自定义加密委托：不为空时优先于 virtual Encrypt(string) 执行。
        /// 外部注入：`file.EncryptFn = raw => 你的加密实现(raw);`
        /// </summary>
        public Func<string, string> EncryptFn { get; set; }

        /// <summary>
        /// 自定义解密委托：不为空时优先于 virtual Decrypt(string) 执行。
        /// 外部注入：`file.DecryptFn = raw => 你的解密实现(raw);`
        /// </summary>
        public Func<string, string> DecryptFn { get; set; }

        /// <summary>解密一行落盘文本：优先调用 DecryptFn，未注入时原样返回，可由子类重写。</summary>
        public virtual string Decrypt(string content)
        {
            if (DecryptFn != null) return DecryptFn(content) ?? content;
            return content;
        }
        /// <summary>加密一行待写文本：优先调用 EncryptFn，未注入时原样返回，可由子类重写。</summary>
        public virtual string Encrypt(string content)
        {
            if (EncryptFn != null) return EncryptFn(content) ?? content;
            return content;
        }
        /// <summary>按键取内存字典中的原始值（键不存在时抛 KeyNotFoundException）</summary>
        public object Get(string key)
        {
            return dictionaryList[key];
        }
        /// <summary>
        /// 把落盘的弱类型值转换为目标类型（静态工具方法，不访问文件）。
        /// </summary>
        /// <param name="objectString">原始值（字符串或已是目标类型的对象）。</param>
        /// <param name="dictionaryType">目标值类型。</param>
        /// <param name="IsConvertOK">引用返回：转换是否成功。</param>
        /// <param name="format">DateTime 类型的解析格式说明（格式名;格式串 或纯格式串）。</param>
        /// <returns>转换后的值；失败返回 null 且 IsConvertOK=false。</returns>
        public static object GetValue(object objectString, HDictionaryType dictionaryType, ref bool IsConvertOK, string format = null)
        {
            try
            {
                object dicObject = null;
                switch (dictionaryType)
                {
                    case HDictionaryType.String:
                        if (objectString is string)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = objectString.ToString();
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Int:
                    case HDictionaryType.Int32:
                        if (objectString is int)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToInt32(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Int16:
                    case HDictionaryType.Short:
                        if (objectString is short)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToInt16(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Float:
                    case HDictionaryType.Sing:
                        if (objectString is float)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToSingle(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Encoding:
                        if (objectString is Encoding)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            bool dicEncoding = false;
                            try
                            {
                                Convert.ToInt32(objectString);
                                dicEncoding = true;
                            }
                            catch { dicEncoding = false; }
                            if (dicEncoding)
                            {
                                dicObject = Encoding.GetEncoding(Convert.ToInt32(objectString));
                            }
                            else
                            {
                                dicObject = Encoding.GetEncoding(objectString.ToString());
                            }

                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Double:
                        if (objectString is double)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToDouble(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Uint16:
                    case HDictionaryType.Ushort:
                        if (objectString is ushort)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToUInt16(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Uint:
                    case HDictionaryType.Uint32:
                        if (objectString is uint)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToUInt32(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Uint64:
                        if (objectString is UInt64)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToUInt64(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Int64:
                        if (objectString is Int64)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToInt64(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Decimal:
                        if (objectString is decimal)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToDecimal(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Long:
                        if (objectString is long)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToInt64(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Char:
                        if (objectString is char)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToChar(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Byte:
                        if (objectString is byte)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            dicObject = Convert.ToByte(objectString);
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Bool:
                        if (objectString is bool)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            string boolString = objectString.ToString().Trim();
                            if (boolString == "0" || boolString == "1")
                            {
                                if (boolString.Trim() == "0")
                                {
                                    dicObject = false;
                                }
                                else
                                {
                                    dicObject = true;
                                }
                                objectString = dicObject;
                            }
                            else
                            {
                                dicObject = Convert.ToBoolean(objectString);
                                objectString = dicObject;
                            }
                        }
                        break;
                    case HDictionaryType.DateTime:
                        if (objectString is DateTime)
                        {
                            dicObject = objectString;
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(format))
                            {
                                dicObject = Convert.ToDateTime(objectString.ToString());
                            }
                            else
                            {
                                string[] formats = format.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (formats.Length > 1)
                                {
                                    System.IFormatProvider formata = new System.Globalization.CultureInfo(formats[0], true);
                                    string strDateFormat = formats[1];
                                    dicObject = DateTime.ParseExact(objectString.ToString(), strDateFormat, formata);
                                }
                                else
                                {
                                    System.IFormatProvider formata = new System.Globalization.CultureInfo("zh-CN", true);
                                    string strDateFormat = format;
                                    dicObject = DateTime.ParseExact(objectString.ToString(), strDateFormat, formata);
                                }
                            }
                            objectString = dicObject;
                        }
                        break;
                    case HDictionaryType.Object:
                        dicObject = objectString;
                        break;
                    default:
                        dicObject = objectString;
                        break;
                }
                IsConvertOK = true;
                return dicObject;
            }
            catch { IsConvertOK = false; return null; }
        }
        /// <summary>
        /// 静态 GetValue 的布尔结果重载：把转换是否成功作为返回值，转换值由 content 带出。
        /// </summary>
        /// <param name="objectString">原始值。</param>
        /// <param name="dictionaryType">目标值类型。</param>
        /// <param name="content">转换结果输出。</param>
        /// <param name="format">DateTime 类型的解析格式说明。</param>
        /// <returns>转换是否成功。</returns>
        public static bool GetValue(object objectString, HDictionaryType dictionaryType, ref object content, string format = null)
        {
            bool isok = true;
            content = GetValue(objectString, dictionaryType, ref isok, format);
            return isok;
        }
        /// <summary>
        /// 按键取值并按指定类型转换到 content。
        /// </summary>
        /// <param name="key">字典键。</param>
        /// <param name="dictionaryType">目标值类型。</param>
        /// <param name="content">转换结果输出。</param>
        /// <param name="format">DateTime 类型的解析格式说明。</param>
        /// <returns>转换是否成功。</returns>
        public bool Get(string key, HDictionaryType dictionaryType, ref object content, string format = null)
        {
            return GetValue(dictionaryList[key], dictionaryType, ref content, format);

        }
        /// <summary>
        /// 清空内存表并从磁盘重新读取全部键值（逐行解密后按分隔符切分）。
        /// </summary>
        /// <param name="IsCertitude">true=严格模式，遇坏行即整体失败；false=宽松模式，跳过坏行继续解析。</param>
        /// <returns>是否全部解析成功；失败信息写入 Error。</returns>
        public bool ReadAll(bool IsCertitude = true)
        {
            bool IsOK = true;
            try
            {
                dictionaryList.Clear();
                string[] DictionaryLists = txtFile.ReadAllLines();
                List<string> DictionaryListss = new List<string>();
                if (IsCertitude)
                {
                    foreach (var item in DictionaryLists)
                    {
                        DictionaryListss.Add(Decrypt(item));
                    }
                    foreach (var item in DictionaryListss)
                    {
                        int index = item.IndexOf(SplitChar);
                        if (index > 0)
                        {
                            try
                            {
                                dictionaryList[item.Substring(0, index)] = item.Substring(index + SplitChar.Length);
                            }
                            catch 
                            {
                                IsOK &= dictionaryList.TryAdd(item.Substring(0, index), item.Substring(index + SplitChar.Length));
                            }
                        }
                    }
                    DictionaryListss.Clear();
                    DictionaryLists = null;
                }
                else
                {
                    foreach (var item in DictionaryLists)
                    {
                        DictionaryListss.Add(Decrypt(item));
                    }
                    foreach (var item in DictionaryListss)
                    {
                        try
                        {
                            int index = item.IndexOf(SplitChar);
                            if (index > 0)
                            {
                                try
                                {
                                    dictionaryList[item.Substring(0, index)] = item.Substring(index + SplitChar.Length);
                                }
                                catch
                                {
                                    IsOK &= dictionaryList.TryAdd(item.Substring(0, index), item.Substring(index + SplitChar.Length));
                                }
                            }
                        }
                        catch { }
                    }
                    DictionaryListss.Clear();
                    DictionaryLists = null;
                }
            }
            catch (Exception ex) { Error = ex.Message; }
            return IsOK;
        }
        /// <summary>
        /// 整盘重写：先把旧正式文件复制为 .temp 崩溃备份，再清空文件逐行加密写入全部键值，
        /// 全部成功后删除备份。中途异常或任一步骤失败时保留 .temp 供下次构造 Load() 回滚。
        /// </summary>
        /// <returns>全部步骤成功返回 true；失败返回 false 并把原因写入 Error（备份已存在时直接返回 false）。</returns>
        public bool WriteAll()
        {
            lock (mLockRead)
            {
                bool IsOK = true;
                try
                {
                    if (HFile.HFilePath.FileExists(txtFile.FilePath))
                    {
                        // 步骤 1：旧文件备份为 .temp（崩溃后 Load 据此回滚）
                        IsOK&= HFile.HFilePath.CopyFile(txtFile.FilePath, txtFile.FilePath+ suffixPathName,true);
                        if (!IsOK)
                        {
                           // 备份失败但 .temp 已存在：可能上次残留，保守起见不继续写
                           if (HFile.HFilePath.FileExists(txtFile.FilePath + suffixPathName))
                           {
                                  return false;
                           }
                           else
                           {
                                 throw new Exception(HTranslation.GetContent("文件有重复或是已经损坏!WriteAll   The file is duplicated or corrupted!") );
                           }

                        }
                    }
                    // 步骤 2：清空正式文件，逐行写入加密后的 键+分隔符+值
                    if (IsOK&&txtFile.DeleteAllLine())
                    {
                        foreach (var item in dictionaryList.Keys)
                        {
                            if (dictionaryList[item] is Encoding)
                            {
                                // Encoding 不能直接 ToString，落盘其代码页编号
                                Encoding encoding = dictionaryList[item] as Encoding;
                                IsOK &= txtFile.AddLine(Encrypt(item + SplitChar + encoding.CodePage));
                            }
                            else
                            {
                                IsOK &= txtFile.AddLine(Encrypt(item + SplitChar + dictionaryList[item]));
                            }

                        }
                    }
                    else
                    {
                        IsOK = false;
                    }
                    // 步骤 3：全部成功才删除崩溃备份；失败则保留以便回滚
                    if (IsOK)
                    {
                        HFile.HFilePath.DeleteFile(txtFile.FilePath + suffixPathName);
                    }
                }
                catch (Exception ex) { Error = ex.Message; IsOK = false; }
                return IsOK;
            }

        }
        /// <summary>保存（WriteAll 的语义化别名）。</summary>
        /// <returns>整盘重写是否成功。</returns>
        public bool Save()
        {
            return WriteAll();
        }
        /// <summary>
        /// 读取一个键值：键不存在时以默认值建档并追加落盘；容量超限时先淘汰最早的键。
        /// </summary>
        /// <param name="key">字典键。</param>
        /// <param name="o_default">键不存在时写入并返回的默认值。</param>
        /// <param name="dictionaryType">值类型（决定落盘/转换方式，Encoding 按代码页存）。</param>
        /// <param name="format">DateTime 类型的解析格式说明。</param>
        /// <returns>读到的值（或新建的默认值）。</returns>
        /// <exception cref="Exception">默认值落盘失败或类型转换失败时抛出。</exception>
        public object Read(string key, object o_default, HDictionaryType dictionaryType, string format = null)
        {
            lock (mLockRead)
            {
                if (ListMaxNumber > 0)
                {
                    if (dictionaryList.Count > ListMaxNumber)
                    {
                        // 超出容量上限：淘汰枚举到的第一个（最早加入的）键
                        string itemkey = "";
                        foreach (var item in dictionaryList.Keys)
                        {
                            itemkey = item;
                            break;
                        }
                        dictionaryList.TryRemove(itemkey,out object v);
                    }
                }
                bool IsContains = false;
                foreach (var item in dictionaryList.Keys)
                {
                    if (item == key)
                    {
                        IsContains = true;
                        break;
                    }
                }
                bool isok = true;
                if (!IsContains)
                {
                    // 键不存在：内存建档并把默认值以加密行追加落盘
                    isok&= dictionaryList.TryAdd(key, o_default);
                    if (o_default is Encoding)
                    {
                        Encoding encoding = o_default as Encoding;
                        if (!txtFile.AddLine(Encrypt(key + SplitChar + encoding.CodePage)))
                        {
                            throw new Exception(HTranslation.GetContent("写默认参数错误！   Write default parameter error!"));
                        }
                    }
                    else
                    {
                        if (!txtFile.AddLine(Encrypt(key + SplitChar + o_default)))
                        {
                            throw new Exception(HTranslation.GetContent("写默认参数错误！   Write default parameter error!"));
                        }
                    }

                }
                object content = null;
                if (isok)
                {
                    if (!Get(key, dictionaryType, ref content, format))
                    {
                        throw new Exception(HTranslation.GetContent("获取参数错误！   Error in getting parameters!"));
                    }
                }
                else
                {
                    throw new Exception(HTranslation.GetContent("获取参数错误！   Error in getting parameters!"));
                }
                return content;
            }
        }
        /// <summary>读取布尔型键值（字符串 0/false 为假，1/true/yes/y 为真）；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public bool Read(string key, bool o_default)
        {
            return (bool)Read(key, o_default, HDictionaryType.Bool);
        }
        /// <summary>读取 16 位有符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public short Read(string key, short o_default)
        {
            return (short)Read(key, o_default, HDictionaryType.Short);
        }
        /// <summary>读取 16 位无符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public ushort Read(string key, ushort o_default)
        {
            return (ushort)Read(key, o_default, HDictionaryType.Ushort);
        }
        /// <summary>读取 32 位有符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public int Read(string key, int o_default)
        {
            return (int)Read(key, o_default, HDictionaryType.Int);
        }
        /// <summary>读取 32 位无符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public uint Read(string key, uint o_default)
        {
            return (uint)Read(key, o_default, HDictionaryType.Uint);
        }
        /// <summary>读取单精度浮点键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public float Read(string key, float o_default)
        {
            return (float)Read(key, o_default, HDictionaryType.Float);
        }
        /// <summary>读取双精度浮点键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public double Read(string key, double o_default)
        {
            return (double)Read(key, o_default, HDictionaryType.Double);
        }
        /// <summary>读取弱类型对象键值（按 Object 类型原样返回，不做转换）；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public object Read(string key, object o_default)
        {
            return Read(key, o_default, HDictionaryType.Object);
        }
        /// <summary>读取字符串键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public string Read(string key, string o_default)
        {
            return (string)Read(key, o_default, HDictionaryType.String);
        }
        /// <summary>读取字节型键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public byte Read(string key, byte o_default)
        {
            return (byte)Read(key, o_default, HDictionaryType.Byte);
        }
        /// <summary>读取 64 位有符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public Int64 Read(string key, Int64 o_default)
        {
            return (Int64)Read(key, o_default, HDictionaryType.Int64);
        }
        /// <summary>读取 64 位无符号整数键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public UInt64 Read(string key, UInt64 o_default)
        {
            return (UInt64)Read(key, o_default, HDictionaryType.Uint64);
        }
        /// <summary>读取十进制数键值（高精度金额/计量场景）；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public decimal Read(string key, decimal o_default)
        {
            return (decimal)Read(key, o_default, HDictionaryType.Decimal);
        }
        /// <summary>读取字符型键值；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public char Read(string key, char o_default)
        {
            return (char)Read(key, o_default, HDictionaryType.Char);
        }
        /// <summary>读取字符编码键值（落盘为代码页编号）；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public Encoding Read(string key, Encoding o_default)
        {
            return (Encoding)Read(key, o_default, HDictionaryType.Encoding);
        }
        /// <summary>读取日期时间键值，可用 format 指定精确解析格式；键不存在时以默认值建档并落盘，随后返回默认值。</summary>
        public DateTime Read(string key, DateTime o_default, string format = null)
        {
            return (DateTime)Read(key, o_default, HDictionaryType.DateTime, format);
        }

        /// <summary>
        /// 写入一个键值。新键：内存建档并向文件追加加密行；已存在键：先更新内存，
        /// 当 SetWrite&gt;0 且累计增量达到阈值（或 isWriteAll=true）时整盘重写，否则同样追加一行
        /// （追加产生的重复键在下次 ReadAll 时以后写者为准）。
        /// </summary>
        /// <param name="key">字典键。</param>
        /// <param name="content">值（Encoding 按代码页编号落盘）。</param>
        /// <param name="isWriteAll">true 时强制立即整盘重写（忽略 SetWrite 攒批阈值）。</param>
        /// <returns>落盘动作是否成功。</returns>
        public bool Write(string key, object content, bool isWriteAll = false)
        {
            lock (mLockRead)
            {

                bool IsContains = false;
                foreach (var item in dictionaryList.Keys)
                {
                    if (item == key)
                    {
                        IsContains = true;
                        break;
                    }
                }
                if (!IsContains)
                {
                    // 新键：内存建档 + 追加加密行
                    IsContains= dictionaryList.TryAdd(key, content);
                    if (IsContains)
                    {
                        if (content is Encoding)
                        {
                            Encoding encoding = content as Encoding;
                            return txtFile.AddLine(Encrypt(key + SplitChar + encoding.CodePage));
                        }
                        else
                        {
                            return txtFile.AddLine(Encrypt(key + SplitChar + content));
                        }
                    }
                    return IsContains;
                }
                else
                {
                    if (SetWrite>0)
                    {
                        // 攒批模式：达到阈值或外部强制时整盘重写，消除追加产生的重复键行
                        if (SetWrite < writeNumber || isWriteAll)
                        {
                            writeNumber = 0;
                            dictionaryList[key] = content;
                            return WriteAll();
                        }
                        else
                        {
                            writeNumber++;
                            dictionaryList[key] = content;
                            if (content is Encoding)
                            {
                                Encoding encoding = content as Encoding;
                                return txtFile.AddLine(Encrypt(key + SplitChar + encoding.CodePage));
                            }
                            else
                            {
                                return txtFile.AddLine(Encrypt(key + SplitChar + content));
                            }
                        }
                    }
                    else
                    {
                        // 非攒批模式（SetWrite<=0）：仅更新内存并追加一行
                        writeNumber=0;
                        dictionaryList[key] = content;
                        if (content is Encoding)
                        {
                            Encoding encoding = content as Encoding;
                            return txtFile.AddLine(Encrypt(key + SplitChar + encoding.CodePage));
                        }
                        else
                        {
                            return txtFile.AddLine(Encrypt(key + SplitChar + content));
                        }
                    }


                }
            }

        }
    }
}
