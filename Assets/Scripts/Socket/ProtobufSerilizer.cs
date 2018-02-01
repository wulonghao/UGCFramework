using System.Collections;
using System.IO;
using ProtoBuf;
using System;
using UnityEngine;

public class ProtobufSerilizer
{
    /// <summary>
    /// 将消息序列化为二进制的方法
    /// </summary>
    /// <param name="model">要序列化的对象</param>
    public static byte[] Serialize(IExtensible model)
    {
        try
        {
            //涉及格式转换，需要用到流，将二进制序列化到流中
            using (MemoryStream ms = new MemoryStream())
            {
                //使用ProtoBuf工具的序列化方法
                Serializer.Serialize<IExtensible>(ms, model);
                //定义二级制数组，保存序列化后的结果
                byte[] result = new byte[ms.Length];
                //将流的位置设为0，起始点
                ms.Position = 0;
                //将流中的内容读取到二进制数组中
                ms.Read(result, 0, result.Length);
                return result;
            }
        }
        catch (Exception ex)
        {
            UIUtils.Log("序列化失败: " + ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// 将收到的消息反序列化成对象
    /// </summary>
    /// <param name="msg">收到的消息.</param>
    /// <returns></returns>
    public static T DeSerialize<T>(byte[] msg) where T : IExtensible
    {
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //将消息写入流中
                ms.Write(msg, 0, msg.Length);
                //将流的位置归0
                ms.Position = 0;
                //反序列化对象
                T result = Serializer.Deserialize<T>(ms);
                return result;
            }
        }
        catch (Exception ex)
        {
            UIUtils.Log("反序列化失败: " + ex.ToString());
            return default(T);
        }
    }
}
