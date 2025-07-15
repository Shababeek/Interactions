using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.Interactions
{
    public class MultiSocket : AbstractSocket
    {
        [SerializeField] private Vector3 shift;

        private Socketable _hoverSocketable;
        private readonly List<Transform> _availablePivots = new(20);

        private void Awake()
        {
            _availablePivots.Clear();
            for (var i = 0; i < 10; i++)
            {
                CreateNewPivot(i);
            }
        }

        private void CreateNewPivot(int i)
        {
            var parent = Pivot;
            var attachmentPoint = new GameObject($"Pivot{i}").transform;
            attachmentPoint.parent = parent;
            attachmentPoint.localRotation = Quaternion.identity;
            attachmentPoint.Rotate(90, 0, 0);
            attachmentPoint.localPosition = Vector3.zero;
            _availablePivots.Add(attachmentPoint);
        }

        public override Transform Insert(Socketable socketable)
        {
            var socket = GetSocket();
            socket.transform.position =
                shift + Vector3.ProjectOnPlane(socketable.transform.position, transform.forward);
            base.Insert(socketable);
            return socket;
        }

        public override void Remove(Socketable socketable)
        {
            _availablePivots.Add(socketable.transform.parent);
            base.Remove(socketable);
        }

        public override bool CanSocket()
        {
            return true;
        }

        public override void StartHovering(Socketable socketable)
        {
            _hoverSocketable = socketable;
            base.StartHovering(socketable);
        }

        public override void EndHovering(Socketable socketable)
        {
            _hoverSocketable.StopIndication();
            _hoverSocketable = null;
            base.EndHovering(socketable);
        }

        private Transform GetSocket()
        {
            if (_availablePivots.Count == 0)
            {
                CreateNewPivot(_availablePivots.Capacity);
            }

            var socket = _availablePivots[^1];
            _availablePivots.RemoveAt(_availablePivots.Count - 1);
            return socket;
        }

        private void Update()
        {
            if (_hoverSocketable)
            {
                var position = shift +
                               Vector3.ProjectOnPlane(_hoverSocketable.transform.position, transform.forward);
                 _hoverSocketable.Indicate(position, transform.rotation);
            }
        }
    }
}