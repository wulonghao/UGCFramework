using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class LoadMetadataForAOT
    {
        /// <summary>
        /// ����AOT���Ͳ���Ԫ���ݳ��򼯣�AOT��ͨ���������
        /// </summary>
        public static void LoadMetadataForAOTAssemblies(Dictionary<string, byte[]> AOTMetaAssemblyBytes)
        {
            foreach (var dll in AOTMetaAssemblyBytes)
            {
                string fileName = dll.Key;
                byte[] dllBytes = dll.Value;
                if (dllBytes == null)
                {
                    Debug.LogError($"AOT����Ԫ���ݲ����ļ��쳣! fileName:" + fileName);
                    continue;
                }
                HybridCLR.LoadImageErrorCode code = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HybridCLR.HomologousImageMode.SuperSet);
                if (code == HybridCLR.LoadImageErrorCode.OK)
                    Debug.Log($"���Ͳ���, Target;{fileName}");
                else
                    Debug.LogError($"AOT���Ͳ���Ԫ���ݼ���ʧ��! code:{code} fileName:{fileName}");
            }
        }
    }
}
