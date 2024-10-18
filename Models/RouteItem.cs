using Caliburn.Micro;
namespace CRB.Models;

public class RouteItem : PropertyChangedBase
{

    private BindableCollection<string> _altTypes = new BindableCollection<string>();
    public BindableCollection<string> AltTypes 
    { 
        get { return _altTypes; } 
        set { _altTypes = value; }
    }

    public int Index { get; set; }
    public string Name { get; set; }

    public int Alt { get; set; }

    public string AltType { get; set; }

    public TimeSpan ETA { get; set; }

    public bool ETALocked { get; set; }

    private double _lat;
    public double Lat 
    { 
        get { return _lat; }
        set 
        { 
            _lat = value;
            NotifyOfPropertyChange(() => Lat);
        } 
    }
    private double _lon;
    public double Lon
    {
        get { return _lon; }
        set
        {
            _lon = value;
            NotifyOfPropertyChange(() => Lon);
        }
    }
    public static BindableCollection<RouteItem> ConvertFlightPlanToRouteItems(FlightPlan fpl)
    {
        BindableCollection<RouteItem> routeItems = new BindableCollection<RouteItem>();

        if (fpl.WaypointTable == null)
        {
            return routeItems;
        }

        foreach (Waypoint waypoint in fpl.WaypointTable.Waypoints)
        {
            RouteItem routeItem = new RouteItem();

            routeItem.Index = waypoint.Index;
            routeItem.Name = waypoint.Identifier;
            routeItem.Alt = 0;
            routeItem.AltType = "BARO";
            routeItem.ETA = TimeSpan.FromSeconds(0);
            routeItem.ETALocked = false;
            routeItem.Lat = waypoint.Lat;
            routeItem.Lon = waypoint.Lon;
            routeItem._altTypes.Add("BARO");
            routeItem._altTypes.Add("RADIO");
            routeItems.Add(routeItem);
        }

        return routeItems;
    }
}
