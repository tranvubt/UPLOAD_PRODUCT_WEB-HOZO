using System;

namespace ToolList_Ver1.Class
{
    class DataWTM : IEquatable<DataWTM>
    {
        public string FileCode { set; get; }
        public string Title { set; get; }
        private string _description;
        private string price;
        private string categories;
        public string discounted_price { set; get; }

        public string Price
        {
            get
            {
                return price;
            }
            set { price = value; }
        }

        public string Categories
        {
            get
            {
                return categories;
            }
            set { categories = value; }
        }
        public string Description
        {
            get
            {
                return _description;
            }
            set { _description = value; }
        }
        public string Tags { set; get; }
        public DataWTM(string id, string title, string tag,string price, string discounted_price, string categories, string description)
        {
            this.FileCode = id;
            this.Title = title;
            this._description = description;
            this.Tags = tag;
            this.price = price;
            this.categories = categories;
            this.discounted_price = discounted_price;
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
