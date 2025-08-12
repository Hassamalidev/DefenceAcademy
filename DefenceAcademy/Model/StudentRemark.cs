namespace DefenceAcademy.Model
{ 
public class StudentRemark
{
    public int Id { get; set; }
    public string StudentName { get; set; }
    public string Remark { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
}
}
