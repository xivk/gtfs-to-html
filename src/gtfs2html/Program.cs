using GTFS;
using GTFS.Entities;
using GTFS.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace GTFS2HTML
{
    class Program
    {
        static void Main(string[] args)
        {
            // create the reader.
            var reader = new GTFSReader<GTFSFeed>();

            // execute the reader.
            Console.WriteLine("Loading FEED...");
            GTFSFeed feed = null;
            using (var sources = new GTFSDirectorySource(new DirectoryInfo(args[0])))
            {
                feed = reader.Read(sources);
            }

            var agency = "De Lijn";
            var idLinkTemplate = File.ReadAllText("id-link.html");

            var routes = new Dictionary<string, Route>();
            foreach (var route in feed.Routes)
            {
                routes[route.Id] = route;
            }

            Console.WriteLine("Doing stop-times...");
            var stopsToTrips = new Dictionary<string, HashSet<string>>();
            var tripsToStopTimes = new Dictionary<string, List<StopTime>>();
            foreach (var stopTime in feed.StopTimes)
            {
                HashSet<string> trips;
                if (!stopsToTrips.TryGetValue(stopTime.StopId, out trips))
                {
                    trips = new HashSet<string>();
                    stopsToTrips.Add(stopTime.StopId, trips);
                }
                trips.Add(stopTime.TripId);

                List<StopTime> stopTimes;
                if (!tripsToStopTimes.TryGetValue(stopTime.TripId, out stopTimes))
                {
                    stopTimes = new List<StopTime>();
                    tripsToStopTimes.Add(stopTime.TripId, stopTimes);
                }
                stopTimes.Add(stopTime);
            }

            var routeForTrip = new Dictionary<string, string>();
            foreach (var trip in feed.Trips)
            {
                routeForTrip[trip.Id] = trip.RouteId;
            }

            Console.WriteLine("Doing stops...");
            var stopTemplate = File.ReadAllText("stop.html");
            var stops = new Dictionary<string, Stop>();
            foreach (var stop in feed.Stops)
            {
                var lat = stop.Latitude.ToInvariantString();
                var lon = stop.Longitude.ToInvariantString();

                var stopName = stop.Name;
                stops[stop.Id] = stop;

                //var tripsHtml = string.Empty;
                //HashSet<string> trips;
                //if (stopsToTrips.TryGetValue(stop.Id, out trips))
                //{
                //    foreach (var trip in trips)
                //    {
                //        var filename = FileNameBuilder.MakeValidFileName("trip-" + trip + ".html");

                //        var routeName = string.Empty;
                //        Route route = null;
                //        var routeId = string.Empty;
                //        if (routeForTrip.TryGetValue(trip, out routeId) &&
                //            routes.TryGetValue(routeId, out route))
                //        {
                //            routeName = route.LongName;
                //        }

                //        tripsHtml = tripsHtml + "<li>" + idLinkTemplate.Replace("{filename}", filename)
                //            .Replace("{name}", trip)
                //            .Replace("{description}", trip + " - " + routeName) + "</li>";
                //    }
                //}
                //else
                //{
                //    continue;
                //}


                //File.WriteAllText(Path.Combine(args[1], FileNameBuilder.MakeValidFileName("stop-" + stop.Id + ".html")),
                //    stopTemplate.Replace("{lat}", lat)
                //        .Replace("{lon}", lon)
                //        .Replace("{stop-name}", stopName)
                //        .Replace("{agency}", agency)
                //        .Replace("{trips}", tripsHtml));
            }

            //Console.WriteLine("Working on shapes...");
            //var geoJson = File.ReadAllText("template.geojson");
            //var id = string.Empty;
            //var coordinates = string.Empty;
            //var stopFeatures = string.Empty;
            //var stopGeoJson = File.ReadAllText("stop.geojson");
            //foreach (var shape in feed.Shapes)
            //{
            //    if (id != shape.Id)
            //    {
            //        if (!string.IsNullOrWhiteSpace(coordinates))
            //        {
            //            File.WriteAllText(Path.Combine(args[1], id + ".geojson"),
            //                geoJson.Replace("{coordinates}", coordinates));
            //        }

            //        Console.WriteLine(string.Format("{0} written", id));

            //        id = shape.Id;
            //        coordinates = string.Empty;
            //        stopFeatures = string.Empty;
            //    }

            //    var lat = shape.Latitude;
            //    var lon = shape.Longitude;

            //    var coordinate = string.Format("[{0},{1}]",
            //        lon.ToInvariantString(), lat.ToInvariantString());
            //    if (!string.IsNullOrWhiteSpace(coordinates))
            //    {
            //        coordinates = coordinates + ",";
            //    }
            //    coordinates = coordinates + coordinate;
            //}

            //if (!string.IsNullOrWhiteSpace(coordinates))
            //{
            //    File.WriteAllText(Path.Combine(args[1], id + ".geojson"),
            //        geoJson.Replace("{coordinates}", coordinates));
            //}

            //Console.WriteLine(string.Format("{0} written", id));

            Console.WriteLine("Doing trips...");
            var tripsPerRoute = new Dictionary<string, List<Trip>>();
            var tripTemplate = File.ReadAllText("trip.html");
            foreach (var trip in feed.Trips)
            {
                var tripName = trip.ShortName;

                var stopTimesHtml = string.Empty;
                List<StopTime> stopTimes;
                if (tripsToStopTimes.TryGetValue(trip.Id, out stopTimes))
                {
                    foreach (var stopTime in stopTimes)
                    {
                        var filename = FileNameBuilder.MakeValidFileName("stop-" + stopTime.StopId + ".html");
                        var stopName = string.Empty;
                        Stop stop;
                        if (stops.TryGetValue(stopTime.StopId, out stop))
                        {
                            stopName = stop.Name;
                        }

                        var departureTime = string.Format("{0}:{1}",
                            stopTime.DepartureTime.Value.Hours.ToString("00"), stopTime.DepartureTime.Value.Minutes.ToString("00"));

                        stopTimesHtml = stopTimesHtml + "<li>" + idLinkTemplate.Replace("{filename}", filename)
                            .Replace("{name}", stopTime.StopId)
                            .Replace("{description}", stopName + " " + departureTime) + "</li>";
                    }
                }
                else
                {
                    continue;
                }

                List<Trip> trips;
                if (!tripsPerRoute.TryGetValue(trip.RouteId, out trips))
                {
                    trips = new List<Trip>();
                    tripsPerRoute[trip.RouteId] = trips;
                }
                trips.Add(trip);

                var routeName = string.Empty;
                Route route;
                if (routes.TryGetValue(trip.RouteId, out route))
                {
                    routeName = route.LongName;
                }

                //File.WriteAllText(Path.Combine(args[1], FileNameBuilder.MakeValidFileName("trip-" + trip.Id + ".html")),
                //tripTemplate.Replace("{lat}", "0")
                //    .Replace("{lon}", "0")
                //    .Replace("{trip-id}", trip.Id)
                //    .Replace("{route-name}", routeName)
                //    .Replace("{agency}", agency)
                //    .Replace("{stoptimes}", stopTimesHtml)
                //    .Replace("{shapefile}", trip.ShapeId + ".geojson"));
            }

            Console.WriteLine("Doing routes...");
            var routeLinkTemplate = File.ReadAllText("route-link.html");
            var routeTemplate = File.ReadAllText("route.html");
            var routeLinkItems = string.Empty;
            foreach (var route in feed.Routes)
            {
                var filename = FileNameBuilder.MakeValidFileName("route-" + route.Id + ".html");
                var routeLink = idLinkTemplate.Replace("{filename}", filename)
                            .Replace("{name}", route.ShortName)
                            .Replace("{description}", route.LongName) + "</li>";

                routeLinkItems = routeLinkItems + "<li>" + routeLink + "</li>";

                var tripsHtml = string.Empty;
                List<Trip> trips;
                if (tripsPerRoute.TryGetValue(route.Id, out trips))
                {
                    foreach (var trip in trips)
                    {
                        filename = FileNameBuilder.MakeValidFileName("trip-" + trip.Id + ".html");

                        var routeName = string.Empty;
                        Route route1 = null;
                        var routeId = string.Empty;
                        if (routeForTrip.TryGetValue(trip.Id, out routeId) &&
                            routes.TryGetValue(routeId, out route1))
                        {
                            routeName = route1.LongName;
                        }

                        tripsHtml = tripsHtml + "<li>" + idLinkTemplate.Replace("{filename}", filename)
                            .Replace("{name}", trip.Id)
                            .Replace("{description}", trip.Id) + "</li>";
                    }
                }
                else
                {
                    continue;
                }

                File.WriteAllText(Path.Combine(args[1], FileNameBuilder.MakeValidFileName("route-" + route.Id + ".html")),
                    routeTemplate
                        .Replace("{route-name}", route.LongName)
                        .Replace("{agency}", agency)
                        .Replace("{trips}", tripsHtml));
            }
            File.WriteAllText(Path.Combine(args[1], "index.html"),
                File.ReadAllText("routes.html").Replace("{routes}", routeLinkItems)
                    .Replace("{agency}", agency));

        }
    }
}