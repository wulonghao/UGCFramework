using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageProcessor
{
    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="msg">消息主体</param>
    /// <param name="isPrevent">是否显示loading</param>
    public static void SendMessage(Msg_C2S msg, bool isPrevent = false)
    {
        if (!SocketClient.Instance)
            return;
        msg.token = UserInfoModel.Instance.currentToken;
        SocketClient.Instance.AddSendMessageQueue(msg, isPrevent);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="protoId">协议ID</param>
    /// <param name="isPrevent">是否显示loading</param>
    public static void SendMessage(ProtoId protoId, bool isPrevent = false)
    {
        if (!SocketClient.Instance)
            return;
        SocketClient.Instance.AddSendMessageQueue(new Msg_C2S()
        {
            token = UserInfoModel.Instance.currentToken,
            protoId = protoId
        }, isPrevent);
    }

    /// <summary>
    /// 解析服务器推送的消息
    /// </summary>
    /// <param name="msg"></param>
    public static void AnalysisMessage(Msg_S2C msg)
    {
        try
        {
            MessageEventPoolManager.ExecuteTargetListener(msg);
        }
        catch (Exception e)
        {
            LogUtils.LogError(e.ToString(), false);
        }
    }

    /// <summary>
    /// 公共消息事件绑定
    /// </summary>
    public static void CommonMessageEventBind()
    {
        MessageEventPoolManager.AddMessageListener(ProtoId.SYSTEM_ERROR_S2C, SystemError);
    }

    static void SystemError(Msg_S2C msg)
    {
        LogUtils.Log(msg.systemError_S2C.code);
    }
}