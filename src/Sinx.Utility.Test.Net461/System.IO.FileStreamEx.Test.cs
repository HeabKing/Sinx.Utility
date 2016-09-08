using System.IO;
using Xunit;

namespace Sinx.Utility.Test.Net461
{
    public class FileStreamExTest
    {
        [Fact]
        public void SaveTest1()
        {
            var desPath = $@"{Directory.GetCurrentDirectory()}\1\1.html";
            var desPath2 = new FileStream("1.docx", FileMode.Open).SaveAsync(FileStreamEx.FileType.Word, desPath).Result;
            Assert.True(File.Exists(desPath));
            Assert.True(File.Exists(desPath2));
            Assert.True(desPath == desPath2);
        }
    }
}
