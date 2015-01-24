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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using PraiseBase.Presenter.Model;
using PraiseBase.Presenter.Model.Song;

namespace PraiseBase.Presenter.Persistence.PowerPraise
{
    public class PowerPraiseSongFileMapper : ISongFileMapper<PowerPraiseSong>
    {
        /// <summary>
        /// Maps a PowerPraise song to a Song object
        /// </summary>
        /// <param name="ppl"></param>
        /// <returns></returns>
        public Song map(PowerPraiseSong ppl)
        {
            Song song = new Song();

            song.Title = ppl.Title;
            song.Language = ppl.Language;
            song.Themes.Add(ppl.Category);

            // Copyright text
            song.Copyright = String.Join(" ", ppl.CopyrightText.ToArray());
            switch (ppl.CopyrightTextPosition)
            {
                case PowerPraiseSong.CopyrightPosition.FirstSlide:
                    song.CopyrightPosition = "firstslide";
                    break;
                case PowerPraiseSong.CopyrightPosition.LastSlide:
                    song.CopyrightPosition = "lastslide";
                    break;
                case PowerPraiseSong.CopyrightPosition.None:
                    song.CopyrightPosition = "none";
                    break;
            }

            // Source / songbook
            song.SongBooksString = ppl.SourceText;
            song.SourcePosition = ppl.SourceTextEnabled ? "firstslide" : "none";

            // Song parts
            foreach (PowerPraiseSongPart prt in ppl.Parts)
            {
                SongPart part = new SongPart();
                part.Caption = prt.Caption;
                foreach (PowerPraiseSongSlide sld in prt.Slides)
                {
                    SongSlide slide = new SongSlide(song);
                    slide.ImageNumber = sld.BackgroundNr;
                    slide.TextSize = sld.MainSize > 0 ? sld.MainSize : (song.MainText.Font != null ? song.MainText.Font.Size : 0);
                    slide.Lines.AddRange(sld.Lines);
                    slide.Translation.AddRange(sld.Translation);
                    part.Slides.Add(slide);
                }
                song.Parts.Add(part);
            }

            // Order
            foreach (PowerPraiseSongPart o in ppl.Order)
            {
                int i;
                for (i = 0; i < ppl.Parts.Count; i++)
                {
                    if (ppl.Parts[i].Caption == o.Caption)
                    {
                        song.PartSequence.Add(i);
                        break;
                    }
                }
            }

            // Backgrounds
            song.RelativeImagePaths.AddRange(ppl.BackgroundImages);

            // Formatting definitions
            song.MainText = new TextFormatting(
                ppl.MainTextFontFormatting.Font,
                ppl.MainTextFontFormatting.Color,
                new TextOutline(ppl.MainTextFontFormatting.OutlineWidth, ppl.TextOutlineFormatting.Color),
                new TextShadow(10, ppl.MainTextFontFormatting.ShadowDistance, ppl.TextShadowFormatting.Direction, ppl.TextShadowFormatting.Color),
                ppl.MainLineSpacing
            );
            song.TranslationText = new TextFormatting(
                ppl.TranslationTextFontFormatting.Font,
                ppl.TranslationTextFontFormatting.Color,
                new TextOutline(ppl.TranslationTextFontFormatting.OutlineWidth, ppl.TextOutlineFormatting.Color),
                new TextShadow(10, ppl.TranslationTextFontFormatting.ShadowDistance, ppl.TextShadowFormatting.Direction, ppl.TextShadowFormatting.Color),
                ppl.TranslationLineSpacing
            );
            song.CopyrightText = new TextFormatting(
                ppl.CopyrightTextFontFormatting.Font,
                ppl.CopyrightTextFontFormatting.Color,
                new TextOutline(ppl.CopyrightTextFontFormatting.OutlineWidth, ppl.TextOutlineFormatting.Color),
                new TextShadow(10, ppl.CopyrightTextFontFormatting.ShadowDistance, ppl.TextShadowFormatting.Direction, ppl.TextShadowFormatting.Color),
                ppl.MainLineSpacing
            );
            song.SourceText = new TextFormatting(
                ppl.SourceTextFontFormatting.Font,
                ppl.SourceTextFontFormatting.Color,
                new TextOutline(ppl.SourceTextFontFormatting.OutlineWidth, ppl.TextOutlineFormatting.Color),
                new TextShadow(10, ppl.SourceTextFontFormatting.ShadowDistance, ppl.TextShadowFormatting.Direction, ppl.TextShadowFormatting.Color),
                ppl.MainLineSpacing
            );

            // Text orientation
            song.TextOrientation = ppl.TextOrientation;
            // TODO Translation position

            // Enable or disable outline/shadow
            song.TextOutlineEnabled = ppl.TextOutlineFormatting.Enabled;
            song.TextShadowEnabled = ppl.TextShadowFormatting.Enabled;

            // Borders
            song.TextBorders = new SongTextBorders(
                ppl.Borders.TextLeft,
                ppl.Borders.TextTop,
                ppl.Borders.TextRight,
                ppl.Borders.TextBottom,
                ppl.Borders.CopyrightBottom,
                ppl.Borders.SourceTop,
                ppl.Borders.SourceRight
            );            

            song.UpdateSearchText();

            return song;
        }

        /// <summary>
        /// Maps a song to a PowerPraise song object
        /// </summary>
        /// <param name="song"></param>
        /// <param name="ppl"></param>
        public void map(Song song, PowerPraiseSong ppl)
        {
            // General
            ppl.Title = song.Title;
            ppl.Language = song.Language;
            ppl.Category = song.Themes.Count > 0 ? song.Themes[0] : null;

            // Song parts
            foreach (var songPart in song.Parts)
            {
                PowerPraiseSongPart pplPart = new PowerPraiseSongPart();
                pplPart.Caption = songPart.Caption;
                foreach (var songSlide in songPart.Slides)
                {
                    PowerPraiseSongSlide pplSlide = new PowerPraiseSongSlide();
                    pplSlide.BackgroundNr = songSlide.ImageNumber;
                    pplSlide.MainSize = (int)(songSlide.TextSize > 0 ? songSlide.TextSize : (song.MainText != null && song.MainText.Font != null ? song.MainText.Font.Size : 0));
                    pplSlide.Lines.AddRange(songSlide.Lines);
                    pplSlide.Translation.AddRange(songSlide.Translation);
                    pplPart.Slides.Add(pplSlide);
                }
                ppl.Parts.Add(pplPart);
            }

            // Part order
            foreach (int i in song.PartSequence)
            {
                if (ppl.Parts[i] != null)
                {
                    ppl.Order.Add(ppl.Parts[i]);
                }
            }

            // Copyright text
            ppl.CopyrightText.Add(song.Copyright);
            if (song.CopyrightPosition == "firstslide")
            {
                ppl.CopyrightTextPosition = PowerPraiseSong.CopyrightPosition.FirstSlide;
            }
            else if (song.CopyrightPosition == "lastslide")
            {
                ppl.CopyrightTextPosition = PowerPraiseSong.CopyrightPosition.LastSlide;
            }
            else if (song.CopyrightPosition == "none")
            {
                ppl.CopyrightTextPosition = PowerPraiseSong.CopyrightPosition.None;
            }

            // Source / songbook
            ppl.SourceText = song.SongBooksString;
            ppl.SourceTextEnabled = (song.SourcePosition == "firstslide");

            // Formatting definitions
            if (song.MainText != null)
            {
                ppl.MainTextFontFormatting = new PowerPraiseSong.FontFormatting
                {
                    Font = song.MainText.Font,
                    Color = song.MainText.Color,
                    OutlineWidth = song.MainText.Outline.Width,
                    ShadowDistance = song.MainText.Shadow.Distance
                };
            }
            if (song.TranslationText != null)
            {
                ppl.TranslationTextFontFormatting = new PowerPraiseSong.FontFormatting
                {
                    Font = song.TranslationText.Font,
                    Color = song.TranslationText.Color,
                    OutlineWidth = song.TranslationText.Outline.Width,
                    ShadowDistance = song.TranslationText.Shadow.Distance
                };
            }
            if (song.CopyrightText != null)
            {
                ppl.CopyrightTextFontFormatting = new PowerPraiseSong.FontFormatting
                {
                    Font = song.CopyrightText.Font,
                    Color = song.CopyrightText.Color,
                    OutlineWidth = song.CopyrightText.Outline.Width,
                    ShadowDistance = song.CopyrightText.Shadow.Distance
                };
            }
            if (song.SourceText != null)
            {
                ppl.SourceTextFontFormatting = new PowerPraiseSong.FontFormatting
                {
                    Font = song.SourceText.Font,
                    Color = song.SourceText.Color,
                    OutlineWidth = song.SourceText.Outline.Width,
                    ShadowDistance = song.SourceText.Shadow.Distance
                };
            }

            // Enable or disable outline/shadow
            if (song.MainText != null)
            {
                ppl.TextOutlineFormatting = new PowerPraiseSong.OutlineFormatting
                {
                    Color = song.MainText.Outline.Color,
                    Enabled = song.TextOutlineEnabled
                };
                ppl.TextShadowFormatting = new PowerPraiseSong.ShadowFormatting
                {
                    Color = song.MainText.Shadow.Color,
                    Direction = song.MainText.Shadow.Direction,
                    Enabled = song.TextShadowEnabled
                };
            }

            // Backgrounds
            ppl.BackgroundImages.AddRange(song.RelativeImagePaths);

            if (song.MainText != null)
            {
                ppl.MainLineSpacing = song.MainText.LineSpacing;
                ppl.TranslationLineSpacing = song.TranslationText.LineSpacing;
            }

            // Text orientation
            ppl.TextOrientation = song.TextOrientation;
            // TODO Translation position
            ppl.TranslationTextPosition = PowerPraiseSong.TranslationPosition.Inline;

            // Borders
            if (song.TextBorders != null)
            {
                ppl.Borders = new PowerPraiseSong.TextBorders
                {
                    TextLeft = song.TextBorders.TextLeft,
                    TextTop = song.TextBorders.TextTop,
                    TextRight = song.TextBorders.TextRight,
                    TextBottom = song.TextBorders.TextBottom,
                    CopyrightBottom = song.TextBorders.CopyrightBottom,
                    SourceTop = song.TextBorders.SourceTop,
                    SourceRight = song.TextBorders.SourceRight
                };
            }

        }
    }
}