using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XLua;

[Hotfix]
public class CommonAnimation : MonoBehaviour
{
    public bool point, angle, scale, color, alpha, size, fillAmount;
    public UnityAction pointEndAction, angleEndAction, scaleEndAction, colorEndAction, alphaEndAction, sizeEndAction, fillAmountEndAction;//结束后的执行的函数
    public List<Vector3> pointList = new List<Vector3>(), angleList = new List<Vector3>(), scaleList = new List<Vector3>();
    public List<Color> colorList = new List<Color>();
    public List<float> alphaList = new List<float>();
    public List<Vector2> sizeList = new List<Vector2>();
    public List<float> fillAmountList = new List<float>();
    public float pointDelayTime, angleDelayTime, scaleDelayTime, colorDelayTime, alphaDelayTime, sizeDelayTime, fillAmountDelayTime;
    public float pointTime = 1, angleTime = 1, scaleTime = 1, colorTime = 1, alphaTime = 1, sizeTime = 1, fillAmountTime = 1;
    public Space space = Space.Self;
    public bool isLoop, isPingPong, isFoward = true, isAutoPlay, isBackInit, isDisappear, isPause, isStop;
    public DisappearType disType;

    int animationNum = 0, currentFisishNum = 0;
    WaitForSecondsRealtime pauseWait;

    void Start()
    {

    }

    public void Init()
    {
        if (point) Init(AnimationType.Point);
        if (angle) Init(AnimationType.Angle);
        if (scale) Init(AnimationType.Scale);
        if (color) Init(AnimationType.Color);
        if (alpha) Init(AnimationType.Alpha);
        if (size) Init(AnimationType.Size);
        if (fillAmount) Init(AnimationType.FillAmount);
    }

    public void Play()
    {
        pauseWait = new WaitForSecondsRealtime(0.1f);
        animationNum = 0;
        currentFisishNum = 0;
        Stop();
        if (point && pointList != null && (pointList.Count > 2 || (pointList.Count == 2 && pointList[0] != pointList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.Point, pointDelayTime, pointTime, space));
        }

        if (angle && angleList != null && (angleList.Count > 2 || (angleList.Count == 2 && angleList[0] != angleList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.Angle, angleDelayTime, angleTime));
        }

        if (scale && scaleList != null && (scaleList.Count > 2 || (scaleList.Count == 2 && scaleList[0] != scaleList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.Scale, scaleDelayTime, scaleTime));
        }

        if (color && colorList != null && (colorList.Count > 2 || (colorList.Count == 2 && colorList[0] != colorList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.Color, colorDelayTime, colorTime));
        }

        if (alpha && alphaList != null && (alphaList.Count > 2 || (alphaList.Count == 2 && alphaList[0] != alphaList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.Alpha, alphaDelayTime, alphaTime));
        }

        if (size && sizeList != null && (sizeList.Count > 2 || (sizeList.Count == 2 && sizeList[0] != sizeList[1])))
        {
            animationNum++;
            RectTransform rtf = GetComponent<RectTransform>();
            if (!rtf)
                rtf = gameObject.AddComponent<RectTransform>();
            StartCoroutine(PlayAnimation(AnimationType.Size, sizeDelayTime, sizeTime));
        }

        if (fillAmount && fillAmountList != null && (fillAmountList.Count > 2 || (fillAmountList.Count == 2 && fillAmountList[0] != fillAmountList[1])))
        {
            animationNum++;
            StartCoroutine(PlayAnimation(AnimationType.FillAmount, fillAmountDelayTime, fillAmountTime));
        }
    }

    /// <summary>
    /// 播放指定动画
    /// </summary>
    /// <param name="type">动画类型</param>
    /// <param name="delayTime">延迟时间</param>
    /// <param name="time">播放总时长</param>
    /// <param name="space">坐标空间</param>
    /// <returns></returns>
    IEnumerator PlayAnimation(AnimationType type, float delayTime, float time, Space space = Space.Self)
    {
        WaitForSecondsRealtime delayWait = new WaitForSecondsRealtime(delayTime);
        if (delayTime != 0)
            yield return delayWait;
        Init(type);
        bool currentOrder = isFoward;
        float progress = currentOrder ? 0 : 1;
        float speed = 0;//speed为每秒lerp的进度
        bool hadCanvas = true;
        int count = 0, startIndex = 0;
        CanvasGroup cg = GetComponent<CanvasGroup>();
        hadCanvas = cg;
        #region ....根据类型和播放方向，计算变化速度和初始索引
        switch (type)
        {
            case AnimationType.Point:
                speed = (pointList.Count - 1) / time;
                startIndex = currentOrder ? 0 : pointList.Count - 2;
                break;
            case AnimationType.Angle:
                speed = (angleList.Count - 1) / time;
                startIndex = currentOrder ? 0 : angleList.Count - 2;
                break;
            case AnimationType.Scale:
                speed = (scaleList.Count - 1) / time;
                startIndex = currentOrder ? 0 : scaleList.Count - 2;
                break;
            case AnimationType.Color:
                speed = (colorList.Count - 1) / time;
                startIndex = currentOrder ? 0 : colorList.Count - 2;
                break;
            case AnimationType.Alpha:
                speed = (alphaList.Count - 1) / time;
                startIndex = currentOrder ? 0 : alphaList.Count - 2;
                break;
            case AnimationType.Size:
                speed = (sizeList.Count - 1) / time;
                startIndex = currentOrder ? 0 : sizeList.Count - 2;
                break;
            case AnimationType.FillAmount:
                speed = (fillAmountList.Count - 1) / time;
                startIndex = currentOrder ? 0 : fillAmountList.Count - 2;
                break;
        }
        #endregion
        float currentSpeed = 0;
        while (currentOrder ? progress < 1 : progress > 0)
        {
            currentSpeed = speed * Time.deltaTime;
            if (isStop)
            {
                Init(type);
                break;
            }
            yield return ConstantUtils.frameWait;
            if (isPause)
            {
                yield return pauseWait;
                continue;
            }

            if (currentOrder)
            {
                progress += currentSpeed;
                if (progress > 1)
                    progress = 1;
            }
            else
            {
                progress -= currentSpeed;
                if (progress < 0)
                    progress = 0;
            }
            #region ...根据类型，执行不同的渐变方式
            switch (type)
            {
                case AnimationType.Point:
                    #region ...坐标变化
                    if (count == 0)
                        count = pointList.Count;
                    if (space == Space.Self)
                        transform.localPosition = Vector3.Lerp(pointList[startIndex], pointList[startIndex + 1], progress);
                    else
                        transform.position = Vector3.Lerp(pointList[startIndex], pointList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.Angle:
                    #region ...角度变化
                    if (count == 0)
                        count = angleList.Count;
                    transform.localEulerAngles = Vector3.Lerp(angleList[startIndex], angleList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.Scale:
                    #region ...比例变化
                    if (count == 0)
                        count = scaleList.Count;
                    transform.localScale = Vector3.Lerp(scaleList[startIndex], scaleList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.Color:
                    #region ...颜色变化
                    if (count == 0)
                        count = colorList.Count;
                    if (GetComponent<Image>())
                        GetComponent<Image>().color = Color.Lerp(colorList[startIndex], colorList[startIndex + 1], progress);
                    else if (GetComponent<Text>())
                        GetComponent<Text>().color = Color.Lerp(colorList[startIndex], colorList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.Alpha:
                    #region ...透明度变化
                    if (count == 0)
                        count = alphaList.Count;
                    if (!hadCanvas && !cg)
                    {
                        cg = gameObject.AddComponent<CanvasGroup>();
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    }
                    cg.alpha = Mathf.Lerp(alphaList[startIndex], alphaList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.Size:
                    #region ...尺寸变化
                    if (count == 0)
                        count = sizeList.Count;
                    if (GetComponent<RectTransform>())
                        GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(sizeList[startIndex], sizeList[startIndex + 1], progress);
                    break;
                    #endregion
                case AnimationType.FillAmount:
                    #region ...fillAmount变化
                    if (count == 0)
                        count = fillAmountList.Count;
                    if (GetComponent<Image>())
                        GetComponent<Image>().fillAmount = Mathf.Lerp(fillAmountList[startIndex], fillAmountList[startIndex + 1], progress);
                    break;
                    #endregion
            }
            #endregion
            if (currentOrder ? progress >= 1 : progress <= 0)
            {
                progress = currentOrder ? 0 : 1;
                if (currentOrder ? startIndex + 2 < count : startIndex > 1)
                    startIndex += currentOrder ? 1 : -1;
                else
                    if (isLoop)
                    {
                        if (isPingPong)
                        {
                            currentOrder = !currentOrder;
                            startIndex = currentOrder ? 0 : count - 2;
                            progress = currentOrder ? 0 : 1;
                        }
                        else
                            startIndex = 0;
                        if (delayTime != 0)
                            yield return delayWait;
                    }
                    else
                    {
                        progress = currentOrder ? 1 : 0;
                        #region ...回调
                        switch (type)
                        {
                            case AnimationType.Point:
                                if (pointEndAction != null)
                                    pointEndAction();
                                break;
                            case AnimationType.Angle:
                                if (angleEndAction != null)
                                    angleEndAction();
                                break;
                            case AnimationType.Scale:
                                if (scaleEndAction != null)
                                    scaleEndAction();
                                break;
                            case AnimationType.Color:
                                if (colorEndAction != null)
                                    colorEndAction();
                                break;
                            case AnimationType.Alpha:
                                if (!hadCanvas)
                                    Destroy(cg);
                                if (alphaEndAction != null)
                                    alphaEndAction();
                                break;
                            case AnimationType.Size:
                                if (sizeEndAction != null)
                                    sizeEndAction();
                                break;
                            case AnimationType.FillAmount:
                                if (fillAmountEndAction != null)
                                    fillAmountEndAction();
                                break;
                        }
                        #endregion
                        currentFisishNum++;
                        if (isBackInit)
                            Init(type);
                        if (isDisappear && currentFisishNum == animationNum)
                            if (disType == DisappearType.Destroy)
                                Destroy(gameObject);
                            else
                                gameObject.SetActive(false);
                    }
            }
        }
    }

    void Init(AnimationType type)
    {
        int initIndex = 0;
        switch (type)
        {
            case AnimationType.Point:
                #region ...坐标变化
                if (!isFoward)
                    initIndex = pointList.Count - 1;
                if (space == Space.Self)
                    transform.localPosition = pointList[initIndex];
                else
                    transform.position = pointList[initIndex];
                break;
                #endregion
            case AnimationType.Angle:
                #region ...角度变化
                transform.localEulerAngles = angleList[initIndex];
                break;
                #endregion
            case AnimationType.Scale:
                #region ...比例变化
                transform.localScale = scaleList[initIndex];
                break;
                #endregion
            case AnimationType.Color:
                #region ...颜色变化
                if (GetComponent<Image>())
                    GetComponent<Image>().color = colorList[initIndex];
                else if (GetComponent<Text>())
                    GetComponent<Text>().color = colorList[initIndex];
                break;
                #endregion
            case AnimationType.Alpha:
                #region ...透明度变化
                CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
                if (!cg)
                {
                    cg = gameObject.AddComponent<CanvasGroup>();
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
                cg.alpha = alphaList[initIndex];
                break;
                #endregion
            case AnimationType.Size:
                #region ...尺寸变化
                if (GetComponent<RectTransform>())
                    GetComponent<RectTransform>().sizeDelta = sizeList[initIndex];
                break;
                #endregion
            case AnimationType.FillAmount:
                #region ...fillAmount变化
                if (GetComponent<Image>())
                    GetComponent<Image>().fillAmount = fillAmountList[initIndex];
                break;
                #endregion
        }
        isStop = false;
        isPause = false;
    }

    void OnEnable()
    {
        if (isAutoPlay)
            Play();
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
        pointEndAction = angleEndAction = scaleEndAction = colorEndAction = alphaEndAction = sizeEndAction = null;
        pointList.Clear();
        angleList.Clear();
        scaleList.Clear();
        colorList.Clear();
        alphaList.Clear();
        sizeList.Clear();
        pointDelayTime = angleDelayTime = scaleDelayTime = colorDelayTime = alphaDelayTime = sizeDelayTime = 0;
        pointTime = angleTime = scaleTime = colorTime = alphaTime = sizeTime = 1;
        isLoop = isAutoPlay = isDisappear = isPause = isStop = false;
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

    enum AnimationType
    {
        Point = 1,
        Angle, 
        Scale, 
        Color, 
        Alpha, 
        Size, 
        FillAmount
    }
}