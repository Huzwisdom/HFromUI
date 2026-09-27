using System;
using System.Drawing;
using System.IO;

namespace HFromUI.HConvert
{
    using HFromUI.HLangage;
    internal class HRGBToYUV
    {
        /// <summary>PictureToBitmap 方法。</summary>
        public static Bitmap PictureToBitmap(string path)
        {
            Image image = Image.FromFile(path);
            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            Graphics bg = Graphics.FromImage(bitmap);
            bg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            bg.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            bg.Dispose();
            image.Dispose();
            return bitmap;
        }

        /// <summary>获取 rgb32_From_Bitmap。</summary>
        public static byte[] GetRgb32_From_Bitmap(Bitmap Source)
        {
            bool bError = false;
            string errorMsg = string.Empty;

            return GetRgb32_From_Bitmap(Source, ref bError, ref errorMsg);
        }

        /// <summary>
        /// Bitmap转换为RGB32字节数组
        /// </summary>
        /// <param name="Source">Bitmap图片</param>
        /// <returns></returns>
        public static byte[] GetRgb32_From_Bitmap(Bitmap Source, ref bool bError, ref string errorMsg)
        {
            bError = false;

            int lPicWidth = Source.Width;
            int lPicHeight = Source.Height;

            Rectangle rect = new Rectangle(0, 0, lPicWidth, lPicHeight);
            System.Drawing.Imaging.BitmapData bmpData = Source.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            IntPtr iPtr = bmpData.Scan0;

            int picSize = lPicWidth * lPicHeight * 3;

            byte[] pRrgaByte = new byte[picSize];

            System.Runtime.InteropServices.Marshal.Copy(iPtr, pRrgaByte, 0, picSize);

            Source.UnlockBits(bmpData);

            try
            {
                //for (int iRow = 0; iRow < lPicHeight; iRow++)
                //{
                //    for (int jCol = 0; jCol < lPicWidth; jCol++)
                //    {
                //        pRrgaByte[4 * (iRow * lPicWidth + jCol) + 0] = Convert.ToByte(pRrgaByte[iPoint++]);
                //        pRrgaByte[4 * (iRow * lPicWidth + jCol) + 1] = Convert.ToByte(pRrgaByte[iPoint++]);
                //        pRrgaByte[4 * (iRow * lPicWidth + jCol) + 2] = Convert.ToByte(pRrgaByte[iPoint++]);
                //        pRrgaByte[4 * (iRow * lPicWidth + jCol) + 3] = Convert.ToByte(A);
                //    }
                //}

                bError = true;
                errorMsg = HTranslation.GetContent("BMP数据转换成功");

                return pRrgaByte;
            }
            catch (Exception exp)
            {
                pRrgaByte = null;

                bError = false;
                errorMsg = exp.ToString();
                //throw;
            }

            return null;
        }

        /// <summary>
        /// RGB图片字节数组转为YUV图片字节数组
        /// </summary>
        /// <param name="rgbData"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        /// <returns></returns>
        public static byte[] RGB2YUV(byte[] rgbData, int imageWidth, int imageHeight)
        {
            int Height = imageHeight;
            int Width = imageWidth;

            byte r, g, b;
            byte[] yuvList = new byte[Width * Height * 3 / 2];

            int count = 0;
            int ycount = 0;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    r = rgbData[3 * (i * Width + j) + 0];
                    g = rgbData[3 * (i * Width + j) + 1];
                    b = rgbData[3 * (i * Width + j) + 2];

                    yuvList[ycount] = Convert.ToByte(((66 * r + 129 * g + 25 * b + 128) >> 8) + 16);
                    ycount++;

                    if (j % 2 == 0 && i % 2 == 0)
                    {
                        yuvList[Height * Width + count] = Convert.ToByte(((-38 * r - 74 * g + 112 * b + 128) >> 8) + 128);
                        yuvList[Height * Width * 5 / 4 + count] = Convert.ToByte(((112 * r - 94 * g - 18 * b + 128) >> 8) + 128);
                        count++;
                    }
                }
            }

            return yuvList;

            //FileStream fs = new FileStream(@"1.yuv", FileMode.OpenOrCreate);
            //BinaryWriter bw = new BinaryWriter(fs);
            //bw.Write(yuvList);
            //bw.Close();
            //fs.Close();
            //Console.WriteLine("转换成功");
        }

        /// <summary>
        /// 保存YUV数据到文件
        /// </summary>
        /// <param name="yuvData"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool SaveYUV2File(byte[] yuvData, string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(yuvData);
                bw.Close();
                fs.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
