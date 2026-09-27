using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using HFromUI.HFile;
using HFromUI.HInformation.Log;
using HFromUI.HEnum;

namespace HFromUI.HData
{
    using HFromUI.HLangage;

    public  class HAppData
    {
        // 判断相等的容差
        public const double Epsilon = 1e-10;
        public const double EpsilonSmall = 1e-12;
        /// <summary>
        /// 容差
        /// </summary>
        public const double Allowance  = 0.2;
        /// <summary>DefaultMode 成员。</summary>
        public static int DefaultMode { set; get; } = 2;
        /// <summary>名称。</summary>
        public static string Name { set; get; } = "";
        /// <summary>AppDataAddNumList 字段。</summary>
        private static readonly List<HAppDataAddNum> AppDataAddNumList = new List<HAppDataAddNum>();
        /// <summary>taskSetInt 字段。</summary>
        private static int taskSetInt = 0;
        /// <summary>taskSetData 字段。</summary>
        private static Task taskSetData = null;
        /// <summary>isRunTask 字段。</summary>
        private static bool isRunTask = true;
        /// <summary>isRunFinish 字段。</summary>
        private static bool isRunFinish = false;
        /// <summary>addlock 字段。</summary>
        private static object addlock=new object();
        /// <summary>savelock 字段。</summary>
        private static object savelock=new object(); 
        /// <summary>IsTaskSave 成员。</summary>
        public static bool IsTaskSave { set; get; }
        public static bool RunTask
        {
            get
            {
                return isRunTask;
            }
            set
            {
                if (!value)
                {
                    isRunTask = false;
                    if (taskSetData != null)
                    {
                        taskSetData.Wait();
                        taskSetData = null;
                        GC.Collect();
                    }
                }
                isRunTask = value;
            }
        }

       /// <summary>
       /// 只有翻译保存的地址
       /// </summary>
        public static readonly string AppDataPath = AppDomain.CurrentDomain.BaseDirectory + @"AppData\";

        public static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory + @"AppData\";
      
        private static readonly ConcurrentDictionary<string, HDictionaryFile> hConcurrentDictionary = new ConcurrentDictionary<string, HDictionaryFile>();

        /// <summary>SuffixName 成员。</summary>
        public static string SuffixName { get; set; } = @".inl";
        /// <summary>ProductionName 成员。</summary>
        public static string ProductionName { get; set; } = @"Production";

        /// <summary>WriterAppDataAddNum 方法。</summary>
        private static void WriterAppDataAddNum(HAppDataAddNum item)
        {
            string datePath = $"{ProductionName}\\{item.NowTime:yyyyMM}\\{item.NowTime:yyyyMMdd}";
            string hourKey = $"{item.Name}_{item.NowTime:HH}";

            // 1. 更新小时产量
            double hourVal = GetNumber(hourKey, item.NowTime);
            Set(hourKey, hourVal + item.Count, datePath);

            // 2. 更新日产量
            double dayVal = Get(item.Name, "0", datePath) as double? ?? 0; // 
            Set(item.Name, dayVal + item.Count, datePath);

            // 3. 更新总产量
            double totalVal = GetTotalNumber(item.Name);
            SetTotalNumber(item.Name, totalVal + item.Count);

            // 4. 处理 Space 记录
            if (!string.IsNullOrWhiteSpace(item.Space))
            {
                string spaceKey = $"{hourKey}_Space";
                string currentSpace = Get(spaceKey, "", datePath)?.ToString() ?? "";
                Set(spaceKey, $"{currentSpace}_{item.Space}", datePath);
            }

        }
        private static HLogFile hLogFile;
        /// <summary>Log 方法。</summary>
        public static void Log(object content,HLogGrade logGrade = HLogGrade.Info, string typeName = "")
        {
            if (hLogFile==null)
            {
                hLogFile = new HLogFile($@"Log");
            }
            hLogFile.Show(content, logGrade, typeName);
        }


        /// <summary>保存。</summary>
        public static void Save()
        {
            lock (savelock)
            {
                HTranslation.Save();
                foreach (var item in hConcurrentDictionary.Keys)
                {
                    hConcurrentDictionary[item].Save();
                }
            }
        }
        /// <summary>Wait 方法。</summary>
        public static void Wait()
        {
            if (AppDataAddNumList.Count==0)
            {
                return;
            }
            isRunFinish = true;
            if (taskSetData != null)
            {
                taskSetData.Wait();
                taskSetData = null;
            }

            if (hLogFile != null)
            {
                hLogFile.Wait();
                hLogFile.Dispose();
                hLogFile = null;
            }
            GC.Collect();
        }

        /// <summary>AddNumber 方法。</summary>
        public static bool AddNumber(string name, double count = 1, string space = "")
        {
            isRunFinish = false;
                bool isOk = false;
                HAppDataAddNum data = new HAppDataAddNum
                {
                    Name = name,
                    Count = count,
                    NowTime = DateTime.Now,
                    Space = space
                };
            lock (addlock)
            {
                AppDataAddNumList.Add(data);
            }
              
                if (isRunTask == false)
                {
                    if (taskSetData != null)
                    {
                        taskSetData.Wait(100);
                        taskSetData = null;
                        GC.Collect();
                    }
                }
                isRunTask = isOk = true;
                if (taskSetData == null)
                {
                    taskSetData = Task.Run(() => {
                        while (isRunTask)
                        {
                            if (AppDataAddNumList.Count == 0)
                            {
                                Thread.Sleep(1000);
                                taskSetInt++;
                                if (taskSetInt > 5000)
                                {
                                    RunTask = false;
                                }
                                if (isRunFinish)
                                {
                                    isRunTask = false;
                                }
                            }
                            else
                            {
                                taskSetInt = 0;
                                Thread.Sleep(0);
                                HAppDataAddNum drawSetDataAddNumBuffer = AppDataAddNumList[0];

                                double d1CountHH = Convert.ToDouble(Get(drawSetDataAddNumBuffer.Name + "_" + drawSetDataAddNumBuffer.NowTime.ToString("HH"), "0", ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd")));

                                Set(drawSetDataAddNumBuffer.Name + "_" + drawSetDataAddNumBuffer.NowTime.ToString("HH"), d1CountHH + drawSetDataAddNumBuffer.Count, ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd"));


                                double d2Count = Convert.ToDouble(Get(drawSetDataAddNumBuffer.Name, "0", ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd")));

                                Set(drawSetDataAddNumBuffer.Name, d2Count + drawSetDataAddNumBuffer.Count, ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd"));

                                double d3Count = Convert.ToDouble(Get(drawSetDataAddNumBuffer.Name, "0", ProductionName));

                                Set(drawSetDataAddNumBuffer.Name, d3Count + drawSetDataAddNumBuffer.Count, ProductionName);


                                if (!string.IsNullOrWhiteSpace(drawSetDataAddNumBuffer.Space))
                                {
                                    object Spacebuffer = Get(drawSetDataAddNumBuffer.Name + "_" + drawSetDataAddNumBuffer.NowTime.ToString("HH") + "_Space", "", ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd"));
                                    string SpaceString = "";
                                    if (Spacebuffer != null)
                                    {
                                        SpaceString = Spacebuffer.ToString();
                                    }
                                    Set(drawSetDataAddNumBuffer.Name + "_" + drawSetDataAddNumBuffer.NowTime.ToString("HH") + "_Space", SpaceString + "_" + drawSetDataAddNumBuffer.Space, ProductionName + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMM") + @"\" + drawSetDataAddNumBuffer.NowTime.ToString("yyyyMMdd"));
                                }
                                lock (addlock)
                                {
                                    AppDataAddNumList.RemoveAt(0);
                                }

                                if (AppDataAddNumList.Count == 0&& IsTaskSave)
                                {
                                    Save();
                                }

                            }
                        }
                    });
                }
                return isOk;
          
        }

        /// <summary>Set 方法。</summary>
        public static bool Set(string name, object content, string pathSet = "Data")
        {
            lock (savelock)
            {
                if (string.IsNullOrWhiteSpace(pathSet)) pathSet = "Data";
                var recorder = hConcurrentDictionary.GetOrAdd(pathSet, _ =>
                    new HDictionaryFile(AppDataPath + pathSet, SuffixName));
                return recorder.Write(name, content);
            }
          
        }

        /// <summary>Get 方法。</summary>
        public static object Get(string name, object o_default, string pathSet = "Data")
        {
            if (string.IsNullOrWhiteSpace(pathSet)) pathSet = "Data";
            var recorder = hConcurrentDictionary.GetOrAdd(pathSet, _ =>
                new HDictionaryFile(AppDataPath + pathSet, SuffixName));
            return recorder.Read(name, o_default);
        }

        /// <summary>获取 number。</summary>
        public static double GetNumber(string name, DateTime dateTime)
        {
            return Convert.ToDouble(Get(name, "0", $"{ProductionName}\\{dateTime:yyyyMM}\\{dateTime:yyyyMMdd}"));
        }

        /// <summary>获取 number。</summary>
        public static double GetNumber(string name)
        {
            return GetNumber(name, DateTime.Now);
        }

        /// <summary>获取 totalNumber。</summary>
        public static double GetTotalNumber(string name)
        {
            return Convert.ToDouble(Get(name, "0", ProductionName));
        }

        /// <summary>设置 totalNumber。</summary>
        public static bool SetTotalNumber(string name, object content)
        {
            lock (savelock)
            {
                return Set(name, content, ProductionName);
            }
        }

        /// <summary>获取 hourNumber。</summary>
        public static double GetHourNumber(string name, DateTime dateTime)
        {
            return Convert.ToDouble(Get($"{name}_{dateTime:HH}", "0", $"{ProductionName}\\{dateTime:yyyyMM}\\{dateTime:yyyyMMdd}"));
        }

        /// <summary>获取 hourNumbers。</summary>
        public static double[] GetHourNumbers(string name, DateTime dateTime)
        {
            double[] doubles = new double[24];
            for (int i = 0; i < doubles.Length; i++)
            {
                doubles[i] = Convert.ToDouble(Get($"{name}_{i:D2}", "0", $"{ProductionName}\\{dateTime:yyyyMM}\\{dateTime:yyyyMMdd}"));
            }
            return doubles;
        }

        /// <summary>获取 hourNumber。</summary>
        public static double GetHourNumber(string name)
        {
            return GetHourNumber(name, DateTime.Now);
        }
    }
}
