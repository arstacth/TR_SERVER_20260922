using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LocalCommons.Logging;
using LocalCommons.Utilities;

namespace LocalCommons.Network
{
	public abstract class IConnection2 : IDisposable
	{
		protected Socket m_CurrentChannel;

		private SocketAsyncEventArgs m_AsyncReceive;

		private byte[] m_RecvBuffer;

		private readonly object m_SyncRoot = new object();

		private ConcurrentQueue<NetPacket> m_PacketQueue;

		private bool m_Disposing;

		private readonly DateTime m_NextCheckActivity;

		private readonly string m_Address;

		private static int m_CoalesceSleep = -1;

		private bool m_BlockAllPackets;

		private readonly DateTime m_ConnectedOn;

		private static BufferPool m_RecvBufferPool = new BufferPool("Receive", 512, 102400);

		private bool m_Running;

		protected bool m_LittleEndian;

		private readonly bool BreakRunProcess;

		public Socket CurrentChannel => m_CurrentChannel;

		public static int CoalesceSleep
		{
			get
			{
				return m_CoalesceSleep;
			}
			set
			{
				m_CoalesceSleep = value;
			}
		}

		public bool BlockAllPackets
		{
			get
			{
				return m_BlockAllPackets;
			}
			set
			{
				m_BlockAllPackets = value;
			}
		}

		public int ErrorTime { get; set; }

		protected event EventHandler DisconnectedEvent;

		public IConnection2(Socket socket)
		{
			m_CurrentChannel = socket;
			m_ConnectedOn = DateTime.Now;
			m_RecvBuffer = m_RecvBufferPool.AcquireBuffer();
			m_AsyncReceive = new SocketAsyncEventArgs();
			m_AsyncReceive.Completed += M_AsyncReceive_Completed;
			m_AsyncReceive.SetBuffer(m_RecvBuffer, 0, m_RecvBuffer.Length);
			m_PacketQueue = new ConcurrentQueue<NetPacket>();
			m_Address = ((IPEndPoint)m_CurrentChannel.RemoteEndPoint).Address.ToString();
			if (m_CurrentChannel != null)
			{
				RunReceive();
				m_Running = true;
			}
		}

		public void RunReceive()
		{
			try
			{
				bool flag = false;
				while (m_AsyncReceive != null)
				{
					lock (m_SyncRoot)
					{
						flag = !m_CurrentChannel.ReceiveAsync(m_AsyncReceive);
					}
					if (flag)
					{
						ProceedReceiving(m_AsyncReceive);
					}
					if (!flag)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
			}
		}

		public virtual void SendAsync(NetPacket packet)
		{
			if (packet != null)
			{
				m_PacketQueue.Enqueue(packet);
				M_AsyncSend_Do();
			}
		}

		private void M_AsyncSend_Do()
		{
			try
			{
				if (m_PacketQueue.Count <= 0)
				{
					return;
				}
				if (m_CurrentChannel.Connected)
				{
					m_PacketQueue.TryDequeue(out var result);
					byte[] array = result.Compile();
					if (m_CurrentChannel.Send(array, array.Length, SocketFlags.None) == -1)
					{
						Log.Error("Socket Error: Cannot Send Packet");
						this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
					}
				}
				else
				{
					Log.Error("Socket Error: Socket Closed");
					this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
			}
		}

		public virtual void SendAsyncHex(NetPacket packet)
		{
			if (CoalesceSleep != -1)
			{
				Thread.Sleep(CoalesceSleep);
			}
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.ResetColor();
		}

		private void ProceedReceiving(SocketAsyncEventArgs e)
		{
			int bytesTransferred = e.BytesTransferred;
			if (e.SocketError != 0 || bytesTransferred <= 0)
			{
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
				return;
			}
			PacketReader packetReader = new PacketReader(m_RecvBuffer, 0);
			int size = packetReader.Size;
			ushort num = (ushort)(packetReader.ReadLEUInt16() - 2);
			ushort num2 = 2;
			do
			{
				if (num < 1 || num + num2 > size)
				{
					string text = Utility.ByteArrayToString(packetReader.Buffer);
					Log.Error("Packet Length Error: offset: {0}, length: {1}, ip,:{2}, buffer:{3},", num2, num, m_Address, text.Substring(0, Math.Min(512, text.Length)));
					break;
				}
				byte[] array = new byte[num];
				try
				{
					Buffer.BlockCopy(packetReader.Buffer, num2, array, 0, num);
					HandleReceived(array);
				}
				catch (ArgumentException ex)
				{
					string text2 = Utility.ByteArrayToString(packetReader.Buffer);
					Log.Error("[ArgumentException] packets: {0}\r\noffset: {1}, size: {2}, length: {3}, ip,:{4} buffer:{5}", ex.ToString(), num2, size, num, m_Address, text2.Substring(0, Math.Min(512, text2.Length)));
					ErrorTime++;
					if (ErrorTime > 10)
					{
						this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
						break;
					}
				}
				catch (Exception ex2)
				{
					Log.Error("Errors when parsing glued packets : {0}\r\noffset: {1}, size: {2}, length: {3}", ex2.ToString(), num2, size, num);
				}
				int num4 = (packetReader.Offset = (ushort)(num2 + num));
				num2 = (ushort)num4;
				num2 = (ushort)(num2 + 2);
				num = packetReader.ReadLEUInt16();
				if (num >= 2)
				{
					num = (ushort)(num - 2);
				}
			}
			while (num > 0 && num + num2 <= size && num2 < size);
			packetReader.Clear();
		}

		public abstract void HandleReceived(byte[] data);

		public override string ToString()
		{
			return m_Address;
		}

		private void M_AsyncReceive_Completed(object sender, SocketAsyncEventArgs e)
		{
			ProceedReceiving(e);
			if (!m_Disposing)
			{
				RunReceive();
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || m_CurrentChannel == null || m_Disposing)
			{
				return;
			}
			m_Disposing = true;
			try
			{
				m_CurrentChannel.Shutdown(SocketShutdown.Both);
			}
			catch (Exception)
			{
			}
			try
			{
				m_CurrentChannel.Close();
			}
			catch (SocketException ex2)
			{
				Log.Error(ex2.ToString());
			}
			if (m_RecvBuffer != null)
			{
				m_RecvBufferPool.ReleaseBuffer(m_RecvBuffer);
			}
			m_CurrentChannel.Close();
			m_AsyncReceive.Dispose();
			m_CurrentChannel = null;
			m_RecvBuffer = null;
			m_AsyncReceive = null;
			if (m_PacketQueue.Count <= 0)
			{
				NetPacket result;
				while (m_PacketQueue.TryDequeue(out result))
				{
				}
			}
			m_PacketQueue = null;
			m_Disposing = false;
			m_Running = false;
		}
	}
}
