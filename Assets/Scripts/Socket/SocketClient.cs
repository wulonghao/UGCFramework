using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using ProtoBuf;

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
                if (instance.Init())
                    UIUtils.Log("建立连接成功!");
                else
                {
                    TipManager.Instance.OpenTip(TipType.AlertTip, "网络错误，请检查您的网络状况");
                    Destroy(go);
                }
            }
            return instance;
        }
        set { instance = value; }
    }

    string ip;
    int port;
    const int packageMaxLength = 1024;
    static bool hadHeart = false;

    Socket mSocket;
    Thread threadSend;
    Thread threadRecive;
    float lastHeartTime = 0;
    /// <summary>
    /// 判断当前连接状态 -1-未开始连接，0-连接失败，1-连接成功
    /// </summary>
    int isConnected = -1;
    int reconnectTime = 0;//重连次数
    Queue<Message> allPackages = new Queue<Message>();
    List<byte[]> sendList = new List<byte[]>();
    List<byte[]> tempSendList = new List<byte[]>();

    bool Init()
    {
        allPackages.Clear();
        sendList.Clear();
        tempSendList.Clear();
        mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        return SocketConnection();
    }

    /// <summary>
    /// 建立服务器连接
    /// </summary>
    bool SocketConnection()
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
                    threadRecive = new Thread(new ThreadStart(ReceiveMessage));
                    threadRecive.Start();
                    StartCoroutine(AnalysisMessage());
                    if (!hadHeart)
                        StartCoroutine(AddHeartPackage());
                    return true;
                }
                else
                    return false;
            }
            else
            {
                Close();
                OpenTipNetError();
                return false;
            }
        }
        catch (Exception e)
        {
            UIUtils.Log(e.ToString());
            Close();
            OpenTipNetError();
            return false;
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
            UIUtils.Log(e.ToString());
        }
        finally
        {
            asyncresult.AsyncWaitHandle.Close();
        }
    }

    /// <summary>
    /// 每5秒添加心跳包
    /// </summary>
    /// <returns></returns>
    IEnumerator AddHeartPackage()
    {
        yield return new WaitForSecondsRealtime(5);
        AddSendMessageQueue(new Message(MessageId.Heart));
        StartCoroutine(AddHeartPackage());
    }

    #region ...发送消息
    /// <summary>
    /// 添加数据到发送队列
    /// </summary>
    /// <param name="c2s">消息主体</param>
    /// <param name="isPrevent">是否显示loading</param>
    public void AddSendMessageQueue(Message c2s, bool isPrevent = false)
    {
        if (isPrevent && mSocket.Connected)
            LoadingNode.OpenLoadingNode(LoadingType.Common);
        byte[] bytes = BuildPackage(c2s);

        if (!tempSendList.Contains(bytes))
            lock (tempSendList)
                tempSendList.Add(bytes);
        else
            UIUtils.Log("消息重复");

        if (c2s.messageId != MessageId.Heart)
            UIUtils.Log("发送消息：" + c2s.messageId.ToString());
    }

    void SendMessage()
    {
        while (true)
        {
            if (sendList.Count == 0)
            {
                if (tempSendList.Count > 0)
                    lock (tempSendList)
                    {
                        sendList.AddRange(tempSendList);
                        tempSendList.Clear();
                    }
                else
                {
                    Thread.Sleep(50);
                    continue;
                }
            }
            else
                if (!mSocket.Connected && !IsConnected())
                {
                    Loom.QueueOnMainThread(() => { StartCoroutine(Reconnect()); });
                    break;
                }
                else
                    Send(sendList[0]);
        }
    }

    void Send(byte[] bytes)
    {
        try
        {
            mSocket.Send(bytes, SocketFlags.None);
            sendList.RemoveAt(0);
        }
        catch (SocketException ex)
        {
            if (ex.NativeErrorCode.Equals(10035))
                Send(bytes);
            else
                Loom.QueueOnMainThread(() => { TipManager.Instance.OpenTip(TipType.SimpleTip, "请求失败!"); });
        }
    }
    #endregion

    #region ...接收消息
    /// <summary>
    /// 解析收到的消息
    /// </summary>
    IEnumerator AnalysisMessage()
    {
        WaitForSecondsRealtime wfsrAnalysisWait = new WaitForSecondsRealtime(0.005f);
        while (true)
        {
            Message message = null;
            lock (allPackages)
            {
                if (allPackages.Count > 0)
                {
                    message = allPackages.Dequeue();
                    UIUtils.Log("收到消息：" + message.messageId.ToString());
                }
            }

            if (message != null)
            {
                LoadingNode.CloseLoadingNode();
                #region ...消息处理
                switch (message.messageId)
                {
                    default:
                        UIUtils.Log("未处理的消息id :" + message.messageId);
                        break;
                }
                #endregion
            }
            yield return wfsrAnalysisWait;
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    void ReceiveMessage()
    {
        while (true)
        {
            if (!mSocket.Connected && !IsConnected())
                break;
            byte[] recvBytesHead = GetBytesReceive(4);//协议头长度和服务器商定后确定
            int bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvBytesHead, 0));
            byte[] recvBytesId = GetBytesReceive(4);
            int messageid = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(recvBytesId, 0));
            FillAllPackages((MessageId)messageid, GetBytesReceive(bodyLength));
        }
    }

    /// <summary>
    /// 接收数据并处理
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    byte[] GetBytesReceive(int length)
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

    /// <summary>
    /// 填充数据包列表
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="messageBody"></param>
    void FillAllPackages(MessageId messageId, byte[] messageBody)
    {
        Message me = null;
        switch (messageId)
        {
            case MessageId.Test:
                //me = new Message(messageId, ProtobufSerilizer.DeSerialize<Test>(messageBody));
                break;

            default:
                break;
        }
        if (me != null)
            lock (allPackages)
            {
                allPackages.Enqueue(me);
            }
    }
    #endregion

    /// <summary>
    /// 构建消息数据包
    /// </summary>
    /// <param name="protobufModel"></param>
    /// <param name="messageId"></param>
    byte[] BuildPackage(Message c2s)
    {
        byte[] b = ProtobufSerilizer.Serialize(c2s.message);
        ByteBuffer buf = ByteBuffer.Allocate(b.Length + 4 + 4);
        buf.WriteInt(b.Length);
        //buf.WriteShort((short)(b.Length));
        buf.WriteInt((int)c2s.messageId);
        buf.WriteBytes(b);
        return buf.GetBytes();
    }

    /// <summary>
    /// 获取连接状态
    /// </summary>
    /// <returns></returns>
    bool IsConnected()
    {
        bool ConnectState = true;
        bool state = mSocket.Blocking;
        try
        {
            ByteBuffer buf = ByteBuffer.Allocate(4 + 4);
            buf.WriteInt(4);
            buf.WriteInt((int)MessageId.Heart);

            mSocket.Blocking = false;
            int length = mSocket.Send(buf.GetBytes(), 0, SocketFlags.None);
            ConnectState = length != 0;
        }
        catch (SocketException e)
        {
            ConnectState = e.NativeErrorCode.Equals(10035);
        }
        finally
        {
            mSocket.Blocking = state;
        }
        return ConnectState;
    }

    /// <summary>
    /// 断线重连
    /// </summary>
    IEnumerator Reconnect()
    {
        lastHeartTime = Time.realtimeSinceStartup;
        LoadingNode.OpenLoadingNode(LoadingType.Common);
        Close();
        reconnectTime++;
        bool isSuccess = false;
        isSuccess = Init();
        if (!isSuccess)
        {
            if (reconnectTime < 3)
            {
                yield return new WaitForSecondsRealtime(5);
                StartCoroutine(Reconnect());
            }
            else
                OpenTipNetError();
        }
        else
        {
            reconnectTime = 0;
            LoadingNode.instance.Close();
            //重新登录
        }
    }

    void OnDestroy()
    {
        Close();
        sendList.Clear();
        tempSendList.Clear();
        allPackages.Clear();
    }

    /// <summary>
    /// 关闭socket,终止线程
    /// </summary>
    public void Close()
    {
        if (threadSend != null)
        {
            threadSend.Abort();
            threadSend.Join();
            threadSend = null;
        }
        if (threadRecive != null)
        {
            threadRecive.Abort();
            threadRecive.Join();
            threadRecive = null;
        }
        if (mSocket != null)
        {
            if (mSocket.Connected)
                mSocket.Shutdown(SocketShutdown.Both);
            mSocket.Close();
            mSocket = null;
        }
    }

    /// <summary>
    /// 打开网络错误弹窗
    /// </summary>
    void OpenTipNetError()
    {
        TipManager.Instance.OpenTip(TipType.AlertTip, "网络错误，请重新登录", 0, () =>
        {
            SocketClient.instance.Close();
            SocketClient.instance = null;
            //清空登录信息，返回到登录界面
        });
    }
}

public class Message
{
    public MessageId messageId;
    public IExtensible message;
    public Message(MessageId messageId, IExtensible message = null)
    {
        this.messageId = messageId;
        this.message = message;
    }
}
