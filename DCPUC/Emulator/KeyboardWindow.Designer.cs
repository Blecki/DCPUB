namespace DCPUC.Emulator
{
    partial class KeyboardWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Click to set keyboard focus";
            // 
            // KeyboardWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(148, 27);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Name = "KeyboardWindow";
            this.Text = "Keyboard";
            this.Load += new System.EventHandler(this.KeyboardWindow_Load);
            this.Click += new System.EventHandler(this.KeyboardWindow_Click);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyboardWindow_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KeyboardWindow_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KeyboardWindow_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
    }
}