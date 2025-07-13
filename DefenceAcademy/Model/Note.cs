namespace DefenceAcademy.Model
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Explanation { get; set; }
        public string Subject { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}