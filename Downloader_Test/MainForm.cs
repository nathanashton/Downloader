using Download;
using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;

namespace Downloader_Test
{
    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        readonly Downloader _downloader = new Downloader();

        public MainForm()
        {
            InitializeComponent();
            textBox1.Text = "http://mirror.internode.on.net/pub/test/10meg.test";
            label5.DataBindings.Add("Text", _downloader, "DownloadProgress");
            label6.DataBindings.Add("Text", _downloader, "DownloadSpeedAsString");

        }
        
        void Button1Click(object sender, EventArgs e)
        {
            _downloader.Md5Hash = textBox2.Text;
            _downloader.SaveFileAs = textBox3.Text;

            _downloader.OnDownloadCompleted(DownloadComplete);
            _downloader.OnDownloadProgressChanged(DownloadProgress);
            
            try
            {
                _downloader.StartDownload(new Uri(textBox1.Text), checkBox1.Checked, checkBox2.Checked, checkBox3.Checked);       
            } catch (DownloadException ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                {
                    error += " - " + ex.InnerException.Message;
                }
                MessageBox.Show("An error was encountered\n\n" + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            label4.Text = String.Format("{0}\n\n{1}", _downloader.DownloadStatus.ToString(), e.UserState);

        }
        
        void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;  
        }
        
        
        
        void Button2Click(object sender, EventArgs e)
        {
            _downloader.CancelDownload();
        }


    }
}
