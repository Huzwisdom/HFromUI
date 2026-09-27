using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HFromUI;

namespace HFromUI.HConvert
{
    using HFromUI.HLangage;
    /// <summary>
    /// 文件路径/文件信息工具类（兼容 .NET Framework 4.8 / C# 7.3）。
    /// 提供路径验证、分隔符转换、文件属性、文件操作、目录操作、编码检测、哈希计算、
    /// 文件比较、特殊目录、临时文件、驱动器信息、通配符匹配、相对路径计算等超过 60 项实用功能。
    /// 所有面向用户的文本均通过 <see cref="HTranslation.GetContent"/> 进行多语言翻译。
    /// </summary>
    public class HStringPath
    {

        #region 应用程序基目录（静态属性）

        /// <summary>
        /// 应用程序基目录（HFromUI.HData.HAppData.AppPath），自动规范化分隔符。
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                try
                {
                    return NormalizeSeparators(HFromUI.HData.HAppData.AppPath);
                }
                catch { return string.Empty; }
            }
        }

        /// <summary>获取 startsWithStringEnd。</summary>
        public static string GetStartsWithStringEnd(string data,string start)
        {
            if (data.StartsWith(start))
            {
             return   data.Substring(start.Length);
            }
            return "";
        }
        /// <summary>基于基目录拼接相对路径并规范化。</summary>
        public static string CombineWithBaseDirectory(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return BaseDirectory;
            return CombinePath(BaseDirectory, relativePath);
        }

        #endregion

        #region 非法字符（跨平台）

        /// <summary>WindowsInvalidFileNameChars 字段。</summary>
        private static readonly char[] WindowsInvalidFileNameChars = Path.GetInvalidFileNameChars();
        /// <summary>WindowsInvalidPathChars 字段。</summary>
        private static readonly char[] WindowsInvalidPathChars = Path.GetInvalidPathChars();
        /// <summary>LinuxInvalidFileNameChars 字段。</summary>
        private static readonly char[] LinuxInvalidFileNameChars = { '/', '\0' };
        /// <summary>LinuxInvalidPathChars 字段。</summary>
        private static readonly char[] LinuxInvalidPathChars = { '\0' };

        #endregion

        #region 路径分类正则

        /// <summary>WindowsAbsolutePathRegex 字段。</summary>
        private static readonly Regex WindowsAbsolutePathRegex = new Regex(
            @"^[a-zA-Z]:[\\/].*|^\\\\[^\\/]+[\\/].*", RegexOptions.Compiled);
        /// <summary>LinuxAbsolutePathRegex 字段。</summary>
        private static readonly Regex LinuxAbsolutePathRegex = new Regex(
            @"^/[^/].*", RegexOptions.Compiled);
        /// <summary>WindowsUncPathRegex 字段。</summary>
        private static readonly Regex WindowsUncPathRegex = new Regex(
            @"^\\\\[^\\/]+[\\/][^\\/]+.*", RegexOptions.Compiled);
        /// <summary>RelativePathRegex 字段。</summary>
        private static readonly Regex RelativePathRegex = new Regex(
            @"^(?!([a-zA-Z]:[\\/]|\\\\|\/)).+", RegexOptions.Compiled);
        /// <summary>ValidFileNameRegex 字段。</summary>
        private static readonly Regex ValidFileNameRegex = new Regex(
            @"^(?!\.$|\.\.$)(?!.*[\\/:*?""<>|])[^\\/:*?""<>|\0]+$", RegexOptions.Compiled);

        #endregion

        #region 路径验证

        /// <summary>是否绝对路径</summary>
        public static bool IsAbsolutePath(string path) =>
            !string.IsNullOrWhiteSpace(path) &&
            (WindowsAbsolutePathRegex.IsMatch(path) || LinuxAbsolutePathRegex.IsMatch(path));
        /// <summary>是否相对路径</summary>
        public static bool IsRelativePath(string path) =>
            !string.IsNullOrWhiteSpace(path) && RelativePathRegex.IsMatch(path);
        /// <summary>是否 UNC 路径</summary>
        public static bool IsUncPath(string path) =>
            !string.IsNullOrWhiteSpace(path) && WindowsUncPathRegex.IsMatch(path);
        /// <summary>是否有盘符</summary>
        public static bool HasDriveLetter(string path) =>
            !string.IsNullOrWhiteSpace(path) && Regex.IsMatch(path, @"^[a-zA-Z]:[\\/]");
        /// <summary>是否包含非法字符</summary>
        public static bool ContainsInvalidPathChars(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            foreach (char c in path)
                if (WindowsInvalidPathChars.Contains(c) || LinuxInvalidPathChars.Contains(c))
                    return true;
            return false;
        }
        /// <summary>文件名是否有效</summary>
        public static bool IsValidFileName(string fileName) =>
            !string.IsNullOrWhiteSpace(fileName) && ValidFileNameRegex.IsMatch(fileName);
        /// <summary>路径格式是否有效</summary>
        public static bool IsValidPathFormat(string path) =>
            !string.IsNullOrWhiteSpace(path) && !ContainsInvalidPathChars(path) &&
            (IsAbsolutePath(path) || IsRelativePath(path));

        #endregion

        #region 分隔符

        /// <summary>是否混合分隔符</summary>
        public static bool ContainsMixedSeparators(string path) =>
            !string.IsNullOrWhiteSpace(path) && path.Contains('/') && path.Contains('\\');
        /// <summary>是否有错误分隔符</summary>
        public static bool HasIncorrectSeparators(string path, bool targetOs = true)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (targetOs)
                return path.Contains('/') && !path.Contains('\\') && !HasDriveLetter(path) && !IsUncPath(path);
            else
                return path.Contains('\\');
        }
        /// <summary>转为 Windows 分隔符</summary>
        public static string ToWindowsSeparators(string path) =>
            string.IsNullOrWhiteSpace(path) ? path : path.Replace('/', '\\');
        /// <summary>转为 Linux 分隔符</summary>
        public static string ToLinuxSeparators(string path) =>
            string.IsNullOrWhiteSpace(path) ? path : path.Replace('\\', '/');
        /// <summary>规范化分隔符为当前系统</summary>
        public static string NormalizeSeparators(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;
            char desired = Path.DirectorySeparatorChar;
            char undesired = desired == '\\' ? '/' : '\\';
            return path.Replace(undesired, desired);
        }
        /// <summary>分隔符类型描述</summary>
        public static string GetSeparatorType(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return HTranslation.GetContent("无分隔符");
            bool hasSlash = path.Contains('/'), hasBackslash = path.Contains('\\');
            if (hasSlash && hasBackslash) return HTranslation.GetContent("混合分隔符");
            if (hasSlash) return HTranslation.GetContent("Linux分隔符(/)");
            if (hasBackslash) return HTranslation.GetContent("Windows分隔符(\\)");
            return HTranslation.GetContent("无分隔符");
        }

        #endregion

        #region 存在性

        /// <summary>文件是否存在</summary>
        public static bool FileExists(string filePath)
        { try { return File.Exists(filePath); } catch { return false; } }
        /// <summary>目录是否存在</summary>
        public static bool DirectoryExists(string dirPath)
        { try { return Directory.Exists(dirPath); } catch { return false; } }
        /// <summary>路径是否存在（文件/目录）</summary>
        public static bool PathExists(string path) => FileExists(path) || DirectoryExists(path);

        #endregion

        #region 文件元数据

        /// <summary>创建时间</summary>
        public static DateTime? GetCreationTime(string filePath)
        { try { if (FileExists(filePath)) return File.GetCreationTime(filePath); } catch { } return null; }
        /// <summary>最后访问时间</summary>
        public static DateTime? GetLastAccessTime(string filePath)
        { try { if (FileExists(filePath)) return File.GetLastAccessTime(filePath); } catch { } return null; }
        /// <summary>最后写入时间</summary>
        public static DateTime? GetLastWriteTime(string filePath)
        { try { if (FileExists(filePath)) return File.GetLastWriteTime(filePath); } catch { } return null; }
        /// <summary>文件大小(字节)</summary>
        public static long GetFileSize(string filePath)
        { try { if (FileExists(filePath)) return new FileInfo(filePath).Length; } catch { } return -1; }
        /// <summary>文件大小(友好格式)</summary>
        public static string GetFileSizeFriendly(string filePath)
        {
            long bytes = GetFileSize(filePath);
            return bytes < 0 ? HTranslation.GetContent("未知大小") : BytesToFriendlySize(bytes);
        }
        /// <summary>字节转友好大小</summary>
        public static string BytesToFriendlySize(long bytes)
        {
            if (bytes < 0) return HTranslation.GetContent("未知大小");
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return $"{kb:F1} KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return $"{mb:F2} MB";
            return $"{mb / 1024.0:F2} GB";
        }
        /// <summary>是否只读</summary>
        public static bool IsFileReadOnly(string filePath)
        {
            try { if (FileExists(filePath)) return (File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly; }
            catch { }
            return false;
        }
        /// <summary>文件属性描述</summary>
        public static string GetFileAttributesDescription(string filePath)
        {
            try
            {
                if (!FileExists(filePath)) return HTranslation.GetContent("文件不存在");
                FileAttributes attr = File.GetAttributes(filePath);
                var descs = new List<string>();
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) descs.Add(HTranslation.GetContent("只读"));
                if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden) descs.Add(HTranslation.GetContent("隐藏"));
                if ((attr & FileAttributes.System) == FileAttributes.System) descs.Add(HTranslation.GetContent("系统"));
                if ((attr & FileAttributes.Archive) == FileAttributes.Archive) descs.Add(HTranslation.GetContent("存档"));
                if ((attr & FileAttributes.Compressed) == FileAttributes.Compressed) descs.Add(HTranslation.GetContent("压缩"));
                if ((attr & FileAttributes.Encrypted) == FileAttributes.Encrypted) descs.Add(HTranslation.GetContent("加密"));
                if (descs.Count == 0) descs.Add(HTranslation.GetContent("普通"));
                return string.Join("、", descs);
            }
            catch { return HTranslation.GetContent("获取属性失败"); }
        }
        /// <summary>扩展名(无点)</summary>
        public static string GetExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            string ext = Path.GetExtension(path);
            return string.IsNullOrEmpty(ext) ? "" : ext.TrimStart('.');
        }
        /// <summary>文件类型描述(翻译)</summary>
        public static string GetFileTypeDescription(string filePath)
        {
            string ext = GetExtension(filePath);
            return string.IsNullOrEmpty(ext) ? HTranslation.GetContent("未知类型") : HTranslation.GetContent(ext.ToUpperInvariant());
        }
        /// <summary>文件综合摘要</summary>
        public static string GetFileInfoSummary(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return HTranslation.GetContent("路径为空");
            if (!FileExists(filePath)) return string.Format(HTranslation.GetContent("文件“{0}”不存在"), filePath);
            string name = Path.GetFileName(filePath);
            string size = GetFileSizeFriendly(filePath);
            DateTime? create = GetCreationTime(filePath);
            DateTime? modify = GetLastWriteTime(filePath);
            string attr = GetFileAttributesDescription(filePath);
            string type = GetFileTypeDescription(filePath);

            string info = string.Format(HTranslation.GetContent("文件名：{0}"), name) +
                          string.Format(HTranslation.GetContent("，大小：{0}"), size) +
                          string.Format(HTranslation.GetContent("，类型：{0}"), type);
            if (create.HasValue)
                info += string.Format(HTranslation.GetContent("，创建时间：{0}"), create.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            if (modify.HasValue)
                info += string.Format(HTranslation.GetContent("，修改时间：{0}"), modify.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            info += string.Format(HTranslation.GetContent("，属性：{0}"), attr);
            return info;
        }

        #endregion

        #region 文件操作

        /// <summary>复制文件</summary>
        public static bool CopyFile(string source, string dest, bool overwrite = true)
        { try { File.Copy(source, dest, overwrite); return true; } catch { return false; } }
        /// <summary>移动文件（可选覆盖）</summary>
        public static bool MoveFile(string source, string dest, bool overwrite = true)
        {
            try
            {
                if (overwrite && FileExists(dest)) File.Delete(dest);
                File.Move(source, dest);
                return true;
            }
            catch { return false; }
        }
        /// <summary>安全移动文件（自动创建目标目录）</summary>
        public static bool SafeMoveFile(string source, string dest, bool overwrite = true)
        {
            try
            {
                string dir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(dir) && !DirectoryExists(dir))
                    Directory.CreateDirectory(dir);
                return MoveFile(source, dest, overwrite);
            }
            catch { return false; }
        }
        /// <summary>删除文件</summary>
        public static bool DeleteFile(string filePath)
        { try { File.Delete(filePath); return true; } catch { return false; } }
        /// <summary>重命名文件（同目录）</summary>
        public static bool RenameFile(string oldPath, string newName)
        {
            try
            {
                string dir = Path.GetDirectoryName(oldPath);
                string newFull = Path.Combine(dir ?? "", newName);
                File.Move(oldPath, newFull);
                return true;
            }
            catch { return false; }
        }
        /// <summary>修改文件扩展名</summary>
        public static bool ChangeExtension(string filePath, string newExtension)
        {
            try
            {
                string newPath = Path.ChangeExtension(filePath, newExtension);
                File.Move(filePath, newPath);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 文件比较

        /// <summary>比较两个文件内容是否相同（逐字节比较）</summary>
        public static bool FilesAreEqual(string filePath1, string filePath2)
        {
            try
            {
                if (!FileExists(filePath1) || !FileExists(filePath2)) return false;
                if (new FileInfo(filePath1).Length != new FileInfo(filePath2).Length) return false;

                using (var fs1 = File.OpenRead(filePath1))
                using (var fs2 = File.OpenRead(filePath2))
                {
                    int b1, b2;
                    do
                    {
                        b1 = fs1.ReadByte();
                        b2 = fs2.ReadByte();
                        if (b1 != b2) return false;
                    } while (b1 != -1 && b2 != -1);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>比较两个文件的 MD5 是否相同</summary>
        public static bool FilesAreEqualByHash(string filePath1, string filePath2)
        {
            string h1 = ComputeMD5(filePath1);
            string h2 = ComputeMD5(filePath2);
            return h1 != null && h2 != null && string.Equals(h1, h2, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 文件读写

        /// <summary>读取全部文本（默认 UTF8）</summary>
        public static string ReadAllText(string filePath, Encoding encoding = null)
        { try { return File.ReadAllText(filePath, encoding ?? Encoding.UTF8); } catch { return null; } }
        /// <summary>写入全部文本（支持追加）</summary>
        public static bool WriteAllText(string filePath, string content, bool append = false, Encoding encoding = null)
        {
            try
            {
                var enc = encoding ?? Encoding.UTF8;
                if (append) File.AppendAllText(filePath, content, enc);
                else File.WriteAllText(filePath, content, enc);
                return true;
            }
            catch { return false; }
        }
        /// <summary>读取全部字节</summary>
        public static byte[] ReadAllBytes(string filePath)
        { try { return File.ReadAllBytes(filePath); } catch { return null; } }
        /// <summary>写入全部字节</summary>
        public static bool WriteAllBytes(string filePath, byte[] bytes)
        { try { File.WriteAllBytes(filePath, bytes); return true; } catch { return false; } }
        /// <summary>读取所有行</summary>
        public static string[] ReadAllLines(string filePath, Encoding encoding = null)
        { try { return File.ReadAllLines(filePath, encoding ?? Encoding.UTF8); } catch { return null; } }
        /// <summary>打开读取流</summary>
        public static FileStream OpenRead(string filePath)
        { try { return File.OpenRead(filePath); } catch { return null; } }
        /// <summary>打开写入流</summary>
        public static FileStream OpenWrite(string filePath)
        { try { return File.OpenWrite(filePath); } catch { return null; } }
        /// <summary>文件流拷贝（从源文件到目标文件）</summary>
        public static bool CopyStreamToFile(Stream source, string destFilePath, bool overwrite = true)
        {
            try
            {
                using (var destStream = File.Open(destFilePath, overwrite ? FileMode.Create : FileMode.CreateNew))
                    source.CopyTo(destStream);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 临时文件

        /// <summary>创建临时文件并返回路径（文件名随机，扩展名可选）</summary>
        public static string CreateTempFile(string extension = null)
        {
            try
            {
                string tempPath = Path.GetTempFileName();
                if (!string.IsNullOrEmpty(extension))
                {
                    string newPath = Path.ChangeExtension(tempPath, extension.StartsWith(".") ? extension : "." + extension);
                    File.Move(tempPath, newPath);
                    tempPath = newPath;
                }
                return tempPath;
            }
            catch { return null; }
        }

        /// <summary>创建临时文件并写入内容，返回路径</summary>
        public static string CreateTempFileWithContent(string content, Encoding encoding = null)
        {
            try
            {
                string temp = CreateTempFile();
                WriteAllText(temp, content, false, encoding);
                return temp;
            }
            catch { return null; }
        }

        #endregion

        #region 目录操作

        /// <summary>创建目录</summary>
        public static bool CreateDirectory(string dirPath)
        { try { Directory.CreateDirectory(dirPath); return true; } catch { return false; } }
        /// <summary>删除目录（递归）</summary>
        public static bool DeleteDirectory(string dirPath, bool recursive = true)
        { try { Directory.Delete(dirPath, recursive); return true; } catch { return false; } }
        /// <summary>确保目录存在，否则创建</summary>
        public static bool EnsureDirectoryExists(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath)) return false;
            try
            {
                if (!DirectoryExists(dirPath))
                    Directory.CreateDirectory(dirPath);
                return true;
            }
            catch { return false; }
        }
        /// <summary>获取文件列表</summary>
        public static string[] GetFiles(string dirPath, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        { try { return Directory.GetFiles(dirPath, searchPattern, searchOption); } catch { return null; } }
        /// <summary>获取目录列表</summary>
        public static string[] GetDirectories(string dirPath, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        { try { return Directory.GetDirectories(dirPath, searchPattern, searchOption); } catch { return null; } }
        /// <summary>递归获取所有文件（安全，返回完整路径）</summary>
        public static string[] GetAllFilesRecursive(string dirPath)
        {
            try
            {
                if (!DirectoryExists(dirPath)) return new string[0];
                return Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);
            }
            catch { return new string[0]; }
        }
        /// <summary>递归获取所有目录</summary>
        public static string[] GetAllDirectoriesRecursive(string dirPath)
        {
            try
            {
                if (!DirectoryExists(dirPath)) return new string[0];
                return Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories);
            }
            catch { return new string[0]; }
        }

        #endregion

        #region 编码检测

        /// <summary>通过 BOM 检测文件编码</summary>
        public static Encoding DetectEncoding(string filePath)
        {
            try
            {
                if (!FileExists(filePath)) return Encoding.UTF8;
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var bom = new byte[4];
                    int read = fs.Read(bom, 0, 4);
                    if (read >= 2)
                    {
                        if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode;
                        if (bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;
                    }
                    if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                        return Encoding.UTF8;
                    if (read >= 4 && bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
                        return Encoding.UTF32;
                }
                return Encoding.UTF8;
            }
            catch { return Encoding.UTF8; }
        }

        #endregion

        #region 哈希计算

        /// <summary>MD5</summary>
        public static string ComputeMD5(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch { return null; }
        }
        /// <summary>SHA1</summary>
        public static string ComputeSHA1(string filePath)
        {
            try
            {
                using (var sha1 = SHA1.Create())
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] hash = sha1.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch { return null; }
        }
        /// <summary>SHA256</summary>
        public static string ComputeSHA256(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch { return null; }
        }

        #endregion

        #region 路径计算

        /// <summary>安全拼接路径</summary>
        public static string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return string.Empty;
            return Path.Combine(paths);
        }
        /// <summary>拼接并检查文件存在</summary>
        public static bool CombineAndCheckFileExists(params string[] paths)
        {
            if (paths == null || paths.Length == 0) return false;
            return FileExists(CombinePath(paths));
        }
        /// <summary>获取目录名</summary>
        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        /// <summary>获取文件名</summary>
        public static string GetFileName(string path) => Path.GetFileName(path);
        /// <summary>获取文件名（无扩展名）</summary>
        public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
        /// <summary>获取完整路径</summary>
        public static string GetFullPath(string path) => Path.GetFullPath(path);
        /// <summary>计算两个绝对路径之间的相对路径</summary>
        public static string GetRelativePath(string fromAbsolute, string toAbsolute)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fromAbsolute) || string.IsNullOrWhiteSpace(toAbsolute))
                    return toAbsolute;
                Uri fromUri = new Uri(fromAbsolute);
                Uri toUri = new Uri(toAbsolute);
                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                return Uri.UnescapeDataString(relativeUri.ToString());
            }
            catch { return toAbsolute; }
        }

        #endregion

        #region 系统特殊目录

        /// <summary>获取桌面路径</summary>
        public static string GetDesktopPath() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        /// <summary>获取我的文档路径</summary>
        public static string GetMyDocumentsPath() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        /// <summary>获取程序数据路径</summary>
        public static string GetAppDataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        /// <summary>获取临时目录路径</summary>
        public static string GetTempPath() => Path.GetTempPath();

        #endregion

        #region 驱动器信息

        /// <summary>获取驱动器总容量（字节），失败返回 -1</summary>
        public static long GetDriveTotalSize(string driveName)
        {
            try
            {
                var drive = new DriveInfo(driveName);
                if (drive.IsReady) return drive.TotalSize;
            }
            catch { }
            return -1;
        }
        /// <summary>获取驱动器可用空间（字节），失败返回 -1</summary>
        public static long GetDriveAvailableFreeSpace(string driveName)
        {
            try
            {
                var drive = new DriveInfo(driveName);
                if (drive.IsReady) return drive.AvailableFreeSpace;
            }
            catch { }
            return -1;
        }

        #endregion

        #region 通配符匹配

        /// <summary>判断文件名是否匹配指定通配符模式（* 和 ?）</summary>
        public static bool MatchesPattern(string input, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return string.IsNullOrEmpty(input);
            // 转换通配符为正则表达式
            string regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            return Regex.IsMatch(input ?? "", regexPattern, RegexOptions.IgnoreCase);
        }

        #endregion

        #region 文件锁定检测

        /// <summary>判断文件是否被其他进程锁定（不可写）</summary>
        public static bool IsFileLocked(string filePath)
        {
            try
            {
                if (!FileExists(filePath)) return false;
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false; // 可以独占打开，说明未被锁定
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 辅助功能列表

        /// <summary>所有支持的方法名称（翻译后）</summary>
        public static List<string> GetSupportedMethods() => new List<string>
        {
            HTranslation.GetContent("基目录与拼接"),
            HTranslation.GetContent("路径验证"),
            HTranslation.GetContent("分隔符处理"),
            HTranslation.GetContent("文件/目录存在性"),
            HTranslation.GetContent("文件元数据"),
            HTranslation.GetContent("文件复制/移动/删除/重命名"),
            HTranslation.GetContent("文件读写（文本/字节/流）"),
            HTranslation.GetContent("文件比较"),
            HTranslation.GetContent("临时文件"),
            HTranslation.GetContent("目录操作（创建/删除/遍历）"),
            HTranslation.GetContent("编码检测"),
            HTranslation.GetContent("哈希计算(MD5/SHA1/SHA256)"),
            HTranslation.GetContent("路径计算(相对路径、名称解析)"),
            HTranslation.GetContent("系统特殊目录"),
            HTranslation.GetContent("驱动器信息"),
            HTranslation.GetContent("通配符匹配"),
            HTranslation.GetContent("文件锁定检测")
        };

        #endregion
    }
}
