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
        private static readonly string[] _resizeValues = ["none", "keepAR", "stretch", "fit"];
        private static readonly string[] _placementValues = ["none", "before", "after"];
        private static readonly string[] _logLevels = ["none", "normal", "debug"];

        //=================================
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
                FillCombo(dataTab.cmbResizeCover, ["Opt_Resize_None", "Opt_Resize_KeepAR", "Opt_Resize_Stretch", "Opt_Resize_Fit"]);
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

        //================================
        // --- 3. Збереження (Запис) ---
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
                    msg = loc.TryGetValue("ErrSourceMissing", out string? t)
              ? string.Format(t, docTab.txtCustomYamlPath.Text)
              : $"Source file '{docTab.txtCustomYamlPath.Text}' not found!";
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
        // --- 4. Утиліти (Help, GUI) ---
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
            _ = btnOk.Focus();
        }
    }
}
