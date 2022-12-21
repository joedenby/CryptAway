using System.Diagnostics;
using System.Security.AccessControl;
using System.Text;

namespace CryptAway
{
    internal class Folder
    {
        public static bool hasPath => File.Exists("path.dat");

        public static bool SetPath(string path)
        {
            Message.LogDev("SetPath", $"Setting new path [{path}]");
            try
            {
                if (!Directory.Exists(path)) {
                    Message.LogDev("SetPath", $"Path was not found.");
                    return false;
                }

                Message.LogDev("SetPath", "Saved path.");
                byte[] byteArray = Encoding.UTF8.GetBytes(path);
                File.WriteAllBytes("path.dat", byteArray);
                return true;
            }
            catch (ArgumentNullException exception)
            {
                Message.LogError(exception.Message);
                Message.LogDev("SetPath", "Something went wrong.]");
                return false;
            }
        }

        public static string GetPath() {
            if (!hasPath) {
                Message.LogDev("GetPath", "Attempted to get path without allocation.");
                return string.Empty;
            }

            Message.LogDev("GetPath", "Path request honored.");
            return File.ReadAllText("path.dat");
        }

        public static void OpenFolder() {
            if (!hasPath) {
                Message.LogDev("OpenFolder", "Tried to open folder without allocated path.");
                return;
            }

            Message.LogDev("OpenFolder", "Attempted to open folder with allocated path.");
            Process.Start("explorer.exe", GetPath());
        }

        public static void ShowFolder(string folderPath) {
            Message.LogDev("ShowFolder", "Attempting to reveal folder...");
            // Create a new DirectoryInfo object.
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            Message.LogDev("ShowFolder", "Accessed directory info.");

            // Create a new DirectorySecurity object.
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
            Message.LogDev("ShowFolder", "Accessed directory security.");

            // Remove the access rule that denies access to all users.
            dirSecurity.RemoveAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny));
            Message.LogDev("ShowFolder", "Setting system rights to 'FullControl'");

            // Set the modified access control for the folder.
            dirInfo.SetAccessControl(dirSecurity);
            Message.LogDev("ShowFolder", "Rights set to 'FullControl'");

            // Create a DirectoryInfo object for the directory
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            // Set the hidden and system attributes on the directory
            directoryInfo.Attributes = FileAttributes.Normal;
            Message.LogDev("ShowFolder","Folder has been revealed.");
        }

        public static void HideFolder(string folderPath)
        {
            Message.LogDev("HideFolder", "Attempting to hide folder...");
            // Create a DirectoryInfo object for the directory
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            // Set the hidden and system attributes on the directory
            directoryInfo.Attributes = FileAttributes.Hidden | FileAttributes.System;
            Message.LogDev("HideFolder", "Folder has been hidden.");

            // Create a new DirectoryInfo object.
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            Message.LogDev("HideFolder", "Accessed directory info.");

            // Create a new DirectorySecurity object.
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
            Message.LogDev("HideFolder", "Accessed directory security.");

            // Deny access to all users.
            dirSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny));
            Message.LogDev("HideFolder", "Setting system rights to 'Deny'");

            // Set the access control for the folder.
            dirInfo.SetAccessControl(dirSecurity);
            Message.LogDev("ShowFolder", "Rights set to 'Deny'");
        }
    
        public static bool FolderAccessable(string folderPath)
        {
            Message.LogDev("FolderAccessible", "Checking if folder is accessable...");
            // Create a new DirectoryInfo object.
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

            // Get the access control for the folder.
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            // Get the access control rules for the folder.
            AuthorizationRuleCollection rules = dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

            // Iterate through the access control rules.
            foreach (FileSystemAccessRule rule in rules)
            {
                // Check if the rule allows access to the folder.
                if (rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute) &&
                    rule.AccessControlType == AccessControlType.Allow)
                {
                    // The folder is accessable.
                    Message.LogDev("FolderAccessible", "The folder is accessable.");
                    return true;
                }
            }

            // The folder is not accessable.
            Message.LogDev("FolderAccessible", "The folder is not accessable.");
            return false;
        }

    }
}
