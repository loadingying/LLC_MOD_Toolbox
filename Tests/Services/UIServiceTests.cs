using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// UIService单元测试
    /// </summary>
    public class UIServiceTests
    {
        private readonly IUIService _uiService;

        public UIServiceTests()
        {
            var dialogService = new DialogService();
            _uiService = new UIService(dialogService);
        }

        [Fact]
        public void GlobalOperationsEnabled_Initially_ShouldBeTrue()
        {
            Assert.True(_uiService.GlobalOperationsEnabled);
        }

        [Fact]
        public async Task DisableGlobalOperations_ShouldSetFalse()
        {
            await _uiService.DisableGlobalOperationsAsync();
            Assert.False(_uiService.GlobalOperationsEnabled);
        }

        [Fact]
        public async Task EnableAfterDisable_ShouldSetTrue()
        {
            await _uiService.DisableGlobalOperationsAsync();
            await _uiService.EnableGlobalOperationsAsync();
            Assert.True(_uiService.GlobalOperationsEnabled);
        }
    }
}
