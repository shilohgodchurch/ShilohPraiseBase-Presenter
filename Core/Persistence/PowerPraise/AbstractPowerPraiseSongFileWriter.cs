﻿/*
 *   PraiseBase Presenter
 *   The open source lyrics and image projection software for churches
 *
 *   http://praisebase.org
 *
 *   This program is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU General Public License
 *   as published by the Free Software Foundation; either version 2
 *   of the License, or (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program; if not, write to the Free Software
 *   Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 *
 */

using System.Collections.Generic;
using System.Xml;
using PraiseBase.Presenter.Model;

namespace PraiseBase.Presenter.Persistence.PowerPraise
{
    public abstract class AbstractPowerPraiseSongFileWriter<T> : ISongFileWriter<T> where T : ISongFile
    {
        protected const string SupportedFileFormatVersion = "3.0";
        protected const string XmlRootNodeName = "ppl";

        public string GetFileExtension()
        {
            return ".ppl";
        }

        public string GetFileTypeDescription()
        {
            return "PowerPraise Lied";
        }

        public abstract void Save(string filename, T sng);

        /// <summary>
        ///     Parses additional fields (hook)
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="xmlRoot"></param>
        /// <param name="sng"></param>
        protected abstract void WriteAdditionalFields(XmlDocument xmlDoc, XmlElement xmlRoot, PowerPraiseSong sng);

        public void Write(string filename, PowerPraiseSong sng)
        {
            var xml = new XmlWriterHelper(XmlRootNodeName, SupportedFileFormatVersion);
            var xmlRoot = xml.Root;
            var xmlDoc = xml.Doc;

            xmlRoot.AppendChild(xmlDoc.CreateElement("general"));

            // Title
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("title"));
            xmlRoot["general"]["title"].InnerText = sng.Title;

            // Category
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("category"));
            xmlRoot["general"]["category"].InnerText = !string.IsNullOrEmpty(sng.Category)
                ? sng.Category
                : PowerPraiseConstants.NoCategory;

            // Language
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("language"));
            xmlRoot["general"]["language"].InnerText = !string.IsNullOrEmpty(sng.Language)
                ? sng.Language
                : PowerPraiseConstants.Language;

            // Write additional fields
            WriteAdditionalFields(xmlDoc, xmlRoot, sng);

            xmlRoot.AppendChild(xmlDoc.CreateElement("songtext"));

            // Dictionary of backgrouns
            var bgIndex = 0;
            var backgrounds = new Dictionary<string, int>();

            // Song parts
            foreach (var prt in sng.Parts)
            {
                var tn = xmlDoc.CreateElement("part");

                // Caption
                tn.SetAttribute("caption", prt.Caption);

                foreach (var sld in prt.Slides)
                {
                    var tn2 = xmlDoc.CreateElement("slide");

                    // Slide-specific text size
                    var mainsize = sld.MainSize > 0 ? sld.MainSize : (int) sng.Formatting.MainText.Font.Size;
                    tn2.SetAttribute("mainsize", mainsize.ToString());

                    // Backgound number
                    var bg = PowerPraiseFileUtil.MapBackground(sld.Background) ??
                             PowerPraiseFileUtil.MapBackground(PowerPraiseConstants.DefaultBackground);
                    int backgroundNr;
                    if (!backgrounds.ContainsKey(bg))
                    {
                        backgroundNr = bgIndex;
                        backgrounds.Add(bg, bgIndex++);
                    }
                    else
                    {
                        backgroundNr = backgrounds[bg];
                    }
                    tn2.SetAttribute("backgroundnr", backgroundNr.ToString());

                    // Lyrics
                    foreach (var ln in sld.Lines)
                    {
                        var tn3 = xmlDoc.CreateElement("line");
                        tn3.InnerText = ln;
                        tn2.AppendChild(tn3);
                    }
                    foreach (var ln in sld.Translation)
                    {
                        var tn3 = xmlDoc.CreateElement("translation");
                        tn3.InnerText = ln;
                        tn2.AppendChild(tn3);
                    }
                    tn.AppendChild(tn2);
                }
                xmlRoot["songtext"].AppendChild(tn);
            }

            // Order
            xmlRoot.AppendChild(xmlDoc.CreateElement("order"));
            foreach (var prt in sng.Order)
            {
                var tn = xmlDoc.CreateElement("item");
                tn.InnerText = prt.Caption;
                xmlRoot["order"].AppendChild(tn);
            }

            xmlRoot.AppendChild(xmlDoc.CreateElement("information"));

            // Copyright
            xmlRoot["information"].AppendChild(xmlDoc.CreateElement("copyright"));

            // Copyright position
            xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("position"));
            xmlRoot["information"]["copyright"]["position"].InnerText =
                MapAdditionalInformationPosition(sng.Formatting.CopyrightTextPosition);

            // Copyright text
            xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("text"));
            if (sng.CopyrightText != null && !string.IsNullOrEmpty(string.Join("", sng.CopyrightText.ToArray())))
            {
                foreach (var l in sng.CopyrightText)
                {
                    var n = xmlRoot["information"]["copyright"]["text"].AppendChild(xmlDoc.CreateElement("line"));
                    n.InnerText = l;
                }
            }

            // Source
            xmlRoot["information"].AppendChild(xmlDoc.CreateElement("source"));

            // Source enabled
            xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("position"));
            xmlRoot["information"]["source"]["position"].InnerText =
                MapAdditionalInformationPosition(sng.Formatting.SourceTextPosition);

            // Source text
            xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("text"));
            if (!string.IsNullOrEmpty(sng.SourceText))
            {
                xmlRoot["information"]["source"]["text"].AppendChild(xmlDoc.CreateElement("line"));
                xmlRoot["information"]["source"]["text"]["line"].InnerText = sng.SourceText;
            }

            xmlRoot.AppendChild(xmlDoc.CreateElement("formatting"));
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("font"));

            // Font formatting
            ApplyFormatting(xmlDoc, xmlRoot["formatting"]["font"], "maintext", sng.Formatting.MainText);
            ApplyFormatting(xmlDoc, xmlRoot["formatting"]["font"], "translationtext", sng.Formatting.TranslationText);
            ApplyFormatting(xmlDoc, xmlRoot["formatting"]["font"], "copyrighttext", sng.Formatting.CopyrightText);
            ApplyFormatting(xmlDoc, xmlRoot["formatting"]["font"], "sourcetext", sng.Formatting.SourceText);

            // Outline
            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("enabled"));
            xmlRoot["formatting"]["font"]["outline"]["enabled"].InnerText = sng.Formatting.Outline.Enabled
                ? "true"
                : "false";
            xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["outline"]["color"].InnerText =
                PowerPraiseFileUtil.ConvertColor(sng.Formatting.Outline.Color).ToString();

            // Shadow
            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("enabled"));
            xmlRoot["formatting"]["font"]["shadow"]["enabled"].InnerText = sng.Formatting.Shadow.Enabled
                ? "true"
                : "false";
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["shadow"]["color"].InnerText =
                PowerPraiseFileUtil.ConvertColor(sng.Formatting.Shadow.Color).ToString();
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("direction"));
            xmlRoot["formatting"]["font"]["shadow"]["direction"].InnerText =
                sng.Formatting.Shadow.Direction.ToString();

            // Backgrounds
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("background"));
            if (backgrounds.Count == 0)
            {
                backgrounds.Add(PowerPraiseFileUtil.MapBackground(PowerPraiseConstants.DefaultBackground), 0);
            }
            foreach (var bg in backgrounds.Keys)
            {
                var tn = xmlDoc.CreateElement("file");
                tn.InnerText = bg;
                xmlRoot["formatting"]["background"].AppendChild(tn);
            }

            // Linespacing
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("linespacing"));
            xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("main"));
            xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("translation"));
            xmlRoot["formatting"]["linespacing"]["main"].InnerText =
                (sng.Formatting.MainLineSpacing > 0
                    ? sng.Formatting.MainLineSpacing
                    : PowerPraiseConstants.Format.MainLineSpacing).ToString();
            xmlRoot["formatting"]["linespacing"]["translation"].InnerText =
                (sng.Formatting.MainLineSpacing > 0
                    ? sng.Formatting.TranslationLineSpacing
                    : PowerPraiseConstants.Format.TranslationLineSpacing)
                    .ToString();

            // Orientation
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("textorientation"));

            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("horizontal"));
            switch (
                sng.Formatting.TextOrientation != null
                    ? sng.Formatting.TextOrientation.Horizontal
                    : HorizontalOrientation.Center)
            {
                case HorizontalOrientation.Left:
                    xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "left";
                    break;

                case HorizontalOrientation.Center:
                    xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "center";
                    break;

                case HorizontalOrientation.Right:
                    xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "right";
                    break;
            }

            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("vertical"));
            switch (
                sng.Formatting.TextOrientation != null
                    ? sng.Formatting.TextOrientation.Vertical
                    : VerticalOrientation.Middle)
            {
                case VerticalOrientation.Top:
                    xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "top";
                    break;

                case VerticalOrientation.Middle:
                    xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "center";
                    break;

                case VerticalOrientation.Bottom:
                    xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "bottom";
                    break;
            }
            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("transpos"));
            xmlRoot["formatting"]["textorientation"]["transpos"].InnerText = "inline";

            // Borders
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("borders"));
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainleft"));
            xmlRoot["formatting"]["borders"]["mainleft"].InnerText = sng.Formatting.Borders.TextLeft.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("maintop"));
            xmlRoot["formatting"]["borders"]["maintop"].InnerText = sng.Formatting.Borders.TextTop.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainright"));
            xmlRoot["formatting"]["borders"]["mainright"].InnerText = sng.Formatting.Borders.TextRight.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainbottom"));
            xmlRoot["formatting"]["borders"]["mainbottom"].InnerText = sng.Formatting.Borders.TextBottom.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("copyrightbottom"));
            xmlRoot["formatting"]["borders"]["copyrightbottom"].InnerText =
                sng.Formatting.Borders.CopyrightBottom.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourcetop"));
            xmlRoot["formatting"]["borders"]["sourcetop"].InnerText = sng.Formatting.Borders.SourceTop.ToString();
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourceright"));
            xmlRoot["formatting"]["borders"]["sourceright"].InnerText = sng.Formatting.Borders.SourceRight.ToString();

            xml.Write(filename);
        }

        private string MapAdditionalInformationPosition(AdditionalInformationPosition value)
        {
            var pos = "firstslide";
            switch (value)
            {
                case AdditionalInformationPosition.FirstSlide:
                    pos = "firstslide";
                    break;
                case AdditionalInformationPosition.LastSlide:
                    pos = "lastslide";
                    break;
                case AdditionalInformationPosition.None:
                    pos = "none";
                    break;
            }
            return pos;
        }

        private void ApplyFormatting(XmlDocument xmlDoc, XmlElement elem, string key,
            PowerPraiseSongFormatting.FontFormatting f)
        {
            elem.AppendChild(xmlDoc.CreateElement(key));
            elem[key].AppendChild(xmlDoc.CreateElement("name"));
            elem[key]["name"].InnerText = f.Font.Name;
            elem[key].AppendChild(xmlDoc.CreateElement("size"));
            elem[key]["size"].InnerText = ((int) f.Font.Size).ToString();
            elem[key].AppendChild(xmlDoc.CreateElement("bold"));
            elem[key]["bold"].InnerText = (f.Font.Bold).ToString().ToLower();
            elem[key].AppendChild(xmlDoc.CreateElement("italic"));
            elem[key]["italic"].InnerText = (f.Font.Italic).ToString().ToLower();
            elem[key].AppendChild(xmlDoc.CreateElement("color"));
            elem[key]["color"].InnerText = PowerPraiseFileUtil.ConvertColor(f.Color).ToString();
            elem[key].AppendChild(xmlDoc.CreateElement("outline"));
            elem[key]["outline"].InnerText = f.OutlineWidth.ToString();
            elem[key].AppendChild(xmlDoc.CreateElement("shadow"));
            elem[key]["shadow"].InnerText = f.ShadowDistance.ToString();
        }
    }
}