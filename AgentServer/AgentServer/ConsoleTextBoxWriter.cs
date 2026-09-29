using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AgentServer
{
	public class ConsoleTextBoxWriter : TextWriter
	{
		private delegate void WriteFunc(string value);

		private RichTextBox rtextBox;

		private WriteFunc write;

		private WriteFunc writeLine;

		public override Encoding Encoding => Encoding.Unicode;

		public ConsoleTextBoxWriter(RichTextBox rtextBox)
		{
			this.rtextBox = rtextBox;
			write = Write;
			writeLine = WriteLine;
		}

		public override void Write(string value)
		{
			if (rtextBox.InvokeRequired)
			{
				rtextBox.BeginInvoke(write, value);
			}
			else
			{
				rtextBox.AppendText(value);
			}
		}

		public override void WriteLine(string value)
		{
			if (rtextBox.InvokeRequired)
			{
				rtextBox.BeginInvoke(writeLine, value);
			}
			else
			{
				rtextBox.AppendText(value);
				rtextBox.AppendText(NewLine);
			}
		}
	}
}
