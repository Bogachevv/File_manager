using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace File_manager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileTree localTree;
        private FileTree serverTree;
        private FTPClient client;
        private string localDir,serverDir, ip, login, password;

        private void OnKeyDown(object sender, KeyEventArgs e, FileTree fileTree)
        {
            if (e.Key == Key.Delete) Delete(sender, fileTree);
            else if (e.Key == Key.F5) fileTree.Submit(copy);
            else if (e.Key == Key.F6) fileTree.Submit(move);
            else if (e.Key == Key.F4) Edit(fileTree);
            else if (e.Key == Key.F3) View(fileTree);
            else if (e.Key == Key.F7) MakeDir(fileTree);

        }

        public void View(FileTree fileTree)
        {
            try
            {
                string way = fileTree.Way + ((ListBoxItem)fileTree.ListBox.SelectedItem).Content.ToString();
                byte[] bytes;
                if (local.SelectedItems.Count > server.SelectedItems.Count) using (var stream = new FileStream(way, FileMode.Open)) { bytes = new byte[stream.Length]; stream.Read(bytes, 0, (int)stream.Length); stream.Close(); }
                else if (local.SelectedItems.Count < server.SelectedItems.Count) bytes = client.GetFileBytes(way);
                else return;
                Notepad notepad = new Notepad(bytes) { Readonly = true };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Edit(FileTree fileTree)
        {
            try
            {
                string way = fileTree.Way + ((ListBoxItem)fileTree.ListBox.SelectedItem).Content.ToString();
                byte[] bytes;
                if (local.SelectedItems.Count > server.SelectedItems.Count) using (var stream = new FileStream(way, FileMode.Open)) { bytes = new byte[stream.Length]; stream.Read(bytes, 0, (int)stream.Length); stream.Close(); }
                else if (local.SelectedItems.Count < server.SelectedItems.Count) bytes = client.GetFileBytes(way);
                else return;
                Notepad notepad = new Notepad(bytes) { Readonly = false };
                notepad.onSaveSubmit += (string text) =>
                {
                    byte[] bytearr = ASCIIEncoding.ASCII.GetBytes(text);
                    if (local.SelectedItems.Count > server.SelectedItems.Count) File.WriteAllBytes(way, bytearr);
                    else if (local.SelectedItems.Count < server.SelectedItems.Count) client.WriteFile(bytearr, way);
                    else return;
                };
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void MakeDir(FileTree fileTree)
        {
            try
            {
                AskWindow askWindow = new AskWindow();
                askWindow.Title = "Создание директории";
                askWindow.Show();
                askWindow.name.Focus();
                askWindow.Left = Left + Width / 2;
                askWindow.Top = Top + Height / 2;
                askWindow.name.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (askWindow.name.Text == "TextBox") askWindow.name.Text = "";
                };
                askWindow.submit.Click += (object sender, RoutedEventArgs e) =>
                {
                    string name = askWindow.name.Text;
                    if (fileTree == serverTree)
                    {
                        client.CreateDirectory(fileTree.Way + name);
                    }
                    else if (fileTree == localTree)
                    {
                        Directory.CreateDirectory(fileTree.Way + name);
                    }
                    askWindow.Close();
                };
                askWindow.cancel.Click += (object sender, RoutedEventArgs e) =>
                {
                    askWindow.Close();
                };
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
            fileTree.Refresh();
        }

        public void MakeFile(FileTree fileTree)
        {
            try
            {
                AskWindow askWindow = new AskWindow();
                askWindow.Title = "Создание файла";
                askWindow.Show();
                askWindow.name.Focus();
                askWindow.Left = Left + Width / 2;
                askWindow.Top = Top + Height / 2;
                askWindow.name.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    if (askWindow.name.Text == "TextBox") askWindow.name.Text = "";
                };
                askWindow.submit.Click += (object sender, RoutedEventArgs e) =>
                {
                    string name = askWindow.name.Text;
                    if (fileTree == serverTree)
                    {
                        client.CreateFile(fileTree.Way + name);
                    }
                    else if (fileTree == localTree)
                    {
                        File.Create(fileTree.Way + name);
                    }
                    askWindow.Close();
                    fileTree.Refresh();
                };
                askWindow.cancel.Click += (object sender, RoutedEventArgs e) =>
                {
                    askWindow.Close();
                };
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Delete(object sender, FileTree fileTree)
        {
            try {

                string names = "";
                ListBox listBox = (ListBox)sender;
                foreach (ListBoxItem item in listBox.SelectedItems) names += "\n" + (string)item.Content;
                if (MessageBox.Show(names, "Удалить файлы", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                foreach (string str in names.Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries)) fileTree.RemoveData(str);
                fileTree.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Copy(FileTree fileTree, string way, string[] names)
        {
            try 
            {
            if (fileTree == serverTree)
            {
                if (names.Length == 0) return;
                foreach (string name in names)
                {
                        Task task = new Task(()=>client.DownloadFile(way + name, localTree.Way));
                        try
                        {
                            task.Start();
                        }
                        catch(Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                        Thread sleeper = new Thread(new ThreadStart(() =>
                        {
                            Task.WaitAll(task);
                            Dispatcher.Invoke(() => localTree.Refresh());
                            
                        }));
                        sleeper.Start();
                        
                }
            }
            else if (fileTree == localTree)
            {
                if (names.Length == 0) return;
                foreach (string name in names) client.UploadFile(serverTree.Way + name, way + name);
                serverTree.Refresh();
            }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
}

        public void Move(FileTree fileTree, string way, string[] names)
        {
            try { 
            if (fileTree == serverTree)
            {
                if (names.Length == 0) return;
                foreach (string name in names)
                {
                    client.DownloadFileAsync(way + name, localTree.Way);
                    client.DeleteFile(way + name);
                    serverTree.Refresh();
                    localTree.Refresh();
                }
            }
            else if (fileTree == localTree)
            {
                if (names.Length == 0) return;
                foreach (string name in names) client.UploadFile(serverTree.Way + name, way + name);
                foreach (string name in names) File.Delete(way + name);
                serverTree.Refresh();
                localTree.Refresh();
            }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void MainMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            //MessageBox.Show((string)item.Header);
            if ((string)item.Header == "New window") Process.Start("File_manager.exe");
            else if ((string)item.Header == "Close") this.Close();
            else if ((string)item.Header == "Home directory") ChangeHomeDirectory();
            else if ((string)item.Header == "Info") MessageBox.Show("FTP connection: " + "\nIP: " + client.Url + "\nLogin: " + client.Login + "\nPassword: " + client.Password, "Connection info");
            else if ((string)item.Header == "Set")
            {
                System.Windows.Forms.Form form = new System.Windows.Forms.Form();
                form.Width = 270;
                form.Height = 230;
                form.Text = "FTP connection settings";
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                EventHandler eventHandler = new EventHandler((object snd, EventArgs eventArgs) =>
                {
                    if ((((System.Windows.Forms.TextBox)snd).Text == "IP") || (((System.Windows.Forms.TextBox)snd).Text == "Login") || (((System.Windows.Forms.TextBox)snd).Text == "Password")) ((System.Windows.Forms.TextBox)snd).Text = "";
                });
                System.Windows.Forms.TextBox ip = new System.Windows.Forms.TextBox(); ip.Left = 10; ip.Top = 10; ip.Width = 230; ip.Height = 30; ip.Click += eventHandler; ip.Text = "IP";
                System.Windows.Forms.TextBox login = new System.Windows.Forms.TextBox(); login.Left = 10; login.Top = 50; login.Width = 230; login.Height = 30; login.Click += eventHandler; login.Text = "Login";
                System.Windows.Forms.TextBox password = new System.Windows.Forms.TextBox(); password.Left = 10; password.Top = 90; password.Width = 230; password.Height = 30; password.Click += eventHandler; password.Text = "Password";
                System.Windows.Forms.TextBox dir = new System.Windows.Forms.TextBox(); dir.Left = 10; dir.Top = 120; dir.Width = 230; dir.Height = 30; dir.Text = serverDir;
                System.Windows.Forms.Button button = new System.Windows.Forms.Button(); button.Left = 10; button.Top = 160; button.Width = 230; button.Height = 30; button.Text = "Submit";
                form.Controls.Add(ip); form.Controls.Add(login); form.Controls.Add(password); form.Controls.Add(dir); form.Controls.Add(button);
                Thread thread = new Thread(new ThreadStart(() => System.Windows.Forms.Application.Run(form)));
                form.AcceptButton = button;
                thread.Start();
                button.Click += (object snd, EventArgs args) =>
                {
                    string
                        oldUrl = client.Url,
                        oldLogin = client.Login,
                        oldPassword = client.Password,
                        oldDir = serverTree.Way;
                    try
                    {
                        if (MessageBox.Show("Change connection settings?", "FTP connection settings", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            if(ip.Text != "IP") client.Url = @"ftp:\\" + ip.Text;
                            if (login.Text != "Login") client.Login = login.Text;
                            if (password.Text != "Password") client.Password = password.Text;
                            if (dir.Text != serverDir) serverDir = dir.Text;
                            this.ip = client.Url;
                            this.login = client.Login;
                            this.password = client.Password;
                            form.Close();
                            Dispatcher.Invoke(() => serverTree.Way = serverDir);
                        }
                        else return;
                    }
                    catch
                    {
                        MessageBox.Show("FTP connection failure: " + "\nIP: " + client.Url + "\nLogin: " + client.Login + "\nPassword: " + client.Password, "Connection failure");
                        client.Url = oldUrl;
                        client.Login = oldLogin;
                        client.Password = oldPassword;
                        serverTree.Way = oldDir;
                    }
                };
            }
            else if ((string)item.Header == "Save")
            {
                if (MessageBox.Show("Change default FTP setting to:" + "\n" + "IP: " + client.Url + "\n" + "Login: " + client.Login + "\n" + "Password: " + client.Password, "Change default settings?", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
                GenerateConfig("config.txt",localDir,serverDir,client.Url,client.Login,client.Password);
            }
            else if ((string)item.Header == "Support")
            {
                if(File.Exists(@"Readme.txt")) MessageBox.Show(File.OpenText("Readme.txt").ReadToEnd(),"File manager");
            }

        }

        public void ListBoxContextMenuClicked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(sender.GetType().ToString()); - output MenuItem
            MenuItem item = (MenuItem)sender;
            if ((string)item.Header == "Copy")
            {
                if (server.SelectedItems.Count > local.SelectedItems.Count) //Copy from server to local
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in server.SelectedItems) names.Add((string)itm.Content);
                    Copy(serverTree, serverTree.Way, names.ToArray());
                }
                else if (server.SelectedItems.Count < local.SelectedItems.Count) //Copy from local to server
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in local.SelectedItems) names.Add((string)itm.Content);
                    Copy(localTree, localTree.Way, names.ToArray());
                }
            }
            else if((string)item.Header == "Move")
            {
                if (server.SelectedItems.Count > local.SelectedItems.Count) //Move from server to local
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in server.SelectedItems) names.Add((string)itm.Content);
                    Move(serverTree, serverTree.Way, names.ToArray());
                }
                else if (server.SelectedItems.Count < local.SelectedItems.Count) //Move from local to server
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in local.SelectedItems) names.Add((string)itm.Content);
                    Move(localTree, localTree.Way, names.ToArray());
                }
            }
            else if ((string)item.Header == "Delete")
            {
                if (server.SelectedItems.Count > local.SelectedItems.Count) //Delete from server
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in server.SelectedItems) names.Add((string)itm.Content);
                    Delete(server,serverTree);
                }
                else if (server.SelectedItems.Count < local.SelectedItems.Count) //Delete from local
                {
                    List<string> names = new List<string>();
                    foreach (ListBoxItem itm in local.SelectedItems) names.Add((string)itm.Content);
                    Delete(local,localTree);
                }
            }
            else if ((string)item.Header == "Make directory")
            {
                if (server.ContextMenu.Items.IndexOf(item) > 0) //Make directory at server
                {
                    MakeDir(serverTree);
                } 
                else if (server.ContextMenu.Items.IndexOf(item) > 0) //Make directory at local
                {
                    MakeDir(localTree);
                }
            }
            else if ((string)item.Header == "Make file")
            {
                if (server.ContextMenu.Items.IndexOf(item) > 0) //Make file at server
                {
                    MakeFile(serverTree);
                }
                else if (server.ContextMenu.Items.IndexOf(item) > 0) //Make file at local
                {
                    MakeFile(localTree);
                }
            }
            else if ((string)item.Header == "Refresh")
            {
                if ((string)item.Tag == "server") serverTree.Refresh();
                else if ((string)item.Tag == "local") localTree.Refresh();
            }
        }

        public void ChangeHomeDirectory()
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Width = 270;
            form.Height = 120;
            form.Text = "Home directory";
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            System.Windows.Forms.TextBox dir = new System.Windows.Forms.TextBox(); dir.Left = 10; dir.Top = 10; dir.Width = 230; dir.Height = 30; dir.Text = localDir; dir.Select(dir.Text.Length, 0);
            System.Windows.Forms.Button button = new System.Windows.Forms.Button(); button.Left = 10; button.Top = 45; button.Width = 230; button.Height = 30; button.Text = "Submit";
            form.Controls.Add(dir); form.Controls.Add(button);
            form.AcceptButton = button;
            Thread thread = new Thread(new ThreadStart(() => System.Windows.Forms.Application.Run(form)));
            thread.Start();
            button.Click += (object snd, EventArgs args) =>
            {
                string oldDir = localTree.Way;
                try
                {
                    localDir = dir.Text;
                    if (localDir[localDir.Length - 1] != '\\') localDir += '\\';
                    Dispatcher.Invoke(()=>localTree.Way = localDir);
                    GenerateConfig("config.txt",localDir,serverDir,ip,login,password);
                    form.Close();
                }
                catch
                {
                    localTree.Way = oldDir;
                }
            };
        }

        public void InitFileTrees()
        {
            server.PreviewMouseDown += (object sender, MouseButtonEventArgs e) => local.SelectedItems.Clear();
            local.PreviewMouseDown += (object sender, MouseButtonEventArgs e) => server.SelectedItems.Clear();
            localTree = new FileTree(local, localDir);
            localTree.TextBox = localWay;
            local.KeyDown += (object sender, KeyEventArgs e) => OnKeyDown(sender, e, localTree);
            client = new FTPClient();
            client.Url = ip;
            client.Login = login;
            client.Password = password;
            client.onError += (object sender, Exception e) => MessageBox.Show(e.Message);
            serverTree = new FileTree(server, serverDir, client);
            serverTree.TextBox = serverWay;
            server.KeyDown += (object sender, KeyEventArgs e) => OnKeyDown(sender, e, serverTree);
            serverTree.AddSubmitButton(copy);
            serverTree.AddSubmitButton(move);
            localTree.AddSubmitButton(copy);
            localTree.AddSubmitButton(move);
            localWay.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key != Key.Enter) return;
                if (!Directory.Exists(localWay.Text))
                {
                    MessageBox.Show("Incorrect way: " + localWay.Text, "Error");
                    localWay.Text = localTree.Way;
                }
                if (localWay.Text[localWay.Text.Length-1] != '\\') localWay.Text += @"\";
                localTree.Way = localWay.Text;
                localTree.Refresh();
                localWay.CaretIndex = localWay.Text.Length;
            };
            serverWay.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if (e.Key != Key.Enter) return;
                string w = serverWay.Text;
                if (w.Contains('\\')) w = w.Substring(0, serverWay.Text.LastIndexOf(@"\"));
                if (w.Contains('\\')) w = w.Substring(0, w.LastIndexOf(@"\"));
                string mask = serverWay.Text;
                if (mask[mask.Length - 1] == '\\') mask = mask.Substring(0, mask.Length - 1);
                if (mask.Contains('\\')) mask = mask.Substring(mask.LastIndexOf(@"\")+1);
                if (!(client.GetListDirectory(w).Contains(mask)||(w==mask)))
                {
                    MessageBox.Show("Incorrect way: " + serverWay.Text, "Error");
                    serverWay.Text = serverTree.Way;
                }
                if (serverWay.Text[serverWay.Text.Length - 1] != '\\') serverWay.Text += @"\";
                serverTree.Way = serverWay.Text;
                serverTree.Refresh();
                serverWay.CaretIndex = serverWay.Text.Length;
            };
            serverTree.onFileSelect += (object sender, FileTreeEventArgs e) =>
            {
                if (e.Button == copy) Copy((FileTree)sender, e.Way, e.Names);
                if (e.Button == move) Move((FileTree)sender, e.Way, e.Names);
            };
            localTree.onFileSelect += (object sender, FileTreeEventArgs e) =>
            {
                if (e.Button == copy) Copy((FileTree)sender, e.Way, e.Names);
                if (e.Button == move) Move((FileTree)sender, e.Way, e.Names);
            };
        }

        public void InitButtons()
        {
            delete.Click += (object sender, RoutedEventArgs e) =>
            {
                if (server.SelectedItems.Count > local.SelectedItems.Count) Delete(server, serverTree);
                else if (server.SelectedItems.Count < local.SelectedItems.Count) Delete(local, localTree);
            };
            mkdir.Click += (object sender, RoutedEventArgs e) =>
            {
                if (server.SelectedItems.Count > local.SelectedItems.Count) MakeDir(serverTree);
                else if (server.SelectedItems.Count < local.SelectedItems.Count) MakeDir(localTree);
            };
        }

        public void InitMenus()
        {
            //Listbox context menu
            //End listbox context menu
            //Main window menu


            //End main window menu
        }

        public void LoadConfig(string way)
        {
            if (!File.Exists(way))
            {
                localDir= @"c:\";
                serverDir = @"A\";
                ip = @"ftp:\\192.168.1.254";
                login = @"admin";
                password = @"admin";
                return;
            }
            ConfigManager configManager = new ConfigManager(way);
            configManager.Open('b', 2);
            configManager.LoadData();
            localDir = configManager.localHomeDir;
            serverDir = configManager.serverHomeDir;
            ip = configManager.FTPIP;
            login = configManager.FTPLogin;
            password = configManager.FTPPassword;
        }

        public void GenerateConfig(string way)
        {
            ConfigManager configManager = new ConfigManager(way);
            configManager.Open('b', 2);
            configManager.FTPIP = @"ftp:\\192.168.1.254";
            configManager.FTPLogin = "admin";
            configManager.FTPPassword = "admin";
            configManager.localHomeDir = @"c:\";
            configManager.serverHomeDir = @"A\";
            configManager.UploadData();
        }

        public void GenerateConfig(string way, string localdir, string serverdir , string ip, string log, string pas)
        {
            ConfigManager configManager = new ConfigManager(way);
            configManager.Open('b', 2);
            configManager.FTPIP = ip;
            configManager.FTPLogin = log;
            configManager.FTPPassword = pas;
            configManager.localHomeDir = localdir;
            configManager.serverHomeDir = serverdir;
            configManager.UploadData();
        }

        public MainWindow()
        {
            //GenerateConfig("config.txt");
            if (File.Exists("tempConfig.txt"))
            {
                LoadConfig("tempConfig.txt");
                File.Delete("tempConfig.txt");
            } else LoadConfig("config.txt");
            InitializeComponent();
            InitFileTrees();
            InitButtons();
            InitMenus();
        }

    }
}
