using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    /// <summary>时间戳转DateTime</summary>
    public static DateTime GetDateTimeByMillisecondsTimeStamp(long timeStamp)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
        return startTime.AddMilliseconds(timeStamp);
    }

    /// <summary>DateTime转时间戳</summary>
    public static long GetTimeStampByDateTime(DateTime time)
    {
        TimeSpan ts = time - new DateTime(1970, 1, 1, 8, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    /// <summary>DateTime转时间戳,毫秒</summary>
    public static long GetMillisecondsTimeStampByDateTime(DateTime time)
    {
        TimeSpan ts = time - new DateTime(1970, 1, 1, 8, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
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
        try
        {
            if (File.Exists(fileName))
            {
                FileStream file = File.OpenRead(fileName);
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
        catch (Exception e)
        {
            LogUtils.LogError(e.Message);
            return null;
        }
    }

    /// <summary>获取指定长度的随机字符串</summary>
    public static string GetRandomString(int length)
    {
        string str = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
            sb.Append(str.Substring(UnityEngine.Random.Range(0, str.Length), 1));
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

    /// <summary>对指定字符串进行Base64加密</summary>
    public static string EncodeBase64(Encoding encode, string source)
    {
        byte[] bytes = encode.GetBytes(source);
        if (bytes.Length > 0)
            source = Convert.ToBase64String(bytes);
        return source;
    }

    /// <summary>对指定字符串进行Base64解密</summary>
    public static string DecodeBase64(Encoding encode, string result)
    {
        byte[] bytes = Convert.FromBase64String(result);
        if (bytes.Length > 0)
            result = encode.GetString(bytes);
        return result;
    }

    /// <summary>获取当前渠道</summary>
    public static string GetChannel()
    {
        GameObject gameObject = GameObject.Find("EternalGameObject");
        if (!gameObject)
            return null;
        EternalGameObject eternalGameObject = gameObject.GetComponent<EternalGameObject>();
#if UNITY_ANDROID
        return eternalGameObject.androidBuildChannel.ToString();
#elif UNITY_IOS
        return eternalGameObject.iOSBuildChannel.ToString();
#else
        return null;
#endif
    }

    /// <summary>获取当前平台类型字符串</summary>
    public static string GetCurrentPlatform()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
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

    /// <summary>获取摄像机视图，显示在指定RawImage组件中</summary>
    public static void GetCameraRawImage(ref RawImage img, Camera ca)
    {
        if (img && ca)
        {
            RenderTexture rt = new RenderTexture((int)img.GetComponent<RectTransform>().rect.width, (int)img.GetComponent<RectTransform>().rect.height, 24, RenderTextureFormat.ARGB32);
            rt.useMipMap = false;
            rt.filterMode = FilterMode.Trilinear;
            rt.antiAliasing = 4;
            rt.Create();
            ca.targetTexture = rt;
            img.texture = rt;
        }
        else
            LogUtils.Log("RowImage或Camera不存在！");
    }

    /// <summary> 下载图片并保存到指定目录</summary> 
    public static IEnumerator DownloadImage(string url, Action<Sprite> callback = null, string filePath = null)
    {
        if (string.IsNullOrEmpty(url))
        {
            if (callback != null)
                callback(null);
            yield break;
        }
        UnityWebRequest uwr = new UnityWebRequest(url);
        DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(true);
        uwr.downloadHandler = downloadTexture;
        yield return uwr.SendWebRequest();
        Sprite sprite = null;
        if (uwr.isHttpError || uwr.isNetworkError)
        {
            LogUtils.Log("下载失败：url: " + url + ", error: " + uwr.error);
            if (callback != null)
                callback(null);
        }
        else
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (!Directory.Exists(filePath.Substring(0, filePath.LastIndexOf('/'))))
                        Directory.CreateDirectory(filePath.Substring(0, filePath.LastIndexOf('/')));
                    File.WriteAllBytes(filePath, downloadTexture.texture.EncodeToPNG());
                }
                sprite = TextureToSprite(downloadTexture.texture);
                if (callback != null)
                    callback(sprite);
            }
            catch (Exception e)
            {
                LogUtils.Log("下载失败：url: " + url + ", error: " + e.Message);
                if (callback != null)
                    callback(null);
            }
        }
    }

    /// <summary> 发起带参的Get请求</summary>
    public static void SendHttpGetRequest(string url, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback = null, int timeout = 30)
    {
        if (!IsNetworkConnect())
            return;
        PageManager.Instance.StartCoroutine(SendHttpGetRequestAc(url, requestSuccessCallback, requestFailCallback, timeout));
    }

    private static IEnumerator SendHttpGetRequestAc(string url, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback, int timeout)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        uwr.timeout = timeout;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            LogUtils.LogError(uwr.error + "，异常地址：" + url);
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(uwr);
        }
        else
        {
            if (requestSuccessCallback != null && requestSuccessCallback.Target != null)
                requestSuccessCallback(uwr);
        }
    }

    /// <summary> 发起带参的Post请求</summary>
    public static void SendHttpPostRequest(string url, WWWForm wwwf, Action<UnityWebRequest> requestSuccessCallback = null, Action<UnityWebRequest> requestFailCallback = null, int timeout = 30)
    {
        if (!IsNetworkConnect())
            return;
        PageManager.Instance.StartCoroutine(SendHttpPostRequestAc(url, wwwf, requestSuccessCallback, requestFailCallback, timeout));
    }

    private static IEnumerator SendHttpPostRequestAc(string url, WWWForm wwwf, Action<UnityWebRequest> requestSuccessCallback, Action<UnityWebRequest> requestFailCallback, int timeout)
    {
        UnityWebRequest uwr = UnityWebRequest.Post(url, wwwf);
        uwr.timeout = timeout;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            LogUtils.LogError(uwr.error + "，异常地址：" + url);
            if (requestFailCallback != null && requestFailCallback.Target != null)
                requestFailCallback(uwr);
        }
        else
        {
            if (requestSuccessCallback != null && requestSuccessCallback.Target != null)
                requestSuccessCallback(uwr);
        }
    }

    /// <summary> 复制文件夹到指定目录</summary>
    public static void CopyDirectory(string sourceDirectoryPath, string targetDirectoryPath, bool isDeleteExist, bool isReplace = false, string searchPattern = "*.*")
    {
        if (isReplace && Directory.Exists(targetDirectoryPath))
            Directory.Delete(targetDirectoryPath, true);
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
                {
                    if (isDeleteExist)
                        File.Delete(newPath);
                    else
                        continue;
                }
                File.Copy(file, newPath);
            }
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
    public static bool CreateTextFile(string filePath, string contents)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(filePath, contents);
            return true;
        }
        catch (Exception e)
        {
            LogUtils.LogError("创建文件失败：" + e.ToString());
            return false;
        }
    }

    /// <summary> 在指定目录下创建二进制文件</summary>
    public static bool CreateBytesFile(string filePath, byte[] bytes)
    {
        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(filePath, bytes);
            return true;
        }
        catch (Exception e)
        {
            LogUtils.LogError("创建文件失败：" + e.ToString());
            return false;
        }
    }

    /// <summary>字节转兆,B->MB</summary>
    public static float GetMillionFromByte(long bytes)
    {
        return bytes / 1048576f;
    }

    /// <summary>获取枚举成员总数</summary>
    public static int GetEnumCount<T>()
    {
        if (typeof(T).IsEnum)
            return Enum.GetNames(typeof(T)).GetLength(0);
        return 0;
    }

    /// <summary>采用GBK编码缩减字符串到指定字节长度，超出部分使用“...”代替</summary>
    public static string ReduceStringToLength(string str, int length)
    {
        try
        {
            int byteCount = GetStringLengthByGBK(str);
            if (byteCount > length)
            {
                length -= 2;//超出指定长度，则缩减2个字节，使用"..."结尾
                while (byteCount > length)
                {
                    str = str.Substring(0, str.Length - 1);
                    byteCount = GetStringLengthByGBK(str);
                }
                return str + "...";
            }
            else
                return str;
        }
        catch (Exception e)
        {
            LogUtils.Log(e.ToString());
            return str;
        }
    }

    /// <summary> 采用GBK编码的字节结构计算字符串的字节长度 中文2个字节 英文及符号1个字节 </summary>
    public static int GetStringLengthByGBK(string str)
    {
        int byteCount = Encoding.UTF8.GetByteCount(str);
        return (byteCount - str.Length) / 2 + str.Length;
    }

    /// <summary> 将数字转化为###.##万、百万、千万或###.##亿的结构,当数字小于99999时不转化 </summary>
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

    /// <summary> 数字转换中文 工具 </summary>
    /// <param name="numberStr"></param>
    public static string NumberToChinese(string numberStr)
    {
        string numStr = "0123456789";
        string chineseStr = "零一二三四五六七八九";
        char[] c = numberStr.ToCharArray();
        for (int i = 0; i < c.Length; i++)
        {
            int index = numStr.IndexOf(c[i]);
            if (index != -1)
                c[i] = chineseStr.ToCharArray()[index];
        }
        numStr = null;
        chineseStr = null;
        return new string(c);
    }

    /// <summary>
    /// 网络是否出于连接状态
    /// </summary>
    /// <returns></returns>
    public static bool IsNetworkConnect()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    #region ...定位功能
    public static void GetLocationByIP(UnityAction<object> ua)
    {
        WWWForm wwwf = new WWWForm();
        wwwf.AddField("ak", "mW4aUCRrRZIMM8mKWXdlVHq8pOdYEt2o");
        wwwf.AddField("coor", "bd09ll");
        SendHttpPostRequest("https://api.map.baidu.com/location/ip", wwwf, (uwr) =>
        {
            try
            {
                //LogUtils.Log(Regex.Unescape(www.text));
                JsonData jd = JsonMapper.ToObject(Regex.Unescape(uwr.downloadHandler.text));
                JsonData pointJd = jd["content"]["point"];
                JsonData addressComponentJd = jd["content"]["address_detail"];
                ua(new string[] {
                    pointJd.TryGetString("x"),
                    pointJd.TryGetString("y"),
                    addressComponentJd.TryGetString("province"),
                    addressComponentJd.TryGetString("city"),
                    addressComponentJd.TryGetString("city_code")
                });
            }
            catch (Exception e)
            {
                LogUtils.Log("获取定位信息失败:" + e.ToString());
            }
        });
    }
    #endregion
}
