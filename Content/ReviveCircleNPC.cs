using System.IO;
using FishUtils.Helpers;
using NightreignRevives.Core;
using Terraria.Audio;
using Terraria.DataStructures;

namespace NightreignRevives.Content;

// TODO: seems to just die with zero logs if a player joins while someone is downed
public class ReviveCircleNPC : ModNPC
{
	public int ForClient;
	public int LifeMax;
	public int FadeIn;

	public const int DamageDecayTimerMax = 1 * 60;
	public int DamageDecayTimer = DamageDecayTimerMax;

	private bool _firstFrame = true;
	
	private bool _dying = false;
	private bool _dead = false;

	public override void SetDefaults() {
		NPC.width = 40;
		NPC.height = 40;
		NPC.lifeMax = 10;
		NPC.netAlways = true;

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
			
			DustHelpers.MakeDustExplosion(NPC.Center, 30f, ModContent.DustType<ReviveCircleDust>(), 25, 1f, 5f, scale: 1.5f);
		}

		if (_dying) {
			FadeIn--;
			if (FadeIn < 0) {
				NPC.life = 0;
				
				DustHelpers.MakeDustExplosion(NPC.Center, 30f, ModContent.DustType<ReviveCircleDust>(), 20, 2f, 7f);
				
				var sound = SoundID.DD2_BetsyFireballImpact with {
					PitchRange = (-0.8f, -0.5f),
					Variants = [0], // Want specifically variant 0
				};
				SoundEngine.PlaySound(sound, NPC.Center);

				if (Main.netMode == NetmodeID.Server) {
					NightreignRevivePlayer.BroadcastRevive(ForClient);
					Main.player[ForClient].GetModPlayer<NightreignRevivePlayer>().Revive();
				}
			}

			for (int i = 0; i < 3; i++) {
				Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
				var dust = Dust.NewDustPerfect(NPC.Center + offset, ModContent.DustType<ReviveCircleDust>());
				dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
				dust.velocity *= Main.rand.NextFloat(2f, 5f);
			}
			
			return;
		}
		
		FadeIn++;
		if (FadeIn > 60) {
			FadeIn = 60;
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

		float numDust = Utils.Remap(NPC.life, 0f, NPC.lifeMax, 1.5f, 0.1f);
		for (float i = 0; i < numDust; i++) {
			if (i == 0 && Main.rand.NextFloat() > numDust % 1f) {
				continue;			
			}
			
			Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
			var dust = Dust.NewDustPerfect(NPC.Center + offset, ModContent.DustType<ReviveCircleDust>());
			float startScale = Utils.Remap(NPC.life, 0f, NPC.lifeMax, 1.2f, 0.9f);
			dust.scale = startScale + Main.rand.NextFloat(0.3f);
			dust.velocity *= Utils.Remap(NPC.life, 0f, NPC.lifeMax, 0.5f, 0.1f);
			dust.customData = "GoUpPlease";
		}
	}

	public override bool CheckDead() {
		_dying = true;
		NPC.life = 1;
		return _dead;
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
		return false;
	}

	public override bool CanHitNPC(NPC target) {
		if (_dying) {
			return false;
		}
		
		return base.CanHitNPC(target);
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
	
	public override void HitEffect(NPC.HitInfo hit) {
		if (Main.netMode == NetmodeID.Server) {
			return;
		}
		
		DustHelpers.MakeDustExplosion(NPC.Center, 30f, ModContent.DustType<ReviveCircleDust>(), 10, 1f, 5f);
		
		// TODO: variance as npc gets closer to dying
		var sound = SoundID.DD2_BetsyFireballShot with {
			Volume = 0.4f,
			PitchRange = (-0.8f, -0.5f),
		};

		SoundEngine.PlaySound(sound, NPC.Center);
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
