using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class SwitchTests
    {
        private Switch _switch;
        private GameObject _switchBody;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("SwitchParent");
            _switchBody = new GameObject("SwitchBody");
            _switchBody.transform.SetParent(go.transform);

            _switch = go.AddComponent<Switch>();
            _switch.SwitchBody = _switchBody.transform;
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.DestroyGameObject(_switch.gameObject);
            TestHelpers.DestroyGameObject(_switchBody);
        }

        [Test]
        public void Switch_DefaultState_IsOff()
        {
            Assert.IsFalse(_switch.IsOn);
        }

        [Test]
        public void Switch_SetState_On_TurnsOn()
        {
            _switch.SetState(true);
            Assert.IsTrue(_switch.IsOn);
        }

        [Test]
        public void Switch_SetState_Off_TurnsOff()
        {
            _switch.SetState(true);
            _switch.SetState(false);
            Assert.IsFalse(_switch.IsOn);
        }

        [Test]
        public void Switch_Toggle_FlipsState()
        {
            Assert.IsFalse(_switch.IsOn);
            _switch.Toggle();
            Assert.IsTrue(_switch.IsOn);
            _switch.Toggle();
            Assert.IsFalse(_switch.IsOn);
        }

        [Test]
        public void Switch_SetState_FiresOnStateChanged_WithNewState()
        {
            bool? received = null;
            var disposable = _switch.OnStateChanged.Subscribe(state => received = state);

            _switch.SetState(true);

            Assert.AreEqual(true, received);
            disposable.Dispose();
        }

        [Test]
        public void Switch_Toggle_FiresOnStateChanged()
        {
            int fireCount = 0;
            var disposable = _switch.OnStateChanged.Subscribe(_ => fireCount++);

            _switch.Toggle();
            _switch.Toggle();

            Assert.AreEqual(2, fireCount);
            disposable.Dispose();
        }

        [Test]
        public void Switch_SwitchBody_CanBeAssigned()
        {
            var newBody = new GameObject("NewSwitchBody").transform;
            _switch.SwitchBody = newBody;
            Assert.AreEqual(newBody, _switch.SwitchBody);
            TestHelpers.DestroyGameObject(newBody.gameObject);
        }

        [Test]
        public void Switch_TransitionsBetweenStates()
        {
            _switch.SetState(false);
            Assert.IsFalse(_switch.IsOn);

            _switch.SetState(true);
            Assert.IsTrue(_switch.IsOn);

            _switch.SetState(false);
            Assert.IsFalse(_switch.IsOn);
        }
    }
}
