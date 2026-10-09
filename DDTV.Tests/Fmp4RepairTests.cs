using Core.Tools;

namespace DDTV.Tests;

/// <summary>
/// Core.Tools.Fmp4Repair 的 fMP4 结构解析测试
/// </summary>
public class Fmp4RepairTests
{
    private static byte[] BuildBox(string type, int payloadLen)
    {
        // 标准32位长度的box：[4字节大端总长][4字节类型][payload]
        int total = 8 + payloadLen;
        var box = new byte[total];
        box[0] = (byte)(total >> 24); box[1] = (byte)(total >> 16); box[2] = (byte)(total >> 8); box[3] = (byte)total;
        box[4] = (byte)type[0]; box[5] = (byte)type[1]; box[6] = (byte)type[2]; box[7] = (byte)type[3];
        return box;
    }

    private static byte[] BuildExtendedSizeBox(string type, int payloadLen)
    {
        // 扩展长度box：32位长度字段写1，实际64位大端largesize跟在类型后
        int total = 16 + payloadLen;
        var box = new byte[total];
        box[0] = 0; box[1] = 0; box[2] = 0; box[3] = 1;
        box[4] = (byte)type[0]; box[5] = (byte)type[1]; box[6] = (byte)type[2]; box[7] = (byte)type[3];
        long ls = total;
        for (int i = 0; i < 8; i++)
            box[8 + i] = (byte)(ls >> (56 - 8 * i));
        return box;
    }

    [Fact]
    public void Scan_StandardBoxes_ParsesAll()
    {
        var data = BuildBox("ftyp", 16).Concat(BuildBox("moof", 8)).Concat(BuildBox("mdat", 8)).ToArray();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
        try
        {
            File.WriteAllBytes(path, data);
            var result = Fmp4Repair.Scan(path);

            Assert.False(result.IsCorrupted);
            Assert.Equal(3, result.Boxes.Count);
            Assert.Equal((0L, 0x66747970u, 24L), result.Boxes[0]);
            Assert.Equal((24L, 0x6D6F6F66u, 16L), result.Boxes[1]);
            Assert.Equal((40L, 0x6D646174u, 16L), result.Boxes[2]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Scan_ExtendedSizeBox_ReadsFull64BitLargesize()
    {
        // 回归测试：largesize(64位大端)必须读完整8字节。
        // 旧实现只读高4字节，<4GB的box会得到0，被误判为损坏点。
        var extBox = BuildExtendedSizeBox("free", 8); // 总长24，largesize=24
        var data = BuildBox("ftyp", 16).Concat(extBox).Concat(BuildBox("moof", 8)).Concat(BuildBox("mdat", 8)).ToArray();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
        try
        {
            File.WriteAllBytes(path, data);
            var result = Fmp4Repair.Scan(path);

            Assert.False(result.IsCorrupted);
            Assert.Equal(4, result.Boxes.Count);
            Assert.Equal((24L, 0x66726565u, 24L), result.Boxes[1]); // free
            Assert.Equal((48L, 0x6D6F6F66u, 16L), result.Boxes[2]); // moof
            Assert.Equal((64L, 0x6D646174u, 16L), result.Boxes[3]); // mdat
        }
        finally
        {
            File.Delete(path);
        }
    }
}
