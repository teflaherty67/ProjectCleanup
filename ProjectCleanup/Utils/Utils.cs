#region Namespaces

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

#endregion

namespace ProjectCleanup
{
    internal static class Utils
    {

        #region Families


        public static List<Family> GetAllFamilies(Document doc)
        {
            List<Family> m_returnList = new List<Family>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Family));

            foreach (Family family in collector)
            {
                m_returnList.Add(family);
            }

            return m_returnList;
        }

        public static List<Family> GetFamilyByNameContains(Document doc, string familyName)
        {
            List<Family> m_famList = GetAllFamilies(doc);

            List<Family> m_returnList = new List<Family>();

            //loop through family symbols in current project and look for a match
            foreach (Family curFam in m_famList)
            {
                if (curFam.Name.Contains(familyName))
                {
                    m_returnList.Add(curFam);
                }
            }

            return m_returnList;
        }

        #endregion

        #region Parameters

        public struct ParameterData
        {
            public Definition def;
            public ElementBinding binding;
            public string name;
            public bool IsSharedStatusKnown;
            public bool IsShared;
            public string GUID;
            public ElementId id;
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

        public static List<ParameterData> GetAllProjectParameters(Document curDoc)
        {
            if (curDoc.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "Cannot be a family document.");
                return null;
            }

            List<ParameterData> paraList = new List<ParameterData>();

            BindingMap map = curDoc.ParameterBindings;
            DefinitionBindingMapIterator iter = map.ForwardIterator();
            iter.Reset();
            while (iter.MoveNext())
            {
                ParameterData pd = new ParameterData();
                pd.def = iter.Key;
                pd.name = iter.Key.Name;
                pd.binding = iter.Current as ElementBinding;
                paraList.Add(pd);
            }

            return paraList;
        }

        public static bool DoesProjectParamExist(Document curDoc, string pName)
        {
            List<ParameterData> pdList = GetAllProjectParameters(curDoc);
            foreach (ParameterData pd in pdList)
            {
                if (pd.name == pName)
                {
                    return true;
                }
            }
            return false;
        }
        public static void CreateSharedParam(Document curDoc, string groupName, string paramName, BuiltInCategory cat)
        {
            Definition curDef = null;

            //check if current file has shared param file - if not then exit
            DefinitionFile defFile = curDoc.Application.OpenSharedParameterFile();

            //check if file has shared parameter file
            if (defFile == null)
            {
                TaskDialog.Show("Error", "No shared parameter file.");
                //Throw New Exception("No Shared Parameter File!")
            }

            //check if shared parameter exists in shared param file - if not then create
            if (ParamExists(defFile.Groups, groupName, paramName) == false)
            {
                //create param
                curDef = AddParamToFile(defFile, groupName, paramName);
            }
            else
            {
                curDef = GetParameterDefinitionFromFile(defFile, groupName, paramName);
            }

            //check if param is added to views - if not then add
            if (ParamAddedToFile(curDoc, paramName) == false)
            {
                //add parameter to current Revitfile
                AddParamToDocument(curDoc, curDef, cat);
            }
        }


        private static Definition GetParameterDefinitionFromFile(DefinitionFile defFile, string groupName, string paramName)
        {
            // iterate the Definition groups of this file
            foreach (DefinitionGroup group in defFile.Groups)
            {
                if (group.Name == groupName)
                {
                    // iterate the difinitions
                    foreach (Definition definition in group.Definitions)
                    {
                        if (definition.Name == paramName)
                            return definition;
                    }
                }
            }
            return null;
        }

        //check if specified parameter is already added to Revit file
        public static bool ParamAddedToFile(Document curDoc, string paramName)
        {
            foreach (Parameter curParam in curDoc.ProjectInformation.Parameters)
            {
                if (curParam.Definition.Name.Equals(paramName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AddParamToDocument(Document curDoc, Definition curDef, BuiltInCategory cat)
        {
            bool paramAdded = false;

            //define category for shared param
            Category myCat = curDoc.Settings.Categories.get_Item(cat);
            CategorySet myCatSet = curDoc.Application.Create.NewCategorySet();
            myCatSet.Insert(myCat);

            //create binding
            ElementBinding curBinding = curDoc.Application.Create.NewInstanceBinding(myCatSet);

            //do something
            paramAdded = curDoc.ParameterBindings.Insert(curDef, curBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);
            
            return paramAdded;
        }


        //check if specified parameter exists in shared parameter file
        public static bool ParamExists(DefinitionGroups groupList, string groupName, string paramName)
        {
            //loop through groups and look for match
            foreach (DefinitionGroup curGroup in groupList)
            {
                if (curGroup.Name.Equals(groupName) == true)
                {
                    //check if param exists
                    foreach (Definition curDef in curGroup.Definitions)
                    {
                        if (curDef.Name.Equals(paramName))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //add parameter to specified shared parameter file
        public static Definition AddParamToFile(DefinitionFile defFile, string groupName, string paramName)
        {
            //create new shared parameter in specified file
            DefinitionGroup defGroup = GetDefinitionGroup(defFile, groupName);

            //check if group exists - if not then create
            if (defGroup == null)
            {
                //create group
                defGroup = defFile.Groups.Create(groupName);
            }

            //create parameter in group
            ExternalDefinitionCreationOptions curOptions = new ExternalDefinitionCreationOptions(paramName, SpecTypeId.String.Text);
            curOptions.Visible = true;

            Definition newParam = defGroup.Definitions.Create(curOptions);

            return newParam;
        }

        public static DefinitionGroup GetDefinitionGroup(DefinitionFile defFile, string groupName)
        {
            //loop through groups and look for match
            foreach (DefinitionGroup curGroup in defFile.Groups)
            {
                if (curGroup.Name.Equals(groupName))
                {
                    return curGroup;
                }
            }

            return null;
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

        #endregion

        #region Sheets

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

        #endregion

        #region Parameters

        public struct ParameterData
        {
            public Definition def;
            public ElementBinding binding;
            public string name;
            public bool IsSharedStatusKnown;
            public bool IsShared;
            public string GUID;
            public ElementId id;
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

        #endregion


        #region Views


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

        #endregion

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

        #endregion

        #region Schedules

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

        #endregion

        #region Sheets

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

        #endregion

        #region String

        internal static int GetIndexOfFirstLetter(string schedTitle)
        {
            var index = 0;
            foreach (var c in schedTitle)
                if (char.IsLetter(c))
                    return index;
                else
                    index++;

            return schedTitle.Length;
        }

        #endregion

        #region Views

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

        #endregion               

        #region Lines

        internal static LinePatternElement GetLinePatternByName(Document curDoc, string typeName)
        {
            if (typeName != null)
                return LinePatternElement.GetLinePatternElementByName(curDoc, typeName);
            else
                return null;
        }

        internal static GraphicsStyle GetLinestyleByName(Document curDoc, string styleName)
        {
            GraphicsStyle retlinestyle = null;

            FilteredElementCollector gstylescollector = new FilteredElementCollector(curDoc);
            gstylescollector.OfClass(typeof(GraphicsStyle));

            foreach (Element element in gstylescollector)
            {
                GraphicsStyle curLS = element as GraphicsStyle;

                if (curLS.Name == styleName)
                    retlinestyle = curLS;
            }

            return retlinestyle;
        }

        #endregion

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
