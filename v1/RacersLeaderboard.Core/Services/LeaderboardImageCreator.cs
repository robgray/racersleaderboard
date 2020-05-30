using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using RacersLeaderboard.Core.Models;

namespace RacersLeaderboard.Core.Services
{
    public interface ILeaderboardImageCreator
    {
        Image CreateLeaderboad(List<DriverStats> drivers);
        Image CreateStatsTable(List<DriverStats> drivers);
        Image CreateSeasonTable(List<SeasonStanding> standings);
        Image CreateTimeTrialLeaderboad(List<TimeTrialLeaderboard.TimeTrialItem> timeTrials, Func<string,string> urlDecoder);
    }

    public class LeaderboardImageCreator : ILeaderboardImageCreator
    {
        public Image CreateLeaderboad(List<DriverStats> drivers)
		{
			const float COL_RANK = 5;
			const float COL_NAME = 40;
			const float COL_IRATING = 250;
			const float COL_SAFETYRATING = 325;

			const float LINE_HEIGHT = 30;

			int height = Convert.ToInt32(LINE_HEIGHT * drivers.Count() + LINE_HEIGHT + (LINE_HEIGHT / 1.8));
			Bitmap board = new Bitmap(400, height, PixelFormat.Format32bppPArgb);

			using (var g = Graphics.FromImage((Image) board))
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

				for (var i = 0; i < drivers.Count(); i++)
				{
				    y += LINE_HEIGHT;

                    var driver = drivers[i];					
					g.DrawString((i+1).ToString(), font, Brushes.Black, COL_RANK, y);
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

            return CreatePngImageFromBitmap(board);
		}

		public Image CreateStatsTable(List<DriverStats> drivers)
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

			drivers = drivers.OrderByDescending(driver => driver.iRating).ThenByDescending(driver => driver.AvgPointsPerRace).ToList();

			int height = Convert.ToInt32(LINE_HEIGHT * drivers.Count() + LINE_HEIGHT);
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

				for (var i = 0; i < drivers.Count(); i++)
				{
					var driver = drivers[i];
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

            return CreatePngImageFromBitmap(board);
		}

		public Image CreateSeasonTable(List<SeasonStanding> standings)
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

            const float LINE_HEIGHT = 30;

			standings = standings.OrderByDescending(driver => driver.Points).ThenByDescending(driver => driver.AvgFinish).ToList();

			int height = Convert.ToInt32(LINE_HEIGHT * standings.Count() + LINE_HEIGHT + (LINE_HEIGHT / 1.8));
			Bitmap board = new Bitmap(740, height, PixelFormat.Format32bppPArgb);

			using (var g = Graphics.FromImage((Image)board))
			{
				var font = new Font("Verdana", 10);

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				var atomicGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#00A900"));
				g.FillRectangle(Brushes.White, 0, 0, board.Width, board.Height);
				g.FillRectangle(atomicGreenBrush, 0, 0, board.Width, LINE_HEIGHT);

			    float y = 6;
				g.DrawString("Name", font, Brushes.White, COL_NAME, y);				
				g.DrawString("Starts", font, Brushes.White, COL_STARTS, y);
				g.DrawString("Wins", font, Brushes.White, COL_WINS, y);
				g.DrawString("Points", font, Brushes.White, COL_POINTS, y);				
                g.DrawString("Div.", font, Brushes.White, COL_DIVISION, y);
                g.DrawString("Div. Pos.", font, Brushes.White, COL_DIVISION_POSITION, y);
                g.DrawString("Div. %", font, Brushes.White, COL_DIVISION_PERCENT, y);
                g.DrawString("Top %", font, Brushes.White, COL_OVERALL, y);

                for (var i = 0; i < standings.Count(); i++)
				{
				    y += LINE_HEIGHT;

                    var driver = standings[i];					
				    g.DrawString($"{i + 1}", font, Brushes.Black, COL_RANK, y);
                    g.DrawString(driver.Name, font, Brushes.Black, COL_NAME, y);															
					g.DrawString(driver.Starts.ToString(), font, Brushes.Black, COL_STARTS, y);
					g.DrawString(driver.Wins.ToString(), font, Brushes.Black, COL_WINS, y);
					g.DrawString(driver.Points.ToString(), font, Brushes.Black, COL_POINTS, y);
                    g.DrawString(driver.Division.ToString(), font, Brushes.Black, COL_DIVISION + 10, y);
                    g.DrawString($"{driver.DivisionPosition}", font, Brushes.Black, COL_DIVISION_POSITION + 20, y);
                    g.DrawString((driver.DivisionPosition / Convert.ToDecimal(driver.DriversInDivision)).ToString("P"), font, Brushes.Black, COL_DIVISION_PERCENT, y);
                    g.DrawString((driver.Position / Convert.ToDecimal(driver.TotalDrivers)).ToString("P"), font, Brushes.Black, COL_OVERALL, y);
                }


			    // Draw Footer 
			    var smallFont = new Font("Verdana", 6);
			    y += LINE_HEIGHT;
			    g.DrawString($"Rendered at {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallFont, Brushes.Black, COL_RANK, y);
                g.Flush();
			}

            return CreatePngImageFromBitmap(board);
		}

	    public Image CreateTimeTrialLeaderboad(List<TimeTrialLeaderboard.TimeTrialItem> timeTrials, Func<string,string> urlDecoder)
	    {
	        const float COL_RANK = 5;
	        const float COL_NAME = 40;
	        const float COL_DIVISION = 250;
	        const float COL_TTPOINTS = 325;

	        const float LINE_HEIGHT = 30;

	        int height = Convert.ToInt32(LINE_HEIGHT * timeTrials.Count() + LINE_HEIGHT + (LINE_HEIGHT / 1.8));
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
                
	            for (var i = 0; i < timeTrials.Count(); i++)
	            {
	                y += LINE_HEIGHT;

                    var driver = timeTrials[i];	                
	                g.DrawString((i + 1).ToString(), font, Brushes.Black, COL_RANK, y);
	                g.DrawString(urlDecoder(driver.DisplayName), font, Brushes.Black, COL_NAME, y);
	                g.DrawString(driver.Division.ToString(), font, Brushes.Black, COL_DIVISION, y);	                
	                g.DrawString($"{driver.Points:F0}", font, Brushes.Black, COL_TTPOINTS + 10, y);
	            }

                // Draw Footer 
	            var smallFont = new Font("Verdana", 6);
	            y += LINE_HEIGHT;
                g.DrawString($"Rendered at {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallFont, Brushes.Black, COL_RANK, y);
                
	            g.Flush();
	        }

	        return CreatePngImageFromBitmap(board);
        }

        private Image CreatePngImageFromBitmap(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Seek(0, 0);

                return Image.FromStream(stream);
            }
        }

	}
}