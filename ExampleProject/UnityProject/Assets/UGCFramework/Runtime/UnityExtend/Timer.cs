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
        #region ...字段
        [SerializeField] private bool isAutoPlay = false;
        /// <summary> 时间总长度 </summary>
        [SerializeField] private float allLength = 10;
        /// <summary> 刷新间隔 小于0为倒计时，大于0为正计时 </summary>
        [SerializeField] private float refreshSpace = -1;
        /// <summary> 时间文本格式 HHMMSS-00:00:00, MMSS-00:00, SS-00, S-0 </summary>
        [SerializeField]
        [Tooltip("时间文本格式 HHMMSS-00:00:00, MMSS-00:00, SS-00, S-0")]
        private TimeFormat timeFormat1 = TimeFormat.SS;
        /// <summary> 是否只执行一次 </summary>
        [SerializeField] private bool isOnce = false;
        /// <summary> 当前时间文本格式 </summary>
        [SerializeField] private string timeTxtFormat;
        [SerializeField] private bool isEnable = false;
        [SerializeField] private Text timerText;
        bool isBegin = false;
        float nextTime;

        int hours = 0;
        int minutes = 0;
        int seconds = 0;

        //全部的委托
        Dictionary<float, UnityAction> allUA = new Dictionary<float, UnityAction>();
        Dictionary<float, UnityAction> tempAllUA = new Dictionary<float, UnityAction>();
        #endregion

        #region ...属性
        public bool IsAutoPlay { get => isAutoPlay; set => isAutoPlay = value; }
        public float AllLength { get => allLength; set => allLength = value; }
        public float RefreshSpace { get => refreshSpace; set => refreshSpace = value; }
        public TimeFormat TimeFormat1 { get => timeFormat1; set => timeFormat1 = value; }
        public bool IsOnce { get => isOnce; set => isOnce = value; }
        public UnityAction EndAction { get; set; } = null;
        public float CurrentTime { get; set; }
        public string TimeTxtFormat { get => timeTxtFormat; set => timeTxtFormat = value; }
        public bool IsTiming { get; set; } = false;
        public bool IsEnable { get => isEnable; set => isEnable = value; }
        #endregion

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
            IsEnable = true;
            if (IsAutoPlay)
                ResetTimer(true);
        }

        void InitTimer(float _allLength = 0)
        {
            if (_allLength != 0)
                AllLength = _allLength;
            if (Mathf.Abs(RefreshSpace) > AllLength)//刷新间隔大于总时长时，令间隔=时长
                RefreshSpace = RefreshSpace > 0 ? AllLength : -AllLength;
            CurrentTime = RefreshSpace > 0 ? 0 : AllLength;
            nextTime = CurrentTime + RefreshSpace;
            SetTimeText(GetTimerText(CurrentTime, TimeFormat1));
        }

        void Update()
        {
            if (IsTiming && IsEnable)
            {
                CurrentTime += RefreshSpace > 0 ? Time.deltaTime : -Time.deltaTime;
                if (tempAllUA.ContainsKey(CurrentTime) && tempAllUA[CurrentTime] != null)
                {
                    tempAllUA[CurrentTime]();
                    tempAllUA.Remove(CurrentTime);
                }
                CurrentTime = Mathf.Clamp(CurrentTime, 0, AllLength);

                if (RefreshSpace > 0 ? CurrentTime >= nextTime : CurrentTime <= nextTime)
                {
                    SetTimeText(GetTimerText(CurrentTime, TimeFormat1));
                    if (RefreshSpace > 0 ? CurrentTime >= AllLength : CurrentTime <= 0)
                    {
                        if (!IsOnce)
                            ResetTimer(true);
                        else
                            CloseTimer();
                        if (gameObject.activeSelf && EndAction != null)
                            EndAction();
                    }
                    else
                    {
                        nextTime += RefreshSpace;
                        nextTime = Mathf.Clamp(nextTime, 0, AllLength);
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
                    IsTiming = true;
                    IsEnable = true;
                    isBegin = true;
                }
            }
        }

        /// <summary>
        /// 暂停计时
        /// </summary>
        public void PauseTime()
        {
            IsTiming = false;
        }

        /// <summary>
        /// 继续计时（取消暂停）
        /// </summary>
        public void ContinueTime()
        {
            IsTiming = true;
        }

        /// <summary>
        /// 关闭计时器
        /// </summary>
        public void CloseTimer()
        {
            isBegin = false;
            IsEnable = false;
            IsTiming = false;
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
            int timeInt = Mathf.RoundToInt(time);
            switch (timeFormat)
            {
                case TimeFormat.HHMMSS:
                    seconds = timeInt % 60;
                    hours = timeInt >= 3600 ? timeInt / 3600 : 0;
                    minutes = (timeInt - hours * 3600 - seconds) / 60;
                    sb.Append(GetTimeText(hours) + ":" + GetTimeText(minutes) + ":" + GetTimeText(seconds));
                    break;
                case TimeFormat.MMSS:
                    seconds = timeInt % 60;
                    minutes = (timeInt - seconds) / 60;
                    sb.Append(GetTimeText(minutes) + ":" + GetTimeText(seconds));
                    break;
                case TimeFormat.SS:
                    sb.Append(GetTimeText(timeInt));
                    break;
                case TimeFormat.S:
                    sb.Append(GetTimeText(timeInt, false));
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
                if (string.IsNullOrEmpty(TimeTxtFormat))
                    timerText.text = time;
                else
                    timerText.text = string.Format(TimeTxtFormat, time);
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
            timer.AllLength = allLength;
            timer.RefreshSpace = refreshSpace;
            timer.TimeFormat1 = timeFormat;
            timer.IsOnce = isOnce;
            timer.EndAction = action;
            timer.CurrentTime = refreshSpace > 0 ? 0 : allLength;
            timer.timerText = timer.GetComponent<Text>();
            if (timer.timerText)
                timer.timerText.text = timer.GetTimerText(timer.CurrentTime, timeFormat);
            return timer;
        }

        public static Timer CreateTimer(GameObject target, bool _isOnce = true, UnityAction action = null)
        {
            Timer timer = target.GetComponent<Timer>();
            if (!timer)
                timer = target.AddComponent<Timer>();
            timer.IsOnce = true;
            timer.EndAction = action;
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