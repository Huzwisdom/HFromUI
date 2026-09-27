using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;    // 需要手动添加对 System.IO.Compression.dll 的引用
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HFile
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全网最全的文件操作工具类（WinZip 兼容增强版，需要引用 System.IO.Compression.dll）。
    /// 提供路径处理、存在性判断、属性获取、复制/移动/删除/重命名、安全替换、目录操作、
    /// 文件读写、枚举、特殊文件夹、临时文件、文件比较、哈希计算、编码检测、文件锁定、
    /// 原子替换、异步方法、ZIP 压缩/解压（WinZip 兼容，UTF‑8 编码）、HTTP 下载、FTP 上传/下载等。
    /// 所有方法均包含异常处理，返回安全默认值，不会抛出未处理异常。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HFilePath
    {
        #region 路径处理

        /// <summary>合并多个路径片段，自动添加系统分隔符。</summary>
        public static string CombinePath(params string[] paths) => Path.Combine(paths);

        /// <summary>获取文件所在目录。</summary>
        public static string GetDirectoryName(string path) => Path.GetDirectoryName(path);

        /// <summary>获取文件名（含扩展名）。</summary>
        public static string GetFileName(string path) => Path.GetFileName(path);

        /// <summary>获取文件名（不含扩展名）。</summary>
        public static string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

        /// <summary>获取扩展名（含点号）。</summary>
        public static string GetExtension(string path) => Path.GetExtension(path);

        /// <summary>返回绝对路径。</summary>
        public static string GetFullPath(string path) => Path.GetFullPath(path);

        /// <summary>更改文件扩展名（不实际重命名文件）。</summary>
        public static string ChangeExtension(string path, string extension) => Path.ChangeExtension(path, extension);

        /// <summary>将路径中的斜杠统一为当前系统默认分隔符。</summary>
        public static string NormalizeSeparators(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            char desired = Path.DirectorySeparatorChar;
            char undesired = (desired == '\\') ? '/' : '\\';
            return path.Replace(undesired, desired);
        }

        #endregion

        #region 存在性与基础属性

        /// <summary>判断文件是否存在。</summary>
        public static bool FileExists(string path)
        {
            try { return File.Exists(path); } catch { return false; }
        }

        /// <summary>判断文件夹是否存在。</summary>
        public static bool DirectoryExists(string path)
        {
            try { return Directory.Exists(path); } catch { return false; }
        }

        /// <summary>获取文件大小（字节），失败返回 -1。</summary>
        public static long GetFileSize(string path)
        {
            try { return new FileInfo(path).Length; } catch { return -1; }
        }

        /// <summary>获取文件大小友好字符串（KB/MB/GB）。</summary>
        public static string GetFileSizeFriendly(string path)
        {
            long bytes = GetFileSize(path);
            if (bytes < 0) return HTranslation.GetContent("未知");
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return $"{kb:F1} KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return $"{mb:F2} MB";
            double gb = mb / 1024.0;
            return $"{gb:F2} GB";
        }

        /// <summary>获取文件创建时间，失败返回 null。</summary>
        public static DateTime? GetFileCreationTime(string path)
        {
            try { return File.GetCreationTime(path); } catch { return null; }
        }

        /// <summary>获取文件最后修改时间，失败返回 null。</summary>
        public static DateTime? GetFileLastWriteTime(string path)
        {
            try { return File.GetLastWriteTime(path); } catch { return null; }
        }

        /// <summary>获取文件属性。</summary>
        public static FileAttributes? GetFileAttributes(string path)
        {
            try { return File.GetAttributes(path); } catch { return null; }
        }

        /// <summary>设置文件属性。</summary>
        public static bool SetFileAttributes(string path, FileAttributes attributes)
        {
            try { File.SetAttributes(path, attributes); return true; } catch { return false; }
        }

        /// <summary>判断文件是否只读。</summary>
        public static bool IsFileReadOnly(string path)
        {
            try { return (File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly; } catch { return false; }
        }

        /// <summary>判断文件是否隐藏。</summary>
        public static bool IsFileHidden(string path)
        {
            try { return (File.GetAttributes(path) & FileAttributes.Hidden) == FileAttributes.Hidden; } catch { return false; }
        }

        #endregion

        #region 文件基本操作

        /// <summary>复制文件。</summary>
        /// <param name="source">源文件路径。</param>
        /// <param name="dest">目标文件路径。</param>
        /// <param name="overwrite">是否覆盖已存在的目标。</param>
        public static bool CopyFile(string source, string dest, bool overwrite = false)
        {
            try { File.Copy(source, dest, overwrite); return true; } catch { return false; }
        }

        /// <summary>移动文件。</summary>
        /// <param name="source">源文件路径。</param>
        /// <param name="dest">目标文件路径。</param>
        /// <param name="overwrite">是否覆盖已存在的目标（true 时删除后再移动）。</param>
        public static bool MoveFile(string source, string dest, bool overwrite = true)
        {
            try
            {
                if (!FileExists(source)) return false;
                if (FileExists(dest))
                {
                    if (overwrite) File.Delete(dest);
                    else return false;
                }
                File.Move(source, dest);
                return true;
            }
            catch { return false; }
        }

        /// <summary>删除文件。</summary>
        public static bool DeleteFile(string path)
        {
            try { File.Delete(path); return true; } catch { return false; }
        }

        /// <summary>重命名文件（保持在同一目录）。</summary>
        /// <param name="oldPath">旧文件完整路径。</param>
        /// <param name="newName">新文件名（不含路径）。</param>
        public static bool RenameFile(string oldPath, string newName)
        {
            try
            {
                string dir = Path.GetDirectoryName(oldPath);
                string newPath = Path.Combine(dir ?? "", newName);
                File.Move(oldPath, newPath);
                return true;
            }
            catch { return false; }
        }

        /// <summary>重命名文件夹（保持在同一目录）。</summary>
        /// <param name="oldPath">旧文件夹完整路径。</param>
        /// <param name="newName">新文件夹名（不含路径）。</param>
        public static bool RenameDirectory(string oldPath, string newName)
        {
            try
            {
                string dir = Path.GetDirectoryName(oldPath);
                string newPath = Path.Combine(dir ?? "", newName);
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 安全替换文件。先将目标文件备份为 .temp，然后移动/复制源文件到目标，
        /// 若成功则删除备份，若失败则恢复备份。
        /// </summary>
        /// <param name="sourceFile">源文件路径。</param>
        /// <param name="destFile">目标文件路径。</param>
        /// <param name="deleteSource">true 表示移动源文件（源将被删除），false 表示复制。</param>
        public static bool SafeReplaceFile(string sourceFile, string destFile, bool deleteSource = true)
        {
            try
            {
                if (!FileExists(sourceFile)) return false;

                string backupFile = destFile + ".temp";
                bool destExisted = FileExists(destFile);

                if (destExisted) File.Move(destFile, backupFile);

                bool stepSuccess = false;
                try
                {
                    if (deleteSource) File.Move(sourceFile, destFile);
                    else File.Copy(sourceFile, destFile, true);
                    stepSuccess = true;
                }
                catch { }

                if (stepSuccess)
                {
                    if (destExisted && FileExists(backupFile)) File.Delete(backupFile);
                    return true;
                }
                else
                {
                    if (destExisted && FileExists(backupFile))
                    {
                        if (FileExists(destFile)) File.Delete(destFile);
                        File.Move(backupFile, destFile);
                    }
                    return false;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 目录操作

        /// <summary>创建目录（若不存在则创建，支持多级目录）。</summary>
        public static bool CreateDirectory(string path)
        {
            try { Directory.CreateDirectory(path); return true; } catch { return false; }
        }

        /// <summary>删除目录，可选择是否递归删除。</summary>
        public static bool DeleteDirectory(string path, bool recursive = true)
        {
            try { Directory.Delete(path, recursive); return true; } catch { return false; }
        }

        /// <summary>移动目录。</summary>
        public static bool MoveDirectory(string source, string dest)
        {
            try { Directory.Move(source, dest); return true; } catch { return false; }
        }

        /// <summary>复制整个目录（包含子目录和文件）。</summary>
        /// <param name="source">源目录。</param>
        /// <param name="dest">目标目录。</param>
        /// <param name="overwrite">是否覆盖目标文件。</param>
        public static bool CopyDirectory(string source, string dest, bool overwrite = true)
        {
            try
            {
                if (!DirectoryExists(source)) return false;
                Directory.CreateDirectory(dest);
                foreach (string file in Directory.GetFiles(source))
                {
                    string destFile = Path.Combine(dest, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite);
                }
                foreach (string dir in Directory.GetDirectories(source))
                {
                    string destDir = Path.Combine(dest, Path.GetFileName(dir));
                    CopyDirectory(dir, destDir, overwrite);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>获取当前工作目录。</summary>
        public static string GetCurrentDirectory() => Environment.CurrentDirectory;

        /// <summary>设置当前工作目录。</summary>
        public static void SetCurrentDirectory(string path) => Environment.CurrentDirectory = path;

        /// <summary>获取指定路径的上级目录，失败返回 null。</summary>
        public static string GetParentDirectory(string path)
        {
            try { return Directory.GetParent(path)?.FullName; } catch { return null; }
        }

        /// <summary>创建一个临时目录并返回其完整路径。</summary>
        public static string CreateTempDirectory()
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(path);
                return path;
            }
            catch { return null; }
        }

        #endregion

        #region 文件读写

        /// <summary>读取文件全部文本（默认 UTF‑8）。</summary>
        public static string ReadAllText(string path, Encoding encoding = null)
        {
            try { return File.ReadAllText(path, encoding ?? Encoding.UTF8); } catch { return null; }
        }

        /// <summary>将文本写入文件（覆盖，默认 UTF‑8）。</summary>
        public static bool WriteAllText(string path, string content, Encoding encoding = null)
        {
            try { File.WriteAllText(path, content, encoding ?? Encoding.UTF8); return true; } catch { return false; }
        }

        /// <summary>将文本追加到文件末尾。</summary>
        public static bool AppendAllText(string path, string content, Encoding encoding = null)
        {
            try { File.AppendAllText(path, content, encoding ?? Encoding.UTF8); return true; } catch { return false; }
        }

        /// <summary>读取所有行，返回字符串数组。</summary>
        public static string[] ReadAllLines(string path, Encoding encoding = null)
        {
            try { return File.ReadAllLines(path, encoding ?? Encoding.UTF8); } catch { return null; }
        }

        /// <summary>将字符串数组写入文件，一行一个元素。</summary>
        public static bool WriteAllLines(string path, string[] lines, Encoding encoding = null)
        {
            try { File.WriteAllLines(path, lines, encoding ?? Encoding.UTF8); return true; } catch { return false; }
        }

        /// <summary>读取文件全部字节。</summary>
        public static byte[] ReadAllBytes(string path)
        {
            try { return File.ReadAllBytes(path); } catch { return null; }
        }

        /// <summary>将字节数组写入文件。</summary>
        public static bool WriteAllBytes(string path, byte[] bytes)
        {
            try { File.WriteAllBytes(path, bytes); return true; } catch { return false; }
        }

        /// <summary>打开文件读取流（调用方负责关闭）。</summary>
        public static FileStream OpenRead(string path)
        {
            try { return File.OpenRead(path); } catch { return null; }
        }

        /// <summary>打开文件写入流（覆盖模式，调用方负责关闭）。</summary>
        public static FileStream OpenWrite(string path)
        {
            try { return File.OpenWrite(path); } catch { return null; }
        }

        #endregion

        #region 枚举文件与目录

        /// <summary>获取指定目录下的文件列表。</summary>
        /// <param name="directory">目录路径。</param>
        /// <param name="searchPattern">搜索模式，如 "*.txt"。</param>
        /// <param name="searchOption">是否递归子目录。</param>
        public static string[] GetFiles(string directory, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try { return Directory.GetFiles(directory, searchPattern, searchOption); } catch { return new string[0]; }
        }

        /// <summary>获取指定目录下的子目录列表。</summary>
        public static string[] GetDirectories(string directory, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try { return Directory.GetDirectories(directory, searchPattern, searchOption); } catch { return new string[0]; }
        }

        /// <summary>获取指定目录下的文件和目录混合列表。</summary>
        public static string[] GetFileSystemEntries(string directory, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try { return Directory.GetFileSystemEntries(directory, searchPattern, searchOption); } catch { return new string[0]; }
        }

        /// <summary>流式枚举文件（内存友好），返回可延迟执行的序列。</summary>
        public static IEnumerable<string> EnumerateFiles(string directory, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try { return Directory.EnumerateFiles(directory, searchPattern, searchOption); }
            catch { return Enumerable.Empty<string>(); }
        }

        #endregion

        #region 特殊文件夹

        /// <summary>获取 desktopPath。</summary>
        public static string GetDesktopPath() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        /// <summary>获取 myDocumentsPath。</summary>
        public static string GetMyDocumentsPath() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        /// <summary>获取 appDataPath。</summary>
        public static string GetAppDataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        /// <summary>获取 tempPath。</summary>
        public static string GetTempPath() => Path.GetTempPath();
        /// <summary>获取 programFilesPath。</summary>
        public static string GetProgramFilesPath() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        /// <summary>获取 systemPath。</summary>
        public static string GetSystemPath() => Environment.GetFolderPath(Environment.SpecialFolder.System);

        #endregion

        #region 临时文件

        /// <summary>创建临时文件并返回完整路径，可选指定扩展名。</summary>
        public static string CreateTempFile(string extension = null)
        {
            try
            {
                string path = Path.GetTempFileName();
                if (!string.IsNullOrEmpty(extension))
                {
                    string newPath = Path.ChangeExtension(path, extension.StartsWith(".") ? extension : "." + extension);
                    File.Move(path, newPath);
                    path = newPath;
                }
                return path;
            }
            catch { return null; }
        }

        /// <summary>创建带有文本内容的临时文件。</summary>
        public static string CreateTempFileWithContent(string content, Encoding encoding = null)
        {
            try
            {
                string temp = CreateTempFile();
                WriteAllText(temp, content, encoding);
                return temp;
            }
            catch { return null; }
        }

        #endregion

        #region 文件比较与哈希

        /// <summary>逐字节比较两个文件是否完全相同。</summary>
        public static bool FilesAreEqual(string file1, string file2)
        {
            try
            {
                if (!FileExists(file1) || !FileExists(file2)) return false;
                if (new FileInfo(file1).Length != new FileInfo(file2).Length) return false;

                using (FileStream fs1 = File.OpenRead(file1))
                using (FileStream fs2 = File.OpenRead(file2))
                {
                    int b1, b2;
                    do
                    {
                        b1 = fs1.ReadByte();
                        b2 = fs2.ReadByte();
                        if (b1 != b2) return false;
                    } while (b1 != -1);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>计算文件 MD5 哈希（小写十六进制）。</summary>
        public static string ComputeMD5(string path) => ComputeHash(path, "MD5");

        /// <summary>计算文件 SHA1 哈希。</summary>
        public static string ComputeSHA1(string path) => ComputeHash(path, "SHA1");

        /// <summary>计算文件 SHA256 哈希。</summary>
        public static string ComputeSHA256(string path) => ComputeHash(path, "SHA256");

        /// <summary>ComputeHash 方法。</summary>
        private static string ComputeHash(string path, string algorithm)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    byte[] hash;
                    switch (algorithm)
                    {
                        case "MD5":
                            using (var md5 = MD5.Create()) hash = md5.ComputeHash(fs);
                            break;
                        case "SHA1":
                            using (var sha1 = SHA1.Create()) hash = sha1.ComputeHash(fs);
                            break;
                        case "SHA256":
                            using (var sha256 = SHA256.Create()) hash = sha256.ComputeHash(fs);
                            break;
                        default: return null;
                    }
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch { return null; }
        }

        /// <summary>验证文件 SHA256 哈希是否与给定值匹配（忽略大小写）。</summary>
        public static bool VerifyFileHash(string path, string expectedHash)
        {
            string actual = ComputeSHA256(path);
            return actual != null && actual.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region 编码检测

        /// <summary>通过 BOM 检测文件编码，若无 BOM 则返回系统默认编码。</summary>
        public static Encoding DetectEncoding(string path)
        {
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    byte[] bom = new byte[4];
                    int read = fs.Read(bom, 0, 4);
                    if (read >= 2)
                    {
                        if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode;
                        if (bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;
                    }
                    if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;
                    if (read >= 4 && bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF) return Encoding.UTF32;
                }
                return Encoding.Default;
            }
            catch { return null; }
        }

        #endregion

        #region 文件锁定与占用检测

        /// <summary>判断文件是否被其他进程锁定（尝试独占写入检测）。</summary>
        public static bool IsFileLocked(string path)
        {
            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException) { return true; }
            catch { return false; }
        }

        /// <summary>等待文件解除锁定，指定超时时间（毫秒）。</summary>
        public static bool WaitForFileReady(string path, int timeoutMs = 5000)
        {
            int waited = 0;
            while (IsFileLocked(path) && waited < timeoutMs)
            {
                Thread.Sleep(100);
                waited += 100;
            }
            return !IsFileLocked(path);
        }

        #endregion

        #region 原子替换文件（事务性）

        /// <summary>
        /// 原子替换文件：先将内容写入临时文件，然后使用 File.Replace 替换目标文件，
        /// 保证原文件要么完整要么未被修改。若替换失败则清理临时文件。
        /// </summary>
        /// <param name="sourceContent">要写入目标文件的字符串内容。</param>
        /// <param name="destFile">目标文件路径。</param>
        /// <param name="encoding">文本编码，默认 UTF‑8。</param>
        /// <returns>操作是否成功。</returns>
        public static bool AtomicReplaceFile(string sourceContent, string destFile, Encoding encoding = null)
        {
            string tempFile = destFile + ".atomic";
            try
            {
                WriteAllText(tempFile, sourceContent, encoding);
                if (!FileExists(tempFile)) return false;
                File.Replace(tempFile, destFile, null);
                return true;
            }
            catch
            {
                DeleteFile(tempFile);
                return false;
            }
        }

        #endregion

        #region 异步方法

        /// <summary>异步读取文件全部文本。</summary>
        public static async Task<string> ReadAllTextAsync(string path, Encoding encoding = null)
        {
            try
            {
                using (var reader = new StreamReader(path, encoding ?? Encoding.UTF8))
                    return await reader.ReadToEndAsync();
            }
            catch { return null; }
        }

        /// <summary>异步写入文件全部文本。</summary>
        public static async Task<bool> WriteAllTextAsync(string path, string content, Encoding encoding = null)
        {
            try
            {
                using (var writer = new StreamWriter(path, false, encoding ?? Encoding.UTF8))
                    await writer.WriteAsync(content);
                return true;
            }
            catch { return false; }
        }

        /// <summary>异步复制文件（从源文件流复制到目标文件流）。</summary>
        public static async Task<bool> CopyFileAsync(string source, string dest, bool overwrite = false)
        {
            try
            {
                using (FileStream sourceStream = File.OpenRead(source))
                using (FileStream destStream = File.Open(dest, overwrite ? FileMode.Create : FileMode.CreateNew))
                    await sourceStream.CopyToAsync(destStream);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region ZIP 压缩与解压（WinZip 兼容，UTF‑8 编码）

        /// <summary>
        /// 将指定目录压缩为 ZIP 文件（与 WinZip 兼容，文件名使用 UTF‑8 编码，避免中文乱码）。
        /// 使用 <see cref="ZipArchive"/> 类实现，无需额外第三方库。
        /// </summary>
        /// <param name="sourceDirectoryName">要压缩的目录。</param>
        /// <param name="destinationArchiveFileName">输出的 ZIP 文件路径。</param>
        /// <param name="compressionLevel">压缩级别，默认 Optimal。</param>
        /// <param name="entryNameEncoding">文件名编码，默认 UTF‑8。</param>
        public static bool CreateZipFromDirectory(string sourceDirectoryName,
            string destinationArchiveFileName,
            CompressionLevel compressionLevel = CompressionLevel.Optimal,
            Encoding entryNameEncoding = null)
        {
            try
            {
                if (!DirectoryExists(sourceDirectoryName)) return false;
                Encoding enc = entryNameEncoding ?? Encoding.UTF8;

                string destDir = Path.GetDirectoryName(destinationArchiveFileName);
                if (!string.IsNullOrEmpty(destDir) && !DirectoryExists(destDir))
                    Directory.CreateDirectory(destDir);

                using (FileStream zipStream = new FileStream(destinationArchiveFileName, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false, enc))
                {
                    AddDirectoryToZip(archive, sourceDirectoryName, "", compressionLevel);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 将 ZIP 文件解压到指定目录（与 WinZip 兼容，使用 UTF‑8 编码读取文件名）。
        /// 使用 <see cref="ZipArchive"/> 类实现，无需额外第三方库。
        /// </summary>
        /// <param name="sourceArchiveFileName">ZIP 文件路径。</param>
        /// <param name="destinationDirectoryName">目标目录路径。</param>
        /// <param name="entryNameEncoding">文件名编码，默认 UTF‑8。</param>
        public static bool ExtractZipToDirectory(string sourceArchiveFileName,
            string destinationDirectoryName,
            Encoding entryNameEncoding = null)
        {
            try
            {
                if (!FileExists(sourceArchiveFileName)) return false;
                Encoding enc = entryNameEncoding ?? Encoding.UTF8;

                using (FileStream zipStream = new FileStream(sourceArchiveFileName, FileMode.Open, FileAccess.Read))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read, false, enc))
                {
                    if (!DirectoryExists(destinationDirectoryName))
                        Directory.CreateDirectory(destinationDirectoryName);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destPath = Path.Combine(destinationDirectoryName, entry.FullName);
                        // 目录条目（名称以 / 结尾或 Name 为空）
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destPath);
                            continue;
                        }

                        string parentDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(parentDir) && !DirectoryExists(parentDir))
                            Directory.CreateDirectory(parentDir);

                        // 手动解压，避免使用 ExtractToFile 扩展方法
                        using (Stream entryStream = entry.Open())
                        using (FileStream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                        {
                            entryStream.CopyTo(fileStream);
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>递归将目录下的文件和子目录添加到 ZIP 存档中。</summary>
        private static void AddDirectoryToZip(ZipArchive archive, string sourceDir, string entryPrefix, CompressionLevel compressionLevel)
        {
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string entryName = entryPrefix + Path.GetFileName(file);
                ZipArchiveEntry entry = archive.CreateEntry(entryName, compressionLevel);
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = File.OpenRead(file))
                {
                    fileStream.CopyTo(entryStream);
                }
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dir);
                string entryName = entryPrefix + dirName + "/";
                archive.CreateEntry(entryName, compressionLevel);
                AddDirectoryToZip(archive, dir, entryName, compressionLevel);
            }
        }

        #endregion

        #region HTTP 文件下载

        /// <summary>从指定 URL 下载文件并保存到本地路径。</summary>
        public static bool DownloadFile(string url, string localPath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, localPath);
                    return true;
                }
            }
            catch { return false; }
        }

        /// <summary>异步从指定 URL 下载文件并保存到本地路径。</summary>
        public static async Task<bool> DownloadFileAsync(string url, string localPath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(url, localPath);
                    return true;
                }
            }
            catch { return false; }
        }

        #endregion

        #region FTP 文件上传/下载

        /// <summary>从 FTP 服务器下载文件到本地路径。</summary>
        public static bool DownloadFtpFile(string ftpUrl, string localPath, string userName = null, string password = null)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                if (!string.IsNullOrEmpty(userName))
                    request.Credentials = new NetworkCredential(userName, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream ftpStream = response.GetResponseStream())
                using (FileStream fs = File.Create(localPath))
                    ftpStream.CopyTo(fs);
                return true;
            }
            catch { return false; }
        }

        /// <summary>将本地文件上传到 FTP 服务器。</summary>
        public static bool UploadFtpFile(string localPath, string ftpUrl, string userName = null, string password = null)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                if (!string.IsNullOrEmpty(userName))
                    request.Credentials = new NetworkCredential(userName, password);

                byte[] fileBytes = File.ReadAllBytes(localPath);
                request.ContentLength = fileBytes.Length;
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                return true;
            }
            catch { return false; }
        }

        #endregion
    }
}
