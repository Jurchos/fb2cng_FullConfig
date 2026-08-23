using fb2cng_FullConfig.Utils;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace fb2cng_FullConfig.Services
{
    internal class TipForm : Form
    {
        public TipForm()
        {
            DoubleBuffered = true;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.None;
        }
    }

    public static class TooltipManager
    {
        private static TipForm? _tipForm;
        private static string _currentText = string.Empty;

        private const int BasePadding = 8; // відступ для візуального комфорту
        private const int BaseGap = 5;
        private const float WidthRatio = 0.94f;

        public static void Attach(Control control, string key)
        {
            control.MouseEnter += (s, e) => ShowTip(control, key);
            control.MouseLeave += (s, e) => HideTip();
        }

        private static void ShowTip(Control owner, string key)
        {
            if (!Config.Settings.ShowTooltips && key != "ShowTooltips")
            {
                return;
            }

            string lang = Config.Settings.CurrentLanguage;
            if (!(TooltipLocal.Dictionary.ContainsKey(lang) &&
                TooltipLocal.Dictionary[lang].TryGetValue(key, out string? text)))
            {
                return;
            }

            _currentText = text;

            if (_tipForm == null)
            {
                _tipForm = new TipForm();
                _tipForm.Paint += static (s, e) =>
                {
                    if (s is TipForm f)
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        // Налаштування для максимально чіткого тексту в GDI+
                        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                        bool isDark = Config.IsDarkTheme;
                        Color textColor = isDark ? Color.FromArgb(230, 230, 230) : Color.Black;
                        Color borderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.DarkGray;

                        // 1. Рамка
                        using (Pen p = new(borderColor, 1))
                        {
                            e.Graphics.DrawRectangle(p, 0, 0, f.Width - 1, f.Height - 1);
                        }

                        // 2. Текст через GDI+ (DrawString)
                        int pad = UiStyles.GetScaled(BasePadding);
                        // Робимо прямокутник для тексту на 2 пікселі ширшим, щоб дати GDI+ запас для переносу
                        RectangleF textRect = new(pad, pad, f.Width - (pad * 2) + 2, f.Height - (pad * 2));

                        using Font textFont = new("Segoe UI", 10.5F);
                        using SolidBrush textBrush = new(textColor);
                        using StringFormat sf = new(StringFormat.GenericTypographic)
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip  // Забороняє розрив слів
                        };

                        e.Graphics.DrawString(_currentText, textFont, textBrush, textRect, sf);
                    }
                };
            }

            bool isDark = Config.IsDarkTheme;
            _tipForm.BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.FromArgb(255, 255, 240);

            Form? mainForm = owner.FindForm();
            if (mainForm == null)
            {
                return;
            }

            // РОЗРАХУНОК РОЗМІРІВ
            int scaledPadding = UiStyles.GetScaled(BasePadding);
            int maxWidth = (int)(mainForm.ClientSize.Width * WidthRatio);
            int textWidthLimit = maxWidth - (scaledPadding * 2);

            using Font measureFont = new("Segoe UI", 10.5F);
            using Graphics g = _tipForm.CreateGraphics();

            // Використовуємо StringFormat для точного вимірювання
            using StringFormat sfMeasure = new(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.LineLimit
            };

            // Вимірюємо з невеликим запасом
            SizeF sizeF = g.MeasureString(_currentText, measureFont, textWidthLimit, sfMeasure);

            // Округляємо вгору і додаємо падінги
            int finalHeight = (int)Math.Ceiling(sizeF.Height) + (scaledPadding * 2) + (int)(4 * UiStyles.Scale);
            _tipForm.Size = new Size(maxWidth, finalHeight);

            // ЦЕНТРУВАННЯ
            int windowBorder = (mainForm.Width - mainForm.ClientSize.Width) / 2;
            int targetX = mainForm.Location.X + windowBorder + ((mainForm.ClientSize.Width - _tipForm.Width) / 2);

            Point ownerScreenPos = owner.PointToScreen(Point.Empty);
            int targetY = ownerScreenPos.Y - _tipForm.Height - UiStyles.GetScaled(BaseGap);

            // ТОЧКОВЕ ВИПРАВЛЕННЯ:
            // Якщо координата Y виходить за верхню межу екрана (0), 
            // переносимо підказку під контрол
            if (targetY < 0)
            {
                targetY = ownerScreenPos.Y + owner.Height + UiStyles.GetScaled(BaseGap);
            }

            _tipForm.Location = new Point(targetX, targetY);

            if (!_tipForm.Visible)
            {
                _tipForm.Show();
            }
            else
            {
                _tipForm.Invalidate();
            }
        }

        private static void HideTip()
        {
            _tipForm?.Hide();
        }
    }
}