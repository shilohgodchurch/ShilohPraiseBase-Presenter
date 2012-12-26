﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pbp.Data.Song
{
    /// <summary>
    /// Song tempo in bpm (beats per minute, maybe 30-250) or some words like
    /// Very Fast, Fast, Moderate, Slow, Very Slow -->
    /// </summary>
    public  class SongTempo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}