using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UGCF.Manager;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UGCF.Utils
{
    public partial class MiscUtils
    {
        /// <summary> 初始化目标的UI属性 </summary>
        public static void AttachAndReset(GameObject go, Transform parent, GameObject prefab = null)
        {
            RectTransform rectTrans = go.transform as RectTransform;
            if (rectTrans)
            {
                rectTrans.SetParent(parent);
                rectTrans.localPosition = Vector3.zero;
                rectTrans.localScale = Vector3.one;
                if (prefab == null)
                {
                    rectTrans.sizeDelta = Vector2.zero;
                    rectTrans.localPosition = Vector2.zero;
                    rectTrans.offsetMax = Vector2.zero;
                    rectTrans.offsetMin = Vector2.zero;
                }
                else
                {
                    RectTransform prefabRectTrans = prefab.transform as RectTransform;
                    if (prefabRectTrans)
                    {
                        rectTrans.sizeDelta = prefabRectTrans.sizeDelta;
                        rectTrans.localPosition = prefabRectTrans.localPosition;
                        rectTrans.offsetMax = prefabRectTrans.offsetMax;
                        rectTrans.offsetMin = prefabRectTrans.offsetMin;
                    }
                }
            }
        }

        /// <summary> 双线性插值法缩放图片，等比缩放 </summary>
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

        /// <summary> 双线性插值法缩放图片为指定尺寸 </summary>
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

        /// <summary>
        /// 返回时间戳（单位：毫秒）代表的时间。
        /// </summary>
        /// <param name="timestamp">时间戳（单位：毫秒）。</param>
        /// <returns>时间戳（单位：毫秒）代表的时间。</returns>
        public static DateTime TimestampInMillisecondsToDateTime(long timestamp)
        {
            var dateTime19700101 = new DateTime(1970, 1, 1);
            return dateTime19700101.AddMilliseconds(timestamp) + TimeZoneInfo.Local.GetUtcOffset(dateTime19700101);
        }

        /// <summary>
        /// 返回时间戳（单位：秒）代表的时间。
        /// </summary>
        /// <param name="timestamp">时间戳（单位：秒）。</param>
        /// <returns>时间戳（单位：秒）代表的时间。</returns>
        public static DateTime TimestampInSecondsToDateTime(long timestamp)
        {
            var dateTime19700101 = new DateTime(1970, 1, 1);
            return dateTime19700101.AddSeconds(timestamp) + TimeZoneInfo.Local.GetUtcOffset(dateTime19700101);
        }

        /// <summary>
        /// 返回时间代表的时间戳（单位：毫秒）。
        /// </summary>
        /// <param name="dateTime">时间。</param>
        /// <returns>时间代表的时间戳（单位：毫秒）。</returns>
        public static long DateTimeToTimestampInMilliseconds(DateTime dateTime)
        {
            var dateTime19700101 = new DateTime(1970, 1, 1);
            return Convert.ToInt64((dateTime - dateTime19700101 - TimeZoneInfo.Local.GetUtcOffset(dateTime19700101)).TotalMilliseconds);
        }

        /// <summary>
        /// 返回时间代表的时间戳（单位：秒）。
        /// </summary>
        /// <param name="dateTime">时间。</param>
        /// <returns>时间代表的时间戳（单位：秒）。</returns>
        public static long DateTimeToTimestampInSeconds(DateTime dateTime)
        {
            var dateTime19700101 = new DateTime(1970, 1, 1);
            return Convert.ToInt64((dateTime - dateTime19700101 - TimeZoneInfo.Local.GetUtcOffset(dateTime19700101)).TotalSeconds);
        }

        /// <summary> 数字转换中文 工具 </summary>
        public static string NumToString(int num)
        {
            string unit = "零十百千万";
            string[] numStr = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            char[] ns = num.ToString().ToCharArray();
            int zeroCount = 0;
            bool isZ = false;
            bool isInsertZero = false;
            StringBuilder sbNum = new StringBuilder();
            for (int i = ns.Length - 1; i >= 0; i--)
            {
                if (ns[i] == '0')
                {
                    isZ = true;
                    zeroCount++;
                    continue;
                }
                else
                {
                    if (isZ)
                    {
                        if (isInsertZero)
                        {
                            sbNum.Append(unit[0]);
                            isInsertZero = false;
                        }
                        sbNum.Append(unit[zeroCount]);
                    }
                    sbNum.Append(numStr[int.Parse(ns[i].ToString())]);
                    if (i != 0)
                    {
                        if (ns[i - 1] != '0')
                        {
                            sbNum.Append(unit[ns.Length - i]);
                        }
                        else { zeroCount++; isInsertZero = true; }
                    }
                    if (isZ)
                    {
                        isZ = false;
                    }
                }
            }

            Char[] a = sbNum.ToString().ToCharArray();
            Array.Reverse(a);
            string numTostr = new string(a);
            if (numTostr.StartsWith("一十"))
            {
                numTostr = numTostr.Replace("一十", "十");
            }
            return numTostr;
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

        #region ...加密解密相关
        /// <summary>获取文件的md5校验码</summary>
        public static string GetMD5HashFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
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
                LogUtils.LogError(e.ToString(), false);
                return null;
            }
        }

        /// <summary>对指定字符串进行Sha1加密</summary>
        public static string GetSha1Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] outputBytes = sha1.ComputeHash(inputBytes);
            string output = BitConverter.ToString(outputBytes).Replace("-", "");
            return output.ToLower();
        }

        /// <summary>对指定字符串进行Md5加密</summary>
        public static string GetMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] outputBytes = md5.ComputeHash(inputBytes);
            string output = BitConverter.ToString(outputBytes).Replace("-", "");
            return output.ToLower();
        }

        /// <summary>对指定字符串进行Base64加密</summary>
        public static string EncodeBase64(Encoding encode, string source)
        {
            return EncodeBase64(encode.GetBytes(source));
        }

        /// <summary>对指定字节组进行Base64加密</summary>
        public static string EncodeBase64(byte[] source)
        {
            if (source.Length > 0)
                return Convert.ToBase64String(source);
            return null;
        }

        /// <summary>对指定字符串进行Base64解密</summary>
        public static string DecodeBase64(Encoding encode, string result)
        {
            byte[] bytes = Convert.FromBase64String(result);
            if (bytes.Length > 0)
                result = encode.GetString(bytes);
            return result;
        }

        /// <summary>对指定字符串进行Base64解密</summary>
        public static byte[] DecodeBase64ToBytes(string result)
        {
            return Convert.FromBase64String(result);
        }

        /// <summary> 对指定字符串进行AES加密</summary>
        public static string AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key)) return null;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            byte[] resultArray = rm.CreateEncryptor().TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            string temp = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            return temp;
        }

        /// <summary> 对指定字符串进行RSA加密</summary>
        public static string RSAEncrypt(string content, string rsaKey)
        {
            if (string.IsNullOrEmpty(content)) return null;
            string xmlPublicKey = $"<RSAKeyValue><Modulus>{rsaKey}</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            string encryptedContent = string.Empty;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(xmlPublicKey);
                byte[] encryptedData = rsa.Encrypt(Encoding.Default.GetBytes(content), false);
                encryptedContent = Convert.ToBase64String(encryptedData);
            }
            return encryptedContent;
        }
        #endregion

        /// <summary>获取当前平台类型字符串</summary>
        public static string GetCurrentPlatform()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return string.Empty;
#endif
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

        /// <summary>
        /// 匹配对应Json的对应数值并全部返回
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value">对比的目标值</param>
        /// <param name="columnIndex">对比 列索引</param>
        /// <param name="isMultiple">是否返回多条</param>
        /// <returns></returns>
        public static JsonData MathJsonByIndex(JsonData self, string value, int columnIndex = 0, bool isMultiple = true)
        {
            if (self == null || self.Count <= 0)
            {
                return null;
            }
            if (isMultiple)
            {
                JsonData _localjson = new JsonData();
                bool _result = false;
                for (int i = 0; i < self.Count; i++)
                {
                    if (self[i][columnIndex].ToString() == value)
                    {
                        _localjson.Add(self[i]);
                        _result = true;
                    }
                }
                if (_result)
                {
                    return _localjson;
                }

            }
            else
            {
                for (int i = 0; i < self.Count; i++)
                {
                    if (self[i][columnIndex].ToString() == value)
                    {
                        return self[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 匹配对应Json的对应数值并全部返回
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value">对比的目标值</param>
        /// <param name="key">对比的目标key</param>
        /// <param name="isMultiple">是否返回多条</param>
        /// <returns></returns>
        public static JsonData MathJsonByIndex(JsonData self, string value, string key, bool isMultiple = true)
        {
            if (self == null || self.Count <= 0)
            {
                LogUtils.Log("传入的表是空的");
                return null;
            }
            if (isMultiple)
            {
                JsonData _localjson = new JsonData();
                bool _result = false;
                for (int i = 0; i < self.Count; i++)
                {
                    if (self[i][key].ToString() == value)
                    {
                        _localjson.Add(self[i]);
                        _result = true;
                    }
                }
                if (_result)
                {
                    return _localjson;
                }
            }
            else
            {
                for (int i = 0; i < self.Count; i++)
                {
                    if (self[i][key].ToString() == value)
                    {
                        return self[i];
                    }
                }
            }
            return null;
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
        public static void GetCameraRawImage(ref RawImage img, Camera ca, int depthBuffer = 24, RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            if (img && ca)
            {
                Rect rect = img.GetComponent<RectTransform>().rect;
                RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, depthBuffer, format);
                rt.useMipMap = false;
                rt.filterMode = FilterMode.Bilinear;
                rt.antiAliasing = 4;
                rt.Create();
                ca.targetTexture = rt;
                img.texture = rt;
            }
            else
                LogUtils.Log("RowImage或Camera不存在！");
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
                LogUtils.LogError("创建文件失败：" + e.ToString(), false);
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
                LogUtils.LogError("创建文件失败：" + e.ToString(), false);
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
            if (string.IsNullOrEmpty(str))
                return 0;
            int byteCount = Encoding.UTF8.GetByteCount(str);
            return (byteCount - str.Length) / 2 + str.Length;
        }

        /// <summary>
        /// 返回两个向量的夹角
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;
            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }

        /// <summary>
        /// 网络是否出于连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsNetworkConnecting()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// 获取设备的mac地址
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceMacAddress()
        {
            string physicalAddress = "";
            NetworkInterface[] nice = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adaper in nice)
            {
                if (adaper.Description == "en0")
                {
                    physicalAddress = adaper.GetPhysicalAddress().ToString();
                    break;
                }
                else
                {
                    physicalAddress = adaper.GetPhysicalAddress().ToString();
                    if (physicalAddress != "")
                    {
                        break;
                    }
                }
            }
            return physicalAddress;
        }

        /// <summary>
        /// 获取mac
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceIMEI_IDFA()
        {
            string info = string.Empty;
#if UNITY_IOS
        info = Device.advertisingIdentifier;
#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string imei0 = "";
            string imei1 = "";
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var telephoneyManager = context.Call<AndroidJavaObject>("getSystemService", "phone");
            imei0 = telephoneyManager.Call<string>("getImei", 0);//如果手机双卡 双待  就会有两个MIEI号
            imei1 = telephoneyManager.Call<string>("getImei", 1);//如果手机双卡 双待  就会有两个MIEI号
            info = imei0 + (imei1 != null ? "," + imei1 : string.Empty);
        }
        catch (Exception e)
        {
            //不给权限
        }
#endif
            return info;
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <param name="addressType">地址类型 1-ipv4，2-ipv6</param>
        /// <returns></returns>
        public static string GetIP(int addressType)
        {
            if (addressType == 2 && !Socket.OSSupportsIPv6)
                return null;

            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (addressType == 1 && ip.Address.AddressFamily == AddressFamily.InterNetwork)//IPv4
                        {
                            output = ip.Address.ToString();
                        }
                        else if (addressType == 2 && ip.Address.AddressFamily == AddressFamily.InterNetworkV6)//IPv6
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// 获取设备的AndroidId
        /// </summary>
        /// <returns></returns>
        public static string GetAndroidId()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        return secure.CallStatic<string>("getString", contentResolver, "android_id");
#else
            return null;
#endif
        }
    }
}