using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GimpBlocks
{
    public interface IInputState
    {
        bool IsKeyDown(Keys key);
        
        bool IsKeyPressed(Keys key);
        
        bool IsLeftMouseButtonDown { get; }

        bool IsLeftMouseButtonClicked { get; }

        bool IsRightMouseButtonDown { get; }

        bool IsRightMouseButtonClicked { get; }

        int MouseX { get; }

        int MouseY { get; }

        int MouseDeltaX { get; }
        
        int MouseDeltaY { get; }
        
        TimeSpan ElapsedTime { get; }

        void Update(TimeSpan elapsedTime, KeyboardState newKeyboardState, MouseState newMouseState);

        void SetAbsoluteMouseMode();

        void SetRelativeMouseMode(Point mouseOrigin);
    }
}
