using FMDC.Model;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FMDC.Utility
{
	public static class UtilityExtensions
	{
		//Base64 Image Extensions
		public static Bitmap ConvertToBitmap(this byte[] imageBytes)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream(imageBytes))
				{
					return (Bitmap)Image.FromStream(bitmapStream);
				}
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError(string.Format(MessageConstants.BASE64_CONVERSION_ERROR_TEMPLATE, ex.Message));
				return null;
			}
		}


		public static string ConvertToBase64(this Bitmap bitmap)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream())
				{
					bitmap.Save(bitmapStream, ImageFormat.Bmp);
					return Convert.ToBase64String(bitmapStream.ToArray());
				}
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError
				(
					string.Format
					(
						MessageConstants.BITMAP_BASE64_CONVERSION_ERROR_TEMPLATE,
						ex.Message
					)
				);

				return null;
			}
		}


		public static byte[] ConvertToByteArray(this Bitmap bitmap)
		{
			try
			{
				using (MemoryStream bitmapStream = new MemoryStream())
				{
					bitmap.Save(bitmapStream, ImageFormat.Bmp);
					return bitmapStream.ToArray();
				}
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError
				(
					string.Format
					(
						MessageConstants.BITMAP_BYTEARRAY_CONVERSION_ERROR_TEMPLATE,
						ex.Message
					)
				);

				return null;
			}
		}


		public static string CleanseBase64(this string base64)
		{
			return base64.Replace("\r\n", "").Replace(" ", "");
		}
	}
}
