// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using Shababeek.ScriptableSystem;
//
// namespace Shababeek.Sequencing
// {
//     [CustomEditor(typeof(VariableAction))]
//     public class VariableActionEditor : Editor
//     {
//         private SerializedProperty variableReferenceProp;
//         private SerializedProperty operationProp;
//         private SerializedProperty stringValueProp;
//         private SerializedProperty floatValueProp;
//         private SerializedProperty intValueProp;
//         private SerializedProperty boolValueProp;
//         private SerializedProperty vector3ValueProp;
//         private SerializedProperty quaternionValueProp;
//         private SerializedProperty colorValueProp;
//         private SerializedProperty comparisonTypeProp;
//
//         private void OnEnable()
//         {
//             variableReferenceProp = serializedObject.FindProperty("variableReference");
//             operationProp = serializedObject.FindProperty("operation");
//             stringValueProp = serializedObject.FindProperty("stringValue");
//             floatValueProp = serializedObject.FindProperty("floatValue");
//             intValueProp = serializedObject.FindProperty("intValue");
//             boolValueProp = serializedObject.FindProperty("boolValue");
//             vector3ValueProp = serializedObject.FindProperty("vector3Value");
//             quaternionValueProp = serializedObject.FindProperty("quaternionValue");
//             colorValueProp = serializedObject.FindProperty("colorValue");
//             comparisonTypeProp = serializedObject.FindProperty("comparisonType");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();
//
//             EditorGUILayout.HelpBox(
//                 "VariableAction can set, check, increment, or decrement ScriptableVariable values.",
//                 MessageType.Info
//             );
//
//             // Variable Reference
//             EditorGUILayout.PropertyField(variableReferenceProp, new GUIContent("Variable Reference"));
//
//             if (variableReferenceProp.objectReferenceValue != null)
//             {
//                 EditorGUILayout.Space();
//                 
//                 // Operation Type
//                 EditorGUILayout.PropertyField(operationProp, new GUIContent("Operation"));
//                 
//                 var operation = (VariableAction.VariableOperation)operationProp.enumValueIndex;
//                 var variableType = GetVariableType(variableReferenceProp.objectReferenceValue);
//                 
//                 if (operation == VariableAction.VariableOperation.Check)
//                 {
//                     EditorGUILayout.Space();
//                     EditorGUILayout.LabelField("Condition Settings", EditorStyles.boldLabel);
//                     
//                     // Comparison Type
//                     EditorGUILayout.PropertyField(comparisonTypeProp, new GUIContent("Comparison Type"));
//                     
//                     EditorGUILayout.Space();
//                     
//                     // Value field based on variable type
//                     ShowValueFieldForType(variableType);
//                 }
//                 else if (operation == VariableAction.VariableOperation.Set)
//                 {
//                     EditorGUILayout.Space();
//                     EditorGUILayout.LabelField("Set Value", EditorStyles.boldLabel);
//                     
//                     // Value field based on variable type
//                     ShowValueFieldForType(variableType);
//                 }
//                 else if (operation == VariableAction.VariableOperation.Increment || 
//                          operation == VariableAction.VariableOperation.Decrement)
//                 {
//                     EditorGUILayout.Space();
//                     EditorGUILayout.LabelField($"{operation} Operation", EditorStyles.boldLabel);
//                     EditorGUILayout.HelpBox($"Will {operation.ToString().ToLower()} the variable value by 1.", MessageType.Info);
//                 }
//                 
//                 // Show current value if available
//                 ShowCurrentValue(variableType);
//             }
//             else
//             {
//                 EditorGUILayout.HelpBox("Assign a ScriptableVariable to configure the action.", MessageType.Warning);
//             }
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         private System.Type GetVariableType(Object variableObject)
//         {
//             if (variableObject is FloatVariable) return typeof(float);
//             if (variableObject is IntVariable) return typeof(int);
//             if (variableObject is TextVariable) return typeof(string);
//             if (variableObject is BoolVariable) return typeof(bool);
//             if (variableObject is Vector3Variable) return typeof(Vector3);
//             if (variableObject is QuaternionVariable) return typeof(Quaternion);
//             if (variableObject is ColorVariable) return typeof(Color);
//             if (variableObject is GameObjectVariable) return typeof(GameObject);
//             if (variableObject is TransformVariable) return typeof(Transform);
//             if (variableObject is AudioClipVariable) return typeof(AudioClip);
//             if (variableObject is MaterialVariable) return typeof(Material);
//             if (variableObject is EnumVariable) return typeof(int);
//             return null;
//         }
//
//         private void ShowValueFieldForType(System.Type variableType)
//         {
//             if (variableType == typeof(float))
//             {
//                 EditorGUILayout.PropertyField(floatValueProp, new GUIContent("Float Value"));
//             }
//             else if (variableType == typeof(int))
//             {
//                 EditorGUILayout.PropertyField(intValueProp, new GUIContent("Int Value"));
//             }
//             else if (variableType == typeof(string))
//             {
//                 EditorGUILayout.PropertyField(stringValueProp, new GUIContent("String Value"));
//             }
//             else if (variableType == typeof(bool))
//             {
//                 EditorGUILayout.PropertyField(boolValueProp, new GUIContent("Bool Value"));
//             }
//             else if (variableType == typeof(Vector3))
//             {
//                 EditorGUILayout.PropertyField(vector3ValueProp, new GUIContent("Vector3 Value"));
//             }
//             else if (variableType == typeof(Quaternion))
//             {
//                 EditorGUILayout.PropertyField(quaternionValueProp, new GUIContent("Quaternion Value"));
//             }
//             else if (variableType == typeof(Color))
//             {
//                 EditorGUILayout.PropertyField(colorValueProp, new GUIContent("Color Value"));
//             }
//             else
//             {
//                 EditorGUILayout.HelpBox($"Variable type {variableType?.Name} is not supported for this operation.", MessageType.Warning);
//             }
//         }
//
//         private void ShowCurrentValue(System.Type variableType)
//         {
//             if (variableReferenceProp.objectReferenceValue == null) return;
//
//             EditorGUILayout.Space();
//             EditorGUILayout.LabelField("Current Variable Value", EditorStyles.boldLabel);
//             
//             EditorGUI.BeginDisabledGroup(true);
//             
//             if (variableType == typeof(float))
//             {
//                 var floatVar = variableReferenceProp.objectReferenceValue as FloatVariable;
//                 EditorGUILayout.FloatField("Current Float", floatVar?.Value ?? 0f);
//             }
//             else if (variableType == typeof(int))
//             {
//                 var intVar = variableReferenceProp.objectReferenceValue as IntVariable;
//                 EditorGUILayout.IntField("Current Int", intVar?.Value ?? 0);
//             }
//             else if (variableType == typeof(string))
//             {
//                 var textVar = variableReferenceProp.objectReferenceValue as TextVariable;
//                 EditorGUILayout.TextField("Current String", textVar?.Value ?? "");
//             }
//             else if (variableType == typeof(bool))
//             {
//                 var boolVar = variableReferenceProp.objectReferenceValue as BoolVariable;
//                 EditorGUILayout.Toggle("Current Bool", boolVar?.Value ?? false);
//             }
//             else if (variableType == typeof(Vector3))
//             {
//                 var vector3Var = variableReferenceProp.objectReferenceValue as Vector3Variable;
//                 EditorGUILayout.Vector3Field("Current Vector3", vector3Var?.Value ?? Vector3.zero);
//             }
//             else if (variableType == typeof(Quaternion))
//             {
//                 var quaternionVar = variableReferenceProp.objectReferenceValue as QuaternionVariable;
//                 var euler = quaternionVar?.Value.eulerAngles ?? Vector3.zero;
//                 EditorGUILayout.Vector3Field("Current Rotation (Euler)", euler);
//             }
//             else if (variableType == typeof(Color))
//             {
//                 var colorVar = variableReferenceProp.objectReferenceValue as ColorVariable;
//                 EditorGUILayout.ColorField("Current Color", colorVar?.Value ?? Color.white);
//             }
//             else if (variableType == typeof(GameObject))
//             {
//                 var gameObjectVar = variableReferenceProp.objectReferenceValue as GameObjectVariable;
//                 EditorGUILayout.ObjectField("Current GameObject", gameObjectVar?.Value, typeof(GameObject), true);
//             }
//             else if (variableType == typeof(Transform))
//             {
//                 var transformVar = variableReferenceProp.objectReferenceValue as TransformVariable;
//                 EditorGUILayout.ObjectField("Current Transform", transformVar?.Value, typeof(Transform), true);
//             }
//             else if (variableType == typeof(AudioClip))
//             {
//                 var audioClipVar = variableReferenceProp.objectReferenceValue as AudioClipVariable;
//                 EditorGUILayout.ObjectField("Current AudioClip", audioClipVar?.Value, typeof(AudioClip), false);
//             }
//             else if (variableType == typeof(Material))
//             {
//                 var materialVar = variableReferenceProp.objectReferenceValue as MaterialVariable;
//                 EditorGUILayout.ObjectField("Current Material", materialVar?.Value, typeof(Material), false);
//             }
//             else if (variableType == typeof(int)) // EnumVariable
//             {
//                 var enumVar = variableReferenceProp.objectReferenceValue as EnumVariable;
//                 if (enumVar != null)
//                 {
//                     EditorGUILayout.IntField("Current Enum Value", enumVar.Value);
//                     EditorGUILayout.TextField("Current Enum Name", enumVar.GetEnumName());
//                 }
//             }
//             
//             EditorGUI.EndDisabledGroup();
//         }
//     }
// } 