namespace fb2cng_FullConfig.Services
{
    public static class TooltipLocal
    {
        public static Dictionary<string, Dictionary<string, string>> Dictionary { get; } = new()
        {
            ["English"] = new()
            {
                ["ConfigName"] = "A field to specify the name of your custom YAML configuration file.\n" +
                "This allows you to save different conversion profiles (e.g., separate ones for Kindle and PocketBook) in the 'Data' folder or any other path you specify.\n" +
                "(Note: The Configurator does not create new folders; any directories in the path must be created manually in advance).",

                ["CustomYamlEnable"] = "Allows you to select an existing configuration file (e.g., user.yaml) for editing.\n" +
                "File selection is done via a dialog box (the 'Folder' button).",

                ["CssEnable"] = "Enables the connection of an external stylesheet (.css).\n" +
                "If no path is specified, the program will use the converter's built-in standard CSS, which ensures basic proper rendering of FB2 elements.",

                ["TocType"] = "Defines the type of navigation hierarchy for the interactive Table of Contents (TOC) and device navigation maps:\n" +
                "• standard ('normal') — preserves section nesting;\n" +
                "• compatible ('old_kindle') — limits nesting levels for compatibility with older Amazon devices;\n" +
                "• simplified ('flat') — makes all entries siblings on a single level.",

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

                ["KfxCompression"] = "Compression method for KFX and AZW8 formats (ignored for other formats):\n" +
                "• none ('none') — text is written into an ordinary container (CONT) without compression. " +
                "Instant creation and guaranteed opening even on the slowest Kindle readers;\n" +
                "• enabled ('lzma') — compresses data using a special internal wrapper (ProtectedData envelope), without DRM encryption.\n" +
                "Page size adapts automatically (4 to 64 KiB), dividing the book into approximately 256 parts.",

                ["Azw3Compression"] = "Compression method for native KF8 output (--to azw3):\n" +
                "• none ('none') — text is written as is, without compression (results in the largest file size);\n" +
                "• standard ('standard') — fast standard PalmDOC compression (default). Optimal balance of speed and size, supported by all devices and apps;\n" +
                "• maximum ('huffdic') — proprietary lossless text compression algorithm using the HUFF/CDIC method. Takes longer to create, but yields the smallest file size.",

                ["Azw3Apnx"] = "Creates an optional APNX page-map sidecar file for AZW3 (config-file only):\n" +
                "• none (default) — do not create a page file, the book will only use Kindle locations;\n" +
                "• eink — create a page file and place it inside the hidden system folder;\n" +
                "• app — place the .apnx file directly next to the book file in the same folder.\n" +
                "Requires the 'Generate page map' option to be enabled. Page numbering is approximate.",

                ["PdfPageFrame"] = "Draws a subtle, nearly-white outline inside every PDF page " +
                "to discourage automatic margin cropping on certain e-readers.\n" +
                "0 — disabled (default). You can specify thickness such as '0.5pt' or '1px' ('px' uses screen DPI).\n" +
                "Drawn inside every physical page, behind the content, without altering the layout.",

                ["MarkdownImages"] = "How images are handled when exporting to Markdown:\n" +
                "• placeholder (text label) — images are not saved. " +
                "Instead of each illustration, a readable text placeholder like [Image: cover.jpg] or an image description appears;\n" +
                "• external (separate folder) — creates a separate folder next to the Markdown file to store image files, with links to them in the text;\n" +
                "• embedded (data:URI) — images are converted into long text strings (data: URI) and embedded directly inside the Markdown file.",

                ["PageMapEnable"] = "Generates a virtual page map (NCX page-map).\n" +
                "Allows you to see fixed page numbers, as in a printed book, regardless of the font size.\n" +
                "Adobe RMSDK/ADE does not support the pageList element in NCX. Instead, it uses its own method (enabled below).",

                ["PageMapSize"] = "Defines the conventional 'page' size in Unicode characters. The minimum allowed value is 500 characters.\n" +
                "Usually, a value of 2300 is used to simulate a standard printed page.",

                ["AdobeDe"] = "Enables proprietary page number support for Adobe engines (ADE/RMSDK).\n" +
                "This may cause errors during official EpubCheck validation, but it is necessary for proper ADE functionality.\n" +
                "Relevant only for EPUB2/KEPUB.",

                ["FootnotesMode"] = "Footnote processing method:\n" +
                "• standard ('default') — regular hyperlinks at the end of the book or section;\n" +
                "• floating ('float') — pop-up windows or footnotes at the bottom of the page (for PDF) keeping original numbering;\n" +
                "• floating + renumbering ('floatRenumbered') — same as float, but all footnotes are automatically renumbered sequentially.\n" +
                "NOTE: footnote content will follow output format requirements:\n" +
                "~ for epub2 and kepub — bi-directional links (back and forth), which most readers recognize as pop-up tooltips;\n" +
                "~ for epub3 and kepub3 — modern interactive pop-ups (using <aside> tags);\n" +
                "~ for pdf — float footnotes are rendered as printed local footnotes at the bottom of the current page or continuation pages;\n" +
                "~ for azw3 — formatted as a separate plain-text paragraph with a leading title for convenient return to the main text.",

                ["AnnEnable"] = "Creates a separate section (chapter) for the book's annotation if available in the metadata. " +
                "This allows you to read a brief description of the work on a dedicated page within the book.",

                ["AnnInToc"] = "Adds an 'Annotation' entry to the navigation Table of Contents (TOC). This makes it easier to find the book description via your device's quick navigation menu.",

                ["TocPlacement"] = "Enables an additional Table of Contents page (some devices may not support the built-in epub TOC at all):\n" +
                "• none ('none') — no visual TOC page will be created;\n" +
                "• before content ('before') — adds the TOC at the beginning of the book after the cover;\n" +
                "• after content ('after') — places the TOC at the very end of the edition.",

                ["InclNoTitle"] = "Adds sections without titles to the general Table of Contents.\n" +
                "This prevents an empty navigation menu in books with poor formatting.",

                ["Vignettes"] = "Graphic ornaments (vignettes) placed above/below chapter titles or at the end of sections.\n" +
                "Via the Configurator settings, only built-in ('builtin') resources will be applied. To use your own vignette images, " +
                "you must edit the YAML file manually, specifying the path to the correct image file instead of 'builtin'.",

                ["Dropcaps"] = "Automatic drop cap styling (enlarged first letters of paragraphs).",

                ["TextTransformSpeech"] = "Automatic cleanup of messy typography in old FB2 files, where direct speech often starts with a hyphen, a minus sign, " +
                "or an inconsistent mix of en-dashes and em-dashes (along with any following spaces).\n" +
                "Replaces these inconsistent variations with a single standard — an em-dash followed by a thin non-breaking space.\n" +
                "Applied only to regular body paragraphs (leaving titles, poems, and quotes untouched).",

                ["TextTransformDashes"] = "Automatic cleanup of messy typography in old FB2 files, where dashes inside sentences may appear as a regular hyphen or a minus sign.\n" +
                "Replaces any dash-like character in the middle of a sentence surrounded by spaces on both sides with a proper typographic (em) dash, " +
                "without affecting hyphens inside words.\n" +
                "Applied only to regular body paragraphs (leaving titles, poems, and quotes untouched).",

                ["TextTransformDialogue"] = "Automatic cleanup of messy typography in old FB2 files, where paragraph indentation was created manually by padding spaces before a dash. " +
                "E-readers usually ignore regular leading spaces at the start of a line, causing the dialogue line to stick to the left edge.\n" +
                "Replaces the group of leading spaces before a dash with a non-breaking space.\n" +
                "Applied only to regular body paragraphs (leaving titles, poems, and quotes untouched).",

                ["ReaderSize"] = "Sets the physical screen resolution of your reader (Width, Height, and DPI) for correct image adjustment.\n" +
                "Width and height are specified in device pixels; for PDF, they are used along with the DPI value to calculate a fixed page size in PDF points.\n" +
                "This is critical for proper cover preparation and other operations, such as SVG rasterization for Kindle devices.",

                ["UseBroken"] = "Do not skip images that the converter cannot decode on its own. " +
                "Such images will be included in the book in their original form, giving your device a chance to try and render them using its own capabilities.",

                ["RemoveTransp"] = "Controls replacing transparent image backgrounds with solid white during conversion.\n" +
                "• Yes ('true') — transparent elements in images (PNG, GIF, TIFF, WebP) are filled with a white background. " +
                "For EPUB/KEPUB, flattened GIF/TIFF/WebP files are encoded as PNG, while SVG artwork for PDF and Kindle is rasterized on white;\n" +
                "• No ('false') — image transparency is preserved (except for Kindle output, which applies a white background automatically).",

                ["RasterizeSvg"] = "Converts vector SVG images to raster images independently of the optimization setting.\n" +
                "• No ('false') — disabled: formats retain their normal behavior\n" +
                "(PDF and KFX / AZW8 normally use native vectors, AZW3 retains SVG in text, " +
                "EPUB / KEPUB / external Markdown rasterize to PNG with transparency when needed, and Kindle / embedded Markdown use JPEG on white).\n" +
                "• Yes ('true') — enabled: forces SVG rasterization.\n" +
                "PDF keeps intrinsic SVG layout sizing, and Kindle covers remain JPEG.",

                ["RasterFormat"] = "Raster image encoding policy:\n" +
                "• auto — standard behavior for the selected output format.\n" +
                "• jpeg — converts decodable raster images and rasterized SVGs to JPEG on white (even when optimization and transparency removal are disabled). " +
                "Does not force SVG rasterization or change sizing policies. Uses 'JPEG quality level'.\n" +
                "For Kobo, takes precedence over WebP-to-PNG conversion.",

                ["ScaleFactor"] = "Global scaling of all images in the book (except for the cover) according to the specified ratio. " +
                "Helps significantly reduce the size of the final file if the original illustrations are too large.",

                ["FitToScreen"] = "Limits the size of body illustrations to the specified screen dimensions while preserving aspect ratio.\n" +
                "Applied in a single pass after 'Image scale factor' (scale_factor): " +
                "the scaled size is calculated first, then the screen-size limit is applied.\n" +
                "Small images are enlarged only if scale_factor > 1.\n" +
                "Does not affect covers or PDF SVG sizing.",

                ["BaselineJpeg"] = "Re-encodes decodable JPEGs of other types (e.g., progressive) into baseline JPEG for older readers with limited support.\n" +
                "Applies to covers and illustrations regardless of 'Optimize images'. Images already in baseline format are not re-encoded unnecessarily.\n" +
                "Uses 'JPEG quality level' (re-encoding is lossy).",

                ["Grayscale"] = "Converts raster covers, illustrations, and rasterized SVGs to grayscale:\n" +
                "• none — preserves color;\n" +
                "• luma — brightness-weighted grayscale (0.299 R + 0.587 G + 0.114 B). " +
                "JPEGs are saved in single-channel format, and PNG transparency is preserved without darkening semi-transparent areas. " +
                "Other non-JPEG formats become PNG (unless the book format requires JPEG).\n" +
                "Vector graphics and text colors remain unchanged.",

                ["ImgOptimize"] = "Re-compress all supported images (currently JPEG, PNG).\n" +
                "For JPEG, quality depends on the 'JPEG Quality Level' (setting below): if the detected quality level is higher than requested, the image will be re-encoded. " +
                "For PNG, the maximum compression level is always set. Additionally, JPEG images are checked for grayscale.\n" +
                "NOTE: For the Kindle output format, some conversion will still occur even with this option disabled, as raster images " +
                "are normalized to JPEG, and vector (SVG) images are rasterized.",

                ["JpegQuality"] = "Defines the quality level (compression ratio) for JPEG images, ranging from 40% to 100%. " +
                "This parameter also affects the quality of SVG-to-raster copies and PNG/GIF to JPEG conversion for Kindle.",

                ["GenCover"] = "Automatic creation of a cover if it is missing from the source FB2 metadata.\n" +
                "For Amazon formats, this function is always active by default.",

                ["ResizeCover"] = "Method for fitting the cover to the screen resolution:\n" +
                "• original size ('none') — do nothing (for Kindle output, treated as 'keepAR');\n" +
                "• keep aspect ratio ('keepAR') — scales the image to the device height while preserving proportions when the image height is less than the screen height;\n" +
                "• stretch to fill ('stretch') — stretches the image to fill the entire screen, ignoring the original aspect ratio;\n" +
                "• fit to screen ('fit') — resize up or down to fit specified screen width and height while keeping aspect ratio;\n" +
                "• fill screen ('fill') — scales the image to fill the screen width and height while preserving proportions; part of the image may extend beyond the screen.",

                ["LogLevel"] = "Work log detail level:\n" +
                "• disabled ('none') — no log files will be created;\n" +
                "• standard ('normal') — only basic information will be written to the log file;\n" +
                "• full ('debug') — the most detailed report, necessary for troubleshooting conversion errors.",

                ["LogName"] = "Filename template for the standard log file (conversion journal).\n" +
                "Available options for easy report sorting:\n" +
                "• by app name fbc (default),\n" +
                "• by output filename with extension (name + format),\n" +
                "• by conversion time and output filename (time + name), and\n" +
                "• by output filename and timestamp (name + tag).",

                ["LogPanicName"] = "Special filename template for logs automatically created by the engine in case of a critical failure " +
                "(Panic) during conversion operations.\n" +
                "Sorting options are identical to standard logs.",

                ["LogMode"] = "The log update method determines how the log file is handled during each conversion run:\n" +
                "• Replace ('overwrite') — completely clears the file and records only the data from the last session;\n" +
                "• Append ('append') — preserves the entire history of previous runs, adding new data to the end.",

                ["LogFolder"] = "If activated ('Yes'), all log files (.log) will be automatically created and stored in a separate 'logs' " +
                "folder within the application's working directory.\n" +
                "Important: this option is activated (deactivated) only when selected simultaneously with the log name template and/or the panic log template.",

                ["ShowTooltips"] = "Toggle tooltip display.\n" +
                "If you are already familiar with all the settings, you can disable them to simplify the interface.\n" +
                "* Delay time — in \"Data\\Conf_config.json\" (param. 'TooltipDelayMs')."
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
                "• стандартна ('normal') — зберігає вкладеність розділів;\n" +
                "• сумісна ('old_kindle') — обмежує рівні для сумісності зі старими пристроями Amazon;\n" +
                "• спрощена ('flat') — робить усі пункти списком одного рівня.",

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

                ["KfxCompression"] = "Метод стиснення для форматів KFX та AZW8 (ігнорується для інших форматів):\n" +
                "• відсутнє ('none') — текст записується у звичайний контейнер (CONT) без стиснення. " +
                "Миттєве створення і гарантоване відкриття навіть на слабкому рідері Kindle;\n" +
                "• застосувати ('lzma') — стиснення даних через спеціальну внутрішню обгортку (ProtectedData envelope), але без DRM-шифрування.\n" +
                "Розмір сторінки підбирається автоматично (від 4 до 64 кілобайт), поділяючи книгу приблизно на 256 частин.",

                ["Azw3Compression"] = "Метод стиснення для вихідного формату KF8 (--to azw3):\n" +
                "• відсутнє ('none') — текст записується як є, без компресії (файл буде найбільшим);\n" +
                "• стандартне ('standard') — швидке стандартне стиснення PalmDOC (за замовчуванням). Оптимальний баланс швидкості та розміру, розуміють абсолютно всі пристрої та програми;\n" +
                "• максимальне ('huffdic') — пропрієтарний алгоритм стиснення тексту без втрат за методом HUFF/CDIC. Книга створюватиметься довше, але займатиме найменше місця.",

                ["Azw3Apnx"] = "Створення додаткового файлу карти сторінок APNX для AZW3 (працює лише через конфіг):\n" +
                "• none (default) — не створювати файл сторінок, у книзі будуть лише локації Kindle;\n" +
                "• eink — створити файл сторінок і покласти його в приховану системну папку;\n" +
                "• app — покласти файл .apnx просто поруч із файлом книги в ту саму папку.\n" +
                "Потребує увімкненої опції 'Генерація карти сторінок'. Нумерація є наближеною.",

                ["PdfPageFrame"] = "Створення ледь помітної майже білої рамки всередині кожної сторінки PDF, " +
                "щоб запобігти автоматичному обрізанню полів на деяких рідерах.\n" +
                "0 — вимкнено (за замовчуванням). Можна вказати товщину, наприклад '0.5pt' або '1px' ('px' враховує DPI екрана).\n" +
                "Малюється всередині кожної фізичної сторінки, позаду контенту, не змінюючи верстку.",

                ["MarkdownImages"] = "Спосіб обробки зображень при експорті в Markdown:\n" +
                "• текстова позначка ('placeholder') — картинки не зберігаються. " +
                "Замість кожної ілюстрації в тексті просто з'явиться текстова позначка, наприклад [Image: cover.jpg] або опис картинки;\n" +
                "• зовнішні файли ('external') — буде створена окрема папка поруч із текстовим файлом для збереження файлів зображень, а в самому тексті - посилання на них;\n" +
                "• текстовий код ('embedded') — картинки перетворюються на довгий текстовий код (data: URI) і вбудовуються всередину текстового файлу.",

                ["PageMapEnable"] = "Генерація віртуальної карти сторінок (NCX page-map).\n" +
                "Дозволяє бачити фіксовані номери сторінок, як у паперовій книзі, незалежно від розміру шрифту.\n" +
                "Adobe RMSDK/ADE не підтримує елемент pageList в NCX. Замість цього він використовує власний метод (вмикається нижче).",

                ["PageMapSize"] = "Визначає умовний розмір \"сторінки\" в кількості символів Юнікоду. Мінімально допустиме значення — 500 символів.\n" +
                "Зазвичай використовується значення 2300 для імітації стандартної друкованої сторінки.",

                ["AdobeDe"] = "Вмикає пропрієтарну підтримку номерів сторінок для двигунів Adobe (ADE/RMSDK).\n" +
                "Це може викликати помилки при офіційній валідації файлу EpubCheck, але необхідно для коректної роботи ADE.\n" +
                "Актуально тільки для EPUB2/KEPUB.",

                ["FootnotesMode"] = "Метод обробки виносок:\n" +
                "• стандартний ('default') — звичайні гіперпосилання в кінці книги чи розділу;\n" +
                "• спливаючий ('float') — спливаючі вікна або примітки внизу сторінки (для PDF) з оригінальною нумерацією;\n" +
                "• спливаючий + нумерація ('floatRenumbered') — те саме, що й float, але програма сама перенумеровує всі виноски по порядку.\n" +
                "ПРИМІТКА: вміст виносок відповідатиме вимогам вихідного формату:\n" +
                "~ для epub2 та kepub — двосторонні посилання (туди й назад), що більшість рідерів розпізнає як спливаючі підказки;\n" +
                "~ для epub3 та kepub3 — сучасні інтерактивні спливаючі вікна (через теги <aside>);\n" +
                "~ для pdf — float-виноски відображаються як друковані локальні виноски внизу поточної сторінки або сторінок продовження;\n" +
                "~ для azw3 — оформлюється як окремий текстовий абзац із заголовком для зручного повернення до основного тексту.",

                ["AnnEnable"] = "Створює окремий розділ (главу) для анотації книги, якщо вона присутня в метаданих. " +
                "Це дозволяє ознайомитися з коротким описом твору безпосередньо в книзі на окремій сторінці.",

                ["AnnInToc"] = "Додає пункт 'Annotation' до навігаційного змісту ('TOC') книги. Це полегшує пошук опису книги через меню швидкої навігації вашого пристрою.",

                ["TocPlacement"] = "Дозволяє створити додаткову сторінку зі змістом ('TOC') (деякі пристрої можуть взагалі не підтримувати вбудований зміст epub):\n" +
                "• не створювати ('none') — візуальна сторінка змісту не буде створена;\n" +
                "• на початку книги ('before') — додає зміст на початку книги після обкладинки;\n" +
                "• в кінці книги ('after') — розміщує зміст у самому кінці видання.",

                ["InclNoTitle"] = "Додає розділи, що не мають заголовків, до загального змісту.\n" +
                "Це запобігає ситуації, коли в книгах з поганою версткою навігаційне меню виявляється повністю порожнім.",

                ["Vignettes"] = "Графічні прикраси (віньєтки), що розміщуються над/під заголовками глав або в кінці розділів.\n" +
                "Через налаштування Конфігуратора будуть застосовані лише вбудовані ('builtin') ресурси. Щоб використовувати власні зображення віньєток, " +
                "необхідно відредагувати файл YAML вручну, вказавши замість 'builtin' шлях до файлу зображення.",

                ["Dropcaps"] = "Автоматичне оформлення буквиць (збільшених перших літер абзацу).",

                ["TextTransformSpeech"] = "Автовиправлення кривої типографіки у старих FB2-файлах, де пряма мова часто починається то з дефіса, то з мінуса, " +
                "або з плутанини короткого чи довгого тире (разом із наступними пробілами).\n" +
                "Замінює цю різнобійну конструкцію на єдиний стандарт — довге тире та вузький нерозривний пробіл.\n" +
                "Застосовується лише до звичайних абзаців тексту (не чіпаючи заголовки, вірші чи цитати).",

                ["TextTransformDashes"] = "Автовиправлення кривої типографіки у старих FB2-файлах, де тире всередині речень може бути звичайним дефісом або знаком мінус.\n" +
                "Замінює будь-яку риску, яка стоїть у середині речення і оточена пробілами з обох боків, на правильне типографічне (довге) тире, " +
                "при цьому не зачіпаючи дефіси всередині слів.\n" +
                "Застосовується лише до звичайних абзаців тексту (не чіпаючи заголовки, вірші чи цитати).",

                ["TextTransformDialogue"] = "Автовиправлення кривої типографіки у старих FB2-файлах, де «червоний рядок» зроблено вручну шляхом «набивки» пробілів перед тире. " +
                "Електронні читалки зазвичай ігнорують звичайні пробіли на початку рядка, через що репліка «прилипає» до лівого краю.\n" +
                "Замінює групу початкових пробілів перед тире на значення нерозривного пробілу.\n" +
                "Застосовується лише до звичайних абзаців тексту (не чіпаючи заголовки, вірші чи цитати).",


                ["ReaderSize"] = "Встановлення фізичної роздільної здатності екрана вашого рідера (Ширина, Висота та DPI) для коректного налаштування зображень.\n" +
                "Ширина та висота вказуються в пікселях пристрою; для PDF вони використовуються разом із показником DPI, щоб розрахувати фіксований розмір сторінки в пунктах PDF.\n" +
                "Це критично важливо для правильної підготовки обкладинок та деяких інших операцій, наприклад, для растеризації SVG під пристрої Kindle.",

                ["UseBroken"] = "Не пропускати зображення, які конвертер не може розкодувати самостійно. " +
                "Такі картинки потраплять у книгу в оригінальному вигляді, що дає вашому пристрою шанс спробувати відобразити їх своїми засобами.",

                ["RemoveTransp"] = "Параметр щодо заміни прозорого фону на зображеннях суцільним білим кольором під час конвертації.\n" +
                "• Так ('true') — прозорі елементи на зображеннях (PNG, GIF, TIFF, WebP) заливаються білим фоном. " +
                "Для EPUB/KEPUB такі файли перекодовуються у PNG, а SVG-графіка для PDF та Kindle стає растровою на білому тлі;\n" +
                "• Ні ('false') — прозорість зображень зберігається (окрім формату Kindle, який додає білий фон примусово).",

                ["RasterizeSvg"] = "Перетворює векторні SVG-зображення на растрові незалежно від параметра оптимізації.\n" +
                "• Ні ('false') — вимкнено: формати зберігають типову поведінку\n" +
                "(PDF та KFX / AZW8 зазвичай використовують власні векторні зображення, AZW3 зберігає SVG у тексті, " +
                "EPUB / KEPUB / зовнішній Markdown при потребі растеризують у PNG із прозорістю, а Kindle / вбудований Markdown — у JPEG на білому тлі).\n" +
                "• Так ('true') — увімкнено: примусово растеризує SVG.\n" +
                "У PDF зберігаються вихідні розміри макета SVG, обкладинки для Kindle залишаються у JPEG.",

                ["RasterFormat"] = "Політика кодування растрової графіки:\n" +
                "• auto — стандартна поведінка для вибраного вихідного формату.\n" +
                "• jpeg — перетворює декодовані растрові зображення та растеризовані SVG у JPEG на білому тлі (навіть якщо оптимізація та видалення прозорості вимкнені). " +
                "Не примушує до растеризації SVG та не змінює політику розмірів. Використовує 'Рівень якості JPEG'.\n" +
                "Для Kobo має пріоритет над конвертацією WebP у PNG.",

                ["ScaleFactor"] = "Глобальне масштабування всіх зображень у книзі (крім обкладинки) відповідно до вказаного коефіцієнта. " +
                "Допомагає значно зменшити розмір підсумкового файлу, якщо оригінальні ілюстрації занадто великі.",

                ["FitToScreen"] = "Обмежує розмір ілюстрацій у тексті заданими розмірами екрана зі збереженням пропорцій.\n" +
                "Застосовується за один прохід після врахування параметра 'Коефіцієнт розміру картинки' (scale_factor): " +
                "спочатку обчислюється масштабований розмір, потім застосовується обмеження розміром екрана.\n" +
                "Невеликі зображення збільшуються лише якщо scale_factor > 1.\n" +
                "Не впливає на розміри обкладинок або SVG-елементів у PDF.",

                ["BaselineJpeg"] = "Перекодовує декодовані JPEG інших типів (наприклад, progressive) у базовий формат baseline JPEG для старих читалок з обмеженою підтримкою.\n" +
                "Діє на обкладинки та ілюстрації незалежно від параметра 'Оптимізація зображень'. Зображення, що вже мають формат baseline, не перекодовуються без потреби.\n" +
                "Використовує 'Рівень якості JPEG' (кодування із втратами).",

                ["Grayscale"] = "Перетворює растрові обкладинки, ілюстрації та растеризовані SVG у градації сірого:\n" +
                "• none — колір без змін;\n" +
                "• luma — знебарвлення з урахуванням яскравості каналів (0.299 R + 0.587 G + 0.114 B). " +
                "JPEG зберігається в одноканальному форматі, прозорість PNG зберігається без затемнення напівпрозорих ділянок. " +
                "Інші не-JPEG формати стають PNG (якщо формат книги не вимагає JPEG).\n" +
                "Векторні зображення та колір тексту не змінюються.",

                ["ImgOptimize"] = "Повторне стиснення всіх підтримуваних зображень (наразі JPEG, PNG).\n" +
                "Для JPEG якість залежить від 'Рівня якості JPEG' (налаштування нижче): якщо виявлений рівень якості зображення вищий за запитуваний, воно буде перекодовано. " +
                "Для PNG завжди встановлюється максимальний рівень стиснення. Крім того, для JPEG-зображень запускається перевірка на наявність відтінків сірого.\n" +
                "ПРИМІТКА: Для вихідного формату Kindle певна конвертація все одно відбуватиметься навіть із вимкненою опцією, оскільки растрові зображення " +
                "нормалізуються до JPEG, а векторні (SVG) — растеризуються.",

                ["JpegQuality"] = "Визначає рівень якості (ступінь стиснення) для JPEG-зображень у межах від 40% до 100%. " +
                "Також цей параметр впливає на якість растрових копій SVG-графіки та конвертацію PNG/GIF у JPEG для Kindle.",

                ["GenCover"] = "Автоматичне створення обкладинки, якщо вона відсутня у метаданих вхідного FB2.\n" +
                "Для форматів Amazon ця функція завжди активна за замовчуванням.",

                ["ResizeCover"] = "Метод підгонки обкладинки під роздільну здатність екрана:\n" +
                "• не змінювати ('none') — нічого не робити (для вихідного формату Kindle розцінюється як 'keepAR');\n" +
                "• зберегти пропорції ('keepAR') — масштабує зображення по висоті пристрою зі збереженням пропорцій, коли висота зображення менша за висоту екрана;\n" +
                "• розтягнути на екран ('stretch') — розтягує картинку на весь екран без врахування оригінального співвідношення сторін;\n" +
                "• вписати в екран ('fit') — масштабує (збільшує або зменшує) зображення так, щоб воно повністю вписалося в екран зі збереженням пропорцій;\n" +
                "• заповнити екран ('fill') — масштабує так, щоб заповнити ширину й висоту екрана зі збереженням пропорцій, частина зображення може виходити за межі екрана.",

                ["LogLevel"] = "Рівень деталізації журналу роботи:\n" +
                "• вимкнено ('none') — лог-файли не будуть створюватися;\n" +
                "• звичайний ('normal') — до лог-файлу буде записана тільки основна інформація;\n" +
                "• розширений ('debug') — максимально докладний звіт, необхідний для пошуку причин помилок конвертації.",

                ["LogName"] = "Шаблон назви для звичайного файлу логування (журналу конвертації).\n" +
                "Для зручного сортування звітів наявні варіанти:\n" +
                "• за назвою програми fbc (за замовчуванням),\n" +
                "• за назвою вихідного файлу з розширенням (назва + формат),\n" +
                "• за часом конвертації та назвою вихідного файлу (час + назва),\n" +
                "• за назвою вихідного файлу та часовою міткою (назва + мітка).",

                ["LogPanicName"] = "Спеціальний шаблон назви для логів, які автоматично створюються двигуном у разі критичного збою (Panic) під час виконання операцій конвертації.\n" +
                "Варіанти сортування звітів ідентичні звичайним логам.",

                ["LogMode"] = "Метод оновлення логу визначає обробку файлу журналу під час кожного запуску конвертації:\n" +
                "• Заміна ('overwrite') — повністю очищує файл і записує дані тільки останнього сеансу роботи;\n" +
                "• Дозапис ('append') — зберігає всю історію попередніх запусків, додаючи нові дані в кінець.",

                ["LogFolder"] = "Якщо активовано ('Так'), всі файли журналів (.log) будуть автоматично створюватися та зберігатися в окремій папці 'logs' " +
                "всередині робочої директорії програми.\n" +
                "Важливо: ця опція активується (деактивується) лише при одночасному виборі разом із шаблоном назви логу та/або шаблоном панік-логу.",

                ["ShowTooltips"] = "Керування показом спливаючих підказок.\n" +
                "Якщо ви вже добре знайомі з усіма налаштуваннями, ви можете їх вимкнути для спрощення інтерфейсу.\n" +
                "* Час затримки — в \"Data\\Conf_config.json\" (парам. 'TooltipDelayMs')."
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
                "• стандартная ('normal') — сохраняет вложенность разделов;\n" +
                "• совместимая ('old_kindle') — ограничивает уровни для совместимости со старыми устройствами Amazon;\n" +
                "• упрощенная ('flat') — делает все пункты списком одного уровня.",

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

                ["KfxCompression"] = "Метод сжатия для форматов KFX и AZW8 (игнорируется для других форматов):\n" +
                "• отсутствует ('none') — текст записывается в обычный контейнер (CONT) без сжатия. " +
                "Мгновенное создание и гарантированное открытие даже на слабом ридере Kindle;\n" +
                "• применить ('lzma') — сжатие данных через специальную внутреннюю обёртку (ProtectedData envelope), но без DRM-шифрования.\n" +
                "Размер страницы подбирается автоматически (от 4 до 64 килобайт), разделяя книгу примерно на 256 частей.",

                ["Azw3Compression"] = "Метод сжатия для выходного формата KF8 (--to azw3):\n" +
                "• отсутствует ('none') — текст записывается как есть, без компрессии (файл будет самым большим);\n" +
                "• стандартное ('standard') — быстрое стандартное сжатие PalmDOC (по умолчанию). Оптимальный баланс скорости и размера, " +
                "поддерживается абсолютно всеми устройствами и программами;\n" +
                "• максимальное ('huffdic') — проприетарный алгоритм сжатия текста без потерь по методу HUFF/CDIC. " +
                "Книга будет создаваться дольше, но займёт меньше всего места.",

                ["Azw3Apnx"] = "Создание дополнительного файла карты страниц APNX для AZW3 (работает только через конфиг):\n" +
                "• none (default) — не создавать файл страниц, в книге будут только локации Kindle;\n" +
                "• eink — создать файл страниц и поместить его в скрытую системную папку;\n" +
                "• app — поместить файл .apnx рядом с файлом книги в ту же папку.\n" +
                "Требует включённой опции «Генерация карты страниц». Нумерация является приблизительной.",

                ["PdfPageFrame"] = "Создание едва заметной почти белой рамки внутри каждой страницы PDF, " +
                "чтобы предотвратить автоматическую обрезку полей на некоторых ридерах.\n" +
                "0 — выключено (по умолчанию). Можно указать толщину, например '0.5pt' или '1px' ('px' учитывает DPI экрана).\n" +
                "Рисуется внутри каждой физической страницы, позади контента, не изменяя вёрстку.",

                ["MarkdownImages"] = "Способ обработки изображений при экспорте в Markdown:\n" +
                "• текстовая метка ('placeholder') — картинки не сохраняются. " +
                "Вместо каждой иллюстрации в тексте появится текстовая метка, например [Image: cover.jpg] или описание картинки;\n" +
                "• внешний файл ('external') — рядом с текстовым файлом будет создана отдельная папка для сохранения файлов изображений, а в самом тексте — ссылки на них;\n" +
                "• текстовый код ('embedded') — картинки преобразуются в длинный текстовый код (data: URI) и встраиваются внутрь текстового файла.",

                ["PageMapEnable"] = "Генерация виртуальной карты страниц (NCX page-map).\n" +
                "Позволяет видеть фиксированные номера страниц, как в бумажной книге, независимо от размера шрифта.\n" +
                "Adobe RMSDK/ADE не поддерживает элемент pageList в NCX. Вместо этого он использует собственный метод (включается ниже).",

                ["PageMapSize"] = "Определяет условный размер \"страницы\" в количестве символов Юникода. Минимально допустимое значение — 500 символов.\n" +
                "Обычно используется значение 2300 для имитации стандартной печатной страницы.",

                ["AdobeDe"] = "Включает проприетарную поддержку номеров страниц для движков Adobe (ADE/RMSDK).\n" +
                "Это может вызвать ошибки при официальной валидации файла EpubCheck, но необходимо для корректной работы ADE.\n" +
                "Актуально только для EPUB2/KEPUB.",

                ["FootnotesMode"] = "Метод обработки сносок:\n" +
                "• стандартный ('default') — обычные гиперссылки в конце книги или раздела;\n" +
                "• всплывающий ('float') — всплывающие окна или примечания внизу страницы (для PDF) с оригинальной нумерацией;\n" +
                "• всплывающий + нумерация ('floatRenumbered') — то же, что и float, но программа сама перенумеровывает все сноски по порядку.\n" +
                "ПРИМЕЧАНИЕ: содержимое сносок будет соответствовать требованиям выходного формата:\n" +
                "~ для epub2 и kepub — двусторонние ссылки (туда и обратно), которые большинство ридеров распознаёт как всплывающие подсказки;\n" +
                "~ для epub3 и kepub3 — современные интерактивные всплывающие окна (через теги <aside>);\n" +
                "~ для pdf — float-сноски отображаются как печатные локальные сноски внизу текущей страницы или страниц продолжения;\n" +
                "~ для azw3 — оформляется как отдельный текстовый абзац с заголовком для удобного возврата к основному тексту.",

                ["AnnEnable"] = "Создает отдельный раздел (главу) для аннотации книги, если она присутствует в метаданных. " +
                "Это позволяет ознакомиться с кратким описанием произведения непосредственно в книге на отдельной странице.",

                ["AnnInToc"] = "Добавляет пункт 'Annotation' в навигационное содержание ('TOC') книги. " +
                "Это облегчает поиск описания книги через меню быстрой навигации вашего устройства.",

                ["TocPlacement"] = "Позволяет создать дополнительную страницу с содержанием ('TOC') " +
                "(некоторые устройства могут вообще не поддерживать встроенное содержание epub):\n" +
                "• не создавать ('none') — визуальная страница содержания не будет создана;\n" +
                "• в начале книги ('before') — добавляет содержание в начале книги после обложки;\n" +
                "• в конце книги ('after') — размещает содержание в самом конце издания.",

                ["InclNoTitle"] = "Добавляет разделы, не имеющие заголовков, в общее содержание.\n" +
                "Это предотвращает ситуацию, когда в книгах с плохой версткой навигационное меню оказывается полностью пустым.",

                ["Vignettes"] = "Графические украшения (виньетки), размещаемые над/под заголовками глав или в конце разделов.\n" +
                "Через настройки Конфигуратора будут применены только встроенные ('builtin') ресурсы. Чтобы использовать собственные изображения виньеток, " +
                "необходимо отредактировать файл YAML вручную, указав вместо 'builtin' путь к файлу изображения.",

                ["Dropcaps"] = "Автоматическое оформление буквиц (увеличенных первых букв абзаца).",

                ["TextTransformSpeech"] = "Автоисправление кривой типографики в старых FB2-файлах, где прямая речь часто начинается то с дефиса, то с минуса, " +
                "либо с путаницы короткого или длинного тире (вместе с последующими пробелами).\n" +
                "Заменяет эту разнобойную конструкцию на единый стандарт — длинное тире и узкий неразрывный пробел.\n" +
                "Применяется только к обычным абзацам текста (не затрагивая заголовки, стихи или цитаты).",

                ["TextTransformDashes"] = "Автоисправление кривой типографики в старых FB2-файлах, где тире внутри предложений может быть обычным дефисом или знаком минуса.\n" +
                "Заменяет любую чёрточку в середине предложения, окружённую пробелами с обеих сторон, " +
                "на правильное типографическое (длинное) тире, не исправляя при этом дефисы внутри слов.\n" +
                "Применяется только к обычным абзацам текста (не затрагивая заголовки, стихи или цитаты).",

                ["TextTransformDialogue"] = "Автоисправление кривой типографики в старых FB2-файлах, где «красная строка» сделана вручную путём «набивки» пробелов перед тире. " +
                "Электронные читалки обычно игнорируют обычные пробелы в начале строки, из-за чего реплика «прилипает» к левому краю.\n" +
                "Заменяет группу начальных пробелов перед тире на неразрывный пробел.\n" +
                "Применяется только к обычным абзацам текста (не затрагивая заголовки, стихи или цитаты).",

                ["ReaderSize"] = "Установка физического разрешения экрана вашего ридера (Ширина, Высота и DPI) для корректной настройки изображений.\n" +
                "Ширина и высота указываются в пикселях устройства; для PDF они используются вместе с показателем DPI, чтобы рассчитать фиксированный размер страницы в пунктах PDF.\n" +
                "Это критично для правильной подготовки обложек и некоторых других операций, например, для растеризации SVG под устройства Kindle.",

                ["UseBroken"] = "Не пропускать изображения, которые конвертер не может декодировать самостоятельно. " +
                "Такие картинки попадут в книгу в оригинальном виде, что дает вашему устройству шанс попробовать отобразить их своими средствами.",

                ["RemoveTransp"] = "Параметр замены прозрачного фона изображений сплошным белым цветом при конвертации.\n" +
                "• Да ('true') — прозрачные элементы на изображениях (PNG, GIF, TIFF, WebP) заливаются белым фоном. " +
                "Для EPUB/KEPUB обработанные GIF/TIFF/WebP перекодируются в PNG, а SVG-графика для PDF и Kindle становится растровой на белом фоне;\n" +
                "• Нет ('false') — прозрачность изображений сохраняется (кроме форматов Kindle, которые добавляют белый фон принудительно).",

                ["RasterizeSvg"] = "Преобразует векторные SVG-изображения в растровые независимо от параметра оптимизации.\n" +
                "• Нет ('false') — выключено: форматы сохраняют стандартное поведение \n" +
                "(PDF и KFX / AZW8 обычно используют встроенную векторную графику, AZW3 сохраняет SVG в тексте, " +
                "EPUB / KEPUB / внешний Markdown при необходимости растеризуют в PNG с прозрачностью, а Kindle / встроенный Markdown — в JPEG на белом фоне).\n" +
                "• Да ('true') — включено: принудительно растеризует SVG.\n" +
                "В PDF сохраняются исходные размеры макета SVG, обложки для Kindle остаются в JPEG.",

                ["RasterFormat"] = "Политика кодирования растровой графики:\n" +
                "• auto — стандартное поведение для выбранного выходного формата.\n" +
                "• jpeg — преобразует декодированные растровые изображения и растеризованные SVG в JPEG на белом фоне (даже если оптимизация и удаление прозрачности выключены). " +
                "Не принуждает к растеризации SVG и не изменяет политику размеров. Использует 'Уровень качества JPEG'.\n" +
                "Для Kobo имеет приоритет над конвертацией WebP в PNG.",

                ["ScaleFactor"] = "Глобальное масштабирование всех изображений в книге (кроме обложки) в соответствии с указанным коэффициентом. " +
                "Помогает значительно уменьшить размер итогового файла, если оригинальные иллюстрации слишком велики.",

                ["FitToScreen"] = "Ограничивает размер иллюстраций в тексте заданными размерами экрана с сохранением пропорций.\n" +
                "Применяется за один проход после учёта параметра 'Коэффициент размера картинки' (scale_factor): " +
                "сначала вычисляется масштабированный размер, затем применяется ограничение размером экрана.\n" +
                "Небольшие изображения увеличиваются только если scale_factor > 1.\n" +
                "Не влияет на размеры обложек или SVG-элементов в PDF.",

                ["BaselineJpeg"] = "Перекодирует декодированные JPEG других типов (например, progressive) в базовый формат baseline JPEG для старых читалок с ограниченной поддержкой.\n" +
                "Действует на обложки и иллюстрации независимо от параметра 'Оптимизация изображений'. Изображения, уже имеющие формат baseline, не перекодируются без необходимости.\n" +
                "Использует 'Уровень качества JPEG' (кодирование с потерями).",

                ["Grayscale"] = "Преобразует растровые обложки, иллюстрации и растеризованные SVG в оттенки серого:\n" +
                "• none — цвет без изменений;\n" +
                "• luma — обесцвечивание с учётом яркости каналов (0.299 R + 0.587 G + 0.114 B). " +
                "JPEG сохраняется в одноканальном формате, прозрачность PNG сохраняется без затемнения полупрозрачных участков. " +
                "Другие не-JPEG форматы становятся PNG (если формат книги не требует JPEG).\n" +
                "Векторные изображения и цвет текста не изменяются.",

                ["ImgOptimize"] = "Повторное сжатие всех поддерживаемых изображений (в данный момент JPEG, PNG).\n" +
                "Для JPEG качество зависит от 'Уровня качества JPEG' (настройка ниже): если обнаруженный уровень качества изображения выше запрашиваемого, оно будет перекодировано. " +
                "Для PNG всегда устанавливается максимальный уровень сжатия. Кроме того, для JPEG-изображений запускается проверка на наличие оттенков серого.\n" +
                "ПРИМЕЧАНИЕ: Для выходного формата Kindle некоторая конвертация все равно будет происходить даже с выключенной опцией, так как растровые изображения " +
                "нормализуются до JPEG, а векторные (SVG) — растеризуются.",

                ["JpegQuality"] = "Определяет уровень качества (степень сжатия) для JPEG-изображений в пределах от 40% до 100%. " +
                "Также этот параметр влияет на качество растровых копий SVG-графики и конвертацию PNG/GIF в JPEG для Kindle.",

                ["GenCover"] = "Автоматическое создание обложки, если она отсутствует в метаданных входного FB2.\n" +
                "Для форматов Amazon эта функция всегда активна по умолчанию.",

                ["ResizeCover"] = "Метод подгонки обложки под разрешение экрана:\n" +
                "• не изменять ('none') — ничего не делать (для выходного формата Kindle расценивается как 'keepAR');\n" +
                "• сохранить пропорции ('keepAR') — масштабирует изображение по высоте устройства с сохранением пропорций, если высота изображения меньше высоты экрана;\n" +
                "• растянуть на экран ('stretch') — растягивает картинку на весь экран без учета оригинального соотношения сторон;\n" +
                "• вписать в экран ('fit') — масштабирует (увеличивает или уменьшает) изображение так, чтобы оно полностью вписалось в экран с сохранением пропорций;\n" +
                "• заполнить экран ('fill') — масштабирует так, чтобы заполнить ширину и высоту экрана с сохранением пропорций; " +
                "часть изображения может выходить за пределы экрана.",

                ["LogLevel"] = "Уровень детализации журнала работы:\n" +
                "• отключен ('none') — лог-файлы не будут создаваться;\n" +
                "• стандартный ('normal') — в лог-файл будет записана только основная информация;\n" +
                "• расширенный ('debug') — максимально подробный отчет, необходимый для поиска причин ошибок конвертации.",

                ["LogName"] = "Шаблон имени для обычного файла логирования (журнала конвертации).\n" +
                "Для удобной сортировки отчетов доступны варианты:\n" +
                "• по названию программы fbc (по умолчанию),\n" +
                "• по названию выходного файла с расширением (имя + формат),\n" +
                "• по времени конвертации и названию выходного файла (время + имя), и\n" +
                "• по названию выходного файла и временной метке (имя + метка).",

                ["LogPanicName"] = "Специальный шаблон имени для логов, которые автоматически создаются движком в случае критического сбоя (Panic) во время выполнения операций конвертации.\n" +
                "Варианты сортировки отчетов идентичны обычным логам.",

                ["LogMode"] = "Метод обновления лога определяет обработку файла журнала при каждом запуске конвертации:\n" +
                "• Замена ('overwrite') — полностью очищает файл и записывает данные только последнего сеанса работы;\n" +
                "• Дозапись ('append') — сохраняет всю историю предыдущих запусков, добавляя новые данные в конец.",

                ["LogFolder"] = "Если активировано ('Да'), все файлы журналов (.log) будут автоматически создаваться и сохраняться в отдельной папке 'logs' " +
                "внутри рабочей директории программы.\n" +
                "Важно: данная опция активируется (деактивируется) только при одновременном выборе вместе с шаблоном имени лога и/или шаблоном паник-лога.",

                ["ShowTooltips"] = "Управление показом всплывающих подсказок.\n" +
                "Если вы уже хорошо знакомы со всеми настройками, вы можете отключить их для упрощения интерфейса.\n" +
                "* Время задержки — в \"Data\\Conf_config.json\" (парам. 'TooltipDelayMs')."
            }
        };
    }
}