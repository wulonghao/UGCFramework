using UnityEngine;
using UnityEngine.Events;

namespace UGCF.UnityExtend
{
    public class UISwitchAnimation : MonoBehaviour
    {
        /// <summary> 入场动画 </summary>
        [SerializeField] NodeSwitchAnimationType enterAnimationType;
        /// <summary> 入场动画播放时间 </summary>
        [SerializeField] float enterAnimationTime = 1;
        /// <summary> 离场动画 </summary>
        [SerializeField] NodeSwitchAnimationType exitAnimationType;
        /// <summary> 离场动画播放时间 </summary>
        [SerializeField] float exitAnimationTime = 1;
        /// <summary> 是否是弹性动画 </summary>
        [SerializeField, Tooltip("是否是弹性动画")] bool elasticAnimation = false;
        CommonAnimation switchAnimation;

        public bool IsSwitchAnimPlaying { get; set; }
        public NodeSwitchAnimationType EnterAnimationType { get => enterAnimationType; set => enterAnimationType = value; }
        public float EnterAnimationTime { get => enterAnimationTime; set => enterAnimationTime = value; }
        public NodeSwitchAnimationType ExitAnimationType { get => exitAnimationType; set => exitAnimationType = value; }
        public float ExitAnimationTime { get => exitAnimationTime; set => exitAnimationTime = value; }
        public bool ElasticAnimation { get => elasticAnimation; set => elasticAnimation = value; }

        /// <summary>
        /// 播放入场动画
        /// </summary>
        /// <param name="animaFinishUA"></param>
        public bool PlayEnterAnimation(UnityAction animaFinishUA = null)
        {
            if (IsSwitchAnimPlaying) return false;
            if (!ValidEnterAnimation())
            {
                animaFinishUA?.Invoke();
                return true;
            }
            SetSwitchAnimation(EnterAnimationType, true, animaFinishUA);
            PlayAnimation();
            return true;
        }

        /// <summary>
        /// 播放退场动画
        /// </summary>
        /// <param name="animaFinishUA"></param>
        public bool PlayExitAnimation(UnityAction animaFinishUA = null)
        {
            if (IsSwitchAnimPlaying) return false;
            if (!ValidExitAnimation())
            {
                animaFinishUA?.Invoke();
                return true;
            }
            SetSwitchAnimation(ExitAnimationType, false, animaFinishUA);
            PlayAnimation();
            return true;
        }

        public bool ValidEnterAnimation()
        {
            return EnterAnimationType > NodeSwitchAnimationType.None && EnterAnimationTime > 0;
        }

        public bool ValidExitAnimation()
        {
            return ExitAnimationType > NodeSwitchAnimationType.None && ExitAnimationTime > 0;
        }

        void PlayAnimation()
        {
            if (switchAnimation && !IsSwitchAnimPlaying)
            {
                IsSwitchAnimPlaying = true;
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
            if (IsSwitchAnimPlaying) return;
            switchAnimation = GetComponent<CommonAnimation>();
            if (!switchAnimation)
                switchAnimation = gameObject.AddComponent<CommonAnimation>();
            switchAnimation.Clear();
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
                        if (ElasticAnimation)
                            animationScale.ScaleList.Add(Vector3.one * 1.1f);
                        animationScale.ScaleList.Add(Vector3.one);
                        animationScale.PlayTime = EnterAnimationTime;
                    }
                    else
                    {
                        if (ElasticAnimation)
                            animationScale.ScaleList.Insert(0, Vector3.one * 1.1f);
                        animationScale.ScaleList.Insert(0, Vector3.one);
                        animationScale.PlayTime = ExitAnimationTime;
                    }
                    break;
                case NodeSwitchAnimationType.None:
                    action?.Invoke();
                    return;
            }
            switchAnimation.LastEndAction += () =>
            {
                IsSwitchAnimPlaying = false;
                action?.Invoke();
            };
        }

        void SetMoveAnimation(CommonAnimation animation, bool isEnter, Vector3 targetVector)
        {
            CommonAnimationPoint animationPoint = animation.CreatePointAnimation(Vector3.zero);
            if (!isEnter)
            {
                if (ElasticAnimation)
                    animationPoint.PointList.Add(targetVector * -0.03f);
                animationPoint.PointList.Add(targetVector);
                animationPoint.PlayTime = ExitAnimationTime;
            }
            else
            {
                if (ElasticAnimation)
                    animationPoint.PointList.Insert(0, targetVector * -0.03f);
                animationPoint.PointList.Insert(0, targetVector);
                animationPoint.PlayTime = EnterAnimationTime;
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
