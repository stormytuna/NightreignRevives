using System.IO;
using NightreignRevives.Content;
using NightreignRevives.Core;

namespace NightreignRevives;

public class NightreignRevives : Mod
{
	public enum MessageType : byte
	{
		PlayerRevived,
		HitReviveNPC,
	}

	public override void HandlePacket(BinaryReader reader, int whoAmI) {
		MessageType message = (MessageType)reader.ReadByte();

		switch (message) {
			case MessageType.PlayerRevived:
				int playerWhoAmI = reader.Read7BitEncodedInt();
				Main.player[playerWhoAmI].GetModPlayer<NightreignRevivePlayer>().Revive();
				break;
			case MessageType.HitReviveNPC:
				int npcWhoAmI = reader.Read7BitEncodedInt();
				ReviveCircleNPC.ReceiveReviveNPCHit(npcWhoAmI);

				if (Main.netMode == NetmodeID.Server) {
					ReviveCircleNPC.BroadcastReviveNPCHit(-1, whoAmI, npcWhoAmI);
				}

				break;
			default:
				Logger.Error("Unknown message type: " + message);
				return;
		}
	}
}
