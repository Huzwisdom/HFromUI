using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HFromUI.HConvert
{
    public static class HFormatConvert
    {
        /// <summary>
        /// Hex格式的字符串转换为字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string s)
        {
            try
            {
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                }

                return buffer;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 字节数组转换为Hex格式字符串
        /// </summary>
        /// <param name="bmsg"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bmsg)
        {
            StringBuilder sb = new StringBuilder(bmsg.Length * 3);
            foreach (byte b in bmsg)
            {
                //if(b=='\0')//若字节为0说明是空，则跳过
                //{
                //    continue;
                //}
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// 字符串转换为Hex格式的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToHexString(string str)//字符串转为HEX字符串
        {
            StringBuilder sb = new StringBuilder(str.Length * 3);
            foreach (byte b in str)
            {
                //if(b=='\0')//若字节为0说明是空，则跳过
                //{
                //    continue;
                //}
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// 字符数组转字符串，去掉为空的部分
        /// </summary>
        /// <param name="charArray"></param>
        /// <returns></returns>
        public static string CharArrayToString(char[] charArray)
        {
            char[] charA = (char[])charArray;
            string str = string.Empty;
            if (charA == null)
            {
                return "";
            }
            foreach (char c in charA)
            {
                if (c == '\0')
                {
                    break;
                }
                else
                {
                    str += c;
                }
            }
            return str;
        }

        /// <summary>
        /// 将结构体对象转换为byte数组
        /// </summary>
        /// <param name="obj">被转换对象</param>
        /// <returns>转换后byte数组</returns>
        public static byte[] StructToBytes(object obj)
        {
            try
            {
                int size = Marshal.SizeOf(obj);//获取对象的非托管大小
                byte[] bytes = new byte[size];
                IntPtr structPtr = Marshal.AllocHGlobal(size);//从进程的非托管内存中分配内存
                Marshal.StructureToPtr(obj, structPtr, false);//将数据从托管内存中封送到非托管内存中
                Marshal.Copy(structPtr, bytes, 0, size);//将数据从非托管内存指针复制托管整数数组中
                Marshal.FreeHGlobal(structPtr);//释放内存
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将byte数组转换成结构体对象
        /// </summary>
        /// <param name="buff">被转换byte数组</param>
        /// <param name="typ">转换成的类名</param>
        /// <returns>转换完成后的对象</returns>
        public static object BytesToStruct(byte[] buff, Type type)
        {
            try
            {
                int size = Marshal.SizeOf(type);
                if (size > buff.Length)
                {
                    return null;
                }
                IntPtr structPtr = Marshal.AllocHGlobal(size);//开辟内存
                Marshal.Copy(buff, 0, structPtr, size);//
                object obj = Marshal.PtrToStructure(structPtr, type);//将数据从非托管内存块封送到新分配的指定类型的托管对象。
                Marshal.FreeHGlobal(structPtr);//释放内存
                return obj;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 判断是否是正整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }

        /// <summary>
        /// 比较两张图片相等
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns></returns>
        public static bool ImageCompare(Bitmap firstImage, Bitmap secondImage)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                firstImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                String firstBitmap = Convert.ToBase64String(ms.ToArray());
                ms.Position = 0;

                secondImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                String secondBitmap = Convert.ToBase64String(ms.ToArray());

                if (firstBitmap.Equals(secondBitmap))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #region 文件

        /// <summary>
        /// 读取文件到字节数组
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static byte[] FileToBytes(string FilePath)//文件流转为字节数组
        {
            if (!System.IO.File.Exists(FilePath))
            {
                return new byte[0];
            }

            try
            {
                FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                byte[] buff = new byte[fs.Length];

                fs.Read(buff, 0, Convert.ToInt32(fs.Length));
                fs.Close();

                return buff;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 读取文件为流
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Stream FileToStream(string fileName)
        {
            // 打开文件

            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            // 读取文件的 byte[]

            byte[] bytes = new byte[fileStream.Length];

            fileStream.Read(bytes, 0, bytes.Length);

            fileStream.Close();

            // 把 byte[] 转换成 Stream

            Stream stream = new MemoryStream(bytes);

            return stream;
        }

        #endregion 文件

        #region 表格

        /// <summary>
        /// 将Csv读入DataTable
        /// </summary>
        /// <param name="filePath">csv文件路径</param>
        /// <param name="n">表示第n行是字段title,第n+1行是记录开始</param>
        /// <param name="k">可选参数表示最后K行不算记录默认0</param>
        public static DataTable CsvToDataTable(string filePath, int n, DataTable dt) //这个dt 是个空白的没有任何行列的DataTable
        {
            String csvSplitBy = "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)";
            StreamReader reader = new StreamReader(filePath, System.Text.Encoding.UTF8, false);
            int i = 0, m = 0;
            reader.Peek();
            while (reader.Peek() > 0)
            {
                m = m + 1;
                string str = reader.ReadLine();
                if (m >= n + 1)
                {
                    if (m == n + 1) //如果是字段行，则自动加入字段。
                    {
                        MatchCollection mcs = Regex.Matches(str, csvSplitBy);
                        foreach (Match mc in mcs)
                        {
                            dt.Columns.Add(mc.Value); //增加列标题
                        }
                    }
                    else
                    {
                        MatchCollection mcs = Regex.Matches(str, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
                        i = 0;
                        System.Data.DataRow dr = dt.NewRow();
                        foreach (Match mc in mcs)
                        {
                            dr[i] = mc.Value;
                            i++;
                        }
                        dt.Rows.Add(dr);  //DataTable 增加一行
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// 获取datatable中的列头
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetColumnsByDataTable(DataTable dt)
        {
            string strColumns = null;

            if (dt.Columns.Count > 0)
            {
                int columnNum = 0;
                columnNum = dt.Columns.Count;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i == 0)
                    {
                        strColumns = dt.Columns[i].ColumnName;
                    }
                    else
                    {
                        strColumns += "," + dt.Columns[i].ColumnName;
                    }
                }
            }

            return strColumns;
        }

        /// <summary>
        /// dataTable导出到csv
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static bool DataTableToCsv(DataTable dt, string strFilePath)
        {
            try
            {
                string strBufferLine = "";
                StreamWriter strmWriterObj = new StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
                string columname = GetColumnsByDataTable(dt);
                //strmWriterObj.WriteLine(tableheader);
                strmWriterObj.WriteLine(columname);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    strBufferLine = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j > 0)
                            strBufferLine += ",";
                        strBufferLine += dt.Rows[i][j].ToString();
                    }
                    strmWriterObj.WriteLine(strBufferLine);
                }
                strmWriterObj.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 表格
    }
}
