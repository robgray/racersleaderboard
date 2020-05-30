using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using RacersLeaderboard.Core.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class LeaderboardTableBuilder : ITableBuilder
	{
        private List<DriverStats> _driverStats;

        public LeaderboardTableBuilder(List<DriverStats> driverStats)
        {
            _driverStats = driverStats;
        }

        public ImageCreator Create()
        {
			const float COL_RANK = 5;
			const float COL_NAME = 40;
			const float COL_IRATING = 250;
			const float COL_SAFETYRATING = 325;

			const float LINE_HEIGHT = 30;

			int height = Convert.ToInt32(LINE_HEIGHT * _driverStats.Count() + LINE_HEIGHT + (LINE_HEIGHT / 1.8));
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
				g.DrawString("iRating", font, Brushes.White, COL_IRATING, y);
				g.DrawString("SR", font, Brushes.White, COL_SAFETYRATING, y);

				for (var i = 0; i < _driverStats.Count(); i++)
				{
					y += LINE_HEIGHT;

					var driver = _driverStats[i];
					g.DrawString((i + 1).ToString(), font, Brushes.Black, COL_RANK, y);
					g.DrawString(driver.Driver, font, Brushes.Black, COL_NAME, y);
					g.DrawString(driver.iRatingText, font, Brushes.Black, COL_IRATING, y);

					var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));
					var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
					g.FillRectangle(licenseBrush, new RectangleF(COL_SAFETYRATING, LINE_HEIGHT + (i * LINE_HEIGHT), 75, LINE_HEIGHT));
					g.DrawString($"{driver.Class}", font, licenseBrushText, COL_SAFETYRATING + 10, y);
				}

				// Draw Footer 
				var smallFont = new Font("Verdana", 6);
				y += LINE_HEIGHT;
				g.DrawString($"Rendered at {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallFont, Brushes.Black, COL_RANK, y);
				g.Flush();

				g.Flush();
			}

			return new ImageCreator(board);
		}
    }
}
