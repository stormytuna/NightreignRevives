using System.Collections.Generic;
using Terraria.UI;

namespace NightreignRevives.Core;

public class ReplaceDeathScreen : ModSystem
{
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Death Text"));
		if (index == -1) {
			Mod.Logger.Error("Couldn't find Death Text interface layer ??");
			return;
		}

		layers[index].Active = false;
		// TODO: Add own custom death screen!
	}
}
