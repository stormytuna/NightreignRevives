using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NightreignRevives.Core;

public class ServerConfig : ModConfig
{
	public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();
	
	public override ConfigScope Mode => ConfigScope.ServerSide;	
	
	[ReloadRequired]
	public List<NPCDefinition> BossBlacklist { get; set; } = new();

	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableDuringInvasions { get; set; }
	
	[DefaultValue(false)]
	[ReloadRequired]
	public bool EnableDuringBloodMoonAndSolarEclipse { get; set; }
	
	[DefaultValue(0.06f)]
	[ReloadRequired]
	public float LifeMultiplier { get; set; }
	
	[DefaultValue(true)]
	public bool SinglePlayerWarning { get; set; }
}
