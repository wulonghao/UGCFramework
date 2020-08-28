using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace UGCF.Network
{
    public class MessageProcessor
    {
        /// <summary>
        /// 发送HTTP请求（短链接）
        /// </summary>
        /// <param name="msg">消息主体</param>
        public static void SendHttpMessage(Msg_C2S msg)
        {
            if (!MiscUtils.IsNetworkConnecting())
            {
                LogUtils.Log("无法连接到网络：" + msg.protoId);
                return;
            }
            UGCFMain.Instance.StartCoroutine(SendHttpRequestAc(msg));
        }

        /// <summary>
        /// 发送HTTP请求（短链接）
        /// </summary>
        /// <param name="protoId">消息ID</param>
        public static void SendHttpMessage(ProtoId protoId)
        {
            if (!MiscUtils.IsNetworkConnecting())
            {
                LogUtils.Log("无法连接到网络：" + protoId);
                return;
            }
            UGCFMain.Instance.StartCoroutine(SendHttpRequestAc(new Msg_C2S() { protoId = protoId }));
        }

        private static IEnumerator SendHttpRequestAc(Msg_C2S msg)
        {
            LogUtils.Log("发送Http请求：" + msg.protoId);
            
            msg.token = "";
            WWWForm wwwf = new WWWForm();
            wwwf.AddField("data", MiscUtils.EncodeBase64(NetWorkUtils.BuildPackage(msg)));

            UnityWebRequest uwr = UnityWebRequest.Post("你的服务器地址", wwwf);
            uwr.timeout = 5;
            yield return uwr.SendWebRequest();
            LoadingPnl.CloseLoading();
            if (!uwr.isNetworkError && !uwr.isHttpError)
            {
                Msg_S2C msg_S2C = ProtobufSerilizer.DeSerialize<Msg_S2C>(MiscUtils.DecodeBase64ToBytes(uwr.downloadHandler.text));
                LogUtils.Log("收到HTTP返回：" + msg_S2C.protoId);
                AnalysisMessage(msg_S2C);
            }
            else
            {
                LogUtils.Log(uwr.error + "，请求失败：异常协议ID：" + msg.protoId);
            }
        }

        /// <summary>
        /// 发送一个包含消息主体的请求（长链接）
        /// </summary>
        /// <param name="msg">消息主体</param>
        public static void SendMessage(Msg_C2S msg)
        {
            if (!SocketClient.Instance)
                return;
            msg.token = "";
            SocketClient.Instance.AddSendMessageQueue(msg);
        }

        /// <summary>
        /// 发送一个不包含消息主体，只包含协议ID的请求（长链接）
        /// </summary>
        /// <param name="protoId">协议ID</param>
        public static void SendMessage(ProtoId protoId)
        {
            if (!SocketClient.Instance)
                return;
            SocketClient.Instance.AddSendMessageQueue(new Msg_C2S()
            {
                token = "",
                protoId = protoId
            });
        }

        /// <summary>
        /// 解析服务器推送的消息（短、长链接通用）
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
}