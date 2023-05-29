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

                #region Delete Sheet Groups

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

                #endregion

                #region Delete Unused Views

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

                if (curForm.GetCheckBoxViews() == true )
                {
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
                }               

                #endregion

                #region Delete Unused Schedules

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

                #endregion

                #region Rename Schedules

                // create lists for schedules by name contains
                List<ViewSchedule> veneerList = Utils.GetScheduleByNameContains(doc, "Exterior Veneer Calculations");
                List<ViewSchedule> floorList = Utils.GetScheduleByNameContains(doc, "Floor Areas");
                List<ViewSchedule> frameList = Utils.GetScheduleByNameContains(doc, "Frame Areas");
                List<ViewSchedule> atticList = Utils.GetScheduleByNameContains(doc, "Roof Ventilation Calculations");
                List<ViewSchedule> equipmentList = Utils.GetScheduleByNameContains(doc, "Roof Ventilation Equipment");
                List<ViewSchedule> indexList = Utils.GetScheduleByNameContains(doc, "Sheet Index");

                // create a counter for the schedules that will be renamed
                int countRenamed = 0;

                // create a counter for the schedules that will not be renamed
                int countEdit = 0;
                                
                if (curForm.GetCheckBoxSchedRename() == true)
                {
                    foreach (ViewSchedule curSchedule in veneerList)
                    {
                        // get the current name                        
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 28);
                        string schedElev = originalString.Substring(29);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }

                    foreach (ViewSchedule curSchedule in floorList)
                    {
                        // get the current name
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 11);
                        string schedElev = originalString.Substring(12);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }

                    foreach (ViewSchedule curSchedule in frameList)
                    {
                        // get the current name 
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 10);
                        string schedElev = originalString.Substring(11);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }

                    foreach (ViewSchedule curSchedule in atticList)
                    {
                        // get the current name 
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 29);
                        string schedElev = originalString.Substring(30);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }

                    foreach (ViewSchedule curSchedule in equipmentList)
                    {
                        // get the current name 
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 27);
                        string schedElev = originalString.Substring(28);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }

                    foreach (ViewSchedule curSchedule in indexList)
                    {
                        // get the current name 
                        string originalString = curSchedule.Name;

                        // split the name after the last word in the title
                        string schedTitle = originalString.Substring(0, 11);
                        string schedElev = originalString.Substring(12);

                        // get the index of the first character in the second string that is a letter
                        int elevIndex = Utils.GetIndexOfFirstLetter(schedElev);

                        // set the value of curElev to that letter
                        string curElev = schedElev.Substring(elevIndex, 1);

                        // if the letter doesn't equal "E"
                        if (curElev != "E")
                        {
                            try
                            {
                                // try to replace schedElev with "- Elevation " + curElev
                                curSchedule.Name = schedTitle + " - Elevation " + curElev;

                                // if successful, increment the renamed count by 1
                                countRenamed++;
                            }
                            catch (Exception)
                            {
                                // if unsuccessful, increment the edit count by 1
                                countEdit++;
                            }
                        }
                    }                   
                }

                #endregion

                #region Delete Code Bracing Parameter

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

                #endregion

                #region Remove Code Filter From Sheet Name

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

                #endregion

                #region Update Room Tag & Add Ceiling Height

                // set variables

                string paramClgName = "Ceiling Height";
                string tagPath = @"S:\Shared Folders\Lifestyle USA Design\Library 2023\Annotation\Tags\Room Tag.rfa";

                FamilyLoadOptions famRoomTagLoadOptions = new FamilyLoadOptions();

                Family family = null;


                if (curForm.GetCheckBoxRoomTag() == true)
                {
                    if (Utils.DoesProjectParamExist(doc, paramClgName))
                    {
                        return Result.Failed;
                    }
                    else
                    {
                        Utils.CreateSharedParam(doc, "Rooms", paramClgName, BuiltInCategory.OST_Rooms);
                    }

                    doc.LoadFamily(tagPath, famRoomTagLoadOptions, out family);
                }

                #endregion

                #region Update Families               

                // set variables for folder paths

                string pathElectrical = @"S:\Shared Folders\Lifestyle USA Design\Library 2023\Electrical";
                string pathKitchen = @"S:\Shared Folders\Lifestyle USA Design\Library 2023\Kitchen";
                string pathLighting = @"S:\Shared Folders\Lifestyle USA Design\Library 2023\Lighting";
                string pathShelving = @"S:\Shared Folders\Lifestyle USA Design\Library 2023\Shelf";

                // create lists for families to update

                List<string> famListElectrical = new List<string> { "EL-No Base.rfa", "EL-Wall Base.rfa" };
                List<string> famListKitchen = new List<string> { "--Kitchen Counter--.rfa" };
                List<string> famListLighting = new List<string> { "LT-No Base.rfa" };
                List<string> famListShelving = new List<string> { "Rod and Shelf.rfa", "Shelving.rfa" };                

                FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions();                

                if (curForm.GetCheckBoxFamilies() == true)
                {
                    foreach (string curFamString in famListElectrical)
                    {
                        string famPath = (pathElectrical + @"\" + curFamString);

                        //load the family

                        doc.LoadFamily(famPath, familyLoadOptions, out family);
                    }

                    foreach (string curFamString in famListKitchen)
                    {
                        string famPath = (pathKitchen + @"\" + curFamString);

                        // load the family

                        doc.LoadFamily(famPath, familyLoadOptions, out family);
                    }

                    foreach (string curFamString in famListLighting)
                    {
                        string famPath = (pathLighting + @"\" + curFamString);

                        // load the family

                        doc.LoadFamily(famPath, familyLoadOptions, out family);
                    }

                    foreach (string curFamString in famListShelving)
                    {
                        string famPath = (pathShelving + @"\" + curFamString);

                        // load the family

                        doc.LoadFamily(famPath, familyLoadOptions, out family);
                    }
                }

                #endregion

                #region Update Line Styles

                // get the line style called <Centerline>
                GraphicsStyle curCenterline = Utils.GetLinestyleByName(doc, "<Centerline>");

                // get the line pattern called: Center 1/8"
                LinePatternElement newCenterLP = Utils.GetLinePatternByName(doc, "Center 1/8\"");

                if (curForm.GetCheckBoxLinestyles() == true)
                {
                    curCenterline.GraphicsStyleCategory.SetLinePatternId(newCenterLP.Id,
                        GraphicsStyleType.Projection);
                }

                #endregion

                #region Remove Duplicate Electrical & Lighting Families

                // remove electrical families with numbers at the end
                // i.e. EL-Wall Base 2

                // create lists to hold families

                List<Family> listEL_Wall = Utils.GetFamilyByNameContains(doc, "EL-Wall Base");
                List<Family> listEL_NoBase = Utils.GetFamilyByNameContains(doc, "EL-No Base");
                List<Family> listLT_NoBase = Utils.GetFamilyByNameContains(doc, "LT-No Base");

                if (curForm.GetCheckBoxElectrical() == true)
                {

                }

                #endregion

                // TaskDialog.Show("Complete", countRenamed.ToString() + " schedules will be renamed. "
                       // + countEdit.ToString() + " schedules would have duplicate names & can not be renamed.");

                t.Commit();
            }

            return Result.Succeeded;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }

        public class FamilyLoadOptions : IFamilyLoadOptions
        {
            #region IFamilyLoadOptions Members

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (!familyInUse)
                {
                    TaskDialog.Show("SampleFamilyLoadOptions", "The family has not been in use and will keep loading.");

                    overwriteParameterValues = true;
                    return true;
                }
                else
                {
                    TaskDialog.Show("SampleFamilyLoadOptions", "The family has been in use but will still be loaded with existing parameters overwritten.");

                    overwriteParameterValues = true;
                    return true;
                }
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (!familyInUse)
                {
                    TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has not been in use and will keep loading.");

                    source = FamilySource.Family;
                    overwriteParameterValues = true;
                    return true;
                }
                else
                {
                    TaskDialog.Show("SampleFamilyLoadOptions", "The shared family has been in use but will still be loaded from the FamilySource with existing parameters overwritten.");

                    source = FamilySource.Family;
                    overwriteParameterValues = true;
                    return true;
                }
            }

            #endregion
        }
    }

}
