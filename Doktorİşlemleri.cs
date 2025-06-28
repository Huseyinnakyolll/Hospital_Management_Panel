using Hastane_Yönetim_Paneli.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Hastane_Yönetim_Paneli
{
    public partial class Doktorİşlemleri : Form
    {
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Hastana_Yönetimi;Integrated Security=True";
        public Doktorİşlemleri()
        {
            InitializeComponent();
            this.FormClosed += (s, e) => Application.Exit();
            this.MaximizeBox = false;  // Pencereyi büyütme butonunu devre dışı bırak
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

        }


        private void Doktorİşlemleri_Load(object sender, EventArgs e)
        {
            LoadDepartments();
            LoadDoktorlar();
            LoadRandevular();
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;  // sadece 1 satır seçilebilir

        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            this.Hide();
            form.Show();

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
            if (comboBox1.SelectedValue == null || comboBox1.SelectedValue.ToString() == "System.Data.DataRowView")
                return;

            int selectedDepartmentId = Convert.ToInt32(comboBox1.SelectedValue);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Sadece uniq ünvanları çek
                    string query = "SELECT DISTINCT Ünvan FROM DoktorTb WHERE Bölümİd = @bolumId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bolumId", selectedDepartmentId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox2.DisplayMember = "Ünvan";
                    comboBox2.ValueMember = "Ünvan";
                    comboBox2.DataSource = dt;

                    // Manuel yazmaya izin ver
                    comboBox2.DropDownStyle = ComboBoxStyle.DropDown;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ünvanlar yüklenemedi: " + ex.Message);
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

                    dataGridView2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevular yüklenemedi: " + ex.Message);
            }
        }





        private void LoadDoktorlar()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT DoktorAd,DoktorSoyAd, Bölüm, Ünvan FROM DoktorTb";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bilgiler Yüklenmedi: " + ex.Message);
            }
        }

        private void btnRandevuAl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(comboBox1.Text) || string.IsNullOrWhiteSpace(comboBox2.Text) ||
                comboBox1.SelectedItem == null )
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO DoktorTb
                (DoktorAd, DoktorSoyAd , Bölüm,Ünvan)
                VALUES (@DoktorAd, @DoktorSoyAd, @Bölüm, @Ünvan)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@DoktorAd", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@DoktorSoyAd", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@Bölüm", comboBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@Ünvan", comboBox2.Text.Trim());
                    cmd.ExecuteNonQuery();
                }


                MessageBox.Show("Randevu başarıyla alındı.");
                LoadRandevular();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevu alınırken hata oluştu: " + ex.Message);
            }
        }

        private void btnDoktorSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen silmek için bir doktor seçin.");
                return;
            }
                // Seçili satırdaki doktorun bilgilerini al
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            // Burada benzersiz bir sütun yoksa ad-soyad ile deniyoruz
            string doktorAd = selectedRow.Cells["DoktorAd"].Value.ToString();
            string doktorSoyAd = selectedRow.Cells["DoktorSoyAd"].Value.ToString();

            // Silme işlemini onaylatmak iyi olur
            DialogResult dr = MessageBox.Show($"{doktorAd} {doktorSoyAd} adlı doktoru silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Eğer tabloda ID varsa onu kullan, yoksa ad-soyad ile sil
                    string query = "DELETE FROM DoktorTb WHERE DoktorAd = @ad AND DoktorSoyAd = @soyad";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ad", doktorAd);
                    cmd.Parameters.AddWithValue("@soyad", doktorSoyAd);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Doktor başarıyla silindi.");
                        LoadDoktorlar(); // Listeyi güncelle
                    }
                    else
                    {
                        MessageBox.Show("Doktor bulunamadı veya silinemedi.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme işlemi sırasında hata oluştu: " + ex.Message);
            }
        }




        private void doktorevents(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count)
                return;

            DataGridViewRow row = dataGridView1.Rows[rowIndex];

            textBox1.Text = row.Cells["DoktorAd"].Value?.ToString() ?? "";
            textBox2.Text = row.Cells["DoktorSoyAd"].Value?.ToString() ?? "";
            comboBox1.Text = row.Cells["Bölüm"].Value?.ToString() ?? "";
            comboBox2.Text = row.Cells["Ünvan"].Value?.ToString() ?? "";
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            doktorevents(e.RowIndex);
        }



        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
                doktorevents(dataGridView1.CurrentRow.Index);
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
                doktorevents(dataGridView1.CurrentRow.Index);
        }
        private void Temizle()
        {
            textBox1.Clear();
            textBox2.Clear();
            comboBox2.DataSource = null;
     
        }


        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

      

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

      
    }
}
