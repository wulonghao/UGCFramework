using System.Collections;
using System.Collections.Generic;
using UGCF.UGUIExtend;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UGCF.UGCFEditor
{
    [CustomEditor(typeof(CombinedChildScrollRect), true)]
    [CanEditMultipleObjects]
    public class CombinedChildScrollRectEditor : ScrollRectEditor
    {
        SerializedProperty parentScroll;
        protected override void OnEnable()
        {
            base.OnEnable();
            parentScroll = serializedObject.FindProperty("parentScroll");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(parentScroll);
            serializedObject.ApplyModifiedProperties();
        }
    }
}