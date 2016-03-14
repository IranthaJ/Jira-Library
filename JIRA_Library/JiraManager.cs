using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JIRA_Library.Entities.Projects;
using Newtonsoft.Json;
using JIRA_Library.Entities.Issues;
using JIRA_Library.Entities.Searching;
using JIRA_Library.Entities.Transitions;

namespace JIRA_Library
{
    public class JiraManager
    {
        private const string m_BaseUrl = "https://bqujira.atlassian.net/rest/api/2/";
        private string m_Username;
        private string m_Password;

        public JiraManager(string username, string password)
        {
            m_Username = username;
            m_Password = password;
        }

        public Issue getIssueById(string issueID)
        {
            string issueString = RunQuery(JiraResource.issue, issueID);
            return JsonConvert.DeserializeObject<Issue>(issueString);
        }

        public string deleteWorkLog(string issueID, string worklogID)
        {
            string arguments = string.Format("{0}/worklog/{1}", issueID, worklogID);
            string result = RunQuery(JiraResource.issue, arguments, null, "DELETE");
            return result;
        }


        public Worklog getAllworklogsInIssue(string issueID)
        {
            string workLogsString = RunQuery(JiraResource.issue, string.Format("{0}/worklog", issueID));
            return JsonConvert.DeserializeObject<Worklog>(workLogsString);
        }

        public List<ProjectDescription> GetProjects()
        {
            List<ProjectDescription> projects = new List<ProjectDescription>();
            string projectsString = RunQuery(JiraResource.project);

            return JsonConvert.DeserializeObject<List<ProjectDescription>>(projectsString);
        }

        public string updateIssueLog(string issueID, WorkLogDetails update)
        {
            string arguments = string.Format("{0}/worklog", issueID);
            string data = JsonConvert.SerializeObject(update);
            string result = RunQuery(JiraResource.issue, arguments, data, "POST");
            WorkLogDetails log = JsonConvert.DeserializeObject<WorkLogDetails>(result);
            return log.logID.ToString();
        }

        public SearchResponse getTransitions(string issueID)
        {
            string jql = string.Format("{0}/transitions", issueID);
            string data = RunQuery(JiraResource.issue, jql);
            SearchResponse response = JsonConvert.DeserializeObject<SearchResponse>(data);
            return response;
        }

        public void resolveIssue(string issueID, Solve tr)
        {
            string jql = string.Format("{0}/transitions", issueID);
            string data = JsonConvert.SerializeObject(tr);
            string result = RunQuery(JiraResource.issue,jql, data: data, method: "POST");
        }

        private List<Transition> getTransitionsList(string jql, List<string> fields = null, int startAt = 0, int maxResult = 10)
        {
            fields = fields ?? new List<string> { "id", "name" };

            SearchRequest request = new SearchRequest();
            request.Fields = fields;
            request.JQL = jql;
            request.MaxResults = maxResult;
            request.StartAt = startAt;

            string data = JsonConvert.SerializeObject(request);
            string result = RunQuery(JiraResource.issue, jql, data: data, method: "GET");

            SearchResponse response = JsonConvert.DeserializeObject<SearchResponse>(result);

            return response.transitionsDescriptions;
        }



        public List<Issue> GetEmployeeOpenIssues(string username)
        {
            username = username.Replace("@", "\\u0040");
            string jql = "assignee = " + username + " AND Resolution=Unresoved";
            List<Issue> issueList = GetIssues(jql);
            return issueList;
        }

        private List<Issue> GetIssues(string jql, List<string> fields = null, int startAt = 0, int maxResult = 1000)
        {
            fields = fields ?? new List<string> { "summary", "status", "assignee", "project", "issuetype", "worklog"};

            SearchRequest request = new SearchRequest();
            request.Fields = fields;
            request.JQL = jql;
            request.MaxResults = maxResult;
            request.StartAt = startAt;


            string data = JsonConvert.SerializeObject(request);
            string result = RunQuery(JiraResource.search, data: data, method: "POST");

            SearchResponse response = JsonConvert.DeserializeObject<SearchResponse>(result);

            return response.IssueDescriptions;
        }

        /// <summary>
        /// Runs a query towards the JIRA REST api
        /// </summary>
        /// <param name="resource">The kind of resource to ask for</param>
        /// <param name="argument">Any argument that needs to be passed, such as a project key</param>
        /// <param name="data">More advanced data sent in POST requests</param>
        /// <param name="method">Either GET or POST</param>
        /// <returns></returns>
        protected string RunQuery(JiraResource resource, string argument = null, string data = null,string method = "GET")
        {
            string url = string.Format("{0}{1}/", m_BaseUrl, resource.ToString());

            if (argument != null)
            {
                url = string.Format("{0}{1}/", url, argument);
            }

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.ContentType = "application/json";
            request.Method = method;
            request.Accept = "application/json";

            string base64Credentials = GetEncodedCredentials();
            request.Headers.Add("Authorization", "Basic " + base64Credentials);

            if (data != null)
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(data);
                }
            }

            HttpWebResponse response =(HttpWebResponse)request.GetResponse();

            string result = string.Empty;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private string GetEncodedCredentials()
        {
            string mergedCredentials = string.Format("{0}:{1}", m_Username, m_Password);
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes(mergedCredentials);
            return Convert.ToBase64String(byteCredentials);
        }

    }
}
