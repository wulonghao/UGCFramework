using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class XcodeModify
{
    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
#if UNITY_IOS
        // 修改xcode工程 
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        string target = proj.TargetGuidByName("Unity-iPhone");

        //添加xcode默认framework引用
        proj.AddFrameworkToProject(target, "libiconv.dylib", false);
        proj.AddFrameworkToProject(target, "libresolv.tbd", false);
        proj.AddFrameworkToProject(target, "libz.tbd", false);
        proj.AddFrameworkToProject(target, "libsqlite3.0.tbd", false);
        proj.AddFrameworkToProject(target, "libc++.tbd", false);
        proj.AddFrameworkToProject(target, "libstdc++.6.0.9.tbd", false);
        proj.AddFrameworkToProject(target, "UserNotifications.framework", false);
        proj.AddFrameworkToProject(target, "AddressBook.framework", false);
        proj.AddFrameworkToProject(target, "Contacts.framework", false);

        File.Delete(Path.Combine(path, "Classes/UnityAppController.h"));
        File.Delete(Path.Combine(path, "Classes/UnityAppController.mm"));
        File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.h"), Path.Combine(path, "Classes/UnityAppController.h"));
        File.Copy(Application.dataPath.Replace("Assets", "iOS/UnityAppController.mm"), Path.Combine(path, "Classes/UnityAppController.mm"));

        proj.UpdateBuildProperty(target, "OTHER_LDFLAGS", new List<string>() { "-Objc", "-force_load", "$(SRCROOT)/Classes/libWeChatSDK.a" }, new List<string>() { "-Objc" });
        proj.SetBuildProperty(target, "IPHONEOS_DEPLOYMENT_TARGET", "12.0");
        proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        proj.WriteToFile(projPath);

        //获取info.plist
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        PlistElementDict rootDict = plist.root;

        PlistElementDict security = rootDict.CreateDict("NSAppTransportSecurity");
        security.SetBoolean("NSAllowsArbitraryLoads", true);

        AddUrlType(rootDict, "Editor", "weixin", "这里填微信appid");
        AddUrlType(rootDict, "Editor", "qq", "tencent" + "这里填QQappid");
        AddUrlType(rootDict, "Editor", "zhifubao", "这里填支付宝appid");

        AddSchemes(rootDict, "mqq");
        AddSchemes(rootDict, "mqqapi");
        AddSchemes(rootDict, "mqqwpa");
        AddSchemes(rootDict, "mqqbrowser");
        AddSchemes(rootDict, "mttbrowser");
        AddSchemes(rootDict, "mqqOpensdkSSoLogin");
        AddSchemes(rootDict, "mqqopensdkapiV2");
        AddSchemes(rootDict, "mqqopensdkapiV3");
        AddSchemes(rootDict, "mqqopensdkapiV4");
        AddSchemes(rootDict, "wtloginmqq2");
        AddSchemes(rootDict, "mqzone");
        AddSchemes(rootDict, "mqzoneopensdk");
        AddSchemes(rootDict, "mqzoneopensdkapi");
        AddSchemes(rootDict, "mqzoneopensdkapi19");
        AddSchemes(rootDict, "mqzoneopensdkapiV2");
        AddSchemes(rootDict, "mqqapiwallet");
        AddSchemes(rootDict, "mqqopensdkfriend");
        AddSchemes(rootDict, "mqqopensdkdataline");
        AddSchemes(rootDict, "mqqgamebindinggroup");
        AddSchemes(rootDict, "mqqopensdkgrouptribeshare");
        AddSchemes(rootDict, "tencentapi.qq.reqContent");
        AddSchemes(rootDict, "tencentapi.qzone.reqContent");
        plist.WriteToFile(plistPath);
#endif
    }

#if UNITY_IOS
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
        AddSchemes(dict, name);
    }

    //添加LSApplicationQueriesSchemes
    private static void AddSchemes(PlistElementDict dict, string name)
    {
        PlistElementArray schemes;
        if (dict.values.ContainsKey("LSApplicationQueriesSchemes"))
            schemes = dict["LSApplicationQueriesSchemes"].AsArray();
        else
            schemes = dict.CreateArray("LSApplicationQueriesSchemes");
        schemes.AddString(name);
    }
#endif
}

public partial class XClassExt : System.IDisposable
{
    private string filePath;
    public XClassExt(string fPath)
    {
        filePath = fPath;
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError(filePath + "not found in path.");
            return;
        }
    }

    public void WriteBelow(string below, string text)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        int beginIndex = text_all.IndexOf(below);
        if (beginIndex == -1)
        {
            Debug.LogError(filePath + " not found sign in " + below);
            return;
        }

        int endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);

        text_all = text_all.Substring(0, endIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);

        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();
    }

    public void Replace(string below, string newText)
    {
        StreamReader streamReader = new StreamReader(filePath);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();

        int beginIndex = text_all.IndexOf(below);
        if (beginIndex == -1)
        {
            Debug.LogError(filePath + " not found sign in " + below);
            return;
        }

        text_all = text_all.Replace(below, newText);
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(text_all);
        streamWriter.Close();

    }

    public void Dispose() { }
}