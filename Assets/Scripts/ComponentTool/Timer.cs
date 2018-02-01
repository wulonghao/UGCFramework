using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Events;
using System.Collections.Generic;
using XLua;

[Hotfix]
public class Timer : MonoBehaviour
{
    public bool isAutoPlay = false;
    /// <summary>
    /// 时间总长度
    /// </summary>
    public float allLength = 10;
    /// <summary>
    /// 刷新间隔
    /// 小于0为倒计时，大于0为正计时
    /// </summary>
    public float refreshSpace = -1;
    /// <summary>
    /// 时间文本格式 
    /// 1-00:00:00, 2-00:00, 3-00, 4-0
    /// </summary>
    public int timeFormat = 3;
    /// <summary>
    /// 是否只执行一次
    /// </summary>
    public bool isOnce = false;
    /// <summary>
    /// 计时器结束后执行的回调函数
    /// </summary>
    public UnityAction endAction = null;
    /// <summary>
    /// 当前时间
    /// </summary>
    public float currentTime;
    Text timerText;
    bool isEnable = false;
    bool isBegin = false;
    bool isTiming = false;

    int hours = 0;
    int minutes = 0;
    int seconds = 0;

    //全部的委托
    Dictionary<float, UnityAction> allUA = new Dictionary<float, UnityAction>();

    #region ...计时器主体
    void Awake()
    {
        if (!timerText)
        {
            Text temp = GetComponent<Text>();
            if (temp)
                timerText = temp;
        }
    }

    void OnEnable()
    {
        if (isAutoPlay)
            ResetTimer(true);
        isEnable = true;
    }

    void InitTimer(float allLengthPram = 0)
    {
        if (allLengthPram != 0)
            allLength = allLengthPram;
        if (Mathf.Abs(refreshSpace) > this.allLength)
            refreshSpace = refreshSpace > 0 ? allLength : -allLength;
        currentTime = refreshSpace > 0 ? 0 : allLength;
        SetTimeText(GetTimerText(currentTime, timeFormat));
    }

    /// <summary>
    /// 计时器主体(需优化)
    /// </summary>
    /// <returns></returns>
    IEnumerator TimerBody()
    {
        float currentWaitTime = 0;
        while (currentWaitTime < Mathf.Abs(refreshSpace))
        {
            while (!isTiming && isEnable)
                yield return ConstantUtils.frameWait;
            if (!isEnable)
                break;
            yield return ConstantUtils.frameWait;
            currentWaitTime += Time.deltaTime;
        }
        if (isEnable)
        {
            currentTime += refreshSpace;
            SetTimeText(GetTimerText(currentTime, timeFormat));
            if (refreshSpace > 0 ? currentTime < allLength : currentTime > 0)
                StartCoroutine(TimerBody());
            else
            {
                if (!isOnce)
                    ResetTimer(true);
                else
                    CloseTimer();
                if (gameObject.activeSelf && endAction != null)
                    endAction();
            }
        }
    }

    /// <summary>
    /// 重置计时器
    /// </summary>
    /// <param name="isStart">是否立即开始计时</param>
    /// <param name="allLength">计时器长度，为0则为上一次设置的值</param>
    public void ResetTimer(bool isStartTime = false, float allLength = 0)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        StartCoroutine(ResetTimerAc(isStartTime, allLength));
    }

    IEnumerator ResetTimerAc(bool isStartTime, float allLength)
    {
        CloseTimer();
        yield return ConstantUtils.frameWait;
        if (isStartTime)
        {
            foreach (float key in allUA.Keys)
                UIUtils.DelayExecuteAction(key, allUA[key]);
            StartTime();
        }
        else
            InitTimer(allLength);
    }

    /// <summary>
    /// 在指定时间插入指定委托
    /// </summary>
    /// <param name="time"></param>
    /// <param name="ua"></param>
    public void InsertUnityActionFromTime(float time, UnityAction ua)
    {
        allUA.Add(time, ua);
    }

    /// <summary>
    /// 开始计时
    /// </summary>
    public void StartTime()
    {
        if (!isBegin)
        {
            if (!isEnable)
                isEnable = true;
            InitTimer();
            isBegin = true;
            isTiming = true;
            StartCoroutine(TimerBody());
        }
    }

    /// <summary>
    /// 暂停计时
    /// </summary>
    public void PauseTime()
    {
        isTiming = false;
    }

    /// <summary>
    /// 继续计时（取消暂停）
    /// </summary>
    public void ContinueTime()
    {
        isTiming = true;
    }

    /// <summary>
    /// 关闭计时器
    /// </summary>
    public void CloseTimer()
    {
        isBegin = false;
        isEnable = false;
    }

    /// <summary>
    /// 销毁计时器
    /// </summary>
    public void DestroyTimer()
    {
        CloseTimer();
        Destroy(this);
    }

    string GetTimerText(float time, int timeFormat)
    {
        StringBuilder sb = new StringBuilder();
        seconds = (int)time % 60;
        hours = time >= 3600 ? (int)time / 3600 : 0;
        minutes = (int)(time - hours * 3600 - seconds) / 60;
        switch (timeFormat)
        {
            case 1:
                sb.Append(GetTimeText(hours) + ":" + GetTimeText(minutes) + ":" + GetTimeText(seconds));
                break;
            case 2:
                sb.Append(GetTimeText(minutes) + ":" + GetTimeText(seconds));
                break;
            case 3:
                sb.Append(GetTimeText(seconds));
                break;
            case 4:
                sb.Append(GetTimeText(seconds, false));
                break;
        }
        return sb.ToString();
    }

    string GetTimeText(int time, bool isFill = true)
    {
        if (time < 10)
        {
            if (isFill)
                return "0" + time;
            else
                return time.ToString();
        }
        else
            return time.ToString();
    }

    public void SetTimeText(string time)
    {
        if (timerText)
            timerText.text = time;
    }

    void OnDisable()
    {
        CloseTimer();
    }
    #endregion

    /// <summary>
    /// 创建计时器
    /// </summary>
    /// <param name="target">计时器组件绑定的物体</param>
    /// <param name="allLength">计时器长度</param>
    /// <param name="timeFormat">计时器格式 1-00:00:00, 2-00:00, 3-00</param>
    /// <param name="refreshSpace">计时器刷新间隔</param>
    /// <param name="isOnce">计时器是否只执行一次</param>
    /// <param name="action">计时器结束时的回调</param>
    public static Timer CreateTimer(GameObject target, float allLength = 10, int timeFormat = 3, int refreshSpace = -1, bool isOnce = false, UnityAction action = null)
    {
        Timer timer = target.GetComponent<Timer>();
        if (!timer)
            timer = target.AddComponent<Timer>();
        timer.allLength = allLength;
        timer.refreshSpace = refreshSpace;
        timer.timeFormat = timeFormat;
        timer.isOnce = isOnce;
        timer.endAction = action;
        timer.currentTime = refreshSpace > 0 ? 0 : allLength;
        timer.timerText = timer.GetComponent<Text>();
        if (timer.timerText)
            timer.timerText.text = timer.GetTimerText(timer.currentTime, timeFormat);
        return timer;
    }

    public static Timer CreateTimer(GameObject target, float allLength = 10, UnityAction action = null)
    {
        Timer timer = target.GetComponent<Timer>();
        if (!timer)
            timer = target.AddComponent<Timer>();
        timer.allLength = allLength;
        timer.isOnce = true;
        timer.endAction = action;
        return timer;
    }
}
