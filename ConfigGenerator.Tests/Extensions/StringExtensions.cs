using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace ConfigGenerator.Tests.Extensions
{
    public static class StringExtensions
    {
        public static void AssertSourceCodesEquals(this string expected, string actual)
        {
            Assert.Equal(expected.TrimWhiteSpaces(), actual.TrimWhiteSpaces());
        }

        public static string TrimWhiteSpaces(this string text)
        {
            if (text == null) return text;

            return Regex.Replace(text, @"\s+", string.Empty);
        }
    }
}
