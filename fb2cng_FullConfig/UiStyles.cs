using fb2cng_FullConfig;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public static class UiStyles
{
    internal static readonly float[][] InactiveIconMatrix = [
        [1, 0, 0, 0, 0],
        [0, 1, 0, 0, 0],
        [0, 0, 1, 0, 0],
        [0, 0, 0, 0.30f, 0],
        [0, 0, 0, 0, 1]
];

    public static void SetupIconButtonDrawing(Button btn, Image icon, CheckBox dependencyCheckBox, float[][] inactiveMatrix)
    {
        if (btn == null || dependencyCheckBox == null) return;

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
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

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
    public static Bitmap? ResizeImage(Image? img, int width, int height)
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


}