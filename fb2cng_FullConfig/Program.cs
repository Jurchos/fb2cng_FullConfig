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
            ApplicationConfiguration.Initialize();                 // 1. СУЧАСНЕ НАЛАШТУВАННЯ HIGH DPI
            bool hasHandle;
            try { hasHandle = mutex.WaitOne(TimeSpan.Zero, true); } // Намагаємося захопити м'ютекс. TimeSpan.Zero - миттєва перевірка без очікування.
            catch (AbandonedMutexException) { hasHandle = true; }   // Якщо попередній процес аварійно завершився, м'ютекс вважається покинутим.
                                                                    // Ми його успішно захопили.
                                                                    // 2. ПЕРЕВІРКА НА ДУБЛІКАТ ПРОГРАМИ
            if (!hasHandle) { ActivateExistingInstance(); return; } // Тихо закриваємо дублікат, блок finally НЕ викликає ReleaseMutex

            try
            {
                // 3. ІНІЦІАЛІЗАЦІЯ СИСТЕМИ КОНФІГУРАЦІЇ
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Path.Combine("Data", "Conf_config.json"), optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();
                Config.Initialize(configuration);
                Application.Run(new Form1()); // 4. СТАНДАРТНИЙ ЗАПУСК WINFORMS
            }

            finally
            {
                if (hasHandle) // 5. ЗВІЛЬНЕННЯ М'ЮТЕКСА (Тільки якщо ми ним володіємо)
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
                foreach (var p in processes)
                {
                    if (p.Id != current.Id)
                    {
                        IntPtr hWnd = p.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            if (Win32Api.IsIconic(hWnd)) Win32Api.ShowWindow(hWnd, 9);
                            Win32Api.SetForegroundWindow(hWnd);
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
            if (!IsEngineAvailable()) return false;
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
            catch { return false; }
        }

        // Глобальне зчитування (для CSS та іншого) ---
        public static string ReadYamlValue(string filePath, string key)
        {
            if (!File.Exists(filePath)) return string.Empty;
            string targetKey = $"{key}:";
            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    ReadOnlySpan<char> span = line.AsSpan().TrimStart();
                    // Якщо рядок починається з # — ігноруємо його
                    if (span.StartsWith("#")) continue;

                    if (span.StartsWith(targetKey, StringComparison.Ordinal))
                    {
                        return span[targetKey.Length..].Trim().ToString().Trim('"').Trim('\'');
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        public static string ReadYamlSectionValue(string filePath, string[] sectionPath, string key)
        {
            if (!File.Exists(filePath)) return string.Empty;
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int currentLevel = 0;
                int lastIndent = -1;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    ReadOnlySpan<char> trimmed = line.AsSpan().TrimStart();
                    if (trimmed.IsEmpty || trimmed.StartsWith("#")) continue; // Ігноруємо порожні та коментарі

                    int indent = line.Length - trimmed.Length;

                    if (currentLevel < sectionPath.Length)
                    {
                        if (trimmed.StartsWith(sectionPath[currentLevel], StringComparison.Ordinal))
                        {
                            currentLevel++;
                            lastIndent = indent;
                        }
                    }
                    else
                    {
                        if (indent <= lastIndent && i > 0) break;
                        if (trimmed.StartsWith($"{key}:", StringComparison.Ordinal))
                        {
                            return trimmed[(key.Length + 1)..].Trim().ToString().Trim('"').Trim('\'');
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private static string[]? ReplaceYamlSectionValueLine(string[] lines, string[] sectionPath, string key, string newValue)
        {
            int currentLevel = 0;
            int lastIndent = -1;
            int firstIdx = -1;
            List<int> dupes = [];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                ReadOnlySpan<char> trimmed = line.AsSpan().TrimStart();
                if (trimmed.IsEmpty) continue;
                int indent = line.Length - trimmed.Length;
                ReadOnlySpan<char> clean = trimmed.StartsWith("#") ? trimmed[1..].TrimStart() : trimmed;

                if (currentLevel < sectionPath.Length)
                {
                    if (clean.StartsWith(sectionPath[currentLevel], StringComparison.Ordinal))
                    {
                        currentLevel++;
                        lastIndent = indent;
                    }
                }
                else
                {
                    if (indent <= lastIndent && !trimmed.StartsWith("#")) break;
                    if (clean.StartsWith($"{key}:", StringComparison.Ordinal))
                    {
                        if (firstIdx == -1) firstIdx = i;
                        else dupes.Add(i);
                    }
                }
            }

            if (firstIdx != -1)
            {
                int finalIndent = lines[firstIdx].Length - lines[firstIdx].AsSpan().TrimStart().Length;
                lines[firstIdx] = new string(' ', finalIndent) + $"{key}: {newValue}";
                if (dupes.Count > 0)
                {
                    List<string> list = [.. lines];
                    for (int j = dupes.Count - 1; j >= 0; j--)
                    {
                        list.RemoveAt(dupes[j]);
                    }

                    return [.. list];
                }
                return lines;
            }
            ShowYamlError($"{string.Join("->", sectionPath)}->{key}");
            return null;
        }

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

        private static string[]? ReplaceOutputTemplateBlockSafely(string[] lines, string newTemplateCode)
        {
            List<string> result = [];
            bool blockFound = false; bool skipOldBlockMode = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string currentLine = lines[i];
                ReadOnlySpan<char> trimmedSpan = currentLine.AsSpan().TrimStart();
                if (!blockFound && (trimmedSpan.StartsWith("output_name_template:", StringComparison.Ordinal) ||
                   (trimmedSpan.StartsWith("#") && trimmedSpan[1..].TrimStart().StartsWith("output_name_template:", StringComparison.Ordinal))))
                {
                    blockFound = true;
                    int index = currentLine.IndexOf("output_name_template:", StringComparison.Ordinal);
                    if (index == -1) index = currentLine.IndexOf('#');
                    string padding = index > 0 ? currentLine[..index] : string.Empty;
                    result.Add($"{padding}output_name_template: |");
                    result.Add(newTemplateCode);
                    skipOldBlockMode = true; continue;
                }
                if (skipOldBlockMode)
                {
                    if (string.IsNullOrWhiteSpace(currentLine)) continue;
                    if ((currentLine.Length - currentLine.AsSpan().TrimStart().Length) < 8) skipOldBlockMode = false; else continue;
                }
                result.Add(currentLine);
            }
            if (!blockFound) { ShowYamlError("output_name_template"); return null; }
            return [.. result];
        }

        private static string BuildGoTemplateFromUI(int[] fieldIndexes, bool[] folderFlags)
        {
            StringBuilder sb = new();
            bool isFirst = true;
            for (int i = 0; i < 7; i++)
            {
                int selIndex = fieldIndexes[i];
                if (selIndex <= 0) continue;
                bool isFolder = folderFlags[i];
                string chunk = "";
                bool isLastField = !isFolder;
                if (isLastField)
                {
                    for (int next = i + 1; next < 7; next++)
                        if (fieldIndexes[next] > 0 && !folderFlags[next]) { isLastField = false; break; }
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

                    case 3: // Назва книги Title (з номером серії і очисткою)
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

                    case 4: // Назва книги Title (без номера серії)
                        chunk = isFolder ? "        {{- printf \"%s/\" .Title -}}" : "        {{- .Title -}}";
                        break;

                    case 5: // Мова
                        chunk = isFolder ? "        {{- printf \"%s/\" .Language -}}" : "        {{- .Language -}}";
                        break;

                    case 6: // Жанр
                            // Динамічно формуємо внутрішній рядок виводу жанру
                        string genreValue = isFolder
                            ? "        {{-   printf \"%s/\" (index .Genres 0) -}}\n"
                            : "        {{-   index .Genres 0 -}}\n";

                        // Збираємо фінальний шаблон, не дублюючи перевірку довжини масиву
                        chunk = "        {{- if gt (len .Genres) 0 -}}\n" +
                                genreValue +
                                "        {{- end -}}";
                        break;

                    case 7: // Дата
                        chunk = isFolder ? "        {{- printf \"%s/\" .Date -}}" : "        {{- .Date -}}";
                        break;

                    case 8: // Джерело
                        chunk = isFolder ? "        {{- printf \"%s/\" .SourceFile -}}" : "        {{- .SourceFile -}}";
                        break;

                    case 9: // Книжковий UUID
                        chunk = isFolder ? "        {{- printf \"%s/\" .BookID -}}" : "        {{- .BookID -}}";
                        break;

                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(chunk))
                {
                    if (!isFirst)
                    {
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

                        if (prevValidIndex >= 0 && sb.Append('\n') != null)
                        {
                            int prevSelIndex = fieldIndexes[prevValidIndex];
                            bool prevIsFolder = folderFlags[prevValidIndex];

                            // Додаємо статичні роздільники імені (дефіс чи підкреслення) як красиві окремі рядки YAML
                            if (!prevIsFolder)
                            {
                                if (selIndex == 9 && sb.Append("        {{- printf \"_\" -}}\n") != null)
                                {
                                    // Порожньо, все перенесно в if
                                }
                                else if ((selIndex == 2 && !isFolder) || (prevSelIndex == 2 && !prevIsFolder))
                                {
                                    // Пропускаємо дефіс C#, Go сам розставить пробіли/тире через printf
                                }
                                else if (sb.Append("        {{- printf \" - \" -}}\n") != null)
                                {
                                    // Перенесено в else if
                                }
                            }
                        }
                    }
                    isFirst = sb.Append(chunk) == null;
                }
            }

            return sb.Length > 0 ? sb.ToString() : string.Empty;
        }
        // Допоміжні масиви значень для шаблонів логів (без префіксу logs/)
        public static readonly string[] LogNameValues = [
            "{{ .AppName }}.log",
            "{{- .SourceFile -}}.{{- .Format -}}.log",
            "{{- date \\\"02.01.2006 15.04\\\" .Started -}}...{{- .SourceFile -}}.log",
            "{{- .SourceFile -}}_{{- .Unique -}}.log"
        ];

        public static readonly string[] PanicLogNameValues = [
            "{{ .AppName }}-panic.log",
            "{{- .SourceFile -}}.{{- .Format -}}-panic.log",
            "{{- date \\\"02.01.2006 15.04\\\" .Started -}}...{{- .SourceFile -}}-panic.log",
            "{{- .SourceFile -}}_{{- .Unique -}}-panic.log"
        ];
        // МЕТОД Шукає індекс шаблону, ігноруючи екранування лапок
        public static int GetTemplateIndex(string readValue, string[] patterns)
        {
            if (string.IsNullOrEmpty(readValue)) return -1;

            // Прибираємо всі бекслеші для порівняння (" і \" стають однаковими)
            string normalizedRead = readValue.Replace("\\", "");

            for (int i = 0; i < patterns.Length; i++)
            {
                string normalizedPattern = patterns[i].Replace("\\", "");
                if (normalizedRead.Equals(normalizedPattern, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static string[]? HandleCoverPathLogic(string[] lines, string coverPath)
        {
            List<string> result = new();
            string targetKey = "default_image_path:";
            bool foundSection = false;
            bool alreadyHandled = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                ReadOnlySpan<char> trimmed = line.AsSpan().TrimStart();

                // 1. Шукаємо секцію cover
                if (trimmed.StartsWith("cover:", StringComparison.Ordinal)) foundSection = true;

                // 2. Якщо ми в секції cover і знайшли старий default_image_path (НЕ коментар), видаляємо його
                if (foundSection && trimmed.StartsWith(targetKey, StringComparison.Ordinal))
                {
                    continue; // Пропускаємо цей рядок (видаляємо старий запис)
                }

                result.Add(line);

                // 3. Якщо знайшли generate: ..., вставляємо після нього наш новий шлях (якщо він не пустий)
                if (foundSection && !alreadyHandled && trimmed.StartsWith("generate:", StringComparison.Ordinal))
                {
                    if (!string.IsNullOrWhiteSpace(coverPath))
                    {
                        int indent = line.Length - trimmed.Length;
                        string padding = new string(' ', indent);
                        result.Add($"{padding}default_image_path: \"{coverPath}\"");
                    }
                    alreadyHandled = true;
                }

                // Якщо вийшли з секції (відступ зменшився)
                if (foundSection && i + 1 < lines.Length)
                {
                    int currentIndent = line.Length - trimmed.Length;
                    int nextIndent = lines[i + 1].Length - lines[i + 1].AsSpan().TrimStart().Length;
                    if (nextIndent < currentIndent && !lines[i + 1].AsSpan().TrimStart().IsEmpty) foundSection = false;
                }
            }
            return result.ToArray();
        }

        public static bool SaveConfiguration(
          string configName, bool useCustomYaml, string customYamlPath,
          bool useCss, string cssPath,
          bool useCoverMode, string coverMode,
          bool saveFixZip, bool fixZipVal,
          bool saveOpenCover, bool openCoverVal,
          bool saveTranslit, bool translitVal,
          bool useFb2Name,
          bool useDefaultName,
          int[] fieldIndexes, bool[] folderFlags,
          bool customSize, string width, string height, string dpi,
          bool useNotesMode, string notesMode,
          bool useSoftHyphen, bool softHyphenVal,
          bool useRemoveTransp, bool removeTranspVal,
          bool useJpegQuality, string jpegQuality,
          bool useGenCover, bool genCoverVal, string coverPath,
          bool useResizeCover, string resizeCover,
          bool useAnnEnable, bool annEnableVal,
          bool useAnnInToc, bool annInTocVal,
          bool useTocPlacement, string tocPlacement,
          bool useDropcaps, bool dropcapsVal,
          bool useLogLevel, string logLevel,
          bool useLogName, string logNameTmpl,
          bool usePanicLogName, string panicLogNameTmpl,
          bool useLogMode, string logMode,
          bool useLogFolder, bool logFolderVal)
        {
            //document:
            string activeSourcePath = sourcePath;
            if (useCustomYaml && !string.IsNullOrWhiteSpace(customYamlPath))
            {
                string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customYamlPath);
                if (File.Exists(f)) activeSourcePath = f;
            }

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
                if (lines == null) return false;

                // Обробка параметрів
                if (useCss) lines = ReplaceYamlValueLine(lines, "stylesheet_path", string.IsNullOrWhiteSpace(cssPath) ? "" : $"\"{cssPath}\"", true);
                if (lines == null) return false;
                if (useCoverMode) lines = ReplaceYamlValueLine(lines, "toc_type", $"\"{coverMode}\"");
                if (lines == null) return false;
                if (saveFixZip) lines = ReplaceYamlValueLine(lines, "fix_zip", fixZipVal ? "true" : "false");
                if (lines == null) return false;
                if (saveOpenCover) lines = ReplaceYamlValueLine(lines, "open_from_cover", openCoverVal ? "true" : "false");
                if (lines == null) return false;
                if (saveTranslit) lines = ReplaceYamlValueLine(lines, "file_name_transliterate", translitVal ? "true" : "false");
                if (lines == null) return false;

                string templateBlock = "";

                if (useDefaultName)
                {
                    templateBlock =
                        "        {{- $all := \"\" -}}\n" +
                        "        {{- if gt (len .Authors) 0 -}}\n" +
                        "        {{-   with first .Authors -}}\n" +
                        "        {{-     $all = .LastName -}}\n" +
                        "        {{-     if .FirstName }}{{ $all = (cat $all .FirstName) }}{{- end -}}\n" +
                        "        {{-     if .MiddleName }}{{ $all = (cat $all .MiddleName) }}{{- end -}}\n" +
                        "        {{-     if and (not $all) .Nickname }}{{ $all = .Nickname }}{{- end -}}\n" +
                        "        {{-   end -}}\n" +
                        "        {{-   if gt (len .Authors) 1 -}}\n" +
                        "        {{-     if eq .Language \"ru\" }}{{ $all = (cat $all \"и др\") }}{{- else -}}{{ $all = (printf \"%s%s\" $all \", et al\") }}{{- end -}}\n" +
                        "        {{-   end -}}\n" +
                        "        {{-   $all = cat $all \"-\" -}}\n" +
                        "        {{- end -}}\n" +
                        "        {{- if $all -}}\n" +
                        "        {{-   cat $all .Title -}}\n" +
                        "        {{- else -}}\n" +
                        "        {{-   .Title -}}\n" +
                        "        {{- end -}}";
                }
                else if (useFb2Name)
                {
                    templateBlock = "        {{- .OriginalFileName -}}";
                }
                else
                {
                    templateBlock = BuildGoTemplateFromUI(fieldIndexes, folderFlags);
                }

                //==================
                //metainformation:
                if (customSize)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "width", width);
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "height", height);
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "dpi", dpi);
                }
                if (useNotesMode) lines = ReplaceYamlSectionValueLine(lines!, ["footnotes:"], "mode", $"\"{notesMode}\"");
                if (lines == null) return false;
                if (useSoftHyphen) lines = ReplaceYamlValueLine(lines, "insert_soft_hyphen", softHyphenVal ? "true" : "false");

                if (useRemoveTransp)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "remove_transparency", removeTranspVal ? "true" : "false");
                }

                if (useJpegQuality) lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "jpeg_quality_level", jpegQuality);

                if (useGenCover)
                {
                    // 1. Спочатку оновлюємо статус generate
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "cover:"], "generate", genCoverVal ? "true" : "false");
                    if (lines == null) return false;

                    // 2. Викликаємо нову логіку для шляху до картинки
                    lines = HandleCoverPathLogic(lines, coverPath);
                    if (lines == null) return false;
                }

                if (useResizeCover) lines = ReplaceYamlSectionValueLine(lines!, ["images:", "cover:"], "resize", $"\"{resizeCover}\"");

                if (useAnnEnable) lines = ReplaceYamlSectionValueLine(lines!, ["annotation:"], "enable", annEnableVal ? "true" : "false");

                if (useAnnInToc) lines = ReplaceYamlSectionValueLine(lines!, ["annotation:"], "in_toc", annInTocVal ? "true" : "false");

                if (useTocPlacement) lines = ReplaceYamlSectionValueLine(lines!, ["toc_page:"], "placement", $"\"{tocPlacement}\"");

                if (useDropcaps) lines = ReplaceYamlSectionValueLine(lines!, ["dropcaps:"], "enable", dropcapsVal ? "true" : "false");

                //===============
                // logging:
                string[] fileSec = ["logging:", "file:"];
                if (useLogLevel) lines = ReplaceYamlSectionValueLine(lines!, fileSec, "level", logLevel);
                if (lines == null) return false;
                if (useLogMode) lines = ReplaceYamlSectionValueLine(lines!, fileSec, "mode", logMode);
                if (lines == null) return false;

                // визначення префікса папки logs/ для шаблонів логів, якщо чекбокс активний
                string prefix = "";
                if (useLogFolder)
                {
                    // чекбокс активний — беремо значення з радіобатонів
                    prefix = logFolderVal ? "logs/" : "";
                }
                else
                {
                    // Якщо чекбокс НЕ активний — перевіряємо, чи була папка в оригінальному файлі
                    string existingVal = ReadYamlSectionValue(activeSourcePath, fileSec, "destination_template");
                    if (existingVal.StartsWith("logs/")) prefix = "logs/";
                }

                if (useLogName)
                {
                    lines = ReplaceYamlSectionValueLine(lines, fileSec, "destination_template", $"\"{prefix}{logNameTmpl}\"");
                    if (lines == null) return false;
                }
                if (usePanicLogName)
                {
                    lines = ReplaceYamlSectionValueLine(lines, fileSec, "panic_destination_template", $"\"{prefix}{panicLogNameTmpl}\"");
                    if (lines == null) return false;
                }

                // 2. Викликаємо заміну в YAML (тільки один раз!)
                if (!string.IsNullOrEmpty(templateBlock))
                {
                    lines = ReplaceOutputTemplateBlockSafely(lines, templateBlock);
                }

                // 3. Перевірка на помилку (якщо ключ не знайдено)
                if (lines == null)
                {
                    return false;
                }

                // 4. Запис у файл
                File.WriteAllLines(targetPath, lines, Encoding.UTF8);
                Config.SaveSettings();

                // 5. Повідомлення про успіх
                using Form1 tempForm = new();
                var loc = Config.Localization[Config.Settings.CurrentLanguage];
                string cap = loc.GetValueOrDefault("SaveSuccessTitle", "Success");
                string msg = loc.TryGetValue("SaveSuccess", out string? t) ? string.Format(t, targetFileName) : $"Saved to {targetFileName}";
                tempForm.ShowCustomMessageBox(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                using Form1 tempForm = new();
                tempForm.ShowCustomMessageBox("Access Denied! Check file attributes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch { return false; }
        }
        private static void ShowYamlError(string key)
        {
            using Form1 tempForm = new();
            var loc = Config.Localization[Config.Settings.CurrentLanguage];
            string caption = loc.GetValueOrDefault("YamlTitle", "YAML Error");
            string errMsg = loc.TryGetValue("YamlErr", out string? template) ? string.Format(template, key) : $"Error: Parameter '{key}' was not found!";
            tempForm.ShowCustomMessageBox(errMsg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}