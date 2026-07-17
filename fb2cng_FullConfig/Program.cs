using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using fb2cng_FullConfig.Settings;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace fb2cng_FullConfig
{
    internal static class Program
    {
        private static readonly Mutex mutex = new Mutex(true, "fb2cng_Configurator_Unique_Mutex_Key_456");
        //internal static readonly object YamlService;

        [STAThread]
        private static void Main()
        {
            // 1. СУЧАСНЕ НАЛАШТУВАННЯ HIGH DPI ДЛЯ .NET 10
            ApplicationConfiguration.Initialize();

            // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ (Mutex логіка)
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    Process current = Process.GetCurrentProcess();
                    Process[] processes = Process.GetProcessesByName(current.ProcessName);

                    foreach (Process process in processes)
                    {
                        if (process.Id != current.Id)
                        {
                            IntPtr hWnd = process.MainWindowHandle;
                            if (hWnd != IntPtr.Zero)
                            {
                                if (IsIconic(hWnd))
                                {
                                    _ = ShowWindow(hWnd, 9); // 9 = SW_RESTORE
                                }
                                _ = SetForegroundWindow(hWnd);
                            }
                            break;
                        }
                    }
                }
                catch
                {
                    // Захист на випадок збоїв доступу до процесів Windows
                }

                return; // Тихо закриваємо дублікат
            }

            try
            {
                // 3. ІНІЦІАЛІЗАЦІЯ СИСТЕМИ КОНФІГУРАЦІЇ (Новий блок)
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();

                // Ініціалізуємо наш статичний Config
                Config.Initialize(configuration);

                // 4. СТАНДАРТНИЙ ЗАПУСК WINFORMS
                Application.Run(new Form1());
            }
            finally
            {
                // 5. ЗВІЛЬНЕННЯ М'ЮТЕКСА
                GC.KeepAlive(mutex);
                mutex.ReleaseMutex();
            }
        }

        // --- СИСТЕМНІ ІМПОРТИ ДЛЯ WINDOWS API ---
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
    } // ТУТ ЗАКРИВАЄТЬСЯ КЛАС PROGRAM

    // =========================================================================
    // ВНУТРІШНЯ БІЗНЕС-ЛОГІКА ПРОГРАМИ (РОБОТА З YAML ТА ПРОЦЕСАМИ)
    // =========================================================================
    public static class YamlService
    {
        private static readonly string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fbc.exe");
            private static readonly string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yaml");

            public static bool IsEngineAvailable()
            {
                return File.Exists(exePath);
            }

            public static bool ExecuteSyncDumpConfig()
            {
                if (!IsEngineAvailable())
                {
                    return false;
                }

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = "dumpconfig --default config.yaml",
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                using Process? proc = Process.Start(psi);
                    if (proc != null)
                    {
                        // Очікуємо завершення максимум 5 секунд (5000 мілісекунд)
                        if (!proc.WaitForExit(5000))
                        {
                            // Якщо fbc.exe завис або не встиг, примусово вбиваємо його дочірній процес
                            proc.Kill();
                            return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool SaveConfiguration(
                string configName, bool useCss, string cssPath, bool translit,
                bool customSize, string width, string height, string dpi,
                bool useCoverMode, string coverMode, bool useNotesMode, string notesMode,
                bool openFromCover, bool fixZip, bool useFb2Name,
                int[] fieldIndexes, bool[] folderFlags)
            {
            string targetFileName = configName?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(targetFileName))
                {
                    targetFileName = "config.yaml";
                }

                if (!targetFileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
                {
                    targetFileName += ".yaml";
                }

                string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetFileName);

                if (!File.Exists(sourcePath))
                {
                    bool generated = ExecuteSyncDumpConfig();
                    if (!generated || !File.Exists(sourcePath))
                    {
                        return false;
                    }
                }

                try
                {
                string[]? lines = File.ReadAllLines(sourcePath, Encoding.UTF8);

                    if (useCss)
                    {
                        lines = ReplaceYamlValueLine(lines, "stylesheet_path", $"\"{cssPath}\"");
                        if (lines == null)
                        {
                            return false;
                        }
                    }

                    string newTranslitValue = translit ? "true" : "false";
                    lines = ReplaceYamlValueLine(lines, "file_name_transliterate", newTranslitValue);
                    if (lines == null)
                    {
                        return false;
                    }

                    if (customSize)
                    {
                        lines = ReplaceYamlValueLine(lines, "width", width); if (lines == null)
                        {
                            return false;
                        }

                        lines = ReplaceYamlValueLine(lines, "height", height); if (lines == null)
                        {
                            return false;
                        }

                        lines = ReplaceYamlValueLine(lines, "dpi", dpi); if (lines == null)
                        {
                            return false;
                        }
                    }

                    if (useCoverMode)
                    {
                        lines = ReplaceYamlValueLine(lines, "toc_type", $"\"{coverMode}\""); if (lines == null)
                        {
                            return false;
                        }
                    }
                    if (useNotesMode)
                    {
                        lines = ReplaceYamlValueLine(lines, "mode", $"\"{notesMode}\""); if (lines == null)
                        {
                            return false;
                        }
                    }
                    if (openFromCover)
                    {
                        lines = ReplaceYamlValueLine(lines, "open_from_cover", "true"); if (lines == null)
                        {
                            return false;
                        }
                    }
                    if (fixZip)
                    {
                        lines = ReplaceYamlValueLine(lines, "fix_zip", "true"); if (lines == null)
                        {
                            return false;
                        }
                    }

                    string templateBlock = useFb2Name ? "        {{- .OriginalFileName -}}" : BuildGoTemplateFromUI(fieldIndexes, folderFlags);

                    if (!string.IsNullOrEmpty(templateBlock))
                    {
                        lines = ReplaceOutputTemplateBlockSafely(lines, templateBlock);
                        if (lines == null)
                        {
                            return false;
                        }
                    }

                    File.WriteAllLines(targetPath, lines, Encoding.UTF8);
                    Config.SaveSettings();

                    // Викликаємо вікна через тимчасову форму (Варіант 1)
                    using (var tempForm = new Form1())
                    {
                        Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];
                        string successCaption = loc.ContainsKey("SaveSuccessTitle") ? loc["SaveSuccessTitle"] : "Success";
                        _ = tempForm.ShowCustomMessageBox(string.Format(loc["SaveSuccess"], targetFileName), successCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return true;
                }
                catch (UnauthorizedAccessException) // ПЕРЕХОПЛЮЄМО САМЕ ПОМИЛКУ ДОСТУПУ
                {
                    using (var tempForm = new Form1())
                    {
                        Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];
                        string errorCaption = loc.ContainsKey("SaveErrorTitle") ? loc["SaveErrorTitle"] : "Save Error";
                        string msg; // Оголошуємо без присвоєння порожнього рядка "", щоб VS не сварилася

                        // Перевіряємо, чи існує файл і чи є у нього атрибут ReadOnly
                        if (File.Exists(targetPath) && (File.GetAttributes(targetPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            // Якщо файл просто "Тільки для читання"
                            msg = loc.ContainsKey("ErrReadOnly")
                                ? string.Format(loc["ErrReadOnly"], targetFileName)
                                : $"The file '{targetFileName}' is Read-Only! Remove this attribute to save changes.";
                        }
                        else
                        {
                            // Якщо справа дійсно у правах доступу системи / Адміністратора
                            msg = loc.ContainsKey("ErrAccessDenied")
                                ? string.Format(loc["ErrAccessDenied"], targetFileName)
                                : $"Access to the file '{targetFileName}' is denied! Try running as Admin.";
                        }

                        _ = tempForm.ShowCustomMessageBox(msg, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }
            }

        private static string[]? ReplaceYamlValueLine(string[] lines, string key, string newValue)
        {
            bool found = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimStart();

                if (trimmed.StartsWith("#"))
                {
                    string withoutComment = trimmed.Substring(1).TrimStart();
                    if (withoutComment.StartsWith(key + ":"))
                    {
                        string padding = lines[i].Substring(0, lines[i].IndexOf('#'));
                        lines[i] = $"{padding}{key}: {newValue}";
                        found = true;
                        break;
                    }
                }
                else if (trimmed.StartsWith(key + ":"))
                {
                    string padding = lines[i].Substring(0, lines[i].IndexOf(key));
                    lines[i] = $"{padding}{key}: {newValue}";
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                ShowYamlError(key);
                return null; // Повертаємо null, щоб викликаючий код зупинив збереження
            }
            return lines;
        }
        private static string[]? ReplaceOutputTemplateBlockSafely(string[] lines, string newTemplateCode)
            {
                List<string> result = new List<string>();
                bool blockFound = false;
                bool skipOldBlockMode = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string currentLine = lines[i];
                    string trimmed = currentLine.TrimStart();

                    if (!blockFound && (trimmed.StartsWith("output_name_template:") || (trimmed.StartsWith("#") && trimmed.Substring(1).TrimStart().StartsWith("output_name_template:"))))
                    {
                        blockFound = true;
                        int index = currentLine.IndexOf("output_name_template:");
                        if (index == -1)
                        {
                            index = currentLine.IndexOf("#");
                        }

                        string padding = index > 0 ? currentLine.Substring(0, index) : "";

                        result.Add($"{padding}output_name_template: |");
                        result.Add(newTemplateCode);
                        skipOldBlockMode = true;
                        continue;
                    }

                    if (skipOldBlockMode)
                    {
                        if (string.IsNullOrEmpty(currentLine) || currentLine.Trim().Length == 0)
                        {
                            continue;
                        }

                        int leadingSpaces = currentLine.Length - currentLine.TrimStart().Length;
                        if (leadingSpaces < 8)
                        {
                            skipOldBlockMode = false;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    result.Add(currentLine);
                }

                if (!blockFound)
                {
                    ShowYamlError("output_name_template");
                    return null;
                }
                return result.ToArray();
            }

            private static string BuildGoTemplateFromUI(int[] fieldIndexes, bool[] folderFlags)
            {
                StringBuilder sb = new StringBuilder();
                bool isFirst = true;

                for (int i = 0; i < 8; i++)
                {
                    int selIndex = fieldIndexes[i];

                    // Порожні елементи просто пропускаємо
                    if (selIndex <= 0)
                    {
                        continue;
                    }

                    bool isFolder = folderFlags[i];
                    string chunk = "";

                    switch (selIndex)
                    {
                        case 1: // Автор
                            if (isFolder)
                            {
                                chunk = "        {{- $author := \"\" -}}\n" +
                                        "        {{- if gt (len .Authors) 0 -}}\n" +
                                        "        {{-   with first .Authors -}}\n" +
                                        "        {{-     if .LastName -}}\n" +
                                        "        {{-       $author = .LastName -}}\n" +
                                        "        {{-       if .FirstName }}{{ $author = printf \"%s %s\" $author .FirstName }}{{ end -}}\n" +
                                        "        {{-       if .MiddleName }}{{ $author = printf \"%s %s\" $author .MiddleName }}{{ end -}}\n" +
                                        "        {{-     else if .Nickname -}}\n" +
                                        "        {{-       $author = .Nickname -}}\n" +
                                        "        {{-     end -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{-   if gt (len .Authors) 1 -}}\n" +
                                        "        {{-     if eq .Language \"ru\" -}}\n" +
                                        "        {{-       $author = printf \"%s %s\" $author \"и др\" -}}\n" +
                                        "        {{-     else -}}\n" +
                                        "        {{-       $author = printf \"%s %s\" $author \", et al\" -}}\n" +
                                        "        {{-     end -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{- end -}}\n" +
                                        "        {{- if $author }}{{ printf \"%s/\" $author }}{{ end }}";
                            }
                            else
                            {
                                chunk = "        {{- $author := \"\" -}}\n" +
                                        "        {{- if gt (len .Authors) 0 -}}\n" +
                                        "        {{-   with first .Authors -}}\n" +
                                        "        {{-     if .LastName -}}\n" +
                                        "        {{-       $author = .LastName -}}\n" +
                                        "        {{-       if .FirstName }}{{ $author = printf \"%s %s\" $author .FirstName }}{{ end -}}\n" +
                                        "        {{-       if .MiddleName }}{{ $author = printf \"%s %s\" $author .MiddleName }}{{ end -}}\n" +
                                        "        {{-     else if .Nickname -}}\n" +
                                        "        {{-       $author = .Nickname -}}\n" +
                                        "        {{-     end -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{-   if gt (len .Authors) 1 -}}\n" +
                                        "        {{-     if eq .Language \"ru\" -}}\n" +
                                        "        {{-       $author = printf \"%s %s\" $author \"и др\" -}}\n" +
                                        "        {{-     else -}}\n" +
                                        "        {{-       $author = printf \"%s %s\" $author \", et al\" -}}\n" +
                                        "        {{-     end -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{- end -}}\n" +
                                        "        {{- if $author }}{{ printf \"%s\" $author }}{{ end }}";
                            }
                            break;

                        case 2: // Серія
                            if (isFolder)
                            {
                                chunk = "        {{- if gt (len .Series) 0 -}}\n" +
                                        "        {{-   with first .Series -}}\n" +
                                        "        {{-     printf \"%s/\" .Name -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{- end -}}";
                            }
                            else if (isFirst)
                            {
                                chunk = "        {{- if gt (len .Series) 0 -}}\n" +
                                        "        {{-   with first .Series -}}\n" +
                                        "        {{-     printf \"{%s} \" .Name -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{- end -}}";
                            }
                            else
                            {
                                chunk = "        {{- if gt (len .Series) 0 -}}\n" +
                                        "        {{-   with first .Series -}}\n" +
                                        "        {{-     printf \" {%s} \" .Name -}}\n" +
                                        "        {{-   end -}}\n" +
                                        "        {{- else -}}\n" +
                                        "        {{-   printf \" - \" -}}\n" +
                                        "        {{- end -}}";
                            }
                            break;

                        case 3: // Назва книги
                            chunk = "        {{- if gt (len .Series) 0 -}}\n" +
                                    "        {{-   with first .Series -}}\n" +
                                    "        {{-     if .Number -}}\n" +
                                    "        {{-       printf \"%02d \" .Number -}}\n" +
                                    "        {{-     end -}}\n" +
                                    "        {{-   end -}}\n" +
                                    "        {{- end -}}\n" +
                                    (isFolder ? "        {{- printf \"%s/\" .Title -}}" : "        {{- .Title -}}");
                            break;

                        case 4: // Мова
                            chunk = isFolder ? "        {{- printf \"%s/\" .Language -}}" : "        {{- .Language -}}";
                            break;

                        case 5: // Жанр
                            if (isFolder)
                            {
                                chunk = "        {{- if gt (len .Genres) 0 -}}\n" +
                                        "        {{-   printf \"%s/\" (index .Genres 0) -}}\n" +
                                        "        {{- end -}}";
                            }
                            else
                            {
                                chunk = "        {{- if gt (len .Genres) 0 -}}\n" +
                                        "        {{-   index .Genres 0 -}}\n" +
                                        "        {{- end -}}";
                            }
                            break;

                        case 6: // Дата
                            chunk = isFolder ? "        {{- printf \"%s/\" .Date -}}" : "        {{- .Date -}}";
                            break;

                        case 7: // Джерело
                            chunk = isFolder ? "        {{- printf \"%s/\" .SourceFile -}}" : "        {{- .SourceFile -}}";
                            break;

                        case 8: // Книжковий UUID
                            chunk = isFolder ? "        {{- printf \"%s/\" .BookID -}}" : "        {{- .BookID -}}";
                            break;

                        case 9: // Скорочений UUID
                            string shortUuid = "        {{- substr 0 2 .BookID -}}";
                            chunk = isFolder ? shortUuid + "/" : shortUuid;
                            break;
                        default:
                            break;
                    }

                    if (!string.IsNullOrEmpty(chunk))
                    {
                        if (!isFirst)
                        {
                            _ = sb.Append("\n");

                            // Шукаємо реальний попередній вибраний елемент у масиві
                            int prevValidIndex = -1;
                            for (int k = i - 1; k >= 0; k--)
                            {
                                if (fieldIndexes[k] > 0)
                                {
                                    prevValidIndex = k;
                                    break;
                                }
                            }

                            if (prevValidIndex >= 0)
                            {
                                int prevSelIndex = fieldIndexes[prevValidIndex];
                                bool prevIsFolder = folderFlags[prevValidIndex];

                                // Додаємо статичні роздільники імені (дефіс чи підкреслення) як красиві окремі рядки YAML
                                if (!prevIsFolder)
                                {
                                    if (selIndex == 9)
                                    {
                                        _ = sb.Append("        {{- printf \"_\" -}}\n");
                                    }
                                    else if ((selIndex == 2 && !isFolder) || (prevSelIndex == 2 && !prevIsFolder))
                                    {
                                        // Пропускаємо дефіс C#, Go сам розставить пробіли/тире через printf
                                    }
                                    else
                                    {
                                        _ = sb.Append("        {{- printf \" - \" -}}\n");
                                    }
                                }
                            }
                        }

                        _ = sb.Append(chunk);
                        isFirst = false;
                    }
                }

                return sb.Length > 0 ? sb.ToString() : "";
            }


            private static void ShowYamlError(string key)
            {
                using (var tempForm = new Form1())
                {
                    Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

                    // 1. Отримуємо локалізований заголовок ("YAML Error", "Помилка YAML" тощо)
                    string caption = loc.ContainsKey("YamlTitle") ? loc["YamlTitle"] : "YAML Error";

                    // 2. Отримуємо локалізований текст помилки з автоматичною підстановкою назви параметра
                    string errMsg = loc.ContainsKey("YamlErr")
                        ? string.Format(loc["YamlErr"], key)
                        : $"Error: Parameter '{key}' was not found in the original config.yaml file!";

                    // 3. Викликаємо наше кастомне вікно з компактною іконкою по центру
                    _ = tempForm.ShowCustomMessageBox(errMsg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
    }
}
