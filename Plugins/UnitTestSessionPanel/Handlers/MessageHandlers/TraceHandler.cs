using System.Collections.Generic;
using PluginCore;
using PluginCore.Managers;
using UnitTestSessionsPanel.Handlers.MessageHandlers.FlexUnit;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers
{
    class TraceHandler : IEventHandler
    {
        private readonly TestsSessionsPanel ui;
        private readonly ITraceMessageHandler implementation;
        private int lastLogIndex;

        public TraceHandler(TestsSessionsPanel pluginUi)
        {
            ui = pluginUi;
            implementation = new FlexUnitMessageHandler(pluginUi);
            lastLogIndex = 0;
        }

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Trace:
                    ProcessTraces();
                    ui.EndUpdate();
                    break;
            }
        }

        private void ProcessTraces()
        {
            IList<TraceItem> log = TraceManager.TraceLog;

            for (int i = lastLogIndex; i < log.Count; i++)
                implementation.ProcessMessage(log[i].Message);

            lastLogIndex = log.Count;
        }
    }
}