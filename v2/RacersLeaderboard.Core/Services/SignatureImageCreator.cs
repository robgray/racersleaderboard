using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using RacersLeaderboard.Core.Configuration;
using RacersLeaderboard.Core.Services.iRacing.Models;
using RacersLeaderboard.Core.Storage;
using RacersLeaderboard.Core.TableBuilders;

namespace RacersLeaderboard.Core.Services
{
    public interface ISignatureImageCreator
    {
        Task<Image> GetRoadBadge(DriverStats driver);
		Task<Image>  GetOvalBadge(DriverStats driver);
		Task<Image> GetDirtRoadBadge(DriverStats driver);
		Task<Image> GetDirtOvalBadge(DriverStats driver);
		Task<Image> GetBadge(string filename, DriverStats driver);
		Task<Image> GetRoadSignature(DriverStats driver);
		Task<Image> GetRoadMiniSignature(DriverStats driver, string signatureImagepng = "minisig.png", string hexColour = "#00A900");
		Task<Image> GetRoadMiniSrrSignature(DriverStats driver);
		Task<Image> GetOvalSignature(DriverInfo driver);
    }

    public class SignatureImageCreator : ISignatureImageCreator
    {
        private IBlobStore _blobStore;

        public SignatureImageCreator(IBlobStore blobStore)
        {
            _blobStore = blobStore;
        }

	    public async Task<Image> GetRoadBadge(DriverStats driver)
	    {
	        return await GetBadge("asr-badge-road.png", driver);
	    }

	    public async Task<Image> GetOvalBadge(DriverStats driver)
	    {
	        return await GetBadge("asr-badge-oval.png", driver);
	    }

	    public async Task<Image> GetDirtRoadBadge(DriverStats driver)
	    {
	        return await GetBadge("asr-badge-road-dirt.png", driver);
	    }

	    public async Task<Image> GetDirtOvalBadge(DriverStats driver)
	    {
	        return await GetBadge("asr-badge-oval-dirt.png", driver);
	    }

        public async Task<Image> GetBadge(string filename, DriverStats driver)
        {
            var signature = await GetImageOrDefault(filename, 250, 40);
            
			using (var g = Graphics.FromImage(signature))
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

	        return signature;

        }

	    public async Task<Image> GetRoadSignature(DriverStats driver)
		{
			string signatureTemplate = driver.GetSignatureTemplate();
            var signature = await GetImageOrDefault(signatureTemplate, 575, 50);

			using (var g = Graphics.FromImage(signature))
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

			    g.DrawString($"{DateTime.Now}", smallFont, urlBrushText, new RectangleF(8, 35, 200, 20));

				g.Flush();
			}

			return signature;
		}

		public async Task<Image> GetRoadMiniSignature(DriverStats driver, string signatureImagepng = "minisig.png", string hexColour = "#00A900")
		{
            var signature = await GetImageOrDefault(signatureImagepng, 351, 40);
            using (var g = Graphics.FromImage(signature))
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

	    public async Task<Image> GetRoadMiniSrrSignature(DriverStats driver)
	    {
	        return await GetRoadMiniSignature(driver, "minisig-srr.png", "#234A70");
	    }
        
        public async Task<Image> GetOvalSignature(DriverInfo driver)
		{
			string signatureTemplate = driver.OvalLicense.GetSignatureTemplate();
			var signature = await GetImageOrDefault(signatureTemplate, 575, 40);

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
                g.DrawString($"{driver.OvalLicense.srPrime}.{driver.OvalLicense.srSub}", font, Brushes.Black, new RectangleF(270, 22, 80, 25));

				g.Flush();
			}

			return signature;
		}

        private Image CreateBaseImage(int width, int height)
        {
			var baseImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			using (var g = Graphics.FromImage(baseImage))
			{
				g.FillRectangle(Brushes.White, 0, 0, baseImage.Width, baseImage.Height);
				g.Flush();
			}

            return new ImageCreator(baseImage).Create();
        }

        private async Task<Image> GetImageOrDefault(string filename, int defaultWidth = 0, int defaultHeight = 0)
        {
            var exists = await _blobStore.BlobExists(StorageContainers.ImageContainer, $"templates/{filename}");
            if (exists)
            {
                var blob = await _blobStore.GetBlobReference(StorageContainers.ImageContainer, $"templates/{filename}");
                using (var imageStream = new MemoryStream())
                {
                    blob.DownloadToStream(imageStream);
                    return Image.FromStream(imageStream);
                }
			}

            return CreateBaseImage(defaultWidth, defaultHeight);
        }
	}
}