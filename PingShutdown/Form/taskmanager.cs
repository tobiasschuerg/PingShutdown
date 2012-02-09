using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingShutdown.taskmanager
{
    public partial class taskmanager : Form
    {
        public String[] watchlist { get; set; }

        public taskmanager ()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Converts Bytes to Megabytes.
        /// </summary>
        /// <param name="toconvert">The toconvert.</param>
        /// <returns>value as megabytes</returns>
        public float byteToMbyte (long toconvert) {
            return toconvert / 1024 / 1024;
        }

        /// <summary>
        /// Refreshes the tasks.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void refreshTasks (object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                dataGridView1.Rows.Add(false, theprocess.ProcessName, theprocess.Id, byteToMbyte(theprocess.PagedMemorySize64), theprocess.Threads.Count);
            }
        }

        private void button1_Click (object sender, EventArgs e)
        {
            refreshTasks();
        }

        private void refreshTasks ()
        {

        }

        private void taskmanager_FormClosed (object sender, FormClosedEventArgs e)
        {
            int rows =  dataGridView1.Rows.Count;
            watchlist = new String[rows];
            for (int i = 0; i < rows; i++)
            {
                if ((bool) dataGridView1.Rows[i].Cells[0].Value)
                {
                    watchlist[i] = dataGridView1.Rows[i].Cells["id"].Value.ToString();
                }
            }
            this.DialogResult = DialogResult.OK;
        }

        private void taskmanager_Load (object sender, EventArgs e)
        {
            refreshTasks();
        }

    }
}
