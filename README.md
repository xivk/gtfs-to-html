# gtfs-to-html

This is a (very) simple CLI tool that converts a GTFS feed into a set of static HTML pages visualizing the data inside. The stops are put on a leaflet map, the shapes (if any) also.

## Building the tool

1. Install [.NET core](https://www.microsoft.com/net/core) for your platform, OSX, Linux and Windows are supported.
1. Run the publish script, publish.sh or publish.bat on windows.
1. The tool is now available in the output folder, for ubuntu this is ```./src/gtfs2html/bin/Release/netcoreapp1.1/ubuntu.16.04-x64/publish```

## Running

You can run the tool simply by giving it an input folder containing the GTFS files and an output folder for the resulting HTML:

```./GTFS2HTML /path/to/gtfs /path/to/html```

