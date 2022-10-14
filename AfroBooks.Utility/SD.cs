using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.Utility
{
    public static class SD
    {
        public static readonly string[] RolesList = { "Individual", "Company", "Admin", "Employee" };
        public static readonly Dictionary<string, string> RolesDict = new Dictionary<string, string>()
        {
            {"Individual","Individual" },
            {"Company","Company"},
            {"Admin","Admin" },
            {"Employee","Employee" }
        };
    }
}

