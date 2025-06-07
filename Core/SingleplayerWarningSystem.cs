namespace NightreignRevives.Core;

public class SingleplayerWarningSystem : ModSystem
{
	private int _warnDelay = 60;
	private bool _warned = false;

	public override void PreUpdatePlayers() {
		if (Main.netMode != NetmodeID.SinglePlayer) {
			return;
		}

		_warnDelay--;
		if (_warnDelay <= 0 && !_warned && ServerConfig.Instance.SinglePlayerWarning) {
			_warned = true;
			Main.NewText("[WARNING] You are playing with Nightreign Revives in Singleplayer mode.", Color.Orange);
			Main.NewText("This mod does nothing in Singleplayer, please disable it for a more stable experience.", Color.Orange);
		}
	}
}
