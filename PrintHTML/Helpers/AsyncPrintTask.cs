using System;
using System.Windows;
using System.Windows.Threading;

namespace PrintHTML.Helpers
{
    public class AsyncPrintTask
    {
        public static void Exec(bool highPriority, Action action)
        {
            if (highPriority)
            {
                InternalExec(action);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                   new Action(() => InternalExec(action)));
            }
        }

        private static void InternalExec(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"There is a problem while printing.\n\nError Message: {exception.Message}");
            }
        }
    }
}
