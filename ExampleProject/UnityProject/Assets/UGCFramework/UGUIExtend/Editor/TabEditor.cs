using UGCF.UGUIExtend;
using UnityEditor;
using UnityEditor.UI;

namespace UGCF.Editor
{
    [CustomEditor(typeof(Tab), true)]
    [CanEditMultipleObjects]
    public class TabEditor : SelectableEditor
    {
        SerializedProperty m_OnValueChangedProperty;
        SerializedProperty m_TransitionProperty;
        SerializedProperty m_GraphicProperty;
        SerializedProperty m_GroupProperty;
        SerializedProperty m_IsOnProperty;
        SerializedProperty m_targetPage;
        SerializedProperty m_label;
        SerializedProperty m_isChangeActive;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
            m_GraphicProperty = serializedObject.FindProperty("graphic");
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
            m_targetPage = serializedObject.FindProperty("targetPage");
            m_label = serializedObject.FindProperty("label");
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
            EditorGUILayout.PropertyField(m_label);
            EditorGUILayout.PropertyField(m_GroupProperty);
            EditorGUILayout.PropertyField(m_isChangeActive);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}