using Phoenix3D.Properties;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Phoenix3D.Optimization
{
    
    public class LightCallback : GRBCallback
    {
        public List<Tuple<double, double>> LowerBounds = new List<Tuple<double, double>>();
        public List<Tuple<double, double>> UpperBounds = new List<Tuple<double, double>>();
        public double lasttime = 0;

        [DllImport("user32.dll")]
        static extern ushort GetAsyncKeyState(int vKey);
        public static bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (GetAsyncKeyState((int)vKey) & 0x8000);
        }

        public LightCallback()
        {

        }

        protected override void Callback()
        {
            if (IsKeyPushedDown(Keys.F1))
            {
                Abort();
            }
            try
            {
                if (where == GRB.Callback.MIPNODE)
                {
                    double t = GetDoubleInfo(GRB.Callback.RUNTIME);
                    if (t < 10 && t - lasttime > 0.1)
                    {
                        LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBND)));
                        UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBST)));
                        lasttime = t;
                    }
                    else if (t >= 10 && t - lasttime > 3)
                    {
                        LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBND)));
                        UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBST)));
                        lasttime = t;
                    }
                }
                else if (where == GRB.Callback.MIPSOL)
                {
                    double t = GetDoubleInfo(GRB.Callback.RUNTIME);
                    LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPSOL_OBJBND)));
                    UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPSOL_OBJBST)));
                    lasttime = t;
                }
            }
            catch (GRBException e)
            {
                throw new System.InvalidOperationException(e.Message);
            }
        }
    }

    
    public class LogCallback : LightCallback
    {
        public MyLog OutputForm { get; private set; }
        public FormStartPosition Pos { get; private set; }
        public Point Location { get; private set; }
        public Label feedbackLabel { get; private set; }
        public MyListBox LB { get; private set; }


        public LogCallback(FormStartPosition StartPos = FormStartPosition.Manual, Point Location = new Point(), string LogFormName = "MyLogFormName")
        {
            OutputForm = new MyLog(StartPos, Location, LogFormName);
            LB = new MyListBox(OutputForm.Width - 20, OutputForm.Height - 40);
            OutputForm.Controls.Add(LB);
            OutputForm.Dock = DockStyle.Left;
            OutputForm.Show();
            OutputForm.Refresh();
            OutputForm.BringToFront();
        }

        protected override void Callback()
        {
            if (IsKeyPushedDown(Keys.F1))
            {
                Abort();
            }

            try
            {
                if (where == GRB.Callback.MESSAGE)
                {
                    LB.Items.Add(GetStringInfo(GRB.Callback.MSG_STRING));
                    if (LB.Items.Count > 1000)
                    {
                        LB.Items.RemoveAt(0);
                    }
                    LB.SelectedIndex = LB.Items.Count - 1;
                    LB.ClearSelected();
                }
                else if (where == GRB.Callback.MIPNODE)
                {
                    double t = GetDoubleInfo(GRB.Callback.RUNTIME);
                    if (t < 10 && t - lasttime > 0.1)
                    {
                        LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBND)));
                        UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBST)));
                        lasttime = t;
                    }
                    else if (t >= 10 && t - lasttime > 3)
                    {
                        LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBND)));
                        UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPNODE_OBJBST)));
                        lasttime = t;
                    }
                }
                else if (where == GRB.Callback.MIPSOL)
                {
                    double t = GetDoubleInfo(GRB.Callback.RUNTIME);
                    LowerBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPSOL_OBJBND)));
                    UpperBounds.Add(new Tuple<double, double>(t, GetDoubleInfo(GRB.Callback.MIPSOL_OBJBST)));
                    lasttime = t;
                }
            }

            catch (GRBException e)
            {
                LB.Items.Add("Error code: " + e.ErrorCode.ToString());
                LB.Items.Add(e.Message);
                LB.Items.Add(e.StackTrace);
            }
            catch (Exception e)
            {
                LB.Items.Add("Error during callback");
                LB.Items.Add(e.StackTrace);
            }
        }
    }

    
    public class MyListBox : ListBox
    {
        public bool ESCKeyPressed = false;
        public MyListBox(int Width, int Height)
        {
            this.HorizontalScrollbar = true;
            this.Width = Width;
            this.Height = Height;
            this.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left);
            this.ResizeRedraw = true;
            this.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
        }
    }

    
    public class MyLog : Form
    {
        public bool ESCKeyPressed = false;
        public MyLog(FormStartPosition StartPos = FormStartPosition.Manual, Point Location = new Point(), string LogFormName = "MyLogFormName")
        {
            this.ESCKeyPressed = false;
            this.Name = LogFormName;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.SizeGripStyle = SizeGripStyle.Auto;
            this.Width = 500;
            this.Height = 650;
            //this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            //this.TopMost = true;
            this.KeyPreview = true;
            this.StartPosition = StartPos;
            this.Location = Location;
            this.Icon = Resources.ConsoleIcon;
            //this.ResizeRedraw = true;
            this.Text = "Gurobi Optimization Log. Form Name: " + Name + ". Press [F1] to abort optimization";
            this.BringToFront();
        }
    }
}
