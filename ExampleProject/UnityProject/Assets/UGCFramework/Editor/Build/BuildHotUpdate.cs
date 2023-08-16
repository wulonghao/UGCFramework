using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor;
using System.IO;
using System.Text;
using HybridCLR.Editor.Meta;
using HybridCLR.Editor.AOT;
using System;
using System.Linq;
using UGCF.Utils;
using UnityEditor.Callbacks;

namespace HotUpdate
{
    public static class BuildHotUpdate
    {
        public const string HotFolderName = "hot";
        public const string AOTMetaFolderName = "aotmeta";

        [MenuItem("HybridCLR/HotUpdate/GenerateAll")]
        public static void GenerateAll()
        {
            if (!SettingsUtil.Enable)
            {
                Debug.Log("HybridCLRδ���ã�");
                return;
            }
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            Il2CppDefGeneratorCommand.GenerateIl2CppDef();

            // �⼸����������HotUpdateDlls
            LinkGeneratorCommand.GenerateLinkXml(target);

            // ���ɲü����aot dll
            StripAOTDllCommand.GenerateStripedAOTDlls(target, EditorUserBuildSettings.selectedBuildTargetGroup);
            CopyStripedAOTDlls(target);

            // �ŽӺ�������������AOT dll�����뱣֤�Ѿ�build��������AOT dll
            MethodBridgeGeneratorCommand.GenerateMethodBridge(target);
            ReversePInvokeWrapperGeneratorCommand.GenerateReversePInvokeWrapper(target);
        }

        [MenuItem("HybridCLR/HotUpdate/BuildHotUpdate(��������AOTDll)")]
        public static void BuildAndGenerateConfig()
        {
            if (!SettingsUtil.Enable)
            {
                Debug.Log("HybridCLRδ���ã�");
                return;
            }
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            Il2CppDefGeneratorCommand.GenerateIl2CppDef();
            LinkGeneratorCommand.GenerateLinkXml(target);
            StripAOTDllCommand.GenerateStripedAOTDlls(target, EditorUserBuildSettings.selectedBuildTargetGroup);
            CopyStripedAOTDlls(target);
            CopyDllsAndGenerateConfig();
        }

        [MenuItem("HybridCLR/HotUpdate/BuildHotUpdate(����������AOTDll)")]
        public static void BuildAndGenerateConfigSimple()
        {
            if (!SettingsUtil.Enable)
            {
                Debug.Log("HybridCLRδ���ã�");
                return;
            }
            CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
            CopyDllsAndGenerateConfig();
        }

        static void CopyDllsAndGenerateConfig()
        {
            string fileRootPath = ConstantUtils.BundleDirectory + "/HotUpdate/";
            string configPath = fileRootPath + "updateConfig.bytes";
            StringBuilder fileConfig = new StringBuilder();
            CopyHotUpdateAssembliesAndGenerateConfig(fileRootPath, fileConfig);
            fileConfig.Append("|");
            CopyAOTAssembliesAndGenerateConfig(fileRootPath, fileConfig);
            File.WriteAllBytes(configPath, Encoding.Unicode.GetBytes(fileConfig.ToString()));
        }

        static void CopyHotUpdateAssembliesAndGenerateConfig(string fileRootPath, StringBuilder fileConfig)
        {
            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget);
            string hotfixAssembliesDstDir = fileRootPath + HotFolderName;
            if (Directory.Exists(hotfixAssembliesDstDir))
                Directory.Delete(hotfixAssembliesDstDir, true);
            Directory.CreateDirectory(hotfixAssembliesDstDir);

            foreach (string dllName in SettingsUtil.HotUpdateAssemblyFilesIncludePreserved)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dllName}";
                if (!File.Exists(dllPath))
                    continue;
                string fileName = $"{dllName}.bytes";
                string dllBytesPath = $"{hotfixAssembliesDstDir}/{fileName}";
                File.Copy(dllPath, dllBytesPath, true);
                fileConfig.Append($"{HotFolderName}/{fileName},");
                Debug.Log($"[CopyHotUpdateAssembliesToFiles] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }
        }

        static void CopyAOTAssembliesAndGenerateConfig(string fileRootPath, StringBuilder config)
        {
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            string aotAssembliesDstDir = fileRootPath + AOTMetaFolderName;
            if (Directory.Exists(aotAssembliesDstDir))
                Directory.Delete(aotAssembliesDstDir, true);
            Directory.CreateDirectory(aotAssembliesDstDir);

            List<string> metaAOTDllNames = GetMetaAOTDllNames();
            foreach (string dllName in metaAOTDllNames)
            {
                string dllPath = $"{aotAssembliesSrcDir}/{dllName}";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"ab�����AOT����Ԫ���ݲ���dll:{dllPath} ʱ��������,�ļ������ڡ��ü����AOT dll��BuildPlayerʱ�������ɣ������Ҫ����BuildPlayer���ٴ����");
                    continue;
                }
                string fileName = $"{dllName}.bytes";
                string dllBytesPath = $"{aotAssembliesDstDir}/{fileName}";
                File.Copy(dllPath, dllBytesPath, true);
                config.Append($"{AOTMetaFolderName}/{fileName},");
                Debug.Log($"[CopyAOTAssembliesToFiles] copy AOT dll {dllPath} -> {dllBytesPath}");
            }
        }

        /// <summary>
        /// ��ȡ�漰AOT����Ԫ���ݵ�Dll�б�
        /// </summary>
        /// <returns></returns>
        static List<string> GetMetaAOTDllNames()
        {
            List<string> metaAOTDlls = new List<string>();
            HybridCLRSettings gs = SettingsUtil.HybridCLRSettings;
            List<string> hotUpdateDllNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            IAssemblyResolver resolver = MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, hotUpdateDllNames);
            using (AssemblyReferenceDeepCollector collector = new AssemblyReferenceDeepCollector(resolver, hotUpdateDllNames))
            {
                var analyzer = new Analyzer(new Analyzer.Options
                {
                    MaxIterationCount = Math.Min(20, gs.maxGenericReferenceIteration),
                    Collector = collector,
                });
                analyzer.Run();
                List<GenericClass> types = analyzer.AotGenericTypes;
                foreach (GenericClass type in types)
                {
                    string dllName = type.Type.Module.Name;
                    if (!metaAOTDlls.Contains(dllName))
                        metaAOTDlls.Add(dllName);
                }

                List<GenericMethod> methods = analyzer.AotGenericMethods;
                foreach (GenericMethod method in methods)
                {
                    string dllName = method.Method.DeclaringType.Module.Name;
                    if (!metaAOTDlls.Contains(dllName))
                        metaAOTDlls.Add(dllName);
                }
            }
            return metaAOTDlls;
        }

        static void CopyStripedAOTDlls(BuildTarget target)
        {
            if (!SettingsUtil.Enable)
            {
                Debug.Log($"[CopyStrippedAOTAssemblies] disabled");
                return;
            }
            string platformName = "Win64";
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                    platformName = "Win32";
                    break;
                case BuildTarget.StandaloneWindows64:
                    platformName = "Win64";
                    break;
                case BuildTarget.Android:
                    platformName = "Android";
                    break;
                case BuildTarget.iOS:
                    platformName = "iOS";
                    break;
            }
            string srcStripDllPath = $"{SettingsUtil.ProjectDir}/Library/PlayerDataCache/{platformName}/Data/Managed";
            Debug.Log($"[CopyStrippedAOTAssemblies] CopyScripDlls. src:{srcStripDllPath} target:{target}");

            var dstPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            Directory.CreateDirectory(dstPath);
            foreach (var fileFullPath in Directory.GetFiles(srcStripDllPath, "*.dll"))
            {
                var file = Path.GetFileName(fileFullPath);
                Debug.Log($"[CopyStrippedAOTAssemblies] copy strip dll {fileFullPath} ==> {dstPath}/{file}");
                File.Copy($"{fileFullPath}", $"{dstPath}/{file}", true);
            }
        }
    }
}
