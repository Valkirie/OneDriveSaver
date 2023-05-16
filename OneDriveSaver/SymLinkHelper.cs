using System.Runtime.InteropServices;

namespace OneDriveSaver
{
    public static class SymLinkHelper
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkType dwFlags);

        public enum SymbolicLinkType
        {
            File = 0,
            TopDirectoryOnly = 1,
            AllDirectories = 2,
        }
    }
}
