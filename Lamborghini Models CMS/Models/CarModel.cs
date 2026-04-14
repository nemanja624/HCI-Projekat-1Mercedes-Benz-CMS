using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercedes_Models_CMS.Models
{
    [Serializable]
    public class CarModel
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }   
        public string RtfFilePath { get; set; }
        public DateTime DateAdded { get; set; }
        public int HorsePower { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
