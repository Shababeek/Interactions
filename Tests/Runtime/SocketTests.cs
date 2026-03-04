using NUnit.Framework;
using Shababeek.ReactiveVars;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class SocketTests
    {
        private Socket _socket;
        private Socketable _socketable;

        [SetUp]
        public void SetUp()
        {
            var socketGo = new GameObject("Socket");
            _socket = socketGo.AddComponent<Socket>();

            var socketableGo = new GameObject("Socketable");
            socketableGo.AddComponent<Grabable>();
            socketableGo.AddComponent<VariableTweener>();
            _socketable = socketableGo.AddComponent<Socketable>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_socket != null) TestHelpers.DestroyComponent(_socket);
            if (_socketable != null) TestHelpers.DestroyComponent(_socketable);
        }

        [Test]
        public void Socket_InitiallyCanSocket()
        {
            Assert.IsTrue(_socket.CanSocket());
        }

        [Test]
        public void Socket_PivotDefaultsToOwnTransform()
        {
            Assert.AreEqual(_socket.transform, _socket.Pivot);
        }

        [Test]
        public void Socket_GetPivotForSocketableReturnsPivotTransform()
        {
            var (position, rotation) = _socket.GetPivotForSocketable(_socketable);
            Assert.AreEqual(_socket.Pivot.position, position);
            Assert.AreEqual(_socket.Pivot.rotation, rotation);
        }

        [Test]
        public void Socket_InsertViaSocketableMakesSocketFull()
        {
            _socketable.Insert(_socket);
            Assert.IsFalse(_socket.CanSocket());
        }

        [Test]
        public void Socket_InsertViaSocketableSetsIsSocketed()
        {
            _socketable.Insert(_socket);
            Assert.IsTrue(_socketable.IsSocketed);
        }

        [Test]
        public void Socket_InsertViaSocketableSetsSocketTransform()
        {
            _socketable.Insert(_socket);
            Assert.IsNotNull(_socketable.SocketTransform);
        }

        [Test]
        public void Socket_InsertFiresOnSocketConnected()
        {
            Socketable received = null;
            var disposable = _socket.OnSocketConnected
                .Do(s => received = s)
                .Subscribe();

            _socketable.Insert(_socket);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void Socket_RemoveAfterInsertOpensSlot()
        {
            _socketable.Insert(_socket);
            Assert.IsFalse(_socket.CanSocket());

            _socket.Remove(_socketable);
            Assert.IsTrue(_socket.CanSocket());
        }

        [Test]
        public void Socket_RemoveFiresOnSocketDisconnected()
        {
            Socketable received = null;
            var disposable = _socket.OnSocketDisconnected
                .Do(s => received = s)
                .Subscribe();

            _socketable.Insert(_socket);
            _socket.Remove(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void Socket_CannotInsertWhenFull()
        {
            _socketable.Insert(_socket);

            var socketable2Go = new GameObject("Socketable2");
            socketable2Go.AddComponent<Grabable>();
            socketable2Go.AddComponent<VariableTweener>();
            var socketable2 = socketable2Go.AddComponent<Socketable>();

            var result = socketable2.Insert(_socket);
            Assert.IsFalse(result);

            TestHelpers.DestroyComponent(socketable2);
        }

        [Test]
        public void Socket_StartHoveringFiresEvent()
        {
            Socketable received = null;
            var disposable = _socket.OnHoverStart
                .Do(s => received = s)
                .Subscribe();

            _socket.StartHovering(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void Socket_EndHoveringFiresEvent()
        {
            Socketable received = null;
            var disposable = _socket.OnHoverEnd
                .Do(s => received = s)
                .Subscribe();

            _socket.EndHovering(_socketable);

            Assert.AreEqual(_socketable, received);
            disposable.Dispose();
        }

        [Test]
        public void Socket_MultipleSocketsAreIndependent()
        {
            var socket2Go = new GameObject("Socket2");
            var socket2 = socket2Go.AddComponent<Socket>();

            _socketable.Insert(_socket);

            Assert.IsFalse(_socket.CanSocket());
            Assert.IsTrue(socket2.CanSocket());

            TestHelpers.DestroyComponent(socket2);
        }

        [Test]
        public void Socket_InsertReturnsTrueOnSuccess()
        {
            var result = _socketable.Insert(_socket);
            Assert.IsTrue(result);
        }

        [Test]
        public void Socket_SocketableCurrentSocketSetAfterInsert()
        {
            _socketable.Insert(_socket);
            Assert.AreEqual(_socket, _socketable.CurrentSocket);
        }
    }
}
