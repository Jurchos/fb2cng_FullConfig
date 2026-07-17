using System;
using System.Windows.Forms;
using System.Drawing;

namespace fb2cng_FullConfig.Templates;

public partial class FootnotesTab : UserControl
{
    public CheckBox chkNotes = null!;
    public ComboBox cmbNotesMode = null!;

    public FootnotesTab()
    {
        DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        SetupInterface();
    }

    private void SetupInterface()
    {
        float currentScale = CreateGraphics().DpiX / 96f;
        int blockMargin = (int)(9 * currentScale);
        int fieldHeight = (int)(24 * currentScale);
        int checkBoxHeight = (int)(22 * currentScale);
        int xLeft = (int)(16 * currentScale);
        int textLabelWidth = (int)(240 * currentScale);

        // Контроли виносок
        chkNotes = new CheckBox { AutoSize = true };
        cmbNotesMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
        cmbNotesMode.Items.AddRange(["default", "float", "floatRenumbered"]);
        cmbNotesMode.SelectedIndex = 0;

        // Логіка активації
        chkNotes.CheckedChanged += (s, e) => {
            cmbNotesMode.Enabled = chkNotes.Checked;
            (ParentForm as Form1)?.ApplyTheme();
        };

        Controls.AddRange([chkNotes, cmbNotesMode]);

        int nextY = (int)(15 * currentScale);
        int valueFieldWidth = (int)(520 * currentScale) - (xLeft * 2) - textLabelWidth - (int)(11 * currentScale);

        chkNotes.SetBounds(xLeft, nextY + (int)(1 * currentScale), textLabelWidth, checkBoxHeight);
        cmbNotesMode.ItemHeight = fieldHeight - 6;
        cmbNotesMode.SetBounds(xLeft + textLabelWidth, nextY, valueFieldWidth, fieldHeight);
    }
}