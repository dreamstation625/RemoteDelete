using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 远程桌面记录删除助手
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = this.Text + " V1.1";
            lab_github.ForeColor = SystemColors.Highlight;
            refresh.Left = (this.Width - refresh.Width -delete.Width - item_sort.Width-20*2)/2-8;
            delete.Left = refresh.Right + 20;
            item_sort.Left = delete.Right + 20;
            mstsc_info();
        }


        //从注册表获取信息
        public void mstsc_info()
        {
            dataGridView1.Rows.Clear();
            try
            {
                string[] info;
                RegistryKey Key;
                Key = Registry.CurrentUser;
                RegistryKey myreg = Key.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default");
                info = myreg.GetValueNames();
                myreg.Close();
                for (int i = 0; i < info.Count(); i++)
                {
                    //获取键值
                    string zhi = "";
                    RegistryKey Keyzhi;
                    Keyzhi = Registry.CurrentUser;
                    RegistryKey kzhi = Key.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default");
                    zhi = kzhi.GetValue(info[i]).ToString();
                    kzhi.Close();
                    DataGridViewRow dgvr = new DataGridViewRow();
                    foreach (DataGridViewColumn c in dataGridView1.Columns)
                    {
                        dgvr.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);
                    }
                    dgvr.Cells[1].Value = info[i];
                    dgvr.Cells[2].Value = zhi;
                    dataGridView1.Rows.Add(dgvr);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("获取信息失败。\r\n错误原因："+ex.ToString());
            }
            
            
        }
        public int delay;
        private void refresh_Click(object sender, EventArgs e)
        {
            refresh.Enabled = false;
            refresh.Text = "刷新成功 3";
            while (this.dataGridView1.Rows.Count != 0)
            {
                this.dataGridView1.Rows.RemoveAt(0);
            }
            mstsc_info();
            //没啥用的刷新后的等待
            delay=3;
            timer1.Enabled=true;
        }
        //全选按钮
        private void select_all_CheckedChanged(object sender, EventArgs e)
        {
            if (selectall.Checked == true)
            {
                //获取列表的行数
                int sum;
                sum = dataGridView1.Rows.Count;
                for (int i = 0; i < sum; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = "true";
                }
            }
            else
            {
                int sum;
                sum = dataGridView1.Rows.Count;
                for (int i = 0; i < sum; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = "false";
                }
            }
        }
        //单选
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (dataGridView1.Columns[e.ColumnIndex].Name.ToString() == "select")
                {
                    if (dataGridView1.Rows[e.RowIndex].Cells[0].Value != "true")
                        dataGridView1.Rows[e.RowIndex].Cells[0].Value = "true";
                    else
                        dataGridView1.Rows[e.RowIndex].Cells[0].Value = "false";
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            delay--;
            if(delay>0)
            {                
                refresh.Text = "刷新成功 " + delay.ToString();
            }
            else
            {
                refresh.Text = "刷新列表";
                refresh.Enabled = true;
                timer1.Enabled = false;
            }
            
        }

        private void delete_Click(object sender, EventArgs e)
        {
            int sum;bool hs=false;
            sum = dataGridView1.Rows.Count;
            for (int i = 0; i < sum; i++)
            {
                //判断是否勾选
                if (dataGridView1.Rows[i].Cells[0].Value == "true")
                {
                    hs = true;
                }
            }
            if (hs == false) { MessageBox.Show("请选择需要删除的记录。");return; }
            //显示警告
            DialogResult result = MessageBox.Show("此删除操作后，不可恢复，请确认是否删除","警告",MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            //再次判断勾选，删除勾选的
            if (result == DialogResult.OK)
            {
                //删除记录                
                for (int i = 0; i < sum; i++)
                {
                    string xiang="";
                    //判断是否勾选
                    if (dataGridView1.Rows[i].Cells[0].Value == "true")
                    {
                        xiang = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        try
                        {
                            RegistryKey delKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true);
                            delKey.DeleteValue(xiang);                           
                            delKey.Close();
                        }catch(Exception ex)
                        {
                            MessageBox.Show("删除失败。\r\n错误原因："+ex.ToString());
                            return;
                        }                                               
                    }
                }
                MessageBox.Show("删除成功");
                while (this.dataGridView1.Rows.Count != 0)
                {
                    this.dataGridView1.Rows.RemoveAt(0);
                }
                mstsc_info();
            }
            else
            {
                MessageBox.Show("取消删除");
            }
            if (sort() == true)
            {
                mstsc_info();
                MessageBox.Show("重新排序成功");
            }
            else
            {
                MessageBox.Show("重新排序error");
            }


        }

        private void item_sort_Click(object sender, EventArgs e)
        {
            if (sort() == true)
            {
                mstsc_info();
                MessageBox.Show("重新排序成功");
            }
            else
            {
                MessageBox.Show("重新排序error");
            }
        }

        private bool sort()
        {
            bool res=false;
            try
            {
                //获取项名
                string[] info;
                RegistryKey Key;
                Key = Registry.CurrentUser;
                RegistryKey myreg = Key.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default");
                info = myreg.GetValueNames();
                myreg.Close();
                int mstsc_sum = info.Length;
                List<string> zhi = new List<string>();
                zhi.Clear();
                //获取键值，用法找不到重命名键名的，所以和值一起操作
                for (int j = 0; j < mstsc_sum; j++)
                {
                    //获取键值               
                    RegistryKey Keyzhi;
                    Keyzhi = Registry.CurrentUser;
                    RegistryKey kzhi = Key.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default");
                    zhi.Add(kzhi.GetValue(info[j]).ToString());
                    kzhi.Close();
                }
                //重新排序
                //微软远程桌面最多保留10个记录，每次连接后，所有记录都会往后挪一位，处于第10的位置往后挪就删掉了
                for (int i = 0; i < mstsc_sum; i++)
                {
                    //先删除这个项，然后从第一个开始添加
                    RegistryKey delKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true);
                    delKey.DeleteValue(info[i]);
                    delKey.Close();
                    RegistryKey key = Registry.CurrentUser;
                    RegistryKey software = key.OpenSubKey("SOFTWARE\\Microsoft\\Terminal Server Client\\Default", true); //该项必须已存在
                    software.SetValue("MRU" + i.ToString(), zhi[i]);
                }
                res = true;
            }catch(Exception ex)
            {
                MessageBox.Show("获取信息失败。\r\n错误原因：" + ex.ToString());
            }
                       
            return res;
        }

        private void lab_github_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/dreamstation625/Remote-desktop-record-deletion-assistant");
        }
    }
}
