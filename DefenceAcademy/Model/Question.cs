namespace DefenceAcademy.Model
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectOption { get; set; }
        public int CategoryId { get; set; }
    }
}
