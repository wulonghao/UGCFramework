using LitJson;
using System.Collections;
using UnityEngine;

public static class LitJsonExtension
{
    public static string TryGetString(this JsonData self, string key, string defvalue = "")
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }

        if (((IDictionary)self).Contains(key))
        {
            if (self[key] == null)
                return defvalue;
            return self[key].ToString();
        }
        return defvalue;
    }

    public static bool TryGetBool(this JsonData self, string key, bool defvalue = false)
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }

        if (((IDictionary)self).Contains(key))
        {
            if (self[key] == null)
                return defvalue;
            if (bool.TryParse(self[key].ToString(), out bool result))
                return result;
        }
        return defvalue;
    }

    public static bool TryGetBoolBy_1_0(this JsonData self, string key, bool defvalue = false)
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }
        string result = TryGetString(self, key);
        switch (result)
        {
            case "1":
                return true;
            case "0":
                return false;
            default:
                LogUtils.LogError("转换失败，数据异常：" + result);
                return false;
        }
    }

    public static int TryGetInt(this JsonData self, string key, int defvalue = -1)
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }

        if (((IDictionary)self).Contains(key))
        {
            if (self[key] == null)
                return defvalue;
            if (int.TryParse(self[key].ToString(), out int result))
                return result;
        }
        return defvalue;
    }

    public static float TryGetFloat(this JsonData self, string key, float defvalue = -1)
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }

        if (((IDictionary)self).Contains(key))
        {
            if (self[key] == null)
                return defvalue;
            if (float.TryParse(self[key].ToString(), out float result))
                return result;
        }
        return defvalue;
    }

    public static double TryGetDouble(this JsonData self, string key, double defvalue = -1)
    {
        if (self == null || !self.IsObject)
        {
            return defvalue;
        }

        if (((IDictionary)self).Contains(key))
        {
            if (self[key] == null)
                return defvalue;
            if (double.TryParse(self[key].ToString(), out double result))
                return result;
        }
        return defvalue;
    }
}
