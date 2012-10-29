using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Machine.Specifications;
using Microsoft.Xna.Framework.Input;

// ReSharper disable CheckNamespace

namespace GimpBlocks.Specifications.Input.XnaInputStateSpecs
{
    [Subject(typeof(XnaInputState))]
    public class when_it_is_updated : XnaInputStateContext
    {
        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_remember_the_elapsed_time = () =>
            inputState.ElapsedTime.ShouldEqual(elapsedTime);
    }

    [Subject(typeof(XnaInputState))]
    public class when_no_keys_are_down : XnaInputStateContext
    {
        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_not_report_that_any_keys_are_down = () =>
            inputState.IsKeyDown(Keys.A).ShouldBeFalse();

        It should_not_report_that_any_keys_are_pressed = () =>
            inputState.IsKeyPressed(Keys.A).ShouldBeFalse();
    }

    [Subject(typeof(XnaInputState))]
    public class when_a_key_is_down_for_the_first_time : XnaInputStateContext
    {
        Establish context = () =>
            currentKeyboardState = CreateKeyboardState(Keys.A);

        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_report_that_the_key_is_down = () =>
            inputState.IsKeyDown(Keys.A).ShouldBeTrue();

        It should_report_that_the_key_is_pressed = () =>
            inputState.IsKeyPressed(Keys.A).ShouldBeTrue();

        It should_not_report_that_other_keys_are_down = () =>
            inputState.IsKeyDown(Keys.B).ShouldBeFalse();

        It should_not_report_that_other_keys_are_pressed = () =>
            inputState.IsKeyPressed(Keys.B).ShouldBeFalse();
    }

    [Subject(typeof(XnaInputState))]
    public class when_a_key_is_down_for_the_second_time : XnaInputStateContext
    {
        Establish context = () =>
        {
            previousKeyboardState = CreateKeyboardState(Keys.A);
            inputState.Update(elapsedTime, previousKeyboardState, previousMouseState);
            currentKeyboardState = CreateKeyboardState(Keys.A);
        };

        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_report_that_the_key_is_down = () =>
            inputState.IsKeyDown(Keys.A).ShouldBeTrue();

        It should_not_report_that_the_key_is_pressed = () =>
            inputState.IsKeyPressed(Keys.A).ShouldBeFalse();

        It should_not_report_that_other_keys_are_down = () =>
            inputState.IsKeyDown(Keys.B).ShouldBeFalse();

        It should_not_report_that_other_keys_are_pressed = () =>
            inputState.IsKeyPressed(Keys.B).ShouldBeFalse();
    }

    [Subject(typeof(XnaInputState))]
    public class when_the_right_mouse_button_is_not_down : XnaInputStateContext
    {
        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_not_report_that_the_right_mouse_button_is_down = () =>
            inputState.IsRightMouseButtonDown.ShouldBeFalse();
    }

    [Subject(typeof(XnaInputState))]
    public class when_the_right_mouse_button_is_down : XnaInputStateContext
    {
        Establish context = () =>
            currentMouseState = CreateMouseState(ButtonState.Pressed);

        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_report_that_the_right_mouse_button_is_down = () =>
            inputState.IsRightMouseButtonDown.ShouldBeTrue();
    }

    [Subject(typeof(XnaInputState))]
    public class when_the_mouse_is_not_moved : XnaInputStateContext
    {
        Because of = () =>
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);

        It should_not_report_that_the_mouse_moved_horizontally = () =>
            inputState.MouseDeltaX.ShouldEqual(0);

        It should_not_report_that_the_mouse_moved_vertically = () =>
            inputState.MouseDeltaY.ShouldEqual(0);
    }

    [Subject(typeof(XnaInputState))]
    public class when_the_mouse_is_moved : XnaInputStateContext
    {
        Because of = () =>
        {
            previousMouseState = CreateMouseState(3, 15);
            inputState.Update(elapsedTime, previousKeyboardState, previousMouseState);
            currentMouseState = CreateMouseState(10, 13);
            inputState.Update(elapsedTime, currentKeyboardState, currentMouseState);
        };

        It should_report_that_the_mouse_moved_horizontally = () =>
            inputState.MouseDeltaX.ShouldEqual(7);

        It should_not_report_that_the_mouse_moved_vertically = () =>
            inputState.MouseDeltaY.ShouldEqual(-2);
    }

    public class XnaInputStateContext
    {
        static public KeyboardState previousKeyboardState;
        static public KeyboardState currentKeyboardState;
        static public MouseState previousMouseState;
        static public MouseState currentMouseState;
        static public TimeSpan elapsedTime;
        public static XnaInputState inputState;

        Establish context = () =>
        {
            previousKeyboardState = new KeyboardState();
            currentKeyboardState = new KeyboardState();
            previousMouseState = new MouseState();
            currentMouseState = new MouseState();
            elapsedTime = new TimeSpan(0, 0, 1);
            inputState = new XnaInputState();
        };

        static public MouseState CreateMouseState(int x, int y)
        {
            return new MouseState(x, y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        }

        static public MouseState CreateMouseState(ButtonState rightMouseButtonState)
        {
            return new MouseState(0, 0, 0, ButtonState.Released, ButtonState.Released, rightMouseButtonState, ButtonState.Released, ButtonState.Released);
        }

        static public KeyboardState CreateKeyboardState(Keys key)
        {
            return new KeyboardState(key);
        }
    }
}
