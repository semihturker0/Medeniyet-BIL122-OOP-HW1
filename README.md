# Yıldız Toplayıcısı (Console Catch Game)

Bu proje, C# ile geliştirilmiş, konsol tabanlı bir "Yukarıdan Düşen Nesneleri Yakalama" (Arcade) oyunudur. Nesne Yönelimli Programlama (OOP) ve temel oyun döngüsü (Game Loop) mantıklarını kavramak amacıyla hazırlanmıştır.

## 📌 Proje Hakkında

Oyuncu, ekranın alt kısmında bulunan bir sepeti (`\_/`) yön tuşlarıyla kontrol ederek yukarıdan rastgele düşen yıldızları ve halkaları toplamaya çalışır. Oyun, giderek artan bir zorluk eğrisine, kombo ve can (HP) sistemine sahiptir.

Projede özellikle **performans optimizasyonu** (ekran titremesini önlemek için `StringBuilder` tabanlı rendering), **Input Buffer yönetimi** (takılmaları önlemek için) ve **Dosya I/O işlemleri** (skor kaydı ve olay loglama) üzerinde durulmuştur.

## 🕹️ Oynanış ve Mekanikler

* **Sol ve Sağ Ok Tuşları:** Sepeti hareket ettirir.
* `*` (Sarı Yıldız): +1 Puan
* `O` (Mor Halka): +3 Puan
* `X` (Kırmızı Zehir): -1 Can (Kombo bozar)
* `H` (Can İksiri): +1 Can (Maksimum 5 can)

**Sistem Özellikleri:**

* **Kombo Sistemi:** Art arda yakalanan her nesne kombo sayacını artırır. Her 5 komboda ekstra +1 puan kazanılır. Puan veren nesneleri yere düşürmek veya zehir yakalamak komboyu sıfırlar.
* **Dinamik Zorluk:** Oyuncu her 20 puanda bir seviye atlar ve seviye arttıkça oyunun hız/düşme süresi (Thread.Sleep) dinamik olarak azalır.
* **Rekor Sistemi:** Oyun bittiğinde ulaşılan en yüksek skor `en_yuksek_skor.txt` dosyasına kalıcı olarak kaydedilir ve sonraki oyunlarda oyuncuya gösterilir.

## 🛠️ Teknik Detaylar (Debug ve Loglama)

Ödevin bir gereksinimi olarak, program çalışırken oyun döngüsündeki kritik olaylar eş zamanlı olarak `oyun_kayitlari.txt` dosyasına kaydedilmektedir (Loglama). Kaydedilen başlıca olaylar şunlardır:

* `GİRDİ`: Kullanıcının tuş basımları ve karakterin yeni X/Y koordinatları.
* `GÜNCELLEME`: Ekranda yeni bir nesne (X, Y ve sembol bilgisiyle) oluşması.
* `ÇARPIŞMA`: Karakterin nesneleri yakalama anı ve anlık skor/kombo durumu.
* `CAN_KAZANDI` / `ZEHİR_YENDİ`: Can (HP) durumunu etkileyen çarpışmalar.
* `OYUN_BİTTİ`: Final skoru, kalan can ve kırılan rekor durumu.

## 🚀 Kurulum ve Çalıştırma

Projeyi kendi ortamınızda derleyip çalıştırmak için:

1. Depoyu klonlayın veya indirin.
2. JetBrains Rider veya Visual Studio kullanarak `OOP_HW_1.sln` (veya ilgili .sln dosyası) üzerinden projeyi açın.
3. Target Framework olarak `net10.0` (veya üzeri) seçili olduğundan emin olun.
4. Projeyi derleyip (Build) çalıştırın (Run).
