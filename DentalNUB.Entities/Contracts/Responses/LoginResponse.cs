namespace DentalNUB.Entities
{

    public record LoginResponse
    {
        public string? Token { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
    }
}