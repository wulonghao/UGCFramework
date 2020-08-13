using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Events;
using System.Collections.Generic;
using UGCF.Utils;

namespace UGCF.UnityExtend
{
    public class Timer : MonoBehaviour
    {
        public bool isAutoPlay = false;
        /// <summary> 时间总长度 </summary>
        public float allLength = 10;
        /// <summary> 刷新间隔 小于0为倒计时，大于0为正计时 </summary>
        public float refreshSpace = -1;
        /// <summary> 时间文本格式 HHMMSS-00:00:00, MMSS-00:00, SS-00, S-0 </summary>
        [Tooltip("HHMMSS-00:00:00, MMSS-00:00, SS-00, S-0")]
        public TimeFormat timeFormat = TimeFormat.SS;
        /// <summary> 是否只执行一次 </summary>
        public bool isOnce = false;
        /// <summary> 计时器结束后执行的回调函数 </summary>
        public UnityAction endAction = null;
        /// <summary> 当前时间 </summary>
        [HideInInspector]
        public float currentTime;
        /// <summary> 当前时间文本格式 </summary>
        public string timeTxtFormat;
        /// <summary> 是否处于计时中状态 </summary>
        [HideInInspector]
        public bool isTiming = false;
        [SerializeField]
        Text timerText;
        public bool isEnable = false;
        bool isBegin = false;

        int hours = 0;
        int minutes = 0;
        int seconds = 0;

        //全部的委托
        Dictionary<float, UnityAction> allUA = new Dictionary<float, UnityAction>();
        Dictionary<float, UnityAction> tempAllUA = new Dictionary<float, UnityAction>();

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
            isEnable = true;
            if (isAutoPlay)
                ResetTimer(true);
        }

        void InitTimer(float _allLength = 0)
        {
            if (_allLength != 0)
                allLength = _allLength;
            if (Mathf.Abs(refreshSpace) > allLength)//刷新间隔大于总时长时，令间隔=时长
                refreshSpace = refreshSpace > 0 ? allLength : -allLength;
            currentTime = refreshSpace > 0 ? 0 : allLength;
            nextTime = currentTime + refreshSpace;
            SetTimeText(GetTimerText(currentTime, timeFormat));
        }

        float nextTime;
        void Update()
        {
            if (isTiming && isEnable)
            {
                currentTime += refreshSpace > 0 ? Time.deltaTime : -Time.deltaTime;
                if (tempAllUA.ContainsKey(currentTime) && tempAllUA[currentTime] != null)
                {
                    tempAllUA[currentTime]();
                    tempAllUA.Remove(currentTime);
                }
                currentTime = Mathf.Clamp(currentTime, 0, allLength);

                if (refreshSpace > 0 ? currentTime >= nextTime : currentTime <= nextTime)
                {
                    SetTimeText(GetTimerText(currentTime, timeFormat));
                    if (refreshSpace > 0 ? currentTime >= allLength : currentTime <= 0)
                    {
                        if (!isOnce)
                            ResetTimer(true);
                        else
                            CloseTimer();
                        if (gameObject.activeSelf && endAction != null)
                            endAction();
                    }
                    else
                    {
                        nextTime += refreshSpace;
                        nextTime = Mathf.Clamp(nextTime, 0, allLength);
                    }
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
        /// <summary>
        /// 清除所有委托
        /// </summary>
        public void Clear()
        {
            allUA.Clear();
            tempAllUA.Clear();
        }
        IEnumerator ResetTimerAc(bool isStartTime, float allLength)
        {
            CloseTimer();
            yield return WaitForUtils.WaitFrame;
            if (isStartTime)
                StartTime(allLength);
            else
                InitTimer(allLength);
        }

        /// <summary>
        /// 在指定时间插入指定委托
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ua"></param>
        public void InsertActionFromTime(float time, UnityAction ua)
        {
            if (allUA.ContainsKey(time))
                allUA[time] = ua;
            else
                allUA.Add(time, ua);
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public void StartTime(float allLength = -1)
        {
            if (!isBegin)
            {
                if (allLength != -1)
                    InitTimer(allLength);
                tempAllUA.Clear();
                foreach (float key in allUA.Keys)
                    tempAllUA.Add(key, allUA[key]);
                if (this && gameObject.activeSelf)
                {
                    isTiming = true;
                    isEnable = true;
                    isBegin = true;
                }
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
            isTiming = false;
        }

        /// <summary>
        /// 销毁计时器
        /// </summary>
        public void DestroyTimer()
        {
            CloseTimer();
            Destroy(this);
        }

        string GetTimerText(float time, TimeFormat timeFormat)
        {
            StringBuilder sb = new StringBuilder();
            switch (timeFormat)
            {
                case TimeFormat.HHMMSS:
                    seconds = (int)time % 60;
                    hours = time >= 3600 ? (int)time / 3600 : 0;
                    minutes = (int)(time - hours * 3600 - seconds) / 60;
                    sb.Append(GetTimeText(hours) + ":" + GetTimeText(minutes) + ":" + GetTimeText(seconds));
                    break;
                case TimeFormat.MMSS:
                    seconds = (int)time % 60;
                    minutes = (int)(time - seconds) / 60;
                    sb.Append(GetTimeText(minutes) + ":" + GetTimeText(seconds));
                    break;
                case TimeFormat.SS:
                    sb.Append(GetTimeText(time));
                    break;
                case TimeFormat.S:
                    sb.Append(GetTimeText(time, false));
                    break;
            }
            return sb.ToString();
        }

        string GetTimeText(float time, bool isFill = true)
        {
            if (time < 10 && isFill)
                return "0" + time.ToString("F0");
            else
                return time.ToString("F0");
        }

        public void SetTimeText(string time)
        {
            if (timerText)
            {
                if (string.IsNullOrEmpty(timeTxtFormat))
                    timerText.text = time;
                else
                    timerText.text = string.Format(timeTxtFormat, time);
            }
        }

        /// <summary>
        /// 设置时间文本颜色
        /// </summary>
        /// <param name="color"></param>
        public void SetTimeTextColor(Color color)
        {
            timerText.color = color;
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
        public static Timer CreateTimer(GameObject target, float allLength = 10, TimeFormat timeFormat = TimeFormat.SS, int refreshSpace = -1, bool isOnce = false, UnityAction action = null)
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

        public static Timer CreateTimer(GameObject target, bool _isOnce = true, UnityAction action = null)
        {
            Timer timer = target.GetComponent<Timer>();
            if (!timer)
                timer = target.AddComponent<Timer>();
            timer.isOnce = true;
            timer.endAction = action;
            return timer;
        }

        public enum TimeFormat
        {
            /// <summary> 时分秒 00:00:00 </summary>
            HHMMSS = 1,
            /// <summary> 秒 00:00 </summary>
            MMSS,
            /// <summary> 秒 00 不足十秒则用0补充十位 </summary>
            SS,
            /// <summary> 秒 0 不足十秒则显示个位 </summary>
            S
        }
    }
}