using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ3_Client
{
    public class Put
    {
        private int id;
        private string broj;
        private string tip;
        private string image;
        private double value;

        public int Id { get => id; set => id = value; }
        public string Broj { get => broj; set => broj = value; } //naziv puta
        public string Tip { get => tip; set => tip = value; }
        public string Image { get => image; set => image = value; }
        public double Value { get => value; set => this.value = value; }

        //dodaj valuie

        public Put()
        { }

        public Put(int i,string br,string t,string im)
        {
            Id = i;
            Broj = br;
            Tip = t;
            Image = im;
            Value = 10000;
        }
    }
}
