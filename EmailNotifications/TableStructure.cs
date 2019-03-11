using System;
using System.Collections.Generic;
using System.Text;

namespace EmailNotifications
{

    public static class TableStructure
    {
        public class Table : HtmlBase, IDisposable
        {
            public Table(StringBuilder sb, string classAttributes = "", string id = "", string align = "") : base(sb)
            {
                Append("<table" + $" align=\"{align}\"");
                AddOptionalAttributes(classAttributes, id);
            }

            public void StartHead(string classAttributes = "", string id = "")
            {
                Append("<thead");
                AddOptionalAttributes(classAttributes, id);
            }

            public void EndHead()
            {
                Append("</thead>");
            }

            public void StartFoot(string classAttributes = "", string id = "")
            {
                Append("<tfoot");
                AddOptionalAttributes(classAttributes, id);
            }

            public void EndFoot()
            {
                Append("</tfoot>");
            }

            public void StartBody(string classAttributes = "", string id = "")
            {
                Append("<tbody");
                AddOptionalAttributes(classAttributes, id);
            }

            public void EndBody()
            {
                Append("</tbody>");
            }

            public void Dispose()
            {
                Append("</table>");
            }

            public Row AddRow(string classAttributes = "", string id = "")
            {
                return new Row(GetBuilder(), classAttributes, id);
            }
        }

        public class Row : HtmlBase, IDisposable
        {
            public Row(StringBuilder sb, string classAttributes = "", string id = "") : base(sb)
            {
                Append("<tr");
                AddOptionalAttributes(classAttributes, id);
            }
            public void Dispose()
            {
                Append("</tr>");
            }
            public void AddCell(string innerText, string classAttributes = "", string id = "", string colSpan = "", string align = "", string style = "", string fontSize = "", bool url = false)
            {
                string txt = "";
                if(innerText == "")
                {
                    txt = "<b>Logged As Resolved</b>";
                }
                else
                {
                    txt = "<b>Resolve the issue then</b><br><b>Log it here</b>";
                }
                Append("<td");
                AddOptionalAttributes(classAttributes, id, colSpan, align, style); //put style value in here
                if (url == false)
                {
                    Append("<font");
                    AppendOptionFont(classAttributes, id, fontSize, align);
                    Append(innerText);
                    Append("</font>");
                }
                else
                {
                    Append("<a style='color:black;' href=" + innerText + ">");
                    Append("<font");
                    AppendOptionFont(classAttributes, id, fontSize, align);
                    Append(txt);
                    Append("</font>");
                    Append("</a>");
                }

                Append("</td>");
            }
            public void AddImage(string source, string classAttributes = "", string style = "", string id = "", string align = "", string sizeX = "", string sizeY = "")
            {
                Append("<td");
                AddOptionalAttributes(classAttributes, id, align);
                Append("<img");
                AddOptionImage(source, id, style, sizeX, sizeY);
                Append("</img>");
                Append("</td>");
            }
        }

        public abstract class HtmlBase
        {
            private StringBuilder _sb;

            protected HtmlBase(StringBuilder sb)
            {
                _sb = sb;
            }

            public StringBuilder GetBuilder()
            {
                return _sb;
            }

            protected void Append(string toAppend)
            {
                _sb.Append(toAppend);
            }

            protected void AppendOptionFont(string className = "", string id = "", string fontSize = "", string align = "")
            {

                if (!string.IsNullOrEmpty(id))
                {
                    _sb.Append($" id=\"{id}\"");
                }
                if (!string.IsNullOrEmpty(className))
                {
                    _sb.Append($" class=\"{className}\"");
                }
                if (!string.IsNullOrEmpty(fontSize))
                {
                    _sb.Append($" size=\"{fontSize}\"");
                }
                if (!string.IsNullOrEmpty(align))
                {
                    _sb.Append($" align=\"{align}\"");
                }
                _sb.Append(">");
            }

            protected void AddOptionalAttributes(string className = "", string id = "", string colSpan = "", string align = "", string style = "")
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _sb.Append($" id=\"{id}\"");
                }
                if (!string.IsNullOrEmpty(className))
                {
                    _sb.Append($" class=\"{className}\"");
                }
                if (!string.IsNullOrEmpty(colSpan))
                {
                    _sb.Append($" colspan=\"{colSpan}\"");
                }
                if (!string.IsNullOrEmpty(align))
                {
                    _sb.Append($" align=\"{align}\"");
                }
                if (!string.IsNullOrEmpty(style))
                {
                    _sb.Append($" style=\"{style}\"");
                }
                _sb.Append(">");
            }
            protected void AddOptionImage(string source, string id = "", string style = "", string align = "", string sizeX = "", string sizeY = "")
            {

                if (!string.IsNullOrEmpty(id))
                {
                    _sb.Append($" id=\"{source}\"");
                }
                if (!string.IsNullOrEmpty(style))
                {
                    _sb.Append($" style=\"{style}\"");
                }
                if (!string.IsNullOrEmpty(sizeX))
                {
                    _sb.Append($" width=\"{sizeX}\"");
                }
                if (!string.IsNullOrEmpty(sizeY))
                {
                    _sb.Append($" height=\"{sizeY}\"");
                }
                if (!string.IsNullOrEmpty(align))
                {
                    _sb.Append($" align=\"{align}\"");
                }
                if (!string.IsNullOrEmpty(source))
                {
                    _sb.Append($" src=\"{source}\"");
                }
                _sb.Append(">");
            }
        }
    }
}

