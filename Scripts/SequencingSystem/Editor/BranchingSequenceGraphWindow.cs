using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// EditorWindow that hosts the BranchingSequence graph view with a toolbar.
    /// </summary>
    public class BranchingSequenceGraphWindow : EditorWindow
    {
        [SerializeField] private BranchingSequence sequence;
        private BranchingSequenceGraphView _graphView;
        private Step _lastCurrentStep;

        /// <summary>
        /// Opens the graph window for the specified BranchingSequence asset.
        /// </summary>
        public static void Open(BranchingSequence sequence)
        {
            var window = GetWindow<BranchingSequenceGraphWindow>();
            window.sequence = sequence;
            window.titleContent = new GUIContent($"Sequence Graph", EditorGUIUtility.IconContent("d_AnimatorController Icon").image);
            window.BuildGraphView();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update += OnEditorUpdate;

            if (sequence != null)
                BuildGraphView();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorApplication.update -= OnEditorUpdate;
            _graphView?.SavePositions();
        }

        private void BuildGraphView()
        {
            rootVisualElement.Clear();

            if (sequence == null)
            {
                rootVisualElement.Add(new Label("No BranchingSequence selected.")
                {
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        marginTop = 40,
                        color = new StyleColor(new Color(0.6f, 0.6f, 0.6f))
                    }
                });
                return;
            }

            BuildToolbar();

            _graphView = new BranchingSequenceGraphView(sequence);
            rootVisualElement.Add(_graphView);
            _graphView.RefreshGraph();

            _graphView.schedule.Execute(() => _graphView.FrameAll()).ExecuteLater(100);
        }

        private void BuildToolbar()
        {
            var toolbar = new Toolbar();

            var sequenceLabel = new ToolbarButton(() => { Selection.activeObject = sequence; })
            {
                text = sequence.name,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingLeft = 8,
                    paddingRight = 12,
                    minWidth = 100
                }
            };
            sequenceLabel.tooltip = "Click to select the BranchingSequence asset";
            toolbar.Add(sequenceLabel);

            toolbar.Add(new ToolbarSpacer());

            toolbar.Add(new ToolbarButton(() => _graphView?.FrameAll()) { text = "Frame All" });

            toolbar.Add(new ToolbarButton(() =>
            {
                _graphView?.AutoLayout();
                _graphView?.schedule.Execute(() => _graphView.FrameAll()).ExecuteLater(50);
            })
            { text = "Auto Layout" });

            toolbar.Add(new ToolbarButton(() =>
            {
                _graphView?.SavePositions();
                BuildGraphView();
            })
            { text = "Refresh" });

            rootVisualElement.Add(toolbar);
        }

        private void OnUndoRedo()
        {
            if (_graphView != null && sequence != null)
            {
                _graphView.SavePositions();
                _graphView.RefreshGraph();
            }
        }

        private void OnEditorUpdate()
        {
            if (!Application.isPlaying || sequence == null || _graphView == null) return;

            if (sequence.CurrentStep == _lastCurrentStep) return;
            _lastCurrentStep = sequence.CurrentStep;
            _graphView.UpdateRuntimeHighlight(_lastCurrentStep); // null is handled - clears all highlights
            Repaint();
        }
    }
}
