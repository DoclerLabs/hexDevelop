using System;
using System.Collections.Generic;

namespace DSLCompletion
{
    /// <summary>
    /// Uses the haxe compiler together with a Haxe macro to get accurate, but slow completion results.
    /// </summary>
    class CompilerCompletionHandler : HaxeBaseHandler
    {
        public CompilerCompletionHandler() : base()
        {
        }

        override public PositionResult GetPosition(string type)
        {
            base.GetPosition(type);

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.find('" + type + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();
            if (rawResult == null) return null;

            var result = rawResult.Split(';');
            if (result.Length != 2) return null;

            var file = result[0];
            var pos = result[1];

            return new PositionResult(file, Int32.Parse(pos));
        }

        /// <summary>
        /// Gets completion options for the given path
        /// </summary>
        override public List<string> GetCompletion(string path)
        {
            base.GetCompletion(path);

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.getCompletion('" + path + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();
            if (rawResult == null || rawResult == "[]") return null;

            var result = rawResult.Substring(1, rawResult.Length - 2).Split(','); //remove [ and ] and split
            var list = new List<string>();

            foreach (string type in result)
            {
                list.Add(type);
            }

            return list;
        }

        public override string GetFile(string file)
        {
            base.GetFile(file);

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.getFile('" + file + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();

            return rawResult;
        }

        /// <summary>
        /// Starts the compiler and waits for the Macro to print out 
        /// </summary>
        /// <returns></returns>
        private string waitForCompiler()
        {
            try
            {
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.Close();

                output = output.Replace("\r\n", "\n");

                foreach (string line in output.Split('\n', '\r'))
                {
                    if (line.StartsWith("ReferenceMacro "))
                    {
                        return line.Substring("ReferenceMacro ".Length);
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
