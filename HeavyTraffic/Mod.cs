using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HeavyTraffic.Systems;
using JetBrains.Annotations;
using Unity.Entities;

namespace HeavyTraffic
{
    [UsedImplicitly]
    public class Mod : IMod
    {
        public static readonly ILog Log = LogManager.GetLogger($"{nameof(HeavyTraffic)}.{nameof(Mod)}")
            .SetShowsErrorsInUI(false);

        public static Settings Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                Log.Info($"Current mod asset at {asset.path}");
            }

            Settings = new Settings(this);

            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new Settings.LocaleEN(Settings));

            ApplyCustomSystems(updateSystem);
        }

        private void ApplyCustomSystems(UpdateSystem updateSystem)
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Simulation.TrafficSpawnerAISystem>().Enabled = false;
            updateSystem.UpdateAt<TrafficSpawnerAISystem_HeavyTraffic>(SystemUpdatePhase.GameSimulation);
        }

        public void OnDispose()
        {
            Log.Info(nameof(OnDispose));
        }
    }
}
