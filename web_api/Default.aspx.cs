using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Ultimus;
using Ultimus.WFServer;
using System.Net.Json;
using System.Data.SqlClient;


public partial class _Default : System.Web.UI.Page 
{

    private string task(int incidentNo, string processNameFilter)
    {
        Ultimus.WFServer.Tasklist tl = new Ultimus.WFServer.Tasklist();
        Ultimus.WFServer.TasklistFilter tf = new Ultimus.WFServer.TasklistFilter();
        tf.nIncidentNo = incidentNo;
        tf.strProcessNameFilter = processNameFilter;
        tl.LoadFilteredTasks(tf);

        Ultimus.WFServer.Task task = null;
        while (true)
        {
            task = tl.GetNextTask();
            if (task == null) break;
            if (task.nTaskStatus == 1) break;

        }
        return task.strFormUrl;
    }

    protected string condition()
    {
        string sqlCmd = "";

        SqlParameter pmr = null;
       
        string[] list = new string[2];
        if (Request.QueryString["taskuser"] != null)
        {
            pmr = new SqlParameter(Request.QueryString["taskuser"],SqlDbType.NChar);
            list[0] = "TASKUSER ='" + pmr + "'";
        }

        if (Request.QueryString["status"] != null)
        {
            pmr = new SqlParameter(Request.QueryString["status"],SqlDbType.Int);
            list[1] = "STATUS =" + pmr;
        }

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
            {
                if (sqlCmd == "")
                {
                    sqlCmd = "WHERE " + list[i];
                }
                else
                {
                    sqlCmd = sqlCmd + "AND " + list[i];
                }
            }
        }
        return sqlCmd;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        JsonObjectCollection root = new JsonObjectCollection();

        SqlConnection objConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString);

        string strCmd;
        strCmd = @"SELECT 
                     TASKID,PROCESSNAME,PROCESSVERSION,INCIDENT,STEPID,STEPLABEL,RECIPIENT,RECIPIENTTYPE,TASKUSER,ASSIGNEDTOUSER,
                     STATUS,SUBSTATUS,STARTTIME,ENDTIME 
                     FROM TASKS " + condition();

        objConnection.Open();
        SqlCommand cmd = new SqlCommand(strCmd, objConnection);

        SqlDataReader dr = cmd.ExecuteReader();

        JsonArrayCollection list = new JsonArrayCollection("tasks");
        while (dr.Read())
        {
            {   
                JsonObjectCollection sub = new JsonObjectCollection();
                sub.Add(new JsonStringValue("taskid", dr["TASKID"].ToString()));
                sub.Add(new JsonStringValue("processname", dr["PROCESSNAME"].ToString()));
                sub.Add(new JsonStringValue("processversion", dr["PROCESSVERSION"].ToString()));
                sub.Add(new JsonStringValue("incident", dr["INCIDENT"].ToString()));
                sub.Add(new JsonStringValue("stepid", dr["STEPID"].ToString()));
                sub.Add(new JsonStringValue("steplabel", dr["STEPLABEL"].ToString()));
                sub.Add(new JsonStringValue("recipient", dr["RECIPIENT"].ToString()));
                sub.Add(new JsonStringValue("recipienttype", dr["RECIPIENTTYPE"].ToString()));
                sub.Add(new JsonStringValue("taskuser", dr["TASKUSER"].ToString()));
                sub.Add(new JsonStringValue("assignedtouser", dr["ASSIGNEDTOUSER"].ToString()));
                sub.Add(new JsonStringValue("status", dr["STATUS"].ToString()));
                sub.Add(new JsonStringValue("substatus", dr["SUBSTATUS"].ToString()));
                sub.Add(new JsonStringValue("starttime", dr["STARTTIME"].ToString()));
                sub.Add(new JsonStringValue("endtime", dr["ENDTIME"].ToString()));
                sub.Add(new JsonStringValue("url", task(int.Parse(dr["INCIDENT"].ToString()), dr["PROCESSNAME"].ToString())));
                list.Add(sub);
            }
        }
        root.Add(list);
        objConnection.Close();
        Label1.Text = root.ToString();
        //Response.ContentType = "text/json";
        //Response.Write(root.ToString());
        //Response.End();
    }
}
