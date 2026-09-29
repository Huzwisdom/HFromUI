using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HFromUI.HSocket.DeviceID;
using HFromUI.HSocket.HFileTransfer;

namespace HFromUI.HSocket.HFtp
{
    /// <summary>
    /// FTP/FTPS 客户端（以 HFtpAddress 作为连接地址）：上传/下载大文件并支持断点续传，
    /// 另提供目录列举、大小查询、删除、建目录、重命名等常用操作。
    /// <para>续传机制：下载使用 REST 偏移 + 本地 *.hftpart 分片；上传查询远端文件大小后用 APPE 追加。</para>
    /// <para>所有方法为同步阻塞调用，另提供 Async 版本；进度/错误回调在后台线程触发，更新 UI 需 Invoke。</para>
    /// </summary>
    public class HFtpClient : IDisposable
    {
        private FtpWebRequest mActive;
        private volatile bool mCancel;

        /// <summary>FTP 连接地址（主机/端口/账号/SSL/根目录）。</summary>
        public HFtpAddress hFtpAddress { set; get; }

        /// <summary>读写分块大小（字节），默认 64KB。</summary>
        public int ChunkSize { set; get; } = 64 * 1024;

        /// <summary>请求超时（毫秒），默认 30 秒。</summary>
        public int Timeout { set; get; } = 30000;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否处于已登录可用状态（FTP 为无状态协议，此处记录最近一次登录验证结果）。</summary>
        public bool IsConnected { get; private set; }

        /// <summary>传输进度变化（后台线程触发）。</summary>
        public event EventHandler<HFileProgressEventArgs> ProgressChanged;

        /// <summary>仅以主机与账号快速构造（端口 21、被动模式、根目录 /）。</summary>
        public HFtpClient(string host, string userName, string password)
            : this(new HFtpAddress { Host = host, UserName = userName, Password = password })
        {
        }

        /// <summary>以 HFtpAddress 构造。</summary>
        /// <param name="address">FTP 连接地址</param>
        public HFtpClient(HFtpAddress address)
        {
            hFtpAddress = address ?? new HFtpAddress();
            hFtpAddress.SocketType = HEnum.HSocketType.FTP_Client;
        }

        /// <summary>
        /// 验证连接与账号（发送 PWD 命令探测登录态）；FTP 本身无长连接，调用后仅置位 IsConnected。
        /// </summary>
        /// <returns>是否登录成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            try
            {
                FtpWebRequest request = CreateRequest(hFtpAddress.RemoteBaseDir, WebRequestMethods.Ftp.PrintWorkingDirectory);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    IsConnected = (int)response.StatusCode < 400;
                    if (!IsConnected)
                    {
                        StrError = response.StatusDescription;
                    }
                    return IsConnected;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                IsConnected = false;
                return false;
            }
        }

        /// <summary>置为断开状态（清理本地状态，不产生网络请求）。</summary>
        public void Close()
        {
            Cancel();
            IsConnected = false;
        }

        /// <summary>
        /// 清空传输缓存：删除指定本地文件对应的未完成下载分片。
        /// </summary>
        /// <param name="localPath">目标本地文件路径</param>
        /// <returns>是否删除了分片</returns>
        public bool ClearCache(string localPath)
        {
            try
            {
                string part = localPath + HFileTransferBase.PartExt;
                if (File.Exists(part))
                {
                    File.Delete(part);
                    return true;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return false;
        }

        /// <summary>清空目录下全部未完成下载分片（*.hftpart），返回删除数量。</summary>
        /// <param name="dir">本地目录</param>
        public int ClearCacheAll(string dir)
        {
            int count = 0;
            try
            {
                if (Directory.Exists(dir))
                {
                    foreach (string part in Directory.GetFiles(dir, "*" + HFileTransferBase.PartExt))
                    {
                        try
                        {
                            File.Delete(part);
                            count++;
                        }
                        catch
                        {
                            // 单个分片被占用时跳过
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return count;
        }

        /// <summary>取消当前上传/下载（中断请求，分片保留以便续传）。</summary>
        public void Cancel()
        {
            mCancel = true;
            FtpWebRequest request = mActive;
            if (request != null)
            {
                try
                {
                    request.Abort();
                }
                catch
                {
                    // Abort 引发进行中请求异常属预期
                }
            }
        }

        /// <summary>
        /// 上传本地文件到 FTP（自动断点续传：远端存在部分文件时用 APPE 从其长度继续）。
        /// </summary>
        /// <param name="localPath">本地文件路径</param>
        /// <param name="remotePath">远端相对路径（相对 RemoteBaseDir，如 dir/a.zip）；null 使用本地文件名</param>
        /// <param name="resume">是否续传，默认 true</param>
        /// <returns>是否上传完整成功</returns>
        public bool UploadFile(string localPath, string remotePath = null, bool resume = true)
        {
            if (!File.Exists(localPath))
            {
                StrError = "Local file not found:" + localPath;
                return false;
            }
            mCancel = false;
            string name = string.IsNullOrEmpty(remotePath) ? Path.GetFileName(localPath) : remotePath;
            long localSize = new FileInfo(localPath).Length;
            long offset = 0;
            try
            {
                if (resume)
                {
                    long remoteSize = GetFileSizeCore(name, false);
                    if (remoteSize >= 0)
                    {
                        if (remoteSize >= localSize)
                        {
                            // 远端已是完整（或更大的同名旧文件）：先删除再整传，保证内容一致
                            DeleteFile(name);
                        }
                        else
                        {
                            offset = remoteSize;
                        }
                    }
                }
                else
                {
                    DeleteFile(name);
                }

                FtpWebRequest request = CreateRequest(name,
                    offset > 0 ? WebRequestMethods.Ftp.AppendFile : WebRequestMethods.Ftp.UploadFile);
                mActive = request;
                request.ContentLength = localSize - offset;
                using (FileStream fs = File.OpenRead(localPath))
                {
                    if (offset > 0)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                    }
                    using (Stream rs = request.GetRequestStream())
                    {
                        CopyStream(fs, rs, localSize, offset, name);
                    }
                }
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    bool ok = (int)response.StatusCode < 400;
                    if (!ok)
                    {
                        StrError = response.StatusDescription;
                    }
                    else
                    {
                        RaiseProgress(name, localSize, localSize);
                    }
                    return ok;
                }
            }
            catch (Exception ex)
            {
                StrError = mCancel ? "Cancelled" : ex.Message;
                return false;
            }
            finally
            {
                mActive = null;
            }
        }

        /// <summary>
        /// 从 FTP 下载文件到本地（自动断点续传：本地 *.hftpart 存在则用 REST 从其长度续收，完成后改名）。
        /// </summary>
        /// <param name="remotePath">远端相对路径（相对 RemoteBaseDir）</param>
        /// <param name="localPath">本地保存完整路径；null 保存到当前目录并使用远端文件名</param>
        /// <param name="resume">是否续传，默认 true</param>
        /// <returns>最终本地文件完整路径；失败返回 null（原因见 StrError）</returns>
        public string DownloadFile(string remotePath, string localPath = null, bool resume = true)
        {
            mCancel = false;
            string name = Path.GetFileName(remotePath);
            if (string.IsNullOrEmpty(localPath))
            {
                localPath = Path.Combine(Environment.CurrentDirectory, name);
            }
            string partPath = localPath + HFileTransferBase.PartExt;
            try
            {
                long total = GetFileSizeCore(remotePath, true);
                if (total < 0)
                {
                    return null;
                }
                string dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                long offset = 0;
                if (resume && File.Exists(partPath))
                {
                    offset = new FileInfo(partPath).Length;
                    if (offset > total)
                    {
                        File.Delete(partPath);
                        offset = 0;
                    }
                }
                else if (File.Exists(partPath))
                {
                    File.Delete(partPath);
                }

                FtpWebRequest request = CreateRequest(remotePath, WebRequestMethods.Ftp.DownloadFile);
                mActive = request;
                if (offset > 0)
                {
                    request.ContentOffset = offset;
                }
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream ftpStream = response.GetResponseStream())
                using (FileStream fs = new FileStream(partPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    CopyStream(ftpStream, fs, total, offset, name);
                }
                mActive = null;
                if (new FileInfo(partPath).Length != total)
                {
                    StrError = "Size mismatch: part " + new FileInfo(partPath).Length + ", expect " + total;
                    return null;
                }
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
                File.Move(partPath, localPath);
                RaiseProgress(name, total, total);
                return localPath;
            }
            catch (Exception ex)
            {
                StrError = mCancel ? "Cancelled, part kept at " + partPath : ex.Message;
                return null;
            }
            finally
            {
                mActive = null;
            }
        }

        /// <summary>列举目录下条目名称（NLST，仅名称）。</summary>
        /// <param name="remoteDir">远端目录（null 使用 RemoteBaseDir）</param>
        public string[] ListFiles(string remoteDir = null)
        {
            return ListCore(remoteDir, WebRequestMethods.Ftp.ListDirectory);
        }

        /// <summary>列举目录明细行（LIST，含权限/大小/日期，具体格式取决于服务器）。</summary>
        public string[] ListDetails(string remoteDir = null)
        {
            return ListCore(remoteDir, WebRequestMethods.Ftp.ListDirectoryDetails);
        }

        /// <summary>查询远端文件大小（字节）；不存在返回 -1。</summary>
        public long GetFileSize(string remotePath)
        {
            return GetFileSizeCore(remotePath, true);
        }

        /// <summary>判断远端文件是否存在。</summary>
        public bool FileExists(string remotePath)
        {
            return GetFileSizeCore(remotePath, true) >= 0;
        }

        /// <summary>删除远端文件。</summary>
        public bool DeleteFile(string remotePath)
        {
            return SimpleCommand(remotePath, WebRequestMethods.Ftp.DeleteFile);
        }

        /// <summary>创建远端目录。</summary>
        public bool MakeDirectory(string remoteDir)
        {
            return SimpleCommand(remoteDir, WebRequestMethods.Ftp.MakeDirectory);
        }

        /// <summary>重命名/移动远端文件或目录。</summary>
        /// <param name="fromPath">原路径</param>
        /// <param name="toPath">新路径</param>
        public bool Rename(string fromPath, string toPath)
        {
            try
            {
                FtpWebRequest request = CreateRequest(fromPath, WebRequestMethods.Ftp.Rename);
                request.RenameTo = NormalizeRemote(toPath);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return (int)response.StatusCode < 400;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>异步上传。</summary>
        public Task<bool> UploadFileAsync(string localPath, string remotePath = null, bool resume = true)
        {
            return Task.Run(() => UploadFile(localPath, remotePath, resume));
        }

        /// <summary>异步下载。</summary>
        public Task<string> DownloadFileAsync(string remotePath, string localPath = null, bool resume = true)
        {
            return Task.Run(() => DownloadFile(remotePath, localPath, resume));
        }

        /// <summary>创建统一配置的 FTP 请求。</summary>
        private FtpWebRequest CreateRequest(string remotePath, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(BuildUri(remotePath));
            request.Method = method;
            request.Credentials = new NetworkCredential(
                hFtpAddress.UserName ?? "anonymous",
                hFtpAddress.Password ?? string.Empty);
            request.UseBinary = true;
            request.UsePassive = hFtpAddress.UsePassive;
            request.EnableSsl = hFtpAddress.EnableSsl;
            request.KeepAlive = false;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;
            return request;
        }

        /// <summary>拼接 ftp:// 主机:端口/根目录/相对路径，规范斜杠。</summary>
        private string BuildUri(string remotePath)
        {
            string baseDir = hFtpAddress.RemoteBaseDir ?? "/";
            if (!baseDir.StartsWith("/"))
            {
                baseDir = "/" + baseDir;
            }
            string rel = NormalizeRemote(remotePath);
            string path = baseDir.TrimEnd('/') + "/" + rel.TrimStart('/');
            int port = hFtpAddress.Port <= 0 ? 21 : hFtpAddress.Port;
            return "ftp://" + hFtpAddress.Host + ":" + port + "/" + path.TrimStart('/');
        }

        /// <summary>远端相对路径规范化（去掉前导斜杠，反斜杠转正斜杠）。</summary>
        private string NormalizeRemote(string remotePath)
        {
            if (string.IsNullOrEmpty(remotePath))
            {
                return string.Empty;
            }
            return remotePath.Replace('\\', '/').TrimStart('/');
        }

        /// <summary>分块拷贝并上报进度，响应 Cancel。</summary>
        private void CopyStream(Stream source, Stream target, long total, long startOffset, string name)
        {
            byte[] buffer = new byte[Math.Max(64, ChunkSize)];
            long transferred = startOffset;
            while (transferred < total)
            {
                if (mCancel)
                {
                    throw new OperationCanceledException();
                }
                int read = source.Read(buffer, 0, (int)Math.Min(buffer.Length, total - transferred));
                if (read <= 0)
                {
                    break;
                }
                target.Write(buffer, 0, read);
                transferred += read;
                RaiseProgress(name, transferred, total);
            }
            target.Flush();
        }

        /// <summary>通用列举。</summary>
        private string[] ListCore(string remoteDir, string method)
        {
            var list = new List<string>();
            try
            {
                FtpWebRequest request = CreateRequest(remoteDir ?? hFtpAddress.RemoteBaseDir, method);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            list.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return list.ToArray();
        }

        /// <summary>查询远端文件大小；throwOnError=false 时不存在返回 -1 而不写错误。</summary>
        private long GetFileSizeCore(string remotePath, bool throwOnError)
        {
            try
            {
                FtpWebRequest request = CreateRequest(remotePath, WebRequestMethods.Ftp.GetFileSize);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return response.ContentLength;
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    StrError = ex.Message;
                }
                return -1;
            }
        }

        /// <summary>无响应体的简单命令（删除/建目录）。</summary>
        private bool SimpleCommand(string remotePath, string method)
        {
            try
            {
                FtpWebRequest request = CreateRequest(remotePath, method);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return (int)response.StatusCode < 400;
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>触发进度事件。</summary>
        private void RaiseProgress(string name, long transferred, long total)
        {
            EventHandler<HFileProgressEventArgs> handler = ProgressChanged;
            if (handler != null)
            {
                handler(this, new HFileProgressEventArgs
                {
                    FileName = name,
                    BytesTransferred = transferred,
                    TotalBytes = total,
                    Percent = total > 0 ? (int)(transferred * 100 / total) : 0,
                    Tag = hFtpAddress
                });
            }
        }

        /// <summary>释放（FTP 无托管连接，仅复位状态）。</summary>
        public void Dispose()
        {
            Close();
        }
    }
}
