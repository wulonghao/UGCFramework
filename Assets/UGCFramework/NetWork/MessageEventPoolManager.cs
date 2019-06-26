using protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class MessageEventPoolManager
{
    public delegate void MessageEvent(Msg_S2C msg);
    //公共的事件池
    public static Dictionary<ProtoId, MessageEvent> commonMEs = new Dictionary<ProtoId, MessageEvent>();
    //对应协议的的物体关系池
    public static Dictionary<ProtoId, GameObject> allProtoGos = new Dictionary<ProtoId, GameObject>();
    //对应物体的的事件池
    public static Dictionary<GameObject, Dictionary<ProtoId, MessageEvent>> gomes = new Dictionary<GameObject, Dictionary<ProtoId, MessageEvent>>();

    /// <summary>
    /// 执行目标消息处理事件
    /// </summary>
    /// <param name="protoId"></param>
    /// <param name="msg"></param>
    public static void ExecuteTargetListener(Msg_S2C msg)
    {
        if (commonMEs.ContainsKey(msg.protoId))
            commonMEs[msg.protoId](msg);
        else
        {
            if (allProtoGos.ContainsKey(msg.protoId))//commonGos 和 gomes一一对应，commonGos中包含的键值对在gomes中一定存在
                gomes[allProtoGos[msg.protoId]][msg.protoId](msg);
        }
    }

    /// <summary>
    /// 添加一个公共消息监听
    /// </summary>
    /// <param name="go"></param>
    /// <param name="protoId"></param>
    /// <param name="action"></param>
    public static void AddMessageListener(ProtoId protoId, Action<Msg_S2C> action)
    {
        commonMEs[protoId] = new MessageEvent(action);
    }

    /// <summary>
    /// 添加一个消息监听
    /// </summary>
    /// <param name="go"></param>
    /// <param name="protoId"></param>
    /// <param name="action"></param>
    public static void AddMessageListener(this GameObject go, ProtoId protoId, Action<Msg_S2C> action)
    {
        MessageListener ml = go.GetComponent<MessageListener>();
        if (!ml) ml = go.AddComponent<MessageListener>();

        Dictionary<ProtoId, MessageEvent> goDic;
        if (gomes.ContainsKey(go))
            goDic = gomes[go];
        else
        {
            goDic = new Dictionary<ProtoId, MessageEvent>();
            gomes.Add(go, goDic);
        }

        goDic[protoId] = new MessageEvent(action);
        allProtoGos[protoId] = go;
    }

    /// <summary>
    /// 删除一个消息监听
    /// </summary>
    /// <param name="go"></param>
    /// <param name="protoId"></param>
    public static void RemoveTargetListener(this GameObject go, ProtoId protoId)
    {
        if (allProtoGos.ContainsKey(protoId))
        {
            allProtoGos.Remove(protoId);
            gomes[go].Remove(protoId);
        }
    }

    /// <summary>
    /// 删除物体上所有的消息监听
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static bool RemoveAllListener(this GameObject go)
    {
        List<ProtoId> pis = new List<ProtoId>();
        foreach (ProtoId protoId in allProtoGos.Keys)
            if (allProtoGos[protoId].Equals(go))
                pis.Add(protoId);

        for (int i = 0; i < pis.Count; i++)
            allProtoGos.Remove(pis[i]);
        return gomes.Remove(go);
    }
}
