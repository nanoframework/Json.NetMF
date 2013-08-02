using System;
using Microsoft.SPOT;
using Json.NETMF;
using System.Collections;

namespace Test
{
    public class Program
    {
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Birthday { get; set; }
        }

        public static void Main()
        {
            //Hashtable t = new Hashtable();
            //ICollection col = new ArrayList() { 1, null, 2, "blah", false };
            //t.Add("col", col);
            //t.Add("nulltest", null);
            //t.Add("somestring", "dgdg");
            //object[] array = new object[] { t };
            //string json = JsonSerializer.SerializeObject(array);

            Person person = new Person() { FirstName = "John", LastName = "Doe", Birthday = new DateTime(1988, 4, 23) };
            string json = JsonSerializer.SerializeObject(person);
            Debug.Print(json);
        }

    }
}
