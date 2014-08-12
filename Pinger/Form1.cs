using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Pinger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Monitor> mon = new List<Monitor>();
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //
            if (File.Exists("url"))
            {
                string[] v = File.ReadAllLines("url");
                foreach (string ur in v)
                {
                    mon.Add(new Monitor(ur));
                    ListViewItem m = new ListViewItem();
                    m.Text = ur;
                    m.SubItems.Add("Checking...");
                    listView1.Items.Add(m);
                }
            }
            CheckForIllegalCrossThreadCalls = false;
            //Start the checking thread.
            notifyIcon1.Visible = true;
            new Thread(new ThreadStart(Check)) { IsBackground = true }.Start();
        }
        private void Check()
        {
            while (true)
            {
                foreach(Monitor m in mon)
                {
                    if (m.Status == 2)
                        continue;
                    if (m.Status != m.OldStatus || m.OldStatus == 2)
                    {
                        //Check for the changes.
                        foreach (ListViewItem lv in listView1.Items)
                        {
                            if (lv.Text == m.URI)
                            {
                                //Set the status.
                                lv.SubItems[1].Text = (m.Status == 0 ? "Offline" : "Online");
                                //Notify of the new changes.
                                if (m.Status == 1)
                                {
                                    notifyIcon1.BalloonTipTitle = m.URI + " is Online!";
                                    notifyIcon1.BalloonTipText = m.URI + " is now online!";
                                }
                                else if (m.Status == 0)
                                {
                                    notifyIcon1.BalloonTipTitle = m.URI + " is Offline!";
                                    notifyIcon1.BalloonTipText = m.URI + " is now offline! " + m.err;
                                }
                                notifyIcon1.ShowBalloonTip(10000);
                                m.OldStatus = m.Status;
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //Create a new Monitor.
            mon.Add(new Monitor(textBox1.Text));
            ListViewItem m = new ListViewItem();
            m.Text = textBox1.Text;
            m.SubItems.Add("Checking...");
            listView1.Items.Add(m);
            textBox1.Text = "";
            //Add to final list.
            string v = "";
            foreach (Monitor mm in mon)
            {
                v += mm.URI + "\n";
            }
            File.WriteAllText("url", v);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(listView1.SelectedItems.Count > 0)) return;
            string vv = "";
            foreach (Monitor m in mon)
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    if (m.URI == listView1.SelectedItems[0].Text)
                    {
                        //Remove it.
                        m.th.Abort();
                        listView1.Items.Remove(listView1.SelectedItems[0]);
                        //Update the file.
                        mon.Remove(m);
                        break;
                    }                   
                }
            }
            foreach (Monitor mm in mon)
            {
                vv += mm.URI + "\n";
            }
            File.WriteAllText("url", vv);
        }
    }
}
