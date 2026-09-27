using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace HFromUI.HColor
{
    /// <summary>
    /// 全局矢量图片服务：按名称获取控件库内置图片。
    /// 查找顺序：① 进程内全局字典缓存 → ② 程序运行目录下 Photo 文件夹中的 PNG →
    /// ③ 现场用 GDI+ 矢量绘制并保存到 Photo 文件夹，同时放入字典。
    /// 返回的位图为全局共享对象，调用方只使用、不要 Dispose。
    /// </summary>
    public static class HPhoto
    {
        /// <summary>进程内共享的图片字典（名称不区分大小写）。</summary>
        private static readonly ConcurrentDictionary<string, Bitmap> _cache =
            new ConcurrentDictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

        /// <summary>进程内共享的图标字典（名称不区分大小写）。</summary>
        private static readonly ConcurrentDictionary<string, Icon> _iconCache =
            new ConcurrentDictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>图片落地目录：程序所在目录\Photo。</summary>
        public static string PhotoDir
        {
            get { return Path.Combine(HFromUI.HData.HAppData.AppPath, "Photo"); }
        }

        /// <summary>按名称获取图片：先字典、再 Photo 目录文件、最后矢量生成并落盘。</summary>
        public static Bitmap Get(string name)
        {
            return _cache.GetOrAdd(name, LoadOrCreate);
        }

        /// <summary>按名称获取图标：由同名矢量位图转换并缓存，供 Form.Icon 使用。</summary>
        public static Icon GetIcon(string name)
        {
            return _iconCache.GetOrAdd(name, CreateIcon);
        }

        /// <summary>位图转独立图标（释放临时 HICON，返回值自管句柄）。</summary>
        private static Icon CreateIcon(string name)
        {
            IntPtr hIcon = Get(name).GetHicon();
            try
            {
                using (Icon tmp = Icon.FromHandle(hIcon))
                    return (Icon)tmp.Clone();
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        /// <summary>字典没有时：先读 Photo 目录文件，读不到就矢量生成并保存。</summary>
        private static Bitmap LoadOrCreate(string name)
        {
            string file = Path.Combine(PhotoDir, name + ".png");
            if (File.Exists(file))
                return new Bitmap(file);

            Bitmap bmp = HVectorPhoto.Render(name);

            // 首次生成的矢量图落地保存，下次启动可直接从 Photo 目录取图
            try
            {
                Directory.CreateDirectory(PhotoDir);
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                // 目录不可写时仅使用内存位图，不影响控件显示
            }
            return bmp;
        }

        /// <summary>清空字典缓存（不删除 Photo 目录文件），下次获取将重新读取/生成。</summary>
        public static void ClearCache()
        {
            _cache.Clear();
            foreach (Icon icon in _iconCache.Values)
                icon.Dispose();
            _iconCache.Clear();
        }
    }
}
