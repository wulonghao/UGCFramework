using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UGCF.Utils;

namespace UGCF.UnityExtend
{
    public class CommonAnimation : MonoBehaviour
    {
        #region ...所有字段
        [SerializeField] bool point;
        [SerializeField] bool scale;
        [SerializeField] bool alpha;
        [SerializeField] bool color;
        [SerializeField] bool size;
        [SerializeField] bool angle;
        [SerializeField] bool fillAmount;

        [SerializeField] bool isDisappear;
        [SerializeField] bool isLoop;
        [SerializeField] bool isPingPong;
        [SerializeField] bool isFoward;
        [SerializeField] bool isBackInit;
        [SerializeField] bool isAutoPlay;
        [SerializeField] bool isPlayOnDisable;
        [SerializeField] DisappearType disType;

        [SerializeField] List<CommonAnimationPoint> pointAnimationList = new List<CommonAnimationPoint>();
        [SerializeField] List<CommonAnimationAngle> angleAnimationList = new List<CommonAnimationAngle>();
        [SerializeField] List<CommonAnimationScale> scaleAnimationList = new List<CommonAnimationScale>();
        [SerializeField] List<CommonAnimationColor> colorAnimationList = new List<CommonAnimationColor>();
        [SerializeField] List<CommonAnimationAlpha> alphaAnimationList = new List<CommonAnimationAlpha>();
        [SerializeField] List<CommonAnimationSize> sizeAnimationList = new List<CommonAnimationSize>();
        [SerializeField] List<CommonAnimationFillAmount> fillAmountAnimationList = new List<CommonAnimationFillAmount>();

        private bool isPause;
        private MonoBehaviour playBaseMb;
        private int currentPlayingCount = 0;
        private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
        #endregion

        #region ...所有属性
        public bool Point { get => point; set => point = value; }
        public bool Scale { get => scale; set => scale = value; }
        public bool Alpha { get => alpha; set => alpha = value; }
        public bool Color { get => color; set => color = value; }
        public bool Size { get => size; set => size = value; }
        public bool Angle { get => angle; set => angle = value; }
        public bool FillAmount { get => fillAmount; set => fillAmount = value; }

        public bool IsLoop { get => isLoop; set => isLoop = value; }
        public bool IsPingPong { get => isPingPong; set => isPingPong = value; }
        public bool IsFoward { get => isFoward; set => isFoward = value; }
        public bool IsBackInit { get => isBackInit; set => isBackInit = value; }
        public bool IsAutoPlay { get => isAutoPlay; set => isAutoPlay = value; }
        public bool IsPlayOnDisable { get => isPlayOnDisable; set => isPlayOnDisable = value; }
        public bool IsDisappear { get => isDisappear; set => isDisappear = value; }
        public DisappearType DisType { get => disType; set => disType = value; }

        public UnityAction LastEndAction { get; set; }

        public List<CommonAnimationPoint> PointAnimationList { get => pointAnimationList; set => pointAnimationList = value; }
        public List<CommonAnimationAngle> AngleAnimationList { get => angleAnimationList; set => angleAnimationList = value; }
        public List<CommonAnimationScale> ScaleAnimationList { get => scaleAnimationList; set => scaleAnimationList = value; }
        public List<CommonAnimationColor> ColorAnimationList { get => colorAnimationList; set => colorAnimationList = value; }
        public List<CommonAnimationAlpha> AlphaAnimationList { get => alphaAnimationList; set => alphaAnimationList = value; }
        public List<CommonAnimationSize> SizeAnimationList { get => sizeAnimationList; set => sizeAnimationList = value; }
        public List<CommonAnimationFillAmount> FillAmountAnimationList { get => fillAmountAnimationList; set => fillAmountAnimationList = value; }
        #endregion

        /// <summary>
        /// 播放所有启用了的动画
        /// </summary>
        public void PlayAll()
        {
            PlayPoint();
            PlayAngle();
            PlayScale();
            PlayColor();
            PlayAlpha();
            PlaySize();
            PlayFillAmount();
        }

        /// <summary>
        /// 播放指定索引的位移动画
        /// </summary>
        /// <param name="index">位移动画组中的索引，不传即播放所有位移动画</param>
        public CommonAnimation PlayPoint(int index = -1)
        {
            if (!Point)
                return this;
            if (index < 0)
                return PlayAnimations(PointAnimationList);
            else
                return PlayAnimation(PointAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的旋转动画
        /// </summary>
        /// <param name="index">旋转动画组中的索引，不传即播放所有旋转动画</param>
        public CommonAnimation PlayAngle(int index = -1)
        {
            if (!Angle)
                return this;
            if (index < 0)
                return PlayAnimations(AngleAnimationList);
            else
                return PlayAnimation(AngleAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的缩放动画
        /// </summary>
        /// <param name="index">缩放动画组中的索引，不传即播放所有缩放动画</param>
        public CommonAnimation PlayScale(int index = -1)
        {
            if (!Scale)
                return this;
            if (index < 0)
                return PlayAnimations(ScaleAnimationList);
            else
                return PlayAnimation(ScaleAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的颜色变化动画
        /// </summary>
        /// <param name="index">颜色动画组中的索引，不传即播放所有颜色变化动画</param>
        public CommonAnimation PlayColor(int index = -1)
        {
            if (!Color)
                return this;
            if (index < 0)
                return PlayAnimations(ColorAnimationList);
            else
                return PlayAnimation(ColorAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的透明度变化动画
        /// </summary>
        /// <param name="index">透明度动画组中的索引，不传即播放所有透明度变化动画</param>
        public CommonAnimation PlayAlpha(int index = -1)
        {
            if (!Alpha)
                return this;
            if (index < 0)
                return PlayAnimations(AlphaAnimationList);
            else
                return PlayAnimation(AlphaAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的尺寸变化动画
        /// </summary>
        /// <param name="index">尺寸动画组中的索引，不传即播放所有尺寸变化动画</param>
        public CommonAnimation PlaySize(int index = -1)
        {
            if (!Size)
                return this;
            if (index < 0)
                return PlayAnimations(SizeAnimationList);
            else
                return PlayAnimation(SizeAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的填充动画
        /// </summary>
        /// <param name="index">填充动画组中的索引，不传即播放所有填充动画</param>
        public CommonAnimation PlayFillAmount(int index = -1)
        {
            if (!FillAmount)
                return this;
            if (index < 0)
                return PlayAnimations(FillAmountAnimationList);
            else
                return PlayAnimation(FillAmountAnimationList[index]);
        }

        public CommonAnimationPoint CreatePointAnimation(Vector3 startPoint, CommonAnimationPoint.CASpace caSpace = CommonAnimationPoint.CASpace.RectTransformSelf, bool foward = true, int index = 0)
        {
            Point = true;
            CommonAnimationPoint animationPoint = new CommonAnimationPoint();
            animationPoint.Foward = foward;
            animationPoint.PointList.Add(startPoint);
            animationPoint.CaSpace = caSpace;
            if (PointAnimationList.Count > index)
                PointAnimationList[index] = animationPoint;
            else
                PointAnimationList.Add(animationPoint);
            return animationPoint;
        }

        public CommonAnimationScale CreateScaleAnimation(Vector3 startScale, bool foward = true, int index = 0)
        {
            Scale = true;
            CommonAnimationScale animationScale = new CommonAnimationScale();
            animationScale.Foward = foward;
            animationScale.ScaleList.Add(startScale);
            if (ScaleAnimationList.Count > index)
                ScaleAnimationList[index] = animationScale;
            else
                ScaleAnimationList.Add(animationScale);
            return animationScale;
        }

        public void Pause()
        {
            isPause = true;
        }

        public void Continue()
        {
            isPause = false;
        }

        public void Stop()
        {
            if (playBaseMb)
                playBaseMb.StopAllCoroutines();
        }

        public void Clear()
        {
            pointAnimationList.Clear();
            angleAnimationList.Clear();
            alphaAnimationList.Clear();
            scaleAnimationList.Clear();
            sizeAnimationList.Clear();
            colorAnimationList.Clear();
            fillAmountAnimationList.Clear();
            isLoop = isPingPong = isAutoPlay = isBackInit = isDisappear = isPlayOnDisable = isPause = false;
            isFoward = true;
            LastEndAction = null;
            disType = DisappearType.Destroy;
        }

        CommonAnimation PlayAnimations<T>(List<T> allAnimations) where T : CommonAnimationBase
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return null;
            if (allAnimations.Count == 0)
                return null;

            allAnimations.ForEach((t) => t.CurrentFoward = t.Foward);

            playBaseMb = IsPlayOnDisable ? (MonoBehaviour)UGCFMain.Instance : this;
            string coroutineKey = typeof(T).Name;
            if (coroutines.ContainsKey(coroutineKey))
                playBaseMb.StopCoroutine(coroutines[coroutineKey]);

            int startIndex = 0;
            if (!IsFoward)
                startIndex = allAnimations.Count - 1;
            coroutines[coroutineKey] = playBaseMb.StartCoroutine(Play(allAnimations, IsFoward, startIndex));
            return this;
        }

        CommonAnimation PlayAnimation<T>(T commonAnimation) where T : CommonAnimationBase
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return null;
            if (commonAnimation == null)
                return null;

            commonAnimation.CurrentFoward = commonAnimation.Foward;

            playBaseMb = IsPlayOnDisable ? (MonoBehaviour)UGCFMain.Instance : this;
            string coroutineKey = typeof(T).Name;
            if (coroutines.ContainsKey(coroutineKey))
                playBaseMb.StopCoroutine(coroutines[coroutineKey]);

            coroutines[coroutineKey] = playBaseMb.StartCoroutine(Play(new List<T>() { commonAnimation }, IsFoward));
            return this;
        }

        IEnumerator Play<T>(List<T> animations, bool isTotalFoward, int currentIndex = 0) where T : CommonAnimationBase
        {
            yield return WaitForUtils.WaitFrame;
            currentPlayingCount++;
            isPause = false;
            T currentAnimation = animations[currentIndex];
            if (currentAnimation == null)
            {
                LogUtils.LogError("动画播放失败：" + currentIndex);
                yield break;
            }
            WaitForSecondsRealtime delayWait = WaitForUtils.WaitForSecondsRealtime(currentAnimation.DelayTime);
            if (currentAnimation.DelayTime != 0)
                yield return delayWait;
            if (!this)
                yield break;
            currentAnimation.Init(gameObject);
            float speed = currentAnimation.GetSpeed();//speed为每秒lerp的进度
            int count = currentAnimation.GetAnimationListCount();
            int startIndex = currentAnimation.GetCurrentStartIndex();
            float currentSpeed = 0;
            float progress = currentAnimation.CurrentFoward ? 0 : 1;
            while (currentAnimation.CurrentFoward ? progress < 1 : progress > 0)
            {
                #region ...循环执行动画
                yield return WaitForUtils.WaitFrame;
                if (isPause)
                    continue;
                currentSpeed = speed * Time.deltaTime;
                progress += currentAnimation.CurrentFoward ? currentSpeed : -currentSpeed;
                progress = Mathf.Clamp01(progress);
                currentAnimation.PlayAnimation(startIndex, progress);
                //一段动画播放结束
                if (currentAnimation.CurrentFoward ? progress >= 1 : progress <= 0)
                {
                    progress = currentAnimation.CurrentFoward ? 0 : 1;
                    if (currentAnimation.CurrentFoward ? startIndex + 2 < count : startIndex > 0)
                        startIndex += currentAnimation.CurrentFoward ? 1 : -1;
                    else
                    { //一组动画播放结束
                        currentPlayingCount--;
                        currentAnimation.PlayEndAction();
                        if (currentAnimation.PingPong)
                            currentAnimation.CurrentFoward = !currentAnimation.CurrentFoward;
                        currentIndex += isTotalFoward ? 1 : -1;
                        if (isTotalFoward ? currentIndex == animations.Count : currentIndex < 0)
                        {
                            if (IsLoop)
                            {
                                if (IsPingPong)
                                    isTotalFoward = !isTotalFoward;
                                if (isTotalFoward)
                                    playBaseMb.StartCoroutine(Play(animations, isTotalFoward));
                                else
                                    playBaseMb.StartCoroutine(Play(animations, isTotalFoward, animations.Count - 1));
                            }
                            else
                            {
                                if (IsBackInit)
                                {
                                    if (isTotalFoward)
                                        animations[0].Init(gameObject);
                                    else
                                        animations[animations.Count - 1].Init(gameObject);
                                }
                                if (currentPlayingCount <= 0)
                                {
                                    LastEndAction?.Invoke();
                                    if (IsDisappear)
                                    {
                                        if (DisType == DisappearType.Destroy)
                                            Destroy(gameObject);
                                        else
                                            gameObject.SetActive(false);
                                    }
                                }
                                coroutines.Remove(typeof(T).Name);
                            }
                        }
                        else
                        {
                            if (currentIndex < animations.Count)
                                playBaseMb.StartCoroutine(Play(animations, isTotalFoward, currentIndex));
                        }
                        break;
                    }
                }
                #endregion
            }
        }

        void OnEnable()
        {
            if (IsPlayOnDisable && currentPlayingCount > 0)
                return;
            if (IsAutoPlay)
                PlayAll();
            else
            {
                if (Point)
                    PlayAnimations(PointAnimationList.FindAll((cac) => cac.AutoPlay));
                if (Angle)
                    PlayAnimations(AngleAnimationList.FindAll((cac) => cac.AutoPlay));
                if (Scale)
                    PlayAnimations(ScaleAnimationList.FindAll((cac) => cac.AutoPlay));
                if (Color)
                    PlayAnimations(ColorAnimationList.FindAll((cac) => cac.AutoPlay));
                if (Alpha)
                    PlayAnimations(AlphaAnimationList.FindAll((cac) => cac.AutoPlay));
                if (Size)
                    PlayAnimations(SizeAnimationList.FindAll((cac) => cac.AutoPlay));
                if (FillAmount)
                    PlayAnimations(FillAmountAnimationList.FindAll((cac) => cac.AutoPlay));
            }
        }

        void OnDisable()
        {
            if (!IsPlayOnDisable && playBaseMb)
                playBaseMb.StopAllCoroutines();
        }

        /// <summary>
        /// 消失方式
        /// </summary>
        public enum DisappearType
        {
            Destroy,
            Disable
        }
    }
}
