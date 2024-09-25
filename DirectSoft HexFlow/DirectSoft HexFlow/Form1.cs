using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DirectSoft_HexFlow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();

        private string filePath;   // Dosya yolu

        private byte[] fileBytes;  // Dosya byte dizisi
        private string currentFileName = string.Empty; // Açılan dosyanın adını saklamak için
        private int bytesPerLine = 8; // Varsayılan bit değeri

        private void Form1_Load(object sender, EventArgs e)
        {
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            lblFileSize1.Text = "Dosya Boyutu: " + 0 + " bayt";
        }



        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;

                if (!BackgroundWorker.IsBusy)
                {
                    BackgroundWorker.RunWorkerAsync(filePath);  // Arka planda işlemi başlat
                    txtHexView.Visible = false;
                    groupBox1.Visible = true;
                    groupBox1.Text = "Dosya Açılıyor...";
                }
            }
        }



        private void DisplayHexData(byte[] bytes)
        {
            try
            {
                StringBuilder hex = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++)
                {
                    hex.AppendFormat("{0:X2} ", bytes[i]);

                    // Her 16 byte'da bir yeni satıra geç
                    if ((i + 1) % 16 == 0)
                        hex.AppendLine();
                }
                txtHexView.Text = hex.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hex verisi işlenirken hata oluştu: " + ex.Message);
            }
        }

        private void txtHexView_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            // Hex görüntüsünü güncelle
            undoStack.Push(txtHexView.Text); // Mevcut durumu yığına ekle
            UpdateUndoRedoButtons(); // Buton durumunu güncelle
            UpdateHexDisplay();
        }

        private void UpdateFileSizeLabel()
        {
            if (fileBytes != null)
            {
                double fileSize = fileBytes.Length;
                string[] sizeSuffixes = { "bayt", "KB", "MB", "GB", "TB", "PB", "EB" }; // Birimler

                int suffixIndex = 0;
                while (fileSize >= 1024 && suffixIndex < sizeSuffixes.Length - 1)
                {
                    fileSize /= 1024;  // 1024'e bölerek birim değiştir
                    suffixIndex++;  // Bir sonraki birime geç
                }

                // Dosya boyutunu birimle göster
                lblFileSize1.Text = string.Format("Dosya Boyutu: {0:0.##} {1}", fileSize, sizeSuffixes[suffixIndex]);
            }
        }


        private void bitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bytesPerLine = 16;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void bitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            bytesPerLine = 32;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void bitToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            bytesPerLine = 64;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void bitToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            bytesPerLine = 128;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void bitToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            bytesPerLine = 216;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void bitToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            bytesPerLine = 8;
            label22.Text = "Satır Başına Bayt: " + bytesPerLine;
            UpdateHexDisplay();
        }

        private void UpdateHexDisplay()
        {
            string hexString = txtHexView.Text.Replace(" ", "").Replace(Environment.NewLine, "");

            // Geçerli hex karakterlerini kontrol et
            if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"\A\b[0-9a-fA-F]*\b\Z"))
            {
                return; // Hatalı giriş varsa çık
            }

            // Byte dizisini güncelle
            if (hexString.Length % 2 == 0) // Hex çift sayıda karakterden oluşmalıdır
            {
                try
                {
                    byte[] updatedBytes = new byte[hexString.Length / 2];
                    for (int i = 0; i < hexString.Length; i += 2)
                    {
                        updatedBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    }
                    fileBytes = updatedBytes;  // Değişiklikleri byte dizisine uygula
                    UpdateFileSizeLabel();  // Dosya boyutunu güncelle
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}");
                }
            }

            // Metni formatla ve güncelle
            FormatHexText(hexString);
        }

        private void FormatHexText(string hexString)
        {
            StringBuilder formattedHex = new StringBuilder();
            int byteCount = 0; // Geçerli bayt sayısını takip et

            for (int i = 0; i < hexString.Length; i++)
            {
                // Her iki karakterden sonra boşluk ekle
                if (i > 0 && i % 2 == 0)
                {
                    formattedHex.Append(" ");
                }

                // Bayt sayısına göre her satırda yeni satıra geç
                if (i > 0 && (i % (bytesPerLine * 2) == 0))
                {
                    formattedHex.Append(Environment.NewLine);
                    byteCount = 0; // Yeni satıra geçince bayt sayısını sıfırla
                }

                formattedHex.Append(hexString[i]);

                // Her iki karakterde bir bayt tamamlanmış olur
                if (i % 2 == 1)
                {
                    byteCount++;
                }
            }

            // TextBox içeriğini güncelle
            txtHexView.Text = formattedHex.ToString().Trim(); // Boşlukları temizle
            txtHexView.SelectionStart = txtHexView.Text.Length; // İmleci en sona koy
        }



        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void kesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Seçilen metni kes
            if (txtHexView.SelectedText.Length > 0)
            {
                Clipboard.SetText(txtHexView.SelectedText);
                txtHexView.SelectedText = "";
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtHexView.SelectedText.Length > 0)
            {
                Clipboard.SetText(txtHexView.SelectedText);
            }
        }

        private void UpdateUndoRedoButtons()
        {
            geriAlToolStripMenuItem.Enabled = undoStack.Count > 0; // Undo butonu etkin
            yineleToolStripMenuItem.Enabled = redoStack.Count > 0; // Redo butonu etkin
        }

        private void yapıştırToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Panodaki metni yapıştır
            string clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                int selectionStart = txtHexView.SelectionStart;
                txtHexView.Text = txtHexView.Text.Insert(selectionStart, clipboardText);
                txtHexView.SelectionStart = selectionStart + clipboardText.Length; // İmleci yapıştırılan metnin sonuna taşı
            }
        }

        private void geriAlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                string lastState = undoStack.Pop();
                redoStack.Push(txtHexView.Text);
                txtHexView.Text = lastState;
                txtHexView.SelectionStart = txtHexView.Text.Length;
                UpdateUndoRedoButtons(); // Buton durumunu güncelle
            }
        }

        private void yineleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                string redoState = redoStack.Pop();
                undoStack.Push(txtHexView.Text);
                txtHexView.Text = redoState;
                txtHexView.SelectionStart = txtHexView.Text.Length;
                UpdateUndoRedoButtons(); // Buton durumunu güncelle
            }
        }

        private void tümünüSeçToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtHexView.SelectAll(); // TextBox içindeki tüm metni seç
            txtHexView.Focus(); // TextBox'a odaklan
        }

        private void yapıştırToolStripButton_Click(object sender, EventArgs e)
        {
            yapıştırToolStripMenuItem.PerformClick();
        }

        private void kopyalaToolStripButton_Click(object sender, EventArgs e)
        {
            kopyalaToolStripMenuItem.PerformClick();
        }

        private void kesToolStripButton_Click(object sender, EventArgs e)
        {
            kesToolStripMenuItem.PerformClick();
        }

        private void yeniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
            txtHexView.Visible = false;
            groupBox1.Visible = true;
            groupBox1.Text = "Dosya Kaydediliyor...";
        }

        private void farklıKaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
            txtHexView.Visible = false;
            groupBox1.Visible = true;
            groupBox1.Text = "Dosya Kaydediliyor...";
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Tüm Dosyalar (*.*)|*.*";
                    saveFileDialog.Title = "Dosyayı Kaydet";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = saveFileDialog.FileName; // Dosya yolunu güncelle
                        backgroundWorker1.RunWorkerAsync(filePath);
                    }
                }
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                // Arka planda kaydetme işlemini başlat
                backgroundWorker1.RunWorkerAsync(filePath);
            }
        }

        private void SaveHexContent(string fileName)
        {
            // Hex içeriğini al ve byte dizisine dönüştür
            byte[] bytesToSave = GetBytesFromHex(txtHexView.Text);
            File.WriteAllBytes(fileName, bytesToSave);
        }

        private byte[] GetBytesFromHex(string hex)
        {
            // Geçersiz karakterleri temizle ve boşlukları kaldır
            hex = hex.Replace(" ", "").Replace("\n", "").Replace("\r", "");

            // Eğer hex string'i tek karakterle bitiyorsa, sonuna bir '0' ekle
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;  // Başa '0' ekleyerek çift karakterli hale getir
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                string byteValue = hex.Substring(i, 2);
                try
                {
                    bytes[i / 2] = Convert.ToByte(byteValue, 16);
                }
                catch (FormatException)
                {
                    throw new FormatException($"Invalid hex value: {byteValue}");
                }
            }
            return bytes;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;  // Dosya yolu argümanı

            // Dosyayı arka planda oku
            byte[] bytes = File.ReadAllBytes(path);

            // Hex verisini oluştur ve ilerleme durumunu raporla
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                hex.AppendFormat("{0:X2} ", bytes[i]);

                // İlerleme raporu her 1000 baytta bir
                if (i % 1000 == 0)
                {
                    int progressPercentage = (int)((i / (float)bytes.Length) * 100);
                    BackgroundWorker.ReportProgress(progressPercentage);
                }
            }

            e.Result = hex.ToString();  // Hex string sonucu
        }

        

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;  // ProgressBar'ı güncelle
            label3.Text = "%" + ProgressBar.Value.ToString();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtHexView.Text = (string)e.Result;  // Hex verisini göster
                UpdateFileSizeLabel();  // Dosya boyutunu güncelle
                MessageBox.Show("Dosya başarıyla yüklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtHexView.Visible = true;
                groupBox1.Visible = false;
            }
            else
            {
                MessageBox.Show("Hata: " + e.Error.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtHexView.Visible = true;
                groupBox1.Visible = false;
            }
            ProgressBar.Value = 0;  // ProgressBar'ı sıfırla
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;  // Dosya yolu argümanı
            byte[] bytesToSave = GetBytesFromHex(txtHexView.Text);  // Hex'ten byte dizisine dönüştür

            // Eğer byte dizisi boşsa kaydetme işlemini yap
            if (bytesToSave.Length == 0)
            {
                File.WriteAllBytes(path, new byte[0]); // Boş bir dosya kaydet
                return; // Çıkış yap
            }

            // Dosyayı kaydet
            File.WriteAllBytes(path, bytesToSave); // Bütün dosyayı yazıyoruz.

            // İlerlemeyi raporla
            for (int i = 0; i < bytesToSave.Length; i++)
            {
                
                  int progressPercentage = (int)((i / (float)bytesToSave.Length) * 100);
                  backgroundWorker1.ReportProgress(progressPercentage);
                
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                label3.Text = "%100";
                MessageBox.Show("Dosya başarıyla kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtHexView.Visible = true;
                groupBox1.Visible = false;
            }
            else
            {
                label3.Text = "%100";
                MessageBox.Show("Hata: " + e.Error.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtHexView.Visible = true;
                groupBox1.Visible = false;
            }
            ProgressBar.Value = 0;  // ProgressBar'ı sıfırla
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;  // ProgressBar'ı güncelle
            label3.Text = "%" + ProgressBar.Value.ToString();
        }

        private void yeniToolStripButton_Click(object sender, EventArgs e)
        {
            yeniToolStripMenuItem.PerformClick();
        }

        private void açToolStripButton_Click(object sender, EventArgs e)
        {
            açToolStripMenuItem.PerformClick();
        }

        private void kaydetToolStripButton_Click(object sender, EventArgs e)
        {
            kaydetToolStripMenuItem.PerformClick();
        }

        private void çıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void yardımToolStripButton_Click(object sender, EventArgs e)
        {
            hakkındaToolStripMenuItem.PerformClick();
        }
    }
}