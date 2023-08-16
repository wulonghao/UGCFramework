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
        /// ���������ļ������ȸ��£��������е��ȸ�����(dll)
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
                Debug.LogError("�ȸ�������Ϣ����ʧ��!");
                return;
            }

            string config = Encoding.Unicode.GetString(configData);
            if (string.IsNullOrWhiteSpace(config))
            {
                Debug.LogError("�ȸ�������ϢΪ��!");
                return;
            }

            string[] configs = config.Split('|');
            if (configs.Length != 2)
            {
                Debug.LogError("�ȸ�������Ϣ�쳣!");
                return;
            }

            {//����dll
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
                            Debug.LogError("�ȸ��ļ�������!filePath��" + filePath);
                    }

                    bundle.Unload(false);
                }
            }

            {//����AOT����Ԫ���ݲ���dll
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
                                Debug.LogError($"AOT����Ԫ���ݲ����쳣! fileName:" + fileName);
                                continue;
                            }
                            AOTMetaAssemblyPaths.Add(fileName, asset.bytes);
                        }
                        else
                            Debug.LogError("AOT����Ԫ���ݲ����ļ�������! fileName��" + fileName);
                    }
                    Assembly.Load("UGCF.Runtime").GetType("HotUpdate.LoadMetadataForAOT").GetMethod("LoadMetadataForAOTAssemblies").Invoke(null, new object[] { AOTMetaAssemblyPaths });
                    bundle.Unload(false);
                }
            }
        }
    }
}
