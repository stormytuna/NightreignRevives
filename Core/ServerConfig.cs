using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NightreignRevives.Core;

public class ServerConfig : ModConfig
{
	public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();
	
	public override ConfigScope Mode => ConfigScope.ServerSide;	
	
	public List<NPCDefinition> BossBlacklist { get; set; } = new();

	[DefaultValue(true)]
	public bool EnableDuringInvasions { get; set; }
	
	[DefaultValue(0.06f)]
	public float LifeMultiplier { get; set; }
}
