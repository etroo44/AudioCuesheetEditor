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
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportFile
    {
        public const String MimeType = "text/plain";
        public const String FileExtension = ".txt";

        private TextImportScheme textImportScheme;

        public TextImportFile(MemoryStream fileContent, ImportOptions? importOptions = null)
        {
            textImportScheme = new TextImportScheme();
            fileContent.Position = 0;
            using var reader = new StreamReader(fileContent);
            List<String?> lines = new();
            while (reader.EndOfStream == false)
            {
                lines.Add(reader.ReadLine());
            }
            FileContent = lines.AsReadOnly();
            if (importOptions == null)
            {
                TextImportScheme = TextImportScheme.DefaultTextImportScheme;
            }
            else
            {
                TextImportScheme = importOptions.TextImportScheme;
            }
        }

        /// <summary>
        /// File content (each element is a file line)
        /// </summary>
        public IReadOnlyCollection<String?> FileContent { get; private set; }

        /// <summary>
        /// File content with marking which passages has been reconized by scheme
        /// </summary>
        public IReadOnlyCollection<String?>? FileContentRecognized { get; private set; }

        public TextImportScheme TextImportScheme 
        {
            get { return textImportScheme; }
            set 
            { 
                if (textImportScheme != null)
                {
                    textImportScheme.SchemeChanged -= TextImportScheme_SchemeChanged;
                }
                textImportScheme = value;
                textImportScheme.SchemeChanged += TextImportScheme_SchemeChanged;
                Analyse();
            }
        }
        
        public Exception? AnalyseException { get; private set; }

        public Boolean IsValid { get { return AnalyseException == null; } }

        public Cuesheet? Cuesheet { get; private set; }

        private void TextImportScheme_SchemeChanged(object? sender, string e)
        {
            Analyse();
        }

        private void Analyse()
        {
            try
            {
                Cuesheet = new Cuesheet();
                FileContentRecognized = null;
                AnalyseException = null;
                var regicesCuesheet = AnalyseScheme(TextImportScheme.SchemeCuesheet);
                var regicesTracks = AnalyseScheme(TextImportScheme.SchemeTracks);
                Boolean cuesheetRecognized = false;
                List<String?> recognizedFileContent = new();
                foreach (var line in FileContent)
                {
                    var recognizedLine = line;
                    if (String.IsNullOrEmpty(line) == false)
                    {
                        Boolean recognized = false;
                        if ((recognized == false) && (cuesheetRecognized == false) && (regicesCuesheet.Count > 0))
                        {
                            var firstRegiceCuesheet = regicesCuesheet.First();
                            if (firstRegiceCuesheet.Value.IsMatch(line))
                            {
                                recognized = true;
                                cuesheetRecognized = true;
                                recognizedLine = AnalyseLine(line, Cuesheet, regicesCuesheet);
                            }
                        }
                        if ((recognized == false) && (regicesTracks.Count > 0))
                        {
                            var firstRegiceTrack = regicesTracks.First();
                            if (firstRegiceTrack.Value.IsMatch(line))
                            {
                                recognized = true;
                                var track = new Track();
                                recognizedLine = AnalyseLine(line, track, regicesTracks);
                                Cuesheet.AddTrack(track);
                            }
                        }
                    }
                    recognizedFileContent.Add(recognizedLine);
                }
                if (recognizedFileContent.Count > 0)
                {
                    FileContentRecognized = recognizedFileContent.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                FileContentRecognized = FileContent;
                AnalyseException = ex;
                Cuesheet = null;
            }
        }

        /// <summary>
        /// Analyse the input format scheme
        /// </summary>
        /// <param name="scheme">Scheme to analyse</param>
        /// <returns>A read only dictionary with regular expressions and in format: "PropertyBefore", "PropertyAfter", Regular Expression </returns>
        private IReadOnlyDictionary<Tuple<String, String>, Regex> AnalyseScheme(String? scheme)
        {
            Dictionary<Tuple<String, String>, Regex> regices = new();
            try
            {
                AnalyseException = null;
                if (String.IsNullOrEmpty(scheme) == false)
                {
                    for (int i = 0; i <= scheme.Length; i++)
                    {
                        if (scheme.Substring(i).StartsWith(TextImportScheme.SchemeCharacter) == true)
                        {
                            var endIndex = scheme.IndexOf(TextImportScheme.SchemeCharacter, i + 1);
                            //Only search if there is somethine behind the Scheme identifier
                            if ((endIndex > 0) && ((endIndex + 1) < scheme.Length))
                            {
                                var propertyBefore = scheme.Substring(i + TextImportScheme.SchemeCharacter.Length, endIndex - (i + TextImportScheme.SchemeCharacter.Length));
                                var nextPropertyStartIndex = scheme.IndexOf(TextImportScheme.SchemeCharacter, endIndex + 1);
                                var nextPropertyEndIndex = scheme.IndexOf(TextImportScheme.SchemeCharacter, nextPropertyStartIndex + 1);
                                var propertyAfter = scheme.Substring(nextPropertyStartIndex + 1, nextPropertyEndIndex - (nextPropertyStartIndex + 1));
                                var regExString = scheme.Substring(endIndex + 1, nextPropertyStartIndex - (endIndex + 1));
                                //Remove entity names
                                propertyBefore = propertyBefore.Replace(string.Format("{0}.", nameof(AudioCuesheet.Cuesheet)), string.Empty).Replace(string.Format("{0}.", nameof(AudioCuesheet.Track)), string.Empty).Replace(string.Format("{0}.", nameof(Cuesheet)), string.Empty);
                                propertyAfter = propertyAfter.Replace(string.Format("{0}.", nameof(AudioCuesheet.Cuesheet)), string.Empty).Replace(string.Format("{0}.", nameof(AudioCuesheet.Track)), string.Empty).Replace(string.Format("{0}.", nameof(Cuesheet)), string.Empty);
                                regices.Add(new Tuple<string, string>(propertyBefore, propertyAfter), new Regex(regExString));
                                //Recalculate next index
                                i = nextPropertyStartIndex - 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnalyseException = ex;
            }
            return regices;
        }

        /// <summary>
        /// Analyses a line and sets the properties on the entity
        /// </summary>
        /// <param name="line">Line to analyse</param>
        /// <param name="entity">Entity to set properties on</param>
        /// <param name="regices">Regular expressions for analysing</param>
        /// <returns>Analysed line with marking what has been matched</returns>
        private static String? AnalyseLine(String line, ICuesheetEntity entity, IReadOnlyDictionary<Tuple<String, String>, Regex> regices)
        {
            String? recognized = null;
            if ((String.IsNullOrEmpty(line) == false) && (entity != null) && (regices != null))
            {
                var index = 0;
                foreach (var regexRelation in regices)
                {
                    var match = regexRelation.Value.Match(line.Substring(index));
                    if (match.Success == true)
                    {
                        var propertyValueBefore = line.Substring(index, match.Index);
                        var propertyBefore = entity.GetType().GetProperty(regexRelation.Key.Item1, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var propertyValueAfter = line.Substring(index + match.Index + match.Length);
                        var propertyAfter = entity.GetType().GetProperty(regexRelation.Key.Item2, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        //Check if other regices might match the value after
                        Boolean otherMatchRegEx = false;
                        var elementIndex = regices.ToList().IndexOf(regexRelation);
                        for (int i = elementIndex; i < regices.Count; i++)
                        {
                            var nextRegExRelation = regices.ElementAt(i);
                            if (nextRegExRelation.Value.IsMatch(propertyValueAfter) == true)
                            {
                                otherMatchRegEx = true;
                                i = regices.Count;
                            }
                        }
                        if (propertyBefore != null)
                        {
                            SetValue(entity, propertyBefore, propertyValueBefore);
                        }
                        else
                        {
                            throw new NullReferenceException(String.Format("PropertyInfo before was not found {0} for line content {1}", regexRelation.Key.Item1, line));
                        }
                        if (propertyAfter != null)
                        {
                            //Set recognized
                            recognized += String.Format(CuesheetConstants.RecognizedMarkHTML, line.Substring(index, match.Index)) + match.Value;
                            if (otherMatchRegEx == false)
                            {
                                SetValue(entity, propertyAfter, propertyValueAfter);
                                recognized += String.Format(CuesheetConstants.RecognizedMarkHTML, line.Substring(index + match.Index + match.Length));
                            }
                        }
                        else
                        {
                            throw new NullReferenceException(String.Format("PropertyInfo after was not found {0} for line content {1}", regexRelation.Key.Item2, line));
                        }
                        index = index + match.Index + match.Length;
                    }
                }
            }
            return recognized;
        }

        /// <summary>
        /// Set the value on the entity
        /// </summary>
        /// <param name="entity">Entity object to set the value on</param>
        /// <param name="property">Property to set</param>
        /// <param name="value">Value to set</param>
        private static void SetValue(ICuesheetEntity entity, PropertyInfo property, string value)
        {
            if (property.PropertyType == typeof(TimeSpan?))
            {
                property.SetValue(entity, TimeSpan.Parse(value));
            }
            if (property.PropertyType == typeof(uint?))
            {
                property.SetValue(entity, Convert.ToUInt32(value));
            }
            if (property.PropertyType == typeof(String))
            {
                property.SetValue(entity, value);
            }
            if (property.PropertyType == typeof(IReadOnlyCollection<Flag>))
            {
                var list = Flag.AvailableFlags.Where(x => value.Contains(x.CuesheetLabel));
                ((Track)entity).SetFlags(list);
            }
            if (property.PropertyType == typeof(AudioFile))
            {
                property.SetValue(entity, new AudioFile(value));
            }
            if (property.PropertyType == typeof(Cataloguenumber))
            {
                ((Cuesheet)entity).Cataloguenumber.Value = value;
            }
            if (property.PropertyType == typeof(CDTextfile))
            {
                property.SetValue(entity, new CDTextfile(value));
            }
        }
    }
}
