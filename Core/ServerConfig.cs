using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NightreignRevives.Core;

public class ServerConfig : ModConfig
{
	public static ServerConfig Instance {
		get => ModContent.GetInstance<ServerConfig>();
	}

	public override ConfigScope Mode {
		get => ConfigScope.ServerSide;
	}

	[Header("Gameplay")]
	[DefaultValue(0.06f)]
	[Range(0.01f, 0.5f)]
	[ReloadRequired]
	public float LifeMultiplier { get; set; }

	[DefaultValue(2.5f)]
	[Range(0.5f, 10f)]
	[ReloadRequired]
	public float DamageDecayDelay { get; set; }

	[DefaultValue(6f)]
	[Range(2f, 100f)]
	[ReloadRequired]
	public float DamageDecayRate { get; set; }

 	[DefaultValue(100f)]
    [Range(1f, 500f)]
    [Slider]
    [ReloadRequired]
    public int ReviveHealth { get; set; }

	[Header("Toggles")]
	[ReloadRequired]
	public List<NPCDefinition> BossBlacklist { get; set; } = new();

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableDuringInvasions { get; set; }

	[DefaultValue(false)]
	[ReloadRequired]
	public bool EnableDuringBloodMoonAndSolarEclipse { get; set; }

	[DefaultValue(false)]
	[ReloadRequired]
	public bool AllowRegularRespawnTimerInBackground { get; set; }

	[Header("Misc")]
	[DefaultValue(true)]
	public bool SinglePlayerWarning { get; set; }
}
