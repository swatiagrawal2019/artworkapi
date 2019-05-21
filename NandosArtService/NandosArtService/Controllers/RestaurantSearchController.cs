using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandosArtService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NandosArtService.Controllers
{
    [Route("v1/location")]
    [ApiController]
    public class RestaurantSearchController : ControllerBase
    {
        private readonly ArtContext _context;
        private IConfiguration _configuration;
        private readonly ILogger<RestaurantSearchController> _log;

        public RestaurantSearchController(ArtContext context, IConfiguration Configuration,ILogger<RestaurantSearchController> log)
        {
            _context = context;
            _configuration = Configuration;
            _log = log;
        }

        // POST: api/location
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LocationResult>>> PostLocationResult(LocationSearch locationSearch)
        {
            List<LocationResult> locationResults = new List<LocationResult>();

            try
            {
                int restaurantResultLimit = Convert.ToInt32(_configuration["RestaurantResultLimit"]);
                double restaurantSearchRadius = Convert.ToDouble(_configuration["RestaurantSearchRadius"]);
                string distanceUnit = _configuration["DistanceUnit"];

                if ((locationSearch.longitude.HasValue || locationSearch.latitude.HasValue) && (locationSearch.locationName == ""))
                {
                    //Search for geo only. If the range is within the configured range, retrn the first value 

                    //var allLocations = await _context.RestaurantLocations.ToListAsync();

                    var allLocations = await _context.RestaurantLocations.Where(a => a.longitude != 0).ToListAsync();

                    //for each entry in artists, get artist links
                    foreach (RestaurantLocation restaurantLocation in allLocations)
                    {
                        if (restaurantLocation.latitude.HasValue)
                        {
                            restaurantLocation.distance = new Coordinates(locationSearch.latitude.GetValueOrDefault(), locationSearch.longitude.GetValueOrDefault()).DistanceTo(new Coordinates(restaurantLocation.latitude.Value, restaurantLocation.longitude.Value), UnitOfLength.Miles);
                        }
                    }

                    var distanceInAscOrder = allLocations.OrderBy(s => s.distance).Take(restaurantResultLimit);

                    //for each entry in artists, get artist links
                    foreach (RestaurantLocation restaurantLocation in distanceInAscOrder)
                    {
                        if (restaurantLocation.distance <= restaurantSearchRadius)
                        {
                            LocationResult locationResult = new LocationResult();
                            locationResult.isCustAtRestaurant = true;
                            locationResult.locationId = restaurantLocation.id;
                            locationResult.locationName = restaurantLocation.locationName;
                            locationResult.distanceFromLocation = restaurantLocation.distance.ToString();
                            locationResult.distanceUnit = distanceUnit;
                            locationResults.Add(locationResult);
                            break;
                        }
                        else
                        {
                            LocationResult locationResult = new LocationResult();
                            locationResult.isCustAtRestaurant = false;
                            locationResult.locationId = restaurantLocation.id;
                            locationResult.locationName = restaurantLocation.locationName;
                            locationResult.distanceFromLocation = restaurantLocation.distance.ToString();
                            locationResult.distanceUnit = distanceUnit;
                            locationResults.Add(locationResult);
                        }
                    }
                }
                else
                {
                    var restaurantLocations= new List<RestaurantLocation>(); 
                    if (locationSearch.locationName != "")
                    {
                        restaurantLocations = await _context.RestaurantLocations.Where(a => a.locationName.Contains(locationSearch.locationName)).OrderBy(s => s.locationName).Take(restaurantResultLimit).ToListAsync();
                    }
                    else
                    {
                        restaurantLocations = await _context.RestaurantLocations.OrderBy(s => s.locationName).Take(restaurantResultLimit).ToListAsync();
                    }

                    if (restaurantLocations != null)
                    {
                        //for each entry in artists, get artist links
                        foreach (RestaurantLocation restaurantLocation in restaurantLocations)
                        {
                            LocationResult locationResult = new LocationResult();
                            locationResult.isCustAtRestaurant = false;
                            locationResult.locationId = restaurantLocation.id;
                            locationResult.locationName = restaurantLocation.locationName;
                            locationResult.distanceFromLocation = restaurantLocation.distance.ToString();
                            locationResult.distanceUnit = distanceUnit;
                            locationResults.Add(locationResult);
                        }
                    }
                }
                _log.LogInformation("Identified if customer is present at resturant or not...");
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }

            return locationResults;
        }
    }

    public class Coordinates
    {
        public double _latitude { get; private set; }
        public double _longitude { get; private set; }
        public Coordinates(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }
    }
    public static class CoordinatesDistanceExtensions
    {
        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
        {
            return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Kilometers);
        }

        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates, UnitOfLength unitOfLength)
        {
            var baseRad = Math.PI * baseCoordinates._latitude / 180;
            var targetRad = Math.PI * targetCoordinates._latitude / 180;
            var theta = baseCoordinates._longitude - targetCoordinates._longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            Math.Round(unitOfLength.ConvertFromMiles(dist), 1);

            return Math.Round(unitOfLength.ConvertFromMiles(dist), 1);
        }
    }

    public class UnitOfLength
    {
        public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
        public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
        public static UnitOfLength Miles = new UnitOfLength(1);

        private readonly double _fromMilesFactor;

        private UnitOfLength(double fromMilesFactor)
        {
            _fromMilesFactor = fromMilesFactor;
        }

        public double ConvertFromMiles(double input)
        {
            return input * _fromMilesFactor;
        }
    }


}
