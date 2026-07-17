using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace fb2cng_FullConfig.Templates
{

    public partial class DocumentTab : UserControl
    {
        // Елементи інтерфейсу цієї вкладки (перенесені з Form1)
        public Panel scrollMenuPanel = null!;
        public ComboBox langComboBox = null!;
        public Button btnDumpConfig = null!;
        public TextBox txtConfigName = null!;
        public CheckBox chkCss = null!;
        public TextBox txtCssPath = null!;
        public Button btnBrowseCss = null!;

        public CheckBox chkNotes = null!;
        public ComboBox cmbNotesMode = null!;
        public CheckBox chkCover = null!;
        public ComboBox cmbCoverMode = null!;

        public CheckBox chkReaderSize = null!;
        public Label lblWidth = null!;
        public Label lblHeight = null!;
        public Label lblDpi = null!;
        public TextBox txtWidth = null!;
        public TextBox txtHeight = null!;
        public TextBox txtDpi = null!;

        public CheckBox chkOpenFromCover = null!;
        public CheckBox chkFixZip = null!;
        public CheckBox chkFb2Name = null!;
        public CheckBox chkTranslit = null!;

        public GroupBox grpOutName = null!;
        public ComboBox[]? cmbOutFields;
        public CheckBox[]? chkAsFolder;

        public Label lblLang = null!;
        public Label lblConfigName = null!;
        public Label lblOutNameTitle = null!;

        // Статична матриця для вимкненої іконки папки
        private static readonly float[][] InactiveIconMatrix = [
        [1, 0, 0, 0, 0],
        [0, 1, 0, 0, 0],
        [0, 0, 1, 0, 0],
        [0, 0, 0, 0.30f, 0],
        [0, 0, 0, 0, 1]
        ];

        public DocumentTab()
        {
            DoubleBuffered = true;

            // ЗАХИСТ ВІД РОЗ'ЇЗДУ: Вимикаємо автоматичне системне масштабування для UserControl, 
            // оскільки ми вже розраховуємо всі SetBounds вручну через currentScale!
            AutoScaleMode = AutoScaleMode.None;

            SetupInterface();
        }

        private void SetupInterface()
        {
            float currentScale;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                currentScale = g.DpiX / 96f;
            }

            int blockMargin = (int)(9 * currentScale);
            int labelHeight = (int)(20 * currentScale);
            int fieldHeight = (int)(24 * currentScale);
            int checkBoxHeight = (int)(22 * currentScale);
            int sidePadding = (int)(2 * currentScale);

            // 1. Ініціалізація чистих контейнерів
            scrollMenuPanel = new Panel { AutoScroll = true };
            scrollMenuPanel.HorizontalScroll.Enabled = false; // ЗАБОРОНЯЄМО ГОРИЗОНТАЛЬНИЙ СКРОЛ
            scrollMenuPanel.HorizontalScroll.Visible = false;
            Controls.Add(scrollMenuPanel);

            grpOutName = new GroupBox { Text = "" };
            scrollMenuPanel.Controls.Add(grpOutName);

            // 2. Створення елементів верхнього блоку (у scrollMenuPanel)
            lblLang = new Label { Text = "Language:", AutoSize = true };
            langComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            langComboBox.Items.AddRange(["English", "Українська", "Русский"]);
            // Зверніть увагу: оскільки метод кліку мови лежить у Form1_Logic, подія прив'яжеться звідти, або через метод форми
            scrollMenuPanel.Controls.AddRange([lblLang, langComboBox]);

            btnDumpConfig = new Button();
            scrollMenuPanel.Controls.Add(btnDumpConfig);

            lblConfigName = new Label { AutoSize = true };
            txtConfigName = new TextBox { Text = "config.yaml" };
            scrollMenuPanel.Controls.AddRange([lblConfigName, txtConfigName]);

            // Налаштування CSS та кнопки з малюванням іконки
            chkCss = new CheckBox { AutoSize = true };
            txtCssPath = new TextBox { Enabled = false };
            btnBrowseCss = new Button { Text = string.Empty, FlatStyle = FlatStyle.Flat };
            btnBrowseCss.FlatAppearance.BorderSize = 0;

            bool isOutFolderHovered = false;
            btnBrowseCss.MouseEnter += (s, e) => { isOutFolderHovered = true; btnBrowseCss.Invalidate(); };
            btnBrowseCss.MouseLeave += (s, e) => { isOutFolderHovered = false; btnBrowseCss.Invalidate(); };
            Image outFolderIcon = Properties.Resources.folder;

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

                    int paddingX = (int)(btnBrowseCss.Width * 0.24);
                    int paddingY = (int)(btnBrowseCss.Height * 0.12);
                    Rectangle destRect = new(paddingX, paddingY, btnBrowseCss.Width - (paddingX * 2), btnBrowseCss.Height - (paddingY * 2));

                    if (!chkCss.Checked)
                    {
                        using ImageAttributes imageAttributes = new();
                        imageAttributes.SetColorMatrix(new ColorMatrix(InactiveIconMatrix));
                        e.Graphics.DrawImage(outFolderIcon, destRect, 0, 0, outFolderIcon.Width, outFolderIcon.Height, GraphicsUnit.Pixel, imageAttributes);
                        return;
                    }
                    e.Graphics.DrawImage(outFolderIcon, destRect);
                }
            };

            chkCss.CheckedChanged += (s, e) =>
            {
                txtCssPath.Enabled = chkCss.Checked;
                btnBrowseCss.Enabled = chkCss.Checked;
                btnBrowseCss.Invalidate();
            };
            scrollMenuPanel.Controls.AddRange([chkCss, txtCssPath, btnBrowseCss]);

            // Виноски та обкладинка
            chkNotes = new CheckBox { AutoSize = true };
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
            cmbNotesMode.SelectedIndex = 0;
            chkNotes.CheckedChanged += (s, e) => { cmbNotesMode.Enabled = chkNotes.Checked; (ParentForm as Form1)?.ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange([chkNotes, cmbNotesMode]);

            chkCover = new CheckBox { AutoSize = true };
            cmbCoverMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbCoverMode.Items.AddRange(["normal", "old_kindle", "flat"]);
            cmbCoverMode.SelectedIndex = 0;
            chkCover.CheckedChanged += (s, e) => { cmbCoverMode.Enabled = chkCover.Checked; (ParentForm as Form1)?.ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange([chkCover, cmbCoverMode]);

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
                (ParentForm as Form1)?.ApplyTheme();
            };
            scrollMenuPanel.Controls.AddRange([chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi]);

            chkFixZip = new CheckBox { AutoSize = true };
            chkOpenFromCover = new CheckBox { AutoSize = true };
            scrollMenuPanel.Controls.AddRange([chkFixZip, chkOpenFromCover]);

            chkFb2Name = new CheckBox { AutoSize = true };
            scrollMenuPanel.Controls.Add(chkFb2Name);

            chkTranslit = new CheckBox { AutoSize = true };
            scrollMenuPanel.Controls.Add(chkTranslit);
            lblOutNameTitle = new Label();

            // Конструктор структури назви (8 елементів)
            cmbOutFields = new ComboBox[8];
            chkAsFolder = new CheckBox[8];

            for (int i = 0; i < 8; i++)
            {
                int index = i;
                cmbOutFields[index] = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                chkAsFolder[index] = new CheckBox { Text = "Fold", Enabled = false, Tag = "FolderCheckBox" };
                cmbOutFields[index].Items.AddRange(["", "", "", "", "", "", "", "", ""]);
                cmbOutFields[index].SelectedIndex = 0;
                if (index > 0) cmbOutFields[index].Enabled = false;

                grpOutName.Controls.AddRange([cmbOutFields[index], chkAsFolder[index]]);
            }

            // ========================================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ ВСЕРЕДИНІ USERCONTROL
            // ========================================================
            int xLeft = (int)(16 * currentScale);
            int fieldWidth = (int)(520 * currentScale) - (xLeft * 2) - (int)(6 * currentScale);

            // Задаємо базову висоту під центральний контент-контейнер Form1
            Size = new Size((int)(520 * currentScale), (int)(565 * currentScale));

            int scrollPanelHeight = (int)(545 * currentScale);
            scrollMenuPanel.Dock = DockStyle.Fill;

            int scrollFieldWidth = fieldWidth - (int)(3 * currentScale);
            int scrollRightField = fieldWidth + xLeft - (int)(3 * currentScale);

            int nextY = (int)(12 * currentScale);
            int textLabelWidth = (int)(240 * currentScale);
            int valueFieldWidth = scrollFieldWidth - textLabelWidth - (int)(5 * currentScale);

            // Позиціонування елементів
            lblLang.SetBounds(xLeft, nextY + (int)(2 * currentScale), textLabelWidth, labelHeight);
            langComboBox.ItemHeight = fieldHeight - 6;
            langComboBox.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = langComboBox.Bottom + blockMargin;
            nextY = langComboBox.Bottom + blockMargin;
            btnDumpConfig.SetBounds(xLeft + sidePadding, nextY, scrollFieldWidth - (sidePadding * 3), fieldHeight + (int)(4 * currentScale));

            nextY = btnDumpConfig.Bottom + blockMargin;
            lblConfigName.SetBounds(xLeft, nextY + (int)(2 * currentScale), textLabelWidth, labelHeight);
            txtConfigName.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = txtConfigName.Bottom + blockMargin;
            chkCss.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);

            int browseBtnWidth = (int)(55 * currentScale);
            int cssTxtWidth = valueFieldWidth - browseBtnWidth - (int)(5 * currentScale) - sidePadding;
            txtCssPath.Multiline = true;
            txtCssPath.SetBounds(xLeft + textLabelWidth, nextY, cssTxtWidth, fieldHeight);
            btnBrowseCss.SetBounds(scrollRightField - browseBtnWidth - sidePadding, nextY, browseBtnWidth, fieldHeight);

            nextY = txtCssPath.Bottom + blockMargin;
            chkNotes.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbNotesMode.ItemHeight = fieldHeight - 6;
            cmbNotesMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = cmbNotesMode.Bottom + blockMargin;
            chkCover.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbCoverMode.ItemHeight = fieldHeight - 6;
            cmbCoverMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = cmbCoverMode.Bottom + blockMargin; // Розмір екрана читалки
            chkReaderSize.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);

            // --- НАЛАШТУВАННЯ ВІДСТАНІ ---
            int labelWidthSpace = (int)(22 * currentScale); // Збільшена ширина мітки 
            int exactBoxWidth = (int)(44 * currentScale);
            int betweenGroupsSpacing = (int)(10 * currentScale);

            // КРИТИЧНО ВАЖЛИВО: Вимикаємо авторозмір та тиснемо текст ліворуч
            lblWidth.AutoSize = lblHeight.AutoSize = lblDpi.AutoSize = false;
            lblWidth.TextAlign = lblHeight.TextAlign = lblDpi.TextAlign = ContentAlignment.MiddleLeft;
            // -----------------------------

            int sizeInputX = xLeft + textLabelWidth;
            txtWidth.Margin = txtHeight.Margin = txtDpi.Margin = new Padding(0);

            // 1. Блок Width
            int wLabelWidth = labelWidthSpace + (int)(4 * currentScale);
            lblWidth.SetBounds(sizeInputX, nextY + (int)(2 * currentScale), wLabelWidth, labelHeight);
            txtWidth.Multiline = true;
            txtWidth.SetBounds(lblWidth.Right, nextY, exactBoxWidth, fieldHeight);

            // 2. Блок Height
            lblHeight.SetBounds(txtWidth.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), labelWidthSpace, labelHeight);
            txtHeight.Multiline = true;
            txtHeight.SetBounds(lblHeight.Right, nextY, exactBoxWidth, fieldHeight);

            // 3. Блок DPI (для DPI робимо ширше, бо "DPI" довший за "W" чи "H")
            int dpiLabelWidth = labelWidthSpace + (int)(12 * currentScale);
            lblDpi.SetBounds(txtHeight.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), dpiLabelWidth, labelHeight);
            txtDpi.Multiline = true;
            txtDpi.SetBounds(lblDpi.Right, nextY, exactBoxWidth, fieldHeight);

            nextY = chkReaderSize.Bottom + blockMargin;
            chkFixZip.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            nextY = chkFixZip.Bottom + blockMargin;
            chkOpenFromCover.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            nextY = chkOpenFromCover.Bottom + blockMargin;
            chkFb2Name.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            nextY = chkFb2Name.Bottom + blockMargin;
            chkTranslit.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            // Налаштування групи структури назви файлу
            int rowHeight = fieldHeight + (int)(4 * currentScale);
            int grpOutHeight = (rowHeight * 8) + (int)(25 * currentScale);
            grpOutName.SetBounds(xLeft, chkTranslit.Bottom + blockMargin, fieldWidth, grpOutHeight);

            int comboWidth = (int)(grpOutName.Width * 0.76f);
            int checkFoldWidth = grpOutName.Width - comboWidth - (int)(15 * currentScale);
            int itemY = (int)(20 * currentScale);

            for (int i = 0; i < 8; i++)
            {
                if (cmbOutFields != null && chkAsFolder != null)
                {
                    cmbOutFields[i].ItemHeight = fieldHeight - 6;
                    cmbOutFields[i].SetBounds((int)(10 * currentScale), itemY, comboWidth, fieldHeight);
                    chkAsFolder[i].SetBounds(cmbOutFields[i].Right + (int)(5 * currentScale), itemY + (int)(1 * currentScale), checkFoldWidth, checkBoxHeight);
                    itemY += rowHeight;
                }
            }

            // ВИПРАВЛЕНО CS0747: Правильне роздільне створення об'єкта та встановлення його координат
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, itemY + (int)(10 * currentScale), 1, 1);
            grpOutName.Controls.Add(lblScrollAnchor);
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // Якщо вкладку додали на форму і ми можемо отримати доступ до Form1
            if (ParentForm is Form1 mainForm)
            {
                float currentScale = CreateGraphics().DpiX / 96f;

                // 1. Закруглюємо кнопку (це безпечно робити повторно)
                Form1.MakeButtonRounded(btnBrowseCss, (int)(5 * currentScale));

                // 2. ЗАХИСТ ВІД ПОВТОРНИХ СПРАЦЮВАНЬ (.NET 10 Best Practice):
                // Спочатку відписуємося від подій, а потім підписуємося знову.
                // Це гарантує, що метод викликається рівно один раз при кліку.
                langComboBox.SelectedIndexChanged -= mainForm.LangComboBox_SelectedIndexChanged;
                langComboBox.SelectedIndexChanged += mainForm.LangComboBox_SelectedIndexChanged;

                btnBrowseCss.Click -= mainForm.BtnBrowseCss_Click;
                btnBrowseCss.Click += mainForm.BtnBrowseCss_Click;

                btnDumpConfig.Click -= mainForm.BtnDumpConfig_Click;
                btnDumpConfig.Click += mainForm.BtnDumpConfig_Click;

                chkFb2Name.CheckedChanged -= mainForm.ChkFb2Name_CheckedChanged;
                chkFb2Name.CheckedChanged += mainForm.ChkFb2Name_CheckedChanged;

                // 3. Такий самий захист для всіх 8 комбобоксів структури назви
                if (cmbOutFields != null)
                {
                    for (int i = 0; i < cmbOutFields.Length; i++)
                    {
                        int index = i;

                        // Спочатку повністю очищаємо старі приховані лямбда - підписки, щоб не було дублювання
                        // Для цього у WinForms найкраще скористатися утилітарним скиданням події
                        cmbOutFields[index]!.SelectedIndexChanged += (s, ev) => mainForm.CmbOutFields_SelectedIndexChanged(index);
                    }
                }
            }
        }
    }
}
