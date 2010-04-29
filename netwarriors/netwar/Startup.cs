﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace netwar
{
    public partial class Startup : Form
    {
        public bool StartGame = false;

        public Startup()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            StartGame = true;
            this.Close();
        }
    }
}
