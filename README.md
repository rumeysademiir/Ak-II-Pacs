# Akıllı PACS – Yapay Zekâ Destekli Radyoloji Görüntüleme ve İş Akışı Sistemi

Bu proje, **Yozgat Bozok Üniversitesi Bilgisayar Mühendisliği Bölümü** yazılım stajı kapsamında, **InterPacs Sağlık Çözümleri Ltd. Şti.** bünyesinde geliştirilmiş küçük ölçekli bir PACS (Picture Archiving and Communication System) prototipidir.

Sistem; yüklenen radyolojik görüntüleri (CT/MR/X-Ray) otomatik olarak analiz ederek bir **kalite skoru** üretir, bu skora göre **öncelik** belirler, **DICOM metadata**sını okur, basitleştirilmiş bir **HL7 mesajı** oluşturur ve tüm sonuçları bir **PACS Worklist (iş listesi)** ekranında sunar.

**Not:** Bu proje eğitim/öğretim amaçlı bir prototiptir. Görüntü üzerindeki "AI ön bulgu", "AI Heatmap" ve güven skoru gibi göstergeler **gerçek bir yapay zekâ teşhis modelinden gelmemektedir**; yalnızca arayüz simülasyonudur ve klinik karar desteği için kullanılamaz.

## Kullanılan Teknolojiler

**Backend:** ASP.NET Core Web API (C#)
**Veri Erişim Katmanı:** Dapper
**Veritabanı:** SQL Server
**Görüntü Analizi:** Python + OpenCV (Laplacian varyansı / kontrast analizi)
**DICOM Okuma:** pydicom
**Frontend:** Bootstrap 5.3, vanilla JavaScript

## Sistem Akışı

Kullanıcı (Görüntü Yükleme)
        │
        ▼
ASP.NET Core Web API
        │
        ▼
Görüntünün Sunucuya Kaydedilmesi
        │
        ▼
Python Analiz Scripti (analyzer.py + OpenCV)
        │
        ▼
Kalite Skoru + DICOM Metadata Üretimi
        │
        ▼
Öncelik Skoru Hesaplama
        │
        ▼
HL7 Mesajının Oluşturulması
        │
        ▼
SQL Server'a Kayıt (Studies Tablosu)
        │
        ▼
PACS Worklist (Web Dashboard)
        │
        ▼
Raporlama → "Rapor Tamamlandı"
```

## Proje Yapısı

```
├── Controllers/         → API uç noktaları (StudiesController.cs)
├── Models/               → Veri modelleri (Study.cs)
├── Views/Home/           → Razor sayfaları (Index.cshtml, Login.cshtml)
├── wwwroot/
│   ├── css/              → style.css
│   └── pacs_images/      → yüklenen görüntülerin kaydedildiği klasör
├── App_Data/             → HL7 log dosyaları
├── analyzer.py           → Python görüntü kalite/DICOM analiz scripti
└── Program.cs            → Uygulama giriş noktası


## Çalıştırma

1. `appsettings.json` içindeki `ConnectionStrings` alanının kendi SQL Server ortamınıza uygun olduğundan emin olun (varsayılan: `(localdb)\MSSQLLocalDB`).
2. Gerekli Python kütüphanelerini kurun:
   ```
   pip install pydicom opencv-python numpy
   ```
3. Visual Studio'da projeyi açıp **F5** ile çalıştırın.

## Geliştirici

Rümeysa Demir — Bilgisayar Mühendisliği, Yozgat Bozok Üniversitesi
Staj: InterPacs Sağlık Çözümleri Ltd. Şti. (01.07.2026 – 29.07.2026)
