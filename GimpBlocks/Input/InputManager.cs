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
        readonly Game _game;
        readonly IInputState _inputState;
        readonly IInputMapper _globalMapper;
        readonly IInputMapper _mouseLookMapper;
        readonly IInputMapper _normalMapper;
        bool _mouseLookMode = true;
        Rectangle _clientBounds;

        public InputManager(Game game, IInputState inputState)
        {
            _game = game;
            _inputState = inputState;
            // TODO: fix this
            _globalMapper = ObjectFactory.GetInstance<IInputMapper>();
            _mouseLookMapper = ObjectFactory.GetInstance<IInputMapper>();
            _normalMapper = ObjectFactory.GetInstance<IInputMapper>();

            SetInputBindings();
        }

        public void SetClientBounds(Rectangle clientBounds)
        {
            _clientBounds = clientBounds;
            if (_mouseLookMode)
            {
                EnableMouseLookMode();
            }
        }

        void SetInputBindings()
        {
            _globalMapper.AddKeyDownMessage<MoveForward>(Keys.W);
            _globalMapper.AddKeyDownMessage<MoveBackward>(Keys.S);
            _globalMapper.AddKeyDownMessage<MoveLeft>(Keys.A);
            _globalMapper.AddKeyDownMessage<MoveRight>(Keys.D);
            _globalMapper.AddKeyDownMessage<MoveUp>(Keys.E);
            _globalMapper.AddKeyDownMessage<MoveDown>(Keys.C);
            _globalMapper.AddKeyPressMessage<IncreaseCameraSpeed>(Keys.OemPlus);
            _globalMapper.AddKeyPressMessage<DecreaseCameraSpeed>(Keys.OemMinus);
            _globalMapper.AddKeyPressMessage<ZoomIn>(Keys.OemPeriod);
            _globalMapper.AddKeyPressMessage<ZoomOut>(Keys.OemComma);
            _globalMapper.AddKeyDownMessage<GoToGround>(Keys.Z);

            _globalMapper.AddKeyPressMessage<ToggleDrawWireframeSetting>(Keys.F);
            _globalMapper.AddKeyPressMessage<ToggleUpdateSetting>(Keys.U);
            _globalMapper.AddKeyPressMessage<ToggleSingleStepSetting>(Keys.P);

            _globalMapper.AddKeyPressMessage<GarbageCollect>(Keys.G);

            _globalMapper.AddKeyPressMessage<ToggleInputMode>(Keys.Escape);
            _normalMapper.AddGeneralInputMessage<ToggleInputMode>(inputState => inputState.IsLeftMouseButtonClicked);

            _mouseLookMapper.AddGeneralInputMessage<MouseLook>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            _mouseLookMapper.AddGeneralInputMessage<PlaceBlock>(inputState => inputState.IsRightMouseButtonClicked);
            _mouseLookMapper.AddGeneralInputMessage<DestroyBlock>(inputState => inputState.IsLeftMouseButtonClicked);
        }

        public void HandleInput(GameTime gameTime)
        {
            _inputState.Update(gameTime.ElapsedGameTime, Keyboard.GetState(), Mouse.GetState());

            Debug.WriteLine(gameTime.TotalGameTime + ": " + _inputState.MouseX + ", " + _inputState.MouseY);

            if (_mouseLookMode)
            {
                _mouseLookMapper.HandleInput(_inputState);
                Mouse.SetPosition(_clientBounds.Width / 2, _clientBounds.Height / 2);
            }
            else
            {
                _normalMapper.HandleInput(_inputState);
            }

            // TODO: right now order here is important. The global mapper needs to run after
            // the others because otherwise we can toggle mouselook first then the mouselook handler
            // runs with imputs that aren't correct. A better way to fix this might be to have the toggle
            // mouselook handler queue up an action to be run after all input is handled for this frame,
            // or something like that.

            _globalMapper.HandleInput(_inputState);
        }

        public void Handle(ToggleInputMode message)
        {
            _mouseLookMode = !_mouseLookMode;

            if (_mouseLookMode)
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
            _inputState.SetRelativeMouseMode(new Point(_clientBounds.Width / 2, _clientBounds.Height / 2));
            _game.IsMouseVisible = false;
            Mouse.SetPosition(_clientBounds.Width / 2, _clientBounds.Height / 2);
        }

        void DisableMouseLookMode()
        {
            _inputState.SetAbsoluteMouseMode();
            Mouse.SetPosition(_clientBounds.Width / 2, _clientBounds.Height / 2);
            _game.IsMouseVisible = true;
        }

        public void OnActivated()
        {
            if (_mouseLookMode)
            {
                EnableMouseLookMode();
            }
        }
    }
}
