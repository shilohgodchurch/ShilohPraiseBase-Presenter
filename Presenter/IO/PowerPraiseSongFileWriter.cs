﻿/*
 *   PraiseBase Presenter
 *   The open source lyrics and image projection software for churches
 *
 *   http://code.google.com/p/praisebasepresenter
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
 *   Author:
 *      Nicolas Perrenoud <nicu_at_lavine.ch>
 *   Co-authors:
 *      ...
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Pbp.IO
{
    public class PowerPraiseSongFileWriter : SongFileWriter
    {
        public const string FileFormatVersion = "3.0";

        public override void save(string filename, Song sng)
        {
            var xmlDoc = new XmlDocument();

            XmlNode xmlnode = xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            xmlDoc.AppendChild(xmlnode);

            xmlDoc.AppendChild(xmlDoc.CreateElement("ppl"));
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            xmlRoot.SetAttribute("version", FileFormatVersion);

            xmlRoot.AppendChild(xmlDoc.CreateElement("general"));
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("title"));
            xmlRoot["general"]["title"].InnerText = sng.Title;
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("category"));
            xmlRoot["general"]["category"].InnerText = sng.Tags.Count > 0 ? sng.Tags[0] : "Keine Kategorie";
            xmlRoot["general"].AppendChild(xmlDoc.CreateElement("language"));
            if (sng.Language != string.Empty)
            {
                xmlRoot["general"]["language"].InnerText = sng.Language;
            }
            if (sng.Comment != string.Empty)
            {
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("comment"));
                xmlRoot["general"]["comment"].InnerText = sng.Comment;
            }
            if (sng.QualityIssues.Count > 0)
            {
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("qualityissues"));
                foreach (QualityAssuranceIndicators i in sng.QualityIssues)
                {
                    XmlNode qaChld = xmlRoot["general"]["qualityissues"].AppendChild(xmlDoc.CreateElement("issue"));
                    qaChld.InnerText = Enum.GetName(typeof(QualityAssuranceIndicators), i);
                }
            }
            if (sng.CcliID != null && sng.CcliID != String.Empty)
            {
                xmlRoot["general"].AppendChild(xmlDoc.CreateElement("ccliNo"));
                xmlRoot["general"]["ccliNo"].InnerText = sng.CcliID;
            }

            xmlRoot.AppendChild(xmlDoc.CreateElement("songtext"));

            var usedImages = new List<string>();
            foreach (SongPart prt in sng.Parts)
            {
                XmlElement tn = xmlDoc.CreateElement("part");
                tn.SetAttribute("caption", prt.Caption);
                foreach (SongSlide sld in prt.Slides)
                {
                    if (sld.ImageNumber > 0)
                    {
                        if (!usedImages.Contains(sng.RelativeImagePaths[sld.ImageNumber - 1]))
                        {
                            usedImages.Add(sng.RelativeImagePaths[sld.ImageNumber - 1]);
                        }
                        sld.ImageNumber = usedImages.IndexOf(sng.RelativeImagePaths[sld.ImageNumber - 1]) + 1;
                    }
                }
            }
            sng.RelativeImagePaths = usedImages;

            foreach (SongPart prt in sng.Parts)
            {
                XmlElement tn = xmlDoc.CreateElement("part");
                tn.SetAttribute("caption", prt.Caption);
                foreach (SongSlide sld in prt.Slides)
                {
                    XmlElement tn2 = xmlDoc.CreateElement("slide");
                    tn2.SetAttribute("mainsize", sng.TextFont.Size.ToString());
                    tn2.SetAttribute("backgroundnr", (sld.ImageNumber - 1).ToString());

                    foreach (string ln in sld.Lines)
                    {
                        XmlElement tn3 = xmlDoc.CreateElement("line");
                        tn3.InnerText = ln;
                        tn2.AppendChild(tn3);
                    }
                    foreach (string ln in sld.Translation)
                    {
                        XmlElement tn3 = xmlDoc.CreateElement("translation");
                        tn3.InnerText = ln;
                        tn2.AppendChild(tn3);
                    }
                    tn.AppendChild(tn2);
                }
                xmlRoot["songtext"].AppendChild(tn);
            }
            sng.updateSearchText();

            xmlRoot.AppendChild(xmlDoc.CreateElement("order"));
            foreach (SongPart prt in sng.Parts)
            {
                XmlElement tn = xmlDoc.CreateElement("item");
                tn.InnerText = prt.Caption;
                xmlRoot["order"].AppendChild(tn);
            }

            xmlRoot.AppendChild(xmlDoc.CreateElement("information"));
            xmlRoot["information"].AppendChild(xmlDoc.CreateElement("copyright"));
            xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("position"));
            xmlRoot["information"]["copyright"]["position"].InnerText = "lastslide";
            xmlRoot["information"]["copyright"].AppendChild(xmlDoc.CreateElement("text"));
            if (sng.Copyright != null && sng.Copyright != string.Empty)
            {
                xmlRoot["information"].AppendChild(xmlDoc.CreateElement("source"));
            }
            xmlRoot["information"].AppendChild(xmlDoc.CreateElement("source"));
            xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("position"));
            xmlRoot["information"]["source"]["position"].InnerText = "firstslide";
            xmlRoot["information"]["source"].AppendChild(xmlDoc.CreateElement("text"));

            xmlRoot.AppendChild(xmlDoc.CreateElement("formatting"));

            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("font"));
            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("maintext"));
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("name"));
            xmlRoot["formatting"]["font"]["maintext"]["name"].InnerText = sng.TextFont.Name;
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("size"));
            xmlRoot["formatting"]["font"]["maintext"]["size"].InnerText = sng.TextFont.Size.ToString();
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("bold"));
            xmlRoot["formatting"]["font"]["maintext"]["bold"].InnerText = (sng.TextFont.Bold).ToString().ToLower();
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("italic"));
            xmlRoot["formatting"]["font"]["maintext"]["italic"].InnerText = (sng.TextFont.Italic).ToString().ToLower();
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["maintext"]["color"].InnerText =
                (16777216 + sng.TextColor.ToArgb()).ToString();
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["maintext"]["outline"].InnerText = "25";
            xmlRoot["formatting"]["font"]["maintext"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["maintext"]["shadow"].InnerText = "20";

            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("translationtext"));
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("name"));
            xmlRoot["formatting"]["font"]["translationtext"]["name"].InnerText = sng.TranslationFont.Name;
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("size"));
            xmlRoot["formatting"]["font"]["translationtext"]["size"].InnerText = sng.TranslationFont.Size.ToString();
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("bold"));
            xmlRoot["formatting"]["font"]["translationtext"]["bold"].InnerText =
                (sng.TranslationFont.Bold).ToString().ToLower();
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("italic"));
            xmlRoot["formatting"]["font"]["translationtext"]["italic"].InnerText =
                (sng.TranslationFont.Italic).ToString().ToLower();
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["translationtext"]["color"].InnerText =
                (16777216 + sng.TranslationColor.ToArgb()).ToString();
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["translationtext"]["outline"].InnerText = "25";
            xmlRoot["formatting"]["font"]["translationtext"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["translationtext"]["shadow"].InnerText = "20";

            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("copyrighttext"));
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("name"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["name"].InnerText = sng.TranslationFont.Name;
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("size"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["size"].InnerText = sng.TranslationFont.Size.ToString();
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("bold"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["bold"].InnerText =
                (sng.TranslationFont.Bold).ToString().ToLower();
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("italic"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["italic"].InnerText =
                (sng.TranslationFont.Italic).ToString().ToLower();
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["color"].InnerText =
                (16777216 + sng.TranslationColor.ToArgb()).ToString();
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["outline"].InnerText = "25";
            xmlRoot["formatting"]["font"]["copyrighttext"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["copyrighttext"]["shadow"].InnerText = "20";

            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("sourcetext"));
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("name"));
            xmlRoot["formatting"]["font"]["sourcetext"]["name"].InnerText = sng.TranslationFont.Name;
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("size"));
            xmlRoot["formatting"]["font"]["sourcetext"]["size"].InnerText = sng.TranslationFont.Size.ToString();
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("bold"));
            xmlRoot["formatting"]["font"]["sourcetext"]["bold"].InnerText =
                (sng.TranslationFont.Bold).ToString().ToLower();
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("italic"));
            xmlRoot["formatting"]["font"]["sourcetext"]["italic"].InnerText =
                (sng.TranslationFont.Italic).ToString().ToLower();
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["sourcetext"]["color"].InnerText =
                (16777216 + sng.TranslationColor.ToArgb()).ToString();
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["sourcetext"]["outline"].InnerText = "25";
            xmlRoot["formatting"]["font"]["sourcetext"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["sourcetext"]["shadow"].InnerText = "20";

            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("outline"));
            xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("enabled"));
            xmlRoot["formatting"]["font"]["outline"]["enabled"].InnerText = "true";
            xmlRoot["formatting"]["font"]["outline"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["outline"]["color"].InnerText = "0";

            xmlRoot["formatting"]["font"].AppendChild(xmlDoc.CreateElement("shadow"));
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("enabled"));
            xmlRoot["formatting"]["font"]["shadow"]["enabled"].InnerText = "true";
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("color"));
            xmlRoot["formatting"]["font"]["shadow"]["color"].InnerText = "0";
            xmlRoot["formatting"]["font"]["shadow"].AppendChild(xmlDoc.CreateElement("direction"));
            xmlRoot["formatting"]["font"]["shadow"]["direction"].InnerText = "125";

            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("background"));
            foreach (string imp in usedImages)
            {
                XmlElement tn = xmlDoc.CreateElement("file");
                tn.InnerText = imp;
                xmlRoot["formatting"]["background"].AppendChild(tn);
            }
            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("linespacing"));

            xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("main"));
            xmlRoot["formatting"]["linespacing"].AppendChild(xmlDoc.CreateElement("translation"));
            xmlRoot["formatting"]["linespacing"]["main"].InnerText = sng.TextLineSpacing.ToString();
            xmlRoot["formatting"]["linespacing"]["translation"].InnerText = sng.TextLineSpacing.ToString();

            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("textorientation"));
            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("horizontal"));
            if (sng.Parts[0].Slides[0].HorizontalAlign == Song.SongTextHorizontalAlign.Left)
                xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "left";
            else if (sng.Parts[0].Slides[0].HorizontalAlign == Song.SongTextHorizontalAlign.Right)
                xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "right";
            else
                xmlRoot["formatting"]["textorientation"]["horizontal"].InnerText = "center";
            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("vertical"));
            if (sng.Parts[0].Slides[0].VerticalAlign == Song.SongTextVerticalAlign.Top)
                xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "top";
            else if (sng.Parts[0].Slides[0].VerticalAlign == Song.SongTextVerticalAlign.Bottom)
                xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "bottom";
            else
                xmlRoot["formatting"]["textorientation"]["vertical"].InnerText = "center";
            xmlRoot["formatting"]["textorientation"].AppendChild(xmlDoc.CreateElement("transpos"));
            xmlRoot["formatting"]["textorientation"]["transpos"].InnerText = "inline";

            xmlRoot["formatting"].AppendChild(xmlDoc.CreateElement("borders"));
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainleft"));
            xmlRoot["formatting"]["borders"]["mainleft"].InnerText = "40";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("maintop"));
            xmlRoot["formatting"]["borders"]["maintop"].InnerText = "70";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainright"));
            xmlRoot["formatting"]["borders"]["mainright"].InnerText = "40";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("mainbottom"));
            xmlRoot["formatting"]["borders"]["mainbottom"].InnerText = "80";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("copyrightbottom"));
            xmlRoot["formatting"]["borders"]["copyrightbottom"].InnerText = "30";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourcetop"));
            xmlRoot["formatting"]["borders"]["sourcetop"].InnerText = "20";
            xmlRoot["formatting"]["borders"].AppendChild(xmlDoc.CreateElement("sourceright"));
            xmlRoot["formatting"]["borders"]["sourceright"].InnerText = "40";

            XmlWriterSettings wrtStn = new XmlWriterSettings();
            wrtStn.Encoding = Encoding.UTF8;
            wrtStn.Indent = true;
            XmlWriter wrt = XmlTextWriter.Create(filename, wrtStn);
            xmlDoc.WriteTo(wrt);
            wrt.Flush();
            wrt.Close();
        }
    }
}