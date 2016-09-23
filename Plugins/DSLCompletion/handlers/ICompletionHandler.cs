using System.Collections.Generic;

namespace DSLCompletion
{
    delegate void ListCallback(List<string> list);
    delegate void PositionCallback(PositionResult pos);
    delegate void StringCallback(string str);

    interface ICompletionHandler
    {
        void GetPosition(string type, PositionCallback callback);
        void GetFile(string file, StringCallback callback);
        void GetCompletion(string path, ListCallback callback);
        void GetCompletePath(string module, ListCallback callback);
    }
}
