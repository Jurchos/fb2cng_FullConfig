using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Utils;
namespace fb2cng_FullConfig.Templates
{

    public partial class DocumentTab : UserControl, IThemableTab
    {
        // --- 1. Поля стану та константи ---
        private bool _isFirstLaunchApplied;
        private const int OutFieldsCount = 7;
        // --- Елементи інтерфейсу (Публічні) ---
        public Panel scrollMenuPanel = null!;
        public ComboBox langComboBox = null!;
        public Button btnReset = null!;
        public Button btnDumpConfig = null!;
        public TextBox txtConfigName = null!;
        public Label lblLang = null!;
        public Label lblConfigName = null!;
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

        // --- Конструктор ---
        public DocumentTab()
        {
            DoubleBuffered = true;

            AutoScaleMode = AutoScaleMode.None;

            SetupInterface();
        }

        // --- Реалізація IThemableTab ---
        public void ApplyTheme(bool isDark, Color foreColor, Color backColor, Color disabledColor)
        {
            // 1. Фарбуємо власні унікальні елементи
            scrollMenuPanel.BackColor = BackColor;
            grpOutName.BackColor = BackColor;

            bool isNamingLocked = chkFb2Name.Checked || chkDefaultName.Checked;

            // ВАЖЛИВО: Ми НЕ пишемо grpOutName.Enabled = false, щоб Windows не малювала чорним.
            // Замість цього ми ставимо мітку в Tag
            grpOutName.Tag = isNamingLocked ? "ForceDisabled" : null;

            // Вимикаємо елементи всередині групи вручну, щоб вони стали сірими через рекурсію менеджера
            if (cmbOutFields != null && chkAsFolder != null)
            {
                for (int i = 0; i < cmbOutFields.Length; i++)
                {
                    // Якщо naming locked — елементи вимкнені. 
                    // Якщо ні — їх стан залежить від логіки Form1_Logic (яка вже там є)
                    if (isNamingLocked)
                    {
                        cmbOutFields[i].Enabled = false;
                        chkAsFolder[i].Enabled = false;
                    }
                }
            }

            if (!_isFirstLaunchApplied)
            {
                if (!chkCustomYaml.Checked) { chkCustomYaml.Checked = true; chkCustomYaml.Checked = false; }
                _isFirstLaunchApplied = true;
            }
        }

        // --- Ініціалізація інтерфейсу --- 
        private void SetupInterface()
        {
            // Отримуємо всі готові прораховані метрики в один рядок
            UiStyles.LayoutMetrics m = new(UiStyles.Scale);

            // Панель скролу, забороняєм горизонтальний скрол
            scrollMenuPanel = new Panel { Dock = DockStyle.Fill };
            Controls.Add(scrollMenuPanel);
            UiStyles.DisableHorizontalScroll(scrollMenuPanel);

            // Блок мови та ресет
            lblLang = new Label { Text = "Language:", AutoSize = true };
            langComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            langComboBox.Items.AddRange(["English", "Українська", "Русский"]);
            btnReset = new Button
            {
                Image = UiStyles.ResizeImage(Properties.Resources.icon_reset, m.IconSize, m.IconSize),
                ImageAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent
            };
            UiStyles.MakeButtonRounded(btnReset, m.BtnRadius);

            // Блок конфігу та дампу
            btnDumpConfig = new Button();
            lblConfigName = new Label { AutoSize = true };
            txtConfigName = new TextBox { Text = Config.DefaultConfigPath };

            // Групи вибору файлів (Custom YAML, CSS)
            ControlGroup yamlGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCustomYaml = yamlGroup.CheckBox; txtCustomYamlPath = yamlGroup.TextBox; btnBrowseCustomYaml = yamlGroup.Button;
            ControlGroup cssGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCss = cssGroup.CheckBox; txtCssPath = cssGroup.TextBox; btnBrowseCss = cssGroup.Button;

            // Навігація та бінарні налаштування
            chkCover = new CheckBox { AutoSize = true }; // обкладинка
            cmbTocType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbTocType.Items.AddRange(["normal", "old_kindle", "flat"]);
            cmbTocType.SelectedIndex = 0;
            chkCover.CheckedChanged += (s, e) =>
            {
                cmbTocType.Enabled = chkCover.Checked;
                ApplyThemeViaForm();
            };
            // Fix ZIP, Open From Cover, Translit (радіо-кнопки)
            chkFixZip = new CheckBox { AutoSize = true };
            Panel pnlFixZip = UiStyles.CreateRadioGroup(out rbFixZipYes, out rbFixZipNo);

            chkOpenFromCover = new CheckBox { AutoSize = true };
            Panel pnlOpenCover = UiStyles.CreateRadioGroup(out rbOpenCoverYes, out rbOpenCoverNo);

            chkTranslit = new CheckBox { AutoSize = true };
            Panel pnlTranslit = UiStyles.CreateRadioGroup(out rbTranslitYes, out rbTranslitNo);

            chkFb2Name = new CheckBox { AutoSize = true };
            chkDefaultName = new CheckBox { AutoSize = true };

            // Конструктор структури назви
            cmbOutFields = new ComboBox[OutFieldsCount];
            chkAsFolder = new CheckBox[OutFieldsCount];
            grpOutName = new GroupBox { Text = "", Name = "grpOutName" };
            for (int i = 0; i < cmbOutFields.Length; i++)
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
            scrollMenuPanel.Controls.AddRange([lblLang, langComboBox, btnReset, btnDumpConfig, lblConfigName,
                txtConfigName, chkCover, cmbTocType, chkFixZip, pnlFixZip, chkOpenFromCover, pnlOpenCover,
                chkTranslit, pnlTranslit, chkFb2Name, chkDefaultName, grpOutName]);

            // ====================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ 
            // ====================================

            Size = m.TotalSize;

            // Створюємо змінну для крокування вниз (вона починається зі стартового Y)
            int nextY = m.StartY;
            // Позиціонування елементів
            int yamlTxtWidth = m.ValueFieldWidth - m.BrowseBtnWidth - UiStyles.GetScaled(5) - m.SidePadding;
            lblLang.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.LabelHeight);
            langComboBox.ItemHeight = m.FieldHeight - m.ElementSpacing;
            langComboBox.SetBounds(m.XLeft + m.TextLabelWidth, nextY, yamlTxtWidth, m.FieldHeight);
            int resetWidth = m.BrowseBtnWidth - m.IconSize;
            int resetX = m.FieldWidth + m.XLeft - m.BrowseBtnWidth + UiStyles.GetScaled(18);
            btnReset.SetBounds(resetX, nextY - UiStyles.GetScaled(2), resetWidth, m.FieldHeight + UiStyles.GetScaled(3));

            nextY = btnReset.Bottom + m.BlockMargin + UiStyles.GetScaled(2);
            btnDumpConfig.SetBounds(m.XLeft + m.SidePadding, nextY, m.FieldWidth - m.ElementSpacing, m.FieldHeight + UiStyles.GetScaled(4));

            nextY = btnDumpConfig.Bottom + m.BlockMargin;
            lblConfigName.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.LabelHeight);
            txtConfigName.SetBounds(m.XLeft + m.TextLabelWidth, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY = txtConfigName.Bottom + m.BlockMargin;
            // 1. Блок Custom YAML (новий)
            int browseX = m.FieldWidth + m.XLeft - m.BrowseBtnWidth - m.ElementSpacing;
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
            cmbTocType.ItemHeight = m.FieldHeight - m.ElementSpacing;
            cmbTocType.SetBounds(m.SizeInputX, nextY, m.ValueFieldWidth, m.FieldHeight);

            nextY = cmbTocType.Bottom + m.BlockMargin;
            // Позиціонуємо FixZip
            chkFixZip.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlFixZip.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY = chkFixZip.Bottom + m.BlockMargin;
            // Позиціонуємо OpenFromCover
            chkOpenFromCover.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlOpenCover.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY = chkOpenFromCover.Bottom + m.BlockMargin;
            // Позиціонуємо Translit
            chkTranslit.SetBounds(m.XLeft, nextY, m.TextLabelWidth, m.CheckBoxHeight);
            pnlTranslit.SetBounds(m.RadioX, nextY, m.RadioGroupWidth, m.FieldHeight);

            nextY = chkTranslit.Bottom + m.BlockMargin;
            // Позиціонуємо Fb2Name (Він займає всю ширину, бо не має радіобатонів)
            chkFb2Name.SetBounds(m.XLeft, nextY, m.FieldWidth, m.CheckBoxHeight);

            nextY = chkFb2Name.Bottom + m.BlockMargin;
            chkDefaultName.SetBounds(m.XLeft, nextY, m.FieldWidth, m.CheckBoxHeight);

            // Налаштування групи структури назви файлу
            int rowHeight = m.FieldHeight + m.HeaderTopPadding;                           // Висота рядка (поле + відступ)
            int grpOutHeight = (rowHeight * cmbOutFields.Length) + UiStyles.GetScaled(32);// Висота групи (рядки + заголовок)
            grpOutName.SetBounds(m.XLeft, chkDefaultName.Bottom + m.BlockMargin, m.FieldWidth, grpOutHeight);

            int innerPadding = UiStyles.GetScaled(10);                                    // Внутрішній відступ від країв GroupBox
                                                                                          // Ширина ComboBox (ширина "as fold"= ширині кнопок футера 90)
            int comboWidth = grpOutName.Width - m.FooterBtnWidth - m.BlockMargin - (innerPadding * 2);
            int itemY = m.StartY * 2;                                                     // Початкова позиція Y під заголовком групи

            if (cmbOutFields != null && chkAsFolder != null)
            {
                for (int i = 0; i < cmbOutFields.Length; i++)
                {
                    cmbOutFields[i].ItemHeight = m.FieldHeight - m.ElementSpacing;
                    cmbOutFields[i].SetBounds(innerPadding, itemY, comboWidth, m.FieldHeight);
                    chkAsFolder[i].SetBounds(cmbOutFields[i].Right + m.BlockMargin, itemY + UiStyles.GetScaled(1), m.FooterBtnWidth, m.CheckBoxHeight);
                    itemY += rowHeight;
                }
            }
            // Правильне роздільне створення об'єкта та встановлення його координат
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, grpOutName.Bottom + m.BlockMargin, 1, 1);
            scrollMenuPanel.Controls.Add(lblScrollAnchor);
        }

        // --- Допоміжні методи ---
        private static ControlGroup SetupToggleGroup(Image icon, Panel parentPanel)
        {
            CheckBox chk = new() { AutoSize = true };
            TextBox txt = new() { Enabled = false };
            Button btn = new() { Text = string.Empty, FlatStyle = FlatStyle.Flat, Enabled = false };
            btn.FlatAppearance.BorderSize = 0;

            // Викликаємо наш новий єдиний метод малювання (він у UiStyles)
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

        private void ApplyThemeViaForm()
        {
            if (FindForm() is Form1 mainForm)
            {
                mainForm.ApplyTheme();
            }
        }

        public class ControlGroup
        {
            public required CheckBox CheckBox { get; set; }
            public required TextBox TextBox { get; set; }
            public required Button Button { get; set; }
        }

    }
}