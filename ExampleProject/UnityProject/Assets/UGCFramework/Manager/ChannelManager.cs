using UnityEngine;

namespace UGCF.Manager
{
    public class ChannelManager : MonoBehaviour
    {
        private static string channel;

        /// <summary>获取当前渠道</summary>
        public static string GetChannel()
        {
            if (string.IsNullOrEmpty(channel))
                channel = ThirdPartySdkManager.Instance.GetFileByStreaming(Application.streamingAssetsPath + "/Channel.txt");
            if (string.IsNullOrEmpty(channel))
                channel = "Test";
            return channel.Trim();
        }
    }
}