using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LocalCommons.Logging;

namespace LocalCommons.Network
{
	public abstract class IConnectionUDP : IDisposable
	{
		protected Socket m_CurrentChannel;

		private SocketAsyncEventArgs m_AsyncReceive;

		private byte[] m_RecvBuffer;

		private readonly object m_SyncRoot = new object();

		private Queue<NetPacket> m_PacketQueue;

		private bool m_Disposing;

		private readonly DateTime m_NextCheckActivity;

		private readonly string m_Address;

		private EndPoint m_EndPoint;

		private static int m_CoalesceSleep = -1;

		private bool m_BlockAllPackets;

		private readonly DateTime m_ConnectedOn;

		private static BufferPool m_RecvBufferPool = new BufferPool("Receive", 1024, 4096);

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

		protected event EventHandler DisconnectedEvent;

		public IConnectionUDP(Socket socket)
		{
			m_CurrentChannel = socket;
			m_ConnectedOn = DateTime.Now;
			m_RecvBuffer = m_RecvBufferPool.AcquireBuffer();
			m_AsyncReceive = new SocketAsyncEventArgs();
			m_AsyncReceive.Completed += M_AsyncReceive_Completed;
			m_AsyncReceive.SetBuffer(m_RecvBuffer, 0, m_RecvBuffer.Length);
			m_PacketQueue = new Queue<NetPacket>();
			m_EndPoint = new IPEndPoint(IPAddress.Any, 0);
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
				do
				{
					m_CurrentChannel.BeginReceiveFrom(m_RecvBuffer, 0, m_RecvBuffer.Length, SocketFlags.None, ref m_EndPoint, ReceiveDataAsync, null);
				}
				while (flag);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
			}
		}

		private void ReceiveDataAsync(IAsyncResult ar)
		{
			int num = -1;
			try
			{
				num = m_CurrentChannel.EndReceiveFrom(ar, ref m_EndPoint);
				byte[] array = new byte[num];
				Buffer.BlockCopy(m_RecvBuffer, 0, array, 0, num);
				HandleReceived(array, (IPEndPoint)m_EndPoint);
			}
			catch (Exception)
			{
			}
			finally
			{
				BeginReceiveFrom(m_CurrentChannel);
			}
		}

		private void BeginReceiveFrom(Socket m_CurrentChannel)
		{
			try
			{
				if (this.m_CurrentChannel != null)
				{
					m_CurrentChannel.BeginReceiveFrom(m_RecvBuffer, 0, m_RecvBuffer.Length, SocketFlags.None, ref m_EndPoint, ReceiveDataAsync, this.m_CurrentChannel);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on receive data:\r\n{0}", ex.ToString());
				BeginReceiveFrom(m_CurrentChannel);
			}
		}

		public virtual void SendAsync(NetPacket packet, EndPoint endPoint)
		{
			if (CoalesceSleep != -1)
			{
				Thread.Sleep(CoalesceSleep);
			}
			m_PacketQueue.Enqueue(packet);
			M_AsyncSend_Do(endPoint);
		}

		private void M_AsyncSend_Do(EndPoint endPoint)
		{
			try
			{
				if (m_PacketQueue.Count > 0)
				{
					byte[] array = m_PacketQueue.Dequeue().Compile();
					m_CurrentChannel.SendTo(array, 0, array.Length, SocketFlags.None, endPoint);
				}
			}
			catch (Exception ex)
			{
				Log.Info(ex.ToString());
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
			_ = e.BytesTransferred;
		}

		public abstract void HandleReceived(byte[] data, IPEndPoint endPoint);

		public override string ToString()
		{
			return m_Address;
		}

		private void M_AsyncReceive_Completed(object sender, SocketAsyncEventArgs e)
		{
			m_CurrentChannel.BeginReceiveFrom(m_RecvBuffer, 0, m_RecvBuffer.Length, SocketFlags.None, ref m_EndPoint, ReceiveDataAsync, null);
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
			catch (SocketException ex)
			{
				Log.Info(ex.ToString());
			}
			try
			{
				m_CurrentChannel.Close();
			}
			catch (SocketException ex2)
			{
				Log.Info(ex2.ToString());
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
				lock (m_PacketQueue)
				{
					m_PacketQueue.Clear();
				}
			}
			m_PacketQueue = null;
			m_Disposing = false;
			m_Running = false;
		}
	}
}
