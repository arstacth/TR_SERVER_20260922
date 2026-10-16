using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Structuring;
using Akka.Actor;
using LocalCommons.Network;
using NetMsg.LBS;
using NetMsg.Room;
using Serilog;

namespace AgentServer
{
	public class RoomServer : ReceiveActor
	{
		public static Dictionary<int, IActorRef> RoomServerList = new Dictionary<int, IActorRef>();

		private static int ID = 1;

		private static string RMIP = Conf.RMServerIP;

		private static int RMPORT = Conf.RMLocalPort;

		private static string RMPath = $"akka.tcp://Room@{RMIP}:{RMPORT}/user/AgentClient";

		private static int RMID = 0;

		private readonly ActorSelection _rmserver = UntypedActor.Context.ActorSelection(RMPath);

		public RoomServer()
		{
			Receive(delegate(AgentConnectRequest cr)
			{
				Log.Information("Connecting to RoomServer......");
				_rmserver.Tell(cr);
			});
			Receive(delegate(AgentConnectResponse rsp)
			{
				Log.Information("Connected to RoomServer My ID:{0}", rsp.AgentID);
				ServerStatus.MyAgentID = rsp.AgentID;
				Form1.UpdateLableStatic(ServerStatus.MyAgentID);
				ServerStatus.RoomServerConnected = true;
				RMID = rsp.ConnectedRoomID;
				RoomServerList.Add(rsp.ConnectedRoomID, base.Sender);
				if (ServerSettingHolder.ServerSettings.useThankOfferingSystem)
				{
					ThankOfferingSystem.LoadSchedule();
				}
			});
			Receive(delegate(RemoveRoom rsp)
			{
				Rooms.RemoveRoom(rsp.RoomID);
			});
			Receive(delegate(RemoveFarmRoom rsp)
			{
				Rooms.PublicFarmRoom.TryRemove(rsp.RoomID, out var _);
			});
			Receive(delegate(ReloadSetting re)
			{
				_rmserver.Tell(re);
			});
			new MemoryStream();
			Receive(delegate(byte[] packet)
			{
				byte[] array = new byte[packet.Length];
				Buffer.BlockCopy(packet, 0, array, 0, packet.Length);
				HandlePacket(array);
			});
		}

		private void HandlePacket(byte[] data)
		{
			PacketReader packetReader = new PacketReader(data, 0);
			short num = packetReader.ReadLEInt16();
			packetReader.Buffer.LastOrDefault();
			switch ((RMProtocol)num)
			{
			case RMProtocol.RM_CreateRoom_ACK:
				RoomServerHandle.CreateRoom_AddList(packetReader);
				break;
			case RMProtocol.RM_StartGame_REQ:
				RoomServerHandle.GamerRoom_PlayingUpdate(packetReader);
				break;
			case RMProtocol.RM_ChangeMap_REQ:
				RoomServerHandle.GamerRoom_ChangeMap(packetReader);
				break;
			case RMProtocol.RM_ChangeSetting_REQ:
				RoomServerHandle.GamerRoom_ChangeSetting(packetReader);
				break;
			case RMProtocol.RM_AddPublicFarmList_REQ:
				RoomServerHandle.AddPublicFarmList(packetReader);
				break;
			case RMProtocol.RM_UpdateFarmFishingReward_ACK:
				RoomServerHandle.UpdateFarmFishingReward(packetReader);
				break;
			case RMProtocol.RM_UpdateTeamInfo:
				RoomServerHandle.UpdateTeamInfo(packetReader);
				break;
			}
		}

		private void Handle_ToPlayerPacket(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			if (ClientConnection.CurrentAccounts.TryGetValue(key, out var value))
			{
				ushort length = reader.ReadLEUInt16();
				byte[] packet = reader.ReadByteArray(length);
				value.Connection.SendAsync(packet);
			}
		}
	}
}
