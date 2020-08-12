using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UGCF.UGUIExtend
{
    [CustomEditor(typeof(RoundRectangleImage))]
    [CanEditMultipleObjects]
    public class RoundRectangleImageEditor : UnityEditor.UI.GraphicEditor
    {
        SerializedProperty segements;
        SerializedProperty roundRectangleRadius;
        SerializedProperty fillPercent;
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
            segements = serializedObject.FindProperty("segements");
            roundRectangleRadius = serializedObject.FindProperty("roundRectangleRadius");
            fillPercent = serializedObject.FindProperty("fillPercent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Sprite, m_SpriteContent);
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.PropertyField(m_Material);
            EditorGUILayout.PropertyField(m_RaycastTarget);
            EditorGUILayout.IntSlider(segements, 3, 100);
            Rect rect = ((RoundRectangleImage)target).GetComponent<RectTransform>().rect;
            int maxRadius = (int)(Mathf.Min(rect.width, rect.height) * 0.5f);
            EditorGUILayout.IntSlider(roundRectangleRadius, 1, maxRadius);
            EditorGUILayout.Slider(fillPercent, 0, 1);
            SetShowNativeSize(true, true);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}