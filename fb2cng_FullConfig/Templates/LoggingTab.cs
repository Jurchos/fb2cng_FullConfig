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
            float currentScale = Win32Api.GetDpiScale();

            int blockMargin = (int)(12 * currentScale);
            int fieldHeight = (int)(24 * currentScale);
            int checkBoxHeight = (int)(22 * currentScale);
            int xLeft = (int)(16 * currentScale);
            int textLabelWidth = (int)(240 * currentScale);
            int valueFieldWidth = textLabelWidth - (int)(5 * currentScale);
            int radioX = xLeft + textLabelWidth + (int)(5 * currentScale);

            static Panel CreateRadioGroup(out RadioButton rb1, string text1, out RadioButton rb2, string text2, float scale)
            {
                Panel p = new() { AutoSize = true, Enabled = false };
                rb1 = new RadioButton { AutoSize = true, Location = new Point(0, 0), Text = text1 };
                rb2 = new RadioButton { AutoSize = true, Location = new Point((int)(85 * scale), 0), Text = text2 };
                p.Controls.AddRange([rb1, rb2]);
                return p;
            }

            // 1. Рівень логування
            chkLogLevel = new CheckBox { AutoSize = true };
            cmbLogLevel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbLogLevel.Items.AddRange(["none", "normal", "debug"]);
            cmbLogLevel.SelectedIndex = 2; // "debug" за замовчуванням
            chkLogLevel.CheckedChanged += (s, e) =>
            {
                cmbLogLevel.Enabled = chkLogLevel.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            // 2. Назва звичайних логів
            chkLogName = new CheckBox { AutoSize = true };
            cmbLogName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbLogName.Items.AddRange(["", "", "", ""]);
            cmbLogName.SelectedIndex = 0;
            chkLogName.CheckedChanged += (s, e) =>
            {
                cmbLogName.Enabled = chkLogName.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            // 3. Назва логів збоїв (Panic)
            chkPanicLogName = new CheckBox { AutoSize = true };
            cmbPanicLogName = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbPanicLogName.Items.AddRange(["", "", "", ""]);
            cmbPanicLogName.SelectedIndex = 0;
            chkPanicLogName.CheckedChanged += (s, e) =>
            {
                cmbPanicLogName.Enabled = chkPanicLogName.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            // 4. Режим логування
            chkLogMode = new CheckBox { AutoSize = true };
            Panel pnlLogMode = CreateRadioGroup(out rbLogModeOnlyNew, "only_new", out rbLogModeOldNew, "old+new", currentScale);
            rbLogModeOnlyNew.Checked = true;
            chkLogMode.CheckedChanged += (s, e) =>
            {
                pnlLogMode.Enabled = chkLogMode.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            // 5. Папка для логів
            chkLogFolder = new CheckBox { AutoSize = true };
            Panel pnlLogFolder = CreateRadioGroup(out rbLogFolderYes, "Так", out rbLogFolderNo, "Ні", currentScale);
            rbLogFolderNo.Checked = true;
            chkLogFolder.CheckedChanged += (s, e) =>
            {
                pnlLogFolder.Enabled = chkLogFolder.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            Controls.AddRange([
                chkLogLevel, cmbLogLevel,
                chkLogName, cmbLogName,
                chkPanicLogName, cmbPanicLogName,
                chkLogMode, pnlLogMode,
                chkLogFolder, pnlLogFolder
            ]);

            // Геометрія розставлення
            int nextY = (int)(11 * currentScale);

            // 1. Log Level
            chkLogLevel.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbLogLevel.ItemHeight = fieldHeight - 6;
            cmbLogLevel.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 2. Log Name
            nextY = cmbLogLevel.Bottom + blockMargin;
            chkLogName.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbLogName.ItemHeight = fieldHeight - 6;
            cmbLogName.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 3. Panic Log Name
            nextY = cmbLogName.Bottom + blockMargin;
            chkPanicLogName.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbPanicLogName.ItemHeight = fieldHeight - 6;
            cmbPanicLogName.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            // 4. Log Mode
            nextY = cmbPanicLogName.Bottom + blockMargin;
            chkLogMode.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            pnlLogMode.SetBounds(radioX, nextY, (int)(180 * currentScale), fieldHeight);

            // 5. Log Folder
            int bigBlockMargin = blockMargin + (int)(8 * currentScale);
            nextY = chkLogMode.Bottom + bigBlockMargin;
            chkLogFolder.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            pnlLogFolder.SetBounds(radioX, nextY, (int)(180 * currentScale), fieldHeight);
        }
    }
}