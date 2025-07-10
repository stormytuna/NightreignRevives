using FishUtils.Helpers;
using NightreignRevives.Content;
using NightreignRevives.Core;
using Terraria.ModLoader.Config;

namespace NightreignRevives;

public static class NightreignReviveHelpers
{
	public static bool AnyReviveNPC(this Player player, out NPC reviveNPC) {
		foreach (NPC npc in Main.ActiveNPCs) {
			if (npc.ModNPC is ReviveCircleNPC x && x.ForClient == player.whoAmI) {
				reviveNPC = npc;
				return true;
			}
		}

		reviveNPC = null;
		return false;
	}

	public static bool NoReviveNPC(this Player player) {
		return !player.AnyReviveNPC(out _);
	}

	public static bool AnyBossOrInvasionForReviveNPC() {
		if (ServerConfig.Instance.EnableDuringInvasions && Main.invasionType != InvasionID.None) {
			return true;
		}

		foreach (NPC npc in Main.ActiveNPCs) {
			bool notBlacklisted = !ServerConfig.Instance.BossBlacklist.Contains(new NPCDefinition(npc.type));
			if (npc.CountsAsBoss() && notBlacklisted) {
				return true;
			}
		}

		return false;
	}

	public static bool TryGetReviveNPCStats(this Player player, out int maxLife) {
		maxLife = 0;
		int numDownsThisFight = player.GetModPlayer<NightreignRevivePlayer>().NumDownsThisFight;

		if (ServerConfig.Instance.EnableDuringInvasions && Main.invasionType != InvasionID.None) {
			int invasionLife = Main.invasionType switch {
				InvasionID.GoblinArmy => Main.hardMode ? 15000 : 5000,
				InvasionID.SnowLegion => 12000,
				InvasionID.PirateInvasion => 18000,
				InvasionID.MartianMadness => 80000,
				_ => 1000,
			};

			if (Main.snowMoon || Main.pumpkinMoon) {
				invasionLife = 50000;
			}

			if (Main.masterMode) {
				invasionLife *= 2;
			}
			else if (Main.expertMode) {
				invasionLife = (int)(invasionLife * 1.5);
			}

			maxLife = (int)(invasionLife * numDownsThisFight * ServerConfig.Instance.LifeMultiplier);
			return true;
		}

		if (ServerConfig.Instance.EnableDuringBloodMoonAndSolarEclipse && (Main.bloodMoon || Main.eclipse)) {
			int life = NPC.downedPlantBoss ? 50000 : Main.hardMode ? 25000 : 10000;
			maxLife = (int)(life * numDownsThisFight * ServerConfig.Instance.LifeMultiplier);
			return true;
		}

		NPC boss = null;
		foreach (NPC npc in Main.ActiveNPCs) {
			bool notBlacklisted = !ServerConfig.Instance.BossBlacklist.Contains(new NPCDefinition(npc.type));
			if (npc.CountsAsBoss() && notBlacklisted && npc.lifeMax > (boss?.lifeMax ?? 0)) {
				boss = npc;
			}
		}

		if (boss is null) {
			return false;
		}

		// Downs + 1 as downs start at 0 and are incremented after this method is called
		maxLife = (int)(boss.lifeMax * (numDownsThisFight + 1) * ServerConfig.Instance.LifeMultiplier);

		if (boss.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail) {
			maxLife = Main.masterMode ? 19224 : Main.expertMode ? 15120 : 10050;
			maxLife = (int)(maxLife * (1f + Main.CurrentFrameFlags.ActivePlayersCount * 0.35));
		}

		return true;
	}
}
