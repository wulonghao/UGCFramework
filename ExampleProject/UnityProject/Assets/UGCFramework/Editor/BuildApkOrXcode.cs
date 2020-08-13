using System.Collections;
using System.Collections.Generic;
using System.IO;
using UGCF.Manager;
using UGCF.Utils;
using UnityEditor;
using UnityEngine;

namespace UGCF.Editor
{
    public class BuildApkOrXcode : EditorWindow
    {
        BuildTarget buildTarget;
        string savePath;
        bool useDefaultPath = true;
        bool isDebugLog;
        bool useLocalSource;
        bool isCopyBundleToStreaming;
        static UGCFMain ugcfMain;

        [MenuItem("自定义工具/Build")]
        static void ShowExcelTools()
        {
            BuildApkOrXcode instance = GetWindow<BuildApkOrXcode>("生成APK或Xcode工程");
            ugcfMain = FindObjectOfType<UGCFMain>();
            instance.isDebugLog = ugcfMain.isDebugLog;
            instance.useLocalSource = ugcfMain.useLocalSource;
            instance.Show();
        }

        void OnGUI()
        {
            DrawOptions();
        }

        void DrawOptions()
        {
            ChannelManager channelManager = FindObjectOfType<ChannelManager>();
            if (!channelManager)
                return;
#if UNITY_ANDROID
            buildTarget = BuildTarget.Android;
#elif UNITY_IOS
        buildTarget = BuildTarget.iOS;
#endif
            EditorGUILayout.LabelField("当前打包的目标平台：", buildTarget.ToString(), GUILayout.Width(300));

            if (buildTarget == BuildTarget.Android)
            {
                channelManager.androidBuildChannel =
                    (ChannelManager.AndroidBuildChannel)EditorGUILayout.EnumPopup("请选择要打包的渠道：", channelManager.androidBuildChannel, GUILayout.Width(300));
                PlayerSettings.bundleVersion = EditorGUILayout.TextField("要打包的版本名：", PlayerSettings.bundleVersion, GUILayout.Width(300));
                PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("要打包的版本号：", PlayerSettings.Android.bundleVersionCode, GUILayout.Width(300));
            }
            else
            {
                channelManager.iOSBuildChannel = (ChannelManager.IOSBuildChannel)EditorGUILayout.EnumPopup("请选择要打包的渠道：", channelManager.iOSBuildChannel, GUILayout.Width(300));
                PlayerSettings.bundleVersion = EditorGUILayout.TextField("要打包的版本名：", PlayerSettings.bundleVersion, GUILayout.Width(300));
                PlayerSettings.iOS.buildNumber = EditorGUILayout.TextField("要打包的版本号：", PlayerSettings.iOS.buildNumber, GUILayout.Width(300));
            }
            PlayerSettings.productName = EditorGUILayout.TextField("APP名称：", PlayerSettings.productName, GUILayout.Width(300));

            useDefaultPath = EditorGUILayout.Toggle("使用默认路径", useDefaultPath);

            GUILayout.BeginHorizontal();
#if UNITY_ANDROID
            if (!useDefaultPath)
            {
                if (GUILayout.Button("选择保存地址", GUILayout.Width(100)))
                {
                    string apkDirectory = Application.dataPath.Replace("Assets", "AndroidApk/") + PlayerSettings.bundleVersion;
                    if (!Directory.Exists(apkDirectory))
                        apkDirectory = Application.dataPath.Replace("Assets", "AndroidApk");
                    string apkName = string.Format("{0}_{1}", channelManager.androidBuildChannel.ToString().ToLower(), PlayerSettings.bundleVersion);
                    savePath = EditorUtility.SaveFilePanel("选择保存地址", apkDirectory, apkName, "apk");
                }
                EditorGUILayout.LabelField(savePath);
            }
            else
            {
                savePath = Application.dataPath.Replace("Assets", "AndroidApk") +
                    string.Format("/{1}/{0}_{1}.apk", channelManager.androidBuildChannel.ToString().ToLower(), PlayerSettings.bundleVersion);
                EditorGUILayout.LabelField("默认路径：", savePath);
            }
#elif UNITY_IOS
        if (!useDefaultPath)
        {
            if (GUILayout.Button("选择保存地址", GUILayout.Width(100)))
            {
                string xcodePath = Application.dataPath.Replace("Assets", "XcodeProject");
                savePath = EditorUtility.SaveFolderPanel("选择保存地址", xcodePath, "");
            }
            EditorGUILayout.LabelField(savePath);
        }
        else
        {
            savePath = Application.dataPath.Replace("Assets", "XcodeProject");
            EditorGUILayout.LabelField("默认路径：", savePath);
        }
#endif
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            isDebugLog = EditorGUILayout.Toggle("是否开启日志", isDebugLog, GUILayout.Width(160));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            useLocalSource = EditorGUILayout.Toggle("是否关闭更新功能", useLocalSource, GUILayout.Width(160));
            EditorGUILayout.LabelField("(是否为读取StreamingAssets/AssetBundle下的资源)");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            isCopyBundleToStreaming = EditorGUILayout.Toggle("是否拷贝资源到Streaming", useLocalSource ? useLocalSource : isCopyBundleToStreaming, GUILayout.Width(160));
            EditorGUILayout.LabelField("(取消勾选将删除StreamingAssets/AssetBundle文件夹)");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("开始Build", GUILayout.Width(200)))
            {
                Caching.ClearCache();
                //EditorCreateBundle.DeleteBundleFiles(BuildTarget.Android);
                //EditorCreateBundle.DeleteBundleFiles(BuildTarget.iOS);
                if (buildTarget == BuildTarget.Android)
                {
                    ugcfMain.isDebugLog = isDebugLog;
                    ugcfMain.useLocalSource = useLocalSource;
                    SetKetstore();
                    EditorCreateBundle.BuildReleaseAndroidBundle();
                    if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                        Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                    if (isCopyBundleToStreaming)
                        MiscUtils.CopyDirectory(Application.dataPath.Replace("Assets", "Release/") + buildTarget + "/" + channelManager.androidBuildChannel, ConstantUtils.StreamingAssetBundleFolderPath, true, true);
                }
                else
                {
                    string channel = channelManager.iOSBuildChannel.ToString();
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, channel);
                    ugcfMain.isDebugLog = isDebugLog;
                    ugcfMain.useLocalSource = useLocalSource;
                    EditorCreateBundle.BuildReleaseIOSBundle();
                    if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                        Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                    if (isCopyBundleToStreaming)
                        MiscUtils.CopyDirectory(Application.dataPath.Replace("Assets", "Release/") + buildTarget + "/" + channel, ConstantUtils.StreamingAssetBundleFolderPath, true, true);
                }

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = GetBuildScenes(),
                    locationPathName = savePath,
#if UNITY_ANDROID
                    targetGroup = BuildTargetGroup.Android,
#elif UNITY_IOS
                targetGroup = BuildTargetGroup.iOS,
#endif
                    target = buildTarget,
                    options = BuildOptions.None
                };
                BuildPipeline.BuildPlayer(buildPlayerOptions);
#if UNITY_ANDROID
                Debug.Log("Android平台" + channelManager.androidBuildChannel + "渠道包生成完毕");
#elif UNITY_IOS
            Debug.Log("iOS平台" + eternalGameObject.iOSBuildChannel + "渠道Xcode工程生成完毕");
#endif
            }
        }

        string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null)
                    continue;
                if (e.enabled)
                    names.Add(e.path);
            }
            return names.ToArray();
        }

        static void SetKetstore()
        {
            PlayerSettings.Android.keystoreName = "签名文件路径";
            PlayerSettings.Android.keyaliasName = "xxxx";
            PlayerSettings.Android.keystorePass = "xxxx";
            PlayerSettings.Android.keyaliasPass = "xxxx";
        }
    }
}