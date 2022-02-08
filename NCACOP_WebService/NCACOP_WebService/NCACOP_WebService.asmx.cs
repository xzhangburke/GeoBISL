//
// the project is for NCACOP web application - create NCACOP web service
// visual studio 2019
// autor: xiaoyi zhang
// date: January, 2020
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
//
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;
//
using System.Text;
using System.Collections;
//
using System.Web.Script.Services;
using System.Web.Script.Serialization;
//
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// user
using System.Security.Principal;
using System.Threading;


using System.Net.Mail;
using System.Net.Mime;
using System.ComponentModel;

//
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;


namespace NCACOP_WebService
{
    /// <summary>
    /// Summary description for NCAOD_WebService - get data from sql server for NCAOD project
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class NCACOP_WebService : System.Web.Services.WebService
    {
        // nca email
        public readonly string nca_email = ConfigurationManager.AppSettings.Get("nca_email");

        // dn connection string
        public readonly string conStr = ConfigurationManager.ConnectionStrings["ncaDbConnectionString"].ConnectionString;

        // user data structure

        public struct UserClass
        {
            public string status;
            public string userName;
            public UserPermissionClass UserPermissionData;
        }

        // user assigned site data structure
        public struct UserPermissionClass
        {
            public string status;
            public string editor;
            public string defaultSite;
            public List<SiteClass> siteList;
            public List<DistrictClass> districtList;
            public List<commentLookupClass> commentLookupList;
        }

        // district data structure
        public struct DistrictClass
        {
            public string district;
            public string districtName;
        }

        // site data structure
        public struct SiteClass
        {
            public string site_id;
            public string siteName;
            public string district;
            public string districtName;
            public string editor;
        }

        // burial schedule data structure
        public struct ScheduleClass
        {
            public string status;
            public string html;
        }

        // summary data structure
        public struct SummaryStatusCountClass
        {
            public string status;
            public string value;
        }

        // error summary data structure
        public struct SummaryErrorCountClass
        {
            public string errorType;
            public string count;
        }

        // summary status data structure
        public struct SummaryStatusTypeCountClass
        {
            public string status;
            public string type;
            public string value;
        }

        // summary data list structure
        public struct SummaryListClass
        {
            public string status;
            public string totalCount;
            public string totalCount_error;
            public string lastUpdateDate;
            public List<SummaryStatusCountClass> statusList;
            public List<SummaryStatusTypeCountClass> statusTypeList;
            public List<SummaryErrorCountClass> errorList;
            public List<SummaryStatusTypeCountClass> errorTypeList;
        }

        // count data structure
        public struct CountClass
        {
            public string status;
            public string type;
            public string value;
            public List<SectionOutClass> sectionData;
        }

        // section data count structure

        public struct SectionOutClass
        {
            public string section;
            public string type;
            public string size;
            public string total_count;
            public string Reserved_count;
            public string Occupied_count;
            public string Obstructed_count;
            public string Available_count;
            public string null_count;
        }

        // section data structure
        public struct SectionClass
        {
            public string section;
            public string type;
            public string size;
            public string status;
            public string value;
        }

        // gravesite error data structure
        public struct GraveErrorClass
        {
            public long error_id;
            public string site_id;
            public string grave_id;
            public string grave_type;
            public string error_type;
            public string error_desc;
            //public string resolve_flag;
            public string record_date;
            //public string app_flag;
            public string Collected_By;
            public string USER_ACCOUNT;
            public string DEVICE_TYPE;
            public string CORRECTION_STATUS;
            public string CORRECTION_SOURCE;
            public string EST_ACCURACY_IN;
            public string CAPTURE_TYPE;
            public string PDOP;
            public string HDOP;
            public string COLLECTION_DATE_UTC;
            public string UPDATE_DATE_UTC;
            public string Elevation;
            public string Photo1;
            public string Photo2;
            public string interment_date;
            public string interment_type;
            public string decedent_id;
            public string Section;
            public string Wall;
            public string Row;
            public string Gravesite;
            public string firstName;
            public string lastName;
            public string suffix;
            public string comments;
            public string Verified_Date;
            public string Marker_Type;
            public string Marker_Material;
            public string status;
            public string NameFirstMiddleLast;
            public string AddlNameFirstMiddleLast;
            public string Addl3rdNameFirstMiddleLast;
            public string Addl4thNameFirstMiddleLast;
            public string geom;
        }

        // gravesite error list data structure
        public struct GraveErrorListClass
        {
            public string status;
            public List<GraveErrorClass> graveErrorList;
        }

        // gravesite error comment data structure
        public struct GraveErrorCommentClass
        {
            public int comment_id;
            public int error_id;
            public string comment_text;
            public string comment_user;
            public string comment_date;
        }

        // gravesite error data list structure
        public struct GraveErrorCommentListClass
        {
            public string status;
            public List<GraveErrorCommentClass> graveErrorCommentList;
        }

        // comment lookup
        public struct commentLookupClass
        {
            public string errorType;
            public string dropdownText;
            public string resolveType;
        }


        [WebMethod(Description = "get user name and assign site data and return json data")]
        public string GetUserInfo()
        {
            string userName = GetUserName(); // get user's  WindowsPrincipal name

            UserPermissionClass UserPermissionData = GetUserPermissionInfo(userName); //get user assigned site data 

            // create json reture data
            UserClass userData = new UserClass()
            {
                status = "OK",
                userName = userName,
                UserPermissionData = UserPermissionData
            };

            var json = new JavaScriptSerializer().Serialize(userData);

            return json;
        }

        [WebMethod(Description = "Get NCA Burial Schedule data from url and pare unwanted elements to return json data. input parameter - site_id")]
        public string GetNCAScheduleData(string site_id)
        {
            // burial schedule url, 4 days in furture and drag parameter is not useful
            var url = "https://sched.cem.va.gov/DisplaySchedule/DisplaySched?ID=" + site_id + "&DT=4&DRAG=40";

            // open url and retrieve html contents
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var html = doc.DocumentNode.OuterHtml;

            // parse unwanted elements from html
            html = html.Replace("\"", "'");
            string[] separatingStrings = { "content[0]=" };
            string[] split0 = html.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            html = split0[1];

            string[] separatingStrings2 = { "content[1]=content[0]" };
            split0 = html.Split(separatingStrings2, System.StringSplitOptions.RemoveEmptyEntries);

            html = split0[0];
            html = html.Replace("'", "");
            html = html.Replace("+", "");
            html = html.Replace("&nbsp;", "");

            html = html.Replace("< img src = images / dot.jpg width = 100 % height = 1 >", "");
            html = html.Replace("<img src=images/dot.jpg width=100% height=1>", "");
            html = html.Replace("font size=4", "font size=1");
            html = html.Replace("<td colspan=14 align=center><font size=1>", "<td colspan=14 align=center><font size=4>");
            html = html.Replace("<table", "<table id='scheduleTable'");

            // create json reture data
            ScheduleClass scheduleData = new ScheduleClass()
            {
                status = "OK",
                html = html
            };

            var json = new JavaScriptSerializer().Serialize(scheduleData);

            return json;
        }

        
        [WebMethod(Description = "Get GraveSite marker Data by site_id and two date range")]
        public string GetSiteSummaryDataByTimeRange(string site_id, string date1, string date2, string userName)
        {
            List<GraveErrorClass> graveErrorList = new List<GraveErrorClass>();
            GraveErrorClass graveErrorData;

            string checkUserStaus = CheckUser(userName);
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        string sqlcomm = "SELECT SITE_ID,GRAVEMARKER_ID,SECTION,WALL,ROW,GRAVESITE,INSTALL_DATE,MARKER_TYPE,MARKER_MATERIAL,FRONT_IMG,FRONT_IMG_URL,REAR_IMG,REAR_IMG_URL,LATITUDE_DD,LONGITUDE_DD,ELEVATION_FT,POSITION_SRC,COMMENT,ACTIVE,created_user,created_date,last_edited_user,last_edited_date,shape.STX as x,shape.STY as y FROM [NCA].[GRAVESITE_MARKER_PT] where site_id=@site_id AND (CONVERT(date, [created_date]) BETWEEN @date1 AND @date2) ORDER BY created_date DESC";
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);
                        comm.Parameters.AddWithValue("@date1", date1);
                        comm.Parameters.AddWithValue("@date2", date2);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            graveErrorData = new GraveErrorClass()
                            {
                                error_id = Convert.ToUInt16(row[0]),
                                site_id = Convert.ToString(row[1]),
                                grave_id = Convert.ToString(row[2]),
                                grave_type = Convert.ToString(row[3]),
                                error_type = Convert.ToString(row[4]),
                                error_desc = Convert.ToString(row[5]),
                                //resolve_flag = Convert.ToString(row[6]),
                                geom = Convert.ToString(row[7]) + "," + Convert.ToString(row[8])
                            };
                            graveErrorList.Add(graveErrorData);
                        }
                    }

                    GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                    {
                        status = "OK",
                        graveErrorList = graveErrorList
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                    return json;
                }
                catch (Exception e)
                {
                    GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                    return json;
                }
            }
            else
            {
                GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                return json;
            }
        }
        

        [WebMethod(Description = "get user assign site data by user name")]
        public string GetUserData(string userName)
        {
            // get site list
            UserPermissionClass UserPermissionData = GetUserPermissionInfo(userName); //get user assigned site data 

            // create json reture data
            UserClass userData = new UserClass()
            {
                status = "OK",
                userName = userName,
                UserPermissionData = UserPermissionData
            };

            var json = new JavaScriptSerializer().Serialize(userData);

            return json;
        }


        [WebMethod(Description = "Get Site Summary Data by site_id")]
        public string GetSiteSummaryData(string site_id, string userName)
        {
            List<SummaryStatusTypeCountClass> statusTypeList = new List<SummaryStatusTypeCountClass>();
            List<SummaryStatusCountClass> statusList = new List<SummaryStatusCountClass>();
            List<SummaryStatusTypeCountClass> errorTypeList = new List<SummaryStatusTypeCountClass>();
            List<SummaryErrorCountClass> errorList = new List<SummaryErrorCountClass>();

            long totalCount = 0;
            long totalCount_error = 0;
            string update_date = "";

            string checkUserStaus = CheckUser(userName);
            //checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        //
                        string sqlcomm = "Select GRAVESITE_STATUS, GRAVESITE_TYPE, COUNT(*) as count From nca.GraveSite_Poly_evw Where SITE_ID=@site_id GROUP BY GRAVESITE_STATUS, GRAVESITE_TYPE ORDER BY GRAVESITE_STATUS, GRAVESITE_TYPE";
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        string prevStatus = "";
                        long statusCount = 0;
                        int i = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            i += 1;
                            string status = Convert.ToString(row[0]);
                            string type = Convert.ToString(row[1]);
                            string value = Convert.ToString(row[2]);

                            totalCount += Convert.ToInt32(value);

                            SummaryStatusTypeCountClass summaryStatusTypeCountData = new SummaryStatusTypeCountClass()
                            {
                                status = status,
                                type = type,
                                value = value
                            };
                            statusTypeList.Add(summaryStatusTypeCountData);
                            //
                            if (prevStatus != status)
                            {
                                if (i > 1)
                                {
                                    SummaryStatusCountClass SummaryStatusCountData = new SummaryStatusCountClass()
                                    {
                                        status = prevStatus,
                                        value = Convert.ToString(statusCount)
                                    };
                                    statusList.Add(SummaryStatusCountData);
                                }
                                statusCount = Convert.ToInt32(value);
                            }
                            else
                            {
                                statusCount += Convert.ToInt32(value);
                            }
                            //
                            prevStatus = status;
                        }
                        //
                        if (i > 0)
                        {
                            SummaryStatusCountClass SummaryStatusCountData = new SummaryStatusCountClass()
                            {
                                status = prevStatus,
                                value = Convert.ToString(statusCount)
                            };
                            statusList.Add(SummaryStatusCountData);
                        }

                        // site error summary
                        //sqlcomm = "Select error_type, grave_type, count(*) as count From nca.Grave_Error_evw Where APP_FLAG='T' AND SITE_ID=@site_id Group By error_type, grave_type Order by error_type, grave_type";
                        sqlcomm = "Select error_type, grave_type, count(*) as count From nca.v_GraveError_Unresolved Where APP_FLAG='T' AND SITE_ID=@site_id Group By error_type, grave_type Order by error_type, grave_type";
                        
                                                comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);
                        dr = null;
                        dr = comm.ExecuteReader();
                        dt = new DataTable();
                        dt.Load(dr);

                        string prevErrorType = "";
                        int errorTypeCount = 0;
                        int n = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            n += 1;
                            string error_type = Convert.ToString(row[0]);
                            string grave_type = Convert.ToString(row[1]);
                            string value = Convert.ToString(row[2]);

                            totalCount_error += Convert.ToInt32(value);

                            SummaryStatusTypeCountClass errorTypeCountData = new SummaryStatusTypeCountClass()
                            {
                                status = error_type,
                                type = grave_type,
                                value = value
                            };
                            errorTypeList.Add(errorTypeCountData);

                            //SummaryErrorCountClass summaryErrorCountData = new SummaryErrorCountClass()
                            //{
                            //    errorType = error_type,
                            //    count = count
                            //};
                            //errorTypeCountList.Add(summaryErrorCountData);

                            //
                            if (prevErrorType != error_type)
                            {
                                if (n > 1)
                                {
                                    SummaryErrorCountClass errorCountData = new SummaryErrorCountClass()
                                    {
                                        errorType = prevErrorType,
                                        count = Convert.ToString(errorTypeCount)
                                    };
                                    errorList.Add(errorCountData);
                                }
                                errorTypeCount = Convert.ToInt32(value);
                            }
                            else
                            {
                                errorTypeCount += Convert.ToInt32(value);
                            }
                            //
                            prevErrorType = error_type;

                        }
                        //
                        if (n > 0)
                        {
                            SummaryErrorCountClass errorCountData = new SummaryErrorCountClass()
                            {
                                errorType = prevErrorType,
                                count = Convert.ToString(errorTypeCount)
                            };
                            errorList.Add(errorCountData);
                        }

                        // last update date
                        //sqlcomm = "Select cast(Last_Updated_date as date) as updateDate From nca.v_GPS_LastImport Where SITE_ID=@site_id";
                        sqlcomm = "Select Last_Updated_date From nca.v_GPS_LastImport Where SITE_ID=@site_id";
                        comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);
                        dr = null;
                        dr = comm.ExecuteReader();
                        dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            update_date = Convert.ToString(row[0]);
                        }

                    }


                    // return
                    SummaryListClass summaryListData = new SummaryListClass()
                    {
                        status = "OK",
                        totalCount = Convert.ToString(totalCount),
                        totalCount_error = Convert.ToString(totalCount_error),
                        statusList = statusList,
                        statusTypeList = statusTypeList,
                        errorList = errorList,
                        errorTypeList = errorTypeList,
                        lastUpdateDate = update_date
                    };

                    var json = new JavaScriptSerializer().Serialize(summaryListData);
                    return json;
                }
                catch (Exception e)
                {
                    SummaryListClass summaryListData = new SummaryListClass()
                    {
                        status = "Error - " + e.Message
                    };
                    var json = new JavaScriptSerializer().Serialize(summaryListData);
                    return json;
                }
            }
            else
            {
                SummaryListClass summaryListData = new SummaryListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };
                var json = new JavaScriptSerializer().Serialize(summaryListData);
                return json;
            }
        }

        [WebMethod(Description = "Get GraveSite Error Data by site_id")]
        public string GetGraveSiteErrorData(string site_id, string resolve_flag, string userName)
        {
            List<GraveErrorClass> graveErrorList = new List<GraveErrorClass>();
            GraveErrorClass graveErrorData;

            string checkUserStaus = CheckUser(userName);
            checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        //
                        //string sqlcomm = "Select error_id,site_id,grave_id,grave_type,error_type,error_desc,resolve_flag, shape.STX as x, shape.STY as y, record_date, app_flag, Collected_By, USER_ACCOUNT, DEVICE_TYPE, CORRECTION_STATUS, CORRECTION_SOURCE, EST_ACCURACY_IN, CAPTURE_TYPE, PDOP, HDOP, COLLECTION_DATE_UTC, UPDATE_DATE_UTC, Elevation, Photo1, Photo2,  First_Name, Last_Name, Suffix, Comments From nca.Grave_Error_evw Where SITE_ID=@site_id Order By grave_id";
                        // NameFirstMiddleLast;
                        // AddlNameFirstMiddleLast;
                        // Addl3rdNameFirstMiddleLast;
                        // Addl4thNameFirstMiddleLast;


                        string sqlcomm = "";
                        
                        if(resolve_flag == "F")
                        {
                            sqlcomm = "Select error_id,site_id,grave_id,grave_type,error_type,error_desc,shape.STX as x, shape.STY as y, interment_date,Collected_By, USER_ACCOUNT, DEVICE_TYPE, CORRECTION_STATUS, CORRECTION_SOURCE, EST_ACCURACY_IN, CAPTURE_TYPE, PDOP, HDOP, COLLECTION_DATE_UTC, UPDATE_DATE_UTC, Elevation, Photo1, Photo2,  First_Name, Last_Name, Suffix, Comments, Status, interment_type, decedent_id, verified_date, marker_type, marker_material, record_date, section, wall, row, gravesite, NameFirstMiddleLast, AddlNameFirstMiddleLast, Addl3rdNameFirstMiddleLast, Addl4thNameFirstMiddleLast From nca.v_GraveError_Unresolved Where SITE_ID=@site_id And resolve_flag='F' AND app_flag='T' Order By grave_id, error_id";
                        }
                        else
                        {
                            sqlcomm = "Select error_id,site_id,grave_id,grave_type,error_type,error_desc,shape.STX as x, shape.STY as y, interment_date,Collected_By, USER_ACCOUNT, DEVICE_TYPE, CORRECTION_STATUS, CORRECTION_SOURCE, EST_ACCURACY_IN, CAPTURE_TYPE, PDOP, HDOP, COLLECTION_DATE_UTC, UPDATE_DATE_UTC, Elevation, Photo1, Photo2,  First_Name, Last_Name, Suffix, Comments, Status, interment_type, decedent_id, verified_date, marker_type, marker_material, record_date, section, wall, row, gravesite, NameFirstMiddleLast, AddlNameFirstMiddleLast, Addl3rdNameFirstMiddleLast, Addl4thNameFirstMiddleLast From nca.vGraveError_Resolved90 Where SITE_ID=@site_id And resolve_flag='T' AND app_flag='T' Order By grave_id, error_id";
                        }

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            graveErrorData = new GraveErrorClass()
                            {
                                error_id = Convert.ToUInt32(row[0]),
                                site_id = Convert.ToString(row[1]),
                                grave_id = Convert.ToString(row[2]),
                                grave_type = Convert.ToString(row[3]),
                                error_type = Convert.ToString(row[4]),
                                error_desc = Convert.ToString(row[5]),
                                //resolve_flag = Convert.ToString(row[6]),
                                geom = Convert.ToString(row[6]) + "," + Convert.ToString(row[7]),
                                interment_date = ParseDateTime0(Convert.ToString(row[8])),
                                //app_flag = Convert.ToString(row[10]),
                                Collected_By = Convert.ToString(row[9]),
                                USER_ACCOUNT = Convert.ToString(row[10]),
                                DEVICE_TYPE = Convert.ToString(row[11]),
                                CORRECTION_STATUS = Convert.ToString(row[12]),
                                CORRECTION_SOURCE = Convert.ToString(row[13]),
                                EST_ACCURACY_IN = Convert.ToString(row[14]),
                                CAPTURE_TYPE = Convert.ToString(row[15]),
                                PDOP = Convert.ToString(row[16]),
                                HDOP = Convert.ToString(row[17]), 
                                COLLECTION_DATE_UTC = ParseDateTime1(Convert.ToString(row[18])),
                                UPDATE_DATE_UTC = ParseDateTime1(Convert.ToString(row[19])),
                                Elevation = Convert.ToString(row[20]),
                                Photo1 = Convert.ToString(row[21]),
                                Photo2 = Convert.ToString(row[22]),
                                firstName = Convert.ToString(row[23]),
                                lastName = Convert.ToString(row[24]),
                                suffix = Convert.ToString(row[25]),
                                comments = Convert.ToString(row[26]),
                                status = Convert.ToString(row[27]),
                                interment_type = Convert.ToString(row[28]),
                                decedent_id = Convert.ToString(row[29]),
                                Verified_Date = Convert.ToString(row[30]),
                                Marker_Type = Convert.ToString(row[31]),
                                Marker_Material = Convert.ToString(row[32]),
                                record_date = Convert.ToString(row[33]),
                                Section = Convert.ToString(row[34]),
                                Wall = Convert.ToString(row[35]),
                                Row = Convert.ToString(row[36]),
                                Gravesite = Convert.ToString(row[37]),
                                NameFirstMiddleLast = Convert.ToString(row[38]),
                                AddlNameFirstMiddleLast = Convert.ToString(row[39]),
                                Addl3rdNameFirstMiddleLast = Convert.ToString(row[40]),
                                Addl4thNameFirstMiddleLast = Convert.ToString(row[41])

                            };
                            graveErrorList.Add(graveErrorData);
                        }
                    }

                    GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                    {
                        status = "OK",
                        graveErrorList = graveErrorList
                    };

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;
                    var json = serializer.Serialize(graveErrorListData);
                    //var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                    return json;
                }
                catch (Exception e)
                {
                    GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                    return json;
                }
            }
            else
            {
                GraveErrorListClass graveErrorListData = new GraveErrorListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorListData);
                return json;
            }
        }

        [WebMethod(Description = "Get GraveSite error Comments Data by error_id")]
        public string GetGraveSiteErrorCommentData(string error_id, string userName)
        {
            List<GraveErrorCommentClass> graveErrorCommentList = new List<GraveErrorCommentClass>();
            GraveErrorCommentClass graveErrorCommentData;

            string checkUserStaus = CheckUser(userName);
            //checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        //
                        string sqlcomm = "Select comment_id,error_id,comment_text,comment_user, comment_date From nca.Grave_Comment Where ERROR_ID=@error_id ORDER BY comment_date DESC";

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@error_id", error_id);

                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            graveErrorCommentData = new GraveErrorCommentClass()
                            {
                                comment_id = Convert.ToInt16(row[0]),
                                error_id = Convert.ToInt16(row[1]),
                                comment_text = Convert.ToString(row[2]),
                                comment_user = Convert.ToString(row[3]),
                                comment_date = Convert.ToString(row[4])
                            };
                            graveErrorCommentList.Add(graveErrorCommentData);
                        }
                    }

                    GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                    {
                        status = "OK",
                        graveErrorCommentList = graveErrorCommentList
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                    return json;
                }
                catch (Exception e)
                {
                    GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                    return json;
                }
            }
            else
            {
                GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                return json;
            }
        }

        [WebMethod(Description = "save error comments by error_id, comments and user name")]
        public string SaveGraveSiteErrorCommentData(string gravesite_id, string gravesite_type, string error_id, string comment_text, string resolve_flag, string marker_type, string userName)
        {
            string checkUserStaus = CheckUser(userName);
            //checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        // add comment
                        string sqlcomm = "INSERT nca.Grave_Comment (error_id,comment_text,comment_user,comment_date) VALUES (@error_id,@comment_text,@userName,GETDATE())";

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@error_id", error_id);
                        comm.Parameters.AddWithValue("@comment_text", comment_text);
                        comm.Parameters.AddWithValue("@userName", userName);

                        comm.ExecuteNonQuery();

                        // resolve flag
                        sqlcomm = "UPDATE nca.Grave_Error_evw SET RESOLVE_Flag=@resolve_flag Where error_id=@error_id";

                        comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@error_id", error_id);
                        comm.Parameters.AddWithValue("@resolve_flag", resolve_flag);

                        //comm.ExecuteNonQuery();

                        // store procedure
                        sqlcomm = "NCA.ResolveError";

                        comm = new SqlCommand(sqlcomm, conn);
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("@GRAVE_ID", gravesite_id);
                        comm.Parameters.AddWithValue("@GRAVE_TYPE", gravesite_type);
                        comm.Parameters.AddWithValue("@ERROR_ID", error_id);
                        comm.Parameters.AddWithValue("@MarkerType", marker_type);


                        comm.ExecuteNonQuery();

                        // return
                        GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                        {
                            status = "OK"
                        };
                        var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                        return json;
                    }

                }
                catch (Exception e)
                {
                    GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                    return json;
                }
            }
            else
            {
                GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                return json;
            }
        }

        [WebMethod(Description = "save error comments by error_id, comments and user name and send emails")]
        public string SaveGraveSiteErrorCommentDataAndSendEmail(string site_id, string gravesite_id, string error_id, string comment_text, string additionalEmailList, string userName)
        {
            string emailProcessStatus = "";
            string checkUserStaus = CheckUser(userName);
            //checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        // get email cc list
                        int emailStatus = 1;
                        string email_cc_list = "";
                        string sqlcomm = "Select distinct username, email FROM nca.LU_COP_APPUSERS Where DefaultSite=@site_id AND email is not Null";

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);

                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            string email = Convert.ToString(row[1]);
                            if(email_cc_list == "")
                            {
                                email_cc_list = email;
                            }
                            else
                            {
                                email_cc_list += ";" + email;

                            }
                        }

                        // additionalEmailList
                        additionalEmailList = additionalEmailList.Trim();
                        if (additionalEmailList.Length > 0)
                        {
                            if(email_cc_list.Length > 0)
                            {
                                email_cc_list += ";" + additionalEmailList;
                            }
                            else
                            {
                                email_cc_list = additionalEmailList;
                            }
                        }

                        // add comment
                        sqlcomm = "INSERT nca.Grave_Comment (error_id,comment_text,comment_user,comment_date, email_status, email_cc) VALUES (@error_id,@comment_text,@userName,GETDATE(),@emailStatus, @email_cc)";


                        comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@error_id", error_id);
                        comm.Parameters.AddWithValue("@comment_text", comment_text);
                        comm.Parameters.AddWithValue("@userName", userName);
                        comm.Parameters.AddWithValue("@emailStatus", emailStatus);
                        comm.Parameters.AddWithValue("@email_cc", email_cc_list);

                        comm.ExecuteNonQuery();


                        /*
                        // send EMails
                        List<string> emailList = new List<string>();
                        emailList.Add("GeoBISL@va.gov");


                        sqlcomm = "Select distinct username, email FROM nca.LU_COP_APPUSERS Where DefaultSite=@site_id AND email is not Null";

                        comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@site_id", site_id);

                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            string email = Convert.ToString(row[1]);
                            emailList.Add(email);
                        }

                        // email
                        string emailStatus = "OK";
                        try
                        {
                            SmtpClient server = new SmtpClient("smtp.va.gov");
                            server.Port = 25;
                            server.EnableSsl = false;
                            server.Credentials = new System.Net.NetworkCredential("vhamaster\arcgis", "4uTkZta1#$2y");
                            server.Timeout = 5000;
                            server.UseDefaultCredentials = false;

                            MailMessage mail = new MailMessage();
                            mail.From = new MailAddress("ARCGIS@va.med.gov");
                            mail.To.Add(nca_email);
                            if (emailList.Count > 0)
                            {
                                foreach (string email_cc in emailList)
                                {
                                    mail.CC.Add(email_cc);
                                }
                            }

                            mail.Subject = "New Error Comment for Error " + error_id;
                            mail.Body = "New error comment have been generated for error_id = " + error_id + ". Please review the error comment. Contact NCA GIS at NCAGIS@va.gov for help<br><br>"
                                + "<table style=\"border-collapse:collapse;border: solid 1px;\">"
                                + "<tr><td>Gravesite</td><td>" + gravesite_id + "</td></tr>"
                                + "<tr><td>User Name</td><td>" + userName + "</td></tr>"
                                + "<tr><td>DateTime</td><td>" + DateTime.Now.ToString() + "</td></tr>"
                                + "<tr><td>Comment</td><td style=\"white-space:pre-line;\">" + comment_text + "</td></tr>"
                                + "</table>";

                            mail.IsBodyHtml = true;

                            server.Send(mail);
                        }
                        catch (Exception e)
                        {
                            emailStatus = e.Message + "|" + nca_email;
                        }
                        */


                        emailProcessStatus = "OK";
                        //
                        GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                        {
                            status = emailProcessStatus
                        };
                        var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                        return json;
                    }

                }
                catch (Exception e)
                {
                    GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                    return json;
                }
            }
            else
            {
                GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                return json;
            }
        }

        [WebMethod(Description = "update resolve_flag in error table by error_id, resolve_flag and user name")]
        public string UpdateResolveFlagOnErrorData(string error_id, string resolve_flag, string userName)
        {
            string checkUserStaus = CheckUser(userName);
            //checkUserStaus = "";
            if (checkUserStaus == "")
            {
                try
                {

                    using (SqlConnection conn = new SqlConnection(conStr))
                    {
                        conn.Open();

                        //
                        string sqlcomm = "UPDATE nca.Grave_Error_evw SET RESOLVE_Flag=@resolve_flag Where error_id=@error_id";

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.Parameters.AddWithValue("@error_id", error_id);
                        comm.Parameters.AddWithValue("@resolve_flag", resolve_flag);

                        comm.ExecuteNonQuery();

                        GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                        {
                            status = "OK"
                        };
                        var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                        return json;
                    }

                }
                catch (Exception e)
                {
                    GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                    {
                        status = "Error - " + e.Message
                    };

                    var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                    return json;
                }
            }
            else
            {
                GraveErrorCommentListClass graveErrorCommentListData = new GraveErrorCommentListClass()
                {
                    status = "Error - " + "You are not userself..." + checkUserStaus
                };

                var json = new JavaScriptSerializer().Serialize(graveErrorCommentListData);
                return json;
            }
        }

        // coronavirtus




        // functions

        // parse datetime
        public string ParseDateTime0(string dateTimeStr)
        {
            string[] splitStr = dateTimeStr.Split(' ');
            return splitStr[0];
        }
        public string ParseDateTime1(string dateTimeStr)
        {
            string[] splitStr = dateTimeStr.Split('T');
            return splitStr[0];
        }

        // get user's window name
        public string GetUserName()
        {
            WindowsPrincipal p = Thread.CurrentPrincipal as WindowsPrincipal;
            return p.Identity.Name;
        }
        
        // check if user is valid
        public string CheckUser(string userName)
        {
            string userName_current = GetUserName();
            if (userName_current.ToUpper() == userName.ToUpper())
            {
                return "";
            }
            else
            {
                return userName_current.ToUpper() + " = " + userName.ToUpper();
            }
        }

        // get user assigned site data for given user
        public UserPermissionClass GetUserPermissionInfo(string userName)
        {
            List<SiteClass> siteList = new List<SiteClass>();
            List<DistrictClass> districtList = new List<DistrictClass>();
            List<commentLookupClass> commentLookupList = new List<commentLookupClass>();
            string editorAll = "";
            string defaultSiteAll = "";
            //string email = "";

            // retrieve data from sql server table NCA.v_LU_COP_AppUsers by filtering userName
            // create siteList, districtList, editor and defaultSite
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();


                    string sqlcomm = "Select username, site_id, editor, defaultSite, short_name, full_name, district, districtName From NCA.v_LU_COP_AppUsers Where UPPER(username)=@userName ORDER BY site_id";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    comm.Parameters.AddWithValue("@userName", userName.ToUpper());
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    List<string> siteDataList = new List<string>();

                    foreach (DataRow row in dt.Rows)
                    {
                        string user = row[0].ToString();
                        string site_id = row[1].ToString();
                        string editor = row[2].ToString();
                        string defaultSite = row[3].ToString();
                        string site_shortname = row[4].ToString();
                        string site_fullname = row[5].ToString();
                        string district = row[6].ToString();
                        string districtName = row[7].ToString();
                        //email = row[8].ToString();

                        if (editor != "") editorAll = editor;
                        if (defaultSite != "") defaultSiteAll = defaultSite;

                        // district
                        if(districtList.Count <= 0)
                        {
                            DistrictClass districtData = new DistrictClass()
                            {
                                district = district,
                                districtName = districtName
                            };
                            districtList.Add(districtData);
                        }
                        else
                        {
                            DistrictClass districtData = new DistrictClass()
                            {
                                district = district,
                                districtName = districtName
                            };
                            if (!districtList.Contains(districtData))
                            {
                                districtList.Add(districtData);
                            }
                        }
                        SiteClass siteData = new SiteClass()
                        {
                            site_id = site_id,
                            siteName = site_fullname,
                            district = district,
                            districtName = districtName,
                            editor = editor
                        };
                        siteList.Add(siteData);
                    }

                    // comment lookup
                    sqlcomm = "SELECT ErrorType, DropdownText, ResolveType FROM NCA.ResolveLookup";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string errorTyper = row[0].ToString();
                        string dropdownText = row[1].ToString();
                        string resolveType = row[2].ToString();

                        commentLookupClass commentLookupData = new commentLookupClass
                        {
                            errorType = errorTyper,
                            dropdownText = dropdownText,
                            resolveType = resolveType
                        };
                        commentLookupList.Add(commentLookupData);
                    }

                }

                // crdate json return data
                UserPermissionClass UserPermissionData = new UserPermissionClass()
                {
                    status = "OK",
                    editor = editorAll,
                    defaultSite = defaultSiteAll,
                    siteList = siteList,
                    districtList = districtList,
                    commentLookupList = commentLookupList
                };
                return UserPermissionData;
            }
            catch (Exception e)
            {
                UserPermissionClass UserPermissionData = new UserPermissionClass()
                {
                    status = "ERROR - " + e.Message
                };
                return UserPermissionData;
            }
        }

        // get site name from site_id
        public string GetSiteName(string site_id)
        {
            string site_name = "";
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();

                string sqlcomm = "Select site_id, full_name From nca.site_pt Where SITE_ID=@site_id";
                SqlCommand comm = new SqlCommand(sqlcomm, conn);
                comm.Parameters.AddWithValue("@site_id", site_id);
                SqlDataReader dr = null;
                dr = comm.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);

                foreach (DataRow row in dt.Rows)
                {
                    site_name = row[1].ToString();
                }
            }
            return site_name;
        }

    }
}
