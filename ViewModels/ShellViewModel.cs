using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using CRB.Models;
using CRB.Properties;
using System.Diagnostics;
namespace CRB.ViewModels;

public class ShellViewModel : Conductor<object>
{
    private readonly IEventAggregator _events;
    public ShellViewModel(IEventAggregator events)
    {
        _status = _settings.LoadSettings();
        _events = events;
    }

    private Settings _settings = new Settings();
    public Settings Settings
    { 
        get { return _settings; }
        set { _settings = value; }
    }

    private FlightPlan _fpl = new FlightPlan();
    public FlightPlan FPL
    {
        get { return _fpl; }
        set
        {
            _fpl = value;
            NotifyOfPropertyChange(() => FPL);
        }
    }

    private BindableCollection<RouteItem> _routeItems = new BindableCollection<RouteItem>();
    public BindableCollection<RouteItem> RouteItems
    {
        get { return _routeItems; }
        set 
        { 
            _routeItems = value;
            NotifyOfPropertyChange(() => RouteItems);

            //set map based on first coordinate of route
            Maps.Coordinate point = new Maps.Coordinate();
            if (RouteItems.Count > 0)
            {
                point.Lat = RouteItems.First().Lat;
                point.Lon = RouteItems.First().Lon;
                Coordinate = point;
            }
        }
    }

    private string _routeName;
    public string RouteName
    {
        get { return _routeName; }
        set
        {
            _routeName = value;
            NotifyOfPropertyChange(() => RouteName);
        }
    }

    private Maps.Coordinate _coordinate = new Maps.Coordinate();
    public Maps.Coordinate Coordinate
    {
        get { return _coordinate; }
        set 
        { 
            _coordinate = value; 
            NotifyOfPropertyChange(() => Coordinate);
            Region = Maps.Projections.Bounds.FindRegion(Coordinate);
        }
    }

    private string _status;
    public string Status 
    {   
        get { return _status; }
        set { 
            _status = value;
            NotifyOfPropertyChange(() => Status);
        }
    }

    private string _region;
    public string Region
    {
        get { return _region; }
        set {   
            _region = value;
            NotifyOfPropertyChange(() => Region);
        }
    }

    public void LoadFPL()
    {
        string xml;

        (xml, Status) = FlightPlan.LoadFplFile();
        if (!String.IsNullOrEmpty(xml))
        {
            (FPL, Status) = FlightPlan.ConvertXMLtoFlightPlan(xml);
            if (FPL != null)
            {
                //Convert FPL Waypoints to Route Items for DCS
                RouteItems = RouteItem.ConvertFlightPlanToRouteItems(FPL);
                RouteName = FPL.Route.RouteName;

                Debug.WriteLine(xml);
            }
        }  
    }

    public void ClearRoute()
    {
        RouteItems.Clear();
        RouteName = null;
        Status = "Route cleared.";
        Region = null;
        FPL = null;
    }

    public void ExportToDCS()
    {
        Debug.WriteLine(RouteName);
        Debug.WriteLine(Region);
        Debug.WriteLine(RouteItems.Count());
        Debug.WriteLine(_settings.RouteToolPresetsPath);
        Status = RouteExporter.DoTheLuaLua(RouteItems, RouteName, Region, _settings.RouteToolPresetsPath, FPL);
    }
}