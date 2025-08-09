using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Business.Rules;
using DreamAquascape.Services.Core.Tests.Infrastructure;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestBusinessRulesTests : ServiceTestBase
    {
        private ContestBusinessRules _businessRules = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _businessRules = new ContestBusinessRules();
        }

        [Test]
        public void CanUserVoteShouldReturnTrueWhenValidActiveContestDuringVotingPeriod()
        {
            // Arrange
            var contest = CreateTestContest(1, true);
            contest.IsDeleted = false;
            contest.VotingStartDate = TestDateTime.AddDays(-1); // Started yesterday
            contest.VotingEndDate = TestDateTime.AddDays(1);    // Ends tomorrow

            // Act
            var result = _businessRules.CanUserVote(contest, "test-user", TestDateTime);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanUserVoteShouldReturnFalseWhenContestIsInactiveOrDeleted()
        {
            // Arrange
            var contest = CreateTestContest(1, false); // Inactive contest
            contest.IsDeleted = true;
            contest.VotingStartDate = TestDateTime.AddDays(-1);
            contest.VotingEndDate = TestDateTime.AddDays(1);

            // Act
            var result = _businessRules.CanUserVote(contest, "test-user", TestDateTime);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsVotingPeriodActiveShouldReturnTrueWhenCurrentTimeIsWithinVotingPeriod()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.VotingStartDate = TestDateTime.AddDays(-1); // Started yesterday
            contest.VotingEndDate = TestDateTime.AddDays(1);    // Ends tomorrow

            // Act
            var result = _businessRules.IsVotingPeriodActive(contest, TestDateTime);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsVotingPeriodActiveShouldReturnFalseWhenVotingHasNotStartedOrEnded()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.VotingStartDate = TestDateTime.AddDays(1);  // Starts tomorrow
            contest.VotingEndDate = TestDateTime.AddDays(5);    // Ends in 5 days

            // Act
            var result = _businessRules.IsVotingPeriodActive(contest, TestDateTime);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsSubmissionPeriodActiveShouldReturnTrueWhenCurrentTimeIsWithinSubmissionPeriod()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.SubmissionStartDate = TestDateTime.AddDays(-1); // Started yesterday
            contest.SubmissionEndDate = TestDateTime.AddDays(1);    // Ends tomorrow

            // Act
            var result = _businessRules.IsSubmissionPeriodActive(contest, TestDateTime);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsSubmissionPeriodActiveShouldReturnFalseWhenSubmissionHasNotStartedOrEnded()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.SubmissionStartDate = TestDateTime.AddDays(-5); // Started 5 days ago
            contest.SubmissionEndDate = TestDateTime.AddDays(-1);   // Ended yesterday

            // Act
            var result = _businessRules.IsSubmissionPeriodActive(contest, TestDateTime);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsAnnouncementPeriodShouldReturnTrueWhenVotingHasEnded()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.VotingEndDate = TestDateTime.AddDays(-1); // Ended yesterday

            // Act
            var result = _businessRules.IsAnnouncementPeriod(contest, TestDateTime);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
