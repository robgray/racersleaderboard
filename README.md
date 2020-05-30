# Racers Leaderboard

## What does it do?

Racers Leaderboard started off as a tool to create leaderboards of where people in my sim racing club stood on the iRacing platform.  The idea was to generate tables of the various seasons as well as overall stats comparisons.  I chose to create these tables as images, so they could be easily embedded in webpages and our forum (phpBB).  We originally had a forum thread for each different series in iRacing, e.g. Skip Barber, Pro Mazda and so on, as well as a general thread, where we'd display overall tables.  

The platform also produces "signatures" that can be used on the iRacing forums.  These signatures include a quick summary of name, iRating, and Safety racing. Again, these are done as images.

## Versions

### Original

The original version (in the /original folder) was a single ASP.NET MVC app (in .NET 4.6.1) I banged together over a few weeks, to do this basic job.  It scraped and interacted with iRacing and downloaded CSV files (the same as can be downloaded from iRacing's member site yourself).  These CSV files are used as the data source.

The system is limited to only members of my old sim racing club, so it has iracing customer ids' in the configuration and uses these to filter data.  

### Version 1

Version 1 is my first step towards modernisation. It's an upgrade to .NET Core 3.1 and a little better design.  It provides a WebApi to get the charts and signatures and provides Swagger documentation.  It does not have a UI (swagger replaces that function was served by MVC views/pages in the original version)

## How do I get going with it?

Grab the source for the version you want and build it.  You'll need to add two environment variables "**iracing.password**" and "**iracing.username**".  Yeah, no way I was checking those into public source control :).  

You'll also need to adjust the custid's and they'll need to be people you are "studying" in iracing.  I hope to fix that soon.

