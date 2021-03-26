using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace File_manager
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    { 
        protected override void OnStartup(StartupEventArgs e)
        {
            if ((e.Args!=null)&&(e.Args.Length>4))
            ConfigManager.UploadData("tempConfig.txt",2,'b',e.Args);
        }



    }
}
