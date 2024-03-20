using System.Diagnostics;
using System.IO;
#if NETCOREAPP
using Telerik.Zip;
#else
using Telerik.Windows.Zip;
#endif

namespace CreateZipArchive_NetStandard
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string zipFileName = "MyFiles.zip";

            using (Stream stream = File.Open(zipFileName, FileMode.Create))
            {

                //By default the EncryptionStrenght is 256 bits but it can be explicitly specified (EncryptionStrength.Aes128, EncryptionStrength.Aes192, and EncryptionStrength.Aes256) by passing it to the constructor 
                PasswordEncryptionSettings aesEncryptionSettings = EncryptionSettings.CreateAesPasswordEncryptionSettings(); 
 
                //You can also use the PKWARE encryption algorithm instead of the AES one 
                PasswordEncryptionSettings pkwareEncryptionSettings = EncryptionSettings.CreatePkzipPasswordEncryptionSettings(); 
 
                aesEncryptionSettings.Password = "password";  
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

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = zipFileName,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
