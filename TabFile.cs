using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ukulele_tab
{
    /// <summary>
    /// Class to store and interact with tab as written by virtual ukulele or writing in textbox
    /// </summary>
    internal class TabFile
    {
        //store tab as matrix of char because can store each digit as a char
        //and easier to maintain the evenness of each line when adding separators to remaining rows (for double digits fret)
        List<char>[] TabArray_ = new List<char>[4];
        
        public TabFile()
        {
            for(int i = 0; i < 4; ++i)
            {
                TabArray_[i] = new List<char> ();
            }
        }

        /// <summary>
        /// Clear all notes/symbols in the tab
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < 4; ++i)
            {
                TabArray_[i].Clear();
            }
        }

        /// <summary>
        /// Save tab to a txt file
        /// </summary>
        /// <param name="filename">Name of txt file (without .txt) </param>
        /// <returns>True if saved successfully; False otherwise</returns>
        public bool SaveTab(string filename) 
        {
            // https://stackoverflow.com/questions/816566/how-do-you-get-the-current-project-directory-from-c-sharp-code-when-creating-a-c
            // get the current WORKING directory (i.e. \bin\Debug)
            string workingDirectory = Environment.CurrentDirectory;
            // get the current PROJECT directory
            string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            if(dir == null)
            {
                Console.WriteLine("error");
            }
            string path = dir + @"\tabs\"+filename+".txt";

            //pad the TabArrays to make each string array have even char count
            this.AddSeparatorToRemainingRows();
            int longest_tab_length = TabArray_[0].Count;
            for(uint i = 1; i<4; ++i)
            {
                if (TabArray_[i].Count > longest_tab_length)
                {
                    longest_tab_length = TabArray_[i].Count;
                }
            }
            
            try
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    //100 chars each line
                    int start_idx = 0;
                    int end_idx = 100;
                    while(start_idx< longest_tab_length)
                    {
                        for (uint str = 0; str < 4; ++str)
                        {
                            string line = "";
                            int idx = start_idx;
                            for (int i = start_idx; i < Math.Min(end_idx, TabArray_[str].Count); ++i)
                            {
                                line += TabArray_[str][i];

                            }
                            sw.WriteLine(line);
            
                        }
                        sw.WriteLine();

                        if (longest_tab_length - end_idx >= 100)
                        {
                            start_idx = end_idx+1;
                            end_idx += 100;
                        }
                        else
                        {
                            start_idx = end_idx+1;
                            end_idx = longest_tab_length;
                        }
                    }

                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;

        }
        
        /// <summary>
        /// Delete a note/symbol in the tab
        /// </summary>
        /// <param name="whole_col">bool of whether to delete a whole column of symbols; True if yes; False otherwise</param>
        /// <param name="idx">index of note to be deleted</param>
        /// <param name="str_pos">string position of note to be deleted</param>
        /// Note: need improvement or to delete later on
        public void DeleteNote(bool whole_col, int idx, int str_pos = 0)
        {
            if(whole_col)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if(idx < TabArray_[i].Count())
                    {
                        TabArray_[i].RemoveAt(idx);
                    }
                    
                }
            }
            else
            {
                if(idx < TabArray_[str_pos].Count())
                {
                    TabArray_[str_pos].RemoveAt(idx);
                }
            }
            
        }

        /// <summary>
        /// Add a note/symbol to the tab
        /// </summary>
        /// <param name="str_pos">string position of note to be added</param>
        /// <param name="fret_pos">the symbol/note to be added (a note is based on fret position of ukulele)</param>
        /// <param name="cur_idx"></param>
        public void AddNote(int str_pos, string fret_pos, int cur_idx = 0)
        {
            //split the note into digits if needed before adding to tab
            if (cur_idx <= TabArray_[str_pos].Count())
            {
                TabArray_[str_pos].Insert(cur_idx, fret_pos.ElementAt(0));
                if (fret_pos.Length > 1)
                {
                    TabArray_[str_pos].Insert(cur_idx+1, fret_pos.ElementAt(1));
                }
            }
            else
            {
                TabArray_[str_pos].Add(fret_pos.ElementAt(0));
                if (fret_pos.Length > 1)
                {
                    TabArray_[str_pos].Add(fret_pos.ElementAt(1));
                }
            }
            
        }

        /// <summary>
        /// Add a separator (a dash) to the right of the given index
        /// </summary>
        /// <param name="str_pos">string position of the separator to be added</param>
        /// <param name="cur_idx">index of the separator to be added</param>
        public void AddSeparatorToTheRight(int str_pos, int cur_idx)
        {
            if (cur_idx <= TabArray_[str_pos].Count())
            {
                TabArray_[str_pos].Insert(cur_idx, '-');
            }
            else
            {
                TabArray_[str_pos].Add('-');
            }
        }

        /// <summary>
        /// Add separtors to all rows up to the longest row in the tab (to even out the tab)
        /// </summary>
        public void AddSeparatorToRemainingRows()
        {
            int longest_row = -1;
            for(int i = 0; i < 4; ++i)
            {
                if (TabArray_[i].Count > longest_row)
                {
                    longest_row = TabArray_[i].Count();
                }
            }
            for(int i = 0; i < 4; ++i)
            {
                while(TabArray_[i].Count() < longest_row)
                {
                    TabArray_[i].Add('-');
                }
            }
            
        }
   
        /// <returns>Tab to be displayed</returns>
        public List<char>[] GetTab()
        {
            return TabArray_;
        }

        /// <summary>
        /// Update tab array using values in the textbox
        /// </summary>
        /// <param name="str_pos">string position of the tab to be updated</param>
        /// <param name="new_tab">new tab to update the tab array</param>
        public void UpdateTabByStringOfRow(int str_pos, string new_tab)
        {
            if (new_tab.Length == 0)
            {
                return;
            }
            //parse the string version of the tab into an array of digits and dashes
            List<char> temp_tab = new List<char>();
            
            for(int i=0; i<new_tab.Length; ++i)
            {
                temp_tab.Add(new_tab[i]);
            }

            //change the tab array to the new tab 
            TabArray_[str_pos] = temp_tab;
        }
    }
}
