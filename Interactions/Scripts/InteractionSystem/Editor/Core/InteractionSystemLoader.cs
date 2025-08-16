using System.Collections.Generic;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    /// <summary>
    /// It is used to initialize the input manager, layers, physics, and config file.
    /// It is also used to add the packages to the project.
    /// </summary>
    [ScriptedImporter(1, "shababeek")]
    public class InteractionSystemLoader : ScriptedImporter
    {
        private const string Shababeek_LAYERS_INITIALIZED = "Shababeek_Layers_Initialized";
        private const string PLAYER_LAYER_NAME = "Shababeek_PlayerLayer";
        private const string LEFT_INTERACTOR_LAYER_NAME = "Shababeek_LeftInteractor";
        private const string RIGHT_INTERACTOR_LAYER_NAME = "Shababeek_RightInteractor";
        private const string INTERACTABLE_LAYER_NAME = "Shababeek_Interactable";


        /// <summary>
        /// this list is copied from unity SeedXR Binding
        /// 
        /// </summary>
        private List<(string name, string descriptiveName, float dead, int axis, int type, string positiveButton, float gravity, float sensitivity,string altPositiveButton)> axisList =
            new()
            {
                #region LeftHand

                //######################################################################################################################################
                // Left Hand
                //######################################################################################################################################  
                // Axis Data
                new()
                {
                    name = "Shababeek_Left_Trigger",
                    descriptiveName = "Device trigger axis",
                    axis = 9,
                    type = 2,
                    sensitivity = 1,
                },
                new()
                {
                    name = "Shababeek_Left_Grip",
                    descriptiveName = "Device grip axis",
                    axis = 11,
                    type = 2,
                    sensitivity = 1,
                },
                new()
                {
                    name = "Shababeek_Left_PrimaryButton",
                    descriptiveName = "Device primary button",
                    positiveButton = "joystick button 2",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_SecondaryButton",
                    descriptiveName = "Device secondary button",
                    positiveButton = "joystick button 3",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_GripButton",
                    descriptiveName = "Device grip button",
                    positiveButton = "joystick button 4",
                    gravity = 0.0f,
                    dead = 0.0f,
                    sensitivity = 0.1f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_TriggerButton",
                    descriptiveName = "Device trigger button",
                    positiveButton = "joystick button 14",
                    gravity = 0.0f,
                    dead = 0.0f,
                    sensitivity = 0.1f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_Grip_DebugKey",
                    descriptiveName = "Left Grip Debug Key (Z)",
                    positiveButton = "z",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_Index_DebugKey",
                    descriptiveName = "Left Index Debug Key (X)",
                    positiveButton = "x",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Left_Primary_DebugKey",
                    descriptiveName = "Left Primary Debug Key (C)",
                    positiveButton = "c",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                #endregion

                #region RightHand

                //######################################################################################################################################
                // Right Hand
                //######################################################################################################################################
                new()
                {
                    name = "Shababeek_Right_Trigger",
                    descriptiveName = "Device trigger axis",
                    axis = 10,
                    type = 2,
                    sensitivity = 1
                },
                new()
                {
                    name = "Shababeek_Right_Grip",
                    descriptiveName = "Device grip axis",
                    axis = 12,
                    type = 2,
                    sensitivity = 1
                },
                new()
                {
                    name = "Shababeek_Right_PrimaryButton",
                    descriptiveName = "Device primary button",
                    positiveButton = "joystick button 0",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_SecondaryButton",
                    descriptiveName = "Device secondary button",
                    positiveButton = "joystick button 1",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_GripButton",
                    descriptiveName = "Device grip button",
                    positiveButton = "joystick button 5",
                    gravity = 0.0f,
                    dead = 0.0f,
                    sensitivity = 0.1f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_TriggerButton",
                    descriptiveName = "Device trigger button",
                    positiveButton = "joystick button 15",
                    gravity = 0.0f,
                    dead = 0.0f,
                    sensitivity = 0.1f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_Grip_DebugKey",
                    descriptiveName = "Right Grip Debug Key (m)",
                    positiveButton = "m",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_Index_DebugKey",
                    descriptiveName = "Right Index Debug Key (n)",
                    positiveButton = "n",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                new()
                {
                    name = "Shababeek_Right_Primary_DebugKey",
                    descriptiveName = "Right Primary Debug Key (b)",
                    positiveButton = "b",
                    gravity = 1000.0f,
                    sensitivity = 1000.0f,
                    type = 0,
                },
                #endregion


            };

        public override void OnImportAsset(AssetImportContext ctx)
        {
            InitializeInputManager();
            InitializeLayers();
            InitializePhysics();
            InitializeConfigFile();
        }


        private void InitializePhysics()
        {
            var leftLayer = LayerMask.NameToLayer(LEFT_INTERACTOR_LAYER_NAME);
            var rightLayer = LayerMask.NameToLayer(RIGHT_INTERACTOR_LAYER_NAME);
            var playerLayer = LayerMask.NameToLayer(PLAYER_LAYER_NAME);
            Physics.IgnoreLayerCollision(leftLayer, leftLayer);
            Physics.IgnoreLayerCollision(rightLayer, rightLayer);
            Physics.IgnoreLayerCollision(rightLayer, leftLayer);
            Physics.IgnoreLayerCollision(playerLayer, leftLayer);
            Physics.IgnoreLayerCollision(playerLayer,rightLayer);
            Physics.IgnoreLayerCollision(playerLayer, playerLayer);

        }

        private void InitializeConfigFile()
        {
            var configFilesGUIDs = AssetDatabase.FindAssets("t:Shababeek.Interactions.Runtime.Config");
            foreach (var guid in configFilesGUIDs)
            {
                var configFilePath = AssetDatabase.GUIDToAssetPath(guid);
                if (configFilePath == null) continue;
                var configFile = AssetDatabase.LoadAssetAtPath<Config>(configFilePath);
                var serializedConfigFile = new SerializedObject(configFile);
                serializedConfigFile.FindProperty("leftHandLayer").intValue = 1 << LayerMask.NameToLayer(LEFT_INTERACTOR_LAYER_NAME);
                serializedConfigFile.FindProperty("rightHandLayer").intValue = 1 << LayerMask.NameToLayer(RIGHT_INTERACTOR_LAYER_NAME);
                serializedConfigFile.FindProperty("interactableLayer").intValue = 1 << LayerMask.NameToLayer(INTERACTABLE_LAYER_NAME);
                serializedConfigFile.FindProperty("playerLayer").intValue = 1 << LayerMask.NameToLayer(PLAYER_LAYER_NAME);
                serializedConfigFile.ApplyModifiedProperties();
            }
        }

        private void InitializeLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            int index = 6;
            int count = 0;
            string[] layersName = new[]
            {
                LEFT_INTERACTOR_LAYER_NAME,
                RIGHT_INTERACTOR_LAYER_NAME,
                INTERACTABLE_LAYER_NAME,
                PLAYER_LAYER_NAME
            };
            while (index < 32 && count < layersName.Length)
            {
                index++;
                if (layers.GetArrayElementAtIndex(index).stringValue == layersName[count])
                {
                    count++;
                    continue;
                }
                if (layers.GetArrayElementAtIndex(index).stringValue?.Length > 0) continue;
             
                layers.GetArrayElementAtIndex(index).stringValue = layersName[count];
                count++;
            }

            tagManager.ApplyModifiedProperties();
            EditorPrefs.SetBool(Shababeek_LAYERS_INITIALIZED, true);
        }
        
        private void InitializeInputManager()
        {
            var inputManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var serializedObject = new SerializedObject(inputManagerAsset);
            var axis = serializedObject.FindProperty("m_Axes");
            if (axis is not { isArray: true }) return;
            for (int i = 18; i < axis.arraySize; i++)
            {
                axis.DeleteArrayElementAtIndex(i);
            }

            axis.arraySize = 18;
            var count = axis.arraySize;

            //RemoveDefinedAxis(count, axis);
            AddNewAxes(axis, count);
            serializedObject.ApplyModifiedProperties();
        }

        private void AddNewAxes(SerializedProperty axis, int count)
        {
            for (var i = 0; i < axisList.Count; i++)
            {
                var axe = axisList[i];
                axis.InsertArrayElementAtIndex(count + i);
                var property = axis.GetArrayElementAtIndex(count + i);
                SetAxe(property, axe);
            }
        }

        private void RemoveDefinedAxis(int count, SerializedProperty axis)
        {
            for (int i = 0; i < count; i++)
            {
                InitializeAxe(axis, i);
            }
        }

        private void InitializeAxe(SerializedProperty axis, int i)
        {
            var name = axis.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name")?.stringValue;
            if (name != null)
            {
                FindAndRemoveItem(name);
            }
        }

        private static void SetAxe(SerializedProperty property,
            (string name, string descriptiveName, float dead, int axis, int type, string positiveButton, float gravity, float sensitivity,string altPostive) axe)
        {
            property.FindPropertyRelative("m_Name").stringValue = axe.name;
            property.FindPropertyRelative("descriptiveName").stringValue = axe.descriptiveName;
            property.FindPropertyRelative("positiveButton").stringValue = axe.positiveButton;
            property.FindPropertyRelative("gravity").floatValue = axe.gravity;
            property.FindPropertyRelative("dead").floatValue = axe.dead;
            property.FindPropertyRelative("sensitivity").floatValue = axe.sensitivity;
            property.FindPropertyRelative("type").intValue = axe.type;
            property.FindPropertyRelative("axis").intValue = axe.axis-1;
            property.FindPropertyRelative("altPositiveButton").stringValue = axe.altPostive;
        }

        private void FindAndRemoveItem(string name)
        {
            var index = axisList.FindIndex(tuple => tuple.name == name);
            if (index >= 0)
                axisList.RemoveAt(index);
        }
    }
}