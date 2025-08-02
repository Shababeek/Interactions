using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Shababeek.Core
{
    /// <summary>
    /// Listens to game events and raises Unity events accordingly.
    /// Provides a bridge between the ScriptableObject-based GameEvent system and Unity's event system.
    /// </summary>
    /// <remarks>
    /// This component allows you to respond to GameEvent triggers by raising Unity events
    /// that can be connected to other systems in the scene. It supports multiple GameEvents
    /// with their associated Unity events.
    /// </remarks>
    [AddComponentMenu("Shababeek/Scriptable System/Game Event Listener")]
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("List of game events to listen to with their associated Unity events.")]
        [SerializeField] internal List<GameEventWithEvents> gameEventList;
        
        private CompositeDisposable _disposable;

        /// <summary>
        /// Gets or sets the list of game events with their associated events.
        /// </summary>
        /// <value>The list of GameEventWithEvents containing game events and their Unity events.</value>
        public List<GameEventWithEvents> GameEventList
        {
            get => gameEventList;
            set => gameEventList = value;
        }

        /// <summary>
        /// Adds a new game event to the listener with default event settings.
        /// </summary>
        /// <param name="gameEvent">The game event to add to the listener.</param>
        /// <remarks>
        /// This method creates a new GameEventWithEvents with the provided game event and adds it to the list.
        /// The game event will be automatically subscribed to when the component is enabled.
        /// </remarks>
        public void AddGameEvent(GameEvent gameEvent)
        {
            var gwe = new GameEventWithEvents();
            gwe.gameEvent = gameEvent;
            gameEventList ??= new List<GameEventWithEvents>();
            gameEventList.Add(gwe);
        }

        private void OnEnable()
        {
            _disposable = new CompositeDisposable();
            foreach (var gameEventWithEvents in gameEventList)
            {
                if (gameEventWithEvents.gameEvent != null)
                {
                    gameEventWithEvents.gameEvent.OnRaised
                        .Do(_ => OnGameEventRaised(gameEventWithEvents))
                        .Subscribe().AddTo(_disposable);
                }
            }
        }

        private void OnDisable()
        {
            _disposable?.Dispose();
        }

        private void OnGameEventRaised(GameEventWithEvents gameEventWithEvents)
        {
            gameEventWithEvents.onGameEventRaised?.Invoke();
        }

        /// <summary>
        /// Container class that associates a game event with Unity events.
        /// </summary>
        [Serializable]
        public class GameEventWithEvents
        {
            [Tooltip("The game event to listen to for triggers.")]
            public GameEvent gameEvent;
            
            [Tooltip("Unity event raised when the game event is triggered.")]
            public UnityEvent onGameEventRaised;
        }
    }
}