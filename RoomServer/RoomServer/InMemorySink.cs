using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace RoomServer
{
	public class InMemorySink : ILogEventSink, IDisposable
	{
		private readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{Exception}");
		private readonly RichTextBox rtextBox;
		private readonly object _gate = new object();
		private readonly Queue<string> _queue = new Queue<string>();
		private readonly System.Windows.Forms.Timer _flushTimer;
		private const int MaxQueued = 200;
		private const int MaxLines = 150;
		private bool _disposed;

		public InMemorySink(RichTextBox rtextBox)
		{
			this.rtextBox = rtextBox;
			_flushTimer = new System.Windows.Forms.Timer { Interval = 250 };
			_flushTimer.Tick += FlushTick;
			_flushTimer.Start();
		}

		public void Emit(LogEvent logEvent)
		{
			if (logEvent == null || _disposed)
			{
				return;
			}
			StringWriter stringWriter = new StringWriter();
			_textFormatter.Format(logEvent, stringWriter);
			string line = stringWriter.ToString();
			lock (_gate)
			{
				if (_queue.Count >= MaxQueued)
				{
					_queue.Dequeue();
				}
				_queue.Enqueue(line);
			}
		}

		private void FlushTick(object sender, EventArgs e)
		{
			if (_disposed || rtextBox.IsDisposed)
			{
				return;
			}
			List<string> batch;
			lock (_gate)
			{
				if (_queue.Count == 0)
				{
					return;
				}
				batch = new List<string>(_queue.Count);
				while (_queue.Count > 0)
				{
					batch.Add(_queue.Dequeue());
				}
			}
			StringBuilder sb = new StringBuilder(batch.Count * 64);
			foreach (string line in batch)
			{
				sb.AppendLine(line);
			}
			try
			{
				rtextBox.SuspendLayout();
				rtextBox.AppendText(sb.ToString());
				string[] lines = rtextBox.Lines;
				if (lines.Length > MaxLines)
				{
					// Assigning .Lines selects all text (looks highlighted). Rebuild instead.
					int skip = lines.Length - MaxLines;
					StringBuilder kept = new StringBuilder(MaxLines * 64);
					for (int i = skip; i < lines.Length; i++)
					{
						kept.AppendLine(lines[i]);
					}
					rtextBox.Clear();
					rtextBox.AppendText(kept.ToString());
				}
				rtextBox.Select(rtextBox.TextLength, 0);
				rtextBox.DeselectAll();
				rtextBox.ScrollToCaret();
			}
			catch
			{
			}
			finally
			{
				try
				{
					rtextBox.ResumeLayout();
				}
				catch
				{
				}
			}
		}

		public void Dispose()
		{
			_disposed = true;
			if (_flushTimer != null)
			{
				_flushTimer.Stop();
				_flushTimer.Tick -= FlushTick;
				_flushTimer.Dispose();
			}
		}
	}
}
