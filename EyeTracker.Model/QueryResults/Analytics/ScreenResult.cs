﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyeTracker.Common.QueryResults.Analytics
{
    public class ScreenResult
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public string Path { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string FileExtension { get; set; }

        public Size Size { get { return new Size(this.Width, this.Height); } }
    }
}
