using System;
using System.IO;
using System.Net;
using System.Text;

namespace HFromUI.HSocket.HTTP
{
    public  enum HttpClientMethod
    {
        GET,
        POST,
        HEAD,
        OPTIONS,
        PUT,
        DELETE,
        TRACE,
        CONNECT
    }
    public class HttpClient
    {
        public HttpClient(string uri)
        {
            Create(uri);
            Encoding = Encoding.UTF8;
        }
        /// <summary>错误描述。</summary>
        public string StrError { get; private set; }
        /// <summary>ClientRequest 成员。</summary>
        public HttpWebRequest ClientRequest { get; set; }
        /// <summary>Encoding 成员。</summary>
        public Encoding Encoding { get; set; }
        /// <summary>httpClientMethod 字段。</summary>
        private HttpClientMethod httpClientMethod { get;  set; }
        /// <summary>Create 方法。</summary>
        private void Create(string uri)
        {
            try
            {
                ClientRequest = (HttpWebRequest)WebRequest.Create(uri);
                Initialize();
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
        }
        /// <summary>初始化。</summary>
        public void Initialize()
        {
            ClientRequest.Proxy = null;
            ClientRequest.KeepAlive = true;
            ClientRequest.ProtocolVersion = HttpVersion.Version11;
            ClientRequest.ContentType = "application/json"; 
            ClientRequest.AutomaticDecompression = DecompressionMethods.GZip;
        }

        /// <summary>HttpClientRequest 方法。</summary>
        public string HttpClientRequest(string sendData = null, int method = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(sendData))
                {
                    if (method < 0)
                    {
                        httpClientMethod = HttpClientMethod.GET;
                    }
                    else
                    {
                        httpClientMethod = (HttpClientMethod)method;
                    }
                    ClientRequest.Method = httpClientMethod.ToString();
                }
                else
                {
                    if (method < 0)
                    {
                        httpClientMethod = HttpClientMethod.POST;
                    }
                    else
                    {
                        httpClientMethod = (HttpClientMethod)method;
                    }
                    ClientRequest.Method = httpClientMethod.ToString();
                    byte[] data = Encoding.GetBytes(sendData);
                    ClientRequest.ContentLength = data.Length;
                    using (Stream wStream = ClientRequest.GetRequestStream())
                    {
                        wStream.Write(data, 0, data.Length);
                    }
                }
                HttpWebResponse ClientResponse = (HttpWebResponse)ClientRequest.GetResponse();
                using (Stream responseStream = ClientResponse.GetResponseStream())
                {
                    using (StreamReader sReader = new StreamReader(responseStream, Encoding))
                    {
                        
                        return sReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return null;
            }
        }

        /// <summary>HttpClientRequest 方法。</summary>
        public int HttpClientRequest(ref string Request, string sendData = null, int method = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(sendData))
                {
                    if (method < 0)
                    {
                        httpClientMethod = HttpClientMethod.GET;
                    }
                    else
                    {
                        httpClientMethod = (HttpClientMethod)method;
                    }
                    ClientRequest.Method = httpClientMethod.ToString();
                }
                else
                {
                    if (method < 0)
                    {
                        httpClientMethod = HttpClientMethod.POST;
                    }
                    else
                    {
                        httpClientMethod = (HttpClientMethod)method;
                    }
                    ClientRequest.Method = httpClientMethod.ToString();
                    byte[] data = Encoding.GetBytes(sendData);
                    ClientRequest.ContentLength = data.Length;
                    using (Stream wStream = ClientRequest.GetRequestStream())
                    {
                        wStream.Write(data, 0, data.Length);
                    }
                }
                HttpWebResponse ClientResponse = (HttpWebResponse)ClientRequest.GetResponse();
                using (Stream responseStream = ClientResponse.GetResponseStream())
                {
                    using (StreamReader sReader = new StreamReader(responseStream, Encoding))
                    {
                        Request = sReader.ReadToEnd();
                        return 1;
                    }
                }
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return -101;
            }
        }

        /// <summary>获取 base64String。</summary>
        public string GetBase64String(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>获取 mD5。</summary>
        public string GetMD5(string value)
        {
            string password = string.Empty;
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[]  md5code = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            foreach (var code in md5code)
            {
                password = password + code.ToString("x").PadLeft(2, '0');
            }
            return password;
        }

    }

}

