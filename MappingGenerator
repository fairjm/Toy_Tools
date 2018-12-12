using Newtonsoft.Json.Linq; // JSON.NET for addtional dependency
using System;
using System.Linq;
/**
 * a very simple es mapping generator
 * 
 **/
namespace MappingGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = @"id : long
            name : text
            ";
            var properties = new JObject();
            var lines = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).OfType<string>().ToList().Select(e =>
            {
                var kv = e.Split(":");
                return (kv[0].Trim(), kv[1].Trim());
            });
            foreach (var l in lines)
            {
                var name = l.Item1;
                var typeName = l.Item2;
                if (typeName == "text")
                {
                    properties[name] = new JObject
                    {
                        { "type", "text" },
                        { "analyzer", "ik_max_word" }, // use ik for text
                        { "fields", JObject.FromObject(new
                            {
                                keyword = new // add a keyword type field
                                {
                                    type = "keyword",
                                    ignore_above = 256
                                }
                            })
                        }
                    };
                }
                else
                {
                    properties[l.Item1] = JObject.FromObject(new
                    {
                        type = l.Item2
                    });
                }
            }

            var result = new JObject
            {
                { "mappings", new JObject
                    {
                        { "_doc", new JObject
                            {
                                { "properties", properties}
                            }
                        }
                    }
                }
            };
            Console.WriteLine(result.ToString());
        }
    }
}
