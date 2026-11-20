
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RE_Waste.Areas.Manager.Controllers;
using RE_Waste.Data;
using RE_Waste.Extensions;
using RE_Waste.Models;
using RE_Waste.Models.Enums;
using RE_Waste.Services;

namespace RE_Waste.Tests
{
    public class AnswerValidatorTests
    {
        //This class will contain all of the test methods for testing the answer validator and if empty feilds fail and other feilds fail with invalid data

        //Test that validator calls out lacking input (no chars entered)
        [Fact]
        public void Validate_EmptyAnswer_ReturnsRequiredMessage()
        {

            //First I craete a question that is expecting a text (stirng) answer
            var stringQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Text,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // Empty string that will allow us to simulate users invalid input
            string answer = "";

            //!!!!!!!!!!!

            // Now we call the method we are testing

            string? result = AnswerValidator.Validate(stringQuestion, answer);

            // Check the result is as expected

            Assert.Equal(AnswerValidator.RequiredMessage, result);

        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        //Test that validator calls out lacking input (Only Spaces entered)
        [Fact]
        public void Validate_SpacesOnly_ReturnsRequiredMessage()
        {
            //First I craete a question that is expecting a text (stirng) answer
            var stringQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Text,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // Empty string that will allow us to simulate users invalid input
            string answer = "               "; //only spaces

            //!!!!!!!!!!!

            // Now we call the method we are testing

            string? result = AnswerValidator.Validate(stringQuestion, answer);

            // Check the result is as expected

            Assert.Equal(AnswerValidator.RequiredMessage, result);

        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        //Test that the validator sees the age (0) as valid
        [Fact]
        public void Validate_AgeZero_ReturnsNull()
        {
            //First I craete a question that is expecting a number answer
            var intQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Number,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // create a input representing the user inputing a valid minimum age of '0'

            string answer = "0";

            // Call method
            string? result = AnswerValidator.Validate(intQuestion, answer);

            // Check the result is as expected. (null means passed)
            Assert.Null(result);
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        //Test that the validator sees the age (30) as valid
        [Fact]
        public void Validate_AgeThirty_ReturnsNull()
        {
            //First I craete a question that is expecting a number answer
            var intQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Number,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // create a input representing the user inputing a valid maximum age of '30'

            string answer = "30";

            // Call method
            string? result = AnswerValidator.Validate(intQuestion, answer);

            // Check the result is as expected. (null means passed)
            Assert.Null(result);
        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        //Test that the validator sees the age (31) as invalid
        [Fact]
        public void Validate_AgeThirtyOne_ReturnsNumberMessage()
        {

            //First I craete a question that is expecting a number answer
            var intQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Number,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // create a input representing the user inputing a invalid age of '31'

            string answer = "31";

            //Call method we are testing
            string? result = AnswerValidator.Validate(intQuestion, answer);

            //Cehck that the result is as expected
            Assert.Equal(AnswerValidator.NumberMessage, result);

        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||

        //Test that the validator sees the age ("abc") as invalid
        [Fact]
        public void Validate_NonNumericAge_ReturnsNumberMessage()
        {

            //First I craete a question that is expecting a number answer
            var intQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Number,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // create a input representing the user inputing a invalid age of 'abc'

            string answer = "abc";

            //Call method we are testing
            string? result = AnswerValidator.Validate(intQuestion, answer);

            //Cehck that the result is as expected
            Assert.Equal(AnswerValidator.NumberMessage, result);

        }

        //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
        // Test that a brand name longer than 50 characters is rejected
        [Fact]
        public void Validate_BrandOverFiftyCharacters_ReturnsError()
        {
            //First I craete a question that is expecting a text (stirng) answer
            var stringQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Text,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Brand
            };

            // create a string containing the 51 char brand length which exceeds max length

            string answer = new string('A', 51);

            //call the method we are testing
            string? result = AnswerValidator.Validate(stringQuestion, answer);

            //Check the result is as expected
            //any error mesage means validation failed

            Assert.NotNull(result);


        }

        // Test that a model name longer than 100 characters is rejected
        [Fact]
        public void Validate_ModelOverOneHundredCharacters_ReturnsError()
        {

            //First I craete a question that is expecting a text (stirng) answer
            var stringQuestion = new Question
            {
                //Answer type is set to text
                AnswerType = Models.Enums.AnswerType.Text,

                // I also asign a DeviceFeild since text questions need one
                DeviceField = Models.Enums.DeviceField.Model
            };

            // create a string containing the 101 char model length which exceeds max length

            string answer = new string('A', 101);

            //call the method we are testing
            string? result = AnswerValidator.Validate(stringQuestion, answer);

            //Check the result is as expected
            //any error mesage means validation failed

            Assert.NotNull(result);


        }

        // Test that an option not present in the active choices is rejected
        [Fact]
        public void Validate_InvalidChoice_ReturnsChoiceMessage()
        {
            //Create a question that is expecteding a choice answer

            var choiceQuestion = new Question
            {
                AnswerType = Models.Enums.AnswerType.Choice,

                Options = [new AnswerOption { Value = "Yes", IsActive = true }]
            };

            // User selects a value that is not avaialble on options list
            string answer = "No";

            // Call the method we are testing

            string? result = AnswerValidator.Validate(choiceQuestion, answer);

            // Check results are as expected, and that the correct error is returned
            Assert.Equal(AnswerValidator.ChoiceMessage, result);
        }

        // Test that a valid active choice is accepted
        [Fact]
        public void Validate_ActiveChoice_ReturnsNull()
        {
            //Create a question that is expecteding a choice answer

            var choiceQuestion = new Question
            {
                AnswerType = Models.Enums.AnswerType.Choice,

                Options = [new AnswerOption { Value = "Yes", IsActive = true }]
            };

            // User selects a value that is avaialble on options list
            string answer = "Yes";

            // Call the method we are testing

            string? result = AnswerValidator.Validate(choiceQuestion, answer);

            // Check results are as expected, null means validation passed
            Assert.Null(result);
        }

    }





    //||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||


    public class DeviceBuilderTests
    {
        //This class will contain all of the test methods for testing the device builder and if good answers build a good device or if unreadable answers return nulls

        //Test that all required answers exist and are valid
        [Fact]
        public void GoodAnswers_BuildsDevice()
        {
            //First we create a wizzard state with the valid answers

            var state = new WizardState();

            state.Answers[Models.Enums.DeviceField.Brand] = "Apple";
            state.Answers[Models.Enums.DeviceField.Model] = "iPhone 13";
            state.Answers[Models.Enums.DeviceField.Condition] = "Working";
            state.Answers[Models.Enums.DeviceField.AgeInYears] = "2";
            state.Answers[Models.Enums.DeviceField.HasPersonalData] = "True";
            state.Answers[Models.Enums.DeviceField.HasBatteryDamage] = "false";

            //Call method we are testing
            Device? result = DeviceBuilder.FromAnswers(state);

            // Check result is as expected, valid device should be returned
            Assert.NotNull(result);
        }

        // Test that a missing required answer causes the device builder to return null
        [Fact]
        public void MissingAnswer_ReturnsNull()
        {
            //First we create a wizzard state with the valid answers but with one missing
            var state = new WizardState();

            //Brand omitted
            state.Answers[Models.Enums.DeviceField.Model] = "iPhone 13";
            state.Answers[Models.Enums.DeviceField.Condition] = "Working";
            state.Answers[Models.Enums.DeviceField.AgeInYears] = "2";
            state.Answers[Models.Enums.DeviceField.HasPersonalData] = "True";
            state.Answers[Models.Enums.DeviceField.HasBatteryDamage] = "false";

            //Call method we are testing
            Device? result = DeviceBuilder.FromAnswers(state);

            // Check result is as expected, method should return null
            Assert.Null(result);

        }

        // Test that invalid age data causes DeviceBuilder to return null
        [Fact]
        public void UnreadableAnswer_ReturnsNull()
        {
            //First we create a wizzard state with invalid age data
            var state = new WizardState();

            state.Answers[Models.Enums.DeviceField.Brand] = "Apple";
            state.Answers[Models.Enums.DeviceField.Model] = "iPhone 13";

            //Invalid age value
            state.Answers[Models.Enums.DeviceField.AgeInYears] = "abc";
            
            state.Answers[Models.Enums.DeviceField.Condition] = "Working";
            state.Answers[Models.Enums.DeviceField.HasPersonalData] = "True";
            state.Answers[Models.Enums.DeviceField.HasBatteryDamage] = "false";

            //Call method we are testing
            Device? result = DeviceBuilder.FromAnswers(state);

            // Check result is as expected, method should return null
            Assert.Null(result);
        }

        // Test that the condition string is correctly converted into a Condition enum
        [Fact]
        public void Condition_ParsedFromEnumName()
        {
            //First we create a wizzard state with the valid answers

            var state = new WizardState();

            state.Answers[Models.Enums.DeviceField.Brand] = "Apple";
            state.Answers[Models.Enums.DeviceField.Model] = "iPhone 13";
            state.Answers[Models.Enums.DeviceField.Condition] = "Working";
            state.Answers[Models.Enums.DeviceField.AgeInYears] = "2";
            state.Answers[Models.Enums.DeviceField.HasPersonalData] = "True";
            state.Answers[Models.Enums.DeviceField.HasBatteryDamage] = "false";

            // Build the device
            Device? result = DeviceBuilder.FromAnswers(state);

            //Check the results are as expected, ensure that the enum was parsed correctly

            Assert.NotNull(result);
            Assert.Equal(Condition.Working, result.Condition);
        }

    }
    //This class will contain all of the test methods for testing the the Distance Km service see if it provides correct results.
    public class DistanceKmTests
    {
        // Test that the distance between identical coordinates is zero
        [Fact]
        public void DistanceKm_SameLocation_ReturnsZero()
        {
            //First we create coordinates representing the same place

            double latitude = 55.9558;
            double longitude = -3.1884;

            //Find the distance between the 2 same points

            double result = DropOffService.DistanceKm(latitude, longitude, latitude, longitude);

            //Check results are as expected, distance should be 0 Km
            Assert.Equal(0, result, 2);
        }

        // Test that known coordinates return approximately 3.2 km
        [Fact]
        public void DistanceKm_KnownCoordinates_ReturnsApproxThreePointTwoKm()
        {
            // Test the services ability to find the distance between two differnet locations

            double result = DropOffService.DistanceKm(55.9558,-3.1884,55.9748,-3.1500);

            //check the results are as expected, result should roughly equal 3.2km
            Assert.InRange(result, 3.0, 3.4);
        }


    }

    //This class will contain all of the test methods for testing the the recommendation service and to see if it provides dififerent routes up to 3 with a reason each.
    public class RecomendationServiceTests
    {
        // Creates a separate in-memory database for each test
        private ApplicationDbContext GetDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // Creates a standard test device
        private Device GetTestDevice(int age = 2)
        {
            return new Device
            {
                Brand = "Apple",
                Model = "iPhone",
                AgeInYears = age,
                Condition = Condition.Working,
                HasPersonalData = true,
                HasBatteryDamage = false
            };
        }

        // Creates a standard recommendation rule
        private RecommendationRule GetTestRule(
            string name,
            string reason,
            int priority = 1,
            bool isActive = true)
        {
            return new RecommendationRule
            {
                Name = name,
                Reason = reason,
                Priority = priority,
                IsActive = isActive,
                Route = RouteType.Donate,
                MinAgeInYears = 2,
                MaxAgeInYears = 5
            };
        }

        // Test that devices with battery damage only receive a recycle recommendation
        [Fact]
        public async Task BatteryDamaged_ReturnsOnlyRecycle()
        {
            //First we create an in-memory database for these tests
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("BatteryTest").Options;

            var context = new ApplicationDbContext(options);

            // Create recommendation service for tests

            var service = new Models.RecommendationService(context);

            // Create a device containing a damaged battery

            var device = new Device
            {
                Brand = "Apple",
                Model = "iPhone",
                AgeInYears = 2,
                Condition = Condition.Working,
                HasPersonalData = true,
                HasBatteryDamage = true
            };

            // Get the recommendations for this device

            var result = await service.RecommendAsync(device);

            // Check results are as expected, only one recommendation should have come through and it should be recycle

            Assert.Single(result);
            Assert.Equal(RouteType.Recycle, result[0].Route);

        }

        // Test that a device with no matching rules receives a recycle recommendation
        [Fact]
        public async Task NoMatchingRules_ReturnsRecycleRecommendation()
        {
            //First we create an in-memory database for these tests
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("NoRulesTest").Options;

            var context = new ApplicationDbContext(options);

            // Create recommendation service for tests

            var service = new Models.RecommendationService(context);

            // Create a device

            var device = new Device
            {
                Brand = "Apple",
                Model = "iPhone",
                AgeInYears = 2,
                Condition = Condition.Working,
                HasPersonalData = true,
                HasBatteryDamage = false
            };

            // Get the recommendations for this device

            var result = await service.RecommendAsync(device);

            // Check results are as expected, should fall back on to recycle

            Assert.Single(result);
            Assert.Equal(RouteType.Recycle, result[0].Route);
            Assert.Contains("does not have much information", result[0].Reason);

        }

        // Test that matching rules generate recommendations containing reasons
        [Fact]
        public async Task MatchingRule_ReturnsRecommendationWithReason()
        {
            //First we create an in-memory database for these tests
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("NoRulesTest").Options;

            var context = new ApplicationDbContext(options);

            // Create recommendation service for tests

            var service = new Models.RecommendationService(context);

            // Create a matching device

            var device = new Device
            {
                Brand = "Apple",
                Model = "iPhone",
                AgeInYears = 2,
                Condition = Condition.Working,
                HasPersonalData = true,
                HasBatteryDamage = false
            };

            //Get recommendations for this device
            var result = await service.RecommendAsync(device);

            //Check the results are as expected, recomendation exists and contains a reason
            Assert.NotEmpty(result);

            Assert.False(string.IsNullOrWhiteSpace(result[0].Reason));
        }


        // TEST 1: Minimum age boundary
        [Fact]
        public async Task MinimumAgeBoundary_ReturnsMatchingRule()
        {
            using var context = GetDatabase();

            context.RecommendationRules.Add(
                GetTestRule("Minimum age rule", "Minimum age matched"));

            await context.SaveChangesAsync();

            var service = new RecommendationService(context);

            var result = await service.RecommendAsync(GetTestDevice(2));

            // Age 2 should match because it is the minimum age
            Assert.Contains(result, r => r.Reason == "Minimum age matched");
        }

        // TEST 2: Maximum age boundary
        [Fact]
        public async Task MaximumAgeBoundary_ReturnsMatchingRule()
        {
            using var context = GetDatabase();

            context.RecommendationRules.Add(
                GetTestRule("Maximum age rule", "Maximum age matched"));

            await context.SaveChangesAsync();

            var service = new RecommendationService(context);

            var result = await service.RecommendAsync(GetTestDevice(5));

            // Age 5 should match because it is the maximum age
            Assert.Contains(result, r => r.Reason == "Maximum age matched");
        }

        // TEST 3: Higher priority rule wins
        [Fact]
        public async Task HigherPriorityRuleWins()
        {
            using var context = GetDatabase();

            context.RecommendationRules.AddRange(
                GetTestRule("Low priority rule", "Low priority", 1),
                GetTestRule("High priority rule", "High priority", 10)
            );

            await context.SaveChangesAsync();

            var service = new RecommendationService(context);

            var result = await service.RecommendAsync(GetTestDevice());

            // The higher priority rule should win
            Assert.Contains(result, r => r.Reason == "High priority");

            // The lower priority rule should not be returned
            Assert.DoesNotContain(result, r => r.Reason == "Low priority");
        }

        // TEST 4: Inactive rules are ignored
        [Fact]
        public async Task InactiveRulesAreIgnored()
        {
            using var context = GetDatabase();

            context.RecommendationRules.Add(
                GetTestRule("Inactive rule", "Inactive recommendation", 10, false)
            );

            await context.SaveChangesAsync();

            var service = new RecommendationService(context);

            var result = await service.RecommendAsync(GetTestDevice());

            // Inactive recommendation should not appear
            Assert.DoesNotContain(result, r => r.Reason == "Inactive recommendation");

            // With no active matching rules, the service should recycle
            Assert.Contains(result, r => r.Route == RouteType.Recycle);
        }

        // TEST 5: Ties between rules
        [Fact]
        public async Task TiedRules_ReturnOneRecommendationPerRoute()
        {
            using var context = GetDatabase();

            context.RecommendationRules.AddRange(
                GetTestRule("First tied rule", "First tied recommendation", 5),
                GetTestRule("Second tied rule", "Second tied recommendation", 5)
            );

            await context.SaveChangesAsync();

            var service = new RecommendationService(context);

            var result = await service.RecommendAsync(GetTestDevice());

            // Both rules have the same route, so only one recommendation is returned
            Assert.Single(result);

            // Either rule can win because they have equal priority
            Assert.Contains(result[0].Reason, new[]
            {
        "First tied recommendation",
        "Second tied recommendation"
    });
        }


    }

    public class ManagerQuestionsTests
    {
        // Creates a separate in-memory database for each test.
        private ApplicationDbContext GetDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // TEST 1: Correct answer type is accepted.
        [Fact]
        public async Task CorrectAnswerType_AllowsQuestionCreation()
        {
            using var context = GetDatabase();

            var controller = new QuestionsController(context);

            var question = new Question
            {
                Text = "What is your phone brand?",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text
            };

            var result = await controller.Create(question);

            // Question should be saved successfully.
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Single(context.Questions);
        }

        // TEST 2: Incorrect answer type is rejected.
        [Fact]
        public async Task IncorrectAnswerType_RejectsQuestionCreation()
        {
            using var context = GetDatabase();

            var controller = new QuestionsController(context);

            var question = new Question
            {
                Text = "What is your phone brand?",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Number
            };

            var result = await controller.Create(question);

            // Question should not be saved.
            Assert.IsType<ViewResult>(result);
            Assert.Empty(context.Questions);

            // ModelState should contain an answer type error.
            Assert.True(controller.ModelState.ContainsKey(
                nameof(Question.AnswerType)));
        }

        // TEST 3: Two active questions cannot use the same device field.
        [Fact]
        public async Task DuplicateActiveQuestion_IsRejected()
        {
            using var context = GetDatabase();

            context.Questions.Add(new Question
            {
                Text = "First brand question",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new QuestionsController(context);

            var question = new Question
            {
                Text = "Second brand question",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text
            };

            var result = await controller.Create(question);

            // Duplicate question should be rejected.
            Assert.IsType<ViewResult>(result);

            Assert.Single(context.Questions);

            Assert.True(controller.ModelState.ContainsKey(
                nameof(Question.DeviceField)));
        }

        // TEST 4: An inactive question does not prevent another
        // question from using the same device field.
        [Fact]
        public async Task InactiveQuestion_AllowsNewActiveQuestion()
        {
            using var context = GetDatabase();

            context.Questions.Add(new Question
            {
                Text = "Old brand question",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text,
                IsActive = false
            });

            await context.SaveChangesAsync();

            var controller = new QuestionsController(context);

            var question = new Question
            {
                Text = "New brand question",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text
            };

            var result = await controller.Create(question);

            // New question should be accepted.
            Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal(2, context.Questions.Count());

            Assert.Equal(1, context.Questions.Count(q => q.IsActive));
        }

        // TEST 5: Different device fields can have active questions.
        [Fact]
        public async Task DifferentDeviceFields_AllowActiveQuestions()
        {
            using var context = GetDatabase();

            context.Questions.Add(new Question
            {
                Text = "What is your phone brand?",
                DeviceField = DeviceField.Brand,
                AnswerType = AnswerType.Text,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new QuestionsController(context);

            var question = new Question
            {
                Text = "What is your phone model?",
                DeviceField = DeviceField.Model,
                AnswerType = AnswerType.Text
            };

            var result = await controller.Create(question);

            // Both questions should be accepted.
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(2, context.Questions.Count());
        }
    }


    //This class will contain all of the test methods for testing all four routes give the words people see
    public class ToDisplayNameTests
    {
        // Test that all route types return the correct display name
        [Fact]
        public void ToDisplayName_AllRoutes_ReturnCorrectNames()
        {
            //Check Donation routes
            Assert.Equal("Donate", RouteType.Donate.ToDisplayName());

            //Check ResaleOrTradeIn Routes
            Assert.Equal("Resale or trade-in", RouteType.ResaleOrTradeIn.ToDisplayName());

            //Check Repair Route
            Assert.Equal("Repair", RouteType.Repair.ToDisplayName());

            //Check Recycle Route
            Assert.Equal("Recycle", RouteType.Recycle.ToDisplayName());

        }
    }

    // Tests for requirement 7: one top recommendation plus at most two alternatives, none repeating the top route.
    public class AlternativesTests
    {
        // Builds a service whose database holds six active rules that match ANY device (all criteria null).
        // There are 4 routes, and Donate and Repair each appear twice, so there are more rules than routes.
        private static async Task<RecommendationService> BuildServiceAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("Alternatives_" + Guid.NewGuid()).Options; // unique name so tests never share data
            var context = new ApplicationDbContext(options);

            context.RecommendationRules.AddRange(
                new RecommendationRule { Name = "Donate high", Route = RouteType.Donate, Reason = "Donate (priority 10)", Priority = 10 },
                new RecommendationRule { Name = "Donate low", Route = RouteType.Donate, Reason = "Donate (priority 9)", Priority = 9 },
                new RecommendationRule { Name = "Resale", Route = RouteType.ResaleOrTradeIn, Reason = "Resale (priority 8)", Priority = 8 },
                new RecommendationRule { Name = "Repair high", Route = RouteType.Repair, Reason = "Repair (priority 7)", Priority = 7 },
                new RecommendationRule { Name = "Recycle", Route = RouteType.Recycle, Reason = "Recycle (priority 6)", Priority = 6 },
                new RecommendationRule { Name = "Repair low", Route = RouteType.Repair, Reason = "Repair (priority 5)", Priority = 5 });
            await context.SaveChangesAsync();

            return new RecommendationService(context);
        }

        private static Device AnyDevice() => new Device
        {
            Brand = "Apple",
            Model = "iPhone",
            AgeInYears = 2,
            Condition = Condition.Working,
            HasPersonalData = true,
            HasBatteryDamage = false
        };

        // Six rules match, but the result must be one top recommendation plus at most two alternatives (3 in total).
        [Fact]
        public async Task ManyMatchingRules_ReturnsTopPlusAtMostTwoAlternatives()
        {
            var service = await BuildServiceAsync();

            var result = await service.RecommendAsync(AnyDevice());

            Assert.Equal(3, result.Count);                          // 1 top + 2 alternatives
            Assert.Equal(RouteType.Donate, result[0].Route);        // top = highest priority rule
            Assert.Equal("Donate (priority 10)", result[0].Reason); // the priority 10 rule won, not the priority 9 one
            Assert.True(result.Skip(1).Count() <= 2);               // never more than two alternatives
        }

        // No alternative may be the same route as the top recommendation (or as each other).
        [Fact]
        public async Task Alternatives_NeverRepeatTheTopRecommendationsRoute()
        {
            var service = await BuildServiceAsync();

            var result = await service.RecommendAsync(AnyDevice());
            var top = result[0];
            var alternatives = result.Skip(1).ToList();

            Assert.All(alternatives, alt => Assert.NotEqual(top.Route, alt.Route)); // none duplicates the top route
            Assert.Equal(result.Count, result.Select(r => r.Route).Distinct().Count()); // and no route appears twice at all
        }
    }
}