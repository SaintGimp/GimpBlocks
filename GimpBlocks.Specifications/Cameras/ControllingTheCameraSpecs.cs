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
            controller.Handle(new MoveForward { InputState = input });

        It should_move_the_camera_forward_the_correct_distance = () =>
            camera.Received().MoveForwardHorizontally(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_backward : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MoveBackward { InputState = input });

        It should_move_the_camera_backward_the_correct_distance = () =>
            camera.Received().MoveBackwardHorizontally(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_left : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MoveLeft { InputState = input });

        It should_move_the_camera_left_the_correct_distance = () =>
            camera.Received().MoveLeft(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_right : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MoveRight() { InputState = input });

        It should_move_the_camera_right_the_correct_distance = () =>
            camera.Received().MoveRight(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_up : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MoveUp { InputState = input });

        It should_move_the_camera_up_the_correct_distance = () =>
            camera.Received().MoveUp(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_move_down : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MoveDown { InputState = input });

        It should_move_the_camera_down_the_correct_distance = () =>
            camera.Received().MoveDown(expectedDistance);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_look_around : ControllingTheCameraContext
    {
        Because of = () =>
            controller.Handle(new MouseLook { InputState = input });

        It should_pitch_the_camera = () =>
            camera.Received().ChangePitch(expectedPitch);

        It should_yaw_the_camera = () =>
        camera.Received().ChangeYaw(expectedYaw);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_zoom_in : ControllingTheCameraContext
    {
        Establish context = () =>
            camera.ZoomLevel = 3f;

        Because of = () =>
            controller.Handle(new ZoomIn());

        It should_increase_the_zoom_level_of_the_camera = () =>
            camera.ZoomLevel.ShouldBeGreaterThan(3f);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_should_zoom_out : ControllingTheCameraContext
    {
        Establish context = () =>
            camera.ZoomLevel = 3f;

        Because of = () =>
            controller.Handle(new ZoomOut());

        It should_decrease_the_zoom_level_of_the_camera = () =>
            camera.ZoomLevel.ShouldBeLessThan(3f);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_speed_is_increased : ControllingTheCameraContext
    {
        Establish context = () =>
            Settings.Instance.Handle(new IncreaseCameraSpeed());

        Because of = () =>
            controller.Handle(new MoveForward { InputState = input });

        It should_move_the_camera_more_quickly = () =>
            camera.Received().MoveForwardHorizontally(expectedDistance * 2);
    }

    [Subject("Controlling the camera")]
    public class when_the_camera_speed_is_decreased : ControllingTheCameraContext
    {
        Establish context = () =>
            Settings.Instance.Handle(new DecreaseCameraSpeed());

        Because of = () =>
            controller.Handle(new MoveForward { InputState = input });

        It should_move_the_camera_more_quickly = () =>
            camera.Received().MoveForwardHorizontally(expectedDistance / 2);
    }

    public class ControllingTheCameraContext
    {
        public static ICamera camera;

        public static IInputState input;
        public static float expectedDistance;
        public static float expectedPitch;
        public static float expectedYaw;
        public static CameraController controller;

        Establish context = () =>
        {
            camera = Substitute.For<ICamera>();

            Settings.Instance.CameraMoveSpeedPerSecond = 10;
            Settings.Instance.CameraMouseLookDamping = 300f;

            input = Substitute.For<IInputState>();
            input.ElapsedTime.Returns(new TimeSpan(0, 0, 0, 0, 500));
            input.MouseDeltaX.Returns(100);
            input.MouseDeltaY.Returns(150);

            expectedDistance = 10 / 2;
            expectedYaw = -100 / 300.0f;
            expectedPitch = -150 / 300.0f;

            controller = new CameraController(camera);
        };
    }
}
