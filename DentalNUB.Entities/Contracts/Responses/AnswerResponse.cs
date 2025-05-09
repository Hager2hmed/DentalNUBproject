namespace DentalNUB.Entities
{
    public record AnswerResponse
    {
        public int AnsID { get; set; }
        public string AnsText { get; set; } = string.Empty;
        public int QuestID { get; set; }
    }
}