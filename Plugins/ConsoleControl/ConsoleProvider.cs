using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleControl
{
    public interface ConsoleProvider : System.Windows.Forms.IContainerControl
    {
        ConsoleColor ConsoleBackColor
        {
            get;
            set;
        }

        ConsoleColor ConsoleForeColor
        {
            get;
            set;
        }

        string WorkingDirectory
        {
            set;
        }

        event EventHandler Exited;

        void Cancel();
        void SendString(string str, bool execute = true);

    }
}
