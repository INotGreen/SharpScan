using SharpHostInfo.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpScan
{
    public class TargetParser
    {
       

        public static void ReadFileToList(String path, ref HashSet<string> ips)
        {
            ips.Clear();
            FileStream fs_dir = null;
            StreamReader reader = null;
            try
            {
                fs_dir = new FileStream(path, FileMode.Open, FileAccess.Read);

                reader = new StreamReader(fs_dir);

                String lineStr;

                while ((lineStr = reader.ReadLine()) != null)
                {
                    if (!lineStr.Equals(""))
                    {
                        ips.Add(lineStr);
                    }
                }
            }
            catch (Exception e)
            {
                Writer.Failed("An exception occurred while reading the file list!" + e.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (fs_dir != null)
                {
                    fs_dir.Close();
                }
            }
        }
    }
}
