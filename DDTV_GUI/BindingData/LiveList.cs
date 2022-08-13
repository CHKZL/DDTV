using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDTV_GUI.BindingData
{
    public class LiveList : INotifyPropertyChanged
    {
        private string name;
        private string state;
        private int liveState;
        private string isremind;
        private string isrec;
        private int roomid;
        private long uid;
        private string isdanmu;
        private string title;
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
        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string IsRemind
        {
            get
            {
                return isremind;
            }
            set
            {
                isremind = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string IsRec
        {
            get
            {
                return isrec;
            }
            set
            {
                isrec = value;
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
        public int LiveState
        {
            get
            {
                return liveState;
            }
            set
            {
                liveState = value;
                if (this.PropertyChanged != null)//激发事件，参数为Age属性  
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Age"));
                }
            }
        }
        public string IsDanmu
        {
            get
            {
                return isdanmu;
            }
            set
            {
                isdanmu = value;
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
        
        public LiveList(string name, string state, string isremind, string isrec, int roomid, long uid,int liveState,string isdanmu,string title)
        {
            this.name = name;
            this.state = state;
            this.isremind = isremind;
            this.isrec = isrec;
            this.roomid = roomid;
            this.uid = uid;
            this.liveState = liveState;
            this.isdanmu=isdanmu;
            this.title = title;
        }
    }
}
