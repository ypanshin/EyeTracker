﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EyeTracker.Common.QueryResults;
using EyeTracker.Common.QueryResults.Users;

namespace EyeTracker.Common.Queries.Admin
{
    public class AllMembersResult : PageingResult
    {
        public IEnumerable<UserFullDetailsResult> Users { get; set; }
    }
}
