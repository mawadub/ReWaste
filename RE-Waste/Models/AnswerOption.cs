using System.ComponentModel.DataAnnotations;

namespace RE_Waste.Models
{
    // One choice for a Choice-type question.
    public class AnswerOption
    {
        public int Id { get; set; }

        //answer option text, what customer sees e.g. "Yes", "No", "Maybe" etc.
        [Required]
        [StringLength(100)]
        public string Text { get; set; } = string.Empty;

        //the value that will be stored in the database when this option is selected, e.g. "Y", "N", "M" etc.
        [Required]
        [StringLength(50)]
        public string Value { get; set; } = string.Empty;

        //be used to order the questions in the wizard form, to make ordering easier 
        public int DisplayOrder { get; set; }

        //we can't delete questions (older data would break), but we can make them inactive so they won't be shown in the wizard form
        public bool IsActive { get; set; } = true;

        //navigation property
        // A question can have multiple answer options (for Choice-type questions)
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}