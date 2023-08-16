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
        public static Dictionary<ProtoId, List<GameObject>> AllProtoGos = new Dictionary<ProtoId, List<GameObject>>();
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
                //commonGos 和 gameObjs一一对应，commonGos中包含的键值对在gomes中一定存在
                if (AllProtoGos.ContainsKey(msg.protoId))
                {
                    List<GameObject> gos = AllProtoGos[msg.protoId];
                    for (int i = 0; i < gos.Count; i++)
                        GameEvents[gos[i]][msg.protoId](msg);
                }
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
            if (AllProtoGos.ContainsKey(protoId))
                AllProtoGos[protoId].Add(go);
            else
                AllProtoGos.Add(protoId, new List<GameObject>() { go });
        }

        /// <summary>
        /// 删除物体上指定的消息监听
        /// </summary>
        /// <param name="go"></param>
        /// <param name="protoId"></param>
        public static void RemoveTargetListener(this GameObject go, ProtoId protoId)
        {
            if (AllProtoGos.ContainsKey(protoId))
            {
                AllProtoGos[protoId].RemoveAll((g) => g == go);
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
            foreach (List<GameObject> gos in AllProtoGos.Values)
                gos.RemoveAll((g) => g == go);
            return GameEvents.Remove(go);
        }
    }
}