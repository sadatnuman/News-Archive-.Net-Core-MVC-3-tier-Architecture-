using AutoMapper;
using AutoMapper.Internal;
using BLL.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace NewsArchive.Tests.Security
{
    public class MappingDepthTests
    {
        [Fact]
        public void Every_mapping_has_a_depth_limit()
        {
            var mapper = new ServiceCollection().AddLogging().AddAutoMapper(typeof(MappingProfile)).BuildServiceProvider().GetRequiredService<IMapper>();
            var maps = mapper.ConfigurationProvider.Internal().GetAllTypeMaps();

            Assert.NotEmpty(maps);
            Assert.All(maps, map => Assert.InRange(map.MaxDepth, 1, MappingProfile.MaxDepth));
        }
    }
}
