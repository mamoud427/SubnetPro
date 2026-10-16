namespace SubnetPro.Core.Calculators;

/// <summary>Conversion décimal ⇄ binaire pour un octet (0-255).</summary>
public static class BaseConverter
{
    public static string DecimalToBinary(int value)
    {
        if (value < 0 || value > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Un octet doit être compris entre 0 et 255.");
        return Convert.ToString(value, 2).PadLeft(8, '0');
    }

    public static int BinaryToDecimal(string? bits)
    {
        var text = bits?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > 8 || text.Any(c => c != '0' && c != '1'))
            throw new FormatException("Saisir de 1 à 8 bits (0 ou 1).");
        return Convert.ToInt32(text, 2);
    }

    public static bool TryDecimalToBinary(string? input, out string binary)
    {
        binary = string.Empty;
        if (!int.TryParse(input?.Trim(), out var value) || value < 0 || value > 255) return false;
        binary = DecimalToBinary(value);
        return true;
    }

    public static bool TryBinaryToDecimal(string? input, out int value)
    {
        value = 0;
        try { value = BinaryToDecimal(input); return true; }
        catch (FormatException) { return false; }
    }
}