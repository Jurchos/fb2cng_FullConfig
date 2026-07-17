using System.ComponentModel;
using System.Diagnostics;

namespace fb2cng_FullConfig
{
    [DesignerCategory("Code")]

    public partial class Form1
    {
        // Логічні прапорці захисту від зациклювання графічних подій
        private bool _isThemeApplying;
        private bool _isChangingStates;

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
                return loc.TryGetValue(key, out string? value) ? value : defaultText;
            }

            Text = GetText("Title", "fb2cng Configurator");
            lblLang.Text = GetText("Language", "Language:");
            btnDumpConfig.Text = GetText("DumpConfig", "Dump Default Config");
            lblConfigName.Text = GetText("ConfigName", "Config Name:");
            chkCss.Text = GetText("CssEnable", "Use Custom CSS");
            btnBrowseCss.Text = GetText("Browse", "Browse...");

            chkNotes.SetTextIfNotNull(GetText("FootnotesMode", "Footnotes Mode"));
            chkCover.SetTextIfNotNull(GetText("TocType", "Cover Mode"));

            chkReaderSize.Text = GetText("ReaderSize", "Set Custom Display Size");
            lblWidth.Text = GetText("Width", "W:");
            lblHeight.Text = GetText("Height", "H:");
            lblDpi.Text = GetText("Dpi", "DPI:");

            chkFixZip.SetTextIfNotNull(GetText("FixZip", "Fix Broken ZIP Archives"));

            chkOpenFromCover.SetTextIfNotNull(GetText("OpenCover", "Open from Cover"));

            chkFb2Name.Text = GetText("Fb2Name", "Use Original FB2 Name");
            chkTranslit.Text = GetText("Translit", "Transliterate Output Name");

            lblOutNameTitle.SetTextIfNotNull(GetText("OutNameTitle", "Output Name Template Constructor"));

            grpOutName.SetTextIfNotNull(GetText("OutNameTitle", "Output Structure"));

            chkAsFolder.SetTextForAllIfNotNull(GetText("AsFolder", "Fold"));

            string[] itemKeys = ["Item_Empty", "Item_Author", "Item_Series", "Item_Title", "Item_Lang", "Item_Genre", "Item_Date", "Item_Source", "Item_Uuid", "Item_Short_Uuid"];
            string[] defaultItems = ["", "Author", "Series", "Title", "Language", "Genre", "Date", "Source File", "Book UUID", "Shortened UUID"];

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
            Message msgDisable = Message.Create(Handle, WM_SETREDRAW, 0, 0); // 0 = false
            DefWndProc(ref msgDisable); // Надсилаємо його безпосередньо в ОС
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
                Message msgEnable = Message.Create(Handle, WM_SETREDRAW, 1, 0); // 1 = true
                DefWndProc(ref msgEnable);

                // Примусово оновлюємо інтерфейс форми після увімкнення рендерингу
                Invalidate(true);
                Update();

                // Примушуємо ОС перерендерити вікно та всі дочірні елементи знизу-вгору одним кадром
                Refresh();

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
                using SolidBrush bgBrush = new(Color.FromArgb(45, 45, 48));
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

            using OpenFileDialog ofd = new();
            ofd.Filter = "CSS Files (*.css)|*.css";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string selectedFile = ofd.FileName;
                string relativePath = Path.GetRelativePath(appPath, selectedFile); // 1. Безпечно отримуємо відносний шлях засобами .NET
                txtCssPath.Text = relativePath.Replace('\\', '/');                 // 2. Оптимально замінюємо Windows-слеші на зворотні слеші (використовуючи char '')
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
            // Ігноруємо bool-результат через '_ =', залишаючи лише отриманий out Dictionary<string, string>? langDict
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            if (!YamlService.IsEngineAvailable())
            {
                string caption = langDict?.GetValueOrDefault("ErrTitle", "Error") ?? "Error";
                string text = langDict?.GetValueOrDefault("ErrFbc", "Error: fbc.exe not found!") ?? "Error: fbc.exe not found!";

                // Ігноруємо DialogResult вікна через '_ ='
                _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnDumpConfig.Enabled = false;
            string prevText = btnDumpConfig.Text;
            btnDumpConfig.Text = Config.Settings.CurrentLanguage == "Ukrainian" ? "Генерація..." : "Generating...";

            bool success = await Task.Run(YamlService.ExecuteSyncDumpConfig);

            btnDumpConfig.Text = prevText;
            btnDumpConfig.Enabled = true;

            if (success)
            {
                string caption = langDict?.GetValueOrDefault("GenTitle", "Success") ?? "Success";
                string msg = langDict?.GetValueOrDefault("GenSuccess", "config.yaml successfully generated!") ?? "config.yaml successfully generated!";

                // Ігноруємо DialogResult вікна через '_ ='
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
                    if (Win32Api.IsIconic(hWnd))
                    {
                        _ = Win32Api.ShowWindow(hWnd, 9); // SW_RESTORE
                    }

                    _ = Win32Api.SetForegroundWindow(hWnd);
                    return;
                }
            }

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe"); // чи fbc.exe

            // 1. Повертаємо залізобетонну перевірку існування файлу
            if (File.Exists(exePath))
            {
                try
                {
                    // 2. Додаємо '_ =', щоб задовольнити аналізатор Visual Studio
                    _ = Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                }
                catch (Exception)
                {
                    // На випадок, якщо файл є, але заблокований антивірусом чи системою
                    ShowGuiMissingError();
                }
            }
            else
            {
                // 3. Якщо файлу фізично немає на диску — гарантовано викликаємо ваше вікно
                ShowGuiMissingError();
            }
        }

        private void ShowGuiMissingError()
        {
            // 1. Отримуємо словник для поточної мови один раз із явним визначенням типу
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            // 2. Безпечно дістаємо значення або беремо англійський дефолт
            string caption = langDict?.GetValueOrDefault("ErrTitle", "Configuration Error") ?? "Configuration Error";
            string text = langDict?.GetValueOrDefault("ErrGui", "GUI program not found: please verify that 'fb2cng_GUI.exe' is present in the application folder!") ?? "GUI program not found: please verify that 'fb2cng_GUI.exe' is present in the application folder!";

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
            // 1. Отримуємо словник для поточної мови один раз із явним визначенням типу
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            // 2. Безпечно дістаємо локалізований заголовок вікна або беремо дефолт
            string caption = langDict?.GetValueOrDefault("Help", "Help / Довідка") ?? "Help / Довідка";

            // 3. Безпечно отримуємо розширений текст довідки
            string msg = langDict?.GetValueOrDefault("HelpText", "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.") ?? "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.";

            // 4. Викликаємо наше кастомне вікно повідомлення
            _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    public static class ControlExtensions // Розширення для перевірки null перед
    {
        public static void SetTextIfNotNull(this Control? control, string text)
        {
            if (control == null)
            {
                return;
            }
            control.Text = text;
        }
        // Новий метод для масивів/списків контролів
        public static void SetTextForAllIfNotNull(this IEnumerable<Control?>? controls, string text)
        {
            if (controls == null)
            {
                return;
            }

            foreach (Control? control in controls)
            {
                control.SetTextIfNotNull(text); // Викликаємо існуючий метод, він сам перевірить null
            }
        }
    }
}
