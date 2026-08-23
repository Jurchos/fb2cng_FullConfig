using System.ComponentModel;
using System.Diagnostics;
using fb2cng_FullConfig.Templates;
using fb2cng_FullConfig.Utils;
using static fb2cng_FullConfig.Utils.UiComponents;
using fb2cng_FullConfig.Services;

namespace fb2cng_FullConfig
{
    [DesignerCategory("Code")]
    public partial class Form1
    {
        // Логічні прапорці захисту від зациклювання графічних подій
        private bool _isChangingStates;

        // Технічні значення для YAML (співпадають за порядком із локалізацією в FillCombo)
        private static readonly string[] _tocValues = ["normal", "old_kindle", "flat"];
        private static readonly string[] _noteValues = ["default", "float", "floatRenumbered"];
        private static readonly string[] _resizeValues = ["none", "keepAR", "stretch"];
        private static readonly string[] _placementValues = ["none", "before", "after"];
        private static readonly string[] _logLevels = ["none", "normal", "debug"];

        //=====================================
        // --- 1. Тема та Локалізація ---
        public void ApplyTheme()
        {
            // Тепер менеджеру не потрібен словник кешу, він сам знайде вкладки в pnlContent.Controls
            ThemeManager.Apply(this, headerPanel, footerPanel, pnlContent);
        }

        private void UpdateLocalization()
        {
            Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

            // Допоміжний метод для заповнення ComboBox
            void FillCombo(ComboBox combo, string[] keys)
            {
                int selected = combo.SelectedIndex;
                combo.Items.Clear();
                foreach (string key in keys)
                {
                    _ = combo.Items.Add(loc.GetValueOrDefault(key, key));
                }

                combo.SelectedIndex = selected >= 0 ? selected : 0;
            }

            string GetText(string key, string defaultText)
            {
                return loc.TryGetValue(key, out string? value) ? value : defaultText;
            }

            // Локалізація статичної (головної) форми та хідера/футера
            Text = GetText("Title", "fb2cng Configurator");
            btnHelp.Text = GetText("Help", "Help");
            btnTheme.Text = GetText("Theme", "Theme");
            btnOk.Text = GetText("Ok", "OK");
            btnCancel.Text = GetText("Cancel", "Cancel");
            btGui?.Text = GetText("Gui", "GUI");
            // Локалізація кнопок ТАК/НІ для всіх груп
            string yes = GetText("Yes", "Yes");
            string no = GetText("No", "No");

            // ЛОКАЛІЗАЦІЯ ВКЛАДОК
            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                FillCombo(docTab.cmbTocType, ["Opt_Toc_Normal", "Opt_Toc_OldKindle", "Opt_Toc_Flat"]);
                docTab.lblLang.Text = GetText("Language", "Language:");
                docTab.btnDumpConfig.Text = GetText("DumpConfig", "Dump Default Config");
                docTab.lblConfigName.Text = GetText("ConfigName", "Config Name:");
                docTab.chkCustomYaml.Text = GetText("CustomYamlEnable", "Edit custom .yaml template");
                docTab.chkCss.Text = GetText("CssEnable", "Use Custom CSS");
                docTab.btnBrowseCss.Text = GetText("Browse", " ...");
                docTab.btnBrowseCustomYaml.Text = GetText("Browse", " ...");
                docTab.btnReset.Text = string.Empty;

                docTab.chkCover.SetTextIfNotNull(GetText("TocType", "Navigation hierarchy"));
                docTab.chkFixZip.SetTextIfNotNull(GetText("FixZip", "Fix Broken ZIP Archives"));
                docTab.rbFixZipYes.Text = docTab.rbOpenCoverYes.Text = docTab.rbTranslitYes.Text = GetText("Yes", "Yes");
                docTab.rbFixZipNo.Text = docTab.rbOpenCoverNo.Text = docTab.rbTranslitNo.Text = GetText("No", "No");

                docTab.chkOpenFromCover.SetTextIfNotNull(GetText("OpenCover", "Open from Cover"));
                docTab.chkTranslit.Text = GetText("Translit", "Transliterate Output Name");

                docTab.chkFb2Name.Text = GetText("Fb2Name", "Use Original FB2 Name");
                docTab.chkDefaultName.Text = GetText("DefaultName", "Default Filename");

                docTab.grpOutName.Text = GetText("OutNameTitle", "Output Structure");
                docTab.chkAsFolder.SetTextForAllIfNotNull(GetText("AsFolder", "Fold"));

                string[] itemKeys = ["Item_Empty", "Item_Author", "Item_Series", "Item_Title", "Item_Title_Pure", "Item_Lang", "Item_Genre", "Item_Date", "Item_Source", "Item_Uuid"];
                string[] defaultItems = ["", "Author", "Series", "Title", "Pure Title", "Language", "Genre", "Date", "Source File", "Book UUID"];

                if (docTab.cmbOutFields != null)
                {
                    for (int i = 0; i < docTab.cmbOutFields.Length; i++)
                    {
                        if (docTab.cmbOutFields[i] == null)
                        {
                            continue;
                        }

                        docTab.cmbOutFields[i].BeginUpdate();
                        int currSel = docTab.cmbOutFields[i].SelectedIndex;
                        docTab.cmbOutFields[i].Items.Clear();

                        for (int k = 0; k < itemKeys.Length; k++)
                        {
                            _ = docTab.cmbOutFields[i].Items.Add(GetText(itemKeys[k], defaultItems[k]));
                        }
                        docTab.cmbOutFields[i].SelectedIndex = currSel >= 0 ? currSel : 0;
                        docTab.cmbOutFields[i].EndUpdate();
                    }
                }
            }
            if (_tabsCache.TryGetValue("metadata:", out UserControl? data) && data is MetadataTab dataTab) // Локалізація вкладки "metadata:"
            {
                FillCombo(dataTab.cmbNotesMode, ["Opt_Note_Default", "Opt_Note_Float", "Opt_Note_FloatRen"]);
                FillCombo(dataTab.cmbResizeCover, ["Opt_Resize_None", "Opt_Resize_KeepAR", "Opt_Resize_Stretch"]);
                FillCombo(dataTab.cmbTocPlacement, ["Opt_TocPlace_None", "Opt_TocPlace_Before", "Opt_TocPlace_After"]);
                dataTab.chkSoftHyphen.Text = GetText("SoftHyphen", "Soft Hyphen");
                dataTab.chkPageMapEnable.Text = GetText("PageMapEnable", "Page Map");
                dataTab.chkPageMapSize.Text = GetText("PageMapSize", "Page Size");
                dataTab.chkAdobeDe.Text = GetText("AdobeDe", "Adobe RMSDK");
                dataTab.chkUseBroken.Text = GetText("UseBroken", "Broken Images");
                dataTab.chkRemoveTransp.Text = GetText("RemoveTransp", "Transparency");
                dataTab.chkScaleFactor.Text = GetText("ScaleFactor", "Scale Factor");
                dataTab.chkImgOptimize.Text = GetText("ImgOptimize", "Optimization");
                dataTab.chkJpegQuality.Text = GetText("JpegQuality", "JPEG Quality");
                dataTab.chkReaderSize.Text = GetText("ReaderSize", "Screen Size");
                dataTab.lblWidth.Text = GetText("Width", "W:");
                dataTab.lblHeight.Text = GetText("Height", "H:");
                dataTab.lblDpi.Text = GetText("Dpi", "DPI:");
                dataTab.chkGenerateCover.Text = GetText("GenCover", "Cover Gen");
                dataTab.chkResizeCover.Text = GetText("ResizeCover", "Resize Mode");
                dataTab.btnBrowseCover.Text = GetText("Browse", " ...");
                dataTab.chkNotes.Text = GetText("FootnotesMode", "Footnotes display method:");
                dataTab.chkAnnEnable.Text = GetText("AnnEnable", "Annotation");
                dataTab.chkAnnInToc.Text = GetText("AnnInToc", "Ann in TOC");
                dataTab.chkTocPlacement.Text = GetText("TocPlacement", "TOC Page");
                dataTab.chkInclNoTitle.Text = GetText("InclNoTitle", "Untitled Chapters");
                dataTab.chkDropcaps.Text = GetText("Dropcaps", "Dropcaps");
                dataTab.chkVignettes.Text = GetText("Vignettes", "Vignettes");
                dataTab.btnVignetteSettings.Text = "⚙ " + GetText("Vig_Options", "Options");
                // Список віньєток
                // --- ЛОКАЛІЗАЦІЯ ВІНЬЄТОК З ЗБЕРЕЖЕННЯМ СТАНУ ---
                int vigCount = dataTab.clbVignettesItems.Items.Count; // Отримуємо кількість пунктів
                // 1. Запам'ятовуємо, які галочки були поставлені
                bool[] checkedStates = new bool[vigCount];
                // Перевіряємо, чи список не порожній (щоб не було помилок при першому запуску)
                if (dataTab.clbVignettesItems.Items.Count > 0)
                {
                    for (int i = 0; i < vigCount; i++)
                    {
                        checkedStates[i] = dataTab.clbVignettesItems.GetItemChecked(i);
                    }
                }

                // 2. Оновлюємо назви (очищуємо та додаємо нові переклади)
                dataTab.clbVignettesItems.BeginUpdate(); // Вимикаємо перемалювання для швидкості
                dataTab.clbVignettesItems.Items.Clear();
                dataTab.clbVignettesItems.Items.AddRange([
                    GetText("Vig_B_T", "Book Top"),
                    GetText("Vig_B_B", "Book Bottom"),
                    GetText("Vig_C_T", "Chapter Top"),
                    GetText("Vig_C_B", "Chapter Bottom"),
                    GetText("Vig_C_E", "Chapter End"),
                    GetText("Vig_S_T", "Section Top"),
                    GetText("Vig_S_B", "Section Bottom"),
                    GetText("Vig_S_E", "Section End")
                ]);

                // 3. Відновлюємо галочки, якщо вони були
                if (dataTab.clbVignettesItems.Items.Count == vigCount)
                {
                    for (int i = 0; i < vigCount; i++)
                    {
                        dataTab.clbVignettesItems.SetItemChecked(i, checkedStates[i]);
                    }
                }
                dataTab.clbVignettesItems.EndUpdate();

                dataTab.rbSoftHyphenYes.Text = dataTab.rbPageMapYes.Text = dataTab.rbAdobeDeYes.Text =
                dataTab.rbUseBrokenYes.Text = dataTab.rbRemoveTranspYes.Text = dataTab.rbImgOptimizeYes.Text =
                dataTab.rbGenCoverYes.Text = dataTab.rbAnnEnableYes.Text = dataTab.rbAnnInTocYes.Text =
                dataTab.rbInclNoTitleYes.Text = dataTab.rbVignettesYes.Text = dataTab.rbDropcapsYes.Text = yes;

                dataTab.rbSoftHyphenNo.Text = dataTab.rbPageMapNo.Text = dataTab.rbAdobeDeNo.Text =
                dataTab.rbUseBrokenNo.Text = dataTab.rbRemoveTranspNo.Text = dataTab.rbImgOptimizeNo.Text =
                dataTab.rbGenCoverNo.Text = dataTab.rbAnnEnableNo.Text = dataTab.rbAnnInTocNo.Text =
                dataTab.rbInclNoTitleNo.Text = dataTab.rbVignettesNo.Text = dataTab.rbDropcapsNo.Text = no;
            }

            if (_tabsCache.TryGetValue("logging:", out UserControl? log) && log is LoggingTab logTab)
            {
                FillCombo(logTab.cmbLogLevel, ["Opt_Log_None", "Opt_Log_Normal", "Opt_Log_Debug"]);
                logTab.chkLogLevel.Text = GetText("LogLevel", "Logging level:");
                logTab.chkLogName.Text = GetText("LogName", "Log file name:");
                logTab.chkPanicLogName.Text = GetText("LogPanicName", "Panic log file name:");
                logTab.chkLogMode.Text = GetText("LogMode", "Logging mode:");
                logTab.chkLogFolder.Text = GetText("LogFolder", "Logs folder:");

                logTab.rbLogModeOnlyNew.Text = GetText("LogMode_OnlyNew", "only_new");
                logTab.rbLogModeOldNew.Text = GetText("LogMode_OldNew", "old+new");
                logTab.rbLogFolderYes.Text = GetText("Yes", "Yes");
                logTab.rbLogFolderNo.Text = GetText("No", "No");
                logTab.lblShowTips.Text = GetText("ShowTooltips", "* Увімкнути спливаючі підказки");
                logTab.rbTipsYes.Text = GetText("Yes", "Yes");
                logTab.rbTipsNo.Text = GetText("No", "No");

                string[] logOptions = [
                    GetText("LogOpt_Default", "default"),
                    GetText("LogOpt_NameFormat", "name + format"),
                    GetText("LogOpt_TimeName", "time + name"),
                    GetText("LogOpt_NameTag", "name + tag")
                ];

                int selLog = logTab.cmbLogName.SelectedIndex;
                int selPanic = logTab.cmbPanicLogName.SelectedIndex;

                logTab.cmbLogName.BeginUpdate();
                logTab.cmbLogName.Items.Clear();
                logTab.cmbLogName.Items.AddRange(logOptions);
                logTab.cmbLogName.SelectedIndex = selLog >= 0 ? selLog : 0;
                logTab.cmbLogName.EndUpdate();

                logTab.cmbPanicLogName.BeginUpdate();
                logTab.cmbPanicLogName.Items.Clear();
                logTab.cmbPanicLogName.Items.AddRange(logOptions);
                logTab.cmbPanicLogName.SelectedIndex = selPanic >= 0 ? selPanic : 0;
                logTab.cmbPanicLogName.EndUpdate();

            }
        }

        //=====================================================
        // --- 2. Логіка взаємодії елементів (DocumentTab) ---
        internal void LangComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Оскільки langComboBox тепер лежить всередині вкладки DocumentTab, 
            // дістаємо посилання на нього через кеш вкладок
            if (_tabsCache.TryGetValue("document:", out UserControl? tab) && tab is DocumentTab docTab)
            {
                Config.Settings.CurrentLanguage = docTab.langComboBox.SelectedIndex switch
                {
                    1 => "Ukrainian",
                    2 => "Russian",
                    _ => "English",
                };
                UpdateLocalization();
                ApplyTheme();
                Config.SaveSettings(); // Миттєве збереження обраної мови
            }
        }

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

        //==========================================================
        // --- 3. Обробники дій вкладок (Browse, Reset, Dump) ---
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
        }

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
        }

        internal void BtnBrowseCover_Click(object? sender, EventArgs e)
        {
            if (_tabsCache.TryGetValue("metadata:", out UserControl? tab) && tab is MetadataTab dataTab)
            {
                using OpenFileDialog ofd = new();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    dataTab.txtCoverPath.Text = Path.GetRelativePath(appPath, ofd.FileName).Replace('\\', '/');
                }
            }
        }

        //===========================================
        // --- 4. Синхронізація з YAML (Читання) ---
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

        private void SyncMetadataWithYaml(DocumentTab docTab)
        {
            if (!_tabsCache.TryGetValue("metadata:", out UserControl? m) || m is not MetadataTab dataTab)
            {
                return;
            }

            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, docTab.txtCustomYamlPath.Text.Trim());
                if (File.Exists(yamlPath))
                {
                    // 1. Зчитуємо значення для Soft Hyphen
                    bool isSoftHyphen = string.Equals(YamlService.ReadYamlValue(yamlPath, "insert_soft_hyphen"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbSoftHyphenYes.Checked = isSoftHyphen;
                    dataTab.rbSoftHyphenNo.Checked = !isSoftHyphen;

                    // Page Map
                    dataTab.rbPageMapYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "enable") == "true";
                    dataTab.rbPageMapNo.Checked = !dataTab.rbPageMapYes.Checked;
                    dataTab.txtPageMapSize.Text = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "size") switch { "" => "2300", var s => s };
                    dataTab.rbAdobeDeYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "adobe_de") == "true";
                    dataTab.rbAdobeDeNo.Checked = !dataTab.rbAdobeDeYes.Checked;

                    // Images Extra
                    dataTab.rbUseBrokenYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "use_broken") == "true";
                    dataTab.rbUseBrokenNo.Checked = !dataTab.rbUseBrokenYes.Checked;
                    dataTab.txtScaleFactor.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "scale_factor") switch { "" => "1.0", var s => s };
                    dataTab.rbImgOptimizeYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "optimize") == "true";
                    dataTab.rbImgOptimizeNo.Checked = !dataTab.rbImgOptimizeYes.Checked;

                    // 2. Зчитуємо значення для Remove Transparency
                    bool isRemoveTransp = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "remove_transparency"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbRemoveTranspYes.Checked = isRemoveTransp;
                    dataTab.rbRemoveTranspNo.Checked = !isRemoveTransp;

                    // 3. JPEG Quality
                    string jpegVal = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "jpeg_quality_level");
                    dataTab.txtJpegQuality.Text = string.IsNullOrEmpty(jpegVal) ? "95" : jpegVal;

                    // Reader Size 
                    dataTab.txtWidth.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "width") switch { "" => "1264", var s => s };
                    dataTab.txtHeight.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "height") switch { "" => "1680", var s => s };
                    dataTab.txtDpi.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "dpi") switch { "" => "300", var s => s };

                    // 4. Generate Cover
                    bool isGenCover = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "generate"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbGenCoverYes.Checked = isGenCover;
                    dataTab.rbGenCoverNo.Checked = !isGenCover;
                    dataTab.txtCoverPath.Text = YamlService.ReadYamlValue(yamlPath, "default_image_path");

                    string resize = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "resize");
                    int rIdx = Array.IndexOf(_resizeValues, resize.Replace("\"", ""));
                    dataTab.cmbResizeCover.SelectedIndex = rIdx >= 0 ? rIdx : 2; // stretch

                    //Footnotes
                    string noteMode = YamlService.ReadYamlSectionValue(yamlPath, ["footnotes:"], "mode");
                    int nIdx = Array.IndexOf(_noteValues, noteMode.Replace("\"", ""));
                    dataTab.cmbNotesMode.SelectedIndex = nIdx >= 0 ? nIdx : 0;

                    // --- Annotation Enable (Default: false) ---
                    string annVal = YamlService.ReadYamlSectionValue(yamlPath, ["annotation:"], "enable").ToLowerInvariant();
                    dataTab.rbAnnEnableYes.Checked = annVal == "true";
                    dataTab.rbAnnEnableNo.Checked = annVal != "true";

                    // --- Annotation In TOC (Default: true) ---
                    string inTocVal = YamlService.ReadYamlSectionValue(yamlPath, ["annotation:"], "in_toc").ToLowerInvariant();
                    // Оскільки за замовчуванням true, ми вимикаємо 'Yes' ТІЛЬКИ якщо там явно написано "false"
                    dataTab.rbAnnInTocYes.Checked = inTocVal != "false";
                    dataTab.rbAnnInTocNo.Checked = inTocVal == "false";

                    string placement = YamlService.ReadYamlSectionValue(yamlPath, ["toc_page:"], "placement");
                    int pIdx = Array.IndexOf(_placementValues, placement.Replace("\"", ""));
                    dataTab.cmbTocPlacement.SelectedIndex = pIdx >= 0 ? pIdx : 0; // none

                    // 3. NoTitleinTOC & Vignettes
                    dataTab.rbInclNoTitleYes.Checked = YamlService.ReadYamlValue(yamlPath, "include_chapters_without_title") == "true";
                    dataTab.rbInclNoTitleNo.Checked = !dataTab.rbInclNoTitleYes.Checked;

                    // 2. Логіка віньєток (незалежна)
                    string vigRaw = YamlService.ReadYamlValue(yamlPath, "vignettes:");
                    bool vigRoot = !string.IsNullOrEmpty(vigRaw) && !vigRaw.StartsWith('#');
                    dataTab.rbVignettesYes.Checked = vigRoot;
                    dataTab.rbVignettesNo.Checked = !vigRoot;

                    // 3. Зчитування галочок віньєток 
                    string[] lines = File.ReadAllLines(yamlPath);

                    // Додаємо двокрапки відразу в масив, щоб прибрати "+" у циклі
                    string[] vigKeysWithColon = [ "title_top:", "title_bottom:", "title_top:", "title_bottom:",
                                                "end:", "title_top:", "title_bottom:", "end:"
                                                 ];

                    // 1. Отримуємо кількість ключів безпосередньо з масиву
                    int totalVigKeys = vigKeysWithColon.Length;

                    // 2. Отримуємо кількість пунктів у списку інтерфейсу
                    int uiItemsCount = dataTab.clbVignettesItems.Items.Count;

                    int foundKeys = 0;
                    // Використовуємо totalVigKeys замість 8
                    for (int i = 0; i < lines.Length && foundKeys < totalVigKeys; i++)
                    {
                        string currentLine = lines[i];

                        // Перевіряємо, чи рядок містить ключ
                        if (currentLine.Contains(vigKeysWithColon[foundKeys], StringComparison.Ordinal))
                        {
                            // Перевіряємо, чи цей ключ не виходить за межі пунктів у UI
                            if (foundKeys < uiItemsCount)
                            {
                                // Перевіряємо на закоментованість
                                bool isChecked = !currentLine.TrimStart().StartsWith('#');
                                dataTab.clbVignettesItems.SetItemChecked(foundKeys, isChecked);
                            }
                            foundKeys++;
                        }
                    }

                    dataTab.rbDropcapsYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["dropcaps:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbDropcapsNo.Checked = !dataTab.rbDropcapsYes.Checked;

                    return;
                }
            }
            // Дефолти
            dataTab.rbSoftHyphenNo.Checked = true;
            dataTab.rbPageMapYes.Checked = true;      // за замовчуванням Так
            dataTab.txtPageMapSize.Text = "2300";    // за замовчуванням 2300
            dataTab.rbAdobeDeNo.Checked = true;      // за замовчуванням Ні
            dataTab.rbUseBrokenNo.Checked = true;    // за замовчуванням Ні
            dataTab.rbRemoveTranspNo.Checked = true;
            dataTab.txtScaleFactor.Text = "1.0";     // за замовчуванням 1.0
            dataTab.rbImgOptimizeYes.Checked = true; // за замовчуванням Так
            dataTab.txtJpegQuality.Text = "95";
            dataTab.txtWidth.Text = "1264";
            dataTab.txtHeight.Text = "1680";
            dataTab.txtDpi.Text = "300";
            dataTab.rbGenCoverNo.Checked = true;
            dataTab.txtCoverPath.Text = "";
            dataTab.cmbResizeCover.SelectedIndex = 2; // stretch
            dataTab.cmbNotesMode.SelectedIndex = 0;
            dataTab.rbAnnEnableNo.Checked = true;
            dataTab.rbAnnInTocYes.Checked = true; // default true
            dataTab.cmbTocPlacement.SelectedIndex = 0; // none
            dataTab.rbInclNoTitleNo.Checked = true;  // за замовчуванням Ні
            dataTab.rbVignettesNo.Checked = true;    // за замовчуванням Ні
            dataTab.rbDropcapsNo.Checked = true;
        }

        private void SyncLoggingSettingsWithYaml(DocumentTab docTab)
        {
            // Отримуємо посилання на вкладку логів
            if (!_tabsCache.TryGetValue("logging:", out UserControl? tab) || tab is not LoggingTab logTab)
            {
                return;
            }

            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string[] fileSec = ["logging:", "file:"];

                        // 1. Level (тільки вибір значення, без активації чекбокса)
                        string level = YamlService.ReadYamlSectionValue(fullPath, fileSec, "level").ToLowerInvariant();
                        if (!string.IsNullOrEmpty(level))
                        {
                            int idx = Array.IndexOf(_logLevels, level.Replace("\"", ""));
                            if (idx >= 0)
                            {
                                logTab.cmbLogLevel.SelectedIndex = idx;
                            }
                        }

                        // 2. Mode
                        string mode = YamlService.ReadYamlSectionValue(fullPath, fileSec, "mode").ToLowerInvariant();
                        if (!string.IsNullOrEmpty(mode))
                        {
                            logTab.rbLogModeOldNew.Checked = mode == "append";
                            logTab.rbLogModeOnlyNew.Checked = mode != "append";
                        }

                        // 3. ШАБЛОН ІМЕНІ (тепер через GetTemplateIndex)
                        string dest = YamlService.ReadYamlSectionValue(fullPath, fileSec, "destination_template");
                        if (!string.IsNullOrEmpty(dest))
                        {
                            bool hasF = dest.StartsWith("logs/");
                            logTab.rbLogFolderYes.Checked = hasF;
                            logTab.rbLogFolderNo.Checked = !hasF;

                            string cleanRead = hasF ? dest[5..] : dest;
                            // Виклик логіки з Program.cs
                            int idx = YamlService.GetTemplateIndex(cleanRead, YamlService.LogNameValues);
                            if (idx >= 0)
                            {
                                logTab.cmbLogName.SelectedIndex = idx;
                            }
                        }

                        // 4. PANIC ШАБЛОН (тепер через GetTemplateIndex)
                        string panic = YamlService.ReadYamlSectionValue(fullPath, fileSec, "panic_destination_template");
                        if (!string.IsNullOrEmpty(panic))
                        {
                            string cleanPRead = panic.StartsWith("logs/") ? panic[5..] : panic;
                            // Виклик логіки з Program.cs
                            int pIdx = YamlService.GetTemplateIndex(cleanPRead, YamlService.PanicLogNameValues);
                            if (pIdx >= 0)
                            {
                                logTab.cmbPanicLogName.SelectedIndex = pIdx;
                            }
                        }
                        return;
                    }
                }
            }
            // Якщо файл не вибрано — просто ставимо дефолти у поля
            logTab.cmbLogLevel.SelectedIndex = 2;
            logTab.cmbLogName.SelectedIndex = 0;
            logTab.cmbPanicLogName.SelectedIndex = 0;
            logTab.rbLogModeOnlyNew.Checked = true;
            logTab.rbLogFolderNo.Checked = true;
        }

        //================================
        // --- 5. Збереження (Запис) ---
        private void SaveYamlConfiguration()
        {
            if (!_tabsCache.TryGetValue("document:", out UserControl? doc) || doc is not DocumentTab docTab)
            {
                return;
            }

            MetadataTab? dataTab = _tabsCache.TryGetValue("metadata:", out UserControl? data) ? data as MetadataTab : null;
            LoggingTab? logTab = _tabsCache.TryGetValue("logging:", out UserControl? log) ? log as LoggingTab : null;
            // ПЕРЕВІРКА НА NULL (Один раз на початку)
            if (docTab.cmbOutFields == null || docTab.chkAsFolder == null)
            {
                return;
            }
            int fieldsCount = docTab.cmbOutFields.Length;
            int[] fieldIndexes = new int[fieldsCount];
            bool[] folderFlags = new bool[fieldsCount];
            for (int i = 0; i < fieldsCount; i++)
            {
                fieldIndexes[i] = docTab.cmbOutFields[i].SelectedIndex; 
                folderFlags[i] = docTab.chkAsFolder[i].Checked;
            }

            // ФОРМУВАННЯ МАСИВУ ВІНЬЄТОК (Динамічно)
            // Визначаємо розмір масиву на основі кількості пунктів у списку
            int vigCount = dataTab?.clbVignettesItems.Items.Count ?? 0;
            bool[] vignettesArray = new bool[vigCount];

            if (dataTab != null && vigCount > 0)
            {
                for (int i = 0; i < vigCount; i++)
                {
                    vignettesArray[i] = dataTab.clbVignettesItems.GetItemChecked(i);
                }
            }

            try
            {
                // Виклик сервісу
                bool saved = YamlService.SaveConfiguration(
                    docTab.txtConfigName.Text, docTab.chkCustomYaml.Checked, docTab.txtCustomYamlPath.Text,
                    docTab.chkCss.Checked, docTab.txtCssPath.Text,
                    docTab.chkCover.Checked, _tocValues[docTab.cmbTocType.SelectedIndex],
                    docTab.chkFixZip.Checked, docTab.rbFixZipYes.Checked,
                    docTab.chkOpenFromCover.Checked, docTab.rbOpenCoverYes.Checked,
                    docTab.chkTranslit.Checked, docTab.rbTranslitYes.Checked,
                    docTab.chkFb2Name.Checked, docTab.chkDefaultName.Checked,
                    fieldIndexes, folderFlags,
                    dataTab?.chkSoftHyphen.Checked ?? false, dataTab?.rbSoftHyphenYes.Checked ?? false,
                    dataTab?.chkPageMapEnable.Checked ?? false, dataTab?.rbPageMapYes.Checked ?? true, 
                    dataTab?.txtPageMapSize.Text ?? "2300", dataTab?.rbAdobeDeYes.Checked ?? false,
                    dataTab?.chkUseBroken.Checked ?? false, dataTab?.rbUseBrokenYes.Checked ?? false, 
                    dataTab?.chkRemoveTransp.Checked ?? false, dataTab?.rbRemoveTranspYes.Checked ?? false,
                    dataTab?.txtScaleFactor.Text ?? "1.0", dataTab?.rbImgOptimizeYes.Checked ?? true,
                    dataTab?.chkJpegQuality.Checked ?? false, dataTab?.txtJpegQuality.Text ?? "95",
                    dataTab?.chkReaderSize.Checked ?? false, dataTab?.txtWidth.Text ?? "1264", dataTab?.txtHeight.Text ?? "1680", dataTab?.txtDpi.Text ?? "300",
                    dataTab?.chkGenerateCover.Checked ?? false, dataTab?.rbGenCoverYes.Checked ?? false, dataTab?.txtCoverPath.Text ?? "",
                    dataTab?.chkResizeCover.Checked ?? false, _resizeValues[dataTab?.cmbResizeCover.SelectedIndex ?? 2],
                    dataTab?.chkNotes.Checked ?? false, _noteValues[dataTab?.cmbNotesMode.SelectedIndex ?? 0],
                    dataTab?.chkAnnEnable.Checked ?? false, dataTab?.rbAnnEnableYes.Checked ?? false,
                    dataTab?.chkAnnInToc.Checked ?? false, dataTab?.rbAnnInTocYes.Checked ?? true,
                    dataTab?.chkTocPlacement.Checked ?? false, _placementValues[dataTab?.cmbTocPlacement.SelectedIndex ?? 0],
                    dataTab?.chkInclNoTitle.Checked ?? false, dataTab?.rbInclNoTitleYes.Checked ?? false,
                    dataTab?.chkVignettes.Checked ?? false, dataTab?.rbVignettesYes.Checked ?? false, vignettesArray,
                    dataTab?.chkDropcaps.Checked ?? false, dataTab?.rbDropcapsYes.Checked ?? false,
                    logTab?.chkLogLevel.Checked ?? false, _logLevels[logTab?.cmbLogLevel.SelectedIndex ?? 2],
                    logTab?.chkLogName.Checked ?? false, logTab?.cmbLogName.SelectedIndex >= 0 ? YamlService.LogNameValues[logTab.cmbLogName.SelectedIndex] : "",
                    logTab?.chkPanicLogName.Checked ?? false, logTab?.cmbPanicLogName.SelectedIndex >= 0 ? YamlService.PanicLogNameValues[logTab.cmbPanicLogName.SelectedIndex] : "",
                    logTab?.chkLogMode.Checked ?? false, logTab?.rbLogModeOldNew.Checked ?? false ? "append" : "overwrite",
                    logTab?.chkLogFolder.Checked ?? false, logTab?.rbLogFolderYes.Checked ?? false
                );

                if (saved)
                {
                    Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];
                    string cap = loc.GetValueOrDefault("SaveSuccessTitle", "Success");
                    string msg = loc.TryGetValue("SaveSuccess", out string? t) ? string.Format(t, docTab.txtConfigName.Text) : "Saved!";
                    _ = ShowCustomMessageBox(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];
                string title = loc.GetValueOrDefault("SaveErrorTitle", "Error");
                string msg = "Unknown error";

                if (ex.Message == "ERR_NO_ENGINE")
                {
                    // Повідомлення про відсутність fbc.exe
                    msg = loc.GetValueOrDefault("ErrFbc", "Error: fbc.exe not found!");
                }
                else if (ex.Message == "ERR_SOURCE_MISSING")
                {
                    // Повідомлення про те, що кастомний файл не знайдено
                    msg = loc.GetValueOrDefault("ErrDirNotFound", "Source file not found!");
                }
                else if (ex.Message == "ERR_READONLY")
                {
                    msg = loc.TryGetValue("ErrReadOnly", out string? t) ? string.Format(t, docTab.txtConfigName.Text) : "File is Read-Only!";
                }
                else if (ex.Message == "ERR_DIRNOTFOUND")
                {
                    msg = loc.TryGetValue("ErrDirNotFound", out string? t) ? string.Format(t, docTab.txtConfigName.Text) : "Directory not found!";
                }
                else if (ex.Message.StartsWith("YAML_KEY:"))
                {
                    title = loc.GetValueOrDefault("YamlTitle", "YAML Error");
                    string key = ex.Message.Replace("YAML_KEY:", "");
                    msg = loc.TryGetValue("YamlErr", out string? t) ? string.Format(t, key) : $"Key {key} not found!";
                }

                _ = ShowCustomMessageBox(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //================================
        // --- 6. Утиліти (Help, GUI) ---
        private static void ShowHelp()
        {
            // 1. Отримуємо словник локалізації для поточної мови
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            // 2. Отримуємо заголовок (Help або Допомога)
            string caption = langDict?.GetValueOrDefault("Help") ?? "Help";

            // 3. Отримуємо сирий текст шаблону з плейсхолдерами {0} та {1}
            // Якщо ключа немає, використовуємо дефолтний текст (бажано теж із плейсхолдерами)
            string rawMsg = langDict?.GetValueOrDefault("HelpText") ??
                            "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.\n{0}\nVersion: {1}";

            // 4. Отримуємо дані про програму
            string version = AppInfo.GetSimpleVersion();
            string copyright = AppInfo.GetCopyright();

            // 5. Форматуємо рядок (підставляємо copyright замість {0} та version замість {1})
            string msg = string.Format(rawMsg, copyright, version);

            // 6. Виводимо результат
            _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtGui_Click(object? sender, EventArgs e)
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fb2cng_GUI.exe");

            try
            {
                Process[] runningProcesses = Process.GetProcessesByName("fb2cng_GUI");
                if (runningProcesses.Length > 0)
                {
                    IntPtr hWnd = runningProcesses[0].MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        if (Win32Api.IsIconic(hWnd))
                        {
                            _ = Win32Api.ShowWindow(hWnd, 9);
                        }

                        _ = Win32Api.SetForegroundWindow(hWnd);
                        return;
                    }
                }

                // ВАЖЛИВО: додаємо WorkingDirectory, щоб GUI не вилітав!
                _ = Process.Start(new ProcessStartInfo(exePath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                });
            }
            catch (Exception ex)
            {
                // Логуємо, чому не вдалося відкрити графічний інтерфейс
                Config.LogError("Failed to start fb2cng_GUI.exe", ex);
            }
        }
    }
}
