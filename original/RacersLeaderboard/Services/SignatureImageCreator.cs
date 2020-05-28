using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using RacersLeaderboard.Models;

namespace RacersLeaderboard.Services
{
	public class SignatureImageCreator
	{
		private readonly HttpServerUtilityBase _serverUtility;		
		
		public SignatureImageCreator(HttpServerUtilityBase serverUtility)
		{
			_serverUtility = serverUtility;			
		}

	    public Image GetRoadBadge(DriverStats driver)
	    {
	        return GetBadge("asr-badge-road.png", driver);
	    }

	    public Image GetOvalBadge(DriverStats driver)
	    {
	        return GetBadge("asr-badge-oval.png", driver);
	    }

	    public Image GetDirtRoadBadge(DriverStats driver)
	    {
	        return GetBadge("asr-badge-road-dirt.png", driver);
	    }

	    public Image GetDirtOvalBadge(DriverStats driver)
	    {
	        return GetBadge("asr-badge-oval-dirt.png", driver);
	    }

        public Image GetBadge(string filename, DriverStats driver)
	    {
	        var badgeBaseImage = Image.FromFile(_serverUtility.MapPath($"~/img/{filename}"));

	        using (var g = Graphics.FromImage((Image)badgeBaseImage))
	        {
	            string fontName = "Verdana";

	            var font = new Font(fontName, 10, FontStyle.Bold);
	            StringFormat sf = new StringFormat()
	            {
	                LineAlignment = StringAlignment.Center,
	                Alignment = StringAlignment.Center
	            };

                g.SmoothingMode = SmoothingMode.AntiAlias;
	            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
	            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

	            g.DrawString(driver.iRatingText, font, Brushes.Black, new RectangleF(140, 0, 65, 20), sf);
	            
	            var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));
	            var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
	            
	            g.FillRectangle(licenseBrush, new RectangleF(140, 20, 65, 20));	            	            	            
	            g.DrawString($"{driver.Class}", font, licenseBrushText, new RectangleF(140, 20, 65, 20), sf);

	            g.Flush();
	        }

	        return badgeBaseImage;

        }

	    public Image GetRoadSignature(DriverStats driver)
		{
			string signatureTemplate = driver.GetSignatureTemplate();
			
			var signature = Image.FromFile(_serverUtility.MapPath($"~/img/{signatureTemplate}"));

			using (var g = Graphics.FromImage((Image)signature))
			{
				string fontName = "Verdana";

				var font = new Font(fontName, 15, FontStyle.Bold);

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				// Draw License Colour & Name
				var greenBrush = new SolidBrush(ColorTranslator.FromHtml($"#00A900"));
				g.FillRectangle(greenBrush, 0, 0, 588, 1);
				g.FillRectangle(greenBrush, 0, 49, 588, 1);
				g.FillRectangle(greenBrush, 0, 0, 1, 50);
				g.DrawString(driver.Driver, font, Brushes.Black, new RectangleF(8, 8, 185, 20));

				var fontBoxHeading = new Font(fontName, 10);
				g.DrawString("iRating", fontBoxHeading, Brushes.White, new RectangleF(205, 6, 45, 12));

				font = new Font(fontName, 13);
			    var smallFont = new Font(fontName, 10);
                g.DrawString(driver.iRatingText, font, Brushes.Black, new RectangleF(205, 22, 60, 25));
                //g.DrawString(cardData.License.Replace("Class", "").Trim(), font, Brushes.Black, new RectangleF(267, 22, 15, 25));
                var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));			    
                var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
                var urlBrushText = new SolidBrush(ColorTranslator.FromHtml("#949494"));
                g.FillRectangle(licenseBrush, new RectangleF(263, 18, 50, 23));
                g.DrawString($"{driver.Class}", font, licenseBrushText, new RectangleF(264, 22, 80, 25));

			    g.DrawString("atomicsimracing.net", smallFont, urlBrushText, new RectangleF(8, 35, 200, 20));

				g.Flush();
			}

			return signature;
		}

		public Image GetRoadMiniSignature(DriverStats driver, string signatureImagepng = "minisig.png", string hexColour = "#00A900")
		{			
			var signature = Image.FromFile(_serverUtility.MapPath($"~/img/{signatureImagepng}"));

			using (var g = Graphics.FromImage((Image)signature))
			{
				string fontName = "Verdana";

				var font = new Font(fontName, 8, FontStyle.Bold);

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
				// Draw License Colour & Name
				var boxBrush = new SolidBrush(ColorTranslator.FromHtml(hexColour));				
				g.DrawString(driver.Driver, font, Brushes.Black, new RectangleF(85,3, 150, 14));
				
				//font = new Font(fontName, 8);

				g.FillRectangle(boxBrush, new RectangleF(228, 0, 23, 20));
				g.DrawString("iR", font, Brushes.White, new RectangleF(232,3,20,14));
				g.DrawString(driver.iRatingText, font, Brushes.Black, new RectangleF(255, 3, 60, 14));
				var licenseBrush = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColor}"));
				var licenseBrushText = new SolidBrush(ColorTranslator.FromHtml($"#{driver.LicenseColorForeground}"));
				g.FillRectangle(licenseBrush, new RectangleF(300,0, 51, 20));				
				g.DrawString($"{driver.Class}", font, licenseBrushText, new RectangleF(305, 3, 45, 14));

				g.Flush();
			}

			return signature;
		}

	    public Image GetRoadMiniSrrSignature(DriverStats driver)
	    {
	        return GetRoadMiniSignature(driver, "minisig-srr.png", "#234A70");
	    }
        
        public Image GetOvalSignature(DriverInfo driver)
		{
			string signatureTemplate = driver.OvalLicense.GetSignatureTemplate();

			var signature = Image.FromFile(_serverUtility.MapPath($"~/img/{signatureTemplate}"));

			using (var g = Graphics.FromImage((Image)signature))
			{
				string fontName = "Verdana";

				var font = new Font(fontName, 15, FontStyle.Bold);

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				// Draw License Colour & Name
				var greenBrush = new SolidBrush(ColorTranslator.FromHtml($"#00A900"));
				g.FillRectangle(greenBrush, 0, 0, 588, 1);
				g.FillRectangle(greenBrush, 0, 49, 588, 1);
				g.FillRectangle(greenBrush, 0, 0, 1, 50);
				g.DrawString(driver.displayName, font, Brushes.Black, new RectangleF(8, 15, 185, 20));

				var fontBoxHeading = new Font(fontName, 10);
				g.DrawString("iRating", fontBoxHeading, Brushes.White, new RectangleF(205, 6, 45, 12));

				font = new Font(fontName, 13);
				g.DrawString(driver.OvalLicense.iRating, font, Brushes.Black, new RectangleF(205, 22, 60, 25));
				//g.DrawString(cardData.License.Replace("Class", "").Trim(), font, Brushes.Black, new RectangleF(267, 22, 15, 25));
				g.DrawString($"{driver.OvalLicense.srPrime}.{driver.OvalLicense.srSub}", font, Brushes.Black, new RectangleF(270, 22, 80, 25));

				g.Flush();
			}

			return signature;
		}
	}
}