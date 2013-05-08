using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace NetSynth
{
	public static class Serializer
	{
		public static string Inject(this string formatString, object injectionObject)
		{
			return formatString.Inject(GetPropertyHash(injectionObject));
		}

		public static string Inject(this string formatString, IDictionary dictionary)
		{
			return formatString.Inject(new Hashtable(dictionary));
		}

		public static string Inject(this string formatString, Hashtable attributes)
		{
			string result = formatString;
			if (attributes == null || formatString == null)
				return result;

			foreach (string attributeKey in attributes.Keys)
			{
				result = result.InjectSingleValue(attributeKey, attributes[attributeKey]);
			}
			return result;
		}

		public static string InjectSingleValue(this string formatString, string key, object replacementValue)
		{
			string result = formatString;
			//regex replacement of key with value, where the generic key format is:
			//Regex foo = new Regex("{(foo)(?:}|(?::(.[^}]*)}))");
			Regex attributeRegex = new Regex("{(" + key + ")(?:}|(?::(.[^}]*)}))");  //for key = foo, matches {foo} and {foo:SomeFormat}

			//loop through matches, since each key may be used more than once (and with a different format string)
			foreach (Match m in attributeRegex.Matches(formatString))
			{
				string replacement = m.ToString();
				if (m.Groups[2].Length > 0) //matched {foo:SomeFormat}
				{
					//do a double string.Format - first to build the proper format string, and then to format the replacement value
					string attributeFormatString = string.Format(CultureInfo.InvariantCulture, "{{0:{0}}}", m.Groups[2]);
					replacement = string.Format(CultureInfo.CurrentCulture, attributeFormatString, replacementValue);
				}
				else //matched {foo}
				{
					replacement = (replacementValue ?? string.Empty).ToString();
				}
				//perform replacements, one match at a time
				result = result.Replace(m.ToString(), replacement);  //attributeRegex.Replace(result, replacement, 1);
			}
			return result;

		}

		private static Hashtable GetPropertyHash(object properties)
		{
			Hashtable values = null;
			if (properties != null)
			{
				values = new Hashtable();
				PropertyDescriptorCollection props = TypeDescriptor.GetProperties(properties);
				foreach (PropertyDescriptor prop in props)
				{
					values.Add(prop.Name, prop.GetValue(properties));
				}
			}
			return values;
		}

		public static string SerializeToXML(object data)
		{
			XmlSerializer serializer = new XmlSerializer(data.GetType());
			StringWriter sw = new StringWriter();
			serializer.Serialize(sw, data);
			return sw.ToString();
		}

		public static object DeserializeToXML(string root, Type outType)
		{
			XmlSerializer serializer = new XmlSerializer(outType);
			StringReader sr = new StringReader(root);
			var rootDir = serializer.Deserialize(sr);
			return rootDir;
		}
	}
}
