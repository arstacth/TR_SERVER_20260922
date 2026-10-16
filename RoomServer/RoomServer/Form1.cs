using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Akka.Actor;
using Akka.Configuration;
using IniParser;
using IniParser.Model;
using MySql.Data.MySqlClient;
using RoomServer.Holders;
using RoomServer.Room;
using RoomServer.Structuring;
using Serilog;
using Serilog.Events;

namespace RoomServer
{
	public class Form1 : Form
	{
		private delegate void EnableDelegate(int id);

		private static Form1 form;

		private IContainer components;

		private RichTextBox richTextBox1;

		private Label label1;
		private Label labelAgentCaption;
		private Label labelAgentValue;
		private Label titleLabel;
		private Button btnClose;
		private Panel logScrollRail;
		private Panel logScrollThumb;

		static readonly Color ThemeBg = Color.FromArgb(22, 22, 24);
		static readonly Color ThemePanel = Color.FromArgb(32, 32, 36);
		static readonly Color ThemeBorder = Color.FromArgb(58, 58, 64);
		static readonly Color ThemeText = Color.FromArgb(232, 232, 236);
		static readonly Color ThemeMuted = Color.FromArgb(150, 150, 158);
		static readonly Color ThemeAccent = Color.FromArgb(88, 166, 255);
		static readonly Color ThemeSelect = Color.FromArgb(48, 72, 104);
		static readonly Color ThemeScrollThumb = Color.FromArgb(110, 110, 120);

		Point _dragOffset;
		bool _dragging;
		bool _thumbDrag;
		int _thumbDragOffsetY;

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

		[DllImport("user32.dll")]
		private static extern bool GetScrollInfo(IntPtr hwnd, int nBar, ref SCROLLINFO lpsi);

		[DllImport("user32.dll")]
		private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int SB_VERT = 1;
		private const int WM_VSCROLL = 0x0115;
		private const int SB_THUMBPOSITION = 4;
		private const uint SIF_RANGE = 0x1;
		private const uint SIF_PAGE = 0x2;
		private const uint SIF_POS = 0x4;

		[StructLayout(LayoutKind.Sequential)]
		private struct SCROLLINFO
		{
			public uint cbSize;
			public uint fMask;
			public int nMin;
			public int nMax;
			public uint nPage;
			public int nPos;
			public int nTrackPos;
		}

		public Form1()
		{
			InitializeComponent();
			ApplyDarkTheme();
			SetupBorderlessChrome();
			SetupDarkLogScroll();
			form = this;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= 0x20000;
				return cp;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			using (Pen pen = new Pen(ThemeBorder))
			{
				e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
			}
		}

		private void SetupBorderlessChrome()
		{
			DoubleBuffered = true;
			MouseDown += OnDragStart;
			MouseMove += OnDragMove;
			MouseUp += OnDragEnd;
			foreach (Control c in new Control[] { titleLabel, labelAgentCaption, labelAgentValue })
			{
				c.MouseDown += OnDragStart;
				c.MouseMove += OnDragMove;
				c.MouseUp += OnDragEnd;
			}
		}

		private void OnDragStart(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			_dragging = true;
			_dragOffset = e.Location;
			if (sender != this && sender is Control c)
			{
				_dragOffset = new Point(e.X + c.Left, e.Y + c.Top);
			}
		}

		private void OnDragMove(object sender, MouseEventArgs e)
		{
			if (!_dragging)
			{
				return;
			}
			Point screen = PointToScreen(e.Location);
			if (sender != this && sender is Control c)
			{
				screen = c.PointToScreen(e.Location);
			}
			Location = new Point(screen.X - _dragOffset.X, screen.Y - _dragOffset.Y);
		}

		private void OnDragEnd(object sender, MouseEventArgs e)
		{
			_dragging = false;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void SetupDarkLogScroll()
		{
			// Match Agent: native scrollbar only. Custom overlay thumb was out of sync.
			richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
			if (logScrollRail != null)
			{
				Controls.Remove(logScrollRail);
				logScrollRail.Dispose();
				logScrollRail = null;
				logScrollThumb = null;
			}
			richTextBox1.HandleCreated += (_, __) =>
			{
				try
				{
					SetWindowTheme(richTextBox1.Handle, "DarkMode_Explorer", null);
				}
				catch
				{
				}
			};
			if (richTextBox1.IsHandleCreated)
			{
				try
				{
					SetWindowTheme(richTextBox1.Handle, "DarkMode_Explorer", null);
				}
				catch
				{
				}
			}
		}

		private bool TryGetLogScroll(out int pos, out int max, out int page)
		{
			pos = 0; max = 0; page = 0;
			if (!richTextBox1.IsHandleCreated) return false;
			SCROLLINFO si = new SCROLLINFO
			{
				cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO)),
				fMask = SIF_RANGE | SIF_PAGE | SIF_POS
			};
			if (!GetScrollInfo(richTextBox1.Handle, SB_VERT, ref si)) return false;
			pos = si.nPos; max = si.nMax; page = (int)si.nPage;
			return true;
		}

		private void SyncLogScrollThumb()
		{
			if (logScrollRail == null || logScrollThumb == null || _thumbDrag) return;
			if (!TryGetLogScroll(out int pos, out int max, out int page))
			{
				logScrollThumb.Visible = false;
				return;
			}
			int range = Math.Max(1, max - page + 1);
			bool needed = max > page && page > 0;
			logScrollThumb.Visible = needed;
			if (!needed) return;
			int track = Math.Max(20, logScrollRail.Height);
			int thumbH = Math.Max(24, Math.Min(track, (int)(track * ((double)page / (max + 1)))));
			int maxTop = Math.Max(0, track - thumbH);
			logScrollThumb.Height = thumbH;
			logScrollThumb.Top = Math.Max(0, Math.Min(maxTop, (int)(maxTop * ((double)pos / Math.Max(1, range - 1)))));
		}

		private void ScrollLogToThumb()
		{
			if (!TryGetLogScroll(out _, out int max, out int page)) return;
			int range = Math.Max(1, max - page + 1);
			int maxTop = Math.Max(1, logScrollRail.Height - logScrollThumb.Height);
			int pos = Math.Max(0, Math.Min(range - 1, (int)((range - 1) * ((double)logScrollThumb.Top / maxTop))));
			SetScrollPos(richTextBox1.Handle, SB_VERT, pos, true);
			SendMessage(richTextBox1.Handle, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION | (pos << 16)), IntPtr.Zero);
			richTextBox1.Invalidate();
		}

		private void ApplyDarkTheme()
		{
			BackColor = ThemeBg;
			ForeColor = ThemeText;
			Font = new Font("Segoe UI", 9F);

			richTextBox1.BackColor = ThemePanel;
			richTextBox1.ForeColor = ThemeText;
			richTextBox1.BorderStyle = BorderStyle.None;
			richTextBox1.SelectionBackColor = ThemePanel;
			richTextBox1.SelectionColor = ThemeText;
			richTextBox1.HideSelection = true;

			titleLabel.ForeColor = ThemeAccent;
			labelAgentCaption.ForeColor = ThemeMuted;
			labelAgentValue.ForeColor = ThemeAccent;

			btnClose.FlatStyle = FlatStyle.Flat;
			btnClose.FlatAppearance.BorderSize = 0;
			btnClose.BackColor = ThemePanel;
			btnClose.ForeColor = ThemeText;
			btnClose.Cursor = Cursors.Hand;
			btnClose.UseVisualStyleBackColor = false;
			btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 70, 70);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			base.Activated += Init;
		}

		private void Init(object sender, EventArgs e)
		{
			base.Activated -= Init;
			string dates = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Room.log";
			InMemorySink logEventSink = new InMemorySink(richTextBox1);
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
				.MinimumLevel.Override("Quartz", LogEventLevel.Warning)
				.MinimumLevel.Override("Akka", LogEventLevel.Warning)
				.WriteTo.Sink(logEventSink, restrictedToMinimumLevel: LogEventLevel.Information)
				.WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Information).WriteTo.File(".\\Logs\\Room\\Info\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Warning).WriteTo.File(".\\Logs\\Room\\Warning\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Error).WriteTo.File(".\\Logs\\Room\\Error\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).CreateLogger();
			if (!File.Exists("settings.ini"))
			{
				Log.Error("settings.ini doesn't exist!");
				return;
			}
			Boot();
			Config config = ConfigurationFactory.ParseString("\r\n                                akka { \r\n                                    loglevel = INFO,\r\n                                    actor {\r\n                                        provider = remote\r\n                                    }\r\n                                    remote {\r\n                                        dot-netty.tcp {\r\n                                            port = {RMPORT} \r\n                                            hostname = {RMIP}\r\n                                            public-hostname = {RMIP}\r\n                                        }\r\n                                    }\r\n                                    log-dead-letters-during-shutdown = off,\r\n                                    log-dead-letters = 0,\r\n                                    loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]\r\n                                }".Replace("{RMIP}", Conf.RMServerIP).Replace("{RMPORT}", Conf.RMLocalPort.ToString()));
			ServerStatus.MainActorSystem = ActorSystem.Create("Room", config);
			Log.Information("RoomServer loading game data in background...");
			// Heavy DB/holder loads must not run on the UI thread (causes Not Responding).
			Task.Run(delegate
			{
				try
				{
					RoomHolder.LoadRoomKindInfo();
					MapHolder.LoadMapInfo();
					MapHolder.LoadMapRoomKind();
					MapHolder.LoadAssaultModeLimitInfo();
					MapHolder.LoadRunlympicMapInfo();
					MapHolder.LoadBonusStageInfo();
					MapItemHolder.LoadCapsuleItemMapJoint();
					MapItemHolder.LoadMapCapsuleItemInfo();
					MapItemHolder.LoadAssaultModeRewardInfo();
					MapCardHolder.LoadMapCardRateInfo();
					ItemHolder.LoadItemInfo();
					ItemHolder.LoadItemSetInfo();
					ItemHolder.LoadPetExpInfo();
					ItemHolder.LoadExchangeSystemInfo();
					ItemHolder.LoadItemTransformInfo();
					AccountHolder.LoadLevelInfo();
					RunQuizHolder.LoadRunQuizInfo();
					GameModeHolder.LoadCorunModeResultInfo();
					GameRewardHolder.LoadGameRewardInfo();
					ServerSettingHolder.LoadHashList();
					SubjectKingHolder.LoadSubjectKingQuestionAnswer();
					ItemRacingHolder.LoadItemRacingData();
					TypingRunHolder.LoadTypingRunData();
					TowerEventHolder.LoadTowerQuizInfo();
					if (ServerSettingHolder.ServerSettings.useThankOfferingSystem)
					{
						ThankOfferingSystem.LoadSchedule();
					}
					IceFlowerHolder.LoadIceFlowerInfo();
					RoomResultTable.InitResultTable();
					if (IsDisposed)
					{
						return;
					}
					BeginInvoke(new Action(delegate
					{
						if (IsDisposed)
						{
							return;
						}
						ServerStatus.ServerActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new AgentServer()), "AgentClient");
						Log.Information("RoomServer ready (AgentClient started)");
					}));
				}
				catch (Exception ex)
				{
					Log.Error(ex, "RoomServer data load failed");
				}
			});
		}

		private bool Boot()
		{
			try
			{
				Text = "TR Server : RoomServer";
				titleLabel.Text = "TR Server : RoomServer";
				IniData iniData = new FileIniDataParser().ReadFile("settings.ini");
				Conf.ServerIP = iniData["Server"]["AgentServerIP"].Replace(" ", "");
				Conf.AgentPort = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort"].Replace(" ", ""));
				Conf.AgentPort2 = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort2"].Replace(" ", ""));
				Conf.RelayPort = Convert.ToUInt16(iniData["Server"]["RelayServerPort"].Replace(" ", ""));
				Conf.CommunityAgentServerPort = Convert.ToUInt16(iniData["Server"]["CommunityServerPort"].Replace(" ", ""));
				Conf.LoadBalanceServerPort = Convert.ToUInt16(iniData["Server"]["LoadBalanceServerPort"].Replace(" ", ""));
				Conf.RMLocalPort = Convert.ToUInt16(iniData["Server"]["RoomServerLocalPort"].Replace(" ", ""));
				Conf.RMServerIP = iniData["Server"]["RoomServerIP"].Replace(" ", "");
				Conf.Connstr = iniData["Server"]["MySQLConnection"];
				Conf.HashCheck = Convert.ToBoolean(iniData["Server"]["HashCheck"].Replace(" ", ""));
				Conf.MaxUserCount = Convert.ToInt32(iniData["Server"]["MaxUserCount"].Replace(" ", ""));
				Conf.BlockDDOS = Convert.ToBoolean(iniData["Server"]["BlockDDOS"].Replace(" ", ""));
				Conf.JudgeTime = Convert.ToInt32(iniData["Server"]["JudgeTime"].Replace(" ", ""));
				Conf.MaxConnectTime = Convert.ToInt32(iniData["Server"]["MaxConnectTime"].Replace(" ", ""));
				Conf.TimedOutCheckTime = Convert.ToInt32(iniData["Server"]["TimedOutCheckTime"].Replace(" ", ""));
				Conf.EnableTimedOutCheck = Convert.ToBoolean(iniData["Server"]["EnableTimedOutCheck"].Replace(" ", ""));
				Log.Information("Loading Settings.ini........Done");
				MySqlConnection mySqlConnection = CreateConnection();
				if (mySqlConnection == null)
				{
					return false;
				}
				mySqlConnection.Close();
				ServerSettingHolder.LoadServerSettingInfo();
				Log.Information("Initializing DB........Done");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
			}
			return false;
		}

		private MySqlConnection CreateConnection()
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				return mySqlConnection;
			}
			catch (Exception ex)
			{
				Log.Error("Error on DB connect: {0}", ex.Message);
				return null;
			}
		}

		public static void UpdateLableStatic(int ID)
		{
			if (form != null)
			{
				form.UpdateLable(ID);
			}
		}

		private void UpdateLable(int ID)
		{
			if (base.InvokeRequired)
			{
				Invoke(new EnableDelegate(UpdateLable), ID);
			}
			else
			{
				labelAgentValue.Text = ID.ToString();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.labelAgentCaption = new System.Windows.Forms.Label();
			this.labelAgentValue = new System.Windows.Forms.Label();
			this.titleLabel = new System.Windows.Forms.Label();
			this.btnClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// titleLabel
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.titleLabel.Location = new System.Drawing.Point(12, 10);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(98, 20);
			this.titleLabel.TabIndex = 3;
			this.titleLabel.Text = "TR Server : RoomServer";
			// btnClose
			this.btnClose.FlatAppearance.BorderSize = 0;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Location = new System.Drawing.Point(694, 8);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(28, 28);
			this.btnClose.TabIndex = 4;
			this.btnClose.TabStop = false;
			this.btnClose.Text = "×";
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// richTextBox1
			this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(232, 232, 236);
			this.richTextBox1.HideSelection = true;
			this.richTextBox1.Location = new System.Drawing.Point(12, 56);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(710, 370);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// labelAgentCaption
			this.labelAgentCaption.AutoSize = true;
			this.labelAgentCaption.Location = new System.Drawing.Point(540, 34);
			this.labelAgentCaption.Name = "labelAgentCaption";
			this.labelAgentCaption.Size = new System.Drawing.Size(100, 15);
			this.labelAgentCaption.TabIndex = 1;
			this.labelAgentCaption.Text = "Connected Agent:";
			// labelAgentValue
			this.labelAgentValue.AutoSize = true;
			this.labelAgentValue.Location = new System.Drawing.Point(648, 34);
			this.labelAgentValue.Name = "labelAgentValue";
			this.labelAgentValue.Size = new System.Drawing.Size(13, 15);
			this.labelAgentValue.TabIndex = 2;
			this.labelAgentValue.Text = "0";
			// label1 kept for binary compat (hidden)
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(0, 15);
			this.label1.TabIndex = 5;
			this.label1.Visible = false;
			// Form1
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(22, 22, 24);
			this.ClientSize = new System.Drawing.Size(734, 439);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.titleLabel);
			this.Controls.Add(this.labelAgentValue);
			this.Controls.Add(this.labelAgentCaption);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.richTextBox1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.FromArgb(232, 232, 236);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TR Server : RoomServer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
