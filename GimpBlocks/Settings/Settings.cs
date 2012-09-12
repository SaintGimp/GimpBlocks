using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    // TODO: it's not clear this is needed
    public interface ISettings
    {
        bool ShouldUpdate { get; set; }

        bool ShouldSingleStep { get; set; }

        bool ShouldDrawWireframe { get; set; }

        Vector3 CameraStartingLocation { get; set; }

        Vector3 CameraStartingLookAt { get; set; }

        float CameraMoveSpeedPerSecond { get; set; }

        float CameraMouseLookDamping { get; set; }

        int MaximumQuadNodeLevel { get; set; }

        bool ShowQuadBoundaries { get; set; }

        float FarClippingPlaneDistance { get; set; }

        bool ShouldDrawMeshBoundingBoxes { get; set; }
    }

    public class Settings : ISettings,
                            IListener<ToggleDrawWireframeSetting>,
                            IListener<ToggleUpdateSetting>,
                            IListener<ToggleSingleStepSetting>,
                            IListener<IncreaseCameraSpeed>,
                            IListener<DecreaseCameraSpeed>
    {
        public Settings()
        {
            ShouldUpdate = true;
            ShouldSingleStep = false;
            ShouldDrawWireframe = false;
            CameraStartingLocation = Vector3.Backward * 40 + Vector3.Up * 20;
            CameraStartingLookAt = Vector3.Zero;
            CameraMoveSpeedPerSecond = 10;
            CameraMouseLookDamping = 300f;
            MaximumQuadNodeLevel = 19;
            ShowQuadBoundaries = true;
            FarClippingPlaneDistance = 10000000;
            ShouldDrawMeshBoundingBoxes = false;
        }

        bool _shouldUpdate;
        public bool ShouldUpdate
        {
            get { return _shouldUpdate; }
            set { SetFieldValue(ref _shouldUpdate, value); }
        }

        bool _shouldSingleStep;
        public bool ShouldSingleStep
        {
            get { return _shouldSingleStep; }
            set { SetFieldValue(ref _shouldSingleStep, value); }
        }

        bool _shouldDrawWireframe;
        public bool ShouldDrawWireframe
        {
            get { return _shouldDrawWireframe; }
            set { SetFieldValue(ref _shouldDrawWireframe, value); }
        }

        Vector3 _cameraStartingLocation;
        public Vector3 CameraStartingLocation
        {
            get { return _cameraStartingLocation; }
            set { SetFieldValue(ref _cameraStartingLocation, value); }
        }

        Vector3 _cameraStartingLookAt;
        public Vector3 CameraStartingLookAt
        {
            get { return _cameraStartingLookAt; }
            set { SetFieldValue(ref _cameraStartingLookAt, value); }
        }

        float _cameraMoveSpeedPerSecond;
        public float CameraMoveSpeedPerSecond
        {
            get { return _cameraMoveSpeedPerSecond; }
            set { SetFieldValue(ref _cameraMoveSpeedPerSecond, value); }
        }

        float _cameraMouseLookDamping;
        public float CameraMouseLookDamping
        {
            get { return _cameraMouseLookDamping; }
            set { SetFieldValue(ref _cameraMouseLookDamping, value); }
        }

        int _maximumQuadNodeLevel;
        public int MaximumQuadNodeLevel
        {
            get { return _maximumQuadNodeLevel; }
            set { SetFieldValue(ref _maximumQuadNodeLevel, value); }
        }

        bool _showQuadBoundaries;
        public bool ShowQuadBoundaries
        {
            get { return _showQuadBoundaries; }
            set { SetFieldValue(ref _showQuadBoundaries, value); }
        }

        float _farClippingPlaneDistance;
        public float FarClippingPlaneDistance
        {
            get { return _farClippingPlaneDistance; }
            set { SetFieldValue(ref _farClippingPlaneDistance, value); }
        }

        bool _shouldDrawMeshBoundingBoxes;
        public bool ShouldDrawMeshBoundingBoxes
        {
            get { return _shouldDrawMeshBoundingBoxes; }
            set { SetFieldValue(ref _shouldDrawMeshBoundingBoxes, value); }
        }

        public void Handle(ToggleDrawWireframeSetting message)
        {
            ShouldDrawWireframe = !ShouldDrawWireframe;
        }

        public void Handle(ToggleUpdateSetting message)
        {
            ShouldUpdate = !ShouldUpdate;
        }

        public void Handle(ToggleSingleStepSetting message)
        {
            ShouldSingleStep = !ShouldSingleStep;
        }

        public void Handle(IncreaseCameraSpeed message)
        {
            CameraMoveSpeedPerSecond *= 2;
        }

        public void Handle(DecreaseCameraSpeed message)
        {
            CameraMoveSpeedPerSecond /= 2;
        }

        void SetFieldValue<T>(ref T field, T value)
        {
            if (!field.Equals(value))
            {
                field = value;
                EventAggregator.Instance.SendMessage(new SettingsChanged());
            }
        }
    }
}
