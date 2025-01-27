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
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.Entity;
using AudioCuesheetEditor.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportScheme : Validateable
    {
        public const String EnterRegularExpressionHere = "ENTER REGULAR EXPRESSION HERE";

        public static readonly IReadOnlyDictionary<String, String> AvailableSchemeCuesheet;
        public static readonly IReadOnlyDictionary<String, String> AvailableSchemesTrack;

        static TextImportScheme()
        {
            var schemeCuesheetArtist = String.Format("(?'{0}.{1}'{2})", nameof(Cuesheet), nameof(Cuesheet.Artist), EnterRegularExpressionHere);
            var schemeCuesheetTitle = String.Format("(?'{0}.{1}'{2})", nameof(Cuesheet), nameof(Cuesheet.Title), EnterRegularExpressionHere);
            var schemeCuesheetAudioFile = String.Format("(?'{0}.{1}'{2})", nameof(Cuesheet), nameof(Cuesheet.Audiofile), EnterRegularExpressionHere);
            var schemeCuesheetCDTextfile = String.Format("(?'{0}.{1}'{2})", nameof(Cuesheet), nameof(Cuesheet.CDTextfile), EnterRegularExpressionHere);
            var schemeCuesheetCatalogueNumber = String.Format("(?'{0}.{1}'{2})", nameof(Cuesheet), nameof(Cuesheet.Cataloguenumber), EnterRegularExpressionHere);

            AvailableSchemeCuesheet = new Dictionary<string, string>
            {
                {nameof(Cuesheet.Artist), schemeCuesheetArtist },
                {nameof(Cuesheet.Title), schemeCuesheetTitle },
                {nameof(Cuesheet.Audiofile), schemeCuesheetAudioFile },
                {nameof(Cuesheet.Cataloguenumber), schemeCuesheetCatalogueNumber },
                {nameof(Cuesheet.CDTextfile), schemeCuesheetCDTextfile }
            };

            var schemeTrackArtist = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Artist), EnterRegularExpressionHere);
            var schemeTrackTitle = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Title), EnterRegularExpressionHere);
            var schemeTrackBegin = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Begin), EnterRegularExpressionHere);
            var schemeTrackEnd = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.End), EnterRegularExpressionHere);
            var schemeTrackLength = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Length), EnterRegularExpressionHere);
            var schemeTrackPosition = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Position), EnterRegularExpressionHere);
            var schemeTrackFlags = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.Flags), EnterRegularExpressionHere);
            var schemeTrackPreGap = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.PreGap), EnterRegularExpressionHere);
            var schemeTrackPostGap = String.Format("(?'{0}.{1}'{2})", nameof(Track), nameof(Track.PostGap), EnterRegularExpressionHere);

            AvailableSchemesTrack = new Dictionary<string, string>
            {
                { nameof(Track.Position), schemeTrackPosition },
                { nameof(Track.Artist), schemeTrackArtist },
                { nameof(Track.Title), schemeTrackTitle },
                { nameof(Track.Begin), schemeTrackBegin },
                { nameof(Track.End), schemeTrackEnd },
                { nameof(Track.Length), schemeTrackLength },
                { nameof(Track.Flags), schemeTrackFlags },
                { nameof(Track.PreGap), schemeTrackPreGap },
                { nameof(Track.PostGap), schemeTrackPostGap }
            };
        }

        public static readonly String DefaultSchemeCuesheet = @"(?'Cuesheet.Artist'\A.*) - (?'Cuesheet.Title'\w{1,})\t{1,}(?'Cuesheet.Audiofile'.{1,})";
        public static readonly String DefaultSchemeTracks = @"(?'Track.Artist'[a-zA-Z0-9_ .();äöü&:,'*-]{1,}) - (?'Track.Title'[a-zA-Z0-9_ .();äöü&'*-]{1,})\t{0,}(?'Track.End'.{1,})";

        public static readonly TextImportScheme DefaultTextImportScheme = new()
        { 
            SchemeCuesheet = DefaultSchemeCuesheet,
            SchemeTracks = DefaultSchemeTracks
        };

        private string? schemeTracks;
        private string? schemeCuesheet;

        public event EventHandler<String>? SchemeChanged;

        public String? SchemeTracks
        {
            get { return schemeTracks; }
            set
            {
                schemeTracks = value;
                OnValidateablePropertyChanged();
                SchemeChanged?.Invoke(this, nameof(SchemeTracks));
            }
        }

        public String? SchemeCuesheet
        {
            get { return schemeCuesheet; }
            set
            {
                schemeCuesheet = value;
                OnValidateablePropertyChanged();
                SchemeChanged?.Invoke(this, nameof(SchemeCuesheet));
            }
        }

        protected override void Validate()
        {
            if (String.IsNullOrEmpty(SchemeCuesheet))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeCuesheet)), ValidationErrorType.Warning, "{0} has no value!", nameof(SchemeCuesheet)));
            }
            else
            {
                if (AvailableSchemesTrack != null)
                {
                    List<String> schemesFound = new();
                    foreach (var availableScheme in AvailableSchemesTrack)
                    {
                        if (SchemeCuesheet.Contains(availableScheme.Value.Substring(0, availableScheme.Value.IndexOf(EnterRegularExpressionHere))) == true)
                        {
                            var startIndex = SchemeCuesheet.IndexOf(availableScheme.Value.Substring(0, availableScheme.Value.IndexOf(EnterRegularExpressionHere)));
                            var realRegularExpression = SchemeCuesheet.Substring(startIndex, (SchemeCuesheet.IndexOf(")", startIndex) + 1) - startIndex);
                            schemesFound.Add(realRegularExpression);
                        }
                    }
                    if (schemesFound.Count > 0)
                    {
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeCuesheet)), ValidationErrorType.Warning, "Scheme contains placeholders that can not be solved! Please remove invalid placeholder '{0}'.", String.Join(",", schemesFound)));
                    }
                }
            }
            if (String.IsNullOrEmpty(SchemeTracks))
            {
                validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeTracks)), ValidationErrorType.Warning, "{0} has no value!", nameof(SchemeTracks)));
            }
            else
            {
                if (AvailableSchemeCuesheet != null)
                {
                    List<String> schemesFound = new();
                    foreach (var availableScheme in AvailableSchemeCuesheet)
                    {
                        if (SchemeTracks.Contains(availableScheme.Value.Substring(0, availableScheme.Value.IndexOf(EnterRegularExpressionHere))) == true)
                        {
                            var startIndex = SchemeTracks.IndexOf(availableScheme.Value.Substring(0, availableScheme.Value.IndexOf(EnterRegularExpressionHere)));
                            var realRegularExpression = SchemeTracks.Substring(startIndex, (SchemeTracks.IndexOf(")", startIndex) + 1) - startIndex);
                            schemesFound.Add(realRegularExpression);
                        }
                    }
                    if (schemesFound.Count > 0)
                    {
                        validationErrors.Add(new ValidationError(FieldReference.Create(this, nameof(SchemeTracks)), ValidationErrorType.Warning, "Scheme contains placeholders that can not be solved! Please remove invalid placeholder '{0}'.", String.Join(",", schemesFound)));
                    }
                }
            }
        }
    }
}
