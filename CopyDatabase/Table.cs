using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CopyDatabase
{
    public class Table : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this.TABLE_NAME;
        }

        public string TABLE_NAME { get; set; }
        public List<Columns> Columns { get; set; }
        public List<Table> References { get; set; }

        private bool _IsChecked;

        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (_IsChecked == value) return;
                _IsChecked = value;
                this.OnPropertyChanged("IsChecked");
            }
        }

        private Int64 _Row_count;

        public Int64 Row_count
        {
            get { return _Row_count; }
            set
            {
                if (_Row_count == value) return;
                _Row_count = value;
                this.OnPropertyChanged("Row_count");
            }
        }
        
        private bool _IsTrans = false;

        public bool IsTrans
        {
            get { return _IsTrans; }
            set
            {
                if (_IsTrans == value) return;
                _IsTrans = value;
                this.OnPropertyChanged("IsTrans");
            }
        }

        private bool _isExecuting;

        public bool Executing
        {
            get { return _isExecuting; }
            set
            {
                if (_isExecuting == value) return;
                _isExecuting = value;
                this.OnPropertyChanged("Executing");
            }
        }
        
        private int _percent;

        public int Percent
        {
            get { return _percent; }
            set 
            {
                if (_percent == value) return;
                _percent = value;
                this.OnPropertyChanged("Percent");
            }
        }

        private int _count;

        public int Count
        {
            get { return _count; }
            set
            {
                if (_count == value) return;
                _count = value;
                this.OnPropertyChanged("Count");
            }
        }

        private int _count_current;

        public int Count_Current
        {
            get { return _count_current; }
            set
            {
                if (_count_current == value) return;
                _count_current = value;
                this.OnPropertyChanged("Count_Current");
            }
        }

        public Table()
        {
            this.Columns = new List<Columns>();
            this.References = new List<Table>();
        }

        private bool _error;

        public bool Error
        {
            get { return _error; }
            set
            {
                if (_error == value) return;
                _error = value;
                this.OnPropertyChanged("Error");
            }
        }
    }
}