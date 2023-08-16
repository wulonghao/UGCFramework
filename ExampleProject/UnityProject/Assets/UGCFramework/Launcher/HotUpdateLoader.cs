using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HotUpdate
{
    public class HotUpdateLoader
    {
        private static string HotUpdateFolderPath => Application.persistentDataPath + "/AssetBundle/hotupdate/";
        private const string UpdateConfigBundleName = "updateConfig";
        private const string HotBundleName = "hot";
        private const string AOTMetaBundleName = "aotmeta";

        /// <summary>
        /// 依据配置文件加载热更新，包括所有的热更程序集(dll)
        /// </summary>
        public static void LoadHotUpdate()
        {
            string configPath = HotUpdateFolderPath + UpdateConfigBundleName;
            byte[] configData;
            if (File.Exists(configPath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(configPath);
                configData = bundle.LoadAsset<TextAsset>(UpdateConfigBundleName).bytes;
                bundle.Unload(false);
            }
            else
            {
                Debug.LogError("热更配置信息加载失败!");
                return;
            }

            string config = Encoding.Unicode.GetString(configData);
            if (string.IsNullOrWhiteSpace(config))
            {
                Debug.LogError("热更配置信息为空!");
                return;
            }

            string[] configs = config.Split('|');
            if (configs.Length != 2)
            {
                Debug.LogError("热更配置信息异常!");
                return;
            }

            {//加载dll
                string hotPath = HotUpdateFolderPath + HotBundleName;
                if (File.Exists(hotPath))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(hotPath);

                    string[] filePaths = configs[0].Split(',');
                    foreach (string filePath in filePaths)
                    {
                        if (string.IsNullOrWhiteSpace(filePath))
                            continue;
                        TextAsset asset = bundle.LoadAsset<TextAsset>(Path.GetFileNameWithoutExtension(filePath));
                        if (asset)
                            Assembly.Load(asset.bytes);
                        else
                            Debug.LogError("热更文件不存在!filePath：" + filePath);
                    }

                    bundle.Unload(false);
                }
            }

            {//加载AOT泛型元数据补充dll
                string aotPath = HotUpdateFolderPath + AOTMetaBundleName;
                if (File.Exists(aotPath))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(aotPath);

                    Dictionary<string, byte[]> AOTMetaAssemblyPaths = new Dictionary<string, byte[]>();
                    string[] allAotMetaNames = configs[1].Split(',');
                    foreach (string fileName in allAotMetaNames)
                    {
                        if (string.IsNullOrWhiteSpace(fileName))
                            continue;
                        TextAsset asset = bundle.LoadAsset<TextAsset>(Path.GetFileNameWithoutExtension(fileName));
                        if (asset)
                        {
                            if (asset.bytes == null)
                            {
                                Debug.LogError($"AOT泛型元数据补充异常! fileName:" + fileName);
                                continue;
                            }
                            AOTMetaAssemblyPaths.Add(fileName, asset.bytes);
                        }
                        else
                            Debug.LogError("AOT泛型元数据补充文件不存在! fileName：" + fileName);
                    }
                    Assembly.Load("UGCF.Runtime").GetType("HotUpdate.LoadMetadataForAOT").GetMethod("LoadMetadataForAOTAssemblies").Invoke(null, new object[] { AOTMetaAssemblyPaths });
                    bundle.Unload(false);
                }
            }
        }
    }
}
