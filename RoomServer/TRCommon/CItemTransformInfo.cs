namespace TRCommon
{
	public class CItemTransformInfo
	{
		public cpk_type m_iCharacter;

		public cpk_type m_iPosition;

		public cpk_type m_iOriginKind;

		public cpk_type m_iTransKind;

		public eItemTransformType m_transformType;

		public string m_strValue;

		public CItemTransformInfo()
		{
			m_iCharacter = ushort.MaxValue;
			m_iPosition = ushort.MaxValue;
			m_iOriginKind = ushort.MaxValue;
			m_iTransKind = ushort.MaxValue;
			m_transformType = eItemTransformType.eItemTransformType_UNKNOWN;
		}
	}
}
