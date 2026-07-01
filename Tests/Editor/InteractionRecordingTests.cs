using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    public class InteractionRecordingTests
    {
        private InteractionRecording _recording;

        [SetUp]
        public void SetUp()
        {
            _recording = ScriptableObject.CreateInstance<InteractionRecording>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_recording);
        }

        private static PoseSample Pose(Vector3 position) => new() { position = position, rotation = Quaternion.identity };

        private static FingerSample Fingers(float value) =>
            new() { thumb = value, index = value, middle = value, ring = value, pinky = value };

        [Test]
        public void Duration_ComputedFromSampleCountAndRate()
        {
            var head = new[] { Pose(Vector3.zero), Pose(Vector3.one), Pose(Vector3.up) };
            _recording.SetData(2f, head, new HandRecordingTrack(), new HandRecordingTrack());

            Assert.AreEqual(3, _recording.SampleCount);
            Assert.AreEqual(1f, _recording.Duration, 1e-4f); // (3 - 1) / 2 Hz
        }

        [Test]
        public void EvaluateHead_Midway_LerpsPosition()
        {
            var head = new[] { Pose(Vector3.zero), Pose(new Vector3(0f, 0f, 2f)) };
            _recording.SetData(1f, head, new HandRecordingTrack(), new HandRecordingTrack());

            var pose = _recording.EvaluateHead(0.5f);

            Assert.AreEqual(1f, pose.position.z, 1e-4f);
        }

        [Test]
        public void EvaluateHead_BeyondDuration_ReturnsLastSample()
        {
            var head = new[] { Pose(Vector3.zero), Pose(new Vector3(0f, 0f, 2f)) };
            _recording.SetData(1f, head, new HandRecordingTrack(), new HandRecordingTrack());

            var pose = _recording.EvaluateHead(99f);

            Assert.AreEqual(2f, pose.position.z, 1e-4f);
        }

        [Test]
        public void EvaluateHead_BeforeStart_ReturnsFirstSample()
        {
            var head = new[] { Pose(new Vector3(0f, 0f, 5f)), Pose(new Vector3(0f, 0f, 7f)) };
            _recording.SetData(1f, head, new HandRecordingTrack(), new HandRecordingTrack());

            var pose = _recording.EvaluateHead(-3f);

            Assert.AreEqual(5f, pose.position.z, 1e-4f);
        }

        [Test]
        public void EvaluateFingers_Midway_LerpsCurl()
        {
            var left = new HandRecordingTrack { fingers = new[] { Fingers(0f), Fingers(1f) } };
            _recording.SetData(1f, new[] { Pose(Vector3.zero), Pose(Vector3.zero) }, left, new HandRecordingTrack());

            var fingers = _recording.EvaluateFingers(HandIdentifier.Left, 0.5f);

            Assert.AreEqual(0.5f, fingers.index, 1e-4f);
            Assert.AreEqual(0.5f, fingers[1], 1e-4f);
        }

        [Test]
        public void EvaluatePose_SingleSample_ReturnsThatSample()
        {
            var head = new[] { Pose(new Vector3(3f, 0f, 0f)) };
            _recording.SetData(30f, head, new HandRecordingTrack(), new HandRecordingTrack());

            var pose = _recording.EvaluateHead(10f);

            Assert.AreEqual(3f, pose.position.x, 1e-4f);
            Assert.AreEqual(0f, _recording.Duration, 1e-4f);
        }

        [Test]
        public void EvaluateHead_EmptyRecording_ReturnsIdentity()
        {
            _recording.SetData(30f, null, null, null);

            var pose = _recording.EvaluateHead(0f);

            Assert.AreEqual(Vector3.zero, pose.position);
            Assert.AreEqual(Quaternion.identity, pose.rotation);
        }
    }
}
