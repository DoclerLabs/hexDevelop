using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers.HexUnit
{
    class HexUnitSocketHandler
    {
        private HexUnitHelper helper;
        private TestsSessionsPanel pluginUI;

        private TcpListener server;
        private Thread listenerThread;

        public HexUnitSocketHandler(TestsSessionsPanel ui)
        {
            helper = new HexUnitHelper();
            pluginUI = ui;
            server = new TcpListener(IPAddress.Any, 6661);

            listenerThread = new Thread(listen);
            listenerThread.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

        private void listen()
        {
            try
            {
                server.Start();
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.ToString(), "Error starting hexUnit server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
                //PluginCore.Managers.ErrorManager.ShowError("Could not start hexUnit server", e); //does not work
            }

            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    using (var stream = new StreamReader(client.GetStream()))
                    {
                        try
                        {
                            while (!stream.EndOfStream)
                            {
                                var line = stream.ReadLine();
                                parseMessage(line);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch
            {
            }
            
            
            
        }

        private void parseMessage(string msg)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                var info = helper.ParseMessage(msg);
                if (info != null)
                {
                    if (pluginUI.InvokeRequired)
                    {
                        pluginUI.Invoke((MethodInvoker)delegate
                        {
                            pluginUI.AddTest(info);
                            pluginUI.EndUpdate();
                        });
                    }
                    else
                    {
                        pluginUI.AddTest(info);
                        pluginUI.EndUpdate();
                    }
                }
            });
        }
    }
}
