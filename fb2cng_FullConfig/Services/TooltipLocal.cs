namespace fb2cng_FullConfig.Services
{
    public static class TooltipLocal
    {
        public static Dictionary<string, Dictionary<string, string>> Dictionary { get; } = new()
        {
            ["English"] = new()
            {
                ["ConfigName"] = "A field to specify the name of your custom YAML configuration file.\n" +
                "This allows you to save different conversion profiles (e.g., separate ones for Kindle and PocketBook) in the 'Data' folder or any other path you specify\n" +
                "(Note: The Configurator does not create new folders; any directories in the path must be created manually in advance).",

                ["CustomYamlEnable"] = "Allows you to select an existing configuration file (e.g., user.yaml) for editing.\n" +
                "File selection is done via a dialog box (the 'Folder' button).",

                ["CssEnable"] = "Enables the connection of an external stylesheet (.css).\n" +
                "If no path is specified, the program will use the converter's built-in standard CSS, which ensures basic proper rendering of FB2 elements.",

                ["TocType"] = "Defines the type of navigation hierarchy for the interactive Table of Contents (TOC) and device navigation maps:\n" +
                "Standard ('normal') — preserves section nesting;\n" +
                "Compatible ('old_kindle') — limits nesting levels for compatibility with older Amazon devices;\n" +
                "Simplified ('flat') — makes all entries siblings on a single level.",

                ["FixZip"] = "Some programs (CoolReader and certain versions of FBReader) expect a ZIP archive that does not contain a data descriptor.\n" +
                "Enable this option if the resulting epub/kepub files appear corrupted or fail to open on your device.",

                ["OpenCover"] = "Forces the e-reader to open the book directly on the cover page rather than at the beginning of the text.\n" +
                "Note: On Kindle devices, this function is usually ignored by the device's own operating system.",

                ["Translit"] = "Automatic transliteration of the output filename (converting Cyrillic characters to Latin).\n" +
                "This action is performed at the very last stage, after the name has been generated based on the selected structure template.",

                ["Fb2Name"] = "The output file will be named after the original .fb2 file (.SourceFile), cleaned of technical junk.\n" +
                "Automatically removed: archive UUIDs, digital BookIDs at the end of the name (_123456), library tags ([Litres], litres, " +
                "flibusta, author.today, etc.), and technical suffixes (_fb2, _full, v1.0, etc.).\n" +
                "Underscores are replaced with spaces, double spaces and trailing whitespaces are removed.",

                ["DefaultName"] = "For the file name, the converter's original default template will be applied: 'Author's Last Name First Name - Book Title'.",

                ["OutNameTitle"] = "A constructor for creating your own file and folder structure.\n" +
                "Allows you to automatically create subfolders (e.g., by author or series) and generate complex filenames based on book metadata.",

                ["SoftHyphen"] = "Inserts hidden soft hyphen symbols (\\u00AD) inside words " +
                "for devices that do not support proper automatic hyphenation.\n" +
                "May cause conflicts with the built-in hyphenation function of the viewer. For example, in modern Kindle models, hyphenation is always " +
                "enabled for Russian (and disabled for English).",

                ["PageMapEnable"] = "Generates a virtual page map (NCX page-map).\n" +
                "Allows you to see fixed page numbers, as in a printed book, regardless of the font size.\n" +
                "Adobe RMSDK/ADE does not support the pageList element in NCX. Instead, it uses its own method (enabled below).",

                ["PageMapSize"] = "Defines the conventional 'page' size in Unicode characters. The minimum allowed value is 500 characters.\n" +
                "Usually, a value of 2300 is used to simulate a standard printed page.",

                ["AdobeDe"] = "Enables proprietary page number support for Adobe engines (ADE/RMSDK).\n" +
                "This may cause errors during official EpubCheck validation, but it is necessary for proper ADE functionality.\n" +
                "Relevant only for EPUB2/KEPUB.",

                ["UseBroken"] = "Do not skip images that the converter cannot decode on its own. " +
                "Such images will be included in the book in their original form, giving your device a chance to try and render them using its own capabilities.",

                ["RemoveTransp"] = "Kindle devices with E-Ink screens often incorrectly render transparency in PNG/GIF files. This option replaces the transparent background with white.\n" +
                "For Kindle output formats, this function is applied automatically.",

                ["ScaleFactor"] = "Global scaling of all images in the book (except for the cover) according to the specified ratio. " +
                "Helps significantly reduce the size of the final file if the original illustrations are too large.",

                ["ImgOptimize"] = "Re-compress all supported images (currently JPEG, PNG).\n" +
                "For JPEG, quality depends on the 'JPEG Quality Level' (setting below): if the detected quality level is higher than requested, the image will be re-encoded. " +
                "For PNG, the maximum compression level is always set. Additionally, JPEG images are checked for grayscale.\n" +
                "NOTE: For the Kindle output format, some conversion will still occur even with this option disabled, as raster images " +
                "are normalized to JPEG, and vector (SVG) images are rasterized.",

                ["JpegQuality"] = "Defines the quality level (compression ratio) for JPEG images, ranging from 40% to 100%. " +
                "This parameter also affects the quality of SVG-to-raster copies and PNG/GIF to JPEG conversion for Kindle.",

                ["ReaderSize"] = "Sets the physical screen resolution of your reader (Width, Height, and DPI) for correct image adjustment.\n" +
                "Width and height are specified in device pixels; for PDF, they are used along with the DPI value to calculate a fixed page size in PDF points.\n" +
                "This is critical for proper cover preparation and other operations, such as SVG rasterization for Kindle devices.",

                ["GenCover"] = "Automatic creation of a cover if it is missing from the source FB2 metadata.\n" +
                "For Amazon formats, this function is always active by default.",

                ["ResizeCover"] = "Method for fitting the cover to the screen resolution:\n" +
                "Original size ('none') — do nothing (for Kindle output, treated as 'keepAR');\n" +
                "Keep Aspect Ratio ('keepAR') — scales the image to the device height while preserving proportions;\n" +
                "Stretch to fill ('stretch') — stretches the image to fill the entire screen, ignoring the original aspect ratio;\n" +
                "Fit to screen ('fit') — resize up or down to fit specified screen width and height while keeping aspect ratio.",

                ["FootnotesMode"] = "Footnote processing method:\n" +
                "Standard ('default') — regular links at the end of the book;\n" +
                "Floating ('float') — pop-up windows or footnotes at the bottom of the page (for PDF);\n" +
                "Floating + Renumbering ('floatRenumbered') — same as 'float', but footnotes use sequential numbering " +
                "controlled by output format rules.\n" +
                "NOTE: The actual generated content will follow output format requirements:\nfor epub2 and kepub, floating footnotes use the 'bi-directional links' method " +
                "(A -> B and B -> A),\nfor epub3, they use the <aside> element and required <epub> elements, and\nfor PDF float modes, footnotes are rendered as printed " +
                "local footnotes at the bottom of the current page or continuation pages.",

                ["AnnEnable"] = "Creates a separate section (chapter) for the book's annotation if available in the metadata. " +
                "This allows you to read a brief description of the work on a dedicated page within the book.",

                ["AnnInToc"] = "Adds an 'Annotation' entry to the navigation Table of Contents (TOC). This makes it easier to find the book description via your device's quick navigation menu.",

                ["TocPlacement"] = "Enables an additional Table of Contents page (some devices may not support the built-in epub TOC at all):\n" +
                "None ('none') — no visual TOC page will be created;\n" +
                "Before content ('before') — adds the TOC at the beginning of the book after the cover;\n" +
                "After content ('after') — places the TOC at the very end of the edition.",

                ["InclNoTitle"] = "Adds sections without titles to the general Table of Contents.\n" +
                "This prevents an empty navigation menu in books with poor formatting.",

                ["Vignettes"] = "Graphic ornaments (vignettes) placed above/below chapter titles or at the end of sections.\n" +
                "Via the Configurator settings, only built-in ('builtin') resources will be applied. To use your own vignette images, " +
                "you must edit the YAML file manually, specifying the path to the correct image file instead of 'builtin'.",

                ["Dropcaps"] = "Automatic drop cap styling (enlarged first letters of paragraphs).",

                ["LogLevel"] = "Work log detail level:\n" +
                "Disabled ('none') — no log files will be created;\n" +
                "Standard ('normal') — only basic information will be written to the log file;\n" +
                "Full ('debug') — the most detailed report, necessary for troubleshooting conversion errors.",

                ["LogName"] = "Filename template for the standard log file (conversion journal).\n" +
                "Available options for easy report sorting:\n" +
                "by app name fbc (default),\n" +
                "by output filename with extension (name + format),\n" +
                "by conversion time and output filename (time + name), and\n" +
                "by output filename and timestamp (name + tag).",

                ["LogPanicName"] = "Special filename template for logs automatically created by the engine in case of a critical failure (Panic) during conversion operations.\n" +
                "Sorting options are identical to standard logs.",

                ["LogMode"] = "The log update method determines how the log file is handled during each conversion run:\n" +
                "Replace ('overwrite') — completely clears the file and records only the data from the last session;\n" +
                "Append ('append') — preserves the entire history of previous runs, adding new data to the end.",

                ["LogFolder"] = "If activated ('Yes'), all log files (.log) will be automatically created and stored in a separate 'logs' " +
                "folder within the application's working directory.\n" +
                "Important: this option is activated (deactivated) only when selected simultaneously with the log name template and/or the panic log template.",

                ["ShowTooltips"] = "Toggle tooltip display.\n" +
                 "If you are already familiar with all the settings, you can disable them to simplify the interface."
            },
            ["Ukrainian"] = new()
            {
                ["ConfigName"] = "Поле для зазначення назви власного файлу налаштувань YAML.\n" +
                "Це дозволяє зберігати різні профілі конвертації (наприклад, окремо для Kindle та PocketBook) у папці 'Data' чи за іншим вказаним вами шляхом.\n" +
                "(Увага: Конфігуратор не створює папки, директорії в шляху мають бути створені заздалегідь самостійно).",

                ["CustomYamlEnable"] = "Дозволяє обрати для редагування вже наявний файл конфігурації (наприклад: user.yaml).\n" +
                "Вибір файлу здійснюється через діалогове вікно (кнопка 'Папка').",

                ["CssEnable"] = "Можливість підключити зовнішню таблицю стилів (.css).\n" +
                "Якщо шлях не вказано, програма використає вбудований стандартний CSS конвертера, який забезпечує базове правильне відображення елементів FB2.",

                ["TocType"] = "Визначає тип навігаційної ієрархії для інтерактивного змісту (TOC) та навігаційних карт пристрою:\n" +
                "Стандартна ('normal') — зберігає вкладеність розділів;\n" +
                "Сумісна ('old_kindle') — обмежує рівні для сумісності зі старими пристроями Amazon;\n" +
                "Спрощена ('flat') — робить усі пункти списком одного рівня.",

                ["FixZip"] = "Деякі програми (CoolReader та деякі версії FBReader) очікують ZIP-архів, який не містить дескриптора даних.\n" +
                "Увімкніть цей параметр, якщо отримані файли epub/kepub відображаються як пошкоджені або не відкриваються на вашому пристрої.",

                ["OpenCover"] = "Змушує читалку відкривати книгу відразу на сторінці обкладинки, а не на початку тексту.\n" +
                "Зверніть увагу: на пристроях Kindle ця функція зазвичай ігнорується самою операційною системою пристрою.",

                ["Translit"] = "Автоматична транслітерація назви вихідного файлу (перетворення кирилиці в латиницю).\n" +
                "Ця дія виконується на самому останньому етапі, вже після того, як назву було сформовано за обраним шаблоном структури.",

                ["Fb2Name"] = "Вихідний файл отримає назву оригінального файлу fb2 (.SourceFile), очищену від технічного сміття.\n" +
                "Автоматично видаляються: архівні UUID, цифрові BookID у кінці назви (_123456), рекламні мітки бібліотек ([Литрес], litres, flibusta, " +
                "author.today та ін.) та технічні суфікси (_fb2, _full, v1.0 тощо).\n" +
                "Символи підкреслення замінюються на пробіли, видаляються подвійні пробіли та зайві пустоти.",

                ["DefaultName"] = "Для назви файлу буде застосовано оригінальний шаблон автора конвертера за замовчуванням: 'Прізвище Ім'я автора - Назва книги'.",

                ["OutNameTitle"] = "Конструктор для створення власної структури назви файлу та папок.\n" +
                "Дозволяє автоматично створювати вкладені папки (наприклад, за автором чи серією) та формувати складні назви файлів на основі метаданих книги.",

                ["SoftHyphen"] = "Вставляє приховані символи переносу (\\u00AD) всередині слів " +
                "для пристроїв, які не підтримують правильний автоматичний перенос.\n" +
                "Може спричинити конфлікт із вбудованою функцією розбиття слів у програмі перегляду. Наприклад, у сучасних моделях Kindle функція розбиття слів завжди " +
                "увімкнена для російської мови (а для англійської — вимкнена).",

                ["PageMapEnable"] = "Генерація віртуальної карти сторінок (NCX page-map).\n" +
                "Дозволяє бачити фіксовані номери сторінок, як у паперовій книзі, незалежно від розміру шрифту.\n" +
                "Adobe RMSDK/ADE не підтримує елемент pageList в NCX. Замість цього він використовує власний метод (вмикається нижче).",

                ["PageMapSize"] = "Визначає умовний розмір \"сторінки\" в кількості символів Юнікоду. Мінімально допустиме значення — 500 символів.\n" +
                "Зазвичай використовується значення 2300 для імітації стандартної друкованої сторінки.",

                ["AdobeDe"] = "Вмикає пропрієтарну підтримку номерів сторінок для двигунів Adobe (ADE/RMSDK).\n" +
                "Це може викликати помилки при офіційній валідації файлу EpubCheck, але необхідно для коректної роботи ADE.\n" +
                "Актуально тільки для EPUB2/KEPUB.",

                ["UseBroken"] = "Не пропускати зображення, які конвертер не може розкодувати самостійно. " +
                "Такі картинки потраплять у книгу в оригінальному вигляді, що дає вашому пристрою шанс спробувати відобразити їх своїми засобами.",

                ["RemoveTransp"] = "Пристрої Kindle з E-Ink екранами часто некоректно відображають прозорість у PNG/GIF. Ця опція замінює прозорий фон на білий.\n" +
                "Для вихідних форматів Kindle ця функція застосовується автоматично.",

                ["ScaleFactor"] = "Глобальне масштабування всіх зображень у книзі (крім обкладинки) відповідно до вказаного коефіцієнта. " +
                "Допомагає значно зменшити розмір підсумкового файлу, якщо оригінальні ілюстрації занадто великі.",

                ["ImgOptimize"] = "Повторно стискати всі підтримувані зображення (наразі JPEG, PNG).\n" +
                "Для JPEG якість залежить від 'Рівня якості JPEG' (налаштування нижче): якщо виявлений рівень якості зображення вищий за запитуваний, воно буде перекодовано. " +
                "Для PNG завжди встановлюється максимальний рівень стиснення. Крім того, для JPEG-зображень запускається перевірка на наявність відтінків сірого.\n" +
                "ПРИМІТКА: Для вихідного формату Kindle певна конвертація все одно відбуватиметься навіть із вимкненою опцією, оскільки растрові зображення " +
                "нормалізуються до JPEG, а векторні (SVG) — растеризуються.",

                ["JpegQuality"] = "Визначає рівень якості (ступінь стиснення) для JPEG-зображень у межах від 40% до 100%. " +
                "Також цей параметр впливає на якість растрових копій SVG-графіки та конвертацію PNG/GIF у JPEG для Kindle.",

                ["ReaderSize"] = "Встановлення фізичної роздільної здатності екрана вашого рідера (Ширина, Висота та DPI) для коректного налаштування зображень.\n" +
                "Ширина та висота вказуються в пікселях пристрою; для PDF вони використовуються разом із показником DPI, щоб розрахувати фіксований розмір сторінки в пунктах PDF.\n" +
                "Це критично важливо для правильної підготовки обкладинок та деяких інших операцій, наприклад, для растеризації SVG під пристрої Kindle.",

                ["GenCover"] = "Автоматичне створення обкладинки, якщо вона відсутня у метаданих вхідного FB2.\n" +
                "Для форматів Amazon ця функція завжди активна за замовчуванням.",

                ["ResizeCover"] = "Метод підгонки обкладинки під роздільну здатність екрана:\n" +
                "Не змінювати ('none') — нічого не робити (для вихідного формату Kindle розцінюється як 'keepAR');\n" +
                "Зберегти пропорції ('keepAR') — масштабує зображення по висоті пристрою зі збереженням пропорцій;\n" +
                "Розтягнути на екран ('stretch') — розтягує картинку на весь екран без врахування оригінального співвідношення сторін;\n" +
                "Вписати в екран ('fit') — масштабує (збільшує або зменшує) зображення так, щоб воно повністю вписалося в екран із збереженням пропорцій.",

                ["FootnotesMode"] = "Метод обробки виносок:\n" +
                "Стандартний ('default') — звичайні посилання в кінці книги;\n" +
                "Спливаючий ('float') — спливаючі вікна або примітки внизу сторінки (для PDF);\n" +
                "Спливаючий + нумерація ('floatRenumbered') — те саме, що й float, але для посилань на виноски використовується послідовна нумерація, " +
                "що контролюється правилами вихідного формату.\n" +
                "ПРИМІТКА: фактично згенерований вміст відповідатиме вимогам вихідного формату:\nдля epub2 та kepub спливаючі виноски використовують метод \"двонаправлених посилань\" " +
                "(A -> B та B -> A),\nдля epub3 вони використовують елемент <aside> та необхідні елементи <epub>, а\nдля режимів PDF float виноски відображаються як друковані " +
                "локальні виноски внизу поточної сторінки або сторінок продовження.",

                ["AnnEnable"] = "Створює окремий розділ (главу) для анотації книги, якщо вона присутня в метаданих. " +
                "Це дозволяє ознайомитися з коротким описом твору безпосередньо в книзі на окремій сторінці.",

                ["AnnInToc"] = "Додає пункт 'Annotation' до навігаційного змісту ('TOC') книги. Це полегшує пошук опису книги через меню швидкої навігації вашого пристрою.",

                ["TocPlacement"] = "Дозволяє створити додаткову сторінку зі змістом ('TOC') (деякі пристрої можуть взагалі не підтримувати вбудований зміст epub):\n" +
                "Не створювати ('none') — візуальна сторінка змісту не буде створена;\n" +
                "На початку книги ('before') — додає зміст на початку книги після обкладинки;\n" +
                "В кінці книги ('after') — розміщує зміст у самому кінці видання.",

                ["InclNoTitle"] = "Додає розділи, що не мають заголовків, до загального змісту.\n" +
                "Це запобігає ситуації, коли в книгах з поганою версткою навігаційне меню виявляється повністю порожнім.",

                ["Vignettes"] = "Графічні прикраси (віньєтки), що розміщуються над/під заголовками глав або в кінці розділів.\n" +
                "Через налаштування Конфігуратора будуть застосовані лише вбудовані ('builtin') ресурси. Щоб використовувати власні зображення віньєток, " +
                "необхідно відредагувати файл YAML вручну, вказавши замість 'builtin' шлях до файлу зображення.",

                ["Dropcaps"] = "Автоматичне оформлення буквиць (збільшених перших літер абзацу).",

                ["LogLevel"] = "Рівень деталізації журналу роботи:\n" +
                "Вимкнено ('none') — лог-файли не будуть створюватися;\n" +
                "Звичайний ('normal') — до лог-файлу буде записана тільки основна інформація;\n" +
                "Розширений ('debug') — максимально докладний звіт, необхідний для пошуку причин помилок конвертації.",

                ["LogName"] = "Шаблон назви для звичайного файлу логування (журналу конвертації).\n" +
                "Для зручного сортування звітів наявні варіанти:\nза назвою програми fbc (за замовчуванням),\nза назвою вихідного файлу з розширенням (назва + формат),\n" +
                "за часом конвертації та назвою вихідного файлу (час + назва),\nза назвою вихідного файлу та часовою міткою (назва + мітка).",

                ["LogPanicName"] = "Спеціальний шаблон назви для логів, які автоматично створюються двигуном у разі критичного збою (Panic) під час виконання операцій конвертації.\n" +
                "Варіанти сортування звітів ідентичні звичайним логам.",

                ["LogMode"] = "Метод оновлення логу визначає обробку файлу журналу під час кожного запуску конвертації:\n" +
                "Заміна ('overwrite') — повністю очищує файл і записує дані тільки останнього сеансу роботи;\n" +
                "Дозапис ('append') — зберігає всю історію попередніх запусків, додаючи нові дані в кінець.",

                ["LogFolder"] = "Якщо активовано ('Так'), всі файли журналів (.log) будуть автоматично створюватися та зберігатися в окремій папці 'logs' " +
                "всередині робочої директорії програми.\n" +
                "Важливо: дана опція активується (деактивується) лише при одночасному виборі разом шаблоном назви логу та/або шаблоном панік-логу.",

                ["ShowTooltips"] = "Управління показом спливаючих підказок.\n" +
                "Якщо ви вже добре знайомі з усіма налаштуваннями, ви можете їх вимкнути для спрощення інтерфейсу."
            },
            ["Russian"] = new()
            {
                ["ConfigName"] = "Поле для указания имени вашего собственного файла настроек YAML.\n" +
                "Это позволяет хранить различные профили конвертации (например, отдельно для Kindle и PocketBook) в папке 'Data' или по другому указанному пути.\n" +
                "(Внимание: Конфигуратор не создает папки, директории в пути должны быть созданы заранее самостоятельно).",

                ["CustomYamlEnable"] = "Позволяет выбрать для редактирования уже существующий файл конфигурации (например: user.yaml).\n" +
                "Выбор файла осуществляется через диалоговое окно (кнопка 'Папка').",

                ["CssEnable"] = "Возможность подключить внешнюю таблицу стилей (.css).\n" +
                "Если путь не указан, программа использует встроенный стандартный CSS конвертера, который обеспечивает базовое правильное отображение элементов FB2.",

                ["TocType"] = "Определяет тип навигационной иерархии для интерактивного содержания (TOC) и навигационных карт устройства:\n" +
                "Стандартная ('normal') — сохраняет вложенность разделов;\n" +
                "Совместимая ('old_kindle') — ограничивает уровни для совместимости со старыми устройствами Amazon;\n" +
                "Упрощенная ('flat') — делает все пункты списком одного уровня.",

                ["FixZip"] = "Некоторые программы (CoolReader и некоторые версии FBReader) ожидают ZIP-архив, не содержащий дескриптора данных.\n" +
                "Включите этот параметр, если полученные файлы epub/kepub отображаются как поврежденные или не открываются на вашем устройстве.",

                ["OpenCover"] = "Заставляет читалку открывать книгу сразу на странице обложки, а не в начале текста.\n" +
                "Обратите внимание: на устройствах Kindle эта функция обычно игнорируется самой операционной системой устройства.",

                ["Translit"] = "Автоматическая транслитерация названия выходного файла (преобразование кириллицы в латиницу).\n" +
                "Это действие выполняется на самом последнем этапе, уже после того, как название было сформировано по выбранному шаблону структуры.",

                ["Fb2Name"] = "Выходной файл получит название оригинального файла fb2 (.SourceFile), очищенное от технического мусора.\n" +
                "Автоматически удаляются: архивные UUID, цифровые BookID в конце названия (_123456), рекламные метки библиотек ([Литрес], litres, flibusta, " +
                "author.today и др.) и технические суффиксы (_fb2, _full, v1.0 и т.д.).\n" +
                "Символы подчеркивания заменяются пробелами, удаляются двойные пробелы и лишние пустоты.",

                ["DefaultName"] = "Для имени файла будет применен оригинальный шаблон автора конвертера по умолчанию: 'Фамилия Имя автора - Название книги'.",

                ["OutNameTitle"] = "Конструктор для создания собственной структуры имени файла и папок.\n" +
                "Позволяет автоматически создавать вложенные папки (например, по автору или серии) и формировать сложные имена файлов на основе метаданных книги.",

                ["SoftHyphen"] = "Вставляет скрытые символы переноса (\\u00AD) внутри слов " +
                "для устройств, не поддерживающих правильный автоматический перенос.\n" +
                "Может вызвать конфликт со встроенной функцией разбиения слов в программе просмотра. Например, в современных моделях Kindle функция разбиения слов всегда " +
                "включена для русского языка (а для английского — выключена).",

                ["PageMapEnable"] = "Генерация виртуальной карты страниц (NCX page-map).\n" +
                "Позволяет видеть фиксированные номера страниц, как в бумажной книге, независимо от размера шрифта.\n" +
                "Adobe RMSDK/ADE не поддерживает элемент pageList в NCX. Вместо этого он использует собственный метод (включается ниже).",

                ["PageMapSize"] = "Определяет условный размер \"страницы\" в количестве символов Юникода. Минимально допустимое значение — 500 символов.\n" +
                "Обычно используется значение 2300 для имитации стандартной печатной страницы.",

                ["AdobeDe"] = "Включает проприетарную поддержку номеров страниц для движков Adobe (ADE/RMSDK).\n" +
                "Это может вызвать ошибки при официальной валидации файла EpubCheck, но необходимо для корректной работы ADE.\n" +
                "Актуально только для EPUB2/KEPUB.",

                ["UseBroken"] = "Не пропускать изображения, которые конвертер не может декодировать самостоятельно. " +
                "Такие картинки попадут в книгу в оригинальном виде, что дает вашему устройству шанс попробовать отобразить их своими средствами.",

                ["RemoveTransp"] = "Устройства Kindle с E-Ink экранами часто некорректно отображают прозрачность в PNG/GIF. Эта опция заменяет прозрачный фон на белый.\n" +
                "Для выходных форматов Kindle эта функция применяется автоматически.",

                ["ScaleFactor"] = "Глобальное масштабирование всех изображений в книге (кроме обложки) в соответствии с указанным коэффициентом. " +
                "Помогает значительно уменьшить размер итогового файла, если оригинальные иллюстрации слишком велики.",

                ["ImgOptimize"] = "Повторно сжимать все поддерживаемые изображения (в данный момент JPEG, PNG).\n" +
                "Для JPEG качество зависит от 'Уровня качества JPEG' (настройка ниже): если обнаруженный уровень качества изображения выше запрашиваемого, оно будет перекодировано. " +
                "Для PNG всегда устанавливается максимальный уровень сжатия. Кроме того, для JPEG-изображений запускается проверка на наличие оттенков серого.\n" +
                "ПРИМЕЧАНИЕ: Для выходного формата Kindle некоторая конвертация все равно будет происходить даже с выключенной опцией, так как растровые изображения " +
                "нормализуются до JPEG, а векторные (SVG) — растеризуются.",

                ["JpegQuality"] = "Определяет уровень качества (степень сжатия) для JPEG-изображений в пределах от 40% до 100%. " +
                "Также этот параметр влияет на качество растровых копий SVG-графики и конвертацию PNG/GIF в JPEG для Kindle.",

                ["ReaderSize"] = "Установка физического разрешения экрана вашего ридера (Ширина, Высота и DPI) для корректной настройки изображений.\n" +
                "Ширина и высота указываются в пикселях устройства; для PDF они используются вместе с показателем DPI, чтобы рассчитать фиксированный размер страницы в пунктах PDF.\n" +
                "Это критично для правильной подготовки обложек и некоторых других операций, например, для растеризации SVG под устройства Kindle.",

                ["GenCover"] = "Автоматическое создание обложки, если она отсутствует в метаданных входного FB2.\n" +
                "Для форматов Amazon эта функция всегда активна по умолчанию.",

                ["ResizeCover"] = "Метод подгонки обложки под разрешение экрана:\n" +
                "Не изменять ('none') — ничего не делать (для выходного формата Kindle расценивается как 'keepAR');\n" +
                "Сохранить пропорции ('keepAR') — масштабирует изображение по высоте устройства с сохранением пропорций;\n" +
                "Растянуть на экран ('stretch') — растягивает картинку на весь экран без учета оригинального соотношения сторон;\n" +
                "Вписать в экран ('fit') — масштабирует (увеличивает или уменьшает) изображение так, чтобы оно полностью вписалось в экран с сохранением пропорций.",

                ["FootnotesMode"] = "Метод обработки сносок:\n" +
                "Стандартный ('default') — обычные ссылки в конце книги;\n" +
                "Всплывающий ('float') — всплывающие окна или примечания внизу страницы (для PDF);\n" +
                "Всплывающий + нумерация ('floatRenumbered') — то же самое, что и float, но для ссылок на сноски используется последовательная нумерация, " +
                "контролируемая правилами выходного формата.\n" +
                "ПРИМЕЧАНИЕ: фактически сгенерированный контент будет соответствовать требованиям выходного форматов:\nдля epub2 и kepub всплывающие сноски используют метод \"двунаправленных ссылок\" " +
                "(A -> B и B -> A),\nдля epub3 они используют элемент <aside> и необходимые элементы <epub>, а\nдля режимов PDF float сноски отображаются как печатные " +
                "локальные сноски внизу текущей страницы или страниц продолжения.",

                ["AnnEnable"] = "Создает отдельный раздел (главу) для аннотации книги, если она присутствует в метаданных. " +
                "Это позволяет ознакомиться с кратким описанием произведения непосредственно в книге на отдельной странице.",

                ["AnnInToc"] = "Добавляет пункт 'Annotation' в навигационное содержание ('TOC') книги. Это облегчает поиск описания книги через меню быстрой навигации вашего устройства.",

                ["TocPlacement"] = "Позволяет создать дополнительную страницу с содержанием ('TOC') (некоторые устройства могут вообще не поддерживать встроенное содержание epub):\n" +
                "Не создавать ('none') — визуальная страница содержания не будет создана;\n" +
                "В начале книги ('before') — добавляет содержание в начале книги после обложки;\n" +
                "В конце книги ('after') — размещает содержание в самом конце издания.",

                ["InclNoTitle"] = "Добавляет разделы, не имеющие заголовков, в общее содержание.\n" +
                "Это предотвращает ситуацию, когда в книгах с плохой версткой навигационное меню оказывается полностью пустым.",

                ["Vignettes"] = "Графические украшения (виньетки), размещаемые над/под заголовками глав или в конце разделов.\n" +
                "Через настройки Конфигуратора будут применены только встроенные ('builtin') ресурсы. Чтобы использовать собственные изображения виньеток, " +
                "необходимо отредактировать файл YAML вручную, указав вместо 'builtin' путь к файлу изображения.",

                ["Dropcaps"] = "Автоматическое оформление буквиц (увеличенных первых букв абзаца).",

                ["LogLevel"] = "Уровень детализации журнала работы:\n" +
                "Выключено ('none') — лог-файлы не будут создаваться;\n" +
                "Стандартный ('normal') — в лог-файл будет записана только основная информация;\n" +
                "Расширенный ('debug') — максимально подробный отчет, необходимый для поиска причин ошибок конвертации.",

                ["LogName"] = "Шаблон имени для обычного файла логирования (журнала конвертации).\n" +
                "Для удобной сортировки отчетов доступны варианты:\n" +
                "по названию программы fbc (по умолчанию),\n" +
                "по названию выходного файла с расширением (имя + формат),\n" +
                "по времени конвертации и названию выходного файла (время + имя), и\n" +
                "по названию выходного файла и временной метке (имя + метка).",

                ["LogPanicName"] = "Специальный шаблон имени для логов, которые автоматически создаются движком в случае критического сбоя (Panic) во время выполнения операций конвертации.\n" +
                "Варианты сортировки отчетов идентичны обычным логам.",

                ["LogMode"] = "Метод обновления лога определяет обработку файла журнала при каждом запуске конвертации:\n" +
                "Замена ('overwrite') — полностью очищает файл и записывает данные только последнего сеанса работы;\n" +
                "Дозапись ('append') — сохраняет всю историю предыдущих запусков, добавляя новые данные в конец.",

                ["LogFolder"] = "Если активировано ('Да'), все файлы журналов (.log) будут автоматически создаваться и сохраняться в отдельной папке 'logs' " +
                "внутри рабочей директории программы.\n" +
                "Важно: данная опция активируется (деактивируется) только при одновременном выборе вместе с шаблоном имени лога и/или шаблоном паник-лога.",

                ["ShowTooltips"] = "Настройка отображения всплывающих подсказок.\n" +
                "Если вы уже хорошо знакомы со всеми настройками, вы можете отключить их для упрощения интерфейса."
            }
        };
    }
}