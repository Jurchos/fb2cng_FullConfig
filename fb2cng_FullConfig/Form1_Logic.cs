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

        // 1. Керування мовою та локалізацією
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

        private void UpdateLocalization()
        {
            Dictionary<string, string> loc = Config.Localization[Config.Settings.CurrentLanguage];

            // Допоміжний метод для заповнення ComboBox
            void FillCombo(ComboBox combo, string[] keys)
            {
                int selected = combo.SelectedIndex;
                combo.Items.Clear();
                foreach (var key in keys) combo.Items.Add(loc.GetValueOrDefault(key, key));
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
                docTab.btnReset.Text = GetText("Reset", " ");

                docTab.chkCover.SetTextIfNotNull(GetText("TocType", "Navigation hierarchy"));
                docTab.chkFixZip.SetTextIfNotNull(GetText("FixZip", "Fix Broken ZIP Archives"));
                docTab.rbFixZipYes.Text = docTab.rbOpenCoverYes.Text = docTab.rbTranslitYes.Text = GetText("Yes", "Yes");
                docTab.rbFixZipNo.Text = docTab.rbOpenCoverNo.Text = docTab.rbTranslitNo.Text = GetText("No", "No");

                docTab.chkOpenFromCover.SetTextIfNotNull(GetText("OpenCover", "Open from Cover"));
                docTab.chkTranslit.Text = GetText("Translit", "Transliterate Output Name");

                docTab.chkFb2Name.Text = GetText("Fb2Name", "Use Original FB2 Name");
                docTab.chkDefaultName.Text = GetText("DefaultName", "Default Filename");

                docTab.lblOutNameTitle.SetTextIfNotNull(GetText("OutNameTitle", "Output Name Template Constructor"));
                docTab.grpOutName.Text = GetText("OutNameTitle", "Output Structure");
                docTab.chkAsFolder.SetTextForAllIfNotNull(GetText("AsFolder", "Fold"));

                string[] itemKeys = ["Item_Empty", "Item_Author", "Item_Series", "Item_Title", "Item_Title_Pure", "Item_Lang", "Item_Genre", "Item_Date", "Item_Source", "Item_Uuid"];
                string[] defaultItems = ["", "Author", "Series", "Title", "Pure Title", "Language", "Genre", "Date", "Source File", "Book UUID"];

                if (docTab.cmbOutFields != null)
                {
                    for (int i = 0; i < 7; i++)
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
                dataTab.chkReaderSize.Text = GetText("ReaderSize", "Screen Size");
                dataTab.lblWidth.Text = GetText("Width", "W:");
                dataTab.lblHeight.Text = GetText("Height", "H:");
                dataTab.lblDpi.Text = GetText("Dpi", "DPI:");
                dataTab.chkNotes.Text = GetText("FootnotesMode", "Footnotes display method:");
                dataTab.chkSoftHyphen.Text = GetText("SoftHyphen", "Soft Hyphen");
                dataTab.chkRemoveTransp.Text = GetText("RemoveTransp", "Transparency");
                dataTab.chkJpegQuality.Text = GetText("JpegQuality", "JPEG Quality");
                dataTab.chkGenerateCover.Text = GetText("GenCover", "Cover Gen");
                dataTab.chkResizeCover.Text = GetText("ResizeCover", "Resize Mode");
                dataTab.btnBrowseCover.Text = GetText("Browse", " ...");
                dataTab.chkAnnEnable.Text = GetText("AnnEnable", "Annotation");
                dataTab.chkAnnInToc.Text = GetText("AnnInToc", "Ann in TOC");
                dataTab.chkTocPlacement.Text = GetText("TocPlacement", "TOC Page");
                dataTab.chkDropcaps.Text = GetText("Dropcaps", "Dropcaps");

                dataTab.rbSoftHyphenYes.Text = dataTab.rbRemoveTranspYes.Text = dataTab.rbGenCoverYes.Text =
                dataTab.rbAnnEnableYes.Text = dataTab.rbAnnInTocYes.Text = dataTab.rbDropcapsYes.Text = GetText("Yes", "Yes");

                dataTab.rbSoftHyphenNo.Text = dataTab.rbRemoveTranspNo.Text = dataTab.rbGenCoverNo.Text =
                dataTab.rbAnnEnableNo.Text = dataTab.rbAnnInTocNo.Text = dataTab.rbDropcapsNo.Text = GetText("No", "No");
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

        public void ApplyTheme()
        {
            // Передаємо саму форму, хідер, футер, контент-панель та кеш вкладок
            ThemeManager.Apply(this, headerPanel, footerPanel, pnlContent, _tabsCache);
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

                    _ = Process.Start(new ProcessStartInfo
                    {
                        FileName = currentExePath,
                        UseShellExecute = true
                    });

                    Close();
                }
                catch (Exception ex)
                {
                    Config.LogError("Application reset/restart failed", ex); // Додаємо логування
                    string errTitle = langDict?.GetValueOrDefault("ErrTitle", "Error") ?? "Error";
                    _ = ShowCustomMessageBox($"Reset Error:\n\n{ex.Message}\n\nDetails can be found in logs/Conf_errors.log",
                        errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Метод вибору YAML файлу:
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
                    bool isFb2Enabled = docTab.chkFb2Name.Checked;
                    // 1. Блокуємо чекбокс "Назва за замовчуванням"
                    docTab.chkDefaultName.Enabled = !isFb2Enabled;

                    docTab.grpOutName.Enabled = true;

                    // 2. Вимикаємо тільки елементи всередині GroupBox, якщо FB2 Name увімкнено
                    for (int i = 0; i < 7; i++)
                    {
                        docTab.cmbOutFields![i].Enabled = !isFb2Enabled;
                        docTab.chkAsFolder![i].Enabled = !isFb2Enabled;

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
                    bool isDefaultEnabled = docTab.chkDefaultName.Checked;

                    docTab.chkFb2Name.Enabled = !isDefaultEnabled;
                    docTab.grpOutName.Enabled = true;

                    for (int i = 0; i < 7; i++)
                    {
                        docTab.cmbOutFields![i].Enabled = !isDefaultEnabled;
                        docTab.chkAsFolder![i].Enabled = !isDefaultEnabled;

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
                    bool hasSelection = docTab.cmbOutFields![index].SelectedIndex > 0;
                    docTab.chkAsFolder![index].Enabled = hasSelection;
                    if (!hasSelection)
                    {
                        docTab.chkAsFolder[index].Checked = false;
                    }

                    if (index < 6)
                    {
                        if (hasSelection)
                        {
                            docTab.cmbOutFields[index + 1].Enabled = true;
                        }
                        else
                        {
                            for (int i = index + 1; i < 7; i++)
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
                    string caption = langDict?.GetValueOrDefault("GenTitle", "Success") ?? "Success";
                    string msg = langDict?.GetValueOrDefault("GenSuccess", "config.yaml successfully generated!") ?? "config.yaml successfully generated!";

                    _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

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
                // Якщо вимкнено - повертаємо стандарт
                docTab.txtConfigName.Text = "Data/config.yaml";
            }
        }

        private static void SyncCssWithCustomYaml(DocumentTab docTab)
        {
            if (docTab.chkCustomYaml.Checked && docTab.chkCss.Checked)
            {
                // ВАЖЛИВО: Якщо користувач вже обрав файл (поле не порожнє), 
                // ми не затираємо його автоматично при кожному кліку чекбокса.
                if (!string.IsNullOrEmpty(docTab.txtCssPath.Text))
                {
                    return;
                }

                string yamlPath = docTab.txtCustomYamlPath.Text.Trim();
                if (!string.IsNullOrEmpty(yamlPath))
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, yamlPath);
                    if (File.Exists(fullPath))
                    {
                        string value = YamlService.ReadYamlValue(fullPath, "stylesheet_path");
                        if (!string.IsNullOrEmpty(value))
                        {
                            docTab.txtCssPath.Text = value;
                        }
                    }
                }
            }
            if (!docTab.chkCustomYaml.Checked)
            {
                docTab.txtCssPath.Text = "";
                return;
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
                    // Наявні (Reader Size & Footnotes)
                    dataTab.txtWidth.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "width") switch { "" => "1264", var s => s };
                    dataTab.txtHeight.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "height") switch { "" => "1680", var s => s };
                    dataTab.txtDpi.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "dpi") switch { "" => "300", var s => s };

                    string noteMode = YamlService.ReadYamlSectionValue(yamlPath, ["footnotes:"], "mode");
                    string[] noteValues = ["default", "float", "floatRenumbered"];
                    int nIdx = dataTab.cmbNotesMode.Items.IndexOf(noteMode);
                    dataTab.cmbNotesMode.SelectedIndex = nIdx >= 0 ? nIdx : 0;

                    // 1. Зчитуємо значення для Soft Hyphen
                    bool isSoftHyphen = string.Equals(YamlService.ReadYamlValue(yamlPath, "insert_soft_hyphen"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbSoftHyphenYes.Checked = isSoftHyphen;
                    dataTab.rbSoftHyphenNo.Checked = !isSoftHyphen;

                    // 2. Зчитуємо значення для Remove Transparency
                    bool isRemoveTransp = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "remove_transparency"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbRemoveTranspYes.Checked = isRemoveTransp;
                    dataTab.rbRemoveTranspNo.Checked = !isRemoveTransp;

                    // 3. JPEG Quality
                    string jpegVal = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "jpeg_quality_level");
                    dataTab.txtJpegQuality.Text = string.IsNullOrEmpty(jpegVal) ? "95" : jpegVal;

                    // 4. Generate Cover
                    bool isGenCover = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "generate"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbGenCoverYes.Checked = isGenCover;
                    dataTab.rbGenCoverNo.Checked = !isGenCover;
                    dataTab.txtCoverPath.Text = YamlService.ReadYamlValue(yamlPath, "default_image_path");

                    string resize = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "resize");
                    int rIdx = dataTab.cmbResizeCover.Items.IndexOf(resize);
                    dataTab.cmbResizeCover.SelectedIndex = rIdx >= 0 ? rIdx : 2; // stretch

                    dataTab.rbAnnEnableYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["annotation:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbAnnEnableNo.Checked = !dataTab.rbAnnEnableYes.Checked;

                    dataTab.rbAnnInTocYes.Checked = !string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["annotation:"], "in_toc"), "false", StringComparison.OrdinalIgnoreCase); // default true
                    dataTab.rbAnnInTocNo.Checked = !dataTab.rbAnnInTocYes.Checked;

                    string placement = YamlService.ReadYamlSectionValue(yamlPath, ["toc_page:"], "placement");
                    int pIdx = dataTab.cmbTocPlacement.Items.IndexOf(placement);
                    dataTab.cmbTocPlacement.SelectedIndex = pIdx >= 0 ? pIdx : 0; // none

                    dataTab.rbDropcapsYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["dropcaps:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbDropcapsNo.Checked = !dataTab.rbDropcapsYes.Checked;

                    return;
                }
            }
            // Дефолти
            dataTab.txtWidth.Text = "1264";
            dataTab.txtHeight.Text = "1680";
            dataTab.txtDpi.Text = "300";
            dataTab.cmbNotesMode.SelectedIndex = 0;

            dataTab.rbSoftHyphenNo.Checked = true;
            dataTab.rbRemoveTranspNo.Checked = true;
            dataTab.txtJpegQuality.Text = "95";
            dataTab.rbGenCoverNo.Checked = true;
            dataTab.txtCoverPath.Text = "";
            dataTab.cmbResizeCover.SelectedIndex = 2; // stretch
            dataTab.rbAnnEnableNo.Checked = true;
            dataTab.rbAnnInTocYes.Checked = true; // default true
            dataTab.cmbTocPlacement.SelectedIndex = 0; // none
            dataTab.rbDropcapsNo.Checked = true;
        }

        // ========================================================
        // СИНХРОНІЗАЦІЯ LOGGING З YAML
        // ========================================================
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
                            int idx = logTab.cmbLogLevel.Items.IndexOf(level);
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

        private void SaveYamlConfiguration()
        {
            if (!_tabsCache.TryGetValue("document:", out UserControl? doc) || doc is not DocumentTab docTab)
            {
                return;
            }

            MetadataTab? dataTab = _tabsCache.TryGetValue("metadata:", out UserControl? data) ? data as MetadataTab : null;
            LoggingTab? logTab = _tabsCache.TryGetValue("logging:", out UserControl? log) ? log as LoggingTab : null;

            int[] fieldIndexes = new int[7];
            bool[] folderFlags = new bool[7];
            if (docTab.cmbOutFields != null)
            {
                for (int i = 0; i < 7; i++) { fieldIndexes[i] = docTab.cmbOutFields[i].SelectedIndex; folderFlags[i] = docTab.chkAsFolder![i].Checked; }
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
                    dataTab?.chkReaderSize.Checked ?? false, dataTab?.txtWidth.Text ?? "1264", dataTab?.txtHeight.Text ?? "1680", dataTab?.txtDpi.Text ?? "300",
                    dataTab?.chkNotes.Checked ?? false, _noteValues[dataTab?.cmbNotesMode.SelectedIndex ?? 0],
                    dataTab?.chkSoftHyphen.Checked ?? false, dataTab?.rbSoftHyphenYes.Checked ?? false,
                    dataTab?.chkRemoveTransp.Checked ?? false, dataTab?.rbRemoveTranspYes.Checked ?? false,
                    dataTab?.chkJpegQuality.Checked ?? false, dataTab?.txtJpegQuality.Text ?? "95",
                    dataTab?.chkGenerateCover.Checked ?? false, dataTab?.rbGenCoverYes.Checked ?? false, dataTab?.txtCoverPath.Text ?? "",
                    dataTab?.chkResizeCover.Checked ?? false, _resizeValues[dataTab?.cmbResizeCover.SelectedIndex ?? 2],
                    dataTab?.chkAnnEnable.Checked ?? false, dataTab?.rbAnnEnableYes.Checked ?? false,
                    dataTab?.chkAnnInToc.Checked ?? false, dataTab?.rbAnnInTocYes.Checked ?? true,
                    dataTab?.chkTocPlacement.Checked ?? false, _placementValues[dataTab?.cmbTocPlacement.SelectedIndex ?? 0],
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

        private static void ShowHelp()
        {
            _ = Config.Localization.TryGetValue(Config.Settings.CurrentLanguage, out Dictionary<string, string>? langDict);

            string caption = langDict?.GetValueOrDefault("Help", "Help / Довідка") ?? "Help / Довідка";
            string msg = langDict?.GetValueOrDefault("HelpText", "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.") ?? "fb2cng Template Configurator\nCreated for fb2cng GUI toolset.";

            _ = ShowCustomMessageBox(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
