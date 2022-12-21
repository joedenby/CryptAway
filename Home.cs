namespace CryptAway
{
    internal class Home
    {
        public static bool devMode = false;
        private static Dictionary<string, Command> commands = new();
        public static bool signedIn = false;

        public static void Start() {
            if (commands.Count == 0) { 
                BuildCommandDictionary();
            }
            if (!Encryption.HasPassword())
            {
                if (!Folder.hasPath) {
                    Execute("password");
                    return;
                }

                Message.LogError("Could not find password.dat!");
                return;
            }
            else if (!signedIn) {
                Message.Log("Enter password:");
                var pass = Console.ReadLine();
                signedIn = Encryption.CheckPassword(pass);
                if (!signedIn)
                {
                    Message.LogError("Incorrect Password.");
                    Start();
                    return;
                }

                Execute("clear");
                return;
            }


            string response = Console.ReadLine();
            Execute(response);
        }

        private static void Execute(string call) {
            if (string.IsNullOrEmpty(call)) {
                Console.Clear();
                Start();
                return;
            }

            // Split the input into words, separated by spaces
            string[] words = Array.ConvertAll(call.Split(' '), s => s.Replace("_", " "));

            // The first word is the command name
            string commandName = words[0];

            // The rest of the words are the arguments
            string[] args = new string[words.Length - 1];
            for (int i = 1; i < words.Length; i++)
            {
                args[i - 1] = words[i];
            }

            // Look up the command by name and execute it
            Command command;
            if (commands.TryGetValue(commandName, out command)) {
                command.Execute(args);
            }
            else
            {
                Message.LogError("Unknown command: " + commandName);
                Start();
            }
        }

        private static void BuildCommandDictionary() {
            // Find all types that derive from the Command class
            Type commandType = typeof(Command);
            Type[] commandTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(commandType)).ToArray();

            // Add an instance of each command type to the commands dictionary
            foreach (Type type in commandTypes) {
                Command command = (Command)Activator.CreateInstance(type);
                commands.Add(type.Name.ToLower(), command);
            }
        }
    }

    internal abstract class Command { 
        public abstract void Execute(string[] args);
    }

    internal class Clear : Command
    {
        public override void Execute(string[] args)
        {
            Console.Clear();
            Message.WelcomeMessage();
            Home.Start();
        }
    }

    internal class Dev : Command
    {
        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Message.LogError("Error: Did not provide -a request!");
                Message.LogError(@"Usage: dev 'on/off'");
                Home.Start();
                return;
            }

            if (args[0].ToLower().Equals("on") || args[0].ToLower().Equals("off"))
            {
                Home.devMode = args[0].ToLower().Equals("on");
                Message.Log("done." +
                    $"\nDev mode {(Home.devMode ? "enabled." : "disabled.")}");
            }
            else {
                Message.LogError($"Could not recognize '{args[0]}'. " +
                    $"\nTry 'dev on' or 'dev off'");
            }

            Home.Start();
        }
    }

    internal class Check : Command {
        public override void Execute(string[] args)
        {
            if (args.Length < 1) {
                Message.LogError("Error: Did not provide -a for path!");
                Message.LogError(@"Usage: check C:\Example\Folder");
                Home.Start();
                return;
            }

            if (!Folder.hasPath)
            {
                Message.Log("Currently no path has been allocated.");
                Message.Log(Directory.Exists(args[0]) ? $"Found '{args[0]}'" : $"Could not find '{args[0]}'");
            }
            else {
                string path = Folder.GetPath();
                Message.Log(path.Equals(args[0]) ? $"CryptAway is aware of [{args[0]}]" :
                    $"CryptAway has a different path assigned than the one provided.");
            }

            Home.Start();
        }
    }

    internal class Set : Command
    {
        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Message.LogError("Error: Did not provide -a for path!");
                Message.LogError(@"Usage: set C:\Example\Folder");
                Home.Start();
                return;
            }

            if (!Directory.Exists(args[0])) {
                Message.LogError($"'{args[0]}' does not exist.");
                Home.Start();
                return;
            }

            if (Folder.hasPath) {
                Message.LogError("WAIT!");
                Message.Log("It appears that a folder is already allocated. Only one directory can be under the control of CryptAway at any " +
                    "\none time. If you continue, the program will release the active folder in favor of this new path");
                Message.LogError("If this is what you want, say 'Do it'");

                string response = Console.ReadLine();
                if (string.IsNullOrEmpty(response) || !response.ToLower().Equals("do it")) {
                    Message.Log("Okay. Let's just forget about it.");
                    Home.Start();
                    return;
                } else {
                    if(Encryption.Encrypted()) {
                        Encryption.DecryptFiles(Folder.GetPath());
                    }

                    Folder.ShowFolder(Folder.GetPath());
                    Message.Log("CryptAway has decrypted the folder and made it visible again.");
                }
            }

            if (!Folder.SetPath(args[0]))
            {
                Message.LogError($"Could not establish path '{args[0]}'" +
                    $"\nCheck that the path specified is both possible and accessible.");
            }
            else {
                Message.Log("Done." +
                    $"\n'{args[0]}' is now assigned to CryptAway.");
            }

            Home.Start();
        }
    }

    internal class Open : Command
    {
        public override void Execute(string[] args)
        {
            if (!Folder.hasPath) {
                Message.LogError("No path has been assigned.");
                Home.Start();
                return;
            }

            if (!Directory.Exists(Folder.GetPath())) {
                Message.LogError("Could not find the allocated path.");
                Home.Start();
                return;
            }

            Folder.OpenFolder();
            Home.Start();
        }
    }

    internal class Hide : Command
    {
        public override void Execute(string[] args)
        {
            if (!Folder.hasPath) {
                Message.LogError("No folder has been assigned.");
                Home.Start();
                return;
            }

            Folder.HideFolder(Folder.GetPath());
            Home.Start();
        }
    }

    internal class Show : Command
    {
        public override void Execute(string[] args)
        {
            if (!Folder.hasPath)
            {
                Message.LogError("No folder has been assigned.");
                Home.Start();
                return;
            }

            Folder.ShowFolder(Folder.GetPath());
            Home.Start();
        }
    }

    internal class Encrypt : Command
    {
        public override void Execute(string[] args)
        {
            if (!Folder.hasPath)
            {
                Message.LogError("No folder has been assigned.");
                Home.Start();
                return;
            }

            if (Encryption.Encrypted()) {
                Message.LogError("Files are already encrypted.");
                Home.Start();
                return;
            }

            Encryption.EncryptFiles(Folder.GetPath());
            Message.Log("Done. Your files are now encrypted.");
            Home.Start();
        }
    }

    internal class Decrypt : Command
    {
        public override void Execute(string[] args)
        {
            if (!Folder.hasPath)
            {
                Message.LogError("No folder has been assigned.");
                Home.Start();
                return;
            }

            if (!Encryption.Encrypted())
            {
                Message.LogError("Files are not encrypted.");
                Home.Start();
                return;
            }

            Encryption.DecryptFiles(Folder.GetPath());
            Message.Log("Done. Your files are decrypted.");
            Home.Start();
        }
    }

    internal class Password : Command
    {
        public override void Execute(string[] args)
        {
            // Has password
            string response = string.Empty;
            if (Encryption.HasPassword())
            {
                Message.LogError("Password already set.");
                Message.Log($"Input current password to change it.");

                response = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(response) && Encryption.CheckPassword(response))
                {
                    Message.Log("New password:");
                    response = Console.ReadLine();
                    Message.Log("Confirm password:");
                    string previous = response;
                    response = Console.ReadLine();
                    if (response.Equals(previous))
                    {
                        Encryption.EncryptPassword(response);
                        Message.Log("Password changed.");
                    }
                    else {
                        Message.LogError("Passwords did not match.");
                    }
                    
                    Home.Start();
                    return;
                }
                else
                {
                    Message.LogError("Incorrect password.");
                    Home.Start();
                    return;
                }
            }

            // Set new password
            Message.LogError("No password has been set.");
            Message.Log("Enter password:");
            response = Console.ReadLine();
            string pass = response;
            Message.Log("Confirm password:");
            response = Console.ReadLine();
            if (response.Equals(pass))
            {
                Encryption.EncryptPassword(response);
                Message.Log("Password set.");
            }
            else
            {
                Message.LogError("Passwords did not match.");
                Execute(args);
                return;
            }

            Home.signedIn = true;
            Home.Start();
        }
    }

    internal class List : Command
    {
        public override void Execute(string[] args)
        {

            string[] filePaths = Directory.GetFiles(Folder.GetPath());

            foreach (string filePath in filePaths)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                Message.Log($" - {fileInfo.Name} | {fileInfo.Length}bytes | {fileInfo.LastWriteTime}");
            }

            Home.Start();
        }
    }

    internal class Reset : Command {
        public override void Execute(string[] args)
        {
            Message.LogError("WAIT!");
            Message.Log("Reset will set CryptAway back to its initial state. The folder and all its contents\n" +
                "will be reverted back.");
            Message.LogError("If this is what you want, say 'Do it'");

            string response = Console.ReadLine();
            if (string.IsNullOrEmpty(response) || !response.ToLower().Equals("do it"))
            {
                Message.Log("Okay. Let's just forget about it.");
                Home.Start();
                return;
            }
            else
            {
                if (Encryption.Encrypted()) {
                    Encryption.DecryptFiles(Folder.GetPath());
                }

                if (Folder.hasPath) {
                    Folder.ShowFolder(Folder.GetPath());
                }

                string[] files = new string[] {
                    "bool.dat", "password.dat", "iv.dat", "key.dat", "path.dat"
                };

                foreach (string file in files) {
                    // Construct the full path to the bool.dat file
                    string filePath = Path.Combine(Environment.CurrentDirectory, file);

                    // Check if the file exists
                    if (File.Exists(filePath))
                    {
                        // If the file exists, delete it
                        File.Delete(filePath);
                    }
                }

                Console.Clear();
                Message.WelcomeMessage();
                Home.Start();
            }
        }
    }

    internal class Help : Command
    {
        public override void Execute(string[] args)
        {
            Message.Log("-- Below is a list of commands you can use to interact with CryptAway --" +
                      "\n    help | List all commands that can be called in CryptAway." +
                      "\n   clear | Clears the console." +
                      "\n   check | Clarifies if a folder is allocated." +
                      "\n     set | Specify a path for CryptAway to work with." +
                      "\n    open | Opens allocated folder in a new explorer window." +
                      "\n    hide | Hides the folder from view in explorer." +
                      "\n    show | Reveals the folder to explorer." +
                      "\n encrypt | Encrypts the text of all files in the folder." +
                      "\n decrypt | Decrypts all text from the files in the folder." +
                      "\n    list | Show all files in the folder." +
                      "\npassword | Change the password." +
                      "\n     dev | Enable/Disable dev messages to the console." +
                      "\n    exit | Quit the application.");

            Home.Start();
        }
    }

    internal class Exit : Command {
        public override void Execute(string[] args)
        {
            Environment.Exit(0);
        }
    }
}
