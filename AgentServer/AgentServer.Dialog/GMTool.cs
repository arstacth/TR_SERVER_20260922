using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AgentServer.Dialog
{
	public class GMTool : Form
	{
		private IContainer components;

		private Button btn_GiveItemDialog;

		private Button btn_GiveUserPoint;

		private Button btn_DisconnectUser;

		public GMTool()
		{
			InitializeComponent();
			DarkUi.Apply(this, primaryButtons: true);
		}

		private void btn_GiveItemDialog_Click(object sender, EventArgs e)
		{
			new GMTool_GiveItemDialog().Show();
		}

		private void btn_GiveUserPoint_Click(object sender, EventArgs e)
		{
			new GMTool_GiveUserPoint().Show();
		}

		private void btn_DisconnectUser_Click(object sender, EventArgs e)
		{
			new GMTool_DisconnectUser().Show();
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
			this.btn_GiveItemDialog = new System.Windows.Forms.Button();
			this.btn_GiveUserPoint = new System.Windows.Forms.Button();
			this.btn_DisconnectUser = new System.Windows.Forms.Button();
			base.SuspendLayout();
			this.btn_GiveItemDialog.Font = new System.Drawing.Font("OK", 12.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			this.btn_GiveItemDialog.Location = new System.Drawing.Point(34, 22);
			this.btn_GiveItemDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btn_GiveItemDialog.Name = "btn_GiveItemDialog";
			this.btn_GiveItemDialog.Size = new System.Drawing.Size(134, 48);
			this.btn_GiveItemDialog.TabIndex = 0;
			this.btn_GiveItemDialog.Text = "Give Item";
			this.btn_GiveItemDialog.UseVisualStyleBackColor = true;
			this.btn_GiveItemDialog.Click += new System.EventHandler(btn_GiveItemDialog_Click);
			this.btn_GiveUserPoint.Font = new System.Drawing.Font("OK", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			this.btn_GiveUserPoint.Location = new System.Drawing.Point(34, 78);
			this.btn_GiveUserPoint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btn_GiveUserPoint.Name = "btn_GiveUserPoint";
			this.btn_GiveUserPoint.Size = new System.Drawing.Size(134, 48);
			this.btn_GiveUserPoint.TabIndex = 1;
			this.btn_GiveUserPoint.Text = "Give User Point";
			this.btn_GiveUserPoint.UseVisualStyleBackColor = true;
			this.btn_GiveUserPoint.Click += new System.EventHandler(btn_GiveUserPoint_Click);
			this.btn_DisconnectUser.Font = new System.Drawing.Font("OK", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			this.btn_DisconnectUser.Location = new System.Drawing.Point(184, 22);
			this.btn_DisconnectUser.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.btn_DisconnectUser.Name = "btn_DisconnectUser";
			this.btn_DisconnectUser.Size = new System.Drawing.Size(134, 48);
			this.btn_DisconnectUser.TabIndex = 2;
			this.btn_DisconnectUser.Text = "DisconnectUser";
			this.btn_DisconnectUser.UseVisualStyleBackColor = true;
			this.btn_DisconnectUser.Click += new System.EventHandler(btn_DisconnectUser_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(401, 209);
			base.Controls.Add(this.btn_DisconnectUser);
			base.Controls.Add(this.btn_GiveUserPoint);
			base.Controls.Add(this.btn_GiveItemDialog);
			this.Font = new System.Drawing.Font("OK", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			base.MaximizeBox = false;
			base.Name = "GMTool";
			this.Text = "GMTool";
			base.ResumeLayout(false);
		}
	}
}
