using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;

namespace ukulele_tab
{
    /// <summary>
    /// Interaction logic for main_page.xaml
    /// </summary>
    public partial class Main_Page : Page
    {
        readonly UkulelePlayer Ukulele = new();
        readonly TabFile NewTabFile = new();
        TextboxDataBinding Txtbox = new TextboxDataBinding { Row0 = "", Row1 = "", Row2 = "", Row3 = "" };
        int CursorIndex = 0;

        //original tuning is gCEA
        List<int> Tuning = new List<int> { 0,0,0,0}; //tuning values for virtual ukulele
        readonly List<string>[] Notes = new List<string>[4]; //const list of notes for display
        List<int> TuningForNotes = new List<int> { 5, 5, 5, 0 }; //tuning values to get notes

        public Main_Page()
        {
            this.DataContext = Txtbox;
            this.FontFamily = new FontFamily("Courier New");
            InitializeNotesList();
            InitializeComponent();
            //initialize labels' content
            ((Label)this.FindName("t0")).Content = Notes[0][TuningForNotes[0]];
            ((Label)this.FindName("t1")).Content = Notes[1][TuningForNotes[1]];
            ((Label)this.FindName("t2")).Content = Notes[2][TuningForNotes[2]];
            ((Label)this.FindName("t3")).Content = Notes[3][TuningForNotes[3]];
        }

        private void InitializeNotesList()
        {
            Notes[0] = new List<string> { "E4", "F4", "F4#", "G4","G4#", "A4", "A4#", "B4", "C5", "C5#", "D5", "D5#", 
                "E5", "F5", "F5#", "G5", "G5#", "A5", "A5#", "B5", "C6", "C6#", "D6", "D6#", "E6", "F6" };
            Notes[1] = new List<string> { "B3", "C4", "C4#", "D4", "D4#", "E4", "F4", "F4#",
                "G4","G4#","A4","A4#","B4","C5", "C5#", "D5", "D5#", "E5", "F5", "F5#",
                "G5", "G5#", "A5", "A5#", "B5", "C6","C6#", "D6" };
            Notes[2] = new List<string> { "G3", "G3#", "A3", "A3#", "B3", "C4", "C4#", "D4", "D4#", "E4", "F4", "F4#",
                "G4","G4#","A4","A4#","B4","C5", "C5#", "D5", "D5#", "E5", "F5", "F5#",
                "G5", "G5#", "A5", "A5#" };
            Notes[3] = new List<string> { "G3", "G3#", "A3", "A3#", "B3", "C4", "C4#", "D4", "D4#", "E4", "F4", "F4#",
                "G4","G4#","A4","A4#","B4","C5", "C5#", "D5", "D5#", "E5", "F5", "F5#", 
                "G5", "G5#", "A5", "A5#", "B5", "C6" };
        }

        private async void NoteClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            string content = (btn.Name).ToString();

            //Task task1 = Task.Run(() => WriteNote(content));
            //await task1;
            WriteNote(content);
            Task task2 = Task.Run(() => PlayNote(content));
            await task2;

        }

        private void PlayNote(string btn)
        {
            ///parse str btn into int str_pos and fret_pos
            int str_pos =0;
            int fret_pos;
            string temp = "";
            //format: "n{str_pos}_{fret_pos}"
            foreach (char c in btn)
            {
                if(c =='n')
                {
                    continue;
                }
                if(c=='_')
                  
                {
                    str_pos = Convert.ToInt32(temp);
                    temp = "";
                    continue;
                }
                temp += c;
            }
            fret_pos = Convert.ToInt32(temp);

            //play note based on parsed str_pos and fret_pos
            Ukulele.PlayNote(str_pos, fret_pos);

        }

        private void WriteNote(string btn)
        {
            ///parse str btn into int str_pos and fret_pos
            int str_pos = 0;
            string fret_pos = "";
            string temp = "";
            //format: "n{str_pos}_{fret_pos}"
            foreach (char c in btn)
            {
                if (c == 'n')
                {
                    continue;
                }
                if (c == '_')

                {
                    str_pos = Convert.ToInt32(temp);
                    temp = "";
                    continue;
                }
                temp += c;
            }
            fret_pos = temp;

            //add new note to tab object
            NewTabFile.AddNote(str_pos, fret_pos, CursorIndex);
            ++CursorIndex;
            NewTabFile.AddSeparatorToTheRight(str_pos, CursorIndex);
            ++CursorIndex;
            //update textbox based on new tab array
            UpdateTxtbox();
        }

        private void PromptForFilename(object sender, EventArgs e)
        {
            //show modal dialog box for user to enter filename
            FilenameInput window = new FilenameInput();
            window.Owner = this.Parent as Window;
            window.InputChanged += SaveTab;
            window.ShowDialog();

        }
        
        private void SaveTab(object sender, DialogInputEventArgs e)
        {
            //get the filename from user's input
            string filename = e.Input;
            //possibly check for invalid chars in the future
            if(filename == "")
            {
                MessageBox.Show("Must enter a file name, try again");
            }
            //save using the filename
            if (!NewTabFile.SaveTab(filename))
            {
                MessageBox.Show("Unable to save the tab, try again.");
            }
            //update textbox if necessary
            UpdateTxtbox();
        }

        private void ClearTab(object sender, EventArgs e)
        {
            NewTabFile.Clear();
            UpdateTxtbox();
        }

        private void UpdateTxtbox()
        {
            //get list representation of tab
            List<string>[] tab = NewTabFile.GetTab();
            
            //update the textboxes
            Txtbox.Row0 = string.Join("", tab[0]);
            Txtbox.Row1 = string.Join("", tab[1]);
            Txtbox.Row2 = string.Join("", tab[2]);
            Txtbox.Row3 = string.Join("", tab[3]);
            
        }
        
        /// <summary>
        /// Handle key events in text box
        /// </summary>
        private void OnKeyDownHandler(object sender, KeyEventArgs key_e)
        {
            //need fix: cursor update
            CursorIndex = ((TextBox)sender).CaretIndex;
            if (key_e.Key == Key.Left)
            {
                if (CursorIndex > 0)
                {
                    --CursorIndex;
                }
                return;
            }
                    
            if (key_e.Key == Key.Right)
            {
                ++CursorIndex;
                return;
            }
                    
            if (key_e.Key == Key.Space)
            {
                NewTabFile.AddSeparatorToRemainingRows();
                UpdateTxtbox();
                key_e.Handled = true;
                return;
            }
                    
            if(key_e.Key == Key.Back)
            {
                --CursorIndex;
                ///get the string position based on the textbox being typed in
                //format: "n{str_pos}"
                int str_pos = (int)Char.GetNumericValue(((TextBox)sender).Name[1]);

                //delete note based on that index and string position/textbox
                //NewTabFile.DeleteNote(false, CursorIndex, str_pos);
                string text;
                if (str_pos == 0)
                {
                    Txtbox.Row0 = ((TextBox)sender).Text;
                    text = Txtbox.Row0;
                }
                else if (str_pos == 1)
                {
                    Txtbox.Row1 = ((TextBox)sender).Text;
                    text = Txtbox.Row1;
                }
                else if (str_pos == 2)
                {
                    Txtbox.Row2 = ((TextBox)sender).Text;
                    text = Txtbox.Row2;
                }
                else
                {
                    Txtbox.Row3 = ((TextBox)sender).Text;
                    text = Txtbox.Row3;
                }
                NewTabFile.UpdateTabByStringOfRow(str_pos, text);
                
                return;
            }
                   
            //Alt+Backspace
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) // Is Alt key pressed? from StackOverflow as source
            {
                if (key_e.Key == Key.Back)
                {
                    //delete elements in column at that index
                    NewTabFile.DeleteNote(true, CursorIndex);
                    UpdateTxtbox();
                    return;
                }
            }
        }
        
        private void OnTextboxChangedHandler(object sender, TextChangedEventArgs args)
        {
            //get the string position based on the textbox being typed in
            //format: "n{str_pos}"
            int str_pos = (int)Char.GetNumericValue(((TextBox)sender).Name[1]);

            string text;
            if (str_pos == 0)
            {
                Txtbox.Row0 = ((TextBox)sender).Text;
                text = Txtbox.Row0;
            }
            else if (str_pos == 1)
            {
                Txtbox.Row1 = ((TextBox)sender).Text;
                text = Txtbox.Row1;
            }
            else if (str_pos == 2)
            {
                Txtbox.Row2 = ((TextBox)sender).Text;
                text = Txtbox.Row2;
            }
            else
            {
                Txtbox.Row3 = ((TextBox)sender).Text;
                text = Txtbox.Row3;
            }
            NewTabFile.UpdateTabByStringOfRow(str_pos, text);
            return;
        }
         
        private void ChangeTuning(object sender, EventArgs e)
        {
            //parse for string position based on btn clicked
            var btnObj = (Button)sender;
            string btn = (btnObj.Name).ToString();
            int str_pos = 0;
            int tuning = 0;
            //formats: "u{str_pos}", "d{str_pos}" --> u = up, d = down
            foreach (char c in btn)
            {
                if (c == 'u')
                {
                    tuning = 1;
                    continue;
                }else if (c == 'd')
                {
                    tuning = -1;
                    continue;
                }
                str_pos = Convert.ToInt32(c)-48;
            }

            //update tuning list with constraints on how low and high certain strings can be
            int old_tuning = Tuning[str_pos];
            if(str_pos == 1 || str_pos == 2)
            {
                if(Tuning[str_pos]+tuning > 5 || Tuning[str_pos]+tuning < -5)
                {
                    return;
                }
            }
            else if (str_pos == 0)
            {
                if (Tuning[str_pos]+tuning > 3 || Tuning[str_pos]+tuning < -5)
                {
                    return;
                }
            }
            else if (str_pos == 3)
            {
                if (Tuning[str_pos]+tuning > 12 || Tuning[str_pos]+tuning < 0)
                {
                    return;
                }
            }
            Tuning[str_pos] += tuning;
            TuningForNotes[str_pos] += tuning;

            //update ukulele
            UpdateUkulele(str_pos, old_tuning, Tuning[str_pos]);
        }

        private void UpdateUkulele(int str_pos, int old_tuning, int new_tuning)
        {
            //change text of ukulele buttons
            string btn_name = "n" + str_pos + "_";
            for(int i=0; i<18; ++i)
            {
                var btn = (Button)this.FindName(btn_name+i);
                btn.Content = Notes[str_pos][i + TuningForNotes[str_pos]];
            }
            //change text of tuning label
            ((Label)this.FindName("t"+str_pos)).Content = Notes[str_pos][TuningForNotes[str_pos]];

            //change sound of ukulele
            Ukulele.ChangeTuning(str_pos, new_tuning-old_tuning);
        }
    }

    public class TextboxDataBinding : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string row0 = "";
        public string Row0
        {
            get{ return row0; }
            set { row0 = value;
                NotifyPropertyChanged("Row0");
            }
        }

        private string row1 = "";
        public string Row1
        {
            get{ return row1; }
            set { row1 = value;
                NotifyPropertyChanged("Row1");
            }
        }

        private string row2 = "";
        public string Row2
        {
            get{ return row2; }
            set { row2 = value;
                NotifyPropertyChanged("Row2");
            }
        }
        private string row3 = "";
        public string Row3
        {
            get{ return row3; }
            set { row3 = value;
                NotifyPropertyChanged("Row3");
            }
        }
    }
}
