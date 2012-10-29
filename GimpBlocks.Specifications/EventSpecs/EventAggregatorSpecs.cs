using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using NSubstitute;

namespace GimpBlocks.Specifications.EventSpecs
{
    [Subject(typeof(EventAggregator))]
    public class when_a_message_is_sent_and_the_aggregator_has_no_listeners : EventAggregatorContext
    {
        Because of = () =>
            eventAggregator.SendMessage(helloMessage);

        It should_not_send_any_messages = () =>
        {
            // Don't crash, I guess
        };            
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_message_with_no_eligible_listeners_is_sent : EventAggregatorContext
    {
        Establish context = () =>
            eventAggregator.AddListener(helloListener1);

        Because of = () =>
            eventAggregator.SendMessage(goodbyeMessage);

        It should_not_send_any_messages = () =>
            helloListener1.DidNotReceive().Handle(Arg.Any<Hello>());
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_message_with_one_eligible_listener_is_sent : EventAggregatorContext
    {
        Establish context = () =>
            eventAggregator.AddListener(helloListener1);

        Because of = () =>
            eventAggregator.SendMessage(helloMessage);

        It should_send_the_message_to_the_listener = () =>
            helloListener1.Received().Handle(helloMessage);
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_message_with_multiple_eligible_listeners_is_sent : EventAggregatorContext
    {
        Establish context = () =>
        {
            eventAggregator.AddListener(helloListener1);
            eventAggregator.AddListener(helloListener2);
        };

        Because of = () =>
            eventAggregator.SendMessage(helloMessage);

        It should_send_the_message_to_all_eligible_listeners = () =>
        {
            helloListener1.Received().Handle(helloMessage);
            helloListener2.Received().Handle(helloMessage);
        };
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_listener_is_added_multiple_times : EventAggregatorContext
    {
        static int handleCounter;

        Establish context = () =>
        {
            helloListener1.When(x => x.Handle(helloMessage)).Do(x => handleCounter++);

            eventAggregator.AddListener(helloListener1);
            eventAggregator.AddListener(helloListener1);
        };

        Because of = () =>
            eventAggregator.SendMessage(helloMessage);

        It should_send_the_message_to_the_listener_only_once = () =>
            handleCounter++;
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_listener_wants_to_modify_the_aggregator_in_response_to_a_message : EventAggregatorContext
    {
        Establish context = () =>
        {
            helloListener1.When(x => x.Handle(helloMessage)).Do(x => eventAggregator.AddListener(helloListener2));
            
            eventAggregator.AddListener(helloListener1);
        };

        Because of = () =>
            eventAggregator.SendMessage(helloMessage);

        It should_allow_the_modification = () =>
            eventAggregator.HasListener(helloListener2);
    }

    [Subject(typeof(EventAggregator))]
    public class when_a_listener_loses_all_strong_references : EventAggregatorContext
    {
        Establish context = () =>
        {
            var disposableListener = Substitute.For<IListener<Hello>>();
            eventAggregator.AddListener(disposableListener);
        };

        Because of = () =>
            GC.Collect();

        It should_remove_the_listener_from_the_aggregator = () =>
            eventAggregator.HasListener(helloListener1).ShouldBeFalse();
    }

    public class EventAggregatorContext
    {
        public static Hello helloMessage;
        public static Goodbye goodbyeMessage;
        public static IListener<Hello> helloListener1;
        public static IListener<Hello> helloListener2;
        public static IListener<Goodbye> goodbyeListener;
        public static TestableEventAggregator eventAggregator;

        Establish context = () =>
        {
            helloMessage = new Hello();
            goodbyeMessage = new Goodbye();
            helloListener1 = Substitute.For<IListener<Hello>>();
            helloListener2 = Substitute.For<IListener<Hello>>();
            goodbyeListener = Substitute.For<IListener<Goodbye>>();
            eventAggregator = new TestableEventAggregator();
        };
    }

    public class TestableEventAggregator : EventAggregator
    {
        public bool HasListener(object listener)
        {
            return base.IsRegistered(listener);
        }
    }

    public class Hello
    {
    }

    public class Goodbye
    {
    }
}
