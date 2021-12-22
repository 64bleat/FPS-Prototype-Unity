using System;
using System.IO;
using UnityEngine;

namespace MPCore.Screenshots
{
	public class ScreenshotManager : ScriptableObject
	{
		const string FILENAME = "Screenshot";

		public void SaveScreenshotToDesktop()
		{
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			int counter = 0;

			while(File.Exists($"{desktopPath + FILENAME + counter}.png"))
				counter++;

			ScreenCapture.CaptureScreenshot($"{desktopPath}\\{FILENAME}{counter:000}.png");
		}
	}
}
