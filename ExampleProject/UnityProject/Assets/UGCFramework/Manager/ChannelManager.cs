using System.IO;
using UnityEngine;

namespace UGCF.Manager
{
    public class ChannelManager : MonoBehaviour
    {
        private static string channel;

        /// <summary>获取当前渠道</summary>
        public static string GetChannel()
        {
            string path = Application.streamingAssetsPath + "/Channel.txt";
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(channel) && File.Exists(path))
                channel = File.ReadAllText(path);
#else
            if (string.IsNullOrEmpty(channel))
                channel = ThirdPartySdkManager.Instance.GetFileByStreaming(path);
#endif
            if (string.IsNullOrEmpty(channel))
                channel = "Test";
            return channel.Trim();
        }
    }
}