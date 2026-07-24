
namespace fb2cng_FullConfig.Utils
{
    public static class UiComponents
    {
        public static DialogResult ShowCustomMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using Form msgForm = new();

            bool isDark = Config.IsDarkTheme;
            bool isUa = Config.Settings.CurrentLanguage == "Ukrainian";

            msgForm.Text = caption;
            msgForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            msgForm.MaximizeBox = false;
            msgForm.MinimizeBox = false;
            msgForm.StartPosition = FormStartPosition.CenterScreen;
            msgForm.Font = new Font("Segoe UI", 10F);
            msgForm.BackColor = isDark ? Color.FromArgb(24, 24, 24) : Color.FromArgb(245, 245, 245);

            // --- 1. ВИЗНАЧАЄМО БАЗОВУ ШИРИНУ ТА СТВОРЮЄМО МЕТРИКИ ---
            int baseWidth = (icon != MessageBoxIcon.None) ? 360 : 330;
            // Передаємо поточний масштаб та потрібну ширину вікна повідомлення
            UiStyles.LayoutMetrics m = new(UiStyles.Scale, baseWidth);

            // --- 2. МАСШТАБОВАНІ ВІДСТУПИ ТА РОЗМІРИ ---
            int paddingTop = UiStyles.GetScaled(18);
            int paddingMiddle = UiStyles.GetScaled(15);
            int paddingBottom = UiStyles.GetScaled(12);
            int buttonHeight = UiStyles.GetScaled(26);
            int buttonWidth = UiStyles.GetScaled(85);

            msgForm.ClientSize = new Size(m.TotalSize.Width, msgForm.ClientSize.Height);

            PictureBox? picIcon = null;
            int msgIconSize = UiStyles.GetScaled(24);
            int textTopOffset = paddingTop;

            if (icon != MessageBoxIcon.None)
            {
                picIcon = new PictureBox
                {
                    Size = new Size(msgIconSize, msgIconSize),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point((msgForm.ClientSize.Width - msgIconSize) / 2, paddingTop)
                };

                Bitmap bmp = new(msgIconSize, msgIconSize);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // switch замість ланцюжка if-else
                    switch (icon)
                    {
                        case MessageBoxIcon.Error or MessageBoxIcon.Hand or MessageBoxIcon.Stop:
                            g.FillEllipse(Brushes.Crimson, 0, 0, msgIconSize - 1, msgIconSize - 1);
                            using (Pen pen = new(Color.White, 2.5f))
                            {
                                int offset = msgIconSize / 4;
                                g.DrawLine(pen, offset, offset, msgIconSize - offset, msgIconSize - offset);
                                g.DrawLine(pen, msgIconSize - offset, offset, offset, msgIconSize - offset);
                            }
                            break;

                        case MessageBoxIcon.Information or MessageBoxIcon.Asterisk:
                            Color infoColor = isDark ? Color.FromArgb(0, 140, 255) : Color.FromArgb(0, 102, 204);
                            using (Brush infoBrush = new SolidBrush(infoColor))
                            {
                                g.FillEllipse(infoBrush, 0, 0, msgIconSize - 1, msgIconSize - 1);
                                using Font infoFont = new("Georgia", 12F, FontStyle.Bold | FontStyle.Italic);
                                g.DrawString("i", infoFont, Brushes.White, new PointF(msgIconSize * 0.26f, msgIconSize * 0.08f));
                            }
                            break;

                        case MessageBoxIcon.Warning or MessageBoxIcon.Exclamation:
                            PointF[] points = [new(msgIconSize / 2f, 0), new(0, msgIconSize - 1), new(msgIconSize - 1, msgIconSize - 1)];
                            g.FillPolygon(Brushes.Orange, points);
                            using (Font warningFont = new("Segoe UI", 11F, FontStyle.Bold))
                            {
                                g.DrawString("!", warningFont, Brushes.White, new PointF(msgIconSize * 0.35f, msgIconSize * 0.18f));
                            }
                            break;

                        case MessageBoxIcon.Question:
                            // Спокійний синій круг
                            Color backColor = isDark ? Color.FromArgb(50, 100, 180) : Color.FromArgb(0, 90, 180);
                            using (Brush brush = new SolidBrush(backColor))
                            {
                                g.FillEllipse(brush, 0, 0, msgIconSize - 1, msgIconSize - 1);

                                // Малюємо стрілку "назад" білим кольором
                                using Pen whitePen = new(Color.White, 2.5f);
                                int mid = msgIconSize / 2;
                                g.DrawLine(whitePen, msgIconSize * 0.25f, mid, msgIconSize * 0.75f, mid);
                                g.DrawLine(whitePen, msgIconSize * 0.25f, mid, msgIconSize * 0.45f, mid - (msgIconSize * 0.2f));
                                g.DrawLine(whitePen, msgIconSize * 0.25f, mid, msgIconSize * 0.45f, mid + (msgIconSize * 0.2f));
                            }
                            break;

                        case MessageBoxIcon.None:
                            break;

                        default:
                            break;
                    }
                }

                picIcon.Image = bmp;
                msgForm.Controls.Add(picIcon);
                textTopOffset = picIcon.Bottom + UiStyles.GetScaled(6);
            }

            RichTextBox rtbText = new()
            {
                Text = text,
                Width = msgForm.ClientSize.Width - UiStyles.GetScaled(32),
                ForeColor = isDark ? Color.White : Color.Black,
                BackColor = msgForm.BackColor,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                TabStop = false,
                TabIndex = 99
            };

            rtbText.SelectAll();
            rtbText.SelectionAlignment = HorizontalAlignment.Center;
            rtbText.DeselectAll();
            rtbText.MouseDown += (s, e) => { _ = Win32Api.HideCaret(rtbText.Handle); _ = msgForm.Focus(); };
            rtbText.GotFocus += (s, e) => { _ = Win32Api.HideCaret(rtbText.Handle); };
            msgForm.Controls.Add(rtbText);

            int lastCharIndex = rtbText.TextLength > 0 ? rtbText.TextLength - 1 : 0;
            Point lastCharPos = rtbText.GetPositionFromCharIndex(lastCharIndex);
            int textHeight = lastCharPos.Y + rtbText.Font.Height + UiStyles.GetScaled(10);
            rtbText.Height = Math.Max(textHeight, UiStyles.GetScaled(40));
            rtbText.Location = new Point((msgForm.ClientSize.Width - rtbText.Width) / 2, textTopOffset);
            int buttonsY = rtbText.Bottom + paddingMiddle;
            Color btnBg = isDark ? Color.FromArgb(50, 50, 50) : Color.FromArgb(225, 228, 232);
            Color btnTextCol = isDark ? Color.White : Color.Black;
            Color accentBg = isDark ? Color.FromArgb(0, 102, 204) : Color.FromArgb(0, 120, 215);

            int primaryHeight = buttonHeight;
            int secondaryHeight = isDark ? buttonHeight : buttonHeight + UiStyles.GetScaled(2);

            Button? primaryButton = null;

            if (buttons == MessageBoxButtons.OK)
            {
                Button btnOkCustom = new()
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Size = new Size(buttonWidth, secondaryHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = accentBg,
                    ForeColor = Color.White,
                    TabIndex = 0
                };
                btnOkCustom.FlatAppearance.BorderSize = 0;

                // Викликаємо новий метод з UiStyles
                UiStyles.MsgBoxButtonStyles(btnOkCustom, m.BtnRadius, isDark, msgForm.BackColor);

                btnOkCustom.Location = new Point((msgForm.ClientSize.Width - btnOkCustom.Width) / 2, buttonsY);
                msgForm.Controls.Add(btnOkCustom);
                msgForm.AcceptButton = btnOkCustom;
                primaryButton = btnOkCustom;
            }
            else if (buttons is MessageBoxButtons.OKCancel or MessageBoxButtons.YesNo)
            {
                // Отримуємо словник для поточної мови інтерфейсу
                _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? loc);

                // Беремо готові значення зі словника, або ставимо англійські дефолти, якщо ключ раптом не знайдено
                string textConfirm = (buttons == MessageBoxButtons.YesNo)
                    ? (loc?.GetValueOrDefault("Yes", "Yes") ?? "Yes")
                    : (loc?.GetValueOrDefault("Ok", "OK") ?? "OK");

                string textCancel = (buttons == MessageBoxButtons.YesNo)
                    ? (loc?.GetValueOrDefault("No", "No") ?? "No")
                    : (loc?.GetValueOrDefault("Cancel", "Cancel") ?? "Cancel");

                Button btnPrimaryCustom = new()
                {
                    Text = textConfirm,
                    DialogResult = (buttons == MessageBoxButtons.YesNo) ? DialogResult.Yes : DialogResult.OK,
                    Size = new Size(buttonWidth, secondaryHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = accentBg,
                    ForeColor = Color.White,
                    TabIndex = 0
                };
                btnPrimaryCustom.FlatAppearance.BorderSize = 0;
                // Виклик нашого методу
                UiStyles.MsgBoxButtonStyles(btnPrimaryCustom, m.BtnRadius, isDark, msgForm.BackColor);

                Button btnSecondaryCustom = new()
                {
                    Text = textCancel,
                    DialogResult = (buttons == MessageBoxButtons.YesNo) ? DialogResult.No : DialogResult.Cancel,
                    Size = new Size(buttonWidth, buttonHeight),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = btnBg,
                    ForeColor = btnTextCol,
                    TabIndex = 1
                };
                btnSecondaryCustom.FlatAppearance.BorderSize = isDark ? 0 : 1;
                btnSecondaryCustom.FlatAppearance.BorderColor = isDark ? Color.FromArgb(80, 80, 80) : Color.FromArgb(200, 200, 200);
                UiStyles.MakeButtonRounded(btnSecondaryCustom, m.BtnRadius);

                int spacing = UiStyles.GetScaled(15);

                // ВИПРАВЛЕНО: беремо msgForm.ClientSize.Width для ідеального центрування обох кнопок
                int startX = (msgForm.ClientSize.Width - ((buttonWidth * 2) + spacing)) / 2;

                btnPrimaryCustom.Location = new Point(startX, buttonsY);
                btnSecondaryCustom.Location = new Point(startX + buttonWidth + spacing, buttonsY);

                msgForm.Controls.AddRange([btnPrimaryCustom, btnSecondaryCustom]);
                msgForm.AcceptButton = btnPrimaryCustom;
                msgForm.CancelButton = btnSecondaryCustom;
                primaryButton = btnPrimaryCustom;
            }

            msgForm.ClientSize = new Size(m.TotalSize.Width, (primaryButton?.Bottom ?? buttonsY) + paddingBottom);

            // Використовуємо PrimaryScreen, оскільки у статичному класі немає доступу до "this"
            Rectangle screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            msgForm.Location = new Point(
                screenBounds.Left + ((screenBounds.Width - msgForm.Width) / 2),
                screenBounds.Top + ((screenBounds.Height - msgForm.Height) / 2)
            );

            msgForm.StartPosition = FormStartPosition.Manual;
            msgForm.TopMost = true;

            msgForm.Shown += (s, e) =>
            {
                try
                {
                    IntPtr msgFormHandle = msgForm.Handle;
                    IntPtr foregroundWindowHandle = Win32Api.GetForegroundWindow();
                    uint foregroundThreadId = Win32Api.GetWindowThreadProcessId(foregroundWindowHandle, IntPtr.Zero);
                    uint currentThreadId = Win32Api.GetCurrentThreadId();

                    if (foregroundThreadId != currentThreadId && foregroundThreadId != 0)
                    {
                        // Додано дискарди _ = за рекомендацією Visual Studio для методів Win32Api
                        _ = Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, true);
                        _ = Win32Api.SetForegroundWindow(msgFormHandle);
                        _ = Win32Api.SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                        _ = Win32Api.AttachThreadInput(currentThreadId, foregroundThreadId, false);
                    }
                    else
                    {
                        _ = Win32Api.SetForegroundWindow(msgFormHandle);
                        _ = Win32Api.SetActiveWindow(msgFormHandle);
                        msgForm.Activate();
                    }
                }
                catch { }

                _ = (primaryButton?.Focus());
                _ = msgForm.BeginInvoke(new Action(() => { _ = Win32Api.HideCaret(rtbText.Handle); }));
            };

            return msgForm.ShowDialog();
        }
    }
}