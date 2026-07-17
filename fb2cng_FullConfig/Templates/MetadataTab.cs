using System;
using System.Windows.Forms;

namespace fb2cng_FullConfig.Templates;

// Обов'язково додаємо спадкування від UserControl
public partial class MetadataTab : UserControl
{
    public MetadataTab()
    {
        DoubleBuffered = true;
        // Сюди у майбутньому ви додасте нові чекбокси та текстові поля для метаданих
        Controls.Add(new Label { Text = "Розділ налаштувань метаданих (В розробці...)", AutoSize = true, Left = 20, Top = 20 });
    }
}

