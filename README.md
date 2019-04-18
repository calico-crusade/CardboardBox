# CardboardBox
A collection of random tidbits of code that warrant throwing under a common namespace because I find my self replicating this code acorss multiple projects.

## Libraries
* [CardboardBox.Setup](https://github.com/calico-crusade/CardboardBox/tree/master/CardboardBox.Setup) - IoC wrappers for different systems
* [CardboardBox.Redis](https://github.com/calico-crusade/CardboardBox/tree/master/CardboardBox.Redis) - Reids connection and object manipulation service
* CardboardBox.Gdi - Extension for the GDI+ framework 
* CardboardBox.Gdi.ColorThief - .net standard implementation of [ColorThief](https://github.com/KSemenenko/ColorThief)
* CardboardBox.WebApi - Helpful extensions for spinning up web api's quicker than ever
* CardboardBox.Extensions - Linq, collection and other extensions
* CardboardBox - Single library referencing the majority of the others under 1 project

## Use & Installation
Currently, I do not have a Nuget Package on [NuGet](https://nuget.org) for this library. I'm still deciding how many different packages I should do for the functionality. 

My current thoughts are:
* CardboardBox.Setup - For the CardboardBox.Setup library
* CardboardBox.Gdi - For both CardboardBox.Gdi and CardboardBox.Gdi.ColorThief
* CardboardBox.Redis - For the CardboardBox.Redis library (references CardboardBox.Setup)
* CardboardBox - linking all other packages together

I don't know if I'm ready to publish WebApi or Extensions yet, I might want to re-work them.