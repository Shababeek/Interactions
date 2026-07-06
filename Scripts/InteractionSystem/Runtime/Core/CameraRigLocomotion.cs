using UnityEngine;
using UnityEngine.InputSystem;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Adds smooth (continuous) locomotion and snap-turn to a <see cref="CameraRig"/>.
    ///
    /// The rig is just a Transform that the camera and hands follow, so moving the player means
    /// moving this Transform. Movement is head-relative (you walk where you look, flattened to the
    /// horizontal plane). A <see cref="CharacterController"/> handles walls, floors, and gravity; its
    /// capsule is re-fitted every frame to sit under the tracked head so collision follows the
    /// player's real-world position, not the rig origin.
    ///
    /// Input is read directly from the Input System (the interaction system's providers don't expose
    /// a thumbstick axis). Default bindings: left thumbstick / WASD to move, right thumbstick / Q-E
    /// to snap turn. Assign your own actions in the Inspector to override.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Shababeek/Interactions/Camera Rig Locomotion")]
    public class CameraRigLocomotion : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The tracked head/camera. Auto-resolved from a CameraRig on this object, or Camera.main, if unset.")]
        [SerializeField] private Transform head;

        [Header("Move")]
        [Tooltip("Horizontal move speed in meters/second.")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;

        [Tooltip("Downward acceleration (m/s²). Keep negative.")]
        [SerializeField] private float gravity = -9.81f;

        [Tooltip("Stick magnitude below this is ignored (drift guard).")]
        [SerializeField, Range(0f, 0.9f)] private float moveDeadzone = 0.15f;

        [Header("Snap Turn")]
        [Tooltip("Degrees rotated per snap.")]
        [SerializeField, Min(0f)] private float snapTurnAngle = 45f;

        [Tooltip("Horizontal stick value needed to trigger a snap. Stick must return below it before the next snap.")]
        [SerializeField, Range(0.1f, 0.95f)] private float snapTurnThreshold = 0.7f;

        [Header("Input")]
        [SerializeField] private InputActionProperty moveAction;
        [SerializeField] private InputActionProperty turnAction;

        private CharacterController _controller;
        private float _verticalVelocity;
        private bool _snapArmed = true;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            ResolveHead();
            EnsureDefaultBindings();
        }

        private void OnEnable()
        {
            moveAction.action?.Enable();
            turnAction.action?.Enable();
        }

        private void OnDisable()
        {
            moveAction.action?.Disable();
            turnAction.action?.Disable();
        }

        private void Update()
        {
            if (head == null) ResolveHead();
            if (head == null || _controller == null) return;

            FitCapsuleToHead();
            HandleSnapTurn();
            HandleMove();
        }

        // Keep the collision capsule under the player's real head position (horizontally),
        // sized to their standing height, so walls/edges respond to where they physically are.
        private void FitCapsuleToHead()
        {
            Vector3 headLocal = transform.InverseTransformPoint(head.position);
            float minHeight = _controller.radius * 2f;
            float height = Mathf.Clamp(headLocal.y, minHeight, 3f);

            _controller.height = height;
            _controller.center = new Vector3(headLocal.x, height * 0.5f + _controller.skinWidth, headLocal.z);
        }

        private void HandleMove()
        {
            Vector2 input = moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            if (input.magnitude < moveDeadzone) input = Vector2.zero;

            // Head yaw, flattened: walk where you look.
            Vector3 forward = head.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = head.right; right.y = 0f; right.Normalize();
            Vector3 horizontal = (forward * input.y + right * input.x) * moveSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // small stick-to-ground bias
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void HandleSnapTurn()
        {
            float x = turnAction.action != null ? turnAction.action.ReadValue<Vector2>().x : 0f;

            if (Mathf.Abs(x) < snapTurnThreshold)
            {
                _snapArmed = true;
                return;
            }

            if (!_snapArmed) return;
            _snapArmed = false;

            // Rotate about the head, not the rig origin, so the player doesn't get shoved sideways.
            transform.RotateAround(head.position, Vector3.up, Mathf.Sign(x) * snapTurnAngle);
        }

        private void ResolveHead()
        {
            if (head != null) return;

            var rig = GetComponent<CameraRig>();
            if (rig != null && rig.XRCamera != null) head = rig.XRCamera.transform;
            if (head == null && Camera.main != null) head = Camera.main.transform;
        }

        // Provide sensible defaults so the component works out of the box, while still letting a
        // user assign their own actions (a reference or pre-bound action skips this).
        private void EnsureDefaultBindings()
        {
            if (moveAction.action == null)
                moveAction = new InputActionProperty(new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2"));
            if (moveAction.action.bindings.Count == 0)
            {
                moveAction.action.AddBinding("<XRController>{LeftHand}/thumbstick");
                moveAction.action.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
            }

            if (turnAction.action == null)
                turnAction = new InputActionProperty(new InputAction("Turn", InputActionType.Value, expectedControlType: "Vector2"));
            if (turnAction.action.bindings.Count == 0)
            {
                turnAction.action.AddBinding("<XRController>{RightHand}/thumbstick");
                turnAction.action.AddCompositeBinding("2DVector")
                    .With("Left", "<Keyboard>/q")
                    .With("Right", "<Keyboard>/e");
            }
        }
    }
}
