using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sinx.Utility.Console
{
    /// <summary>
    /// System.IO.FileStreamEx.Save Method use Aspose.Words whitch not support netcoreapp
    /// this console build for run your netcoreapp in windows and wanna use save method
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WordToHtml
    {
        // src [des]
        public static void Main(string[] args)
        {
            new FileStream(args[0], FileMode.Open)
                .SaveAsync(FileStreamEx.FileType.Word, args.ElementAtOrDefault(1)
                    ?? Path.GetDirectoryName(args[0]) 
                    + @"\Formated\" + Path.GetFileNameWithoutExtension(args[0]) + ".html").GetAwaiter();
        }
    }
}
