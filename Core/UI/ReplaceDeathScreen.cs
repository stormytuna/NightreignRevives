using System.Collections.Generic;
using Terraria.UI;

namespace NightreignRevives.Core.UI;

[Autoload(Side = ModSide.Client)]
public class ReplaceDeathScreen : ModSystem
{
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
		if (index == -1) {
			Mod.Logger.Error("Couldn't find Death Text interface layer ??");
			return;
		}

		if (Main.LocalPlayer.NoReviveNPC()) {
			return;
		}

		layers[index].Active = false;
	}
}
