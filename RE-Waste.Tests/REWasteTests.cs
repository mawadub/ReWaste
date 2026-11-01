
using Microsoft.EntityFrameworkCore;
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
}