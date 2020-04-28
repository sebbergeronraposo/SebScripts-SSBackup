using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Support
{
    public enum ApplicationType
    {
        Service,
        Client,
        AnyApp
    }
    public enum Severity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    public  enum PathType
    {
        Directory,
        File
    }
}
