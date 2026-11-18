using fb2cng_FullConfig.Services;
using fb2cng_FullConfig.Templates;
using fb2cng_FullConfig.Utils;

namespace fb2cng_FullConfig
{
    public partial class Form1
    {
        private void InitializeImagesTabEvents(ImagesTab imgTab)
        {
            UiStyles.MakeButtonRounded(imgTab.btnBrowseCover, UiStyles.GetScaled(4));
            imgTab.btnBrowseCover.Click += BtnBrowseCover_Click;

            CheckBox[] imgChecks = [
               imgTab.chkReaderSize, imgTab.chkUseBroken,
               imgTab.chkRemoveTransp, imgTab.chkRasterizeSvg,
               imgTab.chkRasterFormat, imgTab.chkScaleFactor,
               imgTab.chkFitToScreen, imgTab.chkBaselineJpeg,
               imgTab.chkGrayscale,
               imgTab.chkImgOptimize, imgTab.chkJpegQuality,
               imgTab.chkGenerateCover, imgTab.chkResizeCover
            ];

            foreach (CheckBox chk in imgChecks)
            {
                chk.CheckedChanged += (s, e) => ApplyTheme();
            }
            // спільний метод малювання! Оскільки матриця InactiveIconMatrix лежить у UiStyles, передаємо її через клас
            UiStyles.SetupIconButtonDrawing(
                imgTab.btnBrowseCover,
                Properties.Resources.folder,
                imgTab.chkGenerateCover,
                UiStyles.InactiveIconMatrix
            );

            TooltipManager.Attach(imgTab.chkReaderSize, "ReaderSize");
            TooltipManager.Attach(imgTab.chkUseBroken, "UseBroken");
            TooltipManager.Attach(imgTab.chkRemoveTransp, "RemoveTransp");
            TooltipManager.Attach(imgTab.chkRasterizeSvg, "RasterizeSvg");
            TooltipManager.Attach(imgTab.chkRasterFormat, "RasterFormat");
            TooltipManager.Attach(imgTab.chkScaleFactor, "ScaleFactor");
            TooltipManager.Attach(imgTab.chkFitToScreen, "FitToScreen");
            TooltipManager.Attach(imgTab.chkBaselineJpeg, "BaselineJpeg");
            TooltipManager.Attach(imgTab.chkGrayscale, "Grayscale");
            TooltipManager.Attach(imgTab.chkImgOptimize, "ImgOptimize");
            TooltipManager.Attach(imgTab.chkJpegQuality, "JpegQuality");
            TooltipManager.Attach(imgTab.chkGenerateCover, "GenCover");
            TooltipManager.Attach(imgTab.chkResizeCover, "ResizeCover");
        }

        internal void BtnBrowseCover_Click(object? sender, EventArgs e)
        {
            if (_tabsCache.TryGetValue("images:", out UserControl? tab) && tab is ImagesTab imgTab)
            {
                using OpenFileDialog ofd = new();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    imgTab.txtCoverPath.Text = Path.GetRelativePath(appPath, ofd.FileName).Replace('\\', '/');
                }
            }
            _ = btnOk.Focus();
        }

        private void SyncImagesWithYaml(DocumentTab docTab)
        {
            if (!_tabsCache.TryGetValue("images:", out UserControl? img) || img is not ImagesTab imgTab)
            {
                return;
            }

            if (docTab.chkCustomYaml.Checked)
            {
                string yamlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, docTab.txtCustomYamlPath.Text.Trim());
                if (File.Exists(yamlPath))
                {
                    // Reader Size
                    imgTab.txtWidth.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "width") switch { "" => "1264", var s => s };
                    imgTab.txtHeight.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "height") switch { "" => "1680", var s => s };
                    imgTab.txtDpi.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "screen:"], "dpi") switch { "" => "300", var s => s };

                    imgTab.rbUseBrokenYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "use_broken") == "true";
                    imgTab.rbUseBrokenNo.Checked = !imgTab.rbUseBrokenYes.Checked;

                    // Remove Transparency
                    bool isRemoveTransp = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "remove_transparency"), "true", StringComparison.OrdinalIgnoreCase);
                    imgTab.rbRemoveTranspYes.Checked = isRemoveTransp;
                    imgTab.rbRemoveTranspNo.Checked = !isRemoveTransp;

                    imgTab.rbRasterizeSvgYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "rasterize_svg"), "true", StringComparison.OrdinalIgnoreCase);
                    imgTab.rbRasterizeSvgNo.Checked = !imgTab.rbRasterizeSvgYes.Checked;

                    string rFmt = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "raster_format").ToLowerInvariant();
                    int rfIdx = Array.IndexOf(_rasterFormatValues, rFmt);
                    imgTab.cmbRasterFormat.SelectedIndex = rfIdx >= 0 ? rfIdx : 0;

                    imgTab.txtScaleFactor.Text = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "scale_factor") switch { "" => "1.0", var s => s };

                    imgTab.rbFitToScreenYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "fit_to_screen"), "true", StringComparison.OrdinalIgnoreCase);
                    imgTab.rbFitToScreenNo.Checked = !imgTab.rbFitToScreenYes.Checked;

                    imgTab.rbBaselineJpegYes.Checked = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "baseline_jpeg"), "true", StringComparison.OrdinalIgnoreCase);
                    imgTab.rbBaselineJpegNo.Checked = !imgTab.rbBaselineJpegYes.Checked;

                    string gray = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "grayscale").ToLowerInvariant();
                    int gIdx = Array.IndexOf(_grayscaleValues, gray);
                    imgTab.cmbGrayscale.SelectedIndex = gIdx >= 0 ? gIdx : 0;

                    imgTab.rbImgOptimizeYes.Checked = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "optimize") == "true";
                    imgTab.rbImgOptimizeNo.Checked = !imgTab.rbImgOptimizeYes.Checked;

                    // JPEG Quality
                    string jpegVal = YamlService.ReadYamlSectionValue(yamlPath, ["images:"], "jpeg_quality_level");
                    imgTab.txtJpegQuality.Text = string.IsNullOrEmpty(jpegVal) ? "95" : jpegVal;

                    // Generate Cover
                    bool isGenCover = string.Equals(YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "generate"), "true", StringComparison.OrdinalIgnoreCase);
                    imgTab.rbGenCoverYes.Checked = isGenCover;
                    imgTab.rbGenCoverNo.Checked = !isGenCover;
                    imgTab.txtCoverPath.Text = YamlService.ReadYamlValue(yamlPath, "default_image_path");

                    string resize = YamlService.ReadYamlSectionValue(yamlPath, ["images:", "cover:"], "resize");
                    int rIdx = Array.IndexOf(_resizeValues, resize.Replace("\"", ""));
                    imgTab.cmbResizeCover.SelectedIndex = rIdx >= 0 ? rIdx : 2;

                    return;
                }
            }

            // Дефолтні значення, якщо файл не вибрано
            imgTab.txtWidth.Text = "1264";
            imgTab.txtHeight.Text = "1680";
            imgTab.txtDpi.Text = "300";
            imgTab.rbUseBrokenNo.Checked = true;
            imgTab.rbRemoveTranspNo.Checked = true;
            imgTab.rbRasterizeSvgNo.Checked = true;
            imgTab.cmbRasterFormat.SelectedIndex = 0;
            imgTab.txtScaleFactor.Text = "1.0";
            imgTab.rbFitToScreenNo.Checked = true;
            imgTab.rbBaselineJpegNo.Checked = true;
            imgTab.cmbGrayscale.SelectedIndex = 0;
            imgTab.rbImgOptimizeYes.Checked = true;
            imgTab.txtJpegQuality.Text = "95";
            imgTab.rbGenCoverNo.Checked = true;
            imgTab.txtCoverPath.Text = "";
            imgTab.cmbResizeCover.SelectedIndex = 2;
        }
    }
}