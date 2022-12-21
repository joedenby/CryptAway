using System.Security.Cryptography;
using System.Text;

namespace CryptAway
{
    internal class Encryption
    {
        public static void EncryptFiles(string folderPath)
        {
            Message.LogDev("EncryptFiles", "Attempting to encrypt...");
            if (!Folder.hasPath) {
                Message.LogDev("EncryptFiles","No path assigned.");
                return;
            }

            // Get key and IV for the encryption algorithm
            byte[] key = GetKey();
            byte[] iv = GetIV();

            Message.LogDev("EncryptFiles", $"Key {key.Length} | iv {iv.Length}");

            // Iterate through the files in the folder
            foreach (string filePath in Directory.EnumerateFiles(folderPath))
            {
                Message.LogDev("EncryptFiles", $"Encrypt file: {filePath}");
                // Read the contents of the file
                byte[] data = File.ReadAllBytes(filePath);

                // Encrypt the data using the Aes class
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

                        // Write the encrypted data back to the file
                        File.WriteAllBytes(filePath, encryptedData);
                    }
                }
            }

            Message.LogDev("EncryptFiles", "Finished.");
            SetEncrypted(true);
        }

        public static void DecryptFiles(string folderPath)
        {
            Message.LogDev("DecryptFiles", $"Attempting to decrypt...");
            if (!Folder.hasPath)
            {
                Message.LogDev("DecryptFiles", $"No path assigned.");
                return;
            }


            // Get key and IV for the encryption algorithm
            byte[] key = GetKey();
            byte[] iv = GetIV();

            Message.LogDev("DecryptFiles", $"Key {key.Length} | iv {iv.Length}");

            // Iterate through the files in the folder
            foreach (string filePath in Directory.EnumerateFiles(folderPath))
            {
                Message.LogDev("DecryptFiles", $"Decrypt file: {filePath}");
                // Read the contents of the file
                byte[] data = File.ReadAllBytes(filePath);

                // Decrypt the data using the Aes class
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedData = decryptor.TransformFinalBlock(data, 0, data.Length);

                        // Write the decrypted data back to the file
                        File.WriteAllBytes(filePath, decryptedData);
                    }
                }
            }

            Message.LogDev("DecryptFiles", "Finished.");
            SetEncrypted(false);
        }

        public static void EncryptPassword(string password)
        {
            Message.LogDev("EncryptPassword", $"Encrypting password [{password}]");
            // Use SHA256 hashing algorithm to encrypt the password
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hashedPassword = Convert.ToBase64String(passwordHash);
                Message.LogDev("EncryptPassword", $"Hash [{hashedPassword}]");

                // Save the hashed password to a file in the application folder
                string filePath = Path.Combine(Environment.CurrentDirectory, "password.dat");
                File.WriteAllText(filePath, hashedPassword);
            }

            Message.LogDev("EncryptPassword", "Finished.");
        }

        public static bool CheckPassword(string inputPassword)
        {
            Message.LogDev("CheckPassword", "Attempting to check password ...");

            // Read the hashed password from the file in the application folder
            string filePath = Path.Combine(Environment.CurrentDirectory, "password.dat");
            string encryptedPassword = File.ReadAllText(filePath);

            Message.LogDev("CheckPassword", $"Found hash [{encryptedPassword}]");

            // Encrypt the input password and compare it to the encrypted password
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputPasswordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                string hashedInputPassword = Convert.ToBase64String(inputPasswordHash);
                Message.LogDev("CheckPassword", $"Input hash [{encryptedPassword}] | " +
                    $"Match = {hashedInputPassword.Equals(encryptedPassword)}");
                return hashedInputPassword.Equals(encryptedPassword);
            }
        }

        public static bool HasPassword() => File.Exists("password.dat");

        public static bool Encrypted() {
            Message.LogDev("Encrypted", "Checking for encryption flag...");
            if (!File.Exists("bool.dat")) {
                Message.LogDev("Encrypted", "bool.dat not found.");
                return false;
            }

            Message.LogDev("Encrypted", "bool.dat found.");
            using (FileStream stream = new FileStream("bool.dat", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                bool x = reader.ReadBoolean();
                Message.LogDev("Encrypted", $"bool.dat = {x}");
                return x;
            }

            Message.LogDev("Encrypted", "Finished.");
        }

        public static void SetEncrypted(bool active) {
            Message.LogDev("SetEncrypted", $"Encrypted flag = {active}");
            using (FileStream stream = new FileStream("bool.dat", FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write(active);
            }
        }

        static byte[] GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                byte[] key = aes.Key;
                Message.LogDev("GenerateKey", $"Generated new key [{key}]");
                File.WriteAllBytes("key.dat", key);
                return key;
            }
        }

        static byte[] GenerateIV()
        {
            using (Aes aes = Aes.Create())
            {
                byte[] iv = aes.IV;
                Message.LogDev("GenerateIV", $"Generated new key [{iv}]");
                File.WriteAllBytes("iv.dat", iv);
                return iv;
            }
        }

        static byte[] GetKey() {
            return File.Exists("key.dat") ? File.ReadAllBytes("key.dat") : GenerateKey();
        }

        static byte[] GetIV() {
            return File.Exists("iv.dat") ? File.ReadAllBytes("iv.dat") : GenerateIV();
        }
    }
}
