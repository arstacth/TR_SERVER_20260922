using System;

namespace AgentServer.EasyAntiCheat.Server.Hydra
{
	public struct Client : IEquatable<Client>
	{
		public int ClientID { get; set; }

		internal Client(int clientID)
		{
			this = default(Client);
			ClientID = clientID;
		}

		public bool Equals(Client other)
		{
			return ClientID == other.ClientID;
		}

		public override int GetHashCode()
		{
			return ClientID;
		}
	}
}
