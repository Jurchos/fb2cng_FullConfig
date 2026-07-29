using System.Drawing.Drawing2D;
namespace fb2cng_FullConfig.Utils
{
    internal static class UiStyles
    {
        internal static float Scale => Win32Api.GetDpiScale();
        internal static int GetScaled(int value)
        {
            return (int)(value * Scale);
        }

        internal static readonly float[][] InactiveIconMatrix = [
            [1, 0, 0, 0, 0],
            [0, 1, 0, 0, 0],
            [0, 0, 1, 0, 0],
            [0, 0, 0, 0.30f, 0],
            [0, 0, 0, 0, 1]
         ];

        // Заборона горизонтального скролу для будь-якої вкладки, яка містить вертикальний скрол.
        internal static void DisableHorizontalScroll(Panel panel)
        {
            if (panel == null)
            {
                return;
            }

            // Вмикаємо стандартний автоскрол (для вертикальної прокрутки)
            panel.AutoScroll = true;

            // Константа 0 означає горизонтальний скрол (SB_HORZ)
            const int sbHorz = 0;

            // Підписуємося на події, щоб ховати скрол при зміні розмірів вікна або додаванні нових елементів
            panel.SizeChanged += (s, e) => Win32Api.ShowScrollBar(panel.Handle, sbHorz, false);
            panel.ControlAdded += (s, e) => Win32Api.ShowScrollBar(panel.Handle, sbHorz, false);

            // Ховаємо скрол відразу, якщо дескриптор вікна вже створено
            if (panel.IsHandleCreated)
            {
                _ = Win32Api.ShowScrollBar(panel.Handle, sbHorz, false);
            }
            else
            {
                panel.HandleCreated += (s, e) => { _ = Win32Api.ShowScrollBar(panel.Handle, sbHorz, false); };
            }
        }

        // Один розрахунок для всіх вкладок
        internal class LayoutMetrics
        {
            // Базові висоти та відступи
            public int BaseWidth { get; }
            public int ScaledWidth { get; }
            public Size TotalSize { get; }
            public int BlockMargin { get; }
            public int LabelHeight { get; }
            public int FieldHeight { get; }
            public int CheckBoxHeight { get; }
            public int SidePadding { get; }
            public int RowHeight { get; }
            public int BrowseBtnWidth { get; }
            public int IconSize { get; }
            public int BtnRadius { get; }
            public int XLeft { get; }
            public int StartY { get; }
            public int TextLabelWidth { get; }
            public int FieldWidth { get; }
            public int ValueFieldWidth { get; }
            public int SizeInputX { get; }
            public int RadioX { get; }
            // змінні, перенесені з Form1.cs ---
            public int HeaderTopPadding { get; }
            public int HeaderRowHeight { get; }
            public int HeaderHeight { get; }
            public int HeaderPaddingLeft { get; }
            public int HeaderPaddingRight { get; }
            public int BetweenButtons { get; }
            public int ContentHeight { get; }
            public int FooterHeight { get; }
            public int FooterBtnWidth { get; }
            public int FooterGuiBtnWidth { get; }
            public int FooterBtnHeight { get; }
            public int FooterBtnTop { get; }

            // Конструктор сам все розраховує на основі переданого базового масштабу та ширини
            public LayoutMetrics(float scale, int baseWidth = 520)
            {
                // Локальна функція, щоб не писати всюди (int)(X * scale)
                int Scale(float value)
                {
                    return (int)(value * scale);
                }

                BaseWidth = baseWidth;
                ScaledWidth = Scale(baseWidth);
                TotalSize = new Size(ScaledWidth, Scale(565)); // Загальний розмір форми -заготовка, в основному для розрахунку CustomMessageBox
                BlockMargin = Scale(10);     // Відстань між блоками
                LabelHeight = Scale(20);     // Висота текстових міток
                FieldHeight = Scale(24);     // Висота полів введення
                CheckBoxHeight = Scale(22);  // Висота чекбоксів
                SidePadding = Scale(3);      // Відступ з боків (права сторона) 
                RowHeight = Scale(32);       // Висота одного рядка (для розрахунку вертикального розташування)
                BrowseBtnWidth = Scale(55);  // Ширина кнопки "Папка"
                IconSize = Scale(17);
                BtnRadius = Scale(4);

                XLeft = Scale(16);           // Лівий відступ для всіх елементів
                StartY = Scale(12);           // Початкова координата Y для першого елемента
                TextLabelWidth = Scale(245);  // Ширина текстових міток для всіх елементів
                FieldWidth = ScaledWidth - (XLeft * 2) - Scale(8); // Ширина полів введення
                ValueFieldWidth = FieldWidth - TextLabelWidth - Scale(7); // Ширина полів введення для всіх елементів, що мають текстові мітки
                SizeInputX = XLeft + TextLabelWidth;  // Координата X для полів введення, що мають текстові мітки
                RadioX = XLeft + TextLabelWidth + Scale(60); // Координата X для радіокнопок, що мають текстові мітки
                // Розрахунки для Form1.cs
                HeaderTopPadding = Scale(4);
                HeaderRowHeight = Scale(28);// Висота ряду кнопок хідера
                HeaderHeight = HeaderRowHeight + Scale(8);
                HeaderPaddingLeft = Scale(13); // Відступ зліва для першої кнопки
                HeaderPaddingRight = Scale(14); // Відступ справа для останньої кнопки
                BetweenButtons = Scale(6);

                ContentHeight = Scale(580); // Висота контентної частини форми (між хідером і футером), важливо для розрахунку скролу

                FooterHeight = Scale(24) + Scale(14);// Висота футера з урахуванням відступів
                FooterBtnWidth = Scale(90);
                FooterGuiBtnWidth = Scale(65);
                FooterBtnHeight = Scale(24) + Scale(4);// Висота кнопок футера з урахуванням відступів
                FooterBtnTop = Scale(5);// Відступ зверху для кнопок футера
            }
        }
        // --- Уніфікована фабрика радіобатонів ---
        internal static Panel CreateRadioGroup(
            out RadioButton rbLeft,
            out RadioButton rbRight,
            string leftText = "Yes",
            string rightText = "No",
            int widthBetweenButtons = 80)
        {
            Panel p = new() { AutoSize = true, Enabled = false };
            rbLeft = new RadioButton { AutoSize = true, Location = new Point(0, 0), Text = leftText };
            int secondButtonX = (int)(widthBetweenButtons * Scale);
            rbRight = new RadioButton { AutoSize = true, Location = new Point(secondButtonX, 0), Text = rightText };
            p.Controls.AddRange([rbLeft, rbRight]);
            return p;
        }

        internal static void SetupIconButtonDrawing(Button btn, Image icon, CheckBox dependencyCheckBox, float[][] inactiveMatrix)
        {
            if (btn == null || dependencyCheckBox == null)
            {
                return;
            }

            btn.Tag = false; // Ставимо початковий стан Hovered у Tag

            // Обробка подій миші для ефекту наведення
            btn.EnabledChanged += (s, e) => { if (!btn.Enabled) { btn.Tag = false; btn.Invalidate(); } };
            btn.MouseEnter += (s, e) => { if (btn.Enabled) { btn.Tag = true; btn.Invalidate(); } };
            btn.MouseLeave += (s, e) => { btn.Tag = false; btn.Invalidate(); };

            // Універсальна подія Paint
            btn.Paint += (s, e) =>
            {
                bool isHovered = (bool)btn.Tag;

                // 1. Малювання кастомного фону при наведенні
                if (isHovered && btn.Enabled)
                {
                    Color baseBgColor = btn.BackColor;
                    bool isDark = baseBgColor.R < 128;
                    Color drawBgColor = isDark
                        ? Color.FromArgb(baseBgColor.R + 25, baseBgColor.G + 25, baseBgColor.B + 25)
                        : Color.FromArgb(baseBgColor.R - 20, baseBgColor.G - 20, baseBgColor.B - 20);

                    using Brush backBrush = new SolidBrush(drawBgColor);
                    e.Graphics.FillRectangle(backBrush, 0, 0, btn.Width, btn.Height);
                }

                // 2. Малювання іконки папки з точними налаштуваннями якості
                if (icon != null)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    int paddingX = (int)(btn.Width * 0.24);
                    int paddingY = (int)(btn.Height * 0.12);
                    Rectangle destRect = new(paddingX, paddingY, btn.Width - (paddingX * 2), btn.Height - (paddingY * 2));

                    // Якщо чекбокс НЕ активний — малюємо сіру/прозору іконку за вашою матрицею
                    if (!dependencyCheckBox.Checked)
                    {
                        using System.Drawing.Imaging.ImageAttributes imageAttributes = new();
                        imageAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(inactiveMatrix));
                        e.Graphics.DrawImage(icon, destRect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                    else
                    {
                        e.Graphics.DrawImage(icon, destRect);
                    }
                }
            };

            // Перемальовуємо кнопку при зміні стану чекбокса, щоб іконка миттєво змінювала яскравість
            dependencyCheckBox.CheckedChanged += (s, e) => btn.Invalidate();
        }

        // Масштабує вхідне зображення до заданих розмірів з високою якістю рендерингу.
        internal static Bitmap? ResizeImage(Image? img, int width, int height)
        {
            if (img is null)
            {
                return null;
            }

            Bitmap? bmp = new(width, height);

            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;                  // Увімкнення згладжування ліній та країв
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic; // Бікубічна інтерполяція для чіткості при зміні розміру
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;            // Оптимальне зміщення пікселів для усунення розмиття

                    // Малюємо оригінальну картинку (img) на нашому новому бітмапі, розтягуючи її від лівого верхнього кута (0,0) до нових меж (width, height)
                    g.DrawImage(img, 0, 0, width, height);
                }
                return bmp;
            }
            catch
            {
                // Захист від витоку пам'яті 
                bmp?.Dispose();
                // Прокидаємо помилку далі по стеку викликів, щоб програма знала про збій
                throw;
            }
        }

        internal static void MakeButtonRounded(Button btn, int radius)
        {
            btn.FlatStyle = FlatStyle.Flat; // ОБОВ'ЯЗКОВО
            btn.FlatAppearance.BorderSize = 0;

            // Крок 1. Надійний Region
            using (GraphicsPath path = new())
            {
                float r = radius;
                path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                path.AddArc(btn.Width - (r * 2), 0, r * 2, r * 2, 270, 90);
                path.AddArc(btn.Width - (r * 2), btn.Height - (r * 2), r * 2, r * 2, 0, 90);
                path.AddArc(0, btn.Height - (r * 2), r * 2, r * 2, 90, 90);
                path.CloseAllFigures();

                btn.Region = new Region(path);
            }

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            // Додаємо змінні для світлої теми з перевіркою Enabled (захист від багу при старті)
            bool isHovered = false;
            btn.MouseEnter += (s, e) => { if (btn.Enabled) { isHovered = true; btn.Invalidate(); } };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };

            // Якщо під час зміни Enabled кнопка була під мишкою, скидаємо стан підсвічування
            btn.EnabledChanged += (s, e) => { if (!btn.Enabled) { isHovered = false; btn.Invalidate(); } };

            // Крок 2. Малювання рамки
            btn.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                bool isDarkTheme = Config.IsDarkTheme;

                if (isDarkTheme)
                {
                    // ДЛЯ ТЕМНОЇ ТЕМИ
                    using GraphicsPath buttonFramePath = new();
                    float r = radius;
                    float startXY = 0.5f;
                    float sizeAdjustment = 1.0f;

                    buttonFramePath.AddArc(startXY, startXY, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, startXY, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(0, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    // Якщо кнопка вимкнена в темній темі, робимо рамку тьмяною
                    // 1. Спочатку визначаємо стандартний колір рамки для активної кнопки
                    Color activeBorderColor = btn.FlatAppearance.BorderColor != Color.Empty && btn.FlatAppearance.BorderColor != Color.Transparent
                        ? btn.FlatAppearance.BorderColor
                        : btn.ForeColor;

                    // 2. Тепер легко і читабельно робимо вибір залежно від стану кнопки
                    Color btnBorderColor = !btn.Enabled
                        ? Color.FromArgb(70, Color.Gray)
                        : activeBorderColor;
                    using Pen pen = new(btnBorderColor, 1.2F);
                    ev.Graphics.DrawPath(pen, buttonFramePath);
                }
                else
                {
                    // ДЛЯ СВІТЛОЇ ТЕМИ
                    using GraphicsPath buttonFramePath = new();
                    float r = radius;
                    float startXY = 0.5f;
                    float sizeAdjustment = 1.0f;

                    buttonFramePath.AddArc(startXY, startXY, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, startXY, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2) - sizeAdjustment, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(0, btn.Height - (r * 2) - sizeAdjustment, r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    Color btnBorderColor;
                    if (!btn.Enabled)
                    {
                        btnBorderColor = Color.LightGray;
                    }
                    else if (isHovered)
                    {
                        btnBorderColor = Color.FromArgb(0, 120, 215); // Підсвічування при наведенні
                    }
                    else
                    {
                        btnBorderColor = btn.FlatAppearance.BorderColor != Color.Empty && btn.FlatAppearance.BorderColor != Color.Transparent
                            ? btn.FlatAppearance.BorderColor
                            : Color.DarkGray;
                    }

                    using Pen pen = new(btnBorderColor, 1.0F);
                    ev.Graphics.DrawPath(pen, buttonFramePath);
                }
            };
        }
        internal static void MsgBoxButtonStyles(Button btn, int radius, bool isDark, Color formBackColor)
        {
            if (btn == null)
            {
                return;
            }

            // 1. Спочатку викликаємо базове округлення, яке вже є в UiStyles
            MakeButtonRounded(btn, radius);

            // 2. Якщо це світла тема і кнопка акцентна (синя з білим текстом) — маскуємо драбинку
            if (!isDark && btn.ForeColor == Color.White)
            {
                btn.Paint += (s, ev) =>
                {
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using GraphicsPath buttonFramePath = new();
                    float r = radius;

                    // Будуємо контур чітко по краях кнопки
                    buttonFramePath.AddArc(0, 0, r * 2, r * 2, 180, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2), 0, r * 2, r * 2, 270, 90);
                    buttonFramePath.AddArc(btn.Width - (r * 2), btn.Height - (r * 2), r * 2, r * 2, 0, 90);
                    buttonFramePath.AddArc(0, btn.Height - (r * 2), r * 2, r * 2, 90, 90);
                    buttonFramePath.CloseAllFigures();

                    // Шар 1: Товста підкладка кольору фону форми для зачистки бруду
                    using (Pen bgPen = new(formBackColor, 2.5F))
                    {
                        ev.Graphics.DrawPath(bgPen, buttonFramePath);
                    }

                    // Шар 2: М'яка напівпрозора біла лінія для ідеального згладжування краю
                    using Pen overlayPen = new(Color.FromArgb(160, Color.White), 2.2F);
                    ev.Graphics.DrawPath(overlayPen, buttonFramePath);
                };
            }
        }
    }
}
