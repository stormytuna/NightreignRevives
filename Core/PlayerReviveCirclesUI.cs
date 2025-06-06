using System.Collections.Generic;
using Terraria.UI;

namespace NightreignRevives.Core;

[Autoload(Side = ModSide.Client)]
public class PlayerReviveCirclesUI : ModSystem
{
	public static PlayerReviveCirclesUI Instance {
		get => ModContent.GetInstance<PlayerReviveCirclesUI>();
	}

	private UserInterface _interface;
	private PlayerReviveCirclesUIState _state;

	private GameTime _oldGameTime;

	public override void Load() {
		_interface = new UserInterface();

		_state = new PlayerReviveCirclesUIState();
		_interface.SetState(_state);
	}

	public override void UpdateUI(GameTime gameTime) {
		_oldGameTime = gameTime;
		if (_interface?.CurrentState is not null) {
			_interface.Update(gameTime);
		}
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
		if (index == -1) {
			Mod.Logger.Error("Couldn't find Death Text interface layer ??");
			return;
		}

		bool DrawUI() {
			if (_oldGameTime is not null && _interface?.CurrentState is not null) {
				_interface.Draw(Main.spriteBatch, _oldGameTime);
			}

			return true;
		}

		layers.Insert(index, new LegacyGameInterfaceLayer($"{nameof(NightreignRevives)}:{nameof(PlayerReviveCirclesUI)}", DrawUI));
	}
}
