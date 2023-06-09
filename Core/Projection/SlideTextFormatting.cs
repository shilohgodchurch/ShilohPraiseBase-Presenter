﻿using PraiseBase.Presenter.Model;

namespace PraiseBase.Presenter.Projection
{
    public struct SlideTextFormatting
    {
        /// <summary>
        ///     If true, font size will be scaled down if the text does not fit on the slide
        /// </summary>
        public bool ScaleFontSize { get; set; }

        /// <summary>
        ///     Formatting of the main text
        /// </summary>
        public MainTextFormatting Text { get; set; }

        /// <summary>
        ///     Formatting of the header text box
        /// </summary>
        public TextBoxFormatting Header { get; set; }

        /// <summary>
        ///     Formatting of the footer text box
        /// </summary>
        public TextBoxFormatting Footer { get; set; }

        /// <summary>
        ///     True of the text should be outlined
        /// </summary>
        public bool OutlineEnabled { get; set; }

        /// <summary>
        ///     True if the text should have a shadow
        /// </summary>
        public bool ShadowEnabled { get; set; }

        /// <summary>
        ///     True if the shadow should be smooth
        /// </summary>
        public bool SmoothShadow { get; set; }

        /// <summary>
        ///     Wrap long lines
        /// </summary>
        public bool LineWrap { get; set; }

        public struct MainTextFormatting
        {
            /// <summary>
            ///     Gets or sets the text font and color for the main text
            /// </summary>
            public TextFormatting MainText { get; set; }

            /// <summary>
            ///     Gets or sets the font of tanslation text
            /// </summary>
            public TextFormatting SubText { get; set; }

            /// <summary>
            ///     Main text line spacing
            /// </summary>
            public int MainTextLineSpacing { get; set; }

            /// <summary>
            ///     Translation text line spacing
            /// </summary>
            public int SubTextLineSpacing { get; set; }

            /// <summary>
            ///     Horizontal and vertical text orientation
            /// </summary>
            public TextOrientation Orientation { get; set; }

            /// <summary>
            ///     Vertical padding
            /// </summary>
            public int VerticalPadding { get; set; }

            /// <summary>
            ///     Horizontal padding
            /// </summary>
            public int HorizontalPadding { get; set; }

            /// <summary>
            ///     Horizontal sub text offset
            /// </summary>
            public int HorizontalSubTextOffset { get; set; }
        }

        public struct TextBoxFormatting
        {
            /// <summary>
            ///     Gets or sets the font, color, shadow, outline and line spacing
            /// </summary>
            public TextFormatting Text { get; set; }

            /// <summary>
            ///     Horizontal text orientation
            /// </summary>
            public HorizontalOrientation HorizontalOrientation { get; set; }

            /// <summary>
            ///     Vertical padding
            /// </summary>
            public int VerticalPadding { get; set; }

            /// <summary>
            ///     Horizontal padding
            /// </summary>
            public int HorizontalPadding { get; set; }
        }
    }
}