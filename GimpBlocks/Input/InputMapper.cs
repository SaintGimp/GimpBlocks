using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace GimpBlocks
{
    // TODO: It would be interesting to use the Reactive Framework here

    public class InputMapper
    {
        readonly List<KeyEvent> keyPressEvents = new List<KeyEvent>();
        readonly List<KeyEvent> keyDownEvents = new List<KeyEvent>();
        readonly List<InputEvent> generalInputEvents = new List<InputEvent>();

        public void HandleInput(IInputState inputState)
        {
            SendKeyPressMessages(inputState);
            SendKeyDownMessages(inputState);
            SendGeneralInputMessages(inputState);
        }

        private void SendKeyPressMessages(IInputState inputState)
        {
            foreach (var keyEvent in keyPressEvents.Where(keyEvent => inputState.IsKeyPressed(keyEvent.Key)))
            {
                keyEvent.Send(inputState);
            }
        }

        private void SendKeyDownMessages(IInputState inputState)
        {
            foreach (var keyEvent in keyDownEvents.Where(keyEvent => inputState.IsKeyDown(keyEvent.Key)))
            {
                keyEvent.Send(inputState);
            }
        }

        private void SendGeneralInputMessages(IInputState inputState)
        {
            foreach (var inputEvent in generalInputEvents.Where(inputEvent => inputEvent.Filter(inputState)))
            {
                inputEvent.Send(inputState);
            }
        }

        // We put the key/action pairs into a list rather than a dictionary because
        // we want to be able to support multiple actions per key

        public void AddKeyPressMessage<T>(Keys key) where T : InputMessage, new()
        {
            keyPressEvents.Add(new KeyEvent { Key = key, Send = x => EventAggregator.Instance.SendMessage(new T { InputState = x}) });
        }

        public void AddKeyDownMessage<T>(Keys key) where T : InputMessage, new()
        {
            keyDownEvents.Add(new KeyEvent { Key = key, Send = x => EventAggregator.Instance.SendMessage(new T { InputState = x }) });
        }

        public void AddGeneralInputMessage<T>(Func<IInputState, bool> filter) where T : InputMessage, new()
        {
            generalInputEvents.Add(new InputEvent { Filter = filter, Send = x => EventAggregator.Instance.SendMessage(new T { InputState = x }) });
        }

        private class KeyEvent
        {
            public Keys Key;
            public Action<IInputState> Send;
        }

        private class InputEvent
        {
            public Func<IInputState, bool> Filter;
            public Action<IInputState> Send;
        }
    }
}
