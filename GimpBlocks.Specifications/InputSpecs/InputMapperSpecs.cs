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
            inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            input.IsKeyPressed(Keys.K).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_send_a_keypress_message = () =>
            eventListener.ShouldReceive<DoSomething>();

        It should_send_a_message_containing_the_current_input_state = () =>
            eventListener.Message<DoSomething>().InputState.ShouldEqual(input);
    }

    [Subject(typeof(InputMapper))]
    public class when_a_multimapped_key_is_pressed : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            inputMapper.AddKeyPressMessage<DoSomethingElse>(Keys.K);
            input.IsKeyPressed(Keys.K).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_send_the_first_keypress_message = () =>
            eventListener.ShouldReceive<DoSomething>();

        It should_send_the_second_keypress_message = () =>
            eventListener.ShouldReceive<DoSomethingElse>();
    }

    [Subject(typeof(InputMapper))]
    public class when_an_unmapped_key_is_pressed : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);
            input.IsKeyPressed(Keys.J).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_not_send_a_keypress_message = () =>
            eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_no_keys_are_pressed : InputMapperContext
    {
        Establish context = () =>
            inputMapper.AddKeyPressMessage<DoSomething>(Keys.K);

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_not_send_a_keypress_message = () =>
            eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_a_mapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            input.IsKeyDown(Keys.K).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_send_a_keydown_message = () =>
            eventListener.ShouldReceive<DoSomething>();

        It should_send_a_message_containing_the_current_input_state = () =>
            eventListener.Message<DoSomething>().InputState.ShouldEqual(input);
    }

    [Subject(typeof(InputMapper))]
    public class when_a_multimapped_mapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            inputMapper.AddKeyDownMessage<DoSomethingElse>(Keys.K);
            input.IsKeyDown(Keys.K).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_send_the_first_keydown_message = () =>
            eventListener.ShouldReceive<DoSomething>();

        It should_send_the_second_keydown_message = () =>
            eventListener.ShouldReceive<DoSomethingElse>();
    }

    [Subject(typeof(InputMapper))]
    public class when_an_unmapped_key_is_down : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddKeyDownMessage<DoSomething>(Keys.K);
            input.IsKeyDown(Keys.J).Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_not_send_a_keydown_message = () =>
            eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_no_keys_are_down : InputMapperContext
    {
        Because of = () =>
            inputMapper.HandleInput(input);

        It should_not_send_a_keydown_message = () =>
            eventListener.ShouldNotReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_the_input_matches_a_general_filter : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddGeneralInputMessage<DoSomething>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            input.MouseDeltaX.Returns(10);
            input.IsRightMouseButtonDown.Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_send_the_mapped_message = () =>
            eventListener.ShouldReceive<DoSomething>();
    }

    [Subject(typeof(InputMapper))]
    public class when_the_input_does_not_match_a_general_filter : InputMapperContext
    {
        Establish context = () =>
        {
            inputMapper.AddGeneralInputMessage<DoSomething>(inputState => inputState.MouseDeltaX != 0 || inputState.MouseDeltaY != 0);
            input.IsRightMouseButtonDown.Returns(true);
        };

        Because of = () =>
            inputMapper.HandleInput(input);

        It should_not_send_the_mapped_message = () =>
            eventListener.ShouldNotReceive<DoSomething>();
    }

    public class InputMapperContext
    {
        static public IInputState input;
        static public InputMapper inputMapper;
        public static TestEventListener eventListener;

        Establish context = () =>
        {
            input = Substitute.For<IInputState>();
            inputMapper = new InputMapper();
            eventListener = new TestEventListener();
            EventAggregator.Instance.AddListener(eventListener);
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
        readonly List<object> receivedMessages = new List<object>();

        public T Message<T>()
        {
            return receivedMessages.OfType<T>().FirstOrDefault();
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
            receivedMessages.Add(message);
        }

        public void Handle(DoSomethingElse message)
        {
            receivedMessages.Add(message);
        }
    }
}
