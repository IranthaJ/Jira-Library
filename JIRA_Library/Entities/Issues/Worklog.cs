using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace JIRA_Library.Entities.Issues
{
    public class Worklog
    {
        [JsonProperty("total")]
        public int total { get; set; }

        [JsonProperty("workLogs")]
        public List<WorkLogDetails> WorkLogs { get; set; }

    }
}
