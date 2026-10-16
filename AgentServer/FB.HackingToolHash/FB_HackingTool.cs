using System.Collections.Generic;
using FlatBuffers;

namespace FB.HackingToolHash
{
	public class FB_HackingTool
	{
		public static byte[] GetFB_HackingTool(List<string> list)
		{
			FlatBufferBuilder flatBufferBuilder = new FlatBufferBuilder(1);
			Offset<HackingToolInfo>[] array = new Offset<HackingToolInfo>[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				StringOffset hashOffset = flatBufferBuilder.CreateBig5String(list[i]);
				Offset<HackingToolInfo> offset = (array[i] = HackingToolInfo.CreateHackingToolInfo(flatBufferBuilder, hashOffset, enable: true));
			}
			VectorOffset hacktoolistOffset = HackingToolHash.CreateHacktoolistVector(flatBufferBuilder, array);
			HackingToolHash.StartHackingToolHash(flatBufferBuilder);
			HackingToolHash.AddHacktoolist(flatBufferBuilder, hacktoolistOffset);
			Offset<HackingToolHash> offset2 = HackingToolHash.EndHackingToolHash(flatBufferBuilder);
			HackingToolHash.FinishHackingToolHashBuffer(flatBufferBuilder, offset2);
			return flatBufferBuilder.SizedByteArray();
		}
	}
}
