using System.Collections.Generic;

namespace DSLCompletion
{
    /// <summary>
    /// Uses a combination of CompilerCompletionHandler and FallbackCompletionHandler to get the best results
    /// </summary>
    /// <seealso cref="CompilerCompletionHandler"/>
    /// <seealso cref="FallbackCompletionHandler"/>
    class CombinedCompletionHandler : ICompletionHandler
    {
        CompilerCompletionHandler compilerHandler;
        FallbackCompletionHandler fallbackHandler;

        public CombinedCompletionHandler()
        {
            compilerHandler = new CompilerCompletionHandler();
            fallbackHandler = new FallbackCompletionHandler();
        }

        public PositionResult GetPosition(string type)
        {
            var result = compilerHandler.GetPosition(type);

            if (result != null) return result;

            return fallbackHandler.GetPosition(type);
        }

        public List<string> GetCompletion(string path)
        {
            var result = fallbackHandler.GetCompletion(path);

            if (result != null) return result;

            return compilerHandler.GetCompletion(path);
        }

        public string GetFile(string file)
        {
            return fallbackHandler.GetFile(file);
        }
    }
}
