VenmoWrapper
============

This is a C# library for Venmo (server-side flow). Originally written for Windows Phone 8, but currently released as a portable class library for WP8, Windows Store, .NET 4+, and Silverlight 5. It handles logging in once an access code has been acquired, transactions, getting the friends list, recent transactions, and all of the other actions exposed by Venmo's API.

This library is currently fully functional. There may be bugs, as testing has not been extensive, but all functions work in at least one production application (mine). Please contact me with any other bugs.

This project is hosted on NuGet [here](https://www.nuget.org/packages/VenmoWrapper/) and can be installed in Visual Studio by opening the Package Manager Console and typing `Install-Package VenmoWrapper`.

Required Packages:

Newtonsoft.Json

Microsoft.bcl.async
