using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UGCF.Utils;
using UnityEngine;

public class MessageProcessor
{
    /// <summary>
    /// 发送一个包含消息主体的请求
    /// </summary>
    /// <param name="msg">消息主体</param>
    public static void SendMessage(Msg_C2S msg)
    {
        if (!SocketClient.Instance)
            return;
        msg.token = UserInfoModel.Instance.currentToken;
        SocketClient.Instance.AddSendMessageQueue(msg);
    }

    /// <summary>
    /// 发送一个不包含消息主体，只包含协议ID的请求
    /// </summary>
    /// <param name="protoId">协议ID</param>
    public static void SendMessage(ProtoId protoId)
    {
        if (!SocketClient.Instance)
            return;
        SocketClient.Instance.AddSendMessageQueue(new Msg_C2S()
        {
            token = UserInfoModel.Instance.currentToken,
            protoId = protoId
        });
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
}