using LitJson;
using UGCF.HotUpdate;
using UGCF.UnityExtend;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace UGCF.Manager
{
    public partial class Node : HotFixBaseInheritMono
    {
        #region ...字段
        /// <summary> 受适配影响的页面主体 </summary>
        [SerializeField] private RectTransform main;
        /// <summary> 播放动画的主体 </summary>
        [SerializeField] private UISwitchAnimation animationMian;
        /// <summary> 关闭按钮 </summary>
        [SerializeField] private GameObject btnClose;
        /// <summary> 蒙层 </summary>
        [SerializeField] private GameObject maskLayer;
        /// <summary> 点击蒙层是否关闭Node </summary>
        [SerializeField] private bool isClickMaskClose = false;
        /// <summary> 是否响应设备键盘（用于返回等） </summary>
        [SerializeField] private bool isRespondDeviceKeyboard = false;
        protected AssetBundle spriteAB;
        #endregion

        #region ...属性
        public RectTransform Main { get => main; set => main = value; }
        public UISwitchAnimation AnimationMian { get => animationMian; set => animationMian = value; }
        public GameObject BtnClose { get => btnClose; set => btnClose = value; }
        public GameObject MaskLayer { get => maskLayer; set => maskLayer = value; }
        public bool IsClickMaskClose { get => isClickMaskClose; set => isClickMaskClose = value; }
        public bool IsRespondDeviceKeyboard { get => isRespondDeviceKeyboard; set => isRespondDeviceKeyboard = value; }
        public string NodePath { get; set; }
        public string DirectoryPath { get; set; }
        #endregion

        public virtual void Init()
        {
            LogUtils.Log(name + "：Init");
            if (IsClickMaskClose && MaskLayer)
                UGUIEventListener.Get(MaskLayer).OnClick += delegate { Close(); };
            if (BtnClose)
                UGUIEventListener.Get(BtnClose).OnClick += delegate { Close(); };
            gameObject.SetActive(false);
        }

        public virtual void Open()
        {
            LogUtils.Log(name + "：Open");
            gameObject.SetActive(true);
        }

        /// <summary>
        /// UI入场动画播放完毕后执行，无动画则立刻执行
        /// </summary>
        public virtual void EnterAnimationEndAction()
        {
            RequestData();
        }

        /// <summary>
        /// UI离场动画播放完毕后执行，无动画则立刻执行
        /// </summary>
        public virtual void ExitAnimationEndAction() { }

        /// <summary>
        /// 请求数据专用
        /// </summary>
        public virtual void RequestData() { }

        /// <summary>
        /// 播放入场或离场动画
        /// </summary>
        /// <param name="isEnter">true为入场动画，false为离场动画</param>
        /// <param name="isCloseAfterFinish">是否在动画播放完毕后执行Close，仅isEnter = false时有效</param>
        public bool PlayAnimation(bool isEnter, UnityAction callback = null)
        {
            if (isEnter)
            {
                callback += EnterAnimationEndAction;
                if (!AnimationMian)
                {
                    callback?.Invoke();
                    return true;
                }
                return AnimationMian.PlayEnterAnimation(callback);
            }
            else
            {
                callback += ExitAnimationEndAction;
                if (!AnimationMian)
                {
                    callback?.Invoke();
                    return true;
                }
                return AnimationMian.PlayExitAnimation(callback);
            }
        }

        /// <summary>
        /// 关闭Node
        /// </summary>
        /// <param name="isActiveClose">是否为主动关闭</param>
        public virtual void Close(bool isActiveClose = true)
        {
            LogUtils.Log(name + "：Close");
            if (!this)
                return;
            bool closeSuccess = false;
            if (isActiveClose && AnimationMian && AnimationMian.ExitAnimationType != NodeSwitchAnimationType.None)
            {
                closeSuccess = PlayAnimation(false, () => CloseNode(isActiveClose));
            }
            else
            {
                CloseNode(isActiveClose);
                closeSuccess = true;
            }

            if (closeSuccess)
            {
                NodeManager.CurrentNode = NodeManager.GetLastNode(false, false);
            }
        }

        void CloseNode(bool isActiveClose)
        {
            DestroyImmediate(gameObject);
        }

        void OnDestroy()
        {
            if (spriteAB)
                spriteAB.Unload(true);
            Resources.UnloadUnusedAssets();
        }

        #region ...工具函数
        public void SetSpriteAB(AssetBundle ab)
        {
            spriteAB = ab;
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
            return BundleManager.Instance.GetCommonJsonData(jsonName, DirectoryPath + "/" + NodePath);
        }

        public GameObject GetPrefab(string gameObjectName)
        {
            return BundleManager.Instance.GetGameObjectByUI(DirectoryPath + "/" + NodePath + "/" + gameObjectName + "/Prefab");
        }
        #endregion
    }
}