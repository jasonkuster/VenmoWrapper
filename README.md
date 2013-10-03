VenmoWrapper
============

This is a C# wrapper for the Venmo service. Originally written for Windows Phone 8, but currently released as a portable class library for WP8, Windows Store, .NET 4+, and Silverlight 5.

*_STILL UNDER HEAVY DEVELOPMENT, SHOULD NOT BE USED IN PRODUCTION CODE_*

After the latest commit, the VenmoGet and VenmoPost methods both likely work. I haven't yet tested the ancillary business like the helper functions or different classes, but at least we can get stuff from and send stuff to Venmo. I'll be migrating my app over to this library over the weekend - I'll update this readme then.

Required Packages:
Newtonsoft.Json
Microsoft.bcl.async
