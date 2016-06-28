# Bloodhound

Analytic software used to track feature usage, exceptions, and all other interesting points.

### Known Issues

* DataService: You can only filter and group charts by the day, month, year. Currently do not support less than a day filtering.

## Tracker

The tracker represents a Bloodhound session. A session represents a group of events. There are three different types of events of 
Session, Event, and Exception. The event type Session represents the root event of the group of events. This is not required but
the Tracker will create a root session event for you. This Session event will contain the following provided values.

Session Values

```
Name                          |Value
.NET Framework Version        |4.0.30319.42000
Amount Of Memory              |8
Application Bitness           |32
Application Name              |Sample.Console
Application Version           |1.2.3.4
Bloodhound Version            |0.9.5778.29531
Machine ID                    |848f1c86a8b7691206ea6c4b544a700bb8c92e9c213013ffbff5c7b50626622b
Machine Name                  |BOBBYS-SURFACE
Machine User Name             |bobby
Number Of Processors          |4
Operating System Bitness      |64
Operating System Name         |Windows 10 Pro
Operating System Service Pack |
Operating System Version      |6.2.9200.0
Screen Resolution             |1920x1080
Storage Available Space       |99
Storage Total Space           |237
IP Address                    |::1
```

#### Creating Trackers

There are two ways to create a tracker. You can use the factory method "Start" or the constructor. I suggest just using
factory method unless you don't want to start the tracker immediately.

```C#
using (var tracker = Tracker.Start(client, provider))
{
}
```

```C#
using (var tracker = new Tracker(client, provider))
{
	tracker.Start();
}
```

> Note: Tracker is thread safe. It's expected to use a single tracker per process so feel free to share a single instance across all your threads. However if you want to use more than one tracker then have at it.

#### Tracking An Event

It's very simple to track an event. Just call the "Event" method with the name of the event.

```C#
var client = new WebDataChannel("http://localhost");
var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Tracker";
var provider = new RepositoryProvider(path, TimeSpan.FromDays(1), 10000);

using (var tracker = Tracker.Start(client, provider))
{
    tracker.Event("PrintReport");
}
```

Now let's say you want to track printing of a reports but you want to include the report name.

```C#
var client = new WebDataChannel("http://localhost");
var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Tracker";
var provider = new RepositoryProvider(path, TimeSpan.FromDays(1), 10000);

using (var tracker = Tracker.Start(client, provider))
{
    tracker.Event("PrintReport", new EventValue("ReportName", "DailySummary"));
}
```

#### Full Example of All The Things

Here is a simple sample of how to use the tracker.

```C#
var client = new WebDataChannel("http://localhost");
var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Bloodhound";
var provider = new RepositoryProvider(path, TimeSpan.FromDays(1), 10000);

using (var tracker = Tracker.Start(client, provider))
{
	tracker.AddEvent("Hello World!");

	var timedEvent = tracker.StartEvent("TimedEvent");
	Thread.Sleep(1000);
	timedEvent.Complete();

	try
	{
		throw new Exception("BOOM!");
	}
	catch (Exception ex)
	{
		tracker.AddException(ex);
	}
}
```

Results of the code sample.

Events
```
ID |CompletedOn           |CreatedOn             |ElapsedTicks |ElapsedTime      |Name         |ParentId  |SessionId                            |Type |UniqueId
1  |10/20/2015 11:09:10 AM|10/20/2015 11:09:08 AM|12178243     |00:00:01.2178243 |Session      |          |6c5f0cc8-2307-4c65-938d-24a406b857eb |1    |6c5f0cc8-2307-4c65-938d-24a406b857eb
2  |10/20/2015 11:09:09 AM|10/20/2015 11:09:09 AM|0            |00:00:00         |Hello World! |1         |6c5f0cc8-2307-4c65-938d-24a406b857eb |2    |e6ab5eb0-6eaf-469a-a4b9-949674a88ee7
3  |10/20/2015 11:09:10 AM|10/20/2015 11:09:10 AM|0            |00:00:00         |Exception    |1         |6c5f0cc8-2307-4c65-938d-24a406b857eb |3    |64ba579f-aa0a-423b-8368-ee1ed7dff573
4  |10/20/2015 11:09:10 AM|10/20/2015 11:09:09 AM|10023237     |00:00:01.0023237 |TimedEvent   |1         |6c5f0cc8-2307-4c65-938d-24a406b857eb |2    |cad62bab-713b-40ec-a489-1b33a01c884d
```

Event Values
```
ID |Event ID |Name                          |Value
1  |1        |.NET Framework Version        |4.0.30319.42000
2  |1        |Amount Of Memory              |8
3  |1        |Application Bitness           |32
4  |1        |Application Name              |Sample.Console
5  |1        |Application Version           |1.2.3.4
6  |1        |Bloodhound Version            |0.9.5778.29531
7  |1        |Machine ID                    |848f1c86a8b7691206ea6c4b544a700bb8c92e9c213013ffbff5c7b50626622b
8  |1        |Machine Name                  |BOBBYS-SURFACE
9  |1        |Machine User Name             |bobby
10 |1        |Number Of Processors          |4
11 |1        |Operating System Bitness      |64
12 |1        |Operating System Name         |Windows 10 Pro
13 |1        |Operating System Service Pack |
14 |1        |Operating System Version      |6.2.9200.0
15 |1        |Screen Resolution             |1920x1080
16 |1        |Storage Available Space       |99
17 |1        |Storage Total Space           |237
18 |1        |IP Address                    |::1
19 |3        |Message                       |BOOM!
20 |3        |Stack Trace                   |   at Sample.Console.Program.Main(String[] args) in 
                                             C:\Workspaces\GitHub\Bloodhound\Sample.Console\Program.cs:line 40
```

## Benchmarks

These are preliminary numbers. Will provide final numbers when we hit release (v1.0).

#### Setup

Host:

* Windows Server 2012 R2    
* Intel i7-4700HQ @ 2.4GHZ
* 32 GB DDR3 
* Hyper-V
* 512 GB x 2 Raid0 SSDs

Server (VM):

* Windows Server 2012 R2
* 4 processors
* 4 GB 
* IIS 

Client (VM)

* Windows Server 2012 R2
* 4 processors
* 4 GB 

Client (laptop)

* Windows 10
* 8 processors
* 32 GB

#### Benchmark

Included in the solution is the BloodhoundPerformanceTests project. This is what I refer to as the client. Each client
starts one writing threads per processor and one reading thread. The reading thread is just reading the database directly to show
entity counts. Each writing thread will start up a tracker then write a random amount from 10 to 1000 events. Each event will
delay, to simulate work, for a random value of 1 to 100 milliseconds. Once the writer has tracked the events the tracker is 
closed and the writer thread creates a new one.

##### Results

The client VM was able to run 4 client applications (16 write threads) and maintain ~13-15% CPU utilization while maintaining 
a constant ~760 MB memory consumption for the system.

The client laptop ran 1 client application (8 write threads) and maintain ~6-9% CPU utilization while maintaining between 
~45-50 MB memory consumption for the client process.

The server VM was able to maintain ~60-80% CPU utilization while maintaining a constant ~3.3 GB memory consumption. The 
averaged ~448-450 event writes per second. Here is the query used to calculate events per seconds.

* 457-468 per second
* ~27k per minute
* ~1.6m per hour

During the benchmark the server stayed around ~75% CPU and  ~80% memory utilization. The impact on the client system is very
minimal even in a benchmark scenario.

```
Events
.GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.Month, x.CreatedOn.Day, x.CreatedOn.Hour, x.CreatedOn.Minute, x.CreatedOn.Second })
.Select(x => new
{
	x.Key,
	Count = x.Count()
})
.OrderByDescending(x => x.Count)
.Take(5)
.Select(x => x.Count)
```

## OData / Rest

The sample Bloodhound.Website allows querying all the event data via OData. Here is an example of the 
[metadata](http://localhost/odata/$metadata). I'm not going to teach you OData but here are some examples. Check out 
[odata.org](http://www.odata.org/) if you want to learn more about OData.

Here are the end points that are available to you.

* odata/Events
* odata/Events(id)
* odata/Events(id)/Values
* odata/Events(id)/Values(id)

> Do NOT forget that OData endpoints are case sensitive this means "odata/events" != "odata/Events".

#### http://localhost/odata/Events

> Note: The server is limit to 1000 results. You'll need to use $orderby, $skip, $take to page results.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#Events","value":[
    {
      "CompletedOn":"2015-10-17T17:58:52.5074347","CreatedOn":"2015-10-17T17:58:51.3037736","ElapsedTicks":"12036611","ElapsedTime":"PT1.2036611S","Name":"Session","ParentId":null,"SessionId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Type":"Session","UniqueId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Id":1
    },{
      "CompletedOn":"2015-10-17T17:58:51.498029","CreatedOn":"2015-10-17T17:58:51.498029","ElapsedTicks":"0","ElapsedTime":"PT0S","Name":"Hello World!","ParentId":1,"SessionId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Type":"Event","UniqueId":"a8671691-245f-463d-a30c-66a47414d09e","Id":2
    },{
      "CompletedOn":"2015-10-17T17:58:52.5074347","CreatedOn":"2015-10-17T17:58:52.5074347","ElapsedTicks":"0","ElapsedTime":"PT0S","Name":"Exception","ParentId":1,"SessionId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Type":"Exception","UniqueId":"2e7e499f-ca08-4227-a78f-7750afb94f04","Id":3
    },{
      "CompletedOn":"2015-10-17T17:58:52.499935","CreatedOn":"2015-10-17T17:58:51.4995309","ElapsedTicks":"10004041","ElapsedTime":"PT1.0004041S","Name":"TimedEvent","ParentId":1,"SessionId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Type":"Event","UniqueId":"59f16a1a-f2b6-45fa-95a3-f1865c4da053","Id":4
    }
  ]
}
```

#### http://localhost/odata/Events(1)

This reads a single event with the ID of 1.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#Events/@Element",
  "CompletedOn":"2015-10-17T17:58:52.5074347","CreatedOn":"2015-10-17T17:58:51.3037736","ElapsedTicks":"12036611","ElapsedTime":"PT1.2036611S","Name":"Session","ParentId":null,"SessionId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Type":"Session","UniqueId":"eb576f2f-cc2c-4cfd-a6b5-c0e5f29df745","Id":1
}

```

#### http://localhost/odata/Events(1)?$select=Name,ElapsedTime

Example of reading a partial view single event with the ID of 1.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#Events/@Element&$select=Name,ElapsedTime",
  "Name":"Session","ElapsedTime":"PT1.2036611S"
}
```

#### http://localhost/odata/Events(1)?$expand=Values&$select=Name,Values

Example of reading a partial view single event with the ID of 1. This view also contains all the event values.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#Events/@Element&$select=Name,Values","Values":[
    {
      "EventId":1,"Name":".NET Framework Version","Value":"4.0.30319.42000","Id":1
    },{
      "EventId":1,"Name":"Amount Of Memory","Value":"32","Id":2
    },{
      "EventId":1,"Name":"Application Bitness","Value":"32","Id":3
    },{
      "EventId":1,"Name":"Application Name","Value":"Sample.Console","Id":4
    },{
      "EventId":1,"Name":"Application Version","Value":"1.2.3.4","Id":5
    },{
      "EventId":1,"Name":"Bloodhound Version","Value":"0.9.5771.19945","Id":6
    },{
      "EventId":1,"Name":"Machine Name","Value":"BOBBYS-RIG","Id":7
    },{
      "EventId":1,"Name":"Number Of Processors","Value":"8","Id":8
    },{
      "EventId":1,"Name":"Operating System Bitness","Value":"64","Id":9
    },{
      "EventId":1,"Name":"Operating System Name","Value":"Windows 10 Pro","Id":10
    },{
      "EventId":1,"Name":"Operating System Service Pack","Value":"","Id":11
    },{
      "EventId":1,"Name":"Operating System Version","Value":"6.2.9200.0","Id":12
    },{
      "EventId":1,"Name":"User Name","Value":"bobby","Id":13
    },{
      "EventId":1,"Name":"IP Address","Value":"::1","Id":14
    }
  ],"Name":"Session"
}
```

#### http://localhost/odata/Events(1)/Values

Example of reading all the values of the event with an ID of 1.

> Note: The server is limit to 1000 results. You'll need to use $orderby, $skip, $take to page results.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#EventValues","value":[
    {
      "EventId":1,"Name":"Amount Of Memory","Value":"32","Id":1
    },{
      "EventId":1,"Name":"Application Name","Value":"Sample.Console","Id":2
    },{
      "EventId":1,"Name":"Application Version","Value":"0.6.5768.36381","Id":3
    },{
      "EventId":1,"Name":"IP Address","Value":"127.0.0.1","Id":4
    },{
      "EventId":1,"Name":"Machine Name","Value":"BOBBYS-RIG","Id":5
    },{
      "EventId":1,"Name":"Framework Version","Value":"4.0.30319.42000","Id":6
    },{
      "EventId":1,"Name":"Number Of Processors","Value":"8","Id":7
    },{
      "EventId":1,"Name":"Operating System Name","Value":"Windows 10 Pro","Id":8
    },{
      "EventId":1,"Name":"Operating System Version","Value":"6.2.9200.0","Id":9
    },{
      "EventId":1,"Name":"User Name","Value":"bobby","Id":10
    },{
      "EventId":1,"Name":"Version","Value":"0.6.5768.36381","Id":11
    }
  ]
}
```

#### http://localhost/odata/Events(1)/Values(1)

Example of reading the single value with ID of 1 from the event with an ID of 1.

```
{
  "odata.metadata":"http://localhost/odata/$metadata#EventValues/@Element",
  "EventId":1,"Name":"Amount Of Memory","Value":"32","Id":1
}
```


## Developers Notes

To run the integration test, you need to ensure you have deployed website to a localhost instance. If you 
have installed IIS with default then you simply just have to publish the localhost profile.