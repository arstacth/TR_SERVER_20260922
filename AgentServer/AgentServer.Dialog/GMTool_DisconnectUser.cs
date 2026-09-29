using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AgentServer.Network.Connections;
using AgentServer.Structuring;

namespace AgentServer.Dialog
{
	public class GMTool_DisconnectUser : Form
	{
		private IContainer components;

		private ListBox listBox1;

		private Button button1;

		private Button button2;

		public GMTool_DisconnectUser()
		{
			InitializeComponent();
			DarkUi.Apply(this, primaryButtons: true);
		}

		private void GMTool_DisconnectUser_Load(object sender, EventArgs e)
		{
			listBox1.DataSource = ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin).ToList();
			listBox1.DisplayMember = "NickName";
			listBox1.ValueMember = "Session";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (listBox1.SelectedIndex == -1)
			{
				DarkUi.Alert(this, "Please select a player.", "OK");
				return;
			}
			int key = (int)listBox1.SelectedValue;
			if (ClientConnection.CurrentAccounts.ContainsKey(key))
			{
				ClientConnection.CurrentAccounts[key].Connection.Disconnect();
				DarkUi.Alert(this, "The player's connection has been disconnected.", "OK");
			}
			refresh();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			refresh();
		}

		private void refresh()
		{
			listBox1.DataSource = null;
			listBox1.DataSource = ClientConnection.CurrentAccounts.Values.Where((Account w) => w.isLogin).ToList();
			listBox1.DisplayMember = "NickName";
			listBox1.ValueMember = "Session";
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Font = new System.Drawing.Font("Microsoft JhengHei", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 19;
            this.listBox1.Location = new System.Drawing.Point(12, 21);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(224, 213);
            this.listBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(264, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(151, 46);
            this.button1.TabIndex = 1;
            this.button1.Text = "Disconnect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(264, 188);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(151, 46);
            this.button2.TabIndex = 2;
            this.button2.Text = "Refresh";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // GMTool_DisconnectUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 255);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "GMTool_DisconnectUser";
            this.Text = "GMTool_DisconnectUser";
            this.Load += new System.EventHandler(this.GMTool_DisconnectUser_Load);
            this.ResumeLayout(false);

		}
	}
}
