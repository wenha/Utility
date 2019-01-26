using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.SharpZip
{

    public class SharpZip
    {
        public SharpZip() { }

        /// <summary>  
        /// 压缩  
        /// </summary>   
        /// <param name="filename"> 压缩后的文件名(包含物理路径)</param>  
        /// <param name="directory">待压缩的文件夹(包含物理路径)</param>  
        public static void PackFiles(string filename, string directory, string password)
        {
            try
            {
                FastZip fz = new FastZip();
                fz.CreateEmptyDirectories = true;
                if (!string.IsNullOrEmpty(password))
                {
                    fz.Password = password;
                }
                fz.CreateZip(filename, directory, true, "");
                fz = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>  
        /// 解压缩  
        /// </summary>  
        /// <param name="file">待解压文件名(包含物理路径)</param>  
        /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>  
        public static bool UnpackFiles(string file, string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                ZipInputStream s = new ZipInputStream(File.OpenRead(file));
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (directoryName != String.Empty)
                    {
                        // Directory.CreateDirectory(dir + directoryName);
                    }
                    if (fileName != String.Empty)
                    {
                        FileStream streamWriter = File.Create(dir + fileName);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
                s.Close();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class ClassZip
    {
        #region 私有方法
        /// <summary>  
        /// 递归压缩文件夹方法  
        /// </summary>  
        private static bool ZipFileDictory(string FolderToZip, ZipOutputStream s, string ParentFolderName)
        {
            bool res = true;
            string[] folders, filenames;
            ZipEntry entry = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();
            try
            {
                entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/"));
                s.PutNextEntry(entry);
                s.Flush();
                filenames = Directory.GetFiles(FolderToZip);
                foreach (string file in filenames)
                {
                    fs = File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/" + Path.GetFileName(file)));
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                {
                    entry = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            folders = Directory.GetDirectories(FolderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(folder, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip))))
                {
                    return false;
                }
            }
            return res;
        }

        /// <summary>  
        /// 压缩目录  
        /// </summary>  
        /// <param name="FolderToZip">待压缩的文件夹，全路径格式</param>  
        /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>  
        private static bool ZipFileDictory(string FolderToZip, string ZipedFile, int level)
        {
            bool res;
            if (!Directory.Exists(FolderToZip))
            {
                return false;
            }
            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(level);
            res = ZipFileDictory(FolderToZip, s, "");
            s.Finish();
            s.Close();
            return res;
        }

        /// <summary>  
        /// 压缩文件  
        /// </summary>  
        /// <param name="FileToZip">要进行压缩的文件名</param>  
        /// <param name="ZipedFile">压缩后生成的压缩文件名</param>  
        private static bool ZipFile(string FileToZip, string ZipedFile, int level)
        {
            if (!File.Exists(FileToZip))
            {
                throw new System.IO.FileNotFoundException("指定要压缩的文件: " + FileToZip + " 不存在!");
            }
            FileStream ZipFile = null;
            ZipOutputStream ZipStream = null;
            ZipEntry ZipEntry = null;
            bool res = true;
            try
            {
                ZipFile = File.OpenRead(FileToZip);
                byte[] buffer = new byte[ZipFile.Length];
                ZipFile.Read(buffer, 0, buffer.Length);
                ZipFile.Close();

                ZipFile = File.Create(ZipedFile);
                ZipStream = new ZipOutputStream(ZipFile);
                ZipEntry = new ZipEntry(Path.GetFileName(FileToZip));
                ZipStream.PutNextEntry(ZipEntry);
                ZipStream.SetLevel(level);

                ZipStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (ZipEntry != null)
                {
                    ZipEntry = null;
                }
                if (ZipStream != null)
                {
                    ZipStream.Finish();
                    ZipStream.Close();
                }
                if (ZipFile != null)
                {
                    ZipFile.Close();
                    ZipFile = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            return res;
        }
        #endregion

        /// <summary>  
        /// 压缩  
        /// </summary>  
        /// <param name="FileToZip">待压缩的文件目录</param>  
        /// <param name="ZipedFile">生成的目标文件</param>  
        /// <param name="level">6</param>  
        public static bool Zip(String FileToZip, String ZipedFile, int level)
        {
            if (Directory.Exists(FileToZip))
            {
                return ZipFileDictory(FileToZip, ZipedFile, level);
            }
            else if (File.Exists(FileToZip))
            {
                return ZipFile(FileToZip, ZipedFile, level);
            }
            else
            {
                return false;
            }
        }

        /// <summary>  
        /// 解压  
        /// </summary>  
        /// <param name="FileToUpZip">待解压的文件</param>  
        /// <param name="ZipedFolder">解压目标存放目录</param>  
        public static void UnZip(string FileToUpZip, string ZipedFolder)
        {
            if (!File.Exists(FileToUpZip))
            {
                return;
            }
            if (!Directory.Exists(ZipedFolder))
            {
                Directory.CreateDirectory(ZipedFolder);
            }
            ZipInputStream s = null;
            ZipEntry theEntry = null;
            string fileName;
            FileStream streamWriter = null;
            try
            {
                s = new ZipInputStream(File.OpenRead(FileToUpZip));
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Name != String.Empty)
                    {
                        fileName = Path.Combine(ZipedFolder, theEntry.Name);
                        if (fileName.EndsWith("/") || fileName.EndsWith("\\"))
                        {
                            Directory.CreateDirectory(fileName);
                            continue;
                        }
                        streamWriter = File.Create(fileName);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter = null;
                }
                if (theEntry != null)
                {
                    theEntry = null;
                }
                if (s != null)
                {
                    s.Close();
                    s = null;
                }
                GC.Collect();
                GC.Collect(1);

            }
        }
    }

    public class ZipHelper
    {
        #region 私有变量
        String the_rar;
        RegistryKey the_Reg;
        Object the_Obj;
        String the_Info;
        ProcessStartInfo the_StartInfo;
        Process the_Process;
        #endregion

        /// <summary>  
        /// 压缩  
        /// </summary>  
        /// <param name="zipname">要解压的文件名</param>  
        /// <param name="zippath">要压缩的文件目录</param>  
        /// <param name="dirpath">初始目录</param>  
        public void EnZip(string zipname, string zippath, string dirpath)
        {
            try
            {
                the_Reg = Registry.ClassesRoot.OpenSubKey(@"Applications\WinRAR.exe\Shell\Open\Command");
                the_Obj = the_Reg.GetValue("");
                the_rar = the_Obj.ToString();
                the_Reg.Close();
                the_rar = the_rar.Substring(1, the_rar.Length - 7);
                the_Info = " a    " + zipname + "  " + zippath;
                the_StartInfo = new ProcessStartInfo();
                the_StartInfo.FileName = the_rar;
                the_StartInfo.Arguments = the_Info;
                the_StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                the_StartInfo.WorkingDirectory = dirpath;
                the_Process = new Process();
                the_Process.StartInfo = the_StartInfo;
                the_Process.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>  
        /// 解压缩  
        /// </summary>  
        /// <param name="zipname">要解压的文件名</param>  
        /// <param name="zippath">要解压的文件路径</param>  
        public void DeZip(string zipname, string zippath)
        {
            try
            {
                the_Reg = Registry.ClassesRoot.OpenSubKey(@"Applications\WinRar.exe\Shell\Open\Command");
                the_Obj = the_Reg.GetValue("");
                the_rar = the_Obj.ToString();
                the_Reg.Close();
                the_rar = the_rar.Substring(1, the_rar.Length - 7);
                the_Info = " X " + zipname + " " + zippath;
                the_StartInfo = new ProcessStartInfo();
                the_StartInfo.FileName = the_rar;
                the_StartInfo.Arguments = the_Info;
                the_StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                the_Process = new Process();
                the_Process.StartInfo = the_StartInfo;
                the_Process.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="source">源目录</param>
        /// <param name="s">ZipOutputStream对象</param>
        public static void Compress(string source, ZipOutputStream s)
        {
            string[] filenames = Directory.GetFileSystemEntries(source);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    // 递归压缩子文件夹
                    Compress(file, s);
                }
                else
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] buffer = new byte[4 * 1024];
                        // 此处去掉盘符，如D:\123\1.txt 去掉D:
                        ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
            }
        }

        /// <summary>
        /// 压缩文件(多个)
        /// </summary>
        /// <param name="list">文件集合</param>
        /// <param name="path">文件名</param>
        public static bool Compress(List<string> list, string path, string password)
        {
            try
            {
                using (var s = new ZipOutputStream(File.Create(path)))
                {
                    s.SetLevel(6);
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        s.Password = password.Trim();
                    }
                    foreach (string file in list)
                    {
                        if (Directory.Exists(file))
                        {
                            // 递归压缩子文件夹
                            Compress(file, s);
                        }
                        else
                        {
                            using (FileStream fs = File.OpenRead(file))
                            {
                                // 此处去掉盘符，如D:\123\1.txt 去掉D:
                                ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                                //entry.DateTime = DateTime.Now;
                                //entry.Size = fs.Length;
                                s.PutNextEntry(entry);

                                byte[] data = new byte[1024 * 1024];
                                int size = fs.Read(data, 0, data.Length);
                                while (size > 0)
                                {
                                    s.Write(data, 0, size);
                                    size = fs.Read(data, 0, data.Length);
                                }
                            }
                        }
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="sourceFile">压缩包完整路径地址</param>
        /// <param name="targetPath">解压路径是哪里</param>
        /// <returns></returns>
        public static bool Decompress(string sourceFile, string targetPath, string password)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", sourceFile));
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            using (var s = new ZipInputStream(File.OpenRead(sourceFile)))
            {
                if (!string.IsNullOrWhiteSpace(password))
                {
                    s.Password = password.Trim();
                }

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.IsDirectory)
                    {
                        continue;
                    }
                    string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
                    string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));
                    if (!Directory.Exists(directorName))
                    {
                        Directory.CreateDirectory(directorName);
                    }
                    if (!String.IsNullOrEmpty(fileName))
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            byte[] data = new byte[1024 * 1024];
                            int size = s.Read(data, 0, data.Length);
                            while (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                                size = s.Read(data, 0, data.Length);
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
