using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;
using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig
{
    public partial class Form1 : Form
    {
        //=====================================
        // --- 1. Поля та елементи каркасу ---
        private Panel headerPanel = null!;
        private Panel footerPanel = null!;
        // Центральний контейнер для вкладок
        private Panel pnlContent = null!;
        // Кнопки Хідера 
        private Button btnTabDocument = null!;
        private Button btnTabMetadata = null!;
        private Button btnTabLogging = null!;
        // Кнопки Футера
        private Button btnHelp = null!;
        private Button btnTheme = null!;
        private Button btGui = null!;
        public Button btnOk = null!;
        private Button btnCancel = null!;
        // Кеш для збереження вкладок (щоб при перемиканні вкладок дані користувача не стиралися)
        private readonly Dictionary<string, UserControl> _tabsCache = [];
        private string _currentActiveTab = "document:";
        private readonly List<Bitmap> _generatedBitmaps = []; // Список для очищення пам'яті

        //======================================================
        // --- 2. Життєвий цикл (Constructor & Overrides) ---
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

                // Встановлюємо мову ТІЛЬКИ після того, як комбобокс локалізовано!
                if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
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
                _ = MessageBox.Show($"Критичний збій ініціалізації вікна:\n\n{ex.Message}",
                                "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        //======================================================
        // --- 3. Побудова каркасу (SetupMainFramework) ---
        private void SetupMainFramework()
        {
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // Базові налаштування вікна
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // Задаємо фіксовану ідеальну ширину вікна 
            ClientSize = new Size(m.ScaledWidth, ClientSize.Height);

            // СТВОРЕННЯ ХІДЕРА (ВЕРХНЯ ПАНЕЛЬ)
            headerPanel = new Panel();
            headerPanel.SetBounds(0, 0, ClientSize.Width, m.HeaderHeight);
            Controls.Add(headerPanel);

            // Автоматичний розрахунок ширини кнопок з урахуванням нових відступів
            int totalInterButtonSpace = m.BetweenButtons * 2; // Два проміжки між трьома кнопками
            int tabWidthRow1 = (headerPanel.Width - m.HeaderPaddingLeft - m.HeaderPaddingRight - totalInterButtonSpace) / 3;

            // Головна вкладка, метаінформація та логування
            btnTabDocument = new Button { Text = "document:", Tag = "document:" };
            btnTabMetadata = new Button { Text = "metainformation:", Tag = "metadata:" };
            btnTabLogging = new Button { Text = "logging:", Tag = "logging:" };

            // Координати (використовуємо змінні відступів)
            btnTabDocument.SetBounds(m.HeaderPaddingLeft, m.HeaderTopPadding, tabWidthRow1, m.HeaderRowHeight);
            btnTabMetadata.SetBounds(btnTabDocument.Right + m.BetweenButtons, m.HeaderTopPadding, tabWidthRow1, m.HeaderRowHeight);
            btnTabLogging.SetBounds(btnTabMetadata.Right + m.BetweenButtons, m.HeaderTopPadding, tabWidthRow1, m.HeaderRowHeight);

            // Зв'язуємо всі кнопки хідера з одним методом перемикання вкладок
            Button[] tabButtons = [btnTabDocument, btnTabMetadata, btnTabLogging];
            foreach (Button btn in tabButtons)
            {
                btn.Click += TabButton_Click;
                UiStyles.MakeButtonRounded(btn, m.BtnRadius); // Заокруглення для вкладок
                headerPanel.Controls.Add(btn);
            }

            // СТВОРЕННЯ ЦЕНТРАЛЬНОГО КОНТЕНТ-КОНТЕЙНЕРА
            pnlContent = new Panel();
            pnlContent.SetBounds(0, headerPanel.Bottom, ClientSize.Width, m.ContentHeight);
            Controls.Add(pnlContent);

            // СТВОРЕННЯ СТАТИЧНОГО ФУТЕРА
            int footerHeight = m.FieldHeight + m.HeaderPaddingRight;// Висота футера з урахуванням відступів
            footerPanel = new Panel();
            footerPanel.SetBounds(0, pnlContent.Bottom, ClientSize.Width, footerHeight);// Встановлюємо футер чітко під контентом
            Controls.Add(footerPanel);

            // Кнопки футера (Створення та розстановка)
            InitializeFooterButtons(m);

            // Адаптація висоти під екран
            AdjustWindowSize();
        }

        private void AdjustWindowSize()
        {
            // --- АДАПТАЦІЯ ГЕОМЕТРІЇ ПРИ ВЕЛИКОМУ МАСШТАБІ (200-225%) ---
            int finalHeight = footerPanel.Bottom + UiStyles.GetScaled(8);
            int maxAllowedHeight = Screen.FromControl(this).WorkingArea.Height - UiStyles.GetScaled(40);

            if (finalHeight > maxAllowedHeight)
            {
                int heightDeficit = finalHeight - maxAllowedHeight;

                // 1. Стискаємо центральний контейнер контенту під екран користувача
                pnlContent.Height -= heightDeficit;
                // 2. Підтягуємо футер чітко до низу нового контенту
                footerPanel.Top = pnlContent.Bottom;
                // Фіксуємо оновлену висоту програми
                finalHeight = footerPanel.Bottom + UiStyles.GetScaled(8);
            }
            // Призначаємо фінальні безпечні розміри вікна програми
            ClientSize = new Size(ClientSize.Width, finalHeight);

            // Центрування на моніторі
            StartPosition = FormStartPosition.Manual;
            Location = new Point(
                Screen.FromControl(this).WorkingArea.Left + ((Screen.FromControl(this).WorkingArea.Width - Width) / 2),
                Screen.FromControl(this).WorkingArea.Top + ((Screen.FromControl(this).WorkingArea.Height - Height) / 2)
            );
        }

        //=======================================================================================
        // --- 4. Логіка перемикання вкладок. Визначає, яку саме вкладку викликав користувач.---
        private void TabButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button tabButton && tabButton.Tag is string tabName)
            {
                SwitchToTab(tabName);
                ApplyTheme(); // Перезапускаємо тему, щоб кольори нових вкладок оновилися миттєво
                _ = btnOk.Focus();
            }
        }

        // Динамічно замінює вміст центральної панелі на обрану вкладку з використанням кешування.
        private void SwitchToTab(string tabName)
        {
            if (_currentActiveTab == tabName && pnlContent.Controls.ContainsKey(tabName))
            {
                return;
            }

            _currentActiveTab = tabName;
            SuspendLayout();

            try // Додаємо блок відстеження помилок
            {
                // Сховати всі наявні вкладки
                foreach (Control ctrl in pnlContent.Controls) ctrl.Visible = false;

                // 1. ПЕРЕВІРЯЄМО, ЧИ Є ВКЛАДКА В КЕШІ
                bool isFirstLoad = !_tabsCache.ContainsKey(tabName);

                if (isFirstLoad)
                {
                    UserControl tabControl = tabName switch
                    {
                        "document:" => new DocumentTab(),
                        "metadata:" => new MetadataTab(),
                        "logging:" => new LoggingTab(),
                        _ => throw new ArgumentException("Error")
                    };
                    tabControl.Name = tabName;
                    tabControl.Dock = DockStyle.Fill;
                    _tabsCache[tabName] = tabControl;

                    if (tabControl is DocumentTab docTab)
                    {
                        InitializeDocumentTabEvents(docTab);
                    }
                    if (tabControl is MetadataTab dataTab)
                    {
                        InitializeMetadataTabEvents(dataTab);
                    }
                    if (tabControl is LoggingTab logTab)
                    {
                        InitializeLoggingTabEvents(logTab); 
                    }
                    pnlContent.Controls.Add(tabControl);
                }

                // 3. ПОКАЗУЄМО ВКЛАДКУ
                _tabsCache[tabName].Visible = true;
                _tabsCache[tabName].BringToFront();
                UpdateLocalization();

                // 2. ЯКЩО ЦЕ ПЕРШИЙ ЗАПУСК ВКЛАДКИ — СИНХРОНІЗУЄМО ЇЇ З ПОТОЧНИМ YAML
                if (isFirstLoad)
                {
                    if (_tabsCache.TryGetValue("document:", out UserControl? baseTab) && baseTab is DocumentTab docTabReference)
                    {
                        if (tabName == "metadata:") SyncMetadataWithYaml(docTabReference);
                        else if (tabName == "logging:") SyncLoggingSettingsWithYaml(docTabReference);
                    }
                }

                ApplyTheme();
            }

            catch (Exception ex)
            {
                // Логуємо помилку перемикання вкладок
                Config.LogError($"Error while switching to tab: {tabName}", ex);

                // Повідомляємо користувача (опціонально)
                _ = MessageBox.Show($"Failed to load tab: {tabName}.\nCheck {Config.LogErrorFile} for details.",
                        "Interface Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                // Обов'язково відновлюємо малювання інтерфейсу, навіть якщо сталася помилка
                ResumeLayout(true);
            }
        }

        private void InitializeFooterButtons(UiStyles.LayoutMetrics m)
        {
            string guiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe");
            bool guiExists = File.Exists(guiPath);
            btnHelp = new Button
            {
                Text = "Help",
                Image = GetResizedIcon(Properties.Resources.icon_info, m.IconSize, m.IconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(0),
                TabStop = false
            };

            btnTheme = new Button
            {
                Text = "Theme",
                Image = GetResizedIcon(Properties.Resources.day_night, m.IconSize, m.IconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(UiStyles.GetScaled(10), 0, 0, 0),
                TabStop = false
            };
            // 3. Створюємо кнопку GUI ТІЛЬКИ якщо файл існує
            if (guiExists)
            {
                btGui = new Button
                {
                    Text = "GUI",
                    ImageAlign = ContentAlignment.MiddleCenter,
                    TextAlign = ContentAlignment.MiddleCenter,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    Padding = new Padding(UiStyles.GetScaled(3), 0, 0, 0),
                    TabStop = false
                };
                if (Properties.Resources.icon_GUI != null)
                {
                    btGui.Image = GetResizedIcon(Properties.Resources.icon_GUI, m.IconSize, m.IconSize);
                }
                // Прив'язуємо подію відразу тут, де ми впевнені, що кнопка не null
                btGui.Click += BtGui_Click;
            }

            btnOk = new Button { Text = "OK" };
            btnCancel = new Button { Text = "Cancel" };

            // 4. БЕЗПЕЧНЕ ДОДАВАННЯ КОНТРОЛІВ (замість AddRange з масивом)
            footerPanel.Controls.Add(btnHelp);
            footerPanel.Controls.Add(btnTheme);
            if (guiExists && btGui != null)
            {
                footerPanel.Controls.Add(btGui);
            }
            footerPanel.Controls.Add(btnOk);
            footerPanel.Controls.Add(btnCancel);

            // 5. Прив'язка подій для статичних кнопок
            btnTheme.Click += (s, e) =>
            {
                Config.IsDarkTheme = !Config.IsDarkTheme; ApplyTheme(); Config.SaveSettings();
                _ = btnOk.Focus();
            };
            btnCancel.Click += (s, e) => Close();
            btnHelp.Click += (s, e) =>
            {
                ShowHelp();
                _ = btnOk.Focus();
            };
            btnOk.Click += (s, e) => SaveYamlConfiguration();
            AcceptButton = btnOk;                    // Натискання Enter тепер викликає збереження (OK)
            CancelButton = btnCancel;                // Натискання Esc тепер закриває вікно (Cancel)

            // 6. РОЗСТАНОВКА (SetBounds) з перевіркою
            btnHelp.SetBounds(m.XLeft, m.FooterBtnTop, m.FooterBtnWidth, m.FooterBtnHeight);
            btnTheme.SetBounds(btnHelp.Right + m.BetweenButtons, m.FooterBtnTop, m.FooterBtnWidth, m.FooterBtnHeight);
            // Кнопка GUI позиціонується лише якщо вона є
            if (guiExists && btGui != null)
            {
                btGui.SetBounds(btnTheme.Right + m.BetweenButtons, m.FooterBtnTop, m.FooterGuiBtnWidth, m.FooterBtnHeight);
            }
            btnCancel.SetBounds(ClientSize.Width - m.XLeft - m.FooterBtnWidth, m.FooterBtnTop, m.FooterBtnWidth, m.FooterBtnHeight);
            btnOk.SetBounds(btnCancel.Left - UiStyles.GetScaled(96), m.FooterBtnTop, m.FooterBtnWidth, m.FooterBtnHeight);

            // 7. ЗАОКРУГЛЕННЯ ТІЛЬКИ ІСНУЮЧИХ КНОПОК
            UiStyles.MakeButtonRounded(btnHelp, m.BtnRadius);
            UiStyles.MakeButtonRounded(btnTheme, m.BtnRadius);
            if (guiExists && btGui != null)
            {
                UiStyles.MakeButtonRounded(btGui, m.BtnRadius);
            }
            UiStyles.MakeButtonRounded(btnOk, m.BtnRadius);
            UiStyles.MakeButtonRounded(btnCancel, m.BtnRadius);
        }

        //===============================
        // --- 5. Системні методи ---

        private Bitmap? GetResizedIcon(Image source, int w, int h)
        {
            Bitmap? bmp = UiStyles.ResizeImage(source, w, h);
            if (bmp != null) _generatedBitmaps.Add(bmp);
            return bmp;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // 1. Очищуємо всі створені нами бітмапи
            foreach (var bmp in _generatedBitmaps)
            {
                bmp.Dispose();
            }
            _generatedBitmaps.Clear();

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

            // Передаємо фокус на кнопку збереження
            _ = btnOk.Focus();
        }
    }
}