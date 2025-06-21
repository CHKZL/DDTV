# shell使用说明

## 系统要求
- linux，并且支持`/bin/bash`命令，以及相关路径有能有读写权限

## 注意事项
- 执行时机为直播间录制结束(房间开播状态变化为未直播)时触发。
- 因为本质是使用DOTNET的Process执行bash程序，使用的命令格式如下，请注意命令内容的和安全性
```C#
Process process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "/bin/bash",
        Arguments = $"-c \"{Command}\"",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }
};
process.Start();
string result = process.StandardOutput.ReadToEnd();
process.WaitForExit();
return result;
```


## 关键配置
- `DDTV_Config.ini`中的`Linux_Only_ShellSwitch`和`Linux_Only_ShellCommand`,前者为录制完成后是否执行shell的开关，布尔值。后者为全局的shell命令模版，当房间没有单独的shell命令时，就会使用这一个模版。
- `RoomListConfig.json`中每个房间的`Shell`字段，这和上面的`Linux_Only_ShellCommand`功能相同，但是优先级更高，当某个房间有单独的shell的时候，会使用房间配置的shell而不是`Linux_Only_ShellCommand`

## 关键字
- `{AfterRepairFiles}`、`{DanmakuFiles}`、`{SCFiles}`、`{GuardFiles}`、`{GiftFiles}`,这5个关键字分别为，最后生成的录制文件，弹幕文件，SC记录文件，大航海记录文件，礼物记录文件，会被替换为半角逗号`,`进行分割的文件队列(例如：`{AfterRepairFiles}`会被替换为`/tmp/1/A.mp4,/tmp/1/B.mp4,/tmp/1/C.mp4`这样的绝对路径文件队列)

## 怎么查看命令的替换和执行情况
- 在本次启动的日志sqlite文件中，能看到Source为shell的记录，会将输入的原始shell以及替换后的shell打印出来
