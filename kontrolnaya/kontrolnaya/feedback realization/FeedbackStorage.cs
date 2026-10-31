namespace kontrolnaya;

public static class FeedbackStorage
{
    public static readonly List<Feedback> Items = new List<Feedback>();
    private static int _nextId = 1;

    public static int NextId() => _nextId++;
    
    public static Feedback? Find(int id) => Items.FirstOrDefault(f => f.Id == id);

    public static bool IsAuthor(string a) => (a.Length >= 2) && (a.Length <= 60);

    public static bool IsMessage(string m) => (m.Length >= 5) && (m.Length <= 300);
    
    public static bool IsStatus(string s) => (s == "pending") || (s == "moderated");
    
    public static bool AuthorFilter(Feedback f, string? query) =>
        string.IsNullOrEmpty(query) ||
        f.Author.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    
    public static IEnumerable<Feedback> SortedByMessageLength(IEnumerable<Feedback> src) =>
        src.OrderBy(f => f.Message.Length).ThenBy(f => f.Id);
}