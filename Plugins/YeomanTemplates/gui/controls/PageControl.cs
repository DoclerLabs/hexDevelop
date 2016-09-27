using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YeomanTemplates.gui.controls
{
    public interface PageControl
    {
        event EventHandler Ready;

        void OnCancel();

        object GetOutput();
        void setInput(object data);

        void RaiseReadyEvent();
    }
}
