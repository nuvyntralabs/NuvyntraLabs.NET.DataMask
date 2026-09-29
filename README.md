# NuvyntraLabs.NET.DataMask

Mask a phone, email, card, Aadhaar, PAN, GSTIN, JWT, API key, connection string, or JSON property before it is logged.

**Version:** 0.1.0. Not published to nuget.org yet. Do not `dotnet nuget push` from a local clone.

```bash
dotnet add package NuvyntraLabs.NET.DataMask
```

```csharp
DataMask.Phone("9876543210");                 // ******3210
DataMask.Phone("+91 98765 43210");            // +** ***** *3210
DataMask.Email("john.doe@gmail.com");         // j*******@gmail.com
DataMask.Card("4111 1111 1111 1111");         // **** **** **** 1111
DataMask.Aadhaar("2341 2341 2346");           // **** **** 2346
DataMask.Pan("ABCDE1234F");                   // *****1234F
DataMask.Gstin("27ABCDE1234F1Z5");            // 27**********1Z5
DataMask.Jwt(token);                          // {header}.***.***
DataMask.ApiKey("sk_live_51Abcd1234");        // sk_l**********1234
DataMask.ConnectionString("Server=localhost;Password=secret;User Id=sa");
                                              // Server=localhost;Password=***;User Id=sa
DataMask.Json(json, new MaskRules { PropertyNames = ["notes"] });
```

`null` returns `""`. A value that does not match the expected shape is masked entirely. These methods do not throw.

| Kind | What stays visible |
| --- | --- |
| Phone, card | Last 4 digits. Separators stay. Fewer than 4 digits masks the whole string. |
| Email | First character of the local part, and the domain. |
| Aadhaar | Last 4 digits as `**** **** nnnn` when 12 digits remain after spaces are removed. |
| PAN | The four digits and the final character of `[A-Z]{5}[0-9]{4}[A-Z]`. |
| GSTIN | First 2 and last 3 characters of a 15-character GSTIN. |
| JWT | The header segment. Payload and signature become `***`. |
| API key | First 4 and last 4 when the key is at least 12 characters. Shorter keys keep the last 2. |
| Connection string | Non-secret keys. `Password`, `Pwd`, `AccountKey`, `SharedAccessKey`, `SharedAccessSignature`, `AccessToken`, `ClientSecret`, and `ApiKey` values become `***`. |
| JSON | Structure and other property names. Default names: `password`, `secret`, `token`, `apiKey`, `api_key`, `pan`, `aadhaar`, `gstin`, `card`, `cardNumber`, `cvv`, `connectionString`. Match is ordinal ignore-case. Nested objects and arrays are walked. |

Card masking is a logging aid. It is not a PCI scope reduction by itself. There is no dedicated CVV method. A JSON property named `cvv` is masked because it is on the default name list.

Extension methods are opt-in. Import `NuvyntraLabs.NET.DataMask.Extensions` for `MaskPhone`, `MaskEmail`, `MaskCard`, `MaskAadhaar`, `MaskPan`, and `MaskGstin`.

The console sample calls every method:

```bash
dotnet run --project samples/DataMask.Sample
dotnet test NuvyntraLabs.NET.DataMask.sln
```

Prefer first: [Microsoft.Extensions.Compliance.Redaction](https://learn.microsoft.com/dotnet/api/microsoft.extensions.compliance.redaction) when the host already classifies data.

Target frameworks: `net8.0`, `net9.0`, and `net10.0`. No package dependencies. Nullable, trim, and Native AOT compatible.

Author: Niladri Prasad Padhy. License: MIT.
