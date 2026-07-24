
using fb2cng_FullConfig.Templates;

namespace fb2cng_FullConfig.Services
{
    public static class ThemeManager
    {
        private static bool _isThemeApplying;
        private static bool _isFirstLaunchApplied;

        public static void Apply(Form form, Panel header, Panel footer, Panel content, Dictionary<string, UserControl> tabsCache)
        {
            if (_isThemeApplying)
            {
                return;
            }

            _isThemeApplying = true;

            form.SuspendLayout();
            try
            {
                bool isDark = Config.IsDarkTheme;
                Color darkBg = Color.FromArgb(37, 37, 38);
                Color elementBg = Color.FromArgb(45, 45, 48);
                Color textWhite = Color.FromArgb(245, 245, 245);
                Color textGray = Color.FromArgb(140, 140, 140);
                Color limeAccent = Color.Lime;

                // Виносимо загальні кольори, щоб не дублювати в параметрах
                Color foreColor = isDark ? textWhite : SystemColors.ControlText;
                Color disabledColor = isDark ? textGray : SystemColors.GrayText;
                Color backColor = isDark ? elementBg : SystemColors.Window;
                Color folderColor = isDark ? limeAccent : SystemColors.HotTrack;

                // Основний фон вікна
                form.BackColor = isDark ? darkBg : SystemColors.Control;

                header.BackColor = isDark ? elementBg : SystemColors.ControlLight;
                footer.BackColor = isDark ? elementBg : SystemColors.ControlLight;

                // Фарбуємо Хідер та Футер
                SetControlsTheme(header, foreColor, disabledColor, backColor, folderColor, isDark, tabsCache);
                SetControlsTheme(footer, foreColor, disabledColor, backColor, folderColor, isDark, tabsCache);

                // Фарбуємо всі закешовані вкладки
                foreach (Control activeTab in content.Controls)
                {
                    activeTab.BackColor = isDark ? darkBg : SystemColors.Window;

                    if (activeTab is DocumentTab docTab)
                    {
                        docTab.scrollMenuPanel.BackColor = activeTab.BackColor;
                        docTab.grpOutName.BackColor = activeTab.BackColor;
                    }

                    SetControlsTheme(activeTab, foreColor, disabledColor, backColor, folderColor, isDark, tabsCache);
                }
                if (!_isFirstLaunchApplied && tabsCache.TryGetValue("document:", out UserControl? mainTab) && mainTab is DocumentTab documentTab)
                {
                    if (!documentTab.chkCustomYaml.Checked)
                    {
                        documentTab.chkCustomYaml.Checked = true;
                        documentTab.chkCustomYaml.Checked = false;
                    }
                    // Кажемо програмі: "Перший запуск оброблено, більше сюди не заходь!"
                    _isFirstLaunchApplied = true;
                }
            }
            finally
            {
                form.ResumeLayout(true);
                _isThemeApplying = false;
            }
        }

        private static void SetControlsTheme(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor, bool isDark, Dictionary<string, UserControl> tabsCache)
        {
            DocumentTab? docTab = tabsCache.TryGetValue("document:", out UserControl? tab) ? tab as DocumentTab : null;

            bool isFb2NameChecked = docTab?.chkFb2Name.Checked ?? false;
            bool isDefaultNameChecked = docTab?.chkDefaultName.Checked ?? false;
            bool isNamingLocked = isFb2NameChecked || isDefaultNameChecked;
            bool isCssChecked = docTab?.chkCss.Checked ?? false;

            SetControlsThemeRecursive(parent, foreColor, disabledColor, backColor, folderColor, isDark, docTab, isNamingLocked, isCssChecked);
        }

        private static void SetControlsThemeRecursive(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor,
            bool isDark, DocumentTab? docTab, bool isNamingLocked, bool isCssChecked)
        {
            Control? currentBrowseCssBtn = docTab?.btnBrowseCss;
            Control? currentGrpOutName = docTab?.grpOutName;

            // Оптимізація: Обчислюємо стан GrpOutName один раз перед циклом, а не для кожного контролу
            bool isOutNameDisabled = isNamingLocked || (currentGrpOutName != null && !currentGrpOutName.Enabled);

            foreach (Control c in parent.Controls)
            {
                // Оптимізація: Перевірку батьківських елементів робимо через локальні змінні, уникаючи глибоких null-conditional перевірок у циклі
                Control? p = c.Parent;
                bool isInsideGrpOutName = currentGrpOutName != null && (p == currentGrpOutName || (p != null && p.Parent == currentGrpOutName));

                bool isControlDisabled = !c.Enabled
                    || (isInsideGrpOutName && isOutNameDisabled)
                    || (isDark && c == currentBrowseCssBtn && !isCssChecked);

                // Використовуємо Pattern Matching для чистоти та швидкості
                switch (c)
                {
                    case GroupBox gb:
                        gb.BackColor = parent.BackColor;
                        gb.ForeColor = isNamingLocked && gb.Name == "grpOutName" ? disabledColor : (isDark ? foreColor : SystemColors.ControlText);
                        break;

                    case Label lbl:
                        lbl.ForeColor = isControlDisabled ? disabledColor : foreColor;
                        lbl.BackColor = Color.Transparent;
                        break;

                    case CheckBox chk:
                        chk.ForeColor = !isControlDisabled && chk.Tag?.ToString() == "FolderCheckBox" ? folderColor : (isControlDisabled ? disabledColor : foreColor);
                        chk.BackColor = Color.Transparent;
                        break;

                    case TextBox txt:
                        txt.BackColor = backColor;
                        txt.ForeColor = isControlDisabled ? disabledColor : foreColor;
                        break;

                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;

                        bool isBrowseBtn = (docTab != null) && (btn == docTab.btnBrowseCss || btn == docTab.btnBrowseCustomYaml);
                        bool isParentChecked = (docTab != null) && (
                            (btn == docTab.btnBrowseCss && isCssChecked) ||
                            (btn == docTab.btnBrowseCustomYaml && docTab.chkCustomYaml.Checked)
                        );

                        if (isDark)
                        {
                            btn.BackColor = (isBrowseBtn && !isParentChecked) ? Color.FromArgb(40, 40, 42) : Color.FromArgb(45, 45, 48);
                            btn.ForeColor = (isBrowseBtn && !isParentChecked) ? disabledColor : foreColor;
                            btn.FlatAppearance.BorderColor = (isBrowseBtn && !isParentChecked) ? Color.FromArgb(55, 55, 58) : Color.FromArgb(100, 100, 105);
                        }
                        else
                        {
                            btn.BackColor = SystemColors.Control;
                            btn.ForeColor = (isBrowseBtn && !isParentChecked) ? disabledColor : SystemColors.ControlText;
                            btn.FlatAppearance.BorderColor = (isBrowseBtn && !isParentChecked) ? Color.LightGray : Color.DarkGray;
                        }
                        break;

                    case RadioButton rb:
                        rb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                        rb.BackColor = Color.Transparent;
                        break;

                    case ComboBox cb:
                        cb.BackColor = backColor;
                        cb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                        cb.FlatStyle = isDark ? FlatStyle.Flat : FlatStyle.Standard;

                        // Безпечне перемикання режимів малювання без дублювання подій
                        DrawMode targetMode = isDark ? DrawMode.OwnerDrawFixed : DrawMode.Normal;
                        if (cb.DrawMode != targetMode)
                        {
                            cb.DrawMode = targetMode;
                        }

                        // Перепідписуємо подію лише для темної теми
                        cb.DrawItem -= ComboBox_DrawItem;
                        if (isDark)
                        {
                            cb.DrawItem += ComboBox_DrawItem;
                        }
                        break;

                    default:
                        break;
                }

                if (c.HasChildren)
                {
                    SetControlsThemeRecursive(c, foreColor, disabledColor, backColor, folderColor, isDark, docTab, isNamingLocked, isCssChecked);
                }
            }
        }

        private static void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || sender is not ComboBox cb)
            {
                return;
            }
            bool isControlDisabled = !cb.Enabled;
            e.DrawBackground();
            // Задаємо колір фону для заблокованого стану залежно від поточної теми
            Color drawTextColor = isControlDisabled ? Color.FromArgb(140, 140, 140) : cb.ForeColor;
            if (isControlDisabled)
            {
                using SolidBrush bgBrush = new(Config.IsDarkTheme ? Color.FromArgb(45, 45, 48) : SystemColors.Control);
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
    }
}