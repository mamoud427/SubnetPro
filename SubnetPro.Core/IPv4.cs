namespace SubnetPro.Core;

/// <summary>Utilitaires bas niveau : une adresse IPv4 est manipulée comme un uint (32 bits).</summary>
public static class IPv4
{
    public static bool TryParse(string? text, out uint value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Trim().Split('.');
        if (parts.Length != 4) return false;

        uint result = 0;
        foreach (var part in parts)
        {
            if (part.Length == 0 || part.Length > 3 || !part.All(char.IsAsciiDigit)) return false;
            if (!int.TryParse(part, out var octet) || octet > 255) return false;
            result = (result << 8) | (uint)octet;
        }

        value = result;
        return true;
    }

    public static uint Parse(string? text) =>
        TryParse(text, out var value)
            ? value
            : throw new FormatException($"Adresse IPv4 invalide : « {text} ».");

    public static string ToText(uint value) =>
        $"{value >> 24}.{(value >> 16) & 255}.{(value >> 8) & 255}.{value & 255}";

    public static uint MaskFromCidr(int cidr)
    {
        if (cidr < 0 || cidr > 32)
            throw new ArgumentOutOfRangeException(nameof(cidr), "Le préfixe CIDR doit être compris entre 0 et 32.");
        return cidr == 0 ? 0u : uint.MaxValue << (32 - cidr);
    }
}