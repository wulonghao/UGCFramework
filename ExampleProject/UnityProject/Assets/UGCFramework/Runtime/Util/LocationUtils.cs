using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UGCF.Manager;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Events;

public class LocationUtils 
{
    #region ...IP定位
    /// <summary>
    /// 根据IP获取大致定位
    /// </summary>
    /// <param name="ua"></param>
    public static void GetLocationByIP(UnityAction<object> ua)
    {
        WWWForm wwwf = new WWWForm();
        wwwf.AddField("ak", "你的百度ak");
        wwwf.AddField("coor", "bd09ll");
        HttpManager.SendHttpPostRequest("https://api.map.baidu.com/location/ip", wwwf, (uwr) =>
        {
            try
            {
                //LogUtils.Log(Regex.Unescape(www.text));
                JToken jContent = JObject.Parse(System.Text.RegularExpressions.Regex.Unescape(uwr.downloadHandler.text))["content"];
                JToken pointJd = jContent["point"];
                JToken addressComponentJd = jContent["address_detail"];
                ua(new string[] {
                        pointJd.Value<string>("x"),
                        pointJd.Value<string>("y"),
                        addressComponentJd.Value<string>("province"),
                        addressComponentJd.Value<string>("city"),
                        addressComponentJd.Value<string>("city_code")
                    });
            }
            catch (Exception e)
            {
                LogUtils.Log("获取定位信息失败:" + e.ToString());
            }
        });
    }
    #endregion

    #region ...GPS定位
    /// <summary>
    /// 开启GPS
    /// </summary>
    /// <param name="desiredAccuracyInMeters"></param>
    /// <param name="updateDistanceInMeters"></param>
    public static void StartGPS(float desiredAccuracyInMeters = 10, float updateDistanceInMeters = 10)
    {
        UGCFMain.Instance.StartCoroutine(StartGPSAc(desiredAccuracyInMeters, updateDistanceInMeters));
    }

    /// <summary> 开启GPS </summary>
    private static IEnumerator StartGPSAc(float desiredAccuracyInMeters, float updateDistanceInMeters)
    {
        if (!Input.location.isEnabledByUser)
        {
            TipManager.Instance.OpenTip(TipType.SimpleTip, "无法获取到定位，需要设置GPS权限");
            yield break;
        }
        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            LogUtils.Log("初始化GPS超时");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            LogUtils.Log("启用GPS定位失败");
            yield break;
        }
        else
        {
            LogUtils.Log("开启定位成功");
        }
    }

    /// <summary>
    /// 获取用户定位，数组内为{经度,纬度},获取成功后关闭定位功能，GPS定位开启后有效
    /// </summary>
    /// <param name="closeGPS">是否在获取到经纬度信息后关闭GPS</param>
    /// <returns>{经度,纬度}</returns>
    public static float[] GetLatitudeAndLongitude(bool closeGPS = true)
    {
        float[] fs = null;
        if (Input.location.status == LocationServiceStatus.Running)
        {
            fs = new float[] { Input.location.lastData.longitude, Input.location.lastData.latitude };
            if (closeGPS)
                Input.location.Stop();
        }
        else
        {
            LogUtils.LogError("定位功能未开启");
        }
        return fs;
    }

    /// <summary>
    /// 根据经纬度信息获取定位信息
    /// </summary>
    /// <param name="ua"></param>
    public static void GetLocation(UnityAction<string> ua)
    {
        UGCFMain.Instance.StartCoroutine(GetLocationAc(ua));
    }

    /// <summary> 根据经纬度信息获取定位信息（百度定位） </summary>
    public static IEnumerator GetLocationAc(UnityAction<string> ua)
    {
        float[] latAndlng = GetLatitudeAndLongitude();
        while (latAndlng == null)
        {
            yield return new WaitForSecondsRealtime(3);
            latAndlng = GetLatitudeAndLongitude();
        }

        WWWForm wwwf = new WWWForm();
        wwwf.AddField("ak", "你的百度ak");
        wwwf.AddField("location", latAndlng[1] + "," + latAndlng[0]);
        wwwf.AddField("output", "json");
        wwwf.AddField("pois", "0");
        HttpManager.SendHttpPostRequest("http://api.map.baidu.com/geocoder/v2", wwwf,
            (request) =>
            {
                try
                {
                    JToken jd = JObject.Parse(System.Text.RegularExpressions.Regex.Unescape(request.downloadHandler.text));
                    ua(jd["result"].Value<string>("addressComponent"));
                }
                catch
                {
                    TipManager.Instance.OpenTip(TipType.SimpleTip, "获取定位信息失败");
                }
            });
    }
    #endregion
}
