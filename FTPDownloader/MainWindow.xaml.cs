    using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTPDownloader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        string ftpAdress = "";
        string ftpPort = "";
        string userName = "";
        string userPassword = "";
        public MainWindow()
        {
            InitializeComponent();
            ftpAdress = "ftp://192.168.0.10/";
            ftpPort = "21";
            GetPhysicalIP();
            userName ="ttemoya";
            userPassword = "ttemoya";

            //
      //      user: "vglc",
      //password: "mIbF6YwU!"
            //ReadNCinfo();
        }
        
        private string ReadNCinfo()
        {
            NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();
            NetworkInterface.GetAllNetworkInterfaces();
            return null;
            //ini 정보 읽는다
        }
        private void ViewNetworkInfo()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nw in networks)
            {
                Console.WriteLine(nw.Name);
                Console.WriteLine(nw.GetPhysicalAddress().ToString());

                IPInterfaceProperties ipProps = nw.GetIPProperties();
                foreach (UnicastIPAddressInformation ucip in ipProps.UnicastAddresses)
                {
                    Console.WriteLine(ucip.Address.ToString());
                }

            }
            
        }
        public static string GetPhysicalIPAdress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return String.Empty;
        }

        private void GetPhysicalIP()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                if (addr != null)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        Console.WriteLine(ni.Name);
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                Console.WriteLine(ip.Address.ToString());
                            }
                        }
                    }
                }
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile();
        }
        private StreamReader FtpWebRequest(string FtpAdress, string userName, string userPassword            )
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpAdress); // FTP Address  
            ftpRequest.Credentials = new NetworkCredential(userName, userPassword); // Credentials  
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
            return new StreamReader(response.GetResponseStream());
        }
        public void DownloadFile()
        {

            if (!File.Exists("NCinfo.ini"))
                return;

            Ini ini = new Ini("NCinfo.ini");
            ftpAdress = $"ftp://{ini.GetValue("NAS", "NCInfo")}";
            ftpPort = ini.GetValue("NAS_Port", "NCInfo");
            //            HDD = NO
            //NetCafe = Y
            ftpAdress = "ftp://192.168.0.186/";
            ftpPort = "14147";
            string contentsID = "JP_M_0000000112";
            string dirpath = LocalPathBox.Text;

            try
            {
                StreamReader streamReader = FtpWebRequest(ftpAdress, userName, userPassword);

                List<string> directories = new List<string>(); // create list to store directories.   
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    directories.Add(line); // Add Each Directory to the List.  
                    line = streamReader.ReadLine();
                }
                using (WebClient ftpClient = new WebClient())
                {

                    ftpClient.Credentials = new NetworkCredential(userName, userPassword);
                    //string[] filename = HttpPathBox.Text.Split('/');
                    //Filename = filename[filename.Length - 1];
                    //for (int k = 0; k <= filename.Length - 1; k++)
                    //{
                    //    dirpath = $@"{dirpath}\{filename[k]}";
                    //    createdir(dirpath);
                    //}
                    try
                    {
                        string ServerDirAdress = "";
                        string localSubDirPath = "";

                        for (int i = 0; i <= directories.Count - 1; i++)
                        {
                            if (directories[i].ToString() == contentsID)
                            {
                                ServerDirAdress = $@"{ftpAdress}\{directories[i].ToString()}";

                                //FileInfo fi = new FileInfo(_Filestr);
                                //if (fi.Exists)
                                if (directories[i].Contains("."))
                                {
                                    //파일인 경우 다운로드
                                    ftpClient.DownloadFile(ServerDirAdress, $@"{dirpath.ToString()}\{directories[i].ToString()}");
                                }
                                else
                                {
                                    //폴더인 경우 폴더 생성
                                    localSubDirPath = $@"{dirpath}\{directories[i].ToString()}";
                                    //directories.Add(localSubDirPath); // Add the Sub-directories with the path to directories.  

                                    createDirectory(localSubDirPath);

                                    //디렉토리의 서브폴더가 있는지
                                    string[] subdirectory = Return(ServerDirAdress, userName, userName);
                                    MakeDirectory(subdirectory, directories, ftpClient, ServerDirAdress, dirpath, localSubDirPath);
                                }
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        String status = ((FtpWebResponse)e.Response).StatusDescription;
                        MessageBox.Show(status.ToString());
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
                streamReader.Close();
            }
            catch (WebException l)
            {
                String status = ((FtpWebResponse)l.Response).StatusDescription;
                MessageBox.Show(status.ToString());
            }
        }

        private void MakeDirectory(string[] subdirectory, List<string> directories, WebClient ftpClient, string path, string dirpath, string subdirpath)
        {
            for (int j = 0; j <= subdirectory.Length - 1; j++)
            {
                if (subdirectory[j].Contains("."))
                {
                    ftpClient.DownloadFile($@"{ftpAdress}\{subdirectory[j]}", $@"{dirpath}\{subdirectory[j]}");
                }
                else
                {
                    string localSubDirPath = $@"{subdirectory[j]}";
                    directories.Add(localSubDirPath); // Add the Sub-directories with the path to directories.  

                    createDirectory(localSubDirPath);

                    subdirectory = Return(path, userName, userName);

                    MakeDirectory(subdirectory, directories, ftpClient, path, dirpath, localSubDirPath);
                }
            }

        }
        // Here i get the list of Sub-directories and the files.   
        public string[] Return(string filepath, string username, string password)
        {
            List<string> directories = new List<string>();
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(filepath);
                ftpRequest.Credentials = new NetworkCredential(username, password);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    directories.Add(line);
                    line = streamReader.ReadLine();
                }
            }
            catch (WebException e)
            {
                String status = ((FtpWebResponse)e.Response).StatusDescription;
                MessageBox.Show(status.ToString());
            }
            return directories.ToArray();
        }

        // In this part i create the sub-directories.   
        public void createDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}
