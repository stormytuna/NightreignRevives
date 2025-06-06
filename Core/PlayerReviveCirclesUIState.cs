using FishUtils.DataStructures;
using NightreignRevives.Content;
using ReLogic.Content;
using Terraria.DataStructures;
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

		Main.spriteBatch.TakeSnapshotAndEnd(out SpriteBatchParams sbParams);

		foreach (Player player in Main.ActivePlayers) {
			if (player.AnyReviveNPC(out NPC reviveNPC)) {
				if (reviveNPC.ModNPC is not ReviveCircleNPC reviveCircleNPC) {
					continue;
				}

				DrawReviveCircle(reviveNPC, reviveCircleNPC, player);
			}
		}

		Main.spriteBatch.Begin(sbParams);
	}

	private void DrawReviveCircle(NPC reviveNPC, ReviveCircleNPC reviveCircleNPC, Player player) {
		var drawData = new DrawData {
			texture = _outlineTexture.Value,
			position = (reviveNPC.Center - Main.screenPosition).Floor(),
			sourceRect = _fillTexture.Frame(),
			origin = _fillTexture.Size() / 2f,
			color = Color.White * reviveCircleNPC.Opacity,
			scale = new Vector2(1f),
		};

		if (player.whoAmI == Main.myPlayer) {
			drawData.position = (Main.ScreenSize.ToVector2() / 2f).Floor();
			drawData.scale = new Vector2(2f);
		}

		SpriteBatchParams circleSBParams = SpriteBatchParams.Default;
		Main.spriteBatch.Begin(circleSBParams);
		drawData.Draw(Main.spriteBatch);
		Main.spriteBatch.End();

		float minProgress = player.GetModPlayer<NightreignRevivePlayer>().NumDownsThisFight switch {
			1 => 0.66f,
			2 => 0.33f,
			_ => 0,
		};

		float fillProgress = Utils.Remap(reviveNPC.life, 0f, reviveNPC.lifeMax, 1f, minProgress, false);
		_radialFillEffect.Value.Parameters["progress"].SetValue(fillProgress);
		_radialFillEffect.Value.Parameters["textureSize"].SetValue(_fillTexture.Size());

		Main.spriteBatch.Begin(circleSBParams with { Effect = _radialFillEffect.Value });
				
		var fillDrawData = drawData with {
			texture = _fillTexture.Value,
		};
		fillDrawData.Draw(Main.spriteBatch);
				
		Main.spriteBatch.End();
	}
}
