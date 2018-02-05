using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class XcodeModifyWechat
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
#if UNITY_IPHONE
            // 修改xcode工程
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");

            //添加xcode默认framework引用
            proj.AddFrameworkToProject(target, "libz.tbd", false);
            proj.AddFrameworkToProject(target, "libsqlite3.0.tbd", false);
            proj.AddFrameworkToProject(target, "libc++.tbd", false);
            proj.AddFrameworkToProject(target, "libstdc++.6.0.9.tbd", false);
            proj.AddFrameworkToProject(target, "Security.framework", false);
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", false);
            proj.AddFrameworkToProject(target, "CoreAudio.framework", false);

            File.Delete(Path.Combine(path, "Classes/UnityAppController.h"));
            File.Delete(Path.Combine(path, "Classes/UnityAppController.mm"));
            File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.h"), Path.Combine(path, "Classes/UnityAppController.h"));
            File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.mm"), Path.Combine(path, "Classes/UnityAppController.mm"));
            File.Move(Path.Combine(path, "Libraries/Plugins/OpenSDK1.8.1/libWeChatSDK.a"), Path.Combine(path, "Classes/libWeChatSDK.a"));

            proj.UpdateBuildProperty(target, "OTHER_LDFLAGS", new List<string>() { "-Objc", "_force_load","$(SRCROOT)/Classes/libWeChatSDK.a" }, new List<string>() { "-Objc" });
            proj.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            proj.WriteToFile(projPath);

            //获取info.plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;

            AddUrlType(rootDict, "Editor", "weixin", "这里填微信appid");
            AddUrlType(rootDict, "Editor", "zhifubao", "这里填支付宝appid");

            plist.WriteToFile(plistPath);
#endif
        }
    }

#if UNITY_IPHONE
    private static void AddUrlType(PlistElementDict dict, string role, string name, string scheme)
    {
        //添加URLTypes
        PlistElementArray URLTypes;
        if (dict.values.ContainsKey("CFBundleURLTypes"))
            URLTypes = dict["CFBundleURLTypes"].AsArray();
        else
            URLTypes = dict.CreateArray("CFBundleURLTypes");

        PlistElementDict elementDict = new PlistElementDict();
        elementDict.SetString("CFBundleTypeRole", role);
        elementDict.SetString("CFBundleURLName", name);
        elementDict.CreateArray("CFBundleURLSchemes").AddString(scheme);

        URLTypes.values.Add(elementDict);

        //添加LSApplicationQueriesSchemes
        PlistElementArray schemes;
        if (dict.values.ContainsKey("LSApplicationQueriesSchemes"))
            schemes = dict["LSApplicationQueriesSchemes"].AsArray();
        else
            schemes = dict.CreateArray("LSApplicationQueriesSchemes");
        schemes.AddString(name);
    }
#endif

}

