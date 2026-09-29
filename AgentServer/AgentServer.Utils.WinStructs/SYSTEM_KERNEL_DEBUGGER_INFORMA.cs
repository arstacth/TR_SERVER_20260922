using System.Runtime.InteropServices;

namespace AgentServer.Utils.WinStructs
{
	public struct SYSTEM_KERNEL_DEBUGGER_INFORMATION
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool KernelDebuggerEnabled;

		[MarshalAs(UnmanagedType.U1)]
		public bool KernelDebuggerNotPresent;
	}
}
