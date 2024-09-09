using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;

namespace HeavyTraffic
{
    public class Settings : ModSetting
    {
        [SettingsUISlider(min = 0, max = 1000, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        public int fake_traffic_spawn_rate { get; set; } = 100;
        
        [SettingsUISlider(min = 0, max = 20, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
        public int fake_traffic_spawn_rate_multiplier { get; set; } = 1;
        
        public int TotalOutsideTrafficPercent => fake_traffic_spawn_rate * fake_traffic_spawn_rate_multiplier;

        public Settings(IMod mod) : base(mod)
        {
        }

        public override void SetDefaults()
        {
            fake_traffic_spawn_rate = 100;
            fake_traffic_spawn_rate_multiplier = 1;
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
                    {m_Setting.GetOptionLabelLocaleID(nameof(Settings.fake_traffic_spawn_rate)), "Outside traffic spawn rate"},
                    {
                        m_Setting.GetOptionDescLocaleID(nameof(Settings.fake_traffic_spawn_rate)),
                        $"How much outside traffic will spawn in %. 100% is a default, vanilla amount of traffic. 0% is no traffic. 200% is double the amount vanilla traffic, etc." +
                        $"\n\nOutside traffic are vehicles spawning at an outside connection with a destination set to a random another outside connection. This includes cars, boats, trains and airplanes." +
                        $"\n\nApplied immediately, no game restart necessary."
                    },
                    
                    {m_Setting.GetOptionLabelLocaleID(nameof(Settings.fake_traffic_spawn_rate_multiplier)), "Outside traffic additional multiplier"},
                    {
                        m_Setting.GetOptionDescLocaleID(nameof(Settings.fake_traffic_spawn_rate_multiplier)),
                        $"If the above setting is not enough, set this to more than 1. The above value will be multiplied by this." +
                        $"\n\ne.g. 200% (above) * 4 (here) will result in 800% or 8x more outside traffic than vanilla." +
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
