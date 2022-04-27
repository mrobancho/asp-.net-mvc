using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MRPatients.Models
{
    public class UserRole
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public bool IsSelected { get; set; }
    }
}
