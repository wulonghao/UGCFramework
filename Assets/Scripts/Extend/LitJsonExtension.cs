using UnityEngine;
using System.Collections;


namespace LitJson
{
	public static class LitJsonExtension
	{
		public static string TryGetString (this JsonData self, string key, string defvalue = "")
		{
			if (!self.IsObject) {
				return defvalue;
			}

			if (((IDictionary)self).Contains (key)) {
				if (self [key] == null)
					return "";
				return self [key].ToString ();
			}
			return defvalue;
		}
	}
}