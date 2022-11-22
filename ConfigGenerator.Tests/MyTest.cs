using ConfigGenerator;
using ConfigGenerator.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ConfigGenerator.Tests.Extensions;
using Xunit;

namespace GeneratorTests.Tests
{
    
    public class GeneratorTests
    {
        [Fact]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");
           
            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            GenerateConfigClasses generator = new GenerateConfigClasses();
            
            // Create the driver that will control the generation, passing in our generator                 
            var appSettingsAdditionalFile = new List<AdditionalText> {new AppSettingsAdditionalText(), new AppSettingsProductionAdditionalText() }.ToImmutableArray();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { generator }, appSettingsAdditionalFile);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            //Debug.Assert(outputCompilation.GetDiagnostics().IsEmpty); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();
            var generatedCode = runResult.Results[0].GeneratedSources.First().SourceText.ToString();

            var expected = FileHelper.TextContent.Expected;
            expected.AssertSourceCodesEquals(generatedCode);                        
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    public class AppSettingsAdditionalText : AdditionalText
    {
        public override string Path => "appsettings.json";

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
           return SourceText.From(FileHelper.TextContent.AppSettings);
        }
    }

    public class AppSettingsProductionAdditionalText : AdditionalText
    {
        public override string Path => "appsettings.Production.json";

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(FileHelper.TextContent.AppSettingsProduction);
        }
    }
}