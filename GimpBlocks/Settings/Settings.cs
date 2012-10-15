using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GimpBlocks
{
    public class Settings : IListener<ToggleDrawWireframeSetting>,
                            IListener<ToggleUpdateSetting>,
                            IListener<ToggleSingleStepSetting>,
                            IListener<IncreaseCameraSpeed>,
                            IListener<DecreaseCameraSpeed>
    {
        static Settings()
        {
            Instance = new Settings();
        }

        public static Settings Instance { get; private set; }

        public Settings()
        {
            EventAggregator.Instance.AddListener(this);

            ShouldUpdate = true;
            ShouldSingleStep = false;
            ShouldDrawWireframe = false;
            CameraStartingLocation = Vector3.Up * 64;
            CameraStartingLookAt = Vector3.Right * 128 + Vector3.Backward * 128;
            CameraMoveSpeedPerSecond = 10;
            CameraMouseLookDamping = 300f;
            FarClippingPlaneDistance = 10000000;
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

        float _farClippingPlaneDistance;
        public float FarClippingPlaneDistance
        {
            get { return _farClippingPlaneDistance; }
            set { SetFieldValue(ref _farClippingPlaneDistance, value); }
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
