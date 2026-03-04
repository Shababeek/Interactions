using NUnit.Framework;
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
        public void Switch_InitialStateIsNeutral()
        {
            _switch.StartingPosition = StartingPosition.Neutral;
            // Trigger Start to initialize
            _switch.gameObject.SetActive(true);
            var state = _switch.GetSwitchState();
            Assert.AreEqual(null, state);
        }

        [Test]
        public void Switch_CanSetPositionToOn()
        {
            _switch.SetPosition(StartingPosition.On);
            var state = _switch.GetSwitchState();
            Assert.AreEqual(true, state);
        }

        [Test]
        public void Switch_CanSetPositionToOff()
        {
            _switch.SetPosition(StartingPosition.Off);
            var state = _switch.GetSwitchState();
            Assert.AreEqual(false, state);
        }

        [Test]
        public void Switch_CanSetPositionToNeutral()
        {
            _switch.SetPosition(StartingPosition.Neutral);
            var state = _switch.GetSwitchState();
            Assert.AreEqual(null, state);
        }

        [Test]
        public void Switch_StayInPosition_PreventsPivotReturn()
        {
            _switch.StayInPosition = true;
            _switch.SetPosition(StartingPosition.On);
            Assert.IsTrue(_switch.StayInPosition);
        }

        [Test]
        public void Switch_ResetSwitch_ResetsToNeutral_WhenNotStaying()
        {
            _switch.StayInPosition = false;
            _switch.SetPosition(StartingPosition.On);
            _switch.ResetSwitch();

            var state = _switch.GetSwitchState();
            Assert.AreEqual(null, state);
        }



        [Test]
        public void Switch_ForceResetSwitch_AlwaysResetsToNeutral()
        {
            _switch.StayInPosition = true;
            _switch.SetPosition(StartingPosition.On);
            _switch.ForceResetSwitch();

            var state = _switch.GetSwitchState();
            Assert.AreEqual(null, state);
        }

        [Test]
        public void Switch_GetCurrentRotation_ReturnsVector3()
        {
            _switch.SetPosition(StartingPosition.On);
            var rotation = _switch.GetCurrentRotation();
            Assert.IsNotNull(rotation);
        }

        [Test]
        public void Switch_SwitchBodyCanBeAssigned()
        {
            var newBody = new GameObject("NewSwitchBody").transform;
            _switch.SwitchBody = newBody;
            Assert.AreEqual(newBody, _switch.SwitchBody);
            TestHelpers.DestroyGameObject(newBody.gameObject);
        }

        [Test]
        [TestCase(StartingPosition.Off)]
        [TestCase(StartingPosition.Neutral)]
        [TestCase(StartingPosition.On)]
        public void Switch_CanSetMultiplePositions(StartingPosition position)
        {
            _switch.SetPosition(position);
            var state = _switch.GetSwitchState();

            switch (position)
            {
                case StartingPosition.Off:
                    Assert.AreEqual(false, state);
                    break;
                case StartingPosition.On:
                    Assert.AreEqual(true, state);
                    break;
                case StartingPosition.Neutral:
                    Assert.AreEqual(null, state);
                    break;
            }
        }

        [Test]
        public void Switch_TransitionsBetweenStates()
        {
            _switch.SetPosition(StartingPosition.Off);
            var offState = _switch.GetSwitchState();
            Assert.AreEqual(false, offState);

            _switch.SetPosition(StartingPosition.On);
            var onState = _switch.GetSwitchState();
            Assert.AreEqual(true, onState);

            _switch.SetPosition(StartingPosition.Neutral);
            var neutralState = _switch.GetSwitchState();
            Assert.AreEqual(null, neutralState);
        }

        [Test]
        public void Switch_StartingPositionCanBeSet()
        {
            _switch.StartingPosition = StartingPosition.On;
            Assert.AreEqual(StartingPosition.On, _switch.StartingPosition);
        }

        [Test]
        public void Switch_MultipleResets()
        {
            _switch.SetPosition(StartingPosition.On);
            _switch.ResetSwitch();
            var state1 = _switch.GetSwitchState();

            _switch.SetPosition(StartingPosition.Off);
            _switch.ResetSwitch();
            var state2 = _switch.GetSwitchState();

            Assert.AreEqual(null, state1);
            Assert.AreEqual(null, state2);
        }
    }
}
