using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tab), true)]
[CanEditMultipleObjects]
public class TabEditor : UnityEditor.UI.SelectableEditor
{
    SerializedProperty m_OnValueChangedProperty;
    SerializedProperty m_TransitionProperty;
    SerializedProperty m_GraphicProperty;
    SerializedProperty m_GroupProperty;
    SerializedProperty m_IsOnProperty;
    SerializedProperty m_targetPage;
    SerializedProperty m_isChangeActive;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
        m_GraphicProperty = serializedObject.FindProperty("graphic");
        m_GroupProperty = serializedObject.FindProperty("m_Group");
        m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
        m_targetPage = serializedObject.FindProperty("targetPage");
        m_isChangeActive = serializedObject.FindProperty("isChangeActive");
        m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_IsOnProperty);
        EditorGUILayout.PropertyField(m_TransitionProperty);
        EditorGUILayout.PropertyField(m_GraphicProperty);
        EditorGUILayout.PropertyField(m_targetPage);
        EditorGUILayout.PropertyField(m_GroupProperty);
        EditorGUILayout.PropertyField(m_isChangeActive);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_OnValueChangedProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
