namespace myapp.Models;

public class Query
{
    public int Id { get; set; }
    public string SqlQuery { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}