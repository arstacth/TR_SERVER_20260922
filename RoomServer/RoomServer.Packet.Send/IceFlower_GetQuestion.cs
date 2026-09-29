using System;
using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.IceFlower;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class IceFlower_GetQuestion : NetPacket
	{
		public IceFlower_GetQuestion(int index, List<IceFlowerText> texts, float end_timing, bool isGameOver, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ICEFLOWER_QUESTION_ACK);
			ns.Write(index);
			ns.Write(texts.Count);
			foreach (IceFlowerText text in texts)
			{
				ns.WriteBIG5Fixed_shortSize(text.Text);
				float[] array = new float[3] { 0.2f, 1f, 1.5f };
				ns.Write(array[new Random(Guid.NewGuid().GetHashCode()).Next(array.Length)]);
			}
			ns.Write(0);
			ns.Write(end_timing);
			ns.Write(isGameOver);
			_ = last;
		}
	}
}
