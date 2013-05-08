using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace NetSynth
{
	public static class Extensions
	{
		internal static List<T> GetFieldsAndProperties<T>(this object baseObject)
		{
			if (baseObject == null)
				return new List<T>();

			var props = baseObject.GetType().GetProperties().Where(x => x.PropertyType == typeof(T));
			var fields = baseObject.GetType().GetFields().Where(x => x.FieldType == typeof(T));
			var propValues = props.Select(x => (T)x.GetValue(baseObject, null)).ToList();
			var fieldValues = fields.Select(x => (T)x.GetValue(baseObject)).ToList();
			propValues.AddRange(fieldValues);
			return propValues;
		}

		public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
		{
			List<T> logicalCollection = new List<T>();
			GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
			return logicalCollection;
		}
		private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
		{
			// WARNING: Evaluating the query in debug mode (reading the value) means the IEnumerable runs through the iterator to the end
			// using the debugger to view values fucks with EXECUTION!!

			IEnumerable children = LogicalTreeHelper.GetChildren(parent);
			foreach (object child in children)
			{
				if (child is DependencyObject)
				{
					DependencyObject depChild = child as DependencyObject;
					if (child is T)
					{
						logicalCollection.Add(child as T);
					}
					GetLogicalChildCollection(depChild, logicalCollection);
				}
			}
		}
	}
}
