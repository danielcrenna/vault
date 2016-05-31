using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace TweetSharp.Tests.Service
{
    [TestFixture]
    public partial class TwitterServiceTests
    {
        [Test]
        public void Can_support_secure_urls_in_entitities()
        {
            var service = GetAuthenticatedService();
            var tweet = service.GetTweet(new GetTweetOptions { Id = 131501393033961472});
            Console.WriteLine(tweet.RawSource);
        }

        [Test]
        public void Can_get_media_links_from_entities()
        {
            var service = GetAuthenticatedService();

            var tweet = service.GetTweet(new GetTweetOptions { Id = 128818112387756032 });
            Assert.IsNotNull(tweet.Entities);
            Assert.AreEqual(1, tweet.Entities.Media.Count);

            var media = tweet.Entities.Media[0];
            Assert.AreEqual("http://pbs.twimg.com/media/AcmnZAXCMAEaDD1.jpg", media.MediaUrl);
            Assert.AreEqual("https://pbs.twimg.com/media/AcmnZAXCMAEaDD1.jpg", media.MediaUrlHttps);
            Assert.AreEqual("http://twitter.com/sarah_hatton/status/128818112387756032/photo/1", media.ExpandedUrl);
            Assert.AreEqual("pic.twitter.com/xCdS2Emt", media.DisplayUrl);
            Assert.AreEqual(TwitterMediaType.Photo, media.MediaType);
            Assert.AreEqual(69, media.Indices[0]);
            Assert.AreEqual(89, media.Indices[1]);
            Assert.AreEqual("128818112391950337", media.IdAsString);
            Assert.AreEqual(128818112391950337, media.Id);

            // Sizes
            Assert.AreEqual(4, media.Sizes.Count());
            Assert.AreEqual("fit", media.Sizes.Large.Resize);
            Assert.AreEqual(597, media.Sizes.Large.Height);
            Assert.AreEqual(800, media.Sizes.Large.Width);
        }

        [Test]
        public void Can_get_basic_place()
        {
            var service = GetAuthenticatedService();

            // Presidio
            var place = service.GetPlace(new GetPlaceOptions { PlaceId = "df51dec6f4ee2b2c" });
            Assert.IsNotNull(place);
            Assert.AreEqual("df51dec6f4ee2b2c", place.Id);
            Assert.AreEqual("Presidio", place.Name);
            Assert.AreEqual("United States", place.Country);
            Assert.AreEqual("US", place.CountryCode);
            Assert.AreEqual("Presidio, San Francisco", place.FullName);
        }

        [Test]
        public void Can_get_reverse_geocode()
        {
            var service = GetAuthenticatedService();

            var places = service.ReverseGeocode(new ReverseGeocodeOptions { Lat = 45.42153, Long = -75.697193 }).ToList();
            Assert.IsNotEmpty(places);
            Assert.AreEqual(4, places.Count);

            places = places.OrderBy(p => p.Id).ToList();

            Assert.AreEqual("Ottawa, Ontario", places[0].FullName);
            Assert.AreEqual(TwitterPlaceType.Admin, places[0].PlaceType);
            Assert.AreEqual("06183ca2a30a18e8", places[0].Id);
            Assert.AreEqual(1, places[0].ContainedWithin.Count());
            Assert.AreEqual("89b2eb8b2b9847f7", places[0].ContainedWithin.ToList()[0].Id);

            Assert.AreEqual("Canada", places[1].FullName);
            Assert.AreEqual("3376992a082d67c7", places[1].Id);
            Assert.AreEqual(TwitterPlaceType.Country, places[1].PlaceType);

						Assert.AreEqual("Ottawa, Ontario", places[2].FullName);
            Assert.AreEqual(TwitterPlaceType.City, places[2].PlaceType);

						Assert.AreEqual("Ontario, Canada", places[3].FullName);
            Assert.AreEqual(TwitterPlaceType.Admin, places[3].PlaceType);
        }

        [Test]
        public void Can_search_geo_by_lat_long()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

            var places = service.GeoSearch(new GeoSearchOptions { Lat = 45.42153, Long = -75.697193}).ToList();
            Assert.IsNotEmpty(places);

            places = places.OrderBy(p => p.Id).ToList();
						Assert.AreEqual("05ebfafd8a5c1f5a", places[0].Id);
        }

        [Test]
        public void Can_parse_hashtag_search_url()
        {
            var service = GetAuthenticatedService();

            //https://twitter.com/PurinaONE/status/306126169743450112
            var tweet = service.GetTweet(new GetTweetOptions() {Id = 306126169743450112});

            Assert.IsNotNull(tweet);
            Assert.IsNotNull(tweet.TextAsHtml);
            Assert.IsTrue(tweet.TextAsHtml.Contains("https://twitter.com/search?q=Ingredientsforgood"));
        }

        [Test]
        public void Can_search_geo_by_ip()
        {
					//This test is currently failing. No matter what IP is provided, no result is returned
					//as Twitter says it has no location associated. Still trying to figure out if this
					//is a Twitter problem, data problem, or problem on our end.
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

						var result = service.GeoSearch(new GeoSearchOptions { Ip = "24.246.1.165" });
						Assert.IsNotNull(result);
						var places = result.ToList();
            Assert.IsNotEmpty(places);

            places = places.OrderBy(p => p.Id).ToList();
            Assert.AreEqual("06183ca2a30a18e8", places[0].Id);
        }

        [Test]
        public void Can_get_geo_coordinates_from_specific_tweet()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

            /*
             "geo": {
                    "type": "Point",
                    "coordinates": [
                        46.01364037,
                        -81.40501187
                    ]
                }, 
             */

            var last = service.GetTweet(new GetTweetOptions { Id = 133314374797492224 });
            Assert.IsNotNull(last.Place);
            Assert.IsNotNull(last.Location);
            Assert.AreEqual("Point", last.Location.Type);
            Assert.AreEqual(46.01364037, last.Location.Coordinates.Latitude);
            Assert.AreEqual(-81.40501187, last.Location.Coordinates.Longitude);
        }

        [Test]
        public void Can_get_parameterized_followers_of_lists()
        {
            const int maxIdsToGet = 100;

            var service = GetAuthenticatedService();

            var ids = service.ListFriendIdsOf(new ListFriendIdsOfOptions { ScreenName = "yortw" });
            
            
            var subList = ids.Count > 100 ? ids.Take(maxIdsToGet) : ids;

            var segment = service.ListUserProfilesFor(new ListUserProfilesForOptions { UserId = subList });
            if (segment == null)
            {
                if(service.Response.StatusCode == HttpStatusCode.OK)
                {
                    throw new Exception("No results, but Twitter returned OK...");
                }
                Console.WriteLine("Twitter failed legitimately: {0} {1}", service.Response.StatusCode, service.Response.StatusDescription);
            }
                
            Assert.AreEqual(subList.Count(), segment.Count());
        }

        [Test]
        [Ignore("This is a brittle test because it requires that you be me (and you are probably not me)")]
        public void Can_get_updated_user_properties()
        {
            //{"is_translator":false,
            //"geo_enabled":false,
            //"profile_background_color":"1A1B1F",
            //"protected":false,
            //"profile_background_tile":false,
            //"created_at":"Fri Dec 14 18:48:52 +0000 2007",
            //"name":"Daniel Crenna",
            //"profile_background_image_url_https":"https:\/\/si0.twimg.com\/images\/themes\/theme9\/bg.gif",
            //"profile_sidebar_fill_color":"252429",
            //"listed_count":102,
            //"notifications":false,
            //"utc_offset":-18000,
            //"friends_count":277,
            //"description":"Code soloist.",
            //"following":false,
            //"verified":false,
            //"profile_sidebar_border_color":"181A1E",
            //"followers_count":1193,
            //"profile_image_url":"http:\/\/a1.twimg.com\/profile_images\/700778992\/684676c4ec78fa88144b5256dd880986_normal.png",
            //"default_profile":false,
            //"contributors_enabled":false,
            //"profile_image_url_https":"https:\/\/si0.twimg.com\/profile_images\/700778992\/684676c4ec78fa88144b5256dd880986_normal.png",
            //"status":{"possibly_sensitive":false,"place":null,"retweet_count":0,"in_reply_to_screen_name":null,"created_at":"Sun Nov 06 14:23:43 +0000 2011","retweeted":false,"in_reply_to_status_id_str":null,"in_reply_to_user_id_str":null,"contributors":null,"id_str":"133187813599481856","in_reply_to_user_id":null,"in_reply_to_status_id":null,"source":"\u003Ca href=\"http:\/\/twitter.com\/tweetbutton\" rel=\"nofollow\"\u003ETweet Button\u003C\/a\u003E","geo":null,"favorited":false,"id":133187813599481856,"entities":{"urls":[{"display_url":"binpress.com\/blog\/2011\/08\/2\u2026","indices":[53,73],"url":"http:\/\/t.co\/KTGrKmeK","expanded_url":"http:\/\/www.binpress.com\/blog\/2011\/08\/25\/why-isnt-it-free-commercial-open-source\/"}],"user_mentions":[{"name":"Binpress","indices":[78,87],"screen_name":"Binpress","id_str":"204787796","id":204787796}],"hashtags":[]},"coordinates":null,"truncated":false,"text":"Why isn't it free - commercial open-source revisited http:\/\/t.co\/KTGrKmeK via @binpress"},"profile_use_background_image":true,"favourites_count":151,"location":"Ottawa, ON, Canada","id_str":"11173402","default_profile_image":false,"show_all_inline_media":true,"profile_text_color":"666666","screen_name":"danielcrenna","statuses_count":4669,"profile_background_image_url":"http:\/\/a1.twimg.com\/images\/themes\/theme9\/bg.gif","url":"http:\/\/danielcrenna.com","time_zone":"Eastern Time (US & Canada)","profile_link_color":"2FC2EF","id":11173402,
            //"follow_request_sent":false,
            //"lang":"en"}

            var service = GetAuthenticatedService();

            var user = service.GetUserProfile(new GetUserProfileOptions());
            Assert.AreEqual(false, user.FollowRequestSent);
            Assert.AreEqual(false, user.IsTranslator);
            Assert.AreEqual(false, user.ContributorsEnabled);
            Assert.IsTrue(user.ProfileBackgroundImageUrlHttps.StartsWith("https://"));
            Assert.IsTrue(user.ProfileImageUrlHttps.StartsWith("https://"));
            Assert.AreEqual(false, user.IsDefaultProfile);
        }

        [Test]
        [Ignore("This is a brittle test because it requires that you be me (and you are probably not me)")]
        public void Can_return_results_from_account_settings_endpoint()
        {
            //{"protected":false,
            //"geo_enabled":false,
            //"trend_location":[{"countryCode":"CA","name":"Canada","country":"Canada","placeType":{"name":"Country","code":12},"woeid":23424775,"url":"http:\/\/where.yahooapis.com\/v1\/place\/23424775","parentid":1}],
            //"language":"en",
            //"sleep_time":{"start_time":0,"end_time":12,"enabled":true},
            //"always_use_https":false,
            //"screen_name":"danielcrenna",
            //"show_all_inline_media":false,
            //"time_zone":{"name":"Eastern Time (US & Canada)","utc_offset":-18000,"tzinfo_name":"America\/New_York"},
            //"discoverable_by_email":true}

            var service = GetAuthenticatedService();

            var account = service.GetAccountSettings();
            Console.WriteLine(account.RawSource);

            Assert.AreEqual(false, account.IsProtected, "IsProtected");
            Assert.AreEqual(true, account.GeoEnabled, "GeoEnabled");
            Assert.IsNotNull(account.TrendLocations);
            Assert.AreEqual(1, account.TrendLocations.Count());
            Assert.AreEqual("CA", account.TrendLocations.Single().CountryCode);
            Assert.AreEqual("Canada", account.TrendLocations.Single().Name);
            Assert.AreEqual("Canada", account.TrendLocations.Single().Country);
            Assert.AreEqual("en", account.Language);
            Assert.AreEqual("danielcrenna", account.ScreenName, "ScreenName");
            Assert.IsNotNull(account.TimeZone);
            Assert.AreEqual("Eastern Time (US & Canada)", account.TimeZone.Name);
            Assert.AreEqual(-18000, account.TimeZone.UtcOffset);
            Assert.AreEqual("America/New_York", account.TimeZone.InfoName);
            Assert.IsNotNull(account.SleepTime);
            Assert.AreEqual(0, account.SleepTime.StartTime, "start_time");
            Assert.AreEqual(12, account.SleepTime.EndTime, "end_time");
            Assert.AreEqual(true, account.SleepTime.Enabled, "SleepTime");
        }

        [Test]
        public void Can_update_account_settings()
        {
            var service = GetAuthenticatedService();

            TwitterAccount original = service.GetAccountSettings();
            var state = !original.SleepTime.Enabled.Value;

            Trace.WriteLine("Sleep state was " + original.SleepTime.Enabled);
            
            var updated = service.UpdateAccountSettings(new UpdateAccountSettingsOptions { SleepTimeEnabled = state });
            Assert.AreEqual(state, updated.SleepTime.Enabled, "Didn't update");

            Trace.WriteLine("Sleep state is now " + updated.SleepTime.Enabled);

            updated = service.UpdateAccountSettings(new UpdateAccountSettingsOptions() { SleepTimeEnabled = !state});
            Assert.AreEqual(!state, updated.SleepTime.Enabled, "Didn't update again");

            Trace.WriteLine("Sleep state is now " + updated.SleepTime.Enabled);
        }
    }
}
