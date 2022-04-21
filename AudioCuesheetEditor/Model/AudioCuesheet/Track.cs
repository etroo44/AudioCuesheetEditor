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
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Reflection;
using AudioCuesheetEditor.Model.UI;
using Blazorise.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AudioCuesheetEditor.Model.AudioCuesheet
{
    public class Track : Validateable, ITrack<Cuesheet>, ITraceable
    {
        private uint? position;
        private String? artist;
        private String? title;
        private TimeSpan? begin;
        private TimeSpan? end;
        private List<Flag> flags = new();
        private Track? clonedFrom = null;
        private Boolean isLinkedToPreviousTrack;
        private Cuesheet? cuesheet;
        private TimeSpan? preGap;
        private TimeSpan? postGap;

        /// <summary>
        /// A property with influence to position of this track in cuesheet has been changed. Name of the property changed is provided in event arguments.
        /// </summary>
        public event EventHandler<String>? RankPropertyValueChanged;

        /// <summary>
        /// Eventhandler for IsLinkedToPreviousTrack has changed
        /// </summary>
        public event EventHandler? IsLinkedToPreviousTrackChanged;

        /// <inheritdoc/>
        public event EventHandler<TraceablePropertiesChangedEventArgs>? TraceablePropertyChanged;

        /// <summary>
        /// Create object with copied values from input
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        public Track(ITrack<Cuesheet> track)
        {
            CopyValues(track);
        }

        /// <summary>
        /// Create object with copied values from input
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        public Track(ITrack<ImportCuesheet> track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            //Use public setter since we need to fire all events with positioning
            Position = track.Position;
            //We use the internal properties because we only want to set the values, everything around like validation or automatic calculation doesn't need to be fired
            artist = track.Artist;
            title = track.Title;
            begin = track.Begin;
            end = track.End;
            flags.Clear();
            flags.AddRange(track.Flags);
            PreGap = track.PreGap;
            PostGap = track.PostGap;
            Validate();
        }

        public Track()
        {
            Validate();
        }

        public uint? Position 
        {
            get { return position; }
            set { var previousValue = position; position = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(Position)); OnTraceablePropertyChanged(previousValue); }
        }
        public String? Artist 
        {
            get { return artist; }
            set { var previousValue = artist; artist = value; OnValidateablePropertyChanged(); OnTraceablePropertyChanged(previousValue); }
        }
        public String? Title 
        {
            get { return title; }
            set { var previousValue = title; title = value; OnValidateablePropertyChanged(); OnTraceablePropertyChanged(previousValue); }
        }        
        public TimeSpan? Begin 
        {
            get { return begin; }
            set { var previousValue = begin; begin = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(Begin)); OnTraceablePropertyChanged(previousValue); }
        }
        public TimeSpan? End 
        {
            get { return end; }
            set { var previousValue = end; end = value; OnValidateablePropertyChanged(); RankPropertyValueChanged?.Invoke(this, nameof(End)); OnTraceablePropertyChanged(previousValue); }
        }
        [JsonIgnore]
        public TimeSpan? Length 
        {
            get
            {
                if ((Begin.HasValue == true) && (End.HasValue == true) && (Begin.Value <= End.Value))
                {
                    return End - Begin;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if ((Begin.HasValue == false) && (End.HasValue == false))
                {
                    Begin = TimeSpan.Zero;
                    End = Begin.Value + value;
                }
                else
                {
                    if ((Begin.HasValue == true) && (End.HasValue == true))
                    {
                        End = Begin.Value + value;
                    }
                    else
                    {
                        if ((End.HasValue == false) && (Begin.HasValue))
                        {
                            End = Begin.Value + value;
                        }
                        if ((Begin.HasValue == false) && (End.HasValue))
                        {
                            Begin = End.Value - value;
                        }
                    }
                }
                OnValidateablePropertyChanged();
            }
        }
        [JsonInclude]
        public IReadOnlyCollection<Flag> Flags
        {
            get { return flags.AsReadOnly(); }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Flags));
                }
                flags = value.ToList();
            }
        }
        [JsonIgnore]
        public Cuesheet? Cuesheet 
        {
            get { return cuesheet; }
            set { var previousValue = cuesheet; cuesheet = value; OnValidateablePropertyChanged(); OnTraceablePropertyChanged(previousValue); }
        }

        /// <summary>
        /// Indicates that this track has been cloned from another track and is a transparent proxy
        /// </summary>
        [JsonIgnore]
        public Boolean IsCloned { get { return clonedFrom != null; } }
        /// <summary>
        /// Get the original track that this track has been cloned from. Can be null on original objects
        /// </summary>
        public Track? ClonedFrom
        {
            get { return clonedFrom; }
            private set { clonedFrom = value; OnValidateablePropertyChanged(); }
        }
        /// <inheritdoc/>
        public TimeSpan? PreGap 
        {
            get { return preGap; }
            set { var previousValue = preGap; preGap = value; OnTraceablePropertyChanged(previousValue); }
        }
        /// <inheritdoc/>
        public TimeSpan? PostGap 
        {
            get { return postGap; }
            set { var previousValue = postGap; postGap = value; OnTraceablePropertyChanged(previousValue); }
        }
        /// <summary>
        /// Set that this track is linked to the previous track in cuesheet
        /// </summary>
        public Boolean IsLinkedToPreviousTrack
        {
            get { return isLinkedToPreviousTrack; }
            set { var previousValue = IsLinkedToPreviousTrack; isLinkedToPreviousTrack = value; IsLinkedToPreviousTrackChanged?.Invoke(this, EventArgs.Empty); OnTraceablePropertyChanged(previousValue); }
        }

        public String? GetDisplayNameLocalized(ITextLocalizer localizer)
        {
            String? identifierString = null;
            if (Position != null)
            {
                identifierString += String.Format("{0}", Position);
            }
            if (identifierString == null)
            {
                if (String.IsNullOrEmpty(Artist) == false)
                {
                    identifierString += String.Format("{0}", Artist);
                }
                if (String.IsNullOrEmpty(Title) == false)
                {
                    if (identifierString != null)
                    {
                        identifierString += ",";
                    }
                    identifierString += String.Format("{0}", Title);
                }
            }
            return String.Format("{0} ({1})", localizer[nameof(Track)], identifierString);
        }

        /// <summary>
        /// Performs a deep clone of the current object
        /// </summary>
        /// <returns>A deep clone of the current object</returns>
        public Track Clone()
        {
            return new Track(this)
            {
                ClonedFrom = this
            };
        }

        /// <summary>
        /// Copies values from input object to this object
        /// </summary>
        /// <param name="track">Object to copy values from</param>
        public void CopyValues(ITrack<Cuesheet> track)
        {
            if (track == null)
            {
                throw new ArgumentNullException(nameof(track));
            }
            Cuesheet = track.Cuesheet;
            //Use public setter since we need to fire all events with positioning
            Position = track.Position;
            //We use the internal properties because we only want to set the values, everything around like validation or automatic calculation doesn't need to be fired
            artist = track.Artist;
            title = track.Title;
            begin = track.Begin;
            end = track.End;
            flags.Clear();
            flags.AddRange(track.Flags);
            PreGap = track.PreGap;
            PostGap = track.PostGap;
            OnValidateablePropertyChanged();
        }

        ///<inheritdoc/>
        public void SetFlag(Flag flag, SetFlagMode flagMode)
        {
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }
            var previousValue = flags;
            if ((flagMode == SetFlagMode.Add) && (Flags.Contains(flag) == false))
            {
                flags.Add(flag);
            }
            if ((flagMode == SetFlagMode.Remove) && (Flags.Contains(flag)))
            {
                flags.Remove(flag);
            }
            OnTraceablePropertyChanged(previousValue);
        }

        ///<inheritdoc/>
        public void SetFlags(IEnumerable<Flag> flags)
        {
            this.flags.Clear();
            this.flags.AddRange(flags);
        }


        public override string ToString()
        {
            return String.Format("({0} {1},{2} {3},{4} {5},{6} {7},{8} {9},{10} {11})", nameof(Position), Position, nameof(Artist), Artist, nameof(Title), Title, nameof(Begin), Begin, nameof(End), End, nameof(Length), Length);
        }

        protected override void Validate()
        {
            if (Position == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} has no value!", nameof(Position)));
            }
            if ((Position != null) && (Position == 0))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} has invalid value!", nameof(Position)));
            }
            //TODO: Add validation for position beeing negative
            if (String.IsNullOrEmpty(Artist) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Artist)), ValidationErrorType.Warning, "{0} has no value!", nameof(Artist)));
            }
            if (String.IsNullOrEmpty(Title) == true)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Title)), ValidationErrorType.Warning, "{0} has no value!", nameof(Title)));
            }
            if (Begin == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "{0} has no value!", nameof(Begin)));
            }
            else
            {
                if (Begin < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Error, "{0} has invalid timespan!", nameof(Begin)));
                }
            }
            if (End == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "{0} has no value!", nameof(End)));
            }
            else
            {
                if (End < TimeSpan.Zero)
                {
                    validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Error, "{0} has invalid timespan!", nameof(End)));
                }
            }
            if (Length == null)
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Length)), ValidationErrorType.Error, "{0} has no value! Please check {1} and {2}.", nameof(Length), nameof(Begin), nameof(End)));
            }
            //Check track overlapping
            if (Cuesheet != null)
            {
                Cuesheet.Tracks.ToList().ForEach(x => x.RankPropertyValueChanged -= Track_RankPropertyValueChanged);
                List<Track> tracksToAttachEventHandlerTo = new();
                if (Position.HasValue)
                {
                    IEnumerable<Track> tracksWithSamePosition;
                    if (ClonedFrom != null)
                    {
                        tracksWithSamePosition = Cuesheet.Tracks.Where(x => x.Position == Position && x.Equals(this) == false && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksWithSamePosition = Cuesheet.Tracks.Where(x => x.Position == Position && x.Equals(this) == false);
                    }
                    if ((tracksWithSamePosition != null) && (tracksWithSamePosition.Any()))
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksWithSamePosition);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} {1} of this track is already in use by track(s) {2}!", nameof(Position), Position, String.Join(", ", tracksWithSamePosition)));
                    }
                    if (IsCloned == false)
                    {
                        Track? trackAtPosition = Cuesheet.Tracks.ElementAtOrDefault((int)Position.Value - 1);
                        if ((trackAtPosition == null) || (trackAtPosition != this))
                        {
                            validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Position)), ValidationErrorType.Error, "{0} {1} of this track does not match track position in cuesheet. Please correct the {2} of this track to {3}!", nameof(Position), Position, nameof(Position), Cuesheet.Tracks.ToList().IndexOf(this) + 1));
                        }
                    }
                }
                if (Begin.HasValue)
                {
                    IEnumerable<Track> tracksOverlapping;
                    if (ClonedFrom != null)
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => Begin >= x.Begin && Begin < x.End && (x.Equals(this) == false) && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => Begin >= x.Begin && Begin < x.End && (x.Equals(this) == false));
                    }
                    if ((tracksOverlapping != null) && tracksOverlapping.Any())
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksOverlapping);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(Begin)), ValidationErrorType.Warning, "{0} is overlapping with other track(s) ({1})!", nameof(Begin), String.Join(", ", tracksOverlapping)));
                    }
                }
                if (End.HasValue)
                {
                    IEnumerable<Track> tracksOverlapping;
                    if (ClonedFrom != null)
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => x.Begin < End && End <= x.End && (x.Equals(this) == false) && (x.Equals(ClonedFrom) == false));
                    }
                    else
                    {
                        tracksOverlapping = Cuesheet.Tracks.Where(x => x.Begin < End && End <= x.End && (x.Equals(this) == false));
                    }
                    if ((tracksOverlapping != null) && tracksOverlapping.Any())
                    {
                        tracksToAttachEventHandlerTo.AddRange(tracksOverlapping);
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(End)), ValidationErrorType.Warning, "{0} is overlapping with other track(s) ({1})!", nameof(End), String.Join(", ", tracksOverlapping)));
                    }
                }
                tracksToAttachEventHandlerTo.ForEach(x => x.RankPropertyValueChanged += Track_RankPropertyValueChanged);
            }
        }

        protected void OnTraceablePropertyChanged(object? previousValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var changes = new Stack<TraceableChange>();
            changes.Push(new TraceableChange(previousValue, propertyName));
            TraceablePropertyChanged?.Invoke(this, new TraceablePropertiesChangedEventArgs(changes));
        }

        private void Track_RankPropertyValueChanged(object? sender, string e)
        {
            if (sender != null)
            {
                Track track = (Track)sender;
                track.RankPropertyValueChanged -= Track_RankPropertyValueChanged;
                OnValidateablePropertyChanged();
            }
            else
            {
                throw new ArgumentNullException(nameof(sender));
            }
        }
    }
}
