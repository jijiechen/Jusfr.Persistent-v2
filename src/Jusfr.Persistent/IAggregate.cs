﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Persistent {

    public interface IAggregate : IEntry<Int32> {
    }

    public interface IAggregate<TKey> : IEntry<TKey> {

    }
}
