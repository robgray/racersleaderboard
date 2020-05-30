using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using RacersLeaderboard.Core.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class StatisticsTableBuilder : ITableBuilder
	{
        private List<DriverStats> _driverStats;

        public StatisticsTableBuilder(List<DriverStats> driverStats)
        {
            _driverStats = driverStats;
        }

        public ImageCreator Create()
        {
			const float COL_RANK = 5;
			const float COL_NAME = 40;
			const float COL_IRATING = 250;
			const float COL_SAFETYRATING = 325;
			const float COL_STARTS = 400;
			const float COL_WINS = 475;
			const float COL_WINRATE = 550;
			const float COL_POINTS_PER_RACE = 625;
			const float COL_AVG_INC = 700;
			const float COL_TTRATING = 775;
			const float COL_LAPS_LEAD = 850;

			const float LINE_HEIGHT = 30;

            _driverStats = _driverStats.OrderByDescending(driver => driver.iRating).ThenByDescending(driver => driver.AvgPointsPerRace).ToList();

			int height = Convert.ToInt32(LINE_HEIGHT * _driverStats.Count() + LINE_HEIGHT);
			Bitmap board = new Bitmap(925, height, PixelFormat.Format32bppPArgb);

			using (var g = Graphics.FromImage((Image)board))
			{
				var font = new Font("Verdana", 10);

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var atomicGreenBrush = new SolidBrush(ColorTranslator.FromHtml($"#00A900"));
				g.FillRectangle(Brushes.White, 0, 0, board.Width, board.Height);
				g.FillRectangle(atomicGreenBrush, 0, 0, board.Width, LINE_HEIGHT);


				g.DrawString("Name", font, Brushes.White, COL_NAME, 6);
				g.DrawString("iRating", font, Brushes.White, COL_IRATING, 6);
				g.DrawString("SR", font, Brushes.White, COL_SAFETYRATING, 6);
				g.DrawString("Starts", font, Brushes.White, COL_STARTS, 6);
				g.DrawString("Wins", font, Brushes.White, COL_WINS, 6);
				g.DrawString("Wins Rate", font, Brushes.White, COL_WINRATE, 6);
				g.DrawString("Pts / Race", font, Brushes.White, COL_POINTS_PER_RACE, 6);
				g.DrawString("Avg. Inc.", font, Brushes.White, COL_AVG_INC, 6);
				g.DrawString("ttRating", font, Brushes.White, COL_TTRATING, 6);
				g.DrawString("Laps Lead", font, Brushes.White, COL_LAPS_LEAD, 6);

				for (var i = 0; i < _driverStats.Count(); i++)
				{
					var driver = _driverStats[i];
					float y = LINE_HEIGHT + (i * LINE_HEIGHT) + 6;
					g.DrawString((i + 1).ToString(), font, Brushes.Black, COL_RANK, y);
					g.DrawString(driver.Driver, font, Brushes.Black, COL_NAME, y);
					g.DrawString(driver.iRatingText, font, Brushes.Black, COL_IRATING, y);

					var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));
					var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
					g.FillRectangle(licenseBrush, new RectangleF(COL_SAFETYRATING, LINE_HEIGHT + (i * LINE_HEIGHT), 65, LINE_HEIGHT));
					g.DrawString(driver.Class, font, licenseBrushText, COL_SAFETYRATING + 10, y);
					g.DrawString(driver.Starts.ToString(), font, Brushes.Black, COL_STARTS, y);
					g.DrawString(driver.Wins.ToString(), font, Brushes.Black, COL_WINS, y);
					g.DrawString(driver.WinRate.ToString("P"), font, Brushes.Black, COL_WINRATE, y);
					g.DrawString(driver.AvgPointsPerRace.ToString(), font, Brushes.Black, COL_POINTS_PER_RACE, y);
					g.DrawString(driver.AverageIncidents.ToString("F"), font, Brushes.Black, COL_AVG_INC, y);
					g.DrawString(driver.ttRating.ToString(), font, Brushes.Black, COL_TTRATING, y);
					g.DrawString(driver.LapsLead.ToString(), font, Brushes.Black, COL_LAPS_LEAD, y); ;
				}

				g.Flush();
			}

			return new ImageCreator(board);
		}
    }
}
