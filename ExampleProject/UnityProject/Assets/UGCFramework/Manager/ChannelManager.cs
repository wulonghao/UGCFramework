using UnityEngine;

namespace UGCF.Manager
{
    public class ChannelManager : MonoBehaviour
    {
        public static ChannelManager Instance;
        public AndroidBuildChannel androidBuildChannel;
        public IOSBuildChannel iOSBuildChannel;

        void Start()
        {
            Instance = this;
        }

#if UNITY_ANDROID
        public AndroidBuildChannel GetCurrentChannel()
        {
            return androidBuildChannel;
        }
#elif UNITY_IOS
        public IOSBuildChannel GetCurrentChannel()
        {
            return iOSBuildChannel;
        }
#endif

        public enum AndroidBuildChannel
        {
            Test,
            Huawei,
            Xiaomi
        }

        public enum IOSBuildChannel
        {
            Test,
            AppStore
        }
    }
}