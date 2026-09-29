using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AgentServer
{
	/// <summary>
	/// Shared TRPkgTools / Agent dark palette for main form and child dialogs.
	/// </summary>
	public static class DarkUi
	{
		public static readonly Color Bg = Color.FromArgb(22, 22, 24);
		public static readonly Color Panel = Color.FromArgb(32, 32, 36);
		public static readonly Color Border = Color.FromArgb(58, 58, 64);
		public static readonly Color Fg = Color.FromArgb(232, 232, 236);
		public static readonly Color Muted = Color.FromArgb(150, 150, 158);
		public static readonly Color Accent = Color.FromArgb(88, 166, 255);
		public static readonly Color AccentText = Color.FromArgb(16, 16, 18);
		public static readonly Color Danger = Color.FromArgb(180, 60, 60);
		public static readonly Color Select = Color.FromArgb(48, 72, 104);
		public static readonly Color GridLine = Color.FromArgb(48, 48, 54);
		public static readonly Color ScrollThumb = Color.FromArgb(72, 72, 80);

		const int ChromeHeight = 36;
		const string ChromeTag = "DarkUiChrome";

		public static void Apply(Form form, bool primaryButtons = false, bool borderless = true)
		{
			if (form == null)
			{
				return;
			}
			form.BackColor = Bg;
			form.ForeColor = Fg;
			form.Font = new Font("Segoe UI", 9F);
			if (borderless)
			{
				MakeBorderless(form);
			}
			ApplyToControls(form.Controls, primaryButtons);
		}

		public static void MakeBorderless(Form form, string title = null)
		{
			if (form.Tag as string == ChromeTag + "_done")
			{
				return;
			}

			form.FormBorderStyle = FormBorderStyle.None;
			form.MaximizeBox = false;
			form.MinimizeBox = false;
			if (form.StartPosition == FormStartPosition.WindowsDefaultLocation)
			{
				form.StartPosition = FormStartPosition.CenterParent;
			}

			// Make room for title chrome.
			foreach (Control c in form.Controls)
			{
				c.Top += ChromeHeight;
			}
			form.ClientSize = new Size(form.ClientSize.Width, form.ClientSize.Height + ChromeHeight);

			Label titleLabel = new Label
			{
				AutoSize = true,
				Font = new Font("Segoe UI", 11F, FontStyle.Bold),
				ForeColor = Accent,
				BackColor = Color.Transparent,
				Location = new Point(12, 10),
				Text = string.IsNullOrEmpty(title) ? form.Text : title,
				Tag = ChromeTag
			};
			Button closeBtn = new Button
			{
				FlatStyle = FlatStyle.Flat,
				Size = new Size(28, 28),
				Location = new Point(Math.Max(40, form.ClientSize.Width - 36), 6),
				Text = "×",
				ForeColor = Fg,
				BackColor = Panel,
				Cursor = Cursors.Hand,
				TabStop = false,
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Tag = ChromeTag
			};
			closeBtn.FlatAppearance.BorderSize = 0;
			closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 70, 70);
			closeBtn.Click += (_, __) => form.Close();

			form.Controls.Add(titleLabel);
			form.Controls.Add(closeBtn);
			titleLabel.BringToFront();
			closeBtn.BringToFront();

			Point dragOffset = Point.Empty;
			bool dragging = false;
			void OnDragStart(object sender, MouseEventArgs e)
			{
				if (e.Button != MouseButtons.Left)
				{
					return;
				}
				dragging = true;
				dragOffset = e.Location;
				if (sender != form && sender is Control ctl)
				{
					dragOffset = new Point(e.X + ctl.Left, e.Y + ctl.Top);
				}
			}
			void OnDragMove(object sender, MouseEventArgs e)
			{
				if (!dragging)
				{
					return;
				}
				Point screen = form.PointToScreen(e.Location);
				if (sender != form && sender is Control ctl)
				{
					screen = ctl.PointToScreen(e.Location);
				}
				form.Location = new Point(screen.X - dragOffset.X, screen.Y - dragOffset.Y);
			}
			void OnDragEnd(object sender, MouseEventArgs e)
			{
				dragging = false;
			}

			form.MouseDown += OnDragStart;
			form.MouseMove += OnDragMove;
			form.MouseUp += OnDragEnd;
			titleLabel.MouseDown += OnDragStart;
			titleLabel.MouseMove += OnDragMove;
			titleLabel.MouseUp += OnDragEnd;

			form.Paint += (s, e) =>
			{
				using (Pen pen = new Pen(Border))
				{
					e.Graphics.DrawRectangle(pen, 0, 0, form.Width - 1, form.Height - 1);
				}
			};
			form.Resize += (_, __) =>
			{
				closeBtn.Left = Math.Max(40, form.ClientSize.Width - 36);
				form.Invalidate();
			};

			try
			{
				form.HandleCreated += (_, __) => EnableDropShadow(form);
				if (form.IsHandleCreated)
				{
					EnableDropShadow(form);
				}
			}
			catch
			{
			}

			form.Tag = ChromeTag + "_done";
		}

		[DllImport("dwmapi.dll")]
		static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		static void EnableDropShadow(Form form)
		{
			if (!form.IsHandleCreated)
			{
				return;
			}
			int useDark = 1;
			// 20 = DWMWA_USE_IMMERSIVE_DARK_MODE (Win10 1809+)
			DwmSetWindowAttribute(form.Handle, 20, ref useDark, sizeof(int));
		}

		public static DialogResult Confirm(IWin32Window owner, string message, string title = "TRServer")
		{
			using (DarkMessageForm dlg = new DarkMessageForm(title, message, yesNo: true))
			{
				return dlg.ShowDialog(owner);
			}
		}

		public static void Alert(IWin32Window owner, string message, string title = "TRServer", bool danger = false)
		{
			using (DarkMessageForm dlg = new DarkMessageForm(title, message, yesNo: false, danger: danger))
			{
				dlg.ShowDialog(owner);
			}
		}

		public static void ApplyToControls(Control.ControlCollection controls, bool primaryButtons = false)
		{
			foreach (Control c in controls)
			{
				if (c.Tag as string == ChromeTag)
				{
					continue;
				}
				ApplyControl(c, primaryButtons);
				if (c.HasChildren)
				{
					ApplyToControls(c.Controls, primaryButtons);
				}
			}
		}

		private static void ApplyControl(Control c, bool primaryButtons)
		{
			if (c is Label lbl)
			{
				lbl.BackColor = Color.Transparent;
				lbl.ForeColor = Muted;
			}
			else if (c is TextBox || c is RichTextBox || c is ComboBox || c is ListBox || c is NumericUpDown)
			{
				c.BackColor = Panel;
				c.ForeColor = Fg;
				if (c is TextBox tb)
				{
					tb.BorderStyle = BorderStyle.FixedSingle;
				}
				if (c is RichTextBox rtb)
				{
					rtb.BorderStyle = BorderStyle.None;
					rtb.SelectionBackColor = Select;
					rtb.SelectionColor = Fg;
				}
			}
			else if (c is CheckBox chk)
			{
				chk.BackColor = Bg;
				chk.ForeColor = Muted;
				chk.FlatStyle = FlatStyle.Standard;
				chk.UseVisualStyleBackColor = false;
			}
			else if (c is Button btn)
			{
				StyleButton(btn, primaryButtons && IsPrimaryName(btn.Name, btn.Text));
			}
			else if (c is DataGridView grid)
			{
				StyleGrid(grid);
			}
			else if (c is Panel || c is GroupBox || c is TabPage)
			{
				c.BackColor = Bg;
				c.ForeColor = Fg;
			}
			else if (c is TabControl tab)
			{
				tab.BackColor = Bg;
				tab.ForeColor = Fg;
			}
		}

		private static bool IsPrimaryName(string name, string text)
		{
			string n = ((name ?? "") + " " + (text ?? "")).ToLowerInvariant();
			return n.Contains("apply") || n.Contains("save") || n.Contains("add")
				|| n.Contains("give") || n.Contains("ok") || n.Contains("edit")
				|| n.Contains("reload") || n.Contains("update");
		}

		public static void StyleButton(Button btn, bool primary)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.Cursor = Cursors.Hand;
			btn.UseVisualStyleBackColor = false;
			btn.Font = new Font("Segoe UI", 9F, primary ? FontStyle.Bold : FontStyle.Regular);
			string key = ((btn.Name ?? "") + " " + (btn.Text ?? "")).ToLowerInvariant();
			bool danger = key.Contains("close") || key.Contains("shut") || key.Contains("delete")
				|| key.Contains("disconnect") || key.Contains("yes");
			if (danger)
			{
				btn.BackColor = Danger;
				btn.ForeColor = Fg;
				btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 70, 70);
			}
			else if (primary)
			{
				btn.BackColor = Accent;
				btn.ForeColor = AccentText;
				btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 180, 255);
			}
			else
			{
				btn.BackColor = Panel;
				btn.ForeColor = Fg;
				btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 54);
			}
		}

		public static void StyleGrid(DataGridView grid)
		{
			grid.BackgroundColor = Panel;
			grid.GridColor = GridLine;
			grid.BorderStyle = BorderStyle.None;
			grid.EnableHeadersVisualStyles = false;
			grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 46);
			grid.ColumnHeadersDefaultCellStyle.ForeColor = Fg;
			grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 46);
			grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Fg;
			grid.DefaultCellStyle.BackColor = Panel;
			grid.DefaultCellStyle.ForeColor = Fg;
			grid.DefaultCellStyle.SelectionBackColor = Select;
			grid.DefaultCellStyle.SelectionForeColor = Fg;
			grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(28, 28, 32);
			grid.AlternatingRowsDefaultCellStyle.ForeColor = Fg;
			grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Select;
			grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Fg;
			grid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 46);
			grid.RowHeadersDefaultCellStyle.ForeColor = Muted;
			grid.RowHeadersDefaultCellStyle.SelectionBackColor = Select;
		}

		sealed class DarkMessageForm : Form
		{
			public DarkMessageForm(string title, string message, bool yesNo, bool danger = false)
			{
				FormBorderStyle = FormBorderStyle.None;
				MaximizeBox = false;
				MinimizeBox = false;
				ShowInTaskbar = false;
				StartPosition = FormStartPosition.CenterParent;
				BackColor = Bg;
				ForeColor = Fg;
				Font = new Font("Segoe UI", 9F);
				ClientSize = new Size(420, 150);
				DoubleBuffered = true;

				Label titleLabel = new Label
				{
					AutoSize = true,
					Font = new Font("Segoe UI", 11F, FontStyle.Bold),
					ForeColor = Accent,
					Location = new Point(16, 12),
					Text = title
				};
				Label msgLabel = new Label
				{
					AutoSize = false,
					ForeColor = Fg,
					Location = new Point(16, 48),
					Size = new Size(388, 48),
					Text = message
				};
				Button btnYes = new Button
				{
					Text = yesNo ? "Yes" : "OK",
					Size = new Size(100, 30),
					Location = new Point(yesNo ? 196 : 304, 106),
					DialogResult = DialogResult.Yes
				};
				Button btnNo = new Button
				{
					Text = "No",
					Size = new Size(100, 30),
					Location = new Point(304, 106),
					DialogResult = DialogResult.No,
					Visible = yesNo
				};
				if (!yesNo)
				{
					btnYes.DialogResult = DialogResult.OK;
					btnYes.Location = new Point(304, 106);
				}
				StyleButton(btnYes, primary: !danger && !yesNo);
				if (yesNo || danger)
				{
					btnYes.BackColor = Danger;
					btnYes.ForeColor = Fg;
					btnYes.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 70, 70);
					btnYes.FlatStyle = FlatStyle.Flat;
					btnYes.FlatAppearance.BorderSize = 0;
					btnYes.UseVisualStyleBackColor = false;
				}
				StyleButton(btnNo, primary: false);

				Controls.Add(titleLabel);
				Controls.Add(msgLabel);
				Controls.Add(btnYes);
				Controls.Add(btnNo);
				AcceptButton = btnYes;
				CancelButton = yesNo ? btnNo : btnYes;

				Paint += (s, e) =>
				{
					using (Pen pen = new Pen(Border))
					{
						e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
					}
				};

				Point dragOffset = Point.Empty;
				bool dragging = false;
				MouseDown += (s, e) =>
				{
					if (e.Button == MouseButtons.Left)
					{
						dragging = true;
						dragOffset = e.Location;
					}
				};
				MouseMove += (s, e) =>
				{
					if (dragging)
					{
						Point screen = PointToScreen(e.Location);
						Location = new Point(screen.X - dragOffset.X, screen.Y - dragOffset.Y);
					}
				};
				MouseUp += (s, e) => dragging = false;
				titleLabel.MouseDown += (s, e) =>
				{
					if (e.Button == MouseButtons.Left)
					{
						dragging = true;
						dragOffset = new Point(e.X + titleLabel.Left, e.Y + titleLabel.Top);
					}
				};
				titleLabel.MouseMove += (s, e) =>
				{
					if (dragging)
					{
						Point screen = titleLabel.PointToScreen(e.Location);
						Location = new Point(screen.X - dragOffset.X, screen.Y - dragOffset.Y);
					}
				};
				titleLabel.MouseUp += (s, e) => dragging = false;
			}
		}
	}
}
