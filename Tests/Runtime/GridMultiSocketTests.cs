using NUnit.Framework;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class GridMultiSocketTests
    {
        private GridMultiSocket _gridSocket;
        private Socketable _socketable;

        [SetUp]
        public void SetUp()
        {
            var gridGo = new GameObject("GridSocket");
            _gridSocket = gridGo.AddComponent<GridMultiSocket>();

            _socketable = CreateSocketable("Socketable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_gridSocket != null) TestHelpers.DestroyComponent(_gridSocket);
            if (_socketable != null) TestHelpers.DestroyComponent(_socketable);
        }

        private Socketable CreateSocketable(string name)
        {
            var go = new GameObject(name);
            go.AddComponent<Grabable>();
            go.AddComponent<VariableTweener>();
            return go.AddComponent<Socketable>();
        }

        [Test]
        public void GridMultiSocket_InitiallyCanSocket()
        {
            Assert.IsTrue(_gridSocket.CanSocket());
        }

        [Test]
        public void GridMultiSocket_PivotDefaultsToOwnTransform()
        {
            Assert.AreEqual(_gridSocket.transform, _gridSocket.Pivot);
        }

        [Test]
        public void GridMultiSocket_DefaultGridSizeIs3x3()
        {
            Assert.AreEqual(9, _gridSocket.SocketsCount());
        }

        [Test]
        public void GridMultiSocket_InsertViaSocketableSetsIsSocketed()
        {
            _socketable.Insert(_gridSocket);
            Assert.IsTrue(_socketable.IsSocketed);
        }

        [Test]
        public void GridMultiSocket_InsertViaSocketableSetsSocketTransform()
        {
            _socketable.Insert(_gridSocket);
            Assert.IsNotNull(_socketable.SocketTransform);
        }

        [Test]
        public void GridMultiSocket_InsertReturnsTrueOnSuccess()
        {
            var result = _socketable.Insert(_gridSocket);
            Assert.IsTrue(result);
        }

        [Test]
        public void GridMultiSocket_InsertFiresOnSocketConnected()
        {
            Socketable received = null;
            var disposable = _gridSocket.OnSocketConnected
                .Do(s => received = s)
                .Subscribe();

            _socketable.Insert(_gridSocket);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void GridMultiSocket_MultipleInsertsGetDifferentSlots()
        {
            _socketable.Insert(_gridSocket);
            var socketTransform1 = _socketable.SocketTransform;

            var socketable2 = CreateSocketable("Socketable2");
            socketable2.Insert(_gridSocket);
            var socketTransform2 = socketable2.SocketTransform;

            Assert.AreNotEqual(socketTransform1.position, socketTransform2.position);

            TestHelpers.DestroyComponent(socketable2);
        }

        [Test]
        public void GridMultiSocket_CanSocketReturnsFalseWhenFull()
        {
            var socketCount = _gridSocket.SocketsCount();
            var socketables = new Socketable[socketCount];

            for (int i = 0; i < socketCount; i++)
            {
                socketables[i] = CreateSocketable($"Socketable_{i}");
                socketables[i].Insert(_gridSocket);
            }

            Assert.IsFalse(_gridSocket.CanSocket());

            foreach (var s in socketables)
                TestHelpers.DestroyComponent(s);
        }

        [Test]
        public void GridMultiSocket_CannotInsertWhenFull()
        {
            var socketCount = _gridSocket.SocketsCount();
            var socketables = new Socketable[socketCount];

            for (int i = 0; i < socketCount; i++)
            {
                socketables[i] = CreateSocketable($"Fill_{i}");
                socketables[i].Insert(_gridSocket);
            }

            var extraSocketable = CreateSocketable("Extra");
            var result = extraSocketable.Insert(_gridSocket);
            Assert.IsFalse(result);

            TestHelpers.DestroyComponent(extraSocketable);
            foreach (var s in socketables)
                TestHelpers.DestroyComponent(s);
        }

        [Test]
        public void GridMultiSocket_RemoveOpensSlot()
        {
            var socketCount = _gridSocket.SocketsCount();
            var socketables = new Socketable[socketCount];

            for (int i = 0; i < socketCount; i++)
            {
                socketables[i] = CreateSocketable($"Fill_{i}");
                socketables[i].Insert(_gridSocket);
            }

            Assert.IsFalse(_gridSocket.CanSocket());

            _gridSocket.Remove(socketables[0]);

            Assert.IsTrue(_gridSocket.CanSocket());

            foreach (var s in socketables)
                TestHelpers.DestroyComponent(s);
        }

        [Test]
        public void GridMultiSocket_RemoveFiresOnSocketDisconnected()
        {
            Socketable received = null;
            var disposable = _gridSocket.OnSocketDisconnected
                .Do(s => received = s)
                .Subscribe();

            _socketable.Insert(_gridSocket);
            _gridSocket.Remove(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void GridMultiSocket_FindClosestAvailableSlotReturnsTransform()
        {
            var closestSlot = _gridSocket.FindClosestAvailableSlot(Vector3.zero);
            Assert.IsNotNull(closestSlot);
        }

        [Test]
        public void GridMultiSocket_FindClosestAvailableSlotReturnsNullWhenFull()
        {
            var socketCount = _gridSocket.SocketsCount();
            var socketables = new Socketable[socketCount];

            for (int i = 0; i < socketCount; i++)
            {
                socketables[i] = CreateSocketable($"Fill_{i}");
                socketables[i].Insert(_gridSocket);
            }

            var closestSlot = _gridSocket.FindClosestAvailableSlot(Vector3.zero);
            Assert.IsNull(closestSlot);

            foreach (var s in socketables)
                TestHelpers.DestroyComponent(s);
        }

        [Test]
        public void GridMultiSocket_GetPivotForSocketableReturnsValidPosition()
        {
            var (position, rotation) = _gridSocket.GetPivotForSocketable(_socketable);
            Assert.IsNotNull(position);
            Assert.IsNotNull(rotation);
        }

        [Test]
        public void GridMultiSocket_StartHoveringFiresEvent()
        {
            Socketable received = null;
            var disposable = _gridSocket.OnHoverStart
                .Do(s => received = s)
                .Subscribe();

            _gridSocket.StartHovering(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void GridMultiSocket_EndHoveringFiresEvent()
        {
            Socketable received = null;
            var disposable = _gridSocket.OnHoverEnd
                .Do(s => received = s)
                .Subscribe();

            _gridSocket.EndHovering(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }
    }
}
