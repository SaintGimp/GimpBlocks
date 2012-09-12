using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using Microsoft.Xna.Framework.Input;
using NSubstitute;

// ReSharper disable CheckNamespace

namespace GimpBlocks.Specifications.Input.InputMapperSpecs
{
    [Subject(typeof(InputMapper))]
    public class when_a_mapped_key_is_pressed : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            _input.IsKeyPressed(Keys.K).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_send_a_keypress_message = () =>
            _eventListener.ShouldReceive<DoSomething>();

        It should_send_a_message_containing_the_current_input_state = () =>
            _eventListener.Message<DoSomething>().InputState.ShouldEqual(_input);
    }

    [Subject(typeof(InputMapper))]
    public class when_a_multimapped_key_is_pressed : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            _inputMapper.AddKeyPressMessage<DoSomethingElse>(Keys.K);
            _input.IsKeyPressed(Keys.K).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_send_the_first_keypress_message = () =>
            _eventListener.ShouldReceive<DoSomething>();

        It should_send_the_second_keypress_message = () =>
            _eventListener.ShouldReceive<DoSomethingElse>();
    }

    [Subject(typeof(InputMapper))]
    public class when_an_unmapped_key_is_pressed : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            _input.IsKeyPressed(Keys.J).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_not_send_a_keypress_message = () =>
            _eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_no_keys_are_pressed : InputMapperContext
    {
        Establish context = () =>
            _inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_not_send_a_keypress_message = () =>
            _eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_a_mapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            _input.IsKeyDown(Keys.K).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_send_a_keydown_message = () =>
            _eventListener.ShouldReceive<DoSomething>();

        It should_send_a_message_containing_the_current_input_state = () =>
            _eventListener.Message<DoSomething>().InputState.ShouldEqual(_input);
    }

    [Subject(typeof(InputMapper))]
    public class when_a_multimapped_mapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            _inputMapper.AddKeyDownMessage<DoSomethingElse>(Keys.K);
            _input.IsKeyDown(Keys.K).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_send_the_first_keydown_message = () =>
            _eventListener.ShouldReceive<DoSomething>();

        It should_send_the_second_keydown_message = () =>
            _eventListener.ShouldReceive<DoSomethingElse>();
    }

    [Subject(typeof(InputMapper))]
    public class when_an_unmapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            _input.IsKeyDown(Keys.J).Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_not_send_a_keydown_message = () =>
            _eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_no_keys_are_down : InputMapperContext
    {
        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_not_send_a_keydown_message = () =>
            _eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_the_input_matches_a_general_filter : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddGeneralInputMessage<DoSomething>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            _input.MouseDeltaX.Returns(10);
            _input.IsRightMouseButtonDown.Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_send_the_mapped_message = () =>
            _eventListener.ShouldReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_the_input_does_not_match_a_general_filter : InputMapperContext
    {
        Establish context = () =>
        {
            _inputMapper.AddGeneralInputMessage<DoSomething>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            _input.IsRightMouseButtonDown.Returns(true);
        };

        Because of = () =>
            _inputMapper.HandleInput(_input);

        It should_not_send_the_mapped_message = () =>
            _eventListener.ShouldNotReceive<DoSomething>();
    }

    public class InputMapperContext
    {
        static public IInputState _input;
        static public InputMapper _inputMapper;
        public static TestEventListener _eventListener;

        Establish context = () =>
        {
            _input = Substitute.For<IInputState>();
            _inputMapper = new InputMapper();
            _eventListener = new TestEventListener();
            EventAggregator.Instance.AddListener(_eventListener);
        };
    }

    public class DoSomething : InputMessage
    {
    }

    public class DoSomethingElse : InputMessage
    {
    }

    public class TestEventListener
        : IListener<DoSomething>,
        IListener<DoSomethingElse>
    {
        readonly List<object> _receivedMessages = new List<object>();

        public T Message<T>()
        {
            return _receivedMessages.OfType<T>().FirstOrDefault();
        }

        public void ShouldReceive<T>()
        {
            Message<T>().ShouldNotBeNull();
        }

        public void ShouldNotReceive<T>()
        {
            Message<T>().ShouldBeNull();
        }

        public void Handle(DoSomething message)
        {
            _receivedMessages.Add(message);
        }

        public void Handle(DoSomethingElse message)
        {
            _receivedMessages.Add(message);
        }
    }
}
