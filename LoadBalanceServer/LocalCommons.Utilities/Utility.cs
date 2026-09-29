using System;
using System.Globalization;
using System.Linq;
using System.Net;
using Force.Crc32;

namespace LocalCommons.Utilities
{
	public class Utility
	{
		private static readonly DateTime Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();

		public static long CurrentTimeMilliseconds()
		{
			return (long)(DateTime.Now - Time).TotalMilliseconds;
		}

		public static long ConvertToTimestamp(DateTime value)
		{
			return (long)(value - Time).TotalMilliseconds;
		}

		public static int IPToInt(string addr)
		{
			return IPAddress.NetworkToHostOrder((int)IPAddress.Parse(addr).Address);
		}

		public static uint CheckSum(byte[] redata)
		{
			int length = ((redata.Length > 16) ? 16 : redata.Length);
			return Crc32Algorithm.Compute(redata, 0, length);
		}

		public static byte[] HexToByteArray(string hex)
		{
			string[] array = hex.Split(new string[1] { " " }, StringSplitOptions.None);
			byte[] array2 = new byte[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = byte.Parse(array[i], NumberStyles.HexNumber);
			}
			return array2;
		}

		public static byte[] StringToByteArray(string hex)
		{
			return (from x in Enumerable.Range(0, hex.Length)
				where x % 2 == 0
				select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
		}

		public static byte[] StringToByteArrayFastest(string hex)
		{
			if (hex.Length % 2 == 1)
			{
				throw new Exception("The binary key cannot have an odd number of digits");
			}
			byte[] array = new byte[hex.Length >> 1];
			for (int i = 0; i < hex.Length >> 1; i++)
			{
				array[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));
			}
			return array;
		}

		public static int GetHexVal(char hex)
		{
			return hex - ((hex < ':') ? 48 : 55);
		}

		public static string IntToHex(int n)
		{
			return $"{n & 0xFF:X2} {(n & 0xFF00) >> 8:X2}";
		}

		public static string ByteArrayToString(byte[] data)
		{
			char[] array = new char[16]
			{
				'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'A', 'B', 'C', 'D', 'E', 'F'
			};
			int num = 0;
			int num2 = 0;
			int num3 = data.Length;
			char[] array2 = new char[num3 * 2 + 2];
			while (num < num3)
			{
				byte b = data[num++];
				array2[num2++] = array[(int)b / 16];
				array2[num2++] = array[(int)b % 16];
			}
			return new string(array2, 0, array2.Length);
		}

		public void Clear(byte[] buf)
		{
			for (int i = 0; i < buf.Length; i++)
			{
				buf[i] = 0;
			}
		}
	}
}
