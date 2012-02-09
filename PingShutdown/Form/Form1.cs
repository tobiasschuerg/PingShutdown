using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace PingShutdown
{
    public partial class Form1 : Form
    {
        private Hostmanager hm;

        private int hostID = 0;
        private int timeToWait = 180;
        private Host host;
        Stopwatch timer = new Stopwatch();

        public Form1 ()
        {
            InitializeComponent();
            // todo loading process
            hm = new Hostmanager(100, 100); // todo
        }

        //todo: wird das noch gebraucht?
        double lasstime = 0;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick (object sender, EventArgs e)
        {            
            //if (timer.Elapsed.TotalSeconds > lasstime + 3)
           // {
             //   lasstime = timer.Elapsed.TotalSeconds;
            createTempHost();
                if (checkBox2.Checked)
                {
                    this.pingTempHost();
                }

                //hm.refreshHosts();
                printHostInfo();
           // }
                toolStripStatusLabel3.Text = "uptime: " + FormTools.TimeSpanToString(timer.Elapsed); ;

        }

        private void printHostInfo ()
        {
            bool alloffline = true;
            Host minOffHost = null;

            // Listeneinträge anpassen
            while (hostlist.RowCount != hm.hostcount)
            {
                if (hostlist.RowCount < hm.hostcount)
                {
                    hostlist.Rows.Add();
                }
                else
                {
                    hostlist.Rows.RemoveAt(0);
                }
            }


            int index = 0;
            // for (int i = 0; i < hosts.Length; i++)
            foreach (Host h in hm.getHosts())
            {
                if (h == null)
                {
                    continue;
                }

                // TOOLTIPPS:
                hostlist.Rows[index].Cells["ip"].ToolTipText = "doubleklick to remove";
                hostlist.Rows[index].Cells["hostname"].ToolTipText = "doubleklick to remove";

                // ID
                hostlist.Rows[index].Cells["id"].Value = h.id;

                // IP
                hostlist.Rows[index].Cells["ip"].Value = h.hostIP;

                // Hostname
                hostlist.Rows[index].Cells["hostname"].Value = h.hostname;
       
                // Pingtime
                if (h.online)
                {
                    hostlist.Rows[index].Cells["status"].Value = "online! - response: " + h.pingtime + " ms";
                    alloffline = false;
                }
                else // Host is offline
                {
                    if (alloffline && (minOffHost == null || h.offline.TotalSeconds < minOffHost.offline.TotalSeconds))
                    {
                        minOffHost = h;
                    }
                    hostlist.Rows[index].Cells["status"].Value = "offline since: " + FormTools.TimeSpanToString(h.offline);
                }
                index++;
            }

            if (minOffHost != null)
            {
                DateTime start = dateTimePicker1.Value;
                DateTime end = dateTimePicker2.Value;
                DateTime now = DateTime.Now;
                bool timetoshutdown = false;
                if (end.TimeOfDay > start.TimeOfDay)
                {
                    if (now.TimeOfDay <= end.TimeOfDay && now.TimeOfDay >= start.TimeOfDay)
                    {
                        timetoshutdown = true;
                    }
                }
                else  // start > end
                {
                    if (now.TimeOfDay >= start.TimeOfDay || now.TimeOfDay <= end.TimeOfDay)
                    {
                        timetoshutdown = true;
                    }
                }


                if (alloffline && timetoshutdown)
                {
                    // Check if Shutdown
                    if (minOffHost.offline.TotalSeconds > this.timeToWait)
                    {
                        if (checkBox1.Enabled)
                        {
                            timer1.Enabled = false;
                            Shutdown.shutdown(loadArguments());
                        }
                        else
                        {
                            progressBar1.Enabled = Visible;
                        }
                    }
                    else
                    {
                        shutdowninfo.Text = "shutdown in " + (int)(this.timeToWait - minOffHost.offline.TotalSeconds) + " seconds...";
                        progressBar1.Value = (int)minOffHost.offline.TotalSeconds;
                    }
                }
                else
                {
                    if (timetoshutdown)
                    {
                        shutdowninfo.Text = "shutdown if " + minOffHost.toString() + " will be offline for more than " + this.timeToWait + " seconds.";
                        progressBar1.Visible = true; ;
                    }
                    else
                    {
                        shutdowninfo.Text = "automatic shutdown will be activated between " + start.TimeOfDay + " and " + end.TimeOfDay + ".";
                        progressBar1.Visible = false;
                    }
                    
                    progressBar1.Value = 0;
                }
            }
        }

        private void Form1_Load (object sender, EventArgs e)
        {
            progressBar1.Maximum = this.timeToWait;
            timer.Start();
            hostaddress1.Text = Properties.Settings.Default.ip;
            waittime.Value = Properties.Settings.Default.countdownTime;
            dateTimePicker1.Value = Properties.Settings.Default.time1;
            dateTimePicker2.Value = Properties.Settings.Default.time2;
            textboxMacAddresse.Text = Properties.Settings.Default.macaddresse;
            version.Text = this.ProductName + " 0.2.2";

            // Properties.Settings.Default.ips.Clear();
            foreach(String newIP in Properties.Settings.Default.ips)
            if (!hm.addHost(new Host(newIP, hostID)))
            {
                MessageBox.Show("error on adding host, maybe it's already added?");
            }

            
        }

        private void numericUpDown1_ValueChanged (object sender, EventArgs e)
        {
            timeToWait = (int)waittime.Value;
            progressBar1.Maximum = this.timeToWait;
        }

        private void loadMacAndWake (object sender, EventArgs e)
        {            
            if (textboxMacAddresse.Text != "")
            {
                // load Mac address from textfield:
                try
                {
                    byte[] towake = Pingman.GetMacArray(textboxMacAddresse.Text);
                    // wake Host
                    Pingman.WakeOnLan(towake);
                }
                catch (ArgumentException ae)
                {
                    
                    MessageBox.Show("Error!: \n" + ae.Message, "invalid mac address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                textboxMacAddresse.Text = "00:00:00:00:00:00";
            }
        }

        private void button2_Click (object sender, EventArgs e)
        {
            if (hostaddress1.Text != "")
            {
                try
                {
                    textboxMacAddresse.Text = Pingman.RequestMACAddressString(hostaddress1.Text);
                }
                catch (Exception ex)
                {
                    textboxMacAddresse.Text = "Error!";
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void textBox2_TextChanged (object sender, EventArgs e)
        {
            buttonWOL.Text = "Wake " + textboxMacAddresse.Text;
        }

        Stopwatch stopwatch = new Stopwatch();
        private void pingTempHost ()
        {
            if (this.host != null)
            {
                if (host.hostIP == "")
                {
                    hostinfo.Text = "no valid IP \n but maybe " + host.hostname + " is any hostname?";
                }
                else
                {
                    hostinfo.Text = "IP is valid: " + this.host.hostIP;
                }

                host.ping((int)timeout.Value, (int)buffersize.Value);
                if (!host.online)
                {
                   hostinfo.Text = "Host not reachable \n typing error?";
                }

                // print some information:
                if (host.online)
                {
                    toolStripStatusLabel1.Text = this.host.hostname + " is online.";
                    toolStripStatusLabel2.Text = "Response time: " + host.pingtime + " ms";
                    progressBar1.Value = 0;                    
                }
                else // offline
                {
                    toolStripStatusLabel1.Text = this.host.hostname + " is offline.";     
                }
            }

        }

        private void createTempHost ()
        {
            // check if host changed 
            if (this.host == null || this.host.hostIP != hostaddress1.Text && this.host.hostname != hostaddress1.Text)
            {
                if (hostaddress1.Text == "" || !(hostaddress1.TextLength > 2))
                {
                    this.host = null;
                }
                else
                {
                    this.host = new Host(hostaddress1.Text, hostID);
                    buttonAddToList.Enabled = true;
                }
            }
        }


        private string loadArguments ()
        {
            string arguments = "";
            if (radioButton1.Checked) arguments += "-s ";
            if (radioButton2.Checked) arguments += "-r ";
            if (radioButton3.Checked) arguments += "-h ";
            //if (radioButton4.Checked) arguments += " ";
            if (radioButton5.Checked) arguments += "-l ";

            arguments += "-t " + numericUpDown2.Value;
            arguments += " -c \"caused by Shutdown tool\" ";

            return arguments;
        }

        private void textBox1_TextChanged (object sender, EventArgs e)
        {
            lasstime = timer.Elapsed.Seconds + 3;
            label9.Text = hostaddress1.Text;
        }

        private void button4_Click (object sender, EventArgs e)
        {
            if (!hm.addHost(host))
            {
                MessageBox.Show("error on adding host, maybe it's already added?");
            }
            else
            {
                // insert new row into hostlist
                hostlist.Rows.Add(host.hostIP, host.hostname, host.pingtime);
                resetFields();
            }
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e)
        {
            // Thread anhalten:
            // todo schöner machen oder in klasse auslagen
            hm.refreshWorker.Abort();

            Properties.Settings.Default.ip = hostaddress1.Text;
            Properties.Settings.Default.countdownTime = waittime.Value;
            Properties.Settings.Default.time1 = dateTimePicker1.Value;
            Properties.Settings.Default.time2 = dateTimePicker2.Value;
            Properties.Settings.Default.macaddresse = textboxMacAddresse.Text;

            // IPs aus Hostliste speichern
            Properties.Settings.Default.ips.Clear();
            foreach(Host host in hm.getHosts()) {
                if (host != null)
                {
                    Properties.Settings.Default.ips.Add(host.hostIP);
                }
            }

            // in Datei speichern
            Properties.Settings.Default.Save();
            // MessageBox.Show("saved");
        }


        private void resetFields() {
            hostaddress1.ResetText();
            hostinfo.Text = "enter IP oder Hostname";
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
        }

        private void hostlist_CellContentClick (object sender, DataGridViewCellEventArgs e)
        {

        }

        private void hostlist_MouseDoubleClick (object sender, MouseEventArgs e)
        {
            int row = hostlist.SelectedCells[0].RowIndex;
            int idToRemove = (int)hostlist.Rows[row].Cells["id"].Value;
            switch (MessageBox.Show("Do you want to remove " + hm.getHost(idToRemove).toString() + " from list?",
                        "remove host",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    // "Yes" processing
                    hm.removeHost(idToRemove);  
                    hostlist.Rows.RemoveAt(row);
                    break;

                case DialogResult.No:
                    // "No" processing
                    // do nothing
                    break;
            }
        }

        private void label3_Click (object sender, EventArgs e)
        {

        }

        private void button3_Click (object sender, EventArgs e)
        {
           
        }

        private void button4_Click_1 (object sender, EventArgs e)
        {
            this.pingTempHost();
        }

        private void numericUpDown1_ValueChanged_1 (object sender, EventArgs e)
        {
            if (hm != null)
            {
                hm.sleeptime = (int)this.numericUpDown1.Value;
            }
        }

        private void timeout_ValueChanged (object sender, EventArgs e)
        {
            if (hm != null)
            {
                hm.pingtimeout = (int)this.timeout.Value;
            }
        }

        private void buffersize_ValueChanged (object sender, EventArgs e)
        {
            if (hm != null)
            {
                hm.pingBufferSize = (int)this.buffersize.Value;
            }
        }

        private void loadMacAndWake ()
        {

        }

        private void toolStripContainer1_LeftToolStripPanel_Click (object sender, EventArgs e)
        {

        }

        public void hideToTray ()
        {
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void resoreFromTray (object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void button2_Click_1 (object sender, EventArgs e)
        {
            
        }

        private void Form1_Resize (object sender, EventArgs e)
        {
            // Is the window minimized?

            if (this.WindowState == FormWindowState.Minimized)
            {
                hideToTray();
            }

            else if (this.WindowState == FormWindowState.Normal)
            {

                // Handling

            }    // Resize handling 
        }

        private void button2_Click_2 (object sender, EventArgs e)
        {
 
        }

        private void button2_Click_3 (object sender, EventArgs e)
        {
            Form taskmanager = new PingShutdown.taskmanager.taskmanager();
            if (taskmanager.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("asdf" + taskmanager);
            }
        }

    }
}
