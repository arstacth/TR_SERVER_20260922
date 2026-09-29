using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Connections;

namespace RelayServer.Network.Packet.AgentServer
{
	public class AgentServerHandle
	{
		private static AgentConnection _mCurrentAgentServer;

		public static AgentConnection CurrentAgentServer => _mCurrentAgentServer;

		public static void Handle_RelayRegisterResult(AgentConnection con, PacketReader reader)
		{
			bool num = reader.ReadBoolean();
			if (num)
			{
				Log.Info("LoginServer successfully installed");
			}
			else
			{
				Log.Info("Some problems are appear while installing LoginServer");
			}
			if (num)
			{
				_mCurrentAgentServer = con;
			}
		}

		public static void Handle_RemoveClient(AgentConnection con, PacketReader reader)
		{
			int num = reader.ReadLEInt32();
			if (ClientConnection.CurrentAccounts.TryRemove(num, out var value))
			{
				Log.Info("Client Session: {0} IP: {1} Remove Success", num, value.IP);
			}
			else
			{
				Log.Info("Client Session: {0} Remove Fail", num);
			}
		}
	}
}
