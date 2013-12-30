/*
Copyright (c) 2013, Dienst Wegverkeer, RDW, All rights reserved. 

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 

• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
• Neither the name of the Dienst Wegverkeer, RDW,  nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

namespace EVR.ReadVehicleLicenseCardDemo
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using EVR.Reader;
    
    /// <summary>
    /// RegistrationCardBack class
    /// </summary>
    public partial class RegistrationCardBack : UserControl
    {
        private eVRCardReader data;
      
        public RegistrationCardBack()
        {
            this.InitializeComponent();
            this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.Insert_card;
            this.pnleVRBack.Visible = false;
        }

        public new event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
                this.AddHandler(this.Controls, value);
            }
            remove
            {
                base.DoubleClick -= value;
                this.RemoveHandler(this.Controls, value);
            }
        }

        private void AddHandler(ControlCollection coll, EventHandler e)
        {
            foreach (Control control in coll)
            {
                control.DoubleClick += e;
                this.AddHandler(control.Controls, e);
            }
        }

        private void RemoveHandler(ControlCollection coll, EventHandler e)
        {
            foreach (Control control in coll)
            {
                control.DoubleClick -= e;
                this.AddHandler(control.Controls, e);
            }
        }
        /// <summary>
        /// Gets or sets Data values
        /// </summary>
        public eVRCardReader Data
        {
            get
            {
                return this.data;
            }

            set
            {
                this.data = value;

                if (this.data != null)
                {
                    this.lblE.Text = this.data.RegistrationA.E;
                    this.lblD1.Text = this.data.RegistrationA.D1;
                    this.lblJ.Text = this.data.RegistrationB.J;

                    List<string> D2 = Helper.StringSplitWrap(this.data.RegistrationA.D2, 40);
                    this.lblD2_1.Text = D2.Count > 0 ? D2[0] : "-";
                    this.lblD2_2.Text = D2.Count > 1 ? D2[1] : "-";
                    this.lblD2_3.Text = D2.Count > 2 ? D2[2] : "-";

                    List<string> D3 = Helper.StringSplitWrap(this.data.RegistrationA.D3, 40);
                    this.lblD3_1.Text = D3.Count > 0 ? D3[0] : "-";
                    this.lblD3_2.Text = D3.Count > 1 ? D3[1] : "-";

                    this.lblR.Text = this.data.RegistrationB.R;
                    this.lblK.Text = this.data.RegistrationA.K;
                    this.lblV9.Text = this.data.RegistrationB.V9;
                    this.lblF1.Text = this.data.RegistrationA.F1;
                    this.lblF2.Text = this.data.RegistrationB.F2;
                    this.lblF3.Text = this.data.RegistrationB.F3;
                    this.lblP1.Text = this.data.RegistrationA.P1;
                    this.lblP2.Text = this.data.RegistrationA.P2;
                    this.lblP3.Text = this.data.RegistrationA.P3;
                    this.lblQ.Text = this.data.RegistrationA.Q;
                    this.lblO1.Text = this.data.RegistrationB.O1;
                    this.lblO2.Text = this.data.RegistrationB.O2;
                    this.lblG.Text = this.data.RegistrationA.G;
                    this.lblT.Text = this.data.RegistrationB.T;
                    this.lblS1.Text = this.data.RegistrationA.S1;
                    this.lblS2.Text = this.data.RegistrationA.S2;

                    this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.kentekenbewijs_ak_new;
                    this.pnleVRBack.Visible = true;
                }
                else
                {
                    this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.Insert_card;
                    this.pnleVRBack.Visible = false;
                }
            }
        }
        /// <summary>
        /// Paint event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">PaintEventsArgs object</param>
        private void RegistrationCardBack_Paint(object sender, PaintEventArgs e)
        {
            if (this.data!=null)
               if (this.data.DisplayError)
                {
                    using (Pen p = new Pen(Color.Red, 2))
                    {
                        Point topLeft = new Point(0, 0);
                        Point topRight = new Point(this.Width - 1, 0);
                        Point bottomLeft = new Point(0, this.Height - 1);
                        Point bottomRight = new Point(this.Width - 1, this.Height - 1);
                        p.Width = 5;
                        e.Graphics.DrawLine(p, topLeft, bottomRight);
                        e.Graphics.DrawLine(p, topRight, bottomLeft);
                    }
                 }
             
        }
    }
}
