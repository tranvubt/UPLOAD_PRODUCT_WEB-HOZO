using System;
using System.Collections.Generic;
using System.Text;

namespace ToolList_Ver1.Class
{
    class image
    {
        public string name { get; set; }
        public string filePath { get; set; }
        public bool Equals(image other)
        {
            if (this.name.Equals(other.name))
                return true;
            return false;
        }

        public image()
        {
        }

        public image(string name)
        {
            this.name = name;
        }
    }
}
