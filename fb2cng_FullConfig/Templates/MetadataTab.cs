using fb2cng_FullConfig.Utils;
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
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // забороняєм горизонтальний скрол
            scrollMetadataPanel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(scrollMetadataPanel);

            UiStyles.DisableHorizontalScroll(scrollMetadataPanel);

            // 1. Reader Size
            chkReaderSize = new CheckBox { AutoSize = true };

            lblWidth = new Label { Text = "W:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtWidth = new TextBox { Text = "1264", Enabled = false };

            lblHeight = new Label { Text = "H:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtHeight = new TextBox { Text = "1680", Enabled = false };

            lblDpi = new Label { Text = "DPI:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtDpi = new TextBox { Text = "300", Enabled = false };

            // 2. Footnotes
            chkNotes = new CheckBox { AutoSize = true };
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
            cmbNotesMode.SelectedIndex = 0;

            // 3. Soft Hyphen
            chkSoftHyphen = new CheckBox { AutoSize = true };
            Panel pnlSH = UiStyles.CreateRadioGroup(out rbSoftHyphenYes, out rbSoftHyphenNo);

            // 4. Transparency
            chkRemoveTransp = new CheckBox { AutoSize = true };
            Panel pnlRT = UiStyles.CreateRadioGroup(out rbRemoveTranspYes, out rbRemoveTranspNo);

            // 5. JPEG Quality
            chkJpegQuality = new CheckBox { AutoSize = true };
            txtJpegQuality = new TextBox { Text = "95", Enabled = false };

            // 6. Generate Cover
            chkGenerateCover = new CheckBox { AutoSize = true };
            Panel pnlGC = UiStyles.CreateRadioGroup(out rbGenCoverYes, out rbGenCoverNo);

            // 7. Cover Path
            txtCoverPath = new TextBox { Enabled = false };
            int coverTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5);
            btnBrowseCover = new Button { FlatStyle = FlatStyle.Flat, Text = "", Enabled = false };

            // 8. Resize Mode
            chkResizeCover = new CheckBox { AutoSize = true };
            cmbResizeCover = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbResizeCover.Items.AddRange(["none", "keepAR", "stretch"]);
            cmbResizeCover.SelectedIndex = 2;

            // 9. Annotation Enable
            chkAnnEnable = new CheckBox { AutoSize = true };
            Panel pnlAE = UiStyles.CreateRadioGroup(out rbAnnEnableYes, out rbAnnEnableNo);

            // 10. Annotation In TOC
            chkAnnInToc = new CheckBox { AutoSize = true };
            Panel pnlAIT = UiStyles.CreateRadioGroup(out rbAnnInTocYes, out rbAnnInTocNo);

            // 11. TOC Placement
            chkTocPlacement = new CheckBox { AutoSize = true };
            cmbTocPlacement = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbTocPlacement.Items.AddRange(["none", "before", "after"]);
            cmbTocPlacement.SelectedIndex = 0;

            // 12. Dropcaps
            chkDropcaps = new CheckBox { AutoSize = true };
            Panel pnlDC = UiStyles.CreateRadioGroup(out rbDropcapsYes, out rbDropcapsNo);

            // ЛОГІКА АКТИВАЦІЇ ПОЛІВ
            chkReaderSize.CheckedChanged += (s, e) =>
            {
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

            // ========================================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ ВСЕРЕДИНІ USERCONTROL
            // ========================================================
            Size = m.TotalSize;

            // Створюємо змінну для крокування вниз (вона починається зі стартового Y)
            int nextY = m.StartY;
            chkReaderSize.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            int wLabelWidth = m.CheckBoxHeight + (int)(4 * UiStyles.Scale);
            lblWidth.SetBounds(m.SizeInputX, nextY, wLabelWidth, m.FieldHeight);
            txtWidth.SetBounds(lblWidth.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);
            lblHeight.SetBounds(txtWidth.Right + (m.SidePadding * 2), nextY, m.CheckBoxHeight, m.FieldHeight);
            txtHeight.SetBounds(lblHeight.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);
            lblDpi.SetBounds(txtHeight.Right + (m.SidePadding * 2), nextY, m.RowHeight + (int)(4 * UiStyles.Scale), m.FieldHeight);
            txtDpi.SetBounds(lblDpi.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkNotes.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbNotesMode.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkSoftHyphen.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlSH.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY += m.RowHeight;
            chkRemoveTransp.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlRT.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY += m.RowHeight;
            chkJpegQuality.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtJpegQuality.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkGenerateCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlGC.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY += m.RowHeight;
            txtCoverPath.SetBounds(m.SizeInputX, nextY, coverTxtWidth, m.FieldHeight);
            btnBrowseCover.SetBounds(txtCoverPath.Right + 5, nextY, m.BrowseBtnWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkResizeCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbResizeCover.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkAnnEnable.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlAE.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY += m.RowHeight;
            chkAnnInToc.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlAIT.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY += m.RowHeight;
            chkTocPlacement.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbTocPlacement.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkDropcaps.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlDC.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            // Якір для скролу
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, nextY + m.RowHeight + 20, 1, 1);
            scrollMetadataPanel.Controls.Add(lblScrollAnchor);
        }
    }
}