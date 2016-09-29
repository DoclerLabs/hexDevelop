using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void GetCompletePath(string module, ListCallback callback)
        {
            fallbackHandler.GetCompletePath(module, delegate (List<string> list)
            {
                if (list != null)
                {
                    callback(list);
                    return;
                }

                compilerHandler.GetCompletePath(module, callback);
            });
        }

        public void GetPosition(string type, PositionCallback callback)
        {
            fallbackHandler.GetPosition(type, callback);
        }

        public void GetCompletion(string path, ListCallback callback)
        {
            fallbackHandler.GetCompletion(path, delegate (List<string> list)
            {
                callback(list);
                compilerHandler.GetCompletion(path, delegate (List<string> compiler)
                {
                    if (compiler != null && compiler.Count != 0)
                    {
                        foreach (var r in compiler)
                        {
                            if (!list.Contains(r))
                            {
                                list.Add(r);
                            }
                        }
                        callback(list);
                    }
                });
            });

            //if (result != null) return result;
            //var compilerResult = compilerHandler.GetCompletion(path);

            //if (compilerResult != null)
            //{
            //    foreach (var r in compilerResult)
            //    {
            //        if (!result.Contains(r))
            //        {
            //            result.Add(r);
            //        }
            //    }
            //}
        }

        public void GetFile(string file, StringCallback callback)
        {
            fallbackHandler.GetFile(file, callback);
        }
    }
}
