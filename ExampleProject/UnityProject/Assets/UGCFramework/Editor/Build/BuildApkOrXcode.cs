using System.Collections.Generic;
using System.IO;
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
        ScriptingImplementation scriptingImplementation;

        [MenuItem("自定义工具/Build")]
        static void ShowExcelTools()
        {
            GetWindow<BuildApkOrXcode>("生成APK或Xcode工程").Show();
        }

        void OnGUI()
        {
            DrawOptions();
        }

        void DrawOptions()
        {
            UGCFMain main = FindObjectOfType<UGCFMain>();
            if (!main) return;
#if UNITY_ANDROID
            buildTarget = BuildTarget.Android;
#elif UNITY_IOS
            buildTarget = BuildTarget.iOS;
#endif
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("当前要打包的目标平台：", buildTarget, GUILayout.Width(300));

            if (buildTarget == BuildTarget.Android)
            {
                scriptingImplementation = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, (ScriptingImplementation)EditorGUILayout.EnumPopup("当前编译背景：", scriptingImplementation, GUILayout.Width(300)));
                PlayerSettings.bundleVersion = EditorGUILayout.TextField("要打包的版本名：", PlayerSettings.bundleVersion, GUILayout.Width(300));
                PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("要打包的版本号：", PlayerSettings.Android.bundleVersionCode, GUILayout.Width(300));
            }
            else
            {
                EditorGUILayout.EnumPopup("当前编译背景：", ScriptingImplementation.IL2CPP, GUILayout.Width(300));
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
                    string apkDirectory = Application.dataPath.Replace("UnityProject/Assets", "AndroidApk/") + PlayerSettings.bundleVersion;
                    if (!Directory.Exists(apkDirectory))
                        apkDirectory = Application.dataPath.Replace("UnityProject/Assets", "AndroidApk");
                    savePath = EditorUtility.SaveFilePanel("选择保存地址", apkDirectory, PlayerSettings.bundleVersion, "apk");
                }
                EditorGUILayout.LabelField(savePath);
            }
            else
            {
                savePath = Application.dataPath.Replace("UnityProject/Assets", "AndroidApk") + string.Format("/{0}.apk", PlayerSettings.bundleVersion);
                EditorGUILayout.LabelField("默认路径：", savePath);
            }
#elif UNITY_IOS
            if (!useDefaultPath)
            {
                if (GUILayout.Button("选择保存地址", GUILayout.Width(100)))
                {
                    string xcodePath = Application.dataPath.Replace("UnityProject/Assets", "XcodeProject");
                    savePath = EditorUtility.SaveFolderPanel("选择保存地址", xcodePath, "");
                }
                EditorGUILayout.LabelField(savePath);
            }
            else
            {
                savePath = Application.dataPath.Replace("UnityProject/Assets", "XcodeProject");
                EditorGUILayout.LabelField("默认路径：", savePath);
            }
#endif
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            main.OpenDebugLog = EditorGUILayout.Toggle("是否开启日志", main.OpenDebugLog, GUILayout.Width(160));
            EditorGUILayout.LabelField("(测试包建议勾选，线上包必须取消勾选)");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            main.UseLocalSource = EditorGUILayout.Toggle("是否关闭更新功能", main.UseLocalSource, GUILayout.Width(160));
            EditorGUILayout.LabelField("(测试包若不开启更新则勾选，线上包必须取消勾选)");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            main.UseLocalSource = EditorGUILayout.Toggle("是否在StreamingAssets文件夹内生成资源", main.UseLocalSource, GUILayout.Width(160));
            EditorGUILayout.LabelField("(取消勾选将删除StreamingAssets/AssetBundle文件夹，线上包必须勾选)");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("设置签名", GUILayout.Width(200)))
            {
                SetKetstore();
            }

            if (GUILayout.Button("开始Build（自动设置签名）", GUILayout.Width(200)))
            {
                Caching.ClearCache();
                if (buildTarget == BuildTarget.Android)
                {
                    SetKetstore();
                    if (main.UseLocalSource)
                    {
                        if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                            Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                        EditorCreateBundle.BuildDebugAndroidBundle();
                    }
                    else
                        EditorCreateBundle.BuildReleaseAndroidBundle();
                }
                else
                {
                    if (main.UseLocalSource)
                    {
                        if (Directory.Exists(Application.streamingAssetsPath + "/AssetBundle"))
                            Directory.Delete(Application.streamingAssetsPath + "/AssetBundle", true);
                        EditorCreateBundle.BuildDebugIOSBundle();
                    }
                    else
                        EditorCreateBundle.BuildReleaseIOSBundle();
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
                Debug.Log("Android平台安装包生成完毕");
#elif UNITY_IOS
                Debug.Log("iOS平台Xcode工程生成完毕");
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