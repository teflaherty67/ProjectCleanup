using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        internal static List<string> GetAllSheetGroupsByCategory(Document doc, string catName)
        {
            // get all the sheets in the project
            List<ViewSheet> m_sheetList = GetAllSheets(doc);

            // create list for sheets in specified category
            List<ViewSheet> m_returnSheets = new List<ViewSheet>();

            // get the value for the parameter
            string paramName = GetParameterValueByName(ViewSheet, BuiltInParameter.ELEM_CATEGORY_PARAM);
            
            // create a return list
            List<string> m_returnList = new List<string>();

            // loop through the sheets and find ones in the specified category
            foreach(ViewSheet curSheet in m_sheetList)
            {
                if (paramName == catName)
                    m_returnSheets.Add(curSheet);

                string groupName = GetParameterValueByName(ViewSheet, "Group");

                // for each sheet found get the value of the group parameter
                foreach (ViewSheet viewSheet in m_sheetList)

                    // add each group name to the list
                    m_returnList.Add(groupName);                    
            }         

            // return the list
            return m_returnList.Distinct().ToList();            
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
            List<View> m_colViews = GetAllViews(doc);

            List<View> m_returnList = new List<View>();

            string viewCat = GetParameterValueByName(ViewType, "Category");

            foreach (View curView in m_colViews)
            {

            }
        }

        private static string GetParameterValueByName(ViewType viewType, string paramName)
        {
            IList<Parameter> paramList = ViewType.GetParameters(paramName);

            if (paramList != null)
                try
                {
                    Parameter param = paramList[0];
                    string paramValue = param.AsValueString();
                    return paramValue;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    return null;
                }

            return "";
        }

        private static string GetParameterValueByName(ViewSheet viewType, string paramName)
        {
            IList<Parameter> paramList = ViewSheet.GetParameters(paramName);

            if (paramList != null)
                try
                {
                    Parameter param = paramList[0];
                    string paramValue = param.AsValueString();
                    return paramValue;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    return null;
                }

            return "";
        }

        internal static List<View> GetAllViewsByCategoryAndViewTemplate(Document doc, string catName, string vtName)
        {
            List<View> m_colViews = GetAllViews(doc);

            List<View> m_returnList = new List<View>();

            foreach (View curView in m_colViews)
            {

            }
        }

        internal static Parameter GetParameterByName(Document doc, string paramName)
        {
            foreach (Parameter curParam in doc.Parameters)
            {
                if (curParam.Definition.Name.ToString() == paramName)
                    return curParam;
            }

            return null;
        }
    }
}
