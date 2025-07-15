using System.Runtime.CompilerServices;
using UnityEngine;
[assembly: InternalsVisibleTo("Kandooz.Interactions.Editor")]

namespace Kandooz.InteractionSystem.Core
{
    [CreateAssetMenu]

    public class Config : ScriptableObject
    {
        [SerializeField] private InputManagerBase inputManager;
        [SerializeField] private LayerMask leftHandLayer;
        [SerializeField] private LayerMask rightHandLayer;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask teleportationLayer;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GameObject gameManager;
        [SerializeField] private InputManagerType inputType;

        public int LeftHandLayer
        {
            get => (int) (Mathf.Log(leftHandLayer,2)+.5f);
            internal set => leftHandLayer = value;
        }

        public int RightHandLayer
        {
            get => (int)(Mathf.Log(rightHandLayer, 2) + .5f);
            internal set => rightHandLayer = value;
        }

        public int InteractableLayer
        {
            get => (int) (Mathf.Log(interactableLayer,2)+.5f);
            internal set => interactableLayer = value;
        }

        public int TeleportationLayer
        {
            get => (int) (Mathf.Log(teleportationLayer,2)+.5f);
            internal set => teleportationLayer = value;
        }
        
        public int PlayerLayer
        {
            get => (int)(Mathf.Log(playerLayer, 2) + .5f);
            internal set => playerLayer = value;
        }

        public InputManagerBase InputManager
        {
            get
            {
                if (inputManager) return inputManager;
                if (gameManager) return CreateInputManager();
                gameManager = new GameObject("VR Manager");
                DontDestroyOnLoad(gameManager);
                return CreateInputManager();
            }
        }

        private InputManagerBase CreateInputManager()
        {
            switch (inputType)
            {
                case InputManagerType.UnityAxisBased:
                    if (inputManager != null && inputManager is AxisBasedInputManager) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    inputManager = gameManager.AddComponent<AxisBasedInputManager>();
                    break;
                case InputManagerType.InputSystem:
                    if (inputManager != null && inputManager is InputSystemBased) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    inputManager = gameManager.AddComponent<InputSystemBased>();
                    break;
                case InputManagerType.KeyboardMock:
                    if (inputManager != null && inputManager is KeyboardBasedInput) return inputManager;
                    if (inputManager) Destroy(inputManager);
                    inputManager = gameManager.AddComponent<KeyboardBasedInput>();

                    break;
            }

            return inputManager;
        }

        private enum InputManagerType
        {
            UnityAxisBased = 0,
            InputSystem = 1,
            KeyboardMock = -1
        }
    }

    public class InputSystemBased : InputManagerBase
    {
    }
}