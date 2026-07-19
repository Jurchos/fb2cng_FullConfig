using System.ComponentModel;
using System.Diagnostics;
using fb2cng_FullConfig.Templates; // Підключаємо папку з вкладками

namespace fb2cng_FullConfig
{
    [DesignerCategory("Code")]
    public partial class Form1
    {
        // Логічні прапорці захисту від зациклювання графічних подій
        private bool _isThemeApplying;
        private bool _isChangingStates;

        // 1. Керування мовою та локалізацією
        internal void LangComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Оскільки langComboBox тепер лежить всередині вкладки DocumentTab, 
            // дістаємо посилання на нього через кеш вкладок
            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                Config.Settings.CurrentLanguage = docTab.langComboBox.SelectedIndex switch
                {
                    1 => "Ukrainian",
                    2 => "Russian",
                    _ => "English",
                };
                UpdateLocalization();
                ApplyTheme();
                Config.SaveSettings(); // Миттєве збереження обраної мови
            }
        }

        private void UpdateLocalization()
        {
            Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

            string GetText(string key, string defaultText)
            {
                return loc.TryGetValue(key, out string? value) ? value : defaultText;
            }

            // Локалізація статичної (головної) форми та хідера/футера
            Text = GetText("Title", "fb2cng Configurator");
            btnHelp.Text = GetText("Help", "Help");
            btnTheme.Text = GetText("Theme", "Theme");
            btnOk.Text = GetText("Ok", "OK");
            btnCancel.Text = GetText("Cancel", "Cancel");

            // ЛОКАЛІЗАЦІЯ ВКЛАДКИ "document:"
            // Звертаємося до елементів всередині DocumentTab, якщо вона створена в кеші
            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                docTab.lblLang.Text = GetText("Language", "Language:");
                docTab.btnDumpConfig.Text = GetText("DumpConfig", "Dump Default Config");
                docTab.lblConfigName.Text = GetText("ConfigName", "Config Name:");
                docTab.chkCustomYaml.Text = GetText("CustomYamlEnable", "Edit custom .yaml template");
                docTab.chkCss.Text = GetText("CssEnable", "Use Custom CSS");
                docTab.btnBrowseCss.Text = GetText("Browse", " ...");
                docTab.btnBrowseCustomYaml.Text = GetText("Browse", " ...");

                docTab.chkCover.SetTextIfNotNull(GetText("TocType", "Cover Mode"));
                docTab.chkFixZip.SetTextIfNotNull(GetText("FixZip", "Fix Broken ZIP Archives"));
                docTab.chkOpenFromCover.SetTextIfNotNull(GetText("OpenCover", "Open from Cover"));

                docTab.chkFb2Name.Text = GetText("Fb2Name", "Use Original FB2 Name");
                docTab.chkTranslit.Text = GetText("Translit", "Transliterate Output Name");

                docTab.rbFixZipYes.Text = docTab.rbOpenCoverYes.Text = docTab.rbTranslitYes.Text = GetText("Yes", "Yes");
                docTab.rbFixZipNo.Text = docTab.rbOpenCoverNo.Text = docTab.rbTranslitNo.Text = GetText("No", "No");

                docTab.lblOutNameTitle.SetTextIfNotNull(GetText("OutNameTitle", "Output Name Template Constructor"));
                docTab.grpOutName.Text = GetText("OutNameTitle", "Output Structure");
                docTab.chkAsFolder.SetTextForAllIfNotNull(GetText("AsFolder", "Fold"));

                string[] itemKeys = ["Item_Empty", "Item_Author", "Item_Series", "Item_Title", "Item_Lang", "Item_Genre", "Item_Date", "Item_Source", "Item_Uuid", "Item_Short_Uuid"];
                string[] defaultItems = ["", "Author", "Series", "Title", "Language", "Genre", "Date", "Source File", "Book UUID", "Shortened UUID"];

                if (docTab.cmbOutFields != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (docTab.cmbOutFields[i] == null) continue;

                        docTab.cmbOutFields[i].BeginUpdate();
                        int currSel = docTab.cmbOutFields[i].SelectedIndex;
                        docTab.cmbOutFields[i].Items.Clear();

                        for (int k = 0; k < itemKeys.Length; k++)
                        {
                            _ = docTab.cmbOutFields[i].Items.Add(GetText(itemKeys[k], defaultItems[k]));
                        }
                        docTab.cmbOutFields[i].SelectedIndex = currSel >= 0 ? currSel : 0;
                        docTab.cmbOutFields[i].EndUpdate();
                    }
                }
            }
            if (_tabsCache.TryGetValue("metadata:", out UserControl? data) && data is MetadataTab dataTab) // Локалізація вкладки "metadata:"
            {
                dataTab.chkReaderSize.Text = GetText("ReaderSize", "Screen Size");
                dataTab.lblWidth.Text = GetText("Width", "W:");
                dataTab.lblHeight.Text = GetText("Height", "H:");
                dataTab.lblDpi.Text = GetText("Dpi", "DPI:");
                dataTab.chkNotes.Text = GetText("FootnotesMode", "Footnotes display method:");
            }

            // ТУТ У МАЙБУТНЬОМУ БУДЕ ЛОКАЛІЗАЦІЯ ДЛЯ ІНШИХ ВКЛАДОК:
            // if (_tabsCache.TryGetValue("metadata:", out var meta) && meta is MetadataTab metaTab) { ... }
        }

        // 2. Керування візуальною темою з блокуванням мерехтіння
        internal void ApplyTheme()
        {
            if (_isThemeApplying) return;
            _isThemeApplying = true;

            SuspendLayout();

            try
            {
                bool isDark = Config.IsDarkTheme;
                Color darkBg = Color.FromArgb(37, 37, 38);
                Color elementBg = Color.FromArgb(45, 45, 48);
                Color textWhite = Color.FromArgb(245, 245, 245);
                Color textGray = Color.FromArgb(140, 140, 140);
                Color limeAccent = Color.Lime;

                // Основний фон вікна
                BackColor = isDark ? darkBg : SystemColors.Control;

                headerPanel.BackColor = isDark ? elementBg : SystemColors.ControlLight;
                footerPanel.BackColor = isDark ? elementBg : SystemColors.ControlLight;

                SetControlsTheme(headerPanel, isDark ? textWhite : SystemColors.ControlText, isDark ? textGray : SystemColors.GrayText, isDark ? elementBg : SystemColors.Window, isDark ? limeAccent : SystemColors.HotTrack, isDark);
                SetControlsTheme(footerPanel, isDark ? textWhite : SystemColors.ControlText, isDark ? textGray : SystemColors.GrayText, isDark ? elementBg : SystemColors.Window, isDark ? limeAccent : SystemColors.HotTrack, isDark);

                // Фарбуємо всі закешовані вкладки
                foreach (Control activeTab in pnlContent.Controls)
                {
                    // ВИПРАВЛЕННЯ: Встановлюємо фоном SystemColors.Window (Білий у світлій темі)
                    activeTab.BackColor = isDark ? darkBg : SystemColors.Window;

                    if (activeTab is DocumentTab docTab)
                    {
                        docTab.scrollMenuPanel.BackColor = activeTab.BackColor;
                        docTab.grpOutName.BackColor = activeTab.BackColor;
                    }

                    // Також перевіряємо MetadataTab, якщо потрібно (тепер він успадкує BackColor вкладки)
                    SetControlsTheme(activeTab, isDark ? textWhite : SystemColors.ControlText, isDark ? textGray : SystemColors.GrayText, isDark ? elementBg : SystemColors.Window, isDark ? limeAccent : SystemColors.HotTrack, isDark);
                }
            }
            finally
            {
                ResumeLayout(true);
                _isThemeApplying = false;
            }
        }

        private void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || sender is not ComboBox cb)
            {
                return;
            }

            bool isControlDisabled = !cb.Enabled || (cb.Parent != null && !cb.Parent.Enabled);
            e.DrawBackground();

            Color drawTextColor = isControlDisabled ? Color.FromArgb(140, 140, 140) : cb.ForeColor;

            if (isControlDisabled)
            {
                // Задаємо колір фону для заблокованого стану залежно від поточної теми
                Color bgDisabledColor = Config.IsDarkTheme ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
                using SolidBrush bgBrush = new(bgDisabledColor);
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            TextRenderer.DrawText(
                e.Graphics,
                cb.Items[e.Index]?.ToString() ?? string.Empty,
                cb.Font,
                e.Bounds,
                drawTextColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            if (!isControlDisabled)
            {
                e.DrawFocusRectangle();
            }
        }

        // Додаємо новий метод вибору YAML файлу:
        internal void BtnBrowseCustomYaml_Click(object? sender, EventArgs e)
        {
            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                if (!docTab.chkCustomYaml.Checked) return;

                using OpenFileDialog ofd = new();
                ofd.Filter = "YAML Files (*.yaml;*.yml)|*.yaml;*.yml|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string selectedFile = ofd.FileName;
                    string relativePath = Path.GetRelativePath(appPath, selectedFile);
                    // Встановлюємо шлях
                    docTab.txtCustomYamlPath.Text = relativePath.Replace('\\', '/');

                    // ЯВНО ВИКЛИКАЄМО СИНХРОНІЗАЦІЮ ТУТ
                    SyncConfigNameWithYaml(docTab);
                }
            }
        }

        internal void BtnBrowseCss_Click(object? sender, EventArgs e)
        {
            // Дістаємо посилання на вкладку документа для роботи з її полями
            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                if (!docTab.chkCss.Checked)
                {
                    return;
                }

                using OpenFileDialog ofd = new();
                ofd.Filter = "CSS Files (*.css)|*.css";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string selectedFile = ofd.FileName;
                    string relativePath = Path.GetRelativePath(appPath, selectedFile);
                    docTab.txtCssPath.Text = relativePath.Replace('\\', '/');
                }
            }
        }

        internal void ChkFb2Name_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isChangingStates) return;
            _isChangingStates = true;

            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                try
                {
                    bool isFb2Enabled = docTab.chkFb2Name.Checked;

                    // 1. САМ GroupBox ЗАЛИШАЄМО ENABLED = TRUE
                    docTab.grpOutName.Enabled = true;

                    // 2. ВИМИКАЄМО ТІЛЬКИ ЕЛЕМЕНТИ ВСЕРЕДИНІ
                    for (int i = 0; i < 8; i++)
                    {
                        docTab.cmbOutFields![i].Enabled = !isFb2Enabled;
                        docTab.chkAsFolder![i].Enabled = !isFb2Enabled;

                        if (isFb2Enabled)
                        {
                            docTab.cmbOutFields[i].SelectedIndex = 0;
                            docTab.chkAsFolder[i].Checked = false;
                        }
                    }

                    if (!isFb2Enabled) CmbOutFields_SelectedIndexChanged(0);
                }
                finally { _isChangingStates = false; }

                ApplyTheme(); // Це викличе SetControlsTheme
            }
        }

        private void SetControlsTheme(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor, bool isDark)
        {
            _tabsCache.TryGetValue("document:", out var tab);
            var docTab = tab as DocumentTab;

            bool isFb2NameChecked = docTab?.chkFb2Name.Checked ?? false;
            bool isGrpOutEnabled = docTab?.grpOutName.Enabled ?? true;
            bool isCssChecked = docTab?.chkCss.Checked ?? false;

            SetControlsThemeRecursive(parent, foreColor, disabledColor, backColor, folderColor, isDark, docTab, isFb2NameChecked, isCssChecked);
        }

        private void SetControlsThemeRecursive(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor, bool isDark, DocumentTab? docTab, bool isFb2NameChecked, bool isCssChecked)
        {
            Control? currentBrowseCssBtn = docTab?.btnBrowseCss;
            Control? currentGrpOutName = docTab?.grpOutName;
            bool isOutNameDisabled = isFb2NameChecked || (docTab != null && !docTab.grpOutName.Enabled);

            foreach (Control c in parent.Controls)
            {
                bool isControlDisabled = !c.Enabled
                    || (currentGrpOutName != null && (c.Parent == currentGrpOutName || c.Parent?.Parent == currentGrpOutName) && isOutNameDisabled)
                    || (isDark && c == currentBrowseCssBtn && !isCssChecked);

                if (c is GroupBox gb)
                {
                    gb.BackColor = parent.BackColor;
                    gb.ForeColor = isFb2NameChecked ? disabledColor : (isDark ? foreColor : SystemColors.ControlText);
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = isControlDisabled ? disabledColor : foreColor;
                    lbl.BackColor = Color.Transparent;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = !isControlDisabled && chk.Tag?.ToString() == "FolderCheckBox" ? folderColor : (isControlDisabled ? disabledColor : foreColor);
                    chk.BackColor = Color.Transparent;
                }
                else if (c is TextBox txt)
                {
                    txt.BackColor = backColor;
                    txt.ForeColor = isControlDisabled ? disabledColor : foreColor;
                }
                else if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;

                    bool isBrowseBtn = (btn == docTab?.btnBrowseCss) || (btn == docTab?.btnBrowseCustomYaml);
                    bool isParentChecked = (btn == docTab?.btnBrowseCss && isCssChecked) ||
                                           (btn == docTab?.btnBrowseCustomYaml && (docTab?.chkCustomYaml.Checked ?? false));
                    if (isDark)
                    {
                        btn.BackColor = (isBrowseBtn && !isParentChecked) ? Color.FromArgb(40, 40, 42) : backColor;
                        btn.ForeColor = (isBrowseBtn && !isParentChecked) ? disabledColor : foreColor;
                        btn.FlatAppearance.BorderColor = (isBrowseBtn && !isParentChecked) ? Color.FromArgb(55, 55, 58) : Color.FromArgb(100, 100, 105);
                    }
                    else
                    {
                        btn.BackColor = SystemColors.Control;
                        btn.ForeColor = (isBrowseBtn && !isParentChecked) ? disabledColor : SystemColors.ControlText;
                        btn.FlatAppearance.BorderColor = (isBrowseBtn && !isParentChecked) ? Color.LightGray : Color.DarkGray;
                    }
                }
                else if (c is RadioButton rb)
                {
                    rb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                    rb.BackColor = Color.Transparent;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = backColor;
                    cb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                    cb.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;
                    cb.DrawMode = isDark ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
                    cb.DrawItem -= ComboBox_DrawItem;
                    if (isDark) cb.DrawItem += ComboBox_DrawItem;
                }

                if (c.HasChildren)
                    SetControlsThemeRecursive(c, foreColor, disabledColor, backColor, folderColor, isDark, docTab, isFb2NameChecked, isCssChecked);
            }
        }

        public DialogResult ShowCustomMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using Form msgForm = new();
            // Використовуємо нашу глобальну змінну теми
            bool isDark = Config.IsDarkTheme;

            // Визначаємо українську мову з нашого глобального конфігу
            bool isUa = Config.Settings.CurrentLanguage == "Ukrainian";

            msgForm.Text = caption;
            msgForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            msgForm.MaximizeBox = false;
            msgForm.MinimizeBox = false;
            msgForm.StartPosition = FormStartPosition.CenterScreen;
            msgForm.Font = new Font("Segoe UI", 10F);
            msgForm.BackColor = isDark ? Color.FromArgb(24, 24, 24) : Color.FromArgb(245, 245, 245);

            // --- 1. АВТОМАТИЧНЕ ВИЗНАЧЕННЯ МАСШТАБУ DPI ---
            float currentScale = msgForm.Font.Height / 18f;

            // --- 2. МАСШТАБОВАНІ ВІДСТУПИ ТА РОЗМІРИ ---
            int paddingTop = (int)(18 * currentScale);
            int paddingMiddle = (int)(15 * currentScale);
            int paddingBottom = (int)(12 * currentScale);
            int buttonHeight = (int)(32 * currentScale);
            int buttonWidth = (int)(100 * currentScale);

            // Збільшуємо базову ширину, якщо є іконка, щоб текст вміщався
            int baseWidth = (icon != MessageBoxIcon.None) ? 360 : 330;
            int calculatedWidth = (int)(baseWidth * currentScale);
            msgForm.ClientSize = new Size(calculatedWidth, msgForm.ClientSize.Height);

            // Налаштування іконки
            PictureBox? picIcon = null;
            // Зменшуємо базовий розмір іконки до 24 для компактності
            int iconSize = (int)(24 * currentScale);
            int textTopOffset = paddingTop;

            if (icon != MessageBoxIcon.None)
            {
                picIcon = new PictureBox
                {
                    Size = new Size(iconSize, iconSize),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    // ЦЕНТРУЄМО ІКОНКУ ПО ГОРИЗОНТАЛІ
                    Location = new Point((msgForm.ClientSize.Width - iconSize) / 2, paddingTop)
                };

                // Малюємо компактні векторні іконки
                Bitmap bmp = new(iconSize, iconSize);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // Спрощуємо перевірки, оскільки Hand/Stop — це те саме, що й Error
                    if (icon == MessageBoxIcon.Error)
                    {
                        g.FillEllipse(Brushes.Crimson, 0, 0, iconSize - 1, iconSize - 1);
                        using Pen pen = new(Color.White, 2.5f);
                        int offset = iconSize / 4;
                        g.DrawLine(pen, offset, offset, iconSize - offset, iconSize - offset);
                        g.DrawLine(pen, iconSize - offset, offset, offset, iconSize - offset);
                    }
                    // Asterisk — це те саме, що й Information
                    else if (icon == MessageBoxIcon.Information)
                    {
                        Color infoColor = isDark ? Color.FromArgb(0, 140, 255) : Color.FromArgb(0, 102, 204);
                        using Brush infoBrush = new SolidBrush(infoColor);
                        g.FillEllipse(infoBrush, 0, 0, iconSize - 1, iconSize - 1);

                        // Виносимо створення шрифту в using, щоб уникнути витоку пам'яті
                        using Font infoFont = new("Georgia", 12F, FontStyle.Bold | FontStyle.Italic);
                        g.DrawString("i", infoFont, Brushes.White, new PointF(iconSize * 0.26f, iconSize * 0.08f));
                    }
                    // Exclamation — це те саме, що й Warning
                    else if (icon == MessageBoxIcon.Warning)
                    {
                        PointF[] points = [new(iconSize / 2f, 0), new(0, iconSize - 1), new(iconSize - 1, iconSize - 1)];
                        g.FillPolygon(Brushes.Orange, points);

                        // Виносимо створення шрифту в using, щоб уникнути витоку пам'яті
                        using Font warningFont = new("Segoe UI", 11F, FontStyle.Bold);
                        g.DrawString("!", warningFont, Brushes.White, new PointF(iconSize * 0.35f, iconSize * 0.18f));
                    }
                }
                picIcon.Image = bmp;
                msgForm.Controls.Add(picIcon);

                // Зменшений відступ тексту від нижнього краю іконки (усього 6 пікселів, масштабованих під DPI)
                textTopOffset = picIcon.Bottom + (int)(6 * currentScale);
            }

            // Налаштування RichTextBox для тексту
            RichTextBox rtbText = new()
            {
                Text = text,
                Width = msgForm.ClientSize.Width - (int)(32 * currentScale),
                ForeColor = isDark ? Color.White : Color.Black,
                BackColor = msgForm.BackColor,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                TabStop = false,
                TabIndex = 99
            };

            // ТЕКСТ ЗАВЖДИ ПО ЦЕНТРУ
            rtbText.SelectAll();
            rtbText.SelectionAlignment = HorizontalAlignment.Center;
            rtbText.DeselectAll();

            rtbText.MouseDown += (s, e) => { _ = Win32Api.HideCaret(rtbText.Handle); _ = msgForm.Focus(); };
            rtbText.GotFocus += (s, e) => { _ = Win32Api.HideCaret(rtbText.Handle); };

            msgForm.Controls.Add(rtbText);

            // --- 3. ДИНАМІЧНИЙ РОЗРАХУНОК ВИСОТИ ТЕКСТУ ---
            int lastCharIndex = rtbText.TextLength > 0 ? rtbText.TextLength - 1 : 0;
            Point lastCharPos = rtbText.GetPositionFromCharIndex(lastCharIndex);
            int textHeight = lastCharPos.Y + rtbText.Font.Height + (int)(10 * currentScale);

            int minTextHeight = (int)(40 * currentScale);
            if (textHeight < minTextHeight)
            {
                textHeight = minTextHeight;
            }

            rtbText.Height = textHeight;

            // Позиціонуємо текст суворо по центру вікна по горизонталі, а по вертикалі — нижче іконки
            rtbText.Location = new Point((msgForm.ClientSize.Width - rtbText.Width) / 2, textTopOffset);

            // Розраховуємо фінальну Y-координату для кнопок під текстом
            int buttonsY = rtbText.Bottom + paddingMiddle;

            Color btnBg = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(230, 230, 230);
            Color btnTextCol = isDark ? Color.White : Color.Black;
            Color accentBg = isDark ? Color.FromArgb(0, 102, 204) : Color.FromArgb(0, 120, 215);

            Button? primaryButton = null;

            if (buttons == MessageBoxButtons.OK)
            {
                Button btnOkCustom = new()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Size = new Size(buttonWidth, buttonHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = accentBg,
                    ForeColor = Color.White,
                    TabIndex = 0
                };
                btnOkCustom.FlatAppearance.BorderSize = 0;
                MakeButtonRounded(btnOkCustom, (int)(4 * currentScale)); // Використовуємо ваш покращений метод

                btnOkCustom.Location = new Point((msgForm.ClientSize.Width - btnOkCustom.Width) / 2, buttonsY);

                msgForm.Controls.Add(btnOkCustom);
                msgForm.AcceptButton = btnOkCustom;
                primaryButton = btnOkCustom;
            }
            else if (buttons == MessageBoxButtons.OKCancel)
            {
                Button btnOkCustom = new()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Size = new Size(buttonWidth, buttonHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = accentBg,
                    ForeColor = Color.White,
                    TabIndex = 0
                };
                btnOkCustom.FlatAppearance.BorderSize = 0;
                MakeButtonRounded(btnOkCustom, (int)(4 * currentScale));

                Button btnCancelCustom = new()
                {
                    Text = isUa ? "Скасувати" : "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(buttonWidth, buttonHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = btnBg,
                    ForeColor = btnTextCol,
                    TabIndex = 1
                };
                btnCancelCustom.FlatAppearance.BorderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200);
                MakeButtonRounded(btnCancelCustom, (int)(4 * currentScale));

                int spacing = (int)(15 * currentScale);
                int totalButtonsWidth = btnOkCustom.Width + spacing + btnCancelCustom.Width;
                int startX = (msgForm.ClientSize.Width - totalButtonsWidth) / 2;

                btnOkCustom.Location = new Point(startX, buttonsY);
                btnCancelCustom.Location = new Point(startX + btnOkCustom.Width + spacing, buttonsY);

                msgForm.Controls.AddRange([btnOkCustom, btnCancelCustom]);
                msgForm.AcceptButton = btnOkCustom;
                msgForm.CancelButton = btnCancelCustom;
                primaryButton = btnOkCustom;
            }

            int finalHeight = buttonsY + buttonHeight + paddingBottom;
            msgForm.ClientSize = new Size(calculatedWidth, finalHeight);

            Rectangle primaryScreen = Screen.FromControl(this).Bounds;
            msgForm.Location = new Point(
                primaryScreen.Left + ((primaryScreen.Width - msgForm.Width) / 2),
                primaryScreen.Top + ((primaryScreen.Height - msgForm.Height) / 2)
            );

            msgForm.StartPosition = FormStartPosition.CenterScreen;
            msgForm.TopMost = true;

            msgForm.Shown += (s, e) =>
            {
                try
                {
                    IntPtr msgFormHandle = msgForm.Handle;
                    IntPtr foregroundWindowHandle = Win32Api.GetForegroundWindow();
                    uint foregroundThreadId = Win32Api.GetWindowThreadProcessId(foregroundWindowHandle, IntPtr.Zero);
                    uint currentThreadId = Win32Api.GetCurrentThreadId();

                    if (foregroundThreadId != currentThreadId && foregroundThreadId != 0)
                    {
                        _ = Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, true);
                        _ = Win32Api.SetForegroundWindow(msgFormHandle);
                        _ = Win32Api.SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                        _ = Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, false);
                    }
                    else
                    {
                        _ = Win32Api.SetForegroundWindow(msgFormHandle);
                        _ = Win32Api.SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                    }
                }
                catch { }

                if (primaryButton != null)
                {
                    _ = primaryButton.Focus();
                }

                _ = msgForm.BeginInvoke(new Action(() => { _ = Win32Api.HideCaret(rtbText.Handle); }));
            };

            return msgForm.ShowDialog();
        }


        internal void CmbOutFields_SelectedIndexChanged(int index)
        {
            bool internalCall = _isChangingStates;
            if (!internalCall)
            {
                _isChangingStates = true;
            }

            // Дістаємо посилання на вкладку документа для роботи з масивами її контролів
            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                try
                {
                    bool hasSelection = docTab.cmbOutFields![index].SelectedIndex > 0;
                    docTab.chkAsFolder![index].Enabled = hasSelection;
                    if (!hasSelection)
                    {
                        docTab.chkAsFolder[index].Checked = false;
                    }

                    if (index < 7)
                    {
                        if (hasSelection)
                        {
                            docTab.cmbOutFields[index + 1].Enabled = true;
                        }
                        else
                        {
                            for (int i = index + 1; i < 8; i++)
                            {
                                docTab.cmbOutFields[i].SelectedIndex = 0;
                                docTab.cmbOutFields[i].Enabled = false;
                                docTab.chkAsFolder[i].Checked = false;
                                docTab.chkAsFolder[i].Enabled = false;
                            }
                        }
                    }
                }
                finally
                {
                    if (!internalCall)
                    {
                        _isChangingStates = false;
                    }
                }
                if (!internalCall)
                {
                    ApplyTheme();
                }
            }
        }

        internal async void BtnDumpConfig_Click(object? sender, EventArgs e)
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            if (_tabsCache.TryGetValue("document:", out var tab) && tab is DocumentTab docTab)
            {
                if (!YamlService.IsEngineAvailable())
                {
                    string caption = langDict?.GetValueOrDefault("ErrTitle", "Error") ?? "Error";
                    string text = langDict?.GetValueOrDefault("ErrFbc", "Error: fbc.exe not found!") ?? "Error: fbc.exe not found!";

                    _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Кнопка дампу тепер береться з об'єкта docTab
                docTab.btnDumpConfig.Enabled = false;
                string prevText = docTab.btnDumpConfig.Text;
                docTab.btnDumpConfig.Text = Config.Settings.CurrentLanguage == "Ukrainian" ? "Генерація..." : "Generating...";

                bool success = await Task.Run(YamlService.ExecuteSyncDumpConfig);

                docTab.btnDumpConfig.Text = prevText;
                docTab.btnDumpConfig.Enabled = true;

                if (success)
                {
                    string caption = langDict?.GetValueOrDefault("GenTitle", "Success") ?? "Success";
                    string msg = langDict?.GetValueOrDefault("GenSuccess", "config.yaml successfully generated!") ?? "config.yaml successfully generated!";

                    _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

private void SyncConfigNameWithYaml(DocumentTab docTab)
{
    if (docTab.chkCustomYaml.Checked)
    {
        // Якщо шлях обрано - копіюємо його
        if (!string.IsNullOrWhiteSpace(docTab.txtCustomYamlPath.Text))
        {
            docTab.txtConfigName.Text = docTab.txtCustomYamlPath.Text;
        }
        else
        {
            // Якщо чекбокс увімкнено, але шлях ще не обрано - 
            // очищуємо поле, щоб не залишався "config.yaml"
            docTab.txtConfigName.Text = ""; 
        }
    }
    else
    {
        // Якщо вимкнено - повертаємо стандарт
        docTab.txtConfigName.Text = "config.yaml";
    }
}

        private void SyncCssWithCustomYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked && docTab.chkCss.Checked)
            {
                // ВАЖЛИВО: Якщо користувач вже обрав файл (поле не порожнє), 
                // ми не затираємо його автоматично при кожному кліку чекбокса.
                if (!string.IsNullOrEmpty(docTab.txtCssPath.Text)) return;

                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string value = YamlService.ReadYamlValue(fullPath, "stylesheet_path");
                        if (!string.IsNullOrEmpty(value))
                        {
                            docTab.txtCssPath.Text = value;
                        }
                    }
                }
            }
            if (!docTab.chkCustomYaml.Checked)
            {
                docTab.txtCssPath.Text = "";
                return;
            }
        }

        private void SyncTocTypeWithCustomYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string value = YamlService.ReadYamlValue(fullPath, "toc_type");

                        if (!string.IsNullOrEmpty(value))
                        {
                            int index = docTab.cmbCoverMode.Items.IndexOf(value);
                            if (index >= 0)
                            {
                                docTab.cmbCoverMode.SelectedIndex = index;
                                return; // Виходимо, якщо значення успішно знайдено і встановлено
                            }
                        }
                    }
                }
            }

            // СКИНУТИ до значення за замовчуванням ("normal"), якщо галочка знята або файл не знайдено
            if (docTab.cmbCoverMode.Items.Count > 0)
            {
                docTab.cmbCoverMode.SelectedIndex = 0; // Перший пункт зазвичай "normal"
            }
        }

        private void SyncBinarySettingsWithYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string fixZip = YamlService.ReadYamlValue(fullPath, "fix_zip").ToLower();
                        string openCover = YamlService.ReadYamlValue(fullPath, "open_from_cover").ToLower();
                        string translit = YamlService.ReadYamlValue(fullPath, "file_name_transliterate").ToLower();

                        docTab.rbFixZipYes.Checked = (fixZip == "true");
                        docTab.rbFixZipNo.Checked = (fixZip != "true"); // за замовчуванням false

                        docTab.rbOpenCoverYes.Checked = (openCover == "true");
                        docTab.rbOpenCoverNo.Checked = (openCover != "true");

                        docTab.rbTranslitYes.Checked = (translit == "true");
                        docTab.rbTranslitNo.Checked = (translit != "true");
                        return;
                    }
                }
            }
            // СКИНУТИ, якщо галочка "Редагувати..." знята
            docTab.rbFixZipNo.Checked = true;
            docTab.rbOpenCoverNo.Checked = true;
            docTab.rbTranslitNo.Checked = true;
        }

        private void BtGui_Click(object? sender, EventArgs e)
        {
            var runningProcesses = Process.GetProcessesByName("fb2cng_GUI");
            if (runningProcesses.Length > 0)
            {
                IntPtr hWnd = runningProcesses[0].MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    if (Win32Api.IsIconic(hWnd))
                    {
                        _ = Win32Api.ShowWindow(hWnd, 9); // SW_RESTORE
                    }

                    _ = Win32Api.SetForegroundWindow(hWnd);
                    return;
                }
            }

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe");

            if (File.Exists(exePath))
            {
                try
                {
                    _ = Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                }
                catch (Exception)
                {
                    ShowGuiMissingError();
                }
            }
            else
            {
                ShowGuiMissingError();
            }
        }

        private void ShowGuiMissingError()
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            string caption = langDict?.GetValueOrDefault("ErrTitle", "Configuration Error") ?? "Configuration Error";
            string text = langDict?.GetValueOrDefault("ErrGui", "GUI program not found: please verify that 'fb2cng_GUI.exe' is present in the application folder!") ?? "GUI program not found: please verify that 'fb2cng_GUI.exe' is present in the application folder!";

            _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SaveYamlConfiguration()
        {
            _tabsCache.TryGetValue("document:", out var doc);
            _tabsCache.TryGetValue("metadata:", out var data);

            var docTab = doc as DocumentTab;
            var dataTab = data as MetadataTab;

            if (docTab != null)
            {
                // 1. Готуємо масиви для конструктора назви
                int[] fieldIndexes = new int[8];
                bool[] folderFlags = new bool[8];
                if (docTab.cmbOutFields != null && docTab.chkAsFolder != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        fieldIndexes[i] = docTab.cmbOutFields[i].SelectedIndex;
                        folderFlags[i] = docTab.chkAsFolder[i].Checked;
                    }
                }

                // 2. Виклик сервісу з ПРАВИЛЬНИМ ПОРЯДКОМ АРГУМЕНТІВ
                bool saved = YamlService.SaveConfiguration(
                    docTab.txtConfigName.Text,             // configName
                    docTab.chkCustomYaml.Checked,          // useCustomYaml
                    docTab.txtCustomYamlPath.Text,         // customYamlPath
                    docTab.chkCss.Checked,                 // useCss
                    docTab.txtCssPath.Text,                // cssPath
                    dataTab?.chkReaderSize.Checked ?? false, // customSize
                    dataTab?.txtWidth.Text ?? "1264",      // width
                    dataTab?.txtHeight.Text ?? "1680",     // height
                    dataTab?.txtDpi.Text ?? "300",         // dpi
                    docTab.chkCover.Checked,               // useCoverMode
                    docTab.cmbCoverMode.SelectedItem?.ToString() ?? "normal", // coverMode
                    dataTab?.chkNotes.Checked ?? false,    // useNotesMode
                    dataTab?.cmbNotesMode.SelectedItem?.ToString() ?? "default", // notesMode
                    docTab.chkTranslit.Checked,            // saveTranslit
                    docTab.rbTranslitYes.Checked,          // translitVal
                    docTab.chkOpenFromCover.Checked,       // saveOpenCover
                    docTab.rbOpenCoverYes.Checked,         // openCoverVal
                    docTab.chkFixZip.Checked,              // saveFixZip
                    docTab.rbFixZipYes.Checked,            // fixZipVal
                    docTab.chkFb2Name.Checked,             // useFb2Name
                    fieldIndexes,                          // fieldIndexes
                    folderFlags                            // folderFlags
                );

                if (saved) Close();
            }
        }

        private void ShowHelp()
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            string caption = langDict?.GetValueOrDefault("Help", "Help / Довідка") ?? "Help / Довідка";
            string msg = langDict?.GetValueOrDefault("HelpText", "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.") ?? "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.";

            _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public static class ControlExtensions // Розширення для перевірки null перед (залишаємо без змін)
    {
        public static void SetTextIfNotNull(this Control? control, string text)
        {
            if (control == null)
            {
                return;
            }
            control.Text = text;
        }

        public static void SetTextForAllIfNotNull(this IEnumerable<Control?>? controls, string text)
        {
            if (controls == null)
            {
                return;
            }

            foreach (Control? control in controls)
            {
                control.SetTextIfNotNull(text);
            }
        }
    }
}
