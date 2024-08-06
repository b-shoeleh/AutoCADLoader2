using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADLoader.Utility
{
    public enum Status
    {
        Existing,
        Updated,
        New
    }

    public enum ResourceType
    {
        Package,
        Setting,
        Support
    }
}
