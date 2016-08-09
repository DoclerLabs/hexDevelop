using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DSLCompletion
{
    public enum CompletionMethod
    {
        Compiler = 0,
        Fallback = 1,
        Combined = 2
    }

    [Serializable]
    class Settings
    {
        private CompletionMethod method = CompletionMethod.Combined;

        [DisplayName("Completion Method"), DefaultValue(CompletionMethod.Combined)]
        public CompletionMethod CompletionMethod
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }
    }
}
