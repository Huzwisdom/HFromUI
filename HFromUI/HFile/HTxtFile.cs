using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HFile
{
    public class HTxtFile
    {
        /// <summary>FilePath 成员。</summary>
        public string FilePath { set; get; }

        /// <summary>FileEncoding 成员。</summary>
        public Encoding FileEncoding { set; get; } = Encoding.UTF8;

        /// <summary>
        /// 最大行数
        /// </summary>
        public int MaxLines { get; set; } = -1;
        /// <summary>
        /// 报错提示
        /// </summary>
        public string StrError { get; private set; }

        /// <summary>mLock 字段。</summary>
        private readonly object mLock = new object();

        /// <summary>FPath 成员。</summary>
        public string FPath { set; get; } = "";
        /// <summary>Extension 成员。</summary>
        public string Extension { set; get; } = @".txt";

        public HTxtFile(string filePath = "", bool isDefault = true)
        {
            if (isDefault)
            {
                GetTxtFullPath(filePath);
            }
        }
        public HTxtFile(string fPath, string filePath, bool isDefault, string extension)
        {
            FPath = fPath; Extension = extension;
            if (isDefault)
            {
                GetTxtFullPath(filePath);
            }
        }
        public HTxtFile()
        {

        }
        public HTxtFile(string filePath, Encoding encoding, bool isDefault = true)
        {
            if (isDefault)
            {
                GetTxtFullPath(filePath);
            }
            FileEncoding = encoding;
        }
        /// <summary>获取 txtFullPath。</summary>
        private bool GetTxtFullPath(string filePath = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    FilePath = Path.GetFullPath(HFromUI.HData.HAppData.AppPath + FPath + @"\TxtDefault" + Extension);
                }
                else
                {
                    if (filePath.Contains("."))
                    {
                        FilePath = Path.GetFullPath(filePath);
                    }
                    else
                    {
                        FilePath = Path.GetFullPath(HFromUI.HData.HAppData.AppPath + FPath + $@"\{filePath}" + Extension);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>判断是否 Documents。</summary>
        public bool IsDocuments()
        {
            return IsDocuments(FilePath);
        }

        /// <summary>判断是否 Documents。</summary>
        public bool IsDocuments(string filePath)
        {
            try
            {
                if (File.Exists(Path.GetFullPath(filePath)))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return false;
        }

        /// <summary>获取 fileSize。</summary>
        public long GetFileSize(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return -1;
        }

        /// <summary>获取 fileSize。</summary>
        public long GetFileSize()
        {
            return GetFileSize(FilePath);
        }

        /// <summary>DeleteText 方法。</summary>
        public void DeleteText(string filePath)
        {
            File.Delete(Path.GetFullPath(filePath));
        }

        /// <summary>DeleteText 方法。</summary>
        public void DeleteText()
        {
            DeleteText(FilePath);
        }

        /// <summary>ReadAllText 方法。</summary>
        public string ReadAllText()
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }

                    return File.ReadAllText(FilePath, FileEncoding);
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return null;
            }


        }
        /// <summary>WriteAllText 方法。</summary>
        public bool WriteAllText(string WriteString)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    using (FileStream WriteFileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (StreamWriter WriteStreamWriter = new StreamWriter(WriteFileStream, FileEncoding))
                        {
                            WriteStreamWriter.WriteLine(WriteString);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }
        /// <summary>
        /// 增加一行数据
        /// </summary>
        /// <param name="strData">数据内容</param>
        /// <returns></returns>
        public bool AddLine(string strData, bool IsReplaceRN = true)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    if (MaxLines > 0)
                    {
                        var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                        while (list.Count >= MaxLines)
                        {
                            list.RemoveAt(0);
                        }

                        File.WriteAllLines(FilePath, list, FileEncoding);
                    }

                    using (var streamWriter = File.AppendText(FilePath))
                    {
                        if (IsReplaceRN)
                        {
                            streamWriter.WriteLine(strData.Replace("\r", "").Replace("\n", "").TrimEnd());
                        }
                        else
                        {
                            streamWriter.WriteLine(strData);
                        }
                        streamWriter.Close();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 插入一行数据
        /// </summary>
        /// <param name="nIndex">行号</param>
        /// <param name="strData">数据内容</param>
        /// <returns></returns>
        public bool InsertLine(int nIndex, string strData)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                    if (nIndex >= 0 && nIndex < list.Count)
                    {
                        list.Insert(nIndex, strData);
                    }
                    else
                    {
                        list.Add(strData);
                    }
                    if (MaxLines > 0)
                    {
                        while (list.Count > MaxLines)
                        {
                            list.RemoveAt(0);
                        }
                    }
                    File.WriteAllLines(FilePath, list, FileEncoding);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }
        /// <summary>
        /// 替换一行数据
        /// </summary>
        /// <param name="nIndex">行号</param>
        /// <param name="strNewData">数据内容</param>
        /// <returns></returns>
        public bool ReplaceLine(int nIndex, string strNewData)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                    if (list.Count <= 0)
                    {
                        return false;
                    }

                    list[nIndex] = strNewData;
                    File.WriteAllLines(FilePath, list, FileEncoding);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 替换一行数据
        /// </summary>
        /// <param name="strOldData">旧数据</param>
        /// <param name="strNewData">新数据</param>
        /// <returns></returns>
        public bool ReplaceLine(string strOldData, string strNewData)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                    if (list.Count <= 0)
                    {
                        return false;
                    }

                    for (var i = 0; i < list.Count; i++)
                    {
                        if (list[i].Contains(strOldData))
                        {
                            list[i] = strNewData;
                            break;
                        }
                    }

                    File.WriteAllLines(FilePath, list, FileEncoding);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 删除一行
        /// </summary>
        /// <param name="nIndex">行号</param>
        /// <returns></returns>
        public bool DeleteLine(int nIndex)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                    if (list.Count <= 0)
                    {
                        return false;
                    }

                    list.RemoveAt(nIndex);
                    File.WriteAllLines(FilePath, list, FileEncoding);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 删除一行数据
        /// </summary>
        /// <param name="strData">数据内容</param>
        /// <returns></returns>
        public bool DeleteLine(string strData)
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }
                    var list = new List<string>(File.ReadAllLines(FilePath, FileEncoding));
                    if (list.Count <= 0)
                    {
                        return false;
                    }

                    for (var i = 0; i < list.Count; i++)
                    {
                        if (list[i].Contains(strData))
                        {
                            list.RemoveAt(i);
                            i--;
                        }
                    }

                    File.WriteAllLines(FilePath, list, FileEncoding);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }

        }

        /// <summary>
        /// 删除所有数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAllLine()
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }

                    if (File.Exists(FilePath))
                    {
                        File.Delete(FilePath);
                    }

                    var fileStream = File.Create(FilePath);
                    fileStream.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 读取所有数据
        /// </summary>
        /// <returns></returns>
        public string[] ReadAllLines()
        {
            try
            {
                lock (mLock)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }

                    if (!File.Exists(FilePath))
                    {
                        var fileStream = File.Create(FilePath);
                        fileStream.Close();
                        return new string[0];
                    }

                    return File.ReadAllLines(FilePath, FileEncoding);
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return new string[0];
            }

        }

        /// <summary>
        /// 查找数据
        /// </summary>
        /// <param name="strData">数据内容</param>
        /// <param name="bEqual">是否完全匹配</param>
        /// <returns></returns>
        public bool FindData(string strData, bool bEqual = true)
        {
            try
            {
                var datas = ReadAllLines();
                foreach (var data in datas)
                {
                    if (bEqual && data == strData)
                    {
                        return true;
                    }

                    if (!bEqual && data.Contains(strData))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 查找记录
        /// </summary>
        /// <param name="strData">包含数据内容</param>
        /// <returns></returns>
        public string[] FindRecord(string strData)
        {
            try
            {
                var datas = ReadAllLines();
                var list = new List<string>();
                foreach (var data in datas)
                {
                    if (data.Contains(strData))
                    {
                        list.Add(data);
                    }
                }

                return list.ToArray();
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return new string[0];
            }
        }

        /// <summary>
        /// 查找数据索引
        /// </summary>
        /// <param name="strData">数据内容</param>
        /// <param name="bEqual">是否完全匹配</param>
        /// <returns></returns>
        public int[] FindDataIndexs(string strData, bool bEqual = true)
        {
            try
            {
                var datas = ReadAllLines();
                var list = new List<int>();
                for (var i = 0; i < datas.Length; i++)
                {
                    if (bEqual && datas[i] == strData)
                    {
                        list.Add(i);
                    }
                    else if (!bEqual && datas[i].Contains(strData))
                    {
                        list.Add(i);
                    }
                }

                return list.ToArray();
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return new int[0];
            }
        }
    }
}
