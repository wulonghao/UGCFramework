using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using protocol;
using UGCF.Manager;
using UGCF.UnityExtend;
using UGCF.Utils;

namespace UGCF.Network
{
    public class SocketClient : MonoBehaviour
    {
        private static SocketClient instance;
        public static SocketClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<SocketClient>();
                    instance.name = instance.GetType().ToString();
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
            set { instance = value; }
        }

        private string ip;
        private int port;
        private const int PackageMaxLength = 1024;

        private Socket mSocket;
        private Coroutine heart;

        private Thread threadSend;//发送消息线程
        private bool threadSendAlive;

        private Thread threadRecive;//接收消息线程
        private bool threadReciveAlive;

        private int isConnected = -1;// 判断当前连接状态 -1-未开始连接，0-连接失败，1-连接成功
        private Queue<Msg_S2C> allPackages = new Queue<Msg_S2C>();//所有接收后还未处理的数据包
        private List<Msg_C2S> sendList = new List<Msg_C2S>();
        private List<Msg_C2S> tempSendList = new List<Msg_C2S>();
        private List<Msg_C2S> reconnectSendList = new List<Msg_C2S>();//重连后需要重新发送的消息
        private ManualResetEvent manual = new ManualResetEvent(false);

        #region ...创建socket连接
        public bool Init(string ip, int port, bool isReconnect = false)
        {
            if (string.IsNullOrEmpty(ip))
                return false;
            this.ip = ip;
            this.port = port;
            if (!isReconnect)
            {
                allPackages.Clear();
                sendList.Clear();
                tempSendList.Clear();
            }
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return SocketConnection(isReconnect);
        }

        /// <summary>
        /// 建立服务器连接
        /// </summary>
        bool SocketConnection(bool isReconnect)
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);
                IAsyncResult asyncresult = mSocket.BeginConnect(ipep, ConnectCallBack, mSocket);
                if (asyncresult.AsyncWaitHandle.WaitOne(5000, false))
                {
                    while (isConnected == -1)
                        Thread.Sleep(100);
                    if (isConnected == 1)
                    {
                        isReconnecting = false;
                        ContinueThread();
                        if (threadSend == null)
                        {
                            threadSendAlive = true;
                            threadSend = new Thread(new ThreadStart(SendMessage));
                            threadSend.Start();
                            LogUtils.Log("(Init):threadSend线程初始化");
                        }

                        if (threadRecive == null)
                        {
                            threadReciveAlive = true;
                            threadRecive = new Thread(new ThreadStart(ReceiveMessage));
                            threadRecive.Start();
                            LogUtils.Log("(Init):threadRecive线程初始化");
                        }
                        StartCoroutine(AnalysisMessage());
                        CreateHeart();
                        if (reconnectSendList.Count > 0)
                        {
                            tempSendList.InsertRange(0, reconnectSendList);
                            reconnectSendList.Clear();
                        }
                        LogUtils.Log("建立连接成功!");
                        return true;
                    }
                    else
                    {
                        ConnectLose(isReconnect);
                        return false;
                    }
                }
                else
                {
                    ConnectLose(isReconnect);
                    return false;
                }
            }
            catch (Exception e)
            {
                LogUtils.Log(e.Message);
                ConnectLose(isReconnect);
                return false;
            }
        }

        void ConnectLose(bool isReconnect)
        {
            isReconnecting = false;
            if (!isReconnect)
            {
                CloseSocket();
            }
        }

        /// <summary>
        /// 连接服务器的回调
        /// </summary>
        /// <param name="asyncresult"></param>
        void ConnectCallBack(IAsyncResult asyncresult)
        {
            try
            {
                isConnected = -1;
                if (asyncresult.AsyncState is Socket socketClient)
                {
                    socketClient.EndConnect(asyncresult);
                    isConnected = 1;
                }
            }
            catch (Exception e)
            {
                isConnected = 0;
                LogUtils.Log(e.ToString());
            }
            finally
            {
                asyncresult.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 创建心跳，并停止旧的心跳协程
        /// </summary>
        public void CreateHeart()
        {
            if (heart != null)
            {
                StopCoroutine(heart);
                heart = null;
            }
            heart = StartCoroutine(AddHeartPackage());
        }

        /// <summary>
        /// 每5秒添加心跳包
        /// </summary>
        /// <returns></returns>
        IEnumerator AddHeartPackage()
        {
            while (true)
            {
                yield return WaitForUtils.WaitForSecondsRealtime(10);
                MessageProcessor.SendMessage(ProtoId.SYSTEM_HEART);
            }
        }
        #endregion

        #region ...发送消息
        /// <summary>
        /// 添加数据到发送队列
        /// </summary>
        /// <param name="c2s">消息主体</param>
        public void AddSendMessageQueue(Msg_C2S c2s)
        {
            lock (tempSendList)
            {
                tempSendList.Add(c2s);
                LogUtils.Log("添加消息到发送队列：" + c2s.protoId);
            }
        }

        void SendMessage()
        {
            while (threadSendAlive)
            {
                manual.WaitOne();
                if (mSocket == null)
                {
                    ToReconnect();
                    continue;
                }

                if (sendList.Count == 0)
                {
                    if (tempSendList.Count > 0)
                    {
                        lock (tempSendList)
                        {
                            sendList.AddRange(tempSendList);
                            tempSendList.Clear();
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
                else
                {
                    if (!IsConnected() && !mSocket.Connected)
                        ToReconnect();
                    else
                    {
                        if (sendList.Count == 0 || !Send(sendList[0]))
                            ToReconnect();
                    }
                }
            }
            LogUtils.Log("结束线程：SendMessage");
        }

        bool Send(Msg_C2S c2s)
        {
            bool sendSuccess = false;
            try
            {
                int sendLength = mSocket.Send(NetWorkUtils.BuildPackage(c2s, true), SocketFlags.None);
                sendSuccess = sendLength > 0;
                if (sendSuccess)
                {
                    if (sendList.Contains(c2s))
                        sendList.Remove(c2s);
                    LogUtils.Log("发送消息成功：" + c2s.protoId);
                }
            }
            catch (SocketException ex)
            {
                if (ex.NativeErrorCode.Equals(10035))//对于10035异常，出现在缓冲区满的情况，消息需要重发
                    return Send(c2s);
                else
                    sendSuccess = false;
            }
            catch (Exception e)
            {
                LogUtils.Log(e.ToString());
                sendSuccess = false;
            }
            return sendSuccess;
        }
        #endregion

        #region ...接收消息
        /// <summary>
        /// 解析收到的消息
        /// </summary>
        IEnumerator AnalysisMessage()
        {
            while (threadReciveAlive)
            {
                Msg_S2C message = null;
                lock (allPackages)
                {
                    if (allPackages.Count > 0)
                        message = allPackages.Dequeue();
                }
                if (message != null)
                {
                    LogUtils.Log("收到消息：" + message.protoId);
                    MessageProcessor.AnalysisMessage(message);
                }
                yield return WaitForUtils.WaitFrame;
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        void ReceiveMessage()
        {
            while (threadReciveAlive)
            {
                manual.WaitOne();
                byte[] recvBytesHead = GetBytesReceive(2);//自定义协议头长度
                if (recvBytesHead != null)
                {
                    int bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvBytesHead, 0));
                    if (bodyLength == 0)
                        continue;
                    lock (allPackages)
                        allPackages.Enqueue(ProtobufSerilizer.DeSerialize<Msg_S2C>(GetBytesReceive(bodyLength)));
                }
                else
                {
                    LogUtils.Log("ReceiveMessage：未接收到数据");
                }
            }
            LogUtils.Log("结束线程：ReceiveMessage");
        }

        /// <summary>
        /// 接收数据并进行处理
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        byte[] GetBytesReceive(int length)
        {
            try
            {
                byte[] recvBytes = new byte[length];
                while (length > 0)
                {
                    manual.WaitOne();
                    byte[] receiveBytes = new byte[length < PackageMaxLength ? length : PackageMaxLength];
                    int iBytesBody = 0;
                    if (length >= receiveBytes.Length)
                        iBytesBody = mSocket.Receive(receiveBytes, receiveBytes.Length, 0);
                    else
                        iBytesBody = mSocket.Receive(receiveBytes, length, 0);
                    receiveBytes.CopyTo(recvBytes, recvBytes.Length - length);
                    length -= iBytesBody;
                }
                return recvBytes;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 获取连接状态
        /// </summary>
        /// <returns></returns>
        private bool IsConnected()
        {
            bool ConnectState = true;
            try
            {
                mSocket.Send(new byte[1], 0, SocketFlags.None);
                ConnectState = true;
            }
            catch (SocketException e)
            {
                ConnectState = e.NativeErrorCode.Equals(10035);
            }
            return ConnectState;
        }

        #region ...重连
        bool isReconnecting = false;
        void ToReconnect()
        {
            LogUtils.Log("正在尝试发起重连");
            if (isReconnecting)
                return;
            isReconnecting = true;
            PauseThread();
            Loom.QueueOnMainThread(Reconnect);
        }

        /// <summary>
        /// 断线重连
        /// </summary>
        void Reconnect()
        {
            LogUtils.Log("正在尝试重连");
            if (!this)
                return;
            CloseSocket();
            lock (sendList)
            {
                if (sendList.Count > 0)
                {
                    reconnectSendList.AddRange(sendList);
                    sendList.Clear();
                }
            }
            lock (tempSendList)
            {
                if (tempSendList.Count > 0)
                {
                    reconnectSendList.AddRange(tempSendList);
                    tempSendList.Clear();
                }
            }
            if (MiscUtils.IsNetworkConnecting())
            {
                Init(ip, port, true);
            }
            else
            {
                isReconnecting = false;
            }
        }
        #endregion

        int closeSocketLoopCount = 0;
        /// <summary>
        /// 关闭socket,不终止线程
        /// </summary>
        public void CloseSocket()
        {
            try
            {
                LogUtils.Log("关闭Socket");
                PauseThread();
                if (mSocket != null && mSocket.Connected)
                {
                    mSocket.Shutdown(SocketShutdown.Both);
                    mSocket.Disconnect(true);
                    mSocket.Close();
                    isConnected = -1;
                    mSocket = null;
                }
            }
            catch (Exception e)
            {
                LogUtils.Log(e.ToString());
                if (closeSocketLoopCount < 2)
                {
                    closeSocketLoopCount++;
                    CloseSocket();
                }
                else
                    closeSocketLoopCount = 0;
            }
        }

        /// <summary>
        /// 销毁网络相关并释放相关线程
        /// </summary>
        public void DestroySocket()
        {
            Destroy(gameObject);
        }

        public void ContinueThread()
        {
            LogUtils.Log("线程继续");
            manual.Set();
        }

        public void PauseThread()
        {
            LogUtils.Log("线程暂停");
            manual.Reset();
        }

        void OnDestroy()
        {
            CloseSocket();
            sendList.Clear();
            tempSendList.Clear();
            allPackages.Clear();
            instance = null;
            if (threadSend != null && threadSend.IsAlive)
            {
                threadSendAlive = false;
                threadSend = null;
            }
            if (threadRecive != null && threadRecive.IsAlive)
            {
                threadReciveAlive = false;
                threadRecive = null;
            }
        }

        public static bool GetSocketClient()
        {
            return instance;
        }
    }
}