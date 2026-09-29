using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgentServer.Database;
using AgentServer.Dialog;
using AgentServer.EasyAntiCheat;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.XignCode;
using Akka.Actor;
using Akka.Configuration;
using Akka.Quartz.Actor;
using Akka.Quartz.Actor.Commands;
using IniParser;
using IniParser.Model;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using NetMsg.Room;
using Quartz;
using Serilog;
using Serilog.Events;
using TRCommon;

namespace AgentServer
{
	public class Form1 : Form
	{
		private delegate void EnableDelegate(int id);

		private CapsuleMachineManager cpm;

		private ServerSettingManager ssm;

		private GMTool gmtool;

		private System.Threading.Timer sockettimer;

		private System.Threading.Timer checktimer;

		private System.Threading.Timer checktimer2;

		private System.Threading.Timer eactimer;

		private static Form1 form;

		private IContainer components;

		private RichTextBox richTextBox1;

		private Button btnStopServer;

		private Label label1;

		private Label label2;

		private TextBox txtNotice;

		private Button btnNotice;

		private Button btnOpenHash;

		private Button btnOpenDir;

		private Button btnShowUserNum;

		private Button btnReloadtblServerSettingInfo;

		private Button btnCapsuleMachineManager;

		private Button btnReloadMap;

		private Button btnGMTool;

		private Button btnReloadGameReward;

		private Button btnReloadHash;

		private Button btnReloadHackingToolHash;

		private Button btnReloadRenewalShop;

		private Label label5;

		private Button btnReloadFishing;

		private Button btnReloadMonthlyGacha;

		private Button btnReloadHuMongPickBoard;

		private CheckBox chkUseLuckyBag;

		private Label label6;

		public CheckBox chkOpenShop;

		public CheckBox chkServerReady;
        private Button btnServerSettingManager;
        private Button btnUpdatCollectioneRanking;
        private Button btnUpdateRanking;
        private Button btnReloadSingleChallenge;
		private Label label4;
		private Label labelServerBuild;
		private Label labelServerIdValue;
		private Label labelServerBuildCaption;
		private Label titleLabel;
		private Button btnClose;
		private Panel logScrollRail;
		private Panel logScrollThumb;

		// TRPkgTools dark palette
		static readonly Color ThemeBg = Color.FromArgb(22, 22, 24);
		static readonly Color ThemePanel = Color.FromArgb(32, 32, 36);
		static readonly Color ThemeBorder = Color.FromArgb(58, 58, 64);
		static readonly Color ThemeText = Color.FromArgb(232, 232, 236);
		static readonly Color ThemeMuted = Color.FromArgb(150, 150, 158);
		static readonly Color ThemeAccent = Color.FromArgb(88, 166, 255);
		static readonly Color ThemeAccentText = Color.FromArgb(16, 16, 18);
		static readonly Color ThemeDanger = Color.FromArgb(180, 60, 60);
		static readonly Color ThemeErr = Color.FromArgb(232, 110, 110);
		static readonly Color ThemeSelect = Color.FromArgb(48, 72, 104);
		static readonly Color ThemeScroll = Color.FromArgb(64, 64, 72);
		static readonly Color ThemeScrollThumb = Color.FromArgb(72, 72, 80);

		Point _dragOffset;
		bool _dragging;
		bool _thumbDrag;
		int _thumbDragOffsetY;
		bool _logUiBusy;
		DateTime _lastScrollSyncUtc = DateTime.MinValue;

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

		[DllImport("user32.dll")]
		private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern bool GetScrollInfo(IntPtr hwnd, int nBar, ref SCROLLINFO lpsi);

		[DllImport("user32.dll")]
		private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

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

		private void SetupDarkLogScroll()
		{
			// Use native scrollbar only. Custom overlay + GetScrollInfo/VScroll sync
			// was contributing to UI freezes under log load.
			richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
			richTextBox1.HandleCreated += (_, __) => ApplyDarkScrollBars(richTextBox1);
			if (richTextBox1.IsHandleCreated)
			{
				ApplyDarkScrollBars(richTextBox1);
			}
		}

		private bool TryGetLogScroll(out int pos, out int max, out int page)
		{
			pos = 0;
			max = 0;
			page = 0;
			if (!richTextBox1.IsHandleCreated)
			{
				return false;
			}
			SCROLLINFO si = new SCROLLINFO
			{
				cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO)),
				fMask = SIF_RANGE | SIF_PAGE | SIF_POS
			};
			if (!GetScrollInfo(richTextBox1.Handle, SB_VERT, ref si))
			{
				return false;
			}
			pos = si.nPos;
			max = si.nMax;
			page = (int)si.nPage;
			return true;
		}

		private void SyncLogScrollThumb()
		{
			if (logScrollRail == null || logScrollThumb == null || _thumbDrag)
			{
				return;
			}
			if (!TryGetLogScroll(out int pos, out int max, out int page))
			{
				logScrollThumb.Visible = false;
				return;
			}
			int range = Math.Max(1, max - page + 1);
			bool needed = max > page && page > 0;
			logScrollThumb.Visible = needed;
			logScrollRail.Visible = true;
			if (!needed)
			{
				return;
			}
			int track = Math.Max(20, logScrollRail.Height);
			int thumbH = Math.Max(24, (int)(track * ((double)page / (max + 1))));
			thumbH = Math.Min(track, thumbH);
			int maxTop = Math.Max(0, track - thumbH);
			int top = (int)(maxTop * ((double)pos / Math.Max(1, range - 1)));
			logScrollThumb.Height = thumbH;
			logScrollThumb.Top = Math.Max(0, Math.Min(maxTop, top));
		}

		private void ScrollLogToThumb()
		{
			if (!TryGetLogScroll(out _, out int max, out int page))
			{
				return;
			}
			int range = Math.Max(1, max - page + 1);
			int maxTop = Math.Max(1, logScrollRail.Height - logScrollThumb.Height);
			int pos = (int)((range - 1) * ((double)logScrollThumb.Top / maxTop));
			pos = Math.Max(0, Math.Min(range - 1, pos));
			SetScrollPos(richTextBox1.Handle, SB_VERT, pos, true);
			SendMessage(richTextBox1.Handle, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION | (pos << 16)), IntPtr.Zero);
			richTextBox1.Invalidate();
		}

		private static void ApplyDarkScrollBars(Control control)
		{
			if (control == null || !control.IsHandleCreated)
			{
				return;
			}
			SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
			EnumChildWindows(control.Handle, (hwnd, _) =>
			{
				StringBuilder name = new StringBuilder(64);
				GetClassName(hwnd, name, name.Capacity);
				if (string.Equals(name.ToString(), "ScrollBar", StringComparison.OrdinalIgnoreCase))
				{
					SetWindowTheme(hwnd, "DarkMode_Explorer", null);
				}
				return true;
			}, IntPtr.Zero);
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
			foreach (Control c in new Control[] { titleLabel, label6, labelServerIdValue, label1, label2, labelServerBuildCaption, labelServerBuild })
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
			btnStopServer_Click(sender, e);
		}

		private void ApplyDarkTheme()
		{
			BackColor = ThemeBg;
			ForeColor = ThemeText;
			Font = new Font("Segoe UI", 9F);

			richTextBox1.BackColor = ThemePanel;
			richTextBox1.ForeColor = ThemeText;
			richTextBox1.BorderStyle = BorderStyle.None;
			// Match selection colors to the log background so any residual selection is invisible.
			richTextBox1.SelectionBackColor = ThemePanel;
			richTextBox1.SelectionColor = ThemeText;
			richTextBox1.HideSelection = true;

			txtNotice.BackColor = ThemePanel;
			txtNotice.ForeColor = ThemeText;
			txtNotice.BorderStyle = BorderStyle.FixedSingle;

			foreach (Control c in Controls)
			{
				if (c is Label lbl)
				{
					lbl.BackColor = Color.Transparent;
					bool value = ReferenceEquals(lbl, label2)
						|| ReferenceEquals(lbl, labelServerIdValue)
						|| ReferenceEquals(lbl, labelServerBuild)
						|| ReferenceEquals(lbl, titleLabel);
					if (ReferenceEquals(lbl, label5))
					{
						lbl.ForeColor = ThemeErr;
					}
					else if (value)
					{
						lbl.ForeColor = ThemeAccent;
					}
					else
					{
						lbl.ForeColor = ThemeMuted;
					}
				}
				else if (c is CheckBox chk)
				{
					chk.BackColor = ThemeBg;
					chk.ForeColor = ThemeMuted;
					chk.FlatStyle = FlatStyle.Standard;
					chk.UseVisualStyleBackColor = false;
				}
				else if (c is Button btn)
				{
					bool primary = ReferenceEquals(btn, btnNotice);
					bool danger = ReferenceEquals(btn, btnStopServer) || ReferenceEquals(btn, btnClose);
					btn.FlatStyle = FlatStyle.Flat;
					btn.FlatAppearance.BorderSize = 0;
					btn.Cursor = Cursors.Hand;
					btn.UseVisualStyleBackColor = false;
					if (primary)
					{
						btn.BackColor = ThemeAccent;
						btn.ForeColor = ThemeAccentText;
						btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
						btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 180, 255);
					}
					else if (danger)
					{
						btn.BackColor = ReferenceEquals(btn, btnClose) ? ThemePanel : ThemeDanger;
						btn.ForeColor = ThemeText;
						btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 70, 70);
					}
					else
					{
						btn.BackColor = ThemePanel;
						btn.ForeColor = ThemeText;
						btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 54);
					}
				}
			}

			label4.BackColor = ThemeBg;
			label4.ForeColor = ThemeMuted;
		}

		private bool _bootStarted;

		private void Form1_Load(object sender, EventArgs e)
		{
			// Boot on Shown (not Activated): Cursor/background launches often never
			// raise Activated, so the UI sat empty and the server never listened.
			Shown += Form1_ShownBoot;
		}

		private void Form1_ShownBoot(object sender, EventArgs e)
		{
			Shown -= Form1_ShownBoot;
			BeginBoot();
		}

		private void BeginBoot()
		{
			if (_bootStarted)
			{
				return;
			}
			_bootStarted = true;
			ShowInTaskbar = true;
			if (WindowState == FormWindowState.Minimized)
			{
				WindowState = FormWindowState.Normal;
			}
			BringToFront();
			Activate();
			Init(this, EventArgs.Empty);
		}

		private void Init(object sender, EventArgs e)
		{
			string dates = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Agent.log";
			InMemorySink logEventSink = new InMemorySink(richTextBox1);
			// UI sink: Information+ only. Debug/Akka spam was freezing the WinForms UI (Not Responding).
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
				.MinimumLevel.Override("Quartz", LogEventLevel.Warning)
				.MinimumLevel.Override("Akka", LogEventLevel.Warning)
				.WriteTo.Sink(logEventSink, restrictedToMinimumLevel: LogEventLevel.Information)
				.WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Information).WriteTo.File(".\\Logs\\Agent\\Info\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Warning).WriteTo.File(".\\Logs\\Agent\\Warning\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e1) => e1.Level == LogEventLevel.Error).WriteTo.File(".\\Logs\\Agent\\Error\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).CreateLogger();
			if (!Boot())
			{
				return;
			}
			Config config = ConfigurationFactory.ParseString("\r\n                                akka { \r\n                                    loglevel = INFO,\r\n                                     actor {\r\n                                        provider = remote\r\n                                    }\r\n                                    remote {\r\n                                        dot-netty.tcp {\r\n                                            port = 0 \r\n                                            hostname = localhost\r\n                                        }\r\n                                    }\r\n                                    log-dead-letters-during-shutdown = off,\r\n                                    log-dead-letters = 0,\r\n                                    loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]\r\n                                }");
			ServerStatus.MainActorSystem = ActorSystem.Create("Agent", config);
			ServerStatus.QuartzActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new QuartzActor()), "Quartz");
			ServerSettingHolder.LoadHashList();
			Task.Run(delegate
			{
				DBInit.initlevel_run();
				DBInit.inithacktool_run();
				ServerSettingHolder.LoadServerSettingInfo();
				DBInit.initSmartChannelModeInfo_run();
				DBInit.initSmartChannelScheduleInfo_run();
				DBInit.initRoomKindPenaltyInfo_run();
				Log.Information("Initializing DB........Done");
				DBInit.startServerInitDB();
				LoginTrafficManager.Init();
				RoomHolder.LoadRoomKindInfo();
				MapHolder.LoadMapInfo();
				MapHolder.LoadAssaultModeLimitInfo();
				SingleChallengeHolder.LoadChallengeMapInfo();
				ShopHolder.LoadShopInfo();
				ItemHolder.LoadItemInfo();
				ItemHolder.LoadItemSetInfo();
				ItemHolder.LoadPetExpInfo();
				ItemHolder.LoadExchangeSystemInfo();
				ItemHolder.LoadItemTransformInfo();
				CapsuleMachineHolder.LoadCapsuleMachineInfo();
				AccountHolder.LoadLevelInfo();
				HotTimeHolder.LoadHotTimeInfo();
				FishingHolder.LoadFishingInfo();
				EventPickBoardHolder.LoadEventPickBoardInfo();
				EventPickBoardHolder.LoadHuMongPickBoardInfo();
				EventPickBoardHolder.LoadDiceBoardOpenList();
				TalesKnightHolder.LoadTalesKnightStageReward();
				MissionHolder.LoadMissionDataInfo();
				AnniversaryHolder.LoadAnniversaryInfo();
				ItemCubeHolder.LoadItemCubeInfo();
				ItemTradingHolder.LoadItemTradeInfo();
				CombinationShopHolder.LoadCombinationShopInfo();
				CapybaraShopHolder.Load();
				AutoEnableServerWhenDatabaseReady();
			});
			ServerStatus.ServerActor = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new AgentServer(new IPEndPoint(IPAddress.Parse(Conf.ServerIP), Conf.AgentPort))), "Client");
			Log.Information("AgentServer is listening on {0}:{1}", Conf.ServerIP, Conf.AgentPort);
			ServerStatus.RoomServerActor = ServerStatus.MainActorSystem.ActorOf(Props.Create<RoomServer>(Array.Empty<object>()));
			ServerStatus.LBServerActor = ServerStatus.MainActorSystem.ActorOf(Props.Create<LBServer>(Array.Empty<object>()), "LBServerActor");
			Task.Run(async delegate
			{
				while (!ServerStatus.RoomServerConnected)
				{
					try
					{
						ServerStatus.RoomServerActor.Tell(new AgentConnectRequest
						{
							Username = "Agent"
						});
					}
					catch (Exception)
					{
						Log.Information("Unable to Connect RoomServer, Retry After 5 Second");
					}
					await Task.Delay(5000);
				}
			});
			Task.Run(async delegate
			{
				while (!ServerStatus.LBServerConnected)
				{
					try
					{
						ServerStatus.LBServerActor.Tell(new AgentToLBSRequest
						{
							Port = Conf.AgentPort
						});
					}
					catch (Exception)
					{
						Log.Information("Unable to Connect LBServer, Retry After 5 Second");
					}
					await Task.Delay(5000);
				}
			});
			try
			{
				if (EACServer.DoStartup())
				{
					EACTimer();
				}
			}
			catch (Exception ex)
			{
				Log.Warning("EAC DoStartup skipped: {0}", ex.Message);
			}
			try
			{
				XignCodeServer.DoStartup();
			}
			catch (Exception ex)
			{
				Log.Warning("XignCode DoStartup skipped: {0}", ex.Message);
			}
			if (Conf.EnableTimedOutCheck)
			{
				IActorRef to = ServerStatus.MainActorSystem.ActorOf(Props.Create(() => new SocketCheck()), "TimeOutCheck");
				ServerStatus.QuartzActor.Tell(new CreateJob(to, "", TriggerBuilder.Create().WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
				{
					x.WithIntervalInSeconds(60).RepeatForever();
				}).Build()));
			}
			SocketTimeoutCheck();
		}

		private bool Boot()
		{
			try
			{
				Text = "TR SERVER : AgentServer · R186632";
				titleLabel.Text = "TR SERVER : AgentServer · R186632";
				IniData iniData = new FileIniDataParser().ReadFile("settings.ini");
				Conf.ServerIP = iniData["Server"]["AgentServerIP"].Replace(" ", "");
				if (iniData["Server"].ContainsKey("RelayAdvertiseIP") && !string.IsNullOrWhiteSpace(iniData["Server"]["RelayAdvertiseIP"]))
				{
					Conf.RelayAdvertiseIP = iniData["Server"]["RelayAdvertiseIP"].Replace(" ", "");
				}
				Conf.AgentPort = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort"].Replace(" ", ""));
				Conf.AgentPort2 = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort2"].Replace(" ", ""));
				Conf.RelayPort = Convert.ToUInt16(iniData["Server"]["RelayServerPort"].Replace(" ", ""));
				Conf.CommunityAgentServerPort = Convert.ToUInt16(iniData["Server"]["CommunityServerPort"].Replace(" ", ""));
				Conf.LBSLocalPort = Convert.ToUInt16(iniData["Server"]["LoadBalanceServerLocalPort"].Replace(" ", ""));
				Conf.RMServerIP = iniData["Server"]["RoomServerIP"].Replace(" ", "");
				Conf.RMLocalPort = Convert.ToUInt16(iniData["Server"]["RoomServerLocalPort"].Replace(" ", ""));
				Conf.Connstr = iniData["Server"]["MySQLConnection"];
				Conf.HashCheck = Convert.ToBoolean(iniData["Server"]["HashCheck"].Replace(" ", ""));
				Conf.MaxUserCount = Convert.ToInt32(iniData["Server"]["MaxUserCount"].Replace(" ", ""));
				Conf.MaxTotalAgentUserCount = Convert.ToInt32(iniData["Server"]["MaxTotalAgentUserCount"].Replace(" ", ""));
				Conf.BlockDDOS = Convert.ToBoolean(iniData["Server"]["BlockDDOS"].Replace(" ", ""));
				Conf.JudgeTime = Convert.ToInt32(iniData["Server"]["JudgeTime"].Replace(" ", ""));
				Conf.MaxConnectTime = Convert.ToInt32(iniData["Server"]["MaxConnectTime"].Replace(" ", ""));
				Conf.TimedOutCheckTime = Convert.ToInt32(iniData["Server"]["TimedOutCheckTime"].Replace(" ", ""));
				Conf.EnableTimedOutCheck = Convert.ToBoolean(iniData["Server"]["EnableTimedOutCheck"].Replace(" ", ""));
				Conf.ProtocolDebug = iniData["Server"].ContainsKey("ProtocolDebug")
					&& Convert.ToBoolean(iniData["Server"]["ProtocolDebug"].Replace(" ", ""));
				Conf.EnableXignCode = iniData["Server"].ContainsKey("EnableXignCode")
					&& Convert.ToBoolean(iniData["Server"]["EnableXignCode"].Replace(" ", ""));
				Log.Information("Loading Settings.ini........Done");
				Log.Information("ProtocolDebug={0}", Conf.ProtocolDebug);
				Log.Information("EnableXignCode={0}", Conf.EnableXignCode);
				Log.Information("Relay advertise IP for LOGIN_OK: {0}:{1}", Conf.GetRelayAdvertiseIP(), Conf.RelayPort);
				string buildStamp = GetAssemblyBuildStamp();
				Log.Information("ServerBuild={0}", buildStamp);
				label2.Text = Conf.ServerIP + ":" + Conf.AgentPort;
				labelServerBuildCaption.Text = "ServerBuild:";
				labelServerBuild.Text = buildStamp;
				MySqlConnection mySqlConnection = CreateConnection();
				if (mySqlConnection == null)
				{
					return false;
				}
				mySqlConnection.Close();
				// Heavy DB init runs in Task.Run below so the form stays responsive.
				Log.Information("Settings OK; loading DB in background...");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
			}
			return false;
		}

		private void SocketTimeoutCheck()
		{
		}

		private void EACTimer()
		{
			eactimer = new System.Threading.Timer(delegate
			{
				EACServer.DoUpdate();
			}, null, 1000, 1000);
		}

		private void btnStopServer_Click(object sender, EventArgs e)
		{
			if (DarkUi.Confirm(this, "Do you want to shut down the server?", "Shut Down") != DialogResult.Yes)
			{
				return;
			}
			foreach (Account item in ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin))
			{
				HandleLogout(item);
			}
			EACServer.DoShutdown();
			try
			{
				XignCodeServer.DoShutdown();
			}
			catch (Exception ex)
			{
				Log.Warning("XignCode DoShutdown: {0}", ex.Message);
			}
			Close();
		}

		private void chkServerReady_CheckedChanged(object sender, EventArgs e)
		{
			if (!ShopItemTable.isRecvItemListFromDB)
			{
				DarkUi.Alert(this, "Loading ItemInfo From DB!", "Error", danger: true);
				bool isReady = (chkServerReady.Checked = false);
				ServerStatus.isReady = isReady;
			}
			else
			{
				ServerStatus.isReady = chkServerReady.Checked;
				ServerStatus.LBServerActor.Tell(new SetServerReady
				{
					isSet = chkServerReady.Checked
				});
				Log.Information("Set Server Ready : {0}", ServerStatus.isReady);
			}
		}

		private void btnNotice_Click(object sender, EventArgs e)
		{
			string text = txtNotice.Text;
			if (text.Length > 0)
			{
				ServerStatus.LBServerActor.Tell(new NoticePacket(text, 16));
				Log.Information("Send notice NoticeType : 0, noticeKind : 1,  {0}", text);
			}
		}

		private void btnOpenHash_Click(object sender, EventArgs e)
		{
			Process.Start("hash.ini");
		}

		private void btnOpenDir_Click(object sender, EventArgs e)
		{
			string startupPath = Application.StartupPath;
			Process.Start("explorer.exe", startupPath);
		}

		private async void btnShowUserNum_Click(object sender, EventArgs e)
		{
			await Task.Run(delegate
			{
				int propertyValue = ClientConnection.CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin);
				int propertyValue2 = Rooms.RoomList.Values.Count((NormalRoom rm) => rm.RoomKindID != 74 && rm.PlayerCount > 0);
				int propertyValue3 = Rooms.RoomList.Values.Count((NormalRoom rm) => rm.RoomKindID == 74);
				Log.Information("ShowInfo - user({0}), room({1}), parkroom({2})", propertyValue, propertyValue2, propertyValue3);
			});
		}

		private void btnReloadtblServerSettingInfo_Click(object sender, EventArgs e)
		{
			ServerStatus.LBServerActor.Tell(new ReloadSetting
			{
				Code = 8
			});
			ServerStatus.ToAllRoomServer(new ReloadSetting
			{
				Code = 8
			});
			Log.Information("Reloaded tblServerSettingInfo!");
		}

		private void btnCapsuleMachineManager_Click(object sender, EventArgs e)
		{
			if (cpm == null || cpm.IsDisposed)
			{
				cpm = new CapsuleMachineManager();
			}
			cpm.Show();
			cpm.Focus();
		}

		private void btnReloadMap_Click(object sender, EventArgs e)
		{
			ServerStatus.ToAllRoomServer(new ReloadSetting
			{
				Code = 5
			});
		}

		private void btnGMTool_Click(object sender, EventArgs e)
		{
			if (gmtool == null || gmtool.IsDisposed)
			{
				gmtool = new GMTool();
			}
			gmtool.Show();
			gmtool.Focus();
		}

		private void btnReloadGameReward_Click(object sender, EventArgs e)
		{
			ServerStatus.ToAllRoomServer(new ReloadSetting
			{
				Code = 6
			});
		}

		private void btnReloadHash_Click(object sender, EventArgs e)
		{
			ServerStatus.LBServerActor.Tell(new ReloadHash
			{
				Hash = File.ReadAllLines("hash.ini")
			});
		}

		private void btnReloadHackingToolHash_Click(object sender, EventArgs e)
		{
			DBInit.inithacktool_run();
			if (DBInit.HackTools != null)
			{
				ServerStatus.MainActorSystem.ActorSelection("/user/Client/*").Tell(new NP_Byte(DBInit.HackTools));
			}
			Log.Information("Reloaded tblHackingToolHash!");
		}

		private void HandleLogout(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_logout";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
				mySqlCommand.Parameters.Add("puid", MySqlDbType.VarString).Value = User.UserID;
				mySqlCommand.Parameters.Add("pexp", MySqlDbType.Int64).Value = User.Exp;
				mySqlCommand.Parameters.Add("ip", MySqlDbType.VarString).Value = User.LastIp;
				mySqlCommand.Parameters.Add("logintime", MySqlDbType.DateTime).Value = User.LoginDateTime;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("Logout sql error: {0}", ex.Message);
			}
		}

		private void btnReloadRenewalShop_Click(object sender, EventArgs e)
		{
			ServerStatus.LBServerActor.Tell(new ReloadSetting
			{
				Code = 2
			});
		}

		private void btnReloadFishing_Click(object sender, EventArgs e)
		{
			ServerStatus.LBServerActor.Tell(new ReloadSetting
			{
				Code = 7
			});
		}

		private void btnReloadMonthlyGacha_Click(object sender, EventArgs e)
		{
		}

		private void btnReloadHuMongPickBoard_Click(object sender, EventArgs e)
		{
			ServerStatus.LBServerActor.Tell(new ReloadSetting
			{
				Code = 3
			});
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

		private void chkUseLuckyBag_CheckedChanged(object sender, EventArgs e)
		{
			ServerStatus.enableUseLuckyBag = chkUseLuckyBag.Checked;
			Log.Information("Set EnableUseLuckyBag : {0}", ServerStatus.enableUseLuckyBag);
		}

		private void chkOpenShop_CheckedChanged(object sender, EventArgs e)
		{
			ServerStatus.CanShopOperation = chkOpenShop.Checked;
			ServerStatus.LBServerActor.Tell(new CanShopOperation
			{
				isSet = chkOpenShop.Checked
			});
			Log.Information("Set CanShopOperation : {0}", ServerStatus.CanShopOperation);
		}

		/// <summary>
		/// After all DB holder loads finish, turn on shop + server-ready if item data loaded OK.
		/// Uses the same checks/path as the CanShopOperation and SetServerReady checkboxes.
		/// </summary>
		private static void AutoEnableServerWhenDatabaseReady()
		{
			if (form == null || form.IsDisposed)
			{
				return;
			}
			form.BeginInvoke(new Action(form.EnableServerWhenDatabaseReady));
		}

		private void EnableServerWhenDatabaseReady()
		{
			if (!ShopItemTable.isRecvItemListFromDB)
			{
				Log.Warning("Database loaders finished but item list is not ready; CanShopOperation and SetServerReady were not auto-enabled");
				return;
			}
			Log.Information("Database ready; auto-enabling CanShopOperation and SetServerReady");
			if (!chkOpenShop.Checked)
			{
				chkOpenShop.Checked = true;
			}
			if (!chkServerReady.Checked)
			{
				chkServerReady.Checked = true;
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
				labelServerIdValue.Text = ID.ToString();
			}
		}

		private static string GetAssemblyBuildStamp()
		{
			try
			{
				string path = Assembly.GetEntryAssembly()?.Location;
				if (!string.IsNullOrEmpty(path) && File.Exists(path))
				{
					// InvariantCulture: Thai locale uses Buddhist year (2569) for "yyyy".
					return File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
				}
			}
			catch
			{
			}
			return "unknown";
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
            this.btnStopServer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkServerReady = new System.Windows.Forms.CheckBox();
            this.txtNotice = new System.Windows.Forms.TextBox();
            this.btnNotice = new System.Windows.Forms.Button();
            this.btnOpenHash = new System.Windows.Forms.Button();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnShowUserNum = new System.Windows.Forms.Button();
            this.btnReloadtblServerSettingInfo = new System.Windows.Forms.Button();
            this.btnCapsuleMachineManager = new System.Windows.Forms.Button();
            this.btnReloadMap = new System.Windows.Forms.Button();
            this.btnGMTool = new System.Windows.Forms.Button();
            this.btnReloadGameReward = new System.Windows.Forms.Button();
            this.btnReloadHash = new System.Windows.Forms.Button();
            this.btnReloadHackingToolHash = new System.Windows.Forms.Button();
            this.btnReloadRenewalShop = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.btnReloadFishing = new System.Windows.Forms.Button();
            this.btnReloadMonthlyGacha = new System.Windows.Forms.Button();
            this.btnReloadHuMongPickBoard = new System.Windows.Forms.Button();
            this.chkUseLuckyBag = new System.Windows.Forms.CheckBox();
            this.chkOpenShop = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.labelServerIdValue = new System.Windows.Forms.Label();
            this.labelServerBuildCaption = new System.Windows.Forms.Label();
            this.labelServerBuild = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnServerSettingManager = new System.Windows.Forms.Button();
            this.btnUpdatCollectioneRanking = new System.Windows.Forms.Button();
            this.btnUpdateRanking = new System.Windows.Forms.Button();
            this.btnReloadSingleChallenge = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(36)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.richTextBox1.Location = new System.Drawing.Point(12, 66);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(731, 465);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // btnStopServer
            // 
            this.btnStopServer.Location = new System.Drawing.Point(604, 630);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(137, 31);
            this.btnStopServer.TabIndex = 1;
            this.btnStopServer.Text = "Shut Down";
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(128, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "ServerIP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(188, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "127.0.0.1:9153";
            // 
            // chkServerReady
            // 
            this.chkServerReady.AutoSize = true;
            this.chkServerReady.Location = new System.Drawing.Point(601, 579);
            this.chkServerReady.Name = "chkServerReady";
            this.chkServerReady.Size = new System.Drawing.Size(110, 19);
            this.chkServerReady.TabIndex = 4;
            this.chkServerReady.Text = "SetServerREADY";
            this.chkServerReady.UseVisualStyleBackColor = true;
            this.chkServerReady.CheckedChanged += new System.EventHandler(this.chkServerReady_CheckedChanged);
            // 
            // txtNotice
            // 
            this.txtNotice.Location = new System.Drawing.Point(10, 726);
            this.txtNotice.Name = "txtNotice";
            this.txtNotice.Size = new System.Drawing.Size(615, 23);
            this.txtNotice.TabIndex = 5;
            // 
            // btnNotice
            // 
            this.btnNotice.Location = new System.Drawing.Point(633, 722);
            this.btnNotice.Name = "btnNotice";
            this.btnNotice.Size = new System.Drawing.Size(108, 31);
            this.btnNotice.TabIndex = 6;
            this.btnNotice.Text = "Send Notice";
            this.btnNotice.UseVisualStyleBackColor = true;
            this.btnNotice.Click += new System.EventHandler(this.btnNotice_Click);
            // 
            // btnOpenHash
            // 
            this.btnOpenHash.Location = new System.Drawing.Point(221, 578);
            this.btnOpenHash.Name = "btnOpenHash";
            this.btnOpenHash.Size = new System.Drawing.Size(101, 31);
            this.btnOpenHash.TabIndex = 7;
            this.btnOpenHash.Text = "Open Hash";
            this.btnOpenHash.UseVisualStyleBackColor = true;
            this.btnOpenHash.Click += new System.EventHandler(this.btnOpenHash_Click);
            // 
            // btnOpenDir
            // 
            this.btnOpenDir.Location = new System.Drawing.Point(129, 578);
            this.btnOpenDir.Name = "btnOpenDir";
            this.btnOpenDir.Size = new System.Drawing.Size(86, 31);
            this.btnOpenDir.TabIndex = 8;
            this.btnOpenDir.Text = "Open Dir";
            this.btnOpenDir.UseVisualStyleBackColor = true;
            this.btnOpenDir.Click += new System.EventHandler(this.btnOpenDir_Click);
            // 
            // btnShowUserNum
            // 
            this.btnShowUserNum.Location = new System.Drawing.Point(10, 578);
            this.btnShowUserNum.Name = "btnShowUserNum";
            this.btnShowUserNum.Size = new System.Drawing.Size(113, 31);
            this.btnShowUserNum.TabIndex = 9;
            this.btnShowUserNum.Text = "Show User Num";
            this.btnShowUserNum.UseVisualStyleBackColor = true;
            this.btnShowUserNum.Click += new System.EventHandler(this.btnShowUserNum_Click);
            // 
            // btnReloadtblServerSettingInfo
            // 
            this.btnReloadtblServerSettingInfo.Location = new System.Drawing.Point(10, 542);
            this.btnReloadtblServerSettingInfo.Name = "btnReloadtblServerSettingInfo";
            this.btnReloadtblServerSettingInfo.Size = new System.Drawing.Size(205, 30);
            this.btnReloadtblServerSettingInfo.TabIndex = 11;
            this.btnReloadtblServerSettingInfo.Text = "Reload tblServerSettingInfo";
            this.btnReloadtblServerSettingInfo.UseVisualStyleBackColor = true;
            this.btnReloadtblServerSettingInfo.Click += new System.EventHandler(this.btnReloadtblServerSettingInfo_Click);
            // 
            // btnCapsuleMachineManager
            // 
            this.btnCapsuleMachineManager.Location = new System.Drawing.Point(221, 542);
            this.btnCapsuleMachineManager.Name = "btnCapsuleMachineManager";
            this.btnCapsuleMachineManager.Size = new System.Drawing.Size(187, 30);
            this.btnCapsuleMachineManager.TabIndex = 12;
            this.btnCapsuleMachineManager.Text = "CapsuleMachine Manager";
            this.btnCapsuleMachineManager.UseVisualStyleBackColor = true;
            this.btnCapsuleMachineManager.Click += new System.EventHandler(this.btnCapsuleMachineManager_Click);
            // 
            // btnReloadMap
            // 
            this.btnReloadMap.Location = new System.Drawing.Point(328, 578);
            this.btnReloadMap.Name = "btnReloadMap";
            this.btnReloadMap.Size = new System.Drawing.Size(119, 31);
            this.btnReloadMap.TabIndex = 14;
            this.btnReloadMap.Text = "Reload MapInfo";
            this.btnReloadMap.UseVisualStyleBackColor = true;
            this.btnReloadMap.Click += new System.EventHandler(this.btnReloadMap_Click);
            // 
            // btnGMTool
            // 
            this.btnGMTool.Location = new System.Drawing.Point(414, 542);
            this.btnGMTool.Name = "btnGMTool";
            this.btnGMTool.Size = new System.Drawing.Size(96, 30);
            this.btnGMTool.TabIndex = 15;
            this.btnGMTool.Text = "GM Tool";
            this.btnGMTool.UseVisualStyleBackColor = true;
            this.btnGMTool.Click += new System.EventHandler(this.btnGMTool_Click);
            // 
            // btnReloadGameReward
            // 
            this.btnReloadGameReward.Location = new System.Drawing.Point(10, 615);
            this.btnReloadGameReward.Name = "btnReloadGameReward";
            this.btnReloadGameReward.Size = new System.Drawing.Size(205, 31);
            this.btnReloadGameReward.TabIndex = 16;
            this.btnReloadGameReward.Text = "Reload GameRewardGroupInfo";
            this.btnReloadGameReward.UseVisualStyleBackColor = true;
            this.btnReloadGameReward.Click += new System.EventHandler(this.btnReloadGameReward_Click);
            // 
            // btnReloadHash
            // 
            this.btnReloadHash.Location = new System.Drawing.Point(453, 578);
            this.btnReloadHash.Name = "btnReloadHash";
            this.btnReloadHash.Size = new System.Drawing.Size(110, 31);
            this.btnReloadHash.TabIndex = 17;
            this.btnReloadHash.Text = "Reload Hash";
            this.btnReloadHash.UseVisualStyleBackColor = true;
            this.btnReloadHash.Click += new System.EventHandler(this.btnReloadHash_Click);
            // 
            // btnReloadHackingToolHash
            // 
            this.btnReloadHackingToolHash.Location = new System.Drawing.Point(221, 615);
            this.btnReloadHackingToolHash.Name = "btnReloadHackingToolHash";
            this.btnReloadHackingToolHash.Size = new System.Drawing.Size(198, 31);
            this.btnReloadHackingToolHash.TabIndex = 18;
            this.btnReloadHackingToolHash.Text = "Reload tblHackingToolHash";
            this.btnReloadHackingToolHash.UseVisualStyleBackColor = true;
            this.btnReloadHackingToolHash.Click += new System.EventHandler(this.btnReloadHackingToolHash_Click);
            // 
            // btnReloadRenewalShop
            // 
            this.btnReloadRenewalShop.Location = new System.Drawing.Point(425, 615);
            this.btnReloadRenewalShop.Name = "btnReloadRenewalShop";
            this.btnReloadRenewalShop.Size = new System.Drawing.Size(138, 31);
            this.btnReloadRenewalShop.TabIndex = 20;
            this.btnReloadRenewalShop.Text = "Reload RenewalShop";
            this.btnReloadRenewalShop.UseVisualStyleBackColor = true;
            this.btnReloadRenewalShop.Click += new System.EventHandler(this.btnReloadRenewalShop_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this.label5.Location = new System.Drawing.Point(85, 559);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 21);
            this.label5.TabIndex = 24;
            // 
            // btnReloadFishing
            // 
            this.btnReloadFishing.Location = new System.Drawing.Point(10, 652);
            this.btnReloadFishing.Name = "btnReloadFishing";
            this.btnReloadFishing.Size = new System.Drawing.Size(124, 31);
            this.btnReloadFishing.TabIndex = 25;
            this.btnReloadFishing.Text = "Reload FishingInfo";
            this.btnReloadFishing.UseVisualStyleBackColor = true;
            this.btnReloadFishing.Click += new System.EventHandler(this.btnReloadFishing_Click);
            // 
            // btnReloadMonthlyGacha
            // 
            this.btnReloadMonthlyGacha.Enabled = false;
            this.btnReloadMonthlyGacha.Location = new System.Drawing.Point(140, 652);
            this.btnReloadMonthlyGacha.Name = "btnReloadMonthlyGacha";
            this.btnReloadMonthlyGacha.Size = new System.Drawing.Size(182, 31);
            this.btnReloadMonthlyGacha.TabIndex = 26;
            this.btnReloadMonthlyGacha.Text = "Reload Monthly Gacha";
            this.btnReloadMonthlyGacha.UseVisualStyleBackColor = true;
            this.btnReloadMonthlyGacha.Click += new System.EventHandler(this.btnReloadMonthlyGacha_Click);
            // 
            // btnReloadHuMongPickBoard
            // 
            this.btnReloadHuMongPickBoard.Location = new System.Drawing.Point(328, 652);
            this.btnReloadHuMongPickBoard.Name = "btnReloadHuMongPickBoard";
            this.btnReloadHuMongPickBoard.Size = new System.Drawing.Size(169, 31);
            this.btnReloadHuMongPickBoard.TabIndex = 27;
            this.btnReloadHuMongPickBoard.Text = "Reload HuMongPickBoard";
            this.btnReloadHuMongPickBoard.UseVisualStyleBackColor = true;
            this.btnReloadHuMongPickBoard.Click += new System.EventHandler(this.btnReloadHuMongPickBoard_Click);
            // 
            // chkUseLuckyBag
            // 
            this.chkUseLuckyBag.AutoSize = true;
            this.chkUseLuckyBag.Checked = true;
            this.chkUseLuckyBag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseLuckyBag.Location = new System.Drawing.Point(601, 604);
            this.chkUseLuckyBag.Name = "chkUseLuckyBag";
            this.chkUseLuckyBag.Size = new System.Drawing.Size(131, 19);
            this.chkUseLuckyBag.TabIndex = 28;
            this.chkUseLuckyBag.Text = "EnableUseLuckyBag";
            this.chkUseLuckyBag.UseVisualStyleBackColor = true;
            this.chkUseLuckyBag.CheckedChanged += new System.EventHandler(this.chkUseLuckyBag_CheckedChanged);
            // 
            // chkOpenShop
            // 
            this.chkOpenShop.AutoSize = true;
            this.chkOpenShop.Location = new System.Drawing.Point(601, 554);
            this.chkOpenShop.Name = "chkOpenShop";
            this.chkOpenShop.Size = new System.Drawing.Size(127, 19);
            this.chkOpenShop.TabIndex = 29;
            this.chkOpenShop.Text = "CanShopOperation";
            this.chkOpenShop.UseVisualStyleBackColor = true;
            this.chkOpenShop.CheckedChanged += new System.EventHandler(this.chkOpenShop_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 15);
            this.label6.TabIndex = 30;
            this.label6.Text = "ServerID:";
            // 
            // labelServerIdValue
            // 
            this.labelServerIdValue.AutoSize = true;
            this.labelServerIdValue.Location = new System.Drawing.Point(72, 42);
            this.labelServerIdValue.Name = "labelServerIdValue";
            this.labelServerIdValue.Size = new System.Drawing.Size(13, 15);
            this.labelServerIdValue.TabIndex = 31;
            this.labelServerIdValue.Text = "0";
            // 
            // labelServerBuildCaption
            // 
            this.labelServerBuildCaption.AutoSize = true;
            this.labelServerBuildCaption.Location = new System.Drawing.Point(320, 42);
            this.labelServerBuildCaption.Name = "labelServerBuildCaption";
            this.labelServerBuildCaption.Size = new System.Drawing.Size(69, 15);
            this.labelServerBuildCaption.TabIndex = 32;
            this.labelServerBuildCaption.Text = "ServerBuild:";
            // 
            // labelServerBuild
            // 
            this.labelServerBuild.AutoSize = true;
            this.labelServerBuild.Location = new System.Drawing.Point(394, 42);
            this.labelServerBuild.Name = "labelServerBuild";
            this.labelServerBuild.Size = new System.Drawing.Size(19, 15);
            this.labelServerBuild.TabIndex = 34;
            this.labelServerBuild.Text = "—";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(158)))));
            this.label4.Location = new System.Drawing.Point(503, 766);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 15);
            this.label4.TabIndex = 33;
            this.label4.Visible = false;
            // 
            // btnServerSettingManager
            // 
            this.btnServerSettingManager.Location = new System.Drawing.Point(10, 689);
            this.btnServerSettingManager.Name = "btnServerSettingManager";
            this.btnServerSettingManager.Size = new System.Drawing.Size(163, 31);
            this.btnServerSettingManager.TabIndex = 38;
            this.btnServerSettingManager.Text = "ServerSetting Manager";
            this.btnServerSettingManager.UseVisualStyleBackColor = true;
            this.btnServerSettingManager.Click += new System.EventHandler(this.btnServerSettingManager_Click);
            // 
            // btnUpdatCollectioneRanking
            // 
            this.btnUpdatCollectioneRanking.Enabled = false;
            this.btnUpdatCollectioneRanking.Location = new System.Drawing.Point(316, 689);
            this.btnUpdatCollectioneRanking.Name = "btnUpdatCollectioneRanking";
            this.btnUpdatCollectioneRanking.Size = new System.Drawing.Size(180, 31);
            this.btnUpdatCollectioneRanking.TabIndex = 41;
            this.btnUpdatCollectioneRanking.Text = "Updat CollectioneRanking";
            this.btnUpdatCollectioneRanking.UseVisualStyleBackColor = true;
            this.btnUpdatCollectioneRanking.Click += new System.EventHandler(this.btnUpdatCollectioneRanking_Click);
            // 
            // btnUpdateRanking
            // 
            this.btnUpdateRanking.Location = new System.Drawing.Point(179, 689);
            this.btnUpdateRanking.Name = "btnUpdateRanking";
            this.btnUpdateRanking.Size = new System.Drawing.Size(131, 31);
            this.btnUpdateRanking.TabIndex = 40;
            this.btnUpdateRanking.Text = "Update Ranking";
            this.btnUpdateRanking.UseVisualStyleBackColor = true;
            this.btnUpdateRanking.Click += new System.EventHandler(this.btnUpdateRanking_Click);
            // 
            // btnReloadSingleChallenge
            // 
            this.btnReloadSingleChallenge.Location = new System.Drawing.Point(502, 689);
            this.btnReloadSingleChallenge.Name = "btnReloadSingleChallenge";
            this.btnReloadSingleChallenge.Size = new System.Drawing.Size(158, 31);
            this.btnReloadSingleChallenge.TabIndex = 39;
            this.btnReloadSingleChallenge.Text = "Reload SingleChallenge";
            this.btnReloadSingleChallenge.UseVisualStyleBackColor = true;
            this.btnReloadSingleChallenge.Click += new System.EventHandler(this.btnReloadSingleChallenge_Click);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(12, 10);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(314, 20);
            this.titleLabel.TabIndex = 50;
            this.titleLabel.Text = "TR SERVER : AgentServer · R186632";
            this.titleLabel.Click += new System.EventHandler(this.titleLabel_Click);
            // 
            // btnClose
            // 
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Location = new System.Drawing.Point(715, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(28, 28);
            this.btnClose.TabIndex = 51;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "×";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(755, 761);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.btnUpdatCollectioneRanking);
            this.Controls.Add(this.btnUpdateRanking);
            this.Controls.Add(this.btnReloadSingleChallenge);
            this.Controls.Add(this.btnServerSettingManager);
            this.Controls.Add(this.chkOpenShop);
            this.Controls.Add(this.btnReloadHuMongPickBoard);
            this.Controls.Add(this.chkUseLuckyBag);
            this.Controls.Add(this.chkServerReady);
            this.Controls.Add(this.btnReloadGameReward);
            this.Controls.Add(this.btnReloadMonthlyGacha);
            this.Controls.Add(this.btnReloadHackingToolHash);
            this.Controls.Add(this.btnReloadFishing);
            this.Controls.Add(this.btnReloadRenewalShop);
            this.Controls.Add(this.btnReloadHash);
            this.Controls.Add(this.btnShowUserNum);
            this.Controls.Add(this.btnReloadMap);
            this.Controls.Add(this.btnGMTool);
            this.Controls.Add(this.btnOpenHash);
            this.Controls.Add(this.btnOpenDir);
            this.Controls.Add(this.btnCapsuleMachineManager);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelServerBuild);
            this.Controls.Add(this.labelServerBuildCaption);
            this.Controls.Add(this.labelServerIdValue);
            this.Controls.Add(this.btnReloadtblServerSettingInfo);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnNotice);
            this.Controls.Add(this.txtNotice);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(236)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TR SERVER : AgentServer · R186632";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private void btnServerSettingManager_Click(object sender, EventArgs e)
        {
			if (ssm == null || ssm.IsDisposed)
			{
				ssm = new ServerSettingManager();
			}
			ssm.Show();
			ssm.Focus();
		}

        private void btnUpdateRanking_Click(object sender, EventArgs e)
        {
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_makerank";
				mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int32).Value = 0;
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();
				mySqlConnection.Close();
			}
			Log.Information("Update Ranking !");
		}

        private void btnReloadSingleChallenge_Click(object sender, EventArgs e)
        {
			SingleChallengeHolder.LoadChallengeMapInfo();
			Log.Information("Reloaded ChallengeMapInfo !!");
		}

        private void btnUpdatCollectioneRanking_Click(object sender, EventArgs e)
        {
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_itemCollection_makeRank";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();
				mySqlConnection.Close();
			}
			Log.Information("Update Collectione Ranking !");
		}

        private void titleLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
