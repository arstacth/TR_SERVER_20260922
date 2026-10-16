using System;

namespace AgentServer.EasyAntiCheat.Server.Hydra
{
	public sealed class ClientStatusUpdate<TClient> where TClient : IEquatable<TClient>
	{
		private DateTime? timeBanExpires;

		public TClient ClientObject { get; set; }

		public string Message { get; set; }

		public bool RequiresKick
		{
			get
			{
				if (Status != ClientStatus.ClientAuthenticatedLocal)
				{
					return Status != ClientStatus.ClientAuthenticatedRemote;
				}
				return false;
			}
		}

		public ClientStatus Status { get; set; }

		internal ClientStatusUpdate(TClient clientObject, ClientStatus clientStatus, DateTime? timeBanExpires, string message = null)
		{
			ClientObject = clientObject;
			Status = clientStatus;
			this.timeBanExpires = timeBanExpires;
			Message = message ?? string.Empty;
		}

		public bool IsBanned(out DateTime? timeBanExpires)
		{
			timeBanExpires = this.timeBanExpires;
			return Status == ClientStatus.ClientBanned;
		}

		public override string ToString()
		{
			object[] obj = new object[4] { "ClientID(", null, null, null };
			int num = 1;
			obj[num] = ClientObject.ToString();
			obj[2] = "): ";
			obj[3] = Status;
			return string.Concat(obj);
		}
	}
}
