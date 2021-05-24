using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace QuantForce
{
    /// <summary>
    /// Async process status
    /// </summary>
    public enum Status { wait = 0, toProcess = 100, stating = 200, running = 300 , done = 400, error = 401, stopped = 402, killed = 500};

    /// <summary>
    /// The value you derive from when calling async api
    /// </summary>
    public class AsyncTaskStart
    {
        /// <summary>
        /// Free to use tag
        /// </summary>
        public string tag { get; set; }
        /// <summary>
        /// When set the callback url to call using a GET.
        /// %%id is replaced with id
        /// %%tag is replaced with tag
        /// </summary>
        public string callback { get; set; }
    }
    /// <summary>
    /// Return value when you query task status
    /// </summary>
    public class AsyncTaskStatus
    {
        /// <summary>
        /// Async id to use for subsequent calls
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// User define tag
        /// </summary>
        public string tag { get; set; }
        /// <summary>
        /// Current task status
        /// </summary>
        public Status status { get; set; }
        /// <summary>
        /// When possible the achievement percentage
        /// </summary>
        public int percent { get; set; }
        /// <summary>
        /// When state = FinishedOK the expected result in json
        /// When state = FinishedError full error infos { "message" : "null pointer", "stack" : "call stack" }
        /// </summary>
        public JObject result { get; set; }
    }

    /// <summary>
    /// Enumeration used for authentication type
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Raw login
        /// </summary>
        Raw,

        /// <summary>
        /// MD5 login
        /// </summary>
        MD5
    }

    /// <summary>
    /// Object to post on auth
    /// </summary>
    public class Auth
    {
        /// <summary>
        /// Authentication type
        /// </summary>
        public Type authType { get; set; }

        /// <summary>
        /// You email address
        /// </summary>
        public string login { get; set; }
        
        /// <summary>
        /// This value depend on authType
        /// Raw then this is raw password
        /// MD5 then this is MD5 password
        /// </summary>
        public string param1 { get; set; }
    }

    /// <summary>
    /// Result from auth
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// The token you have to add to all header using x-token : 
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// Expiration in minute
        /// </summary>
        public int expiration { get; set; }
    }

    /// <summary>
    /// A project
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Unique id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// User set name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// User set description
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Creation date time yyyyMMddHHmmss GMT
        /// </summary>
        public string creationDT { get; set; }
        /// <summary>
        /// Last modification date time yyyyMMddHHmmss GMT
        /// </summary>
        public string modificationDT { get; set; }
        /// <summary>
        /// 0 = QuantDisqovery
        /// 1 = QuantQollect
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 0 = user data, 1 = predefine dataset 1, ... x = predefine dataset x
        /// </summary>
        public int subType { get; set; }
        /// <summary>
        /// The URL to use to acces this project
        /// </summary>
        public string uri { get; set; }
    }


    /// <summary>
    /// A project collect
    /// </summary>
    public class ProjectList
    {
        public List<Project> projects { get; set; }
    }

    public enum VariableType { Undefined = 1, Ignore = 2, Continue = 3, Nominal = 4, Target = 5, Id = 6, Weight = 7 };
    public enum VariableShape { free = 0, pav_growing = 1, pav_decreasing = 2, ushape_convex = 3, ushape_concave = 4 };

    public class Column
    {
        public string name { get; set; } // Name in CSV file
        public string comment { get; set; } // Free field for user
        public VariableType columType { get; set; }
        public VariableShape shape { get; set; }

        // Possible attributes
        // reg_forced = true => this variable is forced during regression
        // reg_llimit => lower limit during regression (double)
        // reg_ulimit => upper limit dureing regression       
        public JObject attributes;

        [JsonExtensionData]
        public IDictionary<string, JToken> catchAll;

        public override string ToString()
        {
            return $"{name}|{columType}";
        }
    }
    public class Dataset
    {
        public List<Column> columns { get; set; }
    }

    public class Bin
    {
        public int id { get; set; }
        public bool selected { get; set; } // Selected for grouping
        public string group { get; set; }
        public string range { get; set; }
        public string nBads { get; set; }
        public string nGoods { get; set; }
        public string nTotal { get; set; }
        public string badRate { get; set; }
        public string pTotal { get; set; }
        public string woe { get; set; }
        // For graphics
        public double graphOptimizedMean { get; set; }
        public double graphBadRate { get; set; }
        public double graphAvgBadRate { get; set; }
        public double graphPercent { get; set; }

        public override string ToString()
        {
            return $"{id}|{selected}";
        }

    }

    public class BinsView
    {
        public string columnName { get; set; }
        public List<Bin> Bins { get; set; } = new List<Bin>();
    }

    public class BinsViewList
    {
        public List<BinsView> all { get; set; } = new List<BinsView>();
    }
}
