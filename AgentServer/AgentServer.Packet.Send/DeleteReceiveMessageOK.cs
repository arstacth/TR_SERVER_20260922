using System;
using System.Linq;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DeleteReceiveMessageOK : NetPacket
	{
		public DeleteReceiveMessageOK(int messageType, byte isAll, string msgnumstr, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MESSAGE_DELETE_ACK);
			ns.Write(0);
			ns.Write(messageType);
			ns.Write(isAll);
			if (msgnumstr != string.Empty)
			{
				string[] array = msgnumstr.Remove(msgnumstr.Length - 1).Split(',');
				ns.Write(array.Count());
				string[] array2 = array;
				foreach (string value in array2)
				{
					ns.Write(Convert.ToInt64(value));
				}
			}
			else
			{
				ns.Write(0);
			}
			_ = last;
		}
	}
}
