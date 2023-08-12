using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace UGCF.Manager
{
    public class DownloadManager : MonoBehaviour
    {
        private static DownloadManager instance = null;
        public static DownloadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<DownloadManager>();
                    instance.name = instance.GetType().Name;
                    DontDestroyOnLoad(instance);
                    Instance.Init();
                }
                return instance;
            }
        }
        public static bool IsRunning = true;
        public static int FinishCount = 0;
        List<DownloadItem> downloadList = new List<DownloadItem>();//待下载队列
        int downloadingCount = 0;
        List<Thread> threadList = new List<Thread>();
        const int maxThreadCount = 10;

        void Init()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        }

        public void DownloadFiles(List<BundleInfo> infos, Action<float> loopCallback = null, Action callback = null)
        {
            StartCoroutine(DownloadFilesAc(infos, loopCallback, callback));
        }

        public void DownloadFilesUseThread(List<BundleInfo> infos, Action<float> loopCallback = null, Action callback = null)
        {
            FinishCount = 0;
            downloadingCount = 0;
            IsRunning = true;
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

        public void DownloadFileUseThread(BundleInfo info, Action<float> processCallback = null, Action callback = null)
        {
            IsRunning = true;
            DownloadItem item = new DownloadItem(info);
            ThreadPool.QueueUserWorkItem(item.Init);
            StartCoroutine(DownloadFileAc(item, processCallback, callback));
        }

        public void DownloadImage(string url, Action<Sprite> callback = null, string path = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                callback?.Invoke(null);
                return;
            }
            StartCoroutine(DownloadImageAc(url, callback, path));
        }

        public void DownloadAudio(string url, string path)
        {
            if (string.IsNullOrEmpty(url))
                return;
            StartCoroutine(DownloadAudioAc(url, path));
        }

        /// <summary> 下载图片并保存到指定目录</summary> 
        public static IEnumerator DownloadImageAc(string url, Action<Sprite> callback = null, string path = null)
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            Sprite sprite = null;
            if (uwr.isHttpError || uwr.isNetworkError)
            {
                LogUtils.Log("下载失败：url: " + url + ", error: " + uwr.error);
                callback?.Invoke(null);
            }
            else
            {
                try
                {
                    Texture2D texture = new Texture2D(100, 100);
                    texture.LoadImage(uwr.downloadHandler.data);
                    if (!string.IsNullOrEmpty(path))
                    {
                        string filePath = ConstantUtils.SpriteFolderPath + path;
                        string directoryPath = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);
                        File.WriteAllBytes(filePath, texture.EncodeToPNG());
                    }
                    sprite = UIUtils.TextureToSprite(texture);
                    callback?.Invoke(sprite);
                }
                catch (Exception e)
                {
                    LogUtils.Log("下载失败：url: " + url + ", error: " + e.Message);
                    callback?.Invoke(null);
                }
            }
        }

        /// <summary> 下载音乐文件并保存到指定目录</summary> 
        public static IEnumerator DownloadAudioAc(string url, string path)
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.isHttpError || uwr.isNetworkError)
            {
                LogUtils.Log("下载失败：url: " + url + ", error: " + uwr.error);
            }
            else
            {
                try
                {
                    byte[] bs = uwr.downloadHandler.data;

                    if (!string.IsNullOrEmpty(path))
                    {
                        string filePath = ConstantUtils.AudioFolderPath + path;
                        string directoryPath = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directoryPath))
                            Directory.CreateDirectory(directoryPath);
                        File.WriteAllBytes(filePath, bs);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.Log("下载失败：url: " + url + ", error: " + e.Message);
                }
            }
        }

        #region ...内部函数
        IEnumerator DownloadFileAc(BundleInfo info, Action<bool> callback = null)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(info.Url);
            uwr.timeout = 10;
            yield return uwr.SendWebRequest();
            bool success;
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                LogUtils.Log("下载失败：url: " + info.Url + ", error: " + uwr.error);
                success = false;
            }
            else
            {
                try
                {
                    string filePath = info.Path;
                    string dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllBytes(filePath, uwr.downloadHandler.data);
                    success = true;
                }
                catch (Exception e)
                {
                    LogUtils.Log("文件保存失败：url: " + info.Url + ", error: " + e.Message);
                    success = false;
                }
            }
            callback?.Invoke(success);
        }

        IEnumerator DownloadFilesAc(List<BundleInfo> infos, Action<float> loopCallback = null, Action callback = null)
        {
            List<BundleInfo> failBundles = new List<BundleInfo>();
            int num = 0;
            string dir;
            for (int i = 0; i < infos.Count; i++)
            {
                BundleInfo info = infos[i];
                UnityWebRequest uwr = UnityWebRequest.Get(info.Url);
                uwr.timeout = 10;
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    LogUtils.Log("下载失败：url: " + info.Url + ", error: " + uwr.error);
                    failBundles.Add(info);
                }
                else
                {
                    try
                    {
                        string filePath = info.Path;
                        dir = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(filePath, uwr.downloadHandler.data);
                        num++;
                        loopCallback?.Invoke((float)num / infos.Count);
                    }
                    catch (Exception e)
                    {
                        LogUtils.Log("文件保存失败：url: " + info.Url + ", error: " + e.Message);
                        failBundles.Add(info);
                    }
                }
            }
            if (failBundles.Count > 0)
            {
                StartCoroutine(DownloadFilesAc(failBundles, loopCallback, callback));
            }
            else callback?.Invoke();
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
            while (FinishCount < downloadList.Count)
            {
                loopCallback?.Invoke((float)FinishCount / downloadList.Count);
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
            while (item.GetProcess() <= 1)
            {
                yield return WaitForUtils.WaitForSecondsRealtime(1);
                processCallback?.Invoke(item.GetProcess());
                if (item.GetProcess() == 1)
                    break;
            }
            callback?.Invoke();
        }

        void CloseAllThread()
        {
            IsRunning = false;
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
        #endregion

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
                return (Url + Path).GetHashCode();
            }
        }
    }
}