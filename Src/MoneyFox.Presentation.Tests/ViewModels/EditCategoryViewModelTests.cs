﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Foundation.Resources;
using MoneyFox.Presentation.ViewModels;
using Moq;
using Should;
using Xunit;

namespace MoneyFox.Presentation.Tests.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class EditCategoryViewModelTests
    {
        [Fact]
        public async Task Prepare_CategoryLoaded()
        {
            // Arrange
            const int categoryId = 99;
            var crudServiceMock = new Mock<ICrudServicesAsync>();
            crudServiceMock.Setup(x => x.ReadSingleAsync<CategoryViewModel>(It.IsAny<int>())).ReturnsAsync(new CategoryViewModel());

            var editAccountVm = new EditCategoryViewModel(crudServiceMock.Object, null, null, null, null);

            // Act
            editAccountVm.CategoryId = categoryId;
            await editAccountVm.InitializeCommand.ExecuteAsync();

            // Assert
            crudServiceMock.Verify(x => x.ReadSingleAsync<CategoryViewModel>(categoryId), Times.Once);
        }

        [Fact]
        public async Task Prepare_Title_Set()
        {
            // Arrange
            const int categoryId = 99;
            var crudServiceMock = new Mock<ICrudServicesAsync>();
            crudServiceMock.Setup(x => x.ReadSingleAsync<CategoryViewModel>(It.IsAny<int>())).ReturnsAsync(new CategoryViewModel());

            var editAccountVm = new EditCategoryViewModel(crudServiceMock.Object, null, null, null, null);

            // Act
            editAccountVm.CategoryId = categoryId;
            await editAccountVm.InitializeCommand.ExecuteAsync();

            // Assert
            editAccountVm.Title.ShouldContain(Strings.EditCategoryTitle);
        }
    }
}
