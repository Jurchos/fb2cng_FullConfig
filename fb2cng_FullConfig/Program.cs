using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace fb2cng_FullConfig
{
    internal static class Program
    {
        private static readonly Mutex mutex = new(true, "fb2cng_Configurator_Unique_Mutex_Key_456");

        [STAThread]
        private static void Main()
        {
            // 1. СУЧАСНЕ НАЛАШТУВАННЯ HIGH DPI
            ApplicationConfiguration.Initialize();

            bool hasHandle; // Просто оголошуємо без присвоєння false;
            try
            {
                // Намагаємося захопити м'ютекс. TimeSpan.Zero - миттєва перевірка без очікування.
                hasHandle = mutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException)
            {
                // Якщо попередній процес аварійно завершився, м'ютекс вважається покинутим.
                // Ми його успішно захопили.
                hasHandle = true;
            }

            // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
            if (!hasHandle)
            {
                ActivateExistingInstance();
                return; // Тихо закриваємо дублікат, блок finally НЕ викликає ReleaseMutex
            }

            try
            {
                // 3. ІНІЦІАЛІЗАЦІЯ СИСТЕМИ КОНФІГУРАЦІЇ
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Path.Combine("Data", "Conf_config.json"), optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();
                Config.Initialize(configuration);

                // 4. СТАНДАРТНИЙ ЗАПУСК WINFORMS
                Application.Run(new Form1());
            }
            finally
            {
                // 5. ЗВІЛЬНЕННЯ М'ЮТЕКСА (Тільки якщо ми ним володіємо)
                if (hasHandle)
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        private static void ActivateExistingInstance() // Метод для активації вже запущеного екземпляра програми
        {
            try
            {
                using Process current = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(current.ProcessName);

                foreach (Process process in processes)
                {
                    if (process.Id != current.Id)
                    {
                        IntPtr hWnd = process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            if (Win32Api.IsIconic(hWnd))
                            {
                                _ = Win32Api.ShowWindow(hWnd, 9);  // 9 = SW_RESTORE
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
    } // КЛАС PROGRAM

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
                ProcessStartInfo psi = new()
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

        public static string ReadYamlValue(string filePath, string key)
        {
            if (!File.Exists(filePath)) return string.Empty;
            string targetKey = $"{key}:";
            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    ReadOnlySpan<char> span = line.AsSpan().TrimStart();
                    // Шукаємо розкоментований ключ
                    if (span.StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        string value = span[targetKey.Length..].Trim().ToString();
                        return value.Trim('"').Trim('\''); // Видаляємо лапки
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        // ОНОВЛЕНИЙ ReplaceYamlValueLine для підтримки закоментування
        private static string[]? ReplaceYamlValueLine(string[] lines, string key, string newValue, bool commentIfEmpty = false)
        {
            bool found = false;
            string targetKey = $"{key}:";

            for (int i = 0; i < lines.Length; i++)
            {
                ReadOnlySpan<char> lineSpan = lines[i];
                ReadOnlySpan<char> trimmed = lineSpan.TrimStart();
                int indentSize = lineSpan.Length - trimmed.Length;
                string padding = lineSpan[..indentSize].ToString();

                // Перевіряємо, чи ми хочемо закоментувати (якщо newValue порожній і прапорець активний)
                if (commentIfEmpty && string.IsNullOrWhiteSpace(newValue))
                {
                    if (trimmed.StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        lines[i] = $"{padding}# {key}: \"mystyle.css\""; // Дефолтне значення
                        found = true;
                        break;
                    }
                    else if (trimmed.StartsWith("#") && trimmed[1..].TrimStart().StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        // Вже закоментовано - нічого не робимо
                        found = true;
                        break;
                    }
                }
                else // Стандартний запис значення
                {
                    if (trimmed.StartsWith("#") && trimmed[1..].TrimStart().StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        lines[i] = $"{padding}{key}: {newValue}";
                        found = true;
                        break;
                    }
                    else if (trimmed.StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        lines[i] = $"{padding}{key}: {newValue}";
                        found = true;
                        break;
                    }
                }
            }

            if (!found && !commentIfEmpty) // Помилка лише якщо не знайшли і не коментуємо
            {
                ShowYamlError(key);
                return null;
            }
            return lines;
        }

        public static bool SaveConfiguration(
            string configName, bool useCustomYaml, string customYamlPath,
            bool useCss, string cssPath,
            bool customSize, string width, string height, string dpi,
            bool useCoverMode, string coverMode,
            bool useNotesMode, string notesMode,
            bool saveTranslit, bool translitVal,
            bool saveOpenCover, bool openCoverVal,
            bool saveFixZip, bool fixZipVal,
            bool useFb2Name,
            int[] fieldIndexes, bool[] folderFlags)
        {
            // 1. Визначаємо шлях до джерела
            string activeSourcePath = sourcePath;
            if (useCustomYaml && !string.IsNullOrWhiteSpace(customYamlPath))
            {
                string fullCustomPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customYamlPath);
                if (File.Exists(fullCustomPath)) activeSourcePath = fullCustomPath;
            }

            // 2. Назва цільового файлу
            string targetFileName = string.IsNullOrWhiteSpace(configName) ? "config.yaml" : configName.Trim();
            if (!targetFileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)) targetFileName += ".yaml";
            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetFileName);

            if (!File.Exists(activeSourcePath))
            {
                if (!ExecuteSyncDumpConfig() || !File.Exists(activeSourcePath)) return false;
            }

            try
            {
                string[]? lines = File.ReadAllLines(activeSourcePath, Encoding.UTF8);

                // --- НОВІ ПАРАМЕТРИ (РАДІОБАТОНИ) ---
                if (saveTranslit)
                {
                    lines = ReplaceYamlValueLine(lines, "file_name_transliterate", translitVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (saveOpenCover)
                {
                    lines = ReplaceYamlValueLine(lines, "open_from_cover", openCoverVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (saveFixZip)
                {
                    lines = ReplaceYamlValueLine(lines, "fix_zip", fixZipVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                // Обробка CSS
                if (useCss)
                {
                    lines = ReplaceYamlValueLine(lines, "stylesheet_path", string.IsNullOrWhiteSpace(cssPath) ? "" : $"\"{cssPath}\"", true);
                    if (lines == null) return false;
                }

                // Обробка розмірів екрану
                if (customSize)
                {
                    lines = ReplaceYamlValueLine(lines, "width", width);
                    if (lines == null) return false; // Захист від null

                    lines = ReplaceYamlValueLine(lines, "height", height);
                    if (lines == null) return false;

                    lines = ReplaceYamlValueLine(lines, "dpi", dpi);
                    if (lines == null) return false;
                }
                if (saveOpenCover)
                {
                    lines = ReplaceYamlValueLine(lines, "open_from_cover", openCoverVal ? "true" : "false");
                    if (lines == null) return false;
                }

                if (saveFixZip)
                {
                    lines = ReplaceYamlValueLine(lines, "fix_zip", fixZipVal ? "true" : "false");
                    if (lines == null) return false;
                }

                if (saveTranslit)
                {
                    lines = ReplaceYamlValueLine(lines, "file_name_transliterate", translitVal ? "true" : "false");
                    if (lines == null) return false;
                }

                // Обробка обкладинки/навігації
                if (useCoverMode)
                {
                    lines = ReplaceYamlValueLine(lines, "toc_type", $"\"{coverMode}\"");
                    if (lines == null) return false;
                }

                // Обробка виносок
                if (useNotesMode)
                {
                    lines = ReplaceYamlValueLine(lines, "mode", $"\"{notesMode}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                // Шаблон назви
                string templateBlock = useFb2Name ? "        {{- .OriginalFileName -}}" : BuildGoTemplateFromUI(fieldIndexes, folderFlags);
                if (!string.IsNullOrEmpty(templateBlock))
                {
                    lines = ReplaceOutputTemplateBlockSafely(lines, templateBlock);
                    if (lines == null) return false;
                }

                File.WriteAllLines(targetPath, lines, Encoding.UTF8);
                Config.SaveSettings();

                // Повідомлення про успіх
                using Form1 tempForm = new();
                var loc = Config.Localization[Config.Settings.CurrentLanguage];
                string cap = loc.GetValueOrDefault("SaveSuccessTitle", "Success");
                string msg = loc.TryGetValue("SaveSuccess", out string? t) ? string.Format(t, targetFileName) : $"Saved to {targetFileName}";
                tempForm.ShowCustomMessageBox(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                using Form1 tempForm = new(); // 1. Сучасний using та спрощений конструктор

                Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

                // 2. Оптимальний пошук заголовка помилки за один крок
                string errorCaption = loc.GetValueOrDefault("SaveErrorTitle", "Save Error");
                string msg;

                // 3. Сучасна та легка для читання перевірка атрибута ReadOnly через .HasFlag
                if (File.Exists(targetPath) && File.GetAttributes(targetPath).HasFlag(FileAttributes.ReadOnly))
                {
                    // 4. Оптимізований пошук перекладу для ReadOnly
                    msg = loc.TryGetValue("ErrReadOnly", out string? template)
                        ? string.Format(template, targetFileName)
                        : $"The file '{targetFileName}' is Read-Only! Remove this attribute to save changes.";
                }
                else
                {
                    // 5. Оптимізований пошук перекладу для AccessDenied
                    msg = loc.TryGetValue("ErrAccessDenied", out string? template)
                        ? string.Format(template, targetFileName)
                        : $"Access to the file '{targetFileName}' is denied! Try running as Admin.";
                }

                _ = tempForm.ShowCustomMessageBox(msg, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static string[]? ReplaceYamlValueLine(string[] lines, string key, string newValue)
        {
            bool found = false;
            string targetKey = $"{key}:"; // Формуємо ключ один раз ДО циклу, щоб не склеювати його на кожній ітерації

            for (int i = 0; i < lines.Length; i++)
            {
                // Створюємо "вікно" (Span) над рядком, що дозволяє аналізувати його без виділення пам'яті
                ReadOnlySpan<char> lineSpan = lines[i];
                ReadOnlySpan<char> trimmed = lineSpan.TrimStart();

                if (trimmed.StartsWith("#"))
                {
                    ReadOnlySpan<char> withoutComment = trimmed[1..].TrimStart(); // Сучасні зрізи (slices) замість Substring
                    if (withoutComment.StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        int hashIndex = lines[i].IndexOf('#');
                        string padding = lines[i][..hashIndex]; // Зріз рядка до символу '#'
                        lines[i] = $"{padding}{key}: {newValue}";
                        found = true;
                        break;
                    }
                }
                else if (trimmed.StartsWith(targetKey, StringComparison.Ordinal))
                {
                    int keyIndex = lines[i].IndexOf(key, StringComparison.Ordinal);
                    string padding = lines[i][..keyIndex]; // Зріз рядка до самого ключа
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
            List<string> result = [];
            bool blockFound = false;
            bool skipOldBlockMode = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string currentLine = lines[i];

                // 1. ВИДАЛЕНО string trimmed — тепер використовуємо швидкий Span без виділення пам'яті
                ReadOnlySpan<char> trimmedSpan = currentLine.AsSpan().TrimStart();

                if (!blockFound && (trimmedSpan.StartsWith("output_name_template:", StringComparison.Ordinal) ||
                   (trimmedSpan.StartsWith("#") && trimmedSpan[1..].TrimStart().StartsWith("output_name_template:", StringComparison.Ordinal))))
                {
                    blockFound = true;
                    int index = currentLine.IndexOf("output_name_template:", StringComparison.Ordinal);
                    if (index == -1)
                    {
                        index = currentLine.IndexOf('#'); // Оптимально шукаємо char, а не string
                    }

                    string padding = index > 0 ? currentLine[..index] : string.Empty;

                    result.Add($"{padding}output_name_template: |");
                    result.Add(newTemplateCode);
                    skipOldBlockMode = true;
                    continue;
                }

                if (skipOldBlockMode)
                {
                    // Спрощуємо перевірку: string.IsNullOrWhiteSpace працює швидше за Trim().Length == 0
                    if (string.IsNullOrWhiteSpace(currentLine))
                    {
                        continue;
                    }

                    // 2. Оптимізуємо підрахунок пробілів через відступ Span
                    int leadingSpaces = currentLine.Length - currentLine.AsSpan().TrimStart().Length;
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
            return [.. result];
        }


        private static string BuildGoTemplateFromUI(int[] fieldIndexes, bool[] folderFlags)
        {
            StringBuilder sb = new();
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

                // ВИЗНАЧЕННЯ: Чи є цей елемент КІНЦЕВИМ у назві файлу (не папка і після нього немає інших полів)
                bool isLastField = !isFolder;
                if (isLastField)
                {
                    for (int next = i + 1; next < 8; next++)
                    {
                        if (fieldIndexes[next] > 0 && !folderFlags[next])
                        {
                            isLastField = false;
                            break;
                        }
                    }
                }

                switch (selIndex)
                {
                    case 1: // Автор
                            // Залишаємо ваш оригінальний рядок (зберігаємо кожну лапку та кожен пробіл відступу)
                        string baseTemplate = "        {{- $author := \"\" -}}\n" +
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

                        // Якщо це папка, ми міняємо конкретне місце на версію зі слешем
                        chunk = isFolder
                                ? baseTemplate.Replace("""{{ printf "%s" $author }}""", """{{ printf "%s/" $author }}""")
                                : baseTemplate;
                        break;

                    case 2: // Серія
                            // Якщо серія є останньою в списку полів, ми ПОВНІСТЮ прибираємо дефіс з блоку {{- else -}}
                        string elseBlock = isLastField ? "        {{-   printf \"\" -}}\n" : "        {{-   printf \" - \" -}}\n";

                        string fullTemplate = "        {{- if gt (len .Series) 0 -}}\n" +
                                              "        {{-   with first .Series -}}\n" +
                                              "        {{-     printf \" {%s} \" .Name -}}\n" +
                                              "        {{-   end -}}\n" +
                                              "        {{- else -}}\n" +
                                              elseBlock +
                                              "        {{- end -}}";

                        if (isFolder)
                        {
                            string updated = fullTemplate.Replace("printf \" {%s} \"", "printf \"%s/\"");
                            chunk = updated.Replace("        {{- else -}}\n" + elseBlock, string.Empty);
                        }
                        else if (isFirst)
                        {
                            string updated = fullTemplate.Replace("printf \" {%s} \"", "printf \"{%s} \"");
                            chunk = updated.Replace("        {{- else -}}\n" + elseBlock, string.Empty);
                        }
                        else
                        {
                            chunk = fullTemplate;
                        }
                        break;

                    case 3: // Назва книги
                            // Замінено regexReplaceAll на надійні послідовні заміни replace, щоб уникнути затирання тексту
                        string cleanLogic = """
                                        {{- $title := .Title -}}
                                        {{- $title = replace "[litres]" "" $title -}}
                                        {{- $title = replace "." "" $title -}}
                                        {{- $title = replace "-" "" $title -}}
                                        {{- $title = trim $title -}}
                                """;

                        chunk = cleanLogic + "\n" +
                                "        {{- if gt (len .Series) 0 -}}\n" +
                                "        {{-   with first .Series -}}\n" +
                                "        {{-     if .Number -}}\n" +
                                "        {{-       printf \"%02d \" .Number -}}\n" +
                                "        {{-     end -}}\n" +
                                "        {{-   end -}}\n" +
                                "        {{- end -}}\n" +
                                (isFolder ? "        {{- printf \"%s/\" $title -}}" : "        {{- $title -}}");
                        break;

                    case 5: // Жанр
                            // Динамічно формуємо внутрішній рядок виводу жанру
                        string genreValue = isFolder
                            ? "        {{-   printf \"%s/\" (index .Genres 0) -}}\n"
                            : "        {{-   index .Genres 0 -}}\n";

                        // Збираємо фінальний шаблон, не дублюючи перевірку довжини масиву
                        chunk = "        {{- if gt (len .Genres) 0 -}}\n" +
                                genreValue +
                                "        {{- end -}}";
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
                        _ = sb.Append('\n');

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

            return sb.Length > 0 ? sb.ToString() : string.Empty;
        }


        private static void ShowYamlError(string key)
        {
            using Form1 tempForm = new(); // 1. Сучасний using без фігурних дужок та скорочений конструктор

            Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

            // 2. Оптимальний пошук заголовка за один крок за допомогою GetValueOrDefault
            string caption = loc.GetValueOrDefault("YamlTitle", "YAML Error");

            // 3. Оптимальний пошук шаблону помилки за допомогою TryGetValue
            string errMsg = loc.TryGetValue("YamlErr", out string? template)
                ? string.Format(template, key)
                : $"Error: Parameter '{key}' was not found in the original config.yaml file!";

            // 4. Виклик кастомного вікна повідомлення
            _ = tempForm.ShowCustomMessageBox(errMsg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
