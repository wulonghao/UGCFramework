using System;
using System.IO;
using UnityEngine;


public class LogUtils
{
    static string logErrorPath;
    static StreamWriter logWrite;
    /// <summary> 封装系统Debug.Log </summary>
    public static void Log(object message)
    {
        if (EternalGameObject.Instance.isDebugLog)
            Debug.Log(message);
    }

    public static void LogError(object message, bool containStackTrace = true)
    {
        string messageStr = message.ToString();
        if (EternalGameObject.Instance.isDebugLog)
            Log(message);
        if (containStackTrace)
            LogToFile(messageStr, new System.Diagnostics.StackTrace().ToString(), LogType.Error);
        else
            LogToFile(messageStr, null, LogType.Error);
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
            logErrorPath = GlobalVariableUtils.LogErrorPath + UserInfoModel.Instance.userId + "_" + MiscUtils.GetTimeStampByDateTime(DateTime.Now) + ".log";
            MiscUtils.CreateTextFile(logErrorPath, "");
            logWrite = new StreamWriter(logErrorPath);
            logWrite.AutoFlush = false;
        }
        else
        {
            if (!File.Exists(logErrorPath))
                return;
            int index = logErrorPath.IndexOf("/_");
            if (index > 0)
            {
                string newName = logErrorPath.Insert(index + 1, UserInfoModel.Instance.userId);
                if (newName != logErrorPath)
                {
                    logWrite.Close();
                    StreamReader sr = new StreamReader(logErrorPath);
                    string fileContent = sr.ReadToEnd();
                    sr.Close();
                    File.Delete(logErrorPath);

                    MiscUtils.CreateTextFile(newName, "");
                    logWrite = new StreamWriter(newName);
                    logWrite.AutoFlush = false;
                    logWrite.Write(fileContent);
                    logWrite.Flush();
                }
            }
        }
    }
}

