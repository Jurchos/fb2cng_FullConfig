using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
namespace fb2cng_FullConfig.Utils
{
    internal static class UiStyles
    {
        // 1. Кешуємо Scale (це безпечно і швидко)
        private static float? _cachedScale;
        internal static float Scale => _cachedScale ??= Win32Api.GetDpiScale();

        // 2. Створюємо "невидимий" реєстр стилізованих кнопок. 
        // Він не заважає властивості Tag і автоматично чиститься, коли кнопка видаляється.
        private static readonly ConditionalWeakTable<Button, object> _styledButtons = [];

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
            public int ElementSpacing { get; }
            public int SidePadding { get; }
            public int RowHeight { get; }
            public int BrowseBtnWidth { get; }
            public int RadioGroupWidth { get; }
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
                BlockMargin = Scale(14);     // Відстань між блоками
                LabelHeight = Scale(20);     // Висота текстових міток
                FieldHeight = Scale(24);     // Висота полів введення
                CheckBoxHeight = Scale(22);  // Висота чекбоксів
                ElementSpacing = Scale(6);   // Масштабований відступ між полем і кнопкою
                SidePadding = Scale(3);      // Відступ з боків (права сторона) 
                RowHeight = Scale(30);       // Висота одного рядка (для розрахунку вертикального розташування)
                BrowseBtnWidth = Scale(55);  // Ширина кнопки "Папка"
                RadioGroupWidth = Scale(140);// Ширина групи радіокнопок
                IconSize = Scale(22);
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

                ContentHeight = Scale(630); // Висота контентної частини форми (між хідером і футером), важливо для розрахунку скролу

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
            if (btn == null || dependencyCheckBox == null) return;

            // прибрали Paint фону, залишаємо ТІЛЬКИ малювання іконки
            btn.Paint += (s, e) =>
            {
                if (icon != null)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    int paddingX = (int)(btn.Width * 0.24);
                    int paddingY = (int)(btn.Height * 0.12);
                    Rectangle destRect = new(paddingX, paddingY, btn.Width - (paddingX * 2), btn.Height - (paddingY * 2));

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
            dependencyCheckBox.CheckedChanged += (s, e) => btn.Invalidate();
        }

        internal static void MakeButtonRounded(Button btn, int radius)
        {
            //  Запобігаємо повторному накладанню подій, перевіряємо реєстр
            if (_styledButtons.TryGetValue(btn, out _))
            {
                return;
            }

            _styledButtons.Add(btn, new object());

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            void UpdateRegion()
            {
                if (btn.Width <= 0 || btn.Height <= 0) return;
                using GraphicsPath path = new();
                float r = radius;
                if (r > btn.Height / 2f) r = btn.Height / 2f;
                path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                path.AddArc(btn.Width - (r * 2), 0, r * 2, r * 2, 270, 90);
                path.AddArc(btn.Width - (r * 2), btn.Height - (r * 2), r * 2, r * 2, 0, 90);
                path.AddArc(0, btn.Height - (r * 2), r * 2, r * 2, 90, 90);
                path.CloseAllFigures();
                // ВАЖЛИВО: Звільняємо старий Region перед призначенням нового
                Region? oldRegion = btn.Region;
                btn.Region = new Region(path);
                oldRegion?.Dispose();
            }

            btn.HandleCreated += (s, e) => UpdateRegion();
            btn.SizeChanged += (s, e) => UpdateRegion();

            bool isHovered = false;
            btn.MouseEnter += (s, e) => { if (btn.Enabled) { isHovered = true; btn.Invalidate(); } };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };

            btn.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ev.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                bool isDark = Config.IsDarkTheme;

                using GraphicsPath path = new();
                float r = radius;
                path.AddArc(0, 0, r * 2, r * 2, 180, 90);
                path.AddArc(btn.Width - (r * 2), 0, r * 2, r * 2, 270, 90);
                path.AddArc(btn.Width - (r * 2), btn.Height - (r * 2), r * 2, r * 2, 0, 90);
                path.AddArc(0, btn.Height - (r * 2), r * 2, r * 2, 90, 90);
                path.CloseAllFigures();

                // 1. ДИНАМІЧНИЙ РОЗРАХУНОК ФОНУ
                Color baseColor = btn.BackColor;
                Color drawBg = baseColor;

                if (isHovered && btn.Enabled)
                {
                    if (isDark)
                    {
                        // В темній темі робимо трохи світлішим
                        drawBg = Color.FromArgb(
                            Math.Min(255, baseColor.R + 25),
                            Math.Min(255, baseColor.G + 25),
                            Math.Min(255, baseColor.B + 30));
                    }
                    else
                    {
                        // В світлій темі: якщо кнопка акцентна (синя/темна) - робимо світлішою
                        if (baseColor.R < 180 || baseColor.G < 180 || baseColor.B < 180)
                        {
                            drawBg = Color.FromArgb(
                                Math.Min(255, baseColor.R + 40),
                                Math.Min(255, baseColor.G + 40),
                                Math.Min(255, baseColor.B + 50));
                        }
                        else // Якщо кнопка звичайна сіра - стандартний колір наведення
                        {
                            drawBg = Color.FromArgb(225, 225, 225);
                        }
                    }
                }

                using (SolidBrush br = new(drawBg))
                {
                    ev.Graphics.FillPath(br, path);
                }

                // 2. ВМІСТ (Текст/Іконки)
                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                if (btn.Image != null)
                {
                    int imgW = btn.Image.Width;
                    int imgH = btn.Image.Height;

                    // ПЕРЕВІРКА: Якщо тексту немає (або лише пробіли), малюємо іконку по центру
                    if (string.IsNullOrWhiteSpace(btn.Text))
                    {
                        int startX = (btn.Width - imgW) / 2;
                        int startY = (btn.Height - imgH) / 2;
                        ev.Graphics.DrawImage(btn.Image, startX, startY, imgW, imgH);
                    }
                    else
                    {
                        // Якщо текст є, малюємо іконку + текст поруч
                        int spacing = GetScaled(4);
                        Size textSize = TextRenderer.MeasureText(btn.Text, btn.Font);
                        int totalW = imgW + spacing + textSize.Width;
                        int startX = (btn.Width - totalW) / 2;
                        int startY = (btn.Height - imgH) / 2;
                        ev.Graphics.DrawImage(btn.Image, startX, startY, imgW, imgH);

                        Rectangle textRect = new(startX + imgW + spacing, 0, textSize.Width, btn.Height);
                        TextRenderer.DrawText(ev.Graphics, btn.Text, btn.Font, textRect, btn.ForeColor, flags);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(btn.Text))
                {
                    TextRenderer.DrawText(ev.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor, flags);
                }

                // 3. РАМКА
                float penWidth = 1.1f;
                float offset = 0.5f;
                using GraphicsPath framePath = new();
                framePath.AddArc(offset, offset, r * 2, r * 2, 180, 90);
                framePath.AddArc(btn.Width - (r * 2) - offset, offset, r * 2, r * 2, 270, 90);
                framePath.AddArc(btn.Width - (r * 2) - offset, btn.Height - (r * 2) - offset, r * 2, r * 2, 0, 90);
                framePath.AddArc(offset, btn.Height - (r * 2) - offset, r * 2, r * 2, 90, 90);
                framePath.CloseAllFigures();

                Color borderColor;
                if (!btn.Enabled) borderColor = isDark ? Color.FromArgb(60, 60, 62) : Color.LightGray;
                else
                {
                    // Якщо кнопка акцентна - рамка має бути кольору тексту або прозорою
                    if (!isDark && baseColor.R < 180) borderColor = Color.FromArgb(100, baseColor);
                    else borderColor = isDark ? Color.FromArgb(85, 85, 90) : Color.DarkGray;
                }

                Form? parentForm = btn.FindForm();
                if (parentForm != null && parentForm.AcceptButton == btn && btn.Enabled && !isHovered)
                    borderColor = isDark ? Color.FromArgb(110, 110, 115) : Color.FromArgb(100, 100, 105);

                if (isHovered && btn.Enabled)
                    borderColor = isDark ? Color.FromArgb(150, 150, 155) : (baseColor.R < 180 ? baseColor : Color.FromArgb(0, 120, 215));

                using Pen pen = new(borderColor, penWidth);
                ev.Graphics.DrawPath(pen, framePath);
            };
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
