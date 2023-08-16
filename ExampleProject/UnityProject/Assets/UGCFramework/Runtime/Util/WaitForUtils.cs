using System.Collections.Generic;
using UnityEngine;

namespace UGCF.Utils
{
    public class WaitForUtils
    {
        public static WaitForEndOfFrame WaitFrame { get; } = new WaitForEndOfFrame();
        private static Dictionary<float, WaitForSeconds> times = new Dictionary<float, WaitForSeconds>();


        public static WaitForSecondsRealtime WaitForSecondsRealtime(float time)
        {
            return new WaitForSecondsRealtime(time);
        }

        public static WaitForSeconds WaitForSecond(float time)
        {
            if (times.ContainsKey(time))
            {
                return times[time];
            }
            else
            {
                WaitForSeconds waitForSeconds = new WaitForSeconds(time);
                times.Add(time, waitForSeconds);
                return waitForSeconds;
            }
        }
    }
}