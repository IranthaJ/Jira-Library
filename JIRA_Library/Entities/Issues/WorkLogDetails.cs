using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace JIRA_Library.Entities.Issues
{
    public class WorkLogDetails
    {
        [JsonProperty("id")]
        public int logID { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("started")]
        public System.Nullable<System.DateTime> workedDate { get; set; }

        [JsonProperty("timeSpent")]
        public string timeSpent { get; set; }
    }
}
