#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

#endregion

namespace ProjectCleanup
{
    [Transaction(TransactionMode.Manual)]
    public class cmdProjectCleanup : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // delete selected groups in the Inactive Category

            // adjust the View Templates to eliminate category 00 and bump current 07 and up by 1 number
                // if view category = 01 make it 08 etc

            // delete unused views in Categories: 01 - 09, elevation in 13 & views named soffit in 14

            // delete unused schedules - code below works; this step is complete

            // remove the Project Parameter "Code Bracing"

            // put any code needed for the form here

                // get all the sheet groups in the Inactive category
                // put them in a list to bind to the listbox

            //List<string> inactiveGroups = Utils.GetAllSheetGroupsByCategory(doc, "Inactive");

            // open form
            frmProjectCleanup curForm = new frmProjectCleanup()
            {
                Width = 800,
                Height = 450,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            // get form data and do something

            string txtClient = curForm.GetComboboxClient();
            string floorNum = curForm.GetComboboxFloors();

            string nameClient = "";

            if (txtClient == "Central Texas")
                nameClient = "LGI-CTX";
            else if (txtClient == "Dallas/Fort Worth")
                nameClient = "LGI-DFW";
            else if (txtClient == "Houston")
                nameClient = "LGI-HOU";
            else if (txtClient == "Maryland")
                nameClient = "LGI-MD";
            else if (txtClient == "Minnesota")
                nameClient = "LGI-MN";
            else if (txtClient == "Oklahoma")
                nameClient = "LGI-OK";
            else if (txtClient == "Pennsylvania")
                nameClient = "LGI-PA";
            else if (txtClient == "Southeast")
                nameClient = "LGI-SE";
            else if (txtClient == "Virginia")
                nameClient = "LGI-VA";
            else if (txtClient == "West Virginia")
                nameClient = "LGI-WV";

            ProjectInfo clientInfo = doc.ProjectInformation;

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Project Cleanup");

                if (null != clientInfo)
                {
                    clientInfo.ClientName = nameClient;
                }

        // POPULATE LISTBOX WITH SHEET GROUPS

        // DELETE UNUSED VIEWS

                // create a list of views to delete
                List<View> viewsToDelete = new List<View>();

                // create a list opf viewsd to keep
                List<View> viewsToKeep = new List<View>();

                // get all the views in the project by category
                List<View> listViews = Utils.GetAllViewsByCategory(doc, "01:Floor Plans");
                List<View> listCat02 = Utils.GetAllViewsByCategory(doc, "02:Elevations");
                List<View> listCat03 = Utils.GetAllViewsByCategory(doc, "03:Roof Plans");
                List<View> listCat04 = Utils.GetAllViewsByCategory(doc, "04:Sections");
                List<View> listCat05 = Utils.GetAllViewsByCategory(doc, "05:Interior Elevations");
                List<View> listCat06 = Utils.GetAllViewsByCategory(doc, "06:Electrical Plans");
                List<View> listCat07 = Utils.GetAllViewsByCategory(doc, "07:Form/Foundation Plans");
                List<View> listCat08 = Utils.GetAllViewsByCategory(doc, "08:Ceiling Framing Plans");
                List<View> listCat09 = Utils.GetAllViewsByCategory(doc, "09:Roof Framing Plans");
                List<View> listCat13 = Utils.GetAllViewsByCategoryAndViewTemplate(doc, "13:Presentation Views", "13-Elevation Presentation");
                List<View> listCat14 = Utils.GetAllViewsByCategoryAndViewTemplate(doc, "14:Ceiling Views", "14-Soffit");

                // combine the lists together
                listViews.AddRange(listCat02);
                listViews.AddRange(listCat03);
                listViews.AddRange(listCat04);
                listViews.AddRange(listCat05);
                listViews.AddRange(listCat06);
                listViews.AddRange(listCat07);
                listViews.AddRange(listCat08);
                listViews.AddRange(listCat09);
                listViews.AddRange(listCat13);
                listViews.AddRange(listCat14);

                // get all the sheets in the project
                List<ViewSheet> sheetList = Utils.GetAllSheets(doc);

                ElementId sheetId = sheetList.First<ViewSheet>().Id;

                // loop through the views
                foreach (View curView in listViews)
                {
            // MODIFIED FROM MRM; DOESN'T WORK

                    // check if the view is already on a sheet            
                    if (Viewport.CanAddViewToSheet(doc, sheetId, curView.Id))
                    {
                        // check if the view has dependent views
                        if(curView.GetDependentViewIds().Count() == 0)
                        {
                            // add view to list of views to delete
                            viewsToDelete.Add(curView);
                        }
                    }
                }

        // DELETE UNUSED SCHEDULES

                string areaString = "";
                string roofString = "";

                if (floorNum == "1")
                {
                    areaString = "(multi-level)";
                    roofString = "(multi-space)";
                }
                else if (floorNum != "1")
                {
                    areaString = "(single level)";
                    roofString = "(single space)";
                }

                List<ViewSchedule> scheduleList = GetScheduleByNameContains(doc, areaString);                

                List<ViewSchedule> roofList = GetScheduleByNameContains(doc, roofString);

                scheduleList.AddRange(roofList);

                if (curForm.GetCheckBoxSchedules() == true)
                {
                    foreach (ViewSchedule curSchedule in scheduleList)
                    {
                        doc.Delete(curSchedule.Id);
                    }                    
                }

        
               
           

                t.Commit();
            }            

            return Result.Succeeded;
        }

        private List<ViewSchedule> GetScheduleByNameContains(Document doc, string scheduleString)
        {
            List<ViewSchedule> m_scheduleList = GetAllSchedules(doc);

            List<ViewSchedule> m_returnList = new List<ViewSchedule>();

            foreach (ViewSchedule curSchedule in m_scheduleList)
            {
                if (curSchedule.Name.Contains(scheduleString))
                    m_returnList.Add(curSchedule);
            }

            return m_returnList;
        }

        public static List<ViewSchedule> GetAllSchedules(Document doc)
        {
            List<ViewSchedule> m_schedList = new List<ViewSchedule>();

            FilteredElementCollector curCollector = new FilteredElementCollector(doc);
            curCollector.OfClass(typeof(ViewSchedule));

            //loop through views and check if schedule - if so then put into schedule list
            foreach (ViewSchedule curView in curCollector)
            {
                if (curView.ViewType == ViewType.Schedule)
                {
                    m_schedList.Add((ViewSchedule)curView);
                }
            }

            return m_schedList;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
