using fb2cng_FullConfig.Settings;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace fb2cng_FullConfig
{
    public static class Config
    {
        // 1. СПОЧАТКУ ОГОЛОШУЄМО ВСІ ПОЛЯ ТА КЕШОВАНИЙ ДИЗАЙН (Тепер усе на своєму місці)
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private static readonly Lock _fileLock = new();

        public const string DataFolder = "Data";
        public const string ConfigFileName = "config.yaml";
        public const string DefaultConfigPath = "Data/config.yaml";
        public const string LogErrorFile = "logs/conf_errors.log";

        // 2. ПОТІМ ЙДУТЬ МЕТОДИ
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
        // 1. Посилання на сам об'єкт налаштувань (для нових фіч)
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

        // Публічна властивість для читання (семантика коду у всій програмі НЕ зміниться, Config.Localization[...] працюватиме як і раніше)
        public static Dictionary<string, Dictionary<string, string>> Localization { get; } = new()

        {
            // 1. АНГЛІЙСЬКА ЛОКАЛІЗАЦІЯ
            ["English"] = new()
            {
                ["Title"] = "fb2cng Template Configurator",
                ["Help"] = "Help",
                ["HelpText"] = "“fb2cng Template Configurator”\nDeveloped for the fb2cng GUI toolkit.\n\n" +
               "If you are too lazy to manually edit YAML files and learn Go template syntax:\n" +
               "• Configuration Management: Extract the default template, create custom settings from scratch, or edit previously created YAML files.\n" +
               "• Visual Builder: Intuitively customize the structure and formatting rules for your converted books.\n" +
               "• Quick Result: Choose your preferences and click 'Save' — the app will assemble your user.yaml automatically.\n\n" +
               "Developed by: Jurchos & Gemini\n" +
               "Version: 1.5",
                ["Theme"] = "Theme",
                ["Ok"] = "Save",
                ["Cancel"] = "Cancel",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Language"] = "Language:",
                ["DumpConfig"] = $"Load Default {ConfigFileName}",
                ["ConfigName"] = "Custom template name:",
                ["CustomYamlEnable"] = "Edit user.yaml",
                ["CssEnable"] = "Use custom CSS stylesheet",
                ["TocType"] = "Navigation type (TOC):",
                ["Opt_Toc_Normal"] = "Normal (nested)",
                ["Opt_Toc_OldKindle"] = "Compatible (old Kindle)",
                ["Opt_Toc_Flat"] = "Flat (single level)",
                ["OpenCover"] = "Open book from the cover",
                ["FixZip"] = "Remove data descriptor (Fix ZIP)",
                ["Fb2Name"] = "Use source fb2 name for the output file",
                ["DefaultName"] = "Reference name for the output file",
                ["OutNameTitle"] = "Output filename structure",
                ["AsFolder"] = "as folder",
                ["Translit"] = "Transliterate output filename",
                ["ReaderSize"] = "Reader screen size (W / H / DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
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
                ["FootnotesMode"] = "Footnotes mode:",
                ["Opt_Note_Default"] = "Standard (links)",
                ["Opt_Note_Float"] = "Floating (popup)",
                ["Opt_Note_FloatRen"] = "Floating + renumber",
                ["SoftHyphen"] = "Insert soft hyphens",
                ["RemoveTransp"] = "Remove image transparency",
                ["JpegQuality"] = "JPEG quality level (40-100%)",
                ["GenCover"] = "Generate book cover",
                ["CoverPath"] = "Default cover path:",
                ["ResizeCover"] = "Cover resize mode:",
                ["Opt_Resize_None"] = "Original size",
                ["Opt_Resize_KeepAR"] = "Keep aspect ratio",
                ["Opt_Resize_Stretch"] = "Stretch to fill",
                ["AnnEnable"] = "Annotation as separate chapter",
                ["AnnInToc"] = "Include annotation in TOC",
                ["TocPlacement"] = "TOC as separate page",
                ["Opt_TocPlace_None"] = "None",
                ["Opt_TocPlace_Before"] = "Before content",
                ["Opt_TocPlace_After"] = "After content",
                ["PageMapEnable"] = "Generate page map",
                ["PageMapSize"] = "Page size(min: 500)",
                ["AdobeDe"] = "Adobe RMSDK/ADE support",
                ["UseBroken"] = "Process invalid images",
                ["ScaleFactor"] = "Image scale factor",
                ["ImgOptimize"] = "Optimize images",
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
                ["LogLevel"] = "Logging level:",
                ["Opt_Log_None"] = "Disabled (none)",
                ["Opt_Log_Normal"] = "Standard (normal)",
                ["Opt_Log_Debug"] = "Full (debug)",
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
                ["ResetTitle"] = "Application Restart",
                ["ResetConfirm"] = "Are you sure you want to reset the configuration settings to defaults?\n(Language and theme settings will be preserved)",
                ["SaveErrorTitle"] = "Save Error",
                ["ErrReadOnly"] = "Access Denied:\n The file '{0}' is locked!\nPlease check if it is marked as 'Read-Only' or opened in another application.",
                ["ErrAccessDenied"] = "Access Denied:\n Access to the file '{0}' is denied!\nPlease run the application as Administrator.",
                ["ErrDirNotFound"] = "The directory specified by the path '{0}' was not found!",
                ["ErrTitle"] = "Component Missing",
                ["ErrFbc"] = "The fb2cng engine was not found: please verify that 'fbc.exe' is present in the application folder!",
                ["SaveSuccessTitle"] = "Success",
                ["SaveSuccess"] = "Configuration successfully saved to {0}!",
                ["YamlTitle"] = "YAML Error",
                ["YamlErr"] = $"Error: Key '{{0}}' not found in template {ConfigFileName}!",
                ["GenTitle"] = "Success",
                ["GenSuccess"] = "File {0} successfully saved to the 'Data' folder."
            },

            // 2. УКРАЇНСЬКА ЛОКАЛІЗАЦІЯ
            ["Ukrainian"] = new()
            {
                ["Title"] = "Конфігуратор шаблона fb2cng",
                ["Help"] = "Довідка",
                ["HelpText"] = "«Конфігуратор шаблона fb2cng»\nРозроблено для набору інструментів fb2cng GUI.\n\n" +
               "Якщо ліньки вручну редагувати YAML-файли та вивчати шаблони мови Go:\n" +
               "• Керуйте конфігурацією: завантажуйте дефолтний шаблон, створюйте власні налаштування на базі стандартних або редагуйте раніше створені YAML-файли.\n" +
               "• Візуальний конструктор: інтуїтивно налаштовуйте структуру та правила форматування ваших готових книг.\n" +
               "• Швидкий результат: оберіть потрібні параметри та натисніть «Зберегти» — програма сама сформує ваш user.yaml .\n\n" +
               "Розробка: Jurchos & Gemini\n" +
               "Версія: 1.5",
                ["Theme"] = "Тема",
                ["Ok"] = "Зберегти",
                ["Cancel"] = "Скасувати",
                ["Yes"] = "Так",
                ["No"] = "Ні",
                ["Language"] = "Мова:",
                ["DumpConfig"] = $"Завантажити дефолтний {ConfigFileName}",
                ["ConfigName"] = "Назва для власного шаблона:",
                ["CustomYamlEnable"] = "Редагувати user.yaml",
                ["CssEnable"] = "CSS-таблиця стилів",
                ["TocType"] = "Тип навігації (TOC):",
                ["Opt_Toc_Normal"] = "Стандартна (багаторівнева)",
                ["Opt_Toc_OldKindle"] = "Сумісна (старі Kindle)",
                ["Opt_Toc_Flat"] = "Спрощена (один рівень)",
                ["OpenCover"] = "Відкривати книгу з обкладинки",
                ["FixZip"] = "Вилучити дескриптор даних (Fix ZIP)",
                ["Fb2Name"] = "Використати назву fb2 для вихідного файлу",
                ["DefaultName"] = "Еталонна назва для вихідного файлу",
                ["OutNameTitle"] = "Структура назви вихідного файлу",
                ["AsFolder"] = "як папка",
                ["Translit"] = "Транслітерувати назву вихідного файлу",
                ["ReaderSize"] = "Розмір екрана рідера (Ш/В/DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
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
                ["FootnotesMode"] = "Режим виносок:",
                ["Opt_Note_Default"] = "Стандартний (посилання)",
                ["Opt_Note_Float"] = "Спливаючий (float/popup)",
                ["Opt_Note_FloatRen"] = "Спливаючий + нумерація",
                ["SoftHyphen"] = "Вставка м'яких переносів",
                ["RemoveTransp"] = "Вилучити прозорість зображень",
                ["JpegQuality"] = "Рівень якості JPEG (40-100%)",
                ["GenCover"] = "Генерувати обкладинку",
                ["CoverPath"] = "Шлях до обкладинки:",
                ["ResizeCover"] = "Розмір обкладинки:",
                ["Opt_Resize_None"] = "Не змінювати",
                ["Opt_Resize_KeepAR"] = "Зберегти пропорції",
                ["Opt_Resize_Stretch"] = "Розтягнути на екран",
                ["AnnEnable"] = "Анотація окремим розділом",
                ["AnnInToc"] = "Включати анотацію до змісту",
                ["TocPlacement"] = "Зміст окремою сторінкою",
                ["Opt_TocPlace_None"] = "Не створювати",
                ["Opt_TocPlace_Before"] = "На початку книги",
                ["Opt_TocPlace_After"] = "В кінці книги",
                ["PageMapEnable"] = "Генерація карти сторінок",
                ["PageMapSize"] = "Розмір сторінки (min: 500)",
                ["AdobeDe"] = "Підтримка Adobe RMSDK/ADE",
                ["UseBroken"] = "Обробка невалідних зображень",
                ["ScaleFactor"] = "Коефіцієнт розміру картинки",
                ["ImgOptimize"] = "Оптимізація зображень",
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
                ["LogLevel"] = "Рівень логування:",
                ["Opt_Log_None"] = "Вимкнено (none)",
                ["Opt_Log_Normal"] = "Звичайний (normal)",
                ["Opt_Log_Debug"] = "Розширений (debug)",
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
                ["ResetTitle"] = "Перезапуск програми",
                ["ResetConfirm"] = "Ви впевнені, що хочете скинути налаштування конфігурації до початкового стану?\n(Параметри мови та теми будуть збережені)",
                ["SaveErrorTitle"] = "Помилка збереження",
                ["ErrReadOnly"] = "Помилка доступу:\n Файл '{0}' заблоковано!\nПеревірте, чи не встановлено атрибут 'Тільки для читання', або чи не відкритий він в іншій програмі.",
                ["ErrAccessDenied"] = "Помилка доступу:\n Відмовлено в доступі до файлу '{0}'!\nЗапустіть програму від імені Адміністратора.",
                ["ErrDirNotFound"] = "Папку, на яку вказує шлях '{0}', не знайдено!",
                ["ErrTitle"] = "Помилка конфігурації",
                ["ErrFbc"] = "Відсутня програма-конвертор: перевірте наявність файлу 'fbc.exe' в папці з програмою!",
                ["SaveSuccessTitle"] = "Успіх",
                ["SaveSuccess"] = "Конфігурацію успішно збережено у файл {0}!",
                ["YamlTitle"] = "Помилка YAML",
                ["YamlErr"] = $"Помилка: Ключ '{{0}}' не знайдено у файлі {ConfigFileName}!",
                ["GenTitle"] = "Успіх",
                ["GenSuccess"] = "Файл {0} успішно збережено в папку 'Data'."
            },

            // 3. РОСІЙСЬКА ЛОКАЛІЗАЦІЯ
            ["Russian"] = new()
            {
                ["Title"] = "Конфигуратор шаблона fb2cng",
                ["Help"] = "Справка",
                ["HelpText"] = "«Конфигуратор шаблона fb2cng»\nРазработано для набора инструментов fb2cng GUI.\n\n" +
               "Если лень вручную редактировать YAML-файлы и изучать шаблоны языка Go:\n" +
               "• Управление конфигурацией: извлекайте дефолтный шаблон, создавайте свои настройки на базе стандартных или редактируйте ранее созданные YAML-файлы.\n" +
               "• Визуальный конструктор: интуитивно настраивайте структуру и правила форматирования ваших готовых книг.\n" +
               "• Быстрый результат: выберите нужные параметры и нажмите «Сохранить» — программа сама сформирует ваш user.yaml .\n\n" +
               "Разработка: Jurchos & Gemini\n" +
               "Версия: 1.5",
                ["Theme"] = "Тема",
                ["Ok"] = "Сохранить",
                ["Cancel"] = "Отмена",
                ["Yes"] = "Да",
                ["No"] = "Нет",
                ["Language"] = "Язык:",
                ["DumpConfig"] = $"Загрузить дефолтный {ConfigFileName}",
                ["ConfigName"] = "Имя пользовательского шаблона:",
                ["CustomYamlEnable"] = "Редактировать user.yaml",
                ["CssEnable"] = "CSS-таблица стилей",
                ["TocType"] = "Тип навигации (TOC):",
                ["Opt_Toc_Normal"] = "Стандартная (многоуровневая)",
                ["Opt_Toc_OldKindle"] = "Совместимая (старые Kindle)",
                ["Opt_Toc_Flat"] = "Упрощенная (один уровень)",
                ["OpenCover"] = "Открывать книгу с обложки",
                ["FixZip"] = "Удалить дескриптор данных (Fix ZIP)",
                ["Fb2Name"] = "Использовать имя fb2 для выходного файла",
                ["DefaultName"] = "Эталонное имя для выходного файла",
                ["OutNameTitle"] = "Структура имени выходного файла",
                ["AsFolder"] = "как папка",
                ["Translit"] = "Транслитерировать имя выходного файла",
                ["ReaderSize"] = "Размер экрана ридера (Ш/В/DPI)",
                ["Width"] = "W:",
                ["Height"] = "H:",
                ["Dpi"] = "DPI:",
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
                ["FootnotesMode"] = "Режим сносок:",
                ["Opt_Note_Default"] = "Стандартный (ссылки)",
                ["Opt_Note_Float"] = "Всплывающий (float/popup)",
                ["Opt_Note_FloatRen"] = "Всплывающий + нумерация",
                ["SoftHyphen"] = "Вставка мягких переносов",
                ["RemoveTransp"] = "Удалить прозрачность изображений",
                ["JpegQuality"] = "Уровень качества JPEG (40-100%)",
                ["GenCover"] = "Генерировать обложку",
                ["CoverPath"] = "Путь к обложке:",
                ["ResizeCover"] = "Размер обложки:",
                ["Opt_Resize_None"] = "Не изменять",
                ["Opt_Resize_KeepAR"] = "Сохранить пропорции",
                ["Opt_Resize_Stretch"] = "Растянуть на экран",
                ["AnnEnable"] = "Аннотация отдельным разделом",
                ["AnnInToc"] = "Включать аннотацию в оглавление",
                ["TocPlacement"] = "Оглавление отдельной страницей",
                ["Opt_TocPlace_None"] = "Не создавать",
                ["Opt_TocPlace_Before"] = "В начале книги",
                ["Opt_TocPlace_After"] = "В конце книги",
                ["PageMapEnable"] = "Генерация карты страниц",
                ["PageMapSize"] = "Размер страницы (min: 500)",
                ["AdobeDe"] = "Поддержка Adobe RMSDK/ADE",
                ["UseBroken"] = "Обработка невалидных изображений",
                ["ScaleFactor"] = "Коэффициент размера картинки",
                ["ImgOptimize"] = "Оптимизация изображений",
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
                ["LogLevel"] = "Уровень логирования:",
                ["Opt_Log_None"] = "Выключено (none)",
                ["Opt_Log_Normal"] = "Стандартный (normal)",
                ["Opt_Log_Debug"] = "Расширенный (debug)",
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
                ["ResetTitle"] = "Перезапуск программы",
                ["ResetConfirm"] = "Вы уверены, что хотите сбросить настройки конфигурации до начального состояния?\n(Язык и тема будут сохранены)",
                ["ErrTitle"] = "Ошибка конфигурации",
                ["ErrFbc"] = "Программа-конвертер не найдена: проверьте наличие файла 'fbc.exe' в папке с программой!",
                ["SaveErrorTitle"] = "Ошибка сохранения",
                ["ErrReadOnly"] = "Ошибка доступа:\n Файл '{0}' заблокирован!\nПроверьте, не установлен ли атрибут 'Только для чтения', или не открыт ли он в другой программе.",
                ["ErrAccessDenied"] = "Ошибка доступа:\n Отказано в доступе к файлу '{0}'!\nЗапустите программу от имени Администратора.",
                ["ErrDirNotFound"] = "Указанный путь к папке '{0}' не существует!",
                ["SaveSuccessTitle"] = "Успех",
                ["SaveSuccess"] = "Конфигурация успешно сохранена в файл {0}!",
                ["YamlTitle"] = "Ошибка YAML",
                ["YamlErr"] = $"Ошибка: Ключ '{{0}}' не найден в файле {ConfigFileName}!",
                ["GenTitle"] = "Успех",
                ["GenSuccess"] = "Файл {0} успешно сохранен в папку 'Data'."
            }
        };
    }
}