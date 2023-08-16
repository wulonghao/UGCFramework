using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class LoadMetadataForAOT
    {
        /// <summary>
        /// 加载AOT泛型补充元数据程序集，AOT中通过反射调用
        /// </summary>
        public static void LoadMetadataForAOTAssemblies(Dictionary<string, byte[]> AOTMetaAssemblyBytes)
        {
            foreach (var dll in AOTMetaAssemblyBytes)
            {
                string fileName = dll.Key;
                byte[] dllBytes = dll.Value;
                if (dllBytes == null)
                {
                    Debug.LogError($"AOT泛型元数据补充文件异常! fileName:" + fileName);
                    continue;
                }
                HybridCLR.LoadImageErrorCode code = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HybridCLR.HomologousImageMode.SuperSet);
                if (code == HybridCLR.LoadImageErrorCode.OK)
                    Debug.Log($"泛型补充, Target;{fileName}");
                else
                    Debug.LogError($"AOT泛型补充元数据加载失败! code:{code} fileName:{fileName}");
            }
        }
    }
}
