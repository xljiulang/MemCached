using MemCachedManager.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace MemCachedManager
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 当前选定的服务
        /// </summary>
        private MemService currentService;

        public MainForm()
        {
            InitializeComponent();

            this.BindingStatus();
            this.btnStop.Click += btnStop_Click;
            this.btnStart.Click += btnStart_Click;
            this.btnAdd.Click += btnAdd_Click;
            this.btnDelete.Click += btnDelete_Click;
            this.txtBoxCmd.KeyPress += txtBoxCmd_KeyPress;
            this.cmbBoxPort.SelectedIndexChanged += cmbBoxPort_SelectedIndexChanged;

            this.RefreshService();
            this.BindingStatus();
        }


        private void RefreshService()
        {
            var ms = MemService.GetMemService();
            this.cmbBoxPort.Items.AddRange(ms);
            if (ms.Length > 0)
            {
                this.cmbBoxPort.SelectedIndex = 0;
            }
        }


        /// <summary>
        /// 选择变化 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbBoxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.currentService = this.cmbBoxPort.SelectedItem as MemService;
            this.BindingStatus();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("删除服务会带来数据丢，确定要继续吗？", "提示", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            if (this.currentService != null)
            {
                this.currentService.Delete();
            }

            var item = this.currentService;
            this.currentService = null;
            this.cmbBoxPort.Items.Remove(item);
            if (this.cmbBoxPort.Items.Count > 0)
            {
                this.cmbBoxPort.SelectedIndex = 0;
            }
            this.BindingStatus();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var add = new AddFrom();
            if (add.ShowDialog() == DialogResult.OK)
            {
                int port = add.Port;
                int mem = add.MaxMem;

                var s = MemService.GetMemService().FirstOrDefault(item => item.Port == port);
                if (s != null)
                {
                    MessageBox.Show("此端口的服务已经存在...", "提示");
                    return;
                }

                s = MemService.AddService(port);
                s.Start(mem, port);
                this.cmbBoxPort.Items.Add(s);
                this.cmbBoxPort.SelectedItem = s;
            }
        }


        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            int port = 0;
            int mem = 0;
            int.TryParse(this.cmbBoxPort.Text.Trim(), out port);
            int.TryParse(this.txtBoxMem.Text.Trim(), out mem);

            if (port == 0 || mem == 0)
            {
                MessageBox.Show("端口或内存选项不正确...", "提示");
                return;
            }
            this.currentService.Start(mem, port);
            this.BindingStatus();
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("停止服务会带来数据丢，确定要继续吗？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.currentService.Stop();
                this.BindingStatus();
            }
        }

        /// <summary>
        /// 绑定控件状态
        /// </summary>
        private void BindingStatus()
        {
            if (this.currentService == null)
            {
                this.txtBoxCmd.Enabled = false;
                this.txtBoxMem.Enabled = false;

                this.btnStart.Enabled = false;
                this.btnStop.Enabled = false;
                this.btnDelete.Enabled = false;
                return;
            }

            var running = this.currentService.HasRunnig;
            this.btnStart.Enabled = !running;
            this.btnStop.Enabled = running;
            this.btnDelete.Enabled = true;
            this.txtBoxMem.Enabled = !running;
            this.txtBoxCmd.Enabled = running;
            this.txtBoxMem.Text = this.currentService.MaxMemory.ToString();
            this.cmbBoxPort.Text = this.currentService.Port.ToString();
        }


        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13)
            {
                return;
            }

            var cmd = this.txtBoxCmd.Text.Split(new[] { "\r\n" }, StringSplitOptions.None).LastOrDefault();
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            e.Handled = true;
            if (cmd == "cls")
            {
                this.txtBoxCmd.Clear();
                return;
            }

            var value = CmdHelper.Exec(cmd, this.currentService.Port);
            var rn = "\r\n";
            if (value.EndsWith(rn) == false)
            {
                value = value + rn;
            }
            this.txtBoxCmd.AppendText(rn + value);
        }

    }
}
