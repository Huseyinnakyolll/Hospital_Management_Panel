using Hastane_Yönetim_Paneli;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Hastane_Yonetim_Paneli
{
    public partial class Hastaİşlemleri : Form
    {
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Hastana_Yönetimi;Integrated Security=True";

        public Hastaİşlemleri()
        {
            InitializeComponent();
            this.MaximizeBox = false;  // Pencereyi büyütme butonunu devre dışı bırak
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormClosed += (s, e) => Application.Exit();
        }

        private void Hastaİşlemleri_Load(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadRandevular();
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            // Etiketleri boş başlat
            label8.Text = "TCNo: ";
            label9.Text = "Ad: ";
            label10.Text = "Doktor: ";
            label11.Text = "Department: ";
        }



        private bool poliklinlikYuklendi = false;

        private void LoadDepartments()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, DepartmentName FROM Department";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBox1.DisplayMember = "DepartmentName";
                comboBox1.ValueMember = "Id";
                comboBox1.DataSource = dt;
                poliklinlikYuklendi = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // combobox henüz veri dolmadan tetiklenirse hata vermemesi için kontrol
            if (comboBox1.SelectedValue == null || comboBox1.SelectedValue.ToString() == "System.Data.DataRowView")
                return;

            // Seçilen polikinliğin Id'sini alıyoruz (DepartmentId)
            int selectedDepartmentId = Convert.ToInt32(comboBox1.SelectedValue);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT Ünvan + ' ' + DoktorAd + ' ' + DoktorSoyAd AS DoktorAdSoyad 
                             FROM DoktorTb 
                             WHERE Bölümİd = @bolumId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bolumId", selectedDepartmentId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox2.DisplayMember = "DoktorAdSoyad";
                    comboBox2.ValueMember = "DoktorAdSoyad"; // istersen id yapabilirsin
                    comboBox2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Doktorlar yüklenemedi: " + ex.Message);
            }
        }


        private void btnRandevuAl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) || !maskedTextBox1.MaskFull ||
                comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO HastaTb 
                (TcNo, Ad, SoyAd, Telefon, Polikinlik, Doktor, Tarih)
                VALUES (@TcNo, @Ad, @SoyAd, @Telefon, @Polikinlik, @Doktor, @Tarih)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TcNo", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@Ad", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@SoyAd", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@Telefon", maskedTextBox1.Text);
                    cmd.Parameters.AddWithValue("@Polikinlik", comboBox1.Text);
                    cmd.Parameters.AddWithValue("@Doktor", comboBox2.Text);
                    cmd.Parameters.AddWithValue("@Tarih", dateTimePicker1.Value);

                    cmd.ExecuteNonQuery();
                }

                // Bilgileri static class'a aktar
                bilgi.tckimlik = textBox1.Text.Trim();
                bilgi.ad = textBox2.Text.Trim();
                bilgi.doktorad = comboBox2.Text;
                bilgi.department = comboBox1.Text;
                bilgi.soyoad= textBox3.Text.Trim();

                // Etiketleri güncelle
                label8.Text = "TCNo: " + bilgi.tckimlik;
                label9.Text = "Ad: " + bilgi.ad;
                label10.Text = "Doktor: " + bilgi.doktorad;
                label11.Text = "Department: " + bilgi.department;
                label12.Text ="SoyAd:"+bilgi.soyoad;

                MessageBox.Show("Randevu başarıyla alındı.");
                LoadRandevular();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevu alınırken hata oluştu: " + ex.Message);
            }
        }


        private void LoadRandevular()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT TcNo, Ad, SoyAd, Telefon, Polikinlik, Doktor, Tarih FROM HastaTb";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevular yüklenemedi: " + ex.Message);
            }
        }



        private void datagridshowevents(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count)
                return;

            DataGridViewRow row = dataGridView1.Rows[rowIndex];

            // Null kontrolleri eklendi, eğer hücre boşsa boş string ata
            bilgi.tckimlik = row.Cells["TcNo"].Value != null ? row.Cells["TcNo"].Value.ToString() : "";
            bilgi.ad = row.Cells["Ad"].Value != null ? row.Cells["Ad"].Value.ToString() : "";
            bilgi.doktorad = row.Cells["Doktor"].Value != null ? row.Cells["Doktor"].Value.ToString() : "";
            bilgi.department = row.Cells["Polikinlik"].Value != null ? row.Cells["Polikinlik"].Value.ToString() : "";
            bilgi.soyoad = row.Cells["SoyAd"].Value != null ? row.Cells["SoyAd"].Value.ToString() : "";

            label8.Text = "TCNO: " + bilgi.tckimlik;
            label9.Text = "Ad: " + bilgi.ad;
            label10.Text = "Doktor: " + bilgi.doktorad;
            label11.Text = "Department: " + bilgi.department;
            label12.Text =  "SoyAd:" + bilgi.soyoad;
        }


        private void Temizle()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.DataSource = null;
            dateTimePicker1.Value = DateTime.Now;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            this.Hide();
            form.Show();
        }

        private void label9_Click(object sender, EventArgs e)
        {


        }
        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                datagridshowevents(e.RowIndex);
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                datagridshowevents(e.RowIndex);
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
                datagridshowevents(e.RowIndex);
        }



    }
}
