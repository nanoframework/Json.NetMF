using System;
using Microsoft.SPOT;
using System.Collections;
using Json.NETMF;

namespace Test
{
    public class Program
    {
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public DateTime Birthday { get; set; }
            public int ID { get; set; }
            public string[] ArrayProperty { get; set; }
            public Person Friend { get; set; }
        }

        public abstract class AbstractClass
        {
            public int ID { get; set; }
            public abstract string Test { get; }
            public virtual string Test2 { get { return "test2"; } }
        }

        public class RealClass : AbstractClass
        {
            public override string Test { get { return "test"; } }
        }

        public static void Main()
        {

            BasicSerializationTest();
            SerializeSimpleClassTest();
            BasicDeserializationTest();
            SerializeAbstractClassTest();
        }

        public static bool SerializeAbstractClassTest()
        {
            try
            {
                AbstractClass a = new RealClass() { ID = 12 };
                string json = JsonSerializer.SerializeObject(a);
                string correctValue = "{\"Test2\":\"test2\",\"ID\":12,\"Test\":\"test\"}";
                if (json != correctValue)
                {
                    Debug.Print("Fail: SerializeAbstractClassTest - Values did not match");
                    return false;
                }

                RealClass b = new RealClass() { ID = 12 };
                json = JsonSerializer.SerializeObject(b);
                correctValue = "{\"Test2\":\"test2\",\"ID\":12,\"Test\":\"test\"}";
                if (json != correctValue)
                {
                    Debug.Print("Fail: SerializeAbstractClassTest - Values did not match");
                    return false;
                }

                Debug.Print("Success: SerializeAbstractClassTest");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Fail: SerializeAbstractClassTest - " + ex.Message);
                return false;
            }
        }

        public static bool SerializeSimpleClassTest()
        {
            try
            {
                Person friend = new Person()
                {
                    FirstName = "Bob",
                    LastName = "Smith",
                    Birthday = new DateTime(1983, 7, 3),
                    ID = 2,
                    Address = "123 Some St",
                    ArrayProperty = new string[] { "hi", "planet" },
                };
                Person person = new Person() 
                { 
                    FirstName = "John", 
                    LastName = "Doe", 
                    Birthday = new DateTime(1988, 4, 23), 
                    ID = 27, 
                    Address = null,
                    ArrayProperty = new string[] { "hello", "world" },
                    Friend = friend
                };
                string json = JsonSerializer.SerializeObject(person);
                string correctValue = "{\"Address\":null,\"ArrayProperty\":[\"hello\",\"world\"],\"ID\":27,\"Birthday\":\"1988-04-23T00:00:00.000Z\",\"LastName\":\"Doe\",\"Friend\""
                    + ":{\"Address\":\"123 Some St\",\"ArrayProperty\":[\"hi\",\"planet\"],\"ID\":2,\"Birthday\":\"1983-07-03T00:00:00.000Z\",\"LastName\":\"Smith\",\"Friend\":null,\"FirstName\":\"Bob\"}"
                    +",\"FirstName\":\"John\"}";
                if (json != correctValue)
                {
                    Debug.Print("Fail: SerializeSimpleClassTest - Values did not match");
                    return false;
                }

                Debug.Print("Success: SerializeSimpleClassTest");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Fail: SerializeSimpleClassTest - " + ex.Message);
                return false;
            }
        } 

        public static bool BasicSerializationTest()
        {
            try
            {
                ICollection collection = new ArrayList() { 1, null, 2, "blah", false };
                Hashtable hashtable = new Hashtable();
                hashtable.Add("collection", collection);
                hashtable.Add("nulltest", null);
                hashtable.Add("stringtest", "hello world");
                object[] array = new object[] { hashtable };
                string json = JsonSerializer.SerializeObject(array);
                string correctValue = "[{\"stringtest\":\"hello world\",\"nulltest\":null,\"collection\":[1,null,2,\"blah\",false]}]";
                if (json != correctValue)
                {
                    Debug.Print("Fail: BasicSerializationTest - Values did not match");
                    return false;
                }

                Debug.Print("Success: BasicSerializationTest");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Fail: BasicSerializationTest - " + ex.Message);
                return false;
            }
        }

        public static bool BasicDeserializationTest()
        {
            try
            {
                string json = "[{\"stringtest\":\"hello world\",\"nulltest\":null,\"collection\":[-1,null,24.565657576,\"blah\",false]}]";
                ArrayList arrayList = JsonSerializer.DeserializeString(json) as ArrayList;
                Hashtable hashtable = arrayList[0] as Hashtable;
                string stringtest = hashtable["stringtest"].ToString();
                object nulltest = hashtable["nulltest"];
                ArrayList collection = hashtable["collection"] as ArrayList;
                long a = (long)collection[0];
                object b = collection[1];
                double c = (double)collection[2];
                string d = collection[3].ToString();
                bool e = (bool)collection[4];

                if (arrayList.Count != 1)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (hashtable.Count != 3)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (stringtest != "hello world")
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (nulltest != null)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (collection.Count != 5)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (a != -1)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (b != null)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (c != 24.565657576)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (d != "blah")
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (e != false)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                Debug.Print("Success: BasicDeserializationTest");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Fail: BasicDeserializationTest - " + ex.Message);
                return false;
            }
        }

        public static bool SerializeCustomTypeTest()
        {
            try
            {
                string json = "[{\"stringtest\":\"hello world\",\"nulltest\":null,\"collection\":[-1,null,24.565657576,\"blah\",false]}]";
                ArrayList arrayList = JsonSerializer.DeserializeString(json) as ArrayList;
                Hashtable hashtable = arrayList[0] as Hashtable;
                string stringtest = hashtable["stringtest"].ToString();
                object nulltest = hashtable["nulltest"];
                ArrayList collection = hashtable["collection"] as ArrayList;
                long a = (long)collection[0];
                object b = collection[1];
                double c = (double)collection[2];
                string d = collection[3].ToString();
                bool e = (bool)collection[4];

                if (arrayList.Count != 1)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (hashtable.Count != 3)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (stringtest != "hello world")
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (nulltest != null)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (collection.Count != 5)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (a != -1)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (b != null)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (c != 24.565657576)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (d != "blah")
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                if (e != false)
                {
                    Debug.Print("Fail: BasicDeserializationTest - Values did not match");
                    return false;
                }

                Debug.Print("Success: BasicDeserializationTest");
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Fail: BasicDeserializationTest - " + ex.Message);
                return false;
            }
        }

    }
}
