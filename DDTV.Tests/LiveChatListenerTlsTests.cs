using System.ComponentModel;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Authentication;

namespace DDTV.Tests;

/// <summary>
/// Core.LiveChat.LiveChatListener TLS协商失败识别的单元测试
/// </summary>
public class LiveChatListenerTlsTests
{
    private static Exception BuildObservedChain()
    {
        //复现用户错误日志中的真实异常链:
        //WebSocketException → HttpRequestException → AuthenticationException → Win32Exception(0x80090302)
        var win32 = new Win32Exception(unchecked((int)0x80090302));
        var auth = new AuthenticationException("Authentication failed, see inner exception.", win32);
        var http = new HttpRequestException("The SSL connection could not be established, see inner exception.", auth);
        return new WebSocketException("Unable to connect to the remote server", http);
    }

    [Fact]
    public void IsSchannelUnsupportedFunction_ObservedUserChain_ReturnsTrue()
    {
        Assert.True(Core.LiveChat.LiveChatListener.IsSchannelUnsupportedFunction(BuildObservedChain()));
    }

    [Fact]
    public void IsSchannelUnsupportedFunction_DeepChain_ReturnsTrue()
    {
        var ex = new Exception("outer", new Exception("middle", new Win32Exception(unchecked((int)0x80090302))));
        Assert.True(Core.LiveChat.LiveChatListener.IsSchannelUnsupportedFunction(ex));
    }

    [Fact]
    public void IsSchannelUnsupportedFunction_OtherWin32Error_ReturnsFalse()
    {
        var ex = new WebSocketException("fail", new Win32Exception(unchecked((int)0x80090325)));
        Assert.False(Core.LiveChat.LiveChatListener.IsSchannelUnsupportedFunction(ex));
    }

    [Fact]
    public void IsSchannelUnsupportedFunction_NoWin32InChain_ReturnsFalse()
    {
        Assert.False(Core.LiveChat.LiveChatListener.IsSchannelUnsupportedFunction(new HttpRequestException("no win32 here")));
    }
}
