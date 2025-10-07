using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CSharpToDoList
{
    internal class Program
    {
        enum Screen
        {
            Title,
            Add,
            Mark,
            Delete,
            Clear,
            End
        };

        static void Main(string[] args)
        {
            string username = Environment.UserName;
            string fileName = "myTasks.txt";
            string saltFile = "salt.bin";
            string input = "";
            Screen activeScreen = Screen.Title;
            byte[] key;
            byte[] salt = new byte[16];
            List<Task> tasks;

            // =========================
            // 1️⃣ Initialize or load salt
            // =========================
            if (!File.Exists(saltFile))
            {
                RandomNumberGenerator.Fill(salt);
                File.WriteAllBytes(saltFile, salt);
            }
            else
            {
                salt = File.ReadAllBytes(saltFile);
            }

            // =========================
            // 2️⃣ Derive AES key from username + salt
            // =========================
            using (var keyGen = new Rfc2898DeriveBytes(username, salt, 50000))
            {
                key = keyGen.GetBytes(32); // 256-bit AES key
            }

            // =========================
            // 3️⃣ Load tasks from file
            // =========================
            tasks = LoadTasksFromFile();

            Console.WriteLine("Welcome to the To-Do List!");

            List<Screen> titleOptions = new List<Screen>
            {
                Screen.Add,
                Screen.Mark,
                Screen.Delete,
                Screen.Clear,
                Screen.End
            };

            while (activeScreen != Screen.End)
            {
                while (activeScreen == Screen.Title)
                {
                    Console.WriteLine("======= YOUR TASKS =======");
                    if (tasks.Count == 0)
                    {
                        Console.WriteLine("No tasks yet!");
                    }
                    else
                    {
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            string filler = tasks[i].completed ? "X" : " ";
                            Console.WriteLine($"{i + 1}. [{filler}] {tasks[i].description}");
                        }
                    }
                    Console.WriteLine("==========================");
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("==========================");
                    Console.WriteLine("1. Add Task");
                    Console.WriteLine("2. Mark Task Completed");
                    Console.WriteLine("3. Delete Task");
                    Console.WriteLine("4. Clear All Tasks");
                    Console.WriteLine("5. Save & Exit");
                    Console.WriteLine("==========================");
                    PromptUser(ref input);
                    Console.Clear();
                    activeScreen = HandleMenuInput(input, titleOptions, activeScreen);
                }

                while (activeScreen == Screen.Add)
                {
                    Console.WriteLine("Enter the new task: ");
                    PromptUser(ref input);
                    Console.Clear();
                    tasks.Add(new Task(input, false));
                    Console.WriteLine("Task added!");
                    activeScreen = Screen.Title;
                }

                while (activeScreen == Screen.Mark)
                {
                    if (tasks.Count == 0)
                    {
                        Console.WriteLine("No tasks to be marked complete.");
                        activeScreen = Screen.Title;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Enter the task number you wish to mark as completed.");
                        Console.WriteLine("======= YOUR TASKS =======");
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            string filler = tasks[i].completed ? "X" : " ";
                            Console.WriteLine($"{i + 1}. [{filler}] {tasks[i].description}");
                        }
                        Console.WriteLine("==========================");
                        PromptUser(ref input);
                        activeScreen = MarkComplete(input, tasks);
                    }
                }

                while (activeScreen == Screen.Delete)
                {
                    if (tasks.Count == 0)
                    {
                        Console.WriteLine("No tasks to be deleted.");
                        activeScreen = Screen.Title;
                    }
                    else
                    {
                        Console.WriteLine("Enter the task number you wish to delete.");
                        Console.WriteLine("======= YOUR TASKS =======");
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            string filler = tasks[i].completed ? "X" : " ";
                            Console.WriteLine($"{i + 1}. [{filler}] {tasks[i].description}");
                        }
                        Console.WriteLine("==========================");
                        PromptUser(ref input);
                        activeScreen = DeleteTask(input, tasks);
                    }
                }

                while (activeScreen == Screen.Clear)
                {
                    Console.WriteLine("Are you sure you want to clear all tasks? (y/n):");
                    PromptUser(ref input);
                    if (input == "y")
                    {
                        tasks.Clear();
                        Console.Clear();
                        Console.WriteLine("Tasks cleared!");
                    }
                    else if (input == "n")
                    {
                        Console.Clear();
                        Console.WriteLine("Clear cancelled.");
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid option.");
                        activeScreen = Screen.Clear;
                        Console.WriteLine("==========================");
                        continue;
                    }
                    activeScreen = Screen.Title;
                }
            }

            // =========================
            // 4️⃣ Save tasks before exit
            // =========================
            SaveTasksToFile(tasks);
            Console.WriteLine("Thank you for using the To-Do List. Goodbye!");

            // ====================
            // LOCAL FUNCTIONS
            // ====================

            List<Task> LoadTasksFromFile()
            {
                if (!File.Exists(fileName))
                {
                    return new List<Task>();
                }

                string[] readFile = File.ReadAllLines(fileName);
                List<Task> loaded = new List<Task>();

                foreach (var line in readFile)
                {
                    try
                    {
                        // Convert from Base64 and decrypt
                        byte[] encryptedBytes = Convert.FromBase64String(line);
                        string decrypted = Decrypt(encryptedBytes, key);
                        if (string.IsNullOrWhiteSpace(decrypted) || !decrypted.Contains("|")) continue;

                        string description = decrypted.Substring(0, decrypted.IndexOf('|'));
                        string completedStr = decrypted.Substring(decrypted.IndexOf('|') + 1);
                        bool completed = (completedStr == "1");
                        loaded.Add(new Task(description, completed));
                    }
                    catch
                    {
                        // if decryption fails, treat as plain text
                        string decoded = line;
                        if (string.IsNullOrWhiteSpace(decoded) || !decoded.Contains("|")) continue;

                        string description = decoded.Substring(0, decoded.IndexOf('|'));
                        string completedStr = decoded.Substring(decoded.IndexOf('|') + 1);
                        bool completed = (completedStr == "1");
                        loaded.Add(new Task(description, completed));
                    }
                }

                return loaded;
            }

            void SaveTasksToFile(List<Task> tasks)
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var task in tasks)
                    {
                        // Encrypt each task individually with a new IV per task
                        byte[] encryptedBytes = Encrypt(task.GetTaskString(), key);
                        string base64 = Convert.ToBase64String(encryptedBytes);
                        writer.WriteLine(base64);
                    }
                }
            }

            Screen HandleMenuInput(string input, List<Screen> options, Screen currentScreen)
            {
                try
                {
                    int choice = Convert.ToInt32(input);
                    if (choice >= 1 && choice <= options.Count)
                    {
                        return options[choice - 1];
                    }
                }
                catch { }
                Console.WriteLine("Please enter a valid option.");
                return currentScreen;
            }

            Screen MarkComplete(string input, List<Task> tasks)
            {
                if (tasks.Count == 0)
                {
                    Console.WriteLine("No tasks to mark as complete.");
                    return Screen.Title;
                }

                if (tasks.All(t => t.completed))
                {
                    Console.WriteLine("No incomplete tasks to mark.");
                    return Screen.Title;
                }

                try
                {
                    int choice = Convert.ToInt32(input);
                    if (choice >= 1 && choice <= tasks.Count)
                    {
                        if (tasks[choice - 1].completed)
                        {
                            Console.WriteLine("That task is already marked complete.");
                        }
                        else
                        {
                            tasks[choice - 1].MarkComplete();
                            Console.Clear();
                            Console.WriteLine($"Task {choice} marked as complete!");
                        }
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid task number.");
                    }
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Invalid task number.");
                }

                return Screen.Title;
            }

            Screen DeleteTask(string input, List<Task> tasks)
            {
                if (tasks.Count == 0)
                {
                    Console.WriteLine("No tasks to delete.");
                    return Screen.Title;
                }

                try
                {
                    int choice = Convert.ToInt32(input);
                    if (choice >= 1 && choice <= tasks.Count)
                    {
                        tasks.RemoveAt(choice - 1);
                        Console.Clear();
                        Console.WriteLine($"Task {choice} deleted.");
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid task number.");
                    }
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Invalid task number.");
                }

                return Screen.Title;
            }

            void PromptUser(ref string input)
            {
                Console.Write($"<{username}>: ");
                input = Console.ReadLine();
            }

            // ====================
            // AES ENCRYPTION / DECRYPTION
            // ====================
            byte[] Encrypt(string plainText, byte[] key)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV(); // new IV for each encryption

                    using (var ms = new MemoryStream())
                    {
                        // Prepend IV to ciphertext
                        ms.Write(aes.IV, 0, aes.IV.Length);

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        return ms.ToArray();
                    }
                }
            }

            string Decrypt(byte[] cipherData, byte[] key)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;

                    // Extract IV from the beginning of cipherData
                    byte[] iv = new byte[aes.BlockSize / 8];
                    Array.Copy(cipherData, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var ms = new MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length))
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        class Task
        {
            public string description;
            public bool completed;

            public Task(string _description, bool _completed)
            {
                description = _description;
                completed = _completed;
            }

            public void MarkComplete() => completed = true;

            public int GetCompletedAsInt() => (completed ? 1 : 0);

            public string GetTaskString() => description + "|" + GetCompletedAsInt();
        };
    }
}
