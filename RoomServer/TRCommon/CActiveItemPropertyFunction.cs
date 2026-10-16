using System.Collections.Generic;

namespace TRCommon
{
	public class CActiveItemPropertyFunction
	{
		public void _pushProperty(int iItemDescNum, cpk_type position, CUserItemAttrManager userItemAttr, ref List<CPropertyCheckSource> vecPropertyCheckSource)
		{
			if (userItemAttr.getCharAttr(iItemDescNum, out CItemAttr rAttr))
			{
				vecPropertyCheckSource.Add(new CPropertyCheckSource(rAttr, position));
				return;
			}
			if (userItemAttr.getItemAttr(iItemDescNum, out rAttr))
			{
				vecPropertyCheckSource.Add(new CPropertyCheckSource(rAttr, position));
				return;
			}
			ItemAttrTable.getItemAttrFromItemDescNum(iItemDescNum, out var it);
			vecPropertyCheckSource.Add(new CPropertyCheckSource(it, position));
		}
	}
}
