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
                                    IListener<ZoomOut>
    {
        readonly ICamera _camera;
        readonly ISettings _settings;

        public CameraController(ICamera camera, ISettings settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Handle(MoveForward message)
        {
            _camera.MoveForwardHorizontally(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveBackward message)
        {
            _camera.MoveBackwardHorizontally(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveLeft message)
        {
            _camera.MoveLeft(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveRight message)
        {
            _camera.MoveRight(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveUp message)
        {
            _camera.MoveUp(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MoveDown message)
        {
            _camera.MoveDown(_settings.CameraMoveSpeedPerSecond * (float)message.InputState.ElapsedTime.TotalSeconds);
        }

        public void Handle(MouseLook message)
        {
            if (message.InputState.MouseDeltaX != 0)
            {
                float changeInYaw = -(float)(message.InputState.MouseDeltaX) / _settings.CameraMouseLookDamping;
                _camera.ChangeYaw(changeInYaw);
            }

            if (message.InputState.MouseDeltaY != 0)
            {
                float changeInPitch = -(float)(message.InputState.MouseDeltaY) / _settings.CameraMouseLookDamping;
                _camera.ChangePitch(changeInPitch);
            }
        }

        public void Handle(ZoomIn message)
        {
            _camera.ZoomLevel *= 1.5f;
        }

        public void Handle(ZoomOut message)
        {
            _camera.ZoomLevel /= 1.5f;
        }
    }
}
