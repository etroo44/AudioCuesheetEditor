﻿//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditorTests.Utility;
using System.Linq;

namespace AudioCuesheetEditor.Model.AudioCuesheet.Tests
{
    [TestClass()]
    public class TrackTests
    {
        [TestMethod()]
        public void TrackTest()
        {
            //Tests for Length
            var track = new Track();
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 2, 30);
            Assert.IsNotNull(track.Length);
            Assert.IsNotNull(track.Begin);
            Assert.IsNotNull(track.End);
            Assert.AreEqual(TimeSpan.Zero, track.Begin);
            Assert.AreEqual(new TimeSpan(0, 2, 30), track.End);
            Assert.AreEqual(new TimeSpan(0, 2, 30), track.Length);
            track.Begin = new TimeSpan(0, 25, 0);
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 3, 30);
            Assert.IsNotNull(track.Length);
            Assert.AreEqual(new TimeSpan(0, 3, 30), track.Length);
            Assert.AreEqual(new TimeSpan(0, 28, 30), track.End);
            track.Begin = null;
            Assert.IsNull(track.Length);
            track.Length = new TimeSpan(0, 5, 15);
            Assert.AreEqual(new TimeSpan(0, 28, 30), track.End);
            Assert.AreEqual(new TimeSpan(0, 5, 15), track.Length);
            Assert.AreEqual(new TimeSpan(0, 23, 15), track.Begin);
        }

        [TestMethod()]
        public void SetLengthTest()
        {
            var track = new Track();
            Assert.IsNull(track.Length);
            Assert.IsNull(track.Begin);
            Assert.IsNull(track.End);
            track.Begin = new TimeSpan(0, 2, 43);
            track.End = new TimeSpan(0, 5, 23);
            Assert.IsNotNull(track.Length);
            track.Length = new TimeSpan(0, 2, 0);
            Assert.AreEqual(new TimeSpan(0, 4, 43), track.End);
        }

        [TestMethod()]
        public void LinkTrackTest()
        {
            //Test LinkedPreviousTrack
            var track = new Track
            {
                Begin = TimeSpan.Zero,
                Position = 1,
                End = new TimeSpan(0, 3, 23)
            };
            Assert.IsFalse(track.IsLinkedToPreviousTrack);
            var track2 = new Track
            {
                Begin = track.End,
                End = new TimeSpan(0, 5, 45),
                Position = 2
            };
            Assert.IsFalse(track2.IsLinkedToPreviousTrack);
            Boolean eventFired = false;
            track2.IsLinkedToPreviousTrackChanged += delegate
            {
                eventFired = true;
            };
            track2.IsLinkedToPreviousTrack = true;
            Assert.IsTrue(eventFired);
        }

        [TestMethod()]
        public void CheckPositionInCuesheetTest()
        {
            var testHelper = new TestHelper();
            var cuesheet = new Cuesheet();
            var track1 = new Track
            {
                Position = 3,
                End = new TimeSpan(0, 5, 0)
            };
            cuesheet.AddTrack(track1, testHelper.ApplicationOptions);
            var validationErrors = track1.GetValidationErrorsFiltered(nameof(Track.Position));
            Assert.IsTrue(validationErrors.Any(x => x.Message.Message.Contains(" of this track does not match track position in cuesheet. Please correct the")));
            track1.Position = 1;
            Assert.IsTrue(track1.GetValidationErrorsFiltered(nameof(Track.Position)).Count == 0);
        }

        [TestMethod()]
        public void CopyValuesTest()
        {
            var sourceTrack = new Track();
            var destinyTrack = new Track();
            Assert.AreNotEqual(sourceTrack, destinyTrack);
            sourceTrack.Cuesheet = new Cuesheet();
            sourceTrack.IsLinkedToPreviousTrack = true;
            sourceTrack.Position = 23;
            sourceTrack.Artist = "Testartist";
            sourceTrack.Title = "Testtitle";
            sourceTrack.Begin = new TimeSpan(0, 2, 10);
            sourceTrack.End = new TimeSpan(0, 5, 32);
            sourceTrack.PreGap = new TimeSpan(0, 0, 10);
            sourceTrack.PostGap = new TimeSpan(0, 0, 12);
            Assert.IsNull(destinyTrack.Cuesheet);
            destinyTrack.CopyValues(sourceTrack);
            Assert.AreEqual(sourceTrack.IsLinkedToPreviousTrack, destinyTrack.IsLinkedToPreviousTrack);
            Assert.AreEqual(sourceTrack.Cuesheet, destinyTrack.Cuesheet);
            Assert.AreEqual(sourceTrack.Position, destinyTrack.Position);
            Assert.AreEqual(sourceTrack.Artist, destinyTrack.Artist);
            Assert.AreEqual(sourceTrack.Title, destinyTrack.Title);
            Assert.AreEqual(sourceTrack.Begin, destinyTrack.Begin);
            Assert.AreEqual(sourceTrack.End, destinyTrack.End);
            Assert.AreEqual(sourceTrack.PreGap, destinyTrack.PreGap);
            Assert.AreEqual(sourceTrack.PostGap, destinyTrack.PostGap);
            var destinationTracks = new List<Track>() { new Track(), new Track(), new Track() };
            destinationTracks.ForEach(x => x.CopyValues(sourceTrack, useInternalSetters: Track.AllPropertyNames));
            Assert.IsTrue(destinationTracks.All(x => x.IsLinkedToPreviousTrack == sourceTrack.IsLinkedToPreviousTrack));
            Assert.IsTrue(destinationTracks.All(x => x.Cuesheet == sourceTrack.Cuesheet));
            Assert.IsTrue(destinationTracks.All(x => x.Position == sourceTrack.Position));
            Assert.IsTrue(destinationTracks.All(x => x.Artist == sourceTrack.Artist));
            Assert.IsTrue(destinationTracks.All(x => x.Title == sourceTrack.Title));
            Assert.IsTrue(destinationTracks.All(x => x.Begin == sourceTrack.Begin));
            Assert.IsTrue(destinationTracks.All(x => x.End == sourceTrack.End));
            Assert.IsTrue(destinationTracks.All(x => x.PreGap == sourceTrack.PreGap));
            Assert.IsTrue(destinationTracks.All(x => x.PostGap == sourceTrack.PostGap));
        }
    }
}