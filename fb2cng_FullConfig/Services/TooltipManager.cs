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
        // Забороняємо вікну забирати фокус введення на себе
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE: підказка не краде фокус клавіатури/миші
                cp.ExStyle |= 0x08000000;
                // WS_EX_TOOLWINDOW: підказка не з'являється у списку вікон Alt+Tab
                cp.ExStyle |= 0x00000080;
                return cp;
            }
        }
    }

    public static class TooltipManager
    {
        private static TipForm? _tipForm;
        private static string _currentText = string.Empty;
        private static Font? _currentFont;

        // Змінні керування затримкою
        // Постійний таймер для повторного використання (запобігає навантаженню на пам'ять)
        private static readonly System.Windows.Forms.Timer _delayTimer = new();
        private static Control? _pendingControl;
        private static string _pendingKey = string.Empty;

        private const int BasePadding = 6; // відступ для візуального комфорту
        private const int BaseGap = 5;
        private const float WidthRatio = 0.94f;

        static TooltipManager()
        {
            // Ініціалізуємо обробник таймера один раз
            _delayTimer.Tick += OnTimerTick;
        }

        public static void Attach(Control control, string key)
        {
            control.MouseEnter += (s, e) => OnMouseEnter(control, key);
            control.MouseLeave += (s, e) => HideTip();
        }

        private static void OnMouseEnter(Control control, string key)
        {
            // Зупиняємо попередній таймер, якщо мишка рухалася між елементами
            _delayTimer.Stop();

            _pendingControl = control;
            _pendingKey = key;

            // Динамічно зчитуємо затримку з конфігу, якщо значення в конфігу раптом <= 0, ставимо мінімум 10 мс, щоб уникнути помилки таймера
            int delay = Config.Settings.TooltipDelayMs > 0 ? Config.Settings.TooltipDelayMs : 10;
            _delayTimer.Interval = delay;
            _delayTimer.Start();
        }

        private static void OnTimerTick(object? sender, EventArgs e)
        {
            _delayTimer.Stop();

            if (_pendingControl != null && !_pendingControl.IsDisposed)
            {
                ShowTip(_pendingControl, _pendingKey);
            }
        }

        private static void ShowTip(Control owner, string key)
        {
            if (!Config.Settings.ShowTooltips && key != "ShowTooltips")
            {
                return;
            }

            string lang = Config.Settings.CurrentLanguage;
            if (!TooltipLocal.Dictionary.TryGetValue(lang, out Dictionary<string, string>? langDict) ||
               !langDict.TryGetValue(key, out string? text) ||
               string.IsNullOrEmpty(text))
            {
                return;
            }

            _currentText = text.Trim();

            if (_tipForm == null)
            {
                _tipForm = new TipForm();
                _tipForm.Paint += static (s, e) =>
                {
                    if (s is not TipForm f)
                    {
                        return;
                    }

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

                    // 2. Текст
                    int pad = UiStyles.GetScaled(BasePadding);
                    // Робимо прямокутник для тексту на 2 пікселі ширшим, щоб дати GDI+ запас для переносу
                    RectangleF textRect = new(pad, pad, f.Width - (pad * 2) + 2, f.Height - (pad * 2));

                    using SolidBrush textBrush = new(textColor);
                    using StringFormat sf = new(StringFormat.GenericTypographic)
                        {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near,
                        FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip  // Забороняє розрив слів
                    };

                    // Використовуємо підготовлений кешований шрифт
                    if (_currentFont != null)
                    {
                        e.Graphics.DrawString(_currentText, _currentFont, textBrush, textRect, sf);
                    }
                };
            }

            // Переконуємось, що дескриптор створений для коректного вимірювання
            if (!_tipForm.IsHandleCreated)
            {
                _tipForm.CreateControl();
            }

            bool isDark = Config.IsDarkTheme;
            _tipForm.BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.FromArgb(255, 255, 240);

            Form? mainForm = owner.FindForm();
            if (mainForm == null)
            {
                return;
            }

            // Оновлюємо шрифт
            _currentFont?.Dispose();
            _currentFont = new Font("Segoe UI", 10.5F);

            // РОЗРАХУНОК РОЗМІРІВ
            int scaledPadding = UiStyles.GetScaled(BasePadding);
            int maxWidth = (int)(mainForm.ClientSize.Width * WidthRatio);
            int textWidthLimit = maxWidth - (scaledPadding * 2);

            using Graphics g = _tipForm.CreateGraphics();

            // Використовуємо StringFormat для точного вимірювання
            using StringFormat sfMeasure = new(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.LineLimit
            };

            // Вимірюємо з невеликим запасом
            SizeF sizeF = g.MeasureString(_currentText, _currentFont, textWidthLimit, sfMeasure);

            // Округляємо вгору і додаємо падінги
            int finalHeight = (int)Math.Ceiling(sizeF.Height) + (scaledPadding * 2) + (int)(2 * UiStyles.Scale);
            _tipForm.Size = new Size(maxWidth, finalHeight);

            // РОЗРАХУНОК ПОЗИЦІЇ ТА ОБРОБКА МЕЖ ЕКРАНА (MULTI-MONITOR SUPPORT)
            Rectangle screen = Screen.FromControl(owner).WorkingArea;

            // ЦЕНТРУВАННЯ
            int windowBorder = (mainForm.Width - mainForm.ClientSize.Width) / 2;
            int targetX = mainForm.Location.X + windowBorder + ((mainForm.ClientSize.Width - _tipForm.Width) / 2);

            // Захист по осі X (щоб підказка не вилазила за екран)
            if (targetX < screen.Left)
            {
                targetX = screen.Left;
            }
            else if (targetX + _tipForm.Width > screen.Right)
            {
                targetX = screen.Right - _tipForm.Width;
            }

            // Позиція по осі Y (спочатку намагаємося розмістити над елементом)
            Point ownerScreenPos = owner.PointToScreen(Point.Empty);
            int gap = UiStyles.GetScaled(BaseGap);
            int targetY = ownerScreenPos.Y - _tipForm.Height - gap;

            // Якщо виходить за верхню межу екрана — переносимо під елемент
            if (targetY < screen.Top)
            {
                targetY = ownerScreenPos.Y + owner.Height + gap;
            }

            // Якщо виходить і за нижню межу (наприклад, панель завдань) — притискаємо до низу
            if (targetY + _tipForm.Height > screen.Bottom)
            {
                targetY = screen.Bottom - _tipForm.Height;
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
            _delayTimer.Stop(); // Скасовуємо запланований показ, якщо мишка пішла раніше часу
            _pendingControl = null;
            _tipForm?.Hide();
        }
    }
}