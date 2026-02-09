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
        private SerializedProperty _grabPointsProperty;

        private Config _config;
        private HandData _handData;
        private HandPoseController _currentHand;
        private Transform _handScaleWrapper;
        private HandPoseController _leftHandPrefab, _rightHandPrefab;
        private HandIdentifier _selectedHand = HandIdentifier.None;
        private int _selectedGrabPointIndex = -1;
        private float _t = 0;

        private bool IsMultiPointMode => _constrainter.ConstraintType == HandConstrainType.MultiPoint;
        private bool HasSelectedGrabPoint => _selectedGrabPointIndex >= 0 && _selectedGrabPointIndex < _grabPointsProperty.arraySize;

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
            _grabPointsProperty = serializedObject.FindProperty("grabPoints");
            _selectedGrabPointIndex = -1;
            InitializeVariables();
            EditorApplication.update += OnUpdate;
        }

        private void OnDisable()
        {
            _selectedHand = HandIdentifier.None;
            _selectedGrabPointIndex = -1;
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
                _selectedGrabPointIndex = -1;
                DrawHandSelection();
                if (_selectedHand != HandIdentifier.None)
                {
                    DrawHandPositionEditor();
                    EditorGUILayout.Space();
                    DrawHandConstraints();
                }
            }
            else if (IsMultiPointMode)
            {
                DrawMultiPointEditor();
            }
            else
            {
                _selectedGrabPointIndex = -1;
                DeselectHands();
            }

            if (GUI.changed)
            {
                UpdateHandTransformFromVectors();
            }

            serializedObject.ApplyModifiedProperties();
        }

        // ─── Constraint Type Header ──────────────────────────────────────

        private void DrawConstraintType()
        {
            EditorGUILayout.PropertyField(_constraintTypeProperty, new GUIContent("Hand Constraint Type"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useSmoothTransitionsProperty, new GUIContent("Use Smooth Transitions"));

            if (_useSmoothTransitionsProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_transitionSpeedProperty, new GUIContent("Transition Speed"));

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

                case HandConstrainType.MultiPoint:
                    EditorGUILayout.HelpBox(
                        "Multiple grab points mode. Each point defines a position on the object with its own hand pose. " +
                        "The nearest point to the interaction contact is selected automatically when grabbed.",
                        MessageType.Info);
                    break;
            }
        }

        // ─── MultiPoint Editor ───────────────────────────────────────────

        private void DrawMultiPointEditor()
        {
            EditorGUILayout.LabelField("Grab Points", EditorStyles.boldLabel);

            EnsureMinimumGrabPoints();

            if (_grabPointsProperty.arraySize > 0)
            {
                DrawGrabPointsList();
            }

            EditorGUILayout.Space();

            if (HasSelectedGrabPoint)
            {
                DrawSelectedGrabPointEditor();
            }
            else if (_grabPointsProperty.arraySize > 0)
            {
                EditorGUILayout.HelpBox("Select a grab point from the list above to edit its hand poses.", MessageType.Info);
            }
        }

        private void EnsureMinimumGrabPoints()
        {
            if (_grabPointsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No grab points defined. Add a default grab point initialized from the singular constraint data.",
                    MessageType.Warning);

                if (GUILayout.Button("Add Default Grab Point", GUILayout.Height(28)))
                {
                    AddGrabPointFromSingular();
                }
            }
        }

        private void AddGrabPointFromSingular()
        {
            _grabPointsProperty.arraySize++;
            int newIndex = _grabPointsProperty.arraySize - 1;
            var newPoint = _grabPointsProperty.GetArrayElementAtIndex(newIndex);

            newPoint.FindPropertyRelative("pointName").stringValue = $"Grab Point {newIndex + 1}";
            newPoint.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
            newPoint.FindPropertyRelative("localRotation").vector3Value = Vector3.zero;

            CopyPoseConstraints(_leftPoseConstraintsProperty, newPoint.FindPropertyRelative("leftPoseConstraints"));
            CopyPoseConstraints(_rightPoseConstraintsProperty, newPoint.FindPropertyRelative("rightPoseConstraints"));
            CopyHandPositioningDirect(_leftHandPositioningProperty, newPoint.FindPropertyRelative("leftHandPositioning"));
            CopyHandPositioningDirect(_rightHandPositioningProperty, newPoint.FindPropertyRelative("rightHandPositioning"));

            serializedObject.ApplyModifiedProperties();
            SelectGrabPoint(newIndex);
        }

        private void AddEmptyGrabPoint()
        {
            _grabPointsProperty.arraySize++;
            int newIndex = _grabPointsProperty.arraySize - 1;
            var newPoint = _grabPointsProperty.GetArrayElementAtIndex(newIndex);

            newPoint.FindPropertyRelative("pointName").stringValue = $"Grab Point {newIndex + 1}";
            newPoint.FindPropertyRelative("localPosition").vector3Value = Vector3.zero;
            newPoint.FindPropertyRelative("localRotation").vector3Value = Vector3.zero;

            serializedObject.ApplyModifiedProperties();
            SelectGrabPoint(newIndex);
        }

        private void DrawGrabPointsList()
        {
            for (int i = 0; i < _grabPointsProperty.arraySize; i++)
            {
                var pointProp = _grabPointsProperty.GetArrayElementAtIndex(i);
                var nameProp = pointProp.FindPropertyRelative("pointName");
                var localPos = pointProp.FindPropertyRelative("localPosition").vector3Value;

                bool isSelected = _selectedGrabPointIndex == i;

                EditorGUILayout.BeginHorizontal(isSelected ? "SelectionRect" : EditorStyles.helpBox);

                string label = $"{nameProp.stringValue}  ({localPos.x:F2}, {localPos.y:F2}, {localPos.z:F2})";
                if (GUILayout.Toggle(isSelected, label, EditorStyles.toolbarButton))
                {
                    if (!isSelected) SelectGrabPoint(i);
                }
                else if (isSelected)
                {
                    SelectGrabPoint(-1);
                }

                if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(18)))
                {
                    RemoveGrabPoint(i);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("+ Add Grab Point", GUILayout.Height(24)))
            {
                AddEmptyGrabPoint();
            }
        }

        private void SelectGrabPoint(int index)
        {
            if (_selectedGrabPointIndex == index) return;

            DeselectHands();
            _selectedHand = HandIdentifier.None;
            _selectedGrabPointIndex = index;
            SceneView.RepaintAll();
        }

        private void RemoveGrabPoint(int index)
        {
            if (_selectedGrabPointIndex == index)
            {
                DeselectHands();
                _selectedHand = HandIdentifier.None;
                _selectedGrabPointIndex = -1;
            }
            else if (_selectedGrabPointIndex > index)
            {
                _selectedGrabPointIndex--;
            }

            _grabPointsProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSelectedGrabPointEditor()
        {
            var pointProp = _grabPointsProperty.GetArrayElementAtIndex(_selectedGrabPointIndex);
            var nameProp = pointProp.FindPropertyRelative("pointName");
            var localPosProp = pointProp.FindPropertyRelative("localPosition");
            var localRotProp = pointProp.FindPropertyRelative("localRotation");

            EditorGUILayout.LabelField($"Editing: {nameProp.stringValue}", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
            EditorGUILayout.PropertyField(localPosProp, new GUIContent("Position on Object"));
            EditorGUILayout.PropertyField(localRotProp, new GUIContent("Rotation on Object"));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            // Reuse the same hand selection / editing flow as Constrained mode
            DrawHandSelection();
            if (_selectedHand != HandIdentifier.None)
            {
                DrawHandPositionEditor();
                EditorGUILayout.Space();
                DrawHandConstraints();
            }
        }

        // ─── Shared Hand Editing (works for both Constrained and MultiPoint) ─

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

            var positioningProp = GetActiveHandPositioningProperty();
            if (positioningProp != null)
            {
                EditorGUILayout.PropertyField(positioningProp, new GUIContent("Positioning"));
            }

            EditorGUI.indentLevel--;
        }

        private void DrawHandConstraints()
        {
            EditorGUILayout.LabelField("Pose Constraints", EditorStyles.boldLabel);

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

            var poseConstraintsProperty = GetActivePoseConstraintsProperty();
            if (poseConstraintsProperty == null) return;

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

        // ─── Property Resolution (context-aware: singular vs grab point) ─

        private SerializedProperty GetActivePoseConstraintsProperty()
        {
            if (IsMultiPointMode && HasSelectedGrabPoint)
            {
                var pointProp = _grabPointsProperty.GetArrayElementAtIndex(_selectedGrabPointIndex);
                return _selectedHand == HandIdentifier.Left
                    ? pointProp.FindPropertyRelative("leftPoseConstraints")
                    : pointProp.FindPropertyRelative("rightPoseConstraints");
            }
            return _selectedHand == HandIdentifier.Left
                ? _leftPoseConstraintsProperty
                : _rightPoseConstraintsProperty;
        }

        private SerializedProperty GetActiveHandPositioningProperty()
        {
            if (IsMultiPointMode && HasSelectedGrabPoint)
            {
                var pointProp = _grabPointsProperty.GetArrayElementAtIndex(_selectedGrabPointIndex);
                return _selectedHand == HandIdentifier.Left
                    ? pointProp.FindPropertyRelative("leftHandPositioning")
                    : pointProp.FindPropertyRelative("rightHandPositioning");
            }
            return _selectedHand == HandIdentifier.Left
                ? _leftHandPositioningProperty
                : _rightHandPositioningProperty;
        }

        // ─── Pose Selection & Finger Constraints ────────────────────────

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

            var currentIndex = targetPoseIndexProperty.intValue;
            if (currentIndex >= availablePoses.Length || currentIndex < 0)
            {
                currentIndex = 0;
            }

            var newIndex = EditorGUILayout.Popup($"{_selectedHand} Hand Target Pose", currentIndex, poseNames);

            if (newIndex != currentIndex)
            {
                targetPoseIndexProperty.intValue = newIndex;
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets available poses from HandData.
        /// </summary>
        private PoseData[] GetAvailablePoses(HandData handData)
        {
            if (handData == null) return new PoseData[0];
            return handData.Poses ?? new PoseData[0];
        }

        private bool IsStaticPose()
        {
            var poseConstraintsProp = GetActivePoseConstraintsProperty();
            if (poseConstraintsProp == null) return false;

            var targetPoseIndex = poseConstraintsProp.FindPropertyRelative("targetPoseIndex").intValue;
            if (_handData == null || targetPoseIndex <= 0 || targetPoseIndex >= _handData.Poses.Length) return false;
            var selectedPose = _handData.Poses[targetPoseIndex];
            return selectedPose.Type == PoseData.PoseType.Static;
        }

        private void DrawFingerConstraint(SerializedProperty poseConstraintsProperty, string fingerPropertyName,
            string fingerDisplayName)
        {
            var fingerProperty = poseConstraintsProperty.FindPropertyRelative(fingerPropertyName);
            EditorGUILayout.PropertyField(fingerProperty, new GUIContent(fingerDisplayName));
        }

        // ─── Initialization ─────────────────────────────────────────────

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
                _handData = _config.HandData;
                _leftHandPrefab = _handData.LeftHandPrefab;
                _rightHandPrefab = _handData.RightHandPrefab;
            }
            else
            {
                Debug.LogWarning("No HandData found in Config. Interactive hand preview will not work.");
            }
        }

        // ─── Hand Preview Lifecycle ─────────────────────────────────────

        private HandPoseController CreateHandInPivot(Transform pivot, HandPoseController handPrefab)
        {
            if (handPrefab == null) return null;

            // Create a scale-compensation wrapper so hand rotation isn't skewed
            // by non-uniform parent scale. Position goes on the wrapper,
            // rotation goes on the hand inside the wrapper's uniform space.
            var wrapper = new GameObject("HandPreviewScaleWrapper").transform;
            wrapper.parent = pivot;
            wrapper.localRotation = Quaternion.identity;

            var pivotScale = pivot.lossyScale;
            wrapper.localScale = new Vector3(
                1f / pivotScale.x,
                1f / pivotScale.y,
                1f / pivotScale.z
            );

            wrapper.localPosition = Vector3.zero;
            _handScaleWrapper = wrapper;

            var initializedHand = Instantiate(handPrefab);
            var handObject = initializedHand.gameObject;
            handObject.transform.parent = wrapper;
            handObject.transform.localScale = Vector3.one;
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
                var interactableBase = _constrainter.GetComponent<InteractableBase>();
                if (interactableBase != null)
                {
                    interactableBase.ValidateAndCreateHierarchy();
                }

                var handPrefab = _selectedHand == HandIdentifier.Left ? _leftHandPrefab : _rightHandPrefab;
                _currentHand = CreateHandInPivot(_constrainter.ConstraintTransform, handPrefab);

                UpdateHandTransformFromVectors();
            }
        }

        private void UpdateHandTransformFromVectors()
        {
            if (_currentHand == null || _selectedHand == HandIdentifier.None) return;

            var positioningProperty = GetActiveHandPositioningProperty();
            if (positioningProperty == null) return;

            var positionOffset = positioningProperty.FindPropertyRelative("positionOffset").vector3Value;
            var rotationOffset = positioningProperty.FindPropertyRelative("rotationOffset").vector3Value;

            // Position on wrapper (in ConstraintTransform space), rotation on hand (in uniform wrapper space)
            if (_handScaleWrapper) _handScaleWrapper.localPosition = positionOffset;
            _currentHand.transform.localPosition = Vector3.zero;
            _currentHand.transform.localRotation = Quaternion.Euler(rotationOffset);
        }

        private void UpdateVectorsFromTransform()
        {
            if (_currentHand == null || _selectedHand == HandIdentifier.None) return;

            var positioningProperty = GetActiveHandPositioningProperty();
            if (positioningProperty == null) return;

            var positionOffset = positioningProperty.FindPropertyRelative("positionOffset");
            var rotationOffset = positioningProperty.FindPropertyRelative("rotationOffset");

            // Read position from wrapper, rotation from hand
            positionOffset.vector3Value = _handScaleWrapper ? _handScaleWrapper.localPosition : _currentHand.transform.localPosition;
            rotationOffset.vector3Value = _currentHand.transform.localEulerAngles;

            serializedObject.ApplyModifiedProperties();
        }

        private void DeselectHands()
        {
            if (_currentHand)
            {
                if (_handScaleWrapper)
                    DestroyImmediate(_handScaleWrapper.gameObject);
                else
                    DestroyImmediate(_currentHand.gameObject);

                _currentHand = null;
                _handScaleWrapper = null;
            }
        }

        // ─── Copy Utilities ─────────────────────────────────────────────

        /// <summary>
        /// Copies finger constraints and pose data from the other hand, with rotation values flipped.
        /// </summary>
        private void CopyFromOtherHand()
        {
            if (_selectedHand == HandIdentifier.None) return;

            var isLeftHandSelected = _selectedHand == HandIdentifier.Left;
            SerializedProperty sourcePose, targetPose, sourcePos, targetPos;

            if (IsMultiPointMode && HasSelectedGrabPoint)
            {
                var pointProp = _grabPointsProperty.GetArrayElementAtIndex(_selectedGrabPointIndex);
                sourcePose = pointProp.FindPropertyRelative(isLeftHandSelected ? "rightPoseConstraints" : "leftPoseConstraints");
                targetPose = pointProp.FindPropertyRelative(isLeftHandSelected ? "leftPoseConstraints" : "rightPoseConstraints");
                sourcePos = pointProp.FindPropertyRelative(isLeftHandSelected ? "rightHandPositioning" : "leftHandPositioning");
                targetPos = pointProp.FindPropertyRelative(isLeftHandSelected ? "leftHandPositioning" : "rightHandPositioning");
            }
            else
            {
                sourcePose = isLeftHandSelected ? _rightPoseConstraintsProperty : _leftPoseConstraintsProperty;
                targetPose = isLeftHandSelected ? _leftPoseConstraintsProperty : _rightPoseConstraintsProperty;
                sourcePos = isLeftHandSelected ? _rightHandPositioningProperty : _leftHandPositioningProperty;
                targetPos = isLeftHandSelected ? _leftHandPositioningProperty : _rightHandPositioningProperty;
            }

            CopyPoseConstraints(sourcePose, targetPose);
            CopyHandPositioning(sourcePos, targetPos);

            serializedObject.ApplyModifiedProperties();

            if (_currentHand != null)
            {
                UpdateHandTransformFromVectors();
            }

            Debug.Log($"Copied {(isLeftHandSelected ? "right" : "left")} hand data to {_selectedHand} hand with flipped rotation values.");
        }

        /// <summary>
        /// Copies pose constraints from source to target.
        /// </summary>
        private void CopyPoseConstraints(SerializedProperty source, SerializedProperty target)
        {
            var sourceTargetPoseIndex = source.FindPropertyRelative("targetPoseIndex");
            var targetTargetPoseIndex = target.FindPropertyRelative("targetPoseIndex");
            targetTargetPoseIndex.intValue = sourceTargetPoseIndex.intValue;

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

            var iterator = source.Copy();
            var enterChildren = iterator.Next(true);
            if (enterChildren)
            {
                do
                {
                    var targetProperty = target.FindPropertyRelative(iterator.name);
                    if (targetProperty != null)
                    {
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
            var sourcePosition = source.FindPropertyRelative("positionOffset");
            var targetPosition = target.FindPropertyRelative("positionOffset");
            targetPosition.vector3Value = sourcePosition.vector3Value;

            var sourceRotation = source.FindPropertyRelative("rotationOffset");
            var targetRotation = target.FindPropertyRelative("rotationOffset");

            var flippedRotation = FlipRotationForOtherHand(sourceRotation.vector3Value);
            targetRotation.vector3Value = flippedRotation;
        }

        /// <summary>
        /// Copies hand positioning from source to target without rotation flipping.
        /// </summary>
        private void CopyHandPositioningDirect(SerializedProperty source, SerializedProperty target)
        {
            var sourcePosition = source.FindPropertyRelative("positionOffset");
            var targetPosition = target.FindPropertyRelative("positionOffset");
            targetPosition.vector3Value = sourcePosition.vector3Value;

            var sourceRotation = source.FindPropertyRelative("rotationOffset");
            var targetRotation = target.FindPropertyRelative("rotationOffset");
            targetRotation.vector3Value = sourceRotation.vector3Value;
        }

        /// <summary>
        /// Flips rotation values for the other hand (mirrors the rotation).
        /// </summary>
        private Vector3 FlipRotationForOtherHand(Vector3 rotation)
        {
            return new Vector3(
                rotation.x,
                -rotation.y,
                -rotation.z
            );
        }

        // ─── Pose Animation Preview ─────────────────────────────────────

        private void OnUpdate()
        {
            SetPose();
        }

        private void SetPose()
        {
            if (_selectedHand == HandIdentifier.None || _currentHand == null) return;

            this._t += 0.01f;
            var finger = Mathf.PingPong(this._t, 1);

            PoseConstrains handConstraints;

            if (IsMultiPointMode && HasSelectedGrabPoint)
            {
                var point = _constrainter.GrabPoints[_selectedGrabPointIndex];
                handConstraints = _selectedHand == HandIdentifier.Left
                    ? point.leftPoseConstraints
                    : point.rightPoseConstraints;
            }
            else
            {
                handConstraints = _selectedHand == HandIdentifier.Left
                    ? _constrainter.LeftPoseConstrains
                    : _constrainter.RightPoseConstrains;
            }

            for (var i = 0; i < 5; i++)
            {
                _currentHand[i] = handConstraints[i].constraints.GetConstrainedValue(finger);
            }

            _currentHand.Pose = handConstraints[0].pose;
            _currentHand.UpdateGraphVariables();
        }

        // ─── Scene GUI ──────────────────────────────────────────────────

        protected virtual void OnSceneGUI()
        {
            if (IsMultiPointMode)
            {
                DrawGrabPointGizmos();
            }

            if (_currentHand && _selectedHand != HandIdentifier.None)
            {
                Tools.hidden = true;

                var worldPosition = _currentHand.transform.position;
                var worldRotation = _currentHand.transform.rotation;
                Handles.TransformHandle(ref worldPosition, ref worldRotation);

                // Position on wrapper (keeps rotation in uniform space), rotation on hand
                if (_handScaleWrapper) _handScaleWrapper.position = worldPosition;
                else _currentHand.transform.position = worldPosition;
                _currentHand.transform.rotation = worldRotation;

                UpdateVectorsFromTransform();
                DrawFingerSlidersInScene();
            }
            else
            {
                Tools.hidden = false;
            }
        }

        private void DrawGrabPointGizmos()
        {
            if (_grabPointsProperty == null) return;

            serializedObject.Update();

            for (int i = 0; i < _grabPointsProperty.arraySize; i++)
            {
                var pointProp = _grabPointsProperty.GetArrayElementAtIndex(i);
                var localPosProp = pointProp.FindPropertyRelative("localPosition");
                var localRotProp = pointProp.FindPropertyRelative("localRotation");
                var nameProp = pointProp.FindPropertyRelative("pointName");

                bool isSelected = i == _selectedGrabPointIndex;
                Transform ct = _constrainter.ConstraintTransform;

                Vector3 worldPos = ct.TransformPoint(localPosProp.vector3Value);
                Quaternion worldRot = ct.rotation * Quaternion.Euler(localRotProp.vector3Value);

                // Draw sphere for each grab point
                float sphereSize = isSelected ? 0.04f : 0.025f;
                Handles.color = isSelected ? Color.yellow : new Color(0f, 0.8f, 1f, 0.8f);
                if (Handles.Button(worldPos, Quaternion.identity, sphereSize, sphereSize * 1.5f, Handles.SphereHandleCap))
                {
                    SelectGrabPoint(i);
                    Repaint();
                }

                // Draw label
                var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = isSelected ? Color.yellow : Color.white },
                    fontSize = 10
                };
                Handles.Label(worldPos + Vector3.up * 0.06f, nameProp.stringValue, labelStyle);

                // Draw orientation axes for selected grab point when no hand is being edited
                if (isSelected && _selectedHand == HandIdentifier.None)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newWorldPos = worldPos;
                    Quaternion newWorldRot = worldRot;
                    Handles.TransformHandle(ref newWorldPos, ref newWorldRot);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_constrainter, "Move Grab Point");
                        localPosProp.vector3Value = ct.InverseTransformPoint(newWorldPos);
                        localRotProp.vector3Value = (Quaternion.Inverse(ct.rotation) * newWorldRot).eulerAngles;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        // ─── Scene View Finger Slider Panel ─────────────────────────────

        private static readonly string[] FingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };
        private static readonly string[] FingerPropertyNames =
        {
            "thumbFingerLimits", "indexFingerLimits", "middleFingerLimits",
            "ringFingerLimits", "pinkyFingerLimits"
        };

        private void DrawFingerSlidersInScene()
        {
            if (_currentHand == null || _selectedHand == HandIdentifier.None) return;
            if (IsStaticPose()) return;

            Handles.BeginGUI();

            float panelWidth = 220f;
            float panelHeight = 180f;
            float padding = 10f;
            float x = SceneView.currentDrawingSceneView.position.width - panelWidth - padding;
            float y = padding;

            var panelRect = new Rect(x, y, panelWidth, panelHeight);
            GUI.Box(panelRect, GUIContent.none, EditorStyles.helpBox);

            GUILayout.BeginArea(new Rect(panelRect.x + 8, panelRect.y + 4, panelRect.width - 16, panelRect.height - 8));
            GUILayout.Label("Finger Constraints", EditorStyles.boldLabel);

            var poseConstraintsProperty = GetActivePoseConstraintsProperty();
            if (poseConstraintsProperty == null)
            {
                GUILayout.EndArea();
                Handles.EndGUI();
                return;
            }

            serializedObject.Update();
            bool changed = false;

            for (int i = 0; i < 5; i++)
            {
                var fingerProp = poseConstraintsProperty.FindPropertyRelative(FingerPropertyNames[i]);
                if (fingerProp == null) continue;

                var minProp = fingerProp.FindPropertyRelative("min");
                var maxProp = fingerProp.FindPropertyRelative("max");
                var lockedProp = fingerProp.FindPropertyRelative("locked");

                GUILayout.BeginHorizontal();
                GUILayout.Label(FingerNames[i], GUILayout.Width(48));

                EditorGUI.BeginChangeCheck();

                float min = minProp.floatValue;
                float max = maxProp.floatValue;
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 1f, GUILayout.Width(120));

                bool locked = GUILayout.Toggle(lockedProp.boolValue, "Lock", GUILayout.Width(40));

                if (EditorGUI.EndChangeCheck())
                {
                    minProp.floatValue = min;
                    maxProp.floatValue = max;
                    lockedProp.boolValue = locked;
                    changed = true;
                }

                GUILayout.EndHorizontal();
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}
