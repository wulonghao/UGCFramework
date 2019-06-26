using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildApkOrXcode : EditorWindow
{
    BuildTarget buildTarget;
    string savePath;
    bool useDefaultPath = true;
    bool isDebugLog;
    bool isLocalVersion;
    bool isCopyBundleToStreaming;

    [MenuItem("自定义工具/Build")]
    static void ShowExcelTools()
    {
        BuildApkOrXcode instance = GetWindow<BuildApkOrXcode>("生成APK或Xcode工程");
        EternalGameObject eternalGameObject = GameObject.Find("EternalGameObject").GetComponent<EternalGameObject>();
        instance.isDebugLog = eternalGameObject.isDebugLog;
        instance.isLocalVersion = eternalGameObject.isLocalVersion;
        instance.Show();
    }

    void OnGUI()
    {
        DrawOptions();
    }

    void DrawOptions()
    {
        GameObject gameObject = GameObject.Find("EternalGameObject");
        if (!gameObject) return;
        EternalGameObject eternalGameObject = gameObject.GetComponent<EternalGameObject>();
#if UNITY_ANDROID
        buildTarget = BuildTarget.Android;
#elif UNITY_IOS
        buildTarget = BuildTarget.iOS;
#endif
        EditorGUILayout.LabelField("当前打包的目标平台：", buildTarget.ToString(), GUILayout.Width(300));

        if (buildTarget == BuildTarget.Android)
        {
            eternalGameObject.androidBuildChannel =
                (AndroidBuildChannel)EditorGUILayout.EnumPopup("请选择要打包的渠道：", eternalGameObject.androidBuildChannel, GUILayout.Width(300));
            PlayerSettings.bundleVersion = EditorGUILayout.TextField("要打包的版本名：", PlayerSettings.bundleVersion, GUILayout.Width(300));
            PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("要打包的版本号：", PlayerSettings.Android.bundleVersionCode, GUILayout.Width(300));
        }
        else
        {
            eternalGameObject.iOSBuildChannel = (IOSBuildChannel)EditorGUILayout.EnumPopup("请选择要打包的渠道：", eternalGameObject.iOSBuildChannel, GUILayout.Width(300));
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
                string apkName = string.Format("{0}_{1}", eternalGameObject.androidBuildChannel.ToString().ToLower(), PlayerSettings.bundleVersion);
                savePath = EditorUtility.SaveFilePanel("选择保存地址", apkDirectory, apkName, "apk");
            }
            EditorGUILayout.LabelField(savePath);
        }
        else
        {
            savePath = Application.dataPath.Replace("Assets", "AndroidApk") +
                string.Format("/{1}/{0}_{1}.apk", eternalGameObject.androidBuildChannel.ToString().ToLower(), PlayerSettings.bundleVersion);
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
        isLocalVersion = EditorGUILayout.Toggle("是否关闭更新功能", isLocalVersion, GUILayout.Width(160));
        EditorGUILayout.LabelField("(是否为读取StreamingAssets/AssetBundle下的资源)");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        isCopyBundleToStreaming = EditorGUILayout.Toggle("是否拷贝资源到Streaming", isLocalVersion ? isLocalVersion : isCopyBundleToStreaming, GUILayout.Width(160));
        EditorGUILayout.LabelField("(取消勾选将删除StreamingAssets/AssetBundle文件夹)");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("开始Build", GUILayout.Width(200)))
        {
            Caching.ClearCache();
            //EditorCreateBundle.DeleteBundleFiles(BuildTarget.Android);
            //EditorCreateBundle.DeleteBundleFiles(BuildTarget.iOS);
            if (buildTarget == BuildTarget.Android)
            {
                eternalGameObject.isDebugLog = isDebugLog;
                eternalGameObject.isLocalVersion = isLocalVersion;
                SetKetstore();
                EditorCreateBundle.BuildReleaseAndroidBundle();
                if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                    Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                if (isCopyBundleToStreaming)
                    MiscUtils.CopyDirectory(Application.dataPath.Replace("Assets", "Release/") + buildTarget + "/" + eternalGameObject.androidBuildChannel, GlobalVariableUtils.StreamingAssetBundleFolderPath, true, true);
            }
            else
            {
                string channel = eternalGameObject.iOSBuildChannel.ToString();
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, channel);
                eternalGameObject.isDebugLog = isDebugLog;
                eternalGameObject.isLocalVersion = isLocalVersion;
                EditorCreateBundle.BuildReleaseIOSBundle();
                if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                    Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                if (isCopyBundleToStreaming)
                    MiscUtils.CopyDirectory(Application.dataPath.Replace("Assets", "Release/") + buildTarget + "/" + channel, GlobalVariableUtils.StreamingAssetBundleFolderPath, true, true);
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
            Debug.Log("Android平台" + eternalGameObject.androidBuildChannel + "渠道包生成完毕");
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