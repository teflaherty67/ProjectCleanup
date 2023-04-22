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

            // put any code needed for the form here

            List<string> grpName = GetAllSheetGroups(doc);

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
                
                t.Commit();
            }            

            return Result.Succeeded;
        }

        private List<string> GetAllSheetGroups(Document doc)
        {
            List<ViewSheet> sheetList = GetAllSheets(doc);

            List<string> m_returnList = new List<string>();

            foreach (ViewSheet curSheet in sheetList)
            {
                Parameter grpName = GetParameterByName(curSheet, "Group");

                if (m_returnList.Contains(grpName.ToString()))
                    continue;
                else m_returnList.Add(grpName.ToString());

                m_returnList.Add(grpName.ToString());
            }            

            return m_returnList;
        }

        public static Parameter GetParameterByName(ViewSheet curSheet, string paramName)
        {
            foreach (Parameter curParam in curSheet.Parameters)
            {
                if (curParam.Definition.Name.ToString() == paramName)
                    return curParam;
            }

            return null;
        }

        private List<ViewSheet> GetAllSheets(Document doc)
        {
            //get all sheets
            FilteredElementCollector m_colViews = new FilteredElementCollector(doc);
            m_colViews.OfCategory(BuiltInCategory.OST_Sheets);

            List<ViewSheet> m_sheets = new List<ViewSheet>();
            foreach (ViewSheet x in m_colViews.ToElements())
            {
                m_sheets.Add(x);
            }

            return m_sheets;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
