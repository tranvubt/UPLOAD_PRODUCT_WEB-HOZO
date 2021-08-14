﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ToolList_Ver1.Class
{
    class WTM
    {
        public string FileCode { set; get; }
        public string filePath { set; get; }
        public DataWTM data { set; get; }
        public List<image> lstImage = new List<image>();
        public void setDataWTM(System.Collections.SortedList h)
        {
            foreach (System.Collections.DictionaryEntry item in h)
            {
                if (item.Key.ToString().Contains(FileCode))
                    data = (DataWTM)item.Value;
            }
        }
        public void getListImg()
        {
            foreach (string d in System.IO.Directory.GetFiles(filePath))
            {
                image temp = new image();
                if (System.Text.RegularExpressions.Regex.IsMatch(d, @".png"))
                {
                    temp.filePath = d;
                    temp.name = System.IO.Path.GetFileName(d);
                    lstImage.Add(temp);
                }
            }
        }
    }
}
