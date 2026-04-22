using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Search
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; } = string.Empty; // Product / Customer / Supplier
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
    }
}
