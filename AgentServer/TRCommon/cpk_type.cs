namespace TRCommon
{
	public struct cpk_type
	{
		private ushort value;

		private cpk_type(ushort value)
		{
			this.value = value;
		}

		public static implicit operator cpk_type(ushort value)
		{
			return new cpk_type(value);
		}

		public static implicit operator cpk_type(int value)
		{
			return new cpk_type((ushort)value);
		}

		public static implicit operator short(cpk_type cpk_type)
		{
			return (short)cpk_type.value;
		}

		public static implicit operator ushort(cpk_type cpk_type)
		{
			return cpk_type.value;
		}

		public static implicit operator int(cpk_type cpk_type)
		{
			return cpk_type.value;
		}

		public static implicit operator ITEM_POSITION(cpk_type cpk_type)
		{
			return (ITEM_POSITION)cpk_type.value;
		}

		public static implicit operator eFuncItemPosition(cpk_type cpk_type)
		{
			return (eFuncItemPosition)cpk_type.value;
		}
	}
}
