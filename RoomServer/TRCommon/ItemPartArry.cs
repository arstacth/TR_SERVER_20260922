using System.Runtime.InteropServices;

namespace TRCommon
{
	public struct ItemPartArry
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
		public unsafe fixed ushort Arry[15];

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
