using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace LocalCommons.Cryptography
{
	public class Encrypt
	{
		[DllImport("TRServer.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr TFUNC_1(byte[] client_pubkeych, byte[] inkey, string c);

		public static byte[] TFUNC_1_W(byte[] client_publickey, byte[] packet_key, string c)
		{
			try
			{
				byte[] array = new byte[257];
				Marshal.Copy(TFUNC_1(client_publickey, packet_key, "1"), array, 0, 257);
				return array;
			}
			catch
			{
				return null;
			}
		}

		public static byte[] newEncryptByte(byte[] inkey, byte[] xorkey, byte[] src)
		{
			int num = src.Length;
			int num2 = num % 16;
			byte[] first;
			using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
			{
				aesCryptoServiceProvider.Key = inkey;
				aesCryptoServiceProvider.Mode = CipherMode.ECB;
				aesCryptoServiceProvider.Padding = PaddingMode.None;
				using ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor();
				if (num2 == 0)
				{
					first = cryptoTransform.TransformFinalBlock(src, 0, src.Length);
				}
				else
				{
					int num3 = num - num2;
					byte[] array = new byte[num3];
					byte[] array2 = new byte[num2];
					Array.Copy(src, 0, array, 0, num3);
					Array.Copy(src, num3, array2, 0, num2);
					first = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
					array2 = xorbyte(array2, xorkey, num2);
					first = first.Concat(array2).ToArray();
				}
			}
			return first.Concat(new byte[1] { 1 }).ToArray();
		}

		public static byte[] EncryptKey(byte[] inkey)
		{
			using RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = inkey;
			rijndaelManaged.Mode = CipherMode.ECB;
			rijndaelManaged.Padding = PaddingMode.None;
			using ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor();
			return cryptoTransform.TransformFinalBlock(inkey, 0, inkey.Length);
		}

		public static byte[] DecryptByte(byte[] inkey, byte[] xorkey, byte[] src)
		{
			int num = src.Length;
			int num2 = num % 16;
			using AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider();
			aesCryptoServiceProvider.Key = inkey;
			aesCryptoServiceProvider.Mode = CipherMode.ECB;
			aesCryptoServiceProvider.Padding = PaddingMode.None;
			using ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateDecryptor();
			if (num2 == 0)
			{
				return cryptoTransform.TransformFinalBlock(src, 0, src.Length);
			}
			int num3 = num - num2;
			byte[] array = new byte[num3];
			byte[] array2 = new byte[num2];
			Array.Copy(src, 0, array, 0, num3);
			Array.Copy(src, num3, array2, 0, num2);
			byte[] first = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			array2 = xorbyte(array2, xorkey, num2);
			return first.Concat(array2).ToArray();
		}

		private static byte[] xorbyte(byte[] input, byte[] xorkey, int size)
		{
			for (int i = 0; i < size; i++)
			{
				input[i] ^= xorkey[i];
			}
			return input;
		}
	}
}
