using System.Net;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Diagnostics;


namespace Download
{
    
    /// <summary>
    /// Async downloader that supports MD5 verification and displays progress using Winforms
    /// </summary>
    public partial class Downloader : Form, INotifyPropertyChanged
    {
        
        /// <summary>
        /// Gets or sets the Download URL
        /// </summary>
        /// <remarks>Input must be a Uri</remarks>
        public Uri DownloadUri { get; set; }
        
        /// <summary>
        /// Gets or sets MD5 which will be validated against downloaded file
        /// </summary>
        /// <remarks>MD5Hash that the downloaded file is validated against to ensure it downloaded successfully</remarks>
        public string Md5Hash { get; set; }
        
        /// <summary>
        /// Closes form when download has completed
        /// </summary>
        private bool _closeOnSave;

        /// <summary>
        /// 
        /// </summary>
        public bool CloseOnSave
        {
            get { return _closeOnSave; }
            set
            {
                _closeOnSave = value;
                NotifyPropertyChanged("CloseOnSave");
            }
        }
        
        /// <summary>
        /// Runs without display form. You need to subscribe to OnDownloadProgressCahnged to get the progress
        /// </summary>
        public bool Headless
        {
            get { return _headLess; }
            set
            {
                _headLess = value;
                NotifyPropertyChanged("Headless");
            }
        }
        
        private bool _headLess;
        
        /// <summary>
        /// Run downloaded file when finished downloading
        /// </summary>
        public bool RunWhenComplete { get; set; }
        
        /// <summary>
        /// Returns the size of the file being Downloaded
        /// </summary>
        public long DownloadSize
        {
            get
            {
                return DownloadUri != null ? _downloadSize : 0;
            }
            private set
            {
                _downloadSize = value;
                NotifyPropertyChanged("DownloadSize");
            }
        }
        
        /// <summary>
        /// Returns the speed of the file currently being downloaded.
        /// </summary>
        public long DownloadSpeed
        {
            get 
            {
                return _downloadSpeed;
            }
        }
        
        /// <summary>
        /// Returns the speed of the file currently being download in plain text
        /// </summary>
        public string DownloadSpeedAsString
        {
            get
            {
                return _downloadSpeed != 0 ? String.Format("{0}/s",Utils.BitsToString(Convert.ToInt32(_downloadSpeed))) : null;
            }
        }
        
        private string FileNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(SaveFileAs);
            }
        }
        
        private string FileToSave
        {
            get
            {
                return FileNameWithoutExtension + "." + "download";
            }
        }
        
        /// <summary>
        /// Displays the size of the file to download and current downloaded. 
        /// </summary>
        /// <remarks>Databind to DownloadProgress</remarks>
        public string DownloadProgress
        {
            get 
            { 
                return _downloadProgress;
            }
            private set 
            { 
                _downloadProgress = value;
                NotifyPropertyChanged("DownloadProgress");
            }
        }
        

        /// <summary>
        /// Gets or sets the optional name of saved file
        /// </summary>
        public string SaveFileAs
        {
            get
            {
                return !String.IsNullOrEmpty(_saveFileAs) ? _saveFileAs : Path.GetFileName(DownloadUri.ToString());
            }
            set
            {
                _saveFileAs = value;
                NotifyPropertyChanged("SaveFileAs");
            }
        }
        
        /// <summary>
        /// Returns the amount of data that has been downloaded.
        /// </summary>
        public long DataDownloaded
        {
            get 
            {
                return _dataDownloaded;
            } 
            private set
            {
                _dataDownloaded = value;
                NotifyPropertyChanged("DataDownloaded");
            }
        }
        
        
        private string PathName
        {
            get
            {
                if (DownloadUri != null)
                {
                    return DownloadUri.ToString();
                }
                if (DownloadUri != null)
                {
                    return DownloadUri.ToString().Remove(DownloadUri.ToString().IndexOf(SaveFileAs, StringComparison.Ordinal), SaveFileAs.Length);
                }
                return null;
            }
        }
        
        
        /// <summary>
        /// Returns the status of the completed download
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Download completed successfully
            /// </summary>
            Successful,
            /// <summary>
            /// Download failed. Additional information will be contained 
            /// </summary>
            Failed,
            /// <summary>
            /// Download was cancelled
            /// </summary>
            Cancelled,
            /// <summary>
            /// MDS Hash verification failed
            /// </summary>
            Md5Failed,
            /// <summary>
            /// Downloading is currently underway
            /// </summary>
            Downloading
               
        }
        
        /// <summary>
        /// Returns the status of the completed download
        /// </summary>
        public Status DownloadStatus {get; set;}

        private readonly WebClient _webClient = new WebClient();
        private readonly Timer _timer = new Timer();
        
        private string _saveFileAs;
        private long _dataDownloaded;
        private long _previousDataDownloaded;
        private long _downloadSize;
        private long _downloadSpeed;
        private string _downloadProgress = String.Empty;

    

        private event AsyncCompletedEventHandler DownloadCompleted;
        
        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;    

        
        

        private void Begin()
        {
            _webClient.DownloadFileCompleted += DownloadCompleted;
            
            if (!ValidateUri(DownloadUri))
            {
                throw new DownloadException("The specified url cannot be found");
            }
            
            if (!String.IsNullOrEmpty(SaveFileAs))
            {
                ValidateSaveAs(SaveFileAs);
            }
            
            if (!Headless)
            {
                Show();
                TopMost = true;
            }
            
            Text = String.Format("0% of {0} Completed", Path.GetFileName(DownloadUri.ToString()));
            lblProgress.Text = "";
            lblFileName.Text = String.Format("{0} from {1}", Path.GetFileName(DownloadUri.ToString()), PathName);
            _webClient.DownloadProgressChanged += DownloadProgressChanged;
            _webClient.DownloadFileCompleted -= DownloadComplete;
            _webClient.DownloadFileCompleted += DownloadComplete;
            progressBar1.Value = 0;
            
            Log.Info("Download started");
            _timer.Interval = 1000;
            _timer.Tick += TimerTick;
            _webClient.Proxy = null;
            _webClient.DownloadFileAsync(DownloadUri, FileToSave, SaveFileAs);
            _timer.Start();
            
        }
        
        
        private void TimerTick(object sender, EventArgs e)
        {
            if (_previousDataDownloaded != 0)
            {
                _downloadSpeed = (_dataDownloaded - _previousDataDownloaded);
            } 
             _previousDataDownloaded = _dataDownloaded;
        }
        
        
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadStatus = Status.Downloading;
            DownloadSize = e.TotalBytesToReceive;
            DataDownloaded = e.BytesReceived;
            
            Text = String.Format("{0}% of {1} Completed",e.ProgressPercentage, Path.GetFileName(DownloadUri.ToString()));
            progressBar1.Value = e.ProgressPercentage;

            DownloadProgress = String.Format("{0} of {1}",Utils.BitsToString(Convert.ToInt32(e.BytesReceived)),Utils.BitsToString(Convert.ToInt32(e.TotalBytesToReceive)));

            lblProgress.Text = String.Format("{0} at {1}", DownloadProgress, DownloadSpeedAsString);
            lblFileName.Text = String.Format("{0} from {1}", Path.GetFileName(DownloadUri.ToString()), PathName);
        }
        
        
        private void DeleteTemporaryFile()
        {
            if (!File.Exists(FileToSave)) return;
            try
            {
                File.Delete(FileToSave);
            } catch (IOException ex)
            {
                Log.Error("Error deleting temporary file: " + FileToSave, ex);
            }
        }


        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            btnCancel.Text = "&Close";
            btnCancel.Click += CloseForm;
            if (e.Cancelled || e.Error != null)
            {
                if (e.Cancelled)
                {
                    CancelDownload();
                    lblProgress.Text = "Download cancelled";
                    DeleteTemporaryFile();
                    EventDownloadComplete(Status.Cancelled, e);
                    if (CloseOnSave)
                    {
                        Hide();
                    }
                    return;
                }
                
                if (e.Error != null) //Exception was encountered downloading the file.
                {
                    Log.Error("Error occurred downloading file: " + e.Error);
                    DeleteTemporaryFile();
                    EventDownloadComplete(Status.Failed, e);
                    lblProgress.Text = "Error downloading file";
                    if (CloseOnSave)
                    {
                        Hide();
                    }
                    return;
                }
            } else
            {
                if (!String.IsNullOrEmpty(Md5Hash))
                {
                    if (!ValidateMd5Hash(FileToSave, Md5Hash))
                    {
                        lblProgress.Text = "Error downloading file";
                        lblProgress.ForeColor = Color.Red;
                        EventDownloadComplete(Status.Md5Failed, e);
                        DeleteTemporaryFile();
                        if (CloseOnSave)
                        {
                            Hide();
                        }
                        return;
                    }
                }
            } // Download successful
            if (File.Exists(SaveFileAs))
            {
                File.Delete(SaveFileAs);
            }
            File.Move(FileToSave, SaveFileAs);
            File.Delete(FileToSave);
            Log.Info("File Downloaded Successfully - " + SaveFileAs);
            EventDownloadComplete(Status.Successful, e);
            lblProgress.Text = "File downloaded successfully - " + SaveFileAs;
            if (CloseOnSave)
            {
                Hide();
            }
            if (!RunWhenComplete) return;
            try
            {
                Process.Start(SaveFileAs);
                Log.Info("File opened: " + SaveFileAs);
            } catch (Win32Exception ex)
            {
                Log.Error("Error trying to open downloaded file " + SaveFileAs, ex);
                throw new DownloadException("Error trying to run downloaded file " + SaveFileAs);
            }
        }
        
        
        private void CloseForm(object sender, EventArgs e)
        {
            Hide();
        }
        
        
        private void BtnCancelClick(object sender, EventArgs e)
        {
            CancelDownload();
        }
        
        

        
        
        private static bool ValidateUri(Uri url)
        {
            try
            {
                var myHttpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                var myHttpWebResponse = (HttpWebResponse) myHttpWebRequest.GetResponse();
                myHttpWebResponse.Close();
                return true;
            }
            catch (WebException ex)
            {
                Log.Error("Url was not valid", ex);
                return false;
            }
        }
        
        
        private static void ValidateSaveAs(string path)
        {
            try
            {
                var file = File.Create(path);
                file.Close();
                File.Delete(path);
            } catch (Exception ex)
            {
                if (!(ex is IOException) && !(ex is UnauthorizedAccessException) && !(ex is ArgumentException) &&
                    !(ex is NotSupportedException)) throw;
                Log.Error("SaveAs path is not valid", ex);
                throw new DownloadException("SaveAs path is not a valid path or filename.", ex);
            } 
        }
        
        
        private bool ValidateMd5Hash(string file, string hash)
        {
            string hashForFile;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    hashForFile = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-","").ToLower();
                }
            }
            
            if (String.Equals(hashForFile, hash))
            {
                return true;
            }
            Log.Error(String.Format("Failed MD5 hash verification. Downloaded File Hash: {0} Compare Hash: {1}", hashForFile, hash));
            DownloadStatus = Status.Md5Failed;
            return false;
        }
        
        
        private void EventDownloadComplete(Status status, AsyncCompletedEventArgs e)
        {
            DownloadStatus = status;
            if (DownloadCompleted != null)
            {
                DownloadCompleted(this, e);
            }
        }
        
        
        private void DownloaderFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_webClient.IsBusy)
            {
                CancelDownload();
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Downloader()
        {
            InitializeComponent();
            Log.level = Log.Level.All; 
        }
        
        
        /// <summary>
        /// Register a delegate to be notified when the Download completes successfully
        /// </summary>
        /// <param name="handler">AsyncCompletedEventHandler delegate that gets registered with the DownloadFileCompleted event</param>
        public void OnDownloadCompleted(AsyncCompletedEventHandler handler)
        {
            DownloadCompleted += handler;
        }
        
        
        /// <summary>
        /// Register a delegate to be notified when the Download progress changes
        /// </summary>
        /// <param name="handler">DownloadProgressChangedHandler delegate that gets registered with the DownloadFileCompleted event</param>        
        public void OnDownloadProgressChanged(DownloadProgressChangedEventHandler handler)
        {
            _webClient.DownloadProgressChanged += handler;
        }
        
        
        /// <summary>
        /// Shows the download dialog and beings downloading
        /// </summary>
        /// <exception cref="DownloadException">Error with download</exception>
        public void StartDownload(Uri downloadUri, bool closeOnSave, bool runWhenComplete, bool headless)
        {
            if (downloadUri == null)
            {
                Log.Error("No URL Specified");
                throw new DownloadException("Download URL must be specified");
            }
            DownloadUri = downloadUri;
            CloseOnSave = closeOnSave;
            RunWhenComplete = runWhenComplete;
            Headless = headless;
            Log.Info(String.Format("DownloadUri: {0} - SaveFileAs: {1} - CloseOnSave: {2} - RunWhenComplete: {3} - Headless: {4}", DownloadUri, SaveFileAs, CloseOnSave, RunWhenComplete, Headless));
            Begin();
        }
        
        
        /// <summary>
        /// Cancels the running download
        /// </summary>
        public void CancelDownload()
        {
            Log.Info("Download cancelled");            
            if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
            }
        }
        
        /// <summary>
        /// Method for INotifyPropertyChanged
        /// </summary>
        /// <param name="property"></param>
        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        

    }
    
   
}
