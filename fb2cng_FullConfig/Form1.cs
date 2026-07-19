using System.Drawing.Drawing2D;
using fb2cng_FullConfig.Templates; // Підключаємо нову папку з вкладками

namespace fb2cng_FullConfig
{
    public partial class Form1 : Form
    {
        // Статичні елементи каркасу програми
        private Panel headerPanel = null!;
        private Panel footerPanel = null!;
        private Panel pnlContent = null!; // Головний центральний контейнер для вкладок

        // Кнопки Хідера 
        private Button btnTabDocument = null!;
        private Button btnTabMetadata = null!;
        private Button btnTabLogging = null!;
        // Кнопки Футера
        private Button btnHelp = null!;
        private Button btnTheme = null!;
        private Button btGui = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        // Кеш для збереження вкладок (щоб при перемиканні назад дані користувача не стиралися)
        private readonly Dictionary<string, UserControl> _tabsCache = [];
        private string _currentActiveTab = "document:";

        // Матриця прозорості іконок футера
        private static readonly float[][] InactiveIconMatrix = [
         [1, 0, 0, 0, 0],
         [0, 1, 0, 0, 0],
         [0, 0, 1, 0, 0],
         [0, 0, 0, 0.30f, 0],
         [0, 0, 0, 0, 1]
        ];

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED (Захист від мерехтіння вкладок при перемиканні)
                return cp;
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;

            try
            {
                SetupMainFramework();     // 1. Будуємо каркас
                SwitchToTab("document:"); // 2. Завантажуємо вкладку в пам'ять
                UpdateLocalization();     // 3. Наповнюємо текстами

                // ПЕРЕНЕСЕНО СЮДИ: Встановлюємо мову ТІЛЬКИ після того, як комбобокс локалізовано!
                if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
                {
                    docTab.langComboBox.SelectedIndex = Config.Settings.CurrentLanguage switch
                    {
                        "Ukrainian" => 1,
                        "Russian" => 2,
                        _ => 0
                    };
                }

                ApplyTheme();             // 4. Фарбуємо тему
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критичний збій ініціалізації вікна:\n\n{ex.Message}",
                                "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupMainFramework()
        {
            float currentScale = Win32Api.GetDpiScale();
            int btnRadius = (int)(6 * currentScale);
            int iconSize = (int)(17 * currentScale);

            // Базові налаштування вікна
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // Задаємо фіксовану ідеальну ширину вікна 
            int calculatedWidth = (int)(515 * currentScale);
            ClientSize = new Size(calculatedWidth, ClientSize.Height);

            // ==========================================
            // КРОК 1: СТВОРЕННЯ ХІДЕРА (ВЕРХНЯ ПАНЕЛЬ)
            // ==========================================
            int rowHeight = (int)(28 * currentScale); // Висота ряду кнопок хідера
            int headerHeight = rowHeight + (int)(8 * currentScale); // Загальна висота 

            headerPanel = new Panel();
            headerPanel.SetBounds(0, 0, ClientSize.Width, headerHeight);
            Controls.Add(headerPanel);

            // --- НАЛАШТУВАННЯ ВІДСТУПІВ КНОПОК ХІДЕРА ---
            int paddingLeft = (int)(13 * currentScale);   // Відступ зліва для першої кнопки (було 16)
            int paddingRight = (int)(14 * currentScale);  // Відступ справа для третьої кнопки (було 16)
            int betweenButtons = (int)(4 * currentScale); // Відступ між самими кнопками
                                                          // --------------------------------------------

            // Автоматичний розрахунок ширини кнопок з урахуванням нових відступів
            int totalInterButtonSpace = betweenButtons * 2; // Два проміжки між трьома кнопками
            int tabWidthRow1 = (headerPanel.Width - paddingLeft - paddingRight - totalInterButtonSpace) / 3;
            int tabWidthRow2 = tabWidthRow1; // Робимо другий ряд таким самим

            // Головна вкладка, метаінформація та логування
            btnTabDocument = new Button { Text = "document:", Tag = "document:" };
            btnTabMetadata = new Button { Text = "metainformation:", Tag = "metadata:" };
            btnTabLogging = new Button { Text = "logging:", Tag = "logging:" };

             // Координати (використовуємо змінні відступів)
            btnTabDocument.SetBounds(paddingLeft, (int)(4 * currentScale), tabWidthRow1, rowHeight);
            btnTabMetadata.SetBounds(btnTabDocument.Right + betweenButtons, btnTabDocument.Top, tabWidthRow1, rowHeight);
            btnTabLogging.SetBounds(btnTabMetadata.Right + betweenButtons, btnTabDocument.Top, tabWidthRow1, rowHeight);

            // Зв'язуємо всі кнопки хідера з одним методом перемикання вкладок
            Button[] tabButtons = [btnTabDocument, btnTabMetadata, btnTabLogging];
            foreach (var btn in tabButtons)
            {
                btn.Click += TabButton_Click;
                MakeButtonRounded(btn, (int)(4 * currentScale)); // Ніжне заокруглення для вкладок
                headerPanel.Controls.Add(btn);
            }

            // ==========================================
            // КРОК 2: СТВОРЕННЯ ЦЕНТРАЛЬНОГО КОНТЕНТ-КОНТЕЙНЕРА
            // ==========================================
            // Базова фіксована висота 
            int contentHeight = (int)(565 * currentScale);

            pnlContent = new Panel();
            pnlContent.SetBounds(0, headerPanel.Bottom, ClientSize.Width, contentHeight);
            Controls.Add(pnlContent);

            // ==========================================
            // КРОК 3: СТВОРЕННЯ СТАТИЧНОГО ФУТЕРА
            // ==========================================
            int footerHeight = (int)(24 * currentScale) + (int)(14 * currentScale);// Висота футера з урахуванням відступів

            footerPanel = new Panel();
            footerPanel.SetBounds(0, pnlContent.Bottom, ClientSize.Width, footerHeight);// Встановлюємо футер чітко під контентом
            Controls.Add(footerPanel);

            // Створення кнопок футера
            btnHelp = new Button { Text = "Help", Image = ResizeImage(Properties.Resources.icon_info, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(2 * currentScale), 0, 0, 0)
            };
            btnTheme = new Button { Text = "Theme", Image = ResizeImage(Properties.Resources.day_night, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(10 * currentScale), 0, 0, 0)
            };
            btGui = new Button { Text = "GUI", ImageAlign = ContentAlignment.MiddleCenter, TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(3 * currentScale), 0, 0, 0)
            };
            if (Properties.Resources.icon_GUI != null) btGui.Image = ResizeImage(Properties.Resources.icon_GUI, iconSize, iconSize);
            btnOk = new Button { Text = "OK" };
            btnCancel = new Button { Text = "Cancel" };

            footerPanel.Controls.AddRange([btnHelp, btnTheme, btGui, btnOk, btnCancel]);

            // Прив'язка гарячих клавіш та подій футера
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            btnTheme.Click += (s, e) => { Config.IsDarkTheme = !Config.IsDarkTheme; ApplyTheme(); Config.SaveSettings(); };
            btnCancel.Click += (s, e) => Close();
            btnHelp.Click += (s, e) => ShowHelp();
            btnOk.Click += (s, e) => SaveYamlConfiguration();
            btGui.Click += BtGui_Click;

            // Roзстановка кнопок футера
            int btnWidth = (int)(90 * currentScale);
            int guiBtnWidth = (int)(65 * currentScale);
            int btnHeight = (int)(24 * currentScale) + (int)(4 * currentScale);
            int btnTop = (int)(5 * currentScale);
            int xLeft = (int)(16 * currentScale);
            int btnspacing = (int)(6 * currentScale); // Відступ між кнопками

            btnHelp.SetBounds(xLeft, btnTop, btnWidth, btnHeight);
            btnTheme.SetBounds(btnHelp.Right + btnspacing, btnTop, btnWidth, btnHeight);
            btGui.SetBounds(btnTheme.Right + btnspacing, btnTop, guiBtnWidth, btnHeight);
            btnCancel.SetBounds(ClientSize.Width - xLeft - btnWidth, btnTop, btnWidth, btnHeight);
            btnOk.SetBounds(btnCancel.Left - (int)(96 * currentScale), btnTop, btnWidth, btnHeight);

            // Заокруглення кнопок футера
            MakeButtonRounded(btnHelp, btnRadius);
            MakeButtonRounded(btnTheme, btnRadius);
            MakeButtonRounded(btGui, btnRadius);
            MakeButtonRounded(btnOk, btnRadius);
            MakeButtonRounded(btnCancel, btnRadius);

            // --- АДАПТАЦІЯ ГЕОМЕТРІЇ ПРИ ВЕЛИКОМУ МАСШТАБІ (200-225%) ---
            int finalHeight = footerPanel.Bottom + (int)(8 * currentScale);
            int maxAllowedHeight = Screen.PrimaryScreen!.WorkingArea.Height - (int)(40 * currentScale);

            if (finalHeight > maxAllowedHeight)
            {
                int heightDeficit = finalHeight - maxAllowedHeight;

                // 1. Стискаємо центральний контейнер контенту під екран користувача
                pnlContent.Height -= heightDeficit;

                // 2. Підтягуємо футер чітко до низу нового контенту
                footerPanel.Top = pnlContent.Bottom;

                // Фіксуємо оновлену висоту програми
                finalHeight = footerPanel.Bottom + (int)(8 * currentScale);
            }

            // Призначаємо фінальні безпечні розміри вікна програми
            ClientSize = new Size(calculatedWidth, finalHeight);

            // Центрування на моніторі
            StartPosition = FormStartPosition.Manual;
            Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Left + ((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2),
                Screen.PrimaryScreen.WorkingArea.Top + ((Screen.PrimaryScreen.WorkingArea.Height - Height) / 2)
            );

        }

        /// <summary>
        /// Обробник події кліку по кнопках Хідера. Визначає, яку саме вкладку викликав користувач.
        /// </summary>
        private void TabButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button tabButton && tabButton.Tag is string tabName)
            {
                SwitchToTab(tabName);
                ApplyTheme(); // Перезапускаємо тему, щоб кольори нових вкладок оновилися миттєво
            }
        }

        /// <summary>
        /// Динамічно замінює вміст центральної панелі на обрану вкладку з використанням кешування.
        /// </summary>
        private void SwitchToTab(string tabName)
        {
            if (_currentActiveTab == tabName && pnlContent.Controls.ContainsKey(tabName)) return;

            _currentActiveTab = tabName;
            SuspendLayout();

            // Ховаємо всі існуючі вкладки, замість того щоб їх видаляти
            foreach (Control ctrl in pnlContent.Controls)
            {
                ctrl.Visible = false;
            }

            if (!_tabsCache.TryGetValue(tabName, out var tabControl))
            {
                tabControl = tabName switch
                {
                    "document:" => new DocumentTab(),
                    "metadata:" => new MetadataTab(),
                    "logging:" => new LoggingTab(),
                    _ => throw new ArgumentException("Error")
                };
                tabControl.Name = tabName; // Для ідентифікації
                tabControl.Dock = DockStyle.Fill;
                _tabsCache[tabName] = tabControl;

                if (tabControl is DocumentTab docTab) InitializeDocumentTabEvents(docTab);

                pnlContent.Controls.Add(tabControl);
            }

            tabControl.Visible = true;
            tabControl.BringToFront();

            UpdateLocalization();
            ApplyTheme();

            ResumeLayout(true);
        }

        // Додайте цей допоміжний метод у Form1.cs або Form1_Logic.cs
        private void InitializeDocumentTabEvents(DocumentTab docTab)
        {
            float currentScale = Win32Api.GetDpiScale();

            // 1. ЗАОКРУГЛЕННЯ ДЛЯ ВСІХ ТРЬОХ КНОПОК
            MakeButtonRounded(docTab.btnBrowseCss, (int)(4 * currentScale));
            MakeButtonRounded(docTab.btnDumpConfig, (int)(4 * currentScale));
            MakeButtonRounded(docTab.btnBrowseCustomYaml, (int)(4 * currentScale));

            // 2. ПРИВ'ЯЗКА КЛІКІВ
            docTab.btnBrowseCss.Click += BtnBrowseCss_Click;
            docTab.btnBrowseCustomYaml.Click += BtnBrowseCustomYaml_Click;
            docTab.btnDumpConfig.Click += BtnDumpConfig_Click;

            // 3. РЕШТА ПОДІЙ
            docTab.langComboBox.SelectedIndexChanged += LangComboBox_SelectedIndexChanged;
            docTab.chkFb2Name.CheckedChanged += ChkFb2Name_CheckedChanged;

            // Синхронізація при активації CSS
            docTab.chkCss.CheckedChanged += (s, e) =>
            {
                if (docTab.chkCss.Checked) SyncCssWithCustomYaml(docTab);
            };
            // Синхронізація імені конфігу при зміні стану чекбокса
            docTab.chkCustomYaml.CheckedChanged += (s, e) =>
            {
                SyncConfigNameWithYaml(docTab);
                SyncCssWithCustomYaml(docTab);
                SyncTocTypeWithCustomYaml(docTab);
                SyncBinarySettingsWithYaml(docTab);
                // Також викликаємо оновлення теми, бо зміна стану чекбокса впливає на візуал
                ApplyTheme();
            };

            // Додаємо обробник для зміни шляху YAML (якщо змінили файл, оновлюємо CSS)
            docTab.txtCustomYamlPath.TextChanged += (s, e) => 
            {
                SyncConfigNameWithYaml(docTab);
                SyncCssWithCustomYaml(docTab); // Можна теж додати про всяк випадок
                SyncTocTypeWithCustomYaml(docTab);
                SyncBinarySettingsWithYaml(docTab);
            };

            if (docTab.cmbOutFields != null)
            {
                for (int i = 0; i < docTab.cmbOutFields.Length; i++)
                {
                    int index = i;
                    docTab.cmbOutFields[i].SelectedIndexChanged += (s, e) => CmbOutFields_SelectedIndexChanged(index);
                }
            }
            // fix_zip
            docTab.chkFixZip.CheckedChanged += (s, e) => {
                if (docTab.rbFixZipYes?.Parent != null)
                    docTab.rbFixZipYes.Parent.Enabled = docTab.chkFixZip.Checked;
                ApplyTheme();
            };
            docTab.chkOpenFromCover.CheckedChanged += (s, e) => {
                if (docTab.rbOpenCoverYes?.Parent != null)
                    docTab.rbOpenCoverYes.Parent.Enabled = docTab.chkOpenFromCover.Checked;
                ApplyTheme();
            };
            docTab.chkTranslit.CheckedChanged += (s, e) => {
                if (docTab.rbTranslitYes?.Parent != null)
                    docTab.rbTranslitYes.Parent.Enabled = docTab.chkTranslit.Checked;
                ApplyTheme();
            };
        }


        /// <summary>
        /// Масштабує вхідне зображення до заданих розмірів з високою якістю рендерингу.
        /// </summary>
        /// <param name="img">Оригінальне зображення (може бути null).</param>
        /// <param name="width">Необхідна ширина нового зображення.</param>
        /// <param name="height">Необхідна висота нового зображення.</param>
        /// <returns>Новий об'єкт Bitmap або null, якщо вхідне зображення відсутнє.</returns>
        private static Bitmap? ResizeImage(Image? img, int width, int height)
        {
            // Перевірка на null за допомогою сучасного патерну 'is null'.
            // Якщо картинку не передали, одразу виходимо, щоб не витрачати ресурси процесора.
            if (img is null) return null;

            // Створюємо порожній бітмап потрібного розміру в пам'яті.
            // Тип визначено як Nullable (Bitmap?), щоб задовольнити сувору перевірку типів .NET 10.
            Bitmap? bmp = new(width, height);

            try
            {
                // Створюємо об'єкт Graphics для малювання на нашому новому порожньому бітмапі.
                // Блок 'using' гарантує автоматичне звільнення системних контекстів малювання (GDI handles).
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Налаштовуємо алгоритми згладжування та інтерполяції для отримання найкращої якості.
                    g.SmoothingMode = SmoothingMode.AntiAlias;                  // Увімкнення згладжування ліній та країв
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic; // Бікубічна інтерполяція для чіткості при зміні розміру
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;            // Оптимальне зміщення пікселів для усунення розмиття

                    // Малюємо оригінальну картинку (img) на нашому новому бітмапі, розтягуючи її від лівого верхнього кута (0,0) до нових меж (width, height)
                    g.DrawImage(img, 0, 0, width, height);
                }

                // Повертаємо готовий, оброблений конкретний об'єкт Bitmap (виправлено зауваження CA1859 щодо продуктивності)
                return bmp;
            }
            catch
            {
                // Захист від витоку пам'яті (Best Practice для графіки): 
                // Якщо під час налаштування Graphics або самого малювання DrawImage станеться будь-який збій (Exception),
                // ми зобов'язані примусово знищити створений бітмап за допомогою .Dispose(), інакше він назавжди «зависне» в некерованій пам'яті Windows.
                bmp?.Dispose();

                // Прокидаємо помилку далі по стеку викликів, щоб програма знала про збій
                throw;
            }
        }

        internal static void MakeButtonRounded(Button btn, int radius)
        {
            btn.FlatStyle = FlatStyle.Flat; // ОБОВ'ЯЗКОВО
            btn.FlatAppearance.BorderSize = 0;

            // Крок 1. Надійний Region (Ваш оригінальний без змін)
            using (GraphicsPath path = new())
            {
                float r = radius;
                path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                path.AddArc(btn.Width - (r * 2), 0, r * 2, r * 2, 270, 90);
                path.AddArc(btn.Width - (r * 2), btn.Height - (r * 2), r * 2, r * 2, 0, 90);
                path.AddArc(0, btn.Height - (r * 2), r * 2, r * 2, 90, 90);
                path.CloseAllFigures();

                btn.Region = new Region(path);
            }

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            // Додаємо змінні для світлої теми з перевіркою Enabled (захист від багу при старті)
            bool isHovered = false;
            btn.MouseEnter += (s, e) => { if (btn.Enabled) { isHovered = true; btn.Invalidate(); } };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };

            // Якщо під час зміни Enabled кнопка була під мишкою, скидаємо стан підсвічування
            btn.EnabledChanged += (s, e) => { if (!btn.Enabled) { isHovered = false; btn.Invalidate(); } };

            // Крок 2. Малювання рамки
            btn.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                bool isDarkTheme = Config.IsDarkTheme;

                if (isDarkTheme)
                {
                    // ДЛЯ ТЕМНОЇ ТЕМИ
                    using GraphicsPath buttonFramePath = new();
                    float r = radius;
                    float startXY = 0.5f;
                    float sizeAdjustment = 1.0f;

                    buttonFramePath.AddArc(startXY, startXY, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, startXY, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(0, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    // Якщо кнопка вимкнена в темній темі, робимо рамку тьмяною
                    // 1. Спочатку визначаємо стандартний колір рамки для активної кнопки
                    Color activeBorderColor = btn.FlatAppearance.BorderColor != Color.Empty && btn.FlatAppearance.BorderColor != Color.Transparent
                        ? btn.FlatAppearance.BorderColor
                        : btn.ForeColor;

                    // 2. Тепер легко і читабельно робимо вибір залежно від стану кнопки
                    Color btnBorderColor = !btn.Enabled
                        ? Color.FromArgb(70, Color.Gray)
                        : activeBorderColor;
                    using Pen pen = new(btnBorderColor, 1.2F);
                    ev.Graphics.DrawPath(pen, buttonFramePath);
                }
                else
                {
                    // ДЛЯ СВІТЛОЇ ТЕМИ
                    using GraphicsPath buttonFramePath = new();
                    float r = radius;
                    float startXY = 0.5f;
                    float sizeAdjustment = 1.0f;

                    buttonFramePath.AddArc(startXY, startXY, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, startXY, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(0, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    Color btnBorderColor;
                    if (!btn.Enabled)
                    {
                        btnBorderColor = Color.LightGray;
                    }
                    else if (isHovered)
                    {
                        btnBorderColor = Color.FromArgb(0, 120, 215); // Підсвічування при наведенні
                    }
                    else
                    {
                        btnBorderColor = btn.FlatAppearance.BorderColor != Color.Empty && btn.FlatAppearance.BorderColor != Color.Transparent
                            ? btn.FlatAppearance.BorderColor
                            : Color.DarkGray;
                    }

                    using Pen pen = new(btnBorderColor, 1.0F);
                    ev.Graphics.DrawPath(pen, buttonFramePath);
                }
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Повідомляє середовище виконання .NET про необхідність чистого закриття
            // усіх фонових потоків, зняття блокувань з файлів та вивантаження додатку.
            Environment.Exit(0);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Примусово наказуємо Windows вивести це вікно на передній план 
            // та засвітити іконку на панелі завдань без перестворення дескрипторів
            _ = Win32Api.SetForegroundWindow(Handle);

            // Передаємо фокус введення всередину програми
            _ = Focus();
        }
        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            Name = "Form1";
            Text = "fb2cng Configurator";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ResumeLayout(false);
        }
    }
}