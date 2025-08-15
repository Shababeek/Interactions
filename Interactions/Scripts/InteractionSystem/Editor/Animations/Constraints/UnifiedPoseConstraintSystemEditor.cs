using Shababeek.Interactions.Animations.Constraints;
using Shababeek.Interactions.Animations;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Editors
{
    [CustomEditor(typeof(PoseConstrainter))]
    public class PoseConstrainerEditor : Editor
    {
        private PoseConstrainter _constrainter;
        private SerializedProperty _constraintTypeProperty;
        private SerializedProperty _useSmoothTransitionsProperty;
        private SerializedProperty _transitionSpeedProperty;
        private SerializedProperty _leftPoseConstraintsProperty;
        private SerializedProperty _rightPoseConstraintsProperty;
        private SerializedProperty _leftHandPositioningProperty;
        private SerializedProperty _rightHandPositioningProperty;

        private Config _config;
        private HandData _handdata;
        private HandPoseController _currentHand;
        private HandPoseController _leftHandPrefab, _rightHandPrefab;
        private HandIdentifier _selectedHand = HandIdentifier.None;
        private float _t = 0;


        private void OnEnable()
        {
            _constrainter = (PoseConstrainter)target;

            _constraintTypeProperty = serializedObject.FindProperty("constraintType");
            _useSmoothTransitionsProperty = serializedObject.FindProperty("useSmoothTransitions");
            _transitionSpeedProperty = serializedObject.FindProperty("transitionSpeed");
            _leftPoseConstraintsProperty = serializedObject.FindProperty("leftPoseConstraints");
            _rightPoseConstraintsProperty = serializedObject.FindProperty("rightPoseConstraints");
            _leftHandPositioningProperty = serializedObject.FindProperty("leftHandPositioning");
            _rightHandPositioningProperty = serializedObject.FindProperty("rightHandPositioning");
            InitializeVariables();
            EditorApplication.update += OnUpdate;
        }

        private void OnDisable()
        {
            _selectedHand = HandIdentifier.None;
            Tools.hidden = false;
            EditorApplication.update -= OnUpdate;
            DeselectHands();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawConstraintType();
            EditorGUILayout.Space();
            if (_constrainter.ConstraintType == HandConstrainType.Constrained)
            {
                DrawHandSelection();
                if (_selectedHand != HandIdentifier.None)
                {
                    DrawHandPositionEditor();
                    EditorGUILayout.Space();
                    DrawHandConstraints();
                }
            }

            if (GUI.changed)
            {
                UpdateHandTransformFromVectors();
            }

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawConstraintType()
        {
            EditorGUILayout.PropertyField(_constraintTypeProperty, new GUIContent("Hand Constraint Type"));
            
            // Smooth transition settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useSmoothTransitionsProperty, new GUIContent("Use Smooth Transitions"));
            
            if (_useSmoothTransitionsProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_transitionSpeedProperty, new GUIContent("Transition Speed"));
                
                // Validate transition speed
                if (_transitionSpeedProperty.floatValue <= 0)
                {
                    EditorGUILayout.HelpBox("Transition speed must be greater than 0 for smooth transitions to work.", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox(
                    "When enabled, hands will smoothly transition to their target positions instead of instantly appearing.\n\n" +
                    "Smooth transitions are useful for:\n" +
                    "• More natural hand movements\n" +
                    "• Reducing jarring visual changes\n" +
                    "• Better user experience in VR\n\n" +
                    "Higher transition speeds make the movement faster.",
                    MessageType.Info);
            }
            
            switch ((HandConstrainType)_constraintTypeProperty.enumValueIndex)
            {
                case HandConstrainType.HideHand:
                    EditorGUILayout.HelpBox("Hands will be hidden during interaction.", MessageType.Info);
                    break;

                case HandConstrainType.FreeHand:
                    EditorGUILayout.HelpBox("Hands will move freely without constraints.", MessageType.Info);
                    break;

                case HandConstrainType.Constrained:
                    EditorGUILayout.HelpBox(
                        "Hands will be constrained with pose click one of the buttons below toedit a hand",
                        MessageType.Info);
                    break;
            }
        }

        private void DrawHandSelection()
        {
            EditorGUILayout.LabelField("Interactive Hand Selection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            var style = new GUIStyle(GUI.skin.button);
            var rightHandClicked =
                GUILayout.Toggle(_selectedHand == HandIdentifier.Right, "Edit Right Hand constraints", style) ^
                _selectedHand == HandIdentifier.Right;
            var leftHandClicked =
                GUILayout.Toggle(_selectedHand == HandIdentifier.Left, "Edit Left Hand constraints", style) ^
                _selectedHand == HandIdentifier.Left;

            if (rightHandClicked)
            {
                SelectHand(_selectedHand == HandIdentifier.Right ? HandIdentifier.None : HandIdentifier.Right);
            }

            if (leftHandClicked)
            {
                SelectHand(_selectedHand == HandIdentifier.Left ? HandIdentifier.None : HandIdentifier.Left);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHandPositionEditor()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                $"Editing {_selectedHand} hand constraints. Use the scene view to move the hand transform.",
                MessageType.Info);
            EditorGUILayout.LabelField("Hand Positioning", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if (_selectedHand == HandIdentifier.Left)
            {
                EditorGUILayout.PropertyField(_leftHandPositioningProperty, new GUIContent("Positioning"));
            }
            else if (_selectedHand == HandIdentifier.Right)
            {
                EditorGUILayout.PropertyField(_rightHandPositioningProperty, new GUIContent("Positioning"));
            }

            EditorGUI.indentLevel--;
        }

        private void DrawHandConstraints()
        {
            EditorGUILayout.LabelField("Pose Constraints", EditorStyles.boldLabel);
            
            // Add copy from other hand button
            EditorGUILayout.BeginHorizontal();
            var otherHand = _selectedHand == HandIdentifier.Left ? "Right" : "Left";
            if (GUILayout.Button(new GUIContent($"Copy from {otherHand} Hand", "Copies finger constraints and pose data from the other hand, with rotation values flipped for proper mirroring")))
            {
                CopyFromOtherHand();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox(
                $"This will copy all finger constraints and pose data from the {otherHand.ToLower()} hand to the {_selectedHand} hand.\n" +
                "Rotation values are automatically flipped to create a proper mirror effect.",
                MessageType.Info);
            EditorGUILayout.Space();
            
            var poseConstraintsProperty = _selectedHand == HandIdentifier.Left
                ? _leftPoseConstraintsProperty
                : _rightPoseConstraintsProperty;
            DrawPoseSelection(poseConstraintsProperty, _config.HandData);
            var isStaticPose = IsStaticPose();
            if (isStaticPose)
            {
                EditorGUILayout.HelpBox("Static pose detected - all fingers are locked. Finger constraints are hidden.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Finger Constraints", EditorStyles.boldLabel);
                DrawFingerConstraint(poseConstraintsProperty, "thumbFingerLimits", "Thumb");
                DrawFingerConstraint(poseConstraintsProperty, "indexFingerLimits", "Index");
                DrawFingerConstraint(poseConstraintsProperty, "middleFingerLimits", "Middle");
                DrawFingerConstraint(poseConstraintsProperty, "ringFingerLimits", "Ring");
                DrawFingerConstraint(poseConstraintsProperty, "pinkyFingerLimits", "Pinky");
            }
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawPoseSelection(SerializedProperty poseConstraintsProperty, HandData handData)
        {
            var targetPoseIndexProperty = poseConstraintsProperty.FindPropertyRelative("targetPoseIndex");

            var availablePoses = GetAvailablePoses(handData);

            if (availablePoses.Length == 0)
            {
                EditorGUILayout.HelpBox("No poses found in HandData.", MessageType.Warning);
                return;
            }

            var poseNames = new string[availablePoses.Length];
            for (int i = 0; i < availablePoses.Length; i++)
            {
                poseNames[i] = availablePoses[i].Name;
            }

            // Get current selection
            var currentIndex = targetPoseIndexProperty.intValue;
            if (currentIndex >= availablePoses.Length || currentIndex < 0)
            {
                currentIndex = 0;
            }

            var newIndex = EditorGUILayout.Popup($"{_selectedHand} Hand Target Pose", currentIndex, poseNames);

            // Update property if selection changed
            if (newIndex != currentIndex)
            {
                targetPoseIndexProperty.intValue = newIndex;
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets available poses from HandData.
        /// </summary>
        /// <param name="handData">The HandData asset.</param>
        /// <returns>Array of available poses.</returns>
        private PoseData[] GetAvailablePoses(HandData handData)
        {
            if (handData == null) return new PoseData[0];

            // Try to get poses from HandData
            // This assumes HandData has a poses array or similar
            // You may need to adjust this based on your actual HandData structure
            return handData.Poses ?? new PoseData[0];
        }
        
        private bool IsStaticPose()
        {
            var constraint = _selectedHand == HandIdentifier.Left
                ? _constrainter.LeftPoseConstrains
                : _constrainter.RightPoseConstrains;
            var targetPoseIndex = constraint.targetPoseIndex;
            if (targetPoseIndex <= 0 || targetPoseIndex >= _handdata.Poses.Length) return false;
            var selectedPose = _handdata.Poses[targetPoseIndex];
            return (selectedPose.Type == PoseData.PoseType.Static);
        }


        private void DrawFingerConstraint(SerializedProperty poseConstraintsProperty, string fingerPropertyName,
            string fingerDisplayName)
        {
            var fingerProperty = poseConstraintsProperty.FindPropertyRelative(fingerPropertyName);
            EditorGUILayout.PropertyField(fingerProperty, new GUIContent(fingerDisplayName));
        }

        private void InitializeVariables()
        {
            _selectedHand = HandIdentifier.None;

            _config = FindAnyObjectByType<CameraRig>()?.Config;
            if (!_config)
            {
                var configAsset = AssetDatabase.FindAssets("t:Shababeek.Interactions.Core.Config");
                if (configAsset.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(configAsset[0]);
                    _config = AssetDatabase.LoadAssetAtPath<Config>(path);
                }
            }

            if (_config?.HandData != null)
            {
                _handdata=_config.HandData;
                _leftHandPrefab = _handdata.LeftHandPrefab;
                _rightHandPrefab = _handdata.RightHandPrefab;
            }
            else
            {
                Debug.LogWarning("No HandData found in Config. Interactive hand preview will not work.");
            }
        }


        private HandPoseController CreateHandInPivot(Transform pivot, HandPoseController handPrefab)
        {
            if (handPrefab == null) return null;

            var initializedHand = Instantiate(handPrefab);
            var handObject = initializedHand.gameObject;
            handObject.transform.localScale = Vector3.one;
            handObject.transform.parent = pivot;
            handObject.transform.localPosition = Vector3.zero;
            handObject.transform.localRotation = Quaternion.identity;
            initializedHand.Initialize();
            return initializedHand;
        }


        private void SelectHand(HandIdentifier hand)
        {
            if (_selectedHand == hand) return;

            DeselectHands();
            _selectedHand = hand;

            if (_selectedHand != HandIdentifier.None)
            {
                // Create the hand as a child of the constraint system object
                var handPrefab = _selectedHand == HandIdentifier.Left ? _leftHandPrefab : _rightHandPrefab;
                _currentHand = CreateHandInPivot(_constrainter.transform, handPrefab);

                // Set the hand's local position and rotation based on the vector values
                UpdateHandTransformFromVectors();
            }
        }

        private void UpdateHandTransformFromVectors()
        {
            if (_currentHand == null || _selectedHand == HandIdentifier.None) return;

            var positioningProperty = _selectedHand == HandIdentifier.Left
                ? _leftHandPositioningProperty
                : _rightHandPositioningProperty;

            var positionOffset = positioningProperty.FindPropertyRelative("positionOffset").vector3Value;
            var rotationOffset = positioningProperty.FindPropertyRelative("rotationOffset").vector3Value;

            _currentHand.transform.localPosition = positionOffset;
            _currentHand.transform.localRotation = Quaternion.Euler(rotationOffset);
        }
        
        private void UpdateVectorsFromTransform()
        {
            if (_currentHand == null || _selectedHand == HandIdentifier.None) return;

            var positioningProperty = _selectedHand == HandIdentifier.Left
                ? _leftHandPositioningProperty
                : _rightHandPositioningProperty;

            var positionOffset = positioningProperty.FindPropertyRelative("positionOffset");
            var rotationOffset = positioningProperty.FindPropertyRelative("rotationOffset");

            positionOffset.vector3Value = _currentHand.transform.localPosition;
            rotationOffset.vector3Value = _currentHand.transform.localEulerAngles;

            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Copies finger constraints and pose data from the other hand, with rotation values flipped.
        /// </summary>
        private void CopyFromOtherHand()
        {
            if (_selectedHand == HandIdentifier.None) return;
            
            // Determine which hand to copy from
            var isLeftHandSelected = _selectedHand == HandIdentifier.Left;
            var sourcePoseConstraintsProperty = isLeftHandSelected ? _rightPoseConstraintsProperty : _leftPoseConstraintsProperty;
            var targetPoseConstraintsProperty = isLeftHandSelected ? _leftPoseConstraintsProperty : _rightPoseConstraintsProperty;
            var sourceHandPositioningProperty = isLeftHandSelected ? _rightHandPositioningProperty : _leftHandPositioningProperty;
            var targetHandPositioningProperty = isLeftHandSelected ? _leftHandPositioningProperty : _rightHandPositioningProperty;
            
            // Copy pose constraints
            CopyPoseConstraints(sourcePoseConstraintsProperty, targetPoseConstraintsProperty);
            
            // Copy hand positioning with flipped rotation
            CopyHandPositioning(sourceHandPositioningProperty, targetHandPositioningProperty);
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
            
            // Update the hand preview if it exists
            if (_currentHand != null)
            {
                UpdateHandTransformFromVectors();
            }
            
            Debug.Log($"Copied {(_selectedHand == HandIdentifier.Left ? "right" : "left")} hand data to {_selectedHand} hand with flipped rotation values.");
        }
        
        /// <summary>
        /// Copies pose constraints from source to target.
        /// </summary>
        private void CopyPoseConstraints(SerializedProperty source, SerializedProperty target)
        {
            // Copy target pose index
            var sourceTargetPoseIndex = source.FindPropertyRelative("targetPoseIndex");
            var targetTargetPoseIndex = target.FindPropertyRelative("targetPoseIndex");
            targetTargetPoseIndex.intValue = sourceTargetPoseIndex.intValue;
            
            // Copy finger constraints
            CopyFingerConstraints(source.FindPropertyRelative("thumbFingerLimits"), target.FindPropertyRelative("thumbFingerLimits"));
            CopyFingerConstraints(source.FindPropertyRelative("indexFingerLimits"), target.FindPropertyRelative("indexFingerLimits"));
            CopyFingerConstraints(source.FindPropertyRelative("middleFingerLimits"), target.FindPropertyRelative("middleFingerLimits"));
            CopyFingerConstraints(source.FindPropertyRelative("ringFingerLimits"), target.FindPropertyRelative("ringFingerLimits"));
            CopyFingerConstraints(source.FindPropertyRelative("pinkyFingerLimits"), target.FindPropertyRelative("pinkyFingerLimits"));
        }
        
        /// <summary>
        /// Copies finger constraints from source to target.
        /// </summary>
        private void CopyFingerConstraints(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null) return;
            
            // Copy all properties of the finger constraints
            var iterator = source.Copy();
            var enterChildren = iterator.Next(true);
            if (enterChildren)
            {
                do
                {
                    var targetProperty = target.FindPropertyRelative(iterator.name);
                    if (targetProperty != null)
                    {
                        // Copy the value based on the property type
                        switch (iterator.propertyType)
                        {
                            case SerializedPropertyType.Integer:
                                targetProperty.intValue = iterator.intValue;
                                break;
                            case SerializedPropertyType.Boolean:
                                targetProperty.boolValue = iterator.boolValue;
                                break;
                            case SerializedPropertyType.Float:
                                targetProperty.floatValue = iterator.floatValue;
                                break;
                            case SerializedPropertyType.String:
                                targetProperty.stringValue = iterator.stringValue;
                                break;
                            case SerializedPropertyType.Vector3:
                                targetProperty.vector3Value = iterator.vector3Value;
                                break;
                            case SerializedPropertyType.Quaternion:
                                targetProperty.quaternionValue = iterator.quaternionValue;
                                break;
                            case SerializedPropertyType.Enum:
                                targetProperty.enumValueIndex = iterator.enumValueIndex;
                                break;
                        }
                    }
                } while (iterator.Next(false));
            }
        }
        
        /// <summary>
        /// Copies hand positioning from source to target with flipped rotation values.
        /// </summary>
        private void CopyHandPositioning(SerializedProperty source, SerializedProperty target)
        {
            // Copy position offset (no flipping needed)
            var sourcePosition = source.FindPropertyRelative("positionOffset");
            var targetPosition = target.FindPropertyRelative("positionOffset");
            targetPosition.vector3Value = sourcePosition.vector3Value;
            
            // Copy rotation offset with flipping
            var sourceRotation = source.FindPropertyRelative("rotationOffset");
            var targetRotation = target.FindPropertyRelative("rotationOffset");
            
            // Flip rotation values for the other hand
            var flippedRotation = FlipRotationForOtherHand(sourceRotation.vector3Value);
            targetRotation.vector3Value = flippedRotation;
        }
        
        /// <summary>
        /// Flips rotation values for the other hand (mirrors the rotation).
        /// </summary>
        private Vector3 FlipRotationForOtherHand(Vector3 rotation)
        {
            // For left/right hand mirroring, we typically flip the Y and Z rotations
            // This creates a mirror effect where the hands look like they're doing the same pose
            return new Vector3(
                rotation.x,           // Keep X rotation (forward/backward tilt)
                -rotation.y,          // Flip Y rotation (left/right tilt)
                -rotation.z           // Flip Z rotation (roll)
            );
        }
        
        private void DeselectHands()
        {
            if (_currentHand)
            {
                DestroyImmediate(_currentHand.gameObject);
                _currentHand = null;
            }
        }
        
        private void OnUpdate()
        {
            SetPose();
        }
        
        private void SetPose()
        {
            if (_selectedHand == HandIdentifier.None || _currentHand == null) return;

            this._t += 0.01f;
            var finger = Mathf.PingPong(this._t, 1);

            var handConstraints = _selectedHand == HandIdentifier.Left
                ? _constrainter.LeftPoseConstrains
                : _constrainter.RightPoseConstrains;

            for (var i = 0; i < 5; i++)
            {
                _currentHand[i] = handConstraints[i].constraints.GetConstrainedValue(finger);
            }

            _currentHand.Pose = handConstraints[0].pose;
            _currentHand.UpdateGraphVariables();
        }
        
        protected virtual void OnSceneGUI()
        {
            if (!_currentHand || _selectedHand == HandIdentifier.None)
            {
                Tools.hidden = false;
                return;
            }
            Tools.hidden = true;
            
            // Use local coordinates for the transform handle since the hand is parented
            var localPosition = _currentHand.transform.localPosition;
            var localRotation = _currentHand.transform.localEulerAngles;
            
            // Convert to world space for the handle
            var worldPosition = _currentHand.transform.position;
            var worldRotation = _currentHand.transform.rotation;
            
            Handles.TransformHandle(ref worldPosition, ref worldRotation);
            
            // Convert back to local space and update
            _currentHand.transform.position = worldPosition;
            _currentHand.transform.rotation = worldRotation;
            
            UpdateVectorsFromTransform();
        }
    }
}