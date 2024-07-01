using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.LogModule
{
    public class Opcode
    {

        public enum Config
        { 
            /// <summary>
            /// 读取配置文件
            /// </summary>
            ReadingConfigurationFile=10101,
            /// <summary>
            /// 更新到配置文件
            /// </summary>
            UpdateToConfigurationFile=10102,
            /// <summary>
            /// 读取房间文件
            /// </summary>
            ReadingRoomFiles=10103,
            /// <summary>
            /// 更新到房间文件
            /// </summary>
            UpdateToRoomFile=10104,
            /// <summary>
            /// 修改配置
            /// </summary>
            ModifyConfiguration=10105,
            /// <summary>
            /// 检测到更新
            /// </summary>
            UpdateDetect=10106,
        }
        public enum Room
        {
            /// <summary>
            /// 新增房间配置成功
            /// </summary>
            SuccessfullyAddedRoom=20101,
            /// <summary>
            /// 新增房间配置失败
            /// </summary>
            FailedToAddRoomConfiguration=20102,
            /// <summary>
            /// 修改房间录制配置
            /// </summary>
            ModifyRoomRecordingConfiguration=20103,
            /// <summary>
            /// 修改房间弹幕配置
            /// </summary>
            ModifyRoomBulletScreenConfiguration=20104,
            /// <summary>
            /// 修改房间提示配置
            /// </summary>
            ModifyRoomPromptConfiguration=20105,
            /// <summary>
            /// 手动触发录制任务
            /// </summary>
            ManuallyTriggeringRecordingTasks=20106,
            /// <summary>
            /// 删除房间成功
            /// </summary>
            SuccessfullyDeletedRoom=20107,
            /// <summary>
            /// 删除房间失败
            /// </summary>
            FailedToDeleteRoom=20108,
            /// <summary>
            /// 取消录制成功
            /// </summary>
            CancelRecordingSuccessful=20109,
            /// <summary>
            /// 取消录制失败
            /// </summary>
            CancelRecordingFail=20110,
            /// <summary>
            /// 触发快剪成功
            /// </summary>
            SuccessfullyTriggeredQuickCut=20111,
            /// <summary>
            /// 触发快剪失败
            /// </summary>
            TriggerQuickCutFail=20112,
            /// <summary>
            /// 新增录制任务成功
            /// </summary>
            SuccessfullyAddedRecordingTask=20113,
            /// <summary>
            /// 新增录制任务失败
            /// </summary>
            FailedToAddRecordingTask=20114,
        }
        public enum Account
        {
            /// <summary>
            /// 用户同意协议
            /// </summary>
            UserConsentAgreement=30101,
            /// <summary>
            /// 用户未同意协议
            /// </summary>
            UserDoesNotAgreeToAgreement=30102,
            /// <summary>
            /// 触发重新登陆
            /// </summary>
            TriggerLoginAgain=30103,
            /// <summary>
            /// 登陆成功
            /// </summary>
            LoginSuccessful=30104,
            /// <summary>
            /// 更新登录态缓存
            /// </summary>
            UpdateLoginStateCache=30105,
            /// <summary>
            /// 登录态失效
            /// </summary>
            InvalidLoginStatus=30106,
            /// <summary>
            /// 扫码登陆确认
            /// </summary>
            ScanCodeConfirmation=30107,
            /// <summary>
            /// 二维码等待扫码
            /// </summary>
            QrCodeWaitingForScann=30108,
            /// <summary>
            /// 已扫码等待确认
            /// </summary>
            ScannedCodeWaitingForConfirmation=30109,
            /// <summary>
            /// 二维码已过期
            /// </summary>
            QrCodeExpir=30110,
        }
        public enum Download
        {
            /// <summary>
            /// 保存弹幕相关文件
            /// </summary>
            SaveBulletScreenFile=40101,
            /// <summary>
            /// 触发开播事件
            /// </summary>
            StartLiveEvent=40102,
            /// <summary>
            /// 开播提醒
            /// </summary>
            StartBroadcastingReminder=40103,
            /// <summary>
            /// 开始录制
            /// </summary>
            StartRecording=40104,
            /// <summary>
            /// 录制结束
            /// </summary>
            RecordingEnd=40105,
            /// <summary>
            /// 停止直播事件
            /// </summary>
            StopLiveEvent=40106,
            /// <summary>
            /// 录制触发重新连接
            /// </summary>
            Reconnect=40107,
            /// <summary>
            /// HLS任务成功开始
            /// </summary>
            HlsTaskStart=40108,
            /// <summary>
            /// FLV任务成功开始
            /// </summary>
            FlvTaskStart=40109,
            /// <summary>
            /// 下播提醒
            /// </summary>
            EndBroadcastingReminder=40110,
        }
    }
}
