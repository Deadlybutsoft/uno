namespace MemoryMatch.Models;

public class MemoryCard
{
    public int Id { get; set; }
    public string Emoji { get; set; } = "";
    public int PairId { get; set; }
    public bool IsFlipped { get; set; }
    public bool IsMatched { get; set; }
    public string GradientStart { get; set; } = "#667eea";
    public string GradientEnd { get; set; } = "#764ba2";
}
