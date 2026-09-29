using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Serilog;

namespace AgentServer.XignCode
{
	internal static class XignCodeRuntime
	{
		public const ushort MachineAmd64 = 0x8664;

		public const string HelperFileName = "zwave_sdk_helper_x64.dll";

		public static bool PackageComplete { get; private set; }

		public static string PackageDirectory { get; private set; }

		private static readonly Dictionary<string, string> ExpectedSha256 = LoadExpectedHashes();

		public static string GetPackageDirectory()
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xigncode");
		}

		public static void InspectNativePackage()
		{
			string directory = GetPackageDirectory();
			PackageDirectory = directory;
			bool complete = true;
			if (!Directory.Exists(directory))
			{
				Log.Warning("XignCode native package missing directory {0}", directory);
				PackageComplete = false;
				return;
			}

			if (ExpectedSha256.Count == 0)
			{
				WriteComputedHashes(directory);
			}
			else
			{
				foreach (KeyValuePair<string, string> item in ExpectedSha256)
				{
					string path = Path.Combine(directory, item.Key);
					if (!File.Exists(path))
					{
						Log.Warning("XignCode native package missing {0}", item.Key);
						complete = false;
						continue;
					}
					string hash = ComputeSha256(path);
					if (!string.Equals(hash, item.Value, StringComparison.OrdinalIgnoreCase))
					{
						Log.Error("XignCode native package hash mismatch for {0}: {1}", item.Key, hash);
						complete = false;
					}
				}
			}

			string helper = Path.Combine(directory, HelperFileName);
			if (!File.Exists(helper))
			{
				Log.Warning("XignCode native package missing {0}", HelperFileName);
				complete = false;
			}
			else if (ReadMachine(helper) != MachineAmd64)
			{
				Log.Error("XignCode helper is not AMD64; refusing activation");
				complete = false;
			}

			PackageComplete = complete && File.Exists(helper) && ReadMachine(helper) == MachineAmd64;
			if (PackageComplete)
			{
				Log.Information("XignCode native package OK at {0}", directory);
			}
			else
			{
				Log.Warning("XignCode native package incomplete; SDK not activated");
			}
		}

		private static Dictionary<string, string> LoadExpectedHashes()
		{
			Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string[] paths =
			{
				Path.Combine(GetPackageDirectory(), "expected_sha256.txt"),
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xigncode", "expected_sha256.txt")
			};
			foreach (string path in paths)
			{
				try
				{
					if (!File.Exists(path))
					{
						continue;
					}
					foreach (string line in File.ReadAllLines(path))
					{
						if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
						{
							continue;
						}
						int sep = line.IndexOf('=');
						if (sep <= 0)
						{
							sep = line.IndexOf('\t');
						}
						if (sep <= 0)
						{
							continue;
						}
						string name = line.Substring(0, sep).Trim();
						string hash = line.Substring(sep + 1).Trim().Replace("-", "");
						if (name.Length > 0 && hash.Length == 64)
						{
							map[name] = hash;
						}
					}
					break;
				}
				catch (Exception ex)
				{
					Log.Warning("XignCode expected_sha256.txt: {0}", ex.Message);
				}
			}
			return map;
		}

		private static void WriteComputedHashes(string directory)
		{
			try
			{
				Directory.CreateDirectory("Logs\\Agent");
				List<string> lines = new List<string>();
				foreach (string file in Directory.GetFiles(directory))
				{
					string name = Path.GetFileName(file);
					if (string.Equals(name, "expected_sha256.txt", StringComparison.OrdinalIgnoreCase)
						|| string.Equals(name, "README.txt", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					lines.Add(name + "=" + ComputeSha256(file));
				}
				File.WriteAllLines("Logs\\Agent\\xigncode_sha256.txt", lines);
				Log.Information("XignCode wrote {0} hashes to Logs\\Agent\\xigncode_sha256.txt (copy to xigncode\\expected_sha256.txt to pin)", lines.Count);
			}
			catch (Exception ex)
			{
				Log.Warning("XignCode hash dump: {0}", ex.Message);
			}
		}

		internal static string ComputeSha256(string path)
		{
			using SHA256 sha = SHA256.Create();
			using FileStream stream = File.OpenRead(path);
			byte[] hash = sha.ComputeHash(stream);
			return BitConverter.ToString(hash).Replace("-", "");
		}

		internal static ushort ReadMachine(string path)
		{
			using FileStream stream = File.OpenRead(path);
			using BinaryReader reader = new BinaryReader(stream);
			if (reader.ReadUInt16() != 0x5A4D)
			{
				return 0;
			}
			stream.Seek(0x3C, SeekOrigin.Begin);
			int pe = reader.ReadInt32();
			stream.Seek(pe, SeekOrigin.Begin);
			if (reader.ReadUInt32() != 0x4550)
			{
				return 0;
			}
			return reader.ReadUInt16();
		}
	}
}
