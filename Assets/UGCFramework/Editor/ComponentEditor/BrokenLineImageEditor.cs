using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BrokenLineImage))]
[CanEditMultipleObjects]
public class BrokenLineImageEditor : UnityEditor.UI.GraphicEditor
{
    SerializedProperty height;
    SerializedProperty width;
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

        height = serializedObject.FindProperty("height");
        width = serializedObject.FindProperty("width");
        allPoints = serializedObject.FindProperty("allPoints");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_RaycastTarget);

        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(allPoints, true);
        serializedObject.ApplyModifiedProperties();
    }
}
