using Codeplex.Data;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApiHelper
{
    public partial class ApiHelper : Form
    {
        public ApiHelper()
        {
            InitializeComponent();
        }

        private CheckBox headerCheckBox = new CheckBox();

        private void openJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.FileName = "sample.json";
            ofd.InitialDirectory = @"E:\work";
            ofd.Filter = "Jsonファイル(*.json)|";
            ofd.FilterIndex = 2;
            ofd.Title = "開くファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var stm = ofd.OpenFile();
                if (stm != null)
                {
                    var api = DynamicJson.Parse(stm);
                    this.txtEndPoint.Text = api.EndPoint;
                    this.txtPath.Text = api.Path;
                    this.txtMethod.Text = api.Method;
                    this.txtHeader.Text = api.Header;
                    headerCheckBox.Location = new Point(6, 4);
                    headerCheckBox.BackColor = Color.White;
                    headerCheckBox.Size = new Size(14, 14);

                    headerCheckBox.Click += new EventHandler(headerCheckBox_Clicked);
                    dgv.Controls.Add(headerCheckBox);

                    DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn
                    {
                        HeaderText = "",
                        Width = 30,
                        Name = "checkBoxColumn"
                    };
                    this.dgv.Columns.Add(checkBoxColumn);
                    this.dgv.Columns.Add("Case", "ケース");
                    this.dgv.Columns.Add("Request", "リクエストデータ");
                    foreach (var rc in api.RequestCase)
                    {
                        this.dgv.Rows.Add(false, rc.Case, rc.Request);
                    }
                    foreach (DataGridViewColumn dgvc in this.dgv.Columns)
                    {
                        dgvc.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    this.dgv.Columns[0].Width = 20;
                    this.dgv.Columns[1].Width = 60;
                    var headersCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter,
                        BackColor = Color.Ivory,
                        ForeColor = Color.IndianRed,
                        Font = new Font("Meiryo UI", 9, FontStyle.Bold)
                    };
                    this.dgv.ColumnHeadersDefaultCellStyle = headersCellStyle;
                    this.dgv.CellContentClick += new DataGridViewCellEventHandler(DataGridView_CellClick);
                }
                else
                {
                    MessageBox.Show("Jsonファイルにはデータがありません！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void headerCheckBox_Clicked(object sender, EventArgs e)
        {
            this.dgv.EndEdit();

            foreach (DataGridViewRow row in this.dgv.Rows)
            {
                DataGridViewCheckBoxCell checkBox = (row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell);
                checkBox.Value = headerCheckBox.Checked;
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                bool isChecked = true;
                foreach (DataGridViewRow row in this.dgv.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["checkBoxColumn"].EditedFormattedValue) == false)
                    {
                        isChecked = false;
                        break;
                    }
                }
                headerCheckBox.Checked = isChecked;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void viewHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void versionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Author:Shiming Xu\r\nDate:2020.4.6\r\nVersion:1.0", "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_Click(object sender, EventArgs e)
        {
            var cnt = 0;
            if (this.dgv.Rows.Count > 0)
            {
                this.tab.TabPages.Clear();
                foreach (DataGridViewRow row in this.dgv.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["checkBoxColumn"].EditedFormattedValue) == false)
                    {
                        cnt++;
                    }
                    else
                    {
                        var client = new RestClient(this.txtEndPoint.Text + "/" + this.txtPath.Text);

                        RestRequest request = new RestRequest();
                        if (Method.GET.ToString().Equals(this.txtMethod.Text))
                        {
                            request = new RestRequest(row.Cells["Request"].Value.ToString(), Method.GET, DataFormat.Json);
                        }
                        else if (Method.POST.ToString().Equals(this.txtMethod.Text))
                        {
                            request = new RestRequest(row.Cells["Request"].Value.ToString(), Method.POST, DataFormat.Json);
                        }
                        else if (Method.PUT.ToString().Equals(this.txtMethod.Text))
                        {
                            request = new RestRequest(row.Cells["Request"].Value.ToString(), Method.PUT, DataFormat.Json);
                        }
                        if (Method.DELETE.ToString().Equals(this.txtMethod.Text))
                        {
                            request = new RestRequest(row.Cells["Request"].Value.ToString(), Method.DELETE, DataFormat.Json);
                        }
                        var c = row.Cells["Case"].Value.ToString();
                        var response = client.Execute(request);
                    }
                }
                if (cnt == this.dgv.Rows.Count)
                {
                    MessageBox.Show("実行対象が選択されていません！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("実行対象が選択されていません！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (this.dgv.DataSource != null)
            {
                if (((DataTable)this.dgv.DataSource).AsEnumerable().Where(x => x["checkBoxColumn"].Equals(true)).Count() > 0)
                {
                    ((DataTable)this.dgv.DataSource).AsEnumerable().Where(x => x["checkBoxColumn"].Equals(true)).Select(x => x).ToList().ForEach(y =>
                    {
                        var c = y.Field<string>("Case");
                        var request = y.Field<string>("Request");
                    });
                }
            }

            foreach (DataGridViewRow row in this.dgv.Rows)
            {
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].EditedFormattedValue) == false)
                {
                    break;
                }
            }
        }
    }
}