using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    void Start()
    {
        //TODO ��������dll�ļ���ɳ�ж�ӦĿ¼
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
