VenmoWrapper
============

This is a C# wrapper for the Venmo service. Originally written for Windows Phone 8, but currently released as a portable class library for WP8, Windows Store, .NET 4+, and Silverlight 5.

*_STILL UNDER HEAVY DEVELOPMENT, SHOULD NOT BE USED IN PRODUCTION CODE. PORTIONS OF THE PUBLIC FACE ARE SUBJECT TO ARCHITECTURAL CHANGE AT ANY TIME_*

This library is currently fully functional - there are still one or two places where it is coupled to the Windows Phone app I'm writing, but those should be coming out in a release or two.

The biggest change that is coming down the pipeline is that Venmo updated their API to V1 from beta. There are several changes, most of which will actually make developing with VenmoWrapper simpler (for example, they have standardized so that there's no different between a VenmoTransaction and a PaymentResult). The update to V1 will probably take the next few days.

Required Packages:

Newtonsoft.Json

Microsoft.bcl.async
