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
                return "👑 PLEASE READ THE DESCRIPTION 👑\n👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇👇\nMY PAYMENT METHOD IS UPDATING.....PLEASE VISIT MY OTHER SHOP TO ORDER FOR 90% OFF" +
                  "\nONLY $2.99 WHEN PURCHASING ON OUR WEBSITE. (Only Today !!)." +
                  "\nWe accept all payment method." +
                  "\nLink bellow:" + _description +
                  "\n90% OFF (Only Today !!)" +
                  "\n* NOTE:" +
                  "\nDO NOT ORDER AT THIS ETSY SHOP, ALL ORDERS AT THIS SHOP WILL BE CANCELED." +
                  "\nPLEASE TO CHECK YOUR EMAIL TO KNOW MORE INFORMATION!";
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
