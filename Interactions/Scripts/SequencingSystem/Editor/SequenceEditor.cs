using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Custom editor for Sequence that provides a reorderable list of steps.
    /// </summary>
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
            serializedObject.ApplyModifiedProperties();
            for (var i = 0; i < sequence.Steps.Count; i++)
            {
                var obj = sequence.Steps[i];
                var semiIndex = obj.name.IndexOf('_');
                obj.name = $"{sequence.name}-{i+1}_{obj.name.Substring(semiIndex + 1)}";
            }

            var path = AssetDatabase.GetAssetPath(sequence);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (_stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue == null) return;
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
                _stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name = $"{sequence.name}-{index}_{newName}";
                OnReorderCallback(_stepList);
            }

            EditorGUI.PropertyField(objRect, _stepList.serializedProperty.GetArrayElementAtIndex(index),
                new GUIContent());
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            var item = sequence.Steps[list.index];
            sequence.Steps.RemoveAt(list.index);

            if (item == null) return;
            AssetDatabase.RemoveObjectFromAsset(item);
            AssetDatabase.SaveAssets();
            OnReorderCallback(list);
        }

        private void OnAddCallback(ReorderableList list)
        {
            var path = AssetDatabase.GetAssetPath(sequence);
            var step = CreateInstance<Step>();
            var index = list.serializedProperty.arraySize;
            step.name = $"{sequence.name}-{index}_step";
            //list.serializedProperty.InsertArrayElementAtIndex(index);
            if (sequence.Steps == null)
            {
                sequence.Init();
            }
            sequence.Steps.Add(step);

            AssetDatabase.AddObjectToAsset(step, $"{path}");
            AssetDatabase.ImportAsset(path);
            serializedObject.ApplyModifiedProperties();
            //list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = step;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
            _stepList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Update Quest object"))
            {
                var obj = new GameObject(sequence.name);
                obj.AddComponent<SequenceBehaviour>().sequence = sequence;
                foreach (var step in sequence.Steps)
                {
                    var stepObj = new GameObject(step.name);
                    var s = new StepEventListener.StepWithEvents();
                    s.step = step;
                    stepObj.AddComponent<StepEventListener>().AddStep( step);
                    stepObj.transform.parent = obj.transform;
                }
            }

            if (Application.isPlaying)
            {
                var text = sequence.Started ? "Restart Quest" : "Start Quest";
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(text)) sequence.Begin();

                if (sequence.Started)
                    if (GUILayout.Button("next"))
                        sequence.CurrentStep.CompleteStep();
                GUILayout.EndHorizontal();

                GUI.enabled = false;
                EditorGUILayout.LabelField(sequence.CurrentStep.ToString());
                GUI.enabled = true;
            }
        }
    }
}