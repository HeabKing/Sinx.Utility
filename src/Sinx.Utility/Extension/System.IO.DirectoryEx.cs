using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace

namespace System.IO
{
    public static class DirectoryEx
    {
        /// <summary>
        ///     获取指定的目录中的所有文件（不包括目录）
        /// </summary>
        /// <param name="directory">指定目录</param>
        /// <returns>文件列表</returns>
        public static IEnumerable<string> GetFiles(string directory)
        {
            // 递归获取下一级目录中的所有文件 - 委托
            Func<string, IEnumerable<string>> getAllSubFiles = dir =>
                    Directory.GetDirectories(dir).SelectMany(GetFiles);

            return Directory.GetFiles(directory) // 获取当前目录所有本目录文件
                .Concat(getAllSubFiles(directory)); // 递归获取下一级目录中的所有文件, 并跟上边的合并
        }
    }
}