
using Microsoft.AspNetCore.Http;

namespace DentalNUB.Entities

{
    public class CreateToolRequest
    {
        public string ToolName { get; set; }
        public decimal? Price { get; set; } // ممكن يكون Null لو Free
        public bool IsFree { get; set; }
        public IFormFile Image { get; set; }
    }
}