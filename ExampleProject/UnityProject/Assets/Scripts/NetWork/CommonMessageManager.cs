using protocol;
using System.Collections;
using System.Collections.Generic;
using UGCF.Network;
using UGCF.Utils;
using UnityEngine;

public class CommonMessageManager : MonoBehaviour
{
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
