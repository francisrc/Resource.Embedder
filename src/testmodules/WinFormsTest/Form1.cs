using System;
using System.Windows.Forms;

namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var helper = new JitDelayHelper();
            helper.Run(str =>
            {
                Action set = () => lblLocalized.Text = str;

                if (lblLocalized.InvokeRequired)
                {
                    lblLocalized.Invoke(set);
                }
                else
                {
                    set();
                }
            });
        }
    }
}
