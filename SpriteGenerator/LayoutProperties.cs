using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteGenerator
{
    class LayoutProperties
    {
        public string[] InputFilePaths { get; set; } 
        public string OutputSpriteFilePath { get; set; }
        public string OutputCssFilePath { get; set; }
        public string Layout { get; set; }
        public int DistanceBetweenImages { get; set; } 
        public int MarginWidth { get; set; }
        public int ImagesInRow { get; set; }
        public int ImagesInColumn { get; set; }
        public string CssPrefix { get; set; }

        public LayoutProperties()
        {
            CssPrefix = "sprite-";
            InputFilePaths = null;
            OutputSpriteFilePath = "";
            OutputCssFilePath = "";
            Layout = "";
            DistanceBetweenImages = 0;
            MarginWidth = 0;
            ImagesInRow = 0;
            ImagesInColumn = 0;
        }
    }
}
