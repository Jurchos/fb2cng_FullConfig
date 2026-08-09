using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig
{
    internal static class Program
    {
        private static readonly Mutex mutex = new(true, "fb2cng_Configurator_Unique_Mutex_Key_456");

        [STAThread]
        private static void Main()
        {
            // 1. ГЛОБАЛЬНА ОБРОБКА ПОМИЛОК (ставимо на самому початку)
            // Обробка помилок у потоці інтерфейсу (WinForms)
            Application.ThreadException += static (s, e) =>
            {
                Config.LogError("UI Thread Exception", e.Exception);
                _ = MessageBox.Show("An unexpected UI error occurred. Check logs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Обробка помилок у фонових потоках
            AppDomain.CurrentDomain.UnhandledException += static (s, e) =>
            {
                Config.LogError("Global Unhandled Exception", (Exception)e.ExceptionObject);
                _ = MessageBox.Show("A critical background error occurred.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            };

            // Налаштування режиму обробки (щоб WinForms передавав помилки в наш обробник)
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            ApplicationConfiguration.Initialize();
            bool hasHandle = false;
            try
            {
                // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
                try { hasHandle = mutex.WaitOne(TimeSpan.Zero, true); } // Намагаємося захопити м'ютекс. TimeSpan.Zero - миттєва перевірка без очікування.
                catch (AbandonedMutexException) { hasHandle = true; }   // Якщо попередній процес аварійно завершився, м'ютекс вважається покинутим.
                                                                        // Ми його успішно захопили.
                                                                        // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
                if (!hasHandle)                                         // Тихо закриваємо дублікат, блок finally НЕ викликає ReleaseMutex
                {
                    ActivateExistingInstance();
                    return;
                }

                try
                {
                    // 3. ОСНОВНИЙ ЦИКЛ ПРОГРАМИ ІНІЦІАЛІЗАЦІЯ СИСТЕМИ КОНФІГУРАЦІЇ
                    IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Path.Combine(Config.DataFolder, "Conf_config.json"), optional: true, reloadOnChange: true);

                    IConfiguration configuration = builder.Build();
                    Config.Initialize(configuration);
                    Application.Run(new Form1()); // СТАНДАРТНИЙ ЗАПУСК WINFORMS
                }
                catch (Exception ex)
                {
                    // Логуємо критичну помилку, яка заважає запуску вікна
                    Config.LogError("FATAL ERROR DURING STARTUP", ex);

                    // Виводимо просте повідомлення для користувача
                    _ = MessageBox.Show($"Critical application startup error.\nDetails can be found in {Config.LogErrorFile}",
                   "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            finally
            {
                if (hasHandle) // ЗВІЛЬНЕННЯ М'ЮТЕКСА (Тільки якщо ми ним володіємо)
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        private static void ActivateExistingInstance() // Метод активації вже запущеного екземпляра програми
        {
            try
            {
                using Process current = Process.GetCurrentProcess();
                // Отримуємо всі процеси з такою ж назвою
                Process[] processes = Process.GetProcessesByName(current.ProcessName);
                foreach (Process p in processes)
                {
                    if (p.Id != current.Id)
                    {
                        // Перевіряємо, чи це точно наша програма (за шляхом до файлу)
                        // Це важливо, якщо у папці можуть бути інші файли з такою ж назвою
                        IntPtr hWnd = p.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            if (Win32Api.IsIconic(hWnd))            // Якщо згорнуто
                            {
                                _ = Win32Api.ShowWindow(hWnd, 9);   // RESTORE
                            }

                            _ = Win32Api.SetForegroundWindow(hWnd); // На передній план
                        }
                        break;
                    }
                }
            }
            catch
            {
                // Захист на випадок збоїв доступу до процесів Windows, ігноруємо помилки доступу до процесів
            }
        }
    }
}