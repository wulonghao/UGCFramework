using System;
using System.Collections;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace UGCF.Utils
{
    public static class MiscUtils
    {
        #region ...时间戳转换
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
        #endregion

        #region ...字符串相关处理
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

        /// <summary>采用GBK编码缩减字符串到指定字节长度，超出部分使用指定文本代替</summary>
        public static string ReduceStringToLength(string str, int length, string replaceTxt = "...")
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
                    return str + replaceTxt;
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
        #endregion

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

        #region ...文件创建、复制、处理等
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
        #endregion

        #region ...本机设备信息获取
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
        #endregion

        #region ...延迟执行函数
        /// <summary> 延迟一段时间执行目标函数，基于真实时间 </summary>
        public static Coroutine InvokeForRealTime(this MonoBehaviour mono, UnityAction ua, float delayTime)
        {
            return mono.StartCoroutine(InvokeForRealTimeAc(delayTime, ua));
        }

        private static IEnumerator InvokeForRealTimeAc(float delayTime, UnityAction ua)
        {
            if (delayTime > 0)
                yield return WaitForUtils.WaitForSecondsRealtime(delayTime);
            ua?.Invoke();
        }

        /// <summary> 延迟一段时间执行目标函数，基于非真实时间 </summary>
        public static Coroutine InvokeForUnrealTime(this MonoBehaviour mono, UnityAction ua, float delayTime)
        {
            return mono.StartCoroutine(InvokeForUnrealTimeAc(delayTime, ua));
        }

        private static IEnumerator InvokeForUnrealTimeAc(float delayTime, UnityAction ua)
        {
            if (delayTime > 0)
                yield return WaitForUtils.WaitForSecond(delayTime);
            ua?.Invoke();
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

        /// <summary>
        /// 网络是否出于连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsNetworkConnecting()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}