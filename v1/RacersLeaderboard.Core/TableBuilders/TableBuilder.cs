using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Microsoft.Azure.Storage.Blob.Protocol;

namespace RacersLeaderboard.Core.TableBuilders
{
    public abstract class TableBuilder : ITableBuilder
    {
        protected const float LINE_HEIGHT = 30;
        protected Font Font { get; set; }
        
        protected TableBuilder()
        {
            Font = new Font("Verdana", 10);
        }

        public abstract ImageCreator Create();

        protected int GetHeight(int numberOfLines) 
            => Convert.ToInt32(LINE_HEIGHT * numberOfLines + LINE_HEIGHT + (LINE_HEIGHT / 1.8));

    }
}
