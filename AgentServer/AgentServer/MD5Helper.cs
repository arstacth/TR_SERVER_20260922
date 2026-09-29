using System.Security.Cryptography;
using System.Text;

namespace AgentServer
{
	public class MD5Helper
	{
		private static MD5 md5 = MD5.Create();

		public static string GetMD5HashString(string sourceStr)
		{
			return GetMD5HashString(Encoding.UTF8, sourceStr);
		}

		public static string GetMD5HashString(string sourceStr, int length)
		{
			string text = GetMD5HashString(Encoding.UTF8, sourceStr);
			if (text.Length > length)
			{
				text = text.Substring(0, length);
			}
			text = text.Replace("o", "a");
			text = text.Replace("O", "a");
			return text.Replace("0", "a");
		}

		public static string GetMD5HashString(Encoding encode, string sourceStr)
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = md5.ComputeHash(encode.GetBytes(sourceStr));
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
