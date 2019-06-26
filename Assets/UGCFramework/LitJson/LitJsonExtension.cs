using LitJson;
using System.Collections;
using UnityEngine;

public static class LitJsonExtension
{
    public static string TryGetString(this JsonData self, string key, string defvalue = "")
    {
        if (!self.IsObject)
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

    public static int IntParse(this JsonData jsonData)
    {
        if (jsonData == null)
        {
            LogUtils.Log("[LitJsonExtension] IntPause null. ");
            return -1;
        }
        int result;
        if (int.TryParse(jsonData.ToString(), out result))
        {
            return result;
        }
        LogUtils.Log("[LitJsonExtension] IntPause error : " + jsonData.ToString());
        return -1;
    }

    public static float FloatParse(this JsonData jsonData)
    {
        if (jsonData == null)
        {
            LogUtils.Log("[LitJsonExtension] FloatParse null. ");
            return -1;
        }
        float result;
        if (float.TryParse(jsonData.ToString(), out result))
        {
            return result;
        }
        LogUtils.Log("[LitJsonExtension] FloatPause error " + jsonData.ToString());
        return -1;
    }

    public static bool BoolParse(this JsonData jsonData)
    {
        if (jsonData == null)
        {
            LogUtils.Log("[LitJsonExtension] BoolParse null. ");
            return false;
        }
        bool result;
        if (bool.TryParse(jsonData.ToString(), out result))
        {
            return result;
        }
        LogUtils.Log("[LitJsonExtension] BoolPause error " + jsonData.ToString());
        return false;
    }

    /// <summary>
    /// 强转 1=True 0=False
    /// </summary>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    public static bool BoolParseIn_1_0(this JsonData jsonData)
    {
        switch (jsonData.ToString())
        {
            case "1":
                return true;
            case "0":
                return false;
            default:
                LogUtils.Log("[LitJsonExtension] BoolPauseIn_1_0 error " + jsonData.ToString());
                return false;
        }
    }

    public static JsonData JsonDataParse(this string data)
    {
        return JsonMapper.ToObject(data);
    }

    public static int[][] JsonDataToIntArray(this JsonData jsonData, int column, int row)
    {
        if (jsonData == null || jsonData.Count == 0)
        {
            LogUtils.Log("[LitJsonExtension] JsonDataToIntArray error " + jsonData + " " + column + " " + row);
            return null;
        }
        int[][] array = new int[column][];
        for (int i = 0; i < column; i++)
        {
            array[i] = new int[row];
        }
        int index = 0;
        for (int x = 0; x < column; x++)
        {
            for (int y = 0; y < row; y++)
            {
                array[x][y] = int.Parse(jsonData[index++].ToString());
            }
        }
        return array;
    }

    public static int[][] JsonDataToIntArray(this string data, int column, int row)
    {
        if (string.IsNullOrEmpty(data))
        {
            LogUtils.Log("[LitJsonExtension] JsonDataToIntArray error " + data + " " + column + " " + row);
            return null;
        }
        JsonData jsonData = JsonMapper.ToObject(data);
        return jsonData.JsonDataToIntArray(column, row);
    }

    public static JsonData JsonCopy(this JsonData jsonData, JsonData _toData )
    {
        if (_toData == null)
        {
            _toData = new JsonData();
        }
        _toData.Clear();
        _toData = JsonMapper.ToObject(jsonData.ToJson());
        return _toData;
    }

}
