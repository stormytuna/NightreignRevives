using FishUtils.DataStructures;
using NightreignRevives.Content;
using ReLogic.Content;
using Terraria.UI;

namespace NightreignRevives.Core;

public class PlayerReviveCirclesUIState : UIState
{
	private static Asset<Texture2D> _outlineTexture;
	private static Asset<Texture2D> _fillTexture;
	private static Asset<Effect> _radialFillEffect;

	public override void OnInitialize() {
		_outlineTexture = ModContent.Request<Texture2D>($"{nameof(NightreignRevives)}/Assets/ReviveUIOutline");
		_fillTexture = ModContent.Request<Texture2D>($"{nameof(NightreignRevives)}/Assets/ReviveUIFill");
		_radialFillEffect = ModContent.Request<Effect>($"{nameof(NightreignRevives)}/Assets/Shaders/RadialMask");
	}

	protected override void DrawSelf(SpriteBatch spriteBatch) {
		base.DrawSelf(spriteBatch);

		// TODO: Maybe draw all outlines, then all fill and backgrounds?
		//       Restarting SB multiple times per downed player is a bit much, but i doubt more than a dozen people will be downed at once
		foreach (Player player in Main.ActivePlayers) {
			if (player.AnyReviveNPC(out NPC reviveNPC)) {
				if (reviveNPC.ModNPC is not ReviveCircleNPC reviveCircleNPC) {
					continue;
				}
				
				float opacity = Utils.Remap(reviveCircleNPC.FadeIn, 30f, 60f, 0f, 1f);
				
				Rectangle rect = new((int)(reviveNPC.Center.X - _outlineTexture.Width() / 2 - Main.screenPosition.X), (int)(reviveNPC.Center.Y - _outlineTexture.Height() / 2 - Main.screenPosition.Y), _outlineTexture.Width(), _outlineTexture.Height());
				Main.spriteBatch.Draw(_outlineTexture.Value, rect, Color.White * opacity);

				Main.spriteBatch.TakeSnapshotAndEnd(out SpriteBatchParams sbParams);

				float minProgress = player.GetModPlayer<NightreignRevivePlayer>().NumDownsThisFight switch {
					1 => 0.66f,
					2 => 0.33f,
					_ => 0,
				};
				
				float fillProgress = Utils.Remap(reviveNPC.life, 0f, reviveNPC.lifeMax, 1f, minProgress, false);
				_radialFillEffect.Value.Parameters["progress"].SetValue(fillProgress);
				_radialFillEffect.Value.Parameters["textureSize"].SetValue(_fillTexture.Size());

				Main.spriteBatch.Begin(sbParams with { Effect = _radialFillEffect.Value });
				Main.spriteBatch.Draw(_fillTexture.Value, rect, Color.White * opacity);
				Main.spriteBatch.Restart(sbParams);
			}
		}
	}
}
