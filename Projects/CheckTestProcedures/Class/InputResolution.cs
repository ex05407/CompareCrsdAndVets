using System.Collections.Generic;

namespace CheckTestProcedures.Class
{
    internal sealed class InputResolution
    {
        public InputKind Kind { get; set; }

        public string InputPath { get; set; }

        public string ResolvedRootPath { get; set; }

        public string OutputBaseName { get; set; }

        public IReadOnlyList<string> SourceFiles { get; set; } = new List<string>();
    }
}
