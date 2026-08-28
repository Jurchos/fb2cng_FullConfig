using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig.Templates
{
    public partial class MetadataTab : UserControl, IThemableTab
    {

        public Panel scrollMetadataPanel = null!;

        // --- Елементи інтерфейсу / Контроли ---
        public CheckBox chkSoftHyphen = null!;
        public RadioButton rbSoftHyphenYes = null!, rbSoftHyphenNo = null!;

        public CheckBox chkPageMapEnable = null!, chkPageMapSize = null!, chkAdobeDe = null!;
        public RadioButton rbPageMapYes = null!, rbPageMapNo = null!, rbAdobeDeYes = null!, rbAdobeDeNo = null!;
        public TextBox txtPageMapSize = null!;
        public CheckBox chkUseBroken = null!, chkScaleFactor = null!, chkImgOptimize = null!;
        public RadioButton rbUseBrokenYes = null!, rbUseBrokenNo = null!, rbImgOptimizeYes = null!, rbImgOptimizeNo = null!;
        public TextBox txtScaleFactor = null!;

        public CheckBox chkRemoveTransp = null!;
        public RadioButton rbRemoveTranspYes = null!, rbRemoveTranspNo = null!;

        public CheckBox chkJpegQuality = null!;
        public TextBox txtJpegQuality = null!;

        public CheckBox chkReaderSize = null!;
        public Label lblWidth = null!, lblHeight = null!, lblDpi = null!;
        public TextBox txtWidth = null!, txtHeight = null!, txtDpi = null!;

        public CheckBox chkGenerateCover = null!;
        public RadioButton rbGenCoverYes = null!, rbGenCoverNo = null!;
        public TextBox txtCoverPath = null!;
        public Button btnBrowseCover = null!;

        public CheckBox chkResizeCover = null!;
        public ComboBox cmbResizeCover = null!;

        public CheckBox chkNotes = null!;
        public ComboBox cmbNotesMode = null!;

        public CheckBox chkAnnEnable = null!;
        public RadioButton rbAnnEnableYes = null!, rbAnnEnableNo = null!;

        public CheckBox chkAnnInToc = null!;
        public RadioButton rbAnnInTocYes = null!, rbAnnInTocNo = null!;

        public CheckBox chkTocPlacement = null!;
        public ComboBox cmbTocPlacement = null!;

        public CheckBox chkInclNoTitle = null!, chkVignettes = null!;
        public RadioButton rbInclNoTitleYes = null!, rbInclNoTitleNo = null!, rbVignettesYes = null!, rbVignettesNo = null!;
        public CheckedListBox clbVignettesItems = null!;
        public Button btnVignetteSettings = null!;
        public Panel vignettePopupContainer = null!;
        private ToolStripDropDown vignettePopup = null!;

        public CheckBox chkDropcaps = null!;
        public RadioButton rbDropcapsYes = null!, rbDropcapsNo = null!;

        // --- Конструктор ---
        public MetadataTab()
        {
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetupInterface();
        }

        // --- Інтерфейс ---
        public void ApplyTheme(bool isDark, Color foreColor, Color backColor, Color disabledColor)
        {
            // Кольори для списку всередині поп-апу
            clbVignettesItems.BackColor = backColor;
            clbVignettesItems.ForeColor = isDark ? Color.FromArgb(245, 245, 245) : SystemColors.ControlText;
            vignettePopupContainer.BackColor = backColor;
        }

        private void SetupInterface()
        {
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // забороняєм горизонтальний скрол
            scrollMetadataPanel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(scrollMetadataPanel);
            UiStyles.DisableHorizontalScroll(scrollMetadataPanel);

            // 1. Soft Hyphen
            chkSoftHyphen = new CheckBox { AutoSize = true };
            Panel pnlSH = UiStyles.CreateRadioGroup(out rbSoftHyphenYes, out rbSoftHyphenNo);

            // 2. Page Map ---
            chkPageMapEnable = new CheckBox { AutoSize = true };
            Panel pnlPME = UiStyles.CreateRadioGroup(out rbPageMapYes, out rbPageMapNo);
            chkPageMapSize = new CheckBox { AutoSize = true };
            txtPageMapSize = new TextBox { Text = "2300", Enabled = false };
            chkAdobeDe = new CheckBox { AutoSize = true };
            Panel pnlADE = UiStyles.CreateRadioGroup(out rbAdobeDeYes, out rbAdobeDeNo);

            // 3.Images ---
            chkUseBroken = new CheckBox { AutoSize = true };
            Panel pnlUB = UiStyles.CreateRadioGroup(out rbUseBrokenYes, out rbUseBrokenNo);
            chkScaleFactor = new CheckBox { AutoSize = true };
            txtScaleFactor = new TextBox { Text = "1.0", Enabled = false };
            chkImgOptimize = new CheckBox { AutoSize = true };
            Panel pnlIO = UiStyles.CreateRadioGroup(out rbImgOptimizeYes, out rbImgOptimizeNo);

            // 4. Transparency
            chkRemoveTransp = new CheckBox { AutoSize = true };
            Panel pnlRT = UiStyles.CreateRadioGroup(out rbRemoveTranspYes, out rbRemoveTranspNo);

            // 5. JPEG Quality
            chkJpegQuality = new CheckBox { AutoSize = true };
            txtJpegQuality = new TextBox { Text = "95", Enabled = false };

            // 6. Reader Size
            chkReaderSize = new CheckBox { AutoSize = true };

            lblWidth = new Label { Text = "W:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtWidth = new TextBox { Text = "1264", Enabled = false };

            lblHeight = new Label { Text = "H:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtHeight = new TextBox { Text = "1680", Enabled = false };

            lblDpi = new Label { Text = "DPI:", TextAlign = ContentAlignment.MiddleRight, Enabled = false };
            txtDpi = new TextBox { Text = "300", Enabled = false };

            // 7. Generate Cover
            chkGenerateCover = new CheckBox { AutoSize = true };
            Panel pnlGC = UiStyles.CreateRadioGroup(out rbGenCoverYes, out rbGenCoverNo);

            // Cover Path
            txtCoverPath = new TextBox { Enabled = false };
            int coverTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5);
            btnBrowseCover = new Button { FlatStyle = FlatStyle.Flat, Text = "", Enabled = false, TabStop = false };

            // 8. Resize Mode
            chkResizeCover = new CheckBox { AutoSize = true };
            cmbResizeCover = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbResizeCover.Items.AddRange(["none", "keepAR", "stretch", "fit"]);
            cmbResizeCover.SelectedIndex = 2;

            // 9. Footnotes
            chkNotes = new CheckBox { AutoSize = true };
            cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
            cmbNotesMode.SelectedIndex = 0;

            // 10. Annotation Enable
            chkAnnEnable = new CheckBox { AutoSize = true };
            Panel pnlAE = UiStyles.CreateRadioGroup(out rbAnnEnableYes, out rbAnnEnableNo);

            //  Annotation In TOC
            chkAnnInToc = new CheckBox { AutoSize = true };
            Panel pnlAIT = UiStyles.CreateRadioGroup(out rbAnnInTocYes, out rbAnnInTocNo);

            // 11. TOC Placement
            chkTocPlacement = new CheckBox { AutoSize = true };
            cmbTocPlacement = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbTocPlacement.Items.AddRange(["none", "before", "after"]);
            cmbTocPlacement.SelectedIndex = 0;

            // --- TOC & Vignettes ---
            chkInclNoTitle = new CheckBox { AutoSize = true };
            Panel pnlINT = UiStyles.CreateRadioGroup(out rbInclNoTitleYes, out rbInclNoTitleNo);
            chkVignettes = new CheckBox { AutoSize = true };
            Panel pnlVig = UiStyles.CreateRadioGroup(out rbVignettesYes, out rbVignettesNo);

            // 1. Створюємо кнопку (стиль як у решти кнопок)
            btnVignetteSettings = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                AutoSize = false, // Важливо
                TextAlign = ContentAlignment.MiddleCenter,
                TabStop = false
            };
            UiStyles.MakeButtonRounded(btnVignetteSettings, m.BtnRadius);

            // 2. Створюємо CheckedListBox зі збільшеним шрифтом
            clbVignettesItems = new CheckedListBox
            {
                CheckOnClick = true,
                BorderStyle = BorderStyle.None, // Прибираємо рамку, щоб вона не заважала відступу
                Font = new Font(Font.FontFamily, 10.5F),
                Width = m.ValueFieldWidth,
                Height = UiStyles.GetScaled(174)
            };

            // Створюємо контейнер для відступу
            vignettePopupContainer = new Panel
            {
                AutoSize = false,
                // Робимо ширину трохи більшою за список, щоб додати місце для відступу
                Width = clbVignettesItems.Width + UiStyles.GetScaled(10),
                Height = clbVignettesItems.Height
            };

            // Встановлюємо список всередині панелі зі зміщенням вправо на 15 пікселів
            clbVignettesItems.Location = new Point(UiStyles.GetScaled(15), UiStyles.GetScaled(5));
            vignettePopupContainer.Controls.Add(clbVignettesItems);

            // Додаємо в DropDown саме контейнер, а не сам список
            vignettePopup = new ToolStripDropDown { Padding = Padding.Empty };
            ToolStripControlHost host = new(vignettePopupContainer)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            _ = vignettePopup.Items.Add(host);

            // Коли поп-ап закривається — повертаємо фокус на головну кнопку ОК
            vignettePopup.Closed += (s, e) => {
                if (FindForm() is Form1 mainForm)
                {
                    _ = mainForm.btnOk.Focus();
                }
            };

            btnVignetteSettings.Click += (s, e) =>
            {
                // Відкриваємо вверх (Y = -висота списку)
                vignettePopup.Show(btnVignetteSettings, new Point(0, -clbVignettesItems.Height));
                // Після кліку по кнопці відразу прибираємо з неї фокус
                if (FindForm() is Form1 mainForm)
                {
                    _ = mainForm.btnOk.Focus();
                }
            };

            // 12. Dropcaps
            chkDropcaps = new CheckBox { AutoSize = true };
            Panel pnlDC = UiStyles.CreateRadioGroup(out rbDropcapsYes, out rbDropcapsNo);

            // ЛОГІКА АКТИВАЦІЇ ПОЛІВ
            chkSoftHyphen.CheckedChanged += (s, e) => pnlSH.Enabled = chkSoftHyphen.Checked;

            chkPageMapEnable.CheckedChanged += (s, e) => pnlPME.Enabled = chkPageMapEnable.Checked;
            chkPageMapSize.CheckedChanged += (s, e) => txtPageMapSize.Enabled = chkPageMapSize.Checked;
            chkAdobeDe.CheckedChanged += (s, e) => pnlADE.Enabled = chkAdobeDe.Checked;
            chkUseBroken.CheckedChanged += (s, e) => pnlUB.Enabled = chkUseBroken.Checked;
            chkScaleFactor.CheckedChanged += (s, e) => txtScaleFactor.Enabled = chkScaleFactor.Checked;
            chkImgOptimize.CheckedChanged += (s, e) => pnlIO.Enabled = chkImgOptimize.Checked;

            chkRemoveTransp.CheckedChanged += (s, e) => pnlRT.Enabled = chkRemoveTransp.Checked;
            chkJpegQuality.CheckedChanged += (s, e) => txtJpegQuality.Enabled = chkJpegQuality.Checked;
            chkReaderSize.CheckedChanged += (s, e) =>
            {
                lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = chkReaderSize.Checked;
            };
            chkGenerateCover.CheckedChanged += (s, e) => pnlGC.Enabled = txtCoverPath.Enabled = btnBrowseCover.Enabled = chkGenerateCover.Checked;
            chkResizeCover.CheckedChanged += (s, e) => cmbResizeCover.Enabled = chkResizeCover.Checked;
            chkNotes.CheckedChanged += (s, e) => cmbNotesMode.Enabled = chkNotes.Checked;
            chkAnnEnable.CheckedChanged += (s, e) => pnlAE.Enabled = chkAnnEnable.Checked;
            chkAnnInToc.CheckedChanged += (s, e) => pnlAIT.Enabled = chkAnnInToc.Checked;
            chkTocPlacement.CheckedChanged += (s, e) => cmbTocPlacement.Enabled = chkTocPlacement.Checked;

            chkInclNoTitle.CheckedChanged += (s, e) => pnlINT.Enabled = chkInclNoTitle.Checked;
            chkDropcaps.CheckedChanged += (s, e) => pnlDC.Enabled = chkDropcaps.Checked;
            chkVignettes.CheckedChanged += (s, e) =>
            {
                pnlVig.Enabled = btnVignetteSettings.Enabled = chkVignettes.Checked;
                ApplyThemeViaForm(); // Метод для оновлення кольорів при активації
            };

            // Додаємо на панель
            scrollMetadataPanel.Controls.AddRange([
                chkSoftHyphen, pnlSH, chkPageMapEnable, pnlPME, chkPageMapSize, txtPageMapSize, chkAdobeDe, pnlADE,
                chkUseBroken, pnlUB, chkScaleFactor, txtScaleFactor, chkImgOptimize, pnlIO,
                chkRemoveTransp, pnlRT, chkJpegQuality, txtJpegQuality,
                chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi, chkGenerateCover, pnlGC, txtCoverPath, btnBrowseCover,
                chkResizeCover, cmbResizeCover, chkNotes, cmbNotesMode, chkAnnEnable, pnlAE, chkAnnInToc, pnlAIT,
                chkTocPlacement, cmbTocPlacement, chkInclNoTitle, pnlINT, chkVignettes, btnVignetteSettings, pnlVig, chkDropcaps, pnlDC
            ]);

            // ===================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ
            // ===================================
            Size = m.TotalSize;

            // Створюємо змінну для крокування вниз (вона починається зі стартового Y)
            int nextY = m.StartY;

            chkSoftHyphen.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlSH.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkPageMapEnable.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlPME.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkPageMapSize.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtPageMapSize.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkAdobeDe.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlADE.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkUseBroken.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlUB.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkRemoveTransp.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlRT.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkScaleFactor.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtScaleFactor.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkImgOptimize.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlIO.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkJpegQuality.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtJpegQuality.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkReaderSize.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            int wLabelWidth = m.CheckBoxHeight + (int)(4 * UiStyles.Scale);
            lblWidth.SetBounds(m.SizeInputX, nextY, wLabelWidth, m.FieldHeight);
            txtWidth.SetBounds(lblWidth.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);
            lblHeight.SetBounds(txtWidth.Right + (m.SidePadding * 2), nextY, m.CheckBoxHeight, m.FieldHeight);
            txtHeight.SetBounds(lblHeight.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);
            lblDpi.SetBounds(txtHeight.Right + (m.SidePadding * 2), nextY, m.RowHeight + (int)(4 * UiStyles.Scale), m.FieldHeight);
            txtDpi.SetBounds(lblDpi.Right, nextY, m.CheckBoxHeight * 2, m.FieldHeight);

            nextY += m.RowHeight;
            chkGenerateCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlGC.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            txtCoverPath.SetBounds(m.SizeInputX, nextY, coverTxtWidth, m.FieldHeight);
            btnBrowseCover.SetBounds(txtCoverPath.Right + 5, nextY, m.BrowseBtnWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkResizeCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbResizeCover.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkNotes.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbNotesMode.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkAnnEnable.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlAE.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkAnnInToc.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlAIT.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkTocPlacement.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbTocPlacement.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkInclNoTitle.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlINT.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            // Наступний рядок - ВІНЬЄТКИ (мінімальний відступ)
            nextY += m.RowHeight;
            chkVignettes.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);

            btnVignetteSettings.SetBounds(m.RadioX - m.FooterBtnWidth - UiStyles.GetScaled(40), nextY, m.FooterBtnWidth, m.FieldHeight);

            pnlVig.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(120), m.FieldHeight);

            // Наступний рядок (Dropcaps) після віньєтки
            nextY += m.RowHeight;
            chkDropcaps.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlDC.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            // Якір для скролу
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, nextY + m.RowHeight + m.BlockMargin, 1, 1);
            scrollMetadataPanel.Controls.Add(lblScrollAnchor);
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