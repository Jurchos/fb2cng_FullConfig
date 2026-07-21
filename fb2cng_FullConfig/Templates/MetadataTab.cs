namespace fb2cng_FullConfig.Templates
{
    public partial class MetadataTab : UserControl
    {
        public Panel scrollMetadataPanel = null!;

        // Елементи інтерфейсу
        public CheckBox chkReaderSize = null!;
        public Label lblWidth = null!, lblHeight = null!, lblDpi = null!;
        public TextBox txtWidth = null!, txtHeight = null!, txtDpi = null!;

        public CheckBox chkNotes = null!;
        public ComboBox cmbNotesMode = null!;

        public CheckBox chkSoftHyphen = null!;
        public RadioButton rbSoftHyphenYes = null!, rbSoftHyphenNo = null!;

        public CheckBox chkRemoveTransp = null!;
        public RadioButton rbRemoveTranspYes = null!, rbRemoveTranspNo = null!;

        public CheckBox chkJpegQuality = null!;
        public TextBox txtJpegQuality = null!;

        public CheckBox chkGenerateCover = null!;
        public RadioButton rbGenCoverYes = null!, rbGenCoverNo = null!;
        public TextBox txtCoverPath = null!;
        public Button btnBrowseCover = null!;

        public CheckBox chkResizeCover = null!;
        public ComboBox cmbResizeCover = null!;

        public CheckBox chkAnnEnable = null!;
        public RadioButton rbAnnEnableYes = null!, rbAnnEnableNo = null!;

        public CheckBox chkAnnInToc = null!;
        public RadioButton rbAnnInTocYes = null!, rbAnnInTocNo = null!;

        public CheckBox chkTocPlacement = null!;
        public ComboBox cmbTocPlacement = null!;

        public CheckBox chkDropcaps = null!;
        public RadioButton rbDropcapsYes = null!, rbDropcapsNo = null!;

        public MetadataTab()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetupInterface();
        }

        private void SetupInterface()
        {
            float currentScale = Win32Api.GetDpiScale();
            int xLeft = (int)(16 * currentScale);
            int rowHeight = (int)(28 * currentScale);
            int textLabelWidth = (int)(240 * currentScale);
            int valueFieldWidth = (int)(240 * currentScale);
            int radioX = xLeft + textLabelWidth + (int)(5 * currentScale);
            int browseBtnWidth = (int)(55 * currentScale);
            int fieldHeight = (int)(24 * currentScale);
            int checkBoxHeight = (int)(22 * currentScale);

            scrollMetadataPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            Controls.Add(scrollMetadataPanel);

            // Хелпер для створення груп радіо-кнопок
            static Panel CreateRadioGroup(out RadioButton rbYes, out RadioButton rbNo, float scale)
            {
                Panel p = new() { AutoSize = true, Enabled = false };
                rbYes = new RadioButton { AutoSize = true, Location = new Point(0, 0), Text = "Yes" };
                rbNo = new RadioButton { AutoSize = true, Location = new Point((int)(65 * scale), 0), Text = "No" };
                p.Controls.AddRange([rbYes, rbNo]);
                return p;
            }

            int nextY = (int)(11 * currentScale);

            // 1. Reader Size
            chkReaderSize = new CheckBox { AutoSize = true };
            chkReaderSize.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);

            int sizeX = xLeft + textLabelWidth + (int)(5 * currentScale);
            int boxW = (int)(44 * currentScale);
            int lblW = (int)(20 * currentScale);

            lblWidth = new Label { Text = "W:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            lblWidth.SetBounds(sizeX, nextY, lblW, fieldHeight);
            txtWidth = new TextBox { Text = "1264", Enabled = false };
            txtWidth.SetBounds(lblWidth.Right + 2, nextY, boxW, fieldHeight);

            lblHeight = new Label { Text = "H:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            lblHeight.SetBounds(txtWidth.Right + (int)(10 * currentScale), nextY, lblW, fieldHeight);
            txtHeight = new TextBox { Text = "1680", Enabled = false };
            txtHeight.SetBounds(lblHeight.Right + 2, nextY, boxW, fieldHeight);

            lblDpi = new Label { Text = "DPI:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            lblDpi.SetBounds(txtHeight.Right + (int)(10 * currentScale), nextY, (int)(32 * currentScale), fieldHeight);
            txtDpi = new TextBox { Text = "300", Enabled = false };
            txtDpi.SetBounds(lblDpi.Right + 2, nextY, boxW, fieldHeight);

            // 2. Footnotes
            nextY += rowHeight;
            chkNotes = new CheckBox { AutoSize = true };
            chkNotes.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
            cmbNotesMode.SelectedIndex = 0;
            cmbNotesMode.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 3. Soft Hyphen
            nextY += rowHeight;
            chkSoftHyphen = new CheckBox { AutoSize = true };
            chkSoftHyphen.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlSH = CreateRadioGroup(out rbSoftHyphenYes, out rbSoftHyphenNo, currentScale);
            pnlSH.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 4. Transparency
            nextY += rowHeight;
            chkRemoveTransp = new CheckBox { AutoSize = true };
            chkRemoveTransp.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlRT = CreateRadioGroup(out rbRemoveTranspYes, out rbRemoveTranspNo, currentScale);
            pnlRT.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 5. JPEG Quality
            nextY += rowHeight;
            chkJpegQuality = new CheckBox { AutoSize = true };
            chkJpegQuality.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            txtJpegQuality = new TextBox { Text = "95", Enabled = false };
            txtJpegQuality.SetBounds(radioX, nextY, boxW, fieldHeight);

            // 6. Generate Cover
            nextY += rowHeight;
            chkGenerateCover = new CheckBox { AutoSize = true };
            chkGenerateCover.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlGC = CreateRadioGroup(out rbGenCoverYes, out rbGenCoverNo, currentScale);
            pnlGC.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 7. Cover Path
            nextY += rowHeight;
            txtCoverPath = new TextBox { Enabled = false };
            int coverTxtWidth = valueFieldWidth - browseBtnWidth - (int)(5 * currentScale);
            txtCoverPath.SetBounds(radioX, nextY, coverTxtWidth, fieldHeight);
            btnBrowseCover = new Button { FlatStyle = FlatStyle.Flat, Text = "", Enabled = false };
            btnBrowseCover.SetBounds(txtCoverPath.Right + 5, nextY, browseBtnWidth, fieldHeight);

            // 8. Resize Mode
            nextY += rowHeight;
            chkResizeCover = new CheckBox { AutoSize = true };
            chkResizeCover.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            cmbResizeCover = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbResizeCover.Items.AddRange(["none", "keepAR", "stretch"]);
            cmbResizeCover.SelectedIndex = 2;
            cmbResizeCover.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 9. Annotation Enable
            nextY += rowHeight;
            chkAnnEnable = new CheckBox { AutoSize = true };
            chkAnnEnable.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlAE = CreateRadioGroup(out rbAnnEnableYes, out rbAnnEnableNo, currentScale);
            pnlAE.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 10. Annotation In TOC
            nextY += rowHeight;
            chkAnnInToc = new CheckBox { AutoSize = true };
            chkAnnInToc.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlAIT = CreateRadioGroup(out rbAnnInTocYes, out rbAnnInTocNo, currentScale);
            pnlAIT.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 11. TOC Placement
            nextY += rowHeight;
            chkTocPlacement = new CheckBox { AutoSize = true };
            chkTocPlacement.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            cmbTocPlacement = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbTocPlacement.Items.AddRange(["none", "before", "after"]);
            cmbTocPlacement.SelectedIndex = 0;
            cmbTocPlacement.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // 12. Dropcaps
            nextY += rowHeight;
            chkDropcaps = new CheckBox { AutoSize = true };
            chkDropcaps.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            Panel pnlDC = CreateRadioGroup(out rbDropcapsYes, out rbDropcapsNo, currentScale);
            pnlDC.SetBounds(radioX, nextY, valueFieldWidth, fieldHeight);

            // ЛОГІКА АКТИВАЦІЇ ПОЛІВ
            chkReaderSize.CheckedChanged += (s, e) => {
                lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = chkReaderSize.Checked;
            };
            chkNotes.CheckedChanged += (s, e) => cmbNotesMode.Enabled = chkNotes.Checked;
            chkSoftHyphen.CheckedChanged += (s, e) => pnlSH.Enabled = chkSoftHyphen.Checked;
            chkRemoveTransp.CheckedChanged += (s, e) => pnlRT.Enabled = chkRemoveTransp.Checked;
            chkJpegQuality.CheckedChanged += (s, e) => txtJpegQuality.Enabled = chkJpegQuality.Checked;
            chkGenerateCover.CheckedChanged += (s, e) => pnlGC.Enabled = txtCoverPath.Enabled = btnBrowseCover.Enabled = chkGenerateCover.Checked;
            chkResizeCover.CheckedChanged += (s, e) => cmbResizeCover.Enabled = chkResizeCover.Checked;
            chkAnnEnable.CheckedChanged += (s, e) => pnlAE.Enabled = chkAnnEnable.Checked;
            chkAnnInToc.CheckedChanged += (s, e) => pnlAIT.Enabled = chkAnnInToc.Checked;
            chkTocPlacement.CheckedChanged += (s, e) => cmbTocPlacement.Enabled = chkTocPlacement.Checked;
            chkDropcaps.CheckedChanged += (s, e) => pnlDC.Enabled = chkDropcaps.Checked;

            // Додаємо на панель
            scrollMetadataPanel.Controls.AddRange([
                chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi,
                chkNotes, cmbNotesMode, chkSoftHyphen, pnlSH, chkRemoveTransp, pnlRT,
                chkJpegQuality, txtJpegQuality, chkGenerateCover, pnlGC, txtCoverPath, btnBrowseCover,
                chkResizeCover, cmbResizeCover, chkAnnEnable, pnlAE, chkAnnInToc, pnlAIT,
                chkTocPlacement, cmbTocPlacement, chkDropcaps, pnlDC
            ]);

            // Якір для скролу
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, nextY + rowHeight + 20, 1, 1);
            scrollMetadataPanel.Controls.Add(lblScrollAnchor);
        }
    }
}