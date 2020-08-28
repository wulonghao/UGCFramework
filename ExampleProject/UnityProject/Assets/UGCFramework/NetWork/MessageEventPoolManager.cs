using protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGCF.Network
{
    public static class MessageEventPoolManager
    {
        public delegate void MessageEvent(Msg_S2C msg);
        //公共的事件池
        public static Dictionary<ProtoId, MessageEvent> CommonMEs = new Dictionary<ProtoId, MessageEvent>();
        //对应协议的的物体关系池
        public static Dictionary<ProtoId, GameObject> AllProtoGos = new Dictionary<ProtoId, GameObject>();
        //对应物体的的事件池
        public static Dictionary<GameObject, Dictionary<ProtoId, MessageEvent>> GameEvents = new Dictionary<GameObject, Dictionary<ProtoId, MessageEvent>>();

        /// <summary>
        /// 执行目标消息处理事件
        /// </summary>
        /// <param name="protoId"></param>
        /// <param name="msg"></param>
        public static void ExecuteTargetListener(Msg_S2C msg)
        {
            if (CommonMEs.ContainsKey(msg.protoId))
                CommonMEs[msg.protoId](msg);
            else
            {
                if (AllProtoGos.ContainsKey(msg.protoId))//commonGos 和 gomes一一对应，commonGos中包含的键值对在gomes中一定存在
                    GameEvents[AllProtoGos[msg.protoId]][msg.protoId](msg);
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
            CommonMEs[protoId] = new MessageEvent(action);
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
            if (GameEvents.ContainsKey(go))
                goDic = GameEvents[go];
            else
            {
                goDic = new Dictionary<ProtoId, MessageEvent>();
                GameEvents.Add(go, goDic);
            }

            goDic[protoId] = new MessageEvent(action);
            AllProtoGos[protoId] = go;
        }

        /// <summary>
        /// 删除一个消息监听
        /// </summary>
        /// <param name="go"></param>
        /// <param name="protoId"></param>
        public static void RemoveTargetListener(this GameObject go, ProtoId protoId)
        {
            if (AllProtoGos.ContainsKey(protoId))
            {
                AllProtoGos.Remove(protoId);
                GameEvents[go].Remove(protoId);
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
            foreach (ProtoId protoId in AllProtoGos.Keys)
                if (AllProtoGos[protoId].Equals(go))
                    pis.Add(protoId);

            for (int i = 0; i < pis.Count; i++)
                AllProtoGos.Remove(pis[i]);
            return GameEvents.Remove(go);
        }
    }
}