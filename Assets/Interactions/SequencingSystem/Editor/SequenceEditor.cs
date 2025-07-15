using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kandooz.Kuest.Editors
{
    [CustomEditor(typeof(Sequence))]
    public class SequenceEditor : Editor
    {
        private ReorderableList _stepList;
        private Sequence sequence;

        private void OnEnable()
        {
            sequence = (Sequence)target;
            _stepList = new ReorderableList(serializedObject, serializedObject.FindProperty("steps"), true, true, true,
                true);
            _stepList.onAddCallback += OnAddCallback;
            _stepList.onRemoveCallback += OnRemoveCallback;
            _stepList.drawElementCallback += DrawElementCallback;
            _stepList.onReorderCallback += OnReorderCallback;
        }

        private void OnReorderCallback(ReorderableList list)
        {
            var count = list.serializedProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                var obj = list.serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                var semiIndex = obj.name.IndexOf('_');
                obj.name = $"{i}_{obj.name.Substring(semiIndex + 1)}";
            }

            var path = AssetDatabase.GetAssetPath(sequence);
            var size = list.count;
            list.serializedProperty.InsertArrayElementAtIndex(size);
            AssetDatabase.ImportAsset(path);
            list.serializedProperty.DeleteArrayElementAtIndex(size);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var nameRect = rect;
            var objRect = nameRect;
            nameRect.width = rect.width / 3 - 2;
            objRect.width = rect.width * 2f / 3 - 2;
            objRect.x += nameRect.width + 4;
            var elementName = _stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
            var semiIndex = elementName.IndexOf('_') + 1;
            elementName = elementName.Substring(semiIndex);
            var newName = EditorGUI.TextField(nameRect, elementName);
            if (newName != elementName)
            {
                _stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name = $"{index}_{newName}";
                OnReorderCallback(_stepList);
            }

            EditorGUI.PropertyField(objRect, _stepList.serializedProperty.GetArrayElementAtIndex(index),
                new GUIContent());
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            var item = list.serializedProperty.GetArrayElementAtIndex(list.index);
            AssetDatabase.RemoveObjectFromAsset(item.objectReferenceValue);
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            OnReorderCallback(list);
        }

        private void OnAddCallback(ReorderableList list)
        {
            var path = AssetDatabase.GetAssetPath(sequence);
            var step = ScriptableObject.CreateInstance<Step>();
            var index = list.serializedProperty.arraySize;
            step.name = $"{index}_step";
            list.serializedProperty.InsertArrayElementAtIndex(index);

            AssetDatabase.AddObjectToAsset(step, $"{path}");
            AssetDatabase.ImportAsset(path);
            serializedObject.ApplyModifiedProperties();
            list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = step;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _stepList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Update Quest object"))
            {
                var obj=new GameObject(sequence.name);
                obj.AddComponent<SequenceStarter>().sequence = sequence;
                foreach (var step in sequence.Steps)
                {
                    var stepObj = new GameObject(step.name);
                    stepObj.AddComponent<StepEvenListener>().step = step;
                    stepObj.transform.parent = obj.transform;
                }
            }
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start quest"))
                {
                    sequence.Begin();
                }

                GUI.enabled = false;
                EditorGUILayout.LabelField(sequence.CurrentStep.ToString());
                GUI.enabled = true;

            }
        }
    }
}