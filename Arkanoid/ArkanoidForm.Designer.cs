using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Arkanoid.WinForms
{
    partial class Arkanoid : Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Arkanoid";
            this.BackColor = System.Drawing.Color.Black;
        }
    }
}