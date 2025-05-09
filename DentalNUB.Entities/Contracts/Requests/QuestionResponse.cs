namespace DentalNUB.Entities

{
    public record QuestionResponse
    {
        public int QuestID { get; set; }
        public string QuesText { get; set; } = string.Empty;
    }
}