using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

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
    /// <param name="index">位移动画组中的索引</param>
    public void PlayPoint(int index = -1)
    {
        if (!point)
            return;
        if (index < 0)
            PlayAllByAnimationType(pointAnimationList.ConvertAll(new Converter<CommonAnimationPoint, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(pointAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的旋转动画
    /// </summary>
    /// <param name="index">旋转动画组中的索引</param>
    public void PlayAngle(int index = -1)
    {
        if (!angle)
            return;
        if (index < 0)
            PlayAllByAnimationType(angleAnimationList.ConvertAll(new Converter<CommonAnimationAngle, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(angleAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的缩放动画
    /// </summary>
    /// <param name="index">缩放动画组中的索引</param>
    public void PlayScale(int index = -1)
    {
        if (!scale)
            return;
        if (index < 0)
            PlayAllByAnimationType(scaleAnimationList.ConvertAll(new Converter<CommonAnimationScale, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(scaleAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的颜色变化动画
    /// </summary>
    /// <param name="index">颜色动画组中的索引</param>
    public void PlayColor(int index = -1)
    {
        if (!color)
            return;
        if (index < 0)
            PlayAllByAnimationType(colorAnimationList.ConvertAll(new Converter<CommonAnimationColor, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(colorAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的透明度变化动画
    /// </summary>
    /// <param name="index">透明度动画组中的索引</param>
    public void PlayAlpha(int index = -1)
    {
        if (!alpha)
            return;
        if (index < 0)
            PlayAllByAnimationType(alphaAnimationList.ConvertAll(new Converter<CommonAnimationAlpha, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(alphaAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的尺寸变化动画
    /// </summary>
    /// <param name="index">尺寸动画组中的索引</param>
    public void PlaySize(int index = -1)
    {
        if (!size)
            return;
        if (index < 0)
            PlayAllByAnimationType(sizeAnimationList.ConvertAll(new Converter<CommonAnimationSize, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(sizeAnimationList[index]);
    }

    /// <summary>
    /// 播放指定索引的填充动画
    /// </summary>
    /// <param name="index">填充动画组中的索引</param>
    public void PlayFillAmount(int index = -1)
    {
        if (!fillAmount)
            return;
        if (index < 0)
            PlayAllByAnimationType(fillAmountAnimationList.ConvertAll(new Converter<CommonAnimationFillAmount, CommonAnimationBase>(ca => ca)));
        else
            PlayOneByAnimation(fillAmountAnimationList[index]);
    }

    MonoBehaviour playBaseMb;
    void PlayAllByAnimationType(List<CommonAnimationBase> allAnimations)
    {
        if (!gameObject || !gameObject.activeInHierarchy)
            return;
        if (allAnimations.Count == 0)
            return;
        if (!isStop) Stop();
        playBaseMb = isPlayOnDisable ? (MonoBehaviour)PageManager.Instance : this;
        if (isFoward)
            playBaseMb.StartCoroutine(PlayAnimation(allAnimations, isFoward));
        else
            playBaseMb.StartCoroutine(PlayAnimation(allAnimations, isFoward, allAnimations.Count - 1));
    }

    void PlayOneByAnimation(CommonAnimationBase commonAnimation)
    {
        if (!gameObject || !gameObject.activeInHierarchy)
            return;
        if (commonAnimation == null)
            return;
        if (!isStop) Stop();
        playBaseMb = isPlayOnDisable ? (MonoBehaviour)PageManager.Instance : this;
        playBaseMb.StartCoroutine(PlayAnimation(new List<CommonAnimationBase>() { commonAnimation }, isFoward));
    }

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
        bool isFoward = isStartFoward;
        isStop = false;
        isPause = false;
        CommonAnimationBase currentAnimation = animations[currentIndex];
        WaitForSecondsRealtime delayWait = new WaitForSecondsRealtime(currentAnimation.delayTime);
        if (currentAnimation.delayTime != 0)
            yield return delayWait;
        currentAnimation.Init(gameObject);
        float speed = currentAnimation.GetSpeed();//speed为每秒lerp的进度
        int count = currentAnimation.GetAnimationListCount();
        int startIndex = currentAnimation.GetCurrentStartIndex();
        float currentSpeed = 0;
        float progress = currentAnimation.foward ? 0 : 1;
        while (currentAnimation.foward ? progress < 1 : progress > 0)
        {
            #region ...循环执行动画
            if (isStop)
                break;
            yield return WaitForUtils.WaitFrame;
            if (isPause)
            {
                yield return WaitForUtils.WaitFrame;
                continue;
            }
            if (!this)
                yield break;
            currentSpeed = speed * Time.deltaTime;
            progress += currentAnimation.foward ? currentSpeed : -currentSpeed;
            progress = Mathf.Clamp01(progress);
            currentAnimation.PlayAnimation(startIndex, progress);
            //动画播放结束
            if (currentAnimation.foward ? progress >= 1 : progress <= 0)
            {
                progress = currentAnimation.foward ? 0 : 1;
                if (currentAnimation.foward ? startIndex + 2 < count : startIndex > 0)
                    startIndex += currentAnimation.foward ? 1 : -1;
                else
                {
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
                            if (lastEndAction != null)
                                lastEndAction();
                            if (isDisappear)
                            {
                                if (disType == DisappearType.Destroy)
                                    Destroy(gameObject);
                                else
                                    gameObject.SetActive(false);
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
        isLoop = isPingPong = isAutoPlay = isBackInit = isDisappear = isPause = isStop = false;
        isFoward = true;
        disType = DisappearType.Destroy;
    }

    /// <summary>
    /// 消失方式
    /// </summary>
    public enum DisappearType
    {
        Destroy,
        Disable
    }

    public enum AnimationType
    {
        /// <summary> 位移 </summary>
        Point = 1,
        /// <summary> 旋转 </summary>
        Angle,
        /// <summary> 缩放 </summary>
        Scale,
        /// <summary> 颜色 </summary>
        Color,
        /// <summary> 透明度 </summary>
        Alpha,
        /// <summary> 尺寸 </summary>
        Size,
        /// <summary> 图片填充 </summary>
        FillAmount
    }

    public enum CASpace
    {
        TransformSelf,
        TransformWorld,
        RectTransformSelf
    }
}
