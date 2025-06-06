namespace NightreignRevives.Core;

public class SingleplayerWarningSystem : ModSystem
{
	private bool _warned = false;
	
	public override void PreUpdatePlayers() {
		if (!_warned && Main.netMode == NetmodeID.SinglePlayer) {
			_warned = true;
			Main.NewText("[WARNING] You are playing Nightreign Revives in Singleplayer mode.", Color.Orange);
			Main.NewText("This mod does nothing in Singleplayer, please disable it for a more stable experience.", Color.Orange);
		}
	}
}
