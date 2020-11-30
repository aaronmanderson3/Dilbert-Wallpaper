using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;
using System.Net;
using HtmlAgilityPack;

namespace Dilbert_Wallpaper
{
	class Program
	{
		static List<HtmlNode> nodes = new List<HtmlNode>();

		static void Main(string[] args)
		{
			try
			{
				HtmlDocument doc = new HtmlDocument();
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

				using (WebClient client = new WebClient())
				{
					string content = client.DownloadString("https://www.dilbert.com");
					doc.LoadHtml(content);

					GetHtmlNodes(doc.DocumentNode);
				}

				Wallpaper.Set(new Uri(getImgPath(GetImgNode())), Wallpaper.Style.Centered);
			}
			catch (Exception ex)
			{
				error();
				return;
			}
			
		}

		static string getImgPath(HtmlNode node)
		{
			try
			{
				return node.Attributes.First(a => a.Name == "src").Value.Insert(0, "https:");
			}
			catch
			{
				return null; 
			}
		}

		static HtmlNode GetImgNode()
		{
			try
			{
				HtmlNode x = nodes.First(n => n.Name == "img" && n.Attributes[0].Value == "img-responsive img-comic");
				return x;
			}
			catch 
			{
				return null;
			}
		}

		static HtmlNode GetHtmlNodes(HtmlNode n)
		{
			try
			{
				if (n.HasChildNodes)
				{
					foreach (HtmlNode child in n.ChildNodes)
						nodes.Add(GetHtmlNodes(child));
				}

				return n;
			}
			catch 
			{
				return null;
			}
		}

		static void error()
		{
			if (System.Windows.Forms.MessageBox.Show("Source of www.dilbert.com has changed!", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.OK)
				Environment.Exit(0);
		}
	}

	public sealed class Wallpaper
	{
		Wallpaper() { }

		const int SPI_SETDESKWALLPAPER = 20;
		const int SPIF_UPDATEINIFILE = 0x01;
		const int SPIF_SENDWININICHANGE = 0x02;

		[DllImport("user32.dll", EntryPoint = "SystemParametersInfo", CharSet = CharSet.Auto)]
		public static extern int SystemParametersInfo(int uAction, int UParam, string lpvParam, int fuWinIni);

		public enum Style : int
		{
			Tiled,
			Centered,
			Stretched
		}

		public static void Set(Uri uri, Style style)
		{
			System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

			System.Drawing.Image img = System.Drawing.Image.FromStream(s);
			string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "wallpaper.bmp");
			Console.WriteLine(tempPath);
			img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

			//Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			//key.SetValue(@"WallpaperStyle", 1.ToString());
			//key.SetValue(@"TileWallpaper", 0.ToString());

			SystemParametersInfo(SPI_SETDESKWALLPAPER,
				0,
				tempPath,
				2);
		}
	}
}
