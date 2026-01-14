using LLC_MOD_Toolbox.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace LLC_MOD_Toolbox.Tests.Services
{
    /// <summary>
    /// LinkService单元测试
    /// </summary>
    public class LinkServiceTests
    {
        private readonly LinkService _linkService;

        public LinkServiceTests()
        {
            _linkService = new LinkService();
        }

        [Fact]
        public void InitializeDefaultLinks_ShouldAddTenLinks()
        {
            // Arrange & Act
            _linkService.InitializeDefaultLinks();
            var links = _linkService.GetLinks();

            // Assert
            Assert.Equal(10, links.Count);
        }

        [Fact]
        public void GetUrl_WithExistingLink_ShouldReturnUrl()
        {
            // Arrange
            _linkService.InitializeDefaultLinks();

            // Act
            var url = _linkService.GetUrl("LinkButton1");

            // Assert
            Assert.Equal("https://www.zeroasso.top", url);
        }

        [Fact]
        public void GetUrl_WithNonExistingLink_ShouldReturnNull()
        {
            // Arrange
            _linkService.InitializeDefaultLinks();

            // Act
            var url = _linkService.GetUrl("NonExistingButton");

            // Assert
            Assert.Null(url);
        }

        [Fact]
        public void GetUrl_WithEmptyControlName_ShouldReturnNull()
        {
            // Arrange
            _linkService.InitializeDefaultLinks();

            // Act
            var url = _linkService.GetUrl("");

            // Assert
            Assert.Null(url);
        }

        [Fact]
        public void AddLink_ShouldAddNewLink()
        {
            // Arrange
            const string controlName = "TestButton";
            const string url = "https://test.com";

            // Act
            _linkService.AddLink(controlName, url);
            var retrievedUrl = _linkService.GetUrl(controlName);

            // Assert
            Assert.Equal(url, retrievedUrl);
        }

        [Fact]
        public void AddLink_WithEmptyControlName_ShouldNotAdd()
        {
            // Arrange
            const string controlName = "";
            const string url = "https://test.com";

            // Act
            _linkService.AddLink(controlName, url);

            // Assert
            Assert.Null(_linkService.GetUrl(controlName));
        }

        [Fact]
        public void RemoveLink_WithExistingLink_ShouldReturnTrue()
        {
            // Arrange
            _linkService.InitializeDefaultLinks();

            // Act
            var removed = _linkService.RemoveLink("LinkButton1");

            // Assert
            Assert.True(removed);
            Assert.Null(_linkService.GetUrl("LinkButton1"));
        }

        [Fact]
        public void RemoveLink_WithNonExistingLink_ShouldReturnFalse()
        {
            // Arrange
            _linkService.InitializeDefaultLinks();

            // Act
            var removed = _linkService.RemoveLink("NonExistingButton");

            // Assert
            Assert.False(removed);
        }
    }
}
