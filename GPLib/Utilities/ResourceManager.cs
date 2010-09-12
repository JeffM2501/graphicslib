/*
    Open Combat/Projekt 2501
    Copyright (C) 2010  Jeffery Allen Myers

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utilities.Paths
{
    public class ResourceManager
    {
        public static List<string> SearchPaths = new List<string>();

        public static void KillPaths ()
        {
            SearchPaths.Clear();
        }

        public static void AddPath ( string path )
        {
            if (Directory.Exists(path))
                SearchPaths.Insert(0,new DirectoryInfo(path).FullName);
        }

        public static string FindFile(string name)
        {
            foreach (string searchpath in SearchPaths)
            {
                FileInfo file = new FileInfo(Path.Combine(searchpath, name));
                if (file.Exists)
                    return file.FullName;
            }

            return string.Empty;
        }

        public static List<string> FindFiles(string path)
        {
            return FindFiles(path, string.Empty);
        }

        public static List<string> FindFiles(string path, string search)
        {
            List<string> files = new List<string>();

            foreach (string searchpath in SearchPaths)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(searchpath, path));
                if (dir.Exists)
                {
                    FileInfo[] dirFiles;
                    if (search == string.Empty)
                        dirFiles = dir.GetFiles();
                    else
                        dirFiles = dir.GetFiles(search);

                    foreach (FileInfo file in dirFiles)
                        files.Add(file.FullName);
                }
            }

            return files;
        }

        public static string FindDirectory(string name)
        {
            foreach (string searchpath in SearchPaths)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(searchpath, name));
                if (dir.Exists)
                    return dir.FullName;
            }

            return string.Empty;
        }
    }
}
