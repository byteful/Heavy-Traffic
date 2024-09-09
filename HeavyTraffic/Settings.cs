using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;

namespace HeavyTraffic
{
    [FileLocation(@"ModsSettings\HeavyTraffic\HeavyTraffic")]
    public class Settings : ModSetting
    {
        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        public int fake_traffic_spawn_rate { get; set; } = 100;
        
        [SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        public int fake_traffic_spawn_rate_fine_control { get; set; } = 0;
        
        [SettingsUISlider(min = 1, max = 1000, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        public int fake_traffic_spawn_rate_extreme { get; set; } = 100;
        
        public int TotalOutsideTrafficPercent => 
            ((fake_traffic_spawn_rate * 100) + fake_traffic_spawn_rate_fine_control) * fake_traffic_spawn_rate_extreme;

        public Settings(IMod mod) : base(mod)
        {
        }

        public override void SetDefaults()
        {
            fake_traffic_spawn_rate = 1;
            fake_traffic_spawn_rate_fine_control = 0;
            fake_traffic_spawn_rate_extreme = 1;
        }

        public class LocaleEN : IDictionarySource
        {
            private readonly Settings m_Setting;

            public LocaleEN(Settings setting)
            {
                m_Setting = setting;
            }

            public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors,
                Dictionary<string, int> indexCounts)
            {
                return new Dictionary<string, string>
                {
                    {m_Setting.GetSettingsLocaleID(), "Outside Traffic"},
                    {m_Setting.GetOptionLabelLocaleID(nameof(Settings.fake_traffic_spawn_rate)), "Spawn rate"},
                    {
                        m_Setting.GetOptionDescLocaleID(nameof(Settings.fake_traffic_spawn_rate)),
                        $"How much outside traffic will spawn relative to vanilla settings. 1 is a default, vanilla amount of traffic. 0 is no traffic. 2 is double the amount vanilla traffic, etc." +
                        $"\n\nOutside traffic are vehicles spawning at an outside connection with a destination set to a random another outside connection. This includes cars, boats, trains and airplanes." +
                        $"\n\nApplied immediately, no game restart necessary."
                    },
                    
                    {m_Setting.GetOptionLabelLocaleID(nameof(Settings.fake_traffic_spawn_rate_fine_control)), "Fine control"},
                    {
                        m_Setting.GetOptionDescLocaleID(nameof(Settings.fake_traffic_spawn_rate_fine_control)),
                        $"If the above setting is not fine enough, use this. This value in percents will be added on top of the above value. " +
                        $"\n\ne.g. 2 (above) * 60% (here) will result in 260% or 2.6x more outside traffic than vanilla." +
                        $"\n\nApplied immediately, no game restart necessary."
                    },
                    
                    {m_Setting.GetOptionLabelLocaleID(nameof(Settings.fake_traffic_spawn_rate_extreme)), "Spawn rate EXTREME"},
                    {
                        m_Setting.GetOptionDescLocaleID(nameof(Settings.fake_traffic_spawn_rate_extreme)),
                        $"If the above settings did not crash your game, this will. The final outside traffic spawn rate is multiplied by this number. " +
                        $"\n\ne.g. 50 (above) * 100 (here) will result in 5000x more outside traffic than vanilla." +
                        $"\n\nApplied immediately, no game restart necessary."
                    },
                };
            }

            public void Unload()
            {
            }
        }
    }
}
