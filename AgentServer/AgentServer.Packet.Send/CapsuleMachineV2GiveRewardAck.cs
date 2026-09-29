using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Capsule V2 GiveReward ACK (wire 2179, ack 11). Milestone claim stub — no grant.
	/// </summary>
	public sealed class CapsuleMachineV2GiveRewardAck : NetPacket
	{
		public CapsuleMachineV2GiveRewardAck(int machineItemNum, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_CapsuleMachineV2Protocol);
			ns.Write(11);
			ns.Write(1);
			ns.Write(machineItemNum);
			ns.Write(0);
			ns.Write(0);
			ns.Write((byte)0);
		}
	}
}
