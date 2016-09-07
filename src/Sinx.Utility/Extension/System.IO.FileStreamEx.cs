// ReSharper disable once CheckNamespace
namespace System.IO
{
    public static class FileStreamEx
    {
#if NET45
/// <summary>
/// 来源文件的类型
/// </summary>
        public enum FileType
        {
            Word,
            Excel,
            Ppt,
            Pdf
        }
        /// <summary>
        /// 将文件流转换为指定文件格式
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="pathDst"></param>
        /// <returns></returns>
        public static string Save(this FileStream stream, FileType type, string pathDst)
        {
            pathDst = Path.GetFullPath(pathDst);
            SaveType saveType = GetSaveType(pathDst);
            switch (type)
            {
                case FileType.Word:
                    var doc = new Aspose.Words.Document(stream);
                    doc.Save(pathDst, (Aspose.Words.SaveFormat)saveType);
                    break;
                case FileType.Excel:
                case FileType.Ppt:
                case FileType.Pdf:
                default:
                    throw new Exception("暂不支持的文件类型");
            }
            // 去除评估版dll声明
            var html = File.ReadAllText(pathDst);
            html = Text.RegularExpressions.Regex.Replace(html, "(Evaluation Only\\. Created with Aspose\\.(.+?)\\. Copyright \\d+-\\d+ Aspose Pty Ltd\\.)|(This document was truncated here because it was created in the Evaluation Mode\\.)", "");
            File.WriteAllText(pathDst, html);
            return pathDst;
        }

        private enum SaveType
        {
            Html = 50 // Words[Checked]
        }

        private static SaveType GetSaveType(string pathDst)
        {
            switch (Path.GetExtension(pathDst))
            {
                case ".html":
                    return SaveType.Html;
                default:
                    throw new Exception("Sinx: 不支持的保存类型");
            }
        }

        //public enum ConvertModel
        //{
        //    SingleDir,
        //    DirAndSubDir
        //}
        ///// <summary>
        ///// 将Office格式, PDF格式转换成HTML格式
        ///// </summary>
        ///// <param name="pathSrc"></param>
        ///// <param name="pathDst"></param>
        ///// <returns></returns>
        //public static string ConvertToHtml(string pathSrc, string pathDst = null)
        //{
        //    if (!System.IO.File.Exists(pathSrc))
        //    {
        //        throw new Exception($"指定文件 {pathSrc} 不存在!");
        //    }

        //    var extfilename = System.IO.Path.GetExtension(pathSrc);
        //    if (string.IsNullOrWhiteSpace(pathDst))
        //    {
        //        pathDst = System.IO.Path.GetDirectoryName(pathSrc) + @"\Formated\" + System.IO.Path.GetFileName(pathSrc) + ".html";
        //    }

        //    SaveType saveType = GetSaveType(pathDst);
        //    switch (extfilename)
        //    {
        //        case ".doc":
        //        case ".docx":
        //            var docSrc = new Aspose.Words.Document(pathSrc);
        //            docSrc.Save(pathDst, (Aspose.Words.SaveFormat)saveType);
        //            break;
        //        //case ".xls":
        //        //case ".xlsx":
        //        //	Cells.Workbook wb = new Cells.Workbook(pathSrc);
        //        //	wb.Save(pathDst, new Cells.HtmlSaveOptions(Cells.SaveFormat.Html));
        //        //	break;
        //        //case ".ppt":
        //        //case ".pptx":
        //        //	using (var pres = new Slides.Presentation(pathSrc))
        //        //	{
        //        //		var htmlOpt = new Slides.Export.HtmlOptions
        //        //		{
        //        //			HtmlFormatter = Slides.Export.HtmlFormatter.CreateDocumentFormatter("", false)
        //        //		};
        //        //		pres.Save(pathDst, Slides.Export.SaveFormat.Html, htmlOpt);
        //        //	}
        //        //	break;
        //        //case ".pdf":
        //        //	var pdfSrc = new Pdf.Document(pathSrc);
        //        //	pdfSrc.Save(pathDst, Pdf.SaveFormat.Html);
        //        //	//pdfSrc.Save(pathDst, new Pdf.HtmlSaveOptions
        //        //	//{
        //        //	//	FixedLayout = true,
        //        //	//	RasterImagesSavingMode = Pdf.HtmlSaveOptions.RasterImagesSavingModes.AsExternalPngFilesReferencedViaSvg,
        //        //	//	FontSavingMode = Pdf.HtmlSaveOptions.FontSavingModes.SaveInAllFormats,
        //        //	//	// Split HTML output into pages
        //        //	//	SplitCssIntoPages = true,
        //        //	//	// Split css into pages
        //        //	//	SplitIntoPages = true
        //        //	//});
        //        //	break;
        //        default:
        //            throw new Exception("不支持的格式");
        //    }
        //    var html = System.IO.File.ReadAllText(pathDst);
        //    html = Regex.Replace(html, "(Evaluation Only\\. Created with Aspose\\.(.+?)\\. Copyright \\d+-\\d+ Aspose Pty Ltd\\.)|(This document was truncated here because it was created in the Evaluation Mode\\.)", "");
        //    System.IO.File.WriteAllText(pathDst, html);
        //    return pathDst;
        //}

        ///// <summary>
        ///// 将指定目录中的所有Office格式和PDF格式的文件转换为HTML格式
        ///// </summary>
        ///// <param name="dirSrc"></param>
        ///// <param name="dirDst"></param>
        ///// <param name="model">SingleDir : 转换指定目录的所有文件    DirAndSubDir : 转换指定目录以及子目录的所有文件</param>
        ///// <returns></returns>
        //public static string ConvertToHtmls(string dirSrc, string dirDst = null, ConvertModel model = ConvertModel.SingleDir)
        //{
        //    if (!System.IO.Directory.Exists(dirSrc))
        //    {
        //        throw new Exception($"指定目录 {dirSrc} 不存在");
        //    }
        //    var files = model == ConvertModel.SingleDir ? System.IO.Directory.GetFiles(dirSrc).ToList() : GetAllFiles(dirSrc);
        //    Parallel.ForEach(files, file =>
        //    {
        //        try
        //        {
        //            dirDst = ConvertToHtml(file);
        //            Debug.WriteLine($"info: 转换 {file} 成功!");
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine("warning: " + file + ex.Message);
        //        }
        //    });
        //    return model == ConvertModel.SingleDir ? System.IO.Path.GetDirectoryName(dirDst) : dirSrc;
        //}
#else
#endif
    }
}