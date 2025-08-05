using System.IO;
using FishUtils.Helpers;
using NightreignRevives.Core;
using Terraria.Audio;
using Terraria.DataStructures;
using Utils = Terraria.Utils;

namespace NightreignRevives.Content;

public class ReviveCircleNPC : ModNPC
{
	public int ForClient;
	public int LifeMax;
	public float Opacity;

	public const int DamageDecayTimerMax = 1 * 60;
	public int DamageDecayTimer = DamageDecayTimerMax;

	private bool _firstFrame = true;
	private bool _dying = false;
	private int _dyingTimer = 90;
	private bool _dead = false;
	private int _opacityDelay;
	private int _preemptiveKillTimer;

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

			if (ForClient != Main.myPlayer) {
				DustHelpers.MakeDustExplosion(NPC.Center, 30f, ModContent.DustType<ReviveCircleDust>(), 25, 1f, 5f, scale: 1.5f);
			}
		}

		if (!Main.player[ForClient].active) {
			NPC.active = false;
			return;
		}

		if (ShouldKill()) {
			_preemptiveKillTimer++;
			if (_preemptiveKillTimer >= 2 * 60) {
				_dying = true;
			}
		}
		else {
			_preemptiveKillTimer = 0;
		}

		float numDust = Utils.Remap(NPC.life, 0f, NPC.lifeMax, 1.5f, 0.3f);
		for (float i = 0; i < numDust; i++) {
			if (i == 0 && Main.rand.NextFloat() > numDust % 1f) {
				continue;
			}

			Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
			Dust dust = Dust.NewDustPerfect(NPC.Center + offset, ModContent.DustType<ReviveCircleDust>());
			float startScale = Utils.Remap(NPC.life, 0f, NPC.lifeMax, 1.2f, 0.9f);
			dust.scale = startScale + Main.rand.NextFloat(0.3f);
			dust.velocity *= Utils.Remap(NPC.life, 0f, NPC.lifeMax, 0.5f, 0.1f);
			dust.customData = "GoUpPlease";

			if (ForClient == Main.myPlayer) {
				dust.scale *= 2f;
			}
		}

		if (_dying) {
			_dyingTimer--;

			Opacity -= 0.03f;
			if (Opacity < 0) {
				Opacity = 0;
			}

			if (_dyingTimer <= 0) {
				NPC.life = 0;

				DustHelpers.MakeDustExplosion(NPC.Center, 30f, ModContent.DustType<ReviveCircleDust>(), 20, 2f, 7f);

				SoundStyle sound = SoundID.DD2_BetsyFireballImpact with {
					PitchRange = (-0.8f, -0.5f),
					Variants = [0], // Want specifically variant 0
				};
				SoundEngine.PlaySound(sound, NPC.Center);

				if (Main.netMode == NetmodeID.Server) {
					NightreignRevivePlayer.BroadcastRevive(ForClient);
					Main.player[ForClient].GetModPlayer<NightreignRevivePlayer>().Revive();
				}

				_dead = true;
			}

			for (int i = 0; i < 2; i++) {
				Vector2 offset = Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust dust = Dust.NewDustPerfect(NPC.Center + offset, ModContent.DustType<ReviveCircleDust>());
				dust.velocity = dust.position.DirectionTo(NPC.Center) * Main.rand.NextFloat(2f, 5f);
				dust.scale = Main.rand.NextFloat(1.2f, 1.5f);

				if (ForClient == Main.myPlayer) {
					dust.scale *= 2f;
				}
			}

			return;
		}

		_opacityDelay++;
		if (_opacityDelay > 45) {
			Opacity += 0.03f;
			if (Opacity > 1f) {
				Opacity = 1f;
			}
		}

		if (DamageDecayTimer > 0) {
			DamageDecayTimer--;
		}
		else {
			NPC.life += int.Max((int)((NPC.lifeMax * ServerConfig.Instance.DamageDecayRate * 0.01f) / 60f), 1);
			if (NPC.life > NPC.lifeMax) {
				NPC.life = NPC.lifeMax;
			}
		}
	}

	public override bool CheckDead() {
		_dying = true;
		NPC.life = 1;
		return _dead || !Main.player[ForClient].active;
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
		return false;
	}

	public override bool? CanBeHitByItem(Player player, Item item) {
		if (_dying || Opacity < 0.8f) {
			return false;
		}

		return base.CanBeHitByItem(player, item);
	}

	public override bool? CanBeHitByProjectile(Projectile projectile) {
		if (_dying || Opacity < 0.8f) {
			return false;
		}

		return base.CanBeHitByProjectile(projectile);
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

		float progress = NPC.life / (float)NPC.lifeMax;
		float pitch = Utils.Remap(progress, 0f, 1f, -0.1f, -0.8f);
		SoundStyle sound = SoundID.DD2_BetsyFireballShot with {
			Volume = 0.5f,
			Pitch = pitch,
			PitchVariance = 0.3f,
			MaxInstances = 3,
		};

		SoundEngine.PlaySound(sound, NPC.Center);
	}

	// Normally wouldn't be required, but some mods are allowing it to hit players somehow
	// Hopefully they respect this
	public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
		return false;
	}

	public override bool CanHitNPC(NPC target) {
		return false;
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
		DamageDecayTimer = (int)(ServerConfig.Instance.DamageDecayDelay * 60);
	}

	private bool ShouldKill() {
		if (!NightreignReviveHelpers.AnyBossOrInvasionForReviveNPC()) {
			return true;
		}

		if (Main.CurrentFrameFlags.ActivePlayersCount <= 1) {
			return true;
		}

		return false;
	}
}
