using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    void Start()
    {
        //TODO 下载最新dll文件到沙盒对应目录
        RefTypes.InstantiateAOTRef();
        LoadHotUpdateDllAndAOT();
        LoadMainScene();
    }

    void LoadHotUpdateDllAndAOT()
    {
#if !UNITY_EDITOR
        HotUpdate.HotUpdateLoader.LoadHotUpdate();
#endif
    }

    void LoadMainScene()
    {
        //AssetBundle.LoadFromFile(Application.persistentDataPath + "/AssetBundle/scene/main");
        AssetBundle main = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundle/scene/main");
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        main.Unload(false);
    }
}
