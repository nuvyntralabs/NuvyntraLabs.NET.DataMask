using System.Text.Json.Nodes;

namespace NuvyntraLabs.NET.DataMask;

/// <summary>Extra JSON property names to mask, in addition to the built-in names.</summary>
public sealed class MaskRules
{
    /// <summary>Property names matched with ordinal ignore-case.</summary>
    public IReadOnlyList<string> PropertyNames { get; init; } = [];
}

/// <summary>
/// Masks a known sensitive string before it is written to a log.
/// Unrecognized input is masked entirely. These methods do not throw on bad input.
/// Card masking is a logging aid. It is not a PCI scope reduction by itself.
/// </summary>
public static class DataMask
{
    private static readonly HashSet<string> SecretKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Pwd",
        "AccountKey",
        "SharedAccessKey",
        "SharedAccessSignature",
        "AccessToken",
        "ClientSecret",
        "ApiKey",
    };

    private static readonly string[] DefaultJsonNames =
    [
        "password",
        "secret",
        "token",
        "apiKey",
        "api_key",
        "pan",
        "aadhaar",
        "gstin",
        "card",
        "cardNumber",
        "cvv",
        "connectionString",
    ];

    /// <summary>Keeps the last four digits. Separators stay. Fewer than four digits masks the whole string.</summary>
    public static string Phone(string? value) => MaskDigitsKeepLast(value, 4);

    /// <summary>Keeps the first character of the local part and the domain.</summary>
    public static string Email(string? value)
    {
        if (value is null)
        {
            return "";
        }

        int at = value.IndexOf('@');
        if (at <= 0 || at == value.Length - 1)
        {
            return MaskAll(value);
        }

        string local = value[..at];
        string domain = value[(at + 1)..];
        return local[0] + new string('*', local.Length - 1) + "@" + domain;
    }

    /// <summary>Keeps the last four digits. Spaces and other separators stay. This does not validate the card.</summary>
    public static string Card(string? value) => MaskDigitsKeepLast(value, 4);

    /// <summary>Shows the last four digits as <c>**** **** nnnn</c> when the value has 12 digits.</summary>
    public static string Aadhaar(string? value)
    {
        if (value is null)
        {
            return "";
        }

        Span<char> digits = stackalloc char[12];
        int count = 0;
        foreach (char character in value)
        {
            if (character == ' ')
            {
                continue;
            }

            if (character is < '0' or > '9' || count == 12)
            {
                return MaskAll(value);
            }

            digits[count++] = character;
        }

        if (count != 12)
        {
            return MaskAll(value);
        }

        return $"**** **** {digits[8]}{digits[9]}{digits[10]}{digits[11]}";
    }

    /// <summary>Masks the five leading letters of a PAN and keeps the four digits and the final character.</summary>
    public static string Pan(string? value)
    {
        if (value is null)
        {
            return "";
        }

        string pan = value.Trim().ToUpperInvariant();
        if (pan.Length == 10
            && IsLetters(pan.AsSpan(0, 5))
            && IsDigits(pan.AsSpan(5, 4))
            && IsLetters(pan.AsSpan(9, 1)))
        {
            return "*****" + pan[5..];
        }

        return MaskAll(value);
    }

    /// <summary>Keeps the first two and last three characters of a 15-character GSTIN.</summary>
    public static string Gstin(string? value)
    {
        if (value is null)
        {
            return "";
        }

        string gstin = value.Trim().ToUpperInvariant();
        if (gstin.Length == 15
            && IsDigits(gstin.AsSpan(0, 2))
            && IsLetters(gstin.AsSpan(2, 5))
            && IsDigits(gstin.AsSpan(7, 4))
            && IsLetters(gstin.AsSpan(11, 1))
            && char.IsAsciiLetterOrDigit(gstin[12])
            && gstin[13] == 'Z'
            && char.IsAsciiLetterOrDigit(gstin[14]))
        {
            return string.Concat(gstin.AsSpan(0, 2), "**********", gstin.AsSpan(12, 3));
        }

        return MaskAll(value);
    }

    /// <summary>Keeps the JWT header and replaces the payload and signature with <c>***</c>.</summary>
    public static string Jwt(string? value)
    {
        if (value is null)
        {
            return "";
        }

        string[] parts = value.Split('.');
        if (parts.Length != 3 || parts[0].Length == 0 || parts[1].Length == 0 || parts[2].Length == 0)
        {
            return MaskAll(value);
        }

        return parts[0] + ".***.***";
    }

    /// <summary>Keeps the first four and last four characters when the key is at least 12 characters. Shorter keys keep the last two.</summary>
    public static string ApiKey(string? value)
    {
        if (value is null)
        {
            return "";
        }

        if (value.Length >= 12)
        {
            return string.Concat(value.AsSpan(0, 4), new string('*', value.Length - 8), value.AsSpan(value.Length - 4));
        }

        if (value.Length >= 2)
        {
            return new string('*', value.Length - 2) + value[^2..];
        }

        return MaskAll(value);
    }

    /// <summary>Replaces secret connection-string values with <c>***</c>. Keys that are not secrets stay.</summary>
    public static string ConnectionString(string? value)
    {
        if (value is null)
        {
            return "";
        }

        if (!value.Contains('=', StringComparison.Ordinal))
        {
            return MaskAll(value);
        }

        string[] parts = value.Split(';');
        for (int index = 0; index < parts.Length; index++)
        {
            string part = parts[index];
            int equals = part.IndexOf('=');
            if (equals <= 0)
            {
                parts[index] = MaskAll(part);
                continue;
            }

            string key = part[..equals].Trim();
            if (SecretKeys.Contains(key))
            {
                parts[index] = key + "=***";
            }
        }

        return string.Join(';', parts);
    }

    /// <summary>
    /// Masks values of sensitive JSON property names. Invalid JSON is masked entirely.
    /// Built-in names: password, secret, token, apiKey, api_key, pan, aadhaar, gstin, card, cardNumber, cvv, connectionString.
    /// </summary>
    public static string Json(string? json, MaskRules? rules = null)
    {
        if (json is null)
        {
            return "";
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (System.Text.Json.JsonException)
        {
            return MaskAll(json);
        }

        if (node is null)
        {
            return MaskAll(json);
        }

        var names = new HashSet<string>(DefaultJsonNames, StringComparer.OrdinalIgnoreCase);
        if (rules?.PropertyNames is not null)
        {
            foreach (string name in rules.PropertyNames)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }

        MaskNode(node, names);
        return node.ToJsonString();
    }

    private static void MaskNode(JsonNode node, HashSet<string> names)
    {
        if (node is JsonObject obj)
        {
            foreach (string key in obj.Select(pair => pair.Key).ToArray())
            {
                if (names.Contains(key))
                {
                    obj[key] = "***";
                }
                else if (obj[key] is JsonNode child)
                {
                    MaskNode(child, names);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (JsonNode? item in array)
            {
                if (item is not null)
                {
                    MaskNode(item, names);
                }
            }
        }
    }

    private static string MaskDigitsKeepLast(string? value, int keep)
    {
        if (value is null)
        {
            return "";
        }

        int digits = 0;
        foreach (char character in value)
        {
            if (character is >= '0' and <= '9')
            {
                digits++;
            }
        }

        if (digits < keep)
        {
            return MaskAll(value);
        }

        int seen = 0;
        int maskUntil = digits - keep;
        char[] chars = value.ToCharArray();
        for (int index = 0; index < chars.Length; index++)
        {
            if (chars[index] is < '0' or > '9')
            {
                continue;
            }

            if (seen < maskUntil)
            {
                chars[index] = '*';
            }

            seen++;
        }

        return new string(chars);
    }

    private static string MaskAll(string value) => new('*', value.Length);

    private static bool IsLetters(ReadOnlySpan<char> value)
    {
        foreach (char character in value)
        {
            if (!char.IsAsciiLetter(character))
            {
                return false;
            }
        }

        return value.Length > 0;
    }

    private static bool IsDigits(ReadOnlySpan<char> value)
    {
        foreach (char character in value)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        return value.Length > 0;
    }
}
