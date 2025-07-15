using System;
using System.Collections.Generic;
using Shababeek.Interactions;
using UniRx;
using UnityEngine;

namespace Shababeek.Sequencing
{
    [CreateAssetMenu(menuName = "Shababeek/Sequencing/Actions/SequenceOrderAction")]
    public class SequenceOrderAction : AbstractSequenceAction
    {
        [System.Serializable]
        public class OrderedInteraction
        {
            public enum InteractionType
            {
                Selection,
                Deselection,
                Activation,
                HoverStart,
                HoverEnd,
                Custom
            }

            public InteractionType interactionType;
            public InteractableBase targetInteractable;
            public string customEventName;
            public int orderIndex;
            public bool completed = false;
        }

        [SerializeField] private List<OrderedInteraction> orderedInteractions = new List<OrderedInteraction>();
        [SerializeField] private bool resetOnWrongOrder = true;
        [SerializeField] private bool allowSkip = false;
        [SerializeField] private float timeoutDuration = 30f;
        [SerializeField] private bool useTimeout = false;

        private StepEventListener listener;
        private int currentOrderIndex = 0;
        private float startTime;
        private CompositeDisposable interactionDisposables = new CompositeDisposable();

        private void Awake()
        {
            listener = GetComponent<StepEventListener>();
        }

        protected override void OnStepStatusChanged(SequenceStatus status)
        {
            if (status == SequenceStatus.Started)
            {
                StartOrderTracking();
            }
            else
            {
                StopOrderTracking();
            }
        }

        private void StartOrderTracking()
        {
            currentOrderIndex = 0;
            startTime = Time.time;
            
            // Reset all interactions
            foreach (var interaction in orderedInteractions)
            {
                interaction.completed = false;
            }

            // Subscribe to all interactions
            SubscribeToInteractions();

            // Start timeout if enabled
            if (useTimeout)
            {
                Observable.Timer(System.TimeSpan.FromSeconds(timeoutDuration))
                    .Where(_ => Started)
                    .Do(_ => OnTimeout())
                    .Subscribe()
                    .AddTo(Disposable);
            }
        }

        private void StopOrderTracking()
        {
            interactionDisposables.Clear();
        }

        private void SubscribeToInteractions()
        {
            foreach (var interaction in orderedInteractions)
            {
                if (interaction.targetInteractable == null) continue;

                switch (interaction.interactionType)
                {
                    case OrderedInteraction.InteractionType.Selection:
                        interaction.targetInteractable.OnSelected
                            .Do(_ => OnInteraction(interaction))
                            .Subscribe()
                            .AddTo(interactionDisposables);
                        break;

                    case OrderedInteraction.InteractionType.Deselection:
                        interaction.targetInteractable.OnDeselected
                            .Do(_ => OnInteraction(interaction))
                            .Subscribe()
                            .AddTo(interactionDisposables);
                        break;

                    case OrderedInteraction.InteractionType.Activation:
                        interaction.targetInteractable.OnActivated
                            .Do(_ => OnInteraction(interaction))
                            .Subscribe()
                            .AddTo(interactionDisposables);
                        break;

                    case OrderedInteraction.InteractionType.HoverStart:
                        interaction.targetInteractable.OnHoverStarted
                            .Do(_ => OnInteraction(interaction))
                            .Subscribe()
                            .AddTo(interactionDisposables);
                        break;

                    case OrderedInteraction.InteractionType.HoverEnd:
                        interaction.targetInteractable.OnHoverEnded
                            .Do(_ => OnInteraction(interaction))
                            .Subscribe()
                            .AddTo(interactionDisposables);
                        break;

                    case OrderedInteraction.InteractionType.Custom:
                        // Custom events are handled externally via TriggerCustomInteraction
                        break;
                }
            }
        }

        private void OnInteraction(OrderedInteraction interaction)
        {
            if (!Started) return;

            // Check if this is the next expected interaction
            if (interaction.orderIndex == currentOrderIndex)
            {
                interaction.completed = true;
                currentOrderIndex++;

                // Check if all interactions are completed
                if (IsSequenceComplete())
                {
                    listener.OnActionCompleted();
                }
            }
            else if (resetOnWrongOrder)
            {
                // Wrong order - reset the sequence
                ResetSequence();
            }
        }

        private bool IsSequenceComplete()
        {
            if (allowSkip)
            {
                // Check if all required interactions are completed
                foreach (var interaction in orderedInteractions)
                {
                    if (interaction.orderIndex < currentOrderIndex && !interaction.completed)
                    {
                        return false;
                    }
                }
                return currentOrderIndex >= orderedInteractions.Count;
            }
            else
            {
                // All interactions must be completed in order
                return currentOrderIndex >= orderedInteractions.Count;
            }
        }

        private void ResetSequence()
        {
            currentOrderIndex = 0;
            foreach (var interaction in orderedInteractions)
            {
                interaction.completed = false;
            }
            startTime = Time.time;
        }

        private void OnTimeout()
        {
            if (Started)
            {
                ResetSequence();
                // Optionally trigger a timeout event or fail the step
            }
        }

        // Public method to trigger custom interactions
        public void TriggerCustomInteraction(string customEventName)
        {
            if (!Started) return;

            var interaction = orderedInteractions.Find(i => 
                i.interactionType == OrderedInteraction.InteractionType.Custom && 
                i.customEventName == customEventName);

            if (interaction != null)
            {
                OnInteraction(interaction);
            }
        }

        // Public methods for external control
        public void AddOrderedInteraction(OrderedInteraction interaction)
        {
            orderedInteractions.Add(interaction);
            // Sort by order index
            orderedInteractions.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));
        }

        public void RemoveOrderedInteraction(int index)
        {
            if (index >= 0 && index < orderedInteractions.Count)
            {
                orderedInteractions.RemoveAt(index);
            }
        }

        public void ClearOrderedInteractions()
        {
            orderedInteractions.Clear();
        }

        public int GetCurrentProgress()
        {
            return currentOrderIndex;
        }

        public int GetTotalSteps()
        {
            return orderedInteractions.Count;
        }

        public float GetProgressPercentage()
        {
            if (orderedInteractions.Count == 0) return 0f;
            return (float)currentOrderIndex / orderedInteractions.Count;
        }

        public float GetElapsedTime()
        {
            return Time.time - startTime;
        }

        // Helper methods to create ordered interactions
        public static OrderedInteraction CreateSelectionInteraction(InteractableBase target, int order)
        {
            return new OrderedInteraction
            {
                interactionType = OrderedInteraction.InteractionType.Selection,
                targetInteractable = target,
                orderIndex = order
            };
        }

        public static OrderedInteraction CreateActivationInteraction(InteractableBase target, int order)
        {
            return new OrderedInteraction
            {
                interactionType = OrderedInteraction.InteractionType.Activation,
                targetInteractable = target,
                orderIndex = order
            };
        }

        public static OrderedInteraction CreateCustomInteraction(string eventName, int order)
        {
            return new OrderedInteraction
            {
                interactionType = OrderedInteraction.InteractionType.Custom,
                customEventName = eventName,
                orderIndex = order
            };
        }
    }
} 