using UGCF.UGUIExtend;
using UnityEditor;
using UnityEditor.UI;

namespace UGCF.Editor
{
    [CustomEditor(typeof(CombinedChildScrollRect), true)]
    [CanEditMultipleObjects]
    public class CombinedChildScrollRectEditor : ScrollRectEditor
    {
        SerializedProperty parentScroll;
        SerializedProperty panelScrollRect;

        protected override void OnEnable()
        {
            base.OnEnable();
            parentScroll = serializedObject.FindProperty("parentScroll");
            panelScrollRect = serializedObject.FindProperty("panelScrollRect");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(parentScroll);
            EditorGUILayout.PropertyField(panelScrollRect);
            serializedObject.ApplyModifiedProperties();
        }
    }
}