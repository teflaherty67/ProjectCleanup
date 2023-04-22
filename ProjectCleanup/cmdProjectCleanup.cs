#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

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

            string txtClient = curForm.GetComboxClient();
            string floorNum = curForm.GetComboxFloors();

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

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}
