using System;

namespace DXSample21
{
    public class Item
    {
        public Item(String caption, String path)
        {
            Caption = caption;
            OriginalPath = Path = path;
        }

        public String Caption { get; }

        public String OriginalPath { get; }

        public String Path { get; set; }
    }
}