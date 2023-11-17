using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Calculadora_WPF
{
    public partial class MainWindow : Window
    {
        private readonly CalculadoraViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new CalculadoraViewModel();
            DataContext = _viewModel;
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string entrada = e.Text;

            if (Regex.IsMatch(entrada, @"^\d$") || entrada == "." || entrada == ",")
            {
                _viewModel.EntradaNumero(entrada);
            }
            else if (Regex.IsMatch(entrada, @"^[\+\-\*\/]$"))
            {
                _viewModel.SetOperador(entrada);
            }
            else if (entrada == "\r") // Tecla Enter
            {
                _viewModel.Calcular(null);
            }
            else if (entrada == "\b") // Tecla Backspace
            {
                _viewModel.Backspace(null);
            }
            e.Handled = true; // Indicar que o evento foi tratado e nenhum manipulador adicional deve processar a tecla
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            historySidebar.Visibility = Visibility.Visible;
            btnHistorico.Visibility = Visibility.Hidden;
        }

        private void CloseHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            historySidebar.Visibility = Visibility.Hidden;
            btnHistorico.Visibility = Visibility.Visible;
        }

        private void MiniButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
