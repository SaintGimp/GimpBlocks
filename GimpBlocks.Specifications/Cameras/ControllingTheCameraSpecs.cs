using System;
using System.Text;
using System.Collections.Generic;

using NSubstitute;
using Machine.Specifications;

// ReSharper disable CheckNamespace

namespace GimpBlocks.Specifications.Cameras.ControllingTheCamera
{
    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_forward : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveForward { InputState = _input });

        It should_move_the_camera_forward_the_correct_distance = () =>
            _camera.Received().MoveForwardHorizontally(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_backward : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveBackward { InputState = _input });

        It should_move_the_camera_backward_the_correct_distance = () =>
            _camera.Received().MoveBackwardHorizontally(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_left : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveLeft { InputState = _input });

        It should_move_the_camera_left_the_correct_distance = () =>
            _camera.Received().MoveLeft(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_right : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveRight() { InputState = _input });

        It should_move_the_camera_right_the_correct_distance = () =>
            _camera.Received().MoveRight(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_up : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveUp { InputState = _input });

        It should_move_the_camera_up_the_correct_distance = () =>
            _camera.Received().MoveUp(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_down : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MoveDown { InputState = _input });

        It should_move_the_camera_down_the_correct_distance = () =>
            _camera.Received().MoveDown(_expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_look_around : ControllingTheCameraContext
    {
        Because of = () =>
            _controller.Handle(new MouseLook { InputState = _input });

        It should_pitch_the_camera = () =>
            _camera.Received().ChangePitch(_expectedPitch);

        It should_yaw_the_camera = () =>
        _camera.Received().ChangeYaw(_expectedYaw);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_zoom_in : ControllingTheCameraContext
    {
        Establish context = () =>
            _camera.ZoomLevel = 3f;

        Because of = () =>
            _controller.Handle(new ZoomIn());

        It should_increase_the_zoom_level_of_the_camera = () =>
            _camera.ZoomLevel.ShouldBeGreaterThan(3f);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_zoom_out : ControllingTheCameraContext
    {
        Establish context = () =>
            _camera.ZoomLevel = 3f;

        Because of = () =>
            _controller.Handle(new ZoomOut());

        It should_decrease_the_zoom_level_of_the_camera = () =>
            _camera.ZoomLevel.ShouldBeLessThan(3f);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_speed_is_increased : ControllingTheCameraContext
    {
        Establish context = () =>
            _settings.Handle(new IncreaseCameraSpeed());

        Because of = () =>
            _controller.Handle(new MoveForward { InputState = _input });

        It should_move_the_camera_more_quickly = () =>
            _camera.Received().MoveForwardHorizontally(_expectedDistance * 2);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_speed_is_decreased : ControllingTheCameraContext
    {
        Establish context = () =>
            _settings.Handle(new DecreaseCameraSpeed());

        Because of = () =>
            _controller.Handle(new MoveForward { InputState = _input });

        It should_move_the_camera_more_quickly = () =>
            _camera.Received().MoveForwardHorizontally(_expectedDistance / 2);
    }

    public class ControllingTheCameraContext
    {
        public static ICamera _camera;

        public static Settings _settings;
        
        public static IInputState _input;
        public static float _expectedDistance;
        public static float _expectedPitch;
        public static float _expectedYaw;
        public static CameraController _controller;

        Establish context = () =>
        {
            _camera = Substitute.For<ICamera>();

            _settings = new Settings();
            _settings.CameraMoveSpeedPerSecond = 10;
            _settings.CameraMouseLookDamping = 300f;

            _input = Substitute.For<IInputState>();
            _input.ElapsedTime.Returns(new TimeSpan(0, 0, 0, 0, 500));
            _input.MouseDeltaX.Returns(100);
            _input.MouseDeltaY.Returns(150);

            _expectedDistance = 10 / 2;
            _expectedYaw = -100 / 300.0f;
            _expectedPitch = -150 / 300.0f;

            _controller = new CameraController(_camera, _settings);
        };
    }
}
