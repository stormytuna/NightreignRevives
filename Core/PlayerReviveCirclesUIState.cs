using FishUtils.DataStructures;
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

		foreach (Player player in Main.ActivePlayers) {
			if (player.AnyReviveNPC(out NPC reviveNPC)) {
				Rectangle rect = new((int)(reviveNPC.Center.X - _outlineTexture.Width() / 2 - Main.screenPosition.X), (int)(reviveNPC.Center.Y - _outlineTexture.Height() / 2 - Main.screenPosition.Y), _outlineTexture.Width(), _outlineTexture.Height());
				Main.spriteBatch.Draw(_outlineTexture.Value, rect, Color.White);

				Main.spriteBatch.TakeSnapshotAndEnd(out SpriteBatchParams sbParams);

				float minProgress = player.GetModPlayer<NightreignRevivePlayer>().NumDownsThisFight switch {
					1 => 0.66f,
					2 => 0.33f,
					_ => 0,
				};
				float progress = Utils.Remap(reviveNPC.life, 0f, reviveNPC.lifeMax, 1f, minProgress, false);
				_radialFillEffect.Value.Parameters["progress"].SetValue(progress);

				Main.spriteBatch.Begin(sbParams with { Effect = _radialFillEffect.Value });

				Main.spriteBatch.Draw(_fillTexture.Value, rect, Color.White);

				Main.spriteBatch.Restart(sbParams);
			}
		}
	}
}
