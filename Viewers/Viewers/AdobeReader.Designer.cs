using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Controls;
using GestureControls.Input;
using System.Windows;
using System.Diagnostics;


namespace Viewers
{
    partial class AdobeReader
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdobeReader));
            this.axAcroPDF1 = new AxAcroPDFLib.AxAcroPDF();
            ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).BeginInit();
            this.SuspendLayout();
            // 
            // axAcroPDF1
            // 
            this.axAcroPDF1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axAcroPDF1.Enabled = true;
            this.axAcroPDF1.Location = new System.Drawing.Point(0, 0);
            this.axAcroPDF1.Name = "axAcroPDF1";
            this.axAcroPDF1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axAcroPDF1.OcxState")));
            this.axAcroPDF1.Size = new System.Drawing.Size(147, 150);
            this.axAcroPDF1.TabIndex = 0;
            // 
            // AdobeReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axAcroPDF1);
            this.Name = "AdobeReader";
            ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).EndInit();
            this.ResumeLayout(false);
            

        }

        #endregion

        private AxAcroPDFLib.AxAcroPDF axAcroPDF1;


        /// <summary>
        /// The code below is the code added by the programmer.  Change it as needed,
        /// according to the uses of the application
        /// </summary>
        /// <param name="FileName">The name of the file to be loaded</param>
        public AdobeReader(string FileName)
        {
            InitializeComponent();
            this.axAcroPDF1.setShowToolbar(false);
            this.axAcroPDF1.LoadFile(FileName);
            //this.axAcroPDF1.setZoom(67);
            this.axAcroPDF1.Visible = true;
        }

        public void NextPage()
        {
            this.axAcroPDF1.gotoNextPage();
        }

        public void PreviousPage()
        {
            this.axAcroPDF1.gotoPreviousPage();
        }
    }
}
