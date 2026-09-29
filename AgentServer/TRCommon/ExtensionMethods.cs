using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TRCommon
{
	public static class ExtensionMethods
	{
		public static T DeepClone<T>(this T a)
		{
			using MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(memoryStream, a);
			memoryStream.Position = 0L;
			return (T)binaryFormatter.Deserialize(memoryStream);
		}
	}
}
