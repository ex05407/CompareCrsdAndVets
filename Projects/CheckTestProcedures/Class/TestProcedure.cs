using System.Collections.Generic;

namespace CheckTestProcedures.Class
{
    internal sealed class TestProcedure
    {
        public string Name { get; set; }

        public List<ResourceRecord> Records { get; } = new List<ResourceRecord>();
    }
}
