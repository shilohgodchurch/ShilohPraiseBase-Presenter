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
using System.Drawing;

namespace PraiseBase.Presenter.Model
{
    public class TextShadow : ICloneable
    {
        public TextShadow(int distance, int size, int direction, Color color)
        {
            Distance = distance;
            Size = size;
            Direction = direction;
            Color = color;
        }

        public int Distance { get; set; }
        public int Size { get; set; }
        public int Direction { get; set; }
        public Color Color { get; set; }

        public object Clone()
        {
            return new TextShadow(Distance, Size, Direction, Color);
        }

        /// <summary>
        ///     Returns a hashcode of the text formatting object, used for example in the
        ///     editor to check if the file was changed
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Distance;
                hashCode = (hashCode*397) ^ Size;
                hashCode = (hashCode*397) ^ Direction;
                hashCode = (hashCode*397) ^ Color.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(TextShadow other)
        {
            return Distance == other.Distance && Size == other.Size && Direction == other.Direction &&
                   Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TextShadow) obj);
        }
    }
}