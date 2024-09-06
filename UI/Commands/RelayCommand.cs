using System;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ek24.UI.Commands;


/// <summary>
/// RealyCommand iss a common pattern used in MVVM for handling commands 
/// in WPF. It's often manually implemented in the project.
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

    public void Execute(object parameter) => _execute((T)parameter);
}

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

    public async void Execute(object parameter) => await _execute();
}




// Non-generic version for commands without parameters
public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action execute, Func<bool> canExecute = null)
        : base(
            p => execute(),
            p => canExecute == null || canExecute()
        )
    {
    }
}
