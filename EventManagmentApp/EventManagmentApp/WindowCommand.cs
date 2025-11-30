using System.Windows;
using System.Windows.Input;

namespace EventManagmentApp
{
    public static class WindowCommand
    {
        public static readonly ICommand MinimizeCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is Window window)
                    window.WindowState = WindowState.Minimized;
            });

        public static readonly ICommand CloseCommand = new RelayCommand(
            parameter =>
            {
                if (parameter is Window window)
                    window.Close();
            });
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}