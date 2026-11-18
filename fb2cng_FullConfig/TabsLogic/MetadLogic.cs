using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;

namespace fb2cng_FullConfig
{
    public partial class Form1
    {
        private void InitializeMetadataTabEvents(MetadataTab dataTab)
        {
            // ВИКЛИК ApplyTheme ДО УСІХ ЧЕКБОКСІВ МЕТАДАНИХ
            CheckBox[] metaChecks = [
                dataTab.chkSoftHyphen, dataTab.chkKfxCompression,
                dataTab.chkAzw3Compression,dataTab.chkAzw3Apnx,
                dataTab.chkPdfPageFrame, dataTab.chkMarkdownImages,
                dataTab.chkPageMapEnable,dataTab.chkPageMapSize,
                dataTab.chkAdobeDe, dataTab.chkNotes,
                dataTab.chkAnnEnable,dataTab.chkAnnInToc,
                dataTab.chkTocPlacement,dataTab.chkInclNoTitle,
                dataTab.chkVignettes, dataTab.chkDropcaps,
                dataTab.chkSpeech, dataTab.chkDashes, dataTab.chkDialogue
            ];

            foreach (CheckBox chk in metaChecks)
            {
                chk.CheckedChanged += (s, e) => ApplyTheme();
            }

            TooltipManager.Attach(dataTab.chkSoftHyphen, "SoftHyphen");
            TooltipManager.Attach(dataTab.chkKfxCompression, "KfxCompression");
            TooltipManager.Attach(dataTab.chkAzw3Compression, "Azw3Compression");
            TooltipManager.Attach(dataTab.chkAzw3Apnx, "Azw3Apnx");
            TooltipManager.Attach(dataTab.chkPdfPageFrame, "PdfPageFrame");
            TooltipManager.Attach(dataTab.chkMarkdownImages, "MarkdownImages");
            TooltipManager.Attach(dataTab.chkPageMapEnable, "PageMapEnable");
            TooltipManager.Attach(dataTab.chkPageMapSize, "PageMapSize");
            TooltipManager.Attach(dataTab.chkAdobeDe, "AdobeDe");
            TooltipManager.Attach(dataTab.chkNotes, "FootnotesMode");
            TooltipManager.Attach(dataTab.chkAnnEnable, "AnnEnable");
            TooltipManager.Attach(dataTab.chkAnnInToc, "AnnInToc");
            TooltipManager.Attach(dataTab.chkTocPlacement, "TocPlacement");
            TooltipManager.Attach(dataTab.chkInclNoTitle, "InclNoTitle");
            TooltipManager.Attach(dataTab.chkVignettes, "Vignettes");
            TooltipManager.Attach(dataTab.chkDropcaps, "Dropcaps");
            TooltipManager.Attach(dataTab.chkSpeech, "TextTransformSpeech");
            TooltipManager.Attach(dataTab.chkDashes, "TextTransformDashes");
            TooltipManager.Attach(dataTab.chkDialogue, "TextTransformDialogue");
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
                    // Soft Hyphen
                    bool isSoftHyphen = string.Equals(YamlService.ReadYamlValue(yamlPath, "insert_soft_hyphen"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbSoftHyphenYes.Checked = isSoftHyphen;
                    dataTab.rbSoftHyphenNo.Checked = !isSoftHyphen;

                    string kfxComp = YamlService.ReadYamlSectionValue(yamlPath, ["kfx:"], "compression").ToLowerInvariant();
                    int kcIdx = Array.IndexOf(_kfxCompressionValues, kfxComp);
                    dataTab.cmbKfxCompression.SelectedIndex = kcIdx >= 0 ? kcIdx : 0;

                    string azw3Comp = YamlService.ReadYamlSectionValue(yamlPath, ["azw3:"], "compression").ToLowerInvariant();
                    int acIdx = Array.IndexOf(_azw3CompressionValues, azw3Comp);
                    dataTab.cmbAzw3Compression.SelectedIndex = acIdx >= 0 ? acIdx : 1;

                    string azw3Apnx = YamlService.ReadYamlSectionValue(yamlPath, ["azw3:"], "apnx").ToLowerInvariant();
                    int apIdx = Array.IndexOf(_azw3ApnxValues, azw3Apnx);
                    dataTab.cmbAzw3Apnx.SelectedIndex = apIdx >= 0 ? apIdx : 0;

                    dataTab.txtPdfPageFrame.Text = YamlService.ReadYamlSectionValue(yamlPath, ["pdf:"], "page_frame") switch { "" => "0", var s => s };

                    string mdImg = YamlService.ReadYamlSectionValue(yamlPath, ["markdown:"], "images").ToLowerInvariant();
                    int miIdx = Array.IndexOf(_markdownImagesValues, mdImg);
                    dataTab.cmbMarkdownImages.SelectedIndex = miIdx >= 0 ? miIdx : 0;

                    // Page Map
                    dataTab.rbPageMapYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "enable") == "true";
                    dataTab.rbPageMapNo.Checked = !dataTab.rbPageMapYes.Checked;
                    dataTab.txtPageMapSize.Text = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "size") switch { "" => "2300", var s => s };
                    dataTab.rbAdobeDeYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["page_map:"], "adobe_de") == "true";
                    dataTab.rbAdobeDeNo.Checked = !dataTab.rbAdobeDeYes.Checked;

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

                    // NoTitleinTOC & Vignettes
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

                    dataTab.rbSpeechYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["text_transformations:", "speech:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbSpeechNo.Checked = !dataTab.rbSpeechYes.Checked;

                    dataTab.rbDashesYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["text_transformations:", "dashes:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbDashesNo.Checked = !dataTab.rbDashesYes.Checked;

                    dataTab.rbDialogueYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["text_transformations:", "dialogue:"], "enable"), "true", StringComparison.OrdinalIgnoreCase);
                    dataTab.rbDialogueNo.Checked = !dataTab.rbDialogueYes.Checked;

                    return;
                }
            }
            // Дефолти
            dataTab.rbSoftHyphenNo.Checked = true;
            dataTab.cmbKfxCompression.SelectedIndex = 0;
            dataTab.cmbAzw3Compression.SelectedIndex = 1; // standard
            dataTab.cmbAzw3Apnx.SelectedIndex = 0;
            dataTab.txtPdfPageFrame.Text = "0";
            dataTab.cmbMarkdownImages.SelectedIndex = 0;
            dataTab.rbPageMapYes.Checked = true;      // за замовчуванням Так
            dataTab.txtPageMapSize.Text = "2300";    // за замовчуванням 2300
            dataTab.rbAdobeDeNo.Checked = true;      // за замовчуванням Ні
            dataTab.cmbNotesMode.SelectedIndex = 0;
            dataTab.rbAnnEnableNo.Checked = true;
            dataTab.rbAnnInTocYes.Checked = true; // default true
            dataTab.cmbTocPlacement.SelectedIndex = 0; // none
            dataTab.rbInclNoTitleNo.Checked = true;  // за замовчуванням Ні
            dataTab.rbVignettesNo.Checked = true;    // за замовчуванням Ні
            dataTab.rbDropcapsNo.Checked = true;
            dataTab.rbSpeechNo.Checked = true;
            dataTab.rbDashesNo.Checked = true;
            dataTab.rbDialogueNo.Checked = true;
        }
    }
}
