
namespace fb2cng_FullConfig.Utils
{
    public static class ControlExtensions // Розширення для перевірки null перед (залишаємо без змін)
    {
        // Встановлює текст, якщо контрол не порожній
        public static void SetTextIfNotNull(this Control? control, string text)
        {
            if (control == null)
            {
                return;
            }
            control.Text = text;
        }

        // Встановлює текст для цілого списку контролів (наприклад, масив чекбоксів)
        public static void SetTextForAllIfNotNull(this IEnumerable<Control?>? controls, string text)
        {
            if (controls == null)
            {
                return;
            }

            foreach (Control? control in controls)
            {
                control.SetTextIfNotNull(text);
            }
        }

        // Метод для безпечного отримання обраного елемента з ComboBox
        public static string GetSelectedText(this ComboBox? comboBox, string @default = "")
        {
            return comboBox?.SelectedItem?.ToString() ?? @default;
        }
    }
}
