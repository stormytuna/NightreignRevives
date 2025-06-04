using System;
using System.IO;
using NightreignRevives.Content;
using NightreignRevives.Core;
using Terraria.Chat;

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

		// TODO: move to classes themselves, switch case is annoying with var names in scope
		// TODO: handle stuff without assumption we're sending from server
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
