using NuvyntraLabs.NET.DataMask;
using NuvyntraLabs.NET.DataMask.Extensions;

Console.WriteLine(DataMask.Phone("9876543210"));
Console.WriteLine(DataMask.Phone("+91 98765 43210"));
Console.WriteLine(DataMask.Phone("123"));
Console.WriteLine(DataMask.Email("john.doe@gmail.com"));
Console.WriteLine(DataMask.Email("not-an-email"));
Console.WriteLine(DataMask.Card("4111111111111111"));
Console.WriteLine(DataMask.Card("4111 1111 1111 1111"));
Console.WriteLine(DataMask.Aadhaar("234123412346"));
Console.WriteLine(DataMask.Pan("ABCDE1234F"));
Console.WriteLine(DataMask.Gstin("27ABCDE1234F1Z5"));
Console.WriteLine(DataMask.Jwt("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIn0.signature"));
Console.WriteLine(DataMask.ApiKey("sk_live_51Abcd1234"));
Console.WriteLine(DataMask.ApiKey("abcxyz"));
Console.WriteLine(DataMask.ConnectionString("Server=localhost;Password=secret;User Id=sa"));
Console.WriteLine(DataMask.Json(
    """{"password":"secret","name":"ada","items":[{"pan":"ABCDE1234F"}]}""",
    new MaskRules { PropertyNames = ["notes"] }));
Console.WriteLine("9876543210".MaskPhone());
Console.WriteLine("john.doe@gmail.com".MaskEmail());
Console.WriteLine("4111111111111111".MaskCard());
Console.WriteLine("234123412346".MaskAadhaar());
Console.WriteLine("ABCDE1234F".MaskPan());
Console.WriteLine("27ABCDE1234F1Z5".MaskGstin());
