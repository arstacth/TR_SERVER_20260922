using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace AgentServer.Cryptography
{
	public class Encrypt
	{
		[DllImport("bin\\TRServer.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr TFUNC_1(byte[] client_pubkeych, byte[] inkey, string c);

		public static byte[] TFUNC_1_W(byte[] client_publickey, byte[] packet_key, string c)
		{
			try
			{
				byte[] array = new byte[256];
				Marshal.Copy(TFUNC_1(client_publickey, packet_key, "1"), array, 0, 256);
				return array;
			}
			catch
			{
				return null;
			}
		}

		public static byte[] EncryptKey(byte[] inkey)
		{
			using AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider();
			aesCryptoServiceProvider.Key = inkey;
			aesCryptoServiceProvider.Mode = CipherMode.ECB;
			aesCryptoServiceProvider.Padding = PaddingMode.None;
			using ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor();
			return cryptoTransform.TransformFinalBlock(inkey, 0, inkey.Length);
		}

		public static byte[] EncryptByte(byte[] inkey, byte[] xorkey, byte[] src)
		{
			int num = src.Length;
			int num2 = num % 16;
			using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
			{
				aesCryptoServiceProvider.Key = inkey;
				aesCryptoServiceProvider.Mode = CipherMode.ECB;
				aesCryptoServiceProvider.Padding = PaddingMode.None;
				using ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor();
				if (num2 == 0)
				{
					src = cryptoTransform.TransformFinalBlock(src, 0, src.Length);
				}
				else
				{
					int num3 = num - num2;
					byte[] array = new byte[num3];
					Buffer.BlockCopy(src, 0, array, 0, num3);
					array = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
					xorbyte(new Span<byte>(src, num3, num2), xorkey, num2);
					Buffer.BlockCopy(array, 0, src, 0, num3);
				}
			}
			byte[] array2 = new byte[src.Length + 1];
			Buffer.BlockCopy(src, 0, array2, 0, src.Length);
			array2[src.Length] = 1;
			return array2;
		}

		private static void xorbyte(Span<byte> input, byte[] xorkey, int size)
		{
			for (int i = 0; i < size; i++)
			{
				input[i] ^= xorkey[i];
			}
		}
	}
}
