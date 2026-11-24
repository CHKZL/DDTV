using Core.RuntimeObject;
using RoomInfoService = Core.RuntimeObject.RoomInfo;
using RoomStatusResponse = Core.Network.Methods.Room.UidsInfo_Class;
using UserInfoModel = Core.Network.Methods.User.UserInfo;

namespace DDTV.Tests;

public class RoomInfoNicknameTests
{
    private static RoomCardClass CreateExistingCard(string name = "原昵称")
    {
        return new RoomCardClass
        {
            UID = 123,
            RoomId = 456,
            Name = name,
            Title = new() { Value = "旧标题" },
            live_status = new() { Value = 0 }
        };
    }

    private static RoomStatusResponse.Data CreateStatusData(string? nickname)
    {
        return new RoomStatusResponse.Data
        {
            uid = 123,
            room_id = 456,
            uname = nickname!,
            title = "新标题",
            live_status = 1
        };
    }

    private static RoomStatusResponse CreateResponse(long uid, string? nickname, int liveStatus)
    {
        return new RoomStatusResponse
        {
            code = 0,
            data = new Dictionary<string, RoomStatusResponse.Data>
            {
                [uid.ToString()] = new()
                {
                    uid = uid,
                    uname = nickname!,
                    title = $"标题-{liveStatus}",
                    live_status = liveStatus
                }
            }
        };
    }

    private static UserInfoModel CreateUserInfo(string? nickname)
    {
        return new UserInfoModel
        {
            code = 0,
            data = new()
            {
                mid = 123,
                name = nickname!,
                live_room = new()
                {
                    roomid = 456,
                    liveStatus = 1,
                    title = "新标题"
                }
            }
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToRoomCard_InvalidStatusNickname_PreservesNameAndUpdatesOtherFields(string? nickname)
    {
        RoomCardClass card = CreateExistingCard();

        RoomCardClass result = RoomInfoService.ToRoomCard(CreateStatusData(nickname), card);

        Assert.Same(card, result);
        Assert.Equal("原昵称", result.Name);
        Assert.Equal("新标题", result.Title.Value);
        Assert.Equal(1, result.live_status.Value);
    }

    [Fact]
    public void ToRoomCard_ValidStatusNickname_UpdatesName()
    {
        RoomCardClass card = CreateExistingCard();

        RoomCardClass result = RoomInfoService.ToRoomCard(CreateStatusData("新昵称"), card);

        Assert.Equal("新昵称", result.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToRoomCard_NewCardWithInvalidNickname_UsesEmptyString(string? nickname)
    {
        RoomCardClass result = RoomInfoService.ToRoomCard(CreateStatusData(nickname), null!);

        Assert.Equal(string.Empty, result.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToRoomCard_InvalidUserInfoNickname_PreservesNameAndUpdatesOtherFields(string? nickname)
    {
        RoomCardClass card = CreateExistingCard();

        RoomCardClass result = RoomInfoService.ToRoomCard(CreateUserInfo(nickname), card);

        Assert.Equal("原昵称", result.Name);
        Assert.Equal("新标题", result.Title.Value);
        Assert.Equal(1, result.roomStatus.Value);
    }

    [Fact]
    public void ApplyRoomStatusResponse_NicknameSequence_PreservesLastKnownGoodName()
    {
        const long uid = 910000000001;
        RoomCardClass card = new()
        {
            UID = uid,
            Name = "初始昵称",
            live_status = new() { Value = 0 }
        };
        _Room.SetRoomCardByUid(uid, card);
        try
        {
            RoomInfoService.ApplyRoomStatusResponse(CreateResponse(uid, "正常昵称", 0));
            Assert.Equal("正常昵称", card.Name);

            RoomInfoService.ApplyRoomStatusResponse(CreateResponse(uid, string.Empty, 1));
            Assert.Equal("正常昵称", card.Name);
            Assert.Equal(1, card.live_status.Value);

            RoomInfoService.ApplyRoomStatusResponse(CreateResponse(uid, null, 0));
            Assert.Equal("正常昵称", card.Name);
            Assert.Equal(0, card.live_status.Value);

            RoomInfoService.ApplyRoomStatusResponse(CreateResponse(uid, "改名后的昵称", 1));
            Assert.Equal("改名后的昵称", card.Name);
        }
        finally
        {
            _Room.GetCardList().TryRemove(uid, out _);
        }
    }

    [Fact]
    public void ApplyRoomStatusResponse_ErrorCode_DoesNotChangeExistingRoom()
    {
        const long uid = 910000000002;
        RoomCardClass card = new()
        {
            UID = uid,
            Name = "原昵称",
            Title = new() { Value = "旧标题" },
            live_status = new() { Value = 0 }
        };
        _Room.SetRoomCardByUid(uid, card);
        try
        {
            RoomStatusResponse response = CreateResponse(uid, "异常响应昵称", 1);
            response.code = -412;
            response.msg = "请求被拦截";

            RoomInfoService.ApplyRoomStatusResponse(response);

            RoomCardClass actual = new();
            Assert.True(_Room.GetCardForUID(uid, ref actual));
            Assert.Same(card, actual);
            Assert.Equal("原昵称", actual.Name);
            Assert.Equal("旧标题", actual.Title.Value);
            Assert.Equal(0, actual.live_status.Value);
        }
        finally
        {
            _Room.GetCardList().TryRemove(uid, out _);
        }
    }
}
