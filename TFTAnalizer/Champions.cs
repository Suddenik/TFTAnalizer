using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TFTAnalizer
{
    internal class Champions
    {
        public string name = "";
        public List<string> traits = new List<string>();
        public int cost;
        public int id;

        public ListViewItem ListViewItem { get; set; }

        public Champions()
        {

        }

        public override string ToString()
        {
            string temp = name + " " + cost.ToString() + " ";
            foreach (var trait in traits)
            {
                temp += trait + " ";
            }

            return temp;
        }

        public Champions(string name, string[] traits,int cost)
        {
            this.name = name;
            foreach (var VARIABLE in traits)
            {
                this.traits.Add(VARIABLE);
            }

            this.cost = cost;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public List<string> Traits
        {
            get => traits;
            set => traits = value;
        }

        public int Cost
        {
            get => cost;
            set => cost = value;
        }

        public void AddTrait(string trait)
        {
            traits.Add(trait);
        }
    }
}