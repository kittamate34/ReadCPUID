using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor;

namespace ReadCPUID
{
    public partial class FrmMain : Form
    {
        Util _Util_Class = new Util();
        private DataTable _dtMonitor = new DataTable("Monitor");
        private Label[,] _lbl_Sensor = null;
        private string[,,] _Sensor = null;
        private string[,,] _SensorDef = null;

        private int _GPU_Sensor = 11;
        private int _Mode = 0;

        string _str = string.Empty;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.pal_Monitor.Controls.Clear();

            try
            {
                string ltype = string.Empty;
                int lGPU_Count = 0;

                OpenHardwareMonitor.Hardware.Computer lComputerHardware = new OpenHardwareMonitor.Hardware.Computer();
                lComputerHardware.MainboardEnabled = false;
                lComputerHardware.FanControllerEnabled = false;
                lComputerHardware.CPUEnabled = false;
                lComputerHardware.GPUEnabled = true;
                lComputerHardware.RAMEnabled = false;
                lComputerHardware.HDDEnabled = false;
                lComputerHardware.Open();

                for (int bInxHardWare = 0; bInxHardWare < lComputerHardware.Hardware.Count(); bInxHardWare++)
                {
                    ltype = lComputerHardware.Hardware[bInxHardWare].HardwareType.ToString();

                    if ((!string.IsNullOrEmpty(ltype)) && (ltype.ToUpper().Substring(0, 3) == "GPU")) { lGPU_Count++; }
                }

                this.ReSize_Monitor(lGPU_Count);
                this.BuildLabel(lGPU_Count);
                this.UpdateSensor(lComputerHardware);
                this.UpdateMinMax(0);

                lComputerHardware.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message.ToString(), "Error : FrmMain_Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void ReSize_Monitor(int pCountHardware)
        {
            try
            {
                this.pal_Monitor.Size = new System.Drawing.Size(1 + (pCountHardware * 100), this.pal_Monitor.Size.Height);

                this.ClientSize = new System.Drawing.Size((this.ClientSize.Width - 100) + (pCountHardware * 100), this.ClientSize.Height);
                this.MaximumSize = new System.Drawing.Size((this.MaximumSize.Width - 100) + (pCountHardware * 100), this.MaximumSize.Height);
                this.MinimumSize = new System.Drawing.Size((this.MinimumSize.Width - 100) + (pCountHardware * 100), this.MinimumSize.Height);
                this.Refresh();

                /*
                 * this.ClientSize = new System.Drawing.Size(260, 194);
                 * 
                 *this.MaximumSize = new System.Drawing.Size(276, 232);
                 *this.MinimumSize = new System.Drawing.Size(276, 232);
                 */
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message.ToString(), "Error : Hardware_Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void BuildLabel(int pCountHardware)
        {
            try
            {
                this._lbl_Sensor = new Label[pCountHardware, this._GPU_Sensor];
                this._Sensor = new string[3, pCountHardware, this._GPU_Sensor];

                for (int bInxHardWare = 0;bInxHardWare < pCountHardware; bInxHardWare++)
                {
                    for (int bInxSensor = 0; bInxSensor < this._GPU_Sensor; bInxSensor++)
                    {
                        this._lbl_Sensor[bInxHardWare, bInxSensor] = new Label();

                        this._lbl_Sensor[bInxHardWare, bInxSensor].Location = new System.Drawing.Point(bInxHardWare * 100, (bInxSensor * 17) - 1);



                        this._lbl_Sensor[bInxHardWare, bInxSensor].Size = new System.Drawing.Size(100, 18);
                        this._lbl_Sensor[bInxHardWare, bInxSensor].Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
                        this._lbl_Sensor[bInxHardWare, bInxSensor].TabIndex = Convert.ToInt32("10" + string.Format("{0:00}", bInxHardWare) + string.Format("{0:00}", bInxSensor));
                        this._lbl_Sensor[bInxHardWare, bInxSensor].Name = "_lbl_Sensor_10" + string.Format("{0:00}", bInxHardWare) + string.Format("{0:00}", bInxSensor);
                        this._lbl_Sensor[bInxHardWare, bInxSensor].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        this._lbl_Sensor[bInxHardWare, bInxSensor].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this._lbl_Sensor[bInxHardWare, bInxSensor].Text = "-";

                        if (bInxSensor != 0) { this._lbl_Sensor[bInxHardWare, bInxSensor].MouseClick += new System.Windows.Forms.MouseEventHandler(this.lbl_MinMax); }                        

                        this.pal_Monitor.Controls.Add(this._lbl_Sensor[bInxHardWare, bInxSensor]);

                        _Sensor[0, bInxHardWare, bInxSensor] = "-";
                        _Sensor[1, bInxHardWare, bInxSensor] = "-";
                        _Sensor[2, bInxHardWare, bInxSensor] = "-";

                        this._SensorDef = this._Sensor;
                    }
                }

                Application.DoEvents();

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message.ToString(), "Error : BuildLabel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }

        private void UpdateSensor(OpenHardwareMonitor.Hardware.Computer pComputerHardware)
        {
            try
            {
                int lGPU_Index = 0;

                for (int bInxHardWare = 0; bInxHardWare < pComputerHardware.Hardware.Count(); bInxHardWare++)
                {
                    string lType = pComputerHardware.Hardware[bInxHardWare].HardwareType.ToString();

                    if ((!string.IsNullOrEmpty(lType)) && (lType.ToUpper().Substring(0, 3) == "GPU"))
                    {
                        this._lbl_Sensor[lGPU_Index, 0].Text = pComputerHardware.Hardware[bInxHardWare].Name.Replace("NVIDIA GeForce ", null);

                        for (int bInxSensor = 0; bInxSensor < pComputerHardware.Hardware[bInxHardWare].Sensors.Count(); bInxSensor++)
                        {

                            string bName = pComputerHardware.Hardware[bInxHardWare].Sensors[bInxSensor].Name.ToUpper().Replace(" ", null);
                            string bValue = pComputerHardware.Hardware[bInxHardWare].Sensors[bInxSensor].Value.ToString().ToUpper().Replace(" ", null);
                            string bSensorType = pComputerHardware.Hardware[bInxHardWare].Sensors[bInxSensor].SensorType.ToString().ToUpper().Replace(" ", null);

                            //this._str += "Name : " + bName + " | Value : " + bValue + " | SensorType : " + bSensorType + Environment.NewLine;

                            if ((bName == "GPUCORE") && (bSensorType == "TEMPERATURE"))
                            {
                                this._Sensor[0, lGPU_Index, 1] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 1],100)) { this._Sensor[1, lGPU_Index, 1] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 1],0)) { this._Sensor[2, lGPU_Index, 1] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 1].Text = this._Sensor[this._Mode, lGPU_Index, 1] + " C";
                            }
                            else if ((bName == "GPUCORE") && (bSensorType == "CLOCK"))
                            {
                                this._Sensor[0, lGPU_Index, 2] = bValue;
                                if (_Util_Class.Str2Double(bValue) <= _Util_Class.Str2Double(this._Sensor[1, lGPU_Index, 2], 9999.0)) { this._Sensor[1, lGPU_Index, 2] = bValue; }
                                if (_Util_Class.Str2Double(bValue) >= _Util_Class.Str2Double(this._Sensor[2, lGPU_Index, 2], 0.0)) { this._Sensor[2, lGPU_Index, 2] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 2].Text = Convert.ToDouble(this._Sensor[this._Mode, lGPU_Index, 2]).ToString("F2") + " Mhz";
                            }
                            else if ((bName == "GPUMEMORY") && (bSensorType == "CLOCK"))
                            {
                                this._Sensor[0, lGPU_Index, 3] = bValue;
                                if (_Util_Class.Str2Double(bValue) <= _Util_Class.Str2Double(this._Sensor[1, lGPU_Index, 3], 9999.0)) { this._Sensor[1, lGPU_Index, 3] = bValue; }
                                if (_Util_Class.Str2Double(bValue) >= _Util_Class.Str2Double(this._Sensor[2, lGPU_Index, 3], 0.0)) { this._Sensor[2, lGPU_Index, 3] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 3].Text = Convert.ToDouble(this._Sensor[this._Mode, lGPU_Index, 3]).ToString("F2") + " Mhz";
                            }
                            else if ((bName == "GPUSHADER") && (bSensorType == "CLOCK"))
                            {
                                this._Sensor[0, lGPU_Index, 4] = bValue;
                                if (_Util_Class.Str2Double(bValue) <= _Util_Class.Str2Double(this._Sensor[1, lGPU_Index, 4], 9999.0)) { this._Sensor[1, lGPU_Index, 4] = bValue; }
                                if (_Util_Class.Str2Double(bValue) >= _Util_Class.Str2Double(this._Sensor[2, lGPU_Index, 4], 0.0)) { this._Sensor[2, lGPU_Index, 4] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 4].Text = Convert.ToDouble(this._Sensor[this._Mode, lGPU_Index, 4]).ToString("F2") + " Mhz";
                            }
                            else if ((bName == "GPUCORE") && (bSensorType == "LOAD"))
                            {
                                this._Sensor[0, lGPU_Index, 5] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 5], 100)) { this._Sensor[1, lGPU_Index, 5] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 5], 0)) { this._Sensor[2, lGPU_Index, 5] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 5].Text = this._Sensor[this._Mode, lGPU_Index, 5] + " %";
                            }
                            else if ((bName == "GPUMEMORYCONTROLLER") && (bSensorType == "LOAD"))
                            {
                                this._Sensor[0, lGPU_Index, 6] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 6], 100)) { this._Sensor[1, lGPU_Index, 6] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 6], 0)) { this._Sensor[2, lGPU_Index, 6] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 6].Text = this._Sensor[this._Mode, lGPU_Index, 6] + " %";
                            }
                            else if ((bName == "GPUVIDEOENGINE") && (bSensorType == "LOAD"))
                            {
                                this._Sensor[0, lGPU_Index, 7] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 7], 100)) { this._Sensor[1, lGPU_Index, 7] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 7], 0)) { this._Sensor[2, lGPU_Index, 7] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 7].Text = this._Sensor[this._Mode, lGPU_Index, 7] + " %";
                            }
                            else if ((bName == "GPUMEMORY") && (bSensorType == "LOAD"))
                            {
                                this._Sensor[0, lGPU_Index, 8] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 8], 100)) { this._Sensor[1, lGPU_Index, 8] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 8], 0)) { this._Sensor[2, lGPU_Index, 8] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 8].Text = this._Sensor[this._Mode, lGPU_Index, 8] + " %";
                            }
                            else if ((bName == "GPU") && (bSensorType == "FAN"))
                            {
                                this._Sensor[0, lGPU_Index, 9] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 9], 10000)) { this._Sensor[1, lGPU_Index, 9] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 9], 0)) { this._Sensor[2, lGPU_Index, 9] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 9].Text = this._Sensor[this._Mode, lGPU_Index, 9] + " RPM";
                            }
                            else if ((bName == "GPUFAN") && (bSensorType == "CONTROL"))
                            {
                                this._Sensor[0, lGPU_Index, 10] = bValue;
                                if (_Util_Class.Str2Int(bValue) <= _Util_Class.Str2Int(this._Sensor[1, lGPU_Index, 10], 100)) { this._Sensor[1, lGPU_Index, 10] = bValue; }
                                if (_Util_Class.Str2Int(bValue) >= _Util_Class.Str2Int(this._Sensor[2, lGPU_Index, 10], 0)) { this._Sensor[2, lGPU_Index, 10] = bValue; }
                                this._lbl_Sensor[lGPU_Index, 10].Text = this._Sensor[this._Mode, lGPU_Index, 10] + " %";
                            }

                            //if ((bName == "GPUCORE") && (bSensorType == "TEMPERATURE")) { this._lbl_Sensor[lGPU_Index, 1].Text = bValue + " C"; }
                            //else if ((bName == "GPUCORE") && (bSensorType == "CLOCK")) { this._lbl_Sensor[lGPU_Index, 2].Text = Convert.ToDouble(bValue).ToString("F2") + " Mhz"; }
                            //else if ((bName == "GPUMEMORY") && (bSensorType == "CLOCK")) { this._lbl_Sensor[lGPU_Index, 3].Text = Convert.ToDouble(bValue).ToString("F2") + " Mhz"; }
                            //else if ((bName == "GPUSHADER") && (bSensorType == "CLOCK")) { this._lbl_Sensor[lGPU_Index, 4].Text = Convert.ToDouble(bValue).ToString("F2") + " Mhz"; }
                            //else if ((bName == "GPUCORE") && (bSensorType == "LOAD")) { this._lbl_Sensor[lGPU_Index, 5].Text = bValue + " %"; }
                            //else if ((bName == "GPUMEMORYCONTROLLER") && (bSensorType == "LOAD")) { this._lbl_Sensor[lGPU_Index, 6].Text = bValue + " %"; }
                            //else if ((bName == "GPUVIDEOENGINE") && (bSensorType == "LOAD")) { this._lbl_Sensor[lGPU_Index, 7].Text = bValue + " %"; }
                            //else if ((bName == "GPUMEMORY") && (bSensorType == "LOAD")) { this._lbl_Sensor[lGPU_Index, 8].Text = bValue + " %"; }
                            //else if ((bName == "GPU") && (bSensorType == "FAN")) { this._lbl_Sensor[lGPU_Index, 9].Text = bValue + " RPM"; }
                            //else if ((bName == "GPUFAN") && (bSensorType == "CONTROL")) { this._lbl_Sensor[lGPU_Index, 10].Text = bValue + " %"; }
                        }

                        lGPU_Index++;
                    }
                }

                //string teststr = this._str;
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(this, ex.Message.ToString(), "Error : UpdateLabel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(0);
            }
        }

        private void lbl_MinMax(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    this._Mode++;
                    if (this._Mode > 2) { this._Mode = 0; }
                    this.UpdateMinMax(this._Mode);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    this._Sensor = this._SensorDef;
                }
            }
            catch
            {

            }
        }

        private void UpdateMinMax(int pMode)
        {
            try
            {
                string lstr = "";
                Color lColor = Color.WhiteSmoke;
                bool lVisible = true;

                if (pMode == 0)
                {
                    lstr = "";
                    lColor = Color.WhiteSmoke;
                    lVisible = false;
                }
                else if (pMode == 1)
                {
                    lstr = "Min";
                    lColor = Color.LimeGreen;
                    lVisible = true;
                }
                else if (pMode == 2)
                {
                    lstr = "Max";
                    lColor = Color.Crimson;
                    lVisible = true;
                }

                this.lbl_MinMax_01.Text = lstr;
                this.lbl_MinMax_02.Text = lstr;
                this.lbl_MinMax_03.Text = lstr;
                this.lbl_MinMax_04.Text = lstr;
                this.lbl_MinMax_05.Text = lstr;
                this.lbl_MinMax_06.Text = lstr;
                this.lbl_MinMax_07.Text = lstr;
                this.lbl_MinMax_08.Text = lstr;
                this.lbl_MinMax_09.Text = lstr;
                this.lbl_MinMax_10.Text = lstr;

                this.lbl_MinMax_01.ForeColor = lColor;
                this.lbl_MinMax_02.ForeColor = lColor;
                this.lbl_MinMax_03.ForeColor = lColor;
                this.lbl_MinMax_04.ForeColor = lColor;
                this.lbl_MinMax_05.ForeColor = lColor;
                this.lbl_MinMax_06.ForeColor = lColor;
                this.lbl_MinMax_07.ForeColor = lColor;
                this.lbl_MinMax_08.ForeColor = lColor;
                this.lbl_MinMax_09.ForeColor = lColor;
                this.lbl_MinMax_10.ForeColor = lColor;

                this.lbl_MinMax_01.Visible = lVisible;
                this.lbl_MinMax_02.Visible = lVisible;
                this.lbl_MinMax_03.Visible = lVisible;
                this.lbl_MinMax_04.Visible = lVisible;
                this.lbl_MinMax_05.Visible = lVisible;
                this.lbl_MinMax_06.Visible = lVisible;
                this.lbl_MinMax_07.Visible = lVisible;
                this.lbl_MinMax_08.Visible = lVisible;
                this.lbl_MinMax_09.Visible = lVisible;
                this.lbl_MinMax_10.Visible = lVisible;
            }
            catch
            {

            }
        }

        private void Thread_HardwareMonitor()
        {
            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    OpenHardwareMonitor.Hardware.Computer lComputerHardware = new OpenHardwareMonitor.Hardware.Computer();
                    lComputerHardware.MainboardEnabled = false;
                    lComputerHardware.FanControllerEnabled = false;
                    lComputerHardware.CPUEnabled = false;
                    lComputerHardware.GPUEnabled = true;
                    lComputerHardware.RAMEnabled = false;
                    lComputerHardware.HDDEnabled = false;
                    lComputerHardware.Open();

                    this.UpdateSensor(lComputerHardware);

                    lComputerHardware.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message.ToString(), "Error : Thread_HardwareMonitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            try
            {
                System.Threading.Thread lThread_Monitor = new System.Threading.Thread(this.Thread_HardwareMonitor);
                lThread_Monitor.Priority = ThreadPriority.AboveNormal;
                lThread_Monitor.IsBackground = true;
                lThread_Monitor.Name = "lThread_Monitor";
                lThread_Monitor.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message.ToString(), "Error : FrmMain_Shown", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
    }
}
