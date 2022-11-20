using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ConfigGenerator
{
    [Generator]
    public class GenerateConfigClasses : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var configurationDictionary = new Dictionary<string, object>();
            
            LoadAndMergeConfigFiles(context, configurationDictionary);

            var topLevelProperties = configurationDictionary
                .Where(dict => !(dict.Value is Dictionary<string, object>))
                .ToList();

            var separateConfigClasses =
                configurationDictionary.Except(topLevelProperties)
                    .ToList();

            var configSectionClasses = new StringBuilder();
            foreach (var configClazz in separateConfigClasses)
            {
                BuildConfigClass(configClazz, configSectionClasses);
            }

            var sourceBuilder = new StringBuilder(@"
            using System;
            using System.Collections.Generic;

            namespace ApplicationConfig
            {
                using ApplicationConfigurationSections;                
                
                public class MyAppConfig
                {
            ");

            //foreach (var (key, value) in topLevelProperties)
            foreach(var item in topLevelProperties)
            {
                var value = item.Value;
                var key = item.Key;

                if (value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    // Check first value to see what kind of array (list) it needs to be
                    var propertyType = GetPropertyTypeNameBasedOnValue(element.EnumerateArray().FirstOrDefault());

                    sourceBuilder.Append(
                        $"public IEnumerable<{propertyType}> {NormalizePropertyName(key)} {{ get; set; }}");
                }
                else
                {
                    if (value is JsonElement element1)
                    {
                        var propertyType = GetPropertyTypeNameBasedOnValue(element1);
                        sourceBuilder.Append($"public {propertyType} {NormalizePropertyName(key)} {{ get; set; }}");
                    }                    
                }
            }

            //foreach (var (key, _) in separateConfigClasses)
            foreach(var item in separateConfigClasses)
            {
                var key = item.Key;
                sourceBuilder.Append(
                    $"public {NormalizePropertyName(key)} {NormalizePropertyName(key)}{{ get; set; }}");
            }

            sourceBuilder.Append("}");
            sourceBuilder.Append("}");

            // Put configSectionClasses in separate namespace
            var configSectionNamespaceSb = new StringBuilder(@"                                    
            namespace ApplicationConfigurationSections
            {
               
            ");
            configSectionNamespaceSb.Append(configSectionClasses.ToString());
            configSectionNamespaceSb.Append("}");

            sourceBuilder.Append(configSectionNamespaceSb.ToString());

            context.AddSource("MyAppConfig", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }

        private static void BuildConfigClass(KeyValuePair<string, object> classInfo, StringBuilder sb)
        {
            StringBuilder nestedClasses = new StringBuilder();

            sb.Append($"public class {classInfo.Key}");
            sb.Append("{");

            foreach (var item in (Dictionary<string, object>)classInfo.Value)
            {
                if (item.Value is Dictionary<string, object>)
                {
                    sb.Append($"public {item.Key} {NormalizePropertyName(item.Key)} {{ get; set; }}");
                    BuildConfigClass(item, nestedClasses);
                }
                else
                {
                    var prop = (JsonElement)item.Value;
                    var propertyType = GetPropertyTypeNameBasedOnValue(prop);
                    if (prop.ValueKind == JsonValueKind.Array)
                    {
                        sb.Append(
                            $"public IEnumerable<{propertyType}> {NormalizePropertyName(item.Key)} {{ get; set; }}");
                    }
                    else
                    {
                        sb.Append($"public {propertyType} {NormalizePropertyName(item.Key)} {{ get; set; }}");
                    }
                }
            }

            sb.Append("}");

            sb.AppendLine(nestedClasses.ToString());
        }

        private static string GetPropertyTypeNameBasedOnValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.Number => "int",
                JsonValueKind.True or JsonValueKind.False => "bool",
                _ => "string",
            };
        }

        private static string NormalizePropertyName(string originalName)
        {
            var underscore = "_";
            var newPropertyName = originalName
                .Replace(".", underscore)
                .Replace("$", underscore);
            return newPropertyName;
        }

        private static void LoadAndMergeConfigFiles(
            GeneratorExecutionContext context, 
            Dictionary<string, object> resultConfigurationDictionary)
        {
            foreach (var configFile in context.AdditionalFiles)
            {
                if (Path.GetExtension(configFile.Path).Equals(".json"))
                {
                    var contentOfFile = configFile.GetText()?.ToString();

                    if (!string.IsNullOrEmpty(contentOfFile))
                    {
                        var deserializedJson = DeserializeToDictionary(contentOfFile ?? "");
                        MergeDictionaries(resultConfigurationDictionary, deserializedJson);
                    }
                }
            }
        }
        
        private static Dictionary<string, object> DeserializeToDictionary(string configJson)
        {
            var configValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(configJson) ?? new Dictionary<string, JsonElement>();
            
            var finalConfigJson = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JsonElement> configValue in configValues)
            {
                if (configValue.Value.ValueKind is JsonValueKind.Object)
                {
                    finalConfigJson.Add(configValue.Key, DeserializeToDictionary(configValue.Value.ToString()));
                }
                else
                {
                    finalConfigJson.Add(configValue.Key, configValue.Value);
                }
            }

            return finalConfigJson;
        }

        private static void MergeDictionaries(Dictionary<string, object> dictionary1, Dictionary<string, object> dictionary2)
        {
            foreach (var entry in dictionary2)
            {
                if (!dictionary1.ContainsKey(entry.Key))
                {
                    dictionary1.Add(entry.Key, entry.Value);
                }
                else
                {
                    // Which one has the most values? (in case of object)
                    if (entry.Value is Dictionary<string, object> existingObjectValue)
                    {
                        int numberOfValuesInExistingObject = existingObjectValue.Count;
                        var numberOfValuesInNewObject =
                            ((Dictionary<string, object>)dictionary1[entry.Key]).Count;

                        if (numberOfValuesInExistingObject < numberOfValuesInNewObject)
                        {
                            // replace existing object with new object
                            dictionary1[entry.Key] = entry.Value;

                            MergeDictionaries((Dictionary<string, object>)dictionary1[entry.Key], (Dictionary<string, object>)entry.Value);
                        }
                    }
                }
            }
        }
    }
}
