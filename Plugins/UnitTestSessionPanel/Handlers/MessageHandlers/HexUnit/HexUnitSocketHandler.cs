using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers.HexUnit
{

    class StateObj
    {
        public TcpClient Client;
        public const int BufferSize = 512;
        public byte[] Buffer = new byte[BufferSize];
        public MemoryStream Data = new MemoryStream(BufferSize);
    }

    class HexUnitSocketHandler
    {
        private HexUnitHelper helper;
        private TestsSessionsPanel pluginUI;

        private TcpListener listener;
        private UTF8Encoding encoding = new UTF8Encoding(false);

        public HexUnitSocketHandler(TestsSessionsPanel ui)
        {
            helper = new HexUnitHelper();
            pluginUI = ui;

            Listen();
        }

        private void Listen()
        {
            listener = new TcpListener(IPAddress.Any, 6662);
            try
            {
                listener.Start();
            }
            catch
            {
                //TODO: Port maybe already in use
            }

            try
            {
                listener.BeginAcceptTcpClient(AcceptCallback, listener);
            }
            catch
            {
                //TODO: Accept again
            }
        }

        private void AcceptCallback(IAsyncResult r)
        {
            var listener = (TcpListener)r.AsyncState;
            var client = listener.EndAcceptTcpClient(r);

            var state = new StateObj();
            state.Client = client;

            try
            {
                client.GetStream().BeginRead(state.Buffer, 0, StateObj.BufferSize, ReadCallback, state);
            }
            catch
            {
                client.Close();
                listener.BeginAcceptTcpClient(AcceptCallback, listener);
            }
        }

        private void ReadCallback(IAsyncResult r)
        {
            var state = (StateObj)r.AsyncState;
            var client = state.Client;

            try
            {
                int bytesLen = client.GetStream().EndRead(r);

                if (bytesLen > 0)
                {
                    state.Data.Write(state.Buffer, 0, bytesLen);

                    while (bytesLen >= StateObj.BufferSize && client.GetStream().DataAvailable)
                    {
                        bytesLen = client.GetStream().Read(state.Buffer, 0, bytesLen);
                        state.Data.Write(state.Buffer, 0, bytesLen);
                    }

                    int size;
                    while (state.Data.Length > 4 && state.Data.Length >= (size = BitConverter.ToInt32(state.Data.ToArray(), 0)) + 4)
                    {
                        //TODO: Extract 
                        var message = encoding.GetString(state.Data.ToArray(), 4, size);
                        //state.Data.RemoveRange(0, size + 4);
                        var buf = state.Data.GetBuffer();
                        var newLen = state.Data.Length - (size + 4);
                        Buffer.BlockCopy(buf, size + 4, buf, 0, (int)newLen);
                        state.Data.SetLength(newLen);

                        try
                        {
                            ParseMessage(message);
                        }
                        catch (Exception e)
                        {
                            //Maybe we should clear state.Data
                        }
                    }

                    client.GetStream().BeginRead(state.Buffer, 0, StateObj.BufferSize, ReadCallback, state);
                }
                else
                {
                    client.Close();
                    // Not really sure we shouldn't accept new clients at any time...
                    listener.BeginAcceptTcpClient(AcceptCallback, listener);
                }
            }
            catch (Exception e)
            {
                client.Close();
                // Not really sure we shouldn't accept new clients at any time...
                listener.BeginAcceptTcpClient(AcceptCallback, listener);
            }
        }

        private void ParseMessage(string msg)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                var info = helper.ParseMessage(msg);
                if (info != null)
                {
                    // TODO: Invoke check inside the pluginui
                    if (pluginUI.InvokeRequired)
                    {
                        pluginUI.Invoke((Action) delegate
                        {
                            try
                            {
                                pluginUI.AddTest(info);
                            }
                            catch
                            {
                            }
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
