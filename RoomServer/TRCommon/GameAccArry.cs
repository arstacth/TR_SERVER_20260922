using System.Runtime.InteropServices;

namespace TRCommon
{
	public struct GameAccArry
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
		public unsafe fixed ushort Arry[7];

		public unsafe ushort this[int i]
		{
			get
			{
				return Arry[i];
			}
			set
			{
				Arry[i] = value;
			}
		}
	}
}
