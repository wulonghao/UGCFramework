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
                    GameObject go = new GameObject();
                    instance = go.AddComponent<SocketClient>();
                    go.name = instance.GetType().ToString();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
            set { instance = value; }
        }

        string ip;
        int port;
        const int packageMaxLength = 1024;

        Socket mSocket;
        Coroutine heart;
        Thread threadSend;//发送消息线程
        bool threadSendAlive = false;
        Thread threadRecive;//接收消息线程
        bool threadReciveAlive = false;
        int isConnected = -1;// 判断当前连接状态 -1-未开始连接，0-连接失败，1-连接成功
        int reconnectCount = 0;//当前重连次数
        Queue<Msg_S2C> allPackages = new Queue<Msg_S2C>();//所有接收后还未处理的数据包
        List<Msg_C2S> sendList = new List<Msg_C2S>();
        List<Msg_C2S> tempSendList = new List<Msg_C2S>();
        List<Msg_C2S> reconnectSendList = new List<Msg_C2S>();//重连后需要重新发送的消息

        #region ...创建socket连接
        public bool Init(string ip, int port, bool isReconnect = false)
        {
            if (string.IsNullOrEmpty(ip) || port == 0)
                return false;
            if (threadSendAlive && threadReciveAlive && threadSend != null && threadRecive != null &&
                threadSend.ThreadState == ThreadState.Running && threadRecive.ThreadState == ThreadState.Running)
                return true;
            this.ip = ip;
            this.port = port;
            if (!isReconnect)
            {
                allPackages.Clear();
                sendList.Clear();
                tempSendList.Clear();
            }
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Blocking = true;
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
                        threadSend = new Thread(new ThreadStart(SendMessage));
                        threadSend.Start();
                        threadSendAlive = true;
                        threadRecive = new Thread(new ThreadStart(ReceiveMessage));
                        threadRecive.Start();
                        threadReciveAlive = true;
                        StartCoroutine(AnalysisMessage());
                        if (heart != null)
                            StopCoroutine(heart);
                        heart = StartCoroutine(AddHeartPackage());
                        if (reconnectSendList.Count > 0)
                        {
                            tempSendList.InsertRange(0, reconnectSendList);
                            reconnectSendList.Clear();
                        }
                        LogUtils.Log("Socket连接建立成功!");
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
            if (!isReconnect)
            {
                OpenTipNetError();
                Destroy(gameObject);
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
                Socket socketClient = asyncresult.AsyncState as Socket;
                if (socketClient != null)
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
        #endregion

        #region ...发送消息
        /// <summary>
        /// 每10秒添加心跳包
        /// </summary>
        /// <returns></returns>
        IEnumerator AddHeartPackage()
        {
            while (true)
            {
                MessageProcessor.SendMessage(ProtoId.SYSTEM_HEART);
                yield return WaitForUtils.WaitForSecondsRealtime(10);
            }
        }

        /// <summary>
        /// 添加数据到发送队列
        /// </summary>
        /// <param name="c2s">消息主体</param>
        public void AddSendMessageQueue(Msg_C2S c2s)
        {
            lock (tempSendList)
            {
                tempSendList.Add(c2s);
                if (c2s.protoId != ProtoId.SYSTEM_HEART)
                    LogUtils.Log("添加消息到发送队列：" + c2s.protoId);
            }
        }

        void SendMessage()
        {
            while (threadSendAlive)
            {
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
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                }
                else
                {
                    if (!IsConnected() || !mSocket.Connected)
                    {
                        ToReconnect();
                        break;
                    }
                    else
                    {
                        if (sendList.Count > 0 && Send(sendList[0]))
                            continue;
                        else
                        {
                            ToReconnect();
                            break;
                        }
                    }
                }
            }
        }

        bool Send(Msg_C2S c2s)
        {
            bool sendSuccess = false;
            try
            {
                int sendLength = mSocket.Send(NetWorkUtils.BuildPackage(c2s), SocketFlags.None);
                sendSuccess = sendLength > 0;
                if (sendSuccess && threadSendAlive)
                {
                    if (sendList.Contains(c2s))
                        sendList.Remove(c2s);
                    if (c2s.protoId != ProtoId.SYSTEM_HEART)
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
                if (!IsConnected() || !mSocket.Connected)
                    break;
                int bodyLength = 0;
                //TODO 数据长度自定义
                lock (allPackages)
                    allPackages.Enqueue(ProtobufSerilizer.DeSerialize<Msg_S2C>(GetBytesReceive(bodyLength)));
            }
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
                    byte[] receiveBytes = new byte[length < packageMaxLength ? length : packageMaxLength];
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
            //catch (Exception e)
            catch
            {
                //LogUtils.Log(e.ToString());
                return null;
            }
        }
        #endregion

        #region ...重连
        /// <summary>
        /// 获取连接状态
        /// </summary>
        /// <returns></returns>
        bool IsConnected()
        {
            bool ConnectState = true;
            try
            {
                ProtobufByteBuffer buf = ProtobufByteBuffer.Allocate(4);
                buf.WriteInt(0);

                int length = mSocket.Send(buf.GetBytes(), 0, SocketFlags.None);
                ConnectState = length >= 0;
            }
            catch (SocketException e)
            {
                ConnectState = e.NativeErrorCode.Equals(10035);
            }
            return ConnectState;
        }

        void ToReconnect()
        {
            Loom.QueueOnMainThread(() => { if (this) StartCoroutine(Reconnect()); });
        }

        /// <summary>
        /// 断线重连
        /// </summary>
        IEnumerator Reconnect()
        {
            LogUtils.Log("正在尝试重连");
            if (!this)
                yield break;
            Close();
            reconnectCount++;
            if (sendList.Count > 0)
            {
                reconnectSendList.AddRange(sendList);
                sendList.Clear();
            }
            if (tempSendList.Count > 0)
            {
                reconnectSendList.AddRange(tempSendList);
                tempSendList.Clear();
            }
            bool isSuccess = Init(ip, port, true);
            if (!isSuccess)
            {
                if (reconnectCount < 3)
                {
                    yield return WaitForUtils.WaitForSecondsRealtime(5);
                    StartCoroutine(Reconnect());
                }
                else
                {
                    OpenTipNetError();
                }
            }
            else
            {
                reconnectCount = 0;
            }
        }
        #endregion

        /// <summary>
        /// 关闭socket,终止线程
        /// </summary>
        public void Close()
        {
            try
            {
                if (mSocket != null && mSocket.Connected)
                    mSocket.Shutdown(SocketShutdown.Both);
                if (threadSend != null && threadSend.IsAlive)
                {
                    threadSendAlive = false;
                    threadSend.Join();
                    threadSend = null;
                }
                if (threadRecive != null && threadRecive.IsAlive)
                {
                    threadReciveAlive = false;
                    threadRecive.Join();
                    threadRecive = null;
                }
                if (mSocket != null && mSocket.Connected)
                {
                    mSocket.Close();
                    mSocket = null;
                }
            }
            catch (SocketException se)
            {
                LogUtils.Log(se.ToString());
                if (se.SocketErrorCode != SocketError.NotConnected)
                    Close();
            }
            catch (Exception e)
            {
                LogUtils.Log(e.ToString());
                Close();
            }
        }

        /// <summary>
        /// 打开网络错误弹窗 
        /// </summary>
        void OpenTipNetError()
        {
            TipManager.Instance.OpenTip(TipType.AlertTip, "与服务器断开连接", 0, () =>
            {
                //TODO 退出登录
            });
        }

        void OnDestroy()
        {
            Close();
            sendList.Clear();
            tempSendList.Clear();
            allPackages.Clear();
            instance = null;
        }
    }
}