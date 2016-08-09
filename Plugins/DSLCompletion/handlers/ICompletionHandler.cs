using System.Collections.Generic;

namespace DSLCompletion
{
    interface ICompletionHandler
    {
        PositionResult GetPosition(string type);
        string GetFile(string file);
        List<string> GetCompletion(string path);
    }
}
