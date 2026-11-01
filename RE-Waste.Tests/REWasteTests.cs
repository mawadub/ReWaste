
using RE_Waste.Models;
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


        //public class DeviceBuilderTests
        //{
        //    //This class will contain all of the test methods for testing the device builder and if good answers build a good device or if unreadable answers return nulls

        //    [Fact]
        //    public void DropOffServiceTest1()
        //    {

        //    }
        //}


        ////This class will contain all of the test methods for testing the the recommendation service and to see if it provides dififerent routes up to 3 with a reason each.
        //public class RecomendationServiceTests
        //{


        //    [Fact]
        //    public void DropOffServiceTest1()
        //    {

        //    }
        //}

        ////This class will contain all of the test methods for testing all four routes give the words people see
        //public class ToDisplayNameTests
        //{


        //    [Fact]
        //    public void DropOffServiceTest1()
        //    {

        //    }
        //}
    }