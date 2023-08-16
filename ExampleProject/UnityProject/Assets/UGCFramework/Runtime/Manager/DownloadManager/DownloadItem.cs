using System;
using System.IO;
using System.Net;
using UGCF.Utils;
using UnityEngine;

namespace UGCF.Manager
{
    public class DownloadItem
    {
        private const string tempFileExt = ".temp";   // 临时文件后缀名
        private DownloadManager.BundleInfo info;
        private int fileLength;                // 原文件大小
        private int currentLength;             // 当前下载好了的大小
        private string tempSaveFilePath;        // 临时文件全路径
        private float progress;

        public DownloadItem(DownloadManager.BundleInfo _info)
        {
            info = _info;
        }

        public void Init(object o = null)
        {
            fileLength = 0;
            currentLength = 0;
            tempSaveFilePath = Path.ChangeExtension(info.Path, tempFileExt);
            Download();
        }

        FileStream fileStream = null;
        HttpWebResponse response = null;
        HttpWebRequest request = null;
        Stream stream = null;
        void Download()
        {
            if (!DownloadManager.IsRunning) return;
            GC.Collect();
            try
            {
                if (File.Exists(info.Path))
                    File.Delete(info.Path);
                request = (HttpWebRequest)WebRequest.Create(info.Url);
                request.Method = "GET";
                request.KeepAlive = false;
                request.Timeout = 10000;

                if (File.Exists(tempSaveFilePath))
                {
                    fileStream = File.OpenWrite(tempSaveFilePath);
                    currentLength = (int)fileStream.Length;
                    fileStream.Seek(currentLength, SeekOrigin.Current);

                    //设置下载的文件读取的起始位置
                    request.AddRange(currentLength);
                }
                else
                {
                    CreateDirectoryByFilePath(tempSaveFilePath);
                    fileStream = new FileStream(tempSaveFilePath, FileMode.Create);
                    currentLength = 0;
                }
                response = (HttpWebResponse)request.GetResponse();
                stream = response.GetResponseStream();
                //总的文件大小=当前需要下载的+已下载的
                fileLength = (int)response.ContentLength + currentLength;
                int lengthOnce;

                byte[] buffer = new byte[fileLength];
                if (stream.CanRead)
                {
                    lengthOnce = stream.Read(buffer, 0, fileLength);
                    while (lengthOnce > 0 && DownloadManager.IsRunning)
                    {
                        currentLength += lengthOnce;
                        fileStream.Write(buffer, 0, lengthOnce);
                        lengthOnce = stream.Read(buffer, 0, fileLength);
                        progress = Mathf.Clamp(currentLength * 1f / fileLength, 0, 1);
                    }
                }
                Close();
                if (currentLength < fileLength || fileLength == 0)
                {
                    Download();
                }
                else
                {
                    DownloadManager.FinishCount++;
                    //临时文件转为最终的下载文件
                    File.Move(tempSaveFilePath, info.Path);
                }
            }
            catch (Exception e)
            {
                LogUtils.Log(e.ToString() + "+" + tempSaveFilePath);
                Close();
                Download();
            }
        }

        void Close()
        {
            if (stream != null)
            {
                stream.Close();
            }
            if (fileStream != null)
            {
                fileStream.Close();
            }
            if (response != null)
            {
                response.Close();
                response = null;
            }
            if (request != null)
            {
                request.Abort();
                request = null;
            }
        }

        public float GetProcess()
        {
            return progress;
        }

        void CreateDirectoryByFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string dirName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
            }
        }
    }
}