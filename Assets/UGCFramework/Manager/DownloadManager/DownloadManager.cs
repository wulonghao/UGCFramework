using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class DownloadManager : MonoBehaviour
{
    private static DownloadManager _instance = null;
    public static DownloadManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject _obj = new GameObject();
                _instance = _obj.AddComponent<DownloadManager>();
                _obj.name = _instance.GetType().ToString();
                DontDestroyOnLoad(_obj);
                Instance.Init();
            }
            return _instance;
        }
    }
    public static bool isRunning = true;
    public static int finishCount = 0;
    List<DownloadItem> downloadList = new List<DownloadItem>();//待下载队列
    int downloadingCount = 0;
    List<Thread> threadList = new List<Thread>();
    const int maxThreadCount = 10;

    void Init()
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }

    public DownloadItem DownloadFile(BundleInfo info, Action<float> processCallback = null, Action callback = null)
    {
        DownloadItem item = new DownloadItem(info);
        Thread thread = new Thread(new ThreadStart(item.Init));
        thread.Start();
        threadList.Add(thread);
        StartCoroutine(DownloadFileAc(item, processCallback, callback));
        return item;
    }

    public void DownloadFiles(List<BundleInfo> infos, Action<float> loopCallback = null, Action callback = null)
    {
        finishCount = 0;
        downloadingCount = 0;
        for (int i = 0; i < infos.Count; i++)
        {
            DownloadItem item = new DownloadItem(infos[i]);
            downloadList.Add(item);
        }
        int parallelDownloadCount = Mathf.Min(infos.Count, maxThreadCount);
        for (int i = 0; i < parallelDownloadCount; i++)
        {
            CreateDownloadThread();
        }
        StartCoroutine(DownloadProgress(loopCallback, callback));
    }

    void CreateDownloadThread()
    {
        Thread thread = new Thread(new ThreadStart(() =>
        {
            while (downloadingCount < downloadList.Count)
            {
                downloadingCount++;
                downloadList[downloadingCount - 1].Init();
            }
        }));
        thread.Start();
        threadList.Add(thread);
    }

    IEnumerator DownloadProgress(Action<float> loopCallback, Action callback)
    {
        while (finishCount < downloadList.Count)
        {
            if (loopCallback != null)
                loopCallback((float)finishCount / downloadList.Count);
            yield return WaitForUtils.WaitFrame;
        }
        loopCallback(1);
        if (callback != null)
        {
            CloseAllThread();
            callback();
        }
    }

    IEnumerator DownloadFileAc(DownloadItem item, Action<float> processCallback, Action callback)
    {
        while (item.GetProcess() < 1)
        {
            if (processCallback != null)
                processCallback(item.GetProcess());
            yield return WaitForUtils.WaitFrame;
        }
        if (callback != null)
            callback();
    }

    void CloseAllThread()
    {
        isRunning = false;
        for (int i = 0; i < threadList.Count; i++)
        {
            Thread thread = threadList[i];
            try
            {
                if (thread != null)
                    thread.Abort();
            }
            catch { }
        }
        threadList.Clear();
    }

    void OnDestroy()
    {
        CloseAllThread();
    }

    public struct BundleInfo
    {
        public string Url { get; set; }

        public string Path { get; set; }

        public long Length { get; set; }

        public override bool Equals(object bi)
        {
            return bi is BundleInfo && Url == ((BundleInfo)bi).Url;
        }

        public static bool operator ==(BundleInfo bi1, BundleInfo bi2)
        {
            return bi1.Url == bi2.Url;
        }

        public static bool operator !=(BundleInfo bi1, BundleInfo bi2)
        {
            return bi1.Url != bi2.Url;
        }

        public override int GetHashCode()
        {
            return Url.GetHashCode();
        }
    }
}