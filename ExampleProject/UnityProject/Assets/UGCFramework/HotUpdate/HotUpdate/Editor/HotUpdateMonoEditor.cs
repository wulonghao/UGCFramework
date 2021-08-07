using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UGCF.HotUpdate;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

[CustomEditor(typeof(HotUpdateMono))]
[CanEditMultipleObjects]
public class HotUpdateMonoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HotUpdateMono hotUpdate = (HotUpdateMono)target;
        MonoScript targetScript = (MonoScript)EditorGUILayout.ObjectField("HotUpdateClass",
            AssetDatabase.LoadAssetAtPath<UnityObject>(AssetDatabase.GUIDToAssetPath(hotUpdate.hotUpdateClassEditorGUID)),
            typeof(MonoScript),
            false);
        if (targetScript != null)
        {
            string targetTypeFullName = targetScript.GetClass().FullName;
            Type type = Assembly.Load("Assembly-CSharp").GetType(targetTypeFullName);
            if (typeof(HotUpdateMonoBehaviour).IsAssignableFrom(type))
            {
                hotUpdate.hotUpdateClassEditorGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(targetScript));
                hotUpdate.hotUpdateClassFullName = targetTypeFullName;
                RefreshFields(type);
            }
        }
        else
        {
            hotUpdate.hotUpdateClassEditorGUID = null;
            hotUpdate.hotUpdateClassFullName = null;
            hotUpdate.hotUpdateFields.Clear();
        }
        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    void RefreshFields(Type type)
    {
        HotUpdateMono hotUpdate = (HotUpdateMono)target;
        List<FieldInfo> fieldInfos = type.GetFieldsContainParent(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        for (int i = hotUpdate.hotUpdateFields.Count - 1; i >= 0; i--)
        {
            if (fieldInfos.Find((fi) => fi.Name == hotUpdate.hotUpdateFields[i].fieldName) == null)
                hotUpdate.hotUpdateFields.RemoveAt(i);
        }
        for (int i = 0; i < fieldInfos.Count; i++)
        {
            FieldInfo fieldInfo = fieldInfos[i];
            if (typeof(ValueType).IsAssignableFrom(fieldInfo.FieldType)
                || typeof(string).IsAssignableFrom(fieldInfo.FieldType)
                || typeof(UnityObject).IsAssignableFrom(fieldInfo.FieldType)
                || fieldInfo.FieldType.IsInheritTargetType(HotUpdateMono.HotUpdateMBTypeFullName))
            {
                bool hasSerializeField = false;
                bool hasHideInInspector = false;
                List<CustomAttributeData> customAttributeDatas = new List<CustomAttributeData>(fieldInfo.CustomAttributes);
                for (int j = 0; j < customAttributeDatas.Count; j++)
                {
                    CustomAttributeData customAttributeData = customAttributeDatas[j];
                    if (customAttributeData.AttributeType.Equals(typeof(HideInInspector)))
                        hasHideInInspector = true;
                    else if (customAttributeData.AttributeType.Equals(typeof(SerializeField)))
                        hasSerializeField = true;
                }
                if (hasSerializeField || (!hasHideInInspector && fieldInfo.IsPublic))
                {
                    HotUpdateField hotUpdateField = hotUpdate.hotUpdateFields.Find((hff) => { return hff.fieldName == fieldInfo.Name; });
                    if (hotUpdateField != null)
                        CreateEditorGUILayout(fieldInfo, hotUpdateField);
                    else
                        hotUpdate.hotUpdateFields.Add(new HotUpdateField(fieldInfo.Name, ""));
                }
            }
        }
    }

    void CreateEditorGUILayout(FieldInfo fieldInfo, HotUpdateField hotUpdateField)
    {
        if (fieldInfo.FieldType.Equals(typeof(string)) || fieldInfo.FieldType.Equals(typeof(char)))
            hotUpdateField.valueStr = EditorGUILayout.TextField(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? "" : hotUpdateField.valueStr.ToString());
        else if (fieldInfo.FieldType.Equals(typeof(long)))
            hotUpdateField.valueStr = EditorGUILayout.LongField(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? 0 : long.Parse(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(int)) || fieldInfo.FieldType.Equals(typeof(short)))
            hotUpdateField.valueStr = EditorGUILayout.IntField(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? 0 : int.Parse(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(float)))
            hotUpdateField.valueStr = EditorGUILayout.FloatField(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? 0 : float.Parse(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(double)))
            hotUpdateField.valueStr = EditorGUILayout.DoubleField(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? 0 : double.Parse(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(bool)))
            hotUpdateField.valueStr = EditorGUILayout.Toggle(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? true : bool.Parse(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(Vector2)))
            hotUpdateField.valueStr = EditorGUILayout.Vector2Field(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? Vector2.zero : GetVector2ByString(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(Vector3)))
            hotUpdateField.valueStr = EditorGUILayout.Vector3Field(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? Vector3.zero : GetVector3ByString(hotUpdateField.valueStr)).ToString();
        else if (fieldInfo.FieldType.Equals(typeof(Vector4)))
            hotUpdateField.valueStr = EditorGUILayout.Vector4Field(fieldInfo.Name, string.IsNullOrEmpty(hotUpdateField.valueStr) ? Vector4.zero : GetVector4ByString(hotUpdateField.valueStr)).ToString();
        else if (typeof(Enum).IsAssignableFrom(fieldInfo.FieldType))
        {
            string value = string.IsNullOrEmpty(hotUpdateField.valueStr) ? Enum.GetNames(fieldInfo.FieldType)[0] : Enum.Parse(fieldInfo.FieldType, hotUpdateField.valueStr).ToString();
            hotUpdateField.valueStr = EditorGUILayout.EnumPopup(fieldInfo.Name, (Enum)Enum.Parse(fieldInfo.FieldType, value)).ToString();
        }
        else if (typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
        {
            //TODO 未完待续
        }
        else if (fieldInfo.FieldType.IsInheritTargetType(HotUpdateMono.HotUpdateMBTypeFullName))
        {
            HotUpdateMono hotUpdateMono = EditorGUILayout.ObjectField(fieldInfo.Name, hotUpdateField.valueUnityObj, typeof(HotUpdateMono), true) as HotUpdateMono;
            if (hotUpdateMono == null)
            {
                hotUpdateField.valueUnityObj = null;
            }
            else
            {
                if (fieldInfo.FieldType.IsInheritTargetType(hotUpdateMono.hotUpdateClassFullName))
                    hotUpdateField.valueUnityObj = hotUpdateMono;
                else
                {
                    HotUpdateMono[] hotUpdateMonos = hotUpdateMono.gameObject.GetComponents<HotUpdateMono>();
                    if (hotUpdateMonos.Length > 1)
                    {
                        for (int i = 1; i < hotUpdateMonos.Length; i++)
                        {
                            HotUpdateMono mono = hotUpdateMonos[i];
                            if (fieldInfo.FieldType.IsInheritTargetType(mono.hotUpdateClassFullName))
                            {
                                hotUpdateField.valueUnityObj = mono;
                                return;
                            }
                        }
                    }
                    hotUpdateField.valueUnityObj = null;
                }
            }
        }
        else if (typeof(UnityObject).IsAssignableFrom(fieldInfo.FieldType))
            hotUpdateField.valueUnityObj = EditorGUILayout.ObjectField(fieldInfo.Name, hotUpdateField.valueUnityObj, fieldInfo.FieldType, true);
    }

    /// <summary>
    /// 字符串转Vector2
    /// </summary>
    /// <param name="vector2Str"></param>
    /// <param name="splitSymbol">分隔符</param>
    /// <returns></returns>
    Vector2 GetVector2ByString(string vector2Str, char splitSymbol = ',')
    {
        if (string.IsNullOrEmpty(vector2Str))
            return default;
        vector2Str = vector2Str.Replace("(", "").Replace(")", "");
        string[] vStr = vector2Str.Trim().Split(splitSymbol);
        Vector2 vector2 = new Vector2();
        vector2.x = string.IsNullOrEmpty(vStr[0]) ? 0 : float.Parse(vStr[0]);
        vector2.y = string.IsNullOrEmpty(vStr[1]) ? 0 : float.Parse(vStr[1]);
        return vector2;
    }

    /// <summary>
    /// 字符串转Vector3
    /// </summary>
    /// <param name="vector3Str"></param>
    /// <param name="splitSymbol">分隔符</param>
    /// <returns></returns>
    Vector3 GetVector3ByString(string vector3Str, char splitSymbol = ',')
    {
        if (string.IsNullOrEmpty(vector3Str))
            return default;
        vector3Str = vector3Str.Replace("(", "").Replace(")", "");
        string[] vStr = vector3Str.Split(splitSymbol);
        Vector3 vector3 = new Vector3();
        vector3.x = string.IsNullOrEmpty(vStr[0]) ? 0 : float.Parse(vStr[0]);
        vector3.y = string.IsNullOrEmpty(vStr[1]) ? 0 : float.Parse(vStr[1]);
        vector3.z = string.IsNullOrEmpty(vStr[2]) ? 0 : float.Parse(vStr[2]);
        return vector3;
    }

    /// <summary>
    /// 字符串转Vector4
    /// </summary>
    /// <param name="vector4Str"></param>
    /// <param name="splitSymbol">分隔符</param>
    /// <returns></returns>
    Vector4 GetVector4ByString(string vector4Str, char splitSymbol = ',')
    {
        if (string.IsNullOrEmpty(vector4Str))
            return default;
        vector4Str = vector4Str.Replace("(", "").Replace(")", "");
        string[] vStr = vector4Str.Split(splitSymbol);
        Vector4 vector4 = new Vector4();
        vector4.x = string.IsNullOrEmpty(vStr[0]) ? 0 : float.Parse(vStr[0]);
        vector4.y = string.IsNullOrEmpty(vStr[1]) ? 0 : float.Parse(vStr[1]);
        vector4.z = string.IsNullOrEmpty(vStr[2]) ? 0 : float.Parse(vStr[2]);
        vector4.w = string.IsNullOrEmpty(vStr[3]) ? 0 : float.Parse(vStr[3]);
        return vector4;
    }
}
