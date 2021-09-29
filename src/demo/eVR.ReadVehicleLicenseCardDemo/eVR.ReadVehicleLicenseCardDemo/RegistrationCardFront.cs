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
    /// <para></para>
    /// </summary>
    public partial class RegistrationCardFront : UserControl
    {
        private eVRCardReader data;
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationCardFront" /> class.
        /// </summary>
        public RegistrationCardFront()
        {
            this.InitializeComponent();
            this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.Insert_card;
            this.pnleVR.Visible = false;
        }

        /// <summary>
        /// Gets or sets data values
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
                    this.lblDocumentnummer.Text = this.data.RegistrationA.DocumentNumber;
                    this.lblA.Text = this.data.RegistrationA.A;
                    this.lblMeldcode.Text = this.data.RegistrationA.A_Meldcode;
                    this.lblB1.Text = Helper.EEYYMMDDToNLDate(this.data.RegistrationC.B1);
                    this.lblB2.Text = Helper.EEYYMMDDToNLDate(this.data.RegistrationC.B2);
                    this.lblI.Text = Helper.EEYYMMDDToNLDate(this.data.RegistrationC.I);
                    List<string> c11 = Helper.StringSplitWrap(this.data.RegistrationA.C11, 35);
                    this.lblC11_1.Text = c11.Count > 0 ? c11[0] : string.Empty;
                    this.lblC11_2.Text = c11.Count > 1 ? c11[1] : string.Empty;
                    this.lblC12.Text = this.data.RegistrationA.C12;
                    List<string> c13 = Helper.ToAdres(this.data.RegistrationA.C13, 30);
                    this.lblC13_1.Text = c13.Count > 0 ? c13[0] : string.Empty;
                    this.lblC13_2.Text = c13.Count > 1 ? c13[1] : string.Empty;
                    this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.kentekenbewijs_vk_new;
                    this.pnleVR.Visible = true;
                }
                else
                {
                    this.BackgroundImage = global::EVR.ReadVehicleLicenseCardDemo.Properties.Resources.Insert_card;
                    this.pnleVR.Visible = false;
                }
            }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="e"></param>
        private void AddHandler(ControlCollection coll, EventHandler e)
        {
            foreach (Control control in coll)
            {
                control.DoubleClick += e;
                this.AddHandler(control.Controls, e);
            }
        }
        ///
        private void RemoveHandler(ControlCollection coll, EventHandler e)
        {
            foreach (Control control in coll)
            {
                control.DoubleClick -= e;
                this.AddHandler(control.Controls, e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>                
        private void RegistrationCardFront_Paint(object sender, PaintEventArgs e)
        {
            // if the eVRCard is InValid this Paint event will draw a RED cross and an error meesage on the image.
            int widthpos = this.Width - 1;
            int heightpos = this.Height - 1;

            if (this.data != null)   
            if (this.data.DisplayError)
                {
                    using (Pen p = new Pen(Color.Red, 2))
                    {
                        Point topLeft = new Point(0, 0);
                        Point topRight = new Point(widthpos, 0);
                        Point bottomLeft = new Point(0, heightpos);
                        Point bottomRight = new Point(widthpos, heightpos);
                        p.Width = 5;
                       
                        e.Graphics.DrawLine(p, topLeft, bottomRight);
                        e.Graphics.DrawLine(p, topRight, bottomLeft);
                        p.Width = 50;
                        e.Graphics.DrawLine(p, bottomLeft, bottomRight);
                                                
                        // Create string to draw.
                        string drawString = global::EVR.ReadVehicleLicenseCardDemo.Properties.Settings.Default.ErrorMessage;
                        
                        // Create font and brush.
                        Font drawFont = new Font("Arial", 14);
                        SolidBrush drawBrush = new SolidBrush(Color.White);
                        
                        // Create point for upper-left corner of drawing.
                        PointF drawPoint = new PointF(60.0F, 350.0F);

                        // Draw string to screen.
                        e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
                    }
              }
        }

       



    }
}
