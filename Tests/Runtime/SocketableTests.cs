using NUnit.Framework;
using Shababeek.ReactiveVars;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class SocketableTests
    {
        private Socketable _socketable;
        private Socket _socket;

        [SetUp]
        public void SetUp()
        {
            var socketableGo = new GameObject("Socketable");
            socketableGo.AddComponent<Grabable>();
            socketableGo.AddComponent<VariableTweener>();
            _socketable = socketableGo.AddComponent<Socketable>();

            var socketGo = new GameObject("Socket");
            _socket = socketGo.AddComponent<Socket>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_socketable != null) TestHelpers.DestroyComponent(_socketable);
            if (_socket != null) TestHelpers.DestroyComponent(_socket);
        }

        [Test]
        public void Socketable_InitiallyNotSocketed()
        {
            Assert.IsFalse(_socketable.IsSocketed);
        }

        [Test]
        public void Socketable_CurrentSocketInitiallyNull()
        {
            Assert.IsNull(_socketable.CurrentSocket);
        }

        [Test]
        public void Socketable_SocketTransformInitiallyNull()
        {
            Assert.IsNull(_socketable.SocketTransform);
        }

        [Test]
        public void Socketable_IsReturningInitiallyFalse()
        {
            Assert.IsFalse(_socketable.IsReturning);
        }

        [Test]
        public void Socketable_OnSocketedObservableExists()
        {
            Assert.IsNotNull(_socketable.OnSocketedAsObservable);
        }

        [Test]
        public void Socketable_CanInsertIntoSocket()
        {
            var result = _socketable.Insert(_socket);
            Assert.IsTrue(result);
            Assert.IsTrue(_socketable.IsSocketed);
        }

        [Test]
        public void Socketable_InsertUpdateCurrentSocket()
        {
            _socketable.Insert(_socket);
            Assert.AreEqual(_socket, _socketable.CurrentSocket);
        }

        [Test]
        public void Socketable_CannotInsertIntoFullSocket()
        {
            var socketable2Go = new GameObject("Socketable2");
            var grabable2 = socketable2Go.AddComponent<Grabable>();
            var tweener2 = socketable2Go.AddComponent<VariableTweener>();
            var socketable2 = socketable2Go.AddComponent<Socketable>();

            _socketable.Insert(_socket);
            var result = socketable2.Insert(_socket);

            Assert.IsFalse(result);
            TestHelpers.DestroyComponent(socketable2);
        }
        
        [Test]
        public void Socketable_ReturnToOriginalState()
        {
            _socketable.Insert(_socket);
            _socketable.ReturnToOriginalState();

            Assert.IsFalse(_socketable.IsSocketed);
            Assert.IsNull(_socketable.CurrentSocket);
        }

        [Test]
        public void Socketable_CanInsertMultipleTimes()
        {
            var result1 = _socketable.Insert(_socket);
            Assert.IsTrue(result1);

            _socketable.ReturnToOriginalState();

            var result2 = _socketable.Insert(_socket);
            Assert.IsTrue(result2);
        }

        [Test]
        public void Socketable_SocketedFlagUpdated()
        {
            Assert.IsFalse(_socketable.IsSocketed);
            _socketable.Insert(_socket);
            Assert.IsTrue(_socketable.IsSocketed);
        }

        [Test]
        public void Socketable_MultipleSocketInserts()
        {
            var socket1 = new GameObject("Socket1").AddComponent<Socket>();
            var socket2 = new GameObject("Socket2").AddComponent<Socket>();

            _socketable.Insert(socket1);
            Assert.IsTrue(_socketable.IsSocketed);

            _socketable.ReturnToOriginalState();
            _socketable.Insert(socket2);
            Assert.AreEqual(socket2, _socketable.CurrentSocket);

            TestHelpers.DestroyComponent(socket1);
            TestHelpers.DestroyComponent(socket2);
        }

        [Test]
        public void Socketable_SocketTransformUpdatedOnInsert()
        {
            _socketable.Insert(_socket);
            // SocketTransform should be set after insert
            Assert.IsNotNull(_socketable.SocketTransform);
        }
    }
}
