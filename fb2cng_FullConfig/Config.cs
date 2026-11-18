using fb2cng_FullConfig.Settings;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace fb2cng_FullConfig
{
    public static class Config
    {
        // 1. ОГОЛОШУЄМО ВСІ ПОЛЯ ТА КЕШОВАНИЙ ДИЗАЙН
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private static readonly Lock _fileLock = new();

        public const string DataFolder = "Data";
        public const string ConfigFileName = "config.yaml";
        public const string DefaultConfigPath = "Data/config.yaml";
        public const string LogErrorFile = "logs/conf_errors.log";

        // 2. ПОТІМ МЕТОДИ
        // Метод ініціалізації (викликається при старті в Program.cs)
        public static void Initialize(IConfiguration config)
        {
            Settings = config.Get<AppSettings>() ?? new AppSettings();
        }

        public static void LogError(string message, Exception? ex = null)
        {
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogErrorFile);

                // Дістаємо назву папки з повного шляху до файлу, для створення
                string? logsDir = Path.GetDirectoryName(logFile);
                if (!string.IsNullOrEmpty(logsDir) && !Directory.Exists(logsDir))
                {
                    _ = Directory.CreateDirectory(logsDir);
                }

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                if (ex != null)
                {
                    // Беремо лише перший рядок StackTrace (де саме стався збій)
                    string firstTraceLine = "";
                    if (!string.IsNullOrEmpty(ex.StackTrace))
                    {
                        firstTraceLine = ex.StackTrace.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                                                             .FirstOrDefault()?.Trim() ?? "";
                    }

                    logMessage += $" | Exception: {ex.Message}";

                    if (!string.IsNullOrEmpty(firstTraceLine))
                    {
                        logMessage += $" | Trace: {firstTraceLine}";
                    }
                }

                File.AppendAllLines(logFile, [logMessage]);
            }
            catch { }
        }

        // Збереження у JSON форматі
        public static void SaveSettings()
        {
            lock (_fileLock) // Блокуємо доступ, поки один потік пише файл
            {
                try
                {
                    // 1. Отримуємо шлях до директорії та створюємо її, якщо вона відсутня
                    string? directoryPath = Path.GetDirectoryName(settingsFile);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        _ = Directory.CreateDirectory(directoryPath);
                    }

                    // 2. Використовуємо глобальні кешовані опції замість локального new()
                    string jsonString = JsonSerializer.Serialize(Settings, JsonOptions);
                    File.WriteAllText(settingsFile, jsonString);
                }
                catch (Exception ex)
                {
                    LogError("SaveSettings error", ex);
                }
            }
        }
        // 1. Посилання на сам об'єкт налаштувань
        public static AppSettings Settings { get; private set; } = new AppSettings();

        // 2. Властивості-перехідники (Маскування під старий код, щоб прибрати всі помилки)
        public static string CurrentLanguage
        {
            get => Settings.CurrentLanguage;
            set => Settings.CurrentLanguage = value;
        }

        public static bool IsDarkTheme
        {
            get => Settings.IsDarkTheme;
            set => Settings.IsDarkTheme = value;
        }

        // Шлях до файлу конфігурації
        private static readonly string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataFolder, "Conf_config.json");

        // Публічна властивість для читання
        public static Dictionary<string, Dictionary<string, string>> Localization { get; } = new()

        {
            // 1. АНГЛІЙСЬКА ЛОКАЛІЗАЦІЯ
            ["English"] = new()
            {
                ["Title"] = "fb2cng Template Configurator",

                ["Language"] = "Language:",
                ["DumpConfig"] = $"Load Default {ConfigFileName}",
                ["ConfigName"] = "Custom template name:",
                ["CustomYamlEnable"] = "Edit user.yaml",
                ["CssEnable"] = "Use custom CSS stylesheet",
                ["TocType"] = "Navigation type (TOC):",
                ["Opt_Toc_Normal"] = "standard (multi-level)",
                ["Opt_Toc_OldKindle"] = "compatible (old Kindle)",
                ["Opt_Toc_Flat"] = "simplified (single-level)",
                ["FixZip"] = "Remove data descriptor (Fix ZIP)",
                ["OpenCover"] = "Open book from the cover",
                ["Translit"] = "Transliterate output filename",
                ["Fb2Name"] = "Use source fb2 name for the output file",
                ["DefaultName"] = "Reference name for the output file",
                ["OutNameTitle"] = "Output filename structure",
                ["AsFolder"] = "as folder",
                ["Item_Empty"] = "[Not selected]",
                ["Item_Author"] = "Author (.Authors)",
                ["Item_Series"] = "Series (.Series)",
                ["Item_Title"] = "Book Title (xx.Title)",
                ["Item_Title_Pure"] = "Pure Title (.Title)",
                ["Item_Lang"] = "Language (.Language)",
                ["Item_Genre"] = "Genre (.Genres)",
                ["Item_Date"] = "Date (.Date)",
                ["Item_Source"] = "Source File (.SourceFile)",
                ["Item_Uuid"] = "Book UUID (.BookID)",

                ["SoftHyphen"] = "Insert soft hyphens",
                ["KfxCompression"] = "KFX/AZW8 compression:",
                ["Opt_KfxComp_None"] = "none (none)",
                ["Opt_KfxComp_Lzma"] = "enabled (lzma)",
                ["Azw3Compression"] = "AZW3 compression:",
                ["Opt_Azw3Comp_None"] = "none (none)",
                ["Opt_Azw3Comp_Standard"] = "standard (standard)",
                ["Opt_Azw3Comp_Huffdic"] = "maximum (huffdic)",
                ["Azw3Apnx"] = "APNX page-map sidecar (AZW3):",
                ["Opt_Apnx_None"] = "no sidecar (none)",
                ["Opt_Apnx_Eink"] = "in system folder (eink)",
                ["Opt_Apnx_App"] = "in book folder (app)",
                ["PdfPageFrame"] = "PDF page frame (0 / pt / px):",
                ["MarkdownImages"] = "Markdown images:",
                ["Opt_MdImg_Placeholder"] = "placeholder (text label)",
                ["Opt_MdImg_External"] = "external (separate folder)",
                ["Opt_MdImg_Embedded"] = "embedded (data:URI)",
                ["PageMapEnable"] = "Generate page map",
                ["PageMapSize"] = "Page size (min: 500)",
                ["AdobeDe"] = "Adobe RMSDK/ADE support",
                ["FootnotesMode"] = "Footnotes mode:",
                ["Opt_Note_Default"] = "standard (links)",
                ["Opt_Note_Float"] = "floating (popup)",
                ["Opt_Note_FloatRen"] = "floating + renumber",
                ["AnnEnable"] = "Annotation as separate chapter",
                ["AnnInToc"] = "Include annotation in TOC",
                ["TocPlacement"] = "TOC as separate page",
                ["Opt_TocPlace_None"] = "none",
                ["Opt_TocPlace_Before"] = "before content",
                ["Opt_TocPlace_After"] = "after content",
                ["InclNoTitle"] = "Untitled chapters in TOC",
                ["Vignettes"] = "Vignettes",
                ["Vig_Options"] = "Options",
                ["Vig_B_T"] = "Book Title (Top)",
                ["Vig_B_B"] = "Book Title (Bottom)",
                ["Vig_C_T"] = "Chapter Title (Top)",
                ["Vig_C_B"] = "Chapter Title (Bottom)",
                ["Vig_C_E"] = "Chapter End",
                ["Vig_S_T"] = "Section Title (Top)",
                ["Vig_S_B"] = "Section Title (Bottom)",
                ["Vig_S_E"] = "Section End",
                ["Dropcaps"] = "Automatic dropcap styling",
                ["TextTransformSpeech"] = "Typography fix: speech dashes",
                ["TextTransformDashes"] = "Typography fix: dashes in text",
                ["TextTransformDialogue"] = "Typography fix: dialogue indent",

                ["ReaderSize"] = "Reader screen size (W / H / DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
                ["UseBroken"] = "Process invalid images",
                ["RemoveTransp"] = "Remove image transparency",
                ["ScaleFactor"] = "Image scale factor",
                ["RasterizeSvg"] = "Force SVG rasterization",
                ["RasterFormat"] = "Raster format policy:",
                ["Opt_RasterFmt_Auto"] = "auto",
                ["Opt_RasterFmt_Jpeg"] = "jpeg",
                ["ImgOptimize"] = "Optimize images",
                ["FitToScreen"] = "Fit images to screen",
                ["BaselineJpeg"] = "Compatible JPEG (Baseline)",
                ["Grayscale"] = "Grayscale conversion:",
                ["Opt_Grayscale_None"] = "keep color (none)",
                ["Opt_Grayscale_Luma"] = "grayscale (luma)",
                ["JpegQuality"] = "JPEG quality level (40-100%)",
                ["GenCover"] = "Generate book cover",
                ["CoverPath"] = "Default cover path:",
                ["ResizeCover"] = "Cover resize mode:",
                ["Opt_Resize_None"] = "original size",
                ["Opt_Resize_KeepAR"] = "keep aspect ratio",
                ["Opt_Resize_Stretch"] = "stretch to fill",
                ["Opt_Resize_Fit"] = "fit to screen",
                ["Opt_Resize_Fill"] = "fill screen",

                ["LogLevel"] = "Logging level:",
                ["Opt_Log_None"] = "disabled (none)",
                ["Opt_Log_Normal"] = "standard (normal)",
                ["Opt_Log_Debug"] = "full (debug)",
                ["LogName"] = "Log name pattern:",
                ["LogPanicName"] = "Panic log pattern:",
                ["LogMode"] = "Logging mode:",
                ["LogFolder"] = "Write to 'logs' folder",
                ["LogOpt_Default"] = "default",
                ["LogOpt_NameFormat"] = "name + format",
                ["LogOpt_TimeName"] = "time + name",
                ["LogOpt_NameTag"] = "name + tag",
                ["LogMode_OnlyNew"] = "Replace",
                ["LogMode_OldNew"] = "Append",
                ["ShowTooltips"] = "* Enable tooltips",

                ["ResetTitle"] = "Application Restart",
                ["ResetConfirm"] = "Are you sure you want to reset the configuration settings to defaults?\n(Language and theme settings will be preserved)",
                ["SaveErrorTitle"] = "Save Error",
                ["ErrReadOnly"] = "Access Denied:\n" +
                "The file '{0}' is locked!\n" +
                "Please check if it is marked as 'Read-Only' or opened in another application.",
                ["ErrDirNotFound"] = "The directory specified by the path '{0}' was not found!",
                ["ErrSourceMissing"] = "Template '{0}' not found!\nPlease check the path or the filename.",
                ["ErrTitle"] = "Component Missing",
                ["ErrFbc"] = "The fb2cng engine was not found: please verify that 'fbc.exe' is present in the application folder!",
                ["SaveSuccessTitle"] = "Success",
                ["SaveSuccess"] = "Configuration successfully saved to {0}!",
                ["YamlTitle"] = "YAML Error",
                ["YamlErr"] = $"Error: Key '{{0}}' not found in template {ConfigFileName}!",
                ["GenTitle"] = "Success",
                ["GenSuccess"] = "File {0} successfully saved to the 'Data' folder.",

                ["Help"] = "Help",
                ["HelpText"] = "“fb2cng Template Configurator”\nDeveloped for the fb2cng GUI toolkit.\n======######======\n" +
               "If you are too lazy to manually edit YAML files and learn Go template syntax:\n" +
               "• Configuration Management: Extract the default template, create custom settings from scratch, or edit previously created YAML files.\n" +
               "• Visual Builder: Intuitively customize the structure and formatting rules for your converted books.\n" +
               "• Quick Result: Choose your preferences and click 'Save' — the app will assemble your user.yaml automatically.\n======######======\n{0}\nVersion: {1}",
                ["Theme"] = "Theme",
                ["Ok"] = "Save",
                ["Cancel"] = "Cancel",
                ["Yes"] = "Yes",
                ["No"] = "No"
            },

            // 2. УКРАЇНСЬКА ЛОКАЛІЗАЦІЯ
            ["Ukrainian"] = new()
            {
                ["Title"] = "Конфігуратор шаблона fb2cng",

                ["Language"] = "Мова:",
                ["DumpConfig"] = $"Завантажити дефолтний {ConfigFileName}",
                ["ConfigName"] = "Назва для власного шаблона:",
                ["CustomYamlEnable"] = "Редагувати user.yaml",
                ["CssEnable"] = "CSS-таблиця стилів",
                ["TocType"] = "Тип навігації (TOC):",
                ["Opt_Toc_Normal"] = "стандартна (багаторівнева)",
                ["Opt_Toc_OldKindle"] = "сумісна (старі Kindle)",
                ["Opt_Toc_Flat"] = "спрощена (один рівень)",
                ["FixZip"] = "Вилучити дескриптор даних (Fix ZIP)",
                ["OpenCover"] = "Відкривати книгу з обкладинки",
                ["Translit"] = "Транслітерувати назву вихідного файлу",
                ["Fb2Name"] = "Використати назву fb2 для вихідного файлу",
                ["DefaultName"] = "Еталонна назва для вихідного файлу",
                ["OutNameTitle"] = "Структура назви вихідного файлу",
                ["AsFolder"] = "як папка",
                ["Item_Empty"] = "[Не вибрано]",
                ["Item_Author"] = "Автор (.Authors)",
                ["Item_Series"] = "Серія (.Series)",
                ["Item_Title"] = "Назва книги (xx.Title)",
                ["Item_Title_Pure"] = "Назва без серії (.Title)",
                ["Item_Lang"] = "Мова (.Language)",
                ["Item_Genre"] = "Жанр (.Genres)",
                ["Item_Date"] = "Дата (.Date)",
                ["Item_Source"] = "Базова назва файлу (.SourceFile)",
                ["Item_Uuid"] = "UUID книги (.BookID)",

                ["SoftHyphen"] = "Вставка м'яких переносів",
                ["KfxCompression"] = "Стиснення KFX/AZW8:",
                ["Opt_KfxComp_None"] = "відсутнє (none)",
                ["Opt_KfxComp_Lzma"] = "застосувати (lzma)",
                ["Azw3Compression"] = "Стиснення AZW3:",
                ["Opt_Azw3Comp_None"] = "відсутнє (none)",
                ["Opt_Azw3Comp_Standard"] = "стандартне (standard)",
                ["Opt_Azw3Comp_Huffdic"] = "максимальне (huffdic)",
                ["Azw3Apnx"] = "Файл нумерації сторінок (AZW3):",
                ["Opt_Apnx_None"] = "не створювати (none)",
                ["Opt_Apnx_Eink"] = "в системній папці (eink)",
                ["Opt_Apnx_App"] = "в папці з книгою (app)",
                ["PdfPageFrame"] = "Рамка сторінки PDF (0 / pt / px):",
                ["MarkdownImages"] = "Зображення в Markdown:",
                ["Opt_MdImg_Placeholder"] = "текстова позначка (заглушка)",
                ["Opt_MdImg_External"] = "зовнішні файли (окрема папка)",
                ["Opt_MdImg_Embedded"] = "текстовий код (вбудовані)",
                ["PageMapEnable"] = "Генерація карти сторінок",
                ["PageMapSize"] = "Розмір сторінки (min: 500)",
                ["AdobeDe"] = "Підтримка Adobe RMSDK/ADE",
                ["FootnotesMode"] = "Режим виносок:",
                ["Opt_Note_Default"] = "стандартний (посилання)",
                ["Opt_Note_Float"] = "спливаючий (float/popup)",
                ["Opt_Note_FloatRen"] = "спливаючий + нумерація",
                ["AnnEnable"] = "Анотація окремим розділом",
                ["AnnInToc"] = "Включати анотацію до змісту",
                ["TocPlacement"] = "Зміст окремою сторінкою",
                ["Opt_TocPlace_None"] = "не створювати",
                ["Opt_TocPlace_Before"] = "на початку книги",
                ["Opt_TocPlace_After"] = "в кінці книги",
                ["InclNoTitle"] = "Безіменні розділи в змісті",
                ["Vignettes"] = "Віньєтки",
                ["Vig_Options"] = "Опції",
                ["Vig_B_T"] = "Зверху назви книги",
                ["Vig_B_B"] = "Знизу назви книги",
                ["Vig_C_T"] = "Зверху назви розділу",
                ["Vig_C_B"] = "Знизу назви розділу",
                ["Vig_C_E"] = "В кінці розділу",
                ["Vig_S_T"] = "Зверху назви підрозділу",
                ["Vig_S_B"] = "Знизу назви підрозділу",
                ["Vig_S_E"] = "В кінці підрозділу",
                ["Dropcaps"] = "Автоматична стилізація буквиць",
                ["TextTransformSpeech"] = "Корекція типографіки: пряма мова",
                ["TextTransformDashes"] = "Корекція типографіки: тире в тексті",
                ["TextTransformDialogue"] = "Корекція типографіки: відступи діалогів",

                ["ReaderSize"] = "Розмір екрана рідера (Ш/В/DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
                ["UseBroken"] = "Обробка невалідних зображень",
                ["RemoveTransp"] = "Вилучити прозорість зображень",
                ["RasterizeSvg"] = "Примусова растеризація SVG",
                ["RasterFormat"] = "Формат растрових зображень:",
                ["Opt_RasterFmt_Auto"] = "auto",
                ["Opt_RasterFmt_Jpeg"] = "jpeg",
                ["ScaleFactor"] = "Коефіцієнт розміру картинки",
                ["FitToScreen"] = "Обмеження картинки екраном",
                ["BaselineJpeg"] = "Сумісний JPEG (Baseline)",
                ["Grayscale"] = "Відтінки сірого (Grayscale):",
                ["Opt_Grayscale_None"] = "без змін (none)",
                ["Opt_Grayscale_Luma"] = "знебарвлення (luma)",
                ["ImgOptimize"] = "Оптимізація зображень",
                ["JpegQuality"] = "Рівень якості JPEG (40-100%)",
                ["GenCover"] = "Генерувати обкладинку",
                ["CoverPath"] = "Шлях до обкладинки:",
                ["ResizeCover"] = "Розмір обкладинки:",
                ["Opt_Resize_None"] = "не змінювати",
                ["Opt_Resize_KeepAR"] = "зберегти пропорції",
                ["Opt_Resize_Stretch"] = "розтягнути на екран",
                ["Opt_Resize_Fit"] = "вписати в екран",
                ["Opt_Resize_Fill"] = "заповнити екран",

                ["LogLevel"] = "Рівень логування:",
                ["Opt_Log_None"] = "вимкнено (none)",
                ["Opt_Log_Normal"] = "звичайний (normal)",
                ["Opt_Log_Debug"] = "розширений (debug)",
                ["LogName"] = "Шаблон назви логу:",
                ["LogPanicName"] = "Шаблон панік-логу:",
                ["LogMode"] = "Режим логування:",
                ["LogFolder"] = "Запис у папку 'logs'",
                ["LogOpt_Default"] = "за замовчуванням",
                ["LogOpt_NameFormat"] = "назва + формат",
                ["LogOpt_TimeName"] = "час + назва",
                ["LogOpt_NameTag"] = "назва + мітка",
                ["LogMode_OnlyNew"] = "Заміна",
                ["LogMode_OldNew"] = "Дозапис",
                ["ShowTooltips"] = "* Увімкнути спливаючі підказки",

                ["ResetTitle"] = "Перезапуск програми",
                ["ResetConfirm"] = "Ви впевнені, що хочете скинути налаштування конфігурації до початкового стану?\n(Параметри мови та теми будуть збережені)",
                ["SaveErrorTitle"] = "Помилка збереження",
                ["ErrReadOnly"] = "Помилка доступу:\n" +
                "Файл '{0}' заблоковано!\n" +
                "Перевірте, чи не встановлено атрибут 'Тільки для читання', або чи не відкритий він в іншій програмі.",
                ["ErrDirNotFound"] = "Папку, на яку вказує шлях '{0}', не знайдено!",
                ["ErrSourceMissing"] = "Шаблон '{0}' не знайдено!\nПеревірте правильність шляху чи імені файлу!",
                ["ErrTitle"] = "Помилка конфігурації",
                ["ErrFbc"] = "Відсутня програма-конвертор: перевірте наявність файлу 'fbc.exe' в папці з програмою!",
                ["SaveSuccessTitle"] = "Успіх",
                ["SaveSuccess"] = "Конфігурацію успішно збережено у файл {0}!",
                ["YamlTitle"] = "Помилка YAML",
                ["YamlErr"] = $"Помилка: Ключ '{{0}}' не знайдено у файлі {ConfigFileName}!",
                ["GenTitle"] = "Успіх",
                ["GenSuccess"] = "Файл {0} успішно збережено в папку 'Data'.",

                ["Help"] = "Довідка",
                ["HelpText"] = "«Конфігуратор шаблона fb2cng»\nРозроблено для набору інструментів fb2cng GUI.\n======######======\n" +
               "Якщо ліньки вручну редагувати YAML-файли та вивчати шаблони мови Go:\n" +
               "• Керуйте конфігурацією: завантажуйте дефолтний шаблон, створюйте власні налаштування на базі стандартних або редагуйте раніше створені YAML-файли.\n" +
               "• Візуальний конструктор: інтуїтивно налаштовуйте структуру та правила форматування ваших готових книг.\n" +
               "• Швидкий результат: оберіть потрібні параметри та натисніть «Зберегти» — програма сама сформує ваш 'user.yaml'.\n======######======\n{0}\nВерсія: {1}",
                ["Theme"] = "Тема",
                ["Ok"] = "Зберегти",
                ["Cancel"] = "Скасувати",
                ["Yes"] = "Так",
                ["No"] = "Ні"
            },

            // 3. РОСІЙСЬКА ЛОКАЛІЗАЦІЯ
            ["Russian"] = new()
            {
                ["Title"] = "Конфигуратор шаблона fb2cng",

                ["Language"] = "Язык:",
                ["DumpConfig"] = $"Загрузить дефолтный {ConfigFileName}",
                ["ConfigName"] = "Имя пользовательского шаблона:",
                ["CustomYamlEnable"] = "Редактировать user.yaml",
                ["CssEnable"] = "CSS-таблица стилей",
                ["TocType"] = "Тип навигации (TOC):",
                ["Opt_Toc_Normal"] = "стандартная (многоуровневая)",
                ["Opt_Toc_OldKindle"] = "совместимая (старые Kindle)",
                ["Opt_Toc_Flat"] = "упрощенная (один уровень)",
                ["FixZip"] = "Удалить дескриптор данных (Fix ZIP)",
                ["OpenCover"] = "Открывать книгу с обложки",
                ["Translit"] = "Транслитерировать имя выходного файла",
                ["Fb2Name"] = "Использовать имя fb2 для выходного файла",
                ["DefaultName"] = "Эталонное имя для выходного файла",
                ["OutNameTitle"] = "Структура имени выходного файла",
                ["AsFolder"] = "как папка",
                ["Item_Empty"] = "[Не выбрано]",
                ["Item_Author"] = "Автор (.Authors)",
                ["Item_Series"] = "Серия (.Series)",
                ["Item_Title"] = "Название книги (xx.Title)",
                ["Item_Title_Pure"] = "Название без серии (.Title)",
                ["Item_Lang"] = "Язык (.Language)",
                ["Item_Genre"] = "Жанр (.Genres)",
                ["Item_Date"] = "Дата (.Date)",
                ["Item_Source"] = "Исходное имя файла (.SourceFile)",
                ["Item_Uuid"] = "UUID книги (.BookID)",

                ["SoftHyphen"] = "Вставка мягких переносов",
                ["KfxCompression"] = "Сжатие KFX/AZW8:",
                ["Opt_KfxComp_None"] = "отсутствует (none)",
                ["Opt_KfxComp_Lzma"] = "применить (lzma)",
                ["Azw3Compression"] = "Сжатие AZW3:",
                ["Opt_Azw3Comp_None"] = "отсутствует (none)",
                ["Opt_Azw3Comp_Standard"] = "стандартное (standard)",
                ["Opt_Azw3Comp_Huffdic"] = "максимальное (huffdic)",
                ["Azw3Apnx"] = "Файл страниц APNX (AZW3):",
                ["Opt_Apnx_None"] = "не создавать (none)",
                ["Opt_Apnx_Eink"] = "в системной папке (eink)",
                ["Opt_Apnx_App"] = "в папке с книгой (app)",
                ["PdfPageFrame"] = "Рамка страницы PDF (0 / pt / px):",
                ["MarkdownImages"] = "Изображения в Markdown:",
                ["Opt_MdImg_Placeholder"] = "текстовая метка (заглушка)",
                ["Opt_MdImg_External"] = "внешний файл (отдельная папка)",
                ["Opt_MdImg_Embedded"] = "текстовый код (встроенные)",
                ["PageMapEnable"] = "Генерация карты страниц",
                ["PageMapSize"] = "Размер страницы (min: 500)",
                ["AdobeDe"] = "Поддержка Adobe RMSDK/ADE",
                ["FootnotesMode"] = "Режим сносок:",
                ["Opt_Note_Default"] = "стандартный (ссылки)",
                ["Opt_Note_Float"] = "всплывающий (float/popup)",
                ["Opt_Note_FloatRen"] = "всплывающий + нумерация",
                ["AnnEnable"] = "Аннотация отдельным разделом",
                ["AnnInToc"] = "Включать аннотацию в оглавление",
                ["TocPlacement"] = "Оглавление отдельной страницей",
                ["Opt_TocPlace_None"] = "не создавать",
                ["Opt_TocPlace_Before"] = "в начале книги",
                ["Opt_TocPlace_After"] = "в конце книги",
                ["InclNoTitle"] = "Безымянные разделы в содержании",
                ["Vignettes"] = "Виньетки",
                ["Vig_Options"] = "Опции",
                ["Vig_B_T"] = "Сверху названия книги",
                ["Vig_B_B"] = "Снизу названия книги",
                ["Vig_C_T"] = "Сверху названия раздела",
                ["Vig_C_B"] = "Снизу названия раздела",
                ["Vig_C_E"] = "В конце раздела",
                ["Vig_S_T"] = "Сверху названия подраздела",
                ["Vig_S_B"] = "Снизу названия подраздела",
                ["Vig_S_E"] = "В конце подраздела",
                ["Dropcaps"] = "Автоматическая стилизация буквиц",
                ["TextTransformSpeech"] = "Коррекция типографики: прямая речь",
                ["TextTransformDashes"] = "Коррекция типографики: тире в тексте",
                ["TextTransformDialogue"] = "Коррекция типографики: отступ диалогов",

                ["ReaderSize"] = "Размер экрана ридера (Ш/В/DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
                ["UseBroken"] = "Обработка невалидных изображений",
                ["RemoveTransp"] = "Удалить прозрачность изображений",
                ["RasterizeSvg"] = "Принудительная растеризация SVG",
                ["RasterFormat"] = "Формат растровых изображений:",
                ["Opt_RasterFmt_Auto"] = "auto",
                ["Opt_RasterFmt_Jpeg"] = "jpeg",
                ["ScaleFactor"] = "Коэффициент размера картинки",
                ["FitToScreen"] = "Ограничение картинки экраном",
                ["BaselineJpeg"] = "Совместимый JPEG (Baseline)",
                ["Grayscale"] = "Оттенки серого (Grayscale):",
                ["Opt_Grayscale_None"] = "без изменений (none)",
                ["Opt_Grayscale_Luma"] = "обесцвечивание (luma)",
                ["ImgOptimize"] = "Оптимизация изображений",
                ["JpegQuality"] = "Уровень качества JPEG (40-100%)",
                ["GenCover"] = "Генерировать обложку",
                ["CoverPath"] = "Путь к обложке:",
                ["ResizeCover"] = "Размер обложки:",
                ["Opt_Resize_None"] = "не изменять",
                ["Opt_Resize_KeepAR"] = "сохранить пропорции",
                ["Opt_Resize_Stretch"] = "растянуть на экран",
                ["Opt_Resize_Fit"] = "вписать в экран",
                ["Opt_Resize_Fill"] = "заполнить экран",

                ["LogLevel"] = "Уровень логирования:",
                ["Opt_Log_None"] = "отключен (none)",
                ["Opt_Log_Normal"] = "стандартный (normal)",
                ["Opt_Log_Debug"] = "расширенный (debug)",
                ["LogName"] = "Шаблон имени лога:",
                ["LogPanicName"] = "Шаблон паник-лога:",
                ["LogMode"] = "Режим логирования:",
                ["LogFolder"] = "Запись в папку 'logs'",
                ["LogOpt_Default"] = "по умолчанию",
                ["LogOpt_NameFormat"] = "имя + формат",
                ["LogOpt_TimeName"] = "время + имя",
                ["LogOpt_NameTag"] = "имя + метка",
                ["LogMode_OnlyNew"] = "Замена",
                ["LogMode_OldNew"] = "Дозапись",
                ["ShowTooltips"] = "* Показывать подсказки",

                ["ResetTitle"] = "Перезапуск программы",
                ["ResetConfirm"] = "Вы уверены, что хотите сбросить настройки конфигурации до начального состояния?\n(Язык и тема будут сохранены)",
                ["ErrTitle"] = "Ошибка конфигурации",
                ["ErrFbc"] = "Программа-конвертер не найдена: проверьте наличие файла 'fbc.exe' в папке с программой!",
                ["SaveErrorTitle"] = "Ошибка сохранения",
                ["ErrReadOnly"] = "Ошибка доступа:\n" +
                "Файл '{0}' заблокирован!\n" +
                "Проверьте, не установлен ли атрибут 'Только для чтения', или не открыт ли он в другой программе.",
                ["ErrDirNotFound"] = "Указанный путь к папке '{0}' не существует!",
                ["ErrSourceMissing"] = "Шаблон '{0}' не найден!\nПроверьте правильность пути или имени файла!",
                ["SaveSuccessTitle"] = "Успех",
                ["SaveSuccess"] = "Конфигурация успешно сохранена в файл {0}!",
                ["YamlTitle"] = "Ошибка YAML",
                ["YamlErr"] = $"Ошибка: Ключ '{{0}}' не найден в файле {ConfigFileName}!",
                ["GenTitle"] = "Успех",
                ["GenSuccess"] = "Файл {0} успешно сохранен в папку 'Data'.",

                ["Help"] = "Справка",
                ["HelpText"] = "«Конфигуратор шаблона fb2cng»\nРазработано для набора инструментов fb2cng GUI.\n======######======\n" +
               "Если лень вручную редактировать YAML-файлы и изучать шаблоны языка Go:\n" +
               "• Управление конфигурацией: извлекайте дефолтный шаблон, создавайте свои настройки на базе стандартных или редактируйте ранее созданные YAML-файлы.\n" +
               "• Визуальный конструктор: интуитивно настраивайте структуру и правила форматирования ваших готовых книг.\n" +
               "• Быстрый результат: выберите нужные параметры и нажмите «Сохранить» — программа сама сформирует ваш 'user.yaml'.\n======######======\n{0}\nВерсия: {1}",
                ["Theme"] = "Тема",
                ["Ok"] = "Сохранить",
                ["Cancel"] = "Отмена",
                ["Yes"] = "Да",
                ["No"] = "Нет"
            }
        };
    }
}