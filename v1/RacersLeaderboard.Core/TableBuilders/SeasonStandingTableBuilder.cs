using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using RacersLeaderboard.Core.Models;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class SeasonStandingTableBuilder : TableBuilder
    {
        private List<SeasonStanding> _data;
        public SeasonStandingTableBuilder(List<SeasonStanding> data)
        {
            _data = data;
        }

        public override ImageCreator Create()
        {
			const float COL_RANK = 5;
			const float COL_NAME = 40;
			const float COL_DIVISION = 250;
			const float COL_DIVISION_POSITION = 290;
			const float COL_POINTS = 365;
			const float COL_STARTS = 440;
			const float COL_WINS = 515;
			const float COL_DIVISION_PERCENT = 590;
			const float COL_OVERALL = 665;
			
            _data = _data.OrderByDescending(driver => driver.Points).ThenByDescending(driver => driver.AvgFinish).ToList();
			
            int height = GetHeight(_data.Count());
			Bitmap board = new Bitmap(740, height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage((Image)board))
			{
                g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var atomicGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#00A900"));
				g.FillRectangle(Brushes.White, 0, 0, board.Width, board.Height);
				g.FillRectangle(atomicGreenBrush, 0, 0, board.Width, LINE_HEIGHT);

				float y = 6;
				g.DrawString("Name", Font, Brushes.White, COL_NAME, y);
				g.DrawString("Starts", Font, Brushes.White, COL_STARTS, y);
				g.DrawString("Wins", Font, Brushes.White, COL_WINS, y);
				g.DrawString("Points", Font, Brushes.White, COL_POINTS, y);
				g.DrawString("Div.", Font, Brushes.White, COL_DIVISION, y);
				g.DrawString("Div. Pos.", Font, Brushes.White, COL_DIVISION_POSITION, y);
				g.DrawString("Div. %", Font, Brushes.White, COL_DIVISION_PERCENT, y);
				g.DrawString("Top %", Font, Brushes.White, COL_OVERALL, y);

				for (var i = 0; i < _data.Count(); i++)
				{
					y += LINE_HEIGHT;

					var driver = _data.ElementAt(i);
					g.DrawString($"{i + 1}", Font, Brushes.Black, COL_RANK, y);
					g.DrawString(driver.Name, Font, Brushes.Black, COL_NAME, y);
					g.DrawString(driver.Starts.ToString(), Font, Brushes.Black, COL_STARTS, y);
					g.DrawString(driver.Wins.ToString(), Font, Brushes.Black, COL_WINS, y);
					g.DrawString(driver.Points.ToString(), Font, Brushes.Black, COL_POINTS, y);
					g.DrawString(driver.Division.ToString(), Font, Brushes.Black, COL_DIVISION + 10, y);
					g.DrawString($"{driver.DivisionPosition}", Font, Brushes.Black, COL_DIVISION_POSITION + 20, y);
					g.DrawString((driver.DivisionPosition / Convert.ToDecimal(driver.DriversInDivision)).ToString("P"), Font, Brushes.Black, COL_DIVISION_PERCENT, y);
					g.DrawString((driver.Position / Convert.ToDecimal(driver.TotalDrivers)).ToString("P"), Font, Brushes.Black, COL_OVERALL, y);
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
