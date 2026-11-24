using Core.RuntimeObject;

namespace DDTV.Tests;

public class SMTPTests
{
    [Fact]
    public void GetDisplayName_ValidNickname_ReturnsNickname()
    {
        RoomCardClass room = new() { UID = 123, Name = "主播昵称" };

        Assert.Equal("主播昵称", SMTP.GetDisplayName(room));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetDisplayName_InvalidNickname_ReturnsUid(string? nickname)
    {
        RoomCardClass room = new() { UID = 123, Name = nickname! };

        Assert.Equal("UID:123", SMTP.GetDisplayName(room));
    }
}
