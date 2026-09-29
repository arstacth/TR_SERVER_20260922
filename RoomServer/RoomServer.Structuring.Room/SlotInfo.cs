namespace RoomServer.Structuring.Room
{
	public class SlotInfo
	{
		public uint m_slotData;

		public void closeSlot(byte numSlotPosition)
		{
			m_slotData |= (uint)(1 << (int)numSlotPosition);
		}

		public void closeSlot(byte iStartSlotPosition, byte iEndSlotPosition)
		{
			for (byte b = iStartSlotPosition; b < iEndSlotPosition + 1; b = (byte)(b + 1))
			{
				closeSlot(b);
			}
		}

		public void openSlot(byte numSlotPosition)
		{
			m_slotData = (uint)(m_slotData & (0xFFFFFFFFu ^ (1 << (int)numSlotPosition)));
		}

		public void closeSlotAll()
		{
			m_slotData = uint.MaxValue;
		}

		public void openSlotAll()
		{
			m_slotData = 0u;
		}

		public bool isCloseSlot(byte numSlotPosition)
		{
			uint num = (uint)(1 << (int)numSlotPosition);
			return (m_slotData & num) == num;
		}

		public int getClosedSlotCount()
		{
			int num = 0;
			for (byte b = 0; b < 32; b = (byte)(b + 1))
			{
				if (isCloseSlot(b))
				{
					num++;
				}
			}
			return num;
		}

		public byte getOpenSlot(byte iStartPositon, byte iEndPositon)
		{
			if (iStartPositon == iEndPositon)
			{
				return 0;
			}
			for (byte b = iStartPositon; b < iEndPositon + 1; b = (byte)(b + 1))
			{
				if (!isCloseSlot(b))
				{
					return b;
				}
			}
			return 0;
		}
	}
}
