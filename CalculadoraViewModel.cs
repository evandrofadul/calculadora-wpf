using System.Globalization;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;

public class CalculadoraViewModel : ViewModelBase
{
    private string _display = "0";
    private string _entradaAtual = string.Empty;
    private string _operadorAtual;
    private double _primeiroNumero;
    private string _ultimaOperacao;
    private bool _resultadoCalculado;

    public string Display
    {
        get { return _display; }
        set
        {
            _display = FormatarNumero(value);
            OnPropertyChanged(nameof(Display));
        }
    }

    public string UltimaOperacao
    {
        get { return _ultimaOperacao; }
        set
        {
            _ultimaOperacao = value;
            OnPropertyChanged(nameof(UltimaOperacao));
        }
    }

    private ObservableCollection<string> _historicoOperacao = new ObservableCollection<string>();
    public ObservableCollection<string> HistoricoOperacao
    {
        get { return _historicoOperacao; }
        set
        {
            _historicoOperacao = value;
            OnPropertyChanged(nameof(HistoricoOperacao));
        }
    }
    public ObservableCollection<string> HistoricoCompleto { get; } = new ObservableCollection<string>();

    // Implementação de comandos seguindo o padrão MVVM, cada comando está associado a uma função
    // específica que será executada quando o comando for acionado pela interface do usuário.
    public ICommand NumeroCommand => new RelayCommand<string>(EntradaNumero);
    public ICommand OperadorCommand => new RelayCommand<string>(SetOperador);
    public ICommand IgualCommand => new RelayCommand<object>(Calcular);
    public ICommand BackCommand => new RelayCommand<object>(Backspace);
    public ICommand RaizQuadradaCommand => new RelayCommand<object>(RaizQuadrada);
    public ICommand TrocarSinalCommand => new RelayCommand<object>(TrocarSinal);
    public ICommand ClearCommand => new RelayCommand<object>(Clear);
    public ICommand LimparHistoricoCommand => new RelayCommand<object>(param => LimparHistorico());
    public ICommand InversoDeXCommand => new RelayCommand<object>(InversoDeX);

    private string FormatarNumero(string entrada)
    {
        if (double.TryParse(entrada, out double resultado))
        {
            // formatação para exibir pontos de milhar e vírgula
            string numeroFormatado = resultado.ToString("N", new CultureInfo("pt-BR"));

            // Remova os zeros à direita após o ponto decimal
            numeroFormatado = numeroFormatado.TrimEnd('0').TrimEnd(',');

            return numeroFormatado;
        }

        return entrada;
    }

    public void EntradaNumero(string numero)
    {
        // Se o visor estiver vazio e a entrada for uma vírgula ou ponto, ignore
        if (string.IsNullOrEmpty(_entradaAtual) && (numero == "." || numero == ","))
        {
            return;
        }

        // Se a entrada for uma vírgula ou ponto e o último caractere do visor for uma vírgula ou ponto, ignore
        if ((numero == "." || numero == ",") && (_entradaAtual.EndsWith(",") || _entradaAtual.EndsWith(".")))
        {
            return;
        }

        if (_resultadoCalculado)
        {
            _entradaAtual = string.Empty;
            _resultadoCalculado = false;
            UltimaOperacao = string.Empty;
        }

        string novaEntrada = _entradaAtual + numero;

        if (novaEntrada.Length > 16)
        {
            return;
        }

        _entradaAtual = novaEntrada;

        Display = _entradaAtual;
    }

    public void SetOperador(string op)
    {
        if (!string.IsNullOrEmpty(_entradaAtual))
        {
            _primeiroNumero = Convert.ToDouble(_entradaAtual);
            _entradaAtual = string.Empty;
            _operadorAtual = op;
        }
    }

    public void Backspace(object parameter)
    {
        if (!string.IsNullOrEmpty(_entradaAtual))
        {
            _entradaAtual = _entradaAtual.Substring(0, _entradaAtual.Length - 1);
            Display = _entradaAtual;
        }
        else if (!string.IsNullOrEmpty(_operadorAtual))
        {
            _operadorAtual = null;
            Display = _primeiroNumero.ToString();
        }
    }

    private void Clear(object parameter)
    {
        _primeiroNumero = 0;
        _entradaAtual = string.Empty;
        _operadorAtual = null;
        UltimaOperacao = string.Empty;
        Display = "0";
    }

    public void AddHistorico(string operation)
    {
        HistoricoOperacao.Add(operation);
        HistoricoCompleto.Insert(0, operation);
        int index = operation.IndexOf('=');
        string last = operation.Substring(0, index + 1).Trim();
        UltimaOperacao = last;
    }

    public void LimparHistorico()
    {
        HistoricoOperacao.Clear();
        HistoricoCompleto.Clear();
    }

    public void Calcular(object parameter)
    {
        if (!string.IsNullOrEmpty(_entradaAtual) && !string.IsNullOrEmpty(_operadorAtual))
        {
            double segundoNumero = Convert.ToDouble(_entradaAtual);
            double resultado = 0;
            string TextoDaOperacao = "";

            switch (_operadorAtual)
            {
                case "+":
                    resultado = _primeiroNumero + segundoNumero;
                    TextoDaOperacao = $"{_primeiroNumero} + {segundoNumero} = {resultado}";
                    break;
                case "-":
                    resultado = _primeiroNumero - segundoNumero;
                    TextoDaOperacao = $"{_primeiroNumero} − {segundoNumero} = {resultado}";
                    break;
                case "*":
                    resultado = _primeiroNumero * segundoNumero;
                    TextoDaOperacao = $"{_primeiroNumero} × {segundoNumero} = {resultado}";
                    break;
                case "/":
                    resultado = _primeiroNumero / segundoNumero;
                    TextoDaOperacao = $"{_primeiroNumero} ÷ {segundoNumero} = {resultado}";
                    break;
                case "^":
                    resultado = Math.Pow(_primeiroNumero, segundoNumero);
                    TextoDaOperacao = $"{_primeiroNumero} ^ {segundoNumero} = {resultado}";
                    break;
                case "mod":
                    resultado = _primeiroNumero % segundoNumero;
                    TextoDaOperacao = $"{_primeiroNumero} Mod {segundoNumero} = {resultado}";
                    break;
                case "%":
                    resultado = (_primeiroNumero * segundoNumero) / 100;
                    TextoDaOperacao = $"{_primeiroNumero} % de {segundoNumero} = {resultado}";
                    break;
            }

            Display = resultado.ToString();
            _entradaAtual = resultado.ToString();
            _operadorAtual = null;
            _resultadoCalculado = true;

            AddHistorico(TextoDaOperacao);
        }
    }

    private void RaizQuadrada(object parameter)
    {
        if (!string.IsNullOrEmpty(_entradaAtual))
        {
            double numero = Convert.ToDouble(_entradaAtual);
            if (numero >= 0)
            {
                double resultado = Math.Sqrt(numero);
                string TextoDaOperacao = $"√({numero}) = {resultado}";

                Display = resultado.ToString();
                _entradaAtual = resultado.ToString();
                _resultadoCalculado = true;

                AddHistorico(TextoDaOperacao);
            }
            else
            {
                Error();
            }
        }
        else if (!string.IsNullOrEmpty(_operadorAtual) && _primeiroNumero >= 0)
        {
            double resultado = Math.Sqrt(_primeiroNumero);
            string TextoDaOperacao = $"√({_primeiroNumero}) = {resultado}";

            Display = resultado.ToString();
            _entradaAtual = resultado.ToString();
            _operadorAtual = null;
            _resultadoCalculado = true;

            AddHistorico(TextoDaOperacao);
        }
        else
        {
            Error();
        }
    }

    private void TrocarSinal(object parameter)
    {
        if (!string.IsNullOrEmpty(_entradaAtual))
        {
            double valorAtual = Convert.ToDouble(_entradaAtual);
            double valorNovo = -valorAtual;
            Display = valorNovo.ToString();
            _entradaAtual = valorNovo.ToString();
        }
        else if (!string.IsNullOrEmpty(_operadorAtual) && _primeiroNumero != 0)
        {
            _primeiroNumero = -_primeiroNumero;
            Display = _primeiroNumero.ToString();
        }
    }

    private void InversoDeX(object parameter)
    {
        if (!string.IsNullOrEmpty(_entradaAtual))
        {
            double numero = Convert.ToDouble(_entradaAtual);
            if (numero != 0)
            {
                double resultado = 1 / numero;
                string TextoDaOperacao = $"1 / {numero} = {resultado}";

                Display = resultado.ToString();
                _entradaAtual = resultado.ToString();
                _resultadoCalculado = true;

                AddHistorico(TextoDaOperacao);
            }
            else
            {
                Error();
            }
        }
        else if (!string.IsNullOrEmpty(_operadorAtual) && _primeiroNumero != 0)
        {
            double resultado = 1 / _primeiroNumero;
            string TextoDaOperacao = $"1 / {_primeiroNumero} = {resultado}";

            Display = resultado.ToString();
            _entradaAtual = resultado.ToString();
            _operadorAtual = null;
            _resultadoCalculado = true;

            AddHistorico(TextoDaOperacao);
        }
    }

    private void Error()
    {
        Clear(null);
        Display = "Erro ☹";
    }
}