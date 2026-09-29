using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LocalCommons.Logging;

namespace LocalCommons.Network
{
	public class AsyncListener : IDisposable
	{
		private IPEndPoint m_EndPoint;

		private Socket m_Root;

		private Type defined;

		private SocketAsyncEventArgs m_SyncArgs;

		private Dictionary<string, long> m_FloodAttempts;

		public AsyncListener(string ip, int port, Type defined)
		{
			this.defined = defined;
			IPEndPoint iPEndPoint = (m_EndPoint = new IPEndPoint(IPAddress.Parse(ip), port));
			try
			{
				Socket socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				socket.LingerState.Enabled = false;
				socket.ExclusiveAddressUse = false;
				socket.Bind(iPEndPoint);
				socket.Listen(10);
				m_Root = socket;
			}
			catch (SocketException ex)
			{
				Log.Info(ex.ToString());
			}
			Log.Info("Installed {0} at {1}", defined.Name, iPEndPoint);
			m_SyncArgs = new SocketAsyncEventArgs();
			m_SyncArgs.Completed += M_SyncArgs_Completed;
			RunAccept();
			m_FloodAttempts = new Dictionary<string, long>();
		}

		private void RunAccept()
		{
			bool flag = false;
			do
			{
				try
				{
					flag = !m_Root.AcceptAsync(m_SyncArgs);
				}
				catch (SocketException ex)
				{
					Log.Info(ex.ToString());
					break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				if (flag)
				{
					AcceptProceed(m_SyncArgs);
				}
			}
			while (flag);
		}

		private void M_SyncArgs_Completed(object sender, SocketAsyncEventArgs e)
		{
			AcceptProceed(e);
			RunAccept();
		}

		private void AcceptProceed(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				Activator.CreateInstance(defined, e.AcceptSocket);
			}
			e.AcceptSocket = null;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Interlocked.Exchange(ref m_Root, null)?.Close();
				if (m_SyncArgs != null)
				{
					m_SyncArgs.Dispose();
				}
			}
		}
	}
}
