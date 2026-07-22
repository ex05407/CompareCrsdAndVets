using System.Collections.Generic;

namespace CheckTestProcedures.Class
{
    internal sealed class ConfigSelectedSettings
    {
        public List<ConfigSelectedGroup> Groups { get; } = new List<ConfigSelectedGroup>();

        public List<ConfigSelectedItem> Items { get; } = new List<ConfigSelectedItem>();
    }
}
