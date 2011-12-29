using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GimpBlocks
{
    public class XnaInputState : IInputState
    {
        KeyboardState _previousKeyboardState;
        KeyboardState _currentKeyboardState;
        MouseState _previousMouseState;
        MouseState _currentMouseState;
        TimeSpan _elapsedTime;
        bool _isRelativeMouseMode;
        Point _mouseOrigin;

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return (!_previousKeyboardState.IsKeyDown(key) && _currentKeyboardState.IsKeyDown(key));
        }

        public bool IsLeftMouseButtonDown
        {
            get { return IsDown(_currentMouseState.LeftButton); }
        }

        public bool IsLeftMouseButtonClicked
        {
            get { return !IsDown(_previousMouseState.LeftButton) && IsDown(_currentMouseState.LeftButton); }
        }

        public bool IsRightMouseButtonDown
        {
            get { return IsDown(_currentMouseState.RightButton); }
        }

        public bool IsRightMouseButtonClicked
        {
            get { return !IsDown(_previousMouseState.RightButton) && IsDown(_currentMouseState.RightButton); }
        }

        private bool IsDown(ButtonState buttonState)
        {
            return buttonState == ButtonState.Pressed;
        }

        public int MouseX
        {
            get { return _currentMouseState.X; }
        }

        public int MouseY
        {
            get { return _currentMouseState.Y; }
        }

        public int MouseDeltaX
        {
            get
            {
                if (_isRelativeMouseMode)
                {
                    return (_currentMouseState.X - _mouseOrigin.X);
                }
                else
                {
                    return (_currentMouseState.X - _previousMouseState.X);
                }
            }
        }

        public int MouseDeltaY
        {
            get
            {
                if (_isRelativeMouseMode)
                {
                    return (_currentMouseState.Y - _mouseOrigin.Y);
                }
                else
                {
                    return (_currentMouseState.Y - _previousMouseState.Y);
                }
            }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
        }

        public void Update(TimeSpan elapsedTime, KeyboardState newKeyboardState, MouseState newMouseState)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = newKeyboardState;
            _previousMouseState = _currentMouseState;
            _currentMouseState = newMouseState;
            _elapsedTime = elapsedTime;

            if (_isRelativeMouseMode)
            {
                Mouse.SetPosition(_mouseOrigin.X, _mouseOrigin.Y);
            }
        }

        public void SetAbsoluteMouseMode()
        {
            _isRelativeMouseMode = false;
        }

        public void SetRelativeMouseMode(Point mouseOrigin)
        {
            _isRelativeMouseMode = true;
            _mouseOrigin = mouseOrigin;
            Mouse.SetPosition(_mouseOrigin.X, _mouseOrigin.Y);
        }
    }
}
