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
        [SettingsUISlider(min = 0, max = 10000, step = 1, scalarMultiplier = 1, unit = Unit.kPercentage)]
        public int fake_traffic_spawn_rate { get; set; } = 100;

        public Settings(IMod mod) : base(mod)
        {
        }

        public override void SetDefaults()
        {
            fake_traffic_spawn_rate = 100;
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
                        $"How much traffic will spawn at outside connections with a destination set to another outside connection in %. 100% is a default, vanilla amount of traffic. 0% is traffic. 200% is double the amount vanilla traffic etc.\n\nApplied immediately, no game restart necessary."
                    },
                };
            }

            public void Unload()
            {
            }
        }
    }
}
