using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TrilliMon
{
    class CommonFuncs
    {
        public static string GetTrillianLogsDir(bool forAstra)
        {
            string trillianDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Trillian\users\default\logs"; //This is the default

            if (forAstra == true)  //User has indicated that they are using Trillian Astra.
            {
                //Logs are in a different location
                trillianDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Trillian\users\";
                //There should be a few folders here so let's try and find the right one
                foreach (string dir in Directory.GetDirectories(trillianDir))
                {
                    DirectoryInfo dirinfo = new DirectoryInfo(dir);
                    if (dirinfo.Name != "global" && dirinfo.LastWriteTime.Date >= DateTime.Now.Date && Directory.Exists(dir + @"\logs"))
                    {
                        //Looks like we found our folder
                        trillianDir += dirinfo.Name;
                        break;
                    }
                }

                //And now the final path to the logs
                trillianDir += @"\logs";
            }

            return trillianDir;
        }
    }
}
