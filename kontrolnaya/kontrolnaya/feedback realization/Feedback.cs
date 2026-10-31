namespace kontrolnaya;

public class Feedback
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "pending";
}