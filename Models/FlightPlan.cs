using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace CRB.Models;




[XmlRoot("flight-plan", Namespace = "http://www8.garmin.com/xmlschemas/FlightPlan/v1")]
public class FlightPlan
{
    [XmlElement("created")]
    public DateTime Created { get; set; }
    [XmlElement("waypoint-table")]
    public WaypointTable WaypointTable { get; set; }
    [XmlElement("route")]
    public Route Route { get; set; }


    public static (FlightPlan, string) ConvertXMLtoFlightPlan(string xml)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FlightPlan));
            using (StringReader reader = new StringReader(xml))
            {
                FlightPlan flightplan = (FlightPlan)serializer.Deserialize(reader);

                //Add waypoint indices
                int i = 0;
                foreach (Waypoint waypoint in flightplan.WaypointTable.Waypoints)
                {
                    waypoint.Index = i++;
                }

                //TODO: remove first waypoint (origin airport) because DCS defaults waypoint 0 to origin?
                return (flightplan, "FPL file loaded successfully!");
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return (null, ex.Message);
        }
        
    }

    public static (string, string) LoadFplFile()
    {
        string fplFileContent;
        string fplFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fpl");

        //If folder doesn't exist, create it
        DirectoryInfo fplDir = Directory.CreateDirectory(fplFolderPath);


        // Open a file dialog to let the user select a .fpl file
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.InitialDirectory = fplFolderPath;
            openFileDialog.Filter = "Flight Plan Files (*.fpl)|*.fpl|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a Flight Plan (.fpl) File";

            var result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    fplFileContent = File.ReadAllText(selectedFilePath);
                    return (fplFileContent,"Got data.");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error reading FPL file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return ("","Error reading FPL file.");
                }
            }
            else
            {
                return ("", "No file selected.");
            }
        }
    }
}

public class WaypointTable
{
    [XmlElement("waypoint")]
    public BindableCollection<Waypoint> Waypoints { get; set; }
}

public class Waypoint
{
    public int Index { get; set; }
    [XmlElement("identifier")]
    public string Identifier { get; set; }
    [XmlElement("type")]
    public string Type { get; set; }
    [XmlElement("country-code")]
    public string CountryCode { get; set; }
    [XmlElement("lat")]
    public double Lat { get; set; }
    [XmlElement("lon")]
    public double Lon { get; set; }
    [XmlElement("comment")]
    public string Comment { get; set; }
}

public class Route
{
    [XmlElement("route-name")]
    public string RouteName { get; set; }
    [XmlElement("flight-plan-index")]
    public int FlightPlanIndex { get; set; }
    [XmlElement("route-point")]
    public List<RoutePoint> RoutePoints { get; set; }
}

public class RoutePoint
{
    [XmlElement("waypoint-identifier")]
    public string WaypointIdentifier { get; set; }
    [XmlElement("waypoint-type")]
    public string WaypointType { get; set; }
    [XmlElement("waypoint-country-code")]
    public string WaypointCountryCode { get; set; }
}

    

    