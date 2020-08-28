using UGCF.UnityExtend;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGCF.UGCFEditor
{
    [CustomEditor(typeof(CommonAnimation))]
    [CanEditMultipleObjects]
    public class CommonAnimationEditor : Editor
    {
        SerializedObject obj;
        SerializedProperty point, scale, alpha, color, size, angle, fillAmount;
        SerializedProperty isLoop, isPingPong, isFoward, isAutoPlay, isBackInit, isDisappear;
        SerializedProperty isPlayOnDisable;
        SerializedProperty disType;
        SerializedProperty pointAnimationList, scaleAnimationList, alphaAnimationList, colorAnimationList, sizeAnimationList, angleAnimationList, fillAmountAnimationList;
        int pointIndex, scaleIndex, alphaIndex, colorIndex, sizeIndex, angleIndex, fillAmountIndex;

        void GetProperty()
        {
            point = obj.FindProperty("point");
            scale = obj.FindProperty("scale");
            alpha = obj.FindProperty("alpha");
            color = obj.FindProperty("color");
            size = obj.FindProperty("size");
            angle = obj.FindProperty("angle");
            fillAmount = obj.FindProperty("fillAmount");

            isLoop = obj.FindProperty("isLoop");
            isPingPong = obj.FindProperty("isPingPong");
            isFoward = obj.FindProperty("isFoward");
            isAutoPlay = obj.FindProperty("isAutoPlay");
            isBackInit = obj.FindProperty("isBackInit");
            isDisappear = obj.FindProperty("isDisappear");
            isPlayOnDisable = obj.FindProperty("isPlayOnDisable");
            disType = obj.FindProperty("disType");

            pointAnimationList = obj.FindProperty("pointAnimationList");
            scaleAnimationList = obj.FindProperty("scaleAnimationList");
            alphaAnimationList = obj.FindProperty("alphaAnimationList");
            colorAnimationList = obj.FindProperty("colorAnimationList");
            sizeAnimationList = obj.FindProperty("sizeAnimationList");
            angleAnimationList = obj.FindProperty("angleAnimationList");
            fillAmountAnimationList = obj.FindProperty("fillAmountAnimationList");
        }

        public override void OnInspectorGUI()
        {
            obj = new SerializedObject(target);
            GetProperty();
            CommonAnimation commonAnimationNew = (CommonAnimation)target;
            Transform curCaTf = commonAnimationNew.transform;
            GUILayout.Label("————————————动画基本属性————————————");
            {
                GUILayout.BeginHorizontal();
                {
                    isLoop.boolValue = GUILayout.Toggle(isLoop.boolValue, "isLoop");
                    isPingPong.boolValue = GUILayout.Toggle(isPingPong.boolValue, "isPingPong");
                    isFoward.boolValue = GUILayout.Toggle(isFoward.boolValue, "isFoward");
                    isPlayOnDisable.boolValue = GUILayout.Toggle(isPlayOnDisable.boolValue, "isPlayOnDisable");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    isAutoPlay.boolValue = GUILayout.Toggle(isAutoPlay.boolValue, "isAutoPlay");
                    isBackInit.boolValue = GUILayout.Toggle(isBackInit.boolValue, "isBackInit");
                    isDisappear.boolValue = GUILayout.Toggle(isDisappear.boolValue, "isDisappear");
                }
                GUILayout.EndHorizontal();
            }

            if (isDisappear.boolValue)
                EditorGUILayout.PropertyField(disType);

            GUILayout.Label("————————————可选动画种类————————————");
            {
                GUILayout.BeginHorizontal();
                {
                    if (!point.boolValue) point.boolValue = GUILayout.Toggle(point.boolValue, "point(位移)");
                    if (!angle.boolValue) angle.boolValue = GUILayout.Toggle(angle.boolValue, "angle(旋转)");
                    if (!scale.boolValue) scale.boolValue = GUILayout.Toggle(scale.boolValue, "scale(缩放)");
                    if (!size.boolValue) size.boolValue = GUILayout.Toggle(size.boolValue, "size(尺寸)");
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    if (!alpha.boolValue) alpha.boolValue = GUILayout.Toggle(alpha.boolValue, "alpha(透明度)");
                    if (!color.boolValue) color.boolValue = GUILayout.Toggle(color.boolValue, "color(颜色)");
                    if (!fillAmount.boolValue) fillAmount.boolValue = GUILayout.Toggle(fillAmount.boolValue, "fillAmount(图片填充)");
                }
                GUILayout.EndHorizontal();
            }

            //动画实体
            {
                //位移
                {
                    if (point.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        point.boolValue = GUILayout.Toggle(point.boolValue, "point(位移)");
                        pointAnimationList.arraySize = Mathf.Max(1, pointAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前坐标到索引：{0}", pointIndex)) && pointAnimationList.arraySize > 0)
                        {
                            SerializedProperty caSpace = pointAnimationList.GetArrayElementAtIndex(pointIndex).FindPropertyRelative("caSpace");
                            SerializedProperty list = pointAnimationList.GetArrayElementAtIndex(pointIndex).FindPropertyRelative("pointList");
                            list.arraySize++;
                            if (caSpace.enumValueIndex == 0)//世界坐标
                                list.GetArrayElementAtIndex(list.arraySize - 1).vector3Value = curCaTf.position;
                            else if (caSpace.enumValueIndex == 1)
                                list.GetArrayElementAtIndex(list.arraySize - 1).vector3Value = curCaTf.localPosition;
                            else
                                if (curCaTf.GetComponent<RectTransform>())
                                list.GetArrayElementAtIndex(list.arraySize - 1).vector3Value = curCaTf.GetComponent<RectTransform>().anchoredPosition3D;
                        }
                        if (GUILayout.Button("+")) { pointIndex = Mathf.Clamp(pointIndex + 1, 0, pointAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { pointIndex = Mathf.Clamp(pointIndex - 1, 0, pointAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(pointAnimationList, true);
                    }
                }
                //旋转
                {
                    if (angle.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        angle.boolValue = GUILayout.Toggle(angle.boolValue, "angle(旋转)");
                        angleAnimationList.arraySize = Mathf.Max(1, angleAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前角度到索引：{0}", angleIndex)) && angleAnimationList.arraySize > 0)
                        {
                            SerializedProperty list = angleAnimationList.GetArrayElementAtIndex(angleIndex).FindPropertyRelative("angleList");
                            list.arraySize++;
                            list.GetArrayElementAtIndex(list.arraySize - 1).vector3Value = curCaTf.localEulerAngles;
                        }
                        if (GUILayout.Button("+")) { angleIndex = Mathf.Clamp(angleIndex + 1, 0, angleAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { angleIndex = Mathf.Clamp(angleIndex - 1, 0, angleAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(angleAnimationList, true);
                    }
                }
                //缩放
                {
                    if (scale.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        scale.boolValue = GUILayout.Toggle(scale.boolValue, "scale(缩放)");
                        scaleAnimationList.arraySize = Mathf.Max(1, scaleAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前缩放比到索引：{0}", scaleIndex)) && scaleAnimationList.arraySize > 0)
                        {
                            SerializedProperty list = scaleAnimationList.GetArrayElementAtIndex(scaleIndex).FindPropertyRelative("scaleList");
                            list.arraySize++;
                            list.GetArrayElementAtIndex(list.arraySize - 1).vector3Value = curCaTf.localScale;
                        }
                        if (GUILayout.Button("+")) { scaleIndex = Mathf.Clamp(scaleIndex + 1, 0, angleAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { scaleIndex = Mathf.Clamp(scaleIndex - 1, 0, angleAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(scaleAnimationList, true);
                    }
                }
                //颜色
                {
                    if (color.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        color.boolValue = GUILayout.Toggle(color.boolValue, "color(颜色)");
                        colorAnimationList.arraySize = Mathf.Max(1, colorAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前颜色到索引：{0}", colorIndex)) && colorAnimationList.arraySize > 0)
                        {
                            if (curCaTf.GetComponent<Graphic>())
                            {
                                SerializedProperty list = colorAnimationList.GetArrayElementAtIndex(colorIndex).FindPropertyRelative("colorList");
                                list.arraySize++;
                                list.GetArrayElementAtIndex(list.arraySize - 1).colorValue = curCaTf.GetComponent<Graphic>().color;
                            }
                            else
                                Debug.Log("没有可以获取到颜色信息的组件");
                        }
                        if (GUILayout.Button("+")) { colorIndex = Mathf.Clamp(colorIndex + 1, 0, colorAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { colorIndex = Mathf.Clamp(colorIndex - 1, 0, colorAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(colorAnimationList, true);
                    }
                }
                //透明度变化
                {
                    if (alpha.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        alpha.boolValue = GUILayout.Toggle(alpha.boolValue, "alpha(透明度变化)");
                        alphaAnimationList.arraySize = Mathf.Max(1, alphaAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前透明度到索引：{0}", alphaIndex)) && alphaAnimationList.arraySize > 0)
                        {
                            if (curCaTf.GetComponent<CanvasGroup>())
                            {
                                SerializedProperty list = alphaAnimationList.GetArrayElementAtIndex(alphaIndex).FindPropertyRelative("alphaList");
                                list.arraySize++;
                                list.GetArrayElementAtIndex(list.arraySize - 1).floatValue = curCaTf.GetComponent<CanvasGroup>().alpha;
                            }
                            else
                                Debug.Log("没有可以获取到透明度信息的组件,需要添加CanvasGroup组件");
                        }
                        if (GUILayout.Button("+")) { alphaIndex = Mathf.Clamp(alphaIndex + 1, 0, alphaAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { alphaIndex = Mathf.Clamp(alphaIndex - 1, 0, alphaAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(alphaAnimationList, true);
                    }
                }
                //尺寸
                {
                    if (size.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        size.boolValue = GUILayout.Toggle(size.boolValue, "size(尺寸)");
                        sizeAnimationList.arraySize = Mathf.Max(1, sizeAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前尺寸到索引：{0}", sizeIndex)) && sizeAnimationList.arraySize > 0)
                        {
                            SerializedProperty list = sizeAnimationList.GetArrayElementAtIndex(sizeIndex).FindPropertyRelative("sizeList");
                            list.arraySize++;
                            list.GetArrayElementAtIndex(list.arraySize - 1).vector2Value = curCaTf.GetComponent<RectTransform>().rect.size;
                        }
                        if (GUILayout.Button("+")) { sizeIndex = Mathf.Clamp(sizeIndex + 1, 0, sizeAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { sizeIndex = Mathf.Clamp(sizeIndex - 1, 0, sizeAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(sizeAnimationList, true);
                    }
                }
                //图片填充
                {
                    if (fillAmount.boolValue)
                    {
                        GUILayout.Label("————————————————————————————————————");
                        GUILayout.BeginHorizontal();
                        fillAmount.boolValue = GUILayout.Toggle(fillAmount.boolValue, "fillAmount(图片填充)");
                        fillAmountAnimationList.arraySize = Mathf.Max(1, fillAmountAnimationList.arraySize);
                        if (GUILayout.Button(string.Format("添加当前填充比到索引：{0}", fillAmountIndex)) && fillAmountAnimationList.arraySize > 0)
                        {
                            if (curCaTf.GetComponent<Image>())
                            {
                                SerializedProperty list = fillAmountAnimationList.GetArrayElementAtIndex(fillAmountIndex).FindPropertyRelative("fillAmountList");
                                list.arraySize++;
                                list.GetArrayElementAtIndex(list.arraySize - 1).floatValue = curCaTf.GetComponent<Image>().fillAmount;
                            }
                            else
                                Debug.Log("没有可以获取到填充信息的组件,需要添加Image组件");
                        }
                        if (GUILayout.Button("+")) { fillAmountIndex = Mathf.Clamp(fillAmountIndex + 1, 0, fillAmountAnimationList.arraySize - 1); }
                        if (GUILayout.Button("-")) { fillAmountIndex = Mathf.Clamp(fillAmountIndex - 1, 0, fillAmountAnimationList.arraySize - 1); }
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(fillAmountAnimationList, true);
                    }
                }
            }

            if (isPingPong.boolValue)
                isLoop.boolValue = true;

            if (isLoop.boolValue)
                isBackInit.boolValue = false;

            if (isDisappear.boolValue)
            {
                isPingPong.boolValue = false;
                isLoop.boolValue = false;
            }
            obj.ApplyModifiedProperties();
        }
    }
}