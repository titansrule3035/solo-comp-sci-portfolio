using System;
using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StringToBin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Prompt the user for input
            Console.WriteLine("Input some text to write to the binary file.");
            Console.Write("<User>: ");
            string text = Console.ReadLine();

            // Ensure the input is not empty or whitespace
            while (String.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Input cannot be empty. Please input some text to write to the binary file.");
                Console.Write("<User>: ");
                text = Console.ReadLine();
            }

            // Write the user input to a binary file
            WriteFile(text);

            Console.WriteLine("Text written to binary file.\n\nBinary string (pulled directly from the file):");

            // Read the binary file and display its contents as binary and as text
            ReadFile();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        // Writes the provided text to a binary file using Latin1 encoding
        static void WriteFile(string text)
        {
            var dir = Directory.GetCurrentDirectory();
            var file = Path.Combine(dir, "binaryFile.bin");

            try
            {
                // "using" ensures the FileStream is properly disposed of, even if an exception occurs
                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    if (fs.CanWrite)
                    {
                        // Convert the text to bytes using Latin1 encoding
                        byte[] buffer = Encoding.Latin1.GetBytes(text);
                        // Write the bytes to the file
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                // Output any errors encountered during writing
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // Reads the binary file and displays its contents as a binary string and as text
        static void ReadFile()
        {
            var dir = Directory.GetCurrentDirectory();
            var file = Path.Combine(dir, "binaryFile.bin");
            string text = String.Empty;

            // Ensure the file exists before reading
            if (File.Exists(file) == false)
            {
                File.Create(file).Close();
            }

            try
            {
                // "using" ensures FileStream is properly closed/disposed
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    List<byte> allBytes = new List<byte>();
                    byte[] buffer = new byte[256]; // smaller buffer for chunked reading
                    int bytesRead;

                    // Keep reading until the end of file
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        allBytes.AddRange(buffer.Take(bytesRead));
                    }

                    // Output each byte as an 8-bit binary string, separated by spaces
                    foreach (byte b in allBytes)
                    {
                        Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
                    }
                    Console.WriteLine();

                    // Convert the bytes back to text using the SAME encoding as WriteFile (Latin1)
                    text = Encoding.Latin1.GetString(allBytes.ToArray());

                    // Output the text representation of the binary data
                    Console.WriteLine("Binary string as text: " + text);
                }
            }
            catch (Exception ex)
            {
                // Output any errors encountered during reading
                Console.WriteLine(ex.Message);
                Console.WriteLine("Binary string as text: " + text);
            }
        }
    }
}
