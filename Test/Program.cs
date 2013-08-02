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
            public DateTime Birthday { get; set; }
        }

        public static void Main()
        {

            Hashtable t = new Hashtable();
            ICollection col = new ArrayList() { 1, null, 2, "blah", false };
            t.Add("col", col);
            t.Add("nulltest", null);
            t.Add("somestring", "dgdg");
            object[] array = new object[] { t };
            string json = JsonSerializer.SerializeObject(array);

            ArrayList a = JsonSerializer.DeserializeString(json) as ArrayList;
            Hashtable b = a[0] as Hashtable;
            ArrayList c = b["col"] as ArrayList;
            object n = b["nulltest"];
            string s = b["somestring"].ToString();
            bool d = (bool)c[4];


            Debug.Print(s);

           // Person person = new Person() { FirstName = "John", LastName = "Doe", Birthday = new DateTime(1988, 4, 23) };
            //string json = JsonSerializer.SerializeObject(person);
        }

    }
}
