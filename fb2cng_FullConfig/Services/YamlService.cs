using System.Diagnostics;
using System.Text;

namespace fb2cng_FullConfig.Services
{
    public static class YamlService
    {
        private static readonly string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fbc.exe");
        private static readonly string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.DataFolder, Config.ConfigFileName);

        public static bool IsEngineAvailable()
        {
            return File.Exists(exePath);
        }

        public static bool ExecuteSyncDumpConfig()
        {
            if (!IsEngineAvailable())
            {
                Config.LogError("Execution failed: fbc.exe not found in working directory.");
                return false;
            }

            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = exePath,
                    Arguments = $"dumpconfig --default \"{Config.DefaultConfigPath}\"",
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
                        Config.LogError("fbc.exe dumpconfig timed out (5s)"); // Лог тайм-ауту
                        return false;
                    }
                    if (proc.ExitCode != 0)
                    {
                        Config.LogError($"fbc.exe exited with error code: {proc.ExitCode}"); // Лог помилки процесу
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Config.LogError("Critical error during ExecuteSyncDumpConfig", ex); // Лог системної помилки
                return false;
            }
        }

        // Глобальне зчитування (для CSS та іншого) ---
        public static string ReadYamlValue(string filePath, string key)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            string targetKey = $"{key}:";
            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    ReadOnlySpan<char> span = line.AsSpan().TrimStart();
                    // Якщо рядок починається з # — ігноруємо його
                    if (span.StartsWith("#"))
                    {
                        continue;
                    }

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
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int currentLevel = 0;
                int lastIndent = -1;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    ReadOnlySpan<char> trimmed = line.AsSpan().TrimStart();
                    if (trimmed.IsEmpty || trimmed.StartsWith("#"))
                    {
                        continue; // Ігноруємо порожні та коментарі
                    }

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
                        if (indent <= lastIndent && i > 0)
                        {
                            break;
                        }

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
                if (trimmed.IsEmpty)
                {
                    continue;
                }

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
                    if (indent <= lastIndent && !trimmed.StartsWith("#"))
                    {
                        break;
                    }

                    if (clean.StartsWith($"{key}:", StringComparison.Ordinal))
                    {
                        if (firstIdx == -1)
                        {
                            firstIdx = i;
                        }
                        else
                        {
                            dupes.Add(i);
                        }
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
                    if (index == -1)
                    {
                        index = currentLine.IndexOf('#');
                    }

                    string padding = index > 0 ? currentLine[..index] : string.Empty;
                    result.Add($"{padding}output_name_template: |");
                    result.Add(newTemplateCode);
                    skipOldBlockMode = true; continue;
                }
                if (skipOldBlockMode)
                {
                    if (string.IsNullOrWhiteSpace(currentLine))
                    {
                        continue;
                    }

                    if ((currentLine.Length - currentLine.AsSpan().TrimStart().Length) < 8) { skipOldBlockMode = false; }
                    else
                    {
                        continue;
                    }
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
                if (selIndex <= 0)
                {
                    continue;
                }

                bool isFolder = folderFlags[i];
                string chunk = "";
                bool isLastField = !isFolder;
                if (isLastField)
                {
                    for (int next = i + 1; next < 7; next++)
                    {
                        if (fieldIndexes[next] > 0 && !folderFlags[next]) { isLastField = false; break; }
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

                    case 3: // Назва книги Title (з номером серії і очисткою)
                            // Замінено regexReplaceAll на надійні послідовні заміни replace, щоб уникнути затирання тексту
                        string cleanLogic = """
                                        {{- $title := .Title -}}
                                        {{- $title = replace "[litres]" "" $title -}}
                                        {{- $title = replace "_litres" "" $title -}}
                                        {{- $title = replace "[Литрес]" "" $title -}}
                                        {{- $title = replace "(flibusta)" "" $title -}}
                                        {{- $title = replace "_flibusta" "" $title -}}
                                        {{- $title = replace "(Самиздат)" "" $title -}}
                                        {{- $title = replace "[author.today]" "" $title -}}
                                        {{- $title = replace "[mybook]" "" $title -}}
                                        {{- $title = replace "[ficbook]" "" $title -}}
                                        {{- $title = replace "[knigogo.net]" "" $title -}}
                                        {{- $title = replace "_royal_lib_ru" "" $title -}}
                                        {{- $title = replace "CoolLib_net" "" $title -}}
                                        {{- $title = replace "lib_ru" "" $title -}}
                                        {{- $title = replace "_fb2" "" $title -}}
                                        {{- $title = replace ".fb2" "" $title -}}
                                        {{- $title = replace "(fb2)" "" $title -}}
                                        {{- $title = replace ".zip" "" $title -}}
                                        {{- $title = replace "[L]" "" $title -}}
                                        {{- $title = replace "_full" "" $title -}}
                                        {{- $title = replace "___" " " $title -}}
                                        {{- $title = replace "__" " " $title -}}
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
                    _ = sb.Append(chunk); isFirst = false;
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
            if (string.IsNullOrEmpty(readValue))
            {
                return -1;
            }

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

        private static string[] HandleCoverPathLogic(string[] lines, string coverPath)
        {
            List<string> result = [];
            string targetKey = "default_image_path:";
            bool foundSection = false;
            bool alreadyHandled = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                ReadOnlySpan<char> trimmed = line.AsSpan().TrimStart();

                // 1. Шукаємо секцію cover
                if (trimmed.StartsWith("cover:", StringComparison.Ordinal))
                {
                    foundSection = true;
                }

                // 2. Якщо знайшли старий шлях, видаляємо його
                if (foundSection && trimmed.StartsWith(targetKey, StringComparison.Ordinal))
                {
                    continue;
                }

                result.Add(line);

                // 3. Вставляємо новий шлях після generate:
                if (foundSection && !alreadyHandled && trimmed.StartsWith("generate:", StringComparison.Ordinal))
                {
                    if (!string.IsNullOrWhiteSpace(coverPath))
                    {
                        int indent = line.Length - trimmed.Length;
                        string padding = new(' ', indent);
                        result.Add($"{padding}default_image_path: \"{coverPath}\"");
                    }
                    alreadyHandled = true;
                }

                // 4. Перевірка на вихід із секції (робимо лише для НЕпустих рядків)
                if (foundSection && !trimmed.IsEmpty && i + 1 < lines.Length)
                {
                    ReadOnlySpan<char> nextTrimmed = lines[i + 1].AsSpan().TrimStart();
                    if (!nextTrimmed.IsEmpty)
                    {
                        int currentIndent = line.Length - trimmed.Length;
                        int nextIndent = lines[i + 1].Length - nextTrimmed.Length;
                        if (nextIndent < currentIndent)
                        {
                            foundSection = false;
                        }
                    }
                }
            }
            return [.. result];
        }

        private static string[] ProcessVignettes(string[] lines, bool useVignettes, bool[] items)
        {
            List<string> result = [];
            string[] keys = ["title_top", "title_bottom", "title_top", "title_bottom", "end", "title_top", "title_bottom", "end"];
            int keyIdx = 0;
            bool inVignettes = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.TrimStart();

                if (trimmed.Contains("vignettes:"))
                {
                    inVignettes = true;
                    result.Add(useVignettes ? "    vignettes:" : "    # vignettes:");
                    continue;
                }

                // Вихід із секції віньєток
                if (inVignettes && (trimmed.Contains("dropcaps:") || (line.Length - trimmed.Length < 4 && trimmed.Length > 0 && !trimmed.StartsWith('#'))))
                {
                    inVignettes = false;
                }

                if (inVignettes)
                {
                    if (trimmed.Contains("book:") || trimmed.Contains("chapter:") || trimmed.Contains("section:"))
                    {
                        result.Add(useVignettes ? line.Replace("#", "") : (trimmed.StartsWith('#') ? line : "    # " + trimmed));
                    }
                    else if (keyIdx < keys.Length && trimmed.Contains(keys[keyIdx] + ":"))
                    {
                        bool active = useVignettes && items[keyIdx];
                        string clean = trimmed.Replace("#", "").Trim();
                        result.Add(active ? $"        {clean}" : $"        # {clean}");
                        keyIdx++;
                    }
                    else { result.Add(line); }
                }
                else { result.Add(line); }
            }
            return [.. result];
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

          bool useSoftHyphen, bool softHyphenVal,                             // 2. insert_soft_hyphen
          bool usePageMap, bool pageMapVal, string pageSize, bool adobeDeVal, // 3. page_map
          bool useBroken, bool useBrokenVal,                                  // 4. images: use_broken
          bool useRemoveTransp, bool removeTranspVal,                         // 5. images: remove_transparency
          string scaleFactor,                                                 // 6. images: scale_factor
          bool optimizeVal,                                                   // 7. images: optimize
          bool useJpegQuality, string jpegQuality,                            // 8. images: jpeg_quality_level
          bool customSize, string width, string height, string dpi,           // 9. images: screen
          bool useGenCover, bool genCoverVal, string coverPath,               // 10. images: cover: generate
          bool useResizeCover, string resizeCover,                            // 11. images: cover: resize
          bool useNotesMode, string notesMode,                                // 12. footnotes: mode
          bool useAnnEnable, bool annEnableVal,                               // 13. annotation: enable
          bool useAnnInToc, bool annInTocVal,                                 // 14. annotation: in_toc
          bool useTocPlacement, string tocPlacement,                          // 15. toc_page: placement
          bool useInclNoTitle, bool inclNoTitleVal,                           // 16. include_chapters_without_title
          bool useVignettes, bool vignettesVal, bool[] vignettesItems,        // 17. vignettes
          bool useDropcaps, bool dropcapsVal,                                 // 18. dropcaps: enable

          bool useLogLevel, string logLevel,
          bool useLogName, string logNameTmpl,
          bool usePanicLogName, string panicLogNameTmpl,
          bool useLogMode, string logMode,
          bool useLogFolder, bool logFolderVal)
        {
            //document:
            // 1. Визначаємо шлях до шаблону
            string activeSourcePath = sourcePath; // За замовчуванням Data/config.yaml

            if (useCustomYaml && !string.IsNullOrWhiteSpace(customYamlPath))
            {
                activeSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customYamlPath);
            }

            // 2. Перевіряємо наявність шаблону. Якщо немає — робимо дамп у Data/config.yaml
            if (!File.Exists(activeSourcePath))
            {
                if (activeSourcePath == sourcePath)
                {
                    if (!ExecuteSyncDumpConfig())
                    {
                        throw new Exception("ERR_NO_ENGINE");
                    }
                }
                else
                {
                    throw new Exception("ERR_SOURCE_MISSING");
                }
            }

            // 3. Визначаємо цільовий шлях збереження
            string targetFileName = configName.Trim();
            if (!targetFileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
            {
                targetFileName += ".yaml";
            }

            // Використовуємо Path.Combine для коректної склейки (якщо користувач ввів Data/file.yaml)
            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetFileName);

            try
            {
                string[]? lines = File.ReadAllLines(activeSourcePath, Encoding.UTF8);
                if (lines == null)
                {
                    return false;
                }

                // Обробка параметрів
                if (useCss)
                {
                    lines = ReplaceYamlValueLine(lines, "stylesheet_path", string.IsNullOrWhiteSpace(cssPath) ? "" : $"\"{cssPath}\"", true);
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useCoverMode)
                {
                    lines = ReplaceYamlValueLine(lines, "toc_type", $"\"{coverMode}\"");
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

                if (saveOpenCover)
                {
                    lines = ReplaceYamlValueLine(lines, "open_from_cover", openCoverVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (saveTranslit)
                {
                    lines = ReplaceYamlValueLine(lines, "file_name_transliterate", translitVal ? "true" : "false");

                    if (lines == null)
                    {
                        return false;
                    }
                }
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
                else
                {
                    if (useFb2Name)
                    {
                        templateBlock = """
                                 {{- $name := .SourceFile | base -}}
                                 {{- $prefix := printf "%.37s" $name -}}
                                 {{- if eq (len $prefix) 37 -}}
                                 {{-   if eq (len (replace "-" "" $prefix)) 33 -}}
                                 {{-     $name = replace $prefix "" $name -}}
                                 {{-   end -}}
                                 {{- end -}}
                                 {{- $name = replace ".fb2" "" $name -}}
                                 {{- $name = replace ".zip" "" $name -}}
                                 {{- $name = replace "_fb2" "" $name -}}
                                 {{- $name = replace "(fb2)" "" $name -}}
                                 {{- $name = replace "[litres]" "" $name -}}
                                 {{- $name = replace "[Литрес]" "" $name -}}
                                 {{- $name = replace "_litres" "" $name -}}
                                 {{- $name = replace "(flibusta)" "" $name -}}
                                 {{- $name = replace "_flibusta" "" $name -}}
                                 {{- $name = replace "(Самиздат)" "" $name -}}
                                 {{- $name = replace "[author.today]" "" $name -}}
                                 {{- $name = replace "[mybook]" "" $name -}}
                                 {{- $name = replace "[ficbook]" "" $name -}}
                                 {{- $name = replace "[knigogo.net]" "" $name -}}
                                 {{- $name = replace "_royal_lib_ru" "" $name -}}
                                 {{- $name = replace "CoolLib_net" "" $name -}}
                                 {{- $name = replace "lib_ru" "" $name -}}
                                 {{- $name = replace "[L]" "" $name -}}
                                 {{- $name = replace "_full" "" $name -}}
                                 {{- $name = replace "(v1.0)" "" $name -}}
                                 {{- $name = replace "(v2.0)" "" $name -}}
                                 {{- $name = replace "_ru" "" $name -}}
                                 {{- $name = replace "_en" "" $name -}}
                                 {{- $name = replace "_ua" "" $name -}}
                                 {{- $name = replace "_uk" "" $name -}}
                                 {{- $parts := splitList "_" $name -}}
                                 {{- if gt (len $parts) 1 -}}
                                 {{-   $lastPart := last $parts -}}
                                 {{-   $lpLen := len $lastPart -}}
                                 {{-     if and (ge $lpLen 5) (le $lpLen 8) -}}
                                 {{-     if and (ge $lastPart "0") (le $lastPart "99999999") -}}
                                 {{-     $name = join "_" (initial $parts) -}}
                                 {{-     end -}}
                                 {{-   end -}}
                                 {{- end -}}
                                 {{- $name = replace "_" " " $name -}}
                                 {{- $name = replace "  " " " $name -}}
                                 {{- $name = trim $name -}}
                                 {{- $name -}}
                         """;
                    }
                    else
                    {
                        templateBlock = BuildGoTemplateFromUI(fieldIndexes, folderFlags);
                    }
                }

                //==================
                //metainformation:
                if (useSoftHyphen)
                {
                    lines = ReplaceYamlValueLine(lines, "insert_soft_hyphen", softHyphenVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (usePageMap)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["page_map:"], "enable", pageMapVal ? "true" : "false");
                    lines = ReplaceYamlSectionValueLine(lines!, ["page_map:"], "size", pageSize);
                    lines = ReplaceYamlSectionValueLine(lines!, ["page_map:"], "adobe_de", adobeDeVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                // images
                if (useBroken)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "use_broken", useBrokenVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useRemoveTransp)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "remove_transparency", removeTranspVal ? "true" : "false");
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "scale_factor", scaleFactor);
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "optimize", optimizeVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useJpegQuality)
                {
                    // 1. Створюємо змінну для фінального значення, за замовчуванням 95
                    string finalQuality = "95";

                    // 2. Перевіряємо вхідну змінну jpegQuality (яка прийшла з логіки або параметрів)
                    // Припускаємо, що jpegQuality — це рядок або число, яке ми намагаємось розпарсити
                    if (int.TryParse(jpegQuality?.ToString(), out int parsedValue))
                    {
                        if (parsedValue is >= 40 and <= 100)
                        {
                            finalQuality = parsedValue.ToString();
                        }
                        // Якщо не вдалося розпарсити (пусте або текст), не в межах — ігноруємо, залишиться "95"
                    }

                    lines = ReplaceYamlSectionValueLine(lines!, ["images:"], "jpeg_quality_level", finalQuality);
                    if (lines == null)
                    {
                        return false;
                    }
                }

                // screen
                if (customSize)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "width", width);
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "height", height);
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "screen:"], "dpi", dpi);
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useGenCover)
                {
                    // 1. Спочатку оновлюємо статус generate
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "cover:"], "generate", genCoverVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }

                    // 2. Викликаємо нову логіку для шляху до картинки
                    lines = HandleCoverPathLogic(lines, coverPath);
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useResizeCover)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["images:", "cover:"], "resize", $"\"{resizeCover}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useNotesMode)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["footnotes:"], "mode", $"\"{notesMode}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useAnnEnable)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["annotation:"], "enable", annEnableVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useAnnInToc)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["annotation:"], "in_toc", annInTocVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useTocPlacement)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["toc_page:"], "placement", $"\"{tocPlacement}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useInclNoTitle)
                {
                    lines = ReplaceYamlValueLine(lines!, "include_chapters_without_title", inclNoTitleVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useVignettes)
                {
                    lines = ProcessVignettes(lines!, vignettesVal, vignettesItems);
                    if (lines == null)
                    {
                        return false;
                    }
                }
                if (useDropcaps)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, ["dropcaps:"], "enable", dropcapsVal ? "true" : "false");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                //===============
                // logging:
                string[] fileSec = ["logging:", "file:"];
                if (useLogLevel)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, fileSec, "level", logLevel);

                    if (lines == null)
                    {
                        return false;
                    }
                }

                if (useLogMode)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, fileSec, "mode", logMode);
                    if (lines == null)
                    {
                        return false;
                    }
                }

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
                    if (existingVal.StartsWith("logs/"))
                    {
                        prefix = "logs/";
                    }
                }

                if (useLogName)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, fileSec, "destination_template", $"\"{prefix}{logNameTmpl}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }
                if (usePanicLogName)
                {
                    lines = ReplaceYamlSectionValueLine(lines!, fileSec, "panic_destination_template", $"\"{prefix}{panicLogNameTmpl}\"");
                    if (lines == null)
                    {
                        return false;
                    }
                }

                // 2. Викликаємо заміну в YAML (тільки один раз!)
                if (!string.IsNullOrEmpty(templateBlock))
                {
                    lines = ReplaceOutputTemplateBlockSafely(lines!, templateBlock);
                }

                // 3. Перевірка на помилку (якщо ключ не знайдено)
                if (lines == null)
                {
                    return false;
                }

                // 4. Запис у файл
                File.WriteAllLines(targetPath, lines, Encoding.UTF8);
                Config.SaveSettings();

                return true;
            }
            catch (UnauthorizedAccessException) { throw new Exception("ERR_READONLY"); }
            catch (DirectoryNotFoundException) { throw new Exception("ERR_DIRNOTFOUND"); }
            catch (Exception ex)
            {
                // Додаємо запис у лог перед тим як кинути помилку далі
                Config.LogError($"SaveConfiguration failed for: {configName}", ex);

                if (ex.Message.StartsWith("YAML_KEY:")) { throw; }
                throw new Exception("ERR_UNKNOWN");
            }
        }

        private static void ShowYamlError(string key)
        {
            throw new Exception("YAML_KEY:" + key);
        }
    }
}