using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace File_manager
{
    class Notepad
    {
        public Stream Stream { get; set; }
        public bool Readonly { 
            get {
                return textReadonly;
            } 
            set {
                textReadonly = value;
                notepadWindow.text.IsReadOnly = textReadonly;
                notepadWindow.text.IsReadOnlyCaretVisible = true;
                if (!textReadonly)
                {
                    Grid.SetRowSpan(notepadWindow.text, 1);
                    notepadWindow.menu.Visibility = Visibility.Visible;
                }
                else
                {
                    Grid.SetRowSpan(notepadWindow.text, 2);
                    notepadWindow.menu.Visibility = Visibility.Collapsed;
                }
            } 
        }

        public delegate void SaveSubmit(string text);
        public event SaveSubmit onSaveSubmit;
        NotepadWindow notepadWindow;
        private bool saved;
        private string text;
        private bool textReadonly = false;

        public Notepad()
        {
            notepadWindow = new NotepadWindow();
            notepadWindow.text.TextChanged += (object sender, TextChangedEventArgs e) => saved = false;
            saved = true;
            notepadWindow.KeyDown += (object sender, KeyEventArgs e) =>
            {
                 if ((e.KeyboardDevice.Modifiers == ModifierKeys.Control)&&(e.Key == Key.S))
                 {
                     saved = true;
                     onSaveSubmit?.Invoke(notepadWindow.text.Text);
                 }
                 
            };
            foreach (MenuItem item in ((MenuItem)notepadWindow.menu.Items[0]).Items) item.Click += (object sender, RoutedEventArgs e) =>
            {
                MenuItem itm = (MenuItem)sender;
                switch ((string)itm.Header)
                {
                    case ("New"):

                        break;

                    case ("Open"):

                        break;

                    case ("Save"):
                        saved = true;
                        onSaveSubmit?.Invoke(notepadWindow.text.Text);
                        break;

                    case ("Save as"):

                        break;

                    case ("Close"):
                        notepadWindow.Close();
                        break;
                }
            };
            notepadWindow.Closing += (object sender, CancelEventArgs e) =>
            {
                if (saved) return;
                switch (MessageBox.Show("Save file?", "Closing", MessageBoxButton.YesNoCancel))
                {
                    case (MessageBoxResult.Yes):
                        onSaveSubmit?.Invoke(notepadWindow.text.Text);
                        break;
                    case (MessageBoxResult.No):

                        break;
                    case (MessageBoxResult.Cancel):
                        e.Cancel = true;
                        break;
                }
                notepadWindow.text.Text = "";
                notepadWindow = null;
            };
            notepadWindow.Title = "Notepad";
            notepadWindow.Show();
        }

        public Notepad(Stream stream):this()
        {
            int d = 0;
            List<byte> bytes = new List<byte>();
            while ((d = stream.ReadByte()) != -1) bytes.Add((byte)d);
            text = ASCIIEncoding.ASCII.GetString(bytes.ToArray());
            notepadWindow.text.Text = text;
            bytes.Clear();
            saved = true;
        }

        public Notepad(byte[] bytes):this()
        {
            text = ASCIIEncoding.ASCII.GetString(bytes.ToArray());
            notepadWindow.text.Text = text;
            saved = true;
        }

    }
}
