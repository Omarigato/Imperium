using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.Admin
{
    public class UpdateAdminDto
    {
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool? IsAdmin { get; set; }
    }
}
