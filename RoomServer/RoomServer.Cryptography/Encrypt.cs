using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace RoomServer.Cryptography
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
				Marshal.Copy(TFUNC_1(client_publickey, packet_key, c), array, 0, 257);
				return array;
			}
			catch
			{
				return null;
			}
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
	}
}
