using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ3_Client
{
    public class Slika
    {
        public string imageUri { get; set; }

        public Slika()
        {
            imageUri = string.Empty;
        }

        public Slika(string uri)
        {
            imageUri = uri;
        }
    }
}
