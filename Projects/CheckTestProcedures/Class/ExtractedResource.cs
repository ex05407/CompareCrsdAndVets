using System.Collections.Generic;

namespace CheckTestProcedures.Class
{
    internal sealed class ExtractedResource
    {
        public string ProcedureName { get; set; }

        public string SourcePath { get; set; }

        public List<ResourceRecord> Records { get; } = new List<ResourceRecord>();
    }
}
