using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGCF.Network
{
    public class NetWorkUtils
    {
        /// <summary>
        /// 构建消息数据包
        /// </summary>
        /// <param name="c2s">消息主体</param>
        /// <param name="insertLength">是否在消息头部插入消息长度</param>
        public static byte[] BuildPackage(Msg_C2S c2s, bool insertLength = false)
        {
            byte[] b = ProtobufSerilizer.Serialize(c2s);
            if (b == null)
                return new byte[0];
            ProtobufByteBuffer buf = null;
            //常规情况下，长度一般为4个字节
            //也可以出于安全目的，自由定义该长度，同时也可以插入一定的安全校验数据
            if (insertLength)
            {
                buf = ProtobufByteBuffer.Allocate(b.Length + 4);
                buf.WriteInt(b.Length);
            }
            else
            {
                buf = ProtobufByteBuffer.Allocate(b.Length);
            }
            buf.WriteBytes(b);
            return buf.GetBytes();
        }
    }
}