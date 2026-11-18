using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Utils;

namespace fb2cng_FullConfig.Templates
{
    public partial class ImagesTab : UserControl, IThemableTab
    {
        public Panel scrollImagesPanel = null!;

        // --- Елементи інтерфейсу ---
        public CheckBox chkReaderSize = null!;
        public Label lblWidth = null!, lblHeight = null!, lblDpi = null!;
        public TextBox txtWidth = null!, txtHeight = null!, txtDpi = null!;

        public CheckBox chkRemoveTransp = null!;
        public RadioButton rbRemoveTranspYes = null!, rbRemoveTranspNo = null!;

        public CheckBox chkRasterizeSvg = null!, chkFitToScreen = null!, chkBaselineJpeg = null!;
        public RadioButton rbRasterizeSvgYes = null!, rbRasterizeSvgNo = null!;
        public CheckBox chkUseBroken = null!, chkScaleFactor = null!, chkImgOptimize = null!;
        public RadioButton rbFitToScreenYes = null!, rbFitToScreenNo = null!;
        public RadioButton rbBaselineJpegYes = null!, rbBaselineJpegNo = null!;

        public CheckBox chkRasterFormat = null!, chkGrayscale = null!;
        public ComboBox cmbRasterFormat = null!, cmbGrayscale = null!;

        public RadioButton rbUseBrokenYes = null!, rbUseBrokenNo = null!, rbImgOptimizeYes = null!, rbImgOptimizeNo = null!;
        public TextBox txtScaleFactor = null!;

        public CheckBox chkJpegQuality = null!;
        public TextBox txtJpegQuality = null!;


        public CheckBox chkGenerateCover = null!;
        public RadioButton rbGenCoverYes = null!, rbGenCoverNo = null!;
        public TextBox txtCoverPath = null!;
        public Button btnBrowseCover = null!;

        public CheckBox chkResizeCover = null!;
        public ComboBox cmbResizeCover = null!;

        public ImagesTab()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetupInterface();
        }

        public void ApplyTheme(bool isDark, Color foreColor, Color backColor, Color disabledColor)
        {
            scrollImagesPanel.BackColor = BackColor;
        }

        private void SetupInterface()
        {
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            scrollImagesPanel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(scrollImagesPanel);
            UiStyles.DisableHorizontalScroll(scrollImagesPanel);

            // 1. Reader Size
            chkReaderSize = new CheckBox { AutoSize = true };
            lblWidth = new Label { Text = "W:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtWidth = new TextBox { Text = "1264", Enabled = false };
            lblHeight = new Label { Text = "H:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtHeight = new TextBox { Text = "1680", Enabled = false };
            lblDpi = new Label { Text = "DPI:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtDpi = new TextBox { Text = "300", Enabled = false };

            // 2. Use Broken
            chkUseBroken = new CheckBox { AutoSize = true };
            Panel pnlUB = UiStyles.CreateRadioGroup(out rbUseBrokenYes, out rbUseBrokenNo);

            // 3. Transparency
            chkRemoveTransp = new CheckBox { AutoSize = true };
            Panel pnlRT = UiStyles.CreateRadioGroup(out rbRemoveTranspYes, out rbRemoveTranspNo);

            // 4. Rasterize SVG
            chkRasterizeSvg = new CheckBox { AutoSize = true };
            Panel pnlRS = UiStyles.CreateRadioGroup(out rbRasterizeSvgYes, out rbRasterizeSvgNo);

            // 5. Raster Format
            chkRasterFormat = new CheckBox { AutoSize = true };
            cmbRasterFormat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbRasterFormat.Items.AddRange(["auto", "jpeg"]);
            cmbRasterFormat.SelectedIndex = 0;

            // 6. Scale Factor /
            chkScaleFactor = new CheckBox { AutoSize = true };
            txtScaleFactor = new TextBox { Text = "1.0", Enabled = false };

            // 7. Fit To Screen
            chkFitToScreen = new CheckBox { AutoSize = true };
            Panel pnlFTS = UiStyles.CreateRadioGroup(out rbFitToScreenYes, out rbFitToScreenNo);

            // 8. Baseline JPEG
            chkBaselineJpeg = new CheckBox { AutoSize = true };
            Panel pnlBJ = UiStyles.CreateRadioGroup(out rbBaselineJpegYes, out rbBaselineJpegNo);

            // 9. Grayscale
            chkGrayscale = new CheckBox { AutoSize = true };
            cmbGrayscale = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbGrayscale.Items.AddRange(["none", "luma"]);
            cmbGrayscale.SelectedIndex = 0;
            // 10. Optimize
            chkImgOptimize = new CheckBox { AutoSize = true };
            Panel pnlIO = UiStyles.CreateRadioGroup(out rbImgOptimizeYes, out rbImgOptimizeNo);

            // 11. JPEG Quality
            chkJpegQuality = new CheckBox { AutoSize = true };
            txtJpegQuality = new TextBox { Text = "95", Enabled = false };

            // 12. Generate Cover + Path
            chkGenerateCover = new CheckBox { AutoSize = true };
            Panel pnlGC = UiStyles.CreateRadioGroup(out rbGenCoverYes, out rbGenCoverNo);
            txtCoverPath = new TextBox { Enabled = false };
            int coverTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5);
            btnBrowseCover = new Button { FlatStyle = FlatStyle.Flat, Text = "", Enabled = false, TabStop = false };

            // 13. Resize Mode
            chkResizeCover = new CheckBox { AutoSize = true };
            cmbResizeCover = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbResizeCover.Items.AddRange(["none", "keepAR", "stretch", "fit", "fill"]);
            cmbResizeCover.SelectedIndex = 2;

            // Логіка активації полів при натисканні чекбокса
            chkUseBroken.CheckedChanged += (s, e) => pnlUB.Enabled = chkUseBroken.Checked;
            chkRemoveTransp.CheckedChanged += (s, e) => pnlRT.Enabled = chkRemoveTransp.Checked;
            chkRasterizeSvg.CheckedChanged += (s, e) => pnlRS.Enabled = chkRasterizeSvg.Checked;
            chkRasterFormat.CheckedChanged += (s, e) => cmbRasterFormat.Enabled = chkRasterFormat.Checked;
            chkScaleFactor.CheckedChanged += (s, e) => txtScaleFactor.Enabled = chkScaleFactor.Checked;
            chkFitToScreen.CheckedChanged += (s, e) => pnlFTS.Enabled = chkFitToScreen.Checked;
            chkBaselineJpeg.CheckedChanged += (s, e) => pnlBJ.Enabled = chkBaselineJpeg.Checked;
            chkGrayscale.CheckedChanged += (s, e) => cmbGrayscale.Enabled = chkGrayscale.Checked;
            chkImgOptimize.CheckedChanged += (s, e) => pnlIO.Enabled = chkImgOptimize.Checked;
            chkJpegQuality.CheckedChanged += (s, e) => txtJpegQuality.Enabled = chkJpegQuality.Checked;
            chkReaderSize.CheckedChanged += (s, e) =>
            {
                lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = chkReaderSize.Checked;
            };
            chkGenerateCover.CheckedChanged += (s, e) => pnlGC.Enabled = txtCoverPath.Enabled = btnBrowseCover.Enabled = chkGenerateCover.Checked;
            chkResizeCover.CheckedChanged += (s, e) => cmbResizeCover.Enabled = chkResizeCover.Checked;

            // Додаємо на панель
            scrollImagesPanel.Controls.AddRange([
                chkUseBroken, pnlUB, chkRemoveTransp, pnlRT,
                chkRasterizeSvg, pnlRS, chkRasterFormat, cmbRasterFormat,
                chkScaleFactor, txtScaleFactor, chkFitToScreen, pnlFTS, chkBaselineJpeg, pnlBJ, 
                chkGrayscale, cmbGrayscale,chkImgOptimize, pnlIO,
                chkJpegQuality, txtJpegQuality,
                chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi,
                chkGenerateCover, pnlGC, txtCoverPath, btnBrowseCover,
                chkResizeCover, cmbResizeCover
            ]);

            // ===================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ
            // ===================================
            Size = m.TotalSize;
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
            chkUseBroken.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlUB.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkRemoveTransp.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlRT.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkRasterizeSvg.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlRS.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkRasterFormat.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbRasterFormat.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkScaleFactor.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtScaleFactor.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkFitToScreen.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlFTS.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkBaselineJpeg.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlBJ.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkGrayscale.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbGrayscale.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkImgOptimize.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlIO.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkJpegQuality.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtJpegQuality.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkGenerateCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlGC.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            txtCoverPath.SetBounds(m.SizeInputX, nextY, coverTxtWidth, m.FieldHeight);
            btnBrowseCover.SetBounds(txtCoverPath.Right + 5, nextY, m.BrowseBtnWidth, m.FieldHeight);

            nextY = txtCoverPath.Bottom + m.BlockMargin;
            chkResizeCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbResizeCover.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            // Якір для коректного відступу при прокрутці
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, nextY + m.RowHeight + m.BlockMargin, 1, 1);
            scrollImagesPanel.Controls.Add(lblScrollAnchor);
        }
    }
}