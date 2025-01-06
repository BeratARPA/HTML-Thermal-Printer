using System;
using System.Windows;
using System.Windows.Threading;

namespace PrintHTML.Core.Helpers
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
                throw new Exception($"There is a problem while printing.\n\nError Message: {exception.Message}");
            }
        }
    }
}
