using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using RacersLeaderboard.Core.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class StatisticsTableBuilder : TableBuilder
	{
        private List<DriverStats> _driverStats;

        public StatisticsTableBuilder(List<DriverStats> driverStats)
        {
            _driverStats = driverStats;
        }

        public override ImageCreator Create()
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

            int height = GetHeight(_driverStats.Count);
			Bitmap board = new Bitmap(925, height, PixelFormat.Format32bppPArgb);

			using (var g = Graphics.FromImage((Image)board))
			{
                g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var atomicGreenBrush = new SolidBrush(ColorTranslator.FromHtml($"#00A900"));
				g.FillRectangle(Brushes.White, 0, 0, board.Width, board.Height);
				g.FillRectangle(atomicGreenBrush, 0, 0, board.Width, LINE_HEIGHT);

                float y = 6;
				g.DrawString("Name", Font, Brushes.White, COL_NAME, y);
				g.DrawString("iRating", Font, Brushes.White, COL_IRATING, y);
				g.DrawString("SR", Font, Brushes.White, COL_SAFETYRATING, y);
				g.DrawString("Starts", Font, Brushes.White, COL_STARTS, y);
				g.DrawString("Wins", Font, Brushes.White, COL_WINS, y);
				g.DrawString("Wins Rate", Font, Brushes.White, COL_WINRATE, y);
				g.DrawString("Pts / Race", Font, Brushes.White, COL_POINTS_PER_RACE, y);
				g.DrawString("Avg. Inc.", Font, Brushes.White, COL_AVG_INC, y);
				g.DrawString("ttRating", Font, Brushes.White, COL_TTRATING, y);
				g.DrawString("Laps Lead", Font, Brushes.White, COL_LAPS_LEAD, y);
				
				for (var i = 0; i < _driverStats.Count(); i++)
				{
					var driver = _driverStats[i];
                    y += LINE_HEIGHT;
					g.DrawString((i + 1).ToString(), Font, Brushes.Black, COL_RANK, y);
					g.DrawString(driver.Driver, Font, Brushes.Black, COL_NAME, y);
					g.DrawString(driver.iRatingText, Font, Brushes.Black, COL_IRATING, y);

					var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));
					var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
					g.FillRectangle(licenseBrush, new RectangleF(COL_SAFETYRATING, LINE_HEIGHT + (i * LINE_HEIGHT), 65, LINE_HEIGHT));
					g.DrawString(driver.Class, Font, licenseBrushText, COL_SAFETYRATING + 10, y);
					g.DrawString(driver.Starts.ToString(), Font, Brushes.Black, COL_STARTS, y);
					g.DrawString(driver.Wins.ToString(), Font, Brushes.Black, COL_WINS, y);
					g.DrawString(driver.WinRate.ToString("P"), Font, Brushes.Black, COL_WINRATE, y);
					g.DrawString(driver.AvgPointsPerRace.ToString(), Font, Brushes.Black, COL_POINTS_PER_RACE, y);
					g.DrawString(driver.AverageIncidents.ToString("F"), Font, Brushes.Black, COL_AVG_INC, y);
					g.DrawString(driver.ttRating.ToString(), Font, Brushes.Black, COL_TTRATING, y);
					g.DrawString(driver.LapsLead.ToString(), Font, Brushes.Black, COL_LAPS_LEAD, y); ;
				}

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
