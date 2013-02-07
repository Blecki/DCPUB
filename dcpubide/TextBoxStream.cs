using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DCPUCIDE
{
    public class TextBoxStream : DCPUB.Assembly.EmissionStream
    {
        public RichTextBox textBox = null;

        public TextBoxStream(RichTextBox textBox)
        {
            this.textBox = textBox;
        }

        public override void WriteLine(string line)
        {
            textBox.AppendText(line + "\r\n");
        }
    }
}
