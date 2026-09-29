using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Database
{
	public sealed class DBHelp
	{
		public static string ReadTable(string query, string column)
		{
			string result = string.Empty;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetString(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTable:\r\n{0}", ex.ToString());
				return string.Empty;
			}
			finally
			{
				mySqlConnection.Close();
			}
		}

		public static string ReadTable(string query, string column, params MySqlParameter[] parameter)
		{
			string result = string.Empty;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetString(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTable:\r\n{0}", ex.ToString());
				return string.Empty;
			}
			finally
			{
				mySqlConnection.Close();
			}
		}

		public static int ReadTableToInt(string query, string column)
		{
			int result = 0;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetInt32(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToInt:\r\n{0}", ex.ToString());
				return 0;
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static int ReadTableToInt(string query, string column, params MySqlParameter[] parameter)
		{
			int result = 0;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetInt32(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToInt:\r\n{0}", ex.ToString());
				return 0;
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static long ReadTableToLong(string query, string column)
		{
			long result = 0L;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetInt64(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToLong:\r\n{0}", ex.ToString());
				return 0L;
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static long ReadTableToLong(string query, string column, params MySqlParameter[] parameter)
		{
			long result = 0L;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.GetInt64(column);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToLong:\r\n{0}", ex.ToString());
				return 0L;
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static string[] ReadTableToStringArray(string query, string column)
		{
			List<string> list = new List<string>();
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					list.Add(mySqlDataReader[column].ToString());
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToStringArray:\r\n{0}", ex.ToString());
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
			return list.ToArray();
		}

		public static string[] ReadTableToStringArray(string query, string column, params MySqlParameter[] parameter)
		{
			List<string> list = new List<string>();
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					list.Add(mySqlDataReader[column].ToString());
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToStringArray:\r\n{0}", ex.ToString());
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
			return list.ToArray();
		}

		public static int[] ReadTableToIntArray(string query, string column)
		{
			List<int> list = new List<int>();
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					list.Add(Convert.ToInt32(mySqlDataReader[column]));
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToIntArray:\r\n{0}", ex.ToString());
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
			return list.ToArray();
		}

		public static int[] ReadTableToIntArray(string query, string column, params MySqlParameter[] parameter)
		{
			List<int> list = new List<int>();
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					list.Add(Convert.ToInt32(mySqlDataReader[column]));
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on ReadTableToIntArray:\r\n{0}", ex.ToString());
			}
			finally
			{
				mySqlCommand.Cancel();
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
			return list.ToArray();
		}

		public static bool ExecuteCommand(string query)
		{
			bool flag = false;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
			try
			{
				mySqlConnection.Open();
				mySqlCommand.CommandText = query;
				mySqlCommand.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ExecuteCommand:\r\n{0}", ex.ToString());
				return false;
			}
			finally
			{
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static bool ExecuteCommand(string query, params MySqlParameter[] parameter)
		{
			bool flag = false;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
			try
			{
				mySqlConnection.Open();
				mySqlCommand.CommandText = query;
				mySqlCommand.Parameters.AddRange(parameter);
				mySqlCommand.ExecuteNonQuery();
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Error on ExecuteCommand:\r\n{0}", ex.ToString());
				return false;
			}
			finally
			{
				mySqlConnection.Close();
				mySqlConnection.Dispose();
			}
		}

		public static bool CheckTable(string query)
		{
			bool result = false;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.HasRows;
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on CheckTable:\r\n{0}", ex.ToString());
				return false;
			}
			finally
			{
				mySqlConnection.Close();
			}
		}

		public static bool CheckTable(string query, params MySqlParameter[] parameter)
		{
			bool result = false;
			MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.CreateCommand();
			MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
			mySqlCommand.Parameters.AddRange(parameter);
			try
			{
				mySqlConnection.Open();
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = mySqlDataReader.HasRows;
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("Error on CheckTable:\r\n{0}", ex.ToString());
				return false;
			}
			finally
			{
				mySqlConnection.Close();
			}
		}
	}
}
