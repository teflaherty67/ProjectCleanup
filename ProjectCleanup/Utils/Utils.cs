using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectCleanup
{
    internal static class Utils
    {
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

            foreach (View curView in m_colViews)
            {
                string viewCat = GetParameterValueByName(curView, "Category");

                if (viewCat == catName)
                    m_returnList.Add(curView);
            }

            return m_returnList;
        }

        internal static List<View> GetAllViewsByCategoryAndViewTemplate(Document doc, string catName, string vtName)
        {
            List<View> m_colViews = GetAllViewsByCategory(doc, catName);

            List<View> m_returnList = new List<View>();

            foreach (View curView in m_colViews)
            {
                ElementId vtId = curView.ViewTemplateId;

                if (vtId != ElementId.InvalidElementId)
                {
                    View vt = doc.GetElement(vtId) as View;

                    if (vt.Name == vtName)
                        m_returnList.Add(curView);
                }
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

        internal static List<ViewSchedule> GetScheduleByNameContains(Document doc, string scheduleString)
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

        public static List<ViewSheet> GetAllSheetsByCategory(Document doc, string categoryValue)
        {
            List<ViewSheet> m_sheets = new List<ViewSheet>();

            // Get all sheets in the project
            FilteredElementCollector sheetCollector = new FilteredElementCollector(doc);
            ICollection<Element> sheetElements = sheetCollector.OfClass(typeof(ViewSheet)).ToElements();

            // Iterate through each sheet and check if it has the specified category parameter with the value of "Inactive"
            foreach (Element sheetElement in sheetElements)
            {
                ViewSheet sheet = sheetElement as ViewSheet;
                if (sheet != null)
                {
                    // Get the category parameter of the sheet
                    Parameter categoryParameter = sheet.LookupParameter("Category");

                    // Check if the category parameter is valid and has the expected value
                    if (categoryParameter != null && categoryParameter.Definition.Name == "Category" &&
                        categoryParameter.AsValueString() == categoryValue)
                    {
                        m_sheets.Add(sheet);
                    }
                }
            }

            return m_sheets;
        }

        internal static List<ViewSheet> GetSheetsByGroupName(Document doc, string stringValue)
        {
            List<ViewSheet> m_viewSheets = GetAllSheets(doc);

            List<ViewSheet> m_returnGroups = new List<ViewSheet>();

            foreach (ViewSheet curSheet in m_viewSheets)
            {
                // Get the "Group" parameter of the sheet view
                Parameter groupParameter = curSheet.LookupParameter("Group");

                // Check for the "Group" parameter and add sheet tolist
                if (groupParameter != null && groupParameter.AsValueString() == stringValue)
                    m_returnGroups.Add(curSheet);
            }

            return m_returnGroups;
        }

        internal static List<string> GetAllSheetGroupsByCategory(Document doc, string categoryValue)
        {
            List<string> m_groups = new List<string>();

            // Get all sheet views in the project that have the specified category value
            List<ViewSheet> m_sheets = GetAllSheetsByCategory(doc, categoryValue);

            // Iterate through each sheet view and get the value of the "Group" parameter
            foreach (ViewSheet sheet in m_sheets)
            {
                // Get the "Group" parameter of the sheet view
                Parameter groupParameter = sheet.LookupParameter("Group");

                // Check if the "Group" parameter is valid and get its value
                if (groupParameter != null && groupParameter.Definition.Name == "Group")
                {
                    string groupValue = groupParameter.AsString();

                    // Check if the group value is not null or empty, and if it hasn't already been added to the list
                    if (!string.IsNullOrEmpty(groupValue) && !m_groups.Contains(groupValue))
                    {
                        m_groups.Add(groupValue);
                    }
                }
            }

            return m_groups;
        }        

        private static string GetParameterValueByName(Element elem, string paramName)
        {
            IList<Parameter> m_paramList = elem.GetParameters(paramName);

            if (m_paramList != null)
                try
                {
                    Parameter param = m_paramList[0];
                    string paramValue = param.AsValueString();
                    return paramValue;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    return null;
                }

            return "";
        }        

        internal static Parameter GetParameterByName(Element element, string paramName)
        {
            foreach (Parameter curParam in element.Parameters)
            {
                if (curParam.Definition.Name.ToString() == paramName)
                    return curParam;
            }

            return null;
        }        

        internal static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T foundChild = FindVisualChild<T>(child);
                    if (foundChild != null)
                        return foundChild;
                }
            }
            return null;
        }        
    }
}
