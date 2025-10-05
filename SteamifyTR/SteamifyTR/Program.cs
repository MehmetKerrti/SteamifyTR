using Steam4NET;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace SteamifyTR
{
    class Program
    {
        private static ISteamClient012 _steamClient012;
        private static ISteamApps001 _steamApps001;
        private const int SecondsBetweenChecks = 15;
        private static BackgroundWorker _bwg;

        static void Main(string[] args)
        {
            Console.Title = "🎮 SteamifyTR | Steam Game Booster";
            Console.Clear();

            // Başlık (normal yazı)
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("SteamifyTR");
            Console.ResetColor();

            // Sağ üst köşeye "Made by guarbey"
            Console.SetCursorPosition(Console.WindowWidth - "Made by guarbey".Length - 1, 0);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Made by guarbey");
            Console.ResetColor();

            // Oyun menüsünü göster
            uint appId = ShowGameMenu();
            Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());

            // Steam bağlantısı
            _bwg = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _bwg.DoWork += BackgroundWork;

            if (!ConnectToSteam())
                return;

            SetConsoleTitleWithGameName(appId);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Steam bağlantısı başarılı!");
            Console.WriteLine($"🎮 Seçilen Oyun ID: {appId}");
            Console.WriteLine("\n🔄 Saat kasma başlatıldı...");
            Console.WriteLine("\nDurdurmak için herhangi bir tuşa basın.\n");
            Console.ResetColor();

            Console.ReadKey();

            if (_bwg.IsBusy)
                _bwg.CancelAsync();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n🛑 İşlem durduruldu. Çıkılıyor...");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        private static uint ShowGameMenu()
        {
            // Menü başlığı kırmızı
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nLütfen boostlamak istediğiniz oyunu seçin:\n");
            Console.ResetColor();

            string[] games = new string[]
            {
                "Counter-Strike 2", "PUBG: Battlegrounds", "Rust", "Grand Theft Auto V",
                "Euro Truck Simulator 2", "Call of Duty: Warzone", "Apex Legends",
                "Dota 2", "Team Fortress 2", "Rainbow Six Siege", "ARK: Survival Ascended",
                "DayZ", "Red Dead Redemption 2", "The Witcher 3: Wild Hunt", "Cyberpunk 2077",
                "FIFA 24", "Minecraft", "Among Us", "Valheim", "Elden Ring", "Hogwarts Legacy",
                "Starfield", "The Last of Us Part I", "Diablo IV", "Horizon Forbidden West",
                "God of War", "Assassin's Creed Valhalla", "Far Cry 6", "Resident Evil Village",
                "Battlefield 2042", "Overwatch 2", "Lost Ark", "Sea of Thieves"
            };

            uint[] appIds = new uint[]
            {
                730, 578080, 252490, 271590, 227300, 1962663, 1172470,
                570, 440, 359550, 2399830, 221100, 1174180, 292030, 1091500,
                1877930, 498940, 945360, 892970, 1245620, 990080,
                1713090, 1887070, 1738980, 1551360, 1593500, 1109140, 552990,
                1517290, 205970, 1599340, 1599340
            };

            for (int i = 0; i < games.Length; i++)
            {
                Console.WriteLine($"[{i + 1}] {games[i]}");
            }
            Console.WriteLine($"[{games.Length + 1}] Diğer (AppID manuel gir)\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Seçiminiz: ");
            Console.ResetColor();

            string choice = Console.ReadLine()?.Trim() ?? "";
            int index;
            if (!int.TryParse(choice, out index) || index < 1 || index > games.Length + 1)
                return AskManualAppId();

            if (index == games.Length + 1)
                return AskManualAppId();

            return appIds[index - 1];
        }

        private static uint AskManualAppId()
        {
            uint appId;
            Console.Write("\nAppID giriniz: ");
            while (!uint.TryParse(Console.ReadLine()?.Trim(), out appId))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("❌ Geçersiz ID. Tekrar girin: ");
                Console.ResetColor();
            }
            return appId;
        }

        private static void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            while (!_bwg.CancellationPending)
                Thread.Sleep(TimeSpan.FromSeconds(SecondsBetweenChecks));
            Environment.Exit(0);
        }

        private static bool ConnectToSteam()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n🔌 Steam'e bağlanılıyor...");
            Console.ResetColor();

            if (!Steamworks.Load(true))
                return ShowError("Steamworks yüklenemedi.");

            _steamClient012 = Steamworks.CreateInterface<ISteamClient012>();
            if (_steamClient012 == null)
                return ShowError("Steam istemcisi oluşturulamadı.");

            int pipe = _steamClient012.CreateSteamPipe();
            if (pipe == 0)
                return ShowError("Steam Pipe oluşturulamadı.");

            int user = _steamClient012.ConnectToGlobalUser(pipe);
            if (user == 0)
                return ShowError("Steam hesabına bağlanılamadı.");

            _steamApps001 = _steamClient012.GetISteamApps<ISteamApps001>(user, pipe);
            if (_steamApps001 == null)
                return ShowError("Steam uygulamaları arayüzü oluşturulamadı.");

            return true;
        }

        private static void SetConsoleTitleWithGameName(uint appId)
        {
            var sb = new StringBuilder(60);
            _steamApps001.GetAppData(appId, "name", sb);
            string gameName = sb.ToString().Trim();
            Console.Title = $"🎮 SteamifyTR - {GetUnicodeString(string.IsNullOrWhiteSpace(gameName) ? "Bilinmeyen Oyun" : gameName)}";
        }

        private static string GetUnicodeString(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            return Encoding.UTF8.GetString(bytes);
        }

        private static bool ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ HATA: {message}");
            Console.ResetColor();
            Console.WriteLine("\nÇıkmak için bir tuşa basın...");
            Console.ReadKey();
            return false;
        }
    }
}
