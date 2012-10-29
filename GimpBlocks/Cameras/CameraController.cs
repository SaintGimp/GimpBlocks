using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace GimpBlocks
{
    public interface ICameraController
    {
    }

    public class CameraController : ICameraController,
                                    IListener<MoveForward>,
                                    IListener<MoveBackward>,
                                    IListener<MoveLeft>,
                                    IListener<MoveRight>,
                                    IListener<MoveUp>,
                                    IListener<MoveDown>,
                                    IListener<MouseLook>,
                                    IListener<ZoomIn>,
                                    IListener<ZoomOut>,
                                    IListener<ResetCamera>
    {
        readonly ICamera camera;

        public CameraController(ICamera camera)
        {
            this.camera = camera;
        }

        public void Handle(MoveForward message)
        {
            camera.MoveForwardHorizontally(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveBackward message)
        {
            camera.MoveBackwardHorizontally(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveLeft message)
        {
            camera.MoveLeft(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveRight message)
        {
            camera.MoveRight(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveUp message)
        {
            camera.MoveUp(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveDown message)
        {
            camera.MoveDown(Settings.Instance.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MouseLook message)
        {
            if (message.InputState.MouseDeltaX != 0)
            {
                float changeInYaw = -(float)(message.InputState.MouseDeltaX) / Settings.Instance.CameraMouseLookDamping;
                camera.ChangeYaw(changeInYaw);
            }

            if (message.InputState.MouseDeltaY != 0)
            {
                float changeInPitch = -(float)(message.InputState.MouseDeltaY) / Settings.Instance.CameraMouseLookDamping;
                camera.ChangePitch(changeInPitch);
            }
        }

        public void Handle(ZoomIn message)
        {
            camera.ZoomLevel *= 1.5f;
        }

        public void Handle(ZoomOut message)
        {
            camera.ZoomLevel /= 1.5f;
        }

        public void Handle(ResetCamera message)
        {
            camera.Reset();
        }
    }
}
