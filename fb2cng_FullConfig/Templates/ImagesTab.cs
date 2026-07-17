using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace fb2cng_FullConfig.Templates;

// Обов'язково додаємо спадкування від UserControl
public partial class ImagesTab : UserControl
{
    public ImagesTab()
    {
        DoubleBuffered = true;
        // Сюди у майбутньому ви додасте нові чекбокси та текстові поля для метаданих
        Controls.Add(new Label { Text = "Розділ налаштувань Images (В розробці...)", AutoSize = true, Left = 20, Top = 20 });
    }
}
