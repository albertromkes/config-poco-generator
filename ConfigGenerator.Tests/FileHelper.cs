using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigGenerator.Tests
{
    public class FileHelper
    {
        private static readonly string app = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static string jsonFolder = Path.Combine(app, "TestFiles");

        private static string MakeFileName(string filename)
        {
            return jsonFolder + "/" + filename;
        }

        private static string GetFile(string filename)
        {
            return File.ReadAllText(MakeFileName(filename));
        }

        public static class TextContent
        {
            public static string Expected = GetFile("expected.cs");
            public static string AppSettings = GetFile("appsettings.json");
            public static string AppSettingsProduction = GetFile("appsettings.Production.json");
        }
    }
}
