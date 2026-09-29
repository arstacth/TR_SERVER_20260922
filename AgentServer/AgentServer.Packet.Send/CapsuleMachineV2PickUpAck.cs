using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Capsule V2 PickUP ACK (wire 2179, ack 7 = PICK_UP_ACK).
	/// Parser after result=0: machineNum, then 0x7912b0 body
	/// (5 ints + item count + items: i64, i32, i32, u8, u8).
	/// </summary>
	public sealed class CapsuleMachineV2PickUpAck : NetPacket
	{
		public CapsuleMachineV2PickUpAck(int machineItemNum, byte ok, int resultItemNum, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_CapsuleMachineV2Protocol);
			ns.Write(7);
			if (ok == 0 || resultItemNum <= 0)
			{
				ns.Write(1);
				ns.Write(machineItemNum);
				return;
			}
			ns.Write(0);
			ns.Write(machineItemNum);
			// 7912b0 header fields (RealMachineNum, kind, rotate, point, reset).
			ns.Write(machineItemNum);
			ns.Write(0);
			ns.Write(-1);
			ns.Write(0);
			ns.Write(0);
			ns.Write(1);
			ns.Write((long)resultItemNum);
			ns.Write(1);
			ns.Write(1);
			ns.Write((byte)0);
			ns.Write((byte)1);
		}
	}
}
