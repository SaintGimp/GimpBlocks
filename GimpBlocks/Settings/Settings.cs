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
            CameraStartingLocation = Vector3.Up * 110 + Vector3.Right * 10 + Vector3.Backward * 10;
            CameraStartingLookAt = Vector3.Up * 30 + Vector3.Right * 128 + Vector3.Backward * 128;
            CameraMoveSpeedPerSecond = 10;
            CameraMouseLookDamping = 300f;
            FarClippingPlaneDistance = 10000000;
        }

        bool shouldUpdate;
        public bool ShouldUpdate
        {
            get { return shouldUpdate; }
            set { SetFieldValue(ref shouldUpdate, value); }
        }

        bool shouldSingleStep;
        public bool ShouldSingleStep
        {
            get { return shouldSingleStep; }
            set { SetFieldValue(ref shouldSingleStep, value); }
        }

        bool shouldDrawWireframe;
        public bool ShouldDrawWireframe
        {
            get { return shouldDrawWireframe; }
            set { SetFieldValue(ref shouldDrawWireframe, value); }
        }

        Vector3 cameraStartingLocation;
        public Vector3 CameraStartingLocation
        {
            get { return cameraStartingLocation; }
            set { SetFieldValue(ref cameraStartingLocation, value); }
        }

        Vector3 cameraStartingLookAt;
        public Vector3 CameraStartingLookAt
        {
            get { return cameraStartingLookAt; }
            set { SetFieldValue(ref cameraStartingLookAt, value); }
        }

        float cameraMoveSpeedPerSecond;
        public float CameraMoveSpeedPerSecond
        {
            get { return cameraMoveSpeedPerSecond; }
            set { SetFieldValue(ref cameraMoveSpeedPerSecond, value); }
        }

        float cameraMouseLookDamping;
        public float CameraMouseLookDamping
        {
            get { return cameraMouseLookDamping; }
            set { SetFieldValue(ref cameraMouseLookDamping, value); }
        }

        float farClippingPlaneDistance;
        public float FarClippingPlaneDistance
        {
            get { return farClippingPlaneDistance; }
            set { SetFieldValue(ref farClippingPlaneDistance, value); }
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
