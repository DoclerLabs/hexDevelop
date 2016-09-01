namespace DSLCompletion
{
    /**
     * TODO: finish
     */
    class ServerCompletionHandler : HaxeBaseHandler
    {
        int Port;

        public ServerCompletionHandler(int port) : base()
        {
            Port = port;
        }

        public bool IsRunning()
        {
            if (process == null) return false;


            return !process.HasExited;
        }

        public void StartServer()
        {
            process.Start();
        }

        override public PositionResult GetPosition(string type)
        {
            if (!IsRunning()) StartServer();

            return null;
        }
    }
}
