using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForUtils
{
    public static WaitForEndOfFrame WaitFrame = new WaitForEndOfFrame();
    static Dictionary<float, List<WaitForSecondsRealtime>> realtimes = new Dictionary<float, List<WaitForSecondsRealtime>>();
    static Dictionary<float, WaitForSeconds> times = new Dictionary<float, WaitForSeconds>();

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float time)
    {
        if (realtimes.ContainsKey(time))
        {
            WaitForSecondsRealtime waitForSecondsRealtime = realtimes[time].Find((wfs) => { return !wfs.keepWaiting; });
            if (waitForSecondsRealtime != null)
                return waitForSecondsRealtime;
            else
            {
                waitForSecondsRealtime = new WaitForSecondsRealtime(time);
                realtimes[time].Add(waitForSecondsRealtime);
                return waitForSecondsRealtime;
            }
        }
        else
        {
            List<WaitForSecondsRealtime> waitFortimes = new List<WaitForSecondsRealtime>();
            WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(time);
            waitFortimes.Add(waitForSecondsRealtime);
            realtimes.Add(time, waitFortimes);
            return waitForSecondsRealtime;
        }
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