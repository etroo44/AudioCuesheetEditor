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
using AudioCuesheetEditor.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Entity
{
    public enum ValidationErrorFilterType
    {
        All,
        WarningOnly,
        ErrorOnly
    }
    public enum ValidationErrorType
    {
        Warning,
        Error
    }
    public class ValidationError
    {
        public ValidationMessage Message { get; private set; }
        public ValidationErrorType Type { get; private set; }
        public FieldReference FieldReference { get; private set; }

        public ValidationError(FieldReference fieldReference, ValidationErrorType validationErrorType, String message, params object[] messageParameter)
        {
            if (String.IsNullOrEmpty(message) == true)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (fieldReference == null)
            {
                throw new ArgumentNullException(nameof(fieldReference));
            }
            Message = new ValidationMessage(message, messageParameter);
            Type = validationErrorType;
            FieldReference = fieldReference;
        }
    }
}
