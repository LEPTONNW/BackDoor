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
    public partial class InputST : Form
    {
        public InputST()
        {
            InitializeComponent();
        }

        public static string Passvalue { get; set; }
        private void InputST_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Passvalue = textBox1.Text;
            textBox1.Text = "";
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Passvalue = "CANC$";
            textBox1.Text = "";
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }
    }
}
