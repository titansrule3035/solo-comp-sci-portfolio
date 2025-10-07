using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetTextbox("0");

            // Prevent resizing
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Enable keyboard input
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        // ============================
        // Button Clicks
        // ============================
        private void zero_button_Click(object sender, EventArgs e) { AppendOutput("0"); }
        private void one_button_Click(object sender, EventArgs e) { AppendOutput("1"); }
        private void two_button_Click(object sender, EventArgs e) { AppendOutput("2"); }
        private void three_button_Click(object sender, EventArgs e) { AppendOutput("3"); }
        private void four_button_Click(object sender, EventArgs e) { AppendOutput("4"); }
        private void five_button_Click(object sender, EventArgs e) { AppendOutput("5"); }
        private void six_button_Click(object sender, EventArgs e) { AppendOutput("6"); }
        private void seven_button_Click(object sender, EventArgs e) { AppendOutput("7"); }
        private void eight_button_Click(object sender, EventArgs e) { AppendOutput("8"); }
        private void nine_button_Click(object sender, EventArgs e) { AppendOutput("9"); }

        private void decimal_button_Click(object sender, EventArgs e)
        {
            string text = text_box.Text;
            int lastOperatorIndex = Math.Max(
                text.LastIndexOf('+'),
                Math.Max(
                    text.LastIndexOf('-'),
                    Math.Max(
                        text.LastIndexOf('*'),
                        Math.Max(
                            text.LastIndexOf('/'),
                            text.LastIndexOf('^')
                        )
                    )
                )
            );

            string lastNumber = (lastOperatorIndex >= 0) ? text.Substring(lastOperatorIndex + 1) : text;

            if (!lastNumber.Contains("."))
            {
                AppendOutput((lastNumber == "") ? "0." : ".");
            }
        }

        private void add_button_Click(object sender, EventArgs e) { AppendOperator("+"); }
        private void subtract_button_Click(object sender, EventArgs e) { AppendOperator("-"); }
        private void multiply_button_Click(object sender, EventArgs e) { AppendOperator("*"); }
        private void divide_button_Click(object sender, EventArgs e) { AppendOperator("/"); }
        private void exponent_button_Click(object sender, EventArgs e) { AppendOperator("^"); }
        private void open_parentheses_button_Click(object sender, EventArgs e) { AppendOutput("("); }
        private void close_parentheses_button_Click(object sender, EventArgs e) { AppendOutput(")"); }
        private void clear_button_Click(object sender, EventArgs e) { SetTextbox("0"); }
        private void delete_button_Click(object sender, EventArgs e)
        {
            if (text_box.Text.Length > 1)
            {
                if (!CheckForError()) { SetTextbox(text_box.Text.Substring(0, text_box.Text.Length - 1)); }
                else { SetTextbox("0"); }
            }
            else { SetTextbox("0"); }
        }
        private void equals_button_Click(object sender, EventArgs e)
        {
            try
            {
                string expression = text_box.Text;
                if (IsLastCharOperator(expression))
                {
                    SetTextbox("ERROR");
                    return;
                }

                expression = InsertImpliedMultiplication(expression);
                double result = EvaluateExpression(expression);
                SetTextbox(result.ToString());
            }
            catch
            {
                SetTextbox("ERROR");
            }
        }

        // ============================
        // Helper Functions
        // ============================
        private void AppendOutput(string newOutput)
        {
            if (text_box.Text == "ERROR" || text_box.Text == "0")
            {
                text_box.Text = newOutput;
            }
            else
            {
                text_box.Text += newOutput;
            }
        }

        private void SetTextbox(string text) { text_box.Text = text; }

        private bool IsLastCharOperator(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            char last = text[text.Length - 1];
            return "+-*/^".Contains(last);
        }

        private bool CheckForError() { return text_box.Text == "ERROR"; }

        private void AppendOperator(string op)
        {
            string text = text_box.Text;

            // Allow minus after operator for negative numbers
            if (op == "-" && IsLastCharOperator(text))
            {
                AppendOutput(op);
                return;
            }

            if (!IsLastCharOperator(text))
            {
                AppendOutput(op);
            }
        }

        // ============================
        // Keyboard Input
        // ============================
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Numbers
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) { AppendOutput((e.KeyCode - Keys.D0).ToString()); }
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) { AppendOutput((e.KeyCode - Keys.NumPad0).ToString()); }

            // Operators
            else if (e.KeyCode == Keys.Add || (e.KeyCode == Keys.Oemplus && e.Shift)) { AppendOperator("+"); }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) { AppendOperator("-"); }
            else if (e.KeyCode == Keys.Multiply || (e.KeyCode == Keys.D8 && e.Shift)) { AppendOperator("*"); }
            else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion) { AppendOperator("/"); }
            else if ((e.KeyCode == Keys.D6 && e.Shift) || e.KeyCode == Keys.Oem6) { AppendOperator("^"); }

            // Decimal
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod) { decimal_button_Click(sender, e); }

            // Parentheses
            else if (e.KeyCode == Keys.D9 && e.Shift || e.KeyCode == Keys.OemOpenBrackets) { open_parentheses_button_Click(sender, e); }
            else if (e.KeyCode == Keys.D0 && e.Shift || e.KeyCode == Keys.OemCloseBrackets) { close_parentheses_button_Click(sender, e); }

            // Equals
            else if (e.KeyCode == Keys.Enter || (e.KeyCode == Keys.Oemplus && !e.Shift)) { equals_button_Click(sender, e); }

            // Delete / Backspace
            else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete) { delete_button_Click(sender, e); }

            // Clear
            else if (e.KeyCode == Keys.C || e.KeyCode == Keys.Escape) { clear_button_Click(sender, e); }

            e.Handled = true;
        }

        // ============================
        // Algebraic Multiplication
        // ============================
        private string InsertImpliedMultiplication(string expr)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < expr.Length; i++)
            {
                char current = expr[i];

                if (i > 0)
                {
                    char prev = expr[i - 1];

                    if ((char.IsDigit(prev) || prev == ')') && current == '(') { sb.Append('*'); }
                    if (prev == ')' && (char.IsDigit(current) || current == '(')) { sb.Append('*'); }
                }

                sb.Append(current);
            }

            return sb.ToString();
        }

        // ============================
        // Expression Evaluation
        // ============================
        private double EvaluateExpression(string expr)
        {
            List<string> tokens = Tokenize(expr);
            List<string> rpn = ToRPN(tokens);
            return EvaluateRPN(rpn);
        }

        private List<string> Tokenize(string expr)
        {
            List<string> tokens = new List<string>();
            StringBuilder number = new StringBuilder();

            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];

                if (char.IsDigit(c) || c == '.')
                {
                    number.Append(c);
                }
                else
                {
                    if (number.Length > 0) { tokens.Add(number.ToString()); number.Clear(); }

                    if (c == '-' && (i == 0 || "+-*/^(".Contains(expr[i - 1])))
                    {
                        number.Append(c); // negative number
                    }
                    else { tokens.Add(c.ToString()); }
                }
            }

            if (number.Length > 0) tokens.Add(number.ToString());
            return tokens;
        }

        private List<string> ToRPN(List<string> tokens)
        {
            List<string> output = new List<string>();
            Stack<string> stack = new Stack<string>();
            Dictionary<string, int> precedence = new Dictionary<string, int>()
            {
                {"^", 4}, {"*", 3}, {"/", 3}, {"+", 2}, {"-", 2}, {"(", 0}
            };

            foreach (string token in tokens)
            {
                double n;
                if (double.TryParse(token, out n)) { output.Add(token); }
                else if (token == "(") { stack.Push(token); }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(") output.Add(stack.Pop());
                    stack.Pop();
                }
                else
                {
                    while (stack.Count > 0 && precedence[stack.Peek()] >= precedence[token]) output.Add(stack.Pop());
                    stack.Push(token);
                }
            }

            while (stack.Count > 0) output.Add(stack.Pop());
            return output;
        }

        private double EvaluateRPN(List<string> rpn)
        {
            Stack<double> stack = new Stack<double>();

            foreach (string token in rpn)
            {
                double n;
                if (double.TryParse(token, out n)) { stack.Push(n); }
                else
                {
                    double b = stack.Pop();
                    double a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/": stack.Push(a / b); break;
                        case "^": stack.Push(Math.Pow(a, b)); break;
                    }
                }
            }

            return stack.Pop();
        }
        void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
