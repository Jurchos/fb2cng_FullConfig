using System.Reflection;

namespace fb2cng_FullConfig.Utils
{
    public static class AppInfo
    {
        public static string GetSimpleVersion()
        {
            // Отримуємо версію з Application.ProductVersion (це значення <Version> з .csproj)
            string version = Application.ProductVersion;
            // Повертаємо лише Major.Minor (наприклад, 1.6)
            return Version.TryParse(version, out Version? v) ? $"{v.Major}.{v.Minor}" : version;
        }
 
        public static string GetCopyright()
        {
            // Отримуємо значення <Copyright> через атрибути збірки
            return Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Jurchos & Gemini";
        }
    }
}