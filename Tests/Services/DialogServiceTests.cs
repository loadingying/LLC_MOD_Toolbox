using LLC_MOD_Toolbox.Services;
using Xunit;

namespace LLC_MOD_Toolbox.Tests.Services
{
    public class DialogServiceTests
    {
        [Fact]
        public async System.Threading.Tasks.Task HeadlessMode_ShouldNotShowDialogs()
        {
            var previous = Environment.GetEnvironmentVariable("LLCMT_HEADLESS");
            Environment.SetEnvironmentVariable("LLCMT_HEADLESS", "1");

            try
            {
                var service = new DialogService();

                service.ShowMessage("msg");
                service.ShowError("err");
                service.ShowWarning("warn");
                var confirm = service.ShowConfirm("confirm");

                var files = await service.ShowOpenFileDialogAsync();
                var folder = await service.ShowFolderDialogAsync();

                Assert.False(confirm);
                Assert.Empty(files);
                Assert.Null(folder);
            }
            finally
            {
                Environment.SetEnvironmentVariable("LLCMT_HEADLESS", previous);
            }
        }
    }
}
