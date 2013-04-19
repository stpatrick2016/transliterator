using System.Runtime.InteropServices;

namespace Transliterator.UI.Windows
{
    internal static class EngineAdapter
    {
        [DllImport("TrEngine.dll", CharSet = CharSet.Unicode)]
        public static extern bool RegisterKeyboardLayout(string filepath);

        [DllImport("TrEngine.dll")]
        public static extern void EnableTransliteration();

        [DllImport("TrEngine.dll")]
        public static extern void DisableTransliteration();

        [DllImport("TrEngine.dll")]
        public static extern void SetCurrentLayout(int index);

        [DllImport("TrEngine.dll")]
        public static extern bool IsTransliterationEnabled();
    }
}
