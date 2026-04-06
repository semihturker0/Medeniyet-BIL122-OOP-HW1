using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace KonsolYakalamaOyunu
{
    class Program
    {
        // --- OYUN AYARLARIM VE SABİTLERİM ---
        const int Genislik = 70;                // Metinlerin alt satıra taşmaması ve daha geniş bir oyun alanı sunmak için genişliği 70 olarak ayarladım.
        const int Yukseklik = 20;
        const string LogDosyasi = "oyun_kayitlari.txt";
        const string SkorDosyasi = "en_yuksek_skor.txt"; // En yüksek skorumu (rekorumu) saklayacağım metin dosyası.

        // Konsolda renkli çıktılar verebilmek için ANSI kaçış kodlarını (Escape Codes) kullanıyorum.
        const string RenkSifirla = "\x1b[0m";
        const string RenkOyuncu = "\x1b[36m";   // Turkuaz (Cyan)
        const string RenkYildiz = "\x1b[33m";   // Sarı
        const string RenkHalka = "\x1b[35m";    // Mor
        const string RenkZehir = "\x1b[31m";    // Kırmızı
        const string RenkBilgi = "\x1b[32m";    // Yeşil
        const string RenkCan = "\x1b[91m";      // Parlak Kırmızı (Can için)
        const string RenkKombo = "\x1b[93m";    // Parlak Sarı (Kombo için)

        // --- OYUNUMUN ANLIK DURUMUNU TUTAN DEĞİŞKENLERİM ---
        const int MaksCan = 5;                  // Oyuncunun ulaşabileceği maksimum can sınırını 5 olarak belirliyorum.
        static int oyuncuX = Genislik / 2;
        static int oyuncuY = Yukseklik - 1;
        static int toplamSkor = 0;
        static int enYuksekSkor = 0;            // Rekor skorumu tuttuğum değişkenim.
        static int komboSayaci = 0;             // Arka arkaya yakalama kombomu takip ettiğim değişkenim.
        static int can = 3;                     
        static int seviye = 1;                  
        static int oyunHizi = 80;               
        static bool oyunDevamEdiyor = true;
        
        static List<DusenNesne> nesneler = new List<DusenNesne>();
        static Random rastgeleSayiUretici = new Random();
        
        // Ekranın titremesini önlemek ve performansımı artırmak için tüm çizimi tek seferde basacağım bir tampon bellek (StringBuilder) oluşturdum.
        static StringBuilder cizimTamponu = new StringBuilder((Genislik + 1) * Yukseklik);

        // Yukarıdan düşen nesnelerimin özelliklerini tutması için bellekte daha hafif olan 'struct' (değer tipi) yapısını tercih ettim.
        struct DusenNesne
        {
            public int X;
            public int Y;
            public char Sembol;
            public int PuanDegeri;
            public string RenkKodu;
        }

        static void Main()
        {
            Console.CursorVisible = false; // Yanıp sönen konsol imlecimi gizliyorum.
            Console.SetWindowSize(Genislik + 1, Yukseklik + 2); // Konsol penceremin boyutunu oyun alanıma göre sabitliyorum.

            HikayeGoster(); // Oyuncuya atmosferik bir giriş sunmak için hikaye ekranımı çağırıyorum.

            // Her yeni oyunda log dosyamın içini temizleyip başlangıç mesajımı yazdırıyorum.
            File.WriteAllText(LogDosyasi, "--- Oyun Başladı ---\n");

            // Eğer daha önceden kaydettiğim bir rekor dosyam varsa, içindeki değeri okuyup değişkenime atıyorum.
            if (File.Exists(SkorDosyasi))
            {
                string okunanSkor = File.ReadAllText(SkorDosyasi);
                int.TryParse(okunanSkor, out enYuksekSkor);
            }

            while (oyunDevamEdiyor)
            {
                if (Console.KeyAvailable)
                {
                    GirdiIsle();
                }

                OyunuGuncelle();
                EkraniCiz();

                // Oyunumun bitiş koşullarını kontrol ediyorum: Oyuncunun canı bittiyse veya 150 puana ulaştıysa oyunu bitireceğim.
                if (can <= 0 || toplamSkor >= 150) 
                {
                    // Oyuncu rekorumu kırdıysa yeni rekoru dosyaya kaydediyorum.
                    bool rekorKirildiMi = toplamSkor > enYuksekSkor;
                    if (rekorKirildiMi)
                    {
                        enYuksekSkor = toplamSkor;
                        File.WriteAllText(SkorDosyasi, enYuksekSkor.ToString());
                    }

                    KayitTut("Oyun_Bitti", $"FinalSkoru={toplamSkor} RekorKırıldı={rekorKirildiMi} UlaşılanSeviye={seviye}");
                    oyunDevamEdiyor = false;
                }

                Thread.Sleep(oyunHizi); // Oyun döngümü, seviyeye göre hesapladığım oyun hızım kadar (milisaniye cinsinden) uyutuyorum.
            }

            // Oyun bittikten sonra ekranı temizleyip oyuncuya nihai sonucunu gösteriyorum.
            Console.Clear();
            Console.WriteLine($"\nOyun Bitti! Nihai Skorunuz: {toplamSkor}");
            if (toplamSkor >= enYuksekSkor && toplamSkor > 0)
            {
                Console.WriteLine($"{RenkKombo}Tebrikler! Yeni Rekor Kırdınız!{RenkSifirla}");
            }
            else
            {
                Console.WriteLine($"En Yüksek Skor (Rekor): {enYuksekSkor}");
            }
            Console.WriteLine("Loglar 'oyun_kayitlari.txt' dosyasına kaydedildi.");
            Console.WriteLine("Çıkmak için bir tuşa basın...");
            
            // Eğer oyuncu heyecan yapıp tuşlara basılı tuttuysa, tampon bellekte kalan tuşları temizliyorum ki oyun hemen kapanmasın.
            while(Console.KeyAvailable) Console.ReadKey(true);
            Console.ReadKey();
        }

        // Oyuncuya göstereceğim retro atari tarzı hikaye ekranımın fonksiyonu.
        static void HikayeGoster()
        {
            Console.Clear();
            string asciiSanati = $@"
{RenkYildiz}       * .  * .             *{RenkSifirla}
{RenkYildiz}     .    _  .    * * .{RenkSifirla}
{RenkHalka}       * / \     .    * .        *{RenkSifirla}
{RenkOyuncu}           /   \   * .      *{RenkSifirla}
{RenkOyuncu}          /_____\{RenkSifirla}     .     * .
{RenkOyuncu}          |     |{RenkSifirla}  {RenkKombo}Yıldız Toplayıcısı{RenkSifirla}
{RenkOyuncu}          |  _  |{RenkSifirla}     {RenkBilgi}(Bölüm 1){RenkSifirla}
{RenkOyuncu}          | | | |{RenkSifirla}
            ";

            Console.WriteLine(asciiSanati);
            
            // Yazıların ekranda satır satır belirmesi için metinlerimi bir diziye doldurdum (dramatik bir etki yaratmak istiyorum).
            string[] satirlar = {
                $"{RenkBilgi}Yüzyıllardır gökyüzünde süzülen Büyülü Ada parçalanıyor...{RenkSifirla}",
                $"Yaşam enerjisi yıldız {RenkYildiz}(*){RenkSifirla} ve halkalar {RenkHalka}(O){RenkSifirla} halinde yere dökülüyor.",
                $"Ama dikkat et! Kırmızı uzay çöpleri {RenkZehir}(X){RenkSifirla} de aralarına karıştı.\n",
                $"Görev: Güvenilir sepetin {RenkOyuncu}(\\_/){RenkSifirla} ile yıldızları toplayıp diyarı kurtar.",
                $"Bazen düşen hayat damlaları {RenkCan}(H){RenkSifirla} sana güç verecek.\n"
            };

            foreach (var satir in satirlar)
            {
                Console.WriteLine(satir);
                Thread.Sleep(800); // Her satırın arasında 0.8 saniye (800 ms) bekleyerek okunabilirliği artırıyorum.
            }

            Console.WriteLine($"{RenkKombo}Maceraya başlamak için herhangi bir tuşa bas...{RenkSifirla}");

            while (Console.KeyAvailable) Console.ReadKey(true); // Hikaye akarken kazara basılmış tuşlar varsa onları temizliyorum.
            Console.ReadKey(true);
            Console.Clear();
        }

        static void GirdiIsle()
        {
            bool hareketEttiMi = false;
            ConsoleKey sonBasilanTus = ConsoleKey.Escape; 

            // İşletim sisteminin tampon belleğinde birikmiş olan tüm tuş vuruşlarını tek bir karede okuyup tüketiyorum, böylece oyunumda takılma (lag) olmasını engelliyorum.
            while (Console.KeyAvailable)
            {
                var basilanTus = Console.ReadKey(true).Key;
                
                switch (basilanTus)
                {
                    // Oyuncumun sepeti 3 karakter genişliğinde olduğu için, ekranın dışına taşmaması adına sınırlarımı 1 ve Genislik-2 olarak belirledim.
                    case ConsoleKey.LeftArrow when oyuncuX > 1:
                        oyuncuX--;
                        hareketEttiMi = true;
                        sonBasilanTus = basilanTus;
                        break;
                    case ConsoleKey.RightArrow when oyuncuX < Genislik - 2:
                        oyuncuX++;
                        hareketEttiMi = true;
                        sonBasilanTus = basilanTus;
                        break;
                }
            }

            // Disk I/O darboğazını önlemek için sadece pozisyon değiştiyse log yazdırıyorum.
            if (hareketEttiMi)
            {
                KayitTut("Girdi", $"tus={sonBasilanTus} oyuncuX={oyuncuX} oyuncuY={oyuncuY}");
            }
        }

        static void OyunuGuncelle()
        {
            // %20 ihtimalle yukarıdan yeni bir nesne düşürüyorum.
            if (rastgeleSayiUretici.Next(0, 10) < 2)
            {
                DusenNesne yeniNesne = NesneUret();
                nesneler.Add(yeniNesne);
                KayitTut("Güncelleme", $"nesneOluşturuldu x={yeniNesne.X} y={yeniNesne.Y} sembol={yeniNesne.Sembol}");
            }

            // Listeden eleman sileceğim için döngümü sondan başa doğru kurdum. Her adımda nesnemi 1 birim aşağı kaydırıyorum.
            for (int i = nesneler.Count - 1; i >= 0; i--)
            {
                var mevcutNesne = nesneler[i];
                mevcutNesne.Y++;
                nesneler[i] = mevcutNesne; // Struct bir değer tipi olduğu için güncellenmiş halini tekrar listeye atıyorum.
                
                // Oyuncumun nesneyi yakalayıp yakalamadığını kontrol ediyorum (Sepetim 3 piksel genişliğinde olduğu için x ekseninde tolerans tanıdım).
                if (mevcutNesne.Y == oyuncuY && Math.Abs(mevcutNesne.X - oyuncuX) <= 1)
                {
                    PuanTopla(mevcutNesne);
                    nesneler.RemoveAt(i);
                }
                // Eğer nesnem ekranın en altına ulaştıysa onu listemden siliyorum.
                else if (mevcutNesne.Y >= Yukseklik)
                {
                    // Eğer oyuncum puan kazandıran bir yıldız veya halkayı yere düşürdüyse, ceza olarak kombosunu sıfırlıyorum.
                    if (mevcutNesne.Sembol == '*' || mevcutNesne.Sembol == 'O')
                    {
                        if (komboSayaci > 0) KayitTut("Kombo_Bozuldu", $"KaçırılanSembol={mevcutNesne.Sembol}");
                        komboSayaci = 0; 
                    }
                    nesneler.RemoveAt(i); 
                }
            }
        }

        // Skor Hesaplama Fonksiyonum
        static void PuanTopla(DusenNesne yakalananNesne)
        {
            if (yakalananNesne.Sembol == 'H')
            {
                if (can < MaksCan)
                {
                    can++; // Oyuncuma can kazandırıyorum.
                    KayitTut("Can_Kazandı", $"YeniCan={can}");
                }
                else
                {
                    KayitTut("Can_Max", $"KapasiteDolu={can}");
                }
            }
            else if (yakalananNesne.Sembol == 'X')
            {
                can--; // Oyuncumun canını azaltıyorum.
                komboSayaci = 0; // Zehirli nesne yakalandığı için komboyu bozuyorum.
                KayitTut("Zehir_Yendi", $"KalanCan={can}");
            }
            else
            {
                // Kombo sistemime göre kazanılacak ekstra puanları hesaplıyorum.
                komboSayaci++;
                int bonusPuan = komboSayaci / 5; // Oyuncuma her 5 komboda ekstra 1 puan veriyorum.
                int kazanilanToplamPuan = yakalananNesne.PuanDegeri + bonusPuan;
                
                toplamSkor += kazanilanToplamPuan; 
                KayitTut("Çarpışma", $"skor={toplamSkor} kombo={komboSayaci} yakalanan={yakalananNesne.Sembol} kazanılan={kazanilanToplamPuan}");
            }

            // Puan arttıkça oyunun zorlaşması için seviye ve hız değerlerimi güncelliyorum.
            seviye = Math.Max(1, (toplamSkor / 20) + 1); // Her 20 puanda bir seviye atlatıyorum.
            oyunHizi = Math.Max(30, 80 - ((seviye - 1) * 6)); // Oyun hızlanmasını yumuşattım, minimum limiti 30 milisaniye olarak belirledim.
        }

        // Farklı ihtimallerle (*, O, X, H) yeni bir düşen nesne yaratıp döndürdüğüm yardımcı metodum.
        static DusenNesne NesneUret()
        {
            int rastgeleTip = rastgeleSayiUretici.Next(0, 100);
            char sembol;
            int puan;
            string renk;

            // %50 ihtimalle normal bir yıldız nesnesi üretiyorum (1 puan verecek).
            if (rastgeleTip < 50) 
            {
                sembol = '*';
                puan = 1;
                renk = RenkYildiz;
            }
            // %20 ihtimalle O halkası nesnesi üretiyorum (3 puan verecek).
            else if (rastgeleTip < 70)
            {
                sembol = 'O';
                puan = 3;
                renk = RenkHalka;
            }
            // %28 ihtimalle zehir/bomba X nesnesi üretiyorum (Puan düşürmek yerine oyuncunun canını azaltacak).
            else if (rastgeleTip < 98)
            {
                sembol = 'X';
                puan = 0; 
                renk = RenkZehir;
            }
            // %2 ihtimalle Hayat/Can H iksiri üretiyorum (+1 Can verecek).
            else
            {
                sembol = 'H';
                puan = 0;
                renk = RenkCan;
            }

            return new DusenNesne 
            { 
                X = rastgeleSayiUretici.Next(0, Genislik), 
                Y = 0, 
                Sembol = sembol,
                PuanDegeri = puan,
                RenkKodu = renk
            };
        }

        static void EkraniCiz()
        {
            Console.SetCursorPosition(0, 0);
            cizimTamponu.Clear();

            // Çizim işlemlerim için arka planda iki boyutlu bir tuval dizisi (string matrisi) oluşturuyorum.
            string[,] tuval = new string[Yukseklik, Genislik];
            for (int y = 0; y < Yukseklik; y++)
                for (int x = 0; x < Genislik; x++)
                    tuval[y, x] = " ";

            // Düşen nesnelerimi renk kodlarıyla birlikte tuvalime yerleştiriyorum.
            foreach (var nesne in nesneler)
            {
                if (nesne.Y >= 0 && nesne.Y < Yukseklik)
                    tuval[nesne.Y, nesne.X] = $"{nesne.RenkKodu}{nesne.Sembol}{RenkSifirla}";
            }

            // Oyuncumun kova/sepet şeklindeki karakterini tuvalimin en alt satırına yerleştiriyorum.
            tuval[oyuncuY, oyuncuX - 1] = $"{RenkOyuncu}\\{RenkSifirla}"; // C# dilinde ekrana ters slash (\) yazdırmak için escape karakteri (\\) kullanmam gerektiğinden bu şekilde yazdım.
            tuval[oyuncuY, oyuncuX] = $"{RenkOyuncu}_{RenkSifirla}";
            tuval[oyuncuY, oyuncuX + 1] = $"{RenkOyuncu}/{RenkSifirla}";

            // Oluşturduğum iki boyutlu tuvali tek bir döngüde birleştirip StringBuilder tamponuma ekliyorum.
            for (int y = 0; y < Yukseklik; y++)
            {
                for (int x = 0; x < Genislik; x++)
                {
                    cizimTamponu.Append(tuval[y, x]);
                }
                cizimTamponu.Append('\n');
            }

            // Hazırladığım tüm ekranı tek bir I/O işlemiyle konsola basıyorum.
            Console.Write(cizimTamponu.ToString());
            
            // Oyunumun alt bilgi panelini ekrana yazdırıyorum. \x1b[K koduyla satır sonlarındaki hayalet yazıları siliyorum ve kaymayı önlemek için son satırda Write kullanıyorum.
            Console.WriteLine($"{RenkBilgi}Seviye: {seviye} | Skor: {toplamSkor} | Rekor: {Math.Max(enYuksekSkor, toplamSkor)} | Can: {can}/{MaksCan} | {RenkKombo}Kombo: x{komboSayaci}{RenkSifirla}\x1b[K");
            Console.Write($"[* = +1] [O = +3] [{RenkZehir}X = -1 Can{RenkSifirla}] [{RenkCan}H = +1 Can{RenkSifirla}] | Çıkış: Ctrl+C\x1b[K");
        }

        static void KayitTut(string etiket, string mesaj)
        {
            // Ödevde benden istenen 'ETİKET → mesaj' formatına uygun olarak kaydımı oluşturuyorum.
            string kayitSatiri = $"{etiket} → {mesaj}";
            File.AppendAllText(LogDosyasi, kayitSatiri + Environment.NewLine);
        }
    }
}