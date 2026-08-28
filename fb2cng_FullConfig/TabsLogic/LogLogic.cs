using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;

namespace fb2cng_FullConfig
{
    public partial class Form1 // Це частина того ж класу Form1
    {
        private void InitializeLoggingTabEvents(LoggingTab logTab)
        {
            // Всі події логування тепер тут (ініціалізуються один раз)
            logTab.chkLogLevel.CheckedChanged += (s, e) => ApplyTheme();
            logTab.chkLogName.CheckedChanged += (s, e) => ApplyTheme();
            logTab.chkPanicLogName.CheckedChanged += (s, e) => ApplyTheme();
            logTab.chkLogMode.CheckedChanged += (s, e) => ApplyTheme();
            logTab.chkLogFolder.CheckedChanged += (s, e) => ApplyTheme();

            TooltipManager.Attach(logTab.chkLogLevel, "LogLevel");
            TooltipManager.Attach(logTab.chkLogName, "LogName");
            TooltipManager.Attach(logTab.chkPanicLogName, "LogPanicName");
            TooltipManager.Attach(logTab.chkLogMode, "LogMode");
            TooltipManager.Attach(logTab.chkLogFolder, "LogFolder");
            TooltipManager.Attach(logTab.lblShowTips, "ShowTooltips");
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
    }
}
