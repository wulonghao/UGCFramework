using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;
using System;
using LitJson;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using XLua;

[Hotfix]
public class MiscUtils
{
    /// <summary> 双线性插值法缩放图片，等比缩放</summary>
    public static Texture2D ScaleTextureBilinear(Texture2D originalTexture, float scaleFactor)
    {
        Texture2D newTexture = new Texture2D(Mathf.CeilToInt(originalTexture.width * scaleFactor), Mathf.CeilToInt(originalTexture.height * scaleFactor));
        float scale = 1.0f / scaleFactor;
        int maxX = originalTexture.width - 1;
        int maxY = originalTexture.height - 1;
        for (int y = 0; y < newTexture.height; y++)
        {
            for (int x = 0; x < newTexture.width; x++)
            {
                // Bilinear Interpolation  
                float targetX = x * scale;
                float targetY = y * scale;
                int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                int x2 = Mathf.Min(maxX, x1 + 1);
                int y2 = Mathf.Min(maxY, y1 + 1);

                float u = targetX - x1;
                float v = targetY - y1;
                float w1 = (1 - u) * (1 - v);
                float w2 = u * (1 - v);
                float w3 = (1 - u) * v;
                float w4 = u * v;
                Color color1 = originalTexture.GetPixel(x1, y1);
                Color color2 = originalTexture.GetPixel(x2, y1);
                Color color3 = originalTexture.GetPixel(x1, y2);
                Color color4 = originalTexture.GetPixel(x2, y2);
                Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                    Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                    Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                    Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                );
                newTexture.SetPixel(x, y, color);

            }
        }
        newTexture.Apply();
        return newTexture;
    }

    /// <summary>将图片缩放为指定尺寸</summary>
    public static Texture2D SizeTextureBilinear(Texture2D originalTexture, Vector2 size)
    {
        Texture2D newTexture = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        float scaleX = originalTexture.width / size.x;
        float scaleY = originalTexture.height / size.y;
        int maxX = originalTexture.width - 1;
        int maxY = originalTexture.height - 1;
        for (int y = 0; y < newTexture.height; y++)
        {
            for (int x = 0; x < newTexture.width; x++)
            {
                float targetX = x * scaleX;
                float targetY = y * scaleY;
                int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                int x2 = Mathf.Min(maxX, x1 + 1);
                int y2 = Mathf.Min(maxY, y1 + 1);

                float u = targetX - x1;
                float v = targetY - y1;
                float w1 = (1 - u) * (1 - v);
                float w2 = u * (1 - v);
                float w3 = (1 - u) * v;
                float w4 = u * v;
                Color color1 = originalTexture.GetPixel(x1, y1);
                Color color2 = originalTexture.GetPixel(x2, y1);
                Color color3 = originalTexture.GetPixel(x1, y2);
                Color color4 = originalTexture.GetPixel(x2, y2);
                Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                    Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                    Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                    Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                );
                newTexture.SetPixel(x, y, color);

            }
        }
        newTexture.Apply();
        return newTexture;
    }

    /// <summary>时间戳转DateTime</summary>
    public static DateTime GetDateTimeByTimeStamp(long timeStamp)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
        return startTime.AddSeconds(timeStamp);
    }

    public static long GetTimeStamp(DateTime time)
    {
        TimeSpan ts = time - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>获取指定文件夹下文件信息</summary>
    public static List<FileInfo> GetFileInfoFromFolder(string folderPath, SearchOption option, string searchPattern = "*")
    {
        List<FileInfo> fileInfos = new List<FileInfo>();
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        if (dirInfo.Exists)
        {
            FileInfo[] fis = dirInfo.GetFiles(searchPattern, option);
            if (fis.Length > 0)
            {
                for (int i = 0; i < fis.Length; i++)
                {
                    if (!fis[i].Name.EndsWith(".DS_Store") && !fis[i].Name.EndsWith(".meta"))
                    {
                        fileInfos.Add(fis[i]);
                    }
                }
            }
        }

        return fileInfos;
    }

    /// <summary>获取文件的md5校验码</summary>
    public static string GetMD5HashFromFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
                sb.Append(retVal[i].ToString("x2"));
            return sb.ToString();
        }
        return null;
    }

    public static string GetRandomString(int length)
    {
        string str = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            sb.Append(str.Substring(UnityEngine.Random.Range(0, str.Length), 1));
        }
        return sb.ToString();
    }

    /// <summary>对指定字符串进行Sha1加密</summary>
    public static string GetSha1Hash(string input)
    {
        SHA1 sha1 = new SHA1CryptoServiceProvider();
        byte[] inputBytes = System.Text.UTF8Encoding.Default.GetBytes(input);
        byte[] outputBytes = sha1.ComputeHash(inputBytes);
        string output = System.BitConverter.ToString(outputBytes).Replace("-", "");
        return output.ToLower();
    }

    /// <summary>对指定字符串进行Md5加密</summary>
    public static string GetMd5Hash(string input)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] inputBytes = System.Text.UTF8Encoding.Default.GetBytes(input);
        byte[] outputBytes = md5.ComputeHash(inputBytes);
        string output = System.BitConverter.ToString(outputBytes).Replace("-", "");
        return output.ToLower();
    }

    /// <summary>获取当前渠道</summary>
    public static string GetChannel()
    {
        string channel = "";
#if UNITY_EDITOR
        channel = "Editor";
#elif UNITY_ANDROID
#if Tencent
		channel = "Tencent";
#elif Qihu
		channel = "Qihu";
#elif Baidu
		channel = "Baidu";
#elif Xiaomi
		channel = "Xiaomi";
#elif Huawei
		channel = "Huawei";
#elif Own
		channel = "Own";
#endif
#elif UNITY_IPHONE
		channel = "AppStore";
#endif
        //if (DebugController.Instance._Debug)
        channel += "_Debug";
        return channel;
    }

    /// <summary>获取当前平台类型字符串</summary>
    public static string GetCurrentPlatform()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IPHONE
		return "iOS";
#else
        return "StandaloneWindows64";
#endif
    }

    /// <summary> 读取指定路径的Json文件(非Bundle)</summary>
    public static JsonData GetJsonFromPath(string path)
    {
        return JsonMapper.ToObject(File.ReadAllText(path));
    }

    /// <summary>Key和Value都需支持ToString</summary>
    public static JsonData GetJsonDataByDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        JsonData data = new JsonData();
        foreach (var p in dict)
        {
            data[p.Key.ToString()] = p.Value.ToString();
        }
        return data;
    }

    /// <summary>获取OrderedDictionary对应索引的Key</summary>
    public static object GetKeyFromIndex(OrderedDictionary od, int index)
    {
        int i = -1;
        foreach (object key in od.Keys)
            if (++i == index)
                return key;
        return null;
    }

    /// <summary>获取摄像机视图</summary>
    public static void GetCameraRawImage(ref RawImage img, Camera ca)
    {
        if (img && ca)
        {
            RenderTexture rt = new RenderTexture((int)img.GetComponent<RectTransform>().rect.width, (int)img.GetComponent<RectTransform>().rect.height, 24, RenderTextureFormat.ARGB32);
            rt.useMipMap = true;
            rt.filterMode = FilterMode.Trilinear;
            rt.antiAliasing = 4;
            rt.Create();
            ca.targetTexture = rt;
            img.texture = rt;
        }
        else
            UIUtils.Log("RowImage或Camera不存在！");
    }

    /// <summary> 下载图片并保存到指定目录</summary>
    public static IEnumerator DownloadImage(string url, Action<Sprite> callback = null, string path = null)
    {
        WWW www = new WWW(url);
        yield return www;
        Sprite sprite = null;
        if (string.IsNullOrEmpty(www.error))
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string filePath = ConstantUtils.SpriteFolderPath + path;
                    if (!Directory.Exists(filePath.Substring(0, filePath.LastIndexOf('/'))))
                        Directory.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf('/')));
                    File.WriteAllBytes(filePath, www.texture.EncodeToPNG());
                }
                sprite = TextureToSprite(www.texture);
                if (callback != null)
                    callback(sprite);
            }
            catch (Exception e)
            {
                UIUtils.Log("下载失败：url: " + url + ", error: " + e.Message);
            }
        }
        else
        {
            UIUtils.Log("下载失败：url: " + url + ", error: " + www.error);
        }
    }

    /// <summary> Texture转Sprite</summary>
    public static Sprite TextureToSprite(Texture texture)
    {
        Sprite sprite = null;
        if (texture)
        {
            Texture2D t2d = (Texture2D)texture;
            sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
        }
        return sprite;
    }

    /// <summary> Texture旋转</summary>
    public static Texture2D RotateTexture(Texture2D texture, float eulerAngles)
    {
        int x;
        int y;
        int i;
        int j;
        float phi = eulerAngles / (180 / Mathf.PI);
        float sn = Mathf.Sin(phi);
        float cs = Mathf.Cos(phi);
        Color32[] arr = texture.GetPixels32();
        Color32[] arr2 = new Color32[arr.Length];
        int W = texture.width;
        int H = texture.height;
        int xc = W / 2;
        int yc = H / 2;

        for (j = 0; j < H; j++)
        {
            for (i = 0; i < W; i++)
            {
                arr2[j * W + i] = new Color32(0, 0, 0, 0);

                x = (int)(cs * (i - xc) + sn * (j - yc) + xc);
                y = (int)(-sn * (i - xc) + cs * (j - yc) + yc);

                if ((x > -1) && (x < W) && (y > -1) && (y < H))
                {
                    arr2[j * W + i] = arr[y * W + x];
                }
            }
        }

        Texture2D newImg = new Texture2D(W, H);
        newImg.SetPixels32(arr2);
        newImg.Apply();

        return newImg;
    }

    /// <summary> 在指定目录下创建文本文件</summary>
    public static void CreateTextFile(string filePath, string contents)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, contents);
    }

    /// <summary> 复制文件夹到指定目录</summary>
    public static void CopyDirectory(string sourceDirectoryPath, string targetDirectoryPath, string searchPattern = "*.*", bool isDeleteExist = false)
    {
        string[] files = Directory.GetFiles(sourceDirectoryPath, searchPattern, SearchOption.AllDirectories);
        string file, newPath, newDir;
        for (int i = 0; i < files.Length; i++)
        {
            file = files[i];
            file = file.Replace("\\", "/");
            if (!file.EndsWith(".meta") && !file.EndsWith(".DS_Store"))
            {
                newPath = file.Replace(sourceDirectoryPath, targetDirectoryPath);
                newDir = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);
                if (File.Exists(newPath))
                    if (isDeleteExist)
                        File.Delete(newPath);
                    else
                        continue;
                if (Application.platform == RuntimePlatform.Android)
                    PageManager.Instance.StartCoroutine(AndroidCopyFile(file, newPath));
                else
                    File.Copy(file, newPath);
            }
        }
    }

    private static IEnumerator AndroidCopyFile(string sourceFilePath, string targetFilePath)
    {
        WWW www = new WWW("file://" + sourceFilePath);
        yield return www;
        File.WriteAllBytes(targetFilePath, UnicodeEncoding.UTF8.GetBytes(www.text));
    }

    /// <summary>获取指定路径的文件名，不包含后缀</summary>
    public static string GetFileName(string path)
    {
        path = path.Replace("\\", "/");
        if (path.IndexOf('/') >= 0)
        {
            if (path.IndexOf('.') > 0)
                return path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.'));
            else
                return path.Substring(path.LastIndexOf('/') + 1);
        }
        else
        {
            if (path.IndexOf('.') > 0)
                return path.Substring(0, path.LastIndexOf('.'));
            else
                return path;
        }
    }

    /// <summary>字节转兆,B->MB</summary>
    public static float GetMillionFromByte(long bytes)
    {
        return bytes / 1024f / 1024f;
    }

    /// <summary>获取枚举成员总数</summary>
    public static int GetEnumCount<T>()
    {
        if (typeof(T).IsEnum)
            return Enum.GetNames(typeof(T)).GetLength(0);
        return 0;
    }

    /// <summary>缩减字符串到指定字节长度</summary>
    public static string ReduceStringToLength(string str, int length)
    {
        int byteCount = Encoding.Default.GetByteCount(str);
        if (byteCount > length)
        {
            while (byteCount > length)
            {
                str = str.Substring(0, str.Length - 1);
                byteCount = Encoding.Default.GetByteCount(str);
            }
            return str + "...";
        }
        else
            return str;
    }

    /// <summary>
    /// 将数字转化为###.##万、百万、千万或###.##亿的结构,当数字小于99999时不转化
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string NumToString(long num)
    {
        if (num > 99999999)
        {
            double n = num * 0.00000001f;
            return n.ToString("F2") + "亿";
        }
        if (num > 9999999)
        {
            double n = num * 0.0000001f;
            return n.ToString("F2") + "千万";
        }
        if (num > 999999)
        {
            double n = num * 0.000001f;
            return n.ToString("F2") + "百万";
        }
        if (num > 99999)
        {
            double n = num * 0.0001f;
            return n.ToString("F2") + "万";
        }
        return num.ToString();
    }

    #region ...定位功能
    /// <summary>
    /// 获取用户定位，数组内为{经度,纬度},获取成功后关闭定位
    /// </summary>
    /// <returns>{经度,纬度}</returns>
    public static float[] GetLatitudeAndLongitude()
    {
        float[] fs = null;
        if (Input.location.status == LocationServiceStatus.Running)
        {
            fs = new float[] { Input.location.lastData.longitude, Input.location.lastData.latitude };
            Input.location.Stop();
        }
        return fs;
    }

    public static void StartGPS(float desiredAccuracyInMeters = 10, float updateDistanceInMeters = 10)
    {
        PageManager.Instance.StartCoroutine(StartGPSAc(desiredAccuracyInMeters, updateDistanceInMeters));
    }

    /// <summary> 开启定位</summary>
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
            UIUtils.Log("初始化GPS超时");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            UIUtils.Log("启用GPS定位失败");
            yield break;
        }
        else
        {
            UIUtils.Log("开启定位成功");
        }
    }

    public static void GetLocation(UnityAction<string> ua)
    {
        PageManager.Instance.StartCoroutine(GetLocationAc(ua));
    }

    /// <summary> 获取大致定位地址 </summary>
    public static IEnumerator GetLocationAc(UnityAction<string> ua)
    {
        float[] latAndlng = GetLatitudeAndLongitude();
        while (latAndlng == null)
        {
            yield return new WaitForSecondsRealtime(3);
            latAndlng = GetLatitudeAndLongitude();
        }
        WWW www = new WWW("http://api.map.baidu.com/geocoder/v2/?location=" + latAndlng[1] + "," + latAndlng[0] + "&output=json&pois=0&ak=hXj0aembE32aNLwNuf2NFv4Id3u407mF");
        //WWW www = new WWW("http://api.map.baidu.com/geocoder/v2/?pois=0&location=30.548883,104.053483&output=json&ak=hXj0aembE32aNLwNuf2NFv4Id3u407mF");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            try
            {
                JsonData jd = JsonMapper.ToObject(Regex.Unescape(www.text));
                string location = jd["result"]["addressComponent"].TryGetString("city");
                ua(location);
            }
            catch
            {
                TipManager.Instance.OpenTip(TipType.SimpleTip, "获取定位信息失败");
            }
        }
    }
    #endregion
}
