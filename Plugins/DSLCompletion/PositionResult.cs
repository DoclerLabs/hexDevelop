
namespace DSLCompletion
{
    class PositionResult
    {
        public int pos;
        public bool lines;
        public string file;

        public PositionResult(string file, int pos, bool lines)
        {
            this.file = file;
            this.pos = pos;
            this.lines = lines;
        }
    }
}
