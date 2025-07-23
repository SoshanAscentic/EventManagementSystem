// <copyright file="NotFoundException.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string resourceName, object key)
            : base($"Entity \"{resourceName}\" with key \"{key}\" was not found.")
        {
        }
    }
}
