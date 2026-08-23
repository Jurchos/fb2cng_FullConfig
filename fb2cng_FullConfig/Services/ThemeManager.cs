
namespace fb2cng_FullConfig.Services
{
    // Інтерфейс для вкладок, які мають унікальні елементи для фарбування
    public interface IThemableTab
    {
        void ApplyTheme(bool isDark, Color foreColor, Color backColor, Color disabledColor);
    }
    public static class ThemeManager
    {
        private static bool _isThemeApplying;

        public static void Apply(Form form, Panel header, Panel footer, Panel content)
        {
            if (_isThemeApplying) return;
            _isThemeApplying = true;

            form.SuspendLayout();
            try
            {
                bool isDark = Config.IsDarkTheme;
                // Визначення палітри
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

                // 1. Фарбування основного фону вікна
                form.BackColor = isDark ? darkBg : SystemColors.Control;
                header.BackColor = isDark ? elementBg : SystemColors.ControlLight;
                footer.BackColor = isDark ? elementBg : SystemColors.ControlLight;

                // 2. Фарбування статичних панелей (Header/Footer)
                SetControlsThemeRecursive(header, foreColor, disabledColor, backColor, folderColor, isDark);
                SetControlsThemeRecursive(footer, foreColor, disabledColor, backColor, folderColor, isDark);

                // 3. Фарбування динамічного контенту (вкладок)
                foreach (Control activeTab in content.Controls)
                {
                    activeTab.BackColor = isDark ? darkBg : SystemColors.Window;

                    // Викликаємо індивідуальне фарбування таба, якщо він підтримує інтерфейс
                    if (activeTab is IThemableTab customTab)
                    {
                        customTab.ApplyTheme(isDark, foreColor, backColor, disabledColor);
                    }

                    SetControlsThemeRecursive(activeTab, foreColor, disabledColor, backColor, folderColor, isDark);
                }
            }
            finally
            {
                form.ResumeLayout(true);
                _isThemeApplying = false;
            }
        }

        // --- Рекурсивний обхід контролів ---
        private static void SetControlsThemeRecursive(Control parent, Color foreColor, Color disabledColor, Color backColor, Color folderColor, bool isDark)
        {
            foreach (Control c in parent.Controls)
            {
                // Визначаємо стан заблокованості чисто за властивістю Enabled самого контрола
                bool isControlDisabled = !c.Enabled;

                switch (c)
                {
                    case GroupBox gb:
                        gb.BackColor = parent.BackColor;

                        // ПЕРЕВІРКА: якщо є мітка ForceDisabled або контроль реально вимкнено
                        if (gb.Tag?.ToString() == "ForceDisabled" || !gb.Enabled)
                        {
                            gb.ForeColor = disabledColor; // Тут буде наш правильний сірий (140, 140, 140)
                        }
                        else
                        {
                            gb.ForeColor = isDark ? foreColor : SystemColors.ControlText;
                        }

                        gb.Invalidate();
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
                        if (isDark)
                        {
                            btn.ForeColor = btn.Enabled ? foreColor : disabledColor;
                            btn.BackColor = btn.Enabled ? Color.FromArgb(45, 45, 48) : Color.FromArgb(40, 40, 42);
                        }
                        else
                        {
                            btn.ForeColor = btn.Enabled ? SystemColors.ControlText : disabledColor;
                            btn.BackColor = SystemColors.Control;
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

                        if (cb.DrawMode != (isDark ? DrawMode.OwnerDrawFixed : DrawMode.Normal))
                            cb.DrawMode = isDark ? DrawMode.OwnerDrawFixed : DrawMode.Normal;

                        cb.DrawItem -= ComboBox_DrawItem;
                        if (isDark) cb.DrawItem += ComboBox_DrawItem;
                        break;

                    case CheckedListBox clb:
                        clb.BackColor = backColor;
                        clb.ForeColor = isControlDisabled ? disabledColor : foreColor;
                        break;
                }

                if (c.HasChildren) SetControlsThemeRecursive(c, foreColor, disabledColor, backColor, folderColor, isDark);
            }
        }

        // --- Малювання ComboBox (Темна тема) ---
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