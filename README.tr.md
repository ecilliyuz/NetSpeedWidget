# 🚀 NetSpeed Widget - Windows Görev Çubuğu Anlık Ağ Hız Göstergesi

[English README](README.md) | [Türkçe Dokümantasyon](README.tr.md)

Windows için özel olarak geliştirilmiş, görev çubuğunda (sağ altta, saat ve bildirim alanının hemen yanında) anlık **Download (İndirme)** ve **Upload (Yükleme)** hızınızı gösteren modern ve hafif bir native Windows uygulamasıdır.

---

## ✨ Özellikler

- **Anlık Hız Takibi:** Gerçek zamanlı indirme (`↓`) ve yükleme (`↑`) hızınızı saniyelik güncellemelerle gösterir.
- **Görev Çubuğu Entegrasyonu:** Windows 11 / Windows 10 görev çubuğu üzerinde saatin hemen soluna oturur.
- **Kişiselleştirilebilir Konum (Sürükle-Bırak):** Farenin sol tuşuyla tutarak ekranın veya görev çubuğunun dilediğiniz yerine taşıyabilir, istediğinizde sağ tıklayıp *"Konumu Kilitle"* seçeneğiyle sabitleyebilirsiniz.
- **Tek Tıkla Saate Hizalama:** *"Saatin Yanına Hizala"* seçeneği ile istediğiniz zaman görev çubuğundaki varsayılan yerine otomatik oturur.
- **Sistem Tepsisi (Tray) İkonu:** Bildirim alanındaki şık ikonu sayesinde anlık hız özetini ve toplam oturum kullanımını gösterir.
- **Görünüm Modları & Temalar:**
  - **Yarı Saydam Koyu (Fluent Dark - Varsayılan):** Windows 11 görev çubuğuyla mükemmel uyumlu cam efekti.
  - **Tam Şeffaf:** Arka planı tamamen gizler, sadece hız yazıları görev çubuğu üstünde görünür.
  - **Açık Tema:** Açık renkli görev çubuğu kullananlar için optimize edilmiş tasarım.
  - **Kompakt Mod (Tek Satır):** Dilerseniz iki satır yerine yan yana tek satır olarak da kullanabilirsiniz.
- **Hız Birimleri:** Otomatik (B/s, KB/s, MB/s, GB/s) veya sabit KB/s / MB/s seçimi.
- **Ağ Ayrıntıları:** Çift tıklayarak veya menüden *"Ağ İstatistikleri"*ne basarak aktif ağ kartı, yerel IP adresi ve toplam indirilen/yüklenen veri miktarını görebilirsiniz.
- **Sıfır Performans Kaybı:** C# .NET üzerinde optimize edilmiştir; 0% işlemci kullanımı ve minimum bellek tüketimi sağlar.
- **Windows ile Otomatik Başlatma:** Bilgisayarınızı her açtığınızda otomatik olarak devreye girer.

---

## 📦 Kurulum ve Çalıştırma

### 1. Kolay Kurulum (Önerilen)
1. Bu klasördeki **`Kurulum.bat`** dosyasına çift tıklayın.
2. Kurulum dosyaları otomatik olarak `%LocalAppData%\NetSpeedWidget` dizinine yüklenir.
3. Masaüstünüze ve Başlat menünüze kısayol eklenir.
4. Windows Başlangıç kaydı otomatik oluşturulur.
5. Program hemen çalışmaya başlar ve saatin yanında hızınız görünür!

### 2. Manuel / Taşınabilir Kullanım
Kurulum yapmadan doğrudan çalıştırmak isterseniz:
- `publish\NetSpeedWidget.exe` dosyasını çift tıklayarak çalıştırabilirsiniz.

---

## 🛠️ Kullanım İpuçları

- **Taşıma:** Widget'ın üzerine sol tıklayıp basılı tutarak ekranın dilediğiniz yerine sürükleyebilirsiniz.
- **Kilitleme:** Konumu sabitledikten sonra istemeden kaymaması için sağ tıklayıp **"Konumu Kilitle"** seçeneğini işaretleyin.
- **Ağ Bilgilerini Görme:** Göstergeye çift tıklayın; IP adresiniz ve toplam oturum indirme/yükleme boyutunuz açılır.
- **Ayarlar Menüsü:** Göstergenin üzerine veya sağ alttaki sistem tepsisi ikonuna sağ tıklayarak tüm ayarları değiştirebilirsiniz.

---

## 🗑️ Kaldırma (Uninstall)

Uygulamayı dilediğiniz zaman kaldırmak için:
1. **`Kaldir.bat`** dosyasına çift tıklayın.
2. Çalışan uygulama kapatılır, başlangıç kaydı, masaüstü kısayolu ve dosyalar tamamen temizlenir.

---

## 💻 Kaynak Koddan Derleme (Geliştiriciler İçin)

Kaynak kodlar `src/` klasöründedir:
```powershell
dotnet build src/NetSpeedWidget.csproj -c Release
```
Tek dosya olarak yayınlamak için:
```powershell
dotnet publish src/NetSpeedWidget.csproj -c Release -o publish
```
