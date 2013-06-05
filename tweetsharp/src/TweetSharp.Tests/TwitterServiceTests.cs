using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NUnit.Framework;

namespace TweetSharp.Tests.Service
{
    [TestFixture]
    public partial class TwitterServiceTests
    {
        private readonly string _hero;
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessTokenSecret;

        public TwitterServiceTests()
        {
            _hero = ConfigurationManager.AppSettings["Hero"];
            _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            _accessToken = ConfigurationManager.AppSettings["AccessToken"];
            _accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
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
            var result = service.ListDirectMessagesReceived(new ListDirectMessagesReceivedOptions(),
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
            var result = service.BeginListDirectMessagesReceived(new ListDirectMessagesReceivedOptions() { Count = 5 });
            var dms = service.EndListDirectMessagesReceived(result, TimeSpan.FromSeconds(5));
            
            Assert.IsNotNull(dms);
            Assert.AreEqual(5, dms.Count());

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
        [Ignore("Makes a live status update")]
        public void Can_tweet()
        {
            var service = GetAuthenticatedService();
            var status = _hero + DateTime.UtcNow.Ticks + " @danielcrenna";
            var tweet = service.SendTweet(new SendTweetOptions { Status = status });

            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        [Ignore("Makes a live status update")]
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
        [Ignore("Makes a live status update")]
        public void Can_tweet_with_special_characters()
        {
            var service = GetAuthenticatedService();

            var message = "!@#$%^&*();:-" + DateTime.UtcNow.Ticks;
            var tweet = service.SendTweet(new SendTweetOptions { Status = message });
            Assert.IsNotNull(tweet);
            Assert.AreNotEqual(0, tweet.Id);
        }

        [Test]
        [Ignore("Makes a live status update")]
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
        [Ignore("Makes a live status update")]
        public void Can_tweet_and_handle_dupes()
        {
            var service = GetAuthenticatedService();

            service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet"});
            var response = service.SendTweet(new SendTweetOptions { Status = "Can_tweet_and_handle_dupes:Tweet"});
            
            if(service.Response != null && service.Response.StatusCode != HttpStatusCode.OK)
            {
                var error = service.Deserialize<TwitterError>(response); // <-- RawSource should have been assigned here
                Assert.IsNotNull(error);
                Assert.IsNotNullOrEmpty(error.Message);
            }

            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore("Makes a live status update")]
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
        [Ignore("Makes a live direct message")]
        public void Can_send_direct_message()
        {
            var service = new TwitterService { IncludeEntities = true };
            service.AuthenticateWith(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            var response = service.SendDirectMessage(new SendDirectMessageOptions
            {
                ScreenName = _hero,
                Text = "http://tweetsharp.com @dimebrain #thisisatest " + DateTime.Now.Ticks
            });
            
            AssertResultWas(service, HttpStatusCode.OK);
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Id == 0);
        }

        [Test]
        [Ignore("Makes a live direct message")]
        public void Can_delete_direct_message()
        {
            var service = new TwitterService { IncludeEntities = true };
            service.AuthenticateWith(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
            var created = service.SendDirectMessage(new SendDirectMessageOptions
            {
                ScreenName = _hero,
                Text = "http://tweetsharp.com @dimebrain #thisisatest " + DateTime.Now.Ticks
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
            
            var tweets = service.ListDirectMessagesSent(new ListDirectMessagesSentOptions());
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
                if(tweet.Entities == null)
                {
                    continue;
                }

                var entities = tweet.Entities.Coalesce();
                if(entities.Count() < 2)
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

        private TwitterService GetAuthenticatedService()
        {
            var service = new TwitterService(_consumerKey, _consumerSecret);
            service.TraceEnabled = true;
            service.AuthenticateWith(_accessToken, _accessTokenSecret);
            return service;
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
    }
}
