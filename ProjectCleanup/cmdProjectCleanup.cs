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
using System.Windows.Controls;
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
            
            // put any code needed for the form here

            // get sheet groups in Inactive category
            List<string> uniqueGroups = Utils.GetAllSheetGroupsByCategory(doc, "Inactive");

            // open form
            frmProjectCleanup curForm = new frmProjectCleanup(uniqueGroups)
            {
                Width = 800,
                Height = 450,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            // get form data and do something

            // set some variables
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

        //SET VALUE OF CLIENT NAME

                if (null != clientInfo)
                {
                    clientInfo.ClientName = nameClient;
                }

        // DELETE SELECTED GROUPS

                foreach (var item in curForm.lbxGroups.Items)
                {
                    ListBoxItem listBoxItem = curForm.lbxGroups.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;

                    if (listBoxItem != null)
                    {
                        CheckBox checkBox = Utils.FindVisualChild<CheckBox>(listBoxItem);
                        if (checkBox != null && checkBox.IsChecked == true)
                        {
                            string stringValue = checkBox.Content.ToString();
                          
                            List<ViewSheet> viewSheets = Utils.GetSheetsByGroupName(doc, stringValue);

                            foreach (ViewSheet curSheet in viewSheets)
                            {
                                doc.Delete(curSheet.Id);
                            }
                        }
                    }
                }

        // DELETE UNUSED VIEWS

                // create a list of views to delete
                List<View> viewsToDelete = new List<View>();

                // create a list opf views to keep
                List<View> viewsToKeep = new List<View>();

                // get all the views in the project by category
                List<View> listViews = new List<View>();
                List<View> listCat01 = Utils.GetAllViewsByCategory(doc, "01:Floor Plans");
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
                listViews.AddRange(listCat01);
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
                FilteredElementCollector sheetColl = new FilteredElementCollector(doc);
                sheetColl.OfClass(typeof(ViewSheet));

                // loop through views
                foreach (View curView in listViews)
                {
                    // check if view is already on sheet
                    if (Viewport.CanAddViewToSheet(doc, sheetColl.FirstElementId(), curView.Id))
                    {

                        // check if view has dependent views
                        if (curView.GetDependentViewIds().Count() == 0)
                        {
                            // add view to list of views to be deleted
                            viewsToDelete.Add(curView);
                        }

                    }
                }

                foreach (View deleteView in viewsToDelete)
                {
                    // delete the view
                    doc.Delete(deleteView.Id);
                }

        // DELETE UNUSED SCHEDULES

                // set some variables
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

                // get all the schedules by type
                List<ViewSchedule> schedules = new List<ViewSchedule>();

                List<ViewSchedule> areaSchedList = Utils.GetScheduleByNameContains(doc, areaString);                

                List<ViewSchedule> roofSchedList = Utils.GetScheduleByNameContains(doc, roofString);

                //combine the lists together
                schedules.AddRange(areaSchedList);
                schedules.AddRange(roofSchedList);

                if (curForm.GetCheckBoxSchedules() == true)
                {
                    foreach (ViewSchedule curSchedule in schedules)
                    {
                        doc.Delete(curSchedule.Id);
                    }                    
                }

        // RENAME SCHEDULES

                // get all the schedules
                List<ViewSchedule> scheduleList = Utils.GetAllSchedules(doc);
                
                if (curForm.GetCheckBoxSchedRename() == true)
                {
                    foreach(ViewSchedule curSchedule in scheduleList)
                    {
                        // create a variable for the schedule name
                        string[] inputString = curSchedule.Name.Split('-');
                        string curElev = inputString[1][0].ToString();

                        string replaceString = "Elevation " + curElev;

                        string originalString = curSchedule.Name;

                        // check if first character after hypen is "E"
                        if (curElev != "E")
                        {
                            string[] splitString = curSchedule.Name.Split('-');
                            splitString[1] = replaceString;

                            string newString = string.Join("-", splitString);
                        }
                        else
                            continue;
                    }
                }

        // DELETE CODE BRACING PARAMETER

                string paramName = "Code Bracing";
                IEnumerable<ParameterElement> _params = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfClass(typeof(ParameterElement))
                        .Cast<ParameterElement>();
                ParameterElement projectParam = null;
                foreach (ParameterElement pElem in _params)
                {
                    if (pElem.GetDefinition().Name.Equals(paramName))
                    {
                        projectParam = pElem;
                        break;
                    }
                }
                if (projectParam == null) 
                    return Result.Cancelled;

                if (curForm.GetCheckBoxCode() == true)
                {
                    doc.Delete(projectParam.Id);
                }

        // DELETE THE CODE FILTER FROM SHEET NAME

                // get all the sheets
                List<ViewSheet> activeSheets = Utils.GetAllSheets(doc);

                if (curForm.GetCheckBoxSheets() == true)
                {
                    foreach (ViewSheet curSheet in activeSheets)
                    {
                        string sheetName = curSheet.Name;
                        // check if sheet name ends with '-#'
                        if (sheetName.Length > 2 && sheetName[sheetName.Length - 2] == '-')
                        {
                            char lastChar = sheetName[sheetName.Length - 1];
                            // check if the last character is a digit
                            if (Char.IsDigit(lastChar))
                            {
                                // check if sheet name ends with '-#g'
                                if (sheetName.EndsWith("-" + lastChar + "g"))
                                {
                                    sheetName = sheetName.Substring(0, sheetName.Length - 2) + "g";
                                }
                                else
                                {
                                    sheetName = sheetName.Substring(0, sheetName.Length - 2);
                                }
                                // set the new sheet name
                                curSheet.Name = sheetName;
                            }
                        }
                    }
                }

        // UPDATE ROOM TAG AND LOAD CEILING HEIGHT PARAMETER

                // update the room tag family
                // add the ceiling height parameter
                // set value to value of ceiling finish parameter
                // clear value of ceiling finish parmeter

                if(curForm.GetCheckBoxRoomTag() == true)
                {
                    
                }

        // UPDATE FAMILIES

                // update shelving.rfa
                // update rod and shelf.rfa
                // update --kitchen counter--.rfa
                // update EL-No Base.rfa
                // update EL-Wall Base.rfa
                // update Lt-No Base.rfa

                if (curForm.GetCheckBoxFamilies() == true)
                {

                }

        // CORRECT LINESTYLES

                // update the <Centerline> line style line pattern to Center 1/8"

                // set some variables for line style & line pattern

                if (curForm.GetCheckBoxLinestyles() == true)
                {

                }

        // CLEAN THE EXTRA ELECTRICAL FAMILIES

                // remove electrical families with numbers at the end
                // i.e. EL-Wall Base 2

                // create lists to hold families

                List<Family> listEL_Wall = Utils.GetFamilyByNameContains(doc, "EL-Wall Base");
                List<Family> listEL_NoBase = Utils.GetFamilyByNameContains(doc, "EL-No Base");
                List<Family> listLT_NoBase = Utils.GetFamilyByNameContains(doc, "LT-No Base");

                if(curForm.GetCheckBoxElectrical()  == true)
                {

                }

                t.Commit();
            }            

            return Result.Succeeded;
        }        

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
