using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal sealed class DefaultConfigReader
    {
        private readonly IConfiguration _configuration;

        public DefaultConfigReader(IConfiguration configuration) => _configuration = configuration;
    }
}
