using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers.HexUnit
{

    class StateObj
    {
        public Socket Client;
        public const int BufferSize = 256;
        public byte[] Buffer = new byte[BufferSize];
        public List<byte> Data = new List<byte>();
    }

    class HexUnitSocketHandler
    {
        private HexUnitHelper helper;
        private TestsSessionsPanel pluginUI;

        //private Thread listenerThread;
        private ManualResetEvent done;
        private TcpListener listener;

        public HexUnitSocketHandler(TestsSessionsPanel ui)
        {
            helper = new HexUnitHelper();
            pluginUI = ui;

            done = new ManualResetEvent(false);
            //listenerThread = new Thread(Listen);
            //listenerThread.IsBackground = true;
            //listenerThread.Start();
            Listen();
        }

        private void Listen()
        {
            listener = new TcpListener(IPAddress.Any, 6662);
            try
            {
                listener.Start();

                done.Reset();
                
                listener.BeginAcceptSocket (AcceptCallback, listener);
            }
            catch
            {
            }
        }

        private void AcceptCallback(IAsyncResult r)
        {
            done.Set();

            var listener = (TcpListener)r.AsyncState;
            var handler = listener.EndAcceptSocket(r);

            var state = new StateObj();
            state.Client = handler;

            handler.BeginReceive(state.Buffer, 0, StateObj.BufferSize, SocketFlags.None, ReadCallback, state);
        }

        private void ReadCallback(IAsyncResult r)
        {
            var state = (StateObj)r.AsyncState;
            var handler = state.Client;

            try
            {
                int bytesLen = handler.EndReceive(r);

                if (bytesLen > 0)
                {
                    var array = new byte[bytesLen];
                    Buffer.BlockCopy(state.Buffer, 0, array, 0, bytesLen);
                    state.Data.AddRange(array);

                    var size = state.Data[0];

                    while (state.Data.Count >= size + 1)
                    {
                        var encoding = new UTF8Encoding(false);
                        var message = encoding.GetString(state.Data.ToArray(), 1, size);
                        state.Data.RemoveRange(0, size);
                        
                        ParseMessage(message);
                    }

                    handler.BeginReceive(state.Buffer, 0, StateObj.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                    done.Reset();
                    listener.BeginAcceptSocket(AcceptCallback, listener);
                }
            }
            catch (SocketException e) //thrown after first message was received, why?
            {
                handler.Close();
                listener.BeginAcceptSocket(AcceptCallback, listener);
            }
            catch (Exception e)
            {
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
                handler.BeginReceive(state.Buffer, 0, StateObj.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                //listener.BeginAcceptSocket(AcceptCallback, listener);
            }
        }

        private void ParseMessage(string msg)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                var info = helper.ParseMessage(msg);
                if (info != null)
                {
                    if (pluginUI.InvokeRequired)
                    {
                        pluginUI.Invoke((Action) delegate
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
