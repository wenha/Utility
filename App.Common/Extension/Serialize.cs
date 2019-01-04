using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Extension
{
    public static class Serialize
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T JsonTo<T>(this string obj)
        {
            return (T)JsonConvert.DeserializeObject(obj, typeof(T));
        }
    }
}
