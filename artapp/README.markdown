![screenshot](https://github.com/danielcrenna/artapp/raw/master/screenshot.jpg) 

artapp
======
A beautiful art showcase app for iPhone, built with [MonoTouch](http://xamarin.com/monotouch).

_Artists and Galleries_: If you really want an app that looks like this one but don't know how to program, and 
don't have any friends that program, [get in touch](mailto:daniel.crenna@gmail.com) and we can work something out
at a reasonable rate to get your app in the store.

Introduction
============
Not every great idea becomes a business. I had the idea for a mobile art showcase application which could be purchased
by subscription, similar to how [MobileRoadie](http://mobileroadie.com)'s business model works with recording artists,
authors, and other A-listers. But rather than a canned mobile CMS, I wanted artists to stand out from the crowd with
a unique, virtual gallery experience. Above all, it had to be beautiful, which is why I enlisted the great [Gilbert
Guttmann](http://gttmnn.com/), a fantastic artist and person, that I was lucky to meet and work with during my short
stint at [Wildbit](http://wildbit.com).

With that vision I went to work, choosing MonoTouch as my development platform mostly due to my lightning speed in
C# and the compressed time I would be able to work on this project (which was in twenty minute doses due to my other,
far more important side project of raising a child). Since MonoTouch is not free, that might be a sticking point
for this particular offering, but I can tell you that even if you're an Objective-C developer on the main, MonoTouch
is a fantastic framework and beyond a few small details, you can translate Objective-C to C# line by line, as it is
not a simulator or translation layer between you and Cocoa, it's pure interop against native iOS APIs.

Launching
=========
Teaming up with the insanely talented [Meaghan Ogilvie](http://meaghanogilvie.com) of Toronto, a photographer with 
a penchant for underwater photography, we launched Meaghan's [own application](http://itunes.apple.com/us/app/meaghan-ogilvie/id504171885?ls=1&mt=8) shortly after [The Artist Project](http://theartistproject.com/), an 
independent fine art exhibition in March 2012. The application was well received by many independent artists, and 
our value proposition seemed sound: developing an iPhone application is a costly venture for anyone, especially one 
of high quality, and providing this service at an ultra affordable price for artists meant they could stand out in 
a crowd, showcase their work, and ultimately get greater exposure, a better portfolio, and more art sales. 
Art shows are an expensive proposition, with no guarantee of success. We wanted a grass roots way to achieve the
same results through a digital medium.

Learning
========
After conducting customer interviews and talking to over a hundred artists about a service like this one, and other
more successful technologists that I respect, it became clear to me that solving the problem of artist exposure is 
not the same proposition as solving the problem of increasing demand for an artist's work, which is the the ultimate
goal. Artists want to sell more of their art, so they can make more art over the long run. Giving them another channel
in which to shout into the void is not the best answer I can come up with. I've got a slow hunch about what a better 
answer could be.

Aftermath
=========
Deciding to re-think the problem, I was left with shuttering this incarnation, but there were a lot of challenges
that were solved in this application I was proud of, from a visual perspective as well as technical. It would be a
shame to toss it in the can and move along, especially if there was still some value, for artists, or programmers, or
both, that could be salvaged from the remains, such as:

* It's an example of a real, professionally developed iOS application using MonoTouch, that made it to the App Store.
* It's a beautiful app that can be repurposed for other artists, and galleries, by their programmer friends, for free.
* It's designed to be very simple to switch out content, just add images, a new JSON manifest, and you're good to go.
* Some of the technical challenges (like transparent nav bars) are tricky to solve in MonoTouch; this is a great cheat sheet.

Future
======
You can use this freely, under the MIT License, to help out your favorite artist or gallery by making them a slick
iPhone application (making it work with an iPad should be simply a matter of adjusting cartesian coordinates, since
the assets are already high resolution, but I didn't have the time to get it into the codebase, sorry). You could
even decide I'm insane for giving this away and try to fill the void (if you do that, and you succeed, please buy 
all of Meaghan's collected works and install them in your swanky new startup office). You could use it to learn iOS 
if you know C# and aren't ready to switch to Objective-C whole hog. You could print out the source code and use the
other side as note paper.


Nitty Gritty
============
* This application is provided as-is. I'll happily take patches but I can't respond to issues. Whatever warts that
came with it, made it into the App Store, and there definitely are warts (some embarassing if you really want to
go hunt for them; code you write to ship is sometimes not the intellectual cornucopia you're capable of, nor should
it be). 
* I cut a vast majority of the features attempting to develop the minimum viable product, so any extensions are up to you. 
* You get to use Gilbert's layered PSD files as part of this open source license, but, it should go without saying, Meaghan's
  fine art is not part of that license, and her work remains fully her own. 
* Some of the assets, namely icons and two backgrounds, are purchased from [Glyphish](http://glyphish.com), so you need to own those _very reasonably
priced_ assets if you want to publish your own apps using this code base. Glyphish is a force of good in the universe. Support it!

Art needs technology, and vice versa. Here's some code to help artists. I hope you'll use it.

Daniel Crenna
