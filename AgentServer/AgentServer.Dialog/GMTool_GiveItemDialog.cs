using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AgentServer.Dialog
{
	public class GMTool_GiveItemDialog : Form
	{
		private IContainer components;

		private Label label1;

		private Button button1;

		private TextBox textBox1;

		private TextBox textBox2;

		private Label label2;

		private TextBox textBox3;

		private Label label3;

		public GMTool_GiveItemDialog()
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
			mySqlCommand.CommandText = "usp_giveItemDescByNickname";
			mySqlCommand.Parameters.Add("itemdesc", MySqlDbType.Int32).Value = textBox2.Text;
			mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = textBox1.Text;
			mySqlCommand.Parameters.Add("pGiveCount", MySqlDbType.Int32).Value = Convert.ToInt32(textBox3.Text);
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			if (Convert.ToInt32(mySqlDataReader["nRet"]) == 0)
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
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(19, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(71, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "NickName:";
			this.button1.Location = new System.Drawing.Point(67, 138);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(115, 34);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(button1_Click);
			this.textBox1.Location = new System.Drawing.Point(96, 14);
			this.textBox1.MaxLength = 30;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(126, 23);
			this.textBox1.TabIndex = 2;
			this.textBox2.Location = new System.Drawing.Point(96, 53);
			this.textBox2.MaxLength = 6;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(126, 23);
			this.textBox2.TabIndex = 4;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(26, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "ItemNum:";
			this.textBox3.Location = new System.Drawing.Point(96, 93);
			this.textBox3.MaxLength = 1;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(126, 23);
			this.textBox3.TabIndex = 6;
			this.textBox3.Text = "1";
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(17, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(73, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Give Count:";
			base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(247, 189);
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
			base.Name = "GMTool_GiveItemDialog";
			this.Text = "Give Item";
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
