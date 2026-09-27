using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>条码/二维码识别结果。</summary>
    public struct HCodeResult
    {
        /// <summary>解码文本内容。</summary>
        public string Text;
        /// <summary>码区域（一维码为条块区域、二维码为符号 XLD 轮廓），调用方负责 Dispose；仅在调用 find 时返回。</summary>
        public HObject Symbol;
    }

    /// <summary>
    /// HALCON 一维码 / 二维码读取完整封装：
    /// 二维码（create_data_code_2d_model / find_data_code_2d）支持 Data Matrix ECC 200、QR Code、
    /// Micro QR Code、PDF417、Aztec、GS1 DataMatrix/GS1 QR 等；
    /// 一维码（create_bar_code_model / find_bar_code）支持 auto 自动判别及 Code 128、EAN-8/13、
    /// Code 39、Code 93、Interleaved 2/5、UPC-A/E、Codabar、Pharmacode 等数十种码制。
    /// 支持增强模式（enhanced_result）取位置/质量等附加信息，句柄由实例持有并在 Dispose 释放。
    /// </summary>
    public class HHalconCodeReader : HHalconBase
    {
        #region ==================== 常量 ====================

        /// <summary>HALCON 常见二维码类型常量。</summary>
        public static class Code2D
        {
            /// <summary>Data Matrix ECC 200（工业最常用）。</summary>
            public const string DataMatrix = "Data Matrix ECC 200";
            /// <summary>QR 码。</summary>
            public const string Qr = "QR Code";
            /// <summary>微型 QR 码。</summary>
            public const string MicroQr = "Micro QR Code";
            /// <summary>PDF417。</summary>
            public const string Pdf417 = "PDF417";
            /// <summary>Aztec 码。</summary>
            public const string Aztec = "Aztec Code";
        }

        /// <summary>HALCON 常见一维码制常量。</summary>
        public static class BarCode
        {
            /// <summary>自动判别码制。</summary>
            public const string Auto = "auto";
            /// <summary>Code 128。</summary>
            public const string Code128 = "Code 128";
            /// <summary>Code 39。</summary>
            public const string Code39 = "Code 39";
            /// <summary>EAN-13 商品码。</summary>
            public const string Ean13 = "EAN-13";
            /// <summary>EAN-8 商品码。</summary>
            public const string Ean8 = "EAN-8";
            /// <summary>UPC-A 商品码。</summary>
            public const string UpcA = "UPC-A";
            /// <summary>交叉 25 码。</summary>
            public const string Itf = "2/5 Interleaved";
        }

        #endregion

        #region ==================== 字段 ====================

        /// <summary>二维码模型句柄（null 表示未创建）。</summary>
        public HTuple DataCode2DHandle { get; private set; } = new HTuple();

        /// <summary>一维码模型句柄（null 表示未创建）。</summary>
        public HTuple BarCodeHandle { get; private set; } = new HTuple();

        private bool _has2D;
        private bool _hasBar;

        #endregion

        #region ==================== 创建/参数 ====================

        /// <summary>
        /// 创建二维码读取模型（create_data_code_2d_model）。
        /// </summary>
        /// <param name="symbolType">码制（取 <see cref="Code2D"/> 常量）。</param>
        /// <param name="paramNames">可选参数名（如 "default_parameters"、"module_size_min"、"module_size_max"、"symbol_cols_min" 等），可空。</param>
        /// <param name="paramValues">参数值（与名称一一对应），可空。"default_parameters" 可设 "enhanced_recognition"/"maximum_recognition"。</param>
        public bool Create2D(string symbolType = Code2D.Qr,
            string[] paramNames = null, string[] paramValues = null)
        {
            Clear2D();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.CreateDataCode2dModel(symbolType ?? Code2D.Qr,
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    paramValues != null ? new HTuple(paramValues) : new HTuple(),
                    out h);
                DataCode2DHandle = h;
                _has2D = true;
            });
        }

        /// <summary>设置二维码模型参数（set_data_code_2d_param）。</summary>
        public bool Set2DParam(string name, object value)
        {
            if (!_has2D) return false;
            return SafeRun(() =>
                HOperatorSet.SetDataCode2dParam(DataCode2DHandle, name ?? string.Empty,
                    HHalconEnv.ToTuple(value)));
        }

        /// <summary>
        /// 创建一维码读取模型（create_bar_code_model）。
        /// </summary>
        /// <param name="paramNames">参数名（如 "element_size_min/max"、"check_char"、"persistence"），可空。</param>
        /// <param name="paramValues">参数值，可空。</param>
        public bool CreateBar(string[] paramNames = null, object[] paramValues = null)
        {
            ClearBar();
            return SafeRun(() =>
            {
                HTuple tupleVals = new HTuple();
                if (paramValues != null)
                {
                    double[] d = new double[paramValues.Length];
                    bool allNumber = true;
                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        try { d[i] = Convert.ToDouble(paramValues[i], System.Globalization.CultureInfo.InvariantCulture); }
                        catch { allNumber = false; }
                    }
                    tupleVals = allNumber ? new HTuple(d) : HHalconEnv.ToTuple(paramValues);
                }
                HTuple h;
                HOperatorSet.CreateBarCodeModel(
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    tupleVals, out h);
                BarCodeHandle = h;
                _hasBar = true;
            });
        }

        #endregion

        #region ==================== 读取 ====================

        /// <summary>
        /// 读取图中全部二维码（find_data_code_2d）。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="symbols">输出结果数组（Text 为解码串，Symbol 为 XLD 轮廓，需逐个 Dispose）。</param>
        /// <param name="paramNames">本次查找附加参数（可空）。</param>
        /// <param name="paramValues">参数值（可空）。</param>
        public bool Find2D(HObject image, out HCodeResult[] symbols,
            string[] paramNames = null, string[] paramValues = null)
        {
            symbols = new HCodeResult[0];
            if (!_has2D || image == null) return false;
            HCodeResult[] resultArr = symbols;
            bool ok = SafeRun(() =>
            {
                HObject xlds;
                HTuple handles, texts;
                HOperatorSet.FindDataCode2d(image, out xlds, DataCode2DHandle,
                    paramNames != null ? new HTuple(paramNames) : new HTuple(),
                    paramValues != null ? new HTuple(paramValues) : new HTuple(),
                    out handles, out texts);
                string[] sa = texts.SArr;
                HCodeResult[] arr = new HCodeResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                {
                    arr[i] = new HCodeResult { Text = sa[i], Symbol = null };
                }
                // XLD 是一个 XLD 容器对象，整体返回（调用方 Dispose 一次即可）
                if (sa.Length > 0) arr[0].Symbol = xlds;
                else if (xlds != null) xlds.Dispose();
                resultArr = arr;
            });
            symbols = resultArr;
            return ok;
        }

        /// <summary>读取图中全部二维码，只取文本（不关心位置轮廓）。</summary>
        public string[] Find2DTexts(HObject image)
        {
            HCodeResult[] arr;
            if (Find2D(image, out arr))
            {
                string[] t = new string[arr.Length];
                for (int i = 0; i < arr.Length; i++) t[i] = arr[i].Text;
                HObject sym0 = arr.Length > 0 ? arr[0].Symbol : null;
                if (sym0 != null) sym0.Dispose();
                return t;
            }
            return new string[0];
        }

        /// <summary>
        /// 读取图中一维码（find_bar_code）。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="symbols">输出结果（Text 文本，Symbol 为整体条块区域容器，Dispose 一次即可）。</param>
        /// <param name="codeType">码制（取 <see cref="BarCode"/> 常量，默认 auto 自动判别）。</param>
        public bool FindBar(HObject image, out HCodeResult[] symbols, string codeType = BarCode.Auto)
        {
            symbols = new HCodeResult[0];
            if (!_hasBar || image == null) return false;
            HCodeResult[] resultArr = symbols;
            bool ok = SafeRun(() =>
            {
                HObject regions;
                HTuple texts;
                HOperatorSet.FindBarCode(image, out regions, BarCodeHandle,
                    codeType ?? BarCode.Auto, out texts);
                string[] sa = texts.SArr;
                HCodeResult[] arr = new HCodeResult[sa.Length];
                for (int i = 0; i < sa.Length; i++)
                    arr[i] = new HCodeResult { Text = sa[i], Symbol = i == 0 ? regions : null };
                if (sa.Length == 0 && regions != null) regions.Dispose();
                resultArr = arr;
            });
            symbols = resultArr;
            return ok;
        }

        /// <summary>读取一维码并只取文本数组。</summary>
        public string[] FindBarTexts(HObject image, string codeType = BarCode.Auto)
        {
            HCodeResult[] arr;
            if (FindBar(image, out arr, codeType))
            {
                string[] t = new string[arr.Length];
                for (int i = 0; i < arr.Length; i++) t[i] = arr[i].Text;
                if (arr.Length > 0 && arr[0].Symbol != null) arr[0].Symbol.Dispose();
                return t;
            }
            return new string[0];
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放二维码模型。</summary>
        public void Clear2D()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (_has2D)
                    {
                        try { HOperatorSet.ClearDataCode2dModel(DataCode2DHandle); } catch { }
                        DataCode2DHandle = new HTuple();
                        _has2D = false;
                    }
                }
                catch { }
            }
        }

        /// <summary>释放一维码模型。</summary>
        public void ClearBar()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (_hasBar)
                    {
                        try { HOperatorSet.ClearBarCodeModel(BarCodeHandle); } catch { }
                        BarCodeHandle = new HTuple();
                        _hasBar = false;
                    }
                }
                catch { }
            }
        }

        /// <summary>释放全部码模型。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear2D();
            ClearBar();
        }

        #endregion
    }
}
