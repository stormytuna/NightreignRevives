using System.IO;
using NightreignRevives.Core;
using Terraria.DataStructures;

namespace NightreignRevives.Content;

public class ReviveCircleNPC : ModNPC
{
	public int ForClient;
	public int LifeMax;

	public const int DamageDecayTimerMax = 1 * 60;
	public int DamageDecayTimer = DamageDecayTimerMax;

	private bool _firstFrame = true;

	public override void SetDefaults() {
		NPC.width = 40;
		NPC.height = 40;
		NPC.lifeMax = 10;

		NPC.aiStyle = -1;
		NPC.knockBackResist = 0f;
		NPC.noGravity = true;
	}

	public override void OnSpawn(IEntitySource source) {
		ForClient = (int)NPC.ai[0];
		LifeMax = (int)NPC.ai[1];
	}

	public override void AI() {
		if (_firstFrame) {
			_firstFrame = false;
			NPC.life = NPC.lifeMax = LifeMax;
		}

		if (DamageDecayTimer > 0) {
			DamageDecayTimer--;
		}
		else {
			NPC.life += int.Max(NPC.lifeMax / (6 * 60), 1);
			if (NPC.life > NPC.lifeMax) {
				NPC.life = NPC.lifeMax;
			}
		}
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
		return false;
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
		modifiers.DamageVariationScale *= 0f;
		modifiers.HideCombatText();
	}

	public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
		ResetDamageDecay();

		if (Main.netMode != NetmodeID.SinglePlayer) {
			BroadcastReviveNPCHit(-1, Main.myPlayer, NPC.whoAmI);
		}
	}

	public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
		ResetDamageDecay();

		if (Main.netMode != NetmodeID.SinglePlayer) {
			BroadcastReviveNPCHit(-1, Main.myPlayer, NPC.whoAmI);
		}
	}

	public override void OnKill() {
		if (Main.netMode != NetmodeID.Server) {
			return;
		}

		NightreignRevivePlayer.BroadcastRevive(ForClient);
		Main.player[ForClient].GetModPlayer<NightreignRevivePlayer>().Revive();
	}

	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write7BitEncodedInt(ForClient);
		writer.Write(LifeMax);
	}

	public override void ReceiveExtraAI(BinaryReader reader) {
		ForClient = reader.Read7BitEncodedInt();
		LifeMax = reader.ReadInt32();
	}

	public static void BroadcastReviveNPCHit(int toWho, int fromWho, int npcWhoAmI) {
		ModPacket packet = ModContent.GetInstance<NightreignRevives>().GetPacket();
		packet.Write((byte)NightreignRevives.MessageType.HitReviveNPC);
		packet.Write7BitEncodedInt(npcWhoAmI);
		packet.Send(toWho, fromWho);
	}

	public static void ReceiveReviveNPCHit(int npcWhoAmI) {
		NPC npc = Main.npc[npcWhoAmI];
		if (npc.ModNPC is ReviveCircleNPC nrNPC) {
			nrNPC.ResetDamageDecay();
		}
	}

	public void ResetDamageDecay() {
		DamageDecayTimer = DamageDecayTimerMax;
	}
}
