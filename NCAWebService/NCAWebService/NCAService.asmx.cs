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
using iTextSharp.text.pdf;
using iTextSharp.text;
//
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// user
using System.Security.Principal;
using System.Threading;

// mail
using System.Net.Mail;
using System.Net.Mime;
using System.ComponentModel;

//
//using System.Threading.Tasks;


// zip
using System.IO.Compression;

namespace NCAWebService
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ncaWebService : System.Web.Services.WebService
    {
        // db connections
        private static string conStr = ConfigurationManager.ConnectionStrings["ncaDbConnectionString"].ConnectionString;

        // photo and report folders
        private static string photoFolder = ConfigurationManager.AppSettings.Get("photoFolder");
        private static string photoFolderUrl = ConfigurationManager.AppSettings.Get("photoFolderUrl");
        private static string reportFolder = ConfigurationManager.AppSettings.Get("reportFolder");

        // urls
        private static string reportFolderUrl = ConfigurationManager.AppSettings.Get("reportFolderUrl");
        private static string mapUrl = ConfigurationManager.AppSettings.Get("mapUrl");
        private static string mapSectionUrl = ConfigurationManager.AppSettings.Get("mapSectionUrl");
        private static string graveSiteMarkerUrl = ConfigurationManager.AppSettings.Get("graveSiteMarkerUrl");
        private static string graveSitePolyUrl = ConfigurationManager.AppSettings.Get("graveSitePolyUrl");
        private static string dbSchema = ConfigurationManager.AppSettings.Get("dbSchema");

        // itext staff
        public iTextSharp.text.Font pFont_header3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public iTextSharp.text.Font pFont3_italic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
        public iTextSharp.text.Font pFont3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
        public iTextSharp.text.Font pFont3_normal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public iTextSharp.text.Font pFont20 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 20, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public iTextSharp.text.Font pFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 13, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public iTextSharp.text.Font pFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 13, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public iTextSharp.text.Font pFontBold = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 13, iTextSharp.text.Font.NORMAL, BaseColor.BLUE);
        public iTextSharp.text.Font pFont14 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        public iTextSharp.text.Font pFont14Italic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.ITALIC, BaseColor.BLUE);
        public iTextSharp.text.Font pFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        public BaseFont helvFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);

        private List<decedentClass> decedentList = new List<decedentClass>();

        public string testStr = "";

        public struct processStatusClass
        {
            public string status;
        }

        public struct dataListClass
        {
            public List<string> gravesiteMarkerList;
            public List<string> gravesitePolyList;
            public List<string> gravesiteList;
        }

        public struct userinfoClass
        {
            public string userName;
            public string site_id;
            public string email;
            public string mapbook;
            public string editor;
        }
        public struct userinfoListClass
        {
            public string status;
            public List<userinfoClass> userinfoList;
        }

        public struct attributeDataClass
        {
            public string status;
            public string objectid_gravesite_poly;
            public string gravesite_status;
        }

        public struct intermentClass
        {
            public string site_id;
            public string gravesite_id;
            public string name;
            public string decedent_id;
        }

        public struct intermentsSummaryByYearClass
        {
            public string month;
            public string count_inground_cremains;
            public string count_columbarium;
            public string count_caskets_rates;
        }

        public struct intermentPhotoClass
        {
            public string gravesite_id;
            public List<string> photoList;
        }

        public struct intermentDataListClass
        {
            public string status;
            public List<string> siteList;
            public List<intermentClass> intermentList;
        }


        public struct nameClass
        {
            public string status;
            public List<string> firstNameList;
            public List<string> lastNameList;
        }

        public struct countClass
        {
            public string status;
            public string type;
            public string value;
            public List<sectionOutClass> sectionData;
        }

        public struct pdfClass
        {
            public string status;
            public string value;
            public string processTime;
        }

        public struct userClass
        {
            public string status;
            public string value;
            public string authenticationType;
        }

        public struct siteListClass
        {
            public string status;
            public List<siteClass> siteData;
        }

        public struct sectionSiteListClass
        {
            public string status;
            public List<sectionSiteClass> sectionSiteData;
        }

        public struct gravesiteListClass
        {
            public string status;
            public List<gravesiteClass> gravesiteData;
        }

        public struct sectionClass
        {
            public string section;
            public string type;
            public string size;
            public string status;
            public string value;
        }

        public struct sectionOutClass
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

        public struct gravePolySiteClass
        {
            public string site_id;
            public int marker_active;
            public int interment_active;
        }

        public struct siteClass
        {
            public string site_id;
            public string name;
            public string site_url;
            public string databook_url;
            public int numMarkers;
            public int interment;
        }

        public struct sectionSiteClass
        {
            public string section;
        }

        public struct gravesiteClass
        {
            public string gravesite;
        }

        public struct decedentClass
        {
            public string gravesite;

            public string last_name;
            public string first_name;
            public string middle_name;
            public string decedent_id;
        }
        
        public struct gravesiteIntermentClass
        {
            public string geom;
            public string attribute;
            public string status;
        }


        //
        // methods
        [WebMethod(Description = "create a Mapbook for GraveSite")]
        public string createMapBookReport(string site_id, string section, string sectionPolygonExtent, string email)
        {
            //
            DateTime startTime = DateTime.Now;

            //
            string pdfFile = createGravesiteReport(site_id, section, "", sectionPolygonExtent);
            string processStatus = "";
            if (pdfFile == "")
            {
                processStatus = "Error: create mapbook repoprt.";
            }
            if (pdfFile.IndexOf("Error") >= 0)
            {
                processStatus = pdfFile;
                pdfFile = "";
            }


            // create zip file
            if (File.Exists(pdfFile))
            {
                string fileName = Path.GetFileName(pdfFile);
                string zipFileName = fileName.Substring(0, fileName.Length - 3) + "zip";
                string filePath_tmp = reportFolder + @"\zip\" + fileName;
                string zipFile = reportFolder + @"\" + zipFileName;
                string zipFileUrl = reportFolderUrl + @"/" + zipFileName;
                string zipFolder = reportFolder + @"\zip";

                // delete all files in the zip folder
                System.IO.DirectoryInfo dirInfo = new DirectoryInfo(zipFolder);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
                
                // copy file to zip file folder
                System.IO.File.Copy(pdfFile, filePath_tmp, true);

                // create zip file
                ZipFile.CreateFromDirectory(zipFolder, zipFile);

                // delete all files in the zip folder
                dirInfo = new DirectoryInfo(zipFolder);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }

                File.Delete(pdfFile);

                pdfFile = zipFileUrl;
            }

            //
            DateTime endTime = DateTime.Now;
            TimeSpan timeDiff = endTime - startTime;

            pdfClass pdfData;
            List<pdfClass> pdfList = new List<pdfClass>();

            string emailStatus = sendEMail("email", site_id, section, email, pdfFile, processStatus);
            pdfData = new pdfClass();
            pdfData.status = processStatus + " " + emailStatus;
            pdfData.value = pdfFile;
            pdfData.processTime = "process time: " + timeDiff.Seconds.ToString() + " in seconds";
            pdfList.Add(pdfData);

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(pdfList);

            
            return json;



        }

        [WebMethod(Description = "get user name")]
        public string userName()
        {
            string userName_current = "";
            string authenticationType = "";

            WindowsPrincipal p = Thread.CurrentPrincipal as WindowsPrincipal;
            
            userName_current = p.Identity.Name;
            authenticationType = p.Identity.AuthenticationType;


            GenericPrincipal genericPrincipal = GetGenericPrincipal();
            GenericIdentity principalIdentity = (GenericIdentity)genericPrincipal.Identity;
            if (principalIdentity.IsAuthenticated)
            {
                //userName_current = principalIdentity.Name;
                //authenticationType = principalIdentity.AuthenticationType;
            }


            userClass userData = new userClass();
            userData.status = "OK";
            userData.value = userName_current;
            userData.authenticationType = authenticationType;

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(userData);

            return json;

        }

        [WebMethod(Description = "store bookmarks")]
        public string storeBookmarks(string user_name, string bookmarks_str)
        {
            string status = "OK";
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // delete existing bookmarks from username
                    string sqlcomm = "Delete From " + dbSchema + "LU_NCA_Bookmarks Where username = '" + user_name + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    comm.ExecuteNonQuery();


                    // add bookmarks from username
                    if (bookmarks_str != "")
                    {
                        if (bookmarks_str != "")
                        {
                            string[] splitStr = bookmarks_str.Split('|');
                            for (int i = 0; i < splitStr.Length; i++)
                            {
                                string bookmark_str = splitStr[i];
                                Console.WriteLine(bookmark_str);

                                sqlcomm = "INSERT LU_NCA_Bookmarks (username, bookmark) VALUES (@username,@bookmark)";

                                comm = new SqlCommand(sqlcomm, conn);
                                comm.Parameters.AddWithValue("@username", user_name);
                                comm.Parameters.AddWithValue("@bookmark", bookmark_str);
                                comm.ExecuteNonQuery();
                            }
                        }

                    }
                }

            }
            catch
            {
                status = "Error";
            }

            userClass userData = new userClass();
            userData.status = status;
            userData.value = "";

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(userData);

            return json;

        }

        [WebMethod(Description = "get bookmarks")]
        public string getBookmarks(string user_name)
        {
            List<string> bookmarkList = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // delete existing bookmarks from username
                    string sqlcomm = "Select bookmark From " + dbSchema + "LU_NCA_Bookmarks Where username = '" + user_name + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        bookmarkList.Add(row[0].ToString());
                    }
                }

            }
            catch
            {
                bookmarkList = new List<string>();
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(bookmarkList);

            return json;

        }

        [WebMethod(Description = "Gravesite Report by Section or Cemetery Site")]
        public string gravesiteReport(string site_id, string section, string gravesite_id, string sectionPolygonExtent)
        {
            //
            DateTime startTime = DateTime.Now;

            //
            string pdfFile = createGravesiteReport(site_id, section, gravesite_id, sectionPolygonExtent);
            string processStatus = "Success";
            if (pdfFile == "")
            {
                processStatus = "Error: create gravesite report pdf file.";
            }
            if (pdfFile.IndexOf("Error") >= 0)
            {
                processStatus = pdfFile;
                pdfFile = "";
            }

            //
            DateTime endTime = DateTime.Now;
            TimeSpan timeDiff = endTime - startTime;

            pdfClass pdfData;
            List<pdfClass> pdfList = new List<pdfClass>();

            pdfData = new pdfClass();
            pdfData.status = processStatus;
            pdfData.value = pdfFile; // +" || " + testStr;
            pdfData.processTime = timeDiff.Seconds.ToString();
            pdfList.Add(pdfData);

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(pdfList);


            return json;
        }

        [WebMethod(Description = "Cemetery Site Summary Report")]
        public string siteSummary(string site_id, bool sectionStatus)
        {
            bool processStatus = false;

            try
            {

                countClass countData;
                List<countClass> countList = new List<countClass>();
                 List<countClass> countListSub = new List<countClass>();

                sectionClass sectionData;
                List<sectionClass> sectionList = new List<sectionClass>();
                List<string> sectionUniqueList = new List<string>();

                sectionOutClass sectionOutData;
                List<sectionOutClass> sectionOutList = new List<sectionOutClass>();


                countData = new countClass();
                countData.status = "site_id";
                countData.value = getSiteName(site_id);
                countList.Add(countData);

                countData = new countClass();
                countData.status = "section_status";
                countData.value = sectionStatus.ToString();
                countList.Add(countData);

                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // total count
                    string sqlcomm = "Select COUNT(*) From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        countData = new countClass();
                        countData.status = "total";
                        countData.value = row[0].ToString();
                        countList.Add(countData);
                    }

                    // total count
                    sqlcomm = "Select COUNT(*), GRAVESITE_TYPE From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' GROUP BY GRAVESITE_TYPE ORDER BY GRAVESITE_TYPE";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        countData = new countClass();
                        countData.status = "total";
                        countData.value = row[0].ToString();
                        countData.type = row[1].ToString();
                        countList.Add(countData);
                    }

                    // total count by status
                    sqlcomm = "Select COUNT(*), GRAVESITE_STATUS From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' GROUP BY GRAVESITE_STATUS";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        countData = new countClass();
                        countData.status = row[1].ToString();
                        countData.value = row[0].ToString();
                        countListSub.Add(countData);
                    }

                    // total count by status, type
                    sqlcomm = "Select COUNT(*), GRAVESITE_STATUS, GRAVESITE_TYPE From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' GROUP BY GRAVESITE_STATUS, GRAVESITE_TYPE ORDER BY GRAVESITE_STATUS, GRAVESITE_TYPE";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    string prev_status = "";
                    string current_status = "";

                    foreach (DataRow row in dt.Rows)
                    {
                        current_status = row[1].ToString();
                        //
                        countData = new countClass();
                        countData.status = row[1].ToString();
                        countData.type = row[2].ToString();
                        countData.value = row[0].ToString();
                        //
                        if (prev_status != current_status)
                        {
                            string current_count = "-9999";
                            foreach (countClass item in countListSub)
                            {
                                if (item.status == current_status)
                                {
                                    current_count = item.value;
                                    break;
                                }
                            }
                            //
                            countClass countData_sub = new countClass();
                            countData_sub.status = current_status;
                            countData_sub.type = "";
                            countData_sub.value = current_count;
                            countList.Add(countData_sub);

                        }
                        //
                        countList.Add(countData);
                        //
                        prev_status = current_status;
                    }

                    // section
                    if (sectionStatus)
                    {
                        sqlcomm = "Select COUNT(*), SECTION, GRAVESITE_TYPE, GRAVESITE_SIZE, GRAVESITE_STATUS From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' GROUP BY SECTION, GRAVESITE_TYPE, GRAVESITE_SIZE, GRAVESITE_STATUS Order by Section";
                        comm = new SqlCommand(sqlcomm, conn);
                        dr = null;
                        dr = comm.ExecuteReader();
                        dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            if (!sectionUniqueList.Exists(x => x == row[1].ToString() + "|" + row[2].ToString() + "|" + row[3].ToString()))
                            {
                                sectionUniqueList.Add(row[1].ToString() + "|" + row[2].ToString() + "|" + row[3].ToString());
                            }

                            //
                            sectionData = new sectionClass();
                            sectionData.section = row[1].ToString();
                            sectionData.type = row[2].ToString();
                            sectionData.size = row[3].ToString();
                            sectionData.status = row[4].ToString();
                            sectionData.value = row[0].ToString();
                            sectionList.Add(sectionData);
                        }
                    }

                    processStatus = true;
                }

                // section
                if (sectionStatus)
                {
                    foreach (string key in sectionUniqueList)
                    {
                        string[] splitStr = key.Split('|');
                        string section = splitStr[0];
                        string type = splitStr[1];
                        string size = splitStr[2];

                        List<sectionClass> sectionListSub = sectionList.Where(x => x.section == section && x.type == type && x.size == size).ToList();
                        int Reserved_count = 0;
                        int Occupied_count = 0;
                        int Obstructed_count = 0;
                        int Available_count = 0;
                        int null_count = 0;

                        foreach (sectionClass sectionDataSub in sectionListSub)
                        {
                            if (sectionDataSub.status == "Reserved") Reserved_count += Convert.ToInt16(sectionDataSub.value);
                            if (sectionDataSub.status == "Occupied") Occupied_count += Convert.ToInt16(sectionDataSub.value);
                            if (sectionDataSub.status == "Obstructed") Obstructed_count += Convert.ToInt16(sectionDataSub.value);
                            if (sectionDataSub.status == "Available") Available_count += Convert.ToInt16(sectionDataSub.value);
                            if (sectionDataSub.status == "") null_count += Convert.ToInt16(sectionDataSub.value);
                        }
                        //
                        sectionOutData = new sectionOutClass();
                        sectionOutData.section = section;
                        sectionOutData.type = type;
                        sectionOutData.size = size;
                        sectionOutData.Obstructed_count = Obstructed_count.ToString();
                        sectionOutData.Reserved_count = Reserved_count.ToString();
                        sectionOutData.Occupied_count = Occupied_count.ToString();
                        sectionOutData.Available_count = Available_count.ToString();
                        sectionOutData.null_count = null_count.ToString();
                        sectionOutData.total_count = (Obstructed_count + Reserved_count + Occupied_count + Available_count + null_count).ToString();
                        //
                        sectionOutList.Add(sectionOutData);
                    }

                    countData = new countClass();
                    countData.status = "section";
                    countData.sectionData = sectionOutList;
                    countList.Add(countData);
                }

                countData = new countClass();
                countData.status = "process_status";
                countData.value = processStatus.ToString();
                countList.Add(countData);


                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(countList);

                return json;
            }
            catch (Exception e)
            {
                processStatus = false;
                countClass countData;
                List<countClass> countList = new List<countClass>();

                countData = new countClass();
                countData.status = "site_id";
                countData.value = site_id;
                countList.Add(countData);

                countData = new countClass();
                countData.status = "process_status";
                countData.value = processStatus.ToString();
                countList.Add(countData);

                countData = new countClass();
                countData.status = "error";
                countData.value = e.ToString();
                countList.Add(countData);

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(countList);

                return json;
            }
        }

        [WebMethod(Description = "Get Site List")]
        public string getSiteList()
        {
            List<siteClass> siteList = new List<siteClass>();
            siteClass siteData;
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // get active siteList from GraveSite_poly_evw
                    List<string> siteList_marker = new List<string>();
                    string sqlcomm_gravesite_marker = "Select DISTINCT site_id From " + dbSchema + "GraveSite_Marker_Pt_evw Where ACTIVE = 1";
                    SqlCommand comm_gravesite_marker = new SqlCommand(sqlcomm_gravesite_marker, conn);
                    SqlDataReader dr_gravesite_marker = comm_gravesite_marker.ExecuteReader();
                    DataTable dt_gravesite_marker = new DataTable();
                    dt_gravesite_marker.Load(dr_gravesite_marker);

                    foreach (DataRow row in dt_gravesite_marker.Rows)
                    {
                        string site_id = row[0].ToString();
                        siteList_marker.Add(site_id);
                    }

                    // get active siteList from GraveSite_Poly_evw
                    sqlcomm_gravesite_marker = "Select DISTINCT site_id From " + dbSchema + "GraveSite_Poly_evw Where ACTIVE = 1";
                    comm_gravesite_marker = new SqlCommand(sqlcomm_gravesite_marker, conn);
                    dr_gravesite_marker = comm_gravesite_marker.ExecuteReader();
                    dt_gravesite_marker = new DataTable();
                    dt_gravesite_marker.Load(dr_gravesite_marker);

                    foreach (DataRow row in dt_gravesite_marker.Rows)
                    {
                        string site_id = row[0].ToString();
                        if (!siteList_marker.Exists(x => string.Equals(x, site_id))) siteList_marker.Add(site_id);
                    }

                    // get active siteList from INTERMENT_INFO
                    List<string> siteList_interment = new List<string>();
                    string sqlcomm_interment = "Select DISTINCT site_id From " + dbSchema + "INTERMENT_INFO Where ACTIVE = 1";
                    SqlCommand comm_interment = new SqlCommand(sqlcomm_interment, conn);
                    SqlDataReader dr_interment = comm_interment.ExecuteReader();
                    DataTable dt_interment = new DataTable();
                    dt_interment.Load(dr_interment);

                    foreach (DataRow row in dt_interment.Rows)
                    {
                        string site_id = row[0].ToString();
                        siteList_interment.Add(site_id);
                    }


                    // get siteList from site_pt table
                    string sqlcomm_sitelist = "Select site_pt_evw.site_id, site_pt_evw.short_name, LU_NCA_SITELINKS.SITE_URL, LU_NCA_SITELINKS.DATABOOK_URL From " + dbSchema + "site_pt_evw LEFT JOIN " + dbSchema + "LU_NCA_SITELINKS ON site_pt_evw.site_id = LU_NCA_SITELINKS.site_id Where site_pt_evw.ACTIVE = 1 And site_pt_evw.site_type <> 'Office' and site_pt_evw.SITE_STATUS <> 'Proposed' And site_pt_evw.site_owner = 'NCA' Order By site_pt_evw.short_name, site_pt_evw.site_id";

                    SqlCommand comm_sitelist = new SqlCommand(sqlcomm_sitelist, conn);
                    SqlDataReader dr_sitelist = null;
                    dr_sitelist = comm_sitelist.ExecuteReader();
                    DataTable dt_sitelist = new DataTable();
                    dt_sitelist.Load(dr_sitelist);

                    foreach (DataRow row in dt_sitelist.Rows)
                    {
                        //
                        string site_id = row[0].ToString();
                        string site_name = row[1].ToString();
                        string site_url = row[2].ToString();
                        string databook_url = row[3].ToString();

                        //
                        siteData = new siteClass();
                        siteData.site_id = site_id;
                        siteData.name = site_name;
                        siteData.site_url = site_url;
                        siteData.databook_url = databook_url;
                        siteData.numMarkers = 0;
                        siteData.interment = 0;
                        
                        //
                        if (siteList_marker.Exists(x => string.Equals(x, site_id))) siteData.numMarkers = 1;
                        if (siteList_interment.Exists(x => string.Equals(x, site_id))) siteData.interment = 1;

                        //
                        siteList.Add(siteData);
                    }
                    
                    /*

                    // get siteList from interment table
                    List<string> siteList_interment = new List<string>();
                    string sqlcomm_interment = "Select DISTINCT site_id From " + dbSchema + "INTERMENT_INFO";

                    SqlCommand comm_interment = new SqlCommand(sqlcomm_interment, conn);
                    SqlDataReader dr_interment = null;
                    dr_interment = comm_interment.ExecuteReader();
                    DataTable dt_interment = new DataTable();
                    dt_interment.Load(dr_interment);

                    foreach (DataRow row in dt_interment.Rows)
                    {
                        siteList_interment.Add(row[0].ToString());
                    }

                    // get siteList from gravesite_marker table
                    List<string> siteList_gravesitemarkers = new List<string>();
                    string sqlcomm_gravesitemarkers = "Select DISTINCT site_id From " + dbSchema + "GraveSite_Marker_Pt_evw";

                    SqlCommand comm_gravesitemarkers = new SqlCommand(sqlcomm_gravesitemarkers, conn);
                    SqlDataReader dr_gravesitemarkers = null;
                    dr_gravesitemarkers = comm_gravesitemarkers.ExecuteReader();
                    DataTable dt_gravesitemarkers = new DataTable();
                    dt_gravesitemarkers.Load(dr_gravesitemarkers);

                    foreach (DataRow row in dt_gravesitemarkers.Rows)
                    {
                        siteList_gravesitemarkers.Add(row[0].ToString());
                    }

                    // get siteList from site_pt table
                    string sqlcomm = "Select site_pt_evw.site_id, site_pt_evw.short_name, LU_NCA_SITELINKS.SITE_URL, LU_NCA_SITELINKS.DATABOOK_URL From " + dbSchema + "site_pt_evw LEFT JOIN " + dbSchema + "LU_NCA_SITELINKS ON site_pt_evw.site_id = LU_NCA_SITELINKS.site_id Where site_pt_evw.ACTIVE = 1 And site_pt_evw.site_type <> 'Office' and site_pt_evw.SITE_STATUS <> 'Proposed' And site_pt_evw.site_owner = 'NCA' Order By site_pt_evw.short_name, site_pt_evw.site_id";

                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        //
                        string site_id = row[0].ToString();
                        string site_name = row[1].ToString();
                        string site_url = row[2].ToString();
                        string databook_url = row[3].ToString();

                        //
                        siteData = new siteClass();
                        siteData.site_id = site_id;
                        siteData.name = site_name;
                        siteData.site_url = site_url;
                        siteData.databook_url = databook_url;
                        siteData.numMarkers = 0;
                        siteData.interment = 0;
                        //
                        if (siteList_gravesitemarkers.Exists(x => string.Equals(x, site_id))) siteData.numMarkers = 1;
                        if (siteList_interment.Exists(x => string.Equals(x, site_id))) siteData.interment = 1;
                        //
                        siteList.Add(siteData);
                    }
                    **/
                }

                // sorting
                List<siteClass> sortedList = siteList.OrderBy(c => c.name).ThenBy(c => c.site_id).ToList();

                siteListClass siteListData = new siteListClass();
                siteListData.status = "OK";
                siteListData.siteData = sortedList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(siteListData);

                return json;
            }
            catch (Exception e)
            {
                string errorMsg = e.Message;

                siteListClass siteListData = new siteListClass();
                siteListData.status = "Error: " + errorMsg;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(siteListData);

                return json;
            }
        }

        [WebMethod(Description = "Get Section List from site")]
        public string getSectionListFromSite(string site_id)
        {
            bool processStatus = false;
            List<sectionSiteClass> sectionList = new List<sectionSiteClass>();
            sectionSiteClass sectionData;
            try
            {


                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // section list from gravesite_poly
                    List<string> sectionList_marker = new List<string>();
                    string sqlcomm = "Select DISTINCT section From " + dbSchema + "SECTION_POLY_evw Where Active = '1' And SITE_ID = '" + site_id + "' Order By section";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string section = row[0].ToString();
                        sectionList_marker.Add(section);
                    }

                    // section list from gravesite_marker
                    sqlcomm = "Select DISTINCT section From " + dbSchema + "GraveSite_Marker_Pt_evw Where Active = '1' And  SITE_ID = '" + site_id + "' Order By section";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string section = row[0].ToString();
                        if (!sectionList_marker.Exists(x => string.Equals(x, section)))
                        {
                            sectionList_marker.Add(section);
                        }
                    }

                    // section list from interment
                    sqlcomm = "Select DISTINCT section From " + dbSchema + "INTERMENT_PT_evw Where Active = '1' And  SITE_ID = '" + site_id + "' Order By section";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string section = row[0].ToString();
                        if (!sectionList_marker.Exists(x => string.Equals(x, section)))
                        {
                            sectionList_marker.Add(section);
                        }
                    }

                    // sort
                    sectionList_marker.Sort();

                    //
                    foreach (string section in sectionList_marker)
                    {
                        sectionData = new sectionSiteClass();
                        sectionData.section = section;
                        //
                        sectionList.Add(sectionData);

                    }
                }


                sectionSiteListClass sectionSiteList = new sectionSiteListClass();
                sectionSiteList.status = "OK";
                sectionSiteList.sectionSiteData = sectionList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(sectionSiteList);

                return json;
            }
            catch (Exception e)
            {
                processStatus = false;
                countClass countData;
                List<countClass> countList = new List<countClass>();

                countData = new countClass();
                countData.status = "site_id";
                countData.value = site_id;
                countList.Add(countData);

                countData = new countClass();
                countData.status = "process_status";
                countData.value = processStatus.ToString();
                countList.Add(countData);

                countData = new countClass();
                countData.status = "error";
                countData.value = e.ToString();
                countList.Add(countData);

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(countList);

                return json;
            }
        }

        [WebMethod(Description = "Get GraveSite List from section")]
        public string getGraveSiteListFromSection(string site_id, string section)
        {
            bool processStatus = false;
            List<gravesiteClass> gravesiteList = new List<gravesiteClass>();
            gravesiteClass gravesiteData;

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // get gravesite from graveSite_Poly 
                    string sqlcomm = "Select DISTINCT gravesite_id From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' And section = '" + section + "' Order By gravesite_id";
                    if (section == "")
                    {
                        sqlcomm = "Select DISTINCT gravesite_id From " + dbSchema + "GraveSite_Poly_evw Where SITE_ID = '" + site_id + "' Order By gravesite_id";
                    }
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        gravesiteData = new gravesiteClass();
                        gravesiteData.gravesite = row[0].ToString();
                        gravesiteList.Add(gravesiteData);
                    }

                    // get gravesite from graveSite_marker
                    sqlcomm = "Select DISTINCT gravemarker_id From " + dbSchema + "GraveSite_Marker_Pt_evw Where SITE_ID = '" + site_id + "' And section = '" + section + "' And Active = '1' Order By gravemarker_id";
                    if (section == "")
                    {
                        sqlcomm = "Select DISTINCT gravemarker_id From " + dbSchema + "GraveSite_Marker_Pt_evw Where SITE_ID = '" + site_id + "' And  Active = '1' Order By gravemarker_id";
                    }
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!gravesiteList.Exists(x => x.gravesite == row[0].ToString()))
                        {
                            gravesiteData = new gravesiteClass();
                            gravesiteData.gravesite = row[0].ToString();
                            gravesiteList.Add(gravesiteData);
                        }
                    }

                    // get gravesite from internment
                    sqlcomm = "Select DISTINCT GRAVESITE_ID From " + dbSchema + "INTERMENT_PT_evw Where SITE_ID = '" + site_id + "' And section = '" + section + "' And Active = '1' Order By GRAVESITE_ID";
                    if (section == "")
                    {
                        sqlcomm = "Select DISTINCT GRAVESITE_ID From " + dbSchema + "INTERMENT_PT_evw Where SITE_ID = '" + site_id + "' And Active = '1' Order By GRAVESITE_ID";
                    }
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (!gravesiteList.Exists(x => x.gravesite == row[0].ToString()))
                        {
                            gravesiteData = new gravesiteClass();
                            gravesiteData.gravesite = row[0].ToString();
                            gravesiteList.Add(gravesiteData);
                        }
                    }

                }

                List<gravesiteClass> gravesiteList_sort = gravesiteList.OrderBy(x => x.gravesite).ToList();
                gravesiteListClass gravesiteList2 = new gravesiteListClass();
                gravesiteList2.status = "OK";
                //gravesiteList2.gravesiteData = gravesiteList_sort;
                gravesiteList2.gravesiteData = gravesiteList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(gravesiteList2);

                return json;
            }
            catch (Exception e)
            {
                processStatus = false;
                countClass countData;
                List<countClass> countList = new List<countClass>();

                countData = new countClass();
                countData.status = "site_id";
                countData.value = site_id;
                countList.Add(countData);

                countData = new countClass();
                countData.status = "process_status";
                countData.value = processStatus.ToString();
                countList.Add(countData);

                countData = new countClass();
                countData.status = "error";
                countData.value = e.ToString();
                countList.Add(countData);

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(countList);

                return json;
            }
        }

        [WebMethod(Description = "Get list of first and last name")]
        public string getNameList(string nameType, string site_id)
        {
            List<string> firstNameList = new List<string>();
            List<string> lastNameList = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // first_name
                    if (nameType == "FIRSTNAME")
                    {
                        string sqlcomm = "Select DISTINCT first_name From " + dbSchema + "Interment_info Where first_name is not Null And site_id = '" + site_id + "' Order By first_name";
                        if (site_id == "")
                        {
                            sqlcomm = "Select DISTINCT first_name From " + dbSchema + "Interment_info Where first_name is not Null Order By first_name";
                        }

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            firstNameList.Add(row[0].ToString());
                        }
                    }

                    // last_name
                    if (nameType == "LASTNAME")
                    {
                        string sqlcomm = "Select DISTINCT last_name From " + dbSchema + "Interment_info Where last_name is not Null And site_id = '" + site_id + "' Order By last_name";
                        if (site_id == "")
                        {
                            sqlcomm = "Select DISTINCT last_name From " + dbSchema + "Interment_info Where last_name is not Null Order By last_name";
                        }

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        SqlDataReader dr = null;
                        dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            lastNameList.Add(row[0].ToString());
                        }
                    }
                }


                nameClass nameData = new nameClass();
                nameData.status = "OK";
                nameData.firstNameList = firstNameList;
                nameData.lastNameList = lastNameList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(nameData);

                return json;
            }
            catch (Exception e)
            {
                nameClass nameData = new nameClass();
                nameData.status = "Error";
                nameData.firstNameList = firstNameList;
                nameData.lastNameList = lastNameList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(nameData);

                testStr = "Error: " + e.Message;

                return json;
            }
        }

        [WebMethod(Description = "Get Interment photo by gravesite_id")]
        public string getGravesiteAndIntermentInfo(string gravesite_id)
        {
            string url_gravemarker = graveSiteMarkerUrl;
            string url_gravesite_poly = graveSitePolyUrl;

            string interment_img1 = "";
            string interment_img2 = "";
            string status = "Point";


            string sqlcomm = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    sqlcomm = "Select gravesite_id, IMG1, IMG2 From " + dbSchema + "INTERMENT_PT_evw Where GRAVESITE_ID='" + gravesite_id + "'";

                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[1] != null) interment_img1 = row[1].ToString();
                        if (row[2] != null) interment_img2 = row[2].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            // feature service
            var url = url_gravemarker + "/query?where=GRAVEMARKER_ID%3D%27" + gravesite_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
            var url_poly = url_gravesite_poly + "/query?where=GRAVESITE_ID%3D%27" + gravesite_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=*&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var jsonStr = readStream.ReadToEnd();

            response.Close();
            readStream.Close();

            // point
            string geom_jason = "";
            string attributes_jason = "";

            JObject jsonObj = JObject.Parse(jsonStr);
            Dictionary<string, object> dictObj = jsonObj.ToObject<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> entry in dictObj)
            {
                if (entry.Key == "features")
                {
                    IEnumerable featList = entry.Value as IEnumerable;
                    foreach (object feat in featList)
                    {
                        JObject jsonObj_feat = JObject.Parse(feat.ToString());
                        Dictionary<string, object> dictObj_feat = jsonObj_feat.ToObject<Dictionary<string, object>>();

                        // geometry
                        geom_jason = dictObj_feat["geometry"].ToString();

                        // attributes
                        attributes_jason = dictObj_feat["attributes"].ToString();
                        //
                        JObject jsonObj_new = JObject.Parse(attributes_jason);
                        jsonObj_new["IMG1"] = interment_img1;
                        jsonObj_new["IMG2"] = interment_img2;

                        attributes_jason = jsonObj_new.ToString();

                        //
                        break;
                    }
                }
            }

            // polygon
            if (geom_jason == "")
            {
                status = "Poly";
                request = (HttpWebRequest)WebRequest.Create(url_poly);
                response = (HttpWebResponse)request.GetResponse();
                receiveStream = response.GetResponseStream();
                readStream = new StreamReader(receiveStream, Encoding.UTF8);
                jsonStr = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                jsonObj = JObject.Parse(jsonStr);
                dictObj = jsonObj.ToObject<Dictionary<string, object>>();
                foreach (KeyValuePair<string, object> entry in dictObj)
                {
                    if (entry.Key == "features")
                    {
                        IEnumerable featList = entry.Value as IEnumerable;
                        foreach (object feat in featList)
                        {
                            JObject jsonObj_feat = JObject.Parse(feat.ToString());
                            Dictionary<string, object> dictObj_feat = jsonObj_feat.ToObject<Dictionary<string, object>>();

                            // geometry
                            geom_jason = dictObj_feat["geometry"].ToString();

                            // attributes
                            attributes_jason = dictObj_feat["attributes"].ToString();
                            //
                            JObject jsonObj_new = JObject.Parse(attributes_jason);
                            jsonObj_new["IMG1"] = interment_img1;
                            jsonObj_new["IMG2"] = interment_img2;

                            attributes_jason = jsonObj_new.ToString();

                            //
                            break;
                        }
                    }
                }
            }

            //
            gravesiteIntermentClass gravesiteIntermentData = new gravesiteIntermentClass();
            gravesiteIntermentData.geom = geom_jason;
            gravesiteIntermentData.attribute = attributes_jason;
            gravesiteIntermentData.status = status;

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(gravesiteIntermentData);

            return json;
        }

        //
        [WebMethod(Description = "Get gracesite_marker photos by gravesite_id")]
        public string getGravesiteMarkerPhotoInfo(string gravesite_id)
        {
            intermentPhotoClass intermentPhotoData = new intermentPhotoClass();
            List<string> photoList = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // string sqlcomm = "Select GRAVEMARKER_ID, FRONT_IMG as IMG1, REAR_IMG as IMG2 From " + dbSchema + "GRAVESITE_MARKER_PT_evw Where Active = '1' And GRAVEMARKER_ID='" + gravesite_id + "'";
                    string sqlcomm = "SELECT t2.[GRAVEMARKER_ID],t2.[FRONT_IMG],t2.[REAR_IMG] FROM " + dbSchema + "GRAVESITE_POLY_evw as t1 LEFT JOIN " + dbSchema + "GRAVESITE_MARKER_PT_evw as t2 ON t1.GRAVEMARKER_ID = t2.GRAVEMARKER_ID WHERE t2.ACTIVE = 1 And t1.GRAVESITE_ID = '" + gravesite_id + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string img1 = "";
                        string img2 = "";

                        if (row[1] != null) img1 = row[1].ToString();
                        if (row[2] != null) img2 = row[2].ToString();
                        if (img1 != "") photoList.Add(img1);
                        if (img2 != "") photoList.Add(img2);
                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            //
            intermentPhotoData = new intermentPhotoClass();
            intermentPhotoData.gravesite_id = gravesite_id;
            intermentPhotoData.photoList = photoList;

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(intermentPhotoData);

            return json;
        }


        //
        [WebMethod(Description = "Get Interment photo by gravesite_id")]
        public string getIntermentPhotoInfo(string gravesite_id)
        {
            intermentPhotoClass intermentPhotoData = new intermentPhotoClass();
            List<string> photoList = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    string sqlcomm = "Select gravesite_id, IMG1, IMG2 From " + dbSchema + "INTERMENT_PT_evw Where  Active = '1' And GRAVESITE_ID='" + gravesite_id + "'";

                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string img1 = "";
                        string img2 = "";

                        if (row[1] != null) img1 = row[1].ToString();
                        if (row[2] != null) img2 = row[2].ToString();
                        if (img1 != "") photoList.Add(img1);
                        if (img2 != "") photoList.Add(img2);
                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            //
            intermentPhotoData = new intermentPhotoClass();
            intermentPhotoData.gravesite_id = gravesite_id;
            intermentPhotoData.photoList = photoList;

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(intermentPhotoData);

            return json;

        }

        [WebMethod(Description = "Get Interment info from first and last name")]
        public string getIntermentInfo(string first_name, string last_name, string site_id)
        {
            List<string> siteList = new List<string>();
            List<intermentClass> intermentList = new List<intermentClass>();
            intermentClass intermentData;
            string sqlcomm = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // first_name and last_name
                    if (first_name != "" && last_name != "")
                    {

                        if (site_id == "")
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where first_name='" + first_name + "' And last_name = '" + last_name + "'";
                        }
                        else
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where first_name='" + first_name + "' And last_name = '" + last_name + "' And site_id = '" + site_id + "'";
                        }

                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            string siteId = "";
                            string gravesite_id = "";
                            string firstName = "";
                            string lastName = "";
                            string name = "";
                            string decedent_id = "";

                            if (row[0] != null) siteId = row[0].ToString();
                            if (row[1] != null) gravesite_id = row[1].ToString();
                            if (row[2] != null) firstName = row[2].ToString();
                            if (row[3] != null) lastName = row[3].ToString();
                            if (row[4] != null) decedent_id = row[4].ToString();

                            if (firstName != "" && lastName != "") name = lastName + ", " + firstName;

                            if (!siteList.Exists(x => x == site_id))
                            {
                                siteList.Add(site_id);
                            }
                            //
                            intermentData = new intermentClass();
                            intermentData.site_id = siteId;
                            intermentData.gravesite_id = gravesite_id;
                            intermentData.name = name;
                            intermentData.decedent_id = decedent_id;
                            intermentList.Add(intermentData);
                        }
                    }
                    // first_name only
                    if (first_name != "" && last_name == "")
                    {
                        if (site_id == "")
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where first_name='" + first_name + "'";
                        }
                        else
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where first_name='" + first_name + "' And site_id = '" + site_id + "'";
                        }
                        
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            string siteId = "";
                            string gravesite_id = "";
                            string firstName = "";
                            string lastName = "";
                            string name = "";
                            string decedent_id = "";

                            if (row[0] != null) siteId = row[0].ToString();
                            if (row[1] != null) gravesite_id = row[1].ToString();
                            if (row[2] != null) firstName = row[2].ToString();
                            if (row[3] != null) lastName = row[3].ToString();
                            if (row[4] != null) decedent_id = row[4].ToString();

                            if (firstName != "" && lastName != "") name = firstName + ", " + lastName;

                            if (!siteList.Exists(x => x == site_id))
                            {
                                siteList.Add(site_id);
                            }
                            //
                            intermentData = new intermentClass();
                            intermentData.site_id = site_id;
                            intermentData.gravesite_id = gravesite_id;
                            intermentData.name = name;
                            intermentData.decedent_id = decedent_id;
                            intermentList.Add(intermentData);
                        }
                    }
                    // last_name only
                    if (first_name == "" && last_name != "")
                    {
                        if (site_id == "")
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where last_name='" + last_name + "'";
                        }
                        else
                        {
                            sqlcomm = "Select site_id, gravesite_id, first_name, last_name, DECEDENT_ID From " + dbSchema + "Interment_info Where last_name='" + last_name + "' And site_id = '" + site_id + "'";
                        }
                        
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        SqlDataReader dr = null;
                        dr = comm.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        foreach (DataRow row in dt.Rows)
                        {
                            string siteId = "";
                            string gravesite_id = "";
                            string firstName = "";
                            string lastName = "";
                            string name = "";
                            string decedent_id = "";

                            if (row[0] != null) siteId = row[0].ToString();
                            if (row[1] != null) gravesite_id = row[1].ToString();
                            if (row[2] != null) firstName = row[2].ToString();
                            if (row[3] != null) lastName = row[3].ToString();
                            if (row[4] != null) decedent_id = row[4].ToString();

                            if (firstName != "" && lastName != "") name = firstName + ", " + lastName;

                            if (!siteList.Exists(x => x == site_id))
                            {
                                siteList.Add(site_id);
                            }
                            //
                            intermentData = new intermentClass();
                            intermentData.site_id = site_id;
                            intermentData.gravesite_id = gravesite_id;
                            intermentData.name = name;
                            intermentData.decedent_id = decedent_id;
                            intermentList.Add(intermentData);
                        }
                    }

                }


                intermentDataListClass intermentListData = new intermentDataListClass();
                intermentListData.status = "OK";
                intermentListData.siteList = siteList;
                intermentListData.intermentList = intermentList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(intermentListData);

                return json;
            }
            catch (Exception e)
            {
                intermentDataListClass intermentListData = new intermentDataListClass();
                intermentListData.status = "Error";
                intermentListData.siteList = siteList;
                intermentListData.intermentList = intermentList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(intermentListData);

                testStr = "Error: " + e.Message;

                return json;
            }
        }

        [WebMethod(Description = "check gravesite")]
        public string checkGraveSite(string gravesite_id)
        {
            bool status = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    string sqlcomm = "Select gravemarker_id From " + dbSchema + "GraveSite_Marker_Pt_evw Where gravemarker_id ='" + gravesite_id + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    if (dt.Rows.Count > 0){
                        status = true;
                    }
                    else
                    {
                        status = false;
                    }

                }

                userinfoListClass userinfoData = new userinfoListClass();
                userinfoData.status = status.ToString();
                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(userinfoData);

                return json;
            }
            catch (Exception e)
            {
                userinfoListClass userinfoData = new userinfoListClass();
                userinfoData.status = "Error";
                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(userinfoData);

                testStr = "Error: " + e.Message;

                return json;
            }
        }

        [WebMethod(Description = "get user permission info")]
        public string getUserPermissionInfo(string userName)
        {
            userinfoClass userinfoData;
            List<userinfoClass> userInfoList = new List<userinfoClass>();

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    string sqlcomm = "Select username, site_id, email, mapbook, editor From " + dbSchema + "LU_NCA_AppUsers Where UPPER(username) ='" + userName.ToUpper() + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string user = row[0].ToString();
                        string site_id = row[1].ToString();
                        string email = row[2].ToString();
                        string mapbook = row[3].ToString();
                        string editor = row[4].ToString();

                        //
                        userinfoData = new userinfoClass();
                        userinfoData.userName = user;
                        userinfoData.site_id = site_id;
                        userinfoData.email = email;
                        userinfoData.mapbook = mapbook;
                        userinfoData.editor = editor;
                        userInfoList.Add(userinfoData);
                    }
                }

                userinfoListClass userinfoListData = new userinfoListClass();
                userinfoListData.status = "OK";
                userinfoListData.userinfoList = userInfoList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(userinfoListData);

                return json;
            }
            catch (Exception e)
            {
                userInfoList = new List<userinfoClass>();
                userinfoListClass userinfoListData = new userinfoListClass();
                userinfoListData.status = "Error";
                userinfoListData.userinfoList = userInfoList;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(userinfoListData);

                testStr = "Error: " + e.Message;

                return json;
            }
        }

        [WebMethod(Description = "get attribute data")]
        public string getAttributeData(string gravesite_id)
        {
            string objectid_gravesite_poly = "";
            string objectid_interment_pt = "";
            string gravesite_status = "";
            string decedent_id = "";
            bool status = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // gravesite_poly table
                    string sqlcomm = "Select objectid, gravesite_status From " + dbSchema + "GraveSite_poly_evw Where gravesite_id ='" + gravesite_id + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {

                        if (row[0] != null) objectid_gravesite_poly = row[0].ToString();
                        if (row[1] != null) gravesite_status = row[1].ToString();
                    }
                    

                    // interment table
                    sqlcomm = "Select objectid, decedent_id From " + dbSchema + "interment_pt_evw Where gravesite_id ='" + gravesite_id + "'";
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[0] != null) objectid_interment_pt = row[0].ToString();
                        if (row[1] != null) decedent_id = row[1].ToString();
                    }
                    //
                    status = true;
                }

                attributeDataClass attributeData = new attributeDataClass();
                attributeData.status = status.ToString();
                attributeData.objectid_gravesite_poly = objectid_gravesite_poly;
                attributeData.gravesite_status = gravesite_status;
                //attributeData.objectid_interment_pt = objectid_interment_pt;
                //attributeData.decedent_id = decedent_id;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(attributeData);

                return json;
            }
            catch
            {
                attributeDataClass attributeData = new attributeDataClass();
                attributeData.status = status.ToString();
                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(attributeData);

                return json;
            }
        }

        [WebMethod(Description = "get attribute data from gravesite")]
        public string getAttributeDataFromGravesite(string gravesite_id)
        {
            string objectid_gravesite_poly = "";
            string gravesite_status = "";
            bool status = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    // gravesite_poly table
                    string sqlcomm = "Select objectid, gravesite_status From " + dbSchema + "GraveSite_poly_evw Where gravesite_id ='" + gravesite_id + "'";
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {

                        if (row[0] != null) objectid_gravesite_poly = row[0].ToString();
                        if (row[1] != null) gravesite_status = row[1].ToString();
                    }
                    status = true;
                }

                attributeDataClass attributeData = new attributeDataClass();
                attributeData.status = status.ToString();
                attributeData.objectid_gravesite_poly = objectid_gravesite_poly;
                attributeData.gravesite_status = gravesite_status;

                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(attributeData);

                return json;
            }
            catch
            {
                attributeDataClass attributeData = new attributeDataClass();
                attributeData.status = status.ToString();
                var jsonSerialiser = new JavaScriptSerializer();
                var json = jsonSerialiser.Serialize(attributeData);

                return json;
            }
        }

        [WebMethod(Description = "Update attributes")]
        public string updateAttributes(string objectid_gravesite_poly, string gravesite_status, string objectid_interment_pt, string decedent_id)
        {
            bool status = false;
            string errorMsg = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    if (objectid_gravesite_poly != "")
                    {
                        string sqlcomm = "UPDATE " + dbSchema + "GraveSite_Poly_evw SET gravesite_status = '" + gravesite_status + "' Where OBJECTID = " + objectid_gravesite_poly;
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.ExecuteNonQuery();
                    }
                    if (objectid_interment_pt != "")
                    {
                        string sqlcomm = "UPDATE " + dbSchema + "interment_pt_evw SET decedent_id = '" + decedent_id + "' Where OBJECTID = " + objectid_interment_pt;
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.ExecuteNonQuery();
                    }

                    status = true;
                }
            }
            catch(Exception ex)
            {
                status = false;
                errorMsg = ex.Message.ToString();
            }
            attributeDataClass attributeData = new attributeDataClass();
            attributeData.status = status.ToString();
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(attributeData);
            //
            return json;
        }

        [WebMethod(Description = "Update Gravesite_poly attributes")]
        public string updateAttributesForGravesitePoly(string objectid_gravesite_poly, string gravesite_status, string userName)
        {
            bool status = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    if (objectid_gravesite_poly != "")
                    {
                        string sqlcomm = "UPDATE " + dbSchema + "GraveSite_Poly_evw SET gravesite_status = '" + gravesite_status + "', last_edited_user = '" + userName + "', last_edited_date = GETDATE() Where OBJECTID = " + objectid_gravesite_poly;
                        SqlCommand comm = new SqlCommand(sqlcomm, conn);
                        comm.ExecuteNonQuery();
                    }
                    status = true;
                }
            }
            catch
            {
                status = false;
            }
            processStatusClass processStatus = new processStatusClass();
            processStatus.status = status.ToString();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(processStatus);
            //
            return json;
        }

        //
        // functions
        private static GenericPrincipal GetGenericPrincipal()
        {
            // Use values from the current WindowsIdentity to construct
            // a set of GenericPrincipal roles.
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            string[] roles = new string[10];
            if (windowsIdentity.IsAuthenticated)
            {
                // Add custom NetworkUser role.
                roles[0] = "NetworkUser";
            }

            if (windowsIdentity.IsGuest)
            {
                // Add custom GuestUser role.
                roles[1] = "GuestUser";
            }

            if (windowsIdentity.IsSystem)
            {
                // Add custom SystemUser role.
                roles[2] = "SystemUser";
            }

            // Construct a GenericIdentity object based on the current Windows
            // identity name and authentication type.
            string authenticationType = windowsIdentity.AuthenticationType;
            string userName = windowsIdentity.Name;
            GenericIdentity genericIdentity =
                new GenericIdentity(userName, authenticationType);

            // Construct a GenericPrincipal object based on the generic identity
            // and custom roles for the user.
            GenericPrincipal genericPrincipal =
                new GenericPrincipal(genericIdentity, roles);

            return genericPrincipal;
        }

        public string createGravesiteReport(string site_id, string section, string gravesite_id, string sectionPolygonExtent)
        {
            DateTime startTime = DateTime.Now;

            // get siteName
            string site_name = getSiteName(site_id);

            string layerId_gravesite_hilight_nolabel = "3";
            string layerId_gravesite_marker_hilight = "4";
            string layerId_section = "6";
            string layerId_section_hilight = "7";
            string section_field = "section";
            string siteid_field = "site_id";
            string gravemarker_field = "GRAVEMARKER_ID";
            string gravesiteid_field = "GRAVESITE_ID";

            string layer_Str = "&layers=show%3A" + layerId_gravesite_marker_hilight + "%2C" + layerId_section + "%2C" + layerId_section_hilight + "%2C" + layerId_gravesite_hilight_nolabel;
            string layerDef_Str = "&layerDefs=%7B+%22" + layerId_gravesite_marker_hilight + "%22%3A%22" + gravemarker_field + "%3D%27" + gravesite_id + "%27%22%3B%22"
                                                       + layerId_gravesite_hilight_nolabel + "%22%3A%22" + gravesiteid_field + "%3D%27" + gravesite_id + "%27%22%3B%22"
                                                       + layerId_section_hilight + "%22%3A%22" + siteid_field + "%3D%27" + site_id + "%27+and+" + section_field + "%3D%27" + section + "%27%22%7D";
            string mapUrl_request = mapUrl + "?bbox=" + sectionPolygonExtent + "&bboxSR=" + layer_Str + layerDef_Str + "&size=&imageSR=&format=png&transparent=false&dpi=&time=&layerTimeOptions=&dynamicLayers=&gdbVersion=&mapScale=&rotation=&datumTransformations=&layerParameterValues=&mapRangeValues=&layerRangeValues=&f=pjson";

            string mapImageUrl_section = "";
            if (sectionPolygonExtent != "")
            {
                HttpWebRequest request_map = (HttpWebRequest)WebRequest.Create(mapUrl_request);
                HttpWebResponse response_map = (HttpWebResponse)request_map.GetResponse();
                Stream receiveStream_map = response_map.GetResponseStream();
                StreamReader readStream_map = new StreamReader(receiveStream_map, Encoding.UTF8);
                var jsonStr_map = readStream_map.ReadToEnd();
                response_map.Close();
                readStream_map.Close();
                mapImageUrl_section = getMapImageUrl(jsonStr_map);
            }

            // GraveSite Data
            string pdfFile = "";

            dataListClass dataList = getGraveSiteMarkerList(site_id, section, gravesite_id);
            List<string> graveSiteMarkerList = dataList.gravesiteMarkerList;
            List<string> graveSitePolyList = dataList.gravesitePolyList;
            List<string> graveSiteList = dataList.gravesiteList;

            //testStr = String.Join(", ", graveSitePolyList.ToArray());
            if (testStr == "")
            {
                if (graveSiteList.Count > 0)
                {
                    List<string> returnList = createPDF(site_id, site_name, section, mapImageUrl_section, graveSiteMarkerList, graveSitePolyList, graveSiteList, gravesite_id);
                    pdfFile = returnList[0];
                    string pdfFileUrl = returnList[1];
                    testStr = returnList[2];

                    if (gravesite_id != "")
                    {
                        pdfFile = pdfFileUrl;
                    }
                    if (testStr != "")
                    {
                        pdfFile = testStr;
                    }

                }
            }
            else
            {
                pdfFile = testStr;
            }

            return pdfFile;
        }

        public List<string> createPDF(string site_id, string site_name, string section, string mapImageUrl_section, List<string> graveSiteMarkerList, List<string> graveSitePolyList, List<string> graveSiteList, string gravesite_id)
        {
            string pdfFile = "";
            string pdfFileUrl = "";
            testStr = "";

            try
            {
                string fileName = site_id + "_" + section;
                if (graveSiteMarkerList.Count == 1)
                {
                    fileName = graveSiteMarkerList[0];
                }
                string timeStr = DateTime.Now.ToString("yyyyMMddhmmss") + ".pdf";
                pdfFile = reportFolder + @"\" + fileName + "_" + timeStr;
                pdfFileUrl = reportFolderUrl + @"/" + fileName + "_" + timeStr;

                // iTextSharp
                iTextSharp.text.Document mDocument = new iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER);
                PdfWriter pdfWriter = PdfWriter.GetInstance(mDocument, new System.IO.FileStream(pdfFile, System.IO.FileMode.Create));
                //
                mDocument.Open();
                PdfContentByte cb = pdfWriter.DirectContent;

                if (graveSiteList.Count > 0)
                {

                    int j = 0;
                    foreach (string graveSiteMarker_id in graveSiteList)
                    {
                        bool gravesiteMarker_status = false;

                        if (graveSiteMarkerList.Exists(x => string.Equals(x, graveSiteMarker_id)))
                        {
                            gravesiteMarker_status = true;
                        }

                        j += 1;
                        //if (j > 3) break;

                        if (gravesiteMarker_status) // gravesiteMarker
                        {
                            string site_text = site_name + " - Section: " + section + " GraveSite: " + graveSiteMarker_id;
                            string mark_type = "";
                            string photo_1 = "";
                            string photo_2 = "";
                            string img1 = "";
                            string img2 = "";
                            double x_value = 0;
                            double y_value = 0;
                            string mapImageUrl_gravesite = "";
                            string graveSiteMarkerUrl_request = graveSiteMarkerUrl + "/query?where=gravemarker_id%3D%27" + graveSiteMarker_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=MARKER_TYPE%2C+FRONT_IMG%2C+REAR_IMG&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";

                            bool requestStatus = false;

                            try
                            {
                                HttpWebRequest request_graveSiteMarker = (HttpWebRequest)WebRequest.Create(graveSiteMarkerUrl_request);
                                HttpWebResponse response_graveSiteMarker = (HttpWebResponse)request_graveSiteMarker.GetResponse();
                                Stream receiveStream_graveSiteMarker = response_graveSiteMarker.GetResponseStream();
                                StreamReader readStream_graveSiteMarker = new StreamReader(receiveStream_graveSiteMarker, Encoding.UTF8);
                                var jsonStr_graveSiteMarker = readStream_graveSiteMarker.ReadToEnd();
                                response_graveSiteMarker.Close();
                                readStream_graveSiteMarker.Close();

                                List<string> graveSiteMarker_data = getGraveSiteMarker_data(site_id, jsonStr_graveSiteMarker, graveSiteMarker_id);

                                mark_type = graveSiteMarker_data[0];
                                photo_1 = graveSiteMarker_data[1];
                                photo_2 = graveSiteMarker_data[2];
                                img1 = graveSiteMarker_data[3];
                                img2 = graveSiteMarker_data[4];
                                x_value = Convert.ToDouble(graveSiteMarker_data[5]);
                                y_value = Convert.ToDouble(graveSiteMarker_data[6]);
                                mapImageUrl_gravesite = graveSiteMarker_data[7];

                                requestStatus = true;

                            }
                            catch (Exception e)
                            {
                                testStr = "gravesite_marker = " + graveSiteMarker_id + " Error: " + e.Message;
                            }

                            if (requestStatus)
                            {
                                bool status = addPDFPage(j, graveSiteList.Count, site_name, section, graveSiteMarker_id, mark_type, photo_1, photo_2, mapImageUrl_section, mapImageUrl_gravesite, mDocument, cb);
                            }
                        }
                        else // gravesitePoly
                        {
                            bool requestStatus = false;
                            string mapImageUrl_gravesitePoly = "";
                            string graveSitePolyUrl_request = graveSitePolyUrl + "/query?where=gravesite_id%3D%27" + graveSiteMarker_id + "%27&text=&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=&returnGeometry=true&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&queryByDistance=&returnExtentsOnly=false&datumTransformation=&parameterValues=&rangeValues=&f=pjson";
                            try
                            {
                                HttpWebRequest request_graveSitePoly = (HttpWebRequest)WebRequest.Create(graveSitePolyUrl_request);
                                HttpWebResponse response_graveSitePoly = (HttpWebResponse)request_graveSitePoly.GetResponse();
                                Stream receiveStream_graveSitePoly = response_graveSitePoly.GetResponseStream();
                                StreamReader readStream_graveSitePoly = new StreamReader(receiveStream_graveSitePoly, Encoding.UTF8);
                                var jsonStr_graveSitePoly = readStream_graveSitePoly.ReadToEnd();
                                response_graveSitePoly.Close();
                                readStream_graveSitePoly.Close();

                                mapImageUrl_gravesitePoly = getGraveSitePoly_data(jsonStr_graveSitePoly, gravesite_id);
                                //

                                if (mapImageUrl_gravesitePoly != "")
                                {
                                    requestStatus = true;
                                }
                            }
                            catch (Exception e)
                            {
                                testStr = "gravesite_poly = " + graveSiteMarker_id + " Error: " + e.Message;
                            }
                            if (requestStatus)
                            {
                                bool status = addPDFPage(j, graveSiteList.Count, site_name, section, graveSiteMarker_id, "", "", "", mapImageUrl_section, mapImageUrl_gravesitePoly, mDocument, cb);
                                if (status)
                                {
                                    testStr = "";
                                }

                            }


                        }
                    }
                }
                // finish
                mDocument.Close();
            }
            catch (Exception e)
            {
                testStr = "Error(createPDF): " + e.Message;
            }

            //
            List<string> returnList = new List<string>();
            returnList.Add(pdfFile);
            returnList.Add(pdfFileUrl);
            returnList.Add(testStr);


            return returnList;
        }

        public bool addPDFPage(int pageNum, int totalPageNum, string site_name, string section, string graveSiteMarker_id, string marker_type, string photo_1, string photo_2, string mapImageUrl_section, string mapImageUrl_gravesite, iTextSharp.text.Document mDocument, PdfContentByte cb)
        {
            bool status = false;
            testStr = "";

            try
            {
                // add new page
                if (pageNum > 1)
                {
                    mDocument.NewPage();
                }

                float x, y, w, h;
                iTextSharp.text.Rectangle rect;
                Chunk c, c2;
                iTextSharp.text.Phrase p;
                ColumnText ct;

                // title
                x = Convert.ToInt32(0);
                y = Convert.ToInt32(mDocument.PageSize.Height - 0.5 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width);
                h = Convert.ToInt32(1 * 72);

                rect = new iTextSharp.text.Rectangle(x, y, w, h);
                c2 = new Chunk("Gravesite Information", pFont4);
                p = new Phrase(); p.Add(c2);
                ct = new ColumnText(cb);
                ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                ct.SetSimpleColumn(rect);
                ct.SetText(p);
                ct.Go();

                // table box
                x = Convert.ToInt32(0.6 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 2.5 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width - 2 * x);
                h = Convert.ToInt32(1 * 72);

                // summary table
                x = Convert.ToInt32(0.6 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 2.5 * 72);
                w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 1.7 * 72);
                h = Convert.ToInt32(1.6 * 72);

                PdfPTable table_summary = new PdfPTable(2);
                table_summary.TotalWidth = Convert.ToInt16(w);

                //
                string[] splitStr2 = graveSiteMarker_id.Split('-');
                string wall = "";
                string row = "";
                string gravesite = "";
                if (splitStr2.Length == 5)
                {
                    wall = splitStr2[2];
                    row = splitStr2[3];
                    gravesite = splitStr2[4];
                }
                else if (splitStr2.Length == 4)
                {
                    wall = splitStr2[2];
                    gravesite = splitStr2[3];
                }
                else
                {
                    gravesite = graveSiteMarker_id;
                }

                // row1
                PdfPTable table_summary_header = new PdfPTable(1);
                table_summary_header.TotalWidth = Convert.ToInt16(w);

                // row1
                //PdfPCell cell = new PdfPCell(new Phrase(site_name, pFont_header3)); cell.Colspan = 2; cell.Rowspan = 2; cell.PaddingTop = 26f; cell.HorizontalAlignment = Element.ALIGN_CENTER; table_summary.AddCell(cell);
                PdfPCell cell = new PdfPCell(new Phrase(site_name, pFont_header3)); cell.Colspan = 2; cell.HorizontalAlignment = Element.ALIGN_CENTER; table_summary.AddCell(cell);

                // row2
                cell = new PdfPCell(new Phrase("Section", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);
                cell = new PdfPCell(new Phrase(section, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);

                // row4
                cell = new PdfPCell(new Phrase("Wall", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);
                cell = new PdfPCell(new Phrase(wall, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);

                // row5
                cell = new PdfPCell(new Phrase("Row", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);
                cell = new PdfPCell(new Phrase(row, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);

                // row3
                cell = new PdfPCell(new Phrase("Gravesite Number", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);
                cell = new PdfPCell(new Phrase(gravesite, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table_summary.AddCell(cell);

                table_summary.WriteSelectedRows(0, -1, Convert.ToInt16(x), Convert.ToInt16(y + h), cb);

                // interment table
                x = Convert.ToInt32((mDocument.PageSize.Width / 2) - 1 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 2.5 * 72);
                w = Convert.ToInt32((mDocument.PageSize.Width / 2) + 0.4 * 72);
                h = Convert.ToInt32(1.6 * 72);
                //cb.Rectangle(x, y, w, h);
                //cb.Stroke();

                PdfPTable table = new PdfPTable(6);
                table.TotalWidth = Convert.ToInt16(w);

                // variable
                string lastName_1 = "";
                string firstName_1 = "";
                string middleName_1 = "";
                string bossName_1 = "";

                string lastName_2 = "";
                string firstName_2 = "";
                string middleName_2 = "";
                string bossName_2 = "";

                string lastName_3 = "";
                string firstName_3 = "";
                string middleName_3 = "";
                string bossName_3 = "";

                string lastName_4 = "";
                string firstName_4 = "";
                string middleName_4 = "";
                string bossName_4 = "";

                string lastName_5 = "";
                string firstName_5 = "";
                string middleName_5 = "";
                string bossName_5 = "";

                // get interment data
                var decedentList_sub = decedentList.Where(v => v.gravesite == gravesite);

                int i = 0;
                foreach (decedentClass item in decedentList_sub)
                {
                    i += 1;
                    if (i == 1)
                    {
                        lastName_1 = item.last_name;
                        firstName_1 = item.first_name;
                        middleName_1 = item.middle_name;
                        bossName_1 = item.decedent_id;
                    }
                    if (i == 2)
                    {
                        lastName_2 = item.last_name;
                        firstName_2 = item.first_name;
                        middleName_2 = item.middle_name;
                        bossName_2 = item.decedent_id;
                    }
                    if (i == 3)
                    {
                        lastName_3 = item.last_name;
                        firstName_3 = item.first_name;
                        middleName_3 = item.middle_name;
                        bossName_3 = item.decedent_id;
                    }
                    if (i == 4)
                    {
                        lastName_4 = item.last_name;
                        firstName_4 = item.first_name;
                        middleName_4 = item.middle_name;
                        bossName_4 = item.decedent_id;
                    }
                    if (i == 5)
                    {
                        lastName_5 = item.last_name;
                        firstName_5 = item.first_name;
                        middleName_5 = item.middle_name;
                        bossName_5 = item.decedent_id;
                    }
                }

                // row1
                cell = new PdfPCell(new Phrase("", pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Interment 1", pFont_header3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Interment 2", pFont_header3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Interment 3", pFont_header3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Interment 4", pFont_header3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Interment 5", pFont_header3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);

                // row2
                cell = new PdfPCell(new Phrase("Last Name", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(lastName_1, pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(lastName_2, pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(lastName_3, pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(lastName_4, pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(lastName_5, pFont3)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);

                // row3
                cell = new PdfPCell(new Phrase("First Name", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(firstName_1, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(firstName_2, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(firstName_3, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(firstName_4, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(firstName_5, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);

                // row4
                cell = new PdfPCell(new Phrase("Middle Initial", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(middleName_1, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(middleName_2, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(middleName_3, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(middleName_4, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(middleName_5, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);

                // row5
                cell = new PdfPCell(new Phrase("BOSS ID", pFont3_italic)); cell.HorizontalAlignment = Element.ALIGN_LEFT; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(bossName_1, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(bossName_2, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(bossName_3, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(bossName_4, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);
                cell = new PdfPCell(new Phrase(bossName_5, pFont3_normal)); cell.HorizontalAlignment = Element.ALIGN_CENTER; table.AddCell(cell);

                table.WriteSelectedRows(0, -1, Convert.ToInt16(x), Convert.ToInt16(y + h), cb);


                // map box
                x = Convert.ToInt32(0.6 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 6.3 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width - 2 * x);
                h = Convert.ToInt32(3.5 * 72);
                //cb.Rectangle(x, y, w, h);
                //cb.Stroke();

                // section map
                x = Convert.ToInt32(0.6 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 5.5 * 72);
                w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.8 * 72);
                h = Convert.ToInt32(3.5 * 72);

                if (mapImageUrl_section == "")
                {
                    mapImageUrl_section = photoFolder + "/Reports/images/noImage.jpg";
                }

                if (mapImageUrl_section != "")
                {
                    iTextSharp.text.Image image_sectionMap;
                    image_sectionMap = iTextSharp.text.Image.GetInstance(mapImageUrl_section);
                    image_sectionMap.SetAbsolutePosition(x, y);
                    image_sectionMap.ScaleAbsoluteWidth(w);
                    image_sectionMap.ScaleAbsoluteHeight(h);
                    cb.AddImage(image_sectionMap);
                }

                cb.Rectangle(x, y, w, h);
                cb.Stroke();


                // gravesite map
                x = Convert.ToInt32((mDocument.PageSize.Width / 2) + 0.2 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 5.5 * 72);
                w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.8 * 72);
                h = Convert.ToInt32(3.5 * 72);

                if (mapImageUrl_gravesite != "")
                {
                    iTextSharp.text.Image image_gravesiteMap;
                    image_gravesiteMap = iTextSharp.text.Image.GetInstance(mapImageUrl_gravesite);
                    image_gravesiteMap.SetAbsolutePosition(x, y);
                    image_gravesiteMap.ScaleAbsoluteWidth(w);
                    image_gravesiteMap.ScaleAbsoluteHeight(h);
                    cb.AddImage(image_gravesiteMap);
                }
                cb.Rectangle(x, y, w, h);
                cb.Stroke();


                // photo box
                x = Convert.ToInt32((mDocument.PageSize.Width / 2) + 0.2 * 72);
                y = Convert.ToInt32(mDocument.PageSize.Height - 10.1 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width - 2 * x);
                h = Convert.ToInt32(3.5 * 72);
                //cb.Rectangle(x, y, w, h);
                //cb.Stroke();

                if (marker_type == "Headstone")
                {
                    // left photo
                    x = Convert.ToInt32(0.6 * 72);
                    y = Convert.ToInt32(mDocument.PageSize.Height - 10.1 * 72);
                    w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.8 * 72);
                    h = Convert.ToInt32(4.3 * 72);

                    if (photo_1 != "")
                    {
                        byte[] buff = System.IO.File.ReadAllBytes(photo_1);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
                        System.IO.Stream stream = new System.IO.MemoryStream();
                        stream.Write(buff, 0, buff.Length);
                        stream.Position = 0;
                        //
                        iTextSharp.text.Image image;
                        image = iTextSharp.text.Image.GetInstance(stream);
                        image.ScaleToFit(w, h);
                        image.SetAbsolutePosition(x + (w - image.ScaledWidth) / 2, y);

                        cb.AddImage(image);
                        stream.Close();
                        stream.Dispose();
                    }
                    cb.Rectangle(x, y, w, h);
                    cb.Stroke();


                    // right photo
                    x = Convert.ToInt32((mDocument.PageSize.Width / 2) + 0.2 * 72);
                    y = Convert.ToInt32(mDocument.PageSize.Height - 10.1 * 72);
                    w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.8 * 72);
                    h = Convert.ToInt32(4.3 * 72);

                    if (photo_2 != "")
                    {
                        byte[] buff = System.IO.File.ReadAllBytes(photo_2);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
                        System.IO.Stream stream = new System.IO.MemoryStream();
                        stream.Write(buff, 0, buff.Length);
                        stream.Position = 0;
                        //
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(stream);
                        image.ScaleToFit(w, h);
                        image.SetAbsolutePosition(x + (w - image.ScaledWidth) / 2, y);

                        cb.AddImage(image);
                        stream.Close();
                        stream.Dispose();
                    }
                    cb.Rectangle(x, y, w, h);
                    cb.Stroke();
                }
                else if (marker_type == "FlatMarker")
                {
                    x = Convert.ToInt32(0.6 * 72);
                    y = Convert.ToInt32(mDocument.PageSize.Height - 10.1 * 72);
                    w = Convert.ToInt32(mDocument.PageSize.Width - 2 * x);
                    h = Convert.ToInt32(4.3 * 72);

                    if (photo_1 != "")
                    {
                        byte[] buff = System.IO.File.ReadAllBytes(photo_1);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
                        System.IO.Stream stream = new System.IO.MemoryStream();
                        stream.Write(buff, 0, buff.Length);
                        stream.Position = 0;
                        //
                        iTextSharp.text.Image image;
                        image = iTextSharp.text.Image.GetInstance(stream);

                        //image.SetAbsolutePosition(x, y);
                        //image.ScaleAbsoluteWidth(w);
                        //image.ScaleAbsoluteHeight(h);

                        image.ScaleToFit(w, h);
                        image.SetAbsolutePosition(x, y + (h - image.ScaledHeight) / 2);

                        cb.AddImage(image);
                        stream.Close();
                        stream.Dispose();
                    }
                    cb.Rectangle(x, y, w, h);
                    cb.Stroke();
                }
                else if (marker_type == "")
                {
                }
                else // if (marker_type == "NicheCover" || marker_type == "MemWallMarker")
                {
                    x = Convert.ToInt32(2.5 * 72);
                    y = Convert.ToInt32(mDocument.PageSize.Height - 10.1 * 72);
                    w = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.8 * 72);
                    h = Convert.ToInt32(4.3 * 72);

                    if (photo_1 != "")
                    {
                        byte[] buff = System.IO.File.ReadAllBytes(photo_1);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
                        System.IO.Stream stream = new System.IO.MemoryStream();
                        stream.Write(buff, 0, buff.Length);
                        stream.Position = 0;
                        //
                        iTextSharp.text.Image image;
                        image = iTextSharp.text.Image.GetInstance(stream);
                        //image.SetAbsolutePosition(x, y);
                        //image.ScaleAbsoluteWidth(w);
                        //image.ScaleAbsoluteHeight(h);

                        image.ScaleToFit(w, h);
                        image.SetAbsolutePosition(x + (w - image.ScaledWidth) / 2, y);

                        cb.AddImage(image);
                        stream.Close();
                        stream.Dispose();
                    }
                    cb.Rectangle(x, y, w, h);
                    cb.Stroke();
                }

                // box header
                //
                int x_left = Convert.ToInt32(0.6 * 72); ;
                int x_right = Convert.ToInt32((mDocument.PageSize.Width / 2) + 4 * 72);
                int y1 = Convert.ToInt32((mDocument.PageSize.Height - 1.7 * 72) - (0.04 * 72));
                int y2 = Convert.ToInt32((mDocument.PageSize.Height - 5.5 * 72) - (0.04 * 72));
                int w_header = Convert.ToInt32((mDocument.PageSize.Width / 2) - 0.3 * 72);

                //
                rect = new iTextSharp.text.Rectangle(x_left, y1, w_header, 1 * 72);
                c = new Chunk("Section Map", pFont3);
                p = new Phrase(c);
                ct = new ColumnText(cb);
                ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                ct.SetSimpleColumn(rect);
                ct.SetText(p);
                ct.Go();

                //
                rect = new iTextSharp.text.Rectangle(x_right, y1, w_header, 1 * 72);
                c = new Chunk("Gravesite Map", pFont3);
                p = new Phrase(c);
                ct = new ColumnText(cb);
                ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                ct.SetSimpleColumn(rect);
                ct.SetText(p);
                ct.Go();

                if (marker_type == "Headstone")
                {
                    //
                    rect = new iTextSharp.text.Rectangle(x_left, y2, w_header, 1 * 72);
                    c = new Chunk("Front Photo", pFont3);
                    p = new Phrase(c);
                    ct = new ColumnText(cb);
                    ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    ct.SetSimpleColumn(rect);
                    ct.SetText(p);
                    ct.Go();

                    //
                    rect = new iTextSharp.text.Rectangle(x_right, y2, w_header, 1 * 72);
                    c = new Chunk("Rear Photo", pFont3);
                    p = new Phrase(c);
                    ct = new ColumnText(cb);
                    ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    ct.SetSimpleColumn(rect);
                    ct.SetText(p);
                    ct.Go();
                }

                // footer
                x = Convert.ToInt32(0);
                y = Convert.ToInt32(0.4 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width);
                h = Convert.ToInt32(0.8 * 72);
                //cb.Rectangle(x, y, w, h);
                //cb.Stroke();

                if (totalPageNum > 1)
                {
                    rect = new iTextSharp.text.Rectangle(x, y, w, h);
                    c = new Chunk("Page " + pageNum.ToString() + " of " + totalPageNum.ToString(), pFont3);
                    p = new Phrase(c);
                    ct = new ColumnText(cb);
                    ct.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    ct.SetSimpleColumn(rect);
                    ct.SetText(p);
                    ct.Go();
                }

                // outbox
                x = Convert.ToInt32(0.5 * 72);
                y = Convert.ToInt32(0.8 * 72);
                w = Convert.ToInt32(mDocument.PageSize.Width - 2 * x);
                h = Convert.ToInt32(mDocument.PageSize.Height - 2 * y);
                cb.MoveTo(0, 0);
                cb.Rectangle(x, y, w, h);
                cb.Stroke();

                //
                status = true;
            }
            catch (Exception e)
            {
                testStr = "Error: " + e.Message;
            }
            return status;
        }


        public string getMapImageUrl(string jsonStr)
        {
            string mapImageUrl = "";

            JObject jsonObj = JObject.Parse(jsonStr);
            Dictionary<string, object> dictObj = jsonObj.ToObject<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> entry in dictObj)
            {
                if (entry.Key == "href")
                {
                    mapImageUrl = entry.Value.ToString();
                }
            }

            return mapImageUrl;

        }

        public List<string> getIntermentPhotos(string gravesite_id)
        {
            string interment_img1 = "";
            string interment_img2 = "";
            string sqlcomm = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    sqlcomm = "Select gravesite_id, IMG1, IMG2 From " + dbSchema + "INTERMENT_PT_evw Where GRAVESITE_ID='" + gravesite_id + "'";

                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[1] != null) interment_img1 = row[1].ToString();
                        if (row[2] != null) interment_img2 = row[2].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

            //
            List<string> intermentPhotoList = new List<string>();
            intermentPhotoList.Add(interment_img1);
            intermentPhotoList.Add(interment_img2);

            return intermentPhotoList;

        }


        public string getFeatureExtent(string jsonStr)
        {
            string extentStr = "";

            double xMin = 9999999999;
            double xMax = -9999999999;
            double yMin = 9999999999;
            double yMax = -9999999999;

            JObject jsonObj = JObject.Parse(jsonStr);
            Dictionary<string, object> dictObj = jsonObj.ToObject<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> entry in dictObj)
            {
                if (entry.Key == "features")
                {
                    IEnumerable featList = entry.Value as IEnumerable;
                    foreach (object feat in featList)
                    {
                        JObject jsonObj_feat = JObject.Parse(feat.ToString());
                        Dictionary<string, object> dictObj_feat = jsonObj_feat.ToObject<Dictionary<string, object>>();
                        //
                        string geom_jason = dictObj_feat["geometry"].ToString();
                        JObject jsonObj_rings = JObject.Parse(geom_jason);
                        Dictionary<string, object> dictObj_rings = jsonObj_rings.ToObject<Dictionary<string, object>>();
                        //

                        IEnumerable ringsList = dictObj_rings["rings"] as IEnumerable;
                        foreach (object ring in ringsList)
                        {
                            IEnumerable ringList = ring as IEnumerable;
                            foreach (object coordinates in ringList)
                            {
                                IEnumerable point = coordinates as IEnumerable;

                                double x_value = -88888;
                                double y_value = -88888;
                                int j = 0;
                                foreach (object item in point)
                                {
                                    j += 1;
                                    if (j == 1) x_value = Convert.ToDouble(item);
                                    if (j == 2) y_value = Convert.ToDouble(item);
                                }
                                //
                                if (x_value < xMin) xMin = x_value;
                                if (x_value > xMax) xMax = x_value;
                                if (y_value < yMin) yMin = y_value;
                                if (y_value > yMax) yMax = y_value;

                            }
                        }

                    }
                }
            }

            double expandRatio_x = 0.5;
            double expandRatio_y = 0.5;

            expandRatio_x = 0;
            expandRatio_y = 0;

            xMin = xMin - (Math.Abs(xMax - xMin) * expandRatio_x);
            xMax = xMax + (Math.Abs(xMax - xMin) * expandRatio_x);
            yMin = yMin - (Math.Abs(yMax - yMin) * expandRatio_y);
            yMax = yMax + (Math.Abs(yMax - yMin) * expandRatio_y);

            extentStr = xMin.ToString() + ", " + yMin.ToString() + ", " + xMax.ToString() + ", " + yMax.ToString();

            return extentStr;

        }

        public dataListClass getGraveSiteMarkerList(string site_id, string section, string gravesite_id)
        {
            List<string> graveSiteMarkerList = new List<string>();
            List<string> graveSitePolyList = new List<string>();
            List<string> graveSiteList = new List<string>();

            decedentList = new List<decedentClass>();

            testStr = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(conStr))
                {
                    conn.Open();

                    string sqlcomm = "";
                    if (gravesite_id == "")
                    {
                        sqlcomm = "Select DISTINCT GRAVEMARKER_ID From " + dbSchema + "GraveSite_Marker_PT_evw Where SITE_ID = '" + site_id + "' And Section = '" + section + "' And Active = '1'";
                    }
                    else
                    {
                        sqlcomm = "Select DISTINCT GRAVEMARKER_ID From " + dbSchema + "GraveSite_Marker_PT_evw Where SITE_ID = '" + site_id + "' And Section = '" + section + "' And GRAVEMARKER_ID = '" + gravesite_id + "' And Active = '1'";
                    }
                    SqlCommand comm = new SqlCommand(sqlcomm, conn);
                    SqlDataReader dr = null;
                    dr = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        graveSiteMarkerList.Add(row[0].ToString());
                        if (!graveSiteList.Exists(x => string.Equals(x, row[0].ToString())))
                        {
                            graveSiteList.Add(row[0].ToString());
                        }
                    }

                    // gravesite_poly - gravesite_id list
                    sqlcomm = "";
                    if (gravesite_id == "")
                    {
                        sqlcomm = "Select DISTINCT GRAVESITE_ID From " + dbSchema + "GraveSite_POLY_evw Where SITE_ID = '" + site_id + "' And Section = '" + section + "' And Active = '1'";
                    }
                    else
                    {
                        sqlcomm = "Select DISTINCT GRAVESITE_ID From " + dbSchema + "GraveSite_POLY_evw Where SITE_ID = '" + site_id + "' And Section = '" + section + "' And GRAVESITE_ID = '" + gravesite_id + "' And Active = '1'";
                    }
                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        graveSitePolyList.Add(row[0].ToString());
                        if (!graveSiteList.Exists(x => string.Equals(x, row[0].ToString())))
                        {
                            graveSiteList.Add(row[0].ToString());
                        }
                    }


                    // decedent_info table
                    if (gravesite_id == "")
                    {
                        sqlcomm = "Select site_id, section, gravesite, last_name, first_name, middle_name, decedent_id From " + dbSchema + "interment_info Where SITE_ID = '" + site_id + "' And Section = '" + section + "'";
                    }
                    else
                    {
                        string[] splitStr = gravesite_id.Split('-');
                        string gravesite = "";
                        if (splitStr.Length == 5)
                        {
                            gravesite = splitStr[splitStr.Length - 1];
                        }
                        sqlcomm = "Select site_id, section, gravesite, last_name, first_name, middle_name, decedent_id From " + dbSchema + "interment_info Where SITE_ID = '" + site_id + "' And Section = '" + section + "' And gravesite = '" + gravesite + "'";
                    }

                    comm = new SqlCommand(sqlcomm, conn);
                    dr = null;
                    dr = comm.ExecuteReader();
                    dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        string gravesite_str = "";
                        string lastname_str = "";
                        string firstname_str = "";
                        string middlename_str = "";
                        string decedent_id_str = "";

                        if (row[2] != null) gravesite_str = row[2].ToString();
                        if (row[3] != null) lastname_str = row[3].ToString();
                        if (row[4] != null) firstname_str = row[4].ToString();
                        if (row[5] != null) middlename_str = row[5].ToString();
                        if (row[6] != null) decedent_id_str = row[6].ToString();

                        decedentClass decedentData = new decedentClass();
                        decedentData.gravesite = gravesite_str;
                        decedentData.last_name = lastname_str;
                        decedentData.first_name = firstname_str;
                        decedentData.middle_name = middlename_str;
                        decedentData.decedent_id = decedent_id_str;

                        decedentList.Add(decedentData);
                    }
                }
            }
            catch (Exception e)
            {
                testStr = "Error(getGraveSiteMarkerList) : " + e.Message;
            }

            dataListClass dataList = new dataListClass();
            dataList.gravesiteMarkerList = graveSiteMarkerList;
            dataList.gravesitePolyList = graveSitePolyList;

            graveSiteList.Sort();
            dataList.gravesiteList = graveSiteList;

            return dataList;
        }

        public List<string> getGraveSiteMarker_data(string site_id, string jsonStr, string gravesite_id)
        {
            List<string> graveSiteMarker_data = new List<string>();

            JObject jsonObj = JObject.Parse(jsonStr);
            Dictionary<string, object> dictObj = jsonObj.ToObject<Dictionary<string, object>>();
            foreach (KeyValuePair<string, object> entry in dictObj)
            {
                if (entry.Key == "features")
                {
                    IEnumerable featList = entry.Value as IEnumerable;
                    foreach (object feat in featList)
                    {
                        JObject jsonObj_feat = JObject.Parse(feat.ToString());
                        Dictionary<string, object> dictObj_feat = jsonObj_feat.ToObject<Dictionary<string, object>>();

                        // geometry
                        string geom_jason = dictObj_feat["geometry"].ToString();
                        JObject jsonObj_geometry = JObject.Parse(geom_jason);
                        Dictionary<string, object> dictObj_geometry = jsonObj_geometry.ToObject<Dictionary<string, object>>();
                        double x_value = Convert.ToDouble(dictObj_geometry["x"]);
                        double y_value = Convert.ToDouble(dictObj_geometry["y"]);

                        // attributes
                        string attributes_jason = dictObj_feat["attributes"].ToString();
                        JObject jsonObj_attributes = JObject.Parse(attributes_jason);
                        Dictionary<string, object> dictObj_attributes = jsonObj_attributes.ToObject<Dictionary<string, object>>();

                        string marker_type = Convert.ToString(dictObj_attributes["MARKER_TYPE"]);
                        string front_img = Convert.ToString(dictObj_attributes["FRONT_IMG"]);
                        string rear_img = Convert.ToString(dictObj_attributes["REAR_IMG"]);

                        // interment photos
                        List<string> intermentPhotoList = getIntermentPhotos(gravesite_id);
                        string img1 = intermentPhotoList[0];
                        string img2 = intermentPhotoList[1];
                        
                        //
                        string imagePath = "";

                        if (front_img != "")
                        {
                            imagePath = photoFolder + "/" + site_id + "/marker/" + front_img;
                            if (System.IO.File.Exists(imagePath))
                            {
                                front_img = imagePath;
                            }
                            else
                            {
                                front_img = photoFolder + "/Reports/images/noImage.jpg";
                            }
                        }
                        if (rear_img != "")
                        {
                            imagePath = photoFolder + "/" + site_id + "/marker/" + rear_img;
                            if (System.IO.File.Exists(imagePath))
                            {
                                rear_img = imagePath;
                            }
                            else
                            {
                                rear_img = photoFolder + "/Reports/images/noImage.jpg";
                            }
                        }
                        if (img1 != "")
                        {
                            imagePath = photoFolder + "/" + site_id + "/marker/" + img1;
                            if (System.IO.File.Exists(imagePath))
                            {
                                img1 = imagePath;
                            }
                            else
                            {
                                img1 = "";
                            }
                        }
                        if (img2 != "")
                        {
                            imagePath = photoFolder + "/" + site_id + "/marker/" + img2;
                            if (System.IO.File.Exists(imagePath))
                            {
                                img2 = imagePath;
                            }
                            else
                            {
                                img2 = "";
                            }
                        }

                        //
                        graveSiteMarker_data.Add(marker_type);
                        graveSiteMarker_data.Add(front_img);
                        graveSiteMarker_data.Add(rear_img);
                        graveSiteMarker_data.Add(img1);
                        graveSiteMarker_data.Add(img2);
                        graveSiteMarker_data.Add(x_value.ToString());
                        graveSiteMarker_data.Add(y_value.ToString());
                    }
                }
            }

            // get map url
            double x = Convert.ToDouble(graveSiteMarker_data[5]);
            double y = Convert.ToDouble(graveSiteMarker_data[6]);

            double scale = 5;

            double xMin = x - scale;
            double yMin = y - scale;
            double xMax = x + scale;
            double yMax = y + scale;

            string[] splitStr1 = gravesite_id.Split('-');
            string gravesite = "";
            if (splitStr1.Length == 5)
            {
                gravesite = splitStr1[splitStr1.Length - 1];
            }

            string layerId_gravesitemarker_hilight = "0";
            string layerId_gravesitmarker = "1";
            string layerId_interment = "2";
            string layerId_gravesite_highlight = "4";
            string layerId_gravesite = "5";

            //string siteid_field = "site_id";
            //string section_field = "section";
            //string gravesite_field = "gravesite";
            string gravesitemarker_field = "GRAVEMARKER_ID";

            string layer_Str = "&layers=show%3A" + layerId_gravesitemarker_hilight + "%2C" + layerId_gravesitmarker + "%2C" + layerId_interment + "%2C" + layerId_gravesite_highlight + "%2C" + layerId_gravesite;
            string layerDef_Str = "&layerDefs=%7B+%22" + layerId_gravesitemarker_hilight + "%22+%3A+%22" + gravesitemarker_field + "+%3D+%27" + gravesite_id + "%27%22%2C+%22" + layerId_gravesite_highlight + "%22%3A+%22" + gravesitemarker_field + "+%3D+%27" + gravesite_id + "%27%22+%7D";
            string mapUrl_request = mapUrl + "?bbox=" + xMin.ToString() + "," + yMin.ToString() + "," + xMax.ToString() + "," + yMax.ToString() + "&bboxSR=" + layer_Str + layerDef_Str + "&size=&imageSR=&format=png&transparent=false&dpi=&time=&layerTimeOptions=&dynamicLayers=&gdbVersion=&mapScale=&rotation=&datumTransformations=&layerParameterValues=&mapRangeValues=&layerRangeValues=&f=pjson";

            HttpWebRequest request_map = (HttpWebRequest)WebRequest.Create(mapUrl_request);
            HttpWebResponse response_map = (HttpWebResponse)request_map.GetResponse();
            Stream receiveStream_map = response_map.GetResponseStream();
            StreamReader readStream_map = new StreamReader(receiveStream_map, Encoding.UTF8);
            var jsonStr_map = readStream_map.ReadToEnd();
            response_map.Close();
            readStream_map.Close();

            string mapImageUrl_gravesite = getMapImageUrl(jsonStr_map);
            graveSiteMarker_data.Add(mapImageUrl_gravesite);

            return graveSiteMarker_data;

        }

        public string getGraveSitePoly_data(string jsonStr, string gravesite_id)
        {
            string extent_str = getFeatureExtent(jsonStr);

            // get map url
            string layerId_gravesitemarker_hilight = "0";
            string layerId_gravesitmarker = "1";
            string layerId_interment = "2";
            string layerId_gravesite_highlight = "4";
            string layerId_gravesite = "5";

            string gravesitemarker_field = "GRAVEMARKER_ID";

            string layer_Str = "&layers=show%3A" + layerId_gravesitemarker_hilight + "%2C" + layerId_gravesitmarker + "%2C" + layerId_interment + "%2C" + layerId_gravesite_highlight + "%2C" + layerId_gravesite;
            string layerDef_Str = "&layerDefs=%7B+%22" + layerId_gravesitemarker_hilight + "%22+%3A+%22" + gravesitemarker_field + "+%3D+%27" + gravesite_id + "%27%22%2C+%22" + layerId_gravesite_highlight + "%22%3A+%22" + gravesitemarker_field + "+%3D+%27" + gravesite_id + "%27%22+%7D";
            string mapUrl_request = mapUrl + "?bbox=" + extent_str + "&bboxSR=" + layer_Str + layerDef_Str + "&size=&imageSR=&format=png&transparent=false&dpi=&time=&layerTimeOptions=&dynamicLayers=&gdbVersion=&mapScale=&rotation=&datumTransformations=&layerParameterValues=&mapRangeValues=&layerRangeValues=&f=pjson";

            HttpWebRequest request_map = (HttpWebRequest)WebRequest.Create(mapUrl_request);
            HttpWebResponse response_map = (HttpWebResponse)request_map.GetResponse();
            Stream receiveStream_map = response_map.GetResponseStream();
            StreamReader readStream_map = new StreamReader(receiveStream_map, Encoding.UTF8);
            var jsonStr_map = readStream_map.ReadToEnd();
            response_map.Close();
            readStream_map.Close();

            string mapImageUrl_gravesite = getMapImageUrl(jsonStr_map);
            return mapImageUrl_gravesite;
        }

        public string getSiteName(string site_id)
        {
            string site_name = "";
            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();

                string sqlcomm = "Select site_id, full_name From " + dbSchema + "site_pt Where SITE_ID = '" + site_id + "'";
                SqlCommand comm = new SqlCommand(sqlcomm, conn);
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

        public string sendEMail(string emailAddress, string site_id, string section, string email, string pdfFile, string processStatus)
        {
            string status = "";

            string smtpHost = "smtp.va.gov";
            //string from_emailAddress = "xiaoyi.zhang2@va.gov";
            string from_emailAddress = "noreply@va.gov";

            string to_emailAddress = email;

            MailAddress to = new MailAddress(to_emailAddress);
            MailAddress from = new MailAddress(from_emailAddress);
            MailMessage message = new MailMessage(from, to);
            message.Subject = "NCA Mapbook for GraveSite Report";
            if (pdfFile != "")
            {
                message.Body = @"Site_id = " + site_id + " Section = " + section + Environment.NewLine + @"The gravesite report in zip file at " + pdfFile;
            }
            else
            {
                message.Body = @"Site_id = " + site_id + " Section = " + section + Environment.NewLine + @processStatus;
            }

            SmtpClient client = new SmtpClient(smtpHost);

            try
            {
                client.Send(message);
                status = "sending email - OK";
            }
            catch (Exception ex)
            {
                status = "sending email - Error(" + ex.Message + ")";
            }

            return status;
        }

        public bool isNumeric(string inStr)
        {
            try
            {
                int.Parse(inStr);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}