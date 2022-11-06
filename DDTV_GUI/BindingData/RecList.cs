using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.BindingData
{
    public class RecList: INotifyPropertyChanged
    {
        private string name;
        private int roomid;
        private string downszie;
        private string starttime;
        private string title;
        private long uid;
        private string filePath;
        private string downloadSpe;
        private string cdn_host;
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
        public int RoomId
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
        public string DownloadSpe
        {
            get
            {
                return downloadSpe;
            }
            set
            {
                downloadSpe = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string Cdn_Host
        {
            get
            {
                return cdn_host;
            }
            set
            {
                cdn_host = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public RecList(string name, int roomid, string downszie, string starttime, string title,long uid,string filePath,string downloadSpe,string cdn_host)
        {
            this.name = name;
            this.roomid = roomid;
            this.downszie = roomid!=0?String.IsNullOrEmpty(downszie)?"连接中": downszie:"";
            this.starttime = starttime;
            this.title = title;
            this.uid = uid;
            this.filePath = filePath;
            this.downloadSpe = downloadSpe;
            this.cdn_host = cdn_host;
        }
    }
}
