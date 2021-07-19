using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Utilities{
	public static class LINQExtensions {
		public static T FirstOr<T>(this IEnumerable<T>? source, T alternative) {
			if (source == null) {
				throw new ArgumentNullException(nameof(source));
			}

			if (source is IList<T> list) {
				if (list.Count > 0) {
					return list[0];
				}
			}
			else {
				using (IEnumerator<T> e = source.GetEnumerator()) {
					if (e.MoveNext()) {
						return e.Current;
					}
				}
			}

			return alternative;
		}
	}
}