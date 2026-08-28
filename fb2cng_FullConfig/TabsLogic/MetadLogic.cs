using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;
using fb2cng_FullConfig.Utils;


namespace fb2cng_FullConfig
{
    public partial class Form1
    {
        private void InitializeMetadataTabEvents(MetadataTab dataTab)
        {
            UiStyles.MakeButtonRounded(dataTab.btnBrowseCover, UiStyles.GetScaled(4));
            dataTab.btnBrowseCover.Click += BtnBrowseCover_Click;

            // ВИКЛИК ApplyTheme ДО УСІХ ЧЕКБОКСІВ МЕТАДАНИХ
            CheckBox[] metaChecks = [
                dataTab.chkReaderSize, dataTab.chkNotes,
                dataTab.chkSoftHyphen, dataTab.chkRemoveTransp,
                dataTab.chkJpegQuality, dataTab.chkGenerateCover,
                dataTab.chkResizeCover, dataTab.chkAnnEnable,
                dataTab.chkAnnInToc, dataTab.chkTocPlacement,
                dataTab.chkPageMapSize, dataTab.chkScaleFactor,
                dataTab.chkPageMapEnable, dataTab.chkAdobeDe,
                dataTab.chkUseBroken, dataTab.chkImgOptimize,
                dataTab.chkInclNoTitle, dataTab.chkVignettes,
                dataTab.chkDropcaps
            ];

            foreach (CheckBox chk in metaChecks)
            {
                chk.CheckedChanged += (s, e) => ApplyTheme();
            }

            // спільний метод малювання! Оскільки матриця InactiveIconMatrix лежить у UiStyles, передаємо її через клас
            UiStyles.SetupIconButtonDrawing(
                dataTab.btnBrowseCover,
                Properties.Resources.folder,
                dataTab.chkGenerateCover,
                UiStyles.InactiveIconMatrix
            );
            TooltipManager.Attach(dataTab.chkSoftHyphen, "SoftHyphen");
            TooltipManager.Attach(dataTab.chkPageMapEnable, "PageMapEnable");
            TooltipManager.Attach(dataTab.chkPageMapSize, "PageMapSize");
            TooltipManager.Attach(dataTab.chkAdobeDe, "AdobeDe");
            TooltipManager.Attach(dataTab.chkUseBroken, "UseBroken");
            TooltipManager.Attach(dataTab.chkRemoveTransp, "RemoveTransp");
            TooltipManager.Attach(dataTab.chkScaleFactor, "ScaleFactor");
            TooltipManager.Attach(dataTab.chkImgOptimize, "ImgOptimize");
            TooltipManager.Attach(dataTab.chkJpegQuality, "JpegQuality");
            TooltipManager.Attach(dataTab.chkReaderSize, "ReaderSize");
            TooltipManager.Attach(dataTab.chkGenerateCover, "GenCover");
            TooltipManager.Attach(dataTab.chkResizeCover, "ResizeCover");
            TooltipManager.Attach(dataTab.chkNotes, "FootnotesMode");
            TooltipManager.Attach(dataTab.chkAnnEnable, "AnnEnable");
            TooltipManager.Attach(dataTab.chkAnnInToc, "AnnInToc");
            TooltipManager.Attach(dataTab.chkTocPlacement, "TocPlacement");
            TooltipManager.Attach(dataTab.chkInclNoTitle, "InclNoTitle");
            TooltipManager.Attach(dataTab.chkVignettes, "Vignettes");
            TooltipManager.Attach(dataTab.chkDropcaps, "Dropcaps");
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
            _ = btnOk.Focus();
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
                    bool vigRootFound = false;
                    string[] allLinesForVig = File.ReadAllLines(yamlPath);
                    foreach (string line in allLinesForVig)
                    {
                        string trimmedLine = line.TrimStart();
                        // Перевіряємо, чи рядок починається саме з "vignettes:" і чи він не закоментований
                        if (trimmedLine.StartsWith("vignettes:", StringComparison.Ordinal))
                        {
                            vigRootFound = true;
                            break;
                        }
                    }
                    dataTab.rbVignettesYes.Checked = vigRootFound;
                    dataTab.rbVignettesNo.Checked = !vigRootFound;

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
    }
}
