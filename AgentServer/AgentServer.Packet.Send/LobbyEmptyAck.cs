using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LobbyEmptyAck : NetPacket
	{
		public LobbyEmptyAck(ushort ackOp, byte last, params int[] values)
			: this(ackOp, last, writeLast: true, values)
		{
		}

		public LobbyEmptyAck(ushort ackOp, byte last, bool writeLast, params int[] values)
		{
			ns.WriteOP(ackOp);
			if (values != null)
			{
				foreach (int value in values)
				{
					ns.Write(value);
				}
			}
			if (writeLast)
			{
				ns.Write(last);
			}
			else
			{
				_ = last;
			}
		}
	}
}
