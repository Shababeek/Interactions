using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shababeek.Interactions.Core
{
    /// <summary>
    /// Composite hand input provider that manages multiple input providers with event-based switching.
    /// Automatically switches between controllers and hand tracking based on device availability.
    /// Controllers have priority over hand tracking (standard VR behavior).
    /// </summary>
    [AddComponentMenu("Shababeek/Interactions/Input Providers/Composite Input Provider")]
    public class CompositeHandInputProvider : MonoBehaviour, IHandInputProvider
    {
        [Header("Input Providers")]
        [Tooltip("List of input providers. Controllers should be listed before hand tracking for proper priority.")]
        [SerializeField] private List<HandInputProviderBase> providers = new();
        
        [Header("Debug")]
        [Tooltip("Log when active provider changes.")]
        [SerializeField] private bool debugLog = false;
        
        private IHandInputProvider currentProvider;
        private bool isInitialized = false;
        
        /// <summary>
        /// Observable for trigger button state changes.
        /// </summary>
        public IObservable<VRButtonState> TriggerObservable => currentProvider?.TriggerObservable;
        
        /// <summary>
        /// Observable for grip button state changes.
        /// </summary>
        public IObservable<VRButtonState> GripObservable => currentProvider?.GripObservable;
        
        /// <summary>
        /// Observable for A button state changes.
        /// </summary>
        public IObservable<VRButtonState> AButtonObservable => currentProvider?.AButtonObservable;
        
        /// <summary>
        /// Observable for B button state changes.
        /// </summary>
        public IObservable<VRButtonState> BButtonObservable => currentProvider?.BButtonObservable;
        
        /// <summary>
        /// Gets finger curl value by index.
        /// </summary>
        public float this[int fingerIndex] => currentProvider?[fingerIndex] ?? 0f;
        
        /// <summary>
        /// Gets finger curl value by finger name.
        /// </summary>
        public float this[FingerName finger] => currentProvider?[finger] ?? 0f;
        
        /// <summary>
        /// Checks if any provider is currently active.
        /// </summary>
        public bool IsActive => currentProvider?.IsActive ?? false;
        
        /// <summary>
        /// Priority of the currently active provider.
        /// </summary>
        public int Priority => currentProvider?.Priority ?? 0;
        
        /// <summary>
        /// Currently active input provider.
        /// </summary>
        public IHandInputProvider CurrentProvider => currentProvider;
        
        void Start()
        {
            InitializeProviders();
        }
        
        void OnDestroy()
        {
            CleanupProviders();
        }
        
        /// <summary>
        /// Initializes all providers and subscribes to their activation events.
        /// </summary>
        private void InitializeProviders()
        {
            if (isInitialized)
                return;
            
            // Sort by priority (highest first) - controllers before hand tracking
            providers = providers.OrderByDescending(p => p.Priority).ToList();
            
            // Subscribe to activation/deactivation events
            foreach (var provider in providers)
            {
                provider.OnProviderActivated += () => OnProviderActivated(provider);
                provider.OnProviderDeactivated += () => OnProviderDeactivated(provider);
            }
            
            // Find initially active provider
            SelectBestProvider();
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        private void CleanupProviders()
        {
            foreach (var provider in providers)
            {
                provider.OnProviderActivated -= () => OnProviderActivated(provider);
                provider.OnProviderDeactivated -= () => OnProviderDeactivated(provider);
            }
        }
        
        /// <summary>
        /// Called when a provider becomes active.
        /// </summary>
        private void OnProviderActivated(HandInputProviderBase provider)
        {
            if (debugLog)
            {
                Debug.Log($"[CompositeInput] Provider activated: {provider.GetType().Name} (Priority: {provider.Priority})");
            }
            
            // If this provider has higher priority than current, switch to it
            if (currentProvider == null || provider.Priority > currentProvider.Priority)
            {
                SwitchToProvider(provider);
            }
        }
        
        /// <summary>
        /// Called when a provider becomes inactive.
        /// </summary>
        private void OnProviderDeactivated(HandInputProviderBase provider)
        {
            if (debugLog)
            {
                Debug.Log($"[CompositeInput] Provider deactivated: {provider.GetType().Name}");
            }
            
            // If the current provider became inactive, find the next best one
            if (currentProvider == provider)
            {
                SelectBestProvider();
            }
        }
        
        /// <summary>
        /// Switches to a specific provider.
        /// </summary>
        private void SwitchToProvider(IHandInputProvider provider)
        {
            if (currentProvider == provider)
                return;
            
            currentProvider = provider;
            
            if (debugLog)
            {
                string providerName = (provider as MonoBehaviour)?.GetType().Name ?? "Unknown";
                Debug.Log($"[CompositeInput] ✓ Switched to: {providerName} (Priority: {provider.Priority})");
            }
        }
        
        /// <summary>
        /// Selects the best available provider based on priority and active state.
        /// </summary>
        private void SelectBestProvider()
        {
            // Find highest priority active provider
            var bestProvider = providers
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Priority)
                .FirstOrDefault();
            
            if (bestProvider != null)
            {
                SwitchToProvider(bestProvider);
            }
            else
            {
                if (debugLog && currentProvider != null)
                {
                    Debug.Log("[CompositeInput] No active providers available");
                }
                currentProvider = null;
            }
        }
        
        /// <summary>
        /// Adds an input provider to the composite.
        /// </summary>
        public void AddProvider(HandInputProviderBase provider)
        {
            if (provider == null || providers.Contains(provider))
                return;
            
            providers.Add(provider);
            providers = providers.OrderByDescending(p => p.Priority).ToList();
            
            // Subscribe to events if already initialized
            if (isInitialized)
            {
                provider.OnProviderActivated += () => OnProviderActivated(provider);
                provider.OnProviderDeactivated += () => OnProviderDeactivated(provider);
                
                // Check if this new provider should become active
                if (provider.IsActive && (currentProvider == null || provider.Priority > currentProvider.Priority))
                {
                    SwitchToProvider(provider);
                }
            }
        }
        
        /// <summary>
        /// Removes an input provider from the composite.
        /// </summary>
        public void RemoveProvider(HandInputProviderBase provider)
        {
            if (!providers.Contains(provider))
                return;
            
            // Unsubscribe from events
            if (isInitialized)
            {
                provider.OnProviderActivated -= () => OnProviderActivated(provider);
                provider.OnProviderDeactivated -= () => OnProviderDeactivated(provider);
            }
            
            providers.Remove(provider);
            
            // If we removed the current provider, find a new one
            if (currentProvider == provider)
            {
                SelectBestProvider();
            }
        }
        
        /// <summary>
        /// Gets all available providers.
        /// </summary>
        public IReadOnlyList<HandInputProviderBase> GetProviders() => providers.AsReadOnly();
    }
}