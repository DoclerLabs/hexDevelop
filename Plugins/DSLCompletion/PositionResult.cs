
namespace DSLCompletion
{
    class PositionResult
    {
        public int pos;
        public string file;

        public PositionResult(string file, int pos)
        {
            this.file = file;
            this.pos = pos;
        }
    }
}
