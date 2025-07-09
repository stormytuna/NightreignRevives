using NightreignRevives.Content;
using Terraria.DataStructures;

namespace NightreignRevives.Core;

public class NightreignRevivePlayer : ModPlayer
{
	public int NumDownsThisFight = 0;

	private bool _beenRevived = false;
	private Vector2? _spawnPos = null;

	public void Revive() {
		_beenRevived = true;
	}

	public override void Load() {
		On_Player.Spawn += RespawnAtDownedLocation;
	}

	private static void RespawnAtDownedLocation(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context) {
		NightreignRevivePlayer nrPlayer = self.GetModPlayer<NightreignRevivePlayer>();

		orig(self, context);

		if (nrPlayer._spawnPos is not null) {
			self.Teleport(nrPlayer._spawnPos.Value, -1, -1);
			self.AddImmuneTime(ImmunityCooldownID.General, 60);
			nrPlayer._spawnPos = null;
		}
	}

	public override void ResetEffects() {
		if (NightreignReviveHelpers.AnyBossOrInvasionForReviveNPC()) {
			return;
		}

		NumDownsThisFight = 0;
		_spawnPos = null;
		_beenRevived = false;
	}

	public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
		if (Player.TryGetReviveNPCStats(out int lifeMax)) {
			_spawnPos = Player.position;
			NumDownsThisFight++;
			
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.NewNPC(Player.GetSource_Death(), (int)Player.Center.X, (int)Player.Center.Y, ModContent.NPCType<ReviveCircleNPC>(), ai0: Player.whoAmI, ai1: lifeMax);
			}
		}
	}

	public override void UpdateDead() {
		if (_beenRevived) {
			_beenRevived = false;
			Player.respawnTimer = 0;

   			int lifeToGive = ServerConfig.Instance.ReviveHealth;
	  		lifeToGive = Math.Clamp(lifeToGive, 1, Player.statLifeMax2);
	 		Player.statLife = lifeToGive;
        	Player.HealEffect(lifeToGive, true);

			return;
		}

		if (Player.AnyReviveNPC(out _) && !ServerConfig.Instance.AllowRegularRespawnTimerInBackground) {
			Player.respawnTimer = 999;
		}
	}

	public static void BroadcastRevive(int playerWhoAmI) {
		ModPacket packet = ModContent.GetInstance<NightreignRevives>().GetPacket();
		packet.Write((byte)NightreignRevives.MessageType.PlayerRevived);
		packet.Write7BitEncodedInt(playerWhoAmI);
		packet.Send();
	}
}
