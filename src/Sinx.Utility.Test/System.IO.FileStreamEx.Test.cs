using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sinx.Utility.Test
{
    public class FileStreamExTest
    {
        [Fact]
        public void SaveAsyncTest()
        {
            var src = @"C:\Users\Administrator\Desktop\1.docx";
            var des = @"C:\Users\Administrator\Desktop\123\1.html";
            if (File.Exists(des)) { File.Delete(des); }
            var result = new FileStream(src, FileMode.Open)
                .SaveAsync(FileStreamEx.FileType.Word, des)
                .Result;
            Assert.True(File.Exists(des));
        }
    }
}
