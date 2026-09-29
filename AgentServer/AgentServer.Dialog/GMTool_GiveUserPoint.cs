using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AgentServer.Dialog
{
	public class GMTool_GiveUserPoint : Form
	{
		private IContainer components;

		private TextBox textBox3;

		private Label label3;

		private TextBox textBox2;

		private Label label2;

		private TextBox textBox1;

		private Button button1;

		private Label label1;

		public GMTool_GiveUserPoint()
		{
			InitializeComponent();
			DarkUi.Apply(this, primaryButtons: true);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
			{
				DarkUi.Alert(this, "Cannot be empty", "OK");
				return;
			}
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_GM_giveUserPoint";
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = textBox1.Text;
			mySqlCommand.Parameters.Add("rewardGroup", MySqlDbType.Int32).Value = textBox2.Text;
			mySqlCommand.Parameters.Add("point", MySqlDbType.Int32).Value = textBox3.Text;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
			{
				DarkUi.Alert(this, "Successfully", "OK");
			}
			else
			{
				DarkUi.Alert(this, "Failed", "OK");
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
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.textBox3.Location = new System.Drawing.Point(84, 99);
			this.textBox3.MaxLength = 65536;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(126, 23);
			this.textBox3.TabIndex = 13;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(38, 102);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 16);
			this.label3.TabIndex = 12;
			this.label3.Text = "Point:";
			this.textBox2.Location = new System.Drawing.Point(84, 59);
			this.textBox2.MaxLength = 4;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(126, 23);
			this.textBox2.TabIndex = 11;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 16);
			this.label2.TabIndex = 10;
			this.label2.Text = "Point Type:";
			this.textBox1.Location = new System.Drawing.Point(84, 20);
			this.textBox1.MaxLength = 30;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(126, 23);
			this.textBox1.TabIndex = 9;
			this.button1.Location = new System.Drawing.Point(70, 143);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(115, 34);
			this.button1.TabIndex = 8;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(button1_Click);
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(19, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 16);
			this.label1.TabIndex = 7;
			this.label1.Text = "NickName:";
			base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(246, 194);
			base.Controls.Add(this.textBox3);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.textBox2);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.textBox1);
			base.Controls.Add(this.button1);
			base.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			base.MaximizeBox = false;
			base.Name = "GMTool_GiveUserPoint";
			this.Text = "GiveUserPoint";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
