# CSharpToDoList

A simple console-based To-Do List application written in C#.
Tasks are stored in an **encrypted format** using AES, making the saved file unreadable in plain text.

## Features

* Add new tasks
* View all tasks
* Mark tasks as completed
* Remove tasks
* Tasks are **saved to disk in an encrypted format** with per-task IVs

## How It Works

Each task is stored as:

```
<description>|<completed_flag>
```

* description -> the task text
* completed\_flag -> 1 if completed, 0 if not

Before saving, each task string is encrypted using AES-256:

1. The program derives a 256-bit AES key from the username + a persistent salt using PBKDF2 (Rfc2898DeriveBytes) with 50,000 iterations.
2. Each task gets its own initialization vector (IV) which is stored alongside the ciphertext.
3. The encrypted bytes are converted to Base64 for safe storage in myTasks.txt.

When loading, the program reads the Base64, retrieves the IV for each task, and decrypts it using the derived key.

### Example

A task like:

```
Buy groceries|0
```

is stored as an encrypted Base64 string in myTasks.txt. The program automatically decrypts it back to Buy groceries|0 when reading the file.

## Getting Started

### Prerequisites

* .NET SDK installed

### Running the Application

Simply clone the repository, navigate to the debug release, and run the CSharpToDoList.exe file.

## Usage

Follow the on-screen menu to add, view, complete, or remove tasks. Use the number keys to choose an option and enter/return to confirm.

### Example Menu

```
1. View Tasks
2. Add Task
3. Mark Task Completed
4. Delete Task
5. Clear All Tasks
6. Save & Exit
```

## License

This project is licensed under the MIT License in its parent repository.
