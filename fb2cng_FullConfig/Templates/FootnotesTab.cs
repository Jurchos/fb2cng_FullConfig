using System;
using System.Windows.Forms;

namespace fb2cng_FullConfig.Templates;

// Обов'язково додаємо спадкування від UserControl
public partial class FootnotesTab : UserControl
{
    public FootnotesTab()
    {
        DoubleBuffered = true;
        // Сюди у майбутньому ви додасте нові чекбокси та текстові поля для метаданих
        Controls.Add(new Label { Text = "Розділ налаштувань Footnotes (В розробці...)", AutoSize = true, Left = 20, Top = 20 });
    }
}

