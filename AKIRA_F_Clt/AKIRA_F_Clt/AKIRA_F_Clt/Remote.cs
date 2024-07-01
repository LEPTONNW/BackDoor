using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AKIRA_F_Clt
{
    public partial class Remote : Form
    {
        public Remote()
        {
            InitializeComponent();
        }

        public static string Passvalue2 { get; set; }

        private void Remote_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Passvalue2 = "/help";
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Passvalue2 = "STT$:";
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Passvalue2 = "STD$:";
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Passvalue2 = "KLO$:";
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Passvalue2 = "KLN$:";
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Passvalue2 = "RMT$:";
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Passvalue2 = "RMN$:";
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Passvalue2 = "CAP$:";
            this.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Passvalue2 = "KILL$:";
            this.Close();
        }
    }
}
