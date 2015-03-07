﻿using PraiseBase.Presenter.Model;
using PraiseBase.Presenter.Model.Song;
using PraiseBase.Presenter.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PraiseBase.Presenter.Projection
{
    public static class SongSlideTextFormattingMapper
    {
        public static void Map(Song s, ref SlideTextFormatting slideFormatting)
        {
            slideFormatting.Text = new SlideTextFormatting.MainTextFormatting()
            {
                // TODO respect specific slide text size
                MainText = (TextFormatting)s.MainText.Clone(),
                SubText = (TextFormatting)s.TranslationText.Clone(),
                Orientation = (TextOrientation)s.TextOrientation.Clone(),
                HorizontalPadding = s.TextBorders.TextLeft,
                VerticalPadding = s.TextBorders.TextTop,
                // TODO Parametrize hard-coded value
                HorizontalSubTextOffset  = 10
            };
            slideFormatting.Header = new SlideTextFormatting.TextBoxFormatting()
            {
                Text = (TextFormatting)s.SourceText.Clone(),
                // TODO Parametrize hard-coded value
                HorizontalOrientation = HorizontalOrientation.Right,
                HorizontalPadding = s.TextBorders.SourceRight,
                VerticalPadding = s.TextBorders.SourceTop,
            };
            slideFormatting.Footer = new SlideTextFormatting.TextBoxFormatting()
            {
                Text = (TextFormatting)s.CopyrightText.Clone(),
                // TODO Parametrize hard-coded value
                HorizontalOrientation = HorizontalOrientation.Left,
                HorizontalPadding = s.TextBorders.CopyrightBottom,
                VerticalPadding = s.TextBorders.CopyrightBottom,
            };
            slideFormatting.OutlineEnabled = s.TextOutlineEnabled;
            slideFormatting.ShadowEnabled = s.TextShadowEnabled;
        }

        public static void Map(Settings settings, ref SlideTextFormatting slideFormatting)
        {
            slideFormatting.Text = new SlideTextFormatting.MainTextFormatting()
            {
                MainText = new TextFormatting(
                    settings.ProjectionMasterFont,
                    settings.ProjectionMasterFontColor,
                    new TextOutline(settings.ProjectionMasterOutlineSize, settings.ProjectionMasterOutlineColor),
                    new TextShadow(settings.ProjectionMasterShadowDistance, settings.ProjectionMasterShadowSize,
                        settings.ProjectionMasterShadowDirection, settings.ProjectionMasterShadowColor),
                    settings.ProjectionMasterLineSpacing
                ),
                SubText = new TextFormatting(
                    settings.ProjectionMasterFontTranslation,
                    settings.ProjectionMasterTranslationColor,
                    new TextOutline(settings.ProjectionMasterOutlineSize, settings.ProjectionMasterOutlineColor),
                    new TextShadow(settings.ProjectionMasterShadowDistance, settings.ProjectionMasterShadowSize, 
                        settings.ProjectionMasterShadowDirection, settings.ProjectionMasterShadowColor),
                    settings.ProjectionMasterTranslationLineSpacing
                ),
                Orientation = new TextOrientation(settings.ProjectionMasterVerticalTextOrientation, settings.ProjectionMasterHorizontalTextOrientation),
                HorizontalPadding = settings.ProjectionMasterHorizontalTextPadding,
                VerticalPadding = settings.ProjectionMasterHorizontalTextPadding,
                HorizontalSubTextOffset = settings.ProjectionMasterHorizontalTranslationTextOffset
            };
            slideFormatting.Header = new SlideTextFormatting.TextBoxFormatting()
            {
                Text = new TextFormatting(
                    settings.ProjectionMasterSourceFont,
                    settings.ProjectionMasterSourceColor,
                    new TextOutline(settings.ProjectionMasterOutlineSize, settings.ProjectionMasterOutlineColor),
                    new TextShadow(settings.ProjectionMasterShadowDistance, settings.ProjectionMasterShadowSize, 
                        settings.ProjectionMasterShadowDirection, settings.ProjectionMasterShadowColor),
                    0
                ),
                HorizontalOrientation = settings.ProjectionMasterHorizontalHeaderOrientation,
                HorizontalPadding = settings.ProjectionMasterHorizontalHeaderPadding,
                VerticalPadding = settings.ProjectionMasterVerticalHeaderPadding,
            };
            slideFormatting.Footer = new SlideTextFormatting.TextBoxFormatting()
            {
                Text = new TextFormatting(
                    settings.ProjectionMasterCopyrightFont,
                    settings.ProjectionMasterCopyrightColor,
                    new TextOutline(settings.ProjectionMasterOutlineSize, settings.ProjectionMasterOutlineColor),
                    new TextShadow(settings.ProjectionMasterShadowDistance, settings.ProjectionMasterShadowSize, 
                        settings.ProjectionMasterShadowDirection, settings.ProjectionMasterShadowColor),
                    0
                ),
                HorizontalOrientation = settings.ProjectionMasterHorizontalFooterOrientation,
                HorizontalPadding = settings.ProjectionMasterHorizontalFooterPadding,
                VerticalPadding = settings.ProjectionMasterVerticalFooterPadding,
            };
            slideFormatting.OutlineEnabled = settings.ProjectionMasterOutlineEnabled;
            slideFormatting.ShadowEnabled = settings.ProjectionMasterShadowEnabled;
        }
    }
}