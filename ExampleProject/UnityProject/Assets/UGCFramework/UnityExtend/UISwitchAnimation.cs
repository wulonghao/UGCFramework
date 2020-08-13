using System.Collections;
using System.Collections.Generic;
using UGCF.UnityExtend;
using UnityEngine;
using UnityEngine.Events;

namespace UGCF.UnityExtend
{
    public class UISwitchAnimation : MonoBehaviour
    {
        #region ...字段定义
        /// <summary> 入场动画 </summary>
        [SerializeField] NodeSwitchAnimationType enterAnimationType;
        /// <summary> 入场动画播放时间 </summary>
        [SerializeField] float enterAnimationTime = 1;
        /// <summary> 离场动画 </summary>
        [SerializeField] public NodeSwitchAnimationType exitAnimationType;
        /// <summary> 离场动画播放时间 </summary>
        [SerializeField] float exitAnimationTime = 1;
        /// <summary> 是否是弹性动画 </summary>
        [SerializeField, Tooltip("是否是弹性动画")] bool elasticAnimation = false;
        /// <summary> 动画播放中 </summary>
        [HideInInspector] public bool isSwitchAnimPlaying;
        CommonAnimation switchAnimation;
        #endregion

        /// <summary>
        /// 播放入场动画
        /// </summary>
        /// <param name="animaFinishUA"></param>
        public bool PlayEnterAnimation(UnityAction animaFinishUA = null)
        {
            if (isSwitchAnimPlaying) return false;
            if (!ValidEnterAnimation())
            {
                animaFinishUA?.Invoke();
                return true;
            }
            SetSwitchAnimation(enterAnimationType, true, animaFinishUA);
            PlayAnimation();
            return true;
        }

        /// <summary>
        /// 播放退场动画
        /// </summary>
        /// <param name="animaFinishUA"></param>
        public bool PlayExitAnimation(UnityAction animaFinishUA = null)
        {
            if (isSwitchAnimPlaying) return false;
            if (!ValidExitAnimation())
            {
                animaFinishUA?.Invoke();
                return true;
            }
            SetSwitchAnimation(exitAnimationType, false, animaFinishUA);
            PlayAnimation();
            return true;
        }

        public bool ValidEnterAnimation()
        {
            return enterAnimationType > NodeSwitchAnimationType.None && enterAnimationTime > 0;
        }

        public bool ValidExitAnimation()
        {
            return exitAnimationType > NodeSwitchAnimationType.None && exitAnimationTime > 0;
        }

        void PlayAnimation()
        {
            if (switchAnimation && !isSwitchAnimPlaying)
            {
                isSwitchAnimPlaying = true;
                switchAnimation.PlayAll();
            }
        }

        /// <summary>
        /// 设置切换动画参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isEnter"></param>
        /// <param name="action"></param>
        void SetSwitchAnimation(NodeSwitchAnimationType type, bool isEnter = true, UnityAction action = null)
        {
            if (isSwitchAnimPlaying) return;
            switchAnimation = GetComponent<CommonAnimation>();
            if (!switchAnimation)
                switchAnimation = gameObject.AddComponent<CommonAnimation>();
            switchAnimation.Clear();
            switchAnimation.isPlayOnDisable = true;
            switch (type)
            {
                case NodeSwitchAnimationType.MoveFromLeft:
                    SetMoveAnimation(switchAnimation, isEnter, Vector3.left * UGCFMain.canvasWidth);
                    break;
                case NodeSwitchAnimationType.MoveFromDown:
                    SetMoveAnimation(switchAnimation, isEnter, Vector3.down * UGCFMain.canvasHeight);
                    break;
                case NodeSwitchAnimationType.MoveFromRight:
                    SetMoveAnimation(switchAnimation, isEnter, Vector3.right * UGCFMain.canvasWidth);
                    break;
                case NodeSwitchAnimationType.Zoom:
                    CommonAnimationScale animationScale = switchAnimation.CreateScaleAnimation(Vector3.zero);
                    if (isEnter)
                    {
                        if (elasticAnimation)
                            animationScale.scaleList.Add(Vector3.one * 1.1f);
                        animationScale.scaleList.Add(Vector3.one);
                        animationScale.playTime = enterAnimationTime;
                    }
                    else
                    {
                        if (elasticAnimation)
                            animationScale.scaleList.Insert(0, Vector3.one * 1.1f);
                        animationScale.scaleList.Insert(0, Vector3.one);
                        animationScale.playTime = exitAnimationTime;
                    }
                    break;
                case NodeSwitchAnimationType.None:
                    action?.Invoke();
                    return;
            }
            switchAnimation.lastEndAction += () =>
            {
                isSwitchAnimPlaying = false;
                action?.Invoke();
            };
        }

        void SetMoveAnimation(CommonAnimation animation, bool isEnter, Vector3 targetVector)
        {
            CommonAnimationPoint animationPoint = animation.CreatePointAnimation(Vector3.zero);
            if (!isEnter)
            {
                if (elasticAnimation)
                    animationPoint.pointList.Add(targetVector * -0.03f);
                animationPoint.pointList.Add(targetVector);
                animationPoint.playTime = exitAnimationTime;
            }
            else
            {
                if (elasticAnimation)
                    animationPoint.pointList.Insert(0, targetVector * -0.03f);
                animationPoint.pointList.Insert(0, targetVector);
                animationPoint.playTime = enterAnimationTime;
            }
        }
    }

    /// <summary>
    /// 窗口入场离场动画方式
    /// </summary>
    public enum NodeSwitchAnimationType
    {
        None,
        MoveFromLeft,//屏幕左侧入场或离场
        MoveFromDown,//屏幕下方入场或离场
        MoveFromRight,//屏幕右侧入场或离场
        MoveFromUp,//屏幕上方入场或离场
        Zoom,//缩放（进场放大，离场缩小）
    }
}
