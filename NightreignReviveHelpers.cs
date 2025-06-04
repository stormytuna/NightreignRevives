using NightreignRevives.Content;

namespace NightreignRevives;

public static class NightreignReviveHelpers
{
	public static bool AnyReviveNPC(this Player player, out NPC reviveNPC) {
		foreach (var npc in Main.ActiveNPCs) {
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
}
