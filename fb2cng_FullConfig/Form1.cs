using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace fb2cng_FullConfig
{
    public partial class Form1 : Form
    {
        // Оголошення основних динамічних контейнерів (без mainLayout)
        private Panel footerPanel = null!;
        private Panel scrollMenuPanel = null!;

        // Елементи інтерфейсу конфігуратора
        private ComboBox langComboBox = null!;
        private Button btnDumpConfig = null!;
        private TextBox txtConfigName = null!;
        private CheckBox chkCss = null!;
        private TextBox txtCssPath = null!;
        private Button btnBrowseCss = null!;

        private CheckBox chkNotes = null!;
        private ComboBox cmbNotesMode = null!;
        private CheckBox chkCover = null!;
        private ComboBox cmbCoverMode = null!;

        private CheckBox chkReaderSize = null!;
        // Розділяємо та ініціалізуємо мітки (Labels)
        private Label lblWidth = null!;
        private Label lblHeight = null!;
        private Label lblDpi = null!;

        // Розділяємо та ініціалізуємо текстові поля (TextBoxes)
        private TextBox txtWidth = null!;
        private TextBox txtHeight = null!;
        private TextBox txtDpi = null!;

        private CheckBox chkOpenFromCover = null!;
        private CheckBox chkFixZip = null!;
        private CheckBox chkFb2Name = null!;
        private CheckBox chkTranslit = null!;

        private readonly Label lblOutNameTitle = null!;
        private GroupBox grpOutName = null!;
        private ComboBox[]? cmbOutFields;
        private CheckBox[]? chkAsFolder;

        // Розділяємо та ініціалізуємо кнопки (Buttons)
        private Button btnHelp = null!;
        private Button btnTheme = null!;
        private Button btGui = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        // Розділяємо та ініціалізуємо останні мітки
        private Label lblLang = null!;
        private Label lblConfigName = null!;

        // ХАК ДЛЯ ПОВНОГО ВИЛУЧЕННЯ РИВКІВ ТА МЕРЕХТІННЯ ПРИ ЗМІНІ ТЕМИ (Рендеринг у буфері ОС)
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);
        protected override CreateParams CreateParams
        {

            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            lblOutNameTitle = new Label();

            try
            {
                SetupInterface();

                // Початковий стан кнопки огляду
                btnBrowseCss.Enabled = Config.IsDarkTheme || chkCss.Checked;

                if (langComboBox != null)
                {
                    langComboBox.SelectedIndex = Config.Settings.CurrentLanguage switch
                    {
                        "Ukrainian" => 1,
                        "Russian" => 2,
                        _ => 0 // значення за замовчуванням (еквівалент else)
                    };
                }
                ApplyTheme();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Критичний збій ініціалізації вікна:\n\n{ex.Message}",
                                "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupInterface()
        {
            // === КРОК 1: РОЗРАХУНОК МАСШТАБУ ТА КОНСТАНТ (ПЕРЕНЕСЕНО НА ПОЧАТОК) ===
            float currentScale = CreateGraphics().DpiX / 96f;  // Масштабування для HiDPI (96 DPI - базовий рівень)

            int blockMargin = (int)(9 * currentScale);         // Простір між блоками параметрів
            int labelToFieldSpace = (int)(3 * currentScale);   // Відступ від тексту до його поля
            int labelHeight = (int)(20 * currentScale);        // Висота написів (Label)
            int fieldHeight = (int)(24 * currentScale);        // Висота полів введення (TextBox, ComboBox)
            int checkBoxHeight = (int)(22 * currentScale);     // Висота чекбоксів (CheckBox)
            int sidePadding = (int)(2 * currentScale);         // Відступ всередину для ідеального вирівнювання країв кнопкою огляду
            int btnRadius = (int)(6 * currentScale);           // Радіус закруглення кнопок

            // Налаштування поведінки вікна (розміри задасть подія Load)
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Базовий шрифт для пропорційного масштабування системую
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            // 1. Ініціалізація чистих контейнерів
            scrollMenuPanel = new Panel { AutoScroll = true };
            Controls.Add(scrollMenuPanel);

            grpOutName = new GroupBox { Text = "" };
            Controls.Add(grpOutName);

            footerPanel = new Panel();
            Controls.Add(footerPanel);

            // 2. Створення елементів верхнього блоку (додаємо у scrollMenuPanel)
            lblLang = new Label { Text = "Language:", AutoSize = true };
            langComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            langComboBox.Items.AddRange(new string[] { "English", "Українська", "Русский" });
            langComboBox.SelectedIndexChanged += LangComboBox_SelectedIndexChanged;
            scrollMenuPanel.Controls.AddRange(new Control[] { lblLang, langComboBox });

            btnDumpConfig = new Button();
            btnDumpConfig.Click += BtnDumpConfig_Click;
            //MakeButtonRounded(btnDumpConfig, btnRadius); // Застосовуємо закруглення кутів
            scrollMenuPanel.Controls.Add(btnDumpConfig);

            lblConfigName = new Label { AutoSize = true };
            txtConfigName = new TextBox { Text = "config.yaml" };
            scrollMenuPanel.Controls.AddRange(new Control[] { lblConfigName, txtConfigName });

            // --- НАЛАШТУВАННЯ CSS ТА КНОПКИ ОГЛЯДУ З ДИНАМІЧНОЮ ІКОНКОЮ ПАПКИ ---
            chkCss = new CheckBox { AutoSize = true };
            txtCssPath = new TextBox { Enabled = false };

            // Порожній текст, малюємо іконку папки вручну через подію Paint
            btnBrowseCss = new Button { Text = string.Empty, FlatStyle = FlatStyle.Flat };
            btnBrowseCss.FlatAppearance.BorderSize = 0;

            bool isOutFolderHovered = false;
            btnBrowseCss.MouseEnter += (s, e) => { isOutFolderHovered = true; btnBrowseCss.Invalidate(); };
            btnBrowseCss.MouseLeave += (s, e) => { isOutFolderHovered = false; btnBrowseCss.Invalidate(); };
            Image outFolderIcon = fb2cng_FullConfig.Properties.Resources.folder;

            btnBrowseCss.Paint += (s, e) =>
            {
                Color baseBgColor = btnBrowseCss.BackColor;
                Color drawBgColor = baseBgColor;

                if (isOutFolderHovered && btnBrowseCss.Enabled)
                {
                    bool isDark = baseBgColor.R < 128;
                    drawBgColor = isDark
                        ? Color.FromArgb(baseBgColor.R + 25, baseBgColor.G + 25, baseBgColor.B + 25)
                        : Color.FromArgb(baseBgColor.R - 20, baseBgColor.G - 20, baseBgColor.B - 20);
                }

                using (Brush backBrush = new SolidBrush(drawBgColor))
                {
                    e.Graphics.FillRectangle(backBrush, 0, 0, btnBrowseCss.Width, btnBrowseCss.Height);
                }

                if (outFolderIcon != null)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Асиметричні відступи: зменшили по вертикалі (0.16), щоб папка не була затиснутою зверху/знизу
                    int paddingX = (int)(btnBrowseCss.Width * 0.24);
                    int paddingY = (int)(btnBrowseCss.Height * 0.12);
                    Rectangle destRect = new Rectangle(paddingX, paddingY, btnBrowseCss.Width - (paddingX * 2), btnBrowseCss.Height - (paddingY * 2));

                    // ВИПРАВЛЕНО: тепер активність залежить ТІЛЬКИ від безпосередньо стану чекбокса
                    bool isBtnActive = chkCss.Checked;

                    if (!isBtnActive)
                    {
                        float[][] ptsArray = {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, 0.30f, 0}, // 30% непрозорості для вимкненого стану (ніжніше виглядає)
                    new float[] {0, 0, 0, 0, 1}
                };
                        using ImageAttributes imageAttributes = new ImageAttributes();
                        imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray));
                        e.Graphics.DrawImage(outFolderIcon, destRect, 0, 0, outFolderIcon.Width, outFolderIcon.Height, GraphicsUnit.Pixel, imageAttributes);
                        return;
                    }

                    e.Graphics.DrawImage(outFolderIcon, destRect);
                }
            };

            chkCss.CheckedChanged += (s, e) =>
            {
                txtCssPath.Enabled = chkCss.Checked;

                // Кнопка активна тільки тоді, коли стоїть прапорець (для обох тем однаково)
                btnBrowseCss.Enabled = chkCss.Checked;

                // Примусово очищаємо кеш малювання та перемальовуємо іконку
                btnBrowseCss.Invalidate();
                ApplyTheme();
            };
            btnBrowseCss.Click += BtnBrowseCss_Click;
            scrollMenuPanel.Controls.AddRange(new Control[] { chkCss, txtCssPath, btnBrowseCss });

            //поля: виноски, навігаціна ієрархія, екран читалки, чек бокси: fix_zip, відкривати з обкладинки, оригінальна назва FB2, транслітерація

            chkNotes = new CheckBox { AutoSize = true };
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(new string[] { "default", "float", "floatRenumbered" });
            cmbNotesMode.SelectedIndex = 0;
            chkNotes.CheckedChanged += (s, e) => { cmbNotesMode.Enabled = chkNotes.Checked; ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange(new Control[] { chkNotes, cmbNotesMode });

            chkCover = new CheckBox { AutoSize = true };
            cmbCoverMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbCoverMode.Items.AddRange(new string[] { "normal", "old_kindle", "flat" });
            cmbCoverMode.SelectedIndex = 0;
            chkCover.CheckedChanged += (s, e) => { cmbCoverMode.Enabled = chkCover.Checked; ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange(new Control[] { chkCover, cmbCoverMode });

            // Розмір екрана читалки
            chkReaderSize = new CheckBox { AutoSize = true };
            lblWidth = new Label { Text = "W:", AutoSize = true, Enabled = false };
            txtWidth = new TextBox { Text = "1264", Enabled = false };
            lblHeight = new Label { Text = "H:", AutoSize = true, Enabled = false };
            txtHeight = new TextBox { Text = "1680", Enabled = false };
            lblDpi = new Label { Text = "DPI:", AutoSize = true, Enabled = false };
            txtDpi = new TextBox { Text = "300", Enabled = false };

            chkReaderSize.CheckedChanged += (s, e) =>
            {
                bool en = chkReaderSize.Checked;
                lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = en;
                ApplyTheme();
            };
            scrollMenuPanel.Controls.AddRange(new Control[] { chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi });

            chkFixZip = new CheckBox { AutoSize = true };
            chkOpenFromCover = new CheckBox { AutoSize = true };
            scrollMenuPanel.Controls.AddRange(new Control[] { chkFixZip, chkOpenFromCover });

            chkFb2Name = new CheckBox { AutoSize = true };
            chkFb2Name.CheckedChanged += ChkFb2Name_CheckedChanged;
            scrollMenuPanel.Controls.Add(chkFb2Name);

            chkTranslit = new CheckBox { AutoSize = true };
            scrollMenuPanel.Controls.Add(chkTranslit);

            // 3. Ініціалізація конструктора структури назви (8 елементів) всередині grpOutName
            cmbOutFields = new ComboBox[8];
            chkAsFolder = new CheckBox[8];

            for (int i = 0; i < 8; i++)
            {
                int index = i;
                cmbOutFields[index] = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                chkAsFolder[index] = new CheckBox
                {
                    Text = "Fold",
                    Enabled = false,
                    Tag = "FolderCheckBox"
                };

                cmbOutFields[index].Items.AddRange(new string[] { "", "", "", "", "", "", "", "", "" });
                cmbOutFields[index].SelectedIndex = 0;
                if (index > 0)
                {
                    cmbOutFields[index].Enabled = false;
                }

                cmbOutFields[index].SelectedIndexChanged += (s, e) => CmbOutFields_SelectedIndexChanged(index);
                grpOutName.Controls.AddRange(new Control[] { cmbOutFields[index], chkAsFolder[index] });
            }

            // === КРОК 5: СТВОРЕННЯ КНОПОК ФУТЕРА З ІКОНКАМИ ===
            int iconSize = (int)(17 * currentScale);

            btnHelp = new Button
            {
                Text = "Help",
                Image = ResizeImage(Properties.Resources.icon_info, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(2 * currentScale), 0, 0, 0)
            };

            btnTheme = new Button
            {
                Text = "Theme",
                Image = ResizeImage(Properties.Resources.day_night, iconSize, iconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(10 * currentScale), 0, 0, 0)
            };

            // НОВА КНОПКА ДЛЯ ЗАПУСКУ fb2cng_GUI
            btGui = new Button
            {
                Text = "GUI",
                ImageAlign = ContentAlignment.MiddleCenter,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding((int)(3 * currentScale), 0, 0, 0)// 10 пікселів відступу зліва для тексту "GUI" забагато
            };

            // Перевіряємо наявність іконки у ресурсах, якщо немає — кнопка залишиться просто з текстом "GUI"
            if (Properties.Resources.icon_GUI != null)
            {
                btGui.Image = ResizeImage(Properties.Resources.icon_GUI, iconSize, iconSize);
            }

            btnOk = new Button { Text = "OK" };
            btnCancel = new Button { Text = "Cancel" };

            // ДОДАЄМО btGui В МАСИВ ЕЛЕМЕНТІВ ФУТЕРА
            footerPanel.Controls.AddRange(new Control[] { btnHelp, btnTheme, btGui, btnOk, btnCancel });

            // НАЛАШТУВАННЯ ГАРЯЧИХ КЛАВІШ 
            AcceptButton = btnOk;     // Enter тепер натискає OK
            CancelButton = btnCancel; // Esc тепер натискає Cancel

            btnTheme.Click += (s, e) => { Config.IsDarkTheme = !Config.IsDarkTheme; ApplyTheme(); Config.SaveSettings(); };
            btnCancel.Click += (s, e) => Close();
            btnHelp.Click += (s, e) => ShowHelp();
            btnOk.Click += (s, e) => SaveYamlConfiguration();

            // ЗВ'ЯЗУЄМО З МЕТОДОМ ЗАПУСКУ З Form1_Logic.cs
            btGui.Click += BtGui_Click;

            // === КРОК 6: ГЕОМЕТРІЯ ТА РОЗРАХУНОК КООРДИНАТ ЕЛЕМЕНТІВ ===
            // Ідеальна базова ширина програми для розміщення всіх елементів
            int calculatedWidth = (int)(520 * currentScale);    // Встановлюємо ширину форми з урахуванням масштабу DPI
            ClientSize = new Size(calculatedWidth, ClientSize.Height);

            // Внутрішні відступи для контенту
            int xLeft = (int)(16 * currentScale);               // Відступ від лівого краю форми до контенту
            int xRightField = ClientSize.Width - xLeft;
            int fieldWidth = xRightField - xLeft;

            // Скрол-меню (займає верхню частину вікна, прибираємо горизонтальну смугу скролу)
            int scrollPanelHeight = (int)(345 * currentScale);       // Висота скрол-панелі
            scrollMenuPanel.SetBounds(0, 0, ClientSize.Width, scrollPanelHeight);

            // Внутрішня ширина скрол-панелі для елементів (трохи менша через вертикальний повзунок)
            int scrollFieldWidth = fieldWidth - (int)(3 * currentScale); // Відступ для вертикального скролу
            int scrollRightField = xRightField - (int)(3 * currentScale);

            // === КРОК 3: РОЗСТАНОВКА ЕЛЕМЕНТІВ ВСЕРЕДИНІ SCROLL PANEL ---
            int nextY = (int)(12 * currentScale);              // Початковий відступ зверху
            int textLabelWidth = (int)(240 * currentScale);    // Фіксована ширина під написи ліворуч
            int valueFieldWidth = scrollFieldWidth - textLabelWidth - (int)(5 * currentScale); // Ширина поля праворуч

            // 1. Мова (Текст ліворуч, комбобокс праворуч)
            lblLang.SetBounds(xLeft, nextY + (int)(2 * currentScale), textLabelWidth, labelHeight);
            langComboBox.ItemHeight = fieldHeight - 6;
            langComboBox.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 2. Кнопка дампу (на всю ширину)
            nextY = langComboBox.Bottom + blockMargin;
            btnDumpConfig.SetBounds(xLeft + sidePadding, nextY, scrollFieldWidth - (sidePadding * 3), fieldHeight + (int)(4 * currentScale));

            // 3. Назва файлу (Текст ліворуч, інпут праворуч)
            nextY = btnDumpConfig.Bottom + blockMargin;
            lblConfigName.SetBounds(xLeft, nextY + (int)(2 * currentScale), textLabelWidth, labelHeight);
            txtConfigName.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 4. CSS налаштування (Чекбокс + Інпут + Кнопка Огляд)
            nextY = txtConfigName.Bottom + blockMargin;
            chkCss.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);

            // Зменшуємо ширину кнопки огляду: було 85, робимо компактніші 55 (приблизно на розмір двох літер менше)
            int browseBtnWidth = (int)(55 * currentScale);
            int cssTxtWidth = valueFieldWidth - browseBtnWidth - (int)(5 * currentScale) - sidePadding;
            txtCssPath.Multiline = true;
            txtCssPath.SetBounds(xLeft + textLabelWidth, nextY, cssTxtWidth, fieldHeight);
            btnBrowseCss.SetBounds(scrollRightField - browseBtnWidth - sidePadding, nextY, browseBtnWidth, fieldHeight);

            // 5. Спосіб обробки виносок
            nextY = txtCssPath.Bottom + blockMargin;
            chkNotes.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbNotesMode.ItemHeight = fieldHeight - 6;
            cmbNotesMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 6. Навігаційна ієрархія
            nextY = cmbNotesMode.Bottom + blockMargin;
            chkCover.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbCoverMode.ItemHeight = fieldHeight - 6;
            cmbCoverMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 7.Розміри екрана рідера(Фіксована компактна ширина полів введення чисел)
            nextY = cmbCoverMode.Bottom + blockMargin;
            chkReaderSize.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight); // Встановлюємо чекбокс для розмірів екрана

            int sizeInputX = xLeft + textLabelWidth;
            int exactBoxWidth = (int)(45 * currentScale);        // Фіксована ширина інпуту під 4-5 цифр
            int labelWidthSpace = (int)(20 * currentScale);      // Ширина під тексти W:, H:, DPI:

            txtWidth.Margin = txtHeight.Margin = txtDpi.Margin = new Padding(0);

            // Розставляємо W
            lblWidth.SetBounds(sizeInputX, nextY + (int)(2 * currentScale), labelWidthSpace, labelHeight);
            txtWidth.Multiline = true;
            txtWidth.SetBounds(lblWidth.Right, nextY, exactBoxWidth, fieldHeight);

            // Розставляємо H
            lblHeight.SetBounds(txtWidth.Right + (int)(12 * currentScale), nextY + (int)(2 * currentScale), labelWidthSpace, labelHeight);
            txtHeight.Multiline = true;
            txtHeight.SetBounds(lblHeight.Right, nextY, exactBoxWidth, fieldHeight);

            // Розставляємо DPI (Тепер воно стоятиме компактно і нікуди не залізе)
            lblDpi.SetBounds(txtHeight.Right + (int)(12 * currentScale), nextY + (int)(2 * currentScale), (int)(32 * currentScale), labelHeight);
            txtDpi.Multiline = true;
            txtDpi.SetBounds(lblDpi.Right, nextY, exactBoxWidth, fieldHeight);

            // 8. чекбокси: (Вилучити дескриптор / Фікс ZIP)
            nextY = chkReaderSize.Bottom + blockMargin; // Також повертаємо повноцінний blockMargin
            chkFixZip.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            // (відкривати зобкладинки)
            nextY = chkFixZip.Bottom + blockMargin;
            chkOpenFromCover.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            // (Оригінальне ім'я FB2)
            nextY = chkOpenFromCover.Bottom + blockMargin;
            chkFb2Name.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            // (Транслітерація)
            nextY = chkFb2Name.Bottom + blockMargin;
            chkTranslit.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);


            // --- БЛОК 4: КОНСТРУКТОР СТРУКТУРИ НАЗВИ (ФІКСОВАНИЙ НИЖЧЕ СКРОЛУ) ---
            int rowHeight = fieldHeight + (int)(3 * currentScale);         // Відстань між полями
            int grpOutHeight = (rowHeight * 8) + (int)(25 * currentScale); // Висота групи з 8 рядків + заголовок групи

            grpOutName.SetBounds(xLeft, scrollMenuPanel.Bottom + blockMargin, fieldWidth, grpOutHeight);

            int comboWidth = (int)(grpOutName.Width * 0.76f);                                // 76% ширини групи для комбобоксу
            int checkFoldWidth = grpOutName.Width - comboWidth - (int)(15 * currentScale);   // Залишок ширини для чекбоксу + відступ
            int itemY = (int)(20 * currentScale);                                            // Початковий відступ від верхньої межі групи до першого рядка

            for (int i = 0; i < 8; i++)
            {
                cmbOutFields[i].ItemHeight = fieldHeight - 6;
                cmbOutFields[i].SetBounds((int)(10 * currentScale), itemY, comboWidth, fieldHeight);
                chkAsFolder[i].SetBounds(cmbOutFields[i].Right + (int)(5 * currentScale), itemY + (int)(1 * currentScale), checkFoldWidth, checkBoxHeight);
                itemY += rowHeight;
            }

            // --- БЛОК 5: НИЖНЯ ПАНЕЛЬ (ФУТЕР) ---
            int footerHeight = fieldHeight + (int)(14 * currentScale);                             // Висота футера з запасом для кнопок
            footerPanel.SetBounds(0, grpOutName.Bottom + blockMargin, ClientSize.Width, footerHeight);

            // Розставляємо кнопки чітко горизонтально за координатами
            int btnWidth = (int)(90 * currentScale);                 // Фіксована ширина для стандартних кнопок
            int guiBtnWidth = (int)(60 * currentScale);              // Компактна ширина спеціально для кнопки "GUI"
            int btnHeight = fieldHeight + (int)(4 * currentScale);   // Висота кнопок
            int btnTop = (int)(5 * currentScale);                    // Відступ від верхньої межі футера

            // Ліві кнопки
            btnHelp.SetBounds(xLeft, btnTop, btnWidth, btnHeight);
            btnTheme.SetBounds(btnHelp.Right + (int)(6 * currentScale), btnTop, btnWidth, btnHeight);
            btGui.SetBounds(btnTheme.Right + (int)(6 * currentScale), btnTop, guiBtnWidth, btnHeight); // Використовуємо компактну ширину

            // Праві кнопки (рахуємо від правого краю форми назад)
            btnCancel.SetBounds(ClientSize.Width - xLeft - btnWidth, btnTop, btnWidth, btnHeight);
            btnOk.SetBounds(btnCancel.Left - (int)(96 * currentScale), btnTop, btnWidth, btnHeight); // Відступ між кнопками 6 пікселів + ширина кнопки Відміна 90

            // === ЗАКРУГЛЕННЯ КНОПОК ПІСЛЯ ТОГО, ЯК ЗАДАНІ ВСІ РОЗМІРИ SETBOUNDS ===
            MakeButtonRounded(btnDumpConfig, btnRadius);
            MakeButtonRounded(btnBrowseCss, (int)(5 * currentScale));
            MakeButtonRounded(btnHelp, btnRadius);
            MakeButtonRounded(btnTheme, btnRadius);
            MakeButtonRounded(btGui, btnRadius); // <--- ДОДАНО: Закруглюємо нову кнопку оболонки
            MakeButtonRounded(btnOk, btnRadius);
            MakeButtonRounded(btnCancel, btnRadius);
            // Фінальний розрахунок висоти програми
            // СТАЛО (Авто-адаптація під висоту монітора + вихід на перший план):
            int finalHeight = footerPanel.Bottom + (int)(8 * currentScale);  // Встановлюємо фінальну висоту форми з урахуванням масштабу DPI

            // Отримуємо реальну висоту екрана користувача БЕЗ врахування панелі задач
            int maxAllowedHeight = Screen.PrimaryScreen!.WorkingArea.Height - (int)(40 * currentScale);  // Віднімаємо 40 пікселів для безпечного відступу від верхньої та нижньої межі екрана

            if (finalHeight > maxAllowedHeight)
            {
                // Якщо форма занадто велика для монітора (наприклад, при 200% на Full HD екрані)
                int heightDeficit = finalHeight - maxAllowedHeight;

                // Зменшуємо висоту верхньої скрол-панелі на величину дефіциту
                int newScrollHeight = scrollMenuPanel.Height - heightDeficit;

                // Ставимо безпечний мінімум для висоти скролу, щоб інтерфейс не стиснувся в нуль
                if (newScrollHeight < (int)(150 * currentScale))
                {
                    newScrollHeight = (int)(150 * currentScale);
                }

                scrollMenuPanel.Height = newScrollHeight;

                // Перераховуємо позиції нижніх блоків, які прив'язані до bottom скрол-панелі
                grpOutName.SetBounds(xLeft, scrollMenuPanel.Bottom + blockMargin, fieldWidth, grpOutHeight);
                footerPanel.SetBounds(0, grpOutName.Bottom + blockMargin, ClientSize.Width, fieldHeight + (int)(14 * currentScale)); // Встановлюємо футер нижче групи з назвою

                // Фіксуємо нову, зменшену висоту форми
                finalHeight = footerPanel.Bottom + (int)(8 * currentScale);
            }

            // Призначаємо фінальні розміри
            ClientSize = new Size(calculatedWidth, finalHeight);

            // Ідеальне центрування вікна на екрані
            StartPosition = FormStartPosition.Manual;
            Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Left + ((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2),
                Screen.PrimaryScreen.WorkingArea.Top + ((Screen.PrimaryScreen.WorkingArea.Height - Height) / 2)
            );
            // Примусово викликаємо перемальовування папки, щоб активувати прозорість при старті
            btnBrowseCss.Invalidate();
        }

        private static Image? ResizeImage(Image img, int width, int height)
        {
            if (img == null)
            {
                return null;
            }

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }

        private void MakeButtonRounded(Button btn, int radius)
        {
            // Крок 1. Надійний Region (Ваш оригінальний без змін)
            using (GraphicsPath path = new GraphicsPath())
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
            btn.MouseEnter += (s, e) => { if (!Config.IsDarkTheme && btn.Enabled) { isHovered = true; btn.Invalidate(); } };
            btn.MouseLeave += (s, e) => { if (!Config.IsDarkTheme) { isHovered = false; btn.Invalidate(); } };

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
                    using GraphicsPath buttonFramePath = new GraphicsPath();
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
                    using Pen pen = new Pen(btnBorderColor, 1.2F);
                    ev.Graphics.DrawPath(pen, buttonFramePath);
                }
                else
                {
                    // ДЛЯ СВІТЛОЇ ТЕМИ
                    using GraphicsPath buttonFramePath = new GraphicsPath();
                    float r = radius;
                    float startXY = 0.5f;
                    float sizeAdjustment = 1.0f;

                    buttonFramePath.AddArc(startXY, startXY, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, startXY, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(startXY, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    // Подвійна перевірка: підсвічуємо лише якщо миша НАВЕДЕНА і кнопка АКТИВНА
                    if (isHovered && btn.Enabled)
                    {
                        using (Pen glowPen = new Pen(Color.FromArgb(60, 0, 120, 215), 2.2F))
                        {
                            ev.Graphics.DrawPath(glowPen, buttonFramePath);
                        }

                        using Pen mainPen = new Pen(Color.FromArgb(0, 120, 215), 1.2F);
                        ev.Graphics.DrawPath(mainPen, buttonFramePath);
                    }
                    else
                    {
                        Color btnBorderColor;

                        // 1. Визначаємо активний колір у нову тимчасову змінну activeBorderColor
                        Color activeBorderColor = btn.FlatAppearance.BorderColor != Color.Empty && btn.FlatAppearance.BorderColor != Color.Transparent
                            ? btn.FlatAppearance.BorderColor
                            : Color.FromArgb(120, Color.Gray);

                        // 2. Присвоюємо значення ОРИГІНАЛЬНІЙ змінній btnBorderColor (БЕЗ слова Color на початку!)
                        btnBorderColor = !btn.Enabled
                            ? Color.FromArgb(180, Color.LightGray)
                            : activeBorderColor;

                        // 3. Відмальовуємо
                        using Pen pen = new Pen(btnBorderColor, 1.0F);
                        ev.Graphics.DrawPath(pen, buttonFramePath);
                    }
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
            _ = SetForegroundWindow(Handle);

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