using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject {

    public static class StringExtensions {

        public static string HorizontalConcat (this string left, string right) {
            return HorizontalConcat(left, right, string.Empty);
        }

        public static string HorizontalConcat (this string left, string right, string separator) {
            var leftLines = left.Split(System.Environment.NewLine);
            var rightLines = right.Split(System.Environment.NewLine);
            var maxLines = Math.Max(leftLines.Length, rightLines.Length);
            var longestLeftLineLength = 0;
            foreach (var line in leftLines) {
                longestLeftLineLength = Math.Max(longestLeftLineLength, line.Length);
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < maxLines; i++) {
                if (i < leftLines.Length) {
                    sb.Append(leftLines[i]);
                    sb.Append(new string(' ', longestLeftLineLength - leftLines[i].Length));
                } else {
                    sb.Append(new string(' ', longestLeftLineLength));
                }
                sb.Append(separator);
                if (i < rightLines.Length) {
                    sb.Append(rightLines[i]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }

}
