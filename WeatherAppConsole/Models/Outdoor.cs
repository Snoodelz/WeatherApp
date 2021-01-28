using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherAppConsole.Models
{
    class Outdoor
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
