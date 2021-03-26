using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace File_manager
{
    public class FileTreeEventArgs
    {
        //public string[] Ways { get; set; }
        public string Way { get; set; }
        public string[] Names { get; set; }

        public Button Button { get; set; }

        public FileTreeEventArgs()
        {

        }

        public FileTreeEventArgs(string way, string name)
        {
            //Ways = new string[1] { way };
            Way = way;
            Names = new string[1] { name };
        }

    }

    public class FileTree
    {
        SolidColorBrush
            dirColor = Brushes.DarkRed,
            fileColor = Brushes.MidnightBlue;


        public delegate void FileSelect(object sender, FileTreeEventArgs eventArgs);
        public event FileSelect onFileSelect;  
        public bool IsFTP { get; set; }
        public bool LineColored { get; set; }
        public FTPClient Client { get; set; }
        public ListBox ListBox { get; set; }
        public TextBox TextBox {
            get { return textBox; } 
            set { textBox = value; textBox.Text = way; } }
        public string Way
        {
            get
            {
                return way;
            }
            set
            {
                way = value;
                Refresh();
            }
        }
        private string way = @"c:\";
        private string root;
        private TextBox textBox;
        //int index;
        public FileTree(ListBox listBox, string way)
        {
            //Initialization
            ListBox = listBox;
            ListBox.SelectionMode = SelectionMode.Extended;
            IsFTP = false;
            this.way = way;
            //Handling events
            ListBox.MouseDoubleClick += onMuseClick;
            //End initialization
            PutDirectoryInListBox(way, IsFTP);
        }

        public FileTree(ListBox listBox, string way, FTPClient client)
        {
            //Initialization
            ListBox = listBox;
            ListBox.SelectionMode = SelectionMode.Extended;
            IsFTP = true;
            Client = client;
            this.way = way;
            root = way;
            //Handling events
            ListBox.MouseDoubleClick += onMuseClick;
            //End initialization
            PutDirectoryInListBox(way, IsFTP);
        }

        public void AddSubmitButton(Button button)
        {
            button.Click += SubmitButtonClick;
        }

        public void RemoveSubmitButton(Button button)
        {
            button.Click -= SubmitButtonClick;
        }

        public void Refresh()
        {
            PutDirectoryInListBox(way, IsFTP);
        }

        public void RemoveData(string name)
        {
            //Удаление данных с диска\сервера
            if (!IsFTP) //С диска
            {
                if (File.Exists(way+name)) File.Delete(way + name);
                else if (Directory.Exists(way+name)) Directory.Delete(way + name, true);
            } else
            { //С FTP сервера
                if (Client.GetFiles(way).Contains(name)) Client.DeleteFileAsync(way + name);
                else if (Client.GetDirectories(way).Contains(name)) Client.DeleteDirectoryAsync(way + name);
            }
            //Удаление строки из listBox'а
            ListBox.Items.Remove(name);
        }

        private int PutDirectoryInListBox(string dir)
        {
            if (dir.IndexOf(".") > 0) return -2; //Попытка открыть файл вместо папки
            if (!Directory.Exists(dir)) return -1; //Попытка открыть несуществующую папку
            ListBox.Items.Clear();
            if (Directory.GetDirectoryRoot(way) != way) ListBox.Items.Add(new ListBoxItem() { Content="...", FontWeight = FontWeights.Bold });
            //MessageBox.Show(Directory.GetDirectoryRoot(way));
            foreach (string dirName in Directory.GetDirectories(dir)) ListBox.Items.Add(new ListBoxItem(){Content = dirName.Substring(dirName.LastIndexOf(@"\") + 1),Foreground = dirColor, FontWeight = FontWeights.Bold});
            foreach (string fName in Directory.GetFiles(dir)) ListBox.Items.Add(new ListBoxItem() { Content = fName.Substring(fName.LastIndexOf(@"\") + 1), Foreground = fileColor, FontWeight = FontWeights.SemiBold });
            if (TextBox != null) TextBox.Text = way;
            return 0; //0 - succes
        }

        private int PutDirectoryInListBox(string dir, bool b)
        {
            if (!b) return PutDirectoryInListBox(dir); //Если подключение через FTP, то выполняется данный метод, если подключение к локальному диску, то выполняется перегруженный выше метод(dir)
            if (dir.IndexOf(".") > 0) return -2; //Попытка открыть файл вместо папки
            if (Client.GetDirectories(way).Contains(dir)) return -1; //Попытка открыть несуществующую папку
            ListBox.Items.Clear();
            if (way.Substring(0, way.LastIndexOf(@"\")+1) != root) ListBox.Items.Add(new ListBoxItem() { Content = "...", FontWeight = FontWeights.Bold });
            foreach (string dirName in Client.GetDirectories(dir)) ListBox.Items.Add(new ListBoxItem() { Content = dirName.Substring(dirName.LastIndexOf(@"\") + 1), Foreground = dirColor, FontWeight = FontWeights.Bold });
            foreach (string fName in Client.GetFiles(dir))
                if(fName!="..") ListBox.Items.Add(new ListBoxItem() { Content = fName.Substring(fName.LastIndexOf(@"\") + 1), Foreground = fileColor, FontWeight = FontWeights.SemiBold });
            if (TextBox != null) TextBox.Text = way;
            return 0; //0 - succes
        }

        public void Submit()
        {
            SubmitButtonClick(this, new EventArgs());
        }

        public void Submit(Button button)
        {
            SubmitButtonClick(button, new EventArgs());
        }

        private void SubmitButtonClick(object sender, EventArgs e)
        {
            List<string> ways = new List<string>();
            List<string> names = new List<string>();
            string list = "";
            //foreach (string fWay in ListBox.SelectedItems) ways.Add(fWay);
            foreach (ListBoxItem item in ListBox.SelectedItems)
            {
                string fName = (string)item.Content;
                names.Add(fName.Substring(fName.LastIndexOf(@"\") + 1));
                list += "\n" + fName;
            }
            if (names.Count == 0) return;
            MessageBoxResult result = MessageBox.Show("Загрузить файлы: " + list + " ?", "Загрузка файла", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) onFileSelect?.Invoke(this, new FileTreeEventArgs() { Way = way, Names = names.ToArray(), Button = (Button)sender });
        }

        private void onMuseClick(object sender, EventArgs e)
        {
            if (ListBox.SelectedIndex == -1) return;
            string selectedItem = (string)((ListBoxItem)ListBox.SelectedItem).Content;
            if (selectedItem == null) return;            
            if (selectedItem == "...")
            {
                if (way.LastIndexOf(@"\") == -1) return;
                way = way.Substring(0, way.LastIndexOf(@"\"));
                if (way.LastIndexOf(@"\") == -1)
                {
                    way += @"\";
                    return;
                }
                way = way.Substring(0, way.LastIndexOf(@"\"));
                way += @"\";
                PutDirectoryInListBox(way, IsFTP);
            }
            else if (((ListBoxItem)ListBox.SelectedItem).Foreground == fileColor)
            {
                MessageBoxResult result = MessageBox.Show("Загрузить файл " + selectedItem  + " ?", "Загрузка файла", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) onFileSelect?.Invoke(this, new FileTreeEventArgs(way,selectedItem));
            }
            else
            {
                way += selectedItem + @"\";
                PutDirectoryInListBox(way, IsFTP);
            }
        }

    }
}
