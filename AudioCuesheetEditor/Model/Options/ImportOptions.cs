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

using AudioCuesheetEditor.Model.IO.Import;
using AudioCuesheetEditor.Model.Utility;

namespace AudioCuesheetEditor.Model.Options
{
    public class ImportOptions : IOptions
    {
        public TextImportScheme TextImportScheme { get; set; }
        public TimeSpanFormat? TimeSpanFormat { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        public ImportOptions()
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
        {
            SetDefaultValues();
        }

        public ImportOptions(TextImportfile textImportfile)
        {
            TextImportScheme = textImportfile.TextImportScheme;
            TimeSpanFormat = textImportfile.TimeSpanFormat;
        }

        public void SetDefaultValues()
        {
            //Declare defaults
            TextImportScheme ??= TextImportScheme.DefaultTextImportScheme;
            TimeSpanFormat ??= new TimeSpanFormat();
        }
    }
}
