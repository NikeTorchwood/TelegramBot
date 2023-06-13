namespace EconomicDirections;

public class EconomicDirection
{
    public EconomicDirection((string, bool) direction)
    {
        Name = direction.Item1;
        IsRank = direction.Item2;
    }

    public bool IsRank { get; set; }
    public string? Name { get; set; }
    public int Plan { get; set; }
    public int Fact { get; set; }
    public int Rank { get; set; }
}