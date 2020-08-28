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

        private bool isPause, isStop;
        private MonoBehaviour playBaseMb;
        private int currentPlayingCount = 0;
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
                return PlayAllByAnimationType(PointAnimationList.ConvertAll(new Converter<CommonAnimationPoint, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(PointAnimationList[index]);
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
                return PlayAllByAnimationType(AngleAnimationList.ConvertAll(new Converter<CommonAnimationAngle, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(AngleAnimationList[index]);
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
                return PlayAllByAnimationType(ScaleAnimationList.ConvertAll(new Converter<CommonAnimationScale, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(ScaleAnimationList[index]);
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
                return PlayAllByAnimationType(ColorAnimationList.ConvertAll(new Converter<CommonAnimationColor, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(ColorAnimationList[index]);
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
                return PlayAllByAnimationType(AlphaAnimationList.ConvertAll(new Converter<CommonAnimationAlpha, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(AlphaAnimationList[index]);
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
                return PlayAllByAnimationType(SizeAnimationList.ConvertAll(new Converter<CommonAnimationSize, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(SizeAnimationList[index]);
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
                return PlayAllByAnimationType(FillAmountAnimationList.ConvertAll(new Converter<CommonAnimationFillAmount, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(FillAmountAnimationList[index]);
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
            isStop = true;
        }

        public void Clear()
        {
            PointAnimationList.Clear();
            AngleAnimationList.Clear();
            AlphaAnimationList.Clear();
            ScaleAnimationList.Clear();
            SizeAnimationList.Clear();
            ColorAnimationList.Clear();
            FillAmountAnimationList.Clear();
            IsLoop = IsPingPong = IsAutoPlay = IsBackInit = IsDisappear = IsPlayOnDisable = isPause = isStop = false;
            IsFoward = true;
            LastEndAction = null;
            DisType = DisappearType.Destroy;
        }

        CommonAnimation PlayAllByAnimationType(List<CommonAnimationBase> allAnimations)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return this;
            if (allAnimations.Count == 0)
                return this;
            if (!isStop) Stop();
            playBaseMb = IsPlayOnDisable ? (MonoBehaviour)UGCFMain.Instance : this;
            if (IsFoward)
                playBaseMb.StartCoroutine(PlayAnimation(allAnimations, IsFoward));
            else
                playBaseMb.StartCoroutine(PlayAnimation(allAnimations, IsFoward, allAnimations.Count - 1));
            return this;
        }

        CommonAnimation PlayOneByAnimation(CommonAnimationBase commonAnimation)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return this;
            if (commonAnimation == null)
                return this;
            if (!isStop) Stop();
            playBaseMb = IsPlayOnDisable ? (MonoBehaviour)UGCFMain.Instance : this;
            playBaseMb.StartCoroutine(PlayAnimation(new List<CommonAnimationBase>() { commonAnimation }, IsFoward));
            return this;
        }

        IEnumerator PlayAnimation(List<CommonAnimationBase> animations, bool isStartFoward, int currentIndex = 0)
        {
            currentPlayingCount++;
            bool isFoward = isStartFoward;
            isStop = false;
            isPause = false;
            CommonAnimationBase currentAnimation = animations[currentIndex];
            if (currentAnimation == null)
            {
                playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward, animations.Count - 1));
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
            float progress = currentAnimation.Foward ? 0 : 1;
            while (currentAnimation.Foward ? progress < 1 : progress > 0)
            {
                #region ...循环执行动画
                yield return WaitForUtils.WaitFrame;
                if (isPause)
                {
                    yield return WaitForUtils.WaitFrame;
                    continue;
                }
                if (isStop || !this)
                    yield break;
                currentSpeed = speed * Time.deltaTime;
                progress += currentAnimation.Foward ? currentSpeed : -currentSpeed;
                progress = Mathf.Clamp01(progress);
                currentAnimation.PlayAnimation(startIndex, progress);
                //一段动画播放结束
                if (currentAnimation.Foward ? progress >= 1 : progress <= 0)
                {
                    progress = currentAnimation.Foward ? 0 : 1;
                    if (currentAnimation.Foward ? startIndex + 2 < count : startIndex > 0)
                        startIndex += currentAnimation.Foward ? 1 : -1;
                    else
                    { //一组动画播放结束
                        currentPlayingCount--;
                        currentAnimation.PlayEndAction();
                        if (currentAnimation.PingPong)
                            currentAnimation.Foward = !currentAnimation.Foward;
                        currentIndex += isFoward ? 1 : -1;
                        if (isFoward ? currentIndex == animations.Count : currentIndex < 0)
                        {
                            if (IsLoop)
                            {
                                if (IsPingPong)
                                    isFoward = !isFoward;
                                if (isFoward)
                                    playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward));
                                else
                                    playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward, animations.Count - 1));
                            }
                            else
                            {
                                if (IsBackInit)
                                {
                                    if (isFoward)
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
                            }
                        }
                        else
                        {
                            if (currentIndex < animations.Count)
                                playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward, currentIndex));
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
                    PlayAllByAnimationType(PointAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationPoint, CommonAnimationBase>(ca => ca)));
                if (Angle)
                    PlayAllByAnimationType(AngleAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationAngle, CommonAnimationBase>(ca => ca)));
                if (Scale)
                    PlayAllByAnimationType(ScaleAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationScale, CommonAnimationBase>(ca => ca)));
                if (Color)
                    PlayAllByAnimationType(ColorAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationColor, CommonAnimationBase>(ca => ca)));
                if (Alpha)
                    PlayAllByAnimationType(AlphaAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationAlpha, CommonAnimationBase>(ca => ca)));
                if (Size)
                    PlayAllByAnimationType(SizeAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationSize, CommonAnimationBase>(ca => ca)));
                if (FillAmount)
                    PlayAllByAnimationType(FillAmountAnimationList.FindAll((cac) => { return cac.AutoPlay; }).ConvertAll(new Converter<CommonAnimationFillAmount, CommonAnimationBase>(ca => ca)));
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
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
