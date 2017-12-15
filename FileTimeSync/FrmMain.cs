using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FileTimeSync
{
    public partial class FrmMain : Form
    {
        private bool _isToClose;

        public FrmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 修改文件创建时间、访问时间、修改时间
        /// </summary>
        /// <param name="filePath">图片的完整路径</param>
        private static void ChangeFileTime(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (filePath.ToLower().EndsWith(".jpg"))
            {
                ChangeImageTime(filePath);
            }
            if (filePath.ToLower().EndsWith(".mp4"))
            {
                ChangeMp4TimeByName(filePath);
            }
        }

        /// <summary>
        /// 修改文件夹下所有文件的创建时间、访问时间、修改时间
        /// </summary>
        /// <param name="dirPath">文件夹完整路径</param>
        private static void ChangeFileTimeByFolder(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath)) return;
            var dir = new DirectoryInfo(dirPath);
            var files = dir.GetFileSystemInfos();
            foreach (var file in files)
            {
                if (file is DirectoryInfo)  //判断是否为文件夹
                {
                    //递归调用
                    ChangeFileTimeByFolder(file.FullName);
                }
                else
                {
                    var filePath = file.FullName;
                    ChangeFileTime(filePath);
                }
            }
        }

        /// <summary>
        /// 更新图片的创建、修改、访问时间为拍摄时间
        /// </summary>
        /// <param name="filePath">图片的完整路径</param>
        private static void ChangeImageTime(string filePath)
        {
            string value = string.Empty;

            var ascii = Encoding.ASCII;
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var image = Image.FromStream(stream, true, false);

            foreach (var p in image.PropertyItems)
            {
                if (p.Id == 0x0132) //修改时间，0x9003拍摄时间
                {
                    stream.Close();
                    value = ascii.GetString(p.Value).Replace("\0", "");
                }
                if (!string.IsNullOrEmpty(value) && value.Length == 19)
                {
                    value = value.Substring(0, 10).Replace(":", "-") + value.Substring(10, 9);
                    var time = DateTime.Parse(value);
                    SyncFileTime(filePath, time);
                }
            }
            stream.Close();
        }


        /// <summary>
        /// 根据文件名，更新图片的创建、修改、访问时间
        /// </summary>
        /// <param name="filePath">图片的完整路径</param>
        private static void ChangeMp4TimeByName(string filePath)
        {
            var file = new FileInfo(filePath);

            var name = file.Name;
            if (name.StartsWith("VID") && name.Length == 23)
            {
                DateTime time = ParseDateTimeByName(name);
                SyncFileTime(filePath, time);
            }
        }

        /// <summary>
        /// 将文件名转换为日期
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns></returns>
        private static DateTime ParseDateTimeByName(string name)
        {
            var time = name.Substring(4, 4) + "-" + name.Substring(8, 2) + "-" + name.Substring(10, 2) + " "
                  + name.Substring(13, 2) + ":" + name.Substring(15, 2) + ":" + name.Substring(17, 2);
            return DateTime.Parse(time);
        }

        /// <summary>
        /// 修改文件创建时间、访问时间、修改时间为拍摄时间
        /// </summary>
        /// <param name="filePath">图片的完整路径</param>
        /// <param name="time">拍摄时间</param>
        private static void SyncFileTime(string filePath, DateTime time)
        {
            // 修改修改时间
            File.SetLastWriteTime(filePath, time);
            // 修改创建时间
            File.SetCreationTime(filePath, time);
            // 修改访问时间
            File.SetLastAccessTime(filePath, time);
        }

        /// <summary>
        /// 获取单个文件路径
        /// </summary>
        private void txtFilePath_DoubleClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = "C://",
                Filter = "图片(*.jpg)|*.jpg|视频(*.mp4)|*.mp4",
                FilterIndex = 1
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// 获取文件夹路径
        /// </summary>
        private void txtFolderPath_DoubleClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = dialog.SelectedPath;
            }
        }


        private void btnModifyByFile_Click(object sender, EventArgs e)
        {
            var thread = new Thread(DoFileClick);
            thread.Start();
            var frm = new FrmProcessStatus(50, "正在更新信息，请稍后...", GetIsToClose);
            frm.ShowDialog();
        }

        private void DoFileClick()
        {
            _isToClose = false;
            var filePath = txtFilePath.Text.Trim();
            ChangeFileTime(filePath);
            _isToClose = true;
        }


        private void btnModifyByFolder_Click(object sender, EventArgs e)
        {
            var thread = new Thread(DoFolderClick);
            thread.Start();
            var frm = new FrmProcessStatus(50, "正在更新信息，请稍后...", GetIsToClose);
            frm.ShowDialog();
        }

        private bool GetIsToClose()
        {
            return _isToClose;
        }

        private void DoFolderClick()
        {
            _isToClose = false;
            var dirPath = txtFolderPath.Text.Trim();
            ChangeFileTimeByFolder(dirPath);
            _isToClose = true;
        }
    }
}
