using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectCleanup
{
    internal static class Utils
    {  
        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel currentPanel = GetRibbonPanelByName(app, tabName, panelName);

            if (currentPanel == null)
                currentPanel = app.CreateRibbonPanel(tabName, panelName);

            return currentPanel;
        }

        internal static RibbonPanel GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }

        internal static List<ViewSheet> GetAllSheets(Document doc)
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

        internal static List<string> GetAllSheetGroupsByCategory(Document doc, string v)
        {
            throw new NotImplementedException();
        }

        internal static List<View> GetAllViews(Document doc)
        {
            {
                FilteredElementCollector m_colviews = new FilteredElementCollector(doc);
                m_colviews.OfCategory(BuiltInCategory.OST_Views);

                List<View> m_views = new List<View>();
                foreach (View x in m_colviews.ToElements())
                {
                    m_views.Add(x);
                }

                return m_views;
            }
        }

        internal static List<View> GetAllViewsByCategory(Document doc, string catName)
        {
            throw new NotImplementedException();
        }

        internal static List<View> GetAllViewsByCategoryAndViewTemplate(Document doc, string v1, string v2)
        {
            throw new NotImplementedException();
        }
    }
}
