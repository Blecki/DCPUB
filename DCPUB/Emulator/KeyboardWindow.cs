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
    public partial class KeyboardWindow : Form
    {
        public KeyboardWindow()
        {
            InitializeComponent();
        }

        private void KeyboardWindow_Load(object sender, EventArgs e)
        {

        }

        private void KeyboardWindow_Click(object sender, EventArgs e)
        {
            Focus();
        }

        private void KeyboardWindow_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void KeyboardWindow_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void KeyboardWindow_KeyUp(object sender, KeyEventArgs e)
        {

        }
    }
}
