using System.Diagnostics;

namespace Verademo.Commands
{
    public class AdminExecuteCommand
    {
        private string _action;
        public string Action
        {
            get { return _action; }
            set
            {
                _action = value;
                Exec();
            }
        }

        private void Exec()
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = _action,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            }.Start();
        }
    }
}