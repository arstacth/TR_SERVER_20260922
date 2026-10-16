using System;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Park;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet.Send
{
	public sealed class GetMachineInfo : NetPacket
	{
		public GetMachineInfo(Account User, int MachineNum, CapsuleMachineData MachineInfo, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_INFO__VER2_ACK);
			ns.Write(MachineInfo.RealMachineNum);
			// Kind 1011 loads missing CapsuleMachineDlg17.gui. Remap UI kind to 1003
			// (Dlg still may be missing, but matches older capsule2 assets on this client).
			int wireKind = MachineInfo.RealMachineNumKind;
			if (wireKind == 1011)
			{
				wireKind = 1003;
			}
			ns.Write(wireKind);
			if (MachineInfo.isRotate)
			{
				ns.Write(MachineNum);
			}
			else
			{
				ns.Write(-1);
			}
			ns.Write((byte)1);
			ns.Write(0);
			ns.Write(MachineInfo.ItemList.Count);
			foreach (CapsuleMachineItemNew item in MachineInfo.ItemList)
			{
				ns.Write(item.ItemNum);
				ns.Write((int)item.ItemCount);
				ns.Write((int)item.ItemMax);
				ns.Write(item.ItemNum);
				ns.Write(item.Level);
			}
			WriteUserMachineTrailer(ns, User, MachineInfo.RealMachineNumKind);
		}

		/// <summary>
		/// 20-byte trailer: myPoint, usingPoint, pickCount, pickMax, 4 flag bytes, pad int.
		/// Matches client onRecvCapsuleMachineGetMachineInfo; do not append TCP last.
		/// </summary>
		internal static void WriteUserMachineTrailer(PacketWriter ns, Account User, int machineKind)
		{
			int myPoint = 0;
			int usingPoint = 0;
			int pickCount = 0;
			int pickMax = 0;
			byte isWinner = 0;
			byte isToday = 0;
			byte isOlympic = 0;
			byte luckyCount = 0;
			if (User == null)
			{
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.Write(isWinner);
				ns.Write(isToday);
				ns.Write(isOlympic);
				ns.Write(luckyCount);
				ns.Write(0);
				return;
			}
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				int priceType = 0;
				int priceV1 = 0;
				int priceV2 = 0;
				using (MySqlCommand opt = new MySqlCommand(
					"SELECT fdOption, fdValue1, fdValue2, fdValue3 FROM EssenCapsuleMachineOption WHERE fdMachineKind=@k",
					conn))
				{
					opt.Parameters.AddWithValue("@k", machineKind);
					using MySqlDataReader r = opt.ExecuteReader();
					while (r.Read())
					{
						int option = r.GetInt32(0);
						int v1 = r.GetInt32(1);
						int v2 = r.GetInt32(2);
						int v3 = r.IsDBNull(3) ? 0 : r.GetInt32(3);
						if (option == 1)
						{
							priceType = v1;
							priceV1 = v2;
							priceV2 = v3;
						}
						else if (option == 2)
						{
							pickMax = v1;
						}
					}
				}
				if (priceType == 5)
				{
					usingPoint = priceV2 > 0 ? priceV2 : 1;
					using (MySqlCommand own = new MySqlCommand(
						"SELECT fdCount FROM tblAvatarUser WHERE fdUserNum=@u AND fdItemDescNum=@i LIMIT 1",
						conn))
					{
						own.Parameters.AddWithValue("@u", User.UserNum);
						own.Parameters.AddWithValue("@i", priceV1);
						object o = own.ExecuteScalar();
						if (o != null && o != DBNull.Value)
						{
							myPoint = Convert.ToInt32(o);
						}
					}
				}
				else
				{
					usingPoint = priceV1;
					myPoint = priceV1;
				}
				using (MySqlCommand pick = new MySqlCommand(
					"SELECT fdPickupCount, fdLuckyStepPickUpCount FROM UserCapsuleMachinePickupCount WHERE fdUserNum=@u AND fdMachineKind=@k LIMIT 1",
					conn))
				{
					pick.Parameters.AddWithValue("@u", User.UserNum);
					pick.Parameters.AddWithValue("@k", machineKind);
					using MySqlDataReader r = pick.ExecuteReader();
					if (r.Read())
					{
						pickCount = r.GetInt32(0);
						luckyCount = (byte)Math.Min(255, r.GetInt32(1));
						if (pickCount > 0)
						{
							isToday = 1;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("capsule trailer user={0} kind={1}: {2}", User.UserID, machineKind, ex.Message);
			}
			ns.Write(myPoint);
			ns.Write(usingPoint);
			ns.Write(pickCount);
			ns.Write(pickMax);
			ns.Write(isWinner);
			ns.Write(isToday);
			ns.Write(isOlympic);
			ns.Write(luckyCount);
			// Kind 1011 loads CapsuleMachineDlg17.gui (missing on this client).
			// No further trailer — RemainSize=0 after flags.
		}
	}
}
