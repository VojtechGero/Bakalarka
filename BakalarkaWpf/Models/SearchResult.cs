namespace BakalarkaWpf.Models;

public class SearchResult
{
    public string FilePath { get; set; }
    public int PageNumber { get; set; }
    public string MatchedText { get; set; }
    public int MatchIndex { get; set; }
    public int BoxIndex { get; set; }
}