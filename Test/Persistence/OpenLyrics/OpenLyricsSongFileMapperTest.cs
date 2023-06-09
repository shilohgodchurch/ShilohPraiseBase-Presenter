﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PraiseBase.Presenter.Model.Song;
using PraiseBase.Presenter.Util;

namespace PraiseBase.Presenter.Persistence.OpenLyrics
{
    /// <summary>
    ///This is a test class for OpenLyricsSongFileReaderTest and is intended
    ///to contain all OpenLyricsSongFileReaderTest Unit Tests
    ///</summary>
    [TestClass]
    public class OpenLyricsSongFileMapperTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod]
        public void LoadSimpleTest()
        {
            OpenLyricsSongFileMapper mapper = new OpenLyricsSongFileMapper();

            OpenLyricsSong source = new OpenLyricsSong
            {
                Title = "Amazing Grace",
                ModifiedTimestamp = "2012-04-10T22:00:00+10:00",
                CreatedIn = "OpenLP 1.9.0",
                ModifiedIn = "MyApp 0.0.1"
            };

            OpenLyricsSong.Verse verse = new OpenLyricsSong.Verse
            {
                Name = "v1"
            };

            OpenLyricsSong.TextLines lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("Amazing grace how sweet the sound");
            lines.Text.Add("that saved a wretch like me;");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);


            Song expected = new Song
            {
                Title = "Amazing Grace",
                ModifiedTimestamp = "2012-04-10T22:00:00+10:00",
                CreatedIn = "OpenLP 1.9.0",
                ModifiedIn = "MyApp 0.0.1"
            };

            var part = new SongPart
            {
                Caption = "v1"
            };

            var slide = new SongSlide
            {
                Text = "Amazing grace how sweet the sound<br/>that saved a wretch like me;"
            };
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            var actual = mapper.Map(source);
            Assert.AreEqual(expected.Title, actual.Title, "Wrong song title");
            Assert.AreEqual(expected.ModifiedTimestamp, actual.ModifiedTimestamp, "Wrong song modified date");
            Assert.AreEqual(expected.CreatedIn, actual.CreatedIn, "Wrong creator app");
            Assert.AreEqual(expected.ModifiedIn, actual.ModifiedIn, "Wrong modifier app");

            Assert.AreEqual(expected.Parts.Count, actual.Parts.Count, "Parts incomplete");
            for (int i = 0; i < expected.Parts.Count; i++)
            {
                Assert.AreEqual(expected.Parts[i].Caption, actual.Parts[i].Caption, "Wrong verse name");
                Assert.AreEqual(expected.Parts[i].Slides.Count, actual.Parts[i].Slides.Count, "Slides incomplete");
                for (int j = 0; j < expected.Parts[i].Slides.Count; j++)
                {
                    Assert.AreEqual(expected.Parts[i].Slides[j].Lines.Count, actual.Parts[i].Slides[j].Lines.Count, "Slide lines incomplete");
                    for (int k = 0; k < expected.Parts[i].Slides[j].Lines.Count; k++)
                    {
                        Assert.AreEqual(expected.Parts[i].Slides[j].Lines[k], actual.Parts[i].Slides[j].Lines[k], "Wrong slide lyrics");
                    }
                }
            }

            Assert.IsTrue(SongSearchUtil.GetSearchableSongText(actual).Contains(expected.Title.ToLower()));
            Assert.IsTrue(SongSearchUtil.GetSearchableSongText(actual).Contains("sweet"));
        }

        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod]
        public void LoadComplexTest()
        {
            OpenLyricsSongFileMapper mapper = new OpenLyricsSongFileMapper();

            OpenLyricsSong source = new OpenLyricsSong
            {
                Title = "Amazing Grace",
                ModifiedTimestamp = "2012-04-10T22:00:00+10:00",
                CreatedIn = "OpenLP 1.9.0",
                ModifiedIn = "ChangingSong 0.0.1",
                CcliIdentifier = "4639462",
                Copyright = "public domain",
                ReleaseYear = "1779"
            };

            source.Comments.Add("This is one of the most popular songs in our congregation.");

            OpenLyricsSong.Verse verse = new OpenLyricsSong.Verse
            {
                Name = "v1",
                Language = "en"
            };
            var lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            verse.Lines.Add(lines);
            lines = new OpenLyricsSong.TextLines
            {
                Part = "women"
            };
            lines.Text.Add("A b c");
            lines.Text.Add("D e f");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);

            verse = new OpenLyricsSong.Verse
            {
                Name = "v1",
                Language = "de"
            };
            lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("Erstaunliche Ahmut, wie");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);

            verse = new OpenLyricsSong.Verse
            {
                Name = "c"
            };
            lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("");
            lines.Text.Add("Line content.");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);

            verse = new OpenLyricsSong.Verse
            {
                Name = "v2",
                Language = "en-US"
            };
            lines = new OpenLyricsSong.TextLines
            {
                Part = "men"
            };
            lines.Text.Add("");
            lines.Text.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            lines.Text.Add("");
            lines.Text.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            lines.Text.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            verse.Lines.Add(lines);
            lines = new OpenLyricsSong.TextLines
            {
                Part = "women"
            };
            lines.Text.Add("A b c");
            lines.Text.Add("");
            lines.Text.Add("D e f");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);

            verse = new OpenLyricsSong.Verse
            {
                Name = "emptyline",
                Language = "de"
            };
            lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("");
            lines.Text.Add("");
            verse.Lines.Add(lines);
            lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("");
            lines.Text.Add("");
            lines.Text.Add("");
            lines.Text.Add("");
            lines.Text.Add("");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);

            verse = new OpenLyricsSong.Verse
            {
                Name = "e",
                Language = "de"
            };
            lines = new OpenLyricsSong.TextLines();
            lines.Text.Add("This is text of ending.");
            verse.Lines.Add(lines);
            source.Verses.Add(verse);


            Song expected = new Song
            {
                Title = "Amazing Grace",
                ModifiedTimestamp = "2012-04-10T22:00:00+10:00",
                CreatedIn = "OpenLP 1.9.0",
                ModifiedIn = "ChangingSong 0.0.1",
                CcliIdentifier = "4639462",
                Copyright = "public domain",
                ReleaseYear = "1779",
                Comment = "This is one of the most popular songs in our congregation."
            };


            var part = new SongPart
            {
                Caption = "v1",
                Language = "en"
            };
            var slide = new SongSlide();
            slide.Lines.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            part.Slides.Add(slide);
            slide = new SongSlide
            {
                PartName = "women"
            };
            slide.Lines.Add("A b c");
            slide.Lines.Add("D e f");
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            part = new SongPart
            {
                Caption = "v1",
                Language = "de"
            };
            slide = new SongSlide
            {
                Text = "Erstaunliche Ahmut, wie"
            };
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            part = new SongPart
            {
                Caption = "c"
            };
            slide = new SongSlide();
            slide.Lines.Add("");
            slide.Lines.Add("Line content.");
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            part = new SongPart
            {
                Caption = "v2",
                Language = "en-US"
            };
            slide = new SongSlide
            {
                PartName = "men"
            };
            slide.Lines.Add("");
            slide.Lines.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            slide.Lines.Add("");
            slide.Lines.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            slide.Lines.Add("Amazing grace how sweet the sound that saved a wretch like me;");
            part.Slides.Add(slide);
            slide = new SongSlide
            {
                PartName = "women"
            };
            slide.Lines.Add("A b c");
            slide.Lines.Add("");
            slide.Lines.Add("D e f");
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            part = new SongPart
            {
                Caption = "emptyline",
                Language = "de"
            };
            slide = new SongSlide();
            slide.Lines.Add("");
            slide.Lines.Add("");
            part.Slides.Add(slide);
            slide = new SongSlide();
            slide.Lines.Add("");
            slide.Lines.Add("");
            slide.Lines.Add("");
            slide.Lines.Add("");
            slide.Lines.Add("");
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            part = new SongPart
            {
                Caption = "e",
                Language = "de"
            };
            slide = new SongSlide
            {
                Text = "This is text of ending."
            };
            part.Slides.Add(slide);
            expected.Parts.Add(part);

            Song actual = mapper.Map(source);
            Assert.AreEqual(expected.Title, actual.Title, "Wrong song title");
            Assert.AreEqual(expected.ModifiedTimestamp, actual.ModifiedTimestamp, "Wrong song modified date");
            Assert.AreEqual(expected.CreatedIn, actual.CreatedIn, "Wrong creator app");
            Assert.AreEqual(expected.ModifiedIn, actual.ModifiedIn, "Wrong modifier app");
            Assert.AreEqual(expected.Copyright, actual.Copyright, "Wrong copyright info");
            Assert.AreEqual(expected.CcliIdentifier, actual.CcliIdentifier, "Wrong CCLI number");
            Assert.AreEqual(expected.ReleaseYear, actual.ReleaseYear, "Wrong release date");

            Assert.AreEqual(expected.Parts.Count, actual.Parts.Count, "Parts incomplete");
            for (int i = 0; i < expected.Parts.Count; i++)
            {
                Assert.AreEqual(expected.Parts[i].Caption, actual.Parts[i].Caption, "Wrong verse name");
                Assert.AreEqual(expected.Parts[i].Language, actual.Parts[i].Language, "Wrong language");
                Assert.AreEqual(expected.Parts[i].Slides.Count, actual.Parts[i].Slides.Count, "Slides incomplete "+i);
                for (int j = 0; j < expected.Parts[i].Slides.Count; j++)
                {
                    Assert.AreEqual(expected.Parts[i].Slides[j].Lines.Count, actual.Parts[i].Slides[j].Lines.Count, "Slide lines incomplete "+i+" "+j);
                    Assert.AreEqual(expected.Parts[i].Slides[j].PartName, actual.Parts[i].Slides[j].PartName, "Wrong slide part name");
                    for (int k = 0; k < expected.Parts[i].Slides[j].Lines.Count; k++)
                    {
                        Assert.AreEqual(expected.Parts[i].Slides[j].Lines[k], actual.Parts[i].Slides[j].Lines[k], "Wrong slide lyrics");
                    }
                }
            }
        }
    }
}
