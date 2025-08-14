using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestCategoryServiceTests : ServiceTestBase
    {
        private ContestCategoryService _contestCategoryService;
        private Mock<IContestCategoryRepository> _mockContestCategoryRepository;
        private Mock<ILogger<ContestCategoryService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<ContestCategoryService>();
            _mockContestCategoryRepository = new Mock<IContestCategoryRepository>();
            MockUnitOfWork.Setup(x => x.ContestCategoryRepository).Returns(_mockContestCategoryRepository.Object);
            _contestCategoryService = new ContestCategoryService(MockUnitOfWork.Object, _mockLogger.Object, MockDateTimeProvider.Object);
        }

        [Test]
        public async Task GetAllCategoriesAsyncShouldReturnPaginatedCategoriesWhenCategoriesExist()
        {
            // Arrange
            var page = 1;
            var pageSize = 5;
            var totalCount = 10;

            var categories = new List<ContestCategory>
            {
                CreateTestCategory(1, "Aquascaping", "Beautiful aquascape designs"),
                CreateTestCategory(2, "Fish Photography", "Best fish photos"),
                CreateTestCategory(3, "Tank Setup", "Creative tank setups")
            };

            _mockContestCategoryRepository
                .Setup(x => x.GetCategoriesWithPaginationAsync(page, pageSize, It.IsAny<string>()))
                .ReturnsAsync((categories, totalCount));

            // Act
            var result = await _contestCategoryService.GetAllCategoriesAsync(page, pageSize);

            // Assert
            Assert.That(result.Categories, Is.Not.Null);

            var categoriesList = result.Categories.ToList();
            Assert.That(categoriesList.Count, Is.EqualTo(3));
            Assert.That(result.TotalCount, Is.EqualTo(totalCount));

            var firstCategory = categoriesList.First();
            Assert.That(firstCategory.Id, Is.EqualTo(1));
            Assert.That(firstCategory.Name, Is.EqualTo("Aquascaping"));
            Assert.That(firstCategory.Description, Is.EqualTo("Beautiful aquascape designs"));

            _mockContestCategoryRepository.Verify(x => x.GetCategoriesWithPaginationAsync(page, pageSize, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CreateCategoryAsyncShouldCreateCategoryWhenValidDataProvided()
        {
            // Arrange
            var createModel = new ContestCategoryCreateViewModel
            {
                Name = "New Category",
                Description = "Test description"
            };

            var createdCategory = CreateTestCategory(1, createModel.Name, createModel.Description);

            _mockContestCategoryRepository
                .Setup(x => x.IsCategoryNameUniqueAsync(createModel.Name, It.IsAny<int?>()))
                .ReturnsAsync(true);

            _mockContestCategoryRepository
                .Setup(x => x.AddAsync(It.IsAny<ContestCategory>()))
                .Callback<ContestCategory>(c => c.Id = 1)
                .Returns(Task.CompletedTask);

            MockUnitOfWork
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _contestCategoryService.CreateCategoryAsync(createModel);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(createModel.Name, It.IsAny<int?>()), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.AddAsync(It.Is<ContestCategory>(c =>
                c.Name == createModel.Name &&
                c.Description == createModel.Description)), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateCategoryAsyncShouldThrowExceptionWhenCategoryNameAlreadyExists()
        {
            // Arrange
            var createModel = new ContestCategoryCreateViewModel
            {
                Name = "Existing Category",
                Description = "Test description"
            };

            _mockContestCategoryRepository
                .Setup(x => x.IsCategoryNameUniqueAsync(createModel.Name, It.IsAny<int?>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _contestCategoryService.CreateCategoryAsync(createModel));

            Assert.That(exception.Message, Is.EqualTo("A category with the name 'Existing Category' already exists."));

            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(createModel.Name, It.IsAny<int?>()), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.AddAsync(It.IsAny<ContestCategory>()), Times.Never);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task UpdateCategoryAsyncShouldUpdateCategoryWhenValidDataProvided()
        {
            // Arrange
            var categoryId = 1;
            var updateModel = new ContestCategoryEditViewModel
            {
                Name = "Updated Category",
                Description = "Updated description"
            };

            var existingCategory = CreateTestCategory(categoryId, "Original Category", "Original description");

            _mockContestCategoryRepository
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockContestCategoryRepository
                .Setup(x => x.IsCategoryNameUniqueAsync(updateModel.Name, categoryId))
                .ReturnsAsync(true);

            _mockContestCategoryRepository
                .Setup(x => x.Update(It.IsAny<ContestCategory>()))
                .Verifiable();

            MockUnitOfWork
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _contestCategoryService.UpdateCategoryAsync(categoryId, updateModel);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(existingCategory.Name, Is.EqualTo(updateModel.Name));
            Assert.That(existingCategory.Description, Is.EqualTo(updateModel.Description));

            _mockContestCategoryRepository.Verify(x => x.GetByIdAsync(categoryId), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(updateModel.Name, categoryId), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.Update(existingCategory), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteCategoryAsyncShouldThrowExceptionWhenCategoryHasAssociatedContests()
        {
            // Arrange
            var categoryId = 1;
            var categoryWithContests = CreateTestCategory(categoryId, "Category With Contests", "Description");

            // Simulate category with associated contests
            var contest = CreateTestContest(1);
            var contestCategory = new ContestsCategories
            {
                ContestId = 1,
                CategoryId = categoryId,
                Contest = contest,
                Category = categoryWithContests
            };

            categoryWithContests.ContestsCategories = new List<ContestsCategories> { contestCategory };

            _mockContestCategoryRepository
                .Setup(x => x.GetCategoryWithContestsAsync(categoryId))
                .ReturnsAsync(categoryWithContests);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _contestCategoryService.DeleteCategoryAsync(categoryId));

            Assert.That(exception.Message,
                Is.EqualTo("Cannot delete a category that has associated contests. Please remove the category from all contests first."));

            _mockContestCategoryRepository.Verify(x => x.GetCategoryWithContestsAsync(categoryId), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.Update(It.IsAny<ContestCategory>()), Times.Never);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task GetCategoryByIdAsyncShouldReturnCategoryWhenExists()
        {
            // Arrange
            var categoryId = 1;
            var category = CreateTestCategory(categoryId, "Test Category", "Test Description");

            _mockContestCategoryRepository
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _contestCategoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(categoryId));
            Assert.That(result.Name, Is.EqualTo("Test Category"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));

            _mockContestCategoryRepository.Verify(x => x.GetByIdAsync(categoryId), Times.Once);
        }

        [Test]
        public async Task GetCategoryByIdAsyncShouldReturnNullWhenNotExists()
        {
            // Arrange
            var categoryId = 999;

            _mockContestCategoryRepository
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((ContestCategory?)null);

            // Act
            var result = await _contestCategoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.That(result, Is.Null);

            _mockContestCategoryRepository.Verify(x => x.GetByIdAsync(categoryId), Times.Once);
        }

        [Test]
        public async Task UpdateCategoryAsyncShouldReturnFalseWhenCategoryNotFound()
        {
            // Arrange
            var categoryId = 999;
            var updateModel = new ContestCategoryEditViewModel
            {
                Name = "Updated Category",
                Description = "Updated description"
            };

            _mockContestCategoryRepository
                .Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((ContestCategory?)null);

            // Act
            var result = await _contestCategoryService.UpdateCategoryAsync(categoryId, updateModel);

            // Assert
            Assert.That(result, Is.False);

            _mockContestCategoryRepository.Verify(x => x.GetByIdAsync(categoryId), Times.Once);
            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
            _mockContestCategoryRepository.Verify(x => x.Update(It.IsAny<ContestCategory>()), Times.Never);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task IsCategoryNameUniqueAsyncShouldReturnTrueWhenNameIsUnique()
        {
            // Arrange
            var categoryName = "Unique Category Name";

            _mockContestCategoryRepository
                .Setup(x => x.IsCategoryNameUniqueAsync(categoryName, null))
                .ReturnsAsync(true);

            // Act
            var result = await _contestCategoryService.IsCategoryNameUniqueAsync(categoryName);

            // Assert
            Assert.That(result, Is.True);

            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(categoryName, null), Times.Once);
        }

        [Test]
        public async Task IsCategoryNameUniqueAsyncShouldReturnFalseWhenNameExists()
        {
            // Arrange
            var categoryName = "Existing Category Name";

            _mockContestCategoryRepository
                .Setup(x => x.IsCategoryNameUniqueAsync(categoryName, null))
                .ReturnsAsync(false);

            // Act
            var result = await _contestCategoryService.IsCategoryNameUniqueAsync(categoryName);

            // Assert
            Assert.That(result, Is.False);

            _mockContestCategoryRepository.Verify(x => x.IsCategoryNameUniqueAsync(categoryName, null), Times.Once);
        }

        #region Helper Methods

        private ContestCategory CreateTestCategory(int id, string name, string? description = null, bool isDeleted = false)
        {
            return new ContestCategory
            {
                Id = id,
                Name = name,
                Description = description,
                IsDeleted = isDeleted,
                CreatedAt = TestDateTime.AddDays(-5),
                UpdatedAt = TestDateTime,
                CreatedBy = "test-admin",
                UpdatedBy = "test-admin",
                ContestsCategories = new List<ContestsCategories>()
            };
        }

        #endregion
    }
}
