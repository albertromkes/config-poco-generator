using System;
using System.Collections.Generic;

namespace ApplicationConfig
{
    using ApplicationConfigurationSections;

    public class MyAppConfig
    {
        public bool DuplicateEntryOnlyShowsOnce { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public long LongValue { get; set; }
        public ulong ULongValue { get; set; }
        public IEnumerable<double> ArrayWithDoubles { get; set; }
        public IEnumerable<string> ArrayWithStrings { get; set; }
        public IEnumerable<int> ArrayWithInts { get; set; }
        public IEnumerable<bool> ArrayWithBools { get; set; }
        public IEnumerable<long> ArrayWithLongs { get; set; }
        public IEnumerable<ulong> ArrayWithULongs { get; set; }
        public bool ShowDeveloperWarnings { get; set; }
        public Logging Logging { get; set; }
    }
}
namespace ApplicationConfigurationSections
{

    public class Logging { public Microsoft Microsoft { get; set; } }
    public class Microsoft { public LogLevel LogLevel { get; set; } }
    public class LogLevel { public string Default { get; set; } public string EasyNetQ_Consumer { get; set; } }


}