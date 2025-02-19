namespace DatasetGenerator
{
    public interface IChunker
    {
        IEnumerable<string> GetChunks(string text);
    }

    public class LineChunker : IChunker
    {
        private readonly int linesCount;

        public LineChunker(int linesCount)
        {
            this.linesCount = linesCount;
        }
        public IEnumerable<string> GetChunks(string text)
        {
            var lines = text.Split('\n');
            if (lines.Length > linesCount)
            {
                return Partition(lines, 30).Select(x => String.Join('\n', x));
            }
            else {
                return Enumerable.Repeat(text, 1);
            }
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> items, int partitionSize)
        {
            return items.Select((item, inx) => new { item, inx })
                        .GroupBy(x => x.inx / partitionSize)
                        .Select(g => g.Select(x => x.item));
        }
    }
}
