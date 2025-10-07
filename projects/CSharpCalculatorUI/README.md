# CSharpCalculatorUI

A simple **WinForms-based calculator** written in C# that supports basic arithmetic operations, **floating-point numbers**, and a **custom algebraic expression parser**.

## Features

* Add, subtract, multiply, and divide numbers
* Handle **floating-point numbers** with decimals
* Evaluate full expressions like `12.5 + 3.2 * 4`
* **Custom algebraic parser** that respects operator precedence, parentheses, and negative numbers
* Prevent invalid operations like division by zero or trailing operators
* Clear output and start a new calculation easily
* Decimal support ensuring only one decimal per number
* Fixed-size window to prevent resizing
* Intuitive button layout for easy use

## How It Works

1. The calculator allows the user to input numbers and operators using buttons on the form.
2. Expressions are constructed in the **text box** as the user clicks buttons.
3. When the **equals** button is pressed:

   * The expression is validated to ensure it does not end with an operator.
   * The **custom parser** tokenizes the expression, converts it to Reverse Polish Notation (RPN), and evaluates it using a stack-based algorithm.
   * The result is returned as a `double` and displayed in the textbox.
4. The **decimal button** ensures that each number can only have one decimal point.
5. Implied multiplication is automatically handled (e.g., `2(3+4)` becomes `2*(3+4)`).

### Example

A user can input:

```
12.5 + 3 * 2.1
```

The calculator will correctly evaluate and display:

```
18.8
```

### Example with Parentheses and Exponent

A user can input a complex expression like:

```
(5 + 3) * 2 - 4 / 2 ^ 2
```

The custom parser evaluates this correctly as `15.0`, respecting parentheses, operator precedence, and exponents.

## Getting Started

### Prerequisites

* [Visual Studio](https://visualstudio.microsoft.com/) or any C# IDE that supports WinForms
* [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) installed

### Running the Application

1. Open the solution in Visual Studio.
2. Build the project (`Ctrl+Shift+B`).
3. Run the application (`F5`) and the calculator form will appear.

## Usage

* Click number buttons (`0-9`) to input numbers.
* Click operator buttons (`+ - * / ^`) to perform operations.
* Use the decimal point button `.` to enter floating-point numbers.
* Use parentheses `(` and `)` for grouping.
* Press `=` to evaluate the expression using the custom parser.
* Press `C` to clear the current calculation.
* Delete or Backspace removes the last character.

## License

This project is licensed under the MIT License in its parent directory.
