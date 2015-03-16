using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo {
    public class JobMap : ClassMap<Job> {
        public JobMap() {
            Id(x => x.Id);
            Map(x => x.Title);
            Map(x => x.Salary);
        }
    }

    public class PersonMap : ClassMap<Person> {
        public PersonMap() {
            Id(x => x.Id);
            Map(x => x.Name);
            Map(x => x.Birth);
            Map(x => x.Address);
            References(x => x.Job).Column("JobId");
        }
    }

}

