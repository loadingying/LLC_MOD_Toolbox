using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// ResourceService单元测试
    /// </summary>
    public class ResourceServiceTests
    {
        private readonly IResourceService _resourceService;

        public ResourceServiceTests()
        {
            var pathService = new PathService();
            _resourceService = new ResourceService(pathService);
        }

        [Fact]
        public void GetOrCreateHashCache_ShouldReturnObject()
        {
            var result = _resourceService.GetOrCreateHashCache();
            Assert.NotNull(result);
        }

        [Fact]
        public void IsCacheValid_WithNonExistentKey_ShouldReturnFalse()
        {
            var result = _resourceService.IsCacheValid("nonexistent", TimeSpan.FromMinutes(10));
            Assert.False(result);
        }

        [Fact]
        public void UpdateCacheTimestamp_ShouldSetTimestamp()
        {
            _resourceService.UpdateCacheTimestamp("test_key");
            Assert.True(_resourceService.IsCacheValid("test_key", TimeSpan.FromMinutes(1)));
        }

        [Fact]
        public async Task ClearAllCache_ShouldNotThrow()
        {
            await _resourceService.ClearAllCacheAsync();
            var result = _resourceService.GetOrCreateHashCache();
            Assert.NotNull(result);
        }
    }
}
