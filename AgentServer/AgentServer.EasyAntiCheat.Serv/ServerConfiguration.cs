namespace AgentServer.EasyAntiCheat.Server
{
	public struct ServerConfiguration
	{
		public static ServerConfiguration Default
		{
			get
			{
				ServerConfiguration result = default(ServerConfiguration);
				result.RegisterTimeout = 60;
				result.ServerName = "N/A";
				return result;
			}
		}

		public int RegisterTimeout { get; set; }

		public string ServerName { get; set; }

		public ServerConfiguration(string ServerName)
			: this(60, ServerName)
		{
		}

		public ServerConfiguration(int RegisterTimeout, string ServerName)
		{
			this = default(ServerConfiguration);
			this.RegisterTimeout = RegisterTimeout;
			this.ServerName = ServerName;
		}
	}
}
