using NUnit.Framework;
using Shababeek.Interactions.Core;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Interactions.Tests
{
    [TestFixture]
    public class InteractionZoneVisualizerTests
    {
        private VRInteractionZoneVisualizer _visualizer;

        [SetUp]
        public void SetUp()
        {
            _visualizer = new GameObject("ZoneTest").AddComponent<VRInteractionZoneVisualizer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_visualizer.gameObject);
        }

        [Test]
        public void NewComponent_StartsWithTheDefaultsOfBothModes()
        {
            Assert.AreEqual(2f, _visualizer.RoomScale.playAreaWidth);
            Assert.AreEqual(0.3f, _visualizer.RoomScale.optimalDistance);
            Assert.AreEqual(new Vector2(-60f, 10f), _visualizer.RoomScale.reachHeightPercent);
            Assert.AreEqual(0.5f, _visualizer.Seated.optimalMax);
            Assert.AreEqual(new Vector2(-35f, 15f), _visualizer.Seated.reachHeightPercent);
        }

        [Test]
        public void SwitchingMode_KeepsTheValuesOfBothModes()
        {
            _visualizer.RoomScale.playAreaWidth = 3.5f;
            _visualizer.RoomScale.reachHeightPercent = new Vector2(-50f, 20f);
            _visualizer.Seated.optimalMax = 0.65f;
            _visualizer.Seated.reachHeightPercent = new Vector2(-30f, 5f);

            SetMode(VRMode.Seated);
            SetMode(VRMode.RoomScale);
            SetMode(VRMode.Seated);

            Assert.AreEqual(3.5f, _visualizer.RoomScale.playAreaWidth);
            Assert.AreEqual(new Vector2(-50f, 20f), _visualizer.RoomScale.reachHeightPercent);
            Assert.AreEqual(0.65f, _visualizer.Seated.optimalMax);
            Assert.AreEqual(new Vector2(-30f, 5f), _visualizer.Seated.reachHeightPercent);
        }

        [Test]
        public void ResetToDefaults_OnlyTouchesTheRequestedMode()
        {
            _visualizer.RoomScale.playAreaWidth = 5f;
            _visualizer.Seated.arcAngle = 90f;

            _visualizer.ResetToDefaults(VRMode.RoomScale);

            Assert.AreEqual(2f, _visualizer.RoomScale.playAreaWidth);
            Assert.AreEqual(90f, _visualizer.Seated.arcAngle);
        }

        [Test]
        public void RoomScaleValidate_KeepsDistancesOrdered()
        {
            var settings = new RoomScaleZoneSettings { optimalDistance = 0.5f, extendedDistance = 0.2f, maximumDistance = 0.1f };

            settings.Validate();

            Assert.AreEqual(0.5f, settings.extendedDistance);
            Assert.AreEqual(0.5f, settings.maximumDistance);
        }

        [Test]
        public void SeatedValidate_KeepsDistancesOrdered()
        {
            var settings = new SeatedZoneSettings { optimalMin = 0.6f, optimalMax = 0.4f, extendedDistance = 0.3f, maximumDistance = 0.2f };

            settings.Validate();

            Assert.AreEqual(0.6f, settings.optimalMax);
            Assert.AreEqual(0.6f, settings.extendedDistance);
            Assert.AreEqual(0.6f, settings.maximumDistance);
        }

        [Test]
        public void Validate_KeepsTheOptimalHeightsInsideTheReachableHeights()
        {
            var settings = new SeatedZoneSettings
            {
                reachHeightPercent = new Vector2(-30f, 10f),
                optimalHeightPercent = new Vector2(-80f, 40f)
            };

            settings.Validate();

            Assert.AreEqual(new Vector2(-30f, 10f), settings.optimalHeightPercent);
        }

        [Test]
        public void PercentToMetres_UsesTheRigEyeHeight()
        {
            _visualizer.CameraRig.CameraHeight = 2f;

            Assert.AreEqual(2f, _visualizer.EyeHeight);
            Assert.AreEqual(-0.5f, _visualizer.PercentToMetres(-25f), 0.0001f);
        }

        [Test]
        public void LegacyLayout_SeatedSave_MigratesIntoThePerModeSettings()
        {
            WriteLegacyLayout(VRMode.Seated);

            Assert.AreEqual(3f, _visualizer.RoomScale.playAreaWidth);
            Assert.AreEqual(4f, _visualizer.RoomScale.playAreaDepth);
            Assert.AreEqual(0.25f, _visualizer.RoomScale.optimalDistance);
            Assert.AreEqual(0.35f, _visualizer.Seated.optimalMin);
            Assert.AreEqual(75f, _visualizer.Seated.arcAngle);
            Assert.AreEqual(new Vector2(-42f, 22f), _visualizer.Seated.reachHeightPercent);
            Assert.AreEqual(new Vector2(-21f, 7f), _visualizer.Seated.optimalHeightPercent);
            Assert.AreEqual(new Vector2(-60f, 10f), _visualizer.RoomScale.reachHeightPercent,
                "shared heights belonged to the mode that was active when the data was saved");
        }

        [Test]
        public void LegacyLayout_RoomScaleSave_GivesTheSharedHeightsToRoomScale()
        {
            WriteLegacyLayout(VRMode.RoomScale);

            Assert.AreEqual(new Vector2(-42f, 22f), _visualizer.RoomScale.reachHeightPercent);
            Assert.AreEqual(new Vector2(-35f, 15f), _visualizer.Seated.reachHeightPercent);
        }

        [Test]
        public void LegacyLayout_MigratesOnlyOnce()
        {
            WriteLegacyLayout(VRMode.Seated);
            _visualizer.RoomScale.playAreaWidth = 6f;
            EditorUtility.SetDirty(_visualizer);

            var serialized = new SerializedObject(_visualizer);
            serialized.FindProperty("playerRadius").floatValue = 0.3f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(6f, _visualizer.RoomScale.playAreaWidth);
        }

        private void SetMode(VRMode mode)
        {
            var serialized = new SerializedObject(_visualizer);
            serialized.FindProperty("vrMode").enumValueIndex = (int)mode;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void WriteLegacyLayout(VRMode savedMode)
        {
            var serialized = new SerializedObject(_visualizer);
            serialized.FindProperty("vrMode").enumValueIndex = (int)savedMode;
            serialized.FindProperty("playAreaWidth").floatValue = 3f;
            serialized.FindProperty("playAreaDepth").floatValue = 4f;
            serialized.FindProperty("roomScaleAtEdgeMax").floatValue = 0.25f;
            serialized.FindProperty("roomScaleExtendedMax").floatValue = 0.55f;
            serialized.FindProperty("roomScaleMaximumMax").floatValue = 0.85f;
            serialized.FindProperty("seatedOptimalMin").floatValue = 0.35f;
            serialized.FindProperty("seatedOptimalMax").floatValue = 0.55f;
            serialized.FindProperty("seatedExtendedMax").floatValue = 0.75f;
            serialized.FindProperty("seatedMaximumMax").floatValue = 0.95f;
            serialized.FindProperty("seatedArcAngle").floatValue = 75f;
            serialized.FindProperty("heightMinPercent").floatValue = -42f;
            serialized.FindProperty("heightMaxPercent").floatValue = 22f;
            serialized.FindProperty("optimalHeightMinPercent").floatValue = -21f;
            serialized.FindProperty("optimalHeightMaxPercent").floatValue = 7f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
