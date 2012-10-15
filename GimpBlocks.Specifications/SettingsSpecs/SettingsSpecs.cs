using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Machine.Specifications;
using NSubstitute;

namespace GimpBlocks.Specifications.SettingsSpecs
{
    [Subject(typeof(Settings))]
    public class when_a_toggle_draw_wireframe_message_is_sent : SettingsContext
    {
        static public bool _oldValue;

        Establish context = () =>
            _oldValue = Settings.Instance.ShouldDrawWireframe;

        Because of = () =>
            Settings.Instance.Handle(new ToggleDrawWireframeSetting());

        It should_toggle_the_draw_wireframe_setting = () =>
            Settings.Instance.ShouldDrawWireframe.ShouldNotEqual(_oldValue);
    }

    [Subject(typeof(Settings))]
    public class when_a_toggle_update_message_is_sent : SettingsContext
    {
        static public bool _oldValue;

        Establish context = () =>
            _oldValue = Settings.Instance.ShouldUpdate;

        Because of = () =>
            Settings.Instance.Handle(new ToggleUpdateSetting());

        It should_toggle_the_update_setting = () =>
            Settings.Instance.ShouldUpdate.ShouldNotEqual(_oldValue);
    }

    [Subject(typeof(Settings))]
    public class when_a_single_step_message_is_sent : SettingsContext
    {
        static public bool _oldValue;

        Establish context = () =>
            _oldValue = Settings.Instance.ShouldSingleStep;

        Because of = () =>
            Settings.Instance.Handle(new ToggleSingleStepSetting());

        It should_toggle_the_single_step_setting = () =>
            Settings.Instance.ShouldSingleStep.ShouldNotEqual(_oldValue);
    }

    // TODO: test that changes to all settings raises a SettingsChanged event,
    // probably by reflecting over all properties and setting them

    public class SettingsContext
    {
        Establish context = () =>
        {
        };
    }
}