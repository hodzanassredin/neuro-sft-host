
namespace DatasetGenerator
{
    public class TextDto {
        public string TextSource { get; set; }
        public string Text { get; set; }
    }

    public interface IDataset
    {
        IEnumerable<TextDto> GetTexts();
    }

    public class RecursiveDirDataset : IDataset
    {
        private readonly string path;

        public RecursiveDirDataset(string path)
        {
            this.path = path;
        }
        public IEnumerable<TextDto> GetTexts()
        {
            var dir = new DirectoryInfo(path);
            return IterateFiles(dir);
        }

        static IEnumerable<TextDto> IterateFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Select(x=> new TextDto { Text = File.ReadAllText(x.FullName), TextSource = x.FullName }).Concat(directory.GetDirectories().SelectMany(IterateFiles));
        }
    }
}
