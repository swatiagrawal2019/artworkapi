using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NandosArtService.Models;
using NandosArtService.Services;
using Clarifai.API;
using Clarifai.DTOs.Searches;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NandosArtService.ClarifaiResponse;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace NandosArtService.Controllers
{
    [Route("v1/scanart")]
    [ApiController]
    public class ArtFinderController : ControllerBase
    {
        private readonly ArtContext _context;
        private IConfiguration _configuration;
        private readonly ILogger<ArtFinderController> _log;

        public ArtFinderController(ArtContext context, IConfiguration Configuration, ILogger<ArtFinderController> log)
        {
            _context = context;
            _configuration = Configuration;
            _log = log;
        }

        // POST: api/scanart
        [HttpPost]
        public async Task<ActionResult<Artwork>> PostArtFinder(ArtFinder artFinder)
        {
            try
            {
                //Get location Id from Nando's API using supplied coordinates (if supplied and pass to Clarifai)
                
                double successScore = Convert.ToDouble(_configuration["ClarifaiSuccessScore"]);             
                var client = new ClarifaiClient(_configuration["ClarifaiAPIKey"]);

                int restaurantResultLimit = Convert.ToInt32(_configuration["RestaurantResultLimit"]);
                double restaurantSearchRadius = Convert.ToDouble(_configuration["RestaurantSearchRadius"]);
                string distanceUnit = _configuration["DistanceUnit"];

                string clarifaiImageInputId = "";
                Double clarifaiMatchScore = 0;

                byte[] imageBytes = null;

                if (_configuration["ClarifaiTestImage"] != "")
                {
                    //temporary code
                    byte[] imageBytesXX = null;
                    imageBytesXX = System.IO.File.ReadAllBytes(_configuration["ClarifaiTestImage"]);

                    string test = System.Convert.ToBase64String(imageBytesXX);
                    imageBytes = System.Convert.FromBase64String(test);
                }
                else
                {
                    //Convert Base64 Encoded string to Byte Array.
                    imageBytes = System.Convert.FromBase64String(artFinder.artImage);
                }


                //Temp code start - not reqired if the search is performed in Clarifai with Geo Coordinates

                string restaurantLocationId = "";

                //Check if the customer is in a Restaurant 
                var allLocations = await _context.RestaurantLocations.Where(a => a.longitude != 0).ToListAsync();

                //for each entry in artists, get artist links
                foreach (RestaurantLocation restaurantLocation in allLocations)
                {
                    if (restaurantLocation.latitude.HasValue)
                    {
                        restaurantLocation.distance = new Coordinates(artFinder.latitude.GetValueOrDefault(), artFinder.longitude.GetValueOrDefault()).DistanceTo(new Coordinates(restaurantLocation.latitude.Value, restaurantLocation.longitude.Value), UnitOfLength.Miles);
                    }
                }

                var distanceInAscOrder = allLocations.OrderBy(s => s.distance).Take(restaurantResultLimit);

                foreach (RestaurantLocation restaurantLocation in distanceInAscOrder)
                {
                    if (restaurantLocation.distance <= restaurantSearchRadius)
                    {
                        restaurantLocationId = restaurantLocation.id;
                        break;
                    }
                }

                //Temp code end - not reqired if the search is performed in Clarifai with Geo Coordinates


                // if location is supplied, pass it to clarifai 
                if (restaurantLocationId != "")
                {
                    var geoPoint = new Clarifai.DTOs.GeoPoint((decimal)artFinder.latitude, (decimal)artFinder.longitude);
                    var geoRadius = new Clarifai.DTOs.GeoRadius((decimal)restaurantSearchRadius, Clarifai.DTOs.GeoRadius.RadiusUnit.WithinMiles);
                                        

                    //var clarifaiResponse = await client.SearchInputs(SearchBy.Geo(geoPoint, geoRadius),
                    //                                                 SearchBy.ImageVisually(imageBytes))
                    //    .Page(1)
                    //    .ExecuteAsync();

                    //Temp change, we will revert it once clarify geo points are present 

                    var clarifaiResponse = await client.SearchInputs(SearchBy.ImageVisually(imageBytes))
                       .Page(1)
                       .PerPage(100)
                       .ExecuteAsync();  // pagination doesn't work, requires investigation. Email sent to Clarifai. 

                    _log.LogInformation("Clarifai Response --" + clarifaiResponse.Status.Type + " " + clarifaiResponse.Status.StatusCode + ", " + clarifaiResponse.Status.Description + ":" + clarifaiResponse.Status.ErrorDetails);
                    
                    if (clarifaiResponse.HttpCode == System.Net.HttpStatusCode.OK)
                    {
                        if (clarifaiResponse.IsSuccessful)
                        {
                            Clarifai.DTOs.ClarifaiStatus clarifaiResponseStatus = (Clarifai.DTOs.ClarifaiStatus)clarifaiResponse.Status;
                            //success code in 10000
                            if (clarifaiResponseStatus.StatusCode == 10000)
                            {
                                SearchResults searchResults = JsonConvert.DeserializeObject<SearchResults>(clarifaiResponse.RawBody);

                                //for each entry in artworks, get location name from restaurant location API
                                foreach (Hit hit in searchResults.hits)
                                {

                                    // check if the match score is heigher than the value specified in config fime 
                                    if (hit.score >= successScore)
                                    {
                                        // temporay change - code should be removed after testing
                                        // check if the Artwork exists in location  
                                        var locationArtworks = await _context.Artwork.Where(b => b.id == Int32.Parse(hit.input.id) && b.locationId == restaurantLocationId).ToListAsync();

                                        foreach (Artwork locationArtwork in locationArtworks)
                                        {
                                            clarifaiImageInputId = hit.input.id;
                                            clarifaiMatchScore = hit.score * 100;
                                            break;
                                        }
          
                                        if (clarifaiImageInputId != "")
                                        {
                                            _log.LogInformation("Retrieved clarifaiImageInputId..");
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _log.LogInformation(@"Clarifai response unsuccessful - Ststus Code:" + clarifaiResponseStatus.StatusCode.ToString());
                                return NotFound(@"Clarifai response unsuccessful - Ststus Code:" + clarifaiResponseStatus.StatusCode.ToString());
                            }
                        }
                        else
                        {
                            _log.LogInformation(@"Clarifai response unsuccessful");
                            return NotFound(@"Clarifai response unsuccessful");
                        }
                    }
                    else
                    {
                        _log.LogInformation(@"Clarifai response HTTPCode:" + clarifaiResponse.HttpCode.ToString());
                        return NotFound(@"Clarifai response HTTPCode:" + clarifaiResponse.HttpCode.ToString());
                    }

                    if (clarifaiImageInputId == "")
                    {
                        _log.LogInformation(@"Image not found in Clarifai");
                        return NotFound(@"Image not found in Clarifai");
                    }
                }
                else
                {
                    var clarifaiResponse = await client.SearchInputs(SearchBy.ImageVisually(imageBytes))
                        .Page(1)
                        .ExecuteAsync();

                    _log.LogInformation("Clarifai Response--" + clarifaiResponse.Status.Type + " " + clarifaiResponse.Status.StatusCode + ", " + clarifaiResponse.Status.Description + ":"  + clarifaiResponse.Status.ErrorDetails );

                    if (clarifaiResponse.HttpCode == System.Net.HttpStatusCode.OK)
                    {
                        if (clarifaiResponse.IsSuccessful)
                        {
                            Clarifai.DTOs.ClarifaiStatus clarifaiResponseStatus = (Clarifai.DTOs.ClarifaiStatus)clarifaiResponse.Status;
                            //success code in 10000
                            if (clarifaiResponseStatus.StatusCode == 10000)
                            {
                                SearchResults searchResults = JsonConvert.DeserializeObject<SearchResults>(clarifaiResponse.RawBody);

                                //for each entry in artworks, get location name from restaurant location API
                                foreach (Hit hit in searchResults.hits)
                                {
                                    // check if the match score is heigher than the value specified in config fime 
                                    if (hit.score >= successScore)
                                    {
                                        clarifaiImageInputId = hit.input.id;
                                        clarifaiMatchScore = hit.score * 100;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                _log.LogInformation(@"Clarifai response unsuccessful - Ststus Code:" + clarifaiResponseStatus.StatusCode.ToString());
                                return NotFound(@"Clarifai response unsuccessful - Ststus Code:" + clarifaiResponseStatus.StatusCode.ToString());
                            }
                        }
                        else
                        {
                            _log.LogInformation(@"Clarifai response unsuccessful");
                            return NotFound(@"Clarifai response unsuccessful");
                        }
                    }
                    else
                    {
                        _log.LogInformation(@"Clarifai response HTTPCode:" + clarifaiResponse.HttpCode.ToString());
                        return NotFound(@"Clarifai response HTTPCode:" + clarifaiResponse.HttpCode.ToString());
                    }

                    if (clarifaiImageInputId == "")
                    {
                        _log.LogInformation(@"Image not found in Clarifai");
                        return NotFound(@"Image not found in Clarifai");
                    }
                }


                //UI only requires artwork id and score
                Artwork artwork = null;
                if (clarifaiImageInputId != "")
                {
                    artwork = new Artwork();
                    artwork.id = Int32.Parse(clarifaiImageInputId);
                    artwork.matchScore = clarifaiMatchScore;
                }

                ////Find artwok matching on clarifai image input id
                //var artworks = await _context.Artwork.Where(b => b.id == Int32.Parse(clarifaiImageInputId)).ToListAsync();

                //if (artworks.Count == 0)
                //{
                //    _log.LogInformation(@"Artwork not found in Artwork DB");
                //    return NotFound(@"Artwork not found in Artwork DB");
                //}
                //else
                //{
                //    //Get 1st matching item
                //    foreach (Artwork artworkTemp in artworks)
                //    {
                //        artworkTemp.matchScore = clarifaiMatchScore;
                //        artwork = artworkTemp;

                //        //Get artwork location Id
                //        var locations = await _context.RestaurantLocations.Where(b => b.id == artwork.locationId).ToListAsync();

                //        foreach (RestaurantLocation location in locations)
                //        {
                //            artwork.locationName = location.locationName;
                //            break;
                //        }
                //        break;
                //    }

                //}
                return artwork;
            }

            catch (Exception e)
            {
                _log.LogError(e.Message);
                Console.WriteLine("{0} Second exception caught.", e);
            }

            return NotFound();
        }

    }
}
