using System.Globalization;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

public class CalculatorViewModel : ViewModelBase
{
    private string _display = "0";
    public string Display
    {
        get { return _display; }
        set
        {
            _display = FormatNumber(value);
            OnPropertyChanged(nameof(Display));
        }
    }

    private ObservableCollection<string> _operationHistory = new ObservableCollection<string>();
    public ObservableCollection<string> OperationHistory
    {
        get { return _operationHistory; }
        set
        {
            _operationHistory = value;
            OnPropertyChanged(nameof(OperationHistory));
        }
    }
    public ObservableCollection<string> FullHistory { get; } = new ObservableCollection<string>();

    private string _lastOperation;
    public string LastOperation
    {
        get { return _lastOperation; }
        set
        {
            _lastOperation = value;
            OnPropertyChanged(nameof(LastOperation));
        }
    }

    private string _currentInput = string.Empty;
    private string _currentOperator;
    private double _firstOperand;

    public ICommand NumberCommand => new RelayCommand<string>(AppendNumber);
    public ICommand OperatorCommand => new RelayCommand<string>(SetOperator);
    public ICommand EqualsCommand => new RelayCommand<object>(Calculate);
    public ICommand BackCommand => new RelayCommand<object>(Backspace);
    public ICommand SquareRootCommand => new RelayCommand<object>(SquareRoot);
    public ICommand ToggleSignCommand => new RelayCommand<object>(ToggleSign);
    public ICommand ClearCommand => new RelayCommand<object>(Clear);
    public ICommand ClearHistoryCommand => new RelayCommand<object>(param => ClearHistory());

    public void AddToHistory(string operation)
    {
        OperationHistory.Add(operation);
        FullHistory.Insert(0, operation);
        int index = operation.IndexOf('=');
        string last = operation.Substring(0, index + 1).Trim();
        LastOperation = last;
    }

    public void ClearHistory()
    {
        OperationHistory.Clear();
        FullHistory.Clear();
    }

    public void AppendNumber(string number)
    {
        // Se o visor estiver vazio e a entrada for uma vírgula ou ponto, ignore
        if (string.IsNullOrEmpty(_currentInput) && (number == "." || number == ","))
        {
            return;
        }

        // Se a entrada for uma vírgula ou ponto e o último caractere do visor for uma vírgula ou ponto, ignore
        if ((number == "." || number == ",") && (_currentInput.EndsWith(",") || _currentInput.EndsWith(".")))
        {
            return;
        }

        // Concatenação de números
        string newInput = _currentInput + number;

        // Verifica se excede o limite de 16 caracteres
        if (newInput.Length > 16)
        {
            // Exibe uma mensagem ou toma outra ação apropriada
            return;
        }

        // Adiciona o número ao _currentInput
        _currentInput = newInput;

        Display =_currentInput;
    }

    private string FormatNumber(string input)
    {
        if (double.TryParse(input, out double result))
        {
            // Use a formatação específica de cultura para exibir pontos de milhar e vírgula como separador decimal
            string formattedNumber = result.ToString("N", new CultureInfo("pt-BR"));

            // Remova os zeros à direita após o ponto decimal
            formattedNumber = formattedNumber.TrimEnd('0').TrimEnd(',');

            return formattedNumber;
        }

        return input;
    }

    public void SetOperator(string op)
    {
        if (!string.IsNullOrEmpty(_currentInput))
        {
            _firstOperand = Convert.ToDouble(_currentInput);
            _currentInput = string.Empty;
            _currentOperator = op;
        }
    }

    public void Calculate(object parameter)
    {
        if (!string.IsNullOrEmpty(_currentInput) && !string.IsNullOrEmpty(_currentOperator))
        {
            double secondOperand = Convert.ToDouble(_currentInput);
            double result = 0;
            string OperationText = "";

            switch (_currentOperator)
            {
                case "+":
                    result = _firstOperand + secondOperand;
                    OperationText = $"{_firstOperand} + {secondOperand} = {result}";
                    break;
                case "-":
                    result = _firstOperand - secondOperand;
                    OperationText = $"{_firstOperand} − {secondOperand} = {result}";
                    break;
                case "*":
                    result = _firstOperand * secondOperand;
                    OperationText = $"{_firstOperand} × {secondOperand} = {result}";
                    break;
                case "/":
                    result = _firstOperand / secondOperand;
                    OperationText = $"{_firstOperand} ÷ {secondOperand} = {result}";
                    break;
                case "^":
                    result = Math.Pow(_firstOperand, secondOperand);
                    OperationText = $"{_firstOperand} ^ {secondOperand} = {result}";
                    break;
            }

            Display = result.ToString();
            _currentInput = result.ToString();
            _currentOperator = null;

            AddToHistory(OperationText);
        }
    }

    public void Backspace(object parameter)
    {
        if (!string.IsNullOrEmpty(_currentInput))
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            Display = _currentInput;
        }
        else if (!string.IsNullOrEmpty(_currentOperator))
        {
            _currentOperator = null;
            Display = _firstOperand.ToString();
        }
    }

    private void Clear(object parameter)
    {
        _firstOperand = 0;
        _currentInput = string.Empty;
        _currentOperator = null;
        Display = "0";
        LastOperation = string.Empty;
    }

    private void SquareRoot(object parameter)
    {
        if (!string.IsNullOrEmpty(_currentInput))
        {
            double operand = Convert.ToDouble(_currentInput);
            if (operand >= 0)
            {
                double result = Math.Sqrt(operand);
                string OperationText = $"√({operand}) = {result}";

                Display = result.ToString();
                _currentInput = result.ToString();

                AddToHistory(OperationText);
            }
            else
            {
                // Trate o caso de tentar calcular raiz quadrada de número negativo
                Error();
            }
        }
        else if (!string.IsNullOrEmpty(_currentOperator) && _firstOperand >= 0)
        {
            double result = Math.Sqrt(_firstOperand);
            string OperationText = $"√({_firstOperand}) = {result}";

            Display = result.ToString();
            _currentInput = result.ToString();
            _currentOperator = null;

            AddToHistory(OperationText);
        }
        else
        {
            // Trate o caso de tentar calcular raiz quadrada de número negativo
            Error();
        }
    }
    private void ToggleSign(object parameter)
    {
        if (!string.IsNullOrEmpty(_currentInput))
        {
            double currentValue = Convert.ToDouble(_currentInput);
            double newValue = -currentValue;
            Display = newValue.ToString();
            _currentInput = newValue.ToString();
        }
        else if (!string.IsNullOrEmpty(_currentOperator) && _firstOperand != 0)
        {
            _firstOperand = -_firstOperand;
            Display = _firstOperand.ToString();
        }
    }

    private void Error()
    {
        Display = "Erro";
    }
}