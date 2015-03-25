/*
 * Created by SharpDevelop.
 * User: NathanA
 * Date: 19/06/2014
 * Time: 11:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Threading;

namespace Downloader_Test
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(error);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException +=new ThreadExceptionEventHandler(errorthread);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        
        
        static void error(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }
        
        
        static void errorthread(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Download.Log.Exception(e.Exception);
        }
        
    }
}
