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
            ApplicationConfiguration.Initialize();
            bool hasHandle = false;
            try
            {
                // 1. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
                try { hasHandle = mutex.WaitOne(TimeSpan.Zero, true); } // Намагаємося захопити м'ютекс. TimeSpan.Zero - миттєва перевірка без очікування.
                catch (AbandonedMutexException) { hasHandle = true; }   // Якщо попередній процес аварійно завершився, м'ютекс вважається покинутим.
                                                                        // Ми його успішно захопили.
                                                                        // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
                if (!hasHandle) { ActivateExistingInstance(); return; } // Тихо закриваємо дублікат, блок finally НЕ викликає ReleaseMutex

                try
                {
                    // 2. ОСНОВНИЙ ЦИКЛ ПРОГРАМИ ІНІЦІАЛІЗАЦІЯ СИСТЕМИ КОНФІГУРАЦІЇ
                    IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Path.Combine("Data", "Conf_config.json"), optional: true, reloadOnChange: true);

                    IConfiguration configuration = builder.Build();
                    Config.Initialize(configuration);
                    Application.Run(new Form1()); // СТАНДАРТНИЙ ЗАПУСК WINFORMS
                }
                catch (Exception ex)
                {
                    // Логуємо критичну помилку, яка заважає запуску вікна
                    Config.LogError("FATAL ERROR DURING STARTUP", ex);

                    // Виводимо просте повідомлення для користувача
                    _ = MessageBox.Show("Critical application startup error.\nDetails can be found in logs/Conf_errors.log",
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
                Process[] processes = Process.GetProcessesByName(current.ProcessName);
                foreach (Process p in processes)
                {
                    if (p.Id != current.Id)
                    {
                        IntPtr hWnd = p.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            if (Win32Api.IsIconic(hWnd))
                            {
                                _ = Win32Api.ShowWindow(hWnd, 9);
                            }

                            _ = Win32Api.SetForegroundWindow(hWnd);
                        }
                        break;
                    }
                }
            }
            catch
            {
                // Захист на випадок збоїв доступу до процесів Windows
            }
        }
    }
}