
namespace org.GoodSpace.Data.Formats.TodoTxt
{
    internal class LineReadEventArgs : EventArgs
    {
        public int LineNumber { get; set; }
        public string? LineContents { get; set; }
    }

    internal class StreamParser
    {
        public event EventHandler<LineReadEventArgs>? LineRead;
        public event EventHandler? ReadComplete;

        readonly Stream stream;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public StreamParser(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentException"></exception>
        public StreamParser(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!Path.IsPathRooted(filePath))
                filePath = Path.GetFullPath(filePath);

            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public StreamParser(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File does not exist.");

            stream = fileInfo.OpenRead();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerable<string> Parse()
        {
            if (!stream.CanRead)
                throw new InvalidOperationException("Stream cannot be read.");

            using BufferedStream bs = new(stream);
            using StreamReader sr = new(bs);
            string? line;
            var lineNo = 1;

            while ((line = sr.ReadLine()) != null)
            {
                LineRead?.Invoke(stream, new LineReadEventArgs()
                {
                    LineNumber = lineNo++,
                    LineContents = line,
                });

                yield return line;
            }

            ReadComplete?.Invoke(stream, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllLines()
        {
            foreach (var line in Parse())
                yield return line;
        }
    }
}
