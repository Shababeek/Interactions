using System.Reflection;
using NUnit.Framework;
using Shababeek.Interactions.Binders;
using Shababeek.Interactions.Core;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class InteractableToBoolBinderTests
    {
        private TestInteractable _interactable;
        private InteractableToBoolBinder _binder;
        private BoolVariable _hoveredVar;
        private BoolVariable _selectedVar;
        private BoolVariable _usedVar;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("BinderTest");
            _interactable = go.AddComponent<TestInteractable>();

            // Create ScriptableObject variables for testing
            _hoveredVar = ScriptableObject.CreateInstance<BoolVariable>();
            _selectedVar = ScriptableObject.CreateInstance<BoolVariable>();
            _usedVar = ScriptableObject.CreateInstance<BoolVariable>();

            // Disable the GO so OnEnable doesn't fire yet
            go.SetActive(false);

            _binder = go.AddComponent<InteractableToBoolBinder>();

            // Set serialized fields via reflection
            SetBinderField("hoveredVariable", _hoveredVar);
            SetBinderField("selectedVariable", _selectedVar);
            SetBinderField("usedVariable", _usedVar);
            SetBinderField("resetOnDisable", true);

            // Re-enable so OnEnable fires with variables set
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_binder);
            Object.DestroyImmediate(_hoveredVar);
            Object.DestroyImmediate(_selectedVar);
            Object.DestroyImmediate(_usedVar);
        }

        private void SetBinderField(string fieldName, object value)
        {
            var field = typeof(InteractableToBoolBinder).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_binder, value);
        }

        // ── Hover State Binding ──

        [Test]
        public void HoverStarted_SetsHoveredVariableTrue()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);

            Assert.IsTrue(_hoveredVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void HoverEnded_SetsHoveredVariableFalse()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.None, interactor);

            Assert.IsFalse(_hoveredVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        // ── Selection State Binding ──

        [Test]
        public void Selected_SetsSelectedVariableTrue()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Selected, interactor);

            Assert.IsTrue(_selectedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void Deselected_SetsSelectedVariableFalse()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.OnStateChanged(InteractionState.None, interactor);

            Assert.IsFalse(_selectedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        // ── Use State Binding ──

        [Test]
        public void UseStarted_SetsUsedVariableTrue()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.StartUsing(interactor);

            Assert.IsTrue(_usedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void UseEnded_SetsUsedVariableFalse()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.StartUsing(interactor);
            _interactable.StopUsing(interactor);

            Assert.IsFalse(_usedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        // ── Reset on Disable ──

        [Test]
        public void OnDisable_WithResetEnabled_ResetsAllVariables()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Hovering, interactor);
            _interactable.OnStateChanged(InteractionState.Selected, interactor);

            _binder.enabled = false;

            Assert.IsFalse(_hoveredVar.Value);
            Assert.IsFalse(_selectedVar.Value);
            Assert.IsFalse(_usedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        // ── IsHovered / IsSelected / IsUsed Properties ──

        [Test]
        public void IsHovered_WhenVariableNull_ReturnsFalse()
        {
            SetBinderField("hoveredVariable", null);

            Assert.IsFalse(_binder.IsHovered);
        }

        [Test]
        public void IsSelected_WhenVariableNull_ReturnsFalse()
        {
            SetBinderField("selectedVariable", null);

            Assert.IsFalse(_binder.IsSelected);
        }

        [Test]
        public void IsUsed_WhenVariableNull_ReturnsFalse()
        {
            SetBinderField("usedVariable", null);

            Assert.IsFalse(_binder.IsUsed);
        }

        [Test]
        public void IsHovered_WhenVariableTrue_ReturnsTrue()
        {
            _hoveredVar.Value = true;

            Assert.IsTrue(_binder.IsHovered);
        }

        [Test]
        public void IsSelected_WhenVariableTrue_ReturnsTrue()
        {
            _selectedVar.Value = true;

            Assert.IsTrue(_binder.IsSelected);
        }
    }

    [TestFixture]
    public class InteractableEventBinderTests
    {
        private TestInteractable _interactable;
        private InteractableEventBinder _binder;
        private GameEvent _selectedEvent;
        private GameEvent _deselectedEvent;
        private BoolVariable _isSelectedVar;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("EventBinderTest");
            _interactable = go.AddComponent<TestInteractable>();

            _selectedEvent = ScriptableObject.CreateInstance<GameEvent>();
            _deselectedEvent = ScriptableObject.CreateInstance<GameEvent>();
            _isSelectedVar = ScriptableObject.CreateInstance<BoolVariable>();

            go.SetActive(false);

            _binder = go.AddComponent<InteractableEventBinder>();

            var binderType = typeof(InteractableEventBinder);
            binderType.GetField("interactable", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_binder, _interactable);
            binderType.GetField("onSelectedEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_binder, _selectedEvent);
            binderType.GetField("onDeselectedEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_binder, _deselectedEvent);
            binderType.GetField("isSelectedVariable", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_binder, _isSelectedVar);

            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyComponent(_binder);
            Object.DestroyImmediate(_selectedEvent);
            Object.DestroyImmediate(_deselectedEvent);
            Object.DestroyImmediate(_isSelectedVar);
        }

        [Test]
        public void Selected_SetsIsSelectedVariableTrue()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);

            Assert.IsTrue(_isSelectedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }

        [Test]
        public void Deselected_SetsIsSelectedVariableFalse()
        {
            var interactor = TestHelpers.CreateMockInteractor(HandIdentifier.Left);
            _interactable.SetSelectResult(false);

            _interactable.OnStateChanged(InteractionState.Selected, interactor);
            _interactable.OnStateChanged(InteractionState.None, interactor);

            Assert.IsFalse(_isSelectedVar.Value);

            TestHelpers.DestroyComponent(interactor);
        }
    }

    [TestFixture]
    public class JoystickToVariableBinderTests
    {
        [Test]
        public void OnRotationChanged_AppliesDeadzone()
        {
            var go = new GameObject("JoystickBinderTest");
            var joystick = go.AddComponent<JoystickInteractable>();

            go.SetActive(false);
            var binder = go.AddComponent<JoystickToVariableBinder>();

            var xOutput = ScriptableObject.CreateInstance<FloatVariable>();
            var yOutput = ScriptableObject.CreateInstance<FloatVariable>();

            var binderType = typeof(JoystickToVariableBinder);
            binderType.GetField("joystick", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, joystick);
            binderType.GetField("xOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, xOutput);
            binderType.GetField("yOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, yOutput);
            binderType.GetField("deadzone", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, 0.1f);

            go.SetActive(true);

            // Test that the deadzone logic works by invoking the private method
            var onRotMethod = binderType.GetMethod("OnRotationChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Rotation below deadzone should output zero
            onRotMethod?.Invoke(binder, new object[] { new Vector2(0.05f, 0.05f) });
            Assert.AreEqual(0f, xOutput.Value, 0.01f);
            Assert.AreEqual(0f, yOutput.Value, 0.01f);

            // Rotation above deadzone should pass through
            onRotMethod?.Invoke(binder, new object[] { new Vector2(0.5f, 0.5f) });
            Assert.AreNotEqual(0f, xOutput.Value);
            Assert.AreNotEqual(0f, yOutput.Value);

            TestHelpers.DestroyGameObject(go);
            Object.DestroyImmediate(xOutput);
            Object.DestroyImmediate(yOutput);
        }

        [Test]
        public void OnRotationChanged_AppliesInversion()
        {
            var go = new GameObject("JoystickBinderInvertTest");
            var joystick = go.AddComponent<JoystickInteractable>();

            go.SetActive(false);
            var binder = go.AddComponent<JoystickToVariableBinder>();

            var xOutput = ScriptableObject.CreateInstance<FloatVariable>();
            var yOutput = ScriptableObject.CreateInstance<FloatVariable>();

            var binderType = typeof(JoystickToVariableBinder);
            binderType.GetField("joystick", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, joystick);
            binderType.GetField("xOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, xOutput);
            binderType.GetField("yOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, yOutput);
            binderType.GetField("invertX", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, true);
            binderType.GetField("invertY", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, true);
            binderType.GetField("deadzone", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, 0f);
            binderType.GetField("outputMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, 1f);

            go.SetActive(true);

            var onRotMethod = binderType.GetMethod("OnRotationChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            onRotMethod?.Invoke(binder, new object[] { new Vector2(0.5f, 0.5f) });

            // With inversion, positive input should give negative output
            Assert.Less(xOutput.Value, 0f);
            Assert.Less(yOutput.Value, 0f);

            TestHelpers.DestroyGameObject(go);
            Object.DestroyImmediate(xOutput);
            Object.DestroyImmediate(yOutput);
        }
    }

    [TestFixture]
    public class LeverToVariableBinderTests
    {
        [Test]
        public void OnLeverChanged_InvertOutput_InvertsValue()
        {
            var go = new GameObject("LeverBinderTest");
            var lever = go.AddComponent<LeverInteractable>();

            go.SetActive(false);
            var binder = go.AddComponent<LeverToVariableBinder>();

            var normalizedOutput = ScriptableObject.CreateInstance<FloatVariable>();

            var binderType = typeof(LeverToVariableBinder);
            binderType.GetField("lever", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, lever);
            binderType.GetField("normalizedOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, normalizedOutput);
            binderType.GetField("invertOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, true);
            binderType.GetField("outputMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, 1f);

            go.SetActive(true);

            // Invoke the private handler with a value
            var handler = binderType.GetMethod("OnLeverChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            handler?.Invoke(binder, new object[] { 0.75f });

            // Inverted: 1 - 0.75 = 0.25
            Assert.AreEqual(0.25f, normalizedOutput.Value, 0.01f);

            TestHelpers.DestroyGameObject(go);
            Object.DestroyImmediate(normalizedOutput);
        }

        [Test]
        public void OnLeverChanged_OutputMultiplier_ScalesValue()
        {
            var go = new GameObject("LeverBinderMultTest");
            var lever = go.AddComponent<LeverInteractable>();

            go.SetActive(false);
            var binder = go.AddComponent<LeverToVariableBinder>();

            var normalizedOutput = ScriptableObject.CreateInstance<FloatVariable>();

            var binderType = typeof(LeverToVariableBinder);
            binderType.GetField("lever", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, lever);
            binderType.GetField("normalizedOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, normalizedOutput);
            binderType.GetField("invertOutput", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, false);
            binderType.GetField("outputMultiplier", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(binder, 2f);

            go.SetActive(true);

            var handler = binderType.GetMethod("OnLeverChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            handler?.Invoke(binder, new object[] { 0.5f });

            Assert.AreEqual(1f, normalizedOutput.Value, 0.01f);

            TestHelpers.DestroyGameObject(go);
            Object.DestroyImmediate(normalizedOutput);
        }
    }
}
