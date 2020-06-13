using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using RacersLeaderboard.Core.Services.iRacing.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class TimeTrialTableBuilder : TableBuilder
    {
        private List<TimeTrialLeaderboard.TimeTrialItem> _timeTrials;
        private Func<string, string> _urlDecoder = (_) => _;
        public TimeTrialTableBuilder(List<TimeTrialLeaderboard.TimeTrialItem> timeTrials)
        {
            _timeTrials = timeTrials;
        }

        public override ImageCreator Create()
        {
			const float COL_RANK = 5;
            const float COL_NAME = 40;
            const float COL_DIVISION = 250;
            const float COL_TTPOINTS = 325;

            const float LINE_HEIGHT = 30;

            int height = Convert.ToInt32(LINE_HEIGHT * _timeTrials.Count() + LINE_HEIGHT + (LINE_HEIGHT / 1.8));
            Bitmap board = new Bitmap(400, height, PixelFormat.Format32bppPArgb);

            using (var g = Graphics.FromImage((Image)board))
            {
                var font = new Font("Verdana", 10);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var atomicGreenBrush = new SolidBrush(ColorTranslator.FromHtml($"#00A900"));
                g.FillRectangle(Brushes.White, 0, 0, board.Width, board.Height);
                g.FillRectangle(atomicGreenBrush, 0, 0, board.Width, LINE_HEIGHT);
                
                float y = 6;
                g.DrawString("Name", font, Brushes.White, COL_NAME, y);
                g.DrawString("Division", font, Brushes.White, COL_DIVISION, y);
                g.DrawString("Points", font, Brushes.White, COL_TTPOINTS, y);

                for (var i = 0; i < _timeTrials.Count(); i++)
                {
                    y += LINE_HEIGHT;

                    var driver = _timeTrials[i];
                    g.DrawString((i + 1).ToString(), font, Brushes.Black, COL_RANK, y);
                    g.DrawString(_urlDecoder(driver.DisplayName), font, Brushes.Black, COL_NAME, y);
                    g.DrawString(driver.Division.ToString(), font, Brushes.Black, COL_DIVISION, y);
                    g.DrawString($"{driver.Points:F0}", font, Brushes.Black, COL_TTPOINTS + 10, y);
                }

                // Draw Footer 
                var smallFont = new Font("Verdana", 6);
                y += LINE_HEIGHT;
                g.DrawString($"Rendered at {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallFont, Brushes.Black, COL_RANK, y);

                g.Flush();
            }

            return new ImageCreator(board);
		}
    }
}
