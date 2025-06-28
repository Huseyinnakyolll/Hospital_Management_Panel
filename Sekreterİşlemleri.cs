using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Hastane_Yönetim_Paneli.Classes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System;

namespace Hastane_Yönetim_Paneli
{
    public partial class Sekreterİşlemleri : Form
    {
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Hastana_Yönetimi;Integrated Security=True";

        public Sekreterİşlemleri()
        {
            InitializeComponent();

            this.FormClosed += (s, e) => Application.Exit();
            this.MaximizeBox = false;  // Pencereyi büyütme butonunu devre dışı bırak
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            this.Hide();
            form.Show();
        }

        private void Sekreterİşlemleri_Load(object sender, EventArgs e)
        {
            textBox7.UseSystemPasswordChar = true;
            LoadDepartments();
        }

        // Randevu alma için poliklinikleri yükle
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
            }
        }

        // Poliklinik seçildiğinde doktorları yükle
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

                    string query = @"SELECT Ünvan + ' ' + DoktorAd + ' ' + DoktorSoyAd AS DoktorAdSoyad 
                             FROM DoktorTb 
                             WHERE Bölümİd = @bolumId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@bolumId", selectedDepartmentId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboBox2.DisplayMember = "DoktorAdSoyad";
                    comboBox2.ValueMember = "DoktorAdSoyad";
                    comboBox2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Doktorlar yüklenemedi: " + ex.Message);
            }
        }

        // Randevu alma butonu
        private void button5_Click(object sender, EventArgs e)
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

                MessageBox.Show("Randevu başarıyla alındı.");
                LoadRandevular();
                TemizleRandevuAlanlari();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevu alınırken hata oluştu: " + ex.Message);
            }
        }

        // Randevu alanlarını temizle
        private void TemizleRandevuAlanlari()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            maskedTextBox1.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.DataSource = null;
            dateTimePicker1.Value = DateTime.Now;
        }

        // Randevuları yükle
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

        // Listele butonu
        private void button3_Click(object sender, EventArgs e)
        {
            SqlCommand commandList = new SqlCommand("Select * from DoktorTb ", SqlOperations.connection);

            SqlOperations.connection.Open();

            SqlDataAdapter da = new SqlDataAdapter(commandList);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;

            SqlOperations.connection.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            string ad = textBox6.Text.Trim();
            string soyad = textBox5.Text.Trim();
            string sifre = textBox7.Text.Trim();

            SqlCommand kontrolKomut = new SqlCommand(
                "SELECT * FROM ScTb WHERE ad = @ad AND soyad = @soyad AND Şifre = @sifre",
                SqlOperations.connection);
            kontrolKomut.Parameters.AddWithValue("@ad", ad);
            kontrolKomut.Parameters.AddWithValue("@soyad", soyad);
            kontrolKomut.Parameters.AddWithValue("@sifre", sifre);

            SqlOperations.connection.Open();
            SqlDataReader reader = kontrolKomut.ExecuteReader();

            if (reader.Read())
            {
                reader.Close();

                // Sekreterleri listele - dataGridView3
                SqlCommand sekreterKomut = new SqlCommand("SELECT * FROM ScTb", SqlOperations.connection);
                SqlDataAdapter daSekreter = new SqlDataAdapter(sekreterKomut);
                DataTable dtSekreter = new DataTable();
                daSekreter.Fill(dtSekreter);
                dataGridView3.DataSource = dtSekreter;

                // Doktorları listele - dataGridView1
                SqlCommand doktorKomut = new SqlCommand("SELECT DoktorAd, DoktorSoyAd, Bölüm, Ünvan FROM DoktorTb", SqlOperations.connection);
                SqlDataAdapter daDoktor = new SqlDataAdapter(doktorKomut);
                DataTable dtDoktor = new DataTable();
                daDoktor.Fill(dtDoktor);
                dataGridView1.DataSource = dtDoktor;

                // Hastaları listele - dataGridView2
                SqlCommand hastaKomut = new SqlCommand("SELECT * FROM HastaTb", SqlOperations.connection);
                SqlDataAdapter daHasta = new SqlDataAdapter(hastaKomut);
                DataTable dtHasta = new DataTable();
                daHasta.Fill(dtHasta);
                dataGridView2.DataSource = dtHasta;

                // Randevuları da yükle
                LoadRandevular();

                MessageBox.Show("Giriş başarılı, tüm veriler yüklendi.");
            }
            else
            {
                reader.Close();
                MessageBox.Show("Ad, soyad veya şifre hatalı.");
            }

            SqlOperations.connection.Close();
            textBox7.Clear();
            textBox5.Clear();
            textBox6.Clear();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ad = textBox6.Text.Trim();
            string soyad = textBox5.Text.Trim();
            string sifre = textBox7.Text.Trim();

            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) || string.IsNullOrWhiteSpace(sifre))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            try
            {
                // Bağlantıyı aç
                if (SqlOperations.connection.State == ConnectionState.Closed)
                    SqlOperations.connection.Open();

                // SQL komutu
                SqlCommand command = new SqlCommand("INSERT INTO ScTb (Ad, SoyAd, Şifre) VALUES (@ad, @soyad, @sifre)", SqlOperations.connection);
                command.Parameters.AddWithValue("@ad", ad);
                command.Parameters.AddWithValue("@soyad", soyad);
                command.Parameters.AddWithValue("@sifre", Convert.ToInt32(sifre)); // çünkü Şifre int 

                command.ExecuteNonQuery();
                MessageBox.Show("Sekreter başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
            finally
            {
                SqlOperations.connection.Close();
            }

            // TextBox temizle
            textBox6.Clear();
            textBox5.Clear();
            textBox7.Clear();
        }

        private int secilenId = -1;

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
                secilenId = Convert.ToInt32(row.Cells["id"].Value);

                // İstersen diğer textboxları doldurabilirsin
                textBox6.Text = row.Cells["Ad"].Value.ToString();
                textBox5.Text = row.Cells["SoyAd"].Value.ToString();
                textBox7.Text = row.Cells["Şifre"].Value.ToString();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (secilenId == -1)
            {
                MessageBox.Show("Lütfen silmek için bir kişi seçin.");
                return;
            }

            try
            {
                SqlCommand command = new SqlCommand("DELETE FROM ScTb WHERE id = @id", SqlOperations.connection);
                command.Parameters.AddWithValue("@id", secilenId);

                SqlOperations.connection.Open();
                int affectedRows = command.ExecuteNonQuery();
                SqlOperations.connection.Close();

                if (affectedRows > 0)
                {
                    MessageBox.Show("Kişi başarıyla silindi.");

                    // Veri tabanını tekrar yükle
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM ScTb", SqlOperations.connection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView3.DataSource = dt;

                    // TextBoxları temizle
                    textBox6.Clear();
                    textBox5.Clear();
                    textBox7.Clear();

                    secilenId = -1; // resetle
                }
                else
                {
                    MessageBox.Show("Silinecek kişi bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                if (SqlOperations.connection.State == ConnectionState.Open)
                    SqlOperations.connection.Close();
            }
        }
    }
}