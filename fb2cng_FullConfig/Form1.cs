using System.Drawing.Drawing2D;
using fb2cng_FullConfig.Templates;

namespace fb2cng_FullConfig
{
    public partial class Form1 : Form
    {
        // Статичні елементи каркасу програми
        private Panel headerPanel = null!;
        private Panel footerPanel = null!;
        private Panel pnlContent = null!; // Центральний контейнер для вкладок

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

        private void SetupMainFramework()
        {
            float currentScale = Win32Api.GetDpiScale();
            int btnRadius = (int)(4 * currentScale);
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
            foreach (Button btn in tabButtons)
            {
                btn.Click += TabButton_Click;
                UiStyles.MakeButtonRounded(btn, (int)(4 * currentScale)); // Заокруглення для вкладок
                headerPanel.Controls.Add(btn);
            }

            // ==================================================
            // КРОК 2: СТВОРЕННЯ ЦЕНТРАЛЬНОГО КОНТЕНТ-КОНТЕЙНЕРА
            // ==================================================
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

            string guiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe");
            bool guiExists = File.Exists(guiPath);

            // Створення кнопок футера
            btnHelp = new Button
            {
                Text = "Help",
                Image = UiStyles.ResizeImage(Properties.Resources.icon_info, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(2 * currentScale), 0, 0, 0)
            };
            btnTheme = new Button
            {
                Text = "Theme",
                Image = UiStyles.ResizeImage(Properties.Resources.day_night, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(10 * currentScale), 0, 0, 0)
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
                    Padding = new Padding((int)(3 * currentScale), 0, 0, 0)
                };
                if (Properties.Resources.icon_GUI != null)
                {
                    btGui.Image = UiStyles.ResizeImage(Properties.Resources.icon_GUI, iconSize, iconSize);
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
            btnTheme.Click += (s, e) => { Config.IsDarkTheme = !Config.IsDarkTheme; ApplyTheme(); Config.SaveSettings(); };
            btnCancel.Click += (s, e) => Close();
            btnHelp.Click += (s, e) => ShowHelp();
            btnOk.Click += (s, e) => SaveYamlConfiguration();

            // 6. РОЗСТАНОВКА (SetBounds) з перевіркою
            int btnWidth = (int)(90 * currentScale);
            int guiBtnWidth = (int)(65 * currentScale);
            int btnHeight = (int)(24 * currentScale) + (int)(4 * currentScale);
            int btnTop = (int)(5 * currentScale);
            int xLeft = (int)(16 * currentScale);
            int btnspacing = (int)(6 * currentScale);

            btnHelp.SetBounds(xLeft, btnTop, btnWidth, btnHeight);
            btnTheme.SetBounds(btnHelp.Right + btnspacing, btnTop, btnWidth, btnHeight);

            // Кнопка GUI позиціонується лише якщо вона є
            if (guiExists && btGui != null)
            {
                btGui.SetBounds(btnTheme.Right + btnspacing, btnTop, guiBtnWidth, btnHeight);
            }

            btnCancel.SetBounds(ClientSize.Width - xLeft - btnWidth, btnTop, btnWidth, btnHeight);
            btnOk.SetBounds(btnCancel.Left - (int)(96 * currentScale), btnTop, btnWidth, btnHeight);

            // 7. ЗАОКРУГЛЕННЯ ТІЛЬКИ ІСНУЮЧИХ КНОПОК
            UiStyles.MakeButtonRounded(btnHelp, btnRadius);
            UiStyles.MakeButtonRounded(btnTheme, btnRadius);
            if (guiExists && btGui != null)
            {
                UiStyles.MakeButtonRounded(btGui, btnRadius);
            }
            UiStyles.MakeButtonRounded(btnOk, btnRadius);
            UiStyles.MakeButtonRounded(btnCancel, btnRadius);

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

        // Обробник події кліку по кнопках Хідера. Визначає, яку саме вкладку викликав користувач.
        private void TabButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button tabButton && tabButton.Tag is string tabName)
            {
                SwitchToTab(tabName);
                ApplyTheme(); // Перезапускаємо тему, щоб кольори нових вкладок оновилися миттєво
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

            // 1. Сховати всі наявні вкладки
            foreach (Control ctrl in pnlContent.Controls)
            {
                ctrl.Visible = false;
            }

            // 2. Отримати або створити цільову вкладку
            if (!_tabsCache.TryGetValue(tabName, out UserControl? tabControl))
            {
                tabControl = tabName switch
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

                pnlContent.Controls.Add(tabControl);
            }

            // 3. СИНХРОНІЗАЦІЯ 
            // Отримуємо посилання на DocumentTab один раз для всіх перевірок
            if (_tabsCache.TryGetValue("document:", out UserControl? baseTab) && baseTab is DocumentTab activeDocTab)
            {
                if (tabName == "metadata:")
                {

                    SyncMetadataWithYaml(activeDocTab);
                }
                else if (tabName == "logging:")
                {
                    SyncLoggingSettingsWithYaml(activeDocTab);
                }
            }

            // 4. Відображення
            tabControl.Visible = true;
            tabControl.BringToFront();
            UpdateLocalization();
            ApplyTheme();
            ResumeLayout(true);
        }

        private void InitializeDocumentTabEvents(DocumentTab docTab)
        {
            float currentScale = Win32Api.GetDpiScale();

            // 1. ЗАОКРУГЛЕННЯ ДЛЯ ВСІХ ТРЬОХ КНОПОК
            UiStyles.MakeButtonRounded(docTab.btnBrowseCss, (int)(4 * currentScale));
            UiStyles.MakeButtonRounded(docTab.btnDumpConfig, (int)(4 * currentScale));
            UiStyles.MakeButtonRounded(docTab.btnBrowseCustomYaml, (int)(4 * currentScale));

            // 2. ПРИВ'ЯЗКА КЛІКІВ
            docTab.btnBrowseCss.Click += BtnBrowseCss_Click;
            docTab.btnBrowseCustomYaml.Click += BtnBrowseCustomYaml_Click;
            docTab.btnDumpConfig.Click += BtnDumpConfig_Click;

            // 3. РЕШТА ПОДІЙ
            docTab.langComboBox.SelectedIndexChanged += LangComboBox_SelectedIndexChanged;
            docTab.chkFb2Name.CheckedChanged += ChkFb2Name_CheckedChanged;
            docTab.chkDefaultName.CheckedChanged += ChkDefaultName_CheckedChanged;

            // Синхронізація при активації CSS
            docTab.chkCss.CheckedChanged += (s, e) =>
            {
                if (docTab.chkCss.Checked)
                {
                    SyncCssWithCustomYaml(docTab);
                }
            };
            // Синхронізація імені конфігу при зміні стану чекбокса
            docTab.chkCustomYaml.CheckedChanged += (s, e) =>
            {
                SyncConfigNameWithYaml(docTab);
                SyncCssWithCustomYaml(docTab);
                SyncTocTypeWithCustomYaml(docTab);
                SyncBinarySettingsWithYaml(docTab);
                SyncLoggingSettingsWithYaml(docTab);
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
                SyncLoggingSettingsWithYaml(docTab);
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
            docTab.chkFixZip.CheckedChanged += (s, e) =>
            {
                if (docTab.rbFixZipYes?.Parent != null)
                {
                    docTab.rbFixZipYes.Parent.Enabled = docTab.chkFixZip.Checked;
                }

                ApplyTheme();
            };
            docTab.chkOpenFromCover.CheckedChanged += (s, e) =>
            {
                if (docTab.rbOpenCoverYes?.Parent != null)
                {
                    docTab.rbOpenCoverYes.Parent.Enabled = docTab.chkOpenFromCover.Checked;
                }

                ApplyTheme();
            };
            docTab.chkTranslit.CheckedChanged += (s, e) =>
            {
                if (docTab.rbTranslitYes?.Parent != null)
                {
                    docTab.rbTranslitYes.Parent.Enabled = docTab.chkTranslit.Checked;
                }

                ApplyTheme();
            };
        }

        private void InitializeMetadataTabEvents(MetadataTab dataTab)
        {
            float currentScale = Win32Api.GetDpiScale();
            UiStyles.MakeButtonRounded(dataTab.btnBrowseCover, (int)(4 * currentScale));
            dataTab.btnBrowseCover.Click += BtnBrowseCover_Click;

            // ВИКЛИК ApplyTheme ДО УСІХ ЧЕКБОКСІВ МЕТАДАНИХ
            CheckBox[] metaChecks = [
                dataTab.chkReaderSize, dataTab.chkNotes,
                dataTab.chkSoftHyphen, dataTab.chkRemoveTransp,
                dataTab.chkJpegQuality, dataTab.chkGenerateCover,
                dataTab.chkResizeCover, dataTab.chkAnnEnable,
                dataTab.chkAnnInToc, dataTab.chkTocPlacement, dataTab.chkDropcaps
            ];

            foreach (CheckBox chk in metaChecks)
            {
                chk.CheckedChanged += (s, e) => ApplyTheme();
            }

            // спільний метод малювання! Оскільки матриця InactiveIconMatrix лежить у UiStyles, передаємо її через клас
            UiStyles.SetupIconButtonDrawing(
                dataTab.btnBrowseCover,
                Properties.Resources.folder,
                dataTab.chkGenerateCover,
                UiStyles.InactiveIconMatrix
            );
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