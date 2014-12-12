using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MemCachedManager
{
    public partial class AddFrom : Form
    {
        public int Port { get; private set; }

        public int MaxMem { get; private set; }

        public AddFrom()
        {
            InitializeComponent();
            this.btnOK.Click += btnOK_Click;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int port = 0;
            int mem = 0;
            int.TryParse(this.textBox1.Text.Trim(), out port);
            int.TryParse(this.textBox2.Text.Trim(), out mem);
            if (port == 0 || mem == 0)
            {
                MessageBox.Show("端口或内存选项不正确...", "提示");
                return;
            }
            this.Port = port;
            this.MaxMem = mem;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
