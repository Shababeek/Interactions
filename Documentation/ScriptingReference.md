# Scripting API Reference

## Overview

This reference provides comprehensive API documentation for the Shababeek Interaction System, including all public methods, properties, and events.

## Core Systems

### **InteractableBase**

Base class for all interactable objects.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/InteractableBase.cs`

#### **Properties**
```csharp
public Transform InteractionPoint { get; set; }
public bool IsSelected { get; }
public InteractorBase CurrentInteractor { get; }
public InteractionState CurrentState { get; }
```

#### **Events**
```csharp
public UnityEvent OnSelected;
public UnityEvent OnDeselected;
public UnityEvent OnHoverStart;
public UnityEvent OnHoverEnd;
public UnityEvent OnActivated;
```

#### **Protected Methods**
```csharp
protected virtual bool Select();
protected virtual void DeSelected();
protected virtual void OnHoverStart();
protected virtual void OnHoverEnd();
protected virtual void OnActivated();
```

#### **Usage Example**
```csharp
public class CustomInteractable : InteractableBase
{
    protected override bool Select()
    {
        Debug.Log("Object selected!");
        return true;
    }
    
    protected override void DeSelected()
    {
        Debug.Log("Object deselected!");
    }
}
```

### **ConstrainedInteractableBase**

Abstract base class for interactables with pose constraints.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/ConstrainedInteractableBase.cs`

#### **Properties**
```csharp
public Transform InteractableObject { get; set; }
```

#### **Protected Methods**
```csharp
protected abstract void HandleObjectMovement();
protected abstract void HandleObjectDeselection();
```

#### **Private Methods**
```csharp
private Hand GetOrCreateFakeHand(HandIdentifier handIdentifier);
private Hand CreateFakeHand(HandIdentifier handIdentifier);
private void PositionFakeHand(Transform fakeHand, HandIdentifier handIdentifier);
```

#### **Usage Example**
```csharp
public class CustomConstrainedInteractable : ConstrainedInteractableBase
{
    protected override void HandleObjectMovement()
    {
        // Implement object-specific movement logic
        transform.position = CurrentInteractor.transform.position;
    }
    
    protected override void HandleObjectDeselection()
    {
        // Implement cleanup logic
        transform.position = originalPosition;
    }
}
```

## Pose Constraint System

### **UnifiedPoseConstraintSystem**

Centralized pose constraint management.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/Constraints/UnifiedPoseConstraintSystem.cs`

#### **Properties**
```csharp
public HandConstrainType ConstraintType => constraintType;
public bool UseSmoothTransitions => useSmoothTransitions;
public float TransitionSpeed => transitionSpeed;
```

#### **Public Methods**
```csharp
public void ApplyConstraints(InteractorBase interactor);
public void RemoveConstraints(InteractorBase interactor);
public HandTransform GetTargetHandTransform(HandIdentifier handIdentifier);
```

#### **Serialized Fields**
```csharp
[SerializeField] private HandConstrainType constraintType = HandConstrainType.Constrained;
[SerializeField] private bool useSmoothTransitions = false;
[SerializeField] private float transitionSpeed = 10f;
[SerializeField] private PoseConstrains leftPoseConstraints;
[SerializeField] private PoseConstrains rightPoseConstraints;
[SerializeField] private HandPositioning leftHandPositioning = HandPositioning.Zero;
[SerializeField] private HandPositioning rightHandPositioning = HandPositioning.Zero;
```

#### **Usage Example**
```csharp
public class PoseConstraintExample : MonoBehaviour
{
    [SerializeField] private UnifiedPoseConstraintSystem poseConstraintSystem;
    [SerializeField] private InteractorBase interactor;
    
    public void ApplyHandConstraints()
    {
        poseConstraintSystem.ApplyConstraints(interactor);
    }
    
    public void RemoveHandConstraints()
    {
        poseConstraintSystem.RemoveConstraints(interactor);
    }
    
    public HandTransform GetHandPosition(HandIdentifier handId)
    {
        return poseConstraintSystem.GetTargetHandTransform(handId);
    }
}
```

### **HandPositioning**

Struct for hand position and rotation offsets.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/Constraints/HandPositioning.cs`

#### **Properties**
```csharp
public Vector3 positionOffset;
public Vector3 rotationOffset;
public static HandPositioning Zero => new HandPositioning { positionOffset = Vector3.zero, rotationOffset = Vector3.zero };
```

#### **Usage Example**
```csharp
var handPositioning = new HandPositioning
{
    positionOffset = new Vector3(0, 0.1f, 0),
    rotationOffset = new Vector3(0, 45f, 0)
};
```

### **HandConstrainType**

Enum for hand constraint behavior.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/Constraints/HandConstrainType.cs`

```csharp
public enum HandConstrainType
{
    HideHand,      // Hide the real hand completely
    FreeHand,      // Allow natural hand movement
    Constrained    // Apply specific pose constraints
}
```

## Hand System

### **Hand**

VR hand representation with model management.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/Hand/Hand.cs`

#### **Properties**
```csharp
public GameObject HandModel { get; set; }
public HandData HandData { get; set; }
```

#### **Public Methods**
```csharp
public void ToggleRenderer(bool visible);
public void AutoAssignHandModel();
```

#### **Serialized Fields**
```csharp
[SerializeField] private GameObject handModel;
[SerializeField] private HandData handData;
```

#### **Usage Example**
```csharp
public class HandController : MonoBehaviour
{
    [SerializeField] private Hand hand;
    
    public void ShowHand()
    {
        hand.ToggleRenderer(true);
    }
    
    public void HideHand()
    {
        hand.ToggleRenderer(false);
    }
    
    public void SetupHand()
    {
        hand.AutoAssignHandModel();
    }
}
```

### **HandData**

ScriptableObject for hand configuration.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/HandData/HandData.cs`

#### **Properties**
```csharp
public GameObject LeftHandPrefab => leftHandPrefab;
public GameObject RightHandPrefab => rightHandPrefab;
public PoseData[] PosesArray => posesArray;
public string HandName => handName;
```

#### **Public Methods**
```csharp
public PoseData GetPoseByName(string poseName);
public PoseData GetPoseByIndex(int index);
public int GetPoseIndex(string poseName);
public bool HasPose(string poseName);
```

#### **Serialized Fields**
```csharp
[SerializeField] private GameObject leftHandPrefab;
[SerializeField] private GameObject rightHandPrefab;
[SerializeField] private PoseData[] posesArray;
[SerializeField] private string handName;
```

#### **Usage Example**
```csharp
public class HandDataExample : MonoBehaviour
{
    [SerializeField] private HandData handData;
    
    public void SetupHandPoses()
    {
        var grabPose = handData.GetPoseByName("Grab");
        if (grabPose != null)
        {
            Debug.Log($"Found grab pose: {grabPose.PoseName}");
        }
    }
    
    public void ListAllPoses()
    {
        foreach (var pose in handData.PosesArray)
        {
            Debug.Log($"Pose: {pose.PoseName}, Type: {pose.PoseType}");
        }
    }
}
```

### **HandPoseController**

Controls hand pose blending and animation.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Animations/HandPoseController.cs`

#### **Public Methods**
```csharp
public void SetPose(PoseData poseData);
public void SetFingerConstraint(FingerName finger, float value);
public void ResetPose();
public void BlendToPose(PoseData targetPose, float duration);
public PoseData GetCurrentPose();
public bool IsPoseActive();
```

#### **Usage Example**
```csharp
public class PoseControllerExample : MonoBehaviour
{
    [SerializeField] private HandPoseController poseController;
    [SerializeField] private HandData handData;
    
    public void SetGrabPose()
    {
        var grabPose = handData.GetPoseByName("Grab");
        if (grabPose != null)
        {
            poseController.SetPose(grabPose);
        }
    }
    
    public void SetFingerBend(FingerName finger, float bendValue)
    {
        poseController.SetFingerConstraint(finger, bendValue);
    }
    
    public void SmoothTransitionToPose(string poseName, float duration)
    {
        var pose = handData.GetPoseByName(poseName);
        if (pose != null)
        {
            poseController.BlendToPose(pose, duration);
        }
    }
}
```

## Feedback System

### **FeedbackSystem**

Unified feedback management.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/Feedback/FeedbackSystem.cs`

#### **Public Methods**
```csharp
public void ApplyFeedback();
public void ApplyFeedback(FeedbackType feedbackType);
```

#### **Serialized Fields**
```csharp
[SerializeField] private List<FeedbackData> feedbacks;
```

#### **Usage Example**
```csharp
public class FeedbackExample : MonoBehaviour
{
    [SerializeField] private FeedbackSystem feedbackSystem;
    
    public void TriggerAllFeedback()
    {
        feedbackSystem.ApplyFeedback();
    }
    
    public void TriggerSpecificFeedback(FeedbackType feedbackType)
    {
        feedbackSystem.ApplyFeedback(feedbackType);
    }
}
```

### **FeedbackData**

Base class for all feedback types.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/Feedback/FeedbackData.cs`

#### **Properties**
```csharp
public FeedbackType FeedbackType => feedbackType;
public bool IsEnabled => isEnabled;
```

#### **Public Methods**
```csharp
public abstract void ApplyFeedback();
public virtual void OnValidate();
```

#### **Serialized Fields**
```csharp
[SerializeField] private FeedbackType feedbackType;
[SerializeField] private bool isEnabled = true;
```

## Scriptable System

### **ScriptableVariable<T>**

Base class for observable, serializable variables.

**Location**: `Assets/Shababeek/Interactions/Scripts/Core/Runtime/ScriptableSystem/Variables/ScriptableVariable.cs`

#### **Properties**
```csharp
public T Value { get; set; }
public string VariableName { get; set; }
public UnityEvent<T> OnValueChanged;
```

#### **Public Methods**
```csharp
public void SetValue(T newValue);
public T GetValue();
public void AddListener(UnityAction<T> listener);
public void RemoveListener(UnityAction<T> listener);
```

#### **Usage Example**
```csharp
public class VariableExample : MonoBehaviour
{
    [SerializeField] private FloatVariable healthVariable;
    [SerializeField] private BoolVariable isAliveVariable;
    
    public void UpdateHealth(float newHealth)
    {
        healthVariable.Value = newHealth;
        
        if (newHealth <= 0)
        {
            isAliveVariable.Value = false;
        }
    }
    
    public void ListenToHealthChanges()
    {
        healthVariable.OnValueChanged.AddListener(OnHealthChanged);
    }
    
    private void OnHealthChanged(float newHealth)
    {
        Debug.Log($"Health changed to: {newHealth}");
    }
}
```

### **GameEvent**

Observable, serializable events.

**Location**: `Assets/Shababeek/Interactions/Scripts/Core/Runtime/ScriptableSystem/Events/GameEvent.cs`

#### **Properties**
```csharp
public string EventName => eventName;
public string EventDescription => eventDescription;
```

#### **Public Methods**
```csharp
public void Raise();
public void AddListener(UnityAction listener);
public void RemoveListener(UnityAction listener);
```

#### **Serialized Fields**
```csharp
[SerializeField] private string eventName;
[SerializeField] private string eventDescription;
```

#### **Usage Example**
```csharp
public class EventExample : MonoBehaviour
{
    [SerializeField] private GameEvent playerDeathEvent;
    [SerializeField] private GameEvent levelCompleteEvent;
    
    public void TriggerPlayerDeath()
    {
        playerDeathEvent.Raise();
    }
    
    public void ListenToEvents()
    {
        playerDeathEvent.AddListener(OnPlayerDeath);
        levelCompleteEvent.AddListener(OnLevelComplete);
    }
    
    private void OnPlayerDeath()
    {
        Debug.Log("Player died!");
    }
    
    private void OnLevelComplete()
    {
        Debug.Log("Level completed!");
    }
}
```

### **GameEventListener**

Component for listening to multiple GameEvents.

**Location**: `Assets/Shababeek/Interactions/Scripts/Core/Runtime/ScriptableSystem/Events/GameEventListener.cs`

#### **Serialized Fields**
```csharp
[SerializeField] private List<GameEventWithEvents> gameEventList;
```

#### **Public Methods**
```csharp
public void AddGameEvent(GameEvent gameEvent);
```

#### **Usage Example**
```csharp
public class EventListenerExample : MonoBehaviour
{
    [SerializeField] private GameEventListener eventListener;
    
    public void SetupEventListeners()
    {
        // Events are configured in the Inspector
        // Each event can have its own Unity Events
    }
}
```

## Sequencing System

### **Sequence**

Linear sequence system.

**Location**: `Assets/Shababeek/Interactions/Scripts/SequencingSystem/Runtime/Core/Sequence.cs`

#### **Properties**
```csharp
public List<Step> Steps => steps;
public bool IsActive => isActive;
public int CurrentStepIndex => currentStepIndex;
```

#### **Public Methods**
```csharp
public void StartSequence();
public void StopSequence();
public void RestartSequence();
public void PauseSequence();
public void ResumeSequence();
```

#### **Serialized Fields**
```csharp
[SerializeField] private List<Step> steps = new List<Step>();
[SerializeField] private bool autoStart = false;
```

#### **Usage Example**
```csharp
public class SequenceExample : MonoBehaviour
{
    [SerializeField] private Sequence sequence;
    
    public void StartTutorial()
    {
        sequence.StartSequence();
    }
    
    public void StopTutorial()
    {
        sequence.StopSequence();
    }
}
```

### **BranchingSequence**

Visual branching sequence system.

**Location**: `Assets/Shababeek/Interactions/Scripts/SequencingSystem/Runtime/Core/BranchingSequence.cs`

#### **Properties**
```csharp
public List<BoolVariable> BoolVariables => boolVariables;
public List<IntVariable> IntVariables => intVariables;
public List<FloatVariable> FloatVariables => floatVariables;
public bool AllowParallelExecution => allowParallelExecution;
public bool AutoAdvance => autoAdvance;
```

#### **Public Methods**
```csharp
public void SetBool(string variableName, bool value);
public bool GetBool(string variableName);
public void SetInt(string variableName, int value);
public int GetInt(string variableName);
public void SetFloat(string variableName, float value);
public float GetFloat(string variableName);
public void AddBoolVariable(BoolVariable variable);
public void AddIntVariable(IntVariable variable);
public void AddFloatVariable(FloatVariable variable);
public void RemoveVariable(string variableName);
```

#### **Serialized Fields**
```csharp
[SerializeField] private List<BoolVariable> boolVariables = new List<BoolVariable>();
[SerializeField] private List<IntVariable> intVariables = new List<IntVariable>();
[SerializeField] private List<FloatVariable> floatVariables = new List<FloatVariable>();
[SerializeField] private bool allowParallelExecution = false;
[SerializeField] private bool autoAdvance = true;
```

#### **Usage Example**
```csharp
public class BranchingSequenceExample : MonoBehaviour
{
    [SerializeField] private BranchingSequence branchingSequence;
    
    public void SetupVariables()
    {
        branchingSequence.SetBool("isPlayerAlive", true);
        branchingSequence.SetInt("playerHealth", 100);
        branchingSequence.SetFloat("playerSpeed", 5.0f);
    }
    
    public void CheckPlayerStatus()
    {
        bool isAlive = branchingSequence.GetBool("isPlayerAlive");
        int health = branchingSequence.GetInt("playerHealth");
        float speed = branchingSequence.GetFloat("playerSpeed");
        
        Debug.Log($"Player - Alive: {isAlive}, Health: {health}, Speed: {speed}");
    }
}
```

### **BranchingStep**

Individual steps in branching sequences.

**Location**: `Assets/Shababeek/Interactions/Scripts/SequencingSystem/Runtime/Core/BranchingStep.cs`

#### **Properties**
```csharp
public string StepName => stepName;
public bool AutoComplete => autoComplete;
public bool AllowManualCompletion => allowManualCompletion;
public Color StepColor => stepColor;
public Sprite StepIcon => stepIcon;
public List<Transition> Transitions => transitions;
```

#### **Public Methods**
```csharp
public async Task ExecuteStep();
public bool EvaluateTransition(Transition transition);
public void CompleteStep();
public void FailStep();
```

#### **Serialized Fields**
```csharp
[SerializeField] private string stepName = "New Step";
[SerializeField] private bool autoComplete = false;
[SerializeField] private bool allowManualCompletion = true;
[SerializeField] private Color stepColor = Color.white;
[SerializeField] private Sprite stepIcon;
[SerializeField] private List<Transition> transitions = new List<Transition>();
```

#### **Usage Example**
```csharp
public class BranchingStepExample : MonoBehaviour
{
    [SerializeField] private BranchingStep step;
    
    public async void ExecuteStep()
    {
        await step.ExecuteStep();
    }
    
    public void CompleteStep()
    {
        step.CompleteStep();
    }
}
```

## Input System

### **ButtonObservable**

UniRx-based button state management.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/Input/ButtonObservable.cs`

#### **Properties**
```csharp
public bool IsDown { get; }
public IObservable<ButtonState> OnStateChanged { get; }
public IObservable<Unit> OnButtonDown { get; }
public IObservable<Unit> OnButtonUp { get; }
```

#### **Public Methods**
```csharp
public void SetState(ButtonState state);
public void SetDown(bool isDown);
public IDisposable Subscribe(IObserver<ButtonState> observer);
```

#### **Usage Example**
```csharp
public class ButtonExample : MonoBehaviour
{
    [SerializeField] private ButtonObservable buttonObservable;
    
    private void Start()
    {
        // Subscribe to button state changes
        buttonObservable.OnButtonDown
            .Subscribe(_ => Debug.Log("Button pressed!"))
            .AddTo(this);
            
        buttonObservable.OnButtonUp
            .Subscribe(_ => Debug.Log("Button released!"))
            .AddTo(this);
    }
}
```

### **XRButton**

Simple button with press detection.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Interactions/Interactables/VRButton.cs`

#### **Events**
```csharp
public UnityEvent onButtonDown;
public UnityEvent onButtonUp;
```

#### **Observables**
```csharp
public IObservable<Unit> OnButtonDown { get; }
public IObservable<Unit> OnButtonUp { get; }
```

#### **Usage Example**
```csharp
public class XRButtonExample : MonoBehaviour
{
    [SerializeField] private VRButton vrButton;
    
    private void Start()
    {
        // Subscribe to UniRx observables
        vrButton.OnButtonDown
            .Subscribe(_ => Debug.Log("XR Button pressed!"))
            .AddTo(this);
            
        vrButton.OnButtonUp
            .Subscribe(_ => Debug.Log("XR Button released!"))
            .AddTo(this);
    }
}
```

## Configuration System

### **Config**

Central configuration for system-wide settings.

**Location**: `Assets/Shababeek/Interactions/Scripts/InteractionSystem/Runtime/Core/Config.cs`

#### **Properties**
```csharp
public LayerMask InteractionLayer => interactionLayer;
public LayerMask HandLayer => handLayer;
public LayerMask InteractableLayer => interactableLayer;
public LayerMask UILayer => uiLayer;
public LayerMask FeedbackLayer => feedbackLayer;
```

#### **Serialized Fields**
```csharp
[SerializeField] private LayerMask interactionLayer = 1 << 8;
[SerializeField] private LayerMask handLayer = 1 << 9;
[SerializeField] private LayerMask interactableLayer = 1 << 10;
[SerializeField] private LayerMask uiLayer = 1 << 11;
[SerializeField] private LayerMask feedbackLayer = 1 << 12;
```

#### **Usage Example**
```csharp
public class ConfigExample : MonoBehaviour
{
    [SerializeField] private Config config;
    
    public void SetupLayers()
    {
        // Layers are automatically configured
        Debug.Log($"Interaction Layer: {config.InteractionLayer}");
        Debug.Log($"Hand Layer: {config.HandLayer}");
        Debug.Log($"Interactable Layer: {config.InteractableLayer}");
    }
}
```

## Enums and Types

### **HandIdentifier**

Enum for identifying left or right hand.

```csharp
public enum HandIdentifier
{
    None,
    Left,
    Right
}
```

### **InteractionState**

Enum for interaction states.

```csharp
public enum InteractionState
{
    None,
    Hovering,
    Selected,
    Activated
}
```

### **FeedbackType**

Enum for feedback types.

```csharp
public enum FeedbackType
{
    Material,
    Haptic,
    Audio,
    Animation,
    SFX
}
```

### **FingerName**

Enum for finger identification.

```csharp
public enum FingerName
{
    Thumb,
    Index,
    Middle,
    Ring,
    Pinky
}
```

### **PoseType**

Enum for pose types.

```csharp
public enum PoseType
{
    Static,     // Fixed pose with no finger constraints
    Dynamic     // Pose with finger constraint support
}
```

### **ButtonState**

Enum for button states.

```csharp
public enum ButtonState
{
    Up,
    Down
}
```

## Best Practices

### **Performance**

- Cache frequently accessed components
- Use object pooling for frequently created objects
- Dispose of UniRx subscriptions properly
- Use static poses when possible

### **Memory Management**

- Remove event listeners when not needed
- Clean up fake hands properly
- Reuse ScriptableObject assets
- Use appropriate variable types

### **Error Handling**

- Validate inputs and references
- Provide meaningful error messages
- Handle edge cases gracefully
- Log important events for debugging

### **Code Organization**

- Follow established naming conventions
- Use XML documentation for public members
- Add tooltips for serialized fields
- Organize code logically

## Common Patterns

### **Event-Driven Architecture**

```csharp
public class EventDrivenExample : MonoBehaviour
{
    [SerializeField] private GameEvent playerDeathEvent;
    [SerializeField] private FloatVariable playerHealth;
    
    private void Start()
    {
        playerHealth.OnValueChanged.AddListener(OnHealthChanged);
    }
    
    private void OnHealthChanged(float newHealth)
    {
        if (newHealth <= 0)
        {
            playerDeathEvent.Raise();
        }
    }
}
```

### **Reactive Programming**

```csharp
public class ReactiveExample : MonoBehaviour
{
    [SerializeField] private ButtonObservable button;
    [SerializeField] private FloatVariable counter;
    
    private void Start()
    {
        button.OnButtonDown
            .Subscribe(_ => counter.Value++)
            .AddTo(this);
    }
}
```

### **Pose Constraint Integration**

```csharp
public class PoseConstraintExample : MonoBehaviour
{
    [SerializeField] private UnifiedPoseConstraintSystem poseConstraintSystem;
    [SerializeField] private InteractorBase interactor;
    
    public void ApplyConstraints()
    {
        poseConstraintSystem.ApplyConstraints(interactor);
    }
    
    public void RemoveConstraints()
    {
        poseConstraintSystem.RemoveConstraints(interactor);
    }
}
```

### **Sequence Management**

```csharp
public class SequenceExample : MonoBehaviour
{
    [SerializeField] private BranchingSequence sequence;
    
    public void StartTutorial()
    {
        sequence.SetBool("tutorialStarted", true);
        sequence.SetInt("currentStep", 0);
    }
    
    public void CompleteStep()
    {
        int currentStep = sequence.GetInt("currentStep");
        sequence.SetInt("currentStep", currentStep + 1);
    }
}
``` 