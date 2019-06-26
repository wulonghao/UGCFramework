using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomImage))]
[CanEditMultipleObjects]
public class CustomImageEditor : UnityEditor.UI.GraphicEditor
{
    SerializedProperty allPoints;
    SerializedProperty m_Sprite;
    GUIContent m_SpriteContent;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_SpriteContent = new GUIContent("Source Image");
        m_Sprite = serializedObject.FindProperty("m_Sprite");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        allPoints = serializedObject.FindProperty("allPoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_RaycastTarget);

        EditorGUILayout.PropertyField(allPoints, true);
        SetShowNativeSize(true, true);
        NativeSizeButtonGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
