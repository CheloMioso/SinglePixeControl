using DMX_v1_0.Properties;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

//using USBHIDDRIVER;

namespace DMX_v1._0
{
    public partial class ControlDMX : Form
    {
        public ControlDMX()
        {
            InitializeComponent();
        }

        

        private void cBoxCOMList_Click(object sender, EventArgs e)
        {
            string[] COMList = SerialPort.GetPortNames();
            cBoxCOMList.Items.Clear();
            cBoxCOMList.Items.AddRange(COMList);
        }

        bool btOpenClose = true;
        SerialPort COMport;
        private void btCOMOpen_Click(object sender, EventArgs e)
        {
            if (btOpenClose)
            {
                if (OpenCOM())
                {
                    btOpenClose = false;
                    btCOMOpen.Text = "Close";
                    cBoxCOMList.Enabled = false;
                    groupBox1.Enabled = true;
                    this.KeyPreview = true;
                }
                else
                    MessageBox.Show("Problem Open COM Port");
            }
            else
            {
                try { COMport.Close(); }
                catch { MessageBox.Show("Problem Close COM Port"); COMport = null; }
                
                btCOMOpen.Text = "Open";
                btOpenClose = true;
                cBoxCOMList.Enabled = true;
                groupBox1.Enabled = false;
                this.KeyPreview = false;
            }
        }

        private bool OpenCOM()
        {
            try
            {
                COMport = new SerialPort(cBoxCOMList.SelectedItem.ToString(), 250000, Parity.None, 8, StopBits.Two);
                COMport.Open();
                return true;
            }
            catch { return false; }
        }

        private void KeyPressUse(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8)
                e.Handled = true;
        }
        TextBox textBoxUser;
        private void tBoxValue(object sender, EventArgs e)
        {
            textBoxUser = (TextBox)sender;
            int rt = Convert.ToInt16(textBoxUser.Text);
            if (rt > 255)
            {
                textBoxUser.Text = "255";
            }
            if (rt > 127 && textBoxUser.Name == "tBoxAddress")
            {
                textBoxUser.Text = "128";
                rt = 128;
            }
            if (rt == 0 && textBoxUser.Name == "tBoxAddress")
            {
                textBoxUser.Text = "1";
                rt = 1;
            }

            if(textBoxUser.Name == "tBoxAddress")
                SetDipImage(rt);
        }
        private void tBox_TextChanged(object sender, EventArgs e)
        {
            textBoxUser = (TextBox)sender;
            if (textBoxUser.Text == "") textBoxUser.Text = "0";
            try
            {
                switch (textBoxUser.Name)
                {
                    case "tBoxW":
                        tBW.Value = Convert.ToInt16(textBoxUser.Text);
                        break;
                    case "tBoxR":
                        tBR.Value = Convert.ToInt16(textBoxUser.Text);
                        break;
                    case "tBoxG":
                        tBG.Value = Convert.ToInt16(textBoxUser.Text);
                        break;
                    case "tBoxB":
                        tBB.Value = Convert.ToInt16(textBoxUser.Text);
                        break;
                }
            }
            catch { }
        }
        private void tB_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBarUser = (TrackBar)sender;
            switch (trackBarUser.Name)
            {
                case "tBW":
                    tBoxW.Text = trackBarUser.Value.ToString();
                    break;
                case "tBR":
                    tBoxR.Text = trackBarUser.Value.ToString();
                    break;
                case "tBG":
                    tBoxG.Text = trackBarUser.Value.ToString();
                    break;
                case "tBB":
                    tBoxB.Text = trackBarUser.Value.ToString();
                    break;
            }
        }

        bool repEnable = false;
        byte[] Start = { 0x00 };
        byte[] MAB = new byte[256];
        byte[] dmx_data = new byte[512];

        private bool SendD()
        {
            Int32 sfd = (Convert.ToInt16(tBoxAddress.Text) - 1) * 4;
            dmx_data[sfd] = Convert.ToByte(tBoxB.Text);
            dmx_data[sfd + 1] = Convert.ToByte(tBoxG.Text);
            dmx_data[sfd + 2] = Convert.ToByte(tBoxR.Text);
            dmx_data[sfd + 3] = Convert.ToByte(tBoxW.Text);

            try
            {
                COMport.BreakState = true;
                Thread.Sleep(10);
                COMport.BreakState = false;
                COMport.Write(Start, 0, Start.Length);
                COMport.Write(dmx_data, 0, dmx_data.Length);
            }
            catch { MessageBox.Show("Problem Send to COM Port"); return false; };
            return true;
        }

        bool statThreadUser = false;
        const int NULLFRAME = 0;
        const int FRAME = 1000/70;
        int DMXFrame = NULLFRAME;
        void SendData()
        {
            while (true)
            {
                if (statThreadUser)
                {
                    if (!SendD())
                        if (!OpenCOM())
                        {
                            SendClick2();
                        }
                }
                Thread.Sleep(DMXFrame);
            }

        }

        private void SendClick2()
        {
            if (InvokeRequired)
                Invoke((Action)SendClick2);
            else
            {
                SendClick();
            }
        }

        private void SendClick()
        {
            if (cbRepeat.Checked || timer1.Enabled)
            {
                if (!repEnable)
                {
                    btContrSand.Text = "Stop";

                    if (!timer1.Enabled)
                    {
                        foreach (Control cnt in groupBox1.Controls)
                        {
                            var ertrt = cnt.Name;
                            if (ertrt == "tBoxW" || ertrt == "tBoxR" || ertrt == "tBoxG" || ertrt == "tBoxB" ||
                                ertrt == "btContrSand" ||
                                ertrt == "tBW" || ertrt == "tBR" || ertrt == "tBG" || ertrt == "tBB" ||
                                ertrt == "lbW" || ertrt == "lbW" || ertrt == "lbW" || ertrt == "lbW" ||
                                ertrt == "butMin" || ertrt == "butNarm" || ertrt == "butMax")
                                cnt.Enabled = true;
                            else
                                cnt.Enabled = false;
                        }
                    }
                    repEnable = true;
                    statThreadUser = true;
                    DMXFrame = FRAME;

                }
                else
                {
                    btContrSand.Text = "Submit";
                    timer1.Enabled = false;

                    foreach (Control cnt in groupBox1.Controls)
                    {
                        cnt.Enabled = true;
                    }
                    repEnable = false;
                    statThreadUser = false;
                    DMXFrame = NULLFRAME;

                }
                return;
            }
            Int32 i;
            for (i=0; i<dmx_data.Length; i++)
                dmx_data[i] = 0;
            SendD();
        }

        private void btContrSand_Click(object sender, EventArgs e)
        {
            SendClick();
        }

        Thread myTread;
        private void ControlDMX_Load(object sender, EventArgs e)
        {
            string[] COMList = SerialPort.GetPortNames();
            cBoxCOMList.Items.Clear();
            cBoxCOMList.Items.AddRange(COMList);
            cBoxCOMList.SelectedIndex = 0;
            myTread = new Thread(SendData);
            myTread.IsBackground = true;
            myTread.Start();
        }

        private void ControlDMX_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        tBoxW.Focus();
                        break;
                    case Keys.R:
                        tBoxR.Focus();
                        break;
                    case Keys.G:
                        tBoxG.Focus();
                        break;
                    case Keys.B:
                        tBoxB.Focus();
                        break;
                    case Keys.Enter:
                        btContrSand.Focus();
                        break;
                }
            }else
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        tBW.Focus();
                        break;
                    case Keys.R:
                        tBR.Focus();
                        break;
                    case Keys.G:
                        tBG.Focus();
                        break;
                    case Keys.B:
                        tBB.Focus();
                        break;
                }
            }
        }

        private void cBoxCOMList_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void ControlDMX_FormClosed(object sender, FormClosedEventArgs e)
        {
           try { statThreadUser = false; myTread.Join(5); myTread.Abort(); COMport.Close(); }
            catch {}
        }

        private void btSetAdr_Click(object sender, EventArgs e)
        {
            Int32 MB = ((Convert.ToInt16(tBoxAddress.Text)-1)*4)+1;

            Byte MBH = Convert.ToByte(MB>>8);

            Byte MBL = Convert.ToByte(MB & 0xff);

            foreach (Int16 i in dmx_data)
                dmx_data[i] = 0;

            dmx_data[0] = 129;
            dmx_data[1] = MBH;
            dmx_data[2] = MBL;
            dmx_data[3] = 0x02;

            try
            {
                COMport.BreakState = true;
                Thread.Sleep(10);
                COMport.BreakState = false;
                COMport.Write(Start, 0, Start.Length);
                COMport.Write(dmx_data, 0, dmx_data.Length);
            }
            catch { MessageBox.Show("Problem Send to COM Port");};
        }

        private void butMin_Click(object sender, EventArgs e)
        {
            tBB.Value = tBG.Value = tBR.Value = tBW.Value = 0;
            tBoxB.Text = tBoxG.Text = tBoxR.Text = tBoxW.Text = tBW.Value.ToString();
        }

        private void butNarm_Click(object sender, EventArgs e)
        {
            tBB.Value = tBG.Value = tBR.Value = tBW.Value = 128;
            tBoxB.Text = tBoxG.Text = tBoxR.Text = tBoxW.Text = tBW.Value.ToString();
        }

        private void butMax_Click(object sender, EventArgs e)
        {
            tBB.Value = tBG.Value = tBR.Value = tBW.Value = 255;
            tBoxB.Text = tBoxG.Text = tBoxR.Text = tBoxW.Text = tBW.Value.ToString();
        }
        private void SetDipImage(int nomber)
        {
            nomber = ((nomber - 1) * 4) +1;
            
            if (nomber < 0) return;

            if ((nomber & 0x04) > 0)
                pictDIP3.Image = Resources.on;
            else
                pictDIP3.Image = Resources.off;

            if ((nomber & 0x08) > 0)
                pictDIP4.Image = Resources.on;
            else
                pictDIP4.Image = Resources.off;

            if ((nomber & 0x10) > 0)
                pictDIP5.Image = Resources.on;
            else
                pictDIP5.Image = Resources.off;

            if ((nomber & 0x20) > 0)
                pictDIP6.Image = Resources.on;
            else
                pictDIP6.Image = Resources.off;

            if ((nomber & 0x40) > 0)
                pictDIP7.Image = Resources.on;
            else
                pictDIP7.Image = Resources.off;

            if ((nomber & 0x80) > 0)
                pictDIP8.Image = Resources.on;
            else
                pictDIP8.Image = Resources.off;

            if ((nomber & 0x100) > 0)
                pictDIP9.Image = Resources.on;
            else
                pictDIP9.Image = Resources.off;
        }

        private void cBoxCOMList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        int PERINTERVALTIMER = 20;

        private void butt_X_UPDWN_Click(object sender, EventArgs e)
        {
            foreach(Control cnt in groupBox1.Controls)
            {
                if ("btContrSand" == cnt.Name)
                    cnt.Enabled = true;
                else
                    cnt.Enabled = false;
            }

            tBoxW.Text = tBoxR.Text = tBoxG.Text = tBoxB.Text = "0";

            timer1.Interval = PERINTERVALTIMER;
            timer1.Tag = sender;
            timer1.Enabled = true;
            SendClick();
        }

        bool stepUpDown = true;
        private int STEP = 15;

        private bool orgStep(object sender, bool timerEnable)
        {

                TextBox trackTextBox = (TextBox)sender;
                if (stepUpDown)
                {
                    trackTextBox.Text = ((Convert.ToInt16(trackTextBox.Text)) + STEP).ToString();
                    if (Convert.ToInt16(trackTextBox.Text) >= 255)
                    {
                        trackTextBox.Text = "255";
                        stepUpDown = false;
                    }
                }
                else
                {
                    if ((Convert.ToInt16(trackTextBox.Text) - STEP) > 0)
                        trackTextBox.Text = ((Convert.ToInt16(trackTextBox.Text)) - STEP).ToString();
                    else
                    {
                        trackTextBox.Text = "0";
                        stepUpDown = true;
                        if (!timerEnable)
                        {
                            SendClick();
                            timer1.Enabled = false;
                        }
                        return true;
                    }
                }
            return false;
        }

        private bool orgStep(object[] senders, bool timerEnable)
        {
            foreach (object sender in senders)
            {
                TextBox trackTextBox = (TextBox)sender;
                if (stepUpDown)
                {
                    trackTextBox.Text = ((Convert.ToInt16(trackTextBox.Text)) + STEP).ToString();
                    if (Convert.ToInt16(trackTextBox.Text) >= 255)
                    {
                        trackTextBox.Text = "255";
                        if(sender == senders[senders.Length-1]) stepUpDown = false;
                    }
                }
                else
                {
                    if ((Convert.ToInt16(trackTextBox.Text) - STEP) > 0)
                        trackTextBox.Text = ((Convert.ToInt16(trackTextBox.Text)) - STEP).ToString();
                    else
                    {
                        trackTextBox.Text = "0";
                        if (sender == senders[senders.Length-1]) stepUpDown = true;
                        if (!timerEnable)
                        {
                            SendClick();
                            timer1.Enabled = false;
                        }
                        if (sender == senders[senders.Length-1]) return true;
                    }
                }
            }
            return false;
        }

        int stepAllUPDOWN = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            Button trackBarUser = (Button)timer1.Tag;
            switch (trackBarUser.Name)
            {
                case "buttWUPDWN":
                    orgStep(tBoxW, false);
                    break;
                case "buttRUPDWN":
                    orgStep(tBoxR, false);
                    break;
                case "buttGUPDWN":
                    orgStep(tBoxG, false);
                    break;
                case "buttBUPDWN":
                    orgStep(tBoxB, false);
                    break;
                case "buttAllUPDWN":
                    switch (stepAllUPDOWN) {
                        case 0:
                            if (orgStep(tBoxW, true)) stepAllUPDOWN++;
                            break;
                        case 1:
                            if (orgStep(tBoxB, true)) stepAllUPDOWN++; 
                            break;
                        case 2:
                            if(orgStep(tBoxG, true)) stepAllUPDOWN++;
                            break;
                        case 3:
                            if (orgStep(tBoxR, true)) stepAllUPDOWN++;
                            break;
                        case 4:
                            object[] arrtBox = { tBoxW, tBoxR, tBoxG, tBoxB };
                            if(orgStep(arrtBox, false))
                                stepAllUPDOWN = 0;
                            break;
                        }
                        break;
            }
        }
    }
}
