using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig.Templates
{

    public partial class DocumentTab : UserControl
    {
        // Елементи інтерфейсу вкладки 
        public Panel scrollMenuPanel = null!;
        public ComboBox langComboBox = null!;
        public Button btnReset = null!;
        public Button btnDumpConfig = null!;
        public TextBox txtConfigName = null!;

        public CheckBox chkCustomYaml = null!;
        public TextBox txtCustomYamlPath = null!;
        public Button btnBrowseCustomYaml = null!;

        public CheckBox chkCss = null!;
        public TextBox txtCssPath = null!;
        public Button btnBrowseCss = null!;

        public CheckBox chkCover = null!;
        public ComboBox cmbTocType = null!;

        public CheckBox chkFixZip = null!, chkOpenFromCover = null!, chkTranslit = null!;
        public RadioButton rbFixZipYes = null!, rbFixZipNo = null!;
        public RadioButton rbOpenCoverYes = null!, rbOpenCoverNo = null!;
        public RadioButton rbTranslitYes = null!, rbTranslitNo = null!;
        public CheckBox chkFb2Name = null!;
        public CheckBox chkDefaultName = null!;

        public GroupBox grpOutName = null!;
        public ComboBox[]? cmbOutFields;
        public CheckBox[]? chkAsFolder;

        public Label lblLang = null!;
        public Label lblConfigName = null!;
        public Label lblOutNameTitle = null!;

        public DocumentTab()
        {
            DoubleBuffered = true;

            AutoScaleMode = AutoScaleMode.None;

            SetupInterface();
        }

        public class ControlGroup
        {
            public required CheckBox CheckBox { get; set; }
            public required TextBox TextBox { get; set; }
            public required Button Button { get; set; }
        }

        private static ControlGroup SetupToggleGroup(Image icon, Panel parentPanel)
        {
            CheckBox chk = new() { AutoSize = true };
            TextBox txt = new() { Enabled = false };
            Button btn = new() { Text = string.Empty, FlatStyle = FlatStyle.Flat, Enabled = false };
            btn.FlatAppearance.BorderSize = 0;

            // Викликаємо наш новий єдиний метод малювання (він у UiStyles, додаєм UiStyles. перед назвою)
            UiStyles.SetupIconButtonDrawing(btn, icon, chk, UiStyles.InactiveIconMatrix);

            // Зв'язуємо стан доступності тексту та кнопки із чекбоксом
            chk.CheckedChanged += (s, e) =>
            {
                txt.Enabled = chk.Checked;
                btn.Enabled = chk.Checked;
            };

            parentPanel.Controls.AddRange([chk, txt, btn]);

            return new ControlGroup { CheckBox = chk, TextBox = txt, Button = btn };
        }
        private void SetupInterface()
        {
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // забороняєм горизонтальний скрол
            scrollMenuPanel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(scrollMenuPanel);

            UiStyles.DisableHorizontalScroll(scrollMenuPanel);

            grpOutName = new GroupBox { Text = "" };
            scrollMenuPanel.Controls.Add(grpOutName);

            // 2. Створення елементів верхнього блоку (у scrollMenuPanel)
            lblLang = new Label { Text = "Language:", AutoSize = true };
            langComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            langComboBox.Items.AddRange(["English", "Українська", "Русский"]);
            btnReset = new Button
            {
                Image = UiStyles.ResizeImage(Properties.Resources.icon_reset, m.IconSize + UiStyles.GetScaled(3), m.IconSize + UiStyles.GetScaled(3)),
                ImageAlign = ContentAlignment.MiddleCenter, // Вирівнюємо іконку чітко по центру
                FlatStyle = FlatStyle.Flat
            };
            scrollMenuPanel.Controls.AddRange([lblLang, langComboBox, btnReset]);

            btnDumpConfig = new Button();
            scrollMenuPanel.Controls.Add(btnDumpConfig);

            lblConfigName = new Label { AutoSize = true };
            txtConfigName = new TextBox { Text = "Data/config.yaml" };
            scrollMenuPanel.Controls.AddRange([lblConfigName, txtConfigName]);

            ControlGroup yamlGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCustomYaml = yamlGroup.CheckBox; txtCustomYamlPath = yamlGroup.TextBox; btnBrowseCustomYaml = yamlGroup.Button;

            // Налаштування CSS та кнопки з малюванням іконки
            ControlGroup cssGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCss = cssGroup.CheckBox; txtCssPath = cssGroup.TextBox; btnBrowseCss = cssGroup.Button;

            chkCover = new CheckBox { AutoSize = true }; // обкладинка
            cmbTocType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbTocType.Items.AddRange(["normal", "old_kindle", "flat"]);
            cmbTocType.SelectedIndex = 0;
            chkCover.CheckedChanged += (s, e) => { cmbTocType.Enabled = chkCover.Checked; (ParentForm as Form1)?.ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange([chkCover, cmbTocType]);

            // Fix ZIP, Open From Cover, Translit (радіо-кнопки)
            chkFixZip = new CheckBox { AutoSize = true };
            Panel pnlFixZip = UiStyles.CreateRadioGroup(out rbFixZipYes, out rbFixZipNo);

            chkOpenFromCover = new CheckBox { AutoSize = true };
            Panel pnlOpenCover = UiStyles.CreateRadioGroup(out rbOpenCoverYes, out rbOpenCoverNo);

            chkTranslit = new CheckBox { AutoSize = true };
            Panel pnlTranslit = UiStyles.CreateRadioGroup(out rbTranslitYes, out rbTranslitNo);

            chkFb2Name = new CheckBox { AutoSize = true };
            chkDefaultName = new CheckBox { AutoSize = true };
            lblOutNameTitle = new Label { AutoSize = true };
            // Конструктор структури назви (7 елементів)
            cmbOutFields = new ComboBox[7];
            chkAsFolder = new CheckBox[7];

            for (int i = 0; i < 7; i++)
            {
                int index = i;
                cmbOutFields[index] = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Tag = i };
                chkAsFolder[index] = new CheckBox { Text = "Fold", Enabled = false, Tag = "FolderCheckBox" };
                cmbOutFields[index].Items.AddRange(["", "", "", "", "", "", "", "", ""]);
                cmbOutFields[index].SelectedIndex = 0;
                if (index > 0)
                {
                    cmbOutFields[index].Enabled = false;
                }

                grpOutName.Controls.AddRange([cmbOutFields[index], chkAsFolder[index]]);
            }
            scrollMenuPanel.Controls.AddRange([chkFixZip, pnlFixZip, chkOpenFromCover, pnlOpenCover,
                chkTranslit, pnlTranslit, chkFb2Name, chkDefaultName, grpOutName]);
            // ========================================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ ВСЕРЕДИНІ USERCONTROL
            // ========================================================
            Size = m.TotalSize;

            // Створюємо змінну для крокування вниз (вона починається зі стартового Y)
            int nextY = m.StartY;
            // Позиціонування елементів
            int yamlTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5) - m.SidePadding;
            int spacing = UiStyles.GetScaled(6); // Масштабований відступ між полем мови і кнопкою
            lblLang.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.LabelHeight);
            langComboBox.ItemHeight = m.FieldHeight - 6;
            langComboBox.SetBounds(m.XLeft + m.TextLabelWidth, nextY, yamlTxtWidth, m.FieldHeight);
            int resetWidth = m.BrowseBtnWidth - UiStyles.GetScaled(24);
            int resetX = m.FieldWidth + m.XLeft - m.BrowseBtnWidth + UiStyles.GetScaled(18);
            btnReset.SetBounds(resetX, nextY - UiStyles.GetScaled(2), resetWidth, m.FieldHeight + UiStyles.GetScaled(3));

            nextY = btnReset.Bottom + m.BlockMargin + UiStyles.GetScaled(2);
            btnDumpConfig.SetBounds(m.XLeft + m.SidePadding, nextY, m.FieldWidth - UiStyles.GetScaled(6), m.FieldHeight + UiStyles.GetScaled(4));

            nextY = btnDumpConfig.Bottom + m.BlockMargin;
            lblConfigName.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.LabelHeight);
            txtConfigName.SetBounds(m.XLeft + m.TextLabelWidth, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY = txtConfigName.Bottom + m.BlockMargin;
            // 1. Блок Custom YAML (новий)
            int browseX = m.FieldWidth + m.XLeft - m.BrowseBtnWidth - UiStyles.GetScaled(6);
            chkCustomYaml.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            txtCustomYamlPath.Multiline = true;
            txtCustomYamlPath.SetBounds(m.XLeft + m.TextLabelWidth, nextY, yamlTxtWidth, m.FieldHeight);
            btnBrowseCustomYaml.SetBounds(browseX, nextY, m.BrowseBtnWidth, m.FieldHeight);

            // 2. Блок CSS (зсувається нижче)
            nextY = txtCustomYamlPath.Bottom + m.BlockMargin;
            chkCss.SetBounds(m.XLeft, nextY + UiStyles.GetScaled(1), m.TextLabelWidth, m.CheckBoxHeight);
            int cssTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5) - m.SidePadding;
            txtCssPath.Multiline = true;
            txtCssPath.SetBounds(m.XLeft + m.TextLabelWidth, nextY, cssTxtWidth, m.FieldHeight);
            btnBrowseCss.SetBounds(browseX, nextY, m.BrowseBtnWidth, m.FieldHeight);

            nextY = txtCssPath.Bottom + m.BlockMargin;
            chkCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            cmbTocType.ItemHeight = m.FieldHeight - 6;
            cmbTocType.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY = cmbTocType.Bottom + m.BlockMargin;
            // Позиціонуємо FixZip
            chkFixZip.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlFixZip.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(150), m.FieldHeight);

            nextY = chkFixZip.Bottom + m.BlockMargin;
            // Позиціонуємо OpenFromCover
            chkOpenFromCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlOpenCover.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(150), m.FieldHeight);

            nextY = chkOpenFromCover.Bottom + m.BlockMargin;
            // Позиціонуємо Translit
            chkTranslit.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlTranslit.SetBounds(m.RadioX, nextY, UiStyles.GetScaled(140), m.FieldHeight);

            nextY = chkTranslit.Bottom + m.BlockMargin;
            // Позиціонуємо Fb2Name (Він займає всю ширину, бо не має радіобатонів)
            chkFb2Name.SetBounds(m.XLeft, nextY, m.FieldWidth, m.CheckBoxHeight);

            nextY = chkFb2Name.Bottom + m.BlockMargin;
            chkDefaultName.SetBounds(m.XLeft, nextY, m.FieldWidth, m.CheckBoxHeight);

            // Налаштування групи структури назви файлу
            int OutNameTopPadding = UiStyles.GetScaled(10);// Відступ зверху для групи структури назви файлу
            int rowHeight = m.FieldHeight + UiStyles.GetScaled(5);// Висота одного рядка з комбо та чекбоксом
            int grpOutHeight = (rowHeight * 7) + UiStyles.GetScaled(25);// Висота групи з 8 рядків + заголовок групи
            grpOutName.SetBounds(m.XLeft, chkDefaultName.Bottom + OutNameTopPadding, m.FieldWidth, grpOutHeight);

            int comboWidth = (int)(grpOutName.Width * 0.76f);
            int checkFoldWidth = grpOutName.Width - comboWidth - UiStyles.GetScaled(15);
            int itemY = UiStyles.GetScaled(20);

            for (int i = 0; i < 7; i++)
            {
                if (cmbOutFields != null && chkAsFolder != null)
                {
                    cmbOutFields[i].ItemHeight = m.FieldHeight - 6;
                    cmbOutFields[i].SetBounds(UiStyles.GetScaled(10), itemY, comboWidth, m.FieldHeight);
                    chkAsFolder[i].SetBounds(cmbOutFields[i].Right + UiStyles.GetScaled(15), itemY + UiStyles.GetScaled(1), checkFoldWidth, m.CheckBoxHeight);
                    itemY += rowHeight;
                }
            }
            // Правильне роздільне створення об'єкта та встановлення його координат
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, grpOutName.Bottom + UiStyles.GetScaled(10), 1, 1);
            scrollMenuPanel.Controls.Add(lblScrollAnchor);
        }
    }
}