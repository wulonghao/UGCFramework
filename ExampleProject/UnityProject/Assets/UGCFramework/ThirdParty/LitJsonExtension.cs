using LitJson;
using System.Collections;
using UGCF.Utils;
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

    /// <summary>
    /// 匹配对应Json的对应数值并全部返回
    /// </summary>
    /// <param name="self"></param>
    /// <param name="value">对比的目标值</param>
    /// <param name="columnIndex">对比 列索引</param>
    /// <param name="isMultiple">是否返回多条</param>
    /// <returns></returns>
    public static JsonData MathJsonByIndex(this JsonData self, string value, int columnIndex = 0, bool isMultiple = true)
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
    public static JsonData MathJsonByIndex(this JsonData self, string value, string key, bool isMultiple = true)
    {
        if (self == null || self.Count <= 0)
        {
            return null;
        }
        if (isMultiple)
        {
            JsonData localjson = new JsonData();
            bool result = false;
            for (int i = 0; i < self.Count; i++)
            {
                if (self[i][key].ToString() == value)
                {
                    localjson.Add(self[i]);
                    result = true;
                }
            }
            if (result)
            {
                return localjson;
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
}
