using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TFTAnalizer
{
    public partial class Form1 : Form
    {
        //loading champs
        private static string _CharactersJsonPath = "D:\\rito\\champions.json";
        private List<Champions> championList = new List<Champions>();
        private List<string> traitsList = new List<string>();

        //gui
        private ColumnClickEventArgs lastColumn;
        private bool reverse;
        private ListView listView1;
        private TextBox search;
        private CheckBox searchId;
        private List<ListViewItem> items;
        private List<CheckBox> traitBoxes;
        private List<string> filterList = new List<string>();
        private CheckBox filterType;


        public Form1()
        {
            InitializeComponent();
            LoadCharacters();
            Form1_Load();
            //DebugDisplayChamps();
        }


        private void DebugDisplayChamps()
        {
            foreach (var champ in championList)
            {
                Debug.WriteLine(champ.ToString());
            }
        }

        private void LoadCharacters()
        {
            JArray o1 = JArray.Parse(File.ReadAllText(_CharactersJsonPath));

            int id = 0;
            foreach (var champion in o1)
            {
                Champions tempChamp = new Champions();

                tempChamp.name = (string) champion["name"];
                tempChamp.cost = int.Parse((string)champion["cost"]);
                tempChamp.id = id;
                foreach (var trait in champion["traits"])
                {
                    string traits = (string) trait;
                    tempChamp.AddTrait(traits);
                    if (!traitsList.Contains(traits))
                    {
                        traitsList.Add(traits);
                    }
                }
                championList.Add(tempChamp);
                //Debug.WriteLine("Załadowano "+tempChamp.ToString());
                id++;
            }
            Debug.WriteLine("Załadowano cały plik danych: "+championList.Count+" postaci");
        }

        private void Form1_Load()
        {
            this.Text = "TFT Character Counter";

            // Create a new ListView control.
            listView1 = new ListView();
            listView1.Dock = DockStyle.Fill;
            this.DockPadding.Top = 100;

            lastColumn = null;
            reverse = false;

            Label lab = new Label();
            lab.Text = "Szukaj:";
            lab.Size = new Size(45, 20);
            lab.Location = new Point(5, 5);
            this.Controls.Add(lab);

            search = new TextBox();
            search.Size = new Size(100, 20);
            search.Location = new Point(50, 5);
            search.TextChanged += Search_TextChanged;
            this.Controls.Add(search);

            searchId = new CheckBox();
            searchId.Text = "Szukaj w id";
            searchId.Size = new Size(80,20);
            searchId.Location = new Point(160,5);
            searchId.Checked = false;
            searchId.CheckStateChanged += searchIdBox_CheckStateChanged;
            this.Controls.Add(searchId);

            filterType = new CheckBox();
            filterType.Text = "filtr or";
            filterType.Size = new Size(80, 20);
            filterType.Location = new Point(250, 5);
            filterType.Checked = false;
            filterType.CheckStateChanged += filterBox_CheckStateChanged;
            this.Controls.Add(filterType);

            int x = 5;
            traitBoxes = new List<CheckBox>();
            foreach (var trait in traitsList)
            {
                CheckBox traitBox = new CheckBox();
                traitBox.Text = trait;
                int textLength = TextRenderer.MeasureText(trait,traitBox.Font).Width+20;
                traitBox.Size = new Size(textLength, 20);
                traitBox.Location = new Point(x, 30);
                traitBox.Checked = false;
                traitBox.CheckStateChanged += checkBoxes_CheckStateChanged;
                traitBoxes.Add(traitBox);
                this.Controls.Add(traitBox);
                x += textLength;
                //Debug.WriteLine("Rozmiar: "+trait.Length+" daje: " + textLength);
            }

            //TODO: sort traits by name

            ResizeTraits();

            // Set the view to show details.
            listView1.View = View.Details;
            // Allow the user to edit item text.
            listView1.LabelEdit = true;
            // Allow the user to rearrange columns.
            listView1.AllowColumnReorder = true;
            // Display check boxes.
            listView1.CheckBoxes = true;
            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
            // Display grid lines.
            listView1.GridLines = true;
            // Sort the items in the list in ascending order.
            listView1.Sorting = SortOrder.Ascending;


            //dodawanie postaci do listy
            items = new List<ListViewItem>();

            foreach (var champ in championList)
            {
                ListViewItem temp = new ListViewItem(champ.id.ToString(), 0);

                temp.SubItems.Add(champ.name);
                temp.SubItems.Add(champ.cost.ToString());
                foreach (var trait in champ.traits)
                {
                    temp.SubItems.Add(trait);
                }
                //Debug.WriteLine("Dodaję do listView "+temp.ToString());
                champ.ListViewItem = temp;
                items.Add(temp);
            }

            // Create columns for the items and subitems.
            // Width of -2 indicates auto-size.
            listView1.Columns.Add("Id", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Nazwa", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Koszt", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cecha 1", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cecha 2", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Cecha 3", -2, HorizontalAlignment.Left);

            listView1.Columns.Add("Max ilość", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Aktualna ilość", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Odejmij 1", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Odejmij 2", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Odejmij 3", -2, HorizontalAlignment.Left);

            //Add the items to the ListView.
            listView1.Items.AddRange(items.ToArray());

            // Add on click listener to column headers
            listView1.ColumnClick += new ColumnClickEventHandler(this.listView1_ColumnClick);

            // Add the ListView to the control collection.
            this.Controls.Add(listView1);
        }

        private void searchIdBox_CheckStateChanged(object sender, EventArgs e)
        {
            SearchList();
        }

        private void filterBox_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            if (box.Checked)
            {
                box.Text = "filtr and";
            }
            else
            {
                box.Text = "filtr or";
            }
            SearchList();
        }

        void checkBoxes_CheckStateChanged(object sender, EventArgs e)
        { //do stuff when check state changed
            CheckBox box = (CheckBox) sender;
            if (box.Checked)
            {
                filterList.Add(box.Text);
            }
            else
            {
                filterList.Remove(box.Text);
            }

            SearchList();
        }

        private void ResizeTraits()
        {
            int x = 5;
            int maxx = this.Size.Width-5;
            int y = 30;

            foreach (var traitBox in traitBoxes)
            {
                int textLength = TextRenderer.MeasureText(traitBox.Text, traitBox.Font).Width + 20;
                if (x + textLength > maxx)
                {
                    x = 5;
                    y += 25;
                }
                traitBox.Size = new Size(textLength, 20);
                traitBox.Location = new Point(x, y);
                x += textLength;
            }
            this.DockPadding.Top = y + 20;
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < 5)
            {
                if (lastColumn != null)
                {
                    // Determine if clicked column is already the column that is being sorted.
                    if (e.Column == lastColumn.Column && reverse == false)
                    {
                        if (e.Column == 0 || e.Column == 2)
                        {
                            // Reverse the current sort direction for this column.
                            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 2);
                            reverse = true;
                        }
                        else
                        {
                            // Reverse the current sort direction for this column.
                            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 1);
                            reverse = true;
                        }
                    }
                    else
                    {
                        if (e.Column == 0 || e.Column == 2)
                        {
                            // Set the column number that is to be sorted; default to ascending.
                            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 3);
                            reverse = false;
                        }
                        else
                        {
                            // Set the column number that is to be sorted; default to ascending.
                            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 0);
                            reverse = false;
                        }
                    }
                }
                else
                {
                    if (e.Column == 0 || e.Column == 2)
                    {
                        // Set the column number that is to be sorted; default to ascending.
                        listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 3);
                        reverse = false;
                    }
                    else
                    {
                        // Set the column number that is to be sorted; default to ascending.
                        listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, 0);
                        reverse = false;
                    }
                }

                // Perform the sort with these new sort options.
                this.lastColumn = e;
            }
        }

        private void Search_TextChanged(object sender, System.EventArgs e)
        {
            SearchList();
        }

        private void SearchList()
        {
            listView1.Items.Clear();
            listView1.Items.AddRange(items.ToArray());
            
            int searchtype = 0;

            if (search.Text != "") searchtype += 4;
            if (filterList.Count > 0)
            {
                searchtype += 2;
                if (filterType.Checked)
                    searchtype += 1;
            }

            Debug.WriteLine("Tryb sortowania " + searchtype);

            if (searchtype == 2 || searchtype == 3 || searchtype == 4 || searchtype == 6 || searchtype == 7)
            {

                foreach (var champ in championList)
                {
                    bool addThisOne = false;
                    switch (searchtype)
                    {
                        case 2:
                            // > 0 or
                            addThisOne = OrCase(champ);
                            break;

                        case 3:
                            // > 0 and 
                            addThisOne = AndCase(champ);

                            break;
                        case 4:
                            //!=""
                            addThisOne = StringCase(champ);

                            break;
                        case 6:
                            // "" >0 or
                            if (StringCase(champ) && OrCase(champ))
                            {
                                addThisOne = true;
                            }
                            break;

                        case 7:
                            // "" >0 and
                            if (StringCase(champ) && AndCase(champ))
                            {
                                addThisOne = true;
                            }
                            break;
                        default:
                            Debug.WriteLine("Tryb sortowania nielegalny, jak tu wlazło xD ");
                            listView1.Items.Clear();
                            listView1.Items.AddRange(items.ToArray());
                            break;
                    }

                    if (!addThisOne)
                    {
                        listView1.Items.Remove(champ.ListViewItem);
                    }
                }
            }
            else
            {
                Debug.WriteLine("Tryb sortowania nielegalny, no problem ");
            }
        }

        private bool StringCase(Champions champ)
        {
            if (searchId.Checked)
            {
                foreach (ListViewItem.ListViewSubItem subItem in champ.ListViewItem.SubItems)
                {
                    if (subItem.Text.ToLower().Contains(search.Text.ToLower()))
                    {
                        return true;
                    }
                }
            }
            else
            {
                //TODO: cannot find shit
                for (int i = champ.ListViewItem.SubItems.Count-1; i < 1; i--)
                {
                    if (champ.ListViewItem.SubItems[i].Text.ToLower().Contains(search.Text.ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool AndCase(Champions champ)
        {
            foreach (var filter in filterList)
            {
                if (!champ.traits.Contains(filter))
                {
                    return false;
                }
            }

            return true;
        }

        private bool OrCase(Champions champ)
        {
            foreach (var filter in filterList)
            {
                if (champ.traits.Contains(filter))
                {
                    return true;
                }
            }

            return false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizeTraits();
        }
    }
}

// Implements the manual sorting of items by columns.
class ListViewItemComparer : IComparer
{
    private int col;
    private int sortType;
    public ListViewItemComparer()
    {
        col = 0;
        sortType = 0;
    }
    public ListViewItemComparer(int column, int sortType)
    {
        this.col = column;
        this.sortType = sortType;
    }
    public int Compare(object x, object y)
    {
        try
        {
            if (this.sortType == 0)
                return String.Compare(((ListViewItem) x).SubItems[col].Text, ((ListViewItem) y).SubItems[col].Text);
            else if (this.sortType == 1)
                return String.Compare(((ListViewItem) y).SubItems[col].Text, ((ListViewItem) x).SubItems[col].Text);
            else if (this.sortType == 2)
            {
                int nx = int.Parse((x as ListViewItem).SubItems[col].Text);
                int ny = int.Parse((y as ListViewItem).SubItems[col].Text);
                return nx.CompareTo(ny);
            }
            else
            {
                int nx = int.Parse((x as ListViewItem).SubItems[col].Text);
                int ny = int.Parse((y as ListViewItem).SubItems[col].Text);
                return ny.CompareTo(nx);
            }
        }
        catch (Exception ex)
        {
            //TODO xD
            return 1.CompareTo(0);
        }
    }
}
