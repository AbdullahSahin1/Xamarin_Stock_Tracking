using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using muammerOrnek;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;





namespace muammerOrnek
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<StokModel> _stokListesi = new ObservableCollection<StokModel>();

        public MainPage()
        {
            InitializeComponent();
            StokCollectionView.ItemsSource = _stokListesi; // CollectionView ile listeyi bağlama

        }

        static SQLiteConnection bag = new SQLiteConnection(@"Data Source=C:\Users\ozans\Desktop\VizeOdev\YBS.db3");

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();
            await DisplayAlert("Veritabanı Bağlandı", bag.State.ToString(), "Tamam");
            AktifMi.Text = "Veritabanı şu an bağlı durumda.";
            AktifMi.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#008000");


            // Tabloyu oluştur
            string sql = "CREATE TABLE IF NOT EXISTS stok(" +
                         "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                         "ad TEXT NOT NULL," +
                         "miktar int NOT NULL," +
                         "sno int)";

            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            cmd.ExecuteNonQuery();
        }


        private async void btn2_Clicked(object sender, EventArgs e)
        {
            if (bag.State == ConnectionState.Open) bag.Close();
            await DisplayAlert("veritabanı devredışı bırakılmıştır", bag.State.ToString(), "Tamam");
            AktifMi.Text = "Veritabanı şu an kapalı durumda.";
            AktifMi.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF0000");

        }




        private async void Liste()
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "SELECT * FROM stok";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            SQLiteDataReader dr = cmd.ExecuteReader();

            _stokListesi.Clear(); // Önce listeyi temizle
            while (dr.Read())
            {
                _stokListesi.Add(new StokModel
                {
                    Id = Convert.ToInt32(dr["id"]),
                    Ad = dr["ad"].ToString(),
                    Miktar = Convert.ToInt32(dr["miktar"]),
                    Sno = Convert.ToInt32(dr["sno"])
                });
            }
            dr.Close();
        }


        private async void btn3_Clicked(object sender, EventArgs e)
        {
            Liste();
        }

        void Ekle(int sno, string ad, int miktar)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "INSERT INTO stok (sno, ad, miktar) VALUES (@sno, @ad, @miktar)";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);

            cmd.Parameters.AddWithValue("@sno", sno);
            cmd.Parameters.AddWithValue("@ad", ad);
            cmd.Parameters.AddWithValue("@miktar", miktar);

            cmd.ExecuteNonQuery();
        }


        private async void btn4_Clicked(object sender, EventArgs e)
        {
            // Entry alanlarından veriyi al
            string ad = txtAd.Text;
            int.TryParse(txtMiktar.Text, out int miktar);
            int.TryParse(txtSno.Text, out int sno);

            if (string.IsNullOrWhiteSpace(ad) || miktar <= 0 || sno <= 0)
            {
                DisplayAlert("Hata", "Lütfen tüm alanları doğru bir şekilde doldurun.", "Tamam");
                return;
            }

            // Ekleme işlemi
            Ekle(sno, ad, miktar);
            DisplayAlert("Başarılı", "Ürün başarıyla eklendi.", "Tamam");

            // Alanları temizle
            txtAd.Text = "";
            txtMiktar.Text = "";
            txtSno.Text = "";

            // Listeyi güncelle
            Liste();
        }

        static void Sil(int ID)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "DELETE FROM stok WHERE id = @id";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            cmd.Parameters.AddWithValue("@id", ID);

            cmd.ExecuteNonQuery();
        }


        private void btn5_Clicked(object sender, EventArgs e)
        {
            // Entry alanından ID al
            if (int.TryParse(txtSilID.Text, out int id))
            {
                Sil(id);
                DisplayAlert("Başarılı", "Kayıt başarıyla silindi.", "Tamam");
                txtSilID.Text = ""; // Alanı temizle
                Liste(); // Listeyi güncelle
            }
            else
            {
                DisplayAlert("Hata", "Lütfen geçerli bir ID giriniz.", "Tamam");
            }


        }
        void Guncelle(int id, string yeniAd, int yeniMiktar, int yeniSno)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "UPDATE stok SET ad = @yeniAd, miktar = @yeniMiktar, sno = @yeniSno WHERE id = @id";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@yeniAd", yeniAd);
            cmd.Parameters.AddWithValue("@yeniMiktar", yeniMiktar);
            cmd.Parameters.AddWithValue("@yeniSno", yeniSno);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                DisplayAlert("Başarılı", "Kayıt başarıyla güncellendi.", "Tamam");
            }
            else
            {
                DisplayAlert("Hata", "Güncelleme işlemi başarısız oldu. Belirtilen ID bulunamadı.", "Tamam");
            }
        }

        private void btnGuncelle_Clicked(object sender, EventArgs e)
        {
            if (int.TryParse(txtGuncelleID.Text, out int id) &&
                !string.IsNullOrWhiteSpace(txtYeniAd.Text) &&
                int.TryParse(txtYeniMiktar.Text, out int yeniMiktar) &&
                int.TryParse(txtYeniSno.Text, out int yeniSno))
            {
                Guncelle(id, txtYeniAd.Text, yeniMiktar, yeniSno);

                txtGuncelleID.Text = "";
                txtYeniAd.Text = "";
                txtYeniMiktar.Text = "";
                txtYeniSno.Text = "";

                Liste();
            }
            else
            {
                DisplayAlert("Hata", "Lütfen tüm alanları doğru bir şekilde doldurun.", "Tamam");
            }
        }

        private void Ara(string aramaTerimi)
        {
            if (bag.State == ConnectionState.Closed) bag.Open();

            string sql = "SELECT * FROM stok WHERE ad LIKE @aramaTerimi";
            SQLiteCommand cmd = new SQLiteCommand(sql, bag);
            cmd.Parameters.AddWithValue("@aramaTerimi", $"%{aramaTerimi}%");

            SQLiteDataReader dr = cmd.ExecuteReader();
            string sonuc = "";

            while (dr.Read())
            {
                sonuc += $"ID: {dr["id"]}, Ad: {dr["ad"]}, Miktar: {dr["miktar"]}, Sno: {dr["sno"]}\n";
            }
            dr.Close();

            if (string.IsNullOrWhiteSpace(sonuc))
            {
                DisplayAlert("Sonuç Yok", "Aradığınız kriterlere uygun bir kayıt bulunamadı.", "Tamam");
            }
            else
            {
                DisplayAlert("Arama Sonuçları", sonuc, "Tamam");
            }
        }

        private void btnAra_Clicked(object sender, EventArgs e)
        {
            string aramaTerimi = txtArama.Text;

            if (string.IsNullOrWhiteSpace(aramaTerimi))
            {
                DisplayAlert("Hata", "Lütfen bir arama terimi giriniz.", "Tamam");
                return;
            }

            Ara(aramaTerimi);
        }


    }
}
