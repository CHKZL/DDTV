namespace Auxiliary.Upload
{
    public enum TaskType
    {
        Uninitialized,
        OneDrive,
        Cos,
        BaiduPan,
        Oss
    }
    public enum FileType
    {
        Uninitialized,
        flv,
        gift,
        danmu,
        mp4
    }
    public enum Status
    {
        Uninitialized,
        Success,
        Fail,
        OnHold,
        OnGoing
    }
}
