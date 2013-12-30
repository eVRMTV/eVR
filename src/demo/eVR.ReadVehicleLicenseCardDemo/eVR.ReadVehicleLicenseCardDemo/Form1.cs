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
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows.Forms;
    using System.Xml;
    using EVR.Reader;
    using PCSC;

    /// <summary>
    /// FormeVRDemo definition
    /// </summary>
    public partial class FormeVRDemo : Form
    {
        /// <summary>
        /// eVRCardReader object
        /// </summary>
        private eVRCardReader evrCardReaderDemo;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FormeVRDemo"/> class
        /// </summary>
        public FormeVRDemo()
        {
            this.InitializeComponent();
            try
            {
                X509Certificate2 csca = new X509Certificate2(EVRCardReaderAppSettings.CSCAFilename);                            
                this.evrCardReaderDemo = new eVRCardReader(csca, null, this.Monitor_CardInserted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// CardInserted event
        /// </summary>
        /// <param name="e">CardStatusEventArgs object</param>
        private void CardInserted(CardStatusEventArgs e)
        {
            /*
             * Timing problem with fast card readers; delay should be > 300ms
             */
            System.Threading.Thread.Sleep(EVRCardReaderAppSettings.CardAccessDelay);
            this.ReadingCardData(e.ReaderName);
        }

        private void Monitor_CardInserted(object sender, CardStatusEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(delegate() { this.CardInserted(e); }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                this.CardInserted(e);
            }
        }

        private void ReadingCardData(string readerName)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                /*
                 * For executing the CardManagerDisable and ATR check we have to select a reader first.
                 */
                this.evrCardReaderDemo.SelectReader(readerName);

                /*
                 * Re-initialize the GUI elements
                 */

                this.registrationCardBack1.Data = null;
                this.registrationCardFront1.Data = null;

                Application.DoEvents();

                /*
                 * Remove CVO tabs
                 */
                int tabpagecount = this.flatTabControl1.TabPages.Count;
                for (int i = tabpagecount - 1; i > 0; i--)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    this.flatTabControl1.TabPages.RemoveAt(i);
                }
                
                /*
                 * Read card data; reader is selected on CardInserted event
                 */
                this.evrCardReaderDemo.Read(readerName);
                this.evrCardReaderDemo.DisplayError = !this.EVRCardValid();                
                this.registrationCardFront1.Data = this.evrCardReaderDemo;
                this.registrationCardBack1.Data = this.evrCardReaderDemo;
                                
                /*
                 * For each CVO add a CVO tabpage
                 */

                if (this.evrCardReaderDemo.RegistrationC.CVOs != null)
                {
                    for (int i = 0; i < this.evrCardReaderDemo.RegistrationC.CVOs.Length; i++)
                    {
                        TabPage tp = new TabPage(string.Format("CVO Data {0}", i + 1));

                        tp.Controls.Add(this.CreateCVODataGrid(this.evrCardReaderDemo.RegistrationC.CVOs[i]));
                        this.flatTabControl1.TabPages.Add(tp);
                    }
                }
            }
            catch (eVRCardReaderException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private DataGridView CreateCVODataGrid(string cvoAsXML)
        {
/*#if DEBUG
            using (TextReader tr = new StreamReader("WBA3G51090F926167 20131028 1136-signed.xml"))
            {
                CVOasXML = tr.ReadToEnd();
            }
#endif*/
            List<CVOItem> cvoItems = new List<CVOItem>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(cvoAsXML);
            XmlNode body = doc.SelectSingleNode("//Body/CocDataGroup");
            this.ProcessNode(body, null, cvoItems);
            BindingSource bs = new BindingSource();
            bs.DataSource = cvoItems;
            DataGridView dg = new DataGridView();
            dg.ReadOnly = true;
            dg.AllowUserToAddRows = false;
            dg.AllowUserToDeleteRows = false;
            dg.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dg.DataSource = bs;
            dg.Dock = DockStyle.Fill;
            dg.AutoGenerateColumns = true;
            bs.ResetBindings(false);
            dg.Columns.Add("Field", "Name");
            dg.Columns[0].DataPropertyName = "Field";
            dg.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dg.Columns[0].DefaultCellStyle.BackColor = Color.LightBlue;
            dg.Columns.Add("Value", "Value");
            dg.Columns[1].DataPropertyName = "Value";
            dg.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            return dg;
        }

        private void ProcessNode(XmlNode node, XmlNode parent, List<CVOItem> l)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        this.ProcessNode(child, node, l);
                    }

                    break;
                case XmlNodeType.Text:
                    l.Add(new CVOItem() { Field = Translator.Translate(parent.Name), Value = node.Value });
                    break;
                default:
                    throw new Exception("Unknown NodeType");
            }
        }

        private bool EVRCardValid()
        {
            try
            {
                if (!this.evrCardReaderDemo.CardManagerDisabled())
                {
                    return false;
                }

                if (!this.evrCardReaderDemo.CheckATR()) return false;
                if (!this.evrCardReaderDemo.EFSOd.PassiveAuthentication) return false;
                if (!this.evrCardReaderDemo.EFSOd.IsValid) return false;
                bool efaaPassiveAuth = this.evrCardReaderDemo.EFAA.PassiveAuthentication;
                if (efaaPassiveAuth)
                {
                    if (!this.evrCardReaderDemo.EFAA.ActiveAuthentication) return false;
                }
                else { return false; }
                ////check CRL
                this.evrCardReaderDemo.RegistrationB.CRLCheckEnabled = this.evrCardReaderDemo.RegistrationA.CRLCheckEnabled;
                bool pAA = this.evrCardReaderDemo.RegistrationA.PassiveAuthentication && this.evrCardReaderDemo.RegistrationA.SignatureValid;
                bool pAB = this.evrCardReaderDemo.RegistrationB.PassiveAuthentication && this.evrCardReaderDemo.RegistrationB.SignatureValid;
                bool pAC = this.evrCardReaderDemo.RegistrationC.PassiveAuthentication;
                if (!pAA | !pAB | !pAC)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
         }
    }
}
