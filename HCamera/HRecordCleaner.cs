using System;
using System.IO;
using System.Linq;

namespace HFromUI.HCamera
{
    using HFromUI.HLangage;
    /// <summary>
    /// 录像磁盘维护：按保留天数自动删除旧录像，并在磁盘剩余空间不足时从最旧文件开始清理，保证磁盘不被录满。
    /// 所有方法对单个文件/目录的异常做容错，不会因个别文件占用而中断整体清理。
    /// </summary>
    public static class HRecordCleaner
    {
        /// <summary>
        /// 录像目录维护（建议程序启动时 + 每隔一段时间调用一次）。
        /// </summary>
        /// <param name="rootDir">录像根目录</param>
        /// <param name="retainDays">保留天数：修改时间早于（今天 - 保留天数）的文件删除；&lt;=0 表示不按天数删</param>
        /// <param name="minFreeGB">磁盘最小剩余空间（GB）：低于此值时从最旧文件开始删除，直到满足；&lt;=0 表示不检查</param>
        /// <param name="log">日志回调（可为 null）</param>
        /// <returns>本次释放的字节数</returns>
        public static long Maintenance(string rootDir, int retainDays, double minFreeGB, Action<string> log = null)
        {
            long freed = 0;
            if (string.IsNullOrWhiteSpace(rootDir) || !Directory.Exists(rootDir)) return 0;
            try
            {
                if (retainDays > 0)
                    freed += CleanOldFiles(rootDir, retainDays, log);

                if (minFreeGB > 0)
                    freed += EnsureFreeSpace(rootDir, minFreeGB, log);

                RemoveEmptyDirectories(rootDir, log);
            }
            catch (Exception ex)
            {
                log?.Invoke(HTranslation.GetContent("磁盘维护异常：") + ex.Message);
            }
            return freed;
        }

        /// <summary>删除修改时间早于 cutoff 的所有文件，返回释放字节数。</summary>
        public static long CleanOldFiles(string rootDir, int retainDays, Action<string> log = null)
        {
            long freed = 0;
            DateTime cutoff = DateTime.Now.AddDays(-retainDays);
            FileInfo[] files = SafeGetAllFiles(rootDir);
            foreach (FileInfo f in files)
            {
                try
                {
                    if (f.LastWriteTime < cutoff)
                    {
                        long len = f.Length;
                        f.Delete();
                        freed += len;
                        log?.Invoke(HTranslation.GetContent("已删除过期录像：") + f.FullName);
                    }
                }
                catch (Exception ex)
                {
                    log?.Invoke(HTranslation.GetContent("删除失败（可能被占用）：") + f.FullName + " " + ex.Message);
                }
            }
            if (freed > 0) log?.Invoke(HTranslation.GetContent("按保留天数清理，共释放 ") + FormatBytes(freed));
            return freed;
        }

        /// <summary>
        /// 保证录像所在盘剩余空间不低于 minFreeGB：不足时从最旧的文件开始删除，直到满足或无文件可删。
        /// </summary>
        public static long EnsureFreeSpace(string rootDir, double minFreeGB, Action<string> log = null)
        {
            long freed = 0;
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(rootDir)));
                long needBytes = (long)(minFreeGB * 1024L * 1024L * 1024L);
                if (drive.AvailableFreeSpace >= needBytes) return 0;

                log?.Invoke(HTranslation.GetContent("磁盘剩余空间不足（当前 ") + FormatBytes(drive.AvailableFreeSpace) +
                            HTranslation.GetContent("，要求 ") + minFreeGB + HTranslation.GetContent("GB），开始清理最旧录像..."));

                FileInfo[] files = SafeGetAllFiles(rootDir)
                    .OrderBy(f => f.LastWriteTime)
                    .ToArray();
                foreach (FileInfo f in files)
                {
                    try
                    {
                        drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(rootDir)));
                        if (drive.AvailableFreeSpace >= needBytes) break;
                        long len = f.Length;
                        f.Delete();
                        freed += len;
                        log?.Invoke(HTranslation.GetContent("空间不足，已删除旧录像：") + f.FullName);
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke(HTranslation.GetContent("删除失败（可能被占用）：") + f.FullName + " " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Invoke(HTranslation.GetContent("磁盘空间检查异常：") + ex.Message);
            }
            if (freed > 0) log?.Invoke(HTranslation.GetContent("按磁盘空间清理，共释放 ") + FormatBytes(freed));
            return freed;
        }

        /// <summary>递归删除空目录（清理按日期分层留下的空文件夹）。</summary>
        public static void RemoveEmptyDirectories(string rootDir, Action<string> log = null)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(rootDir))
                {
                    RemoveEmptyDirectories(dir, log);
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            Directory.Delete(dir);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>获取目录所在盘剩余空间（GB），失败返回 -1。</summary>
        public static double GetFreeSpaceGB(string rootDir)
        {
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(rootDir)));
                return drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            }
            catch { return -1; }
        }

        /// <summary>SafeGetAllFiles 方法。</summary>
        private static FileInfo[] SafeGetAllFiles(string rootDir)
        {
            try
            {
                return new DirectoryInfo(rootDir).GetFiles("*", SearchOption.AllDirectories);
            }
            catch
            {
                return new FileInfo[0];
            }
        }

        /// <summary>字节数转可读文本。</summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return kb.ToString("F1") + " KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return mb.ToString("F1") + " MB";
            return (mb / 1024.0).ToString("F2") + " GB";
        }
    }
}
