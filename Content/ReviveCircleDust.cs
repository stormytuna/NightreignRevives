using FishUtils.Helpers;

namespace NightreignRevives.Content;

public class ReviveCircleDust : ModDust
{
	public override string Texture {
		get => null;
	}

	public override void OnSpawn(Dust dust) {
		dust.frame = DustHelpers.FrameVanillaDust(DustID.PurpleTorch);
		dust.rotation = Main.rand.NextRadian();
		dust.noGravity = true;
	}

	public override bool Update(Dust dust) {
		dust.velocity *= 0.98f;
		dust.scale *= 0.97f;

		if (dust.scale < 0.2f) {
			dust.active = false;
		}

		dust.rotation += dust.velocity.X * 0.1f;

		if (dust.customData is "GoUpPlease") {
			dust.velocity.Y -= 0.05f;
			if (dust.velocity.Y > 8f) {
				dust.velocity.Y = 8f;
			}
		}

		dust.position += dust.velocity;

		return false;
	}
}
