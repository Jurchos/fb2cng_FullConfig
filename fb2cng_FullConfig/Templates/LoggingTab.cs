using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig.Templates
{
    public partial class LoggingTab : UserControl
    {
        // Чекбокси та елементи вибору для Logging
        public CheckBox chkLogLevel = null!;
        public ComboBox cmbLogLevel = null!;

        public CheckBox chkLogName = null!;
        public ComboBox cmbLogName = null!;

        public CheckBox chkPanicLogName = null!;
        public ComboBox cmbPanicLogName = null!;

        public CheckBox chkLogMode = null!;
        public RadioButton rbLogModeOnlyNew = null!, rbLogModeOldNew = null!;

        public CheckBox chkLogFolder = null!;
        public RadioButton rbLogFolderYes = null!, rbLogFolderNo = null!;

        public LoggingTab()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetupInterface();
        }

        private void SetupInterface()
        {
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // 1. Рівень логування
            chkLogLevel = new CheckBox { AutoSize = true };
            cmbLogLevel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbLogLevel.Items.AddRange(["none", "normal", "debug"]);
            cmbLogLevel.SelectedIndex = 2; // "debug" за замовчуванням
            chkLogLevel.CheckedChanged += (s, e) =>
            {
                cmbLogLevel.Enabled = chkLogLevel.Checked;
                ApplyThemeViaForm();
            };

            // 2. Назва звичайних логів
            chkLogName = new CheckBox { AutoSize = true };
            cmbLogName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbLogName.Items.AddRange(["", "", "", ""]);
            cmbLogName.SelectedIndex = 0;
            chkLogName.CheckedChanged += (s, e) =>
            {
                cmbLogName.Enabled = chkLogName.Checked;
                ApplyThemeViaForm();
            };

            // 3. Назва логів збоїв (Panic)
            chkPanicLogName = new CheckBox { AutoSize = true };
            cmbPanicLogName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbPanicLogName.Items.AddRange(["", "", "", ""]);
            cmbPanicLogName.SelectedIndex = 0;
            chkPanicLogName.CheckedChanged += (s, e) =>
            {
                cmbPanicLogName.Enabled = chkPanicLogName.Checked;
                ApplyThemeViaForm();
            };

            // 4. Режим логування
            chkLogMode = new CheckBox { AutoSize = true };
            Panel pnlLogMode = UiStyles.CreateRadioGroup(out rbLogModeOnlyNew, out rbLogModeOldNew);
            rbLogModeOnlyNew.Checked = true;
            chkLogMode.CheckedChanged += (s, e) =>
            {
                pnlLogMode.Enabled = chkLogMode.Checked;
                ApplyThemeViaForm();
            };

            // 5. Папка для логів
            chkLogFolder = new CheckBox { AutoSize = true };
            Panel pnlLogFolder = UiStyles.CreateRadioGroup(out rbLogFolderYes, out rbLogFolderNo);
            rbLogFolderNo.Checked = true;
            chkLogFolder.CheckedChanged += (s, e) =>
            {
                pnlLogFolder.Enabled = chkLogFolder.Checked;
                ApplyThemeViaForm();
            };

            Controls.AddRange([
                chkLogLevel, cmbLogLevel,
                chkLogName, cmbLogName,
                chkPanicLogName, cmbPanicLogName,
                chkLogMode, pnlLogMode,
                chkLogFolder, pnlLogFolder
            ]);

            // Геометрія розставлення
            int nextY = m.StartY;

            // 1. Log Level
            chkLogLevel.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbLogLevel.ItemHeight = m.FieldHeight - UiStyles.GetScaled(6);
            cmbLogLevel.SetBounds(m.XLeft + m.TextLabelWidth, nextY, m.ValueFieldWidth, m.FieldHeight);

            // 2. Log Name
            nextY = cmbLogLevel.Bottom + m.BlockMargin;
            chkLogName.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbLogName.ItemHeight = m.FieldHeight - UiStyles.GetScaled(6);
            cmbLogName.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            // 3. Panic Log Name
            nextY = cmbLogName.Bottom + m.BlockMargin;
            chkPanicLogName.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbPanicLogName.ItemHeight = m.FieldHeight - UiStyles.GetScaled(6);
            cmbPanicLogName.SetBounds(m.XLeft + m.TextLabelWidth, nextY, m.ValueFieldWidth, m.FieldHeight);

            // 4. Log Mode
            nextY = cmbPanicLogName.Bottom + m.BlockMargin;
            chkLogMode.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlLogMode.SetBounds(m.SizeInputX, nextY, UiStyles.GetScaled(180), m.FieldHeight);

            // 5. Log Folder
            int bigBlockMargin = m.BlockMargin + UiStyles.GetScaled(8);
            nextY = chkLogMode.Bottom + bigBlockMargin;
            chkLogFolder.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlLogFolder.SetBounds(m.SizeInputX, nextY, UiStyles.GetScaled(180), m.FieldHeight);
        }
        private void ApplyThemeViaForm()
        {
            if (FindForm() is Form1 mainForm)
            {
                mainForm.ApplyTheme();
            }
        }
    }
}