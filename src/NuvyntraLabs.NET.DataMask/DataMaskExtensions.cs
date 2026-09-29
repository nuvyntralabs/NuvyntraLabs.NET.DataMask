namespace NuvyntraLabs.NET.DataMask.Extensions;

/// <summary>Optional string extensions. Import this namespace only where masking call sites should see them.</summary>
public static class DataMaskExtensions
{
    /// <inheritdoc cref="DataMask.Phone"/>
    public static string MaskPhone(this string? value) => DataMask.Phone(value);

    /// <inheritdoc cref="DataMask.Email"/>
    public static string MaskEmail(this string? value) => DataMask.Email(value);

    /// <inheritdoc cref="DataMask.Card"/>
    public static string MaskCard(this string? value) => DataMask.Card(value);

    /// <inheritdoc cref="DataMask.Aadhaar"/>
    public static string MaskAadhaar(this string? value) => DataMask.Aadhaar(value);

    /// <inheritdoc cref="DataMask.Pan"/>
    public static string MaskPan(this string? value) => DataMask.Pan(value);

    /// <inheritdoc cref="DataMask.Gstin"/>
    public static string MaskGstin(this string? value) => DataMask.Gstin(value);
}
