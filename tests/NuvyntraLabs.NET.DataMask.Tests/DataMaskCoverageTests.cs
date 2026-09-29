using System.Text.Json.Nodes;
using NuvyntraLabs.NET.DataMask;
using NuvyntraLabs.NET.DataMask.Extensions;

namespace NuvyntraLabs.NET.DataMask.Tests;

public class DataMaskCoverageTests
{
    [Fact]
    public void Email_covers_null_missing_at_and_a_single_character_local_part()
    {
        Assert.Equal("", DataMask.Email(null));
        Assert.Equal("***********", DataMask.Email("@domain.com"));
        Assert.Equal("**", DataMask.Email("a@"));
        Assert.Equal("a@b.com", DataMask.Email("a@b.com"));
    }

    [Fact]
    public void Card_and_phone_cover_null_and_short_input()
    {
        Assert.Equal("", DataMask.Card(null));
        Assert.Equal("**", DataMask.Card("12"));
        Assert.Equal("", DataMask.Phone(""));
    }

    [Fact]
    public void Aadhaar_covers_null_letters_and_extra_digits()
    {
        Assert.Equal("", DataMask.Aadhaar(null));
        Assert.Equal("***", DataMask.Aadhaar("12a"));
        Assert.Equal("*************", DataMask.Aadhaar("1234567890123"));
    }

    [Fact]
    public void Pan_covers_null_and_lowercase_input()
    {
        Assert.Equal("", DataMask.Pan(null));
        Assert.Equal("*****1234F", DataMask.Pan(" abcde1234f "));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("AAABCDE1234F1Z5")]
    [InlineData("27123451234F1Z5")]
    [InlineData("27ABCDEABCDF1Z5")]
    [InlineData("27ABCDE123411Z5")]
    [InlineData("27ABCDE1234F-Z5")]
    [InlineData("27ABCDE1234F1Y5")]
    [InlineData("27ABCDE1234F1Z-")]
    public void Gstin_masks_a_shape_that_does_not_match(string value)
    {
        Assert.Equal(new string('*', value.Length), DataMask.Gstin(value));
    }

    [Fact]
    public void Gstin_covers_null_and_lowercase_input()
    {
        Assert.Equal("", DataMask.Gstin(null));
        Assert.Equal("27**********1Z5", DataMask.Gstin("27abcde1234f1z5"));
    }

    [Fact]
    public void Jwt_covers_null_and_incomplete_tokens()
    {
        Assert.Equal("", DataMask.Jwt(null));
        Assert.Equal("***", DataMask.Jwt("a.b"));
        Assert.Equal("****", DataMask.Jwt("a..c"));
    }

    [Fact]
    public void ApiKey_covers_null_and_a_single_character()
    {
        Assert.Equal("", DataMask.ApiKey(null));
        Assert.Equal("*", DataMask.ApiKey("a"));
    }

    [Fact]
    public void ConnectionString_covers_every_secret_key_and_a_broken_segment()
    {
        Assert.Equal("", DataMask.ConnectionString(null));
        Assert.Equal(
            "Server=db;****;******;Password=***;Pwd=***;AccountKey=***;SharedAccessKey=***;SharedAccessSignature=***;AccessToken=***;ClientSecret=***;ApiKey=***",
            DataMask.ConnectionString("Server=db;=bad;orphan;Password=p;Pwd=p;AccountKey=a;SharedAccessKey=s;SharedAccessSignature=sig;AccessToken=t;ClientSecret=c;ApiKey=k"));
    }

    [Fact]
    public void Json_masks_every_default_name_and_a_caller_name()
    {
        const string json = """
            {
              "Password": "a",
              "secret": "b",
              "token": "c",
              "apiKey": "d",
              "api_key": "e",
              "pan": "f",
              "aadhaar": "g",
              "gstin": "h",
              "card": "i",
              "cardNumber": "j",
              "cvv": "k",
              "connectionString": "l",
              "notes": "m",
              "name": "ada",
              "alias": null,
              "items": [null, {"token": "nested"}]
            }
            """;

        JsonObject root = JsonNode.Parse(DataMask.Json(json, new MaskRules { PropertyNames = [" ", "notes"] }))!.AsObject();

        foreach (string name in new[] { "Password", "secret", "token", "apiKey", "api_key", "pan", "aadhaar", "gstin", "card", "cardNumber", "cvv", "connectionString", "notes" })
        {
            Assert.Equal("***", root[name]!.GetValue<string>());
        }

        Assert.Equal("ada", root["name"]!.GetValue<string>());
        Assert.Null(root["alias"]);
        Assert.Equal("***", root["items"]![1]!["token"]!.GetValue<string>());
        Assert.Equal("{\"password\":\"***\"}", DataMask.Json("{\"password\":\"secret\"}"));
        Assert.Equal("[{\"pan\":\"***\"}]", DataMask.Json("[{\"pan\":\"ABCDE1234F\"}]"));
    }

    [Fact]
    public void Json_masks_a_null_document()
    {
        Assert.Equal("****", DataMask.Json("null"));
        Assert.Equal("{}", DataMask.Json("{}", new MaskRules()));
    }

    [Fact]
    public void Extensions_cover_card_aadhaar_pan_and_gstin()
    {
        Assert.Equal("************1111", "4111111111111111".MaskCard());
        Assert.Equal("**** **** 2346", "234123412346".MaskAadhaar());
        Assert.Equal("*****1234F", "ABCDE1234F".MaskPan());
        Assert.Equal("27**********1Z5", "27ABCDE1234F1Z5".MaskGstin());
    }
}
