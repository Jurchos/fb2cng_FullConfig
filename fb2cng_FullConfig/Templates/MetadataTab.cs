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
        public CheckBox chkKfxCompression = null!, chkAzw3Compression = null!, chkAzw3Apnx = null!, chkPdfPageFrame = null!, chkMarkdownImages = null!;
        public ComboBox cmbKfxCompression = null!, cmbAzw3Compression = null!, cmbAzw3Apnx = null!, cmbMarkdownImages = null!;
        public TextBox txtPdfPageFrame = null!;
        public CheckBox chkPageMapEnable = null!, chkPageMapSize = null!, chkAdobeDe = null!;
        public RadioButton rbPageMapYes = null!, rbPageMapNo = null!, rbAdobeDeYes = null!, rbAdobeDeNo = null!;
        public TextBox txtPageMapSize = null!;
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

        public CheckBox chkSpeech = null!, chkDashes = null!, chkDialogue = null!;
        public RadioButton rbSpeechYes = null!, rbSpeechNo = null!;
        public RadioButton rbDashesYes = null!, rbDashesNo = null!;
        public RadioButton rbDialogueYes = null!, rbDialogueNo = null!;

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

            // KFX Compression
            chkKfxCompression = new CheckBox { AutoSize = true };
            cmbKfxCompression = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbKfxCompression.Items.AddRange(["none", "lzma"]);
            cmbKfxCompression.SelectedIndex = 0;

            // AZW3 Compression
            chkAzw3Compression = new CheckBox { AutoSize = true };
            cmbAzw3Compression = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbAzw3Compression.Items.AddRange(["none", "standard", "huffdic"]);
            cmbAzw3Compression.SelectedIndex = 1; // standard за замовчуванням

            // AZW3 APNX
            chkAzw3Apnx = new CheckBox { AutoSize = true };
            cmbAzw3Apnx = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbAzw3Apnx.Items.AddRange(["none", "eink", "app"]);
            cmbAzw3Apnx.SelectedIndex = 0;

            // PDF Page Frame
            chkPdfPageFrame = new CheckBox { AutoSize = true };
            txtPdfPageFrame = new TextBox { Text = "0", Enabled = false };

            // Markdown Images
            chkMarkdownImages = new CheckBox { AutoSize = true };
            cmbMarkdownImages = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbMarkdownImages.Items.AddRange(["placeholder", "external", "embedded"]);
            cmbMarkdownImages.SelectedIndex = 0;

            // 2. Page Map ---
            chkPageMapEnable = new CheckBox { AutoSize = true };
            Panel pnlPME = UiStyles.CreateRadioGroup(out rbPageMapYes, out rbPageMapNo);
            chkPageMapSize = new CheckBox { AutoSize = true };
            txtPageMapSize = new TextBox { Text = "2300", Enabled = false };
            chkAdobeDe = new CheckBox { AutoSize = true };
            Panel pnlADE = UiStyles.CreateRadioGroup(out rbAdobeDeYes, out rbAdobeDeNo);

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

            // 13. Text Transformations
            chkSpeech = new CheckBox { AutoSize = true };
            Panel pnlSpeech = UiStyles.CreateRadioGroup(out rbSpeechYes, out rbSpeechNo);

            chkDashes = new CheckBox { AutoSize = true };
            Panel pnlDashes = UiStyles.CreateRadioGroup(out rbDashesYes, out rbDashesNo);

            chkDialogue = new CheckBox { AutoSize = true };
            Panel pnlDialogue = UiStyles.CreateRadioGroup(out rbDialogueYes, out rbDialogueNo);

            // ЛОГІКА АКТИВАЦІЇ ПОЛІВ
            chkSoftHyphen.CheckedChanged += (s, e) => pnlSH.Enabled = chkSoftHyphen.Checked;
            chkKfxCompression.CheckedChanged += (s, e) => cmbKfxCompression.Enabled = chkKfxCompression.Checked;
            chkAzw3Compression.CheckedChanged += (s, e) => cmbAzw3Compression.Enabled = chkAzw3Compression.Checked;
            chkAzw3Apnx.CheckedChanged += (s, e) => cmbAzw3Apnx.Enabled = chkAzw3Apnx.Checked;
            chkPdfPageFrame.CheckedChanged += (s, e) => txtPdfPageFrame.Enabled = chkPdfPageFrame.Checked;
            chkMarkdownImages.CheckedChanged += (s, e) => cmbMarkdownImages.Enabled = chkMarkdownImages.Checked;
            chkPageMapEnable.CheckedChanged += (s, e) => pnlPME.Enabled = chkPageMapEnable.Checked;
            chkPageMapSize.CheckedChanged += (s, e) => txtPageMapSize.Enabled = chkPageMapSize.Checked;
            chkAdobeDe.CheckedChanged += (s, e) => pnlADE.Enabled = chkAdobeDe.Checked;
            chkNotes.CheckedChanged += (s, e) => cmbNotesMode.Enabled = chkNotes.Checked;
            chkAnnEnable.CheckedChanged += (s, e) => pnlAE.Enabled = chkAnnEnable.Checked;
            chkAnnInToc.CheckedChanged += (s, e) => pnlAIT.Enabled = chkAnnInToc.Checked;
            chkTocPlacement.CheckedChanged += (s, e) => cmbTocPlacement.Enabled = chkTocPlacement.Checked;
            chkInclNoTitle.CheckedChanged += (s, e) => pnlINT.Enabled = chkInclNoTitle.Checked;
            chkVignettes.CheckedChanged += (s, e) =>
            {
                pnlVig.Enabled = btnVignetteSettings.Enabled = chkVignettes.Checked;
                ApplyThemeViaForm(); // Метод для оновлення кольорів при активації
            };
            chkDropcaps.CheckedChanged += (s, e) => pnlDC.Enabled = chkDropcaps.Checked;
            chkSpeech.CheckedChanged += (s, e) => pnlSpeech.Enabled = chkSpeech.Checked;
            chkDashes.CheckedChanged += (s, e) => pnlDashes.Enabled = chkDashes.Checked;
            chkDialogue.CheckedChanged += (s, e) => pnlDialogue.Enabled = chkDialogue.Checked;

            // Додаємо на панель
            scrollMetadataPanel.Controls.AddRange([
                chkSoftHyphen, pnlSH,chkKfxCompression, cmbKfxCompression, chkAzw3Compression, cmbAzw3Compression,
                chkAzw3Apnx, cmbAzw3Apnx, chkPdfPageFrame, txtPdfPageFrame, chkMarkdownImages, cmbMarkdownImages, 
                chkPageMapEnable, pnlPME, chkPageMapSize, txtPageMapSize, chkAdobeDe, pnlADE, chkNotes, cmbNotesMode, 
                chkAnnEnable, pnlAE, chkAnnInToc, pnlAIT, chkTocPlacement, cmbTocPlacement, chkInclNoTitle, pnlINT, 
                chkVignettes, btnVignetteSettings, pnlVig, chkDropcaps, pnlDC,chkSpeech, pnlSpeech, chkDashes, 
                pnlDashes, chkDialogue, pnlDialogue
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
            chkKfxCompression.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbKfxCompression.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkAzw3Compression.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbAzw3Compression.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkAzw3Apnx.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbAzw3Apnx.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkPdfPageFrame.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtPdfPageFrame.SetBounds(m.RadioX, nextY, m.CheckBoxHeight * 3, m.FieldHeight);

            nextY += m.RowHeight;
            chkMarkdownImages.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbMarkdownImages.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

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

            nextY += m.RowHeight;
            chkSpeech.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlSpeech.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkDashes.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlDashes.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY += m.RowHeight;
            chkDialogue.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlDialogue.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

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