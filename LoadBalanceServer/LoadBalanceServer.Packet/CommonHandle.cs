using LoadBalanceServer.Network.Connections;
using LoadBalanceServer.Packet.Send;
using LocalCommons.Network;
using Serilog;

namespace LoadBalanceServer.Packet
{
	public class CommonHandle
	{
		public static void Handle_0x01(ClientConnection Client, PacketReader reader)
		{
			reader.ReadInt32();
			reader.ReadInt32();
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			Log.Information("Client Hash: {0}", text);
			if (Conf.HashCheck && !CheckHashIsValid(text))
			{
				Log.Warning("InCorrect Hash: {0}, IP:{1}", text, Client.IP);
				Client.SendAsync(new HashCheckFail());
			}
			else
			{
				Client.SendAsync(new HashCheckOK());
			}
		}

		private static bool CheckHashIsValid(string hash)
		{
			return Server.HashList.Exists((string serverhash) => serverhash == hash);
		}
	}
}
