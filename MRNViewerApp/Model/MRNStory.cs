using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketDataWebSocket.Models.Data;

namespace MRNWebsocketViewer.Model
{
    public partial class MrnStory : INotifyPropertyChanged
    {

        private int _index;
        private DateTime _timeStamp;
        private StoryData _storyData;
        private string _guid;
        private string _source;
        private int _fragNum;
        private long _totsize;
       
        public int Index
        {
            get => this._index;
            set
            {
                this._index = value;
                this.OnPropertyChanged();
            }
        }
        public int Frag_Num
        {
            get => this._fragNum;
            set
            {
                this._fragNum = value;
                this.OnPropertyChanged();
            }
        }
        public long Tot_Size
        {
            get => this._totsize;
            set
            {
                this._totsize = value;
                this.OnPropertyChanged();
            }
        }
        public DateTime TimeStamp
        {
            get => this._timeStamp;

            set
            {
                this._timeStamp = value;
                this.OnPropertyChanged();
            }
        }
        public string MrnSource
        {
            get => this._source;
            set
            {
                this._source = value;
                this.OnPropertyChanged();
            }
        }

        public StoryData Story
        {
            get => this._storyData;
            set
            {
                this._storyData = value;
                this.OnPropertyChanged();
            }
        }
        public string GUID
        {
            get => this._guid;
            set
            {
                this._guid = value;
                this.OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

      
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
