using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UGCF.Utils
{
    public partial class LogUtils
    {
        public static string LogErrorRootPath { get { return Application.persistentDataPath + "/LogError/"; } }
        static string logErrorPath;
        static StringBuilder allLogStr = new StringBuilder();
        static StreamWriter logWrite;

        /// <summary>
        /// 封装系统Debug.Log
        /// </summary>
        /// <param name="log">输出的日志内容</param>
        /// <param name="isAppend">是否对日志进行保留拼接</param>
        public static void Log(object log, bool isAppend = true)
        {
            if (UGCFMain.Instance.IsDebugLog)
                Debug.Log(log);

            if (isAppend)
                AppendLog(log);
        }

        public static void LogError(object log, bool containStackTrace = true)
        {
            string messageStr = log.ToString();
            if (UGCFMain.Instance.IsDebugLog)
                Log(log);
            if (containStackTrace)
                LogToFile(messageStr, new System.Diagnostics.StackTrace().ToString(), LogType.Error);
            else
                LogToFile(messageStr, null, LogType.Error);
        }

        static void AppendLog(object log)
        {
            try
            {
                if (allLogStr.Length > 1048576)
                    allLogStr.Clear();
                allLogStr.AppendLine();
                allLogStr.Append(string.Format("{0}   {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), log));
                allLogStr.AppendLine();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        public static void LogToFile(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Exception:
                case LogType.Error:
                    RefreshOrCreateFile();
                    if (logWrite == null || !string.IsNullOrEmpty(stackTrace))
                    {
                        logWrite.Close();
                        File.Delete(logErrorPath);
                        return;
                    }
                    logWrite.WriteLine();
                    logWrite.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 错误类型:" + type);
                    logWrite.WriteLine(condition);
                    if (!string.IsNullOrEmpty(stackTrace))
                        logWrite.WriteLine(stackTrace);
                    //logWrite.WriteLine(type);
                    logWrite.Flush();
                    break;
                case LogType.Assert:
                case LogType.Log:
                case LogType.Warning:
                    break;
            }
        }

        static void RefreshOrCreateFile()
        {
            if (string.IsNullOrEmpty(logErrorPath))
            {
                logErrorPath = LogErrorRootPath + MiscUtils.DateTimeToTimestampInMilliseconds(DateTime.Now) + ".log";
                MiscUtils.CreateTextFile(logErrorPath, "");
                logWrite = new StreamWriter(logErrorPath) { AutoFlush = false };
            }
            else
            {
                if (!File.Exists(logErrorPath))
                    return;
                if (logWrite != null)
                    logWrite.Flush();
            }
        }
    }
}
