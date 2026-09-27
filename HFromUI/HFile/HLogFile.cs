using HFromUI.HInformation.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HFromUI.HEnum;

namespace HFromUI.HFile
{
    using HFromUI.HLangage;
    public class HLogFile
    {
        private HTxtFile txtFile;
        /// <summary>timeTxt 字段。</summary>
        private DateTime timeTxt = DateTime.Now;
        /// <summary>LogRecordingList 字段。</summary>
        private ConcurrentQueue<HLogRecording> LogRecordingList = new ConcurrentQueue<HLogRecording>();
        private Task taskLog;
        /// <summary>IsLogRun 字段。</summary>
        private bool IsLogRun = true;
        private string logFile;
        private string CsvTitle;
        /// <summary>NameError 字段。</summary>
        private string NameError = string.Empty;
        /// <summary>isfinish 字段。</summary>
        private bool isfinish = false;

        /// <summary>MaxListCount 成员。</summary>
        public int MaxListCount { set; get; } = 100000;

        /// <summary>IsSuspend 成员。</summary>
        public bool IsSuspend { set; get; } = false;
        public HLogFile(string file)
        {
            timeTxt = DateTime.Now;
            logFile = file;
            if (string.IsNullOrWhiteSpace(logFile))
            {
                txtFile = new HTxtFile(HFromUI.HData.HAppData.AppPath + $@"\Log\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}.txt");
            }
            else
            {
                if (Path.GetFileName(logFile).Contains("."))
                {
                    txtFile = new HTxtFile(logFile);
                }
                else
                {
                    txtFile = new HTxtFile(HFromUI.HData.HAppData.AppPath + $@"\{logFile}\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}.txt");
                }
            }
            int errorInt = 0;
            taskLog = new Task(() => {
                while (IsLogRun)
                {
                    Thread.Sleep(50);
                    if (LogRecordingList.Count==0)
                    {
                        if (isfinish)
                        {
                            IsLogRun = false;
                        }
                    }
                    if (LogRecordingList.Count > 0 && !IsSuspend)
                    {
                        try
                        {
                            if (MaxListCount > 0)
                            {
                                if (Math.Floor(1f * LogRecordingList.Count / MaxListCount) == 0)
                                {
                                    NameError = string.Empty;
                                    errorInt = 0;
                                }
                                else
                                {
                                    errorInt++;
                                    NameError = "Error" + Math.Ceiling(1f * errorInt / 20).ToString();
                                }
                            }
                            else
                            {
                                NameError = string.Empty;
                                errorInt = 0;
                            }
                            timeTxt = DateTime.Now;
                            StringBuilder txtStringBuilder = new StringBuilder(); 
                            bool isok=   LogRecordingList.TryPeek(out HLogRecording hLogRecording);
                            if (isok)
                            {
                                switch (hLogRecording.LOGGrade)
                                {
                                    case HLogGrade.Info:
                                    case HLogGrade.Warm:
                                    case HLogGrade.Error:
                                    case HLogGrade.Bug:
                                    case HLogGrade.BigMistake:
                                        if (string.IsNullOrWhiteSpace(logFile))
                                        {
                                            txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\Log\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.txt";
                                        }
                                        else
                                        {
                                            if (logFile.Contains("."))
                                            {
                                                txtFile.FilePath = logFile;
                                                if (!string.IsNullOrWhiteSpace(NameError))
                                                {
                                                    string filename = Path.GetFileNameWithoutExtension(logFile);
                                                    txtFile.FilePath = logFile.Replace(filename, filename + NameError);
                                                }
                                            }
                                            else
                                            {
                                                txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\{logFile}\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.txt";
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(hLogRecording.TypeName))
                                        {
                                            txtStringBuilder.Append(hLogRecording.TypeName.ToString());
                                        }
                                        else
                                        {
                                            txtStringBuilder.Append(hLogRecording.LOGGrade.ToString());
                                        }
                                        txtStringBuilder.Append(">>");
                                        txtStringBuilder.Append(hLogRecording.Time.ToString("HH:mm:ss"));
                                        txtStringBuilder.Append(" ");
                                        txtStringBuilder.Append(hLogRecording.Time.Millisecond.ToString().PadLeft(3, '0'));
                                        txtStringBuilder.Append("||");
                                        txtStringBuilder.Append(hLogRecording.Record);
                                        break;
                                    case HLogGrade.csvTitle:
                                        CsvTitle = hLogRecording.Record;
                                        break;
                                    case HLogGrade.csv:
                                        if (string.IsNullOrWhiteSpace(logFile))
                                        {
                                            txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\Log\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.csv";
                                            if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                            {
                                                txtFile.AddLine(CsvTitle);
                                            }
                                        }
                                        else
                                        {
                                            if (logFile.Contains("."))
                                            {
                                                txtFile.FilePath = logFile;
                                                if (!string.IsNullOrWhiteSpace(NameError))
                                                {
                                                    string filename = Path.GetFileNameWithoutExtension(logFile);
                                                    txtFile.FilePath = logFile.Replace(filename, filename + NameError);
                                                }
                                                if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                                {
                                                    txtFile.AddLine(CsvTitle);
                                                }
                                            }
                                            else
                                            {
                                                txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\{logFile}\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.csv";
                                                if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                                {
                                                    txtFile.AddLine(CsvTitle);
                                                }
                                            }
                                        }
                                        txtStringBuilder.Append(hLogRecording.Record);
                                        break;
                                    case HLogGrade.csvLog:
                                        if (string.IsNullOrWhiteSpace(logFile))
                                        {
                                            txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\Log\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.csv";
                                            if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                            {
                                                txtFile.AddLine(CsvTitle);
                                            }
                                        }
                                        else
                                        {
                                            if (logFile.Contains("."))
                                            {
                                                txtFile.FilePath = logFile;
                                                if (!string.IsNullOrWhiteSpace(NameError))
                                                {
                                                    string filename = Path.GetFileNameWithoutExtension(logFile);
                                                    txtFile.FilePath = logFile.Replace(filename, filename + NameError);
                                                }
                                                if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                                {
                                                    txtFile.AddLine(CsvTitle);
                                                }
                                            }
                                            else
                                            {
                                                txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\{logFile}\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.csv";
                                                if (!string.IsNullOrWhiteSpace(CsvTitle) && !txtFile.IsDocuments())
                                                {
                                                    txtFile.AddLine(CsvTitle);
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(hLogRecording.TypeName))
                                        {
                                            txtStringBuilder.Append(hLogRecording.TypeName.ToString());
                                        }
                                        else
                                        {
                                            txtStringBuilder.Append(hLogRecording.LOGGrade.ToString());
                                        }
                                        txtStringBuilder.Append(",");
                                        txtStringBuilder.Append(hLogRecording.Time);
                                        txtStringBuilder.Append(",");
                                        txtStringBuilder.Append(hLogRecording.Record);
                                        break;
                                    default:
                                        if (string.IsNullOrWhiteSpace(logFile))
                                        {
                                            txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\Log\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.txt";
                                        }
                                        else
                                        {
                                            if (logFile.Contains("."))
                                            {
                                                txtFile.FilePath = logFile;
                                                if (!string.IsNullOrWhiteSpace(NameError))
                                                {
                                                    string filename = Path.GetFileNameWithoutExtension(logFile);
                                                    txtFile.FilePath = logFile.Replace(filename, filename + NameError);
                                                }
                                            }
                                            else
                                            {
                                                txtFile.FilePath = HFromUI.HData.HAppData.AppPath + $@"\{logFile}\{timeTxt.ToString("yyyyMM")}\{timeTxt.ToString("yyyyMMdd")}{NameError}.txt";
                                            }
                                        }
                                        if (!string.IsNullOrWhiteSpace(hLogRecording.TypeName))
                                        {
                                            txtStringBuilder.Append(hLogRecording.TypeName.ToString());
                                        }
                                        else
                                        {
                                            txtStringBuilder.Append(hLogRecording.LOGGrade.ToString());
                                        }
                                        txtStringBuilder.Append(">>");
                                        txtStringBuilder.Append(hLogRecording.Time.ToString("HH:mm:ss"));
                                        txtStringBuilder.Append(" ");
                                        txtStringBuilder.Append(hLogRecording.Time.Millisecond.ToString().PadLeft(3, '0'));
                                        txtStringBuilder.Append("||");
                                        txtStringBuilder.Append(hLogRecording.Record);
                                        break;
                                }
                            }
                            if (txtFile.AddLine(txtStringBuilder.ToString(), false))
                            {
                                for (int i = 0; i < 7; i++)
                                {
                                    isok = LogRecordingList.TryDequeue(out HLogRecording firstItem);
                                    if (isok)
                                    {
                                        txtStringBuilder.Clear();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Thread.Sleep(5000);
                            }
                          
                        }
                        catch
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                }
            });
            taskLog.Start();
        }

        /// <summary>显示。</summary>
        public void Show(object content, HLogGrade logGrade = HLogGrade.Info, string typeName = "")
        {
            isfinish = false;
            HLogRecording logRecording = new HLogRecording();
            logRecording.LOGGrade = logGrade;
            logRecording.Record = content.ToString();
            logRecording.Time = DateTime.Now;
            logRecording.TypeName = typeName;
            LogRecordingList.Enqueue(logRecording);
        }
        /// <summary>ShowCsv 方法。</summary>
        public void ShowCsv(object content, HLogGrade logGrade = HLogGrade.csvLog, string typeName = "")
        {
            isfinish = false;
            HLogRecording logRecording = new HLogRecording();
            logRecording.LOGGrade = logGrade;
            logRecording.Record = content.ToString();
            logRecording.Time = DateTime.Now;
            logRecording.TypeName = typeName;
            LogRecordingList.Enqueue(logRecording);
        }
        /// <summary>设置 csvTitle。</summary>
        public void SetCsvTitle(string content)
        {
            CsvTitle = content;
        }
        /// <summary>设置 csvTitleLog。</summary>
        public void SetCsvTitleLog()
        {
            CsvTitle = $@"{HTranslation.GetContent("类型")},{HTranslation.GetContent("时间")},{HTranslation.GetContent("记录")}";
        }
        /// <summary>设置 csvTitle。</summary>
        public void SetCsvTitle(string[] contents)
        {
            StringBuilder csvTitleStringBuilder = new StringBuilder();
            foreach (var item in contents)
            {
                csvTitleStringBuilder.Append(item); csvTitleStringBuilder.Append(",");
            }
            CsvTitle = csvTitleStringBuilder.ToString();
            csvTitleStringBuilder.Clear();
        }
        /// <summary>显示。</summary>
        public void Show(string[] contents, HLogGrade logGrade = HLogGrade.csv, string typeName = "")
        {
            isfinish = false;
            HLogRecording logRecording = new HLogRecording();
            logRecording.LOGGrade = logGrade;
            StringBuilder csvStringBuilder = new StringBuilder();
            foreach (var item in contents)
            {
                csvStringBuilder.Append(item); csvStringBuilder.Append(",");
            }
            logRecording.TypeName = typeName;
            logRecording.Record = csvStringBuilder.ToString();
            logRecording.Time = DateTime.Now;
            LogRecordingList.Enqueue(logRecording);
            csvStringBuilder.Clear();
        }
        /// <summary>判断是否 LogFinish。</summary>
        public bool IsLogFinish()
        {
            return LogRecordingList.Count == 0;
        }
        /// <summary>Wait 方法。</summary>
        public void Wait()
        {
            while (!IsLogFinish()||IsLogRun)
            {
                isfinish = true;
            }
        }
        public void Dispose()
        {
            try
            {
                IsLogRun = false;
                if (taskLog != null)
                {
                    taskLog.Wait();
                    taskLog.Dispose();
                    taskLog = null;
                }
            }
            catch { }
        }
    }
}
