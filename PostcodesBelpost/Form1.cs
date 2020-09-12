using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using xNet;

namespace PostcodesBelpost
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private const string SECURITY = "28deada1170d9443e434f4d50826e968";
        private const string GET_REGIONS_URL = "http://ex.belpost.by/addressbook/get_regions.php";
        private const string GET_CITY_URL = "http://ex.belpost.by/addressbook/get_city.php";
        private const string GET_TYPE_STREET_URL = "http://ex.belpost.by/addressbook/get_typestreet.php";
        private const string GET_STREET_URL = "http://ex.belpost.by/addressbook/get_street.php";
        private const string GET_ZIPCODE_URL = "http://ex.belpost.by/addressbook/get_zipcode.php";
        private const string FILE_NAME_OBLAST = @"..\..\db\oblast.txt";
        private const string FILE_NAME_REGION = @"..\..\db\region.txt";
        private const string FILE_NAME_CITY = @"..\..\db\city.txt";
        private const string FILE_NAME_TYPE_STREET = @"..\..\db\typestreet.txt";
        private const string FILE_NAME_STREET = @"..\..\db\street.txt";
        private const string FILE_NAME_ZIPCODE = @"..\..\db\zipcode.txt";
        private List<string> namesObl = new List<string>()
        {
            "МИНСК",
            "БРЕСТСКАЯ",
            "ВИТЕБСКАЯ",
            "ГОМЕЛЬСКАЯ",
            "ГРОДНЕНСКАЯ",
            "МИНСКАЯ",
            "МОГИЛЕВСКАЯ"
        };
        public class Captcha
        {
            public string Number { get; set; }
            public string Code { get; set; }
        }
        private List<Captcha> Captchas = new List<Captcha>()
        {
            new Captcha() { Code = "FFAyINA%3D", Number = "07972" },
            new Captcha() { Code = "F1c6LtE%3D", Number = "30193" },
            new Captcha() { Code = "EVU9JdY%3D", Number = "52624" },
            new Captcha() { Code = "FVI%2FJNA%3D", Number = "15432" },
            new Captcha() { Code = "E1Y6L9A%3D", Number = "71182" },
            new Captcha() { Code = "EVI4JtA%3D", Number = "55312" },
            new Captcha() { Code = "HV4yJtU%3D", Number = "99917" },
            new Captcha() { Code = "HV88LtA%3D", Number = "98792" },
            new Captcha() { Code = "EVA4Jto%3D", Number = "57318" },
            new Captcha() { Code = "FFA6Its%3D", Number = "07159" },
        };
        private Random random = new Random();
        private Regex regTemplat = new Regex("[(\')\"]");
        private (int, string) oblast;
        private (int, string) region;
        private (int, string) city;
        private (int, string) typeStreet;
        private (int, string) street;

        private int countOblast = 1;
        private int countRegion = 1;
        private int countCity = 1;
        private int countType = 1;
        private int countStreet = 1;
        private int countZipcode = 1;

        private bool addMode = false;
        private void btnLoad_Click(object sender, EventArgs e)
        {
            for (var idObl = 0; idObl < namesObl.Count(); idObl++)
            {
                oblast = (countOblast++, namesObl[idObl]);
                if (addMode)
                {
                    if (File.Exists(FILE_NAME_OBLAST) && !FindOblastInFile())
                        continue;
                }
                else
                    WriteToFileOblast();
                if (oblast.Item2 == "МИНСК")
                {
                    region.Item1 = countRegion++;
                    region.Item2 = oblast.Item2;
                    WriteToFileRegion();
                    region.Item2 = "";
                    GetTypeStreet(new List<City>() { new City() { CityName = oblast.Item2 } } );
                    continue;
                }
                using (var requestReg = new HttpRequest())
                {
                    var oblParams = new RequestParams();
                    oblParams["obl_id"] = namesObl[idObl];
                    oblParams["security"] = SECURITY;
                    var regContent = requestReg.Post(GET_REGIONS_URL, oblParams).ToString();
                    var regTemp = Html.ReplaceUnicode(regContent);
                    var namesRegions = JsonConvert.DeserializeObject<RegionList>(regTemp).Regions;
                    GetCity(namesRegions);
                }
            }
        }
        private void WriteToFileOblast()
        {
            using (var sw = new StreamWriter(FILE_NAME_OBLAST,true))
            {
                sw.WriteLine($"({oblast.Item1},\'{oblast.Item2}\'),");
            }
        }
        private bool FindOblastInFile()
        {
            var lastLine = File.ReadLines(FILE_NAME_OBLAST).Last();
            var temp = lastLine.Split(',');
            if (temp[1].Contains(oblast.Item2))
            {
                return true;
            }
            return false;
        }
        private void GetCity(List<Region> regions)
        {
            for (var idReg = 0; idReg < regions.Count(); idReg++)
            {
                region = (countRegion++, regions[idReg].RegionName);
                if (addMode)
                {
                    if (File.Exists(FILE_NAME_REGION) && !FindRegionInFile())
                        continue;
                }
                else
                    WriteToFileRegion();
                using (var requestCity = new HttpRequest())
                {
                    var cityParams = new RequestParams();
                    cityParams["obl_id"] = oblast.Item2;
                    cityParams["region_id"] = regions[idReg].RegionName;
                    cityParams["security"] = SECURITY;
                    var cityContent = requestCity.Post(GET_CITY_URL, cityParams).ToString();
                    var cityTemp = Html.ReplaceUnicode(cityContent);
                    var namesCities = JsonConvert.DeserializeObject<CityList>(cityTemp).Cities;
                    GetTypeStreet(namesCities);
                }
            }
        }
        private void WriteToFileRegion()
        {
            using (var sw = new StreamWriter(FILE_NAME_REGION, true))
            {
                sw.WriteLine($"({region.Item1},\'{region.Item2}\', {oblast.Item1}),");
            }
        }
        private bool FindRegionInFile()
        {
            var lastLine = File.ReadLines(FILE_NAME_REGION).Last();
            var temp = lastLine.Split(',');
            if (regTemplat.Replace(temp[1], "").Trim() == regTemplat.Replace(region.Item2, "").Trim() &&
                regTemplat.Replace(temp[2], "").Trim() == oblast.Item1.ToString().Trim())
            {
                countRegion = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim());
                region.Item1 = countRegion++;
                return true;
            }
            return false;
        }
        private void GetTypeStreet(List<City> cities)
        {
            for (var idCity = 0; idCity < cities.Count(); idCity++)
            {
                city = (countCity++, cities[idCity].CityName);
                if (addMode)
                {
                    if (File.Exists(FILE_NAME_CITY) && !FindCityInFile())
                        continue;
                }
                else
                    WriteToFileCity();
                using (var requestType = new HttpRequest())
                {
                    var typeParams = new RequestParams();
                    typeParams["obl_id"] = oblast.Item2;
                    typeParams["region_id"] = region.Item2;
                    typeParams["city_id"] = cities[idCity].CityName;
                    typeParams["security"] = SECURITY;
                    var typeContent = requestType.Post(GET_TYPE_STREET_URL, typeParams).ToString();
                    var typeTemp = Html.ReplaceUnicode(typeContent);
                    var namesTypes = JsonConvert.DeserializeObject<TypeStreetList>(typeTemp).TypeStreets;
                    if (namesTypes == null && addMode)
                    {
                        var lastLine = File.ReadLines(FILE_NAME_ZIPCODE).Last();
                        var temp = lastLine.Split(',');
                        countZipcode = Convert.ToInt32(regTemplat.Replace(temp[0], "")) + 1;
                        addMode = false;
                    }
                    GetStreet(namesTypes);
                }
            }
        }
        private void WriteToFileCity()
        {
            using (var sw = new StreamWriter(FILE_NAME_CITY, true))
            {
                sw.WriteLine($"({city.Item1},\'{city.Item2}\', {region.Item1}),");
            }
        }
        private bool FindCityInFile()
        {
            var lastLine = File.ReadLines(FILE_NAME_CITY).Last();
            var temp = lastLine.Split(',');
            if (regTemplat.Replace(temp[1], "").Trim() == regTemplat.Replace(city.Item2, "").Trim() &&
                regTemplat.Replace(temp[2], "").Trim() == region.Item1.ToString().Trim())
            {
                countCity = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim());
                city.Item1 = countCity++;
                return true;
            }
            return false;
        }
        private void GetStreet(List<TypeStreet> typeStreets)
        {
            for (var idType = 0; idType < typeStreets?.Count(); idType++)
            {
                typeStreet = (countType++, typeStreets[idType].TypeStreetName);
                if (!FindTypeStreetInFile())
                    WriteToFileTypeStreet();
                using (var requestStreet = new HttpRequest())
                {
                    var streetParams = new RequestParams();
                    streetParams["obl_id"] = oblast.Item2;
                    streetParams["region_id"] = region.Item2;
                    streetParams["city_id"] = city.Item2;
                    streetParams["typestreet_id"] = typeStreets[idType].TypeStreetName;
                    streetParams["security"] = SECURITY;
                    var streetContent = requestStreet.Post(GET_STREET_URL, streetParams).ToString();
                    var streetTemp = Html.ReplaceUnicode(streetContent);
                    var namesStreets = JsonConvert.DeserializeObject<StreetList>(streetTemp).Streets;
                    GetZipcode(namesStreets);
                }
            }
        }
        private void WriteToFileTypeStreet()
        {
            using (var sw = new StreamWriter(FILE_NAME_TYPE_STREET, true))
            {
                sw.WriteLine($"({typeStreet.Item1},\'{typeStreet.Item2}\'),");
            }
        }
        private bool FindTypeStreetInFile()
        {
            if (File.Exists(FILE_NAME_TYPE_STREET))
            {
                var lastLine = File.ReadLines(FILE_NAME_TYPE_STREET).Last();
                using (var sr = new StreamReader(FILE_NAME_TYPE_STREET))
                {
                    while (!sr.EndOfStream)
                    {
                        var currentLine = sr.ReadLine();
                        var temp = currentLine.Split(',');
                        if (temp[1].Contains(typeStreet.Item2))
                        {
                            countType = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim());
                            typeStreet.Item1 = Convert.ToInt32(temp[0].Remove(0,1));
                            typeStreet.Item2 = regTemplat.Replace(temp[1], "");
                            return true;
                        }
                        if (lastLine == currentLine)
                        {
                            countType = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim()) + 1;
                            typeStreet.Item1 = countType++;
                        }
                    }

                }
            }
            return false;
        }
        private void GetZipcode(List<Street> streets)
        {
            for (var idStreet = 0; idStreet < streets.Count(); idStreet++)
            {
                street = (countStreet++, streets[idStreet].StreetName);
                if (!FindStreetInFile())
                    WriteToFileStreet();
                using (var requestStreet = new HttpRequest())
                {
                    int index = random.Next(1, Captchas.Count());
                    var zipcodeParams = new RequestParams();
                    zipcodeParams["obl_id"] = oblast.Item2;
                    zipcodeParams["region_id"] = region.Item2;
                    zipcodeParams["city_id"] = city.Item2;
                    zipcodeParams["typestreet_id"] = typeStreet.Item2;
                    zipcodeParams["street_id"] = streets[idStreet].StreetName;
                    zipcodeParams["capcha_d"] = Captchas[index].Number;
                    zipcodeParams["capchk"] = Captchas[index].Code;
                    var zipcodeContent = requestStreet.Post(GET_ZIPCODE_URL, zipcodeParams).ToString();
                    var zipcodeTemp = Html.ReplaceUnicode(zipcodeContent);
                    var namesZipcodes = JsonConvert.DeserializeObject<ZipcodeList>(zipcodeTemp).Zipcodes;
                    if (namesZipcodes != null)
                    {
                        WriteToFileZipcode(namesZipcodes);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        private void WriteToFileStreet()
        {
            using (var sw = new StreamWriter(FILE_NAME_STREET, true))
            {
                sw.WriteLine($"({street.Item1},\'{street.Item2}\'),");
            }
        }
        private bool FindStreetInFile()
        {
            if (File.Exists(FILE_NAME_STREET))
            {
                var lastLine = File.ReadLines(FILE_NAME_STREET).Last();
                using (var sr = new StreamReader(FILE_NAME_STREET))
                {
                    while (!sr.EndOfStream)
                    {
                        var currentLine = sr.ReadLine();
                        var temp = currentLine.Split(',');
                        if (temp[1].Contains(street.Item2))
                        {
                            countStreet = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim());
                            street.Item1 = Convert.ToInt32(temp[0].Remove(0, 1));
                            street.Item2 = regTemplat.Replace(temp[1], "");
                            return true;
                        }
                        if (lastLine == currentLine)
                        {
                            countStreet = Convert.ToInt32(regTemplat.Replace(temp[0], "").Trim()) + 1;
                            street.Item1 = countStreet++;
                        }
                    }
                }
            }
            return false;
        }
        private void WriteToFileZipcode(List<Zipcode> zipcodes)
        {
            foreach (var zipcode in zipcodes)
            {
                if (addMode)
                {
                    if (!FindZipcodeInFile(zipcode))
                    {
                        continue;
                    }
                }
                else
                {
                    using (var sw = new StreamWriter(FILE_NAME_ZIPCODE, true))
                    {
                        sw.WriteLine($"({countZipcode++},\'{zipcode.ZipcodeNumber}\', \'{zipcode.Building}\', {city.Item1}, {typeStreet.Item1}, {street.Item1}),");
                    }
                }
                
            }
        }
        private bool FindZipcodeInFile(Zipcode zipcode)
        {
            if (File.Exists(FILE_NAME_ZIPCODE))
            {
                var lastLine = File.ReadLines(FILE_NAME_ZIPCODE).Last();
                var temp = lastLine.Split(',');
                if ((regTemplat.Replace(temp[1], "").Trim() == zipcode.ZipcodeNumber &&
                    regTemplat.Replace(temp[2], "").Trim() == regTemplat.Replace(zipcode.Building, "") &&
                    regTemplat.Replace(temp[3], "").Trim() == city.Item1.ToString() &&
                    regTemplat.Replace(temp[4], "").Trim() == typeStreet.Item1.ToString() &&
                    regTemplat.Replace(temp[5], "").Trim() == street.Item1.ToString()))
                {
                    countZipcode = Convert.ToInt32(regTemplat.Replace(temp[0], "")) + 1;
                    addMode = false;
                    return true;
                } else if (regTemplat.Replace(temp[3], "").Trim() != city.Item1.ToString())
                {
                    countZipcode = Convert.ToInt32(regTemplat.Replace(temp[0], "")) + 1;
                    using (var sw = new StreamWriter(FILE_NAME_ZIPCODE, true))
                    {
                        sw.WriteLine($"({countZipcode++},\'{zipcode.ZipcodeNumber}\', \'{zipcode.Building}\', {city.Item1}, {typeStreet.Item1}, {street.Item1}),");
                    }
                    addMode = false;
                    return true;
                }
            }
            return false;
        }
    }
}
