using System;
using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RequestQuizList_Ack : NetPacket
	{
		public RequestQuizList_Ack(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RUN_QUIZMODE_REQUEST_QUIZ_LIST_ACK);
			ns.Write(0);
			ns.Write(6);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 1; i <= 6; i++)
			{
				int num = RunQuizHolder.RunQuizInfo.Keys.ToList()[new Random(Guid.NewGuid().GetHashCode()).Next(0, RunQuizHolder.RunQuizInfo.Count)];
				ns.Write(i);
				ns.Write(num);
				RunQuizHolder.RunQuizInfo.TryGetValue(num, out var value);
				ns.WriteAnsiFixed_intSize(value);
				dictionary.Add(i, num);
			}
			ns.Write(6);
			for (int j = 1; j <= 6; j++)
			{
				ns.Write(j);
				ns.Write(3);
				int num2 = new Random(Guid.NewGuid().GetHashCode()).Next(1, 4);
				dictionary.TryGetValue(j, out var runquiznum);
				List<int> list = (from w in RunQuizHolder.RunQuizInfo.Keys
					where w != runquiznum
					select w into _
					orderby Guid.NewGuid()
					select _).Take(3).ToList();
				ns.Write((num2 == 1) ? runquiznum : list[0]);
				ns.Write((num2 == 2) ? runquiznum : list[1]);
				ns.Write((num2 == 3) ? runquiznum : list[2]);
			}
			_ = last;
		}
	}
}
