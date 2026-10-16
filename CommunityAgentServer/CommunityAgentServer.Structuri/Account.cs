using CommunityAgentServer.Network.Connections;

namespace CommunityAgentServer.Structuring
{
	public class Account
	{
		public string NickName { get; set; }

		public ClientConnection Connection { get; set; }

		public long LastPingTime { get; set; }

		public bool isDisconnected { get; set; }
	}
}
