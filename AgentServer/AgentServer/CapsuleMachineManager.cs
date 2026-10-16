using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AgentServer.Structuring;
using Akka.Actor;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using TRCommon;

namespace AgentServer
{
	public class CapsuleMachineManager : Form
	{
		private IContainer components;

		private ComboBox CapsuleMachineListBox;

		private DataGridView CapsuleMachineData;

		private GroupBox groupBox1;

		private TextBox textBox2;

		private TextBox textBox1;

		private Label label4;

		private Label label3;

		private Label label2;

		private Label label1;

		private Button btnEditApply;

		private Button btnDelete;

		private GroupBox groupBox2;

		private TextBox textBox5;

		private TextBox textBox4;

		private TextBox textBox3;

		private Label label7;

		private Label label6;

		private Label label5;

		private Button btnAddItem;

		private Button btnReload;

		private GroupBox groupBox3;

		private Button btnClose;

		public CapsuleMachineManager()
		{
			InitializeComponent();
			DarkUi.Apply(this, primaryButtons: true);
		}

		private void CapsuleMachineManager_Load(object sender, EventArgs e)
		{
			LoadUsingMachineList();
		}

		private void LoadUsingMachineList()
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM essencapsulemachineusinglist", mySqlConnection);
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				CapsuleMachineListBox.Items.Add(mySqlDataReader["fdMachineNum"]);
			}
			mySqlCommand.Dispose();
			mySqlDataReader.Close();
			mySqlConnection.Close();
		}

		public void LoadMachineData(int MachineItemNum)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_capsuleMachineGetMachineInfoForTool";
				mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineItemNum;
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				mySqlCommand.ExecuteNonQuery();
				DataTable dataTable = new DataTable();
				mySqlDataAdapter.Fill(dataTable);
				CapsuleMachineData.DataSource = dataTable;
				mySqlCommand.Dispose();
				mySqlConnection.Close();
			}
			CapsuleMachineData.Columns[0].Visible = false;
		}

		private byte EditItem(int MachineNum, int ItemNum, int ItemAmount, int ItemMax)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_capsuleMachineEditItem";
			mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
			mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
			mySqlCommand.Parameters.Add("itemAmount", MySqlDbType.Int32).Value = ItemAmount;
			mySqlCommand.Parameters.Add("itemMax", MySqlDbType.Int32).Value = ItemMax;
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			mySqlDataReader.Read();
			b = Convert.ToByte(mySqlDataReader["ret"]);
			mySqlCommand.Dispose();
			mySqlDataReader.Close();
			mySqlConnection.Close();
			return b;
		}

		private byte DeleteItem(int MachineNum, int ItemNum)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_capsuleMachineDeleteItem";
			mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
			mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			mySqlDataReader.Read();
			b = Convert.ToByte(mySqlDataReader["ret"]);
			mySqlCommand.Dispose();
			mySqlDataReader.Close();
			mySqlConnection.Close();
			return b;
		}

		private byte AddItem(int MachineNum, int Level, int ItemNum, int ItemMax)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_capsuleMachineInsertItem";
			mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
			mySqlCommand.Parameters.Add("level", MySqlDbType.Int32).Value = Level;
			mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
			mySqlCommand.Parameters.Add("itemMax", MySqlDbType.Int32).Value = ItemMax;
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			mySqlDataReader.Read();
			b = Convert.ToByte(mySqlDataReader["ret"]);
			mySqlCommand.Dispose();
			mySqlDataReader.Close();
			mySqlConnection.Close();
			return b;
		}

		private byte ResetMachine(int MachineNum)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_capsuleMachineReset";
			mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
			mySqlCommand.Parameters.Add("returnRate", MySqlDbType.Int32).Value = 0;
			MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			mySqlDataReader.Read();
			b = Convert.ToByte(mySqlDataReader["retval"]);
			mySqlCommand.Dispose();
			mySqlDataReader.Close();
			mySqlConnection.Close();
			return b;
		}

		private void CapsuleMachineListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			LoadMachineData(Convert.ToInt32(comboBox.Text));
		}

		private void CapsuleMachineData_SelectionChanged(object sender, EventArgs e)
		{
			foreach (DataGridViewRow selectedRow in CapsuleMachineData.SelectedRows)
			{
				label4.Text = selectedRow.Cells[2].Value.ToString();
				textBox1.Text = selectedRow.Cells[3].Value.ToString();
				textBox2.Text = selectedRow.Cells[4].Value.ToString();
			}
		}

		private void btnEditApply_Click(object sender, EventArgs e)
		{
			if (CapsuleMachineListBox.Text == string.Empty || textBox1.Text == string.Empty || textBox2.Text == string.Empty)
			{
				DarkUi.Alert(this, "Cannot be empty", "Notice");
			}
			else if (EditItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(label4.Text), Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox2.Text)) == 0)
			{
				LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
				DarkUi.Alert(this, "Updated successfully", "Notice");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (CapsuleMachineListBox.Text == string.Empty || textBox1.Text == string.Empty || textBox2.Text == string.Empty)
			{
				DarkUi.Alert(this, "Cannot be empty", "Notice");
			}
			else if (DeleteItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(label4.Text)) == 0)
			{
				LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
				DarkUi.Alert(this, "Item deleted successfully", "Notice");
			}
		}

		private void btnAddItem_Click(object sender, EventArgs e)
		{
			if (CapsuleMachineData.Rows.Count < 10)
			{
				if (CapsuleMachineListBox.Text == string.Empty || textBox3.Text == string.Empty || textBox4.Text == string.Empty || textBox5.Text == string.Empty)
				{
					DarkUi.Alert(this, "Cannot be empty", "Notice");
					return;
				}
				int num = Convert.ToInt32(textBox3.Text);
				if (ShopItemTable.getItemDataFromItemDescNum(num, out var itemData))
				{
					if (itemData.m_iType == 0 || itemData.m_iType == 2)
					{
						DarkUi.Alert(this, "Invalid Item Type!", "Notice", danger: true);
						return;
					}
					switch (AddItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(textBox4.Text), num, Convert.ToInt32(textBox5.Text)))
					{
					case 0:
						LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
						DarkUi.Alert(this, "Item added successfully", "Notice");
						break;
					case 1:
						DarkUi.Alert(this, "This capsule machine already has this item", "Notice");
						break;
					case 2:
						DarkUi.Alert(this, "Level must be 1-5", "Notice");
						break;
					case 3:
						DarkUi.Alert(this, "Capsule machine or item not found", "Notice");
						break;
					}
				}
				else
				{
					DarkUi.Alert(this, "Item not found", "Notice", danger: true);
				}
			}
			else
			{
				DarkUi.Alert(this, "This capsule machine already has 10 items", "Notice");
			}
		}

		private void btnReload_Click(object sender, EventArgs e)
		{
			if (CapsuleMachineListBox.Text == string.Empty)
			{
				DarkUi.Alert(this, "Please select a capsule machine first", "Notice");
				return;
			}
			LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
			ServerStatus.LBServerActor.Tell(new ReloadSetting
			{
				Code = 4
			});
			DarkUi.Alert(this, "Reload OK", "Notice");
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
			//System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentServer.CapsuleMachineManager));
			this.CapsuleMachineListBox = new System.Windows.Forms.ComboBox();
			this.CapsuleMachineData = new System.Windows.Forms.DataGridView();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnDelete = new System.Windows.Forms.Button();
			this.btnEditApply = new System.Windows.Forms.Button();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnAddItem = new System.Windows.Forms.Button();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.btnReload = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.btnClose = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)this.CapsuleMachineData).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			base.SuspendLayout();
			this.CapsuleMachineListBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CapsuleMachineListBox.FormattingEnabled = true;
			this.CapsuleMachineListBox.Location = new System.Drawing.Point(13, 23);
			this.CapsuleMachineListBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.CapsuleMachineListBox.Name = "CapsuleMachineListBox";
			this.CapsuleMachineListBox.Size = new System.Drawing.Size(209, 24);
			this.CapsuleMachineListBox.TabIndex = 0;
			this.CapsuleMachineListBox.SelectedIndexChanged += new System.EventHandler(CapsuleMachineListBox_SelectedIndexChanged);
			this.CapsuleMachineData.AllowUserToAddRows = false;
			this.CapsuleMachineData.AllowUserToDeleteRows = false;
			this.CapsuleMachineData.AllowUserToResizeColumns = false;
			this.CapsuleMachineData.AllowUserToResizeRows = false;
			this.CapsuleMachineData.BackgroundColor = System.Drawing.SystemColors.Control;
			this.CapsuleMachineData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CapsuleMachineData.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft JhengHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.CapsuleMachineData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
			this.CapsuleMachineData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.CapsuleMachineData.Location = new System.Drawing.Point(13, 54);
			this.CapsuleMachineData.MultiSelect = false;
			this.CapsuleMachineData.Name = "CapsuleMachineData";
			this.CapsuleMachineData.ReadOnly = true;
			this.CapsuleMachineData.RowHeadersVisible = false;
			this.CapsuleMachineData.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.CapsuleMachineData.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			this.CapsuleMachineData.RowTemplate.Height = 24;
			this.CapsuleMachineData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.CapsuleMachineData.Size = new System.Drawing.Size(403, 265);
			this.CapsuleMachineData.TabIndex = 1;
			this.CapsuleMachineData.SelectionChanged += new System.EventHandler(CapsuleMachineData_SelectionChanged);
			this.groupBox1.Controls.Add(this.btnDelete);
			this.groupBox1.Controls.Add(this.btnEditApply);
			this.groupBox1.Controls.Add(this.textBox2);
			this.groupBox1.Controls.Add(this.textBox1);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(448, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(180, 168);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit Item";
			this.btnDelete.Location = new System.Drawing.Point(14, 123);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(72, 27);
			this.btnDelete.TabIndex = 7;
			this.btnDelete.Text = "Delete";
			this.btnDelete.UseVisualStyleBackColor = true;
			this.btnDelete.Click += new System.EventHandler(btnDelete_Click);
			this.btnEditApply.Location = new System.Drawing.Point(96, 123);
			this.btnEditApply.Name = "btnEditApply";
			this.btnEditApply.Size = new System.Drawing.Size(72, 27);
			this.btnEditApply.TabIndex = 6;
			this.btnEditApply.Text = "Apply";
			this.btnEditApply.UseVisualStyleBackColor = true;
			this.btnEditApply.Click += new System.EventHandler(btnEditApply_Click);
			this.textBox2.Location = new System.Drawing.Point(86, 88);
			this.textBox2.MaxLength = 10;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(82, 23);
			this.textBox2.TabIndex = 5;
			this.textBox1.Location = new System.Drawing.Point(86, 55);
			this.textBox1.MaxLength = 10;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(82, 23);
			this.textBox1.TabIndex = 4;
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(83, 32);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(0, 16);
			this.label4.TabIndex = 3;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(39, 91);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(36, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "Max:";
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(19, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Amount:";
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "ItemNum:";
			this.groupBox2.Controls.Add(this.btnAddItem);
			this.groupBox2.Controls.Add(this.textBox5);
			this.groupBox2.Controls.Add(this.textBox4);
			this.groupBox2.Controls.Add(this.textBox3);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Location = new System.Drawing.Point(448, 186);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(180, 151);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Add Item";
			this.btnAddItem.Location = new System.Drawing.Point(45, 114);
			this.btnAddItem.Name = "btnAddItem";
			this.btnAddItem.Size = new System.Drawing.Size(84, 27);
			this.btnAddItem.TabIndex = 8;
			this.btnAddItem.Text = "Add Item";
			this.btnAddItem.UseVisualStyleBackColor = true;
			this.btnAddItem.Click += new System.EventHandler(btnAddItem_Click);
			this.textBox5.Location = new System.Drawing.Point(86, 84);
			this.textBox5.MaxLength = 10;
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(82, 23);
			this.textBox5.TabIndex = 11;
			this.textBox4.Location = new System.Drawing.Point(86, 55);
			this.textBox4.MaxLength = 1;
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(82, 23);
			this.textBox4.TabIndex = 10;
			this.textBox3.Location = new System.Drawing.Point(86, 26);
			this.textBox3.MaxLength = 10;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(82, 23);
			this.textBox3.TabIndex = 8;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(39, 87);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(36, 16);
			this.label7.TabIndex = 8;
			this.label7.Text = "Max:";
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(35, 58);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 16);
			this.label6.TabIndex = 2;
			this.label6.Text = "Level:";
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(11, 29);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 16);
			this.label5.TabIndex = 1;
			this.label5.Text = "ItemNum:";
			this.btnReload.Location = new System.Drawing.Point(282, 325);
			this.btnReload.Name = "btnReload";
			this.btnReload.Size = new System.Drawing.Size(142, 30);
			this.btnReload.TabIndex = 5;
			this.btnReload.Text = "Reload Machine";
			this.btnReload.UseVisualStyleBackColor = true;
			this.btnReload.Click += new System.EventHandler(btnReload_Click);
			this.groupBox3.Controls.Add(this.CapsuleMachineListBox);
			this.groupBox3.Controls.Add(this.btnReload);
			this.groupBox3.Controls.Add(this.CapsuleMachineData);
			this.groupBox3.Location = new System.Drawing.Point(12, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(430, 361);
			this.groupBox3.TabIndex = 6;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "CapsuleMachine Info";
			this.btnClose.Location = new System.Drawing.Point(448, 343);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(180, 30);
			this.btnClose.TabIndex = 7;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(btnClose_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 16f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(647, 388);
			base.Controls.Add(this.btnClose);
			base.Controls.Add(this.groupBox3);
			base.Controls.Add(this.groupBox2);
			base.Controls.Add(this.groupBox1);
			this.Font = new System.Drawing.Font("Microsoft JhengHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			//base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "CapsuleMachineManager";
			this.Text = "CapsuleMachine Manager";
			base.Load += new System.EventHandler(CapsuleMachineManager_Load);
			((System.ComponentModel.ISupportInitialize)this.CapsuleMachineData).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
