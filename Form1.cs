using Hastane_Yonetim_Paneli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hastane_Yönetim_Paneli
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += (s, e) => Application.Exit();
            this.MaximizeBox = false;  // Pencereyi büyütme butonunu devre dışı bırak
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Doktorİşlemleri d = new Doktorİşlemleri();

            d.Show();
            this.Hide();

        }

        private void button1_Click(object sender, EventArgs e)
        {
          Hastaİşlemleri h = new Hastaİşlemleri();

            h.Show();
            this.Hide();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            Sekreterİşlemleri s = new Sekreterİşlemleri();

            s.Show();
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
   
}
