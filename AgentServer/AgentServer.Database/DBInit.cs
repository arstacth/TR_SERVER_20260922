using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Database
{
	public class DBInit
	{
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(8.0);

		public static byte[] Levels { get; set; }

		public static byte[] HackTools { get; set; }

		public static byte[] GameServerSetting { get; set; }

		public static byte[] SmartChannelModeInfo { get; set; }

		public static byte[] SmartChannelScheduleInfo { get; set; }

		public static byte[] RoomKindPenaltyInfo { get; set; }

		[DllImport("ntdll.dll")]
		public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);

		[DllImport("ntdll.dll")]
		public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask, IntPtr Parameters, uint ValidResponseOption, out uint Response);

		public static void startServerInitDB()
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_startServerInitDB";
				mySqlCommand.Parameters.Add("sp", MySqlDbType.UInt32).Value = (uint)Utility.IPToInt(Conf.ServerIP);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				Log.Information("Start DB OK {0}...", mySqlDataReader["ret"].ToString());
			}
			catch (Exception)
			{
			}
		}

		public static void initlevel_run()
		{
			try
			{
				PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
				packetWriter.WriteOP(Opcodes.eServer_GET_LEVEL_INFO_ACK);
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					int num = 1;
					while (num <= 8)
					{
						packetWriter.Write((short)num);
						int num2 = 0;
						int num3 = (int)packetWriter.Position;
						packetWriter.Write(num2);
						using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essenlevelinfo WHERE fdLevelKind = " + num, mySqlConnection);
						using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
						while (mySqlDataReader.Read())
						{
							packetWriter.Write(mySqlDataReader.GetInt32("fdLevel"));
							packetWriter.Write(mySqlDataReader.GetInt64("fdExp"));
							num2++;
						}
						int num4 = (int)packetWriter.Position;
						packetWriter.Seek(num3, SeekOrigin.Begin);
						packetWriter.Write(num2);
						packetWriter.Seek(num4, SeekOrigin.Begin);
						num++;
						if (num == 4)
						{
							num += 3;
						}
					}
				}
				Levels = packetWriter.ToArray();
			}
			catch (Exception ex)
			{
				Log.Error("initlevel error:{0}", ex.Message);
			}
		}

		public static void inithacktool_run()
		{
			try
			{
				// Do not keep an opcode-5 buffer. Empty hash ACK still triggers Recv HackingTool.
				HackTools = null;
			}
			catch (Exception ex)
			{
				Log.Error("inithacktool error:{0}", ex.Message);
			}
		}

		public static void initSmartChannelModeInfo_run()
		{
			try
			{
				PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
				packetWriter.WriteOP(Opcodes.eServer_SMART_CHANNEL_MODE_INFO);
				int num = 0;
				int num2 = (int)packetWriter.Position;
				packetWriter.Write(num);
				Dictionary<byte, byte> nextSlot = new Dictionary<byte, byte>();
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essensmartchannelinfo WHERE fdUse = 1 ORDER BY fdSlotNum, fdModeNum", mySqlConnection);
					using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
					{
						while (mySqlDataReader.Read())
						{
							// Dump used fdModeNum as a unique PK (1..13), so lobby tab 0
							// (Special Event) had no rows. fdSlotNum is the 0-based tab.
							byte tab = Convert.ToByte(mySqlDataReader["fdSlotNum"]);
							byte slot;
							if (!nextSlot.TryGetValue(tab, out slot))
							{
								slot = 0;
							}
							nextSlot[tab] = (byte)(slot + 1);
							packetWriter.Write(tab);
							packetWriter.Write(slot);
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdType"]));
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdMaxUserNum"]));
							packetWriter.Write(Convert.ToInt32(mySqlDataReader["fdMapNum"]));
							packetWriter.Write(Convert.ToInt32(mySqlDataReader["fdMapGroupNum"]));
							packetWriter.Write(Convert.ToInt32(mySqlDataReader["fdItemMode"]));
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdTeamPlayMode"]));
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdSteppingMode"]));
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdMinLevel"]));
							packetWriter.Write(Convert.ToByte(mySqlDataReader["fdMaxLevel"]));
							packetWriter.Write(Convert.ToSingle(mySqlDataReader["fdBonusExp"]));
							packetWriter.Write(Convert.ToSingle(mySqlDataReader["fdBonusGameMoney"]));
							packetWriter.Write(Convert.ToInt32(mySqlDataReader["fdMinUserNum"]));
							num++;
						}
					}
					packetWriter.Seek(num2, SeekOrigin.Begin);
					packetWriter.Write(num);
				}
				SmartChannelModeInfo = packetWriter.ToArray();
			}
			catch (Exception ex)
			{
				Log.Error("initSmartChannelModeInfo error:{0}", ex.Message);
			}
		}

		public static void initSmartChannelScheduleInfo_run()
		{
			try
			{
				PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
				packetWriter.WriteOP(Opcodes.eServer_SMART_CHANNEL_SCHEDULE_INFO);
				int num = 0;
				int num2 = (int)packetWriter.Position;
				packetWriter.Write(num);
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essensmartchannelschedule", mySqlConnection);
					using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
					{
						while (mySqlDataReader.Read())
						{
							packetWriter.Write(ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdStartTime"])));
							packetWriter.Write(ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdEndTime"])));
							num++;
						}
					}
					packetWriter.Seek(num2, SeekOrigin.Begin);
					packetWriter.Write(num);
				}
				SmartChannelScheduleInfo = packetWriter.ToArray();
			}
			catch (Exception ex)
			{
				Log.Error("initSmartChannelScheduleInfo error:{0}", ex.Message);
			}
		}

		public static void initRoomKindPenaltyInfo_run()
		{
			try
			{
				PacketWriter packetWriter = PacketWriter.CreateInstance(32, LittleEndian: true);
				packetWriter.WriteOP(Opcodes.eServer_ROOMKIND_PENALTY_INFO);
				int num = 0;
				int num2 = (int)packetWriter.Position;
				packetWriter.Write(num);
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essenpenaltyinfo", mySqlConnection);
					using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
					{
						while (mySqlDataReader.Read())
						{
							packetWriter.Write((int)mySqlDataReader["fdRoomKindID"]);
							packetWriter.Write((int)mySqlDataReader["fdRestrictTime"] * 1000);
							num++;
						}
					}
					packetWriter.Write((byte)1);
					packetWriter.Seek(num2, SeekOrigin.Begin);
					packetWriter.Write(num);
				}
				RoomKindPenaltyInfo = packetWriter.ToArray();
			}
			catch (Exception ex)
			{
				Log.Error("initRoomKindPenaltyInfo error:{0}", ex.Message);
			}
		}

		private static string ByteArrayToString(byte[] ba)
		{
			StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
			int num = 0;
			foreach (byte b in ba)
			{
				if (num >= ba.Length)
				{
					break;
				}
				stringBuilder.AppendFormat("{0:x2}", b.ToString("X2") + " ");
				num++;
			}
			return stringBuilder.ToString();
		}

		private static long ConvertToTimestamp(DateTime value)
		{
			return (long)(value - Epoch).TotalMilliseconds;
		}
	}
}
