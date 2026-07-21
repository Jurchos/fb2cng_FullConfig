namespace fb2cng_FullConfig.Templates
{

    public partial class MetadataTab : UserControl
    {
        public CheckBox chkReaderSize = null!;
        public Label lblWidth = null!, lblHeight = null!, lblDpi = null!;
        public TextBox txtWidth = null!, txtHeight = null!, txtDpi = null!;
        public CheckBox chkNotes = null!;
        public ComboBox cmbNotesMode = null!;

        public MetadataTab()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetupInterface();
        }

        private void SetupInterface()
        {
            float currentScale = Win32Api.GetDpiScale();

            int blockMargin = (int)(16 * currentScale);// Відстань між блоками
            int labelHeight = (int)(20 * currentScale);
            int fieldHeight = (int)(24 * currentScale);
            int checkBoxHeight = (int)(22 * currentScale);
            int xLeft = (int)(16 * currentScale);
            int textLabelWidth = (int)(240 * currentScale);    // Фіксована ширина під написи ліворуч
            int valueFieldWidth = textLabelWidth - (int)(5 * currentScale); // Ширина поля праворуч

            chkReaderSize = new CheckBox { AutoSize = true };
            lblWidth = new Label { Text = "W:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtWidth = new TextBox { Text = "1264", Enabled = false, Multiline = true };
            lblHeight = new Label { Text = "H:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtHeight = new TextBox { Text = "1680", Enabled = false, Multiline = true };
            lblDpi = new Label { Text = "DPI:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            txtDpi = new TextBox { Text = "300", Enabled = false, Multiline = true };

            chkReaderSize.CheckedChanged += (s, e) =>
            {
                bool en = chkReaderSize.Checked;
                lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = en;
                (ParentForm as Form1)?.ApplyTheme();
            };

            Controls.AddRange([chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi]);

            int nextY = (int)(11 * currentScale);
            // Виправлено SetBounds (ширина має бути більшою за висоту)
            chkReaderSize.SetBounds(xLeft, nextY + (int)(1 * currentScale), (int)(300 * currentScale), checkBoxHeight);

            int labelWidthSpace = (int)(22 * currentScale);
            int exactBoxWidth = (int)(44 * currentScale);
            int betweenGroupsSpacing = (int)(10 * currentScale);

            int sizeInputX = xLeft + textLabelWidth;

            // 1. Блок Width
            int wLabelWidth = labelWidthSpace + (int)(4 * currentScale);
            lblWidth.SetBounds(sizeInputX, nextY + (int)(2 * currentScale), wLabelWidth, labelHeight);
            txtWidth.SetBounds(lblWidth.Right, nextY, exactBoxWidth, fieldHeight);

            // 2. Блок Height
            lblHeight.SetBounds(txtWidth.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), labelWidthSpace, labelHeight);
            txtHeight.SetBounds(lblHeight.Right, nextY, exactBoxWidth, fieldHeight);

            // 3. Блок DPI
            int dpiLabelWidth = labelWidthSpace + (int)(12 * currentScale);
            lblDpi.SetBounds(txtHeight.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), dpiLabelWidth, labelHeight);
            txtDpi.SetBounds(lblDpi.Right, nextY, exactBoxWidth, fieldHeight);

            // Контроли виносок
            chkNotes = new CheckBox { AutoSize = true };
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
            cmbNotesMode.SelectedIndex = 0;

            // Логіка активації
            chkNotes.CheckedChanged += (s, e) =>
            {
                cmbNotesMode.Enabled = chkNotes.Checked;
                (ParentForm as Form1)?.ApplyTheme();
            };

            Controls.AddRange([chkNotes, cmbNotesMode]);

            nextY = chkReaderSize.Bottom + blockMargin;
            chkNotes.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            cmbNotesMode.ItemHeight = fieldHeight - 6;
            cmbNotesMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);
        }
    }
}