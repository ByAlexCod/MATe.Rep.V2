﻿using MATeV2;
using Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MATeUI
{
    public partial class EmployeeUI : Form
    {
        public EmployeeUI()
        {
            InitializeComponent();
           
        }

        ContextAndUserManager _ctxuser = Authentification.CurrentCtxUser;

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ctxuser.SaveAs("-Context.MATe");
            //using(var ct = _ctxuser.ObtainAccessor())
            //{
            //    Context ctx = ct.Context;
            //    Thread.Sleep(5000);
            //}
            MessageBox.Show("SUCCESS");
        }

        private void synchronizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncerSender syncsender = new SyncerSender(_ctxuser);
            syncsender.PrepareDatas("-Context.MATe", "sync", "temp.zip", 15000);
            Thread thread = new Thread(syncsender.SpreadDatas);
            thread.IsBackground = true;
            thread.Start();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filename = Application.StartupPath + @"\Read Me.txt";

            System.Diagnostics.Process.Start(fileName: @"" + filename + "");



        }



    

        private void myAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifyAccount changeCount = new ModifyAccount(Authentification.CurrentCtxUser.CurrentUser);
            changeCount.ShowDialog();
        }

        private void deconnexionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dEBUGToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
