# Json.NetMF

JSON Serializer and Deserializer library for the .NET Micro Framework

This library is a forked and modified version of Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357) 
Its hopefuly faster, more lightweight, and more robust.

Here's a short blog post about how this library came to be: http://mattweimer.net/Posts/Details/1

## Requirements
Microsoft .NET Micro Framework 4.2 or higher

## Example Usage

An instance of the JsonSerializer class can be instantiated using the constructor. This will allow the serializier to remember your DateTime format preference.

```c#
JsonSerializer serializer = new JsonSerializer(DateTimeFormat.Default);
string json = serializer.Serialize(o);
```

Additonally, you can just call the static SerializeObject/DeserializeString methods and specifiy your DateTime format preference as an argument.

```c#
string json = JsonSerializer.SerializeObject(o, DateTimeFormat.Default);
```

###Serialization

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

Only properties that have a public getter will be serialized. The getter method must not be virtual or abstract and must return an object that is not abstract, a delegate or be of MethodInfo type.

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

###Deserialization

Deserialization will parse your JSON string and return it contents in either ArrayList, Hashtable, double, string, null, bool. You will need to know what type you are expecting and cast the return object to that type.

```c#
// JSON array to ArrayList
ArrayList arrayList = JsonSerializer.DeserializeString("[\"hello\", \"world\", \"!!!\"]") as ArrayList;
Debug.Print(arrayList[0] + " " + arrayList[1] + arrayList[2]);
// Output: "Hello World!!!"


// JSON object to Hashtable
Hashtable hashTable = JsonSerializer.DeserializeString("{\"firstName\":\"John\",\"lastName\":\"Doe\"}") as Hashtable;
Debug.Print(hashTable["firstName"] + " " + hashTable["lastName"]);
// Output: "John Doe"
```

Extensions methods are provided to parse a DateTime string to a DateTime object
```c#
Hashtable hashTable = JsonSerializer.DeserializeString("{\"firstName\":\"John\",\"lastName\":\"Doe\",\"birthDay\":\"1985-04-27T00:00:00.000Z\"}") as Hashtable;
DateTime birthday = DateTimeExtensions.FromIso8601(hashTable["birthDay"] as string);
Debug.Print(birthday.ToString());
// Output: "04/27/1985 00:00:00"
```

## Nuget
Available through Nuget (http://www.nuget.org/packages/Json.NetMF/)

```
PM> Install-Package Json.NetMF
```
