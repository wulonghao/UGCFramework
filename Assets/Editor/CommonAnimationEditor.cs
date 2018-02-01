using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(CommonAnimation))]
public class CommonAnimationEditor : Editor
{
    SerializedObject obj;
    SerializedProperty pointTime, pointList, pointDelayTime, space;
    SerializedProperty angleTime, angleList, angleDelayTime;
    SerializedProperty scaleTime, scaleList, scaleDelayTime;
    SerializedProperty colorTime, colorList, colorDelayTime;
    SerializedProperty alphaTime, alphaList, alphaDelayTime;
    SerializedProperty sizeTime, sizeList, sizeDelayTime;
    SerializedProperty fillAmountTime, fillAmountList, fillAmountDelayTime;
    SerializedProperty disType;

    void GetProperty()
    {
        pointTime = obj.FindProperty("pointTime");
        pointList = obj.FindProperty("pointList");
        pointDelayTime = obj.FindProperty("pointDelayTime");
        space = obj.FindProperty("space");

        angleTime = obj.FindProperty("angleTime");
        angleList = obj.FindProperty("angleList");
        angleDelayTime = obj.FindProperty("angleDelayTime");

        scaleTime = obj.FindProperty("scaleTime");
        scaleList = obj.FindProperty("scaleList");
        scaleDelayTime = obj.FindProperty("scaleDelayTime");

        colorTime = obj.FindProperty("colorTime");
        colorList = obj.FindProperty("colorList");
        colorDelayTime = obj.FindProperty("colorDelayTime");

        alphaTime = obj.FindProperty("alphaTime");
        alphaList = obj.FindProperty("alphaList");
        alphaDelayTime = obj.FindProperty("alphaDelayTime");

        sizeTime = obj.FindProperty("sizeTime");
        sizeList = obj.FindProperty("sizeList");
        sizeDelayTime = obj.FindProperty("sizeDelayTime");

        fillAmountTime = obj.FindProperty("fillAmountTime");
        fillAmountList = obj.FindProperty("fillAmountList");
        fillAmountDelayTime = obj.FindProperty("fillAmountDelayTime");

        disType = obj.FindProperty("disType");
    }

    public override void OnInspectorGUI()
    {
        obj = new SerializedObject(target);
        Transform curCaTf = ((CommonAnimation)obj.targetObject).transform;
        GetProperty();
        CommonAnimation ca = target as CommonAnimation;
        GUILayout.BeginHorizontal();
        {
            ca.isLoop = GUILayout.Toggle(ca.isLoop, "isLoop");
            ca.isPingPong = GUILayout.Toggle(ca.isPingPong, "isPingPong");
            ca.isFoward = GUILayout.Toggle(ca.isFoward, "isFoward");
            ca.isAutoPlay = GUILayout.Toggle(ca.isAutoPlay, "isAutoPlay");
            ca.isBackInit = GUILayout.Toggle(ca.isBackInit, "isBackInit");
            ca.isDisappear = GUILayout.Toggle(ca.isDisappear, "isDisappear");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            ca.point = GUILayout.Toggle(ca.point, "point");
            ca.angle = GUILayout.Toggle(ca.angle, "angle");
            ca.scale = GUILayout.Toggle(ca.scale, "scale");
            ca.color = GUILayout.Toggle(ca.color, "color");
            ca.alpha = GUILayout.Toggle(ca.alpha, "alpha");
            ca.size = GUILayout.Toggle(ca.size, "size");
            ca.fillAmount = GUILayout.Toggle(ca.fillAmount, "fillAmount");
        }
        GUILayout.EndHorizontal();

        if (ca.isDisappear)
            EditorGUILayout.PropertyField(disType);

        if (ca.isPingPong)
            ca.isLoop = true;

        if (ca.isLoop)
            ca.isBackInit = false;

        if (ca.isDisappear)
        {
            ca.isPingPong = false;
            ca.isLoop = false;
        }

        if (ca.point)
        {
            EditorGUILayout.PropertyField(pointTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(pointList, true);
                if (GUILayout.Button("添加当前坐标"))
                {
                    pointList.arraySize++;
                    if (space.enumValueIndex == 0)//世界坐标
                        pointList.GetArrayElementAtIndex(pointList.arraySize - 1).vector3Value = curCaTf.position;
                    else
                        pointList.GetArrayElementAtIndex(pointList.arraySize - 1).vector3Value = curCaTf.localPosition;
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(pointDelayTime);
            EditorGUILayout.PropertyField(space);
        }

        if (ca.angle)
        {
            EditorGUILayout.PropertyField(angleTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(angleList, true);
                if (GUILayout.Button("添加当前角度"))
                {
                    angleList.arraySize++;
                    angleList.GetArrayElementAtIndex(angleList.arraySize - 1).vector3Value = curCaTf.localEulerAngles;
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(angleDelayTime);
        }

        if (ca.scale)
        {
            EditorGUILayout.PropertyField(scaleTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(scaleList, true);
                if (GUILayout.Button("添加当前比例系数"))
                {
                    scaleList.arraySize++;
                    scaleList.GetArrayElementAtIndex(scaleList.arraySize - 1).vector3Value = curCaTf.localScale;
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(scaleDelayTime);
        }

        if (ca.color)
        {
            EditorGUILayout.PropertyField(colorTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(colorList, true);
                if (GUILayout.Button("添加当前元素的颜色"))
                {
                    if (curCaTf.GetComponent<Graphic>())
                    {
                        colorList.arraySize++;
                        colorList.GetArrayElementAtIndex(colorList.arraySize - 1).colorValue = curCaTf.GetComponent<Graphic>().color;
                    }
                    else
                        Debug.Log("没有可以获取到颜色信息的组件");
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(colorDelayTime);
        }

        if (ca.alpha)
        {
            EditorGUILayout.PropertyField(alphaTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(alphaList, true);
                if (GUILayout.Button("添加当前的透明度"))
                {
                    if (curCaTf.GetComponent<CanvasGroup>())
                    {
                        alphaList.arraySize++;
                        alphaList.GetArrayElementAtIndex(alphaList.arraySize - 1).floatValue = curCaTf.GetComponent<CanvasGroup>().alpha;
                    }
                    else
                        Debug.Log("没有可以获取到透明度信息的组件,需要添加CanvasGroup组件");
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(alphaDelayTime);
        }

        if (ca.size)
        {
            EditorGUILayout.PropertyField(sizeTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(sizeList, true);
                if (GUILayout.Button("添加当前尺寸"))
                {
                    sizeList.arraySize++;
                    sizeList.GetArrayElementAtIndex(sizeList.arraySize - 1).vector2Value = curCaTf.GetComponent<RectTransform>().rect.size;
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(sizeDelayTime);
        }

        if (ca.fillAmount)
        {
            EditorGUILayout.PropertyField(fillAmountTime);
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(fillAmountList, true);
                if (GUILayout.Button("添加当前的fillAmount值"))
                {
                    if (curCaTf.GetComponent<Image>())
                    {
                        fillAmountList.arraySize++;
                        fillAmountList.GetArrayElementAtIndex(fillAmountList.arraySize - 1).floatValue = curCaTf.GetComponent<Image>().fillAmount;
                    }
                    else
                        Debug.Log("没有可以获取到fillAmount值的组件,需要添加Image组件");
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(fillAmountDelayTime);
        }
        obj.ApplyModifiedProperties();
    }
}