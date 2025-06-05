using NightreignRevives.Content;
using Terraria.DataStructures;

namespace NightreignRevives.Core;

public class NightreignRevivePlayer : ModPlayer
{
	public int NumDownsThisFight = 0;
	public int FadeIn = 0;
	
	private bool _beenRevived = false;
	private Point? _spawnPos = null;

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
			self.Center = nrPlayer._spawnPos.Value.ToVector2();
			self.AddImmuneTime(ImmunityCooldownID.General, 60);
			nrPlayer._spawnPos = null;
		}
	}

	public override void ResetEffects() {
		/*
		if (!NPCHelpers.AnyActiveBossOrInvasion()) {
			NumDownsThisFight = 0;
			_spawnPos = null
			// TODO: HANDLE REVIVE after boss
		}
		*/
	}

	public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
		_spawnPos = Player.Center.ToPoint();
		NumDownsThisFight++;

		if (Main.netMode != NetmodeID.Server /*&& NPCHelpers.AnyActiveBossOrInvasion()*/) {
			return;
		}

		int lifeMax = 100 * NumDownsThisFight;
		NPC.NewNPC(Player.GetSource_Death(), (int)Player.Center.X, (int)Player.Center.Y, ModContent.NPCType<ReviveCircleNPC>(), ai0: Player.whoAmI, ai1: lifeMax);
	}

	public override void UpdateDead() {
		if (_beenRevived) {
			_beenRevived = false;
			Player.respawnTimer = 0;
		}

		if (Player.AnyReviveNPC(out _)) {
			Player.respawnTimer = 999;
		}

		// TODO: HANDLE REVIVE AFTER BOSS
		// TODO: SPECIAL DEATH SCREEN
	}

	public static void BroadcastRevive(int playerWhoAmI) {
		ModPacket packet = ModContent.GetInstance<NightreignRevives>().GetPacket();
		packet.Write((byte)NightreignRevives.MessageType.PlayerRevived);
		packet.Write7BitEncodedInt(playerWhoAmI);
		packet.Send();
	}
}
