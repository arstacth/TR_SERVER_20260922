using System;
using System.Net.Sockets;
using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Packet.AgentServer;
using RelayServer.Structuring.Opcode;

namespace RelayServer.Network.Connections
{
	public sealed class AgentConnection : IConnection
	{
		public AgentConnection(Socket socket)
			: base(socket)
		{
			Log.Info("Connected to AgentServer, installing data...");
			base.DisconnectedEvent += LoginConnection_DisconnectedEvent;
			SendAsync(new Net_RegisterRelayServer());
		}

		private void LoginConnection_DisconnectedEvent(object sender, EventArgs e)
		{
			Log.Info("AgentServer IP: {0} disconnected", this);
			Dispose();
		}

		public override void HandleReceived(byte[] data)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			switch ((eRelayAgentProtocol)packetReader.ReadByte())
			{
			case eRelayAgentProtocol.REGISTER_RESULT:
				AgentServerHandle.Handle_RelayRegisterResult(this, packetReader);
				break;
			case eRelayAgentProtocol.REMOVE_CLIENT:
				AgentServerHandle.Handle_RemoveClient(this, packetReader);
				break;
			}
		}
	}
}
