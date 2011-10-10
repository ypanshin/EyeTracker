﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode.Conformist;

namespace EyeTracker.Domain.Model
{

    public class ApplicationMapping : ClassMapping<Application>
    {
        public ApplicationMapping()
        {
            Table("aspnet_Applications");
            Id(x => x.Id, map => map.Column("ApplicationId"));
            Property(x => x.Name, map =>
            {
                map.Column("ApplicationName");
                map.Length(255);
                map.NotNullable(true);
            });
            Property(x => x.Description, map => map.Length(255));
            Bag(x => x.Roles, map =>
            {
                map.Key(k => k.Column("ApplicationId"));
            }, r => r.OneToMany());
            Bag(x => x.Users, map =>
            {
                map.Key(k => k.Column("ApplicationId"));
            }, r => r.OneToMany());
        }
    }

}
