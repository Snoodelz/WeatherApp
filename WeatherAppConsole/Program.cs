using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WeatherAppConsole.Models;

namespace WeatherAppConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            AddDataToDB();
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
                Console.ReadLine();
            }
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("Väderdata app, välj ett val med tangentbordet.");
            Console.WriteLine("Inomhus:");
            Console.WriteLine("1) Varmaste dagen");
            Console.WriteLine("2) Torraste dagen");
            Console.WriteLine("3) Sök medeltemp för dag");
            Console.WriteLine("4) Risk för mögel");
            Console.WriteLine("Utomhus:");
            Console.WriteLine("5) Varmaste dagen");
            Console.WriteLine("6) Torraste dagen");
            Console.WriteLine("7) Höstdagar");
            Console.WriteLine("8) Vinterdagar");
            Console.WriteLine("9) Sök medeltemp för dag");
            Console.WriteLine("10) Risk för mögel");
            Console.WriteLine("0) Avsluta");
            Console.Write("\r\nVälj ett val: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.Clear();
                    HottestDayIndoors();
                    return true;
                case "2":
                    Console.Clear();
                    DriestDayIndoors();
                    return true;
                case "3":
                    Console.Clear();
                    SearchAverageTemperature(true);
                    return true;
                case "4":
                    Console.Clear();
                    RiskOfMoldIndoors();
                    return true;
                case "5":
                    Console.Clear();
                    HottestDayOutdoors();
                    return true;
                case "6":
                    Console.Clear();
                    DriestDayOutdoors();
                    return true;
                case "7":
                    Console.Clear();
                    AutumnDates();
                    return true;
                case "8":
                    Console.Clear();
                    WinterDates();
                    return true;
                case "9":
                    Console.Clear();
                    SearchAverageTemperature(false);
                    return true;
                case "10":
                    Console.Clear();
                    RiskOfMoldOutdoors();
                    return true;
                case "0":
                    return false;
                default:
                    return true;
            }
        }

        static void AddDataToDB()
        {
            using (var db = new EFContext())
            {
                if (db.Outdoors.Any())
                {
                    //Console.Write("Db already exists!");
                    return;
                }

            }

            List<Indoor> indoorList = new List<Indoor>();
            List<Outdoor> outdoorList = new List<Outdoor>();
            using (var reader = new StreamReader("Data\\TemperaturData.csv"))
            {
                var provider = new CultureInfo("en-US");

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values[1] == "Inne")
                    {
                        var indoorData = new Indoor();
                        indoorData.Date = DateTime.Parse(values[0]);
                        indoorData.Temperature = double.Parse(values[2], provider);
                        indoorData.Humidity = int.Parse(values[3]);
                        indoorList.Add(indoorData);
                    }
                    else
                    {
                        var outdoorData = new Outdoor();
                        outdoorData.Date = DateTime.Parse(values[0]);
                        outdoorData.Temperature = double.Parse(values[2], provider);
                        outdoorData.Humidity = int.Parse(values[3]);
                        outdoorList.Add(outdoorData);
                    }
                }
            }

            using (var db = new EFContext())
            {
                Console.Write("Working...");
                db.Indoors.AddRange(indoorList);
                db.Outdoors.AddRange(outdoorList);
                db.SaveChanges();
                Console.WriteLine("Finished.");
            }
        }

        public static void SearchAverageTemperature(bool indoors)
        {
            using (var db = new EFContext())
            {
                int year;
                int month;
                int day;
                Console.WriteLine("Ange årtal: ");
                year = int.Parse(Console.ReadLine());
                Console.WriteLine("Ange månad: ");
                month = int.Parse(Console.ReadLine());
                Console.WriteLine("Ange dag: ");
                day = int.Parse(Console.ReadLine());
                double medelTemp = 0;
                //avrunda dessa till en decimal.
                try
                {
                    if (indoors)
                    {
                        medelTemp = db.Indoors
                    .Where(d => d.Date.Year == year)
                    .Where(d => d.Date.Month == month)
                    .Where(d => d.Date.Day == day)
                    .Select(d => d.Temperature).Average();
                    }
                    else
                    {
                        medelTemp = db.Outdoors
                    .Where(d => d.Date.Year == year)
                    .Where(d => d.Date.Month == month)
                    .Where(d => d.Date.Day == day)
                    .Select(d => d.Temperature).Average();
                    }
                    Console.WriteLine("Medeltemperatur: " + medelTemp);
                }
                catch (Exception)
                {
                    Console.WriteLine("Inte ett korrekt datum.");
                }

            }
        }

        public static void HottestDayIndoors()
        {
            using (var db = new EFContext())
            {
                var averageTempDay =
                    from weatherData in db.Indoors
                    group weatherData by weatherData.Date.Date into dayTemp
                    select new
                    {
                        Date = dayTemp.Key,
                        AverageTemp = Math.Round(dayTemp.Average(x => x.Temperature), 1),
                    };

                var SortedList = averageTempDay.OrderByDescending(o => o.AverageTemp).ToList();


                foreach (var item in SortedList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageTemp + "c");
                }


            }
        }
        public static void DriestDayIndoors()
        {
            using (var db = new EFContext())
            {
                var averageHumidityDay =
                    from weatherData in db.Indoors
                    group weatherData by weatherData.Date.Date into dayHumidity
                    select new
                    {
                        Date = dayHumidity.Key,
                        AverageHumidity = Math.Round(dayHumidity.Average(x => x.Humidity), 1),
                    };


                var SortedList = averageHumidityDay.OrderBy(o => o.AverageHumidity).ToList();


                foreach (var item in SortedList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageHumidity + " luftfuktighet");
                }

            }
        }
        public static void RiskOfMoldIndoors()
        {
            using (var db = new EFContext())
            {
                var averageDayWeather =
                    from weatherData in db.Indoors
                    group weatherData by weatherData.Date.Date into dayData
                    select new
                    {
                        Date = dayData.Key,
                        MoldRisk = ((dayData.Average(x => x.Humidity) - 78) * (dayData.Average(x => x.Temperature) / 15)) / 0.22,
                    };

                var sortedList = averageDayWeather.OrderBy(o => o.MoldRisk).ToList();

                foreach (var item in sortedList)
                {
                    double moldPercentage = 0;

                    if (item.MoldRisk > 100)
                        moldPercentage = 100;
                    else if (item.MoldRisk < 0)
                        moldPercentage = 0;
                    else
                        moldPercentage = item.MoldRisk;

                    Console.WriteLine(item.Date.ToShortDateString() + " " + Math.Round(moldPercentage, 1) + "%");
                }
            }


        }


        public static void HottestDayOutdoors()
        {
            using (var db = new EFContext())
            {
                var averageTempDay =
                    from weatherData in db.Outdoors
                    group weatherData by weatherData.Date.Date into dayTemp
                    select new
                    {
                        Date = dayTemp.Key,
                        AverageTemp = Math.Round(dayTemp.Average(x => x.Temperature), 1),
                    };

                var SortedList = averageTempDay.OrderByDescending(o => o.AverageTemp).ToList();


                foreach (var item in SortedList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageTemp + "c");
                }


            }
        }
        public static void DriestDayOutdoors()
        {
            using (var db = new EFContext())
            {
                var averageHumidityDay =
                    from weatherData in db.Outdoors
                    group weatherData by weatherData.Date.Date into dayHumidity
                    select new
                    {
                        Date = dayHumidity.Key,
                        AverageHumidity = Math.Round(dayHumidity.Average(x => x.Humidity), 1),
                    };


                var SortedList = averageHumidityDay.OrderBy(o => o.AverageHumidity).ToList();


                foreach (var item in SortedList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageHumidity + " luftfuktighet");
                }

            }
        }
        public static void RiskOfMoldOutdoors()
        {
            using (var db = new EFContext())
            {
                //skapar en lista med medeltemperaturen och fuktigheten för o sen kolla mögelrisken.
                var averageDayWeather =
                    from weatherData in db.Outdoors
                    group weatherData by weatherData.Date.Date into dayData
                    select new
                    {
                        Date = dayData.Key,
                        //MoldRisk = ((14 - 78) * (22 / 15)) / 0.22,
                        MoldRisk = ((dayData.Average(x => x.Humidity) - 78) * (dayData.Average(x => x.Temperature) / 15)) / 0.22,
                    };

                var sortedList = averageDayWeather.OrderBy(o => o.MoldRisk).ToList();

                foreach (var item in sortedList)
                {
                    double moldPercentage = 0;

                    if (item.MoldRisk > 100)
                        moldPercentage = 100;
                    else if (item.MoldRisk < 0)
                        moldPercentage = 0;
                    else
                        moldPercentage = item.MoldRisk;

                    Console.WriteLine(item.Date.ToShortDateString() + " " + Math.Round(moldPercentage, 1) + "%");
                }
            }


        }

        public static void AutumnDates()
        {
            using (var db = new EFContext())
            {
                //skapar en lista med medeltemperaturen för varje dag i databasen.
                var averageTempDay =
                    from weatherData in db.Outdoors
                    group weatherData by weatherData.Date.Date into dayTemp
                    select new
                    {
                        Date = dayTemp.Key,
                        AverageTemp = Math.Round(dayTemp.Average(x => x.Temperature), 1),
                    };


                //Gör en linq med where där det är lägre än 10,0 plusgrader men högre än 0,0°.

                var autumnList = averageTempDay.Where(d => d.AverageTemp < 10 && d.AverageTemp > 0);

                foreach (var item in autumnList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageTemp + "c");
                }

            }
        }
        public static void WinterDates()
        {
            using (var db = new EFContext())
            {
                //skapar en lista med medeltemperaturen för varje dag i databasen.
                var averageTempDay =
                    from weatherData in db.Outdoors
                    group weatherData by weatherData.Date.Date into dayTemp
                    select new
                    {
                        Date = dayTemp.Key,
                        AverageTemp = Math.Round(dayTemp.Average(x => x.Temperature), 1),
                    };


                //Gör en linq med where där det är lägre än 0,0 grader.
                var winterList = averageTempDay.Where(d => d.AverageTemp < 0);


                foreach (var item in winterList)
                {
                    Console.WriteLine(item.Date.ToShortDateString() + " " + item.AverageTemp + "c");
                }


            }

        }
    }
}

