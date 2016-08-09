using System.Reflection;
using System.Resources;
using PluginCore.Localization;

namespace UnitTestSessionsPanel.Localization
{
    class LocalizationHelper
    {
        private static ResourceManager resources;

        public static void Initialize(LocaleVersion locale)
        {
            string path = "UnitTestSessionsPanel.Localization." + locale;
            resources = new ResourceManager(path, Assembly.GetExecutingAssembly());
        }

        public static string GetString(string identifier)
        {
            return resources.GetString(identifier);
        }
    }
}