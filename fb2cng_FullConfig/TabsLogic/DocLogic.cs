using System.Diagnostics;
using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;
using fb2cng_FullConfig.Utils;
using static fb2cng_FullConfig.Utils.UiComponents;

namespace fb2cng_FullConfig
{
    public partial class Form1
    {
        //=====================================================-========
        // --- 1. Ініціалізація подій вкладок (Initialize...Events) ---
        private void InitializeDocumentTabEvents(DocumentTab docTab)
        {
            // 1. ЗАОКРУГЛЕННЯ ДЛЯ ВСІХ КНОПОК
            UiStyles.MakeButtonRounded(docTab.btnBrowseCss, UiStyles.GetScaled(4));
            UiStyles.MakeButtonRounded(docTab.btnDumpConfig, UiStyles.GetScaled(4));
            UiStyles.MakeButtonRounded(docTab.btnBrowseCustomYaml, UiStyles.GetScaled(4));
            UiStyles.MakeButtonRounded(docTab.btnReset, UiStyles.GetScaled(4));

            // 2. ПРИВ'ЯЗКА КЛІКІВ
            docTab.btnBrowseCss.Click += BtnBrowseCss_Click;
            docTab.btnBrowseCustomYaml.Click += BtnBrowseCustomYaml_Click;
            docTab.btnDumpConfig.Click += BtnDumpConfig_Click;
            docTab.btnReset.Click += BtnReset_Click;

            // Налаштування малювання іконок для кнопок Папка
            UiStyles.SetupIconButtonDrawing(
            docTab.btnBrowseCss,
            Properties.Resources.folder,
            docTab.chkCss,
            UiStyles.InactiveIconMatrix
            );

            UiStyles.SetupIconButtonDrawing(
            docTab.btnBrowseCustomYaml,
            Properties.Resources.folder,
            docTab.chkCustomYaml,
            UiStyles.InactiveIconMatrix
            );
            // 3. РЕШТА ПОДІЙ
            docTab.langComboBox.SelectedIndexChanged += LangComboBox_SelectedIndexChanged;
            docTab.chkFb2Name.CheckedChanged += ChkFb2Name_CheckedChanged;
            docTab.chkDefaultName.CheckedChanged += ChkDefaultName_CheckedChanged;

            // Синхронізація при активації CSS
            docTab.chkCss.CheckedChanged += (s, e) =>
            {
                if (docTab.chkCss.Checked)
                {
                    SyncCssWithCustomYaml(docTab);
                }
                ApplyTheme();
            };
            // Синхронізація імені конфігу при зміні стану чекбокса
            docTab.chkCustomYaml.CheckedChanged += (s, e) =>
            {
                SyncConfigNameWithYaml(docTab);
                SyncCssWithCustomYaml(docTab);
                SyncTocTypeWithCustomYaml(docTab);
                SyncBinarySettingsWithYaml(docTab);
                SyncMetadataWithYaml(docTab);
                SyncLoggingSettingsWithYaml(docTab);
                // Також викликаємо оновлення теми, бо зміна стану чекбокса впливає на візуал
                ApplyTheme();
            };

            // Додаємо обробник для зміни шляху YAML (якщо змінили файл, оновлюємо CSS)
            docTab.txtCustomYamlPath.TextChanged += (s, e) =>
            {
                SyncConfigNameWithYaml(docTab);
                SyncCssWithCustomYaml(docTab); // Можна теж додати про всяк випадок
                SyncTocTypeWithCustomYaml(docTab);
                SyncBinarySettingsWithYaml(docTab);
                SyncMetadataWithYaml(docTab);
                SyncLoggingSettingsWithYaml(docTab);
            };

            if (docTab.cmbOutFields != null)
            {
                for (int i = 0; i < docTab.cmbOutFields.Length; i++)
                {
                    int index = i;
                    docTab.cmbOutFields[i].SelectedIndexChanged += (s, e) => CmbOutFields_SelectedIndexChanged(index);
                }
            }
            // fix_zip
            docTab.chkFixZip.CheckedChanged += (s, e) =>
            {
                if (docTab.rbFixZipYes?.Parent != null)
                {
                    docTab.rbFixZipYes.Parent.Enabled = docTab.chkFixZip.Checked;
                }

                ApplyTheme();
            };
            docTab.chkOpenFromCover.CheckedChanged += (s, e) =>
            {
                if (docTab.rbOpenCoverYes?.Parent != null)
                {
                    docTab.rbOpenCoverYes.Parent.Enabled = docTab.chkOpenFromCover.Checked;
                }

                ApplyTheme();
            };
            docTab.chkTranslit.CheckedChanged += (s, e) =>
            {
                if (docTab.rbTranslitYes?.Parent != null)
                {
                    docTab.rbTranslitYes.Parent.Enabled = docTab.chkTranslit.Checked;
                }

                ApplyTheme();
            };
            TooltipManager.Attach(docTab.lblConfigName, "ConfigName");
            TooltipManager.Attach(docTab.chkCustomYaml, "CustomYamlEnable");
            TooltipManager.Attach(docTab.chkCss, "CssEnable");
            TooltipManager.Attach(docTab.chkCover, "TocType");
            TooltipManager.Attach(docTab.chkFixZip, "FixZip");
            TooltipManager.Attach(docTab.chkOpenFromCover, "OpenCover");
            TooltipManager.Attach(docTab.chkTranslit, "Translit");
            TooltipManager.Attach(docTab.chkFb2Name, "Fb2Name");
            TooltipManager.Attach(docTab.chkDefaultName, "DefaultName");
            TooltipManager.Attach(docTab.grpOutName, "OutNameTitle");
        }

        //=========================================================
        // --- 2. Обробники дій вкладок (Dump, Browse, Reset) ---
        internal async void BtnDumpConfig_Click(object? sender, EventArgs e)
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                if (!YamlService.IsEngineAvailable())
                {
                    Config.LogError("DumpConfig failed: fbc.exe is missing in the application folder.");
                    string caption = langDict?.GetValueOrDefault("ErrTitle", "Error") ?? "Error";
                    string text = langDict?.GetValueOrDefault("ErrFbc", "Error: fbc.exe not found!") ?? "Error: fbc.exe not found!";

                    _ = ShowCustomMessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Кнопка дампу тепер береться з об'єкта docTab
                docTab.btnDumpConfig.Enabled = false;
                string prevText = docTab.btnDumpConfig.Text;
                docTab.btnDumpConfig.Text = Config.Settings.CurrentLanguage == "Ukrainian" ? "Генерація..." : "Generating...";

                bool success = await Task.Run(YamlService.ExecuteSyncDumpConfig);

                docTab.btnDumpConfig.Text = prevText;
                docTab.btnDumpConfig.Enabled = true;

                if (success)
                {
                    // Оновлюємо інтерфейс відразу після генерації файлу
                    SyncTocTypeWithCustomYaml(docTab);
                    SyncBinarySettingsWithYaml(docTab);
                    SyncLoggingSettingsWithYaml(docTab);
                    SyncMetadataWithYaml(docTab); // Тепер метадані оновляться при створенні файлу

                    string caption = langDict?.GetValueOrDefault("GenTitle", "Success") ?? "Success";
                    string msg = langDict?.GetValueOrDefault("GenSuccess", "{0} successfully generated!") ?? "{0} successfully generated!";
                    msg = string.Format(msg, Config.DefaultConfigPath);

                    _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            _ = btnOk.Focus();
        }

        internal void BtnBrowseCustomYaml_Click(object? sender, EventArgs e)
        {
            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                if (!docTab.chkCustomYaml.Checked)
                {
                    return;
                }

                using OpenFileDialog ofd = new();
                ofd.Filter = "YAML Files (*.yaml;*.yml)|*.yaml;*.yml|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string selectedFile = ofd.FileName;
                    string relativePath = Path.GetRelativePath(appPath, selectedFile);
                    // Встановлюємо шлях
                    docTab.txtCustomYamlPath.Text = relativePath.Replace('\\', '/');
                    // ЯВНО ВИКЛИКАЄМО СИНХРОНІЗАЦІЮ ТУТ
                    SyncConfigNameWithYaml(docTab);
                }
            }
            _ = btnOk.Focus();
        }

        internal void BtnBrowseCss_Click(object? sender, EventArgs e)
        {
            // Дістаємо посилання на вкладку документа для роботи з її полями
            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                if (!docTab.chkCss.Checked)
                {
                    return;
                }

                using OpenFileDialog ofd = new();
                ofd.Filter = "CSS Files (*.css)|*.css";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string selectedFile = ofd.FileName;
                    string relativePath = Path.GetRelativePath(appPath, selectedFile);
                    docTab.txtCssPath.Text = relativePath.Replace('\\', '/');
                }
            }
            _ = btnOk.Focus();
        }

        internal void BtnReset_Click(object? sender, EventArgs e)
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            string msgTitle = langDict?.GetValueOrDefault("ResetTitle", "Reset Settings") ?? "Reset Settings";
            string msgText = langDict?.GetValueOrDefault("ResetConfirm", "Are you sure you want to reset all settings?") ?? "Are you sure you want to reset all settings?";

            // Викликаємо кастомне вікно
            DialogResult result = ShowCustomMessageBox(msgText, msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string currentExePath = Environment.ProcessPath
                        ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName + ".exe");

                    // Використовуємо cmd для затримки в 1 секунду перед запуском нової копії
                    // Це дасть поточній програмі час закритися і звільнити М'ютекс
                    string cmdArgs = $"/c timeout /t 1 && start \"\" \"{currentExePath}\"";

                    _ = Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = cmdArgs,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });

                    Application.Exit(); // Коректне завершення роботи WinForms
                }
                catch (Exception ex)
                {
                    Config.LogError("Application reset/restart failed", ex); // Додаємо логування
                    string errTitle = langDict?.GetValueOrDefault("ErrTitle", "Error") ?? "Error";
                    _ = ShowCustomMessageBox($"Reset Error:\n\n{ex.Message}\n\nDetails can be found in {Config.LogErrorFile}",
                        errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            _ = btnOk.Focus();
        }

        //============================================================
        //--- 3.  Логіка елементів керування (Interactive Logic) ---
        internal void ChkFb2Name_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isChangingStates)
            {
                return;
            }

            _isChangingStates = true;

            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                try
                {
                    // ПЕРЕВІРКА: якщо масивів немає, нічого не робимо
                    if (docTab.cmbOutFields == null || docTab.chkAsFolder == null)
                    {
                        return;
                    }

                    bool isFb2Enabled = docTab.chkFb2Name.Checked;
                    // 1. Блокуємо чекбокс "Назва за замовчуванням"
                    docTab.chkDefaultName.Enabled = !isFb2Enabled;

                    ApplyTheme();

                    // 2. Вимикаємо тільки елементи всередині GroupBox, якщо FB2 Name увімкнено
                    for (int i = 0; i < docTab.cmbOutFields.Length; i++)
                    {
                        docTab.cmbOutFields[i].Enabled = !isFb2Enabled;
                        docTab.chkAsFolder[i].Enabled = !isFb2Enabled;

                        if (isFb2Enabled)
                        {
                            docTab.cmbOutFields[i].SelectedIndex = 0;
                            docTab.chkAsFolder[i].Checked = false;
                        }
                    }

                    if (!isFb2Enabled)
                    {
                        CmbOutFields_SelectedIndexChanged(0);
                    }
                }
                finally { _isChangingStates = false; }

                ApplyTheme(); // Це викличе SetControlsTheme
            }
        }

        internal void ChkDefaultName_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isChangingStates)
            {
                return;
            }

            _isChangingStates = true;

            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                try
                {
                    // ПЕРЕВІРКА
                    if (docTab.cmbOutFields == null || docTab.chkAsFolder == null)
                    {
                        return;
                    }

                    bool isDefaultEnabled = docTab.chkDefaultName.Checked;

                    docTab.chkFb2Name.Enabled = !isDefaultEnabled;
                    docTab.grpOutName.Enabled = true;

                    for (int i = 0; i < docTab.cmbOutFields.Length; i++)
                    {
                        docTab.cmbOutFields[i].Enabled = !isDefaultEnabled;
                        docTab.chkAsFolder[i].Enabled = !isDefaultEnabled;

                        if (isDefaultEnabled)
                        {
                            docTab.cmbOutFields[i].SelectedIndex = 0;
                            docTab.chkAsFolder[i].Checked = false;
                        }
                    }
                    if (!isDefaultEnabled)
                    {
                        CmbOutFields_SelectedIndexChanged(0);
                    }
                }
                finally { _isChangingStates = false; }
                ApplyTheme();
            }
        }

        internal void CmbOutFields_SelectedIndexChanged(int index)
        {
            bool internalCall = _isChangingStates;
            if (!internalCall)
            {
                _isChangingStates = true;
            }

            // Дістаємо посилання на вкладку документа для роботи з масивами її контролів
            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                try
                {
                    // ПЕРЕВІРКА
                    if (docTab.cmbOutFields == null || docTab.chkAsFolder == null)
                    {
                        return;
                    }

                    bool hasSelection = docTab.cmbOutFields[index].SelectedIndex > 0;
                    docTab.chkAsFolder[index].Enabled = hasSelection;
                    if (!hasSelection)
                    {
                        docTab.chkAsFolder[index].Checked = false;
                    }

                    if (index < docTab.cmbOutFields.Length - 1)
                    {
                        if (hasSelection)
                        {
                            docTab.cmbOutFields[index + 1].Enabled = true;
                        }
                        else
                        {
                            for (int i = index + 1; i < docTab.cmbOutFields.Length; i++)
                            {
                                docTab.cmbOutFields[i].SelectedIndex = 0;
                                docTab.cmbOutFields[i].Enabled = false;
                                docTab.chkAsFolder[i].Checked = false;
                                docTab.chkAsFolder[i].Enabled = false;
                            }
                        }
                    }
                }
                finally
                {
                    if (!internalCall)
                    {
                        _isChangingStates = false;
                    }
                }
                if (!internalCall)
                {
                    ApplyTheme();
                }
            }
        }

        //==============================================
        // --- 4. Синхронізація та дані (Sync/Data) ---
        private static void SyncConfigNameWithYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                // Якщо шлях обрано - копіюємо його
                if (!string.IsNullOrWhiteSpace(docTab.txtCustomYamlPath.Text))
                {
                    docTab.txtConfigName.Text = docTab.txtCustomYamlPath.Text;
                }
                else
                {
                    // Якщо чекбокс увімкнено, але шлях ще не обрано - 
                    // очищуємо поле, щоб не залишався "config.yaml"
                    docTab.txtConfigName.Text = "";
                }
            }
            else
            {
                // Коли галочку знято, ми очищуємо саме поле зі шляхом "Data/***.yaml"
                docTab.txtCustomYamlPath.Text = "";

                // Повертаємо стандартне ім'я для вихідного файлу
                docTab.txtConfigName.Text = Config.DefaultConfigPath;
            }
        }

        private static void SyncCssWithCustomYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                // 1. Отримуємо шлях до обраного YAML
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        // 2. Зчитуємо значення ключа stylesheet_path
                        string value = YamlService.ReadYamlValue(fullPath, "stylesheet_path");

                        // 3. Просто записуємо значення в текстове поле. 
                        // Це спрацює навіть якщо чекбокс chkCss вимкнений (так само як з обкладинкою).
                        docTab.txtCssPath.Text = value;
                    }
                }
            }
            else
            {
                // Якщо режим редагування вимкнено - очищуємо шлях
                docTab.txtCssPath.Text = "";
            }
        }

        private static void SyncTocTypeWithCustomYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string value = YamlService.ReadYamlValue(fullPath, "toc_type");

                        if (!string.IsNullOrEmpty(value))
                        {
                            // Мапимо технічне значення назад на індекс
                            string[] techValues = ["normal", "old_kindle", "flat"];
                            int index = Array.IndexOf(techValues, value);
                            if (index >= 0)
                            {
                                docTab.cmbTocType.SelectedIndex = index;
                                return; // Виходимо, якщо значення успішно знайдено і встановлено
                            }
                        }
                    }
                }
            }

            // СКИНУТИ до значення за замовчуванням ("normal"), якщо галочка знята або файл не знайдено
            if (docTab.cmbTocType.Items.Count > 0)
            {
                docTab.cmbTocType.SelectedIndex = 0; // Перший пункт зазвичай "normal"
            }
        }

        private static void SyncBinarySettingsWithYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string fixZip = YamlService.ReadYamlValue(fullPath, "fix_zip").ToLowerInvariant();
                        string openCover = YamlService.ReadYamlValue(fullPath, "open_from_cover").ToLowerInvariant();
                        string translit = YamlService.ReadYamlValue(fullPath, "file_name_transliterate").ToLowerInvariant();

                        docTab.rbFixZipYes.Checked = fixZip == "true";
                        docTab.rbFixZipNo.Checked = fixZip != "true"; // за замовчуванням false

                        docTab.rbOpenCoverYes.Checked = openCover == "true";
                        docTab.rbOpenCoverNo.Checked = openCover != "true";

                        docTab.rbTranslitYes.Checked = translit == "true";
                        docTab.rbTranslitNo.Checked = translit != "true";
                        return;
                    }
                }
            }
            // дефолтні, якщо галочка "Редагувати..." знята
            docTab.rbFixZipNo.Checked = true;
            docTab.rbOpenCoverNo.Checked = true;
            docTab.rbTranslitNo.Checked = true;
        }
    }
}