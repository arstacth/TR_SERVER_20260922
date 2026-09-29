using System;
using System.Runtime.InteropServices;

namespace AgentServer.EasyAntiCheat.Server.Hydra
{
	internal static class NativeModule
	{
		public struct ClientUpdate
		{
			public IntPtr ClientObject;

			public ClientStatus Status;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
			public char[] Message;

			public long TimeBanExpires;
		}

		public delegate void LogEventHandler(LogLevel LogLevel, [MarshalAs(UnmanagedType.LPStr)] string Message);

		private const string DllFileName = "bin\\eac_server.dll";

		private static readonly string InterfaceVersion;

		private static IntPtr Instance;

		private static byte[] StaticMsgBuf;

		static NativeModule()
		{
			InterfaceVersion = "GameServerInterfaceV016";
			Instance = IntPtr.Zero;
			StaticMsgBuf = new byte[256];
		}

		[DllImport("bin\\eac_server.dll", EntryPoint = "CreateGameServer")]
		private static extern IntPtr dotCreateGameServer(string InterfaceVersion);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_InitializeWithGameID")]
		private static extern bool dotInitializeWithGameID(IntPtr Instance, uint GameID, uint RegisterTimeout, [MarshalAs(UnmanagedType.LPStr)] string ServerName);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_Destroy")]
		private static extern void dotDestroy(IntPtr Instance);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_SetMaxAllowedMessageLength")]
		private static extern void dotSetMaxAllowedMessageLength(IntPtr Instance, UIntPtr ClientObject, uint MaxMessageLength);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_Cerberus")]
		private static extern IntPtr dotCerberus(IntPtr Instance);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_NetProtect")]
		private static extern IntPtr dotNetProtect(IntPtr Instance);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_SetClientNetworkState")]
		private static extern void dotSetClientNetworkState(IntPtr Instance, UIntPtr ClientObject, [MarshalAs(UnmanagedType.Bool)] bool NetworkActive);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_SetEACServer")]
		private static extern void dotSetEACServer(IntPtr Instance, [MarshalAs(UnmanagedType.LPStr)] string EACServerName);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_GetGameID")]
		private static extern uint dotGetGameID(IntPtr Instance);

		public static bool Initialize(ServerConfiguration config)
		{
			Instance = dotCreateGameServer(InterfaceVersion);
			return dotInitialize(Instance, (uint)config.RegisterTimeout, config.ServerName);
		}

		public static bool InitializeWithGameID(int GameID, ServerConfiguration config)
		{
			if (GameID > 0 && Instance == IntPtr.Zero)
			{
				Instance = dotCreateGameServer(InterfaceVersion);
				dotInitializeWithGameID(Instance, (uint)GameID, (uint)config.RegisterTimeout, config.ServerName);
			}
			return Instance != IntPtr.Zero;
		}

		public static void Unload()
		{
			if (Instance != IntPtr.Zero)
			{
				dotDestroy(Instance);
				Instance = IntPtr.Zero;
			}
		}

		public static bool RegisterClient(int ClientObject, string PlayerGUID, string PlayerIP, string OwnerGUID, string PlayerName, PlayerRegisterFlags Flags)
		{
			return dotRegisterClient(Instance, (UIntPtr)(ulong)ClientObject, PlayerGUID, PlayerIP, OwnerGUID, PlayerName, (uint)Flags);
		}

		public static int GenerateCompatibilityClientID()
		{
			return (int)dotGenerateCompatibilityClientID(Instance);
		}

		public static void UnregisterClient(int ClientObject)
		{
			dotUnregisterClient(Instance, (UIntPtr)(ulong)ClientObject);
		}

		public unsafe static int GetNextClientUpdate(int ClientObject, out ClientUpdate Msg)
		{
			byte* ptr = stackalloc byte[280];
			ClientObject = (int)(uint)dotGetNextClientUpdate(Instance, (UIntPtr)(ulong)ClientObject, ptr);
			Msg = ((ClientObject != 0) ? ((ClientUpdate)Marshal.PtrToStructure(new IntPtr(ptr), typeof(ClientUpdate))) : default(ClientUpdate));
			return ClientObject;
		}

		public static int PopNetworkMessage(int ClientObject, out byte[] MessageBuffer, out int MessageLength)
		{
			uint MessageLength2 = 0u;
			IntPtr MessageBuffer2 = IntPtr.Zero;
			MessageLength = 0;
			ClientObject = (int)(uint)dotPopNetworkMessage(Instance, (UIntPtr)(ulong)ClientObject, ref MessageBuffer2, out MessageLength2);
			if (ClientObject != 0)
			{
				MessageLength = (int)MessageLength2;
				Array.Clear(StaticMsgBuf, 0, StaticMsgBuf.Length);
				Marshal.Copy(MessageBuffer2, StaticMsgBuf, 0, MessageLength);
			}
			MessageBuffer = StaticMsgBuf;
			return ClientObject;
		}

		public static void SetMaxAllowedMessageLength(int ClientObject, int MaxMessageLength)
		{
			dotSetMaxAllowedMessageLength(Instance, (UIntPtr)(ulong)ClientObject, (uint)MaxMessageLength);
		}

		public static void PushNetworkMessage(int ClientObject, byte[] MessageBuffer, int MessageLength)
		{
			dotPushNetworkMessage(Instance, (UIntPtr)(ulong)ClientObject, MessageBuffer, (uint)MessageLength);
		}

		public static IntPtr Cerberus()
		{
			return dotCerberus(Instance);
		}

		public static IntPtr NetProtect()
		{
			return dotNetProtect(Instance);
		}

		public static void SetClientNetworkState(int ClientObject, bool NetworkActive)
		{
			dotSetClientNetworkState(Instance, (UIntPtr)(ulong)ClientObject, NetworkActive);
		}

		public static void SetEACServer(string EACServerName)
		{
			dotSetEACServer(Instance, EACServerName);
		}

		public static int GetGameID()
		{
			if (!(Instance != IntPtr.Zero))
			{
				return 0;
			}
			return (int)dotGetGameID(Instance);
		}

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_Initialize")]
		private static extern bool dotInitialize(IntPtr Instance, uint RegisterTimeout, [MarshalAs(UnmanagedType.LPStr)] string ServerName);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_SetLogCallback")]
		private static extern void dotSetLogCallback(IntPtr Instance, LogEventHandler LogCallback, LogLevel LogLevel);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_RegisterClient")]
		private static extern bool dotRegisterClient(IntPtr Instance, UIntPtr ClientObject, [MarshalAs(UnmanagedType.LPStr)] string PlayerGUID, [MarshalAs(UnmanagedType.LPStr)] string PlayerIP, [MarshalAs(UnmanagedType.LPStr)] string OwnerGUID, [MarshalAs(UnmanagedType.LPStr)] string PlayerName, uint Flags);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_UnregisterClient")]
		private static extern void dotUnregisterClient(IntPtr Instance, UIntPtr ClientObject);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_GetNextClientUpdate")]
		private unsafe static extern UIntPtr dotGetNextClientUpdate(IntPtr Instance, UIntPtr ClientObject, byte* ClientUpdate);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_GenerateCompatibilityClientID")]
		private static extern uint dotGenerateCompatibilityClientID(IntPtr Instance);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_PopNetworkMessage")]
		private static extern UIntPtr dotPopNetworkMessage(IntPtr Instance, UIntPtr ClientObject, ref IntPtr MessageBuffer, out uint MessageLength);

		[DllImport("bin\\eac_server.dll", EntryPoint = "GameServerHydra_PushNetworkMessage")]
		private static extern void dotPushNetworkMessage(IntPtr Instance, UIntPtr ClientObject, byte[] MessageBuffer, uint MessageLength);
	}
}
