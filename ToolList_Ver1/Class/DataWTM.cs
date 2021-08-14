using System;
using System.Collections.Generic;
using System.Text;

namespace ToolList_Ver1.Class
{
    class DataWTM : IEquatable<DataWTM>
    {
        public string FileCode { set; get; }
        public string Title { set; get; }
        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set { _description = value; }
        }
        public string Tags { set; get; }
        public DataWTM(string id, string title, string description, string tag)
        {
            this.FileCode = id;
            this.Title = title;
            this._description = description;
            this.Tags = tag;
        }
        public DataWTM()
        {
        }
        public bool Equals(DataWTM other)
        {
            if (this.FileCode.Equals(other.FileCode))
                return true;
            return false;
        }
    }
}
