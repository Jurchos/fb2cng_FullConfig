using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace fb2cng_FullConfig.Templates
{

    public partial class DocumentTab : UserControl
    {
        // Елементи інтерфейсу вкладки 
        public Panel scrollMenuPanel = null!;
        public ComboBox langComboBox = null!;
        public Button btnDumpConfig = null!;
        public TextBox txtConfigName = null!;

        public CheckBox chkCustomYaml = null!;
        public TextBox txtCustomYamlPath = null!;
        public Button btnBrowseCustomYaml = null!;

        public CheckBox chkCss = null!;
        public TextBox txtCssPath = null!;
        public Button btnBrowseCss = null!;

        public CheckBox chkCover = null!;
        public ComboBox cmbCoverMode = null!;

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

            // ЗАХИСТ ВІД РОЗ'ЇЗДУ: Вимикаємо автоматичне системне масштабування для UserControl, 
            // оскільки ми вже розраховуємо всі SetBounds вручну через currentScale!
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
            var chk = new CheckBox { AutoSize = true };
            var txt = new TextBox { Enabled = false };
            var btn = new Button { Text = string.Empty, FlatStyle = FlatStyle.Flat, Enabled = false };
            btn.FlatAppearance.BorderSize = 0;

            // Викликаємо наш новий єдиний метод малювання (якщо він у Form1, додайте Form1. перед назвою)
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
            float currentScale = Win32Api.GetDpiScale();

            int blockMargin = (int)(10 * currentScale);// Відстань між блоками елементів, щоб вони не злипалися
            int labelHeight = (int)(20 * currentScale);// Висота текстових міток, щоб вони виглядали пропорційно до текстових полів
            int fieldHeight = (int)(24 * currentScale);// Висота текстових полів, щоб вони виглядали пропорційно до чекбоксів
            int checkBoxHeight = (int)(22 * currentScale);// Висота чекбоксів, щоб вони виглядали пропорційно до текстових полів
            int sidePadding = (int)(3 * currentScale);// Відступ зліва та справа для кнопок та текстових полів

            static Panel CreateRadioGroup(out RadioButton yes, out RadioButton no, float scale)
            {
                Panel p = new() { AutoSize = true, Enabled = false }; // Вимкнені за замовчуванням
                yes = new RadioButton { AutoSize = true, Location = new Point(0, 0), Text = "Yes" };
                no = new RadioButton { AutoSize = true, Location = new Point((int)(65 * scale), 0), Text = "No" };
                p.Controls.AddRange([yes, no]);
                return p;
            }

            // 1. Ініціалізація чистих контейнерів
            scrollMenuPanel = new Panel { AutoScroll = true };
            scrollMenuPanel.HorizontalScroll.Enabled = false; // ЗАБОРОНЯЄМО ГОРИЗОНТАЛЬНИЙ СКРОЛ
            scrollMenuPanel.HorizontalScroll.Visible = false;
            Controls.Add(scrollMenuPanel);

            grpOutName = new GroupBox { Text = "" };
            scrollMenuPanel.Controls.Add(grpOutName);

            // 2. Створення елементів верхнього блоку (у scrollMenuPanel)
            lblLang = new Label { Text = "Language:", AutoSize = true };
            langComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            langComboBox.Items.AddRange(["English", "Українська", "Русский"]);
            scrollMenuPanel.Controls.AddRange([lblLang, langComboBox]);

            btnDumpConfig = new Button();
            scrollMenuPanel.Controls.Add(btnDumpConfig);

            lblConfigName = new Label { AutoSize = true };
            txtConfigName = new TextBox { Text = "config.yaml" };
            scrollMenuPanel.Controls.AddRange([lblConfigName, txtConfigName]);

            var yamlGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCustomYaml = yamlGroup.CheckBox; txtCustomYamlPath = yamlGroup.TextBox; btnBrowseCustomYaml = yamlGroup.Button;

            // Налаштування CSS та кнопки з малюванням іконки
            var cssGroup = SetupToggleGroup(Properties.Resources.folder, scrollMenuPanel);
            chkCss = cssGroup.CheckBox; txtCssPath = cssGroup.TextBox; btnBrowseCss = cssGroup.Button;

            chkCover = new CheckBox { AutoSize = true }; // обкладинка
            cmbCoverMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            cmbCoverMode.Items.AddRange(["normal", "old_kindle", "flat"]);
            cmbCoverMode.SelectedIndex = 0;
            chkCover.CheckedChanged += (s, e) => { cmbCoverMode.Enabled = chkCover.Checked; (ParentForm as Form1)?.ApplyTheme(); };
            scrollMenuPanel.Controls.AddRange([chkCover, cmbCoverMode]);

            // Fix ZIP, Open From Cover, Translit (радіо-кнопки)
            chkFixZip = new CheckBox { AutoSize = true };
            Panel pnlFixZip = CreateRadioGroup(out rbFixZipYes, out rbFixZipNo, currentScale);

            chkOpenFromCover = new CheckBox { AutoSize = true };
            Panel pnlOpenCover = CreateRadioGroup(out rbOpenCoverYes, out rbOpenCoverNo, currentScale);

            chkTranslit = new CheckBox { AutoSize = true };
            Panel pnlTranslit = CreateRadioGroup(out rbTranslitYes, out rbTranslitNo, currentScale);

            // Fb2Name 
            chkFb2Name = new CheckBox { AutoSize = true };
            chkDefaultName = new CheckBox { AutoSize = true };

            // Додаємо всі створені елементи на панель
            scrollMenuPanel.Controls.AddRange([chkFixZip, pnlFixZip, chkOpenFromCover, pnlOpenCover, chkTranslit, pnlTranslit, chkFb2Name, chkDefaultName]);

            lblOutNameTitle = new Label();
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
                if (index > 0) cmbOutFields[index].Enabled = false;

                grpOutName.Controls.AddRange([cmbOutFields[index], chkAsFolder[index]]);
            }

            // ========================================================
            // ГЕОМЕТРІЯ ТА РОЗСТАНОВКА ЕЛЕМЕНТІВ ВСЕРЕДИНІ USERCONTROL
            // ========================================================
            int xLeft = (int)(16 * currentScale);// Відступ зліва для всіх елементів
            int fieldWidth = (int)(520 * currentScale) - (xLeft * 2) - (int)(8 * currentScale);// Враховуємо паддінг зліва та справа, а також невеликий запас для скролу

            // Задаємо базову висоту під центральний контент-контейнер Form1
            Size = new Size((int)(520 * currentScale), (int)(565 * currentScale));             // Висота UserControl, яка включає скролл-контейнер

            int scrollPanelHeight = (int)(545 * currentScale);                                 // Висота скролл-контейнера, яка включає всі елементи всередині
            scrollMenuPanel.Dock = DockStyle.Fill;

            int scrollFieldWidth = fieldWidth - (int)(3 * currentScale);
            int scrollRightField = fieldWidth + xLeft - (int)(3 * currentScale);

            int nextY = (int)(11 * currentScale);                                               // Початкова координата Y для першого елемента
            int textLabelWidth = (int)(240 * currentScale);                                     // Ширина текстових міток
            int valueFieldWidth = scrollFieldWidth - textLabelWidth - (int)(4 * currentScale);  // Ширина полів значень (ComboBox, TextBox) з урахуванням відступу між міткою та полем
            int radioX = xLeft + textLabelWidth + (int)(5 * currentScale);
            // Позиціонування елементів
            lblLang.SetBounds(xLeft, nextY, textLabelWidth, labelHeight);
            langComboBox.ItemHeight = fieldHeight - 6;
            langComboBox.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = langComboBox.Bottom + blockMargin + (int)(2 * currentScale);
            btnDumpConfig.SetBounds(xLeft + sidePadding, nextY, scrollFieldWidth - sidePadding, fieldHeight + (int)(4 * currentScale));

            nextY = btnDumpConfig.Bottom + blockMargin;
            lblConfigName.SetBounds(xLeft, nextY, textLabelWidth, labelHeight);
            txtConfigName.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = txtConfigName.Bottom + blockMargin;

            // 1. Блок Custom YAML (новий)
            chkCustomYaml.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
            int browseBtnWidth = (int)(55 * currentScale);
            int yamlTxtWidth = valueFieldWidth - browseBtnWidth - (int)(5 * currentScale) - sidePadding;
            txtCustomYamlPath.Multiline = true;
            txtCustomYamlPath.SetBounds(xLeft + textLabelWidth, nextY, yamlTxtWidth, fieldHeight);
            btnBrowseCustomYaml.SetBounds(scrollRightField - browseBtnWidth - sidePadding, nextY, browseBtnWidth, fieldHeight);

            // 2. Блок CSS (зсувається нижче)
            nextY = txtCustomYamlPath.Bottom + blockMargin;
            chkCss.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);

            int cssTxtWidth = valueFieldWidth - browseBtnWidth - (int)(5 * currentScale) - sidePadding;
            txtCssPath.Multiline = true;
            txtCssPath.SetBounds(xLeft + textLabelWidth, nextY, cssTxtWidth, fieldHeight);
            btnBrowseCss.SetBounds(scrollRightField - browseBtnWidth - sidePadding, nextY, browseBtnWidth, fieldHeight);

            nextY = txtCssPath.Bottom + blockMargin;
            chkCover.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            cmbCoverMode.ItemHeight = fieldHeight - 6;
            cmbCoverMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);

            nextY = cmbCoverMode.Bottom + blockMargin;
            // Позиціонуємо FixZip
            chkFixZip.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            pnlFixZip.SetBounds(radioX + (int)(80 * currentScale), nextY, (int)(150 * currentScale), fieldHeight);

            nextY = chkFixZip.Bottom + blockMargin;

            // Позиціонуємо OpenFromCover
            chkOpenFromCover.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            pnlOpenCover.SetBounds(radioX + (int)(80 * currentScale), nextY, (int)(150 * currentScale), fieldHeight);

            nextY = chkOpenFromCover.Bottom + blockMargin;

            // Позиціонуємо Translit
            chkTranslit.SetBounds(xLeft, nextY, textLabelWidth, checkBoxHeight);
            pnlTranslit.SetBounds(radioX + (int)(80 * currentScale), nextY, (int)(140 * currentScale), fieldHeight);


            nextY = chkTranslit.Bottom + blockMargin;
            // Позиціонуємо Fb2Name (Він займає всю ширину, бо не має радіобатонів)
            chkFb2Name.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            nextY = chkFb2Name.Bottom + blockMargin;
            chkDefaultName.SetBounds(xLeft, nextY, scrollFieldWidth, checkBoxHeight);

            // Налаштування групи структури назви файлу
            int OutNameTopPadding = (int)(10 * currentScale);// Відступ зверху для групи структури назви файлу
            int rowHeight = fieldHeight + (int)(5 * currentScale);// Висота одного рядка з комбо та чекбоксом
            int grpOutHeight = (rowHeight * 7) + (int)(25 * currentScale);// Висота групи з 8 рядків + заголовок групи
            grpOutName.SetBounds(xLeft, chkDefaultName.Bottom + OutNameTopPadding, fieldWidth, grpOutHeight);

            int comboWidth = (int)(grpOutName.Width * 0.76f);
            int checkFoldWidth = grpOutName.Width - comboWidth - (int)(15 * currentScale);
            int itemY = (int)(20 * currentScale);

            for (int i = 0; i < 7; i++)
            {
                if (cmbOutFields != null && chkAsFolder != null)
                {
                    cmbOutFields[i].ItemHeight = fieldHeight - 6;
                    cmbOutFields[i].SetBounds((int)(10 * currentScale), itemY, comboWidth, fieldHeight);
                    chkAsFolder[i].SetBounds(cmbOutFields[i].Right + (int)(15 * currentScale), itemY + (int)(1 * currentScale), checkFoldWidth, checkBoxHeight);
                    itemY += rowHeight;
                }
            }

            // Правильне роздільне створення об'єкта та встановлення його координат
            Label lblScrollAnchor = new() { BackColor = Color.Transparent };
            lblScrollAnchor.SetBounds(0, itemY + (int)(10 * currentScale), 1, 1);
            grpOutName.Controls.Add(lblScrollAnchor);
        }
    }
}
