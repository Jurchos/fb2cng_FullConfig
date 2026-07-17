using System;
using System.Windows.Forms;
using System.Drawing;

namespace fb2cng_FullConfig.Templates;

public partial class ImagesTab : UserControl
{
    public CheckBox chkReaderSize = null!;
    public Label lblWidth = null!, lblHeight = null!, lblDpi = null!;
    public TextBox txtWidth = null!, txtHeight = null!, txtDpi = null!;

    public ImagesTab()
    {
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        SetupInterface();
    }

    private void SetupInterface()
    {
        float currentScale = CreateGraphics().DpiX / 96f;
        int labelHeight = (int)(20 * currentScale);
        int fieldHeight = (int)(24 * currentScale);
        int checkBoxHeight = (int)(22 * currentScale);
        int xLeft = (int)(16 * currentScale);
        int textLabelWidth = (int)(240 * currentScale); // Додано пропущену змінну

        chkReaderSize = new CheckBox { AutoSize = true };
        lblWidth = new Label { Text = "W:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        txtWidth = new TextBox { Text = "1264", Enabled = false, Multiline = true };
        lblHeight = new Label { Text = "H:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        txtHeight = new TextBox { Text = "1680", Enabled = false, Multiline = true };
        lblDpi = new Label { Text = "DPI:", Enabled = false, AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
        txtDpi = new TextBox { Text = "300", Enabled = false, Multiline = true };

        chkReaderSize.CheckedChanged += (s, e) =>
        {
            bool en = chkReaderSize.Checked;
            lblWidth.Enabled = txtWidth.Enabled = lblHeight.Enabled = txtHeight.Enabled = lblDpi.Enabled = txtDpi.Enabled = en;
            (ParentForm as Form1)?.ApplyTheme();
        };

        Controls.AddRange([chkReaderSize, lblWidth, txtWidth, lblHeight, txtHeight, lblDpi, txtDpi]);

        int nextY = (int)(15 * currentScale);
        // Виправлено SetBounds (ширина має бути більшою за висоту)
        chkReaderSize.SetBounds(xLeft, nextY + (int)(1 * currentScale), (int)(300 * currentScale), checkBoxHeight);

        int labelWidthSpace = (int)(22 * currentScale);
        int exactBoxWidth = (int)(44 * currentScale);
        int betweenGroupsSpacing = (int)(10 * currentScale);

        int sizeInputX = xLeft + textLabelWidth;

        // 1. Блок Width
        int wLabelWidth = labelWidthSpace + (int)(4 * currentScale);
        lblWidth.SetBounds(sizeInputX, nextY + (int)(2 * currentScale), wLabelWidth, labelHeight);
        txtWidth.SetBounds(lblWidth.Right, nextY, exactBoxWidth, fieldHeight);

        // 2. Блок Height
        lblHeight.SetBounds(txtWidth.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), labelWidthSpace, labelHeight);
        txtHeight.SetBounds(lblHeight.Right, nextY, exactBoxWidth, fieldHeight);

        // 3. Блок DPI
        int dpiLabelWidth = labelWidthSpace + (int)(12 * currentScale);
        lblDpi.SetBounds(txtHeight.Right + betweenGroupsSpacing, nextY + (int)(2 * currentScale), dpiLabelWidth, labelHeight);
        txtDpi.SetBounds(lblDpi.Right, nextY, exactBoxWidth, fieldHeight);
    }
}