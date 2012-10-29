using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GimpBlocks
{
    public class XnaInputState : IInputState
    {
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState previousMouseState;
        MouseState currentMouseState;
        TimeSpan elapsedTime;
        bool isRelativeMouseMode;
        Point mouseOrigin;

        public bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return (!previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key));
        }

        public bool IsLeftMouseButtonDown
        {
            get { return IsDown(currentMouseState.LeftButton); }
        }

        public bool IsLeftMouseButtonClicked
        {
            get { return !IsDown(previousMouseState.LeftButton) && IsDown(currentMouseState.LeftButton); }
        }

        public bool IsRightMouseButtonDown
        {
            get { return IsDown(currentMouseState.RightButton); }
        }

        public bool IsRightMouseButtonClicked
        {
            get { return !IsDown(previousMouseState.RightButton) && IsDown(currentMouseState.RightButton); }
        }

        private bool IsDown(ButtonState buttonState)
        {
            return buttonState == ButtonState.Pressed;
        }

        public int MouseX
        {
            get { return currentMouseState.X; }
        }

        public int MouseY
        {
            get { return currentMouseState.Y; }
        }

        public int MouseDeltaX
        {
            get
            {
                if (isRelativeMouseMode)
                {
                    return (currentMouseState.X - mouseOrigin.X);
                }
                else
                {
                    return (currentMouseState.X - previousMouseState.X);
                }
            }
        }

        public int MouseDeltaY
        {
            get
            {
                if (isRelativeMouseMode)
                {
                    return (currentMouseState.Y - mouseOrigin.Y);
                }
                else
                {
                    return (currentMouseState.Y - previousMouseState.Y);
                }
            }
        }

        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
        }

        public void Update(TimeSpan elapsedTime, KeyboardState newKeyboardState, MouseState newMouseState)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = newKeyboardState;
            previousMouseState = currentMouseState;
            currentMouseState = newMouseState;
            this.elapsedTime = elapsedTime;
        }

        public void SetAbsoluteMouseMode()
        {
            isRelativeMouseMode = false;
        }

        public void SetRelativeMouseMode(Point mouseOrigin)
        {
            isRelativeMouseMode = true;
            this.mouseOrigin = mouseOrigin;
        }
    }
}
