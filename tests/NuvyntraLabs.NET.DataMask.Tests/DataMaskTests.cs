using System.Text.Json.Nodes;
using NuvyntraLabs.NET.DataMask;
using NuvyntraLabs.NET.DataMask.Extensions;

namespace NuvyntraLabs.NET.DataMask.Tests;

public class DataMaskTests
{
    [Fact]
    public void Phone_keeps_the_last_four_digits()
    {
        Assert.Equal("******3210", DataMask.Phone("9876543210"));
        Assert.Equal("+** ***** *3210", DataMask.Phone("+91 98765 43210"));
    }

    [Fact]
    public void Phone_masks_a_short_number_entirely()
    {
        Assert.Equal("***", DataMask.Phone("123"));
        Assert.Equal("", DataMask.Phone(null));
    }

    [Fact]
    public void Email_keeps_the_first_character_and_the_domain()
    {
        Assert.Equal("j*******@gmail.com", DataMask.Email("john.doe@gmail.com"));
        Assert.Equal("************", DataMask.Email("not-an-email"));
    }

    [Fact]
    public void Card_keeps_the_last_four_digits()
    {
        Assert.Equal("************1111", DataMask.Card("4111111111111111"));
        Assert.Equal("**** **** **** 1111", DataMask.Card("4111 1111 1111 1111"));
    }

    [Fact]
    public void Aadhaar_groups_the_last_four()
    {
        Assert.Equal("**** **** 2346", DataMask.Aadhaar("234123412346"));
        Assert.Equal("**** **** 2346", DataMask.Aadhaar("2341 2341 2346"));
        Assert.Equal("***", DataMask.Aadhaar("123"));
    }

    [Fact]
    public void Pan_masks_the_leading_letters()
    {
        Assert.Equal("*****1234F", DataMask.Pan("ABCDE1234F"));
        Assert.Equal("****", DataMask.Pan("nope"));
    }

    [Fact]
    public void Gstin_keeps_the_state_and_the_last_three()
    {
        Assert.Equal("27**********1Z5", DataMask.Gstin("27ABCDE1234F1Z5"));
    }

    [Fact]
    public void Jwt_keeps_the_header()
    {
        Assert.Equal("eyJhbGciOiJIUzI1NiJ9.***.***", DataMask.Jwt("eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIn0.signature"));
        Assert.Equal("****", DataMask.Jwt("nope"));
    }

    [Fact]
    public void ApiKey_keeps_a_prefix_and_a_suffix()
    {
        Assert.Equal("sk_l**********1234", DataMask.ApiKey("sk_live_51Abcd1234"));
        Assert.Equal("****yz", DataMask.ApiKey("abcxyz"));
    }

    [Fact]
    public void ConnectionString_masks_secret_keys()
    {
        Assert.Equal(
            "Server=localhost;Password=***;User Id=sa",
            DataMask.ConnectionString("Server=localhost;Password=secret;User Id=sa"));
        Assert.Equal("*****", DataMask.ConnectionString("hello"));
    }

    [Fact]
    public void Json_masks_named_properties_and_nested_values()
    {
        string masked = DataMask.Json(
            """{"user":{"password":"secret","name":"ada"},"items":[{"pan":"ABCDE1234F"}]}""",
            new MaskRules { PropertyNames = ["notes"] });

        JsonObject root = JsonNode.Parse(masked)!.AsObject();
        Assert.Equal("***", root["user"]!["password"]!.GetValue<string>());
        Assert.Equal("ada", root["user"]!["name"]!.GetValue<string>());
        Assert.Equal("***", root["items"]![0]!["pan"]!.GetValue<string>());
    }

    [Fact]
    public void Json_masks_invalid_input()
    {
        Assert.Equal("*", DataMask.Json("{"));
        Assert.Equal("", DataMask.Json(null));
    }

    [Fact]
    public void Extensions_call_the_static_api()
    {
        Assert.Equal("******3210", "9876543210".MaskPhone());
        Assert.Equal("j*******@gmail.com", "john.doe@gmail.com".MaskEmail());
    }
}
