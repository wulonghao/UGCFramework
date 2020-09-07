using LitJson;
using UGCF.HotUpdate;
using UGCF.Utils;
using UnityEngine;

namespace UGCF.Manager
{
    public partial class Page : HotFixBaseInheritMono
    {
        /// <summary> 受适配影响的页面主体 </summary>
        [SerializeField] private RectTransform main;
        protected AssetBundle spriteAB;
        public RectTransform Main { get => main; set => main = value; }
        public string ResourceDirectory { get; set; }

        public void InitData(AssetBundle ab, string resourceDirectory)
        {
            spriteAB = ab;
            ResourceDirectory = resourceDirectory;
            gameObject.name = GetType().Name;
        }

        public virtual void Init()
        {
            LogUtils.Log(name + "：Init");
        }

        public virtual void Open()
        {
            LogUtils.Log(name + "：Open");
            TipManager.Instance.CloseAllTip();
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            LogUtils.Log(name + "：Close");
            AudioManager.Instance.StopMusic();
            TipManager.Instance.CloseAllTip();
            DestroyImmediate(gameObject);
        }

        public virtual void OnDestroy()
        {
            if (spriteAB)
                spriteAB.Unload(true);
            Resources.UnloadUnusedAssets();
        }

        public AssetBundle GetSpriteAB()
        {
            return spriteAB;
        }

        public Sprite GetSpriteByDefaultAB(string spriteName)
        {
            return BundleManager.Instance.GetSprite(spriteName, spriteAB);
        }

        public JsonData GetJsonData(string jsonName)
        {
            return BundleManager.Instance.GetCommonJsonData(jsonName, ResourceDirectory);
        }

        public T OpenFloatNode<T>() where T : Node
        {
            T t = NodeManager.OpenFloatNode<T>(ResourceDirectory + "/Prefab");
            if (!t)
                t = NodeManager.OpenFloatNode<T>();
            return t;
        }

        public T OpenNode<T>(bool isAutoPlayEnter = true, bool isCloseLastNode = true) where T : Node
        {
            T t = NodeManager.OpenNode<T>(isAutoPlayEnter, isCloseLastNode, ResourceDirectory + "/Prefab");
            if (!t)
                t = NodeManager.OpenNode<T>();
            return t;
        }

        public GameObject OpenPrefab(string gameObjectName)
        {
            return BundleManager.Instance.GetGameObjectByUI(ResourceDirectory + "/Prefab/" + gameObjectName);
        }
    }
}