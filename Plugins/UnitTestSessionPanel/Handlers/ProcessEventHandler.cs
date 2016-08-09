using PluginCore;

namespace UnitTestSessionsPanel.Handlers
{
    class ProcessEventHandler : IEventHandler
    {
        private readonly TestsSessionsPanel ui;

        public ProcessEventHandler(TestsSessionsPanel pluginUi)
        {
            ui = pluginUi;
        }

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.ProcessStart:
                    ui.ClearStats();
                    break;
                case EventType.ProcessEnd:
                    ui.EndUpdate();
                    break;
            }
        }
    }
}