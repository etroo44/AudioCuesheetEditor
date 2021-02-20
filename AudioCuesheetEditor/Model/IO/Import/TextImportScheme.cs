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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO.Import
{
    public class TextImportScheme
    {
        public static readonly String DefaultSchemeCuesheet = "\\A.*%Artist% - %Title%[\\t]{1,}%AudioFile%";
        public static readonly String DefaultSchemeTracks = "%Artist% - %Title%[\\t]{1,}%End%";

        public static readonly TextImportScheme DefaultTextImportScheme = new TextImportScheme 
        { 
            SchemeCuesheet = DefaultSchemeCuesheet,
            SchemeTracks = DefaultSchemeTracks
        };

        private string schemeTracks;
        private string schemeCuesheet;

        public event EventHandler<String> SchemeChanged;

        public String SchemeTracks
        {
            get { return schemeTracks; }
            set
            {
                schemeTracks = value;
                SchemeChanged?.Invoke(this, nameof(SchemeTracks));
            }
        }

        public String SchemeCuesheet
        {
            get { return schemeCuesheet; }
            set
            {
                schemeCuesheet = value;
                SchemeChanged?.Invoke(this, nameof(SchemeCuesheet));
            }
        }
    }
}
