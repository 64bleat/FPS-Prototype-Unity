using System;
using System.IO;
using UnityEngine;

namespace MPCore.Screenshots
{
    public class ScreenshotManager : ScriptableObject
    {
        public void SaveScreenshotToDesktop()
        {
            string desktopPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\\";
            string filename = "screenshot";
            int counter = 0;

            while(File.Exists($"{desktopPath + filename + counter}.png"))
                counter++;

            ScreenCapture.CaptureScreenshot($"{desktopPath + filename + counter}.png");
        }
    }
}
