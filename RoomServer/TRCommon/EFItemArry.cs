using System.Runtime.InteropServices;

namespace TRCommon
{
	public struct EFItemArry
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public unsafe fixed ushort Arry[1];

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
