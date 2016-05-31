using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using NUnit.Framework;

namespace TweetSharp.Tests.Service
{
		[TestFixture, System.Runtime.InteropServices.GuidAttribute("DD654DE5-566A-4DAB-A675-7AD6C998F4B9")]
    public partial class TwitterServiceTests
    {
        private readonly string _hero;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;
				private readonly string _twitPicKey;
				private readonly string _twitPicUserName;

        public TwitterServiceTests()
        {
            _hero = ConfigurationManager.AppSettings["Hero"];
            _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            _accessToken = ConfigurationManager.AppSettings["AccessToken"];
            _accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
						_twitPicKey = ConfigurationManager.AppSettings["TwitPicKey"];
						_twitPicUserName = ConfigurationManager.AppSettings["TwitPicUserName"];
        }

				[Test]
				public void Can_get_twitter_configuration()
				{
					var service = GetAuthenticatedService();
					var configuration = service.GetConfiguration();

					Assert.IsNotNull(configuration);
					Assert.Greater(configuration.CharactersReservedPerMedia, 0);
					Assert.Greater(configuration.MaxMediaPerUpload, 0);
					Assert.Greater(configuration.ShortUrlLength, 0);
					Assert.Greater(configuration.ShortUrlLengthHttps, 0);
					Assert.Greater(configuration.PhotoSizeLimit, 0);
					Assert.IsNotNull(configuration.NonUserNamePaths);
					Assert.Greater(configuration.NonUserNamePaths.Count(), 0);
					Assert.IsNotNull(configuration.PhotoSizes);
					Assert.IsNotNull(configuration.PhotoSizes.Thumb);
					Assert.IsNotNull(configuration.PhotoSizes.Small);
					Assert.IsNotNull(configuration.PhotoSizes.Medium);
					Assert.IsNotNull(configuration.PhotoSizes.Large);
					Assert.Greater(configuration.PhotoSizes.Thumb.Height, 0);
					Assert.Greater(configuration.PhotoSizes.Thumb.Width, 0);
					Assert.IsNotNullOrEmpty(configuration.PhotoSizes.Thumb.Resize);
					Assert.Greater(configuration.PhotoSizes.Small.Height, 0);
					Assert.Greater(configuration.PhotoSizes.Small.Width, 0);
					Assert.IsNotNullOrEmpty(configuration.PhotoSizes.Small.Resize);
					Assert.Greater(configuration.PhotoSizes.Medium.Height, 0);
					Assert.Greater(configuration.PhotoSizes.Medium.Width, 0);
					Assert.IsNotNullOrEmpty(configuration.PhotoSizes.Medium.Resize);
					Assert.Greater(configuration.PhotoSizes.Large.Height, 0);
					Assert.Greater(configuration.PhotoSizes.Large.Width, 0);
					Assert.IsNotNullOrEmpty(configuration.PhotoSizes.Large.Resize);
				}

        [Test]
        public void Can_parse_ids_greater_than_53_bits()
        {
            const string json = "{ \"id\": 90071992547409921}";
            var status = new TwitterService().Deserialize<TwitterStatus>(json);
            Assert.IsNotNull(status);
            Assert.AreEqual(90071992547409921, status.Id);
        }

        [Test]
        public void Can_get_direct_messages()
        {
            var service = GetAuthenticatedService();
            var dms = service.ListDirectMessagesReceived(new ListDirectMessagesReceivedOptions());

            Assert.IsNotNull(dms);
            Assert.IsTrue(dms.Count() <= 20);

            Assert.IsNotNull(service.Response);
            AssertResultWas(service, HttpStatusCode.OK);

            foreach (var tweet in dms)
            {
                Assert.IsNotNull(tweet.RawSource);
                Assert.AreNotEqual(default(DateTime), tweet.CreatedDate);

                Console.WriteLine("{0} said '{1}'", tweet.SenderScreenName, tweet.Text);
            }

            AssertRateLimitStatus(service);
        }

        [Test]
        public void Can_get_direct_messages_async_callback_style()
        {
            var service = GetAuthenticatedService();
            var result = service.ListDirectMessagesReceived(new ListDirectMessagesReceivedOptions { FullText = true },
                (dms, response) =>
                    {
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                        Assert.IsNotNull(dms);
                        Assert.AreEqual(20, dms.Count());

                        foreach (var dm in dms)
                        {
                            Console.WriteLine("{0} said '{1}'", dm.SenderScreenName, dm.Text);
                        }
                    });

            result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Can_get_direct_messages_begin_end_style()
        {
            var service = GetAuthenticatedService();
            var result = service.BeginListDirectMessagesReceived(new ListDirectMessagesReceivedOptions { FullText = true, Count = 5 });
            var dms = service.EndListDirectMessagesReceived(result, TimeSpan.FromSeconds(5));
            
            Assert.IsNotNull(dms);
            Assert.Greater(dms.Count(), 0);

            foreach (var dm in dms)
            {
                Console.WriteLine("{0} said '{1}'", dm.SenderScreenName, dm.Text);
            }
        }
        
        [Test]
        public void Can_deserialize_dates()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            Assert.IsNotNull(tweets);
            Assert.IsTrue(tweets.Any());

            foreach (var tweet in tweets)
            {
                Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Id);
                Assert.AreNotEqual(default(DateTime), tweet.CreatedDate);
            }                       
        }

        [Test]
        public void Can_get_mentions_and_fail_due_to_unauthorized_request()
        {
            var service = new TwitterService();
            var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions());

            Assert.AreEqual(HttpStatusCode.BadRequest, service.Response.StatusCode);
            Assert.IsNull(mentions);

            var error = service.Response.Error;
            Assert.IsNotNull(error);
            Assert.IsNotNullOrEmpty(error.Message);
            Trace.WriteLine(error.ToString());
        }

        [Test]
        public void Can_get_mentions()
        {
            var service = GetAuthenticatedService();
            var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions()).ToList();

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(mentions);
            Assert.IsTrue(mentions.Count() <= 20);

            var rate = service.Response.RateLimitStatus;
            Assert.IsNotNull(rate);
            Console.WriteLine("You have " + rate.RemainingHits + " left out of " + rate.HourlyLimit);

            foreach (var dm in mentions)
            {
                Console.WriteLine("{0} said '{1}'", dm.User.ScreenName, dm.Text);
            }
        }

        [Test]
        public void Can_get_authenticated_user_profile()
        {
            var service = GetAuthenticatedService();
            var profile = service.GetUserProfile(new GetUserProfileOptions());

            Trace.WriteLine(service.Response.Response);

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(profile);
            Assert.IsNotNullOrEmpty(profile.ScreenName);
        }

        [Test]
        public void Can_get_user_profile_for()
        {
            var service = GetAuthenticatedService();
            var profile = GetHeroProfile(service);

            Trace.WriteLine(service.Response.Response);

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(profile);
            Assert.IsNotNullOrEmpty(profile.ScreenName);
        }

				[Test]
				public void Can_destroy_tweet()
				{
					var service = GetAuthenticatedService();
					var status = "This tweet should self-destruct in 5 seconds. " + Guid.NewGuid().ToString();
					var tweet = service.SendTweet(new SendTweetOptions { Status = status });

					AssertResultWas(service, HttpStatusCode.OK);
					Assert.IsNotNull(tweet);
					Assert.AreNotEqual(0, tweet.Id);

					System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

					var deletedStatus = service.DeleteTweet(new DeleteTweetOptions() { Id = tweet.Id });
					AssertResultWas(service, HttpStatusCode.OK);
					Assert.IsNotNull(deletedStatus);
					Assert.AreEqual(deletedStatus.Id, tweet.Id);

					var foundStatus = service.GetTweet(new GetTweetOptions() { Id = deletedStatus.Id });
					AssertResultWas(service, HttpStatusCode.NotFound);
					Assert.IsNull(foundStatus);
				}

        [Test]
        public void Can_tweet()
        {
            var service = GetAuthenticatedService();
            var status = _hero + DateTime.UtcNow.Ticks + " Tweet from TweetSharp unit tests";
            var tweet = service.SendTweet(new SendTweetOptions { Status = status });

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

				[Test]
				public void Can_tweet_accented_chars()
				{
					var service = GetAuthenticatedService();
					//var status = "Hello à....";
					var status = "Can_tweet_with_image:Tweet an accented char à....";
					var tweet = service.SendTweet(new SendTweetOptions { Status = status });
					
					AssertResultWas(service, HttpStatusCode.OK);
					Assert.IsNotNull(tweet);
					Assert.AreNotEqual(0, tweet.Id);
				}

        [Test]
        public void Can_tweet_with_geo()
        {
            // status=123&lat=56.95&%40long=24.1&include_entities=1
            var service = GetAuthenticatedService();
            var tweet = service.SendTweet(new SendTweetOptions { Status = DateTime.UtcNow.Ticks.ToString(), Lat = 56.95, Long = 24.1});

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        [Ignore("Makes a live status update")]
        public void Can_direct_message()
        {
            var service = GetAuthenticatedService();
            var recipient = GetHeroProfile(service);
            var tweet = service.SendDirectMessage(new SendDirectMessageOptions() { UserId = recipient.Id, Text = DateTime.UtcNow.Ticks.ToString()});
            
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        [Ignore("Makes a live status update")]
        public void Can_direct_message_with_url_without_double_entities()
        {
            var service = GetAuthenticatedService();

            var recipient = GetHeroProfile(service);
            var tweet = service.SendDirectMessage(new SendDirectMessageOptions { UserId = recipient.Id, Text = string.Format("http://tweetsharp.com {0}", DateTime.UtcNow.Ticks)});
            var urls = tweet.Entities.OfType<TwitterUrl>();

            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
            
            foreach(var url in urls)
            {
                Console.WriteLine(url.Value);
            }
            
            Assert.AreEqual(1, urls.Count());
        }

        [Test]
        [Ignore("Makes a live status update")]
        public void Can_direct_message_with_screen_name()
        {
            var service = GetAuthenticatedService();

            var recipient = GetHeroProfile(service);
            var tweet = service.SendDirectMessage(new SendDirectMessageOptions { ScreenName = recipient.ScreenName, Text = DateTime.UtcNow.Ticks.ToString()});

            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        private TwitterUser GetHeroProfile(TwitterService service)
        {
            return service.GetUserProfileFor(new GetUserProfileForOptions { ScreenName = _hero });
        }

        [Test]
        [Ignore("Makes a live status update")]
        public void Can_direct_message_with_a_url()
        {
            var service = GetAuthenticatedService();

            var recipient = GetHeroProfile(service);
            var tweet = service.SendDirectMessage(new SendDirectMessageOptions { UserId = recipient.Id, Text = "http://tweetsharp.com"});

            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        public void Can_tweet_with_special_characters()
        {
            var service = GetAuthenticatedService();

            var message = "!@#$%^&*();:-" + DateTime.UtcNow.Ticks;
            var tweet = service.SendTweet(new SendTweetOptions { Status = message });
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        public void Can_tweet_with_location_custom_type()
        {
            var service = GetAuthenticatedService();
            var tweet = service.SendTweet(new SendTweetOptions { Status = DateTime.UtcNow.Ticks.ToString(), Lat = 45.43989910068863, Long = -75.69168090820312 });
            
            var uri = service.Response.RequestUri;
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            var location = queryString["location"];
            Assert.AreNotEqual("TweetSharp.TwitterGeoLocation", location);

            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        public void Can_tweet_and_handle_dupes()
        {
            var service = GetAuthenticatedService();

            service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet"});
            var response = service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet"});

						Assert.IsNull(response);
						Assert.IsNotNull(service.Response);
						Assert.AreNotEqual(HttpStatusCode.OK, service.Response.StatusCode);

						var error = service.Response.Error;
						Assert.IsNotNull(error);
						Assert.IsNotNullOrEmpty(error.Message);
        }

        [Test]
        public void Can_tweet_with_image()
        {
            var service = GetAuthenticatedService();
            using (var stream = new FileStream("daniel_8bit.png", FileMode.Open))
            {
                var tweet = service.SendTweetWithMedia(new SendTweetWithMediaOptions
                    {
                        Status = "Can_tweet_with_image:Tweet",
                        Images = new Dictionary<string, Stream> {{"test", stream}}
                    });
                Assert.IsNotNull(tweet);
                Assert.AreNotEqual(0, tweet.Id);
            }            
        }

				[Test]
				public void Can_tweet_with_image_and_accented_char()
				{
					//This test currently fails. Don't know why. Response is an error
					//to do with authorisation failing, but everything looks correct.
					//Tweeting with image an no accented character works, using
					//alternate endpoint to tweet status with accented character and
					//no media also works, but this one fails if both conditions are true.
					//This method is deprecated anywasy and using uploadmedia + the normal
					//status update with mediaids works even when an accented char is present
					//so not critical long term, but it would be great to understand why
					//this is an issue and possibly fix it.
					var service = GetAuthenticatedService();
					service.TraceEnabled = true;
					using (var stream = new FileStream("daniel_8bit.png", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var tweet = service.SendTweetWithMedia(new SendTweetWithMediaOptions
						{
							Status = "Can_tweet_with_image:Tweet and accented char à", 
							Images = new Dictionary<string, Stream> { { "test", stream } }
						});

						AssertResultWas(service, HttpStatusCode.OK);
						Assert.IsNotNull(tweet);
						Assert.AreNotEqual(0, tweet.Id);
					}
				}

				[Test]
				public void Can_upload_media()
				{
					var service = GetAuthenticatedService();
					service.TraceEnabled = true;
					using (var stream = new FileStream("daniel_8bit.png", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var uploadedMedia = service.UploadMedia(new UploadMediaOptions
						{
							Media = new MediaFile() { FileName = "test", Content = stream }
						});

						AssertResultWas(service, HttpStatusCode.OK);
						Assert.IsNotNull(uploadedMedia);
						Assert.AreNotEqual(0, uploadedMedia.Media_Id);
					}
				}

				[Test]
				public void Can_tweet_uploaded_media_and_accented_char()
				{
					List<string> mediaIds = new List<string>(2);

					var service = GetAuthenticatedService();
					service.TraceEnabled = true;
					using (var stream = new FileStream("daniel_8bit.png", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var uploadedMedia = service.UploadMedia(new UploadMediaOptions
						{
							Media = new MediaFile { FileName = "test", Content = stream } 
						});

						AssertResultWas(service, HttpStatusCode.OK);
						Assert.IsNotNull(uploadedMedia);
						Assert.AreNotEqual(0, uploadedMedia.Media_Id);
						mediaIds.Add(uploadedMedia.Media_Id);
					}

					using (var stream = new FileStream("Sparrow.jpg", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						var uploadedMedia = service.UploadMedia(new UploadMediaOptions
						{
							Media = new MediaFile { FileName = "test2", Content = stream } 
						});

						AssertResultWas(service, HttpStatusCode.OK);
						Assert.IsNotNull(uploadedMedia);
						Assert.AreNotEqual(0, uploadedMedia.Media_Id);
						mediaIds.Add(uploadedMedia.Media_Id);
					}

					service.SendTweet(new SendTweetOptions() { Status = "TweetMoaSharp:Can_tweet_uploaded_media_and_accented_char:Tweet and accented char à....", MediaIds = mediaIds });
				}

        [Test]
        public void Can_get_followers_on_first_page()
        {
            var service = GetAuthenticatedService();
            var followers = service.ListFollowers(new ListFollowersOptions());
            Assert.IsNotNull(followers);
        }

        [Test]
        public void Can_get_friends_on_first_page()
        {
            var service = GetAuthenticatedService();
            TwitterCursorList<TwitterUser> followers = service.ListFriends(new ListFriendsOptions());
            Assert.IsNotNull(followers);
        }

        [Test]
        public void Can_get_followers_from_authenticated_user()
        {
            var service = GetAuthenticatedService();
            var followers = service.ListFollowers(new ListFollowersOptions());
            Assert.IsNotNull(followers);
            Assert.IsTrue(followers.Count > 0);
        }
        
        [Test]
        public void Can_get_favorites_for()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListFavoriteTweets(new ListFavoriteTweetsOptions { ScreenName = _hero });

            Console.WriteLine(service.Response.Response);

            foreach (var tweet in tweets)
            {
                Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
            }
        }

        [Test]
        public void Can_create_and_destroy_favorite()
        {
            // http://twitter.com/#!/kellabyte/status/16578173168779264
            var service = GetAuthenticatedService();
            var fave = service.FavoriteTweet(new FavoriteTweetOptions { Id = 16578173168779264 });
            if(service.Response != null && (int)service.Response.StatusCode == 403)
            {
                Trace.WriteLine("This tweet is already a favorite.");
            }
            else
            {
                AssertResultWas(service, HttpStatusCode.OK);
                Assert.IsNotNull(fave);
            }

            var unfave = service.UnfavoriteTweet(new UnfavoriteTweetOptions{ Id = 16578173168779264 });
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(unfave);
        }

        [Test]
        public void Can_get_favorites()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListFavoriteTweets(new ListFavoriteTweetsOptions());

            Console.WriteLine(service.Response.Response);

            foreach (var tweet in tweets)
            {
                Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
            }
        }

        [Test]
        public void Can_get_favorites_async()
        {
            var service = GetAuthenticatedService();
            var result = service.BeginListFavoriteTweets(new ListFavoriteTweetsOptions { ScreenName = _hero});
            var tweets = service.EndListFavoriteTweets(result);

            Console.WriteLine(service.Response.Response);

            foreach (var tweet in tweets)
            {
                Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
            }
        }
        
        [Test]
        public void Can_get_friends_or_followers_with_next_cursor()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

            var followers = service.ListFollowers(new ListFollowersOptions { ScreenName = _hero });
            Assert.IsNotNull(followers);
            Assert.IsNotNull(followers.NextCursor);
            Assert.IsNotNull(followers.PreviousCursor);
        }

        [Test]
        public void Can_create_and_destroy_saved_search()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.AuthenticateWith(_accessToken, _accessTokenSecret);

            // Twitter 403's on duplicate saved search requests, so delete if found
            var searches = service.ListSavedSearches();
            Assert.IsNotNull(searches);

            var existing = searches.SingleOrDefault(s => s.Query.Equals("tweetsharp"));
            if(existing != null)
            {
                var deleted = service.DeleteSavedSearch(new DeleteSavedSearchOptions { Id = existing.Id });
                Assert.IsNotNull(deleted);
                Assert.IsNotNullOrEmpty(deleted.Query);
                Assert.AreEqual(deleted.Query, existing.Query);
            }

            var search = service.CreateSavedSearch(new CreateSavedSearchOptions { Query = "tweetsharp" });
            Assert.IsNotNull(search);
            Assert.AreEqual("tweetsharp", search.Query);
        }

        [Test]
        public void Can_search()
        {
            var service = GetAuthenticatedService();
            var results = service.Search(new SearchOptions { Q = "tweetsharp", Count = 10});
            
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Statuses.Count() <= 10);

            foreach(var tweet in results.Statuses)
            {
                Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
            }
        }

        [Test]
        public void Can_search_with_geo_and_lang()
        {
            var italyGeoCode = new TwitterGeoLocationSearch(41.9, 12.5, 10, TwitterGeoLocationSearch.RadiusType.Mi);
            var service = GetAuthenticatedService();
            var results = service.Search(new SearchOptions { Q = "papa", Geocode = italyGeoCode, Lang = "en", Count = 100,  });

            Assert.IsNotNull(results);
            if (!results.Statuses.Any())
            {
                Assert.Inconclusive("No tweets to check the location of to match within search radius");
            }

            Assert.IsTrue(results.Statuses.Count() <= 100);
            var geoTaggedTweets = results.Statuses.Where(x => x.Location != null);
            if (!geoTaggedTweets.Any())
            {
                Assert.Inconclusive("Unable to find tweets that were geo tagged for this test");
            }
            foreach (var tweet in geoTaggedTweets)
            {
                Console.WriteLine("{0} says '{1}' ({2})", tweet.User.ScreenName, tweet.Text, tweet.Id);
                
                //Twitter API does not return coordinates in search request
                Assert.IsTrue(tweet.IsWithinSearchRadius(italyGeoCode));
            }
        }

        [Test]
        public void Searches_with_explicit_include_options_still_work()
        {
            var service = GetAuthenticatedService();
            var results = service.Search(new SearchOptions
            {
                Count = 500,
                Resulttype = TwitterSearchResultType.Mixed,
                IncludeEntities = false,
                Q = "stackoverflow"
            });

            Assert.IsNotNull(results);
            foreach (var result in results.Statuses)
            {
                Console.WriteLine(result.Text);
            }
        }

        [Test]
        public void Can_get_raw_source_from_search()
        {
            var service = GetAuthenticatedService();
            var results = service.Search(new SearchOptions {Q = "tweetsharp", Count = 10});
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Statuses.Count() <= 10);
            if(!results.Statuses.Any())
            {
                Assert.Ignore("No search results provided for this test");
            }

            foreach (var tweet in results.Statuses)
            {
                Assert.IsNotNullOrEmpty(tweet.RawSource);
                Console.WriteLine("{0} says '{1}", tweet.User.ScreenName, tweet.Text);
            }
        }

        [Test]
        public void Can_get_friendship_info()
        {
            var service = GetAuthenticatedService();
            var friendship = service.GetFriendshipInfo(new GetFriendshipInfoOptions { SourceScreenName = "jdiller", TargetScreenName = "danielcrenna" });

            Assert.IsNotNull(friendship);
            Assert.IsNotNull(friendship.Relationship);
        }

        [Test]
        public void Can_get_user_suggestion_categories_and_users()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var service = GetAuthenticatedService();
            var categories = service.ListSuggestedUserCategories(new ListSuggestedUserCategoriesOptions());
            Assert.IsNotNull(categories);
            Assert.IsTrue(categories.Count() > 0);

            foreach(var category in categories)
            {
                Trace.WriteLine(category.RawSource);
                Trace.WriteLine(string.Format("{0}({1})", category.Name, category.Slug));
            }
            
            var suggestions = service.ListSuggestedUsers(new ListSuggestedUsersOptions() { Slug = categories.First().Slug});
            Assert.IsNotNull(suggestions);
            Assert.IsNotNull(suggestions.Users);
            Assert.IsTrue(suggestions.Users.Count() > 0);

            foreach(var user in suggestions.Users)
            {
                Trace.WriteLine(user.ScreenName);
            }
        }

        [Test]
        public void Can_get_tweet()
        {
            var service = GetAuthenticatedService();
            var tweet = service.GetTweet(new GetTweetOptions { Id = 10080880705929216 });

            Assert.IsNotNull(tweet);
            Assert.IsNotNull(service.Response);
            Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
        }

			  [Test]
        public void Can_get_tweet_with_multiple_images()
        {
            var service = GetAuthenticatedService();
						var tweet = service.GetTweet(new GetTweetOptions { Id = 568680219474726912 });

            Assert.IsNotNull(tweet);
            Assert.IsNotNull(service.Response);
            Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
						Assert.AreEqual(3, tweet.ExtendedEntities.Count());
						Assert.AreEqual(1, tweet.Entities.Count());
				}

			  [Test]
        public void Can_get_tweet_with_animated_gif()
        {
            var service = GetAuthenticatedService();
						var tweet = service.GetTweet(new GetTweetOptions { Id = 480032281591939072 });

            Assert.IsNotNull(tweet);
            Assert.IsNotNull(service.Response);
            Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
						Assert.AreEqual(1, tweet.ExtendedEntities.Count());
				}

				[Test]
				public void Can_get_tweet_with_merged_entities()
				{
					var service = GetAuthenticatedService(new JsonSerializer() { MergeMultiplePhotos = true });
					var tweet = service.GetTweet(new GetTweetOptions { Id = 568680219474726912 });

					Assert.IsNotNull(tweet);
					Assert.IsNotNull(service.Response);
					Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
					Assert.AreEqual(3, tweet.ExtendedEntities.Count());
					Assert.AreEqual(3, tweet.Entities.Count());
				}

				[Test]
				public void Can_get_tweet_with_video()
				{
					var service = GetAuthenticatedService(new JsonSerializer() { MergeMultiplePhotos = true });
					var tweet = service.GetTweet(new GetTweetOptions { Id = 560049149836808192 });

					Assert.IsNotNull(tweet);
					Assert.IsNotNull(service.Response);
					Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
					Assert.AreEqual(1, tweet.ExtendedEntities.Count());
					var ve = tweet.ExtendedEntities.First();
					Assert.AreEqual(4, ve.Sizes.Count());
					Assert.IsNotNull(ve.VideoInfo);
					Assert.AreEqual(30008, ve.VideoInfo.DurationMs);
					Assert.AreEqual(5, ve.VideoInfo.Variants.Count());
					Assert.AreEqual(2, ve.VideoInfo.AspectRatio.Count);
					Assert.AreEqual(1, ve.VideoInfo.AspectRatio[0]);
					Assert.AreEqual(1, ve.VideoInfo.AspectRatio[1]);
					Assert.AreEqual("video/webm", ve.VideoInfo.Variants.First().ContentType);
					Assert.AreEqual(832000, ve.VideoInfo.Variants.First().BitRate);
					Assert.AreEqual("https://video.twimg.com/ext_tw_video/560049056895209473/pu/vid/480x480/gj_fzyk29R9dMPBY.webm", ve.VideoInfo.Variants.First().Url.ToString());
				}

        [Test]
        public void Can_get_tweet_async()
        {
            var service = GetAuthenticatedService();
            var result = service.BeginGetTweet(new GetTweetOptions { Id = 10080880705929216 });
            var tweet = service.EndGetTweet(result);

            Assert.IsNotNull(tweet);
            Assert.IsNotNull(service.Response);
            Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
        }

        [Test]
        public void Can_send_direct_message()
        {
            var service = new TwitterService { IncludeEntities = true };
            service.AuthenticateWith(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            var response = service.SendDirectMessage(new SendDirectMessageOptions
            {
                ScreenName = _hero,
                Text = "Test a tweetsharp dm " + DateTime.Now.Ticks
            });
            
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Id == 0);
        }

        [Test]
        public void Can_delete_direct_message()
        {
            var service = new TwitterService { IncludeEntities = true };
            service.AuthenticateWith(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            var created = service.SendDirectMessage(new SendDirectMessageOptions
            {
                ScreenName = _hero,
                Text = "Test of a tweetsharp dm " + DateTime.Now.Ticks
            });
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(created);
            Assert.IsFalse(created.Id == 0);

            var deleted = service.DeleteDirectMessage(new DeleteDirectMessageOptions { Id = created.Id});
            Assert.IsNotNull(deleted);
            Assert.AreEqual(deleted.Id, created.Id);
        }

        [Test]
        public void Can_get_entities_on_direct_messages()
        {
            var service = new TwitterService { IncludeEntities = true };            
            service.AuthenticateWith(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            
            var tweets = service.ListDirectMessagesSent(new ListDirectMessagesSentOptions { FullText = true });
            if(!tweets.Any())
            {
                Assert.Ignore("No direct messages available to verify entities");
            }

            Console.WriteLine(service.Response.Response);

            foreach (var tweet in tweets)
            {
                Assert.IsNotNull(tweet.Entities);
                var coalesced = tweet.Entities.Coalesce();
                var text = tweet.Text;

                Assert.IsNotNull(tweet.TextAsHtml);
                Console.WriteLine("Tweet: " + text);
                Console.WriteLine("HTML: " + tweet.TextAsHtml);
                foreach(var entity in coalesced)
                {
                    switch(entity.EntityType)
                    {
                        case TwitterEntityType.HashTag:
                            var hashtag = ((TwitterHashTag) entity).Text;
                            Console.WriteLine(hashtag);
                            var hashtagText = text.Substring(entity.StartIndex, entity.EndIndex - entity.StartIndex);
                            Assert.AreEqual("#" + hashtag, hashtagText);
                            break;
                        case TwitterEntityType.Mention:
                            var mention = ((TwitterMention) entity).ScreenName;
                            Console.WriteLine(mention);
                            var mentionText = text.Substring(entity.StartIndex, entity.EndIndex - entity.StartIndex);
                            Assert.AreEqual("@" + mention, mentionText);
                            break;
                        case TwitterEntityType.Url:
                            var url = ((TwitterUrl) entity).Value;
                            Console.WriteLine(url);
                            var urlText = text.Substring(entity.StartIndex, entity.EndIndex - entity.StartIndex);
                            Assert.AreEqual(url, urlText);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                Console.WriteLine();
            }
        }
        
        [Test]
        public void Can_get_entities_on_timeline()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());
            
            foreach (var tweet in tweets)
            {
                Assert.IsNotNull(tweet.Entities);
                if (tweet.Entities.HashTags.Any())
                {
                    foreach (var hashtag in tweet.Entities.HashTags)
                    {
                        Assert.IsNotNullOrEmpty(hashtag.Text);
                    }
                }
                if (tweet.Entities.Urls.Count() > 0)
                {
                    foreach (var url in tweet.Entities.Urls)
                    {
                        Assert.IsNotNullOrEmpty(url.Value);
                    }
                }
                if (tweet.Entities.Mentions.Count() > 0)
                {
                    foreach (var mention in tweet.Entities.Mentions)
                    {
                        Assert.IsNotNullOrEmpty(mention.ScreenName);
                    }
                }
            }
        }

        [Test]
        public void Can_coalesce_entities_on_timeline()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions());

            var passed = false;
            
            foreach(var tweet in tweets)
            {
							//Appears on retweets that are over 140 chars multiple entities near the end can end up being assigned a start of 139.
							//Twitter recommends using entities from the original tweet anyway.

							var tweetToTest = tweet;
							if (tweetToTest.RetweetedStatus != null)
								tweetToTest = tweetToTest.RetweetedStatus;

								if (tweetToTest.Entities == null)
                {
                    continue;
                }

								var entities = tweetToTest.Entities.Coalesce();
								if (entities.Count() < 2)
                {
                    continue;
                }

                var previous = -1;
                foreach(var entity in entities)
                {
                    Assert.IsTrue(previous < entity.StartIndex);
                    previous = entity.StartIndex;
                }

                passed = true;
            }

            if(!passed)
            {
                Assert.Ignore("This test pass yielded no entities with both a hashtag and a URL.");
            }
        }

        [Test]
        public void Can_get_tweets_on_user_timeline_with_since_paging()
        {
            var service = GetAuthenticatedService();
            var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { Count = 200 }).ToList();
            foreach (var tweet in tweets)
            {
                Assert.IsNotNull(tweet.RawSource);
                Assert.IsNotNull(tweet.Entities);

                Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Text);
            }
            if(!tweets.Any()) Assert.Ignore();
            var sinceId = tweets.Last().Id;
            var tweets2 = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { SinceId = sinceId, Count = 200 });
            foreach (var tweet in tweets2)
            {
                Assert.IsNotNull(tweet.RawSource);
                Assert.IsNotNull(tweet.Entities);
            }
        }

        [Test]
        public void Can_get_tweets_on_specified_user_timeline()
        {
            var service = GetAuthenticatedService();

            var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = "mabster" });
            foreach(var tweet in tweets)
            {
                Assert.IsNotNull(tweet.RawSource);
                Assert.IsNotNull(tweet.Entities);

                Console.WriteLine("{0} said '{1}'", tweet.User.ScreenName, tweet.Text);
            }
        }

        [Test]
        public void Can_get_user_lists()
        {
            var service = GetAuthenticatedService();
            var lists = service.ListListsFor(new ListListsForOptions() { ScreenName = _hero });

            Assert.IsNotNull(lists);
            if(!lists.Any())
            {
                Assert.Ignore("This test account has no lists");
            }

            foreach(var list in lists)
            {
                Assert.IsNotNullOrEmpty(list.Name);
                Trace.WriteLine(list.Name);
            }
        }


				[Test]
				public void Can_limit_list_members()
				{
					var service = GetAuthenticatedService();
					var lists = service.ListListsFor(new ListListsForOptions() { ScreenName = "yortw" });

					Assert.IsNotNull(lists);
					if (!lists.Any())
					{
						Assert.Ignore("This test account has no lists");
					}
					
					var list = (from l in lists where l.MemberCount > 1 select l).FirstOrDefault();
					Assert.IsNotNull(list, "No lists with more than one member.");

					var membersCursor = service.ListListMembers(new ListListMembersOptions() { ListId = list.Id, Count = 1 });
					Assert.AreEqual(1, membersCursor.Count);
				}

        [Test]
        public void Can_create_and_delete_list()
        {
            var service = GetAuthenticatedService();
            var list =
                service.CreateList(new CreateListOptions
                                       {
                                           ListOwner = _hero,
                                           Name = "test-list",
                                           Mode = TwitterListMode.Public
                                       });

            Assert.IsNotNull(list);
            Assert.IsNotNullOrEmpty(list.Name);
            Assert.AreEqual(list.Name, "test-list");

            list = service.DeleteList(new DeleteListOptions { ListId = list.Id});
            Assert.IsNotNull(list);
            Assert.IsNotNullOrEmpty(list.Name);
            Assert.AreEqual(list.Name, "test-list");
        }

        [Test]
        public void Can_get_followers_ids()
        {
            var service = GetAuthenticatedService();
            var followers = service.ListFollowerIdsOf(new ListFollowerIdsOfOptions(){ScreenName = _hero});
            Assert.IsNotNull(followers);
            Assert.IsTrue(followers.Count > 0);
        }

        [Test]
        public void Can_get_friend_ids()
        {
            var service = GetAuthenticatedService();
            var friends = service.ListFriendIdsOf(new ListFriendIdsOfOptions(){ScreenName = _hero});
            Assert.IsNotNull(friends);
            Assert.IsTrue(friends.Count > 0);
        }
        
        [Test]
        public void Can_get_available_local_trend_locations()
        {
            var service = GetAuthenticatedService();
            var locations = service.ListAvailableTrendsLocations();
            Assert.IsNotNull(locations);

            foreach(var location in locations)
            {
                Trace.WriteLine(string.Format("{0}:{1}, {2}[{3}]", location.WoeId, location.Name, location.Country, location.PlaceType.Name));
            }
        }

        [Test]
        public void Can_update_profile_image()
        {
            var service = GetAuthenticatedService();
            var user = service.UpdateProfileImage(new UpdateProfileImageOptions { ImagePath = "daniel_8bit.png" });
            Assert.IsNotNull(user);
            Assert.AreEqual(HttpStatusCode.OK, service.Response.StatusCode);
        }


        [Test]
        public void Can_get_local_trends()
        {
            var service = GetAuthenticatedService();
            var trendList = service.ListLocalTrendsFor(new ListLocalTrendsForOptions { Id = 4118 });
            Assert.IsNotNull(trendList);

            foreach (var trend in trendList)
            {
                Trace.WriteLine(trend.Query);
            }
        }

        [Test]
        public void Can_get_multiple_user_profiles()
        {
            var service = GetAuthenticatedService();
            var users = service.ListUserProfilesFor(new ListUserProfilesForOptions() { ScreenName = new[] { "danielcrenna", "jdiller" } });

            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count());
        }

        private static void AssertResultWas(TwitterService service, HttpStatusCode statusCode)
        {
            Assert.IsNotNull(service.Response);
            Assert.AreEqual(statusCode, service.Response.StatusCode);
        }

        private static void AssertRateLimitStatus(TwitterService service)
        {
            var rate = service.Response.RateLimitStatus;
            Assert.IsNotNull(rate);
            Assert.AreNotEqual(0, rate.HourlyLimit);
            Console.WriteLine();
            Console.WriteLine("{0} / {1} API calls remaining", rate.RemainingHits, rate.HourlyLimit);
        }

				private TwitterService GetAuthenticatedService(JsonSerializer serializer)
				{
					var service = new TwitterService(_consumerKey, _consumerSecret);
					if (serializer != null)
					{
						service.Serializer = serializer;
						service.Deserializer = serializer;
					}

					service.TraceEnabled = true;
					service.AuthenticateWith(_accessToken, _accessTokenSecret);
					return service;
				}
		
        private TwitterService GetAuthenticatedService()
        {
					return GetAuthenticatedService(null);
				}

        /// <summary>
        /// Tests that can accept a twitter stream
        /// </summary>
        /// <remarks>
        /// Tests for up to 5 events to occur. Test this with a twitter account that follows 
        /// several hundred accounts, or be prepared to send your account a few DM's while
        /// this test is running.
        /// </remarks>
        [Test]
        [Ignore("See remarks - Can potentially stall for a while")]
        public void Can_stream_from_user_stream()
        {
            const int maxStreamEvents = 5;
            
            var block = new AutoResetEvent(false);
            var count = 0;
            
            var service = GetAuthenticatedService();

            service.StreamUser((streamEvent, response) =>
            {
                if (streamEvent is TwitterUserStreamEnd)
                {
                    block.Set();
                }

                if (response.StatusCode == 0)
                {
                    if (streamEvent is TwitterUserStreamFriends)
                    {
                        var friends = (TwitterUserStreamFriends)streamEvent;
                        Assert.IsNotNull(friends);
                        Assert.IsNotNull(friends.RawSource);
                        Assert.IsTrue(friends.Ids.Any());
                    }

                    if (streamEvent is TwitterUserStreamEvent)
                    {
                        var @event = (TwitterUserStreamEvent)streamEvent;
                        Assert.IsNotNull(@event);
                        Assert.IsNotNull(@event.TargetObject);
                        Assert.IsNotNull(@event.RawSource);
                    }

                    if (streamEvent is TwitterUserStreamStatus)
                    {
                        var tweet = ((TwitterUserStreamStatus)streamEvent).Status;
                        Assert.IsNotNull(tweet);
                        Assert.IsNotNull(tweet.Id);
                        Assert.IsNotNull(tweet.User);
                        Assert.IsNotNull(tweet.RawSource);
                        Assert.IsNotNull(tweet.User.ScreenName);
                    }

                    if (streamEvent is TwitterUserStreamDirectMessage)
                    {
                        var dm = ((TwitterUserStreamDirectMessage)streamEvent).DirectMessage;
                        Assert.IsNotNull(dm);
                        Assert.IsNotNull(dm.Id);
                        Assert.IsNotNull(dm.Sender);
                        Assert.IsNotNull(dm.Recipient);
                        Assert.IsNotNull(dm.RawSource);
                    }

                    if (streamEvent is TwitterUserStreamDeleteStatus)
                    {
                        var deleted = (TwitterUserStreamDeleteStatus)streamEvent;
                        Assert.IsNotNull(deleted);
                        Assert.IsTrue(deleted.StatusId > 0);
                        Assert.IsTrue(deleted.UserId > 0);
                    }

                    if (streamEvent is TwitterUserStreamDeleteDirectMessage)
                    {
                        var deleted = (TwitterUserStreamDeleteDirectMessage)streamEvent;
                        Assert.IsNotNull(deleted);
                        Assert.IsTrue(deleted.DirectMessageId > 0);
                        Assert.IsTrue(deleted.UserId > 0);
                    }
                    count++;
                    if (count == maxStreamEvents)
                    {
                        block.Set();
                    }
                }
                else
                {
                    Assert.Ignore("Stream responsed with status code: {0}", response.StatusCode);
                }
            });

            block.WaitOne();
            service.CancelStreaming();
        }

        [Test]
        public void Can_get_friendship_lookup()
        {
            var service = GetAuthenticatedService();
            var friendships = service.ListFriendshipsFor(new ListFriendshipsForOptions() { ScreenName = new[] { "danielcrenna" } });
            Assert.IsNotNull(friendships);
        }

        [Test]
        public void Can_loop_through_followers()
        {
            var service = GetAuthenticatedService();
            var me = service.GetUserProfile(new GetUserProfileOptions());

            var count = 0;
            var followers = service.ListFollowers(new ListFollowersOptions { UserId = me.Id});
            count += followers.Count;
            while (followers.NextCursor != 0)
            {
                followers = service.ListFollowers(new ListFollowersOptions { UserId = me.Id, Cursor = followers.NextCursor});
                count += followers.Count;
            }     
            Assert.AreEqual(me.FollowersCount, count);
        }

        [Test]
        public void Can_list_tweets_on_list()
        {
            var service = GetAuthenticatedService();
            var result = service.BeginListTweetsOnList(new ListTweetsOnListOptions
            {
                OwnerScreenName = "Joesebok",
                Slug = "poker",
                IncludeRts = true,
                SinceId = 308773934705299458
            });
            var tweets = service.EndListTweetsOnList(result);
            Assert.IsNotNull(tweets);
            
            foreach (var tweet in tweets)
            {
                Console.WriteLine(tweet.Id);
            }
        }

        [Test]
        public void Can_get_rate_limit_status_summary()
        {
            var service = GetAuthenticatedService();
            var summary = service.GetRateLimitStatus(new GetRateLimitStatusOptions());
            Assert.IsNotNull(summary);
            Assert.IsNotNullOrEmpty(summary.AccessToken);



            Console.WriteLine(service.Response.Response);

        }

        [Test]
        public void Can_Deserialize_Integer_GeoCoordinates()
        {
            //coordinates of this tweet are 2 integers  "{\"type\":\"Point\",\"coordinates\":[10, 234]}";
            TwitterService service = GetAuthenticatedService();
            TwitterStatus tweet = service.GetTweet(new GetTweetOptions() { Id = 294853375609163776 });
            Assert.NotNull(tweet);
        }

        [Test]
        public void Recursive_issues_on_private_accounts()
        {
            TwitterService service = GetAuthenticatedService();
            var tweets2 = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions() {ScreenName = "ventamatic"});
            Assert.IsNotNull(tweets2);

            string content = @"[{""created_at"":""Sat Jun 22 11:12:07 +0000 2013"",""id"":348397986730086400,""id_str"":""348397986730086400"",""text"":""Ara, mmmmh http://t.co/kUu5oUb5k6"",""source"":""\u003ca href=""http://www.facebook.com/twitter"" rel=""nofollow""\u003eFacebook\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/kUu5oUb5k6"",""expanded_url"":""http://fb.me/ImltDsYP"",""display_url"":""fb.me/ImltDsYP"",""indices"":[11,33]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""et""},{""created_at"":""Sat Jun 22 08:30:37 +0000 2013"",""id"":348357344675119104,""id_str"":""348357344675119104"",""text"":""How social media helped several massive protests this week: http://t.co/SxPYXilUNx #global #trends"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":34,""favorite_count"":10,""entities"":{""hashtags"":[{""text"":""global"",""indices"":[83,90]\},\{""text"":""trends"",""indices"":[91,98]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/SxPYXilUNx"",""expanded_url"":""http://ow.ly/mhc3E"",""display_url"":""ow.ly/mhc3E"",""indices"":[60,82]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 08:08:10 +0000 2013"",""id"":348351692514930688,""id_str"":""348351692514930688"",""text"":""There was only one Freddie http://t.co/kuTVxAVnqn"",""source"":""\u003ca href=""http://www.tweetdeck.com"" rel=""nofollow""\u003eTweetDeck\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":15797140,""id_str"":""15797140"",""name"":""Hadi Hariri"",""screen_name"":""hhariri"",""location"":"""",""description"":""Never believed in Elevator pitches. \r\nI work at JetBrains."",""url"":""http://t.co/AwmH63cIiy"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/AwmH63cIiy"",""expanded_url"":""http://hadihariri.com"",""display_url"":""hadihariri.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5795,""friends_count"":450,""listed_count"":306,""created_at"":""Sun Aug 10 11:15:55 +0000 2008"",""favourites_count"":6,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":false,""verified"":false,""statuses_count"":54054,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""131516"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme14/bg.gif"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme14/bg.gif"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3594928413/66f03a09e6224219792c04f481177593_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3594928413/66f03a09e6224219792c04f481177593_normal.png"",""profile_link_color"":""009999"",""profile_sidebar_border_color"":""EEEEEE"",""profile_sidebar_fill_color"":""EFEFEF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":2,""favorite_count"":2,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/kuTVxAVnqn"",""expanded_url"":""http://bit.ly/dYxFiW"",""display_url"":""bit.ly/dYxFiW"",""indices"":[27,49]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 08:00:01 +0000 2013"",""id"":348349643719069696,""id_str"":""348349643719069696"",""text"":""25 ba\u00f1adores que hicieron historia http://t.co/dJvMnPBo20 via @elle_es"",""source"":""\u003ca href=""http://www.tweetdeck.com"" rel=""nofollow""\u003eTweetDeck\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":40211295,""id_str"":""40211295"",""name"":""Amazon BuyVIP"",""screen_name"":""buyvip"",""location"":""Espa\u00f1a"",""description"":"""",""url"":""http://t.co/lP9DQUKPsA"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/lP9DQUKPsA"",""expanded_url"":""http://www.buyvip.es"",""display_url"":""buyvip.es"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":18361,""friends_count"":252,""listed_count"":248,""created_at"":""Fri May 15 09:56:45 +0000 2009"",""favourites_count"":5,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":false,""verified"":false,""statuses_count"":3112,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""FF9900"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/652550937/66tz8n2u4t7l5bgkthw2.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/652550937/66tz8n2u4t7l5bgkthw2.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2788584443/74d9a2c8954d8c10ebe7e3217ce6b80d_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2788584443/74d9a2c8954d8c10ebe7e3217ce6b80d_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/40211295/1348503179"",""profile_link_color"":""CC7A00"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""F3F3F3"",""profile_text_color"":""333333"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/dJvMnPBo20"",""expanded_url"":""http://www.elle.es/lo-mas-elle/ocio/los-bikinis-que-han-hecho-historia-en-el-cine"",""display_url"":""elle.es/lo-mas-elle/oc\u2026"",""indices"":[35,57]\}\],""user_mentions"":[{""screen_name"":""elle_es"",""name"":""ELLE Espa\u00f1a"",""id"":19963431,""id_str"":""19963431"",""indices"":[62,70]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""es""\},\{""created_at"":""Sat Jun 22 06:26:17 +0000 2013"",""id"":348326055356022784,""id_str"":""348326055356022784"",""text"":""There are some pending issues on EasyHttp. And I'm really keen on accepting pull requests. Ping me if you're willing to help."",""source"":""\u003ca href=""http://www.tweetdeck.com"" rel=""nofollow""\u003eTweetDeck\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":15797140,""id_str"":""15797140"",""name"":""Hadi Hariri"",""screen_name"":""hhariri"",""location"":"""",""description"":""Never believed in Elevator pitches. \r\nI work at JetBrains."",""url"":""http://t.co/AwmH63cIiy"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/AwmH63cIiy"",""expanded_url"":""http://hadihariri.com"",""display_url"":""hadihariri.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5795,""friends_count"":450,""listed_count"":306,""created_at"":""Sun Aug 10 11:15:55 +0000 2008"",""favourites_count"":6,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":false,""verified"":false,""statuses_count"":54054,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""131516"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme14/bg.gif"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme14/bg.gif"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3594928413/66f03a09e6224219792c04f481177593_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3594928413/66f03a09e6224219792c04f481177593_normal.png"",""profile_link_color"":""009999"",""profile_sidebar_border_color"":""EEEEEE"",""profile_sidebar_fill_color"":""EFEFEF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":1,""favorite_count"":1,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 06:19:05 +0000 2013"",""id"":348324240405172224,""id_str"":""348324240405172224"",""text"":""RT @feedly: @shanselman we are taking note. Sorry. The extension is going to evolve to become an optional companion to save feeds and pages."",""source"":""\u003ca href=""http://tweetlogix.com"" rel=""nofollow""\u003eTweetlogix\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":5676102,""id_str"":""5676102"",""name"":""Scott Hanselman"",""screen_name"":""shanselman"",""location"":""Portland, Oregon"",""description"":""Tech, Diabetes, Parenting, Race, Linguistics, Web, Fashion, Podcasting, OSS, Code, Ratchet, Black Hair, Phony. I work for MSFT, but these are my opinions."",""url"":""http://t.co/fTFu7cE8Ox"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/fTFu7cE8Ox"",""expanded_url"":""http://hanselman.com"",""display_url"":""hanselman.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":73284,""friends_count"":3175,""listed_count"":5315,""created_at"":""Tue May 01 05:55:26 +0000 2007"",""favourites_count"":5052,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":86852,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""D1CDC1"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/5676102/1348032691"",""profile_link_color"":""72412C"",""profile_sidebar_border_color"":""B8AA9C"",""profile_sidebar_fill_color"":""B8AA9C"",""profile_text_color"":""696969"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweeted_status"":{""created_at"":""Sat Jun 22 05:59:03 +0000 2013"",""id"":348319201695506432,""id_str"":""348319201695506432"",""text"":""@shanselman we are taking note. Sorry. The extension is going to evolve to become an optional companion to save feeds and pages."",""source"":""\u003ca href=""http://tapbots.com/tweetbot"" rel=""nofollow""\u003eTweetbot for iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":348316999740436480,""in_reply_to_status_id_str"":""348316999740436480"",""in_reply_to_user_id"":5676102,""in_reply_to_user_id_str"":""5676102"",""in_reply_to_screen_name"":""shanselman"",""user"":{""id"":14485018,""id_str"":""14485018"",""name"":""feedly"",""screen_name"":""feedly"",""location"":""San Francisco, CA"",""description"":""A Better Reader."",""url"":""http://t.co/Dy7kQzdNe5"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/Dy7kQzdNe5"",""expanded_url"":""http://feedly.com/k/QdLXQ6"",""display_url"":""feedly.com/k/QdLXQ6"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":34814,""friends_count"":2065,""listed_count"":2573,""created_at"":""Wed Apr 23 04:00:25 +0000 2008"",""favourites_count"":8789,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":false,""statuses_count"":13349,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0C3C0"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/627848665/han2yypye4i8913kbqp0.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/627848665/han2yypye4i8913kbqp0.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620701928/2lvajmuqei8a0xmf9n1u_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620701928/2lvajmuqei8a0xmf9n1u_normal.png"",""profile_link_color"":""82BD1A"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":null,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":2,""favorite_count"":4,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""shanselman"",""name"":""Scott Hanselman"",""id"":5676102,""id_str"":""5676102"",""indices"":[0,11]\}\]\},""favorited"":false,""retweeted"":false,""lang"":""en""\},""retweet_count"":2,""favorite_count"":0,""entities"":\{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""feedly"",""name"":""feedly"",""id"":14485018,""id_str"":""14485018"",""indices"":[3,10]\},\{""screen_name"":""shanselman"",""name"":""Scott Hanselman"",""id"":5676102,""id_str"":""5676102"",""indices"":[12,23]\}\]\},""favorited"":false,""retweeted"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 06:02:17 +0000 2013"",""id"":348320012181856256,""id_str"":""348320012181856256"",""text"":""Nuestra infograf\u00eda las redes sociales representados por Game of Thrones: http://t.co/cBlO64g0ox"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":20,""favorite_count"":10,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/cBlO64g0ox"",""expanded_url"":""http://ow.ly/meXLT"",""display_url"":""ow.ly/meXLT"",""indices"":[73,95]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""es""},{""created_at"":""Sat Jun 22 05:49:42 +0000 2013"",""id"":348316847063580672,""id_str"":""348316847063580672"",""text"":""RT @feedly: @shanselman sorry. Note: you can access Feedly at http://t.co/8Txv0L90rW without the app or extension. Pure web now."",""source"":""\u003ca href=""http://www.twitter.com"" rel=""nofollow""\u003eTwitter for Windows Phone\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":5676102,""id_str"":""5676102"",""name"":""Scott Hanselman"",""screen_name"":""shanselman"",""location"":""Portland, Oregon"",""description"":""Tech, Diabetes, Parenting, Race, Linguistics, Web, Fashion, Podcasting, OSS, Code, Ratchet, Black Hair, Phony. I work for MSFT, but these are my opinions."",""url"":""http://t.co/fTFu7cE8Ox"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/fTFu7cE8Ox"",""expanded_url"":""http://hanselman.com"",""display_url"":""hanselman.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":73284,""friends_count"":3175,""listed_count"":5315,""created_at"":""Tue May 01 05:55:26 +0000 2007"",""favourites_count"":5052,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":86852,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""D1CDC1"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/5676102/1348032691"",""profile_link_color"":""72412C"",""profile_sidebar_border_color"":""B8AA9C"",""profile_sidebar_fill_color"":""B8AA9C"",""profile_text_color"":""696969"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweeted_status"":{""created_at"":""Sat Jun 22 05:48:05 +0000 2013"",""id"":348316439524032513,""id_str"":""348316439524032513"",""text"":""@shanselman sorry. Note: you can access Feedly at http://t.co/8Txv0L90rW without the app or extension. Pure web now."",""source"":""\u003ca href=""http://tapbots.com/tweetbot"" rel=""nofollow""\u003eTweetbot for iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":348308743487885312,""in_reply_to_status_id_str"":""348308743487885312"",""in_reply_to_user_id"":5676102,""in_reply_to_user_id_str"":""5676102"",""in_reply_to_screen_name"":""shanselman"",""user"":{""id"":14485018,""id_str"":""14485018"",""name"":""feedly"",""screen_name"":""feedly"",""location"":""San Francisco, CA"",""description"":""A Better Reader."",""url"":""http://t.co/Dy7kQzdNe5"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/Dy7kQzdNe5"",""expanded_url"":""http://feedly.com/k/QdLXQ6"",""display_url"":""feedly.com/k/QdLXQ6"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":34814,""friends_count"":2065,""listed_count"":2573,""created_at"":""Wed Apr 23 04:00:25 +0000 2008"",""favourites_count"":8789,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":false,""statuses_count"":13349,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0C3C0"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/627848665/han2yypye4i8913kbqp0.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/627848665/han2yypye4i8913kbqp0.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620701928/2lvajmuqei8a0xmf9n1u_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620701928/2lvajmuqei8a0xmf9n1u_normal.png"",""profile_link_color"":""82BD1A"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":null,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":13,""favorite_count"":9,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/8Txv0L90rW"",""expanded_url"":""http://cloud.feedly.com"",""display_url"":""cloud.feedly.com"",""indices"":[50,72]\}\],""user_mentions"":[{""screen_name"":""shanselman"",""name"":""Scott Hanselman"",""id"":5676102,""id_str"":""5676102"",""indices"":[0,11]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},""retweet_count"":13,""favorite_count"":0,""entities"":\{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/8Txv0L90rW"",""expanded_url"":""http://cloud.feedly.com"",""display_url"":""cloud.feedly.com"",""indices"":[62,84]\}\],""user_mentions"":[{""screen_name"":""feedly"",""name"":""feedly"",""id"":14485018,""id_str"":""14485018"",""indices"":[3,10]\},\{""screen_name"":""shanselman"",""name"":""Scott Hanselman"",""id"":5676102,""id_str"":""5676102"",""indices"":[12,23]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 05:17:30 +0000 2013"",""id"":348308743487885312,""id_str"":""348308743487885312"",""text"":""Hey @feedly. You're the only Chrome Extension *constantly* reminding me you exist. Now popping a pinned tab? Nah. Uninstalled."",""source"":""web"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":5676102,""id_str"":""5676102"",""name"":""Scott Hanselman"",""screen_name"":""shanselman"",""location"":""Portland, Oregon"",""description"":""Tech, Diabetes, Parenting, Race, Linguistics, Web, Fashion, Podcasting, OSS, Code, Ratchet, Black Hair, Phony. I work for MSFT, but these are my opinions."",""url"":""http://t.co/fTFu7cE8Ox"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/fTFu7cE8Ox"",""expanded_url"":""http://hanselman.com"",""display_url"":""hanselman.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":73284,""friends_count"":3175,""listed_count"":5315,""created_at"":""Tue May 01 05:55:26 +0000 2007"",""favourites_count"":5052,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":86852,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""D1CDC1"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/5676102/1348032691"",""profile_link_color"":""72412C"",""profile_sidebar_border_color"":""B8AA9C"",""profile_sidebar_fill_color"":""B8AA9C"",""profile_text_color"":""696969"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":13,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""feedly"",""name"":""feedly"",""id"":14485018,""id_str"":""14485018"",""indices"":[4,11]\}\]\},""favorited"":false,""retweeted"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 04:01:36 +0000 2013"",""id"":348289644141625346,""id_str"":""348289644141625346"",""text"":""What are you using Syncfusion's products for? Tell us and you could be featured in our monthly newsletter! http://t.co/g9a6gaI2Vm"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":41152441,""id_str"":""41152441"",""name"":""Syncfusion, Inc."",""screen_name"":""Syncfusion"",""location"":""Morrisville, North Carolina"",""description"":""Provider of UI, reporting, and business intelligence components for the Microsoft .NET platforms."",""url"":""http://t.co/kNbsUGj2WO"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/kNbsUGj2WO"",""expanded_url"":""http://www.syncfusion.com"",""display_url"":""syncfusion.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5955,""friends_count"":3167,""listed_count"":116,""created_at"":""Tue May 19 16:40:23 +0000 2009"",""favourites_count"":444,""utc_offset"":-18000,""time_zone"":""Eastern Time (US & Canada)"",""geo_enabled"":false,""verified"":false,""statuses_count"":5436,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0DEED"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme1/bg.png"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme1/bg.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/246355414/Syncfusion-Logo_JPEG_normal_normal.jpg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/246355414/Syncfusion-Logo_JPEG_normal_normal.jpg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/41152441/1369929003"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":true,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/g9a6gaI2Vm"",""expanded_url"":""http://ow.ly/meK86"",""display_url"":""ow.ly/meK86"",""indices"":[107,129]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 03:38:01 +0000 2013"",""id"":348283708958191616,""id_str"":""348283708958191616"",""text"":""#WindowsAzure Partners: Register for partner-to-partner networking events. Learn more with the #WPC13 Cheat Sheet http://t.co/HJRixPrFXc"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":17000457,""id_str"":""17000457"",""name"":""WindowsAzure"",""screen_name"":""WindowsAzure"",""location"":""Redmond, WA"",""description"":""The official account for Windows Azure. Follow for news and updates from the team and community. We blog at: http://t.co/O0igzCNBki."",""url"":""https://t.co/kBW2qOPFJ5"",""entities"":{""url"":{""urls"":[{""url"":""https://t.co/kBW2qOPFJ5"",""expanded_url"":""https://www.windowsazure.com"",""display_url"":""windowsazure.com"",""indices"":[0,23]\}\]\},""description"":\{""urls"":[{""url"":""http://t.co/O0igzCNBki"",""expanded_url"":""http://www.WindowsAzureBlog.com"",""display_url"":""WindowsAzureBlog.com"",""indices"":[109,131]\}\]\}\},""protected"":false,""followers_count"":81372,""friends_count"":2925,""listed_count"":2324,""created_at"":""Mon Oct 27 15:34:46 +0000 2008"",""favourites_count"":649,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":11764,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""000000"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17000457/1370991002"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C9E7FF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null\},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":5,""favorite_count"":2,""entities"":\{""hashtags"":[{""text"":""WindowsAzure"",""indices"":[0,13]\},\{""text"":""WPC13"",""indices"":[95,101]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/HJRixPrFXc"",""expanded_url"":""http://msft.it/6019kFt1"",""display_url"":""msft.it/6019kFt1"",""indices"":[114,136]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 03:17:40 +0000 2013"",""id"":348278585523568641,""id_str"":""348278585523568641"",""text"":""The issues have now been resolved. Systems are returning to normal operation. Contact http://t.co/xGXdsYjdBC with any questions."",""source"":""\u003ca href=""https://www.pingdom.com/"" rel=""nofollow""\u003ePingdom Issues\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":15674759,""id_str"":""15674759"",""name"":""Pingdom"",""screen_name"":""pingdom"",""location"":""V\u00e4ster\u00e5s, Sweden"",""description"":""Pingdom is a website monitoring service. In short, we monitor sites and servers on the Internet, alerting the site owners if we detect a problem."",""url"":""http://t.co/9mHAqOdh"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/9mHAqOdh"",""expanded_url"":""http://www.pingdom.com"",""display_url"":""pingdom.com"",""indices"":[0,20]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":87940,""friends_count"":2753,""listed_count"":1137,""created_at"":""Thu Jul 31 14:00:29 +0000 2008"",""favourites_count"":67,""utc_offset"":3600,""time_zone"":""Stockholm"",""geo_enabled"":false,""verified"":true,""statuses_count"":6133,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""262626"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/15674759/1367325850"",""profile_link_color"":""60B8FE"",""profile_sidebar_border_color"":""000000"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/xGXdsYjdBC"",""expanded_url"":""http://support.pingdom.com"",""display_url"":""support.pingdom.com"",""indices"":[86,108]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 03:01:47 +0000 2013"",""id"":348274587815002113,""id_str"":""348274587815002113"",""text"":""@thehotnessgrrl @ellalaverne hey!"",""source"":""\u003ca href=""http://www.twitter.com"" rel=""nofollow""\u003eTwitter for Windows Phone\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":5676102,""id_str"":""5676102"",""name"":""Scott Hanselman"",""screen_name"":""shanselman"",""location"":""Portland, Oregon"",""description"":""Tech, Diabetes, Parenting, Race, Linguistics, Web, Fashion, Podcasting, OSS, Code, Ratchet, Black Hair, Phony. I work for MSFT, but these are my opinions."",""url"":""http://t.co/fTFu7cE8Ox"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/fTFu7cE8Ox"",""expanded_url"":""http://hanselman.com"",""display_url"":""hanselman.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":73284,""friends_count"":3175,""listed_count"":5315,""created_at"":""Tue May 01 05:55:26 +0000 2007"",""favourites_count"":5052,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":86852,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""D1CDC1"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/373869284/sepiabackground.jpg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/344513261570921573/8771360a7caf4bfb293497957559fc6d_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/5676102/1348032691"",""profile_link_color"":""72412C"",""profile_sidebar_border_color"":""B8AA9C"",""profile_sidebar_fill_color"":""B8AA9C"",""profile_text_color"":""696969"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""ellalaverne"",""name"":""Ella "",""id"":25840137,""id_str"":""25840137"",""indices"":[16,28]\}\]\},""favorited"":false,""retweeted"":false,""lang"":""und""\},\{""created_at"":""Sat Jun 22 02:31:35 +0000 2013"",""id"":348266988709220353,""id_str"":""348266988709220353"",""text"":""They are not allowed to use social at work, but they do it anyways: http://t.co/MEZcjEk6m5"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":15,""favorite_count"":9,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/MEZcjEk6m5"",""expanded_url"":""http://ow.ly/meXPa"",""display_url"":""ow.ly/meXPa"",""indices"":[68,90]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 01:45:36 +0000 2013"",""id"":348255418075721728,""id_str"":""348255418075721728"",""text"":""Crowd-source your @Windows PC for college - Windows already contributed the first 10%! Details: http://t.co/NZQkCECQaz"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":74286565,""id_str"":""74286565"",""name"":""Microsoft"",""screen_name"":""Microsoft"",""location"":""Redmond, WA"",""description"":""The official Twitter page for Microsoft consumer products and your source for major announcements and events."",""url"":""http://t.co/iIrGzB9fzr"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/iIrGzB9fzr"",""expanded_url"":""http://www.facebook.com/Microsoft"",""display_url"":""facebook.com/Microsoft"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":1435097,""friends_count"":1034,""listed_count"":8545,""created_at"":""Mon Sep 14 22:35:42 +0000 2009"",""favourites_count"":52,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":5285,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""00AEEF"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/74286565/1369170135"",""profile_link_color"":""1570A6"",""profile_sidebar_border_color"":""8FC642"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":21,""favorite_count"":13,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/NZQkCECQaz"",""expanded_url"":""http://msft.it/6014kFSE"",""display_url"":""msft.it/6014kFSE"",""indices"":[96,118]\}\],""user_mentions"":[{""screen_name"":""Windows"",""name"":""Windows"",""id"":15670515,""id_str"":""15670515"",""indices"":[18,26]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 01:06:49 +0000 2013"",""id"":348245658605588480,""id_str"":""348245658605588480"",""text"":""Send gift cards to friends with @BizSpark startup @Celebratoril social media app on #WindowsAzure http://t.co/MhZBJqMy5Q #AzureApps"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17000457,""id_str"":""17000457"",""name"":""WindowsAzure"",""screen_name"":""WindowsAzure"",""location"":""Redmond, WA"",""description"":""The official account for Windows Azure. Follow for news and updates from the team and community. We blog at: http://t.co/O0igzCNBki."",""url"":""https://t.co/kBW2qOPFJ5"",""entities"":\{""url"":\{""urls"":[{""url"":""https://t.co/kBW2qOPFJ5"",""expanded_url"":""https://www.windowsazure.com"",""display_url"":""windowsazure.com"",""indices"":[0,23]\}\]\},""description"":\{""urls"":[{""url"":""http://t.co/O0igzCNBki"",""expanded_url"":""http://www.WindowsAzureBlog.com"",""display_url"":""WindowsAzureBlog.com"",""indices"":[109,131]\}\]\}\},""protected"":false,""followers_count"":81372,""friends_count"":2925,""listed_count"":2324,""created_at"":""Mon Oct 27 15:34:46 +0000 2008"",""favourites_count"":649,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":11764,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""000000"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17000457/1370991002"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C9E7FF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null\},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":3,""favorite_count"":0,""entities"":\{""hashtags"":[{""text"":""WindowsAzure"",""indices"":[84,97]\},\{""text"":""AzureApps"",""indices"":[121,131]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/MhZBJqMy5Q"",""expanded_url"":""http://msft.it/6017kFqJ"",""display_url"":""msft.it/6017kFqJ"",""indices"":[98,120]\}\],""user_mentions"":[{""screen_name"":""bizspark"",""name"":""Microsoft BizSpark"",""id"":16887185,""id_str"":""16887185"",""indices"":[32,41]\},\{""screen_name"":""Celebratoril"",""name"":""Celebrator"",""id"":578221651,""id_str"":""578221651"",""indices"":[50,63]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 01:02:11 +0000 2013"",""id"":348244492761067520,""id_str"":""348244492761067520"",""text"":""We test &amp; ensure our components are enterprise ready by building proof-of-concept samples for line of business apps. http://t.co/JEJtRrE2uB"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":41152441,""id_str"":""41152441"",""name"":""Syncfusion, Inc."",""screen_name"":""Syncfusion"",""location"":""Morrisville, North Carolina"",""description"":""Provider of UI, reporting, and business intelligence components for the Microsoft .NET platforms."",""url"":""http://t.co/kNbsUGj2WO"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/kNbsUGj2WO"",""expanded_url"":""http://www.syncfusion.com"",""display_url"":""syncfusion.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5955,""friends_count"":3167,""listed_count"":116,""created_at"":""Tue May 19 16:40:23 +0000 2009"",""favourites_count"":444,""utc_offset"":-18000,""time_zone"":""Eastern Time (US & Canada)"",""geo_enabled"":false,""verified"":false,""statuses_count"":5436,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0DEED"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme1/bg.png"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme1/bg.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/246355414/Syncfusion-Logo_JPEG_normal_normal.jpg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/246355414/Syncfusion-Logo_JPEG_normal_normal.jpg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/41152441/1369929003"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":true,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/JEJtRrE2uB"",""expanded_url"":""http://ow.ly/meslK"",""display_url"":""ow.ly/meslK"",""indices"":[121,143]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 00:48:16 +0000 2013"",""id"":348240990877921282,""id_str"":""348240990877921282"",""text"":""We\\'re still working on fixing the issues just as fast as we can. Thank you for your patience and understanding."",""source"":""\u003ca href=""https://www.pingdom.com/"" rel=""nofollow""\u003ePingdom Issues\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":15674759,""id_str"":""15674759"",""name"":""Pingdom"",""screen_name"":""pingdom"",""location"":""V\u00e4ster\u00e5s, Sweden"",""description"":""Pingdom is a website monitoring service. In short, we monitor sites and servers on the Internet, alerting the site owners if we detect a problem."",""url"":""http://t.co/9mHAqOdh"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/9mHAqOdh"",""expanded_url"":""http://www.pingdom.com"",""display_url"":""pingdom.com"",""indices"":[0,20]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":87940,""friends_count"":2753,""listed_count"":1137,""created_at"":""Thu Jul 31 14:00:29 +0000 2008"",""favourites_count"":67,""utc_offset"":3600,""time_zone"":""Stockholm"",""geo_enabled"":false,""verified"":true,""statuses_count"":6133,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""262626"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/15674759/1367325850"",""profile_link_color"":""60B8FE"",""profile_sidebar_border_color"":""000000"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 00:41:39 +0000 2013"",""id"":348239324862640128,""id_str"":""348239324862640128"",""text"":""The #WindowsAzure Community News Roundup (Edition #69) is out! http://t.co/30KCpM5giE (cc: @markjbrown)"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":17000457,""id_str"":""17000457"",""name"":""WindowsAzure"",""screen_name"":""WindowsAzure"",""location"":""Redmond, WA"",""description"":""The official account for Windows Azure. Follow for news and updates from the team and community. We blog at: http://t.co/O0igzCNBki."",""url"":""https://t.co/kBW2qOPFJ5"",""entities"":{""url"":{""urls"":[{""url"":""https://t.co/kBW2qOPFJ5"",""expanded_url"":""https://www.windowsazure.com"",""display_url"":""windowsazure.com"",""indices"":[0,23]\}\]\},""description"":\{""urls"":[{""url"":""http://t.co/O0igzCNBki"",""expanded_url"":""http://www.WindowsAzureBlog.com"",""display_url"":""WindowsAzureBlog.com"",""indices"":[109,131]\}\]\}\},""protected"":false,""followers_count"":81372,""friends_count"":2925,""listed_count"":2324,""created_at"":""Mon Oct 27 15:34:46 +0000 2008"",""favourites_count"":649,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":11764,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""000000"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17000457/1370991002"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C9E7FF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null\},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":5,""favorite_count"":3,""entities"":\{""hashtags"":[{""text"":""WindowsAzure"",""indices"":[4,17]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/30KCpM5giE"",""expanded_url"":""http://msft.it/6013kFUb"",""display_url"":""msft.it/6013kFUb"",""indices"":[63,85]\}\],""user_mentions"":[{""screen_name"":""markjbrown"",""name"":""Mark Brown"",""id"":14369922,""id_str"":""14369922"",""indices"":[91,102]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Sat Jun 22 00:39:53 +0000 2013"",""id"":348238881411440640,""id_str"":""348238881411440640"",""text"":""#aRoses Agenda: Sardanes amb la cobla Osona http://t.co/l4VtnyodDc"",""source"":""\u003ca href=""http://www.facebook.com/twitter"" rel=""nofollow""\u003eFacebook\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":361874390,""id_str"":""361874390"",""name"":""RosesComer\u00e7"",""screen_name"":""RosesComerc"",""location"":""Roses"",""description"":"""",""url"":""http://t.co/wAolTFh2TF"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/wAolTFh2TF"",""expanded_url"":""http://www.acoroses.com"",""display_url"":""acoroses.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":271,""friends_count"":362,""listed_count"":3,""created_at"":""Thu Aug 25 13:42:54 +0000 2011"",""favourites_count"":15,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":333,""lang"":""ca"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0DEED"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme1/bg.png"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme1/bg.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2847853356/6cfe03b9800fbbd76a62bacea1f9140c_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2847853356/6cfe03b9800fbbd76a62bacea1f9140c_normal.png"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":true,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":1,""favorite_count"":0,""entities"":{""hashtags"":[{""text"":""aRoses"",""indices"":[0,7]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/l4VtnyodDc"",""expanded_url"":""http://ow.ly/2xMJay"",""display_url"":""ow.ly/2xMJay"",""indices"":[44,66]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""es""},{""created_at"":""Sat Jun 22 00:26:01 +0000 2013"",""id"":348235391670816768,""id_str"":""348235391670816768"",""text"":""New stuff coming!! http://t.co/jothuNjt2b - Using VS 2013 to Diagnose .NET Memory Issues in Production - TY VS ALM &amp; TFS Team Blog for this"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":16913772,""id_str"":""16913772"",""name"":""VisualStudio"",""screen_name"":""VisualStudio"",""location"":""Redmond, WA, USA"",""description"":""The official account for Microsoft Visual Studio. Follow us for the latest Visual Studio news and related information for developers."",""url"":""http://t.co/zWNxMQ7FhY"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/zWNxMQ7FhY"",""expanded_url"":""http://visualstudio.com"",""display_url"":""visualstudio.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":54258,""friends_count"":831,""listed_count"":1999,""created_at"":""Wed Oct 22 22:01:24 +0000 2008"",""favourites_count"":43,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":5309,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""68217A"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16913772/1353364214"",""profile_link_color"":""0844FA"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""1F222B"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":28,""favorite_count"":19,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/jothuNjt2b"",""expanded_url"":""http://bit.ly/11s4268"",""display_url"":""bit.ly/11s4268"",""indices"":[19,41]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Sat Jun 22 00:10:29 +0000 2013"",""id"":348231479094042624,""id_str"":""348231479094042624"",""text"":""Have you see our first #Instagram video? http://t.co/p2DuobckDN #doinitright"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":23,""favorite_count"":10,""entities"":{""hashtags"":[{""text"":""Instagram"",""indices"":[23,33]\},\{""text"":""doinitright"",""indices"":[64,76]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/p2DuobckDN"",""expanded_url"":""http://ow.ly/mhdoI"",""display_url"":""ow.ly/mhdoI"",""indices"":[41,63]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 23:56:20 +0000 2013"",""id"":348227917349789697,""id_str"":""348227917349789697"",""text"":""Bra\u00e7alets voltes #demiim @lapusagirona - http://t.co/z1qoa3Axq8 http://t.co/BQe72PIGGb"",""source"":""\u003ca href=""http://www.ventamatic.com"" rel=""nofollow""\u003eVentamatic\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":115514565,""id_str"":""115514565"",""name"":""Viure a Girona"",""screen_name"":""viureagirona"",""location"":""Girona"",""description"":""http://t.co/WVp8RXh4wv \u00e9s l'aparador virtual de les empreses i botigues de la ciutat de Girona."",""url"":""http://t.co/yTzHQiFsOB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/yTzHQiFsOB"",""expanded_url"":""http://www.viureagirona.cat"",""display_url"":""viureagirona.cat"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[{""url"":""http://t.co/WVp8RXh4wv"",""expanded_url"":""http://ViureaGirona.cat"",""display_url"":""ViureaGirona.cat"",""indices"":[0,22]\}\]\}\},""protected"":false,""followers_count"":1136,""friends_count"":292,""listed_count"":22,""created_at"":""Thu Feb 18 22:42:15 +0000 2010"",""favourites_count"":32,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":false,""verified"":false,""statuses_count"":837,""lang"":""ca"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""1A1B1F"",""profile_background_image_url"":""http://a0.twimg.com/images/themes/theme9/bg.gif"",""profile_background_image_url_https"":""https://si0.twimg.com/images/themes/theme9/bg.gif"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/1166115571/avatar_girona-1_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/1166115571/avatar_girona-1_normal.png"",""profile_link_color"":""2FC2EF"",""profile_sidebar_border_color"":""181A1E"",""profile_sidebar_fill_color"":""252429"",""profile_text_color"":""666666"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null\},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":1,""entities"":\{""hashtags"":[{""text"":""demiim"",""indices"":[17,24]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/z1qoa3Axq8"",""expanded_url"":""http://bit.ly/130AKyD"",""display_url"":""bit.ly/130AKyD"",""indices"":[41,63]\}\],""user_mentions"":[{""screen_name"":""lapusagirona"",""name"":""La Pu\u00e7a Girona"",""id"":525284953,""id_str"":""525284953"",""indices"":[25,38]\}\],""media"":[{""id"":348227917358178305,""id_str"":""348227917358178305"",""indices"":[64,86],""media_url"":""http://pbs.twimg.com/media/BNUnciMCIAECSA0.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNUnciMCIAECSA0.jpg"",""url"":""http://t.co/BQe72PIGGb"",""display_url"":""pic.twitter.com/BQe72PIGGb"",""expanded_url"":""http://twitter.com/viureagirona/status/348227917349789697/photo/1"",""type"":""photo"",""sizes"":\{""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""large"":\{""w"":1024,""h"":870,""resize"":""fit""\},""medium"":\{""w"":600,""h"":510,""resize"":""fit""\},""small"":\{""w"":340,""h"":289,""resize"":""fit""\}\}\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""pt""\},\{""created_at"":""Fri Jun 21 23:53:32 +0000 2013"",""id"":348227214535450624,""id_str"":""348227214535450624"",""text"":""Building Blocks of Great #Cloud Applications by @craigkitterman: http://t.co/5EO7QyWFUD"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17000457,""id_str"":""17000457"",""name"":""WindowsAzure"",""screen_name"":""WindowsAzure"",""location"":""Redmond, WA"",""description"":""The official account for Windows Azure. Follow for news and updates from the team and community. We blog at: http://t.co/O0igzCNBki."",""url"":""https://t.co/kBW2qOPFJ5"",""entities"":\{""url"":\{""urls"":[{""url"":""https://t.co/kBW2qOPFJ5"",""expanded_url"":""https://www.windowsazure.com"",""display_url"":""windowsazure.com"",""indices"":[0,23]\}\]\},""description"":\{""urls"":[{""url"":""http://t.co/O0igzCNBki"",""expanded_url"":""http://www.WindowsAzureBlog.com"",""display_url"":""WindowsAzureBlog.com"",""indices"":[109,131]\}\]\}\},""protected"":false,""followers_count"":81372,""friends_count"":2925,""listed_count"":2324,""created_at"":""Mon Oct 27 15:34:46 +0000 2008"",""favourites_count"":649,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":11764,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""000000"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/661895984/fhge3lvjx0ew8g6p6ouz.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2620080635/qo0dbb36lwvnz8mylvwh_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17000457/1370991002"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C9E7FF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null\},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":6,""favorite_count"":5,""entities"":\{""hashtags"":[{""text"":""Cloud"",""indices"":[25,31]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/5EO7QyWFUD"",""expanded_url"":""http://msft.it/6019kFWv"",""display_url"":""msft.it/6019kFWv"",""indices"":[65,87]\}\],""user_mentions"":[{""screen_name"":""craigkitterman"",""name"":""Craig Kitterman"",""id"":19086137,""id_str"":""19086137"",""indices"":[48,63]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 23:46:11 +0000 2013"",""id"":348225363387764736,""id_str"":""348225363387764736"",""text"":""We are experiencing some issues right now. We will update you just as soon as we can and apologize for the inconvenience."",""source"":""\u003ca href=""https://www.pingdom.com/"" rel=""nofollow""\u003ePingdom Issues\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":15674759,""id_str"":""15674759"",""name"":""Pingdom"",""screen_name"":""pingdom"",""location"":""V\u00e4ster\u00e5s, Sweden"",""description"":""Pingdom is a website monitoring service. In short, we monitor sites and servers on the Internet, alerting the site owners if we detect a problem."",""url"":""http://t.co/9mHAqOdh"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/9mHAqOdh"",""expanded_url"":""http://www.pingdom.com"",""display_url"":""pingdom.com"",""indices"":[0,20]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":87940,""friends_count"":2753,""listed_count"":1137,""created_at"":""Thu Jul 31 14:00:29 +0000 2008"",""favourites_count"":67,""utc_offset"":3600,""time_zone"":""Stockholm"",""geo_enabled"":false,""verified"":true,""statuses_count"":6133,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""262626"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/856298673/2f2f56fdbd5df17bf38e9ad42fff2654.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2669794458/74c50c2938accf866de4cf41fefd3b9c_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/15674759/1367325850"",""profile_link_color"":""60B8FE"",""profile_sidebar_border_color"":""000000"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":2,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 23:05:02 +0000 2013"",""id"":348215009551204352,""id_str"":""348215009551204352"",""text"":""hmmmm...kinda cool! (The Evolution of C#) - Thanks #MSMVP Ken Lin http://t.co/nu5KCQELdI"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":16913772,""id_str"":""16913772"",""name"":""VisualStudio"",""screen_name"":""VisualStudio"",""location"":""Redmond, WA, USA"",""description"":""The official account for Microsoft Visual Studio. Follow us for the latest Visual Studio news and related information for developers."",""url"":""http://t.co/zWNxMQ7FhY"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/zWNxMQ7FhY"",""expanded_url"":""http://visualstudio.com"",""display_url"":""visualstudio.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":54258,""friends_count"":831,""listed_count"":1999,""created_at"":""Wed Oct 22 22:01:24 +0000 2008"",""favourites_count"":43,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":5309,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""68217A"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16913772/1353364214"",""profile_link_color"":""0844FA"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""1F222B"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":39,""favorite_count"":23,""entities"":{""hashtags"":[{""text"":""MSMVP"",""indices"":[51,57]\}\],""symbols"":[],""urls"":[],""user_mentions"":[],""media"":[{""id"":348215009559592961,""id_str"":""348215009559592961"",""indices"":[66,88],""media_url"":""http://pbs.twimg.com/media/BNUbtM5CQAEMtwz.png"",""media_url_https"":""https://pbs.twimg.com/media/BNUbtM5CQAEMtwz.png"",""url"":""http://t.co/nu5KCQELdI"",""display_url"":""pic.twitter.com/nu5KCQELdI"",""expanded_url"":""http://twitter.com/VisualStudio/status/348215009551204352/photo/1"",""type"":""photo"",""sizes"":\{""large"":\{""w"":800,""h"":600,""resize"":""fit""\},""medium"":\{""w"":600,""h"":450,""resize"":""fit""\},""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""small"":\{""w"":340,""h"":255,""resize"":""fit""\}\}\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 23:03:15 +0000 2013"",""id"":348214560731316224,""id_str"":""348214560731316224"",""text"":""YOU can\u2019t always be tweeting. But you CAN always be tweeting: http://t.co/ATsYpbT4J6"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":22,""favorite_count"":16,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/ATsYpbT4J6"",""expanded_url"":""http://ow.ly/meXP2"",""display_url"":""ow.ly/meXP2"",""indices"":[62,84]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 22:29:24 +0000 2013"",""id"":348206043387527168,""id_str"":""348206043387527168"",""text"":""RT @IE: Summer is here! Take the web outside w/ #IE10 http://t.co/5YnoWvt36G"",""source"":""\u003ca href=""http://www.tweetdeck.com"" rel=""nofollow""\u003eTweetDeck\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":74286565,""id_str"":""74286565"",""name"":""Microsoft"",""screen_name"":""Microsoft"",""location"":""Redmond, WA"",""description"":""The official Twitter page for Microsoft consumer products and your source for major announcements and events."",""url"":""http://t.co/iIrGzB9fzr"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/iIrGzB9fzr"",""expanded_url"":""http://www.facebook.com/Microsoft"",""display_url"":""facebook.com/Microsoft"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":1435097,""friends_count"":1034,""listed_count"":8545,""created_at"":""Mon Sep 14 22:35:42 +0000 2009"",""favourites_count"":52,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":5285,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""00AEEF"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/74286565/1369170135"",""profile_link_color"":""1570A6"",""profile_sidebar_border_color"":""8FC642"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweeted_status"":{""created_at"":""Fri Jun 21 19:00:16 +0000 2013"",""id"":348153408454291457,""id_str"":""348153408454291457"",""text"":""Summer is here! Take the web outside w/ #IE10 http://t.co/5YnoWvt36G"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":19315174,""id_str"":""19315174"",""name"":""Internet Explorer"",""screen_name"":""IE"",""location"":""Redmond, WA"",""description"":""The official Twitter page for the Internet Explorer team.\r\n\r\nConversations, dev tools, and IE news for developers and web enthusiasts."",""url"":""http://t.co/b5ecoFGt03"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/b5ecoFGt03"",""expanded_url"":""http://www.BeautyoftheWeb.com"",""display_url"":""BeautyoftheWeb.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":305796,""friends_count"":106,""listed_count"":4909,""created_at"":""Wed Jan 21 23:42:14 +0000 2009"",""favourites_count"":31,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":6653,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""1F417B"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/880767891/25be3999e9380e713764cde6a3114c86.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/880767891/25be3999e9380e713764cde6a3114c86.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/3763523738/02a2d57e28ac0fca12da327383a37f96_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3763523738/02a2d57e28ac0fca12da327383a37f96_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/19315174/1369843052"",""profile_link_color"":""F5120E"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""96C6ED"",""profile_text_color"":""453C3C"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":null,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":59,""favorite_count"":34,""entities"":{""hashtags"":[{""text"":""IE10"",""indices"":[40,45]\}\],""symbols"":[],""urls"":[],""user_mentions"":[],""media"":[{""id"":348153408458485760,""id_str"":""348153408458485760"",""indices"":[46,68],""media_url"":""http://pbs.twimg.com/media/BNTjri6CcAAkYsa.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNTjri6CcAAkYsa.jpg"",""url"":""http://t.co/5YnoWvt36G"",""display_url"":""pic.twitter.com/5YnoWvt36G"",""expanded_url"":""http://twitter.com/IE/status/348153408454291457/photo/1"",""type"":""photo"",""sizes"":\{""large"":\{""w"":1024,""h"":683,""resize"":""fit""\},""small"":\{""w"":340,""h"":227,""resize"":""fit""\},""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""medium"":\{""w"":600,""h"":400,""resize"":""fit""\}\}\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},""retweet_count"":59,""favorite_count"":0,""entities"":\{""hashtags"":[{""text"":""IE10"",""indices"":[48,53]\}\],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""IE"",""name"":""Internet Explorer"",""id"":19315174,""id_str"":""19315174"",""indices"":[3,6]\}\],""media"":[{""id"":348153408458485760,""id_str"":""348153408458485760"",""indices"":[54,76],""media_url"":""http://pbs.twimg.com/media/BNTjri6CcAAkYsa.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNTjri6CcAAkYsa.jpg"",""url"":""http://t.co/5YnoWvt36G"",""display_url"":""pic.twitter.com/5YnoWvt36G"",""expanded_url"":""http://twitter.com/IE/status/348153408454291457/photo/1"",""type"":""photo"",""sizes"":\{""large"":\{""w"":1024,""h"":683,""resize"":""fit""\},""small"":\{""w"":340,""h"":227,""resize"":""fit""\},""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""medium"":\{""w"":600,""h"":400,""resize"":""fit""\}\},""source_status_id"":348153408454291457,""source_status_id_str"":""348153408454291457""\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 22:28:39 +0000 2013"",""id"":348205853922430978,""id_str"":""348205853922430978"",""text"":""Happy #firstdayofsummer! Get started with these weekend getaway apps for your @WindowsPhone: http://t.co/xpX5L4HPme"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":74286565,""id_str"":""74286565"",""name"":""Microsoft"",""screen_name"":""Microsoft"",""location"":""Redmond, WA"",""description"":""The official Twitter page for Microsoft consumer products and your source for major announcements and events."",""url"":""http://t.co/iIrGzB9fzr"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/iIrGzB9fzr"",""expanded_url"":""http://www.facebook.com/Microsoft"",""display_url"":""facebook.com/Microsoft"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":1435097,""friends_count"":1034,""listed_count"":8545,""created_at"":""Mon Sep 14 22:35:42 +0000 2009"",""favourites_count"":52,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":5285,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""00AEEF"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/431832285/Twitter_BG_1040x2000.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2535822544/secztwqh31xo8xydi4kq_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/74286565/1369170135"",""profile_link_color"":""1570A6"",""profile_sidebar_border_color"":""8FC642"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":27,""favorite_count"":8,""entities"":{""hashtags"":[{""text"":""firstdayofsummer"",""indices"":[6,23]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/xpX5L4HPme"",""expanded_url"":""http://msft.it/6010kFcQ"",""display_url"":""msft.it/6010kFcQ"",""indices"":[93,115]\}\],""user_mentions"":[{""screen_name"":""windowsphone"",""name"":""Windows Phone"",""id"":16425197,""id_str"":""16425197"",""indices"":[78,91]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 22:16:02 +0000 2013"",""id"":348202679786405888,""id_str"":""348202679786405888"",""text"":""Attending Build? Then make sure to get the @Ch9 Events app installed on your device! Download the app here: http://t.co/Pl1Yca8Jg0 #bldwin"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":16913772,""id_str"":""16913772"",""name"":""VisualStudio"",""screen_name"":""VisualStudio"",""location"":""Redmond, WA, USA"",""description"":""The official account for Microsoft Visual Studio. Follow us for the latest Visual Studio news and related information for developers."",""url"":""http://t.co/zWNxMQ7FhY"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/zWNxMQ7FhY"",""expanded_url"":""http://visualstudio.com"",""display_url"":""visualstudio.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":54258,""friends_count"":831,""listed_count"":1999,""created_at"":""Wed Oct 22 22:01:24 +0000 2008"",""favourites_count"":43,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":5309,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""68217A"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16913772/1353364214"",""profile_link_color"":""0844FA"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""1F222B"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":4,""favorite_count"":1,""entities"":{""hashtags"":[{""text"":""bldwin"",""indices"":[131,138]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/Pl1Yca8Jg0"",""expanded_url"":""http://bit.ly/1c43J2R"",""display_url"":""bit.ly/1c43J2R"",""indices"":[108,130]\}\],""user_mentions"":[{""screen_name"":""ch9"",""name"":""Microsoft Channel 9"",""id"":9460682,""id_str"":""9460682"",""indices"":[43,47]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 22:10:31 +0000 2013"",""id"":348201291148836864,""id_str"":""348201291148836864"",""text"":""How much has the world's population changed since 1950? Great visualization @TheEconomist: http://t.co/p47OU6ackD http://t.co/271LDpZwuk"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":50393960,""id_str"":""50393960"",""name"":""Bill Gates"",""screen_name"":""BillGates"",""location"":""Seattle, WA"",""description"":""Sharing things I'm learning through my foundation work and other interests..."",""url"":""http://t.co/W7T8IITkPe"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/W7T8IITkPe"",""expanded_url"":""http://www.thegatesnotes.com"",""display_url"":""thegatesnotes.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":11792876,""friends_count"":155,""listed_count"":101679,""created_at"":""Wed Jun 24 18:44:10 +0000 2009"",""favourites_count"":2,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":931,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0DEED"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/777981904/cb1c0df726ec35b8bd3f82d709527f31.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/777981904/cb1c0df726ec35b8bd3f82d709527f31.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/1884069342/BGtwitter_normal.JPG"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/1884069342/BGtwitter_normal.JPG"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/50393960/1359598573"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":933,""favorite_count"":395,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/p47OU6ackD"",""expanded_url"":""http://b-gat.es/19T0q1D"",""display_url"":""b-gat.es/19T0q1D"",""indices"":[91,113]\},\{""url"":""http://t.co/271LDpZwuk"",""expanded_url"":""http://ow.ly/i/2p2og"",""display_url"":""ow.ly/i/2p2og"",""indices"":[114,136]\}\],""user_mentions"":[{""screen_name"":""TheEconomist"",""name"":""The Economist"",""id"":5988062,""id_str"":""5988062"",""indices"":[76,89]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 22:03:04 +0000 2013"",""id"":348199415422541824,""id_str"":""348199415422541824"",""text"":""What do you think makes someone \u201csuccessful\u201d at social media?"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":31,""favorite_count"":12,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 22:03:02 +0000 2013"",""id"":348199407180713984,""id_str"":""348199407180713984"",""text"":"".@HuaweiDeviceUSA is giving away the new W1 #WindowsPhone + @JonasBrothers tix. Sign up for a chance to win http://t.co/ZtkwtNnbQF"",""source"":""\u003ca href=""http://www.windowsphone.com"" rel=""nofollow""\u003eWP White Label Tweet\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":16425197,""id_str"":""16425197"",""name"":""Windows Phone"",""screen_name"":""windowsphone"",""location"":""Redmond, WA"",""description"":""The Official Windows Phone Twitter Channel - keeping you updated with the latest Windows Phone news. For support follow @winphonesupport."",""url"":""http://t.co/OC0Xz55SD2"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/OC0Xz55SD2"",""expanded_url"":""http://www.windowsphone.com"",""display_url"":""windowsphone.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":437366,""friends_count"":692,""listed_count"":6922,""created_at"":""Tue Sep 23 20:43:37 +0000 2008"",""favourites_count"":421,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":10413,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9B4F96"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/344918034408462067/f6fa98aeef13ac217a6d87e9d7d87ca5.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/344918034408462067/f6fa98aeef13ac217a6d87e9d7d87ca5.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2780853212/3de41d0c7005a9b20c92d9942de78b16_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2780853212/3de41d0c7005a9b20c92d9942de78b16_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16425197/1371133449"",""profile_link_color"":""2AA6E4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""E0E0E0"",""profile_text_color"":""000000"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":18,""favorite_count"":7,""entities"":{""hashtags"":[{""text"":""WindowsPhone"",""indices"":[44,57]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/ZtkwtNnbQF"",""expanded_url"":""http://bit.ly/MusicMobile"",""display_url"":""bit.ly/MusicMobile"",""indices"":[108,130]\}\],""user_mentions"":[{""screen_name"":""HuaweiDeviceUSA"",""name"":""HuaweiDeviceUSA"",""id"":533600084,""id_str"":""533600084"",""indices"":[1,17]\},\{""screen_name"":""JonasBrothers"",""name"":""Jonas Brothers"",""id"":24285686,""id_str"":""24285686"",""indices"":[60,74]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 21:07:47 +0000 2013"",""id"":348185504153477121,""id_str"":""348185504153477121"",""text"":""RT @Pacific_Place: Love this photo of @ShowtimeTate's cutest fan via @ElisePollard #microsoftWA #nerdglasses http://t.co/zAp5cO7XJJ Cc @Mic\u2026"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":24741685,""id_str"":""24741685"",""name"":""Microsoft News"",""screen_name"":""MSFTnews"",""location"":""Redmond, WA"",""description"":""The official Twitter account for Microsoft Corporate Communications. For support, please contact @MicrosoftHelps"",""url"":""http://t.co/vzYljEYj9e"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/vzYljEYj9e"",""expanded_url"":""http://www.microsoft.com/news"",""display_url"":""microsoft.com/news"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":378380,""friends_count"":1902,""listed_count"":13190,""created_at"":""Mon Mar 16 18:29:00 +0000 2009"",""favourites_count"":4,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":7977,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""E0F4FC"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/645996269/ej2dufh132nffjui9t3n.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/645996269/ej2dufh132nffjui9t3n.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/1536324742/72x72_msftnews_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/1536324742/72x72_msftnews_normal.png"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""BDDCAD"",""profile_sidebar_fill_color"":""E0F4FC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweeted_status"":{""created_at"":""Fri Jun 21 20:52:15 +0000 2013"",""id"":348181593401200641,""id_str"":""348181593401200641"",""text"":""Love this photo of @ShowtimeTate's cutest fan via @ElisePollard #microsoftWA #nerdglasses http://t.co/zAp5cO7XJJ Cc @MicrosoftStore #Seattle"",""source"":""web"",""truncated"":false,""in_reply_to_status_id"":348180850900361216,""in_reply_to_status_id_str"":""348180850900361216"",""in_reply_to_user_id"":217318426,""in_reply_to_user_id_str"":""217318426"",""in_reply_to_screen_name"":""ElisePollard"",""user"":{""id"":135960816,""id_str"":""135960816"",""name"":""Pacific Place "",""screen_name"":""Pacific_Place"",""location"":""Seattle, WA"",""description"":""Pacific Place is downtown Seattle's premier shopping, dining and entertainment center. It's the place for fashion, food and film."",""url"":""http://t.co/xHilIRSeel"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/xHilIRSeel"",""expanded_url"":""http://blog.pacificplaceseattle.com/"",""display_url"":""blog.pacificplaceseattle.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5376,""friends_count"":4081,""listed_count"":289,""created_at"":""Thu Apr 22 17:10:13 +0000 2010"",""favourites_count"":159,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":false,""statuses_count"":8863,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""C0DEED"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/188279561/Atrium_med.jpg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/188279561/Atrium_med.jpg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3307639844/ce490ff8213c17af791bd1bbd46fd42f_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3307639844/ce490ff8213c17af791bd1bbd46fd42f_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/135960816/1357167956"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""C0DEED"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":null,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":4,""favorite_count"":1,""entities"":{""hashtags"":[{""text"":""microsoftWA"",""indices"":[64,76]\},\{""text"":""nerdglasses"",""indices"":[77,89]\},\{""text"":""Seattle"",""indices"":[132,140]\}\],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""ShowtimeTate"",""name"":""Golden Tate"",""id"":134985071,""id_str"":""134985071"",""indices"":[19,32]\},\{""screen_name"":""ElisePollard"",""name"":""Elise Pollard"",""id"":217318426,""id_str"":""217318426"",""indices"":[50,63]\},\{""screen_name"":""MicrosoftStore"",""name"":""Microsoft Store"",""id"":16409781,""id_str"":""16409781"",""indices"":[116,131]\}\],""media"":[{""id"":348180850904555522,""id_str"":""348180850904555522"",""indices"":[90,112],""media_url"":""http://pbs.twimg.com/media/BNT8o5_CYAIqIVp.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNT8o5_CYAIqIVp.jpg"",""url"":""http://t.co/zAp5cO7XJJ"",""display_url"":""pic.twitter.com/zAp5cO7XJJ"",""expanded_url"":""http://twitter.com/ElisePollard/status/348180850900361216/photo/1"",""type"":""photo"",""sizes"":\{""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""small"":\{""w"":340,""h"":453,""resize"":""fit""\},""medium"":\{""w"":480,""h"":640,""resize"":""fit""\},""large"":\{""w"":480,""h"":640,""resize"":""fit""\}\},""source_status_id"":348180850900361216,""source_status_id_str"":""348180850900361216""\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},""retweet_count"":4,""favorite_count"":0,""entities"":\{""hashtags"":[{""text"":""microsoftWA"",""indices"":[83,95]\},\{""text"":""nerdglasses"",""indices"":[96,108]\}\],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""Pacific_Place"",""name"":""Pacific Place "",""id"":135960816,""id_str"":""135960816"",""indices"":[3,17]\},\{""screen_name"":""ShowtimeTate"",""name"":""Golden Tate"",""id"":134985071,""id_str"":""134985071"",""indices"":[38,51]\},\{""screen_name"":""ElisePollard"",""name"":""Elise Pollard"",""id"":217318426,""id_str"":""217318426"",""indices"":[69,82]\},\{""screen_name"":""mic"",""name"":""mic"",""id"":3171001,""id_str"":""3171001"",""indices"":[135,139]\}\],""media"":[{""id"":348180850904555522,""id_str"":""348180850904555522"",""indices"":[109,131],""media_url"":""http://pbs.twimg.com/media/BNT8o5_CYAIqIVp.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNT8o5_CYAIqIVp.jpg"",""url"":""http://t.co/zAp5cO7XJJ"",""display_url"":""pic.twitter.com/zAp5cO7XJJ"",""expanded_url"":""http://twitter.com/ElisePollard/status/348180850900361216/photo/1"",""type"":""photo"",""sizes"":\{""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""small"":\{""w"":340,""h"":453,""resize"":""fit""\},""medium"":\{""w"":480,""h"":640,""resize"":""fit""\},""large"":\{""w"":480,""h"":640,""resize"":""fit""\}\},""source_status_id"":348180850900361216,""source_status_id_str"":""348180850900361216""\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 21:03:02 +0000 2013"",""id"":348184305664655361,""id_str"":""348184305664655361"",""text"":""How have your summer nights been #reinvented with the Nokia #Lumia928? Take a pic and make it famous http://t.co/aLcPCXykBH"",""source"":""\u003ca href=""http://www.windowsphone.com"" rel=""nofollow""\u003eWP White Label Tweet\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":16425197,""id_str"":""16425197"",""name"":""Windows Phone"",""screen_name"":""windowsphone"",""location"":""Redmond, WA"",""description"":""The Official Windows Phone Twitter Channel - keeping you updated with the latest Windows Phone news. For support follow @winphonesupport."",""url"":""http://t.co/OC0Xz55SD2"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/OC0Xz55SD2"",""expanded_url"":""http://www.windowsphone.com"",""display_url"":""windowsphone.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":437366,""friends_count"":692,""listed_count"":6922,""created_at"":""Tue Sep 23 20:43:37 +0000 2008"",""favourites_count"":421,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":10413,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9B4F96"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/344918034408462067/f6fa98aeef13ac217a6d87e9d7d87ca5.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/344918034408462067/f6fa98aeef13ac217a6d87e9d7d87ca5.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2780853212/3de41d0c7005a9b20c92d9942de78b16_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2780853212/3de41d0c7005a9b20c92d9942de78b16_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16425197/1371133449"",""profile_link_color"":""2AA6E4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""E0E0E0"",""profile_text_color"":""000000"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":15,""favorite_count"":8,""entities"":{""hashtags"":[{""text"":""reinvented"",""indices"":[33,44]\},\{""text"":""Lumia928"",""indices"":[60,69]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/aLcPCXykBH"",""expanded_url"":""http://newwp.it/108jFyP"",""display_url"":""newwp.it/108jFyP"",""indices"":[101,123]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 21:02:57 +0000 2013"",""id"":348184286836424704,""id_str"":""348184286836424704"",""text"":""Can you imagine being born into internet celebrity? This case study highlights a very popular youngster:  http://t.co/ncoZuo7A83"",""source"":""\u003ca href=""http://www.hootsuite.com"" rel=""nofollow""\u003eHootSuite\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":17093617,""id_str"":""17093617"",""name"":""HootSuite"",""screen_name"":""hootsuite"",""location"":""Vancouver, Canada"",""description"":""Updates about the social media management tool which helps teams to securely engage audiences & measure results. See also: @HootSuite_Help @HootWatch & more."",""url"":""http://t.co/jaMIQleseQ"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/jaMIQleseQ"",""expanded_url"":""http://www.hootsuite.com"",""display_url"":""hootsuite.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":5064967,""friends_count"":1271076,""listed_count"":33487,""created_at"":""Fri Oct 31 22:26:54 +0000 2008"",""favourites_count"":204,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":4588,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""9FEAFD"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/677549635/ac3377f66231c13e365f331fd8f08325.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3609491522/388703f14207ddbb9d4f7e56f6644835_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/17093617/1349481478"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EEEEEE"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":13,""favorite_count"":10,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/ncoZuo7A83"",""expanded_url"":""http://ow.ly/meXOW"",""display_url"":""ow.ly/meXOW"",""indices"":[106,128]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 20:51:01 +0000 2013"",""id"":348181283089817600,""id_str"":""348181283089817600"",""text"":""Simplify your life with #Office. Don\u2019t live without it:   http://t.co/3pk5Ajv6mb"",""source"":""\u003ca href=""http://www.microsoft.com"" rel=""nofollow""\u003e_Microsoft\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":22209176,""id_str"":""22209176"",""name"":""Office"",""screen_name"":""Office"",""location"":""Redmond, WA"",""description"":""Tweets to help you do more with @Microsoft Office at work, home and school."",""url"":""http://t.co/PEKRXn0m20"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/PEKRXn0m20"",""expanded_url"":""http://www.office.com"",""display_url"":""office.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":210317,""friends_count"":887,""listed_count"":4485,""created_at"":""Sat Feb 28 00:06:50 +0000 2009"",""favourites_count"":1,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":20448,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""EB3C00"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/691863947/304349beb58fa32deacd2f3403101b14.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/691863947/304349beb58fa32deacd2f3403101b14.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2944394491/c57acc37a1be44dc922d678dcfd6fe3f_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2944394491/c57acc37a1be44dc922d678dcfd6fe3f_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/22209176/1351094827"",""profile_link_color"":""EB3C00"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""E6E6E6"",""profile_text_color"":""666666"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":14,""favorite_count"":4,""entities"":{""hashtags"":[{""text"":""Office"",""indices"":[24,31]\}\],""symbols"":[],""urls"":[{""url"":""http://t.co/3pk5Ajv6mb"",""expanded_url"":""http://msft.it/6014kFb4"",""display_url"":""msft.it/6014kFb4"",""indices"":[58,80]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""},{""created_at"":""Fri Jun 21 20:39:07 +0000 2013"",""id"":348178290281963520,""id_str"":""348178290281963520"",""text"":""Foto de nikoskouen31 http://t.co/5UDPrhZeFq"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/5UDPrhZeFq"",""expanded_url"":""http://instagram.com/p/NCCv1ioBuy/"",""display_url"":""instagram.com/p/NCCv1ioBuy/"",""indices"":[21,43]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""de""},{""created_at"":""Fri Jun 21 20:38:48 +0000 2013"",""id"":348178208664977408,""id_str"":""348178208664977408"",""text"":""Foto de pousaa http://t.co/gjiOdT1FLS"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/gjiOdT1FLS"",""expanded_url"":""http://instagram.com/p/Lfnk6FB30M/"",""display_url"":""instagram.com/p/Lfnk6FB30M/"",""indices"":[15,37]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""pt""},{""created_at"":""Fri Jun 21 20:38:12 +0000 2013"",""id"":348178057674227714,""id_str"":""348178057674227714"",""text"":""http://t.co/gjiOdT1FLS"",""source"":""\u003ca href=""http://www.facebook.com/twitter"" rel=""nofollow""\u003eFacebook\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/gjiOdT1FLS"",""expanded_url"":""http://instagram.com/p/Lfnk6FB30M/"",""display_url"":""instagram.com/p/Lfnk6FB30M/"",""indices"":[0,22]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""und""},{""created_at"":""Fri Jun 21 20:37:35 +0000 2013"",""id"":348177901969104896,""id_str"":""348177901969104896"",""text"":""Foto de cristina_dekapuxiny http://t.co/w52Hww1Fi8"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/w52Hww1Fi8"",""expanded_url"":""http://instagram.com/p/Y5OYPdhCJ2/"",""display_url"":""instagram.com/p/Y5OYPdhCJ2/"",""indices"":[28,50]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""pt""},{""created_at"":""Fri Jun 21 20:32:22 +0000 2013"",""id"":348176590175363073,""id_str"":""348176590175363073"",""text"":""Foto de masramon http://t.co/VPrx7AKYSg"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576435268,""id_str"":""576435268"",""name"":""El Mirador Almadrava"",""screen_name"":""ElMiradorRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""Una terrassa m\u00e0gica per gaudir dies de platja i nits d'estiu, i tamb\u00e9 quan la natura \u00e9s protagonista. A l'Almadrava, la millor platja de Roses."",""url"":""http://t.co/onkmwnB7kB"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/onkmwnB7kB"",""expanded_url"":""http://www.elmiradordelalmadrava.com"",""display_url"":""elmiradordelalmadrava.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":475,""friends_count"":1004,""listed_count"":2,""created_at"":""Thu May 10 17:51:32 +0000 2012"",""favourites_count"":0,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":31,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""022330"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/841278661/892e50485a9340d6cd0184da9c7d8763.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207322311/elmirador-avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576435268/1365845147"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""C0DFEC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/VPrx7AKYSg"",""expanded_url"":""http://instagram.com/p/alhg2XQmdr/"",""display_url"":""instagram.com/p/alhg2XQmdr/"",""indices"":[17,39]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""es""},{""created_at"":""Fri Jun 21 20:30:11 +0000 2013"",""id"":348176040063021057,""id_str"":""348176040063021057"",""text"":""We're on the countdown to //build. So many great sessions will be streamed live on @Ch9 (http://t.co/hY4bVrDbDS)! 6/26 starting at 9am PDT."",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":16913772,""id_str"":""16913772"",""name"":""VisualStudio"",""screen_name"":""VisualStudio"",""location"":""Redmond, WA, USA"",""description"":""The official account for Microsoft Visual Studio. Follow us for the latest Visual Studio news and related information for developers."",""url"":""http://t.co/zWNxMQ7FhY"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/zWNxMQ7FhY"",""expanded_url"":""http://visualstudio.com"",""display_url"":""visualstudio.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":54258,""friends_count"":831,""listed_count"":1999,""created_at"":""Wed Oct 22 22:01:24 +0000 2008"",""favourites_count"":43,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":false,""statuses_count"":5309,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""68217A"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/172101044/VS_twitter_bg_v4.png"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/3477132493/7429b6e2a2456dc0ef20c533b1de4c32_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16913772/1353364214"",""profile_link_color"":""0844FA"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""FFFFFF"",""profile_text_color"":""1F222B"",""profile_use_background_image"":false,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":9,""favorite_count"":3,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/hY4bVrDbDS"",""expanded_url"":""http://bit.ly/12muscb"",""display_url"":""bit.ly/12muscb"",""indices"":[89,111]\}\],""user_mentions"":[{""screen_name"":""ch9"",""name"":""Microsoft Channel 9"",""id"":9460682,""id_str"":""9460682"",""indices"":[83,87]\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 20:20:30 +0000 2013"",""id"":348173601452724226,""id_str"":""348173601452724226"",""text"":""RT @MicrosoftStore: #Seahawks WR @ShowtimeTate is playing Kinect with fans at our @Pacific_Place specialty store! #microsoftWA http://t.co/\u2026"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":24741685,""id_str"":""24741685"",""name"":""Microsoft News"",""screen_name"":""MSFTnews"",""location"":""Redmond, WA"",""description"":""The official Twitter account for Microsoft Corporate Communications. For support, please contact @MicrosoftHelps"",""url"":""http://t.co/vzYljEYj9e"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/vzYljEYj9e"",""expanded_url"":""http://www.microsoft.com/news"",""display_url"":""microsoft.com/news"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":378380,""friends_count"":1902,""listed_count"":13190,""created_at"":""Mon Mar 16 18:29:00 +0000 2009"",""favourites_count"":4,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":false,""verified"":true,""statuses_count"":7977,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""E0F4FC"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/645996269/ej2dufh132nffjui9t3n.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/645996269/ej2dufh132nffjui9t3n.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/1536324742/72x72_msftnews_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/1536324742/72x72_msftnews_normal.png"",""profile_link_color"":""0084B4"",""profile_sidebar_border_color"":""BDDCAD"",""profile_sidebar_fill_color"":""E0F4FC"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweeted_status"":{""created_at"":""Fri Jun 21 20:12:35 +0000 2013"",""id"":348171610072678402,""id_str"":""348171610072678402"",""text"":""#Seahawks WR @ShowtimeTate is playing Kinect with fans at our @Pacific_Place specialty store! #microsoftWA http://t.co/fbR2o1LaFe"",""source"":""\u003ca href=""http://www.sprinklr.com"" rel=""nofollow""\u003eSprinklr\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":16409781,""id_str"":""16409781"",""name"":""Microsoft Store"",""screen_name"":""MicrosoftStore"",""location"":""Redmond, WA"",""description"":""The official Twitter page for Microsoft retail and online stores."",""url"":""http://t.co/7rs1a7m0Mt"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/7rs1a7m0Mt"",""expanded_url"":""http://www.microsoftstore.com/"",""display_url"":""microsoftstore.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":169370,""friends_count"":4322,""listed_count"":1742,""created_at"":""Mon Sep 22 20:52:48 +0000 2008"",""favourites_count"":50,""utc_offset"":-28800,""time_zone"":""Pacific Time (US & Canada)"",""geo_enabled"":true,""verified"":true,""statuses_count"":14575,""lang"":""en"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""FFFFFF"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/378800000001147631/2089fbf0867d3e0814d8f8bb2918416d.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/378800000001147631/2089fbf0867d3e0814d8f8bb2918416d.jpeg"",""profile_background_tile"":true,""profile_image_url"":""http://a0.twimg.com/profile_images/2559384886/3timoniofx6o7idqcurq_normal.jpeg"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2559384886/3timoniofx6o7idqcurq_normal.jpeg"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/16409781/1371494875"",""profile_link_color"":""2D71E1"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""EDEDEE"",""profile_text_color"":""555862"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":null,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":10,""favorite_count"":6,""entities"":{""hashtags"":[{""text"":""Seahawks"",""indices"":[0,9]\},\{""text"":""microsoftWA"",""indices"":[94,106]\}\],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""ShowtimeTate"",""name"":""Golden Tate"",""id"":134985071,""id_str"":""134985071"",""indices"":[13,26]\},\{""screen_name"":""Pacific_Place"",""name"":""Pacific Place "",""id"":135960816,""id_str"":""135960816"",""indices"":[62,76]\}\],""media"":[{""id"":348171610081067008,""id_str"":""348171610081067008"",""indices"":[107,129],""media_url"":""http://pbs.twimg.com/media/BNT0PBPCAAAIZpw.jpg"",""media_url_https"":""https://pbs.twimg.com/media/BNT0PBPCAAAIZpw.jpg"",""url"":""http://t.co/fbR2o1LaFe"",""display_url"":""pic.twitter.com/fbR2o1LaFe"",""expanded_url"":""http://twitter.com/MicrosoftStore/status/348171610072678402/photo/1"",""type"":""photo"",""sizes"":\{""medium"":\{""w"":600,""h"":399,""resize"":""fit""\},""large"":\{""w"":1024,""h"":681,""resize"":""fit""\},""thumb"":\{""w"":150,""h"":150,""resize"":""crop""\},""small"":\{""w"":340,""h"":226,""resize"":""fit""\}\}\}\]\},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""en""\},""retweet_count"":10,""favorite_count"":0,""entities"":\{""hashtags"":[{""text"":""Seahawks"",""indices"":[20,29]\},\{""text"":""microsoftWA"",""indices"":[114,126]\}\],""symbols"":[],""urls"":[],""user_mentions"":[{""screen_name"":""MicrosoftStore"",""name"":""Microsoft Store"",""id"":16409781,""id_str"":""16409781"",""indices"":[3,18]\},\{""screen_name"":""ShowtimeTate"",""name"":""Golden Tate"",""id"":134985071,""id_str"":""134985071"",""indices"":[33,46]\},\{""screen_name"":""Pacific_Place"",""name"":""Pacific Place "",""id"":135960816,""id_str"":""135960816"",""indices"":[82,96]\}\]\},""favorited"":false,""retweeted"":false,""lang"":""en""\},\{""created_at"":""Fri Jun 21 20:19:09 +0000 2013"",""id"":348173263173722112,""id_str"":""348173263173722112"",""text"":""Foto de naumova_lera http://t.co/BldlPlFSp9"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":\{""id"":576411545,""id_str"":""576411545"",""name"":""Bit\u00e0kora Restaurant"",""screen_name"":""BitakoraRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""El restaurant panor\u00e0mic del port esportiu de Roses."",""url"":""http://t.co/bmCvP3FRIG"",""entities"":\{""url"":\{""urls"":[{""url"":""http://t.co/bmCvP3FRIG"",""expanded_url"":""http://www.bitakora.com"",""display_url"":""bitakora.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":212,""friends_count"":444,""listed_count"":3,""created_at"":""Thu May 10 17:23:59 +0000 2012"",""favourites_count"":3,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":66,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""F5FAF6"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/886198192/71316586dc25f62d479da9dbd645393a.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/886198192/71316586dc25f62d479da9dbd645393a.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207269295/Bitakora-Avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207269295/Bitakora-Avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576411545/1365846014"",""profile_link_color"":""07BA22"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/BldlPlFSp9"",""expanded_url"":""http://instagram.com/p/MOi64kSOOn/"",""display_url"":""instagram.com/p/MOi64kSOOn/"",""indices"":[21,43]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""pt""},{""created_at"":""Fri Jun 21 20:18:27 +0000 2013"",""id"":348173085792411650,""id_str"":""348173085792411650"",""text"":""Foto de sandrasermar http://t.co/HiQzWSPyWv"",""source"":""\u003ca href=""http://itunes.apple.com/us/app/instagram/id389801252?mt=8&uo=4"" rel=""nofollow""\u003eInstagram on iOS\u003c/a\u003e"",""truncated"":false,""in_reply_to_status_id"":null,""in_reply_to_status_id_str"":null,""in_reply_to_user_id"":null,""in_reply_to_user_id_str"":null,""in_reply_to_screen_name"":null,""user"":{""id"":576411545,""id_str"":""576411545"",""name"":""Bit\u00e0kora Restaurant"",""screen_name"":""BitakoraRoses"",""location"":""Roses, Girona, Espa\u00f1a"",""description"":""El restaurant panor\u00e0mic del port esportiu de Roses."",""url"":""http://t.co/bmCvP3FRIG"",""entities"":{""url"":{""urls"":[{""url"":""http://t.co/bmCvP3FRIG"",""expanded_url"":""http://www.bitakora.com"",""display_url"":""bitakora.com"",""indices"":[0,22]\}\]\},""description"":\{""urls"":[]}},""protected"":false,""followers_count"":212,""friends_count"":444,""listed_count"":3,""created_at"":""Thu May 10 17:23:59 +0000 2012"",""favourites_count"":3,""utc_offset"":3600,""time_zone"":""Madrid"",""geo_enabled"":true,""verified"":false,""statuses_count"":66,""lang"":""es"",""contributors_enabled"":false,""is_translator"":false,""profile_background_color"":""F5FAF6"",""profile_background_image_url"":""http://a0.twimg.com/profile_background_images/886198192/71316586dc25f62d479da9dbd645393a.jpeg"",""profile_background_image_url_https"":""https://si0.twimg.com/profile_background_images/886198192/71316586dc25f62d479da9dbd645393a.jpeg"",""profile_background_tile"":false,""profile_image_url"":""http://a0.twimg.com/profile_images/2207269295/Bitakora-Avatar_normal.png"",""profile_image_url_https"":""https://si0.twimg.com/profile_images/2207269295/Bitakora-Avatar_normal.png"",""profile_banner_url"":""https://pbs.twimg.com/profile_banners/576411545/1365846014"",""profile_link_color"":""07BA22"",""profile_sidebar_border_color"":""FFFFFF"",""profile_sidebar_fill_color"":""DDEEF6"",""profile_text_color"":""333333"",""profile_use_background_image"":true,""default_profile"":false,""default_profile_image"":false,""following"":true,""follow_request_sent"":null,""notifications"":null},""geo"":null,""coordinates"":null,""place"":null,""contributors"":null,""retweet_count"":0,""favorite_count"":0,""entities"":{""hashtags"":[],""symbols"":[],""urls"":[{""url"":""http://t.co/HiQzWSPyWv"",""expanded_url"":""http://instagram.com/p/OwPo2FKjjz/"",""display_url"":""instagram.com/p/OwPo2FKjjz/"",""indices"":[21,43]\}\],""user_mentions"":[]},""favorited"":false,""retweeted"":false,""possibly_sensitive"":false,""lang"":""es""}]";
            var tweets = service.Deserialize<IEnumerable<TwitterStatus>>(content);
            Assert.IsNotNull(tweets);
        }
    }
}
