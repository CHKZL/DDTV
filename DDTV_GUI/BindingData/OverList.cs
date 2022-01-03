using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.BindingData
{
    public class OverList: INotifyPropertyChanged
    {
        private string name;
        private string roomid;
        private string downszie;
        private string starttime;
        private string endtime;
        private string title;
        private string fileName;
        private string filePath;
        private long uid;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string RoomId
        {
            get
            {
                return roomid;
            }
            set
            {
                roomid = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string DownSzie
        {
            get
            {
                return downszie;
            }
            set
            {
                downszie = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string StartTime
        {
            get
            {
                return starttime;
            }
            set
            {
                starttime = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string EndTime
        {
            get
            {
                return endtime;
            }
            set
            {
                endtime = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public long Uid
        {
            get
            {
                return uid;
            }
            set
            {
                uid = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public OverList(string name, string roomid, string downszie, string starttime, string endTime, string title,long uid,string filePath,string fileName)
        {
            this.name = name;
            this.roomid = roomid;
            this.downszie = downszie;
            this.starttime = starttime;
            this.title = title;
            this.endtime = endTime;
            this.uid = uid;
            this.filePath = filePath;
            this.fileName = fileName;
        }
    }
}
