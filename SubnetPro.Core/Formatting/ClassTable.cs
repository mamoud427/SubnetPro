namespace SubnetPro.Core.Formatting;

/// <summary>Ligne du tableau des classes IPv4 ; IsCurrent met en évidence la classe de l'adresse saisie.</summary>
public sealed record ClassTableRow(string Letter, string FirstOctetRange, string Usage, bool IsCurrent);

public static class ClassTable
{
    public static IReadOnlyList<ClassTableRow> Build(string? currentLetter) => new List<ClassTableRow>
    {
        new("A", "1–126", "Unicast", currentLetter == "A"),
        new("B", "128–191", "Unicast", currentLetter == "B"),
        new("C", "192–223", "Unicast", currentLetter == "C"),
        new("D", "224–239", "Multicast", currentLetter == "D"),
        new("E", "240–255", "Réservé", currentLetter == "E"),
    };
}