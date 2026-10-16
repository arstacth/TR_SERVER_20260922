using AgentServer.Holders;
using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AgentServer
{
    public partial class ServerSettingManager : Form
    {
        public ServerSettingManager()
        {
            InitializeComponent();
            DarkUi.Apply(this, primaryButtons: true);
        }

        private void ServerSettingManager_Load(object sender, EventArgs e)
        {
			LoadServerSettingInfo();
			DarkUi.StyleGrid(tblServerSetting);
		}
		private void LoadServerSettingInfo()
		{
		

			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.CommandText = "SELECT * FROM tblserversettinginfo";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				mySqlCommand.ExecuteNonQuery();
				DataTable dataTable = new DataTable();
				mySqlDataAdapter.Fill(dataTable);
				tblServerSetting.DataSource = dataTable;
				mySqlCommand.Dispose();
				mySqlConnection.Close();
			}
		}

        private void tblServerSetting_SelectionChanged(object sender, EventArgs e)
        {
			foreach (DataGridViewRow selectedRow in tblServerSetting.SelectedRows)
			{
				lblNum.Text = selectedRow.Cells[0].Value.ToString();
				txtKey.Text = selectedRow.Cells[1].Value.ToString();
				txtValue.Text = selectedRow.Cells[2].Value.ToString();
                if (selectedRow.Cells[3].Value.ToString() == "True")
                {
					checkBox1.Checked = true;
				}
				else if (selectedRow.Cells[3].Value.ToString() == "False")
                {
					checkBox1.Checked = false;
				}

					txtDesc.Text = selectedRow.Cells[4].Value.ToString();
			}
		}

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
			(tblServerSetting.DataSource as DataTable).DefaultView.RowFilter =
			string.Format("fdKey LIKE '%{0}%'", textBox1.Text);
		}

        private void btnEdit_Click(object sender, EventArgs e)
        {
			if (EditSetting(Convert.ToString(txtKey.Text), Convert.ToString(txtValue.Text), Convert.ToInt32(checkBox1.Checked), Convert.ToString(txtDesc.Text)) == 0)
			{
				LoadServerSettingInfo();
				textBox1.Clear();
				DarkUi.Alert(this, "Success", "TRServer");
				
			}
		}
		private byte EditSetting(string txtKey , string txtValue, int txtOnly, string txtDesc)
		{
			
			byte result = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.CommandText = "UPDATE tblserversettinginfo SET fdValue = '" + txtValue + "',fdOnlyServerSetting = '" + txtOnly + "',fdDesc = '" + txtDesc + "' WHERE fdKey = '" + txtKey + "'";
				MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
				MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				mySqlDataReader.Read();
				mySqlCommand.Dispose();
				mySqlDataReader.Close();
				mySqlConnection.Close();
			}
			return result;
		}

        private void button1_Click(object sender, EventArgs e)
        {
			
		}

        private void txtOnly_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
