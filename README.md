[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_Json.NetMF&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_Json.NetMF) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_Json.NetMF&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_Json.NetMF) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/nanoframework/Json.NetMF/blob/master/LICENSE) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/master/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

# nanoFramework.Json (Json.NetMF)

Welcome to the JSON Serializer and Deserializer library for nanoFramework and .NET Micro Framework.

This library is now being maintained by the nanoFramework team after being transferred from [Matt Weimer](https://github.com/mweimer).
The library was originally forked and modified from Mike Jones's JSON Serialization and Deserialization library.

Its hopefully faster, more lightweight, and more robust.

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.Json | [![Build Status](https://dev.azure.com/nanoframework/json.NetMF/_apis/build/status/nanoframework.Json.NetMF?branchName=master)](https://dev.azure.com/nanoframework/json.NetMF/_build/latest?definitionId=55&branchName=master) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Json.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Json/)  |
| nanoFramework.Json (preview) | [![Build Status](https://dev.azure.com/nanoframework/json.NetMF/_apis/build/status/nanoframework.Json.NetMF?branchName=develop)](https://dev.azure.com/nanoframework/json.NetMF/_build/latest?definitionId=55&branchName=develop) | [![](https://badgen.net/badge/NuGet/preview/D7B023?icon=https://simpleicons.now.sh/azuredevops/fff)](https://dev.azure.com/nanoframework/feed/_packaging?_a=package&feed=sandbox&package=nanoFramework.Json&protocolType=NuGet&view=overview) |
| Json.NetMF | [![Build Status](https://dev.azure.com/nanoframework/json.NetMF/_apis/build/status/nanoframework.Json.NetMF?branchName=master)](https://dev.azure.com/nanoframework/json.NetMF/_build/latest?definitionId=55&branchName=master) | [![NuGet](https://img.shields.io/nuget/v/Json.NetMF.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/Json.NetMF/)  |
| Json.NetMF (preview) | [![Build Status](https://dev.azure.com/nanoframework/json.NetMF/_apis/build/status/nanoframework.Json.NetMF?branchName=develop)](https://dev.azure.com/nanoframework/json.NetMF/_build/latest?definitionId=55&branchName=develop) | [![](https://badgen.net/badge/NuGet/preview/D7B023?icon=https://simpleicons.now.sh/azuredevops/fff)](https://dev.azure.com/nanoframework/feed/_packaging?_a=package&feed=sandbox&package=json.NetMF&protocolType=NuGet&view=overview) |

## Requirements

* nanoFramework 1.0 or higher
* Microsoft .NET Micro Framework 4.2 or higher

## Example Usage

An instance of the JsonSerializer class can be instantiated using the constructor. This will allow the serializer to remember your DateTime format preference.

```c#
JsonSerializer serializer = new JsonSerializer(DateTimeFormat.Default);
string json = serializer.Serialize(o);
```

Additionally, you can just call the static SerializeObject/DeserializeString methods and specify your DateTime format preference as an argument.

```c#
string json = JsonSerializer.SerializeObject(o,DateTimeFormat.Default);
```

DateTime preference is optional, you can simply use:

```c#
string json = serializer.Serialize(o);
//or
string json = JsonSerializer.SerializeObject(o);
```

### Serialization

Pretty much any object can be serialized.

Any object that implements IEnumerable will get serialized into a JSON array. This include arrays, ArrayList, ICollection, IList, Queue and Stack.

```c#
ArrayList arrayList = new ArrayList() { 1, true, "hello world", null };

string json = JsonSerializer.SerializeObject(arrayList);

Debug.Print(json);
// Output: [1,true,"hello world",null]
```

Any object that implements IDictionary will get serialized into a JSON object. If an object implements both IDictionary and IEnumerable it gets serialized as a JSON object.

```c#
Hashtable hashtable = new Hashtable();
hashtable.Add("a bool", true);
hashtable.Add("a null", null);
hashtable.Add("a string", "hello world");
hashtable.Add("a number", 1);
hashtable.Add("an array", new object[] {1, 2, 3 });

string json = JsonSerializer.SerializeObject(hashtable);

Debug.Print(json);
// Output: {"a string":"hello world","a null":null,"a bool":true,"a number":1,"an array":[1,2,3]}
 ```

Other objects will be serialized using their public properties. Only properties that have a public getter will be serialized.

DateTime objects can be serialized, and their format in JSON will be ISO 8601 format by default.

```c#
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthday { get; set; }
}

...

Person person = new Person() { FirstName = "John", LastName = "Doe", Birthday = new DateTime(1988, 4, 23) };

string json = JsonSerializer.SerializeObject(person);

Debug.Print(json);
// Output: {"Birthday":"1988-04-23T00:00:00.000Z","LastName":"Doe","FirstName":"John"}
```

### Deserialization

Deserialization will parse your JSON string and return it contents in either ArrayList, Hashtable, double, long, string, null, bool. You will need to know what type you are expecting and cast the return object to that type or manually check the type and then cast.

```c#
// JSON array to ArrayList
string json = "[\"hello\", \"world\", \"!!!\"]";
ArrayList arrayList = JsonSerializer.DeserializeString(json) as ArrayList;
Debug.Print(arrayList[0] + " " + arrayList[1] + arrayList[2]);
// Output: "Hello World!!!"

// JSON object to Hashtable
string json = "{\"firstName\":\"John\",\"lastName\":\"Doe\"}";
Hashtable hashTable = JsonSerializer.DeserializeString(json) as Hashtable;

Debug.Print(hashTable["firstName"] + " " + hashTable["lastName"]);
// Output: "John Doe"

// Checking return types
string json = "[1, 2.345, -5, -232323.7878678]";
object deserializedObject = JsonSerializer.DeserializeString(json);

if (deserializedObject is ArrayList)
{
    ArrayList arrayList = deserializedObject as ArrayList;
    foreach (object o in arrayList)
    {
        if (o is long)
        {
            long l = (long) o;
            Debug.Print("long: " + l);
        }
        else if (o is double)
        {
            double d = (double)o;
            Debug.Print("double: " + d);
        }
    }
}
// Output (maybe some prescion errors in the to double.ToString:
// long: 1
// double: 2.3450000000000002
// long: -5
// double: -232323.78786779998
```

Extensions methods are provided to parse a DateTime string to a DateTime object

```c#
string json = "{\"firstName\":\"John\",\"lastName\":\"Doe\",\"birthDay\":\"1985-04-27T00:00:00.000Z\"}";

Hashtable hashTable = JsonSerializer.DeserializeString(json) as Hashtable;

DateTime birthday = DateTimeExtensions.FromIso8601(hashTable["birthDay"] as string);

Debug.Print(birthday.ToString());
// Output: "04/27/1985 00:00:00"
```

## NuGet packages

The nanoFramework version is available through [NuGet](http://www.nuget.org/packages/nanoFramework.Json/)

```powershell
PM> Install-Package nanoFramework.Json
```

The .NetMF version is available through [NuGet](http://www.nuget.org/packages/Json.NetMF/)

```powershell
PM> Install-Package Json.NetMF
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## License

The **nanoFramework.Json** is licensed under the [MIT](LICENSE) license.

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
