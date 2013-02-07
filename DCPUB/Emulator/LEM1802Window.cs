using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DCPUB.Emulator
{
    public partial class LEM1802Window : Form
    {
        private LEM1802 lem = null;

        public LEM1802Window(LEM1802 lem)
        {
            this.lem = lem;
            InitializeComponent();
            this.Show();
        }

        private void LEM1802Window_Paint(object sender, PaintEventArgs e)
        {
            this.pictureBox1.Image = lem.ScreenImage;
        }

    }
}
