using System.Text;
using Telerik.Zip;

namespace EncryptAndDecryptZipArhive_NetStandard
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string zipFileName = "MyFiles.zip";
            string password = "password";
            string pathToExtract = "ExtractedFiles";

            CreateAndEncryptZipArchive(zipFileName, password);
            DecryptAndExtractZipArchive(zipFileName, password, pathToExtract);
        }

        private static void CreateAndEncryptZipArchive(string zipFileName, string password)
        {
            using (Stream stream = File.Open(zipFileName, FileMode.Create))
            {

                //By default the EncryptionStrenght is 256 bits but it can be explicitly specified (EncryptionStrength.Aes128, EncryptionStrength.Aes192, and EncryptionStrength.Aes256) by passing it to the constructor 
                PasswordEncryptionSettings aesEncryptionSettings = EncryptionSettings.CreateAesPasswordEncryptionSettings();

                //You can also use the PKWARE encryption algorithm instead of the AES one 
                PasswordEncryptionSettings pkwareEncryptionSettings = EncryptionSettings.CreatePkzipPasswordEncryptionSettings();

                aesEncryptionSettings.Password = password;
                CompressionSettings compressionSettings = null;
                Encoding encoding = null;

                using (ZipArchive archive = ZipArchive.Create(stream, encoding, compressionSettings, aesEncryptionSettings))
                {
                    using (ZipArchiveEntry entry = archive.CreateEntry("text.txt"))
                    {
                        StreamWriter writer = new StreamWriter(entry.Open());
                        writer.WriteLine("Progress!");
                        writer.Flush();
                    }
                }
            }
        }
        private static void DecryptAndExtractZipArchive(string zipFileName, string password, string pathToExtract)
        {
            if (!Directory.Exists(pathToExtract))
            {
                Directory.CreateDirectory(pathToExtract);
            }

            using (FileStream stream = File.Open(zipFileName, FileMode.Open))
            {
                DecryptionSettings decryptionSettings = EncryptionSettings.CreateDecryptionSettings();
                decryptionSettings.PasswordRequired += (s, a) => a.Password = password;
                CompressionSettings compressionSettings = null;
                Encoding encoding = null;
                using (ZipArchive zipArchive = ZipArchive.Read(stream, encoding, compressionSettings, decryptionSettings))
                {
                    foreach (var zipArchiveEntry in zipArchive.Entries)
                    {
                        using (Stream fileStream = File.Open($"{pathToExtract}\\{zipArchiveEntry.FullName}", FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (Stream entryStream = zipArchiveEntry.Open())
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
        }
    }
}
