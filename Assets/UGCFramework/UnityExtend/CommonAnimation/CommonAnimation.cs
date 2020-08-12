using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace UGCF.UnityExtend
{
    public class CommonAnimation : MonoBehaviour
    {
        public bool point, scale, alpha, color, size, angle, fillAmount;
        public bool isLoop, isPingPong, isFoward = true, isBackInit, isAutoPlay, isPlayOnDisable, isDisappear;
        public DisappearType disType;
        public UnityAction lastEndAction;
        public List<CommonAnimationPoint> pointAnimationList = new List<CommonAnimationPoint>();
        public List<CommonAnimationAngle> angleAnimationList = new List<CommonAnimationAngle>();
        public List<CommonAnimationScale> scaleAnimationList = new List<CommonAnimationScale>();
        public List<CommonAnimationColor> colorAnimationList = new List<CommonAnimationColor>();
        public List<CommonAnimationAlpha> alphaAnimationList = new List<CommonAnimationAlpha>();
        public List<CommonAnimationSize> sizeAnimationList = new List<CommonAnimationSize>();
        public List<CommonAnimationFillAmount> fillAmountAnimationList = new List<CommonAnimationFillAmount>();
        bool isPause, isStop;

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
            if (!point)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(pointAnimationList.ConvertAll(new Converter<CommonAnimationPoint, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(pointAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的旋转动画
        /// </summary>
        /// <param name="index">旋转动画组中的索引，不传即播放所有旋转动画</param>
        public CommonAnimation PlayAngle(int index = -1)
        {
            if (!angle)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(angleAnimationList.ConvertAll(new Converter<CommonAnimationAngle, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(angleAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的缩放动画
        /// </summary>
        /// <param name="index">缩放动画组中的索引，不传即播放所有缩放动画</param>
        public CommonAnimation PlayScale(int index = -1)
        {
            if (!scale)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(scaleAnimationList.ConvertAll(new Converter<CommonAnimationScale, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(scaleAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的颜色变化动画
        /// </summary>
        /// <param name="index">颜色动画组中的索引，不传即播放所有颜色变化动画</param>
        public CommonAnimation PlayColor(int index = -1)
        {
            if (!color)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(colorAnimationList.ConvertAll(new Converter<CommonAnimationColor, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(colorAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的透明度变化动画
        /// </summary>
        /// <param name="index">透明度动画组中的索引，不传即播放所有透明度变化动画</param>
        public CommonAnimation PlayAlpha(int index = -1)
        {
            if (!alpha)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(alphaAnimationList.ConvertAll(new Converter<CommonAnimationAlpha, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(alphaAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的尺寸变化动画
        /// </summary>
        /// <param name="index">尺寸动画组中的索引，不传即播放所有尺寸变化动画</param>
        public CommonAnimation PlaySize(int index = -1)
        {
            if (!size)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(sizeAnimationList.ConvertAll(new Converter<CommonAnimationSize, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(sizeAnimationList[index]);
        }

        /// <summary>
        /// 播放指定索引的填充动画
        /// </summary>
        /// <param name="index">填充动画组中的索引，不传即播放所有填充动画</param>
        public CommonAnimation PlayFillAmount(int index = -1)
        {
            if (!fillAmount)
                return this;
            if (index < 0)
                return PlayAllByAnimationType(fillAmountAnimationList.ConvertAll(new Converter<CommonAnimationFillAmount, CommonAnimationBase>(ca => ca)));
            else
                return PlayOneByAnimation(fillAmountAnimationList[index]);
        }

        public CommonAnimationPoint CreatePointAnimation(Vector3 startPoint, CommonAnimationPoint.CASpace caSpace = CommonAnimationPoint.CASpace.RectTransformSelf, bool foward = true, int index = 0)
        {
            point = true;
            CommonAnimationPoint animationPoint = new CommonAnimationPoint();
            animationPoint.foward = foward;
            animationPoint.pointList.Add(startPoint);
            animationPoint.caSpace = caSpace;
            if (pointAnimationList.Count > index)
                pointAnimationList[index] = animationPoint;
            else
                pointAnimationList.Add(animationPoint);
            return animationPoint;
        }

        public CommonAnimationScale CreateScaleAnimation(Vector3 startScale, bool foward = true, int index = 0)
        {
            scale = true;
            CommonAnimationScale animationScale = new CommonAnimationScale();
            animationScale.foward = foward;
            animationScale.scaleList.Add(startScale);
            if (scaleAnimationList.Count > index)
                scaleAnimationList[index] = animationScale;
            else
                scaleAnimationList.Add(animationScale);
            return animationScale;
        }

        #region ...内部函数
        MonoBehaviour playBaseMb;
        CommonAnimation PlayAllByAnimationType(List<CommonAnimationBase> allAnimations)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return this;
            if (allAnimations.Count == 0)
                return this;
            if (!isStop) Stop();
            playBaseMb = isPlayOnDisable ? (MonoBehaviour)PageManager.Instance : this;
            if (isFoward)
                playBaseMb.StartCoroutine(PlayAnimation(allAnimations, isFoward));
            else
                playBaseMb.StartCoroutine(PlayAnimation(allAnimations, isFoward, allAnimations.Count - 1));
            return this;
        }

        CommonAnimation PlayOneByAnimation(CommonAnimationBase commonAnimation)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
                return this;
            if (commonAnimation == null)
                return this;
            if (!isStop) Stop();
            playBaseMb = isPlayOnDisable ? (MonoBehaviour)PageManager.Instance : this;
            playBaseMb.StartCoroutine(PlayAnimation(new List<CommonAnimationBase>() { commonAnimation }, isFoward));
            return this;
        }

        int currentPlayingCount = 0;
        /// <summary>
        /// 播放指定动画
        /// </summary>
        /// <param name="type">动画类型</param>
        /// <param name="delayTime">延迟时间</param>
        /// <param name="time">播放总时长</param>
        /// <param name="space">坐标空间</param>
        /// <returns></returns>
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
            WaitForSecondsRealtime delayWait = WaitForUtils.WaitForSecondsRealtime(currentAnimation.delayTime);
            if (currentAnimation.delayTime != 0)
                yield return delayWait;
            if (!this)
                yield break;
            currentAnimation.Init(gameObject);
            float speed = currentAnimation.GetSpeed();//speed为每秒lerp的进度
            int count = currentAnimation.GetAnimationListCount();
            int startIndex = currentAnimation.GetCurrentStartIndex();
            float currentSpeed = 0;
            float progress = currentAnimation.foward ? 0 : 1;
            while (currentAnimation.foward ? progress < 1 : progress > 0)
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
                progress += currentAnimation.foward ? currentSpeed : -currentSpeed;
                progress = Mathf.Clamp01(progress);
                currentAnimation.PlayAnimation(startIndex, progress);
                //一段动画播放结束
                if (currentAnimation.foward ? progress >= 1 : progress <= 0)
                {
                    progress = currentAnimation.foward ? 0 : 1;
                    if (currentAnimation.foward ? startIndex + 2 < count : startIndex > 0)
                        startIndex += currentAnimation.foward ? 1 : -1;
                    else
                    { //一组动画播放结束
                        currentPlayingCount--;
                        currentAnimation.PlayEndAction();
                        if (currentAnimation.pingPong)
                            currentAnimation.foward = !currentAnimation.foward;
                        currentIndex += isFoward ? 1 : -1;
                        if (isFoward ? currentIndex == animations.Count : currentIndex < 0)
                        {
                            if (isLoop)
                            {
                                if (isPingPong)
                                    isFoward = !isFoward;
                                if (isFoward)
                                    playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward));
                                else
                                    playBaseMb.StartCoroutine(PlayAnimation(animations, isFoward, animations.Count - 1));
                            }
                            else
                            {
                                if (isBackInit)
                                {
                                    if (isFoward)
                                        animations[0].Init(gameObject);
                                    else
                                        animations[animations.Count - 1].Init(gameObject);
                                }
                                if (currentPlayingCount <= 0)
                                {
                                    lastEndAction?.Invoke();
                                    if (isDisappear)
                                    {
                                        if (disType == DisappearType.Destroy)
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
            if (isPlayOnDisable && currentPlayingCount > 0)
                return;
            if (isAutoPlay)
                PlayAll();
            else
            {
                if (point)
                    PlayAllByAnimationType(pointAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationPoint, CommonAnimationBase>(ca => ca)));
                if (angle)
                    PlayAllByAnimationType(angleAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationAngle, CommonAnimationBase>(ca => ca)));
                if (scale)
                    PlayAllByAnimationType(scaleAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationScale, CommonAnimationBase>(ca => ca)));
                if (color)
                    PlayAllByAnimationType(colorAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationColor, CommonAnimationBase>(ca => ca)));
                if (alpha)
                    PlayAllByAnimationType(alphaAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationAlpha, CommonAnimationBase>(ca => ca)));
                if (size)
                    PlayAllByAnimationType(sizeAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationSize, CommonAnimationBase>(ca => ca)));
                if (fillAmount)
                    PlayAllByAnimationType(fillAmountAnimationList.FindAll((cac) => { return cac.autoPlay; }).ConvertAll(new Converter<CommonAnimationFillAmount, CommonAnimationBase>(ca => ca)));
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
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
            pointAnimationList.Clear();
            angleAnimationList.Clear();
            alphaAnimationList.Clear();
            scaleAnimationList.Clear();
            sizeAnimationList.Clear();
            colorAnimationList.Clear();
            fillAmountAnimationList.Clear();
            isLoop = isPingPong = isAutoPlay = isBackInit = isDisappear = isPlayOnDisable = isPause = isStop = false;
            isFoward = true;
            lastEndAction = null;
            disType = DisappearType.Destroy;
        }
        #endregion

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
