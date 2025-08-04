# Staj Projesi - ASP.NET Core MVC

Bu proje, ASP.NET Core MVC (.NET 8.0) kullanılarak geliştirilmiş bir kullanıcı yönetim sistemi ve Python web servisi entegrasyonu içeren bir web uygulamasıdır.

## 🚀 Özellikler

### Kullanıcı Yönetimi
- ✅ Kullanıcı kaydı ve giriş sistemi
- ✅ Profil güncelleme (kullanıcı adı ve şifre)
- ✅ Tüm kullanıcıları listeleme
- ✅ Kullanıcı düzenleme ve silme
- ✅ Excel ve PDF export özellikleri

### Python Web Servisi Entegrasyonu
- ✅ Python script'i ile hava durumu verisi alma
- ✅ OpenWeatherMap API entegrasyonu
- ✅ Gerçek zamanlı hava durumu bilgileri
- ✅ JSON formatında veri alışverişi

### Güvenlik
- ✅ SHA256 şifre hashleme
- ✅ Session yönetimi
- ✅ Anti-forgery token koruması
- ✅ Oturum timeout kontrolü

### UI/UX
- ✅ Responsive Bootstrap tasarım
- ✅ Modern ve kullanıcı dostu arayüz
- ✅ DataTables entegrasyonu
- ✅ Font Awesome ikonları

## 📋 Gereksinimler

### Sistem Gereksinimleri
- .NET 8.0 SDK
- Python 3.x
- SQL Server (veya SQLite)
- Web tarayıcısı

### Python Paketleri
```bash
# Ubuntu/Debian
sudo apt install python3-requests

# Windows
pip install requests
```

## 🛠️ Kurulum

### 1. Projeyi İndirin
```bash
git clone <repository-url>
cd staj_proje
```

### 2. Veritabanı Bağlantısını Ayarlayın
`appsettings.json` dosyasında connection string'i güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StajProje;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Veritabanını Oluşturun
```bash
dotnet ef database update
```

### 4. Python Script'ini Yapılandırın
`Scripts/weather_service.py` dosyasında OpenWeatherMap API key'inizi ekleyin:

```python
api_key = "YOUR_API_KEY_HERE"  # Buraya kendi API key'inizi yazın
```

### 5. Projeyi Çalıştırın
```bash
dotnet run
```

## 🌐 Kullanım

### Giriş Yapma
1. Tarayıcıda `https://localhost:5001` adresine gidin
2. Kayıt olun veya mevcut hesabınızla giriş yapın

### Ana Sayfa
- Sol üst köşedeki menü butonuna tıklayarak navigasyon yapın
- Hava durumu bilgilerini görüntüleyin
- Hızlı erişim kartlarını kullanın

### Profil Yönetimi
- Kullanıcı adınızı ve şifrenizi güncelleyin
- Mevcut şifrenizi doğrulayın

### Kullanıcı Listesi
- Tüm kullanıcıları görüntüleyin
- Kullanıcıları düzenleyin veya silin
- Excel/PDF formatında export yapın

## 🔧 Teknik Detaylar

### Proje Yapısı
```
staj_proje/
├── Controllers/          # MVC Controllers
├── Models/              # Entity ve Service modelleri
├── Views/               # Razor Views
├── Scripts/             # Python scriptleri
├── Helpers/             # Yardımcı sınıflar
└── wwwroot/             # Statik dosyalar
```

### Kullanılan Teknolojiler
- **Backend**: ASP.NET Core MVC 8.0
- **Veritabanı**: Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery, DataTables
- **Python**: requests kütüphanesi
- **Export**: ClosedXML (Excel), iTextSharp (PDF)

### API Entegrasyonu
- OpenWeatherMap API kullanılarak hava durumu verileri alınır
- Python script'i JSON formatında veri döner
- C# tarafında JSON deserialize edilir

## 🐛 Sorun Giderme

### Python Script Çalışmıyor
1. Python 3.x yüklü olduğundan emin olun
2. `requests` paketinin yüklü olduğunu kontrol edin
3. API key'in doğru olduğunu kontrol edin

### Veritabanı Bağlantı Hatası
1. Connection string'i kontrol edin
2. SQL Server'ın çalıştığından emin olun
3. Migration'ları çalıştırın: `dotnet ef database update`

### Hava Durumu Verisi Görünmüyor
1. Python script'inin çalıştığını kontrol edin
2. API key'in geçerli olduğunu kontrol edin
3. İnternet bağlantısını kontrol edin

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📞 İletişim

Sorularınız için issue açabilir veya iletişime geçebilirsiniz.