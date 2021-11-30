using System.Runtime.InteropServices;

namespace DropboxMe
{
    public class SymLinkHelper
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkType dwFlags);

        public enum SymbolicLinkType
        {
            File = 0,
            Directory = 1
        }
    }
}
