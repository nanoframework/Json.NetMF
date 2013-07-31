# Json.NetMF

JSON Serializer and Deserializer library for the .NET Micro Framework

This library is a slightly modified version of Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357) which utilizes part of Huysentruit Wouter's JSON generation library (https://www.ghielectronics.com/community/codeshare/entry/282)

## Requirements
Microsoft .NET Micro Framework 4.2 or higher

## Example Usage

An instance of the JsonSerializer class can be instantiated using the constructor. This will allow the serialzier to remember your DateTime format preference.

```c#
JsonSerializer serializer = new JsonSerializer(DateTimeFormat.Default);
string json = serializer.Serialize(o);
```

Additonally, you can just call the static SerializeObject/DeserializeString methods and specifiy your DateTime format preference as an argument.

```c#
string json = JsonSerializer.SerializeObject(o, DateTimeFormat.Default);
```

###Serialization

Any object can be serialized as long as its not an interface or abstract. That means an object that is an Array, IList, IDictionary, or IDictionaryEntry will not be serialized. Use ArrayList, Hashtable, or DictionaryEntry instead. 

Only properties that have a public getter and setter will be serialized. The property must be an object that is not an interface, virtual, or abstract.

DateTime objects can be serialized, and their format in JSON will be ISO 8601 format by default. 

```c#
string json = JsonSerializer.SerializeObject(o, DateTimeFormat.Default);
```

###Deserialization

Deserializtion will parse your JSON string and return it contents in either ArrayList, Hashtable, double, string, null, bool. You will need to know what type you are expecting and cast the return object to that type.


```c#
// JSON array to ArrayList
ArrayList arrayList = JsonSerializer.DeserializeString("[\"hello\", \"world\", \"!!!\"]") as ArrayList;
// Output: "Hello World!!!"
Debug.Print(arrayList[0] + " " + arrayList[1] + arrayList[2]);


// JSON object to Hashtable
Hashtable hashTable = JsonSerializer.DeserializeString("{\"firstName\":\"John\",\"lastName\":\"Doe\"}") as Hashtable;
// Output: "John Doe"
Debug.Print(hashTable["firstName"] + " " + hashTable["lastName"]);
```

Extensions methods are provided to parse a DateTime string to a DateTime object
```c#
Hashtable hashTable = JsonSerializer.DeserializeString("{\"firstName\":\"John\",\"lastName\":\"Doe\",\"birthDay\":\"1985-04-27T00:00:00.000Z\"}") as Hashtable;
DateTime birthday = DateTimeExtensions.FromIso8601(hashTable["birthDay"] as string);
// Output: "04/27/1985 00:00:00"
Debug.Print(birthday.ToString());
```

## Nuget
Available through Nuget (http://www.nuget.org/packages/Json.NetMF/)

```
PM> Install-Package Json.NetMF
```
