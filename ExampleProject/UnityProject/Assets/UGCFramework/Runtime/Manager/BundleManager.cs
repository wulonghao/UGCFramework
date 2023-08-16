#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using System;
using UGCF.Utils;

namespace UGCF.Manager
{
    public class BundleManager : MonoBehaviour
    {
        private static BundleManager instance = null;
        public static BundleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<BundleManager>();
                    instance.name = instance.GetType().Name;
                    DontDestroyOnLoad(instance);
                }
                return instance;
            }
        }

        void OnDestroy()
        {
            instance = null;
        }

        #region get bundle
        public string GetCommonJson(string jsonName, string diretoryPath = ConstantUtils.CommonResourcesFolderName)
        {
#if UNITY_EDITOR
            TextAsset textAsset = (TextAsset)EditorGUIUtility.Load("AssetBundle/" + diretoryPath + "/Config/" + jsonName.ToLower() + ".json");
#else
            TextAsset textAsset = GetBundleFile<TextAsset>(diretoryPath + "/Config/" + jsonName);
#endif
            if (textAsset != null)
                return textAsset.text;
            else
                return null;
        }

        /// <summary>
        /// 获取一个公共的JsonData
        /// </summary>
        /// <param name="jsonName"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public JToken GetCommonJsonData(string jsonName, string diretoryPath = ConstantUtils.CommonResourcesFolderName)
        {
            string json = GetCommonJson(jsonName, diretoryPath);
            if (string.IsNullOrEmpty(json))
                return null;
            return JObject.Parse(json);
        }

        public AudioClip GetAudioClip(string soundName, string soundType, string directoryPath = ConstantUtils.CommonResourcesFolderName)
        {
            AudioClip ac = GetBundleFile<AudioClip>(directoryPath + "/Audio/" + soundType + "/" + soundName);
            if (!ac)
                ac = GetBundleFile<AudioClip>(ConstantUtils.CommonResourcesFolderName + "/Audio/" + soundType + "/" + soundName);
            return ac;
        }

        /// <summary>
        /// 获取sprite的bundle资源,并保留在内存中
        /// </summary>
        /// <param name="filePath">资源文件相对路径</param>
        /// <param name="directoryPath">目录名，含相对子路径</param>
        /// <returns></returns>
        public AssetBundle GetSpriteBundle(string filePath, string directoryPath = ConstantUtils.CommonResourcesFolderName)
        {
            return GetBundle(directoryPath + "/Sprite/" + filePath);
        }

        /// <summary>
        /// 从目标Bundle中获取Sprite
        /// </summary>
        /// <param name="spriteName">图片名</param>
        /// <param name="spriteBundle">目标Bundle</param>
        /// <returns></returns>
        public Sprite GetSprite(string spriteName, AssetBundle spriteBundle)
        {
            if (!spriteBundle)
                return null;
            return spriteBundle.LoadAsset<Sprite>(spriteName.ToLower());
        }

        /// <summary>
        /// 获取Bundle中所有的图片资源
        /// </summary>
        /// <param name="spriteBundle">目标Bundle</param>
        /// <param name="isUnload">是否在获取完毕后释放Bundle</param>
        /// <returns></returns>
        public Sprite[] GetSprites(AssetBundle spriteBundle, bool isUnload = true)
        {
            if (!spriteBundle)
                return null;
            Sprite[] s = spriteBundle.LoadAllAssets<Sprite>();
            if (isUnload)
                spriteBundle.Unload(false);
            return s;
        }

        /// <summary>
        /// 从图片组中获取一组帧动画图片
        /// 图片名格式要求为“name”+“_”+“从0开始的索引”
        /// </summary>
        /// <param name="name">图片前缀名</param>
        /// <param name="targetSprites">目标图片组</param>
        /// <returns></returns>
        public Sprite[] GetAnimationSprites(string name, Sprite[] targetSprites)
        {
            List<Sprite> ls = new List<Sprite>();
            string tempName;
            name = name.ToLower();
            for (int i = 0; i < targetSprites.Length; i++)
            {
                tempName = targetSprites[i].name;
                if (tempName == name || tempName.Substring(0, tempName.LastIndexOf('_')) == name)
                    ls.Add(targetSprites[i]);
            }
            Sprite[] sps = new Sprite[ls.Count];
            ls.Sort((a, b) =>
            {
                int aIndex = int.Parse(a.name.Substring(a.name.LastIndexOf('_') + 1));
                int bIndex = int.Parse(b.name.Substring(b.name.LastIndexOf('_') + 1));
                return aIndex.CompareTo(bIndex);
            });
            ls.CopyTo(sps);
            return sps;
        }

        /// <summary>
        /// 获取一个Bundle
        /// </summary>
        /// <param name="bundlePath">bundle的相对路径</param>
        /// <returns></returns>
        public AssetBundle GetBundle(string bundlePath)
        {
            string path = ConstantUtils.AssetBundleFolderPath + bundlePath.ToLower();
            if (UGCFMain.Instance.UseLocalSource || !File.Exists(path))
            {
                path = ConstantUtils.StreamingAssetBundleFolderPath + bundlePath.ToLower();
                if (!ThirdPartySdkManager.Instance.FileExistByStreaming(path))
                    return null;
            }
            return AssetBundle.LoadFromFile(path);
        }

        /// <summary>
        /// 获取一个预制体并实例化
        /// </summary>
        /// <param name="gameObjectName">物体名</param>
        /// <param name="directoryPath">所属文件夹相对Prefab文件夹路径，默认为OtherSundry目录</param>
        /// <returns></returns>
        public GameObject GetGameObject(string gameObjectName, string directoryPath = ConstantUtils.OtherSundryFolderName)
        {
            GameObject pre = GetBundleFile<GameObject>("Prefab/" + directoryPath + "/" + gameObjectName);
            if (pre)
            {
                GameObject go = Instantiate(pre);
                go.name = gameObjectName;
                return go;
            }
            return pre;
        }

        /// <summary>
        /// 获取一个UI预制体并实例化
        /// </summary>
        /// <param name="gameObjectPath">预制体的相对路径</param>
        /// <returns></returns>
        public GameObject GetGameObjectByUI(string gameObjectPath)
        {
            GameObject pre = GetBundleFile<GameObject>(gameObjectPath);
            if (pre)
            {
                GameObject go = Instantiate(pre);
                go.name = Path.GetFileNameWithoutExtension(gameObjectPath);
                return go;
            }
            return pre;
        }

        /// <summary>
        /// 加载一个指定类型的Bundle
        /// </summary>
        /// <typeparam name="T">UnityEngine.Object</typeparam>
        /// <param name="relativePath">相对资源根目录的路径</param>
        /// <param name="isUnload">加载完成后是否Unload</param>
        /// <returns></returns>
        T GetBundleFile<T>(string relativePath, bool isUnload = true) where T : UnityEngine.Object
        {
            try
            {
                T asset = null;
                string relativePathLower = relativePath.ToLower();
                string path = ConstantUtils.AssetBundleFolderPath + relativePathLower;
                if (UGCFMain.Instance.UseLocalSource || !File.Exists(path))
                {
                    path = ConstantUtils.StreamingAssetBundleFolderPath + relativePathLower;
                    if (ThirdPartySdkManager.Instance.FileExistByStreaming(path))
                    {
                        AssetBundle ab = AssetBundle.LoadFromFile(path);
                        AssetBundleRequest abr = ab.LoadAssetAsync<T>(Path.GetFileNameWithoutExtension(relativePathLower));
                        asset = (T)abr.asset;
                        if (isUnload)
                            ab.Unload(false);
                    }
                    else
                    {
                        asset = Resources.Load<T>(relativePath);
                    }
                    return asset;
                }
                else
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(path);
                    AssetBundleRequest abr = ab.LoadAssetAsync<T>(Path.GetFileNameWithoutExtension(relativePathLower));
                    asset = (T)abr.asset;
                    if (isUnload)
                        ab.Unload(false);
                    return asset;
                }
            }
            catch (Exception e)
            {
                LogUtils.Log(e.Message);
                return null;
            }
        }
        #endregion
    }
}