using System;
using System.Data;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using TRCommon;

namespace RoomServer.Database
{
	public class MySqlCommandHelper : IDisposable
	{
		private MySqlConnection connection;

		private MySqlDataReader reader;

		private MySqlCommand sqlCmd = new MySqlCommand();

		private string m_strSPName;

		public bool HasRows => reader.HasRows;

		public MySqlCommandHelper(string USPName, CommandType cmdType = CommandType.StoredProcedure)
		{
			m_strSPName = USPName;
			connection = new MySqlConnection(Conf.Connstr);
			connection.Open();
			sqlCmd.Connection = connection;
			sqlCmd.CommandType = cmdType;
			sqlCmd.CommandText = m_strSPName;
		}

		public void AddParamBoolean(string szName, bool value)
		{
			AddParamByte(szName, (byte)(value ? 1 : 0));
		}

		public void AddParamByte(string szName, byte value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.Byte).Value = value;
		}

		public void AddParamShort(string szName, short value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.Int16).Value = value;
		}

		public void AddParamInt(string szName, int value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.Int32).Value = value;
		}

		public void AddParamLong(string szName, long value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.Int64).Value = value;
		}

		public void AddParamFloat(string szName, float value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.Float).Value = value;
		}

		public void AddParamDateTime(string szName, DateTime value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.DateTime).Value = value.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public void AddParamVarString(string szName, string value)
		{
			sqlCmd.Parameters.Add(szName, MySqlDbType.VarString).Value = value;
		}

		public void Execute()
		{
			reader = sqlCmd.ExecuteReader();
		}

		public void ExecuteSingle()
		{
			reader = sqlCmd.ExecuteReader(CommandBehavior.SingleRow);
		}

		public void ExecuteNonQuery()
		{
			sqlCmd.ExecuteNonQuery();
		}

		public bool HasResult()
		{
			if (reader != null)
			{
				if (reader.HasRows)
				{
					return reader.Read();
				}
				return false;
			}
			return false;
		}

		public void NextResult()
		{
			reader.NextResult();
		}

		public bool GetBoolean(string szName)
		{
			return reader.GetBoolean(szName);
		}

		public byte GetByte(string szName)
		{
			return reader.GetByte(szName);
		}

		public byte GetByteConvert(string szName)
		{
			return Convert.ToByte(reader[szName]);
		}

		public short GetShort(string szName)
		{
			return reader.GetInt16(szName);
		}

		public short GetShortConvert(string szName)
		{
			return Convert.ToInt16(reader[szName]);
		}

		public ushort GetUShort(string szName)
		{
			return reader.GetUInt16(szName);
		}

		public int GetInt(string szName)
		{
			return reader.GetInt32(szName);
		}

		public int GetIntConvert(string szName)
		{
			return Convert.ToInt32(reader[szName]);
		}

		public uint GetUInt(string szName)
		{
			return reader.GetUInt32(szName);
		}

		public float GetFloat(string szName)
		{
			return reader.GetFloat(szName);
		}

		public long GetLong(string szName)
		{
			return reader.GetInt64(szName);
		}

		public long GetLongConvert(string szName)
		{
			return Convert.ToInt64(reader[szName]);
		}

		public string GetString(string szName)
		{
			return reader.GetString(szName);
		}

		public long GetDateTime(string szName, long defaultValue = 0L)
		{
			if (!IsDBNull(szName))
			{
				if (reader.GetString(szName) == "0")
				{
					if (defaultValue <= 0)
					{
						return 0L;
					}
					return defaultValue;
				}
				return Utility.ConvertToTimestamp(reader.GetDateTime(szName));
			}
			if (defaultValue <= 0)
			{
				return 0L;
			}
			return defaultValue;
		}

		public byte GetByte(int cid)
		{
			return Convert.ToByte(reader.GetValue(cid));
		}

		public ushort GetUShort(int cid)
		{
			return Convert.ToUInt16(reader.GetValue(cid));
		}

		public bool IsDBNull(string szName)
		{
			return reader.IsDBNull(reader.GetOrdinal(szName));
		}

		public void getResultAvatarInfo(ref AvatarInfo avatarInfo, bool bCostume = false)
		{
			if (bCostume)
			{
				avatarInfo.m_character = GetUShort("cos_character");
				avatarInfo.m_head = GetUShort("cos_head");
				avatarInfo.m_topBody = GetUShort("cos_topBody");
				avatarInfo.m_downBody = GetUShort("cos_downBody");
				avatarInfo.m_foot = GetUShort("cos_foot");
				avatarInfo.m_acHead = GetUShort("cos_acHead");
				avatarInfo.m_acHand = GetUShort("cos_acHand");
				avatarInfo.m_acFace = GetUShort("cos_acFace");
				avatarInfo.m_acBack = GetUShort("cos_acBack");
				avatarInfo.m_acNeck = GetUShort("cos_acNeck");
				avatarInfo.m_pet = GetUShort("cos_pet");
				avatarInfo.m_expansion = GetUShort("cos_expansion");
				avatarInfo.m_acWrist = GetUShort("cos_acWrist");
				avatarInfo.m_acBooster = GetUShort("cos_acBooster");
				avatarInfo.m_accTail = GetUShort("cos_acTail");
			}
			else
			{
				avatarInfo.m_character = GetUShort("character");
				avatarInfo.m_head = GetUShort("head");
				avatarInfo.m_topBody = GetUShort("topBody");
				avatarInfo.m_downBody = GetUShort("downBody");
				avatarInfo.m_foot = GetUShort("foot");
				avatarInfo.m_acHead = GetUShort("acHead");
				avatarInfo.m_acHand = GetUShort("acHand");
				avatarInfo.m_acFace = GetUShort("acFace");
				avatarInfo.m_acBack = GetUShort("acBack");
				avatarInfo.m_acNeck = GetUShort("acNeck");
				avatarInfo.m_pet = GetUShort("pet");
				avatarInfo.m_expansion = GetUShort("expansion");
				avatarInfo.m_acWrist = GetUShort("acWrist");
				avatarInfo.m_acBooster = GetUShort("acBooster");
				avatarInfo.m_accTail = GetUShort("acTail");
			}
		}

		protected virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		~MySqlCommandHelper()
		{
			Dispose();
		}

		void IDisposable.Dispose()
		{
			if (connection != null)
			{
				if (reader != null)
				{
					reader.Close();
					reader.Dispose();
				}
				sqlCmd.Dispose();
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
					connection.Dispose();
				}
			}
		}
	}
}
