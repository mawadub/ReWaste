using System.ComponentModel.DataAnnotations;
using RE_Waste.Models.Enums;

namespace RE_Waste.Models
{
    // A wizard question. Managers can edit these; the wizard shows active ones in DisplayOrder (W2).
    public class Question
    {
        public int Id { get; set; }

        // The text of the question
        [Required]
        [StringLength(200)]
        public string Text { get; set; } = string.Empty;

        // The type of answer expected (Choice, Text, Number, etc.)
        public AnswerType AnswerType { get; set; }

        // DeviceField tells Wizard which Device property a question’s answer belongs to e.g. question1 is Brand, question3 is Model, question2 is AgeInYears
        // this property will allow managers to reword and change questions without breaking the logic of the wizard form
        public DeviceField DeviceField { get; set; }

        //be used to order the questions in the wizard form, to make ordering easier 
        public int DisplayOrder { get; set; }

        //we can't delete questions (older data would break), but we can make them inactive so they won't be shown in the wizard form
        public bool IsActive { get; set; } = true;

        //navigation property
        // A question can have multiple answer options (for Choice-type questions)
        public List<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }
}