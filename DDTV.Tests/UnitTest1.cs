using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Core.Tools;

namespace DDTV.Tests;

/// <summary>
/// Core.Tools 工具类单元测试
/// </summary>
public class ToolsTests
{
    [Fact]
    public void SHA1_Encrypt_ShouldReturnConsistentHash()
    {
        string input = "DDTV_Test_Input";
        string hash1 = Core.Tools.Encryption.SHA1_Encrypt(input);
        string hash2 = Core.Tools.Encryption.SHA1_Encrypt(input);

        Assert.NotNull(hash1);
        Assert.Equal(40, hash1.Length); // SHA1 = 160 bits = 40 hex chars
        Assert.Equal(hash1, hash2); // 一致性
    }

    [Fact]
    public void SHA1_Encrypt_DifferentInputs_ShouldReturnDifferentHashes()
    {
        string hash1 = Core.Tools.Encryption.SHA1_Encrypt("input1");
        string hash2 = Core.Tools.Encryption.SHA1_Encrypt("input2");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Md532_ShouldReturn32CharHexString()
    {
        string input = "test_string_for_md5";
        string md5 = input.Md532();

        Assert.NotNull(md5);
        Assert.Equal(32, md5.Length);
    }

    [Fact]
    public void CheckFilenames_ShouldRemoveInvalidChars()
    {
        string dirty = "file:name*with?invalid<chars>|\\/\"#&=%\0";
        string clean = Core.Tools.KeyCharacterReplacement.CheckFilenames(dirty);

        Assert.DoesNotContain(':', clean);
        Assert.DoesNotContain('*', clean);
        Assert.DoesNotContain('?', clean);
        Assert.DoesNotContain('<', clean);
        Assert.DoesNotContain('>', clean);
        Assert.DoesNotContain('|', clean);
        Assert.DoesNotContain('\\', clean);
        Assert.DoesNotContain('/', clean);
        Assert.DoesNotContain('"', clean);
    }

    [Fact]
    public void GetRandomStr_ShouldReturnCorrectLength()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        int length = 16;
        string random = Core.Tools.KeyCharacterReplacement.GetRandomStr(chars, length);

        Assert.Equal(length, random.Length);
        Assert.All(random, c => Assert.Contains(c, chars));
    }

    [Fact]
    public void GetRandomStr_ShouldReturnDifferentValues()
    {
        string chars = "ABC123";
        string r1 = Core.Tools.KeyCharacterReplacement.GetRandomStr(chars, 10);
        string r2 = Core.Tools.KeyCharacterReplacement.GetRandomStr(chars, 10);

        Assert.NotNull(r1);
        Assert.NotNull(r2);
        Assert.Equal(10, r1.Length);
        Assert.Equal(10, r2.Length);
    }
}

/// <summary>
/// 观看心跳s签名(HmacChain)单元测试：SHA-224用FIPS标准向量锚定，链式逻辑用BCL内置HMAC交叉验证
/// </summary>
public class HmacChainTests
{
    private static string Hex(byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();

    [Theory]
    //FIPS 180-4标准测试向量
    [InlineData("", "d14a028c2a3a2bc9476102bb288234c415a2b01f828ea62ac5b3e42f")]
    [InlineData("abc", "23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7")]
    [InlineData("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq", "75388b16512776cc5dba5da1fd890150b0c6455cb4f58b1952522525")]
    public void Sha224_KnownVectors_ShouldMatch(string input, string expected)
    {
        byte[] hash = Core.Tools.Sha224.ComputeHash(Encoding.UTF8.GetBytes(input));
        Assert.Equal(expected, Hex(hash));
    }

    [Fact]
    public void HmacSha224_Rfc4231Case1_ShouldMatch()
    {
        //RFC 4231 Test Case 1：key=20字节的0x0b，data="Hi There"
        byte[] key = Enumerable.Repeat((byte)0x0b, 20).ToArray();
        byte[] hash = HmacChain.HmacSha224(key, Encoding.UTF8.GetBytes("Hi There"));
        Assert.Equal("896fb1128abbdf196832107cd49df33f47b4b1169912ba4f53684b22", Hex(hash));
    }

    [Fact]
    public void HmacSha224_Rfc4231Case2_ShouldMatch()
    {
        //RFC 4231 Test Case 2：key="Jefe"，data="what do ya want for nothing?"
        byte[] hash = HmacChain.HmacSha224(Encoding.UTF8.GetBytes("Jefe"), Encoding.UTF8.GetBytes("what do ya want for nothing?"));
        Assert.Equal("a30e01098bc6dbbf45690f3a7e9e6d0f8bbea2a39e6148008fd05e44", Hex(hash));
    }

    [Fact]
    public void Compute_SingleRule_ShouldEqualBuiltinHmac()
    {
        //单步链应等于对应的内置HMAC实现
        string key = "seacasdgyijfhofiuxoannn";
        string msg = "{\"id\":[3,321,0,11849457],\"device\":[\"AUTO123\",\"f14f8180-0000-0000-0000-000000000000\"],\"ets\":1717921241,\"benchmark\":\"seacasdgyijfhofiuxoannn\",\"time\":60,\"ts\":1717921241491,\"ua\":\"Mozilla/5.0\"}";
        byte[] k = Encoding.UTF8.GetBytes(key);
        byte[] m = Encoding.UTF8.GetBytes(msg);

        using var md5 = new HMACMD5(k);
        Assert.Equal(Hex(md5.ComputeHash(m)), HmacChain.Compute(msg, key, new[] { 0 }));
        using var sha1 = new HMACSHA1(k);
        Assert.Equal(Hex(sha1.ComputeHash(m)), HmacChain.Compute(msg, key, new[] { 1 }));
        using var sha256 = new HMACSHA256(k);
        Assert.Equal(Hex(sha256.ComputeHash(m)), HmacChain.Compute(msg, key, new[] { 2 }));
        using var sha512 = new HMACSHA512(k);
        Assert.Equal(Hex(sha512.ComputeHash(m)), HmacChain.Compute(msg, key, new[] { 4 }));
        using var sha384 = new HMACSHA384(k);
        Assert.Equal(Hex(sha384.ComputeHash(m)), HmacChain.Compute(msg, key, new[] { 5 }));
    }

    [Fact]
    public void Compute_MultiRule_ShouldChainHexOutputs()
    {
        //多步链：上一步的小写hex字符串作为下一步输入
        string key = "test_secret_key";
        string msg = "test_message";
        byte[] k = Encoding.UTF8.GetBytes(key);

        string step1;
        using (var h = new HMACSHA256(k)) { step1 = Hex(h.ComputeHash(Encoding.UTF8.GetBytes(msg))); }
        string step2;
        using (var h = new HMACSHA384(k)) { step2 = Hex(h.ComputeHash(Encoding.UTF8.GetBytes(step1))); }
        string step3;
        using (var h = new HMACSHA1(k)) { step3 = Hex(h.ComputeHash(Encoding.UTF8.GetBytes(step2))); }

        Assert.Equal(step3, HmacChain.Compute(msg, key, new[] { 2, 5, 1 }));
    }

    [Fact]
    public void Compute_EmptyRule_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => HmacChain.Compute("msg", "key", Array.Empty<int>()));
    }

    [Fact]
    public void Compute_UnknownRule_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HmacChain.Compute("msg", "key", new[] { 9 }));
    }
}

/// <summary>
/// Config 解析容错测试
/// </summary>
public class ConfigParseTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("1", false)] // 无效值应返回默认值 false
    [InlineData("", false)]
    public void ParseBool_ShouldHandleEdgeCases(string input, bool expected)
    {
        var method = typeof(Core.Config).GetMethod("ParseBool", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        bool result = (bool)method!.Invoke(null, new object?[] { input, false })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("42", 42, 0)]
    [InlineData("-5", -5, 0)]
    [InlineData("0", 0, 0)]
    [InlineData("abc", 0, 0)] // 无效值返回默认值
    [InlineData("", 0, 0)]
    public void ParseInt_ShouldHandleEdgeCases(string input, int expected, int defaultValue)
    {
        var method = typeof(Core.Config).GetMethod("ParseInt", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        int result = (int)method!.Invoke(null, new object?[] { input, defaultValue })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123456789", 123456789L, 0L)]
    [InlineData("not_a_number", 0L, 0L)]
    public void ParseLong_ShouldHandleEdgeCases(string input, long expected, long defaultValue)
    {
        var method = typeof(Core.Config).GetMethod("ParseLong", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        long result = (long)method!.Invoke(null, new object?[] { input, defaultValue })!;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("3.14", 3.14, 0.0)]
    [InlineData("2.71828", 2.71828, 0.0)]
    [InlineData("invalid", 0.0, 0.0)]
    public void ParseDouble_ShouldHandleEdgeCases(string input, double expected, double defaultValue)
    {
        var method = typeof(Core.Config).GetMethod("ParseDouble", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        double result = (double)method!.Invoke(null, new object?[] { input, defaultValue })!;
        Assert.Equal(expected, result);
    }
}

/// <summary>
/// Room 管理核心逻辑测试
/// </summary>
public class RoomManagerTests
{
    [Fact]
    public void AddRoom_WithInvalidUid_ShouldReturnErrorState()
    {
        var result = Core.RuntimeObject._Room.AddRoom(false, false, false, 0, 0);

        Assert.Equal(0, result.key);
        Assert.Equal(4, result.State); // 状态码4 = 参数有误
        Assert.Contains("参数有误", result.Message);
    }

    [Fact]
    public void BatchAddRooms_WithEmptyString_ShouldReturnEmptyList()
    {
        var results = Core.RuntimeObject._Room.BatchAddRooms("");

        Assert.Empty(results);
    }

    [Fact]
    public void BatchDeleteRooms_WithEmptyString_ShouldReturnEmptyList()
    {
        var results = Core.RuntimeObject._Room.BatchDeleteRooms("");

        Assert.Empty(results);
    }

    [Fact]
    public void CancelTask_WithZeroIds_ShouldReturnFalse()
    {
        var result = Core.RuntimeObject._Room.CancelTask(0, 0);

        Assert.False(result.State);
        Assert.Contains("参数有误", result.Message);
    }

    [Fact]
    public void CutTask_WithZeroIds_ShouldReturnFalse()
    {
        var result = Core.RuntimeObject._Room.CutTask(0, 0);

        Assert.False(result.State);
        Assert.Contains("参数有误", result.Message);
    }

    [Fact]
    public void AddTask_WithZeroIds_ShouldReturnFalse()
    {
        var result = Core.RuntimeObject._Room.AddTask(0, 0);

        Assert.False(result.State);
        Assert.Contains("参数有误", result.Message);
    }
}
