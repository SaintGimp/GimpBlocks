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
            _oldValue = _settings.ShouldDrawWireframe;

        Because of = () =>
            _settings.Handle(new ToggleDrawWireframeSetting());

        It should_toggle_the_draw_wireframe_setting = () =>
            _settings.ShouldDrawWireframe.ShouldNotEqual(_oldValue);
    }

    [Subject(typeof(Settings))]
    public class when_a_toggle_update_message_is_sent : SettingsContext
    {
        static public bool _oldValue;

        Establish context = () =>
            _oldValue = _settings.ShouldUpdate;

        Because of = () =>
            _settings.Handle(new ToggleUpdateSetting());

        It should_toggle_the_update_setting = () =>
            _settings.ShouldUpdate.ShouldNotEqual(_oldValue);
    }

    [Subject(typeof(Settings))]
    public class when_a_single_step_message_is_sent : SettingsContext
    {
        static public bool _oldValue;

        Establish context = () =>
            _oldValue = _settings.ShouldSingleStep;

        Because of = () =>
            _settings.Handle(new ToggleSingleStepSetting());

        It should_toggle_the_single_step_setting = () =>
            _settings.ShouldSingleStep.ShouldNotEqual(_oldValue);
    }

    // TODO: test that changes to all settings raises a SettingsChanged event,
    // probably by reflecting over all properties and setting them

    public class SettingsContext
    {
        public static Settings _settings;

        Establish context = () =>
        {
            _settings = new Settings();
        };
    }
}