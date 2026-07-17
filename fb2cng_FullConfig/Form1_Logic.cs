using System.Diagnostics;
using System.ComponentModel;

namespace fb2cng_FullConfig
{
    [DesignerCategory("Code")]
    public partial class Form1
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        // Логічні прапорці захисту від зациклювання графічних подій
        private bool _isThemeApplying = false;
        private bool _isChangingStates = false;

        // 1. Керування мовою та локалізацією
        private void LangComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            Config.Settings.CurrentLanguage = langComboBox.SelectedIndex switch
            {
                1 => "Ukrainian",
                2 => "Russian",
                _ => "English",
            };
            UpdateLocalization();
            ApplyTheme();
            Config.SaveSettings(); // Миттєве збереження обраної мови
        }

        private void UpdateLocalization()
        {
            Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

            string GetText(string key, string defaultText)
            {
                return loc.ContainsKey(key) ? loc[key] : defaultText;
            }

            Text = GetText("Title", "fb2cng Configurator");
            lblLang.Text = GetText("Language", "Language:");
            btnDumpConfig.Text = GetText("DumpConfig", "Dump Default Config");
            lblConfigName.Text = GetText("ConfigName", "Config Name:");
            chkCss.Text = GetText("CssEnable", "Use Custom CSS");
            btnBrowseCss.Text = GetText("Browse", "Browse...");

            if (chkNotes != null)
            {
                chkNotes.Text = GetText("FootnotesMode", "Footnotes Mode");
            }

            if (chkCover != null)
            {
                chkCover.Text = GetText("TocType", "Cover Mode");
            }

            chkReaderSize.Text = GetText("ReaderSize", "Set Custom Display Size");
            lblWidth.Text = GetText("Width", "W:");
            lblHeight.Text = GetText("Height", "H:");
            lblDpi.Text = GetText("Dpi", "DPI:");

            if (chkFixZip != null)
            {
                chkFixZip.Text = GetText("FixZip", "Fix Broken ZIP Archives");
            }

            if (chkOpenFromCover != null)
            {
                chkOpenFromCover.Text = GetText("OpenCover", "Open from Cover");
            }

            chkFb2Name.Text = GetText("Fb2Name", "Use Original FB2 Name");
            chkTranslit.Text = GetText("Translit", "Transliterate Output Name");

            if (lblOutNameTitle != null)
            {
                lblOutNameTitle.Text = GetText("OutNameTitle", "Output Name Template Constructor");
            }

            if (grpOutName != null)
            {
                grpOutName.Text = GetText("OutNameTitle", "Output Structure");
            }

            if (chkAsFolder != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (chkAsFolder[i] != null)
                    {
                        chkAsFolder[i].Text = GetText("AsFolder", "Fold");
                    }
                }
            }

            string[] itemKeys = { "Item_Empty", "Item_Author", "Item_Series", "Item_Title", "Item_Lang", "Item_Genre", "Item_Date", "Item_Source", "Item_Uuid", "Item_Short_Uuid" };
            string[] defaultItems = { "", "Author", "Series", "Title", "Language", "Genre", "Date", "Source File", "Book UUID", "Shortened UUID" };

            if (cmbOutFields != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (cmbOutFields[i] == null)
                    {
                        continue;
                    }

                    cmbOutFields[i].BeginUpdate();

                    int currSel = cmbOutFields[i].SelectedIndex;
                    cmbOutFields[i].Items.Clear();

                    for (int k = 0; k < itemKeys.Length; k++)
                    {
                        _ = cmbOutFields[i].Items.Add(GetText(itemKeys[k], defaultItems[k]));
                    }
                    cmbOutFields[i].SelectedIndex = currSel >= 0 ? currSel : 0;
                    cmbOutFields[i].EndUpdate();
                }
            }
            btnHelp.Text = GetText("Help", "Help");
            btnTheme.Text = GetText("Theme", "Theme");
            btnOk.Text = GetText("Ok", "OK");
            btnCancel.Text = GetText("Cancel", "Cancel");
        }

        // 2. Керування візуальною темою з блокуванням мерехтіння
        private void ApplyTheme()
        {
            if (_isThemeApplying)
            {
                return;
            }

            _isThemeApplying = true;

            // Повністю забороняємо Windows надсилати події малювання для цього вікна
            _ = SendMessage(Handle, WM_SETREDRAW, false, 0);
            SuspendLayout();

            try
            {
                if (Config.IsDarkTheme)
                {
                    Color darkBg = Color.FromArgb(37, 37, 38);
                    Color elementBg = Color.FromArgb(45, 45, 48);
                    Color textWhite = Color.FromArgb(245, 245, 245);
                    Color limeAccent = Color.Lime;
                    Color textGray = Color.FromArgb(140, 140, 140);

                    BackColor = darkBg;
                    scrollMenuPanel.BackColor = darkBg;
                    footerPanel.BackColor = elementBg;
                    grpOutName.BackColor = darkBg;

                    SetControlsTheme(this, textWhite, textGray, elementBg, limeAccent, true);
                }
                else
                {
                    BackColor = SystemColors.Control;
                    scrollMenuPanel.BackColor = SystemColors.Window;
                    footerPanel.BackColor = SystemColors.ControlLight;
                    grpOutName.BackColor = SystemColors.Window;

                    SetControlsTheme(this, SystemColors.ControlText, SystemColors.GrayText, SystemColors.Window, SystemColors.HotTrack, false);
                }
            }
            finally
            {
                ResumeLayout(true);
                // Дозволяємо малювання назад
                _ = SendMessage(Handle, WM_SETREDRAW, true, 0);

                // Примушуємо ОС перерендерити вікно та всі дочірні елементи знизу-вгору одним кадром
                Refresh();

                _isThemeApplying = false;
            }
        }
        private void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(sender is ComboBox cb))
            {
                return;
            }

            bool isControlDisabled = !cb.Enabled || (cb.Parent != null && !cb.Parent.Enabled);
            e.DrawBackground();

            Color drawTextColor = isControlDisabled ? Color.FromArgb(140, 140, 140) : cb.ForeColor;

            if (isControlDisabled)
            {
                using SolidBrush bgBrush = new SolidBrush(Color.FromArgb(45, 45, 48));
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

        private void BtnBrowseCss_Click(object? sender, EventArgs e)
        {
            if (!chkCss.Checked)
            {
                return;
            }

            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSS Files (*.css)|*.css";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string selectedFile = ofd.FileName;
                string relativePath = selectedFile.StartsWith(appPath) ? selectedFile.Substring(appPath.Length) : Path.GetFileName(selectedFile);
                txtCssPath.Text = relativePath.Replace("\\", "/");
            }
        }

        private void ChkFb2Name_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isChangingStates)
            {
                return;
            }

            _isChangingStates = true;

            try
            {
                if (!Config.IsDarkTheme)
                {
                    grpOutName.Enabled = !chkFb2Name.Checked;
                }

                if (chkFb2Name.Checked)
                {
                    for (int i = 0; i < cmbOutFields!.Length; i++)
                    {
                        cmbOutFields[i].SelectedIndex = 0;
                        cmbOutFields[i].Enabled = false;
                    }

                    for (int i = 0; i < chkAsFolder!.Length; i++)
                    {
                        chkAsFolder[i].Checked = false;
                        chkAsFolder[i].Enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < cmbOutFields!.Length; i++)
                    {
                        ComboBox cmb = cmbOutFields[i];
                        cmb.Enabled = true;
                    }

                    cmbOutFields[0].Enabled = true;
                    CmbOutFields_SelectedIndexChanged(0);
                }
            }
            finally { _isChangingStates = false; }
            ApplyTheme();
        }

        private void SetControlsTheme(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor, bool isDark)
        {
            bool isOutNameDisabled = isDark ? chkFb2Name.Checked : !grpOutName.Enabled;

            foreach (Control c in parent.Controls)
            {
                bool isControlDisabled = !c.Enabled || (c.Parent == grpOutName && isOutNameDisabled) || (isDark && c == btnBrowseCss && !chkCss.Checked);

                if (c is GroupBox gb)
                {
                    gb.BackColor = parent.BackColor;
                    gb.ForeColor = isDark ? (chkFb2Name.Checked ? disabledColor : foreColor) : SystemColors.ControlText;
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
                    if (isDark)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = (btn == btnBrowseCss && !chkCss.Checked) ? Color.FromArgb(55, 55, 58) : Color.FromArgb(100, 100, 105);
                        btn.BackColor = (btn == btnBrowseCss && !chkCss.Checked) ? Color.FromArgb(40, 40, 42) : backColor;
                        btn.ForeColor = (btn == btnBrowseCss && !chkCss.Checked) ? disabledColor : foreColor;
                    }
                    else
                    {
                        btn.FlatStyle = FlatStyle.Standard;
                        btn.BackColor = SystemColors.Control;
                        btn.ForeColor = SystemColors.ControlText;
                    }
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = backColor;
                    cb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cb.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;
                    cb.DrawMode = isDark ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
                    cb.DrawItem -= ComboBox_DrawItem;
                    if (isDark)
                    {
                        cb.DrawItem += ComboBox_DrawItem;
                    }
                }

                if (c.HasChildren)
                {
                    SetControlsTheme(c, foreColor, disabledColor, backColor, folderColor, isDark);
                }
            }
        }

        public DialogResult ShowCustomMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using Form msgForm = new Form();
            // Використовуємо вашу глобальну змінну теми
            bool isDark = Config.IsDarkTheme;

            // Визначаємо українську мову з вашого глобального конфігу
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
                Bitmap bmp = new Bitmap(iconSize, iconSize);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    if (icon == MessageBoxIcon.Error || icon == MessageBoxIcon.Hand || icon == MessageBoxIcon.Stop)
                    {
                        g.FillEllipse(Brushes.Crimson, 0, 0, iconSize - 1, iconSize - 1);
                        using Pen pen = new Pen(Color.White, 2.5f);
                        int offset = iconSize / 4;
                        g.DrawLine(pen, offset, offset, iconSize - offset, iconSize - offset);
                        g.DrawLine(pen, iconSize - offset, offset, offset, iconSize - offset);
                    }
                    else if (icon == MessageBoxIcon.Information || icon == MessageBoxIcon.Asterisk)
                    {
                        Color infoColor = isDark ? Color.FromArgb(0, 140, 255) : Color.FromArgb(0, 102, 204);
                        using (Brush infoBrush = new SolidBrush(infoColor))
                        {
                            g.FillEllipse(infoBrush, 0, 0, iconSize - 1, iconSize - 1);
                        }

                        g.DrawString("i", new Font("Georgia", 12F, FontStyle.Bold | FontStyle.Italic), Brushes.White, new PointF(iconSize * 0.26f, iconSize * 0.08f));
                    }
                    else if (icon == MessageBoxIcon.Warning || icon == MessageBoxIcon.Exclamation)
                    {
                        PointF[] points = { new PointF(iconSize / 2f, 0), new PointF(0, iconSize - 1), new PointF(iconSize - 1, iconSize - 1) };
                        g.FillPolygon(Brushes.Orange, points);
                        g.DrawString("!", new Font("Segoe UI", 11F, FontStyle.Bold), Brushes.White, new PointF(iconSize * 0.35f, iconSize * 0.18f));
                    }
                }
                picIcon.Image = bmp;
                msgForm.Controls.Add(picIcon);

                // Зменшений відступ тексту від нижнього краю іконки (усього 6 пікселів, масштабованих під DPI)
                textTopOffset = picIcon.Bottom + (int)(6 * currentScale);
            }

            // Налаштування RichTextBox для тексту
            RichTextBox rtbText = new RichTextBox
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

            rtbText.MouseDown += (s, e) => { _ = HideCaret(rtbText.Handle); _ = msgForm.Focus(); };
            rtbText.GotFocus += (s, e) => { _ = HideCaret(rtbText.Handle); };

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
                Button btnOkCustom = new Button
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
                Button btnOkCustom = new Button
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

                Button btnCancelCustom = new Button
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

                msgForm.Controls.AddRange(new Control[] { btnOkCustom, btnCancelCustom });
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
                    IntPtr foregroundWindowHandle = GetForegroundWindow();
                    uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, IntPtr.Zero);
                    uint currentThreadId = GetCurrentThreadId();

                    if (foregroundThreadId != currentThreadId && foregroundThreadId != 0)
                    {
                        _ = AttachThreadInput(currentThreadId, foregroundThreadId, true);
                        _ = SetForegroundWindow(msgFormHandle);
                        _ = SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                        _ = AttachThreadInput(currentThreadId, foregroundThreadId, false);
                    }
                    else
                    {
                        _ = SetForegroundWindow(msgFormHandle);
                        _ = SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                    }
                }
                catch { }

                if (primaryButton != null)
                {
                    _ = primaryButton.Focus();
                }

                _ = msgForm.BeginInvoke(new Action(() => { _ = HideCaret(rtbText.Handle); }));
            };

            return msgForm.ShowDialog();
        }


        private void CmbOutFields_SelectedIndexChanged(int index)
        {
            bool internalCall = _isChangingStates;
            if (!internalCall)
            {
                _isChangingStates = true;
            }

            try
            {
                bool hasSelection = cmbOutFields![index].SelectedIndex > 0;
                chkAsFolder![index].Enabled = hasSelection;
                if (!hasSelection)
                {
                    chkAsFolder[index].Checked = false;
                }

                if (index < 7)
                {
                    if (hasSelection)
                    {
                        cmbOutFields[index + 1].Enabled = true;
                    }
                    else
                    {
                        for (int i = index + 1; i < 8; i++)
                        {
                            cmbOutFields[i].SelectedIndex = 0;
                            cmbOutFields[i].Enabled = false;
                            chkAsFolder[i].Checked = false;
                            chkAsFolder[i].Enabled = false;
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

        private async void BtnDumpConfig_Click(object? sender, EventArgs e)
        {
            if (!YamlService.IsEngineAvailable())
            {
                // 1. Отримуємо локалізований заголовок для помилки ("Error", "Помилка" тощо)
                string caption = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("ErrTitle")
                    ? Config.Localization[Config.Settings.CurrentLanguage]["ErrTitle"]
                    : "Error";

                // 2. Отримуємо локалізований текст про відсутність двигуна fbc.exe
                string text = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("ErrFbc")
                    ? Config.Localization[Config.Settings.CurrentLanguage]["ErrFbc"]
                    : "Error: fbc.exe not found!";

                // 3. Викликаємо наше кастомне вікно з іконкою помилки по центру
                _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnDumpConfig.Enabled = false;
            string prevText = btnDumpConfig.Text;
            btnDumpConfig.Text = Config.Settings.CurrentLanguage == "Ukrainian" ? "Генерація..." : "Generating...";

            bool success = await Task.Run(() => YamlService.ExecuteSyncDumpConfig());

            btnDumpConfig.Text = prevText;
            btnDumpConfig.Enabled = true;

            if (success)
            {
                // 1. Отримуємо локалізований заголовок ("Success", "Успіх" тощо)
                string caption = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("GenTitle")
                    ? Config.Localization[Config.Settings.CurrentLanguage]["GenTitle"]
                    : "Success";

                // 2. Отримуємо локалізоване повідомлення про успішну генерацію
                string msg = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("GenSuccess")
                    ? Config.Localization[Config.Settings.CurrentLanguage]["GenSuccess"]
                    : "config.yaml successfully generated!";

                // 3. Виводимо наше відцентроване кастомне вікно
                _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtGui_Click(object? sender, EventArgs e)
        {
            var runningProcesses = Process.GetProcessesByName("fb2cng_GUI");
            if (runningProcesses.Length > 0)
            {
                // Беремо ПЕРШИЙ знайдений процес із масиву [0]
                IntPtr hWnd = runningProcesses[0].MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    if (IsIconic(hWnd)) ShowWindow(hWnd, 9); // SW_RESTORE
                    SetForegroundWindow(hWnd);
                    return;
                }
            }

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe");
            if (File.Exists(exePath))
            {
                try { Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = true }); }
                catch { ShowGuiMissingError(); }
            }
            else { ShowGuiMissingError(); }
        }

        private void ShowGuiMissingError()
        {
            // 1. Отримуємо локалізований заголовок для помилки ("Помилка конфігурації" тощо)
            string caption = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("ErrTitle")
                ? Config.Localization[Config.Settings.CurrentLanguage]["ErrTitle"]
                : "Configuration Error";

            // 2. Отримуємо локалізований текст про відсутність fb2cng_GUI.exe
            string text = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("ErrGui")
                ? Config.Localization[Config.Settings.CurrentLanguage]["ErrGui"]
                : "GUI program not found: please verify that 'fb2cng_GUI.exe' is present in the application folder!";

            // 3. Викликаємо відцентроване кастомне вікно з іконкою помилки
            _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void SaveYamlConfiguration()
        {
            int[] fieldIndexes = new int[8];
            bool[] folderFlags = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                fieldIndexes[i] = cmbOutFields![i].SelectedIndex;
                folderFlags[i] = chkAsFolder![i].Checked;
            }

            bool saved = YamlService.SaveConfiguration(
                txtConfigName.Text, chkCss.Checked, txtCssPath.Text, chkTranslit.Checked,
                chkReaderSize.Checked, txtWidth.Text, txtHeight.Text, txtDpi.Text,
                chkCover.Checked, cmbCoverMode.SelectedItem?.ToString()!,
                chkNotes.Checked, cmbNotesMode.SelectedItem?.ToString()!,
                chkOpenFromCover.Checked, chkFixZip.Checked, chkFb2Name.Checked,
                fieldIndexes, folderFlags
            );

            if (saved)
            {
                Close();
            }
        }

        private void ShowHelp()
        {
            // 1. Отримуємо локалізований заголовок вікна ("Довідка", "Help" тощо)
            string caption = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("Help")
                ? Config.Localization[Config.Settings.CurrentLanguage]["Help"]
                : "Help / Довідка";

            // 2. Отримуємо локалізований розширений текст довідки
            string msg = Config.Localization.ContainsKey(Config.Settings.CurrentLanguage) && Config.Localization[Config.Settings.CurrentLanguage].ContainsKey("HelpText")
                ? Config.Localization[Config.Settings.CurrentLanguage]["HelpText"]
                : "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.";

            // 3. Викликаємо наше кастомне вікно повідомлення
            _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
