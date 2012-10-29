using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StructureMap;

namespace GimpBlocks
{
    public class InputManager
        : IListener<ToggleInputMode>
    {
        readonly Game game;
        readonly IInputState inputState;
        readonly InputMapper globalMapper = new InputMapper();
        readonly InputMapper mouseLookMapper = new InputMapper();
        readonly InputMapper normalMapper = new InputMapper();
        bool mouseLookMode = true;
        Rectangle clientBounds;

        public InputManager(Game game, IInputState inputState)
        {
            this.game = game;
            this.inputState = inputState;

            SetInputBindings();
        }

        public void SetClientBounds(Rectangle clientBounds)
        {
            this.clientBounds = clientBounds;
            if (mouseLookMode)
            {
                EnableMouseLookMode();
            }
        }

        void SetInputBindings()
        {
            globalMapper.AddKeyDownMessage<MoveForward>(Keys.W);
            globalMapper.AddKeyDownMessage<MoveBackward>(Keys.S);
            globalMapper.AddKeyDownMessage<MoveLeft>(Keys.A);
            globalMapper.AddKeyDownMessage<MoveRight>(Keys.D);
            globalMapper.AddKeyDownMessage<MoveUp>(Keys.E);
            globalMapper.AddKeyDownMessage<MoveDown>(Keys.C);
            globalMapper.AddKeyPressMessage<IncreaseCameraSpeed>(Keys.OemPlus);
            globalMapper.AddKeyPressMessage<DecreaseCameraSpeed>(Keys.OemMinus);
            globalMapper.AddKeyPressMessage<ZoomIn>(Keys.OemPeriod);
            globalMapper.AddKeyPressMessage<ZoomOut>(Keys.OemComma);
            globalMapper.AddKeyPressMessage<ResetCamera>(Keys.R);
            globalMapper.AddKeyPressMessage<ToggleDrawWireframeSetting>(Keys.F);
            globalMapper.AddKeyPressMessage<ToggleUpdateSetting>(Keys.U);
            globalMapper.AddKeyPressMessage<ToggleSingleStepSetting>(Keys.P);

            globalMapper.AddKeyPressMessage<GarbageCollect>(Keys.G);
            globalMapper.AddKeyPressMessage<ProfileWorldGeneration>(Keys.T);

            globalMapper.AddKeyPressMessage<ToggleInputMode>(Keys.Escape);
            normalMapper.AddGeneralInputMessage<ToggleInputMode>(inputState => inputState.IsLeftMouseButtonClicked);

            mouseLookMapper.AddGeneralInputMessage<MouseLook>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            mouseLookMapper.AddGeneralInputMessage<PlaceBlock>(inputState => inputState.IsRightMouseButtonClicked);
            mouseLookMapper.AddGeneralInputMessage<DestroyBlock>(inputState => inputState.IsLeftMouseButtonClicked);
        }

        public void HandleInput(GameTime gameTime)
        {
            inputState.Update(gameTime.ElapsedGameTime, Keyboard.GetState(), Mouse.GetState());

            if (mouseLookMode)
            {
                mouseLookMapper.HandleInput(inputState);
                Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
            }
            else
            {
                normalMapper.HandleInput(inputState);
            }

            // TODO: right now order here is important. The global mapper needs to run after
            // the others because otherwise we can toggle mouselook first then the mouselook handler
            // runs with imputs that aren't correct. A better way to fix this might be to have the toggle
            // mouselook handler queue up an action to be run after all input is handled for this frame,
            // or something like that.

            globalMapper.HandleInput(inputState);
        }

        public void Handle(ToggleInputMode message)
        {
            mouseLookMode = !mouseLookMode;

            if (mouseLookMode)
            {
                EnableMouseLookMode();
            }
            else
            {
                DisableMouseLookMode();
            }
        }

        void EnableMouseLookMode()
        {
            inputState.SetRelativeMouseMode(new Point(clientBounds.Width / 2, clientBounds.Height / 2));
            game.IsMouseVisible = false;
            Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
        }

        void DisableMouseLookMode()
        {
            inputState.SetAbsoluteMouseMode();
            Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
            game.IsMouseVisible = true;
        }

        public void OnActivated()
        {
            if (mouseLookMode)
            {
                EnableMouseLookMode();
            }
        }
    }
}
